using Tarea2Progra.Models;
using Tarea2Progra.Controllers;
using Tarea2Progra.Utilities;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls;

namespace Tarea2Progra
{
    public partial class MainWindow : Window
    {
        private GameController gameController;
        private int cellSize = 10;
        private bool isRunning = false;
        private CancellationTokenSource cancellationTokenSource;
        private Logger logger;
        private int totalGenerations = 0;
        private int currentGeneration = 0;

        public MainWindow()
        {
            InitializeComponent();
            logger = new Logger();
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

                // Accedemos a StepByStepCheckBox.IsChecked dentro del Dispatcher
                Dispatcher.Invoke(() =>
                {
                    stepByStep = StepByStepCheckBox.IsChecked == true;
                });

                if (stepByStep)
                {
                    // Pausar para permitir al usuario editar el tablero
                    isRunning = false;

                    Dispatcher.Invoke(() =>
                    {
                        PauseButton.IsEnabled = false;
                        ResumeButton.IsEnabled = true;
                        DrawBoard(); // Actualizar la interfaz después de cambiar isRunning
                    });

                    break;
                }
                else
                {
                    Dispatcher.Invoke(() => DrawBoard());
                    Thread.Sleep(500); // Pausa entre generaciones automáticas
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

            for (int x = 0; x < board.Width; x++)
            {
                for (int y = 0; y < board.Height; y++)
                {
                    Rectangle rect = new Rectangle
                    {
                        Width = cellSize,
                        Height = cellSize,
                        Fill = board.Cells[x, y].IsAlive ? Brushes.Black : Brushes.White,
                        Stroke = Brushes.Gray,
                        StrokeThickness = 0.5,
                        Tag = new Tuple<int, int>(x, y)
                    };

                    if (!isRunning && (stepByStep || currentGeneration == 0))
                    {
                        // Permitir interacción cuando la simulación está pausada y se ha activado el paso a paso, o durante la configuración inicial
                        rect.MouseDown += Cell_MouseDown;
                    }

                    Canvas.SetLeft(rect, x * cellSize);
                    Canvas.SetTop(rect, y * cellSize);
                    GameCanvas.Children.Add(rect);
                }
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

                // Alternar el estado de la célula
                var cell = gameController.GameBoard.Cells[x, y];
                cell.IsAlive = !cell.IsAlive;

                // Actualizar la visualización de la célula
                rect.Fill = cell.IsAlive ? Brushes.Black : Brushes.White;
            }
        }
    }
}
