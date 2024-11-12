using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tarea2Progra.Models;
using System.IO;

namespace Tarea2Progra.Utilities
{
    public class Logger
    {
        private string fileName;
        private StringBuilder logBuilder;

        public Logger()
        {
            string date = DateTime.Now.ToString("yyyyMMdd");
            int consecutive = 1;
            do
            {
                fileName = $"Simulacion{date}-{consecutive}.txt";
                consecutive++;
            } while (File.Exists(fileName));

            logBuilder = new StringBuilder();
        }

        public void LogInitialState(GameBoard board, int generation)
        {
            logBuilder.AppendLine($"Generación {generation}: Estado inicial");
            LogBoardState(board);
        }

        public void LogState(GameBoard board, int generation)
        {
            logBuilder.AppendLine($"Generación {generation}:");
            LogBoardState(board);
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
            SaveLog();
        }

        private void SaveLog()
        {
            File.WriteAllText(fileName, logBuilder.ToString());
        }

        public void ClearLog()
        {
            logBuilder.Clear();
            File.Delete(fileName);
        }
    }
}

