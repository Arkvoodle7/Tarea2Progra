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
        private bool isManualSetup = false;
        private CancellationTokenSource cancellationTokenSource;
        private Logger logger;

        public MainWindow()
        {
            InitializeComponent();
            logger = new Logger();
        }

        private void ManualSetupButton_Click(object sender, RoutedEventArgs e)
        {
            int width, height, threadCount;

            if (!int.TryParse(WidthTextBox.Text, out width) ||
                !int.TryParse(HeightTextBox.Text, out height) ||
                !int.TryParse(ThreadCountTextBox.Text, out threadCount))
            {
                MessageBox.Show("Por favor, ingrese valores numéricos válidos para el ancho, alto y número de hilos.");
                return;
            }

            if (threadCount < 2 || threadCount % 2 != 0)
            {
                MessageBox.Show("El número de hilos debe ser par y mayor o igual a 2.");
                return;
            }

            gameController = new GameController(width, height, threadCount, false, logger);
            isManualSetup = true;
            DrawBoard();
            StartButton.IsEnabled = true;

            MessageBox.Show("Haga clic en las celdas para establecer las células vivas. Luego, presione 'Iniciar' para comenzar la simulación.");
        }

        private void RandomSetupButton_Click(object sender, RoutedEventArgs e)
        {
            int width, height, threadCount;

            if (!int.TryParse(WidthTextBox.Text, out width) ||
                !int.TryParse(HeightTextBox.Text, out height) ||
                !int.TryParse(ThreadCountTextBox.Text, out threadCount))
            {
                MessageBox.Show("Por favor, ingrese valores numéricos válidos para el ancho, alto y número de hilos.");
                return;
            }

            if (threadCount < 2 || threadCount % 2 != 0)
            {
                MessageBox.Show("El número de hilos debe ser par y mayor o igual a 2.");
                return;
            }

            gameController = new GameController(width, height, threadCount, true, logger);
            isManualSetup = false;
            DrawBoard();
            StartButton.IsEnabled = true;
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (gameController == null)
            {
                MessageBox.Show("Debe configurar el juego antes de iniciar.");
                return;
            }

            isRunning = true;
            isManualSetup = false;
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
            DrawBoard();
        }

        private void RunGame(CancellationToken token)
        {
            while (isRunning)
            {
                gameController.NextGeneration();
                Dispatcher.Invoke(() => DrawBoard());
                Thread.Sleep(500); // Pausa entre generaciones

                if (token.IsCancellationRequested)
                {
                    break;
                }
            }
        }

        private void DrawBoard()
        {
            GameCanvas.Children.Clear();
            var board = gameController.GameBoard;
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

                    if (isManualSetup && !isRunning)
                    {
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
