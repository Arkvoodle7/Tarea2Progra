using Tarea2Progra.Models;
using System;
using System.IO;
using System.Text;

namespace Tarea2Progra.Utilities
{
    public class Logger
    {
        private string fileName;
        private string filePath;
        private StringBuilder logBuilder;

        public Logger()
        {
            string date = DateTime.Now.ToString("yyyyMMdd");
            int consecutive = 1;

            do
            {
                fileName = $"Simulacion{date}-{consecutive}.txt";
                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
                consecutive++;
            } while (File.Exists(filePath));

            logBuilder = new StringBuilder();
        }

        public void LogInitialState(GameBoard board)
        {
            logBuilder.AppendLine("Estado inicial del tablero:");
            LogBoardState(board);
            SaveLog();
        }

        public void LogState(GameBoard board, int generation)
        {
            logBuilder.AppendLine($"Estado después de la generación {generation}:");
            LogBoardState(board);
            SaveLog();
        }

        private void LogBoardState(GameBoard board)
        {
            for (int y = 0; y < board.Height; y++)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    logBuilder.Append(board.Cells[x, y].IsAlive ? "1" : "0");
                }
                logBuilder.AppendLine();
            }
            logBuilder.AppendLine();
        }

        private void SaveLog()
        {
            File.WriteAllText(filePath, logBuilder.ToString());
        }

        public void ClearLog()
        {
            logBuilder.Clear();
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }
}
