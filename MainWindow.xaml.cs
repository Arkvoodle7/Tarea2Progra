using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Tarea2Progra.Controllers;
using Tarea2Progra.Utilities;

namespace Tarea2Progra
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private GameController gameController;
        private int cellSize = 10;
        private bool isRunning = false;
        private CancellationTokenSource cancellationTokenSource;
        private Logger logger;

        public MainWindow()
        {
            InitializeComponent();
            logger = new Logger();
        }

        private void ManualSetupButton_Click(object sender, RoutedEventArgs e)
        {
            // Implementar la configuración manual del estado inicial
            MessageBox.Show("Función no implementada.");
        }

        private void RandomSetupButton_Click(object sender, RoutedEventArgs e)
        {
            int width = int.Parse(WidthTextBox.Text);
            int height = int.Parse(HeightTextBox.Text);
            int threadCount = int.Parse(ThreadCountTextBox.Text);

            if (threadCount < 2 || threadCount % 2 != 0)
            {
                MessageBox.Show("El número de hilos debe ser par y mayor o igual a 2.");
                return;
            }

            gameController = new GameController(width, height, threadCount, true, logger);
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
                        StrokeThickness = 0.5
                    };
                    Canvas.SetLeft(rect, x * cellSize);
                    Canvas.SetTop(rect, y * cellSize);
                    GameCanvas.Children.Add(rect);
                }
            }
        }
    }
}
