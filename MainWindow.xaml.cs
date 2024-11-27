using Tarea2Progra.Models;
using Tarea2Progra.Controllers;
using Tarea2Progra.Utilities;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Tarea2Progra
{
    public partial class MainWindow : Window
    {
        private GameController gameController;
        private int cellSize = 10;
        private bool isRunning = false;
        private CancellationTokenSource cancellationTokenSource;
        private int totalGenerations = 0;
        private int currentGeneration = 0;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ManualSetupButton_Click(object sender, RoutedEventArgs e)
        {
            if (!InitializeGame(false))
                return;

            DrawBoard();
            StartButton.IsEnabled = true;

            MessageBox.Show("Haga clic en las celdas para establecer las células vivas. Luego, presione 'Iniciar' para comenzar la simulación.");
        }

        private void RandomSetupButton_Click(object sender, RoutedEventArgs e)
        {
            if (!InitializeGame(true))
                return;

            DrawBoard();
            StartButton.IsEnabled = true;
        }

        private bool InitializeGame(bool randomize)
        {
            int width, height, threadCount;

            if (!int.TryParse(WidthTextBox.Text, out width) ||
                !int.TryParse(HeightTextBox.Text, out height) ||
                !int.TryParse(ThreadCountTextBox.Text, out threadCount))
            {
                MessageBox.Show("Por favor, ingrese valores numéricos válidos para el ancho, alto y número de hilos.");
                return false;
            }

            if (threadCount < 2 || threadCount % 2 != 0)
            {
                MessageBox.Show("El número de hilos debe ser par y mayor o igual a 2.");
                return false;
            }

            if (!int.TryParse(GenerationCountTextBox.Text, out totalGenerations) || totalGenerations <= 0)
            {
                MessageBox.Show("Por favor, ingrese un número válido de generaciones mayor que cero.");
                return false;
            }

            Logger logger = new Logger();

            gameController = new GameController(width, height, threadCount, randomize, logger);

            currentGeneration = 0;
            return true;
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (gameController == null)
            {
                MessageBox.Show("Debe configurar el juego antes de iniciar.");
                return;
            }

            isRunning = true;
            StartButton.IsEnabled = false;
            PauseButton.IsEnabled = true;
            ResetButton.IsEnabled = true;

            cancellationTokenSource = new CancellationTokenSource();
            Task.Run(() => RunGame(cancellationTokenSource.Token));
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            isRunning = false;
            PauseButton.IsEnabled = false;
            ResumeButton.IsEnabled = true;
            cancellationTokenSource.Cancel();
        }

        private void ResumeButton_Click(object sender, RoutedEventArgs e)
        {
            isRunning = true;
            PauseButton.IsEnabled = true;
            ResumeButton.IsEnabled = false;

            cancellationTokenSource = new CancellationTokenSource();
            Task.Run(() => RunGame(cancellationTokenSource.Token));
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            isRunning = false;
            StartButton.IsEnabled = true;
            PauseButton.IsEnabled = false;
            ResumeButton.IsEnabled = false;
            ResetButton.IsEnabled = false;
            cancellationTokenSource.Cancel();
            gameController.Reset();
            currentGeneration = 0;
            DrawBoard();
        }

        private void RunGame(CancellationToken token)
        {
            while (isRunning && currentGeneration < totalGenerations)
            {
                currentGeneration++;
                gameController.NextGeneration();

                if (token.IsCancellationRequested)
                {
                    break;
                }

                bool stepByStep = false;

                Dispatcher.Invoke(() =>
                {
                    stepByStep = StepByStepCheckBox.IsChecked == true;
                });

                if (stepByStep)
                {
                    isRunning = false;

                    Dispatcher.Invoke(() =>
                    {
                        PauseButton.IsEnabled = false;
                        ResumeButton.IsEnabled = true;
                        DrawBoard();
                    });

                    break;
                }
                else
                {
                    Dispatcher.Invoke(() => DrawBoard());
                    Thread.Sleep(500);
                }
            }

            if (currentGeneration >= totalGenerations)
            {
                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show("La simulación ha alcanzado el número máximo de generaciones.");
                    isRunning = false;
                    StartButton.IsEnabled = true;
                    PauseButton.IsEnabled = false;
                    ResumeButton.IsEnabled = false;
                    ResetButton.IsEnabled = false;
                });
            }
        }

        private void DrawBoard()
        {
            GameCanvas.Children.Clear();
            var board = gameController.GameBoard;
            bool stepByStep = StepByStepCheckBox.IsChecked == true;

            int regionHeight = board.Height / gameController.ThreadCount;

            for (int x = 0; x < board.Width; x++)
            {
                for (int y = 0; y < board.Height; y++)
                {
                    int regionIndex = y / regionHeight;
                    if (regionIndex >= gameController.ThreadCount)
                    {
                        regionIndex = gameController.ThreadCount - 1;
                    }

                    Brush backgroundBrush = GetRegionBrush(regionIndex);

                    Rectangle rect = new Rectangle
                    {
                        Width = cellSize,
                        Height = cellSize,
                        Fill = board.Cells[x, y].IsAlive ? Brushes.Black : backgroundBrush,
                        Stroke = Brushes.Gray,
                        StrokeThickness = 0.5,
                        Tag = new Tuple<int, int>(x, y)
                    };

                    if (!isRunning && (stepByStep || currentGeneration == 0))
                    {
                        rect.MouseDown += Cell_MouseDown;
                    }

                    Canvas.SetLeft(rect, x * cellSize);
                    Canvas.SetTop(rect, y * cellSize);
                    GameCanvas.Children.Add(rect);
                }
            }

            DrawRegionSeparators(board.Width, board.Height, regionHeight);
        }

        private Brush GetRegionBrush(int regionIndex)
        {
            Brush[] regionBrushes = new Brush[]
            {
                Brushes.LightYellow,
                Brushes.LightBlue,
                Brushes.LightGreen,
                Brushes.LightCoral,
                Brushes.LightGray,
                Brushes.LightPink,
                Brushes.LightCyan,
                Brushes.LightSalmon
            };

            return regionBrushes[regionIndex % regionBrushes.Length];
        }

        private void DrawRegionSeparators(int boardWidth, int boardHeight, int regionHeight)
        {
            int numberOfRegions = gameController.ThreadCount;

            for (int i = 1; i < numberOfRegions; i++)
            {
                double y = i * regionHeight * cellSize;

                Line line = new Line
                {
                    X1 = 0,
                    Y1 = y,
                    X2 = boardWidth * cellSize,
                    Y2 = y,
                    Stroke = Brushes.Red,
                    StrokeThickness = 2
                };

                GameCanvas.Children.Add(line);
            }
        }

        private void Cell_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (gameController == null || isRunning)
                return;

            Rectangle rect = sender as Rectangle;
            if (rect != null)
            {
                var position = (Tuple<int, int>)rect.Tag;
                int x = position.Item1;
                int y = position.Item2;

                var cell = gameController.GameBoard.Cells[x, y];
                cell.IsAlive = !cell.IsAlive;

                rect.Fill = cell.IsAlive ? Brushes.Black : rect.Fill;
            }
        }
    }
}
