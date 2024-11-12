using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tarea2Progra.Models;
using Tarea2Progra.Utilities;

namespace Tarea2Progra.Controllers
{
    public class GameController
    {
        public GameBoard GameBoard { get; private set; }
        private int threadCount;
        private Logger logger;
        private int generation = 0;

        public GameController(int width, int height, int threadCount, bool randomize, Logger logger)
        {
            GameBoard = new GameBoard(width, height);
            this.threadCount = threadCount;
            this.logger = logger;

            if (randomize)
            {
                RandomizeBoard();
            }

            logger.LogInitialState(GameBoard, generation);
        }

        public void NextGeneration()
        {
            generation++;
            var newBoard = new GameBoard(GameBoard.Width, GameBoard.Height);
            int regionHeight = GameBoard.Height / threadCount;

            Parallel.For(0, threadCount, i =>
            {
                int yStart = i * regionHeight;
                int yEnd = (i == threadCount - 1) ? GameBoard.Height : yStart + regionHeight;

                for (int x = 0; x < GameBoard.Width; x++)
                {
                    for (int y = yStart; y < yEnd; y++)
                    {
                        int aliveNeighbors = CountAliveNeighbors(x, y);
                        bool isAlive = GameBoard.Cells[x, y].IsAlive;

                        if (!isAlive && aliveNeighbors == 3)
                        {
                            newBoard.Cells[x, y].IsAlive = true;
                        }
                        else if (isAlive && (aliveNeighbors == 2 || aliveNeighbors == 3))
                        {
                            newBoard.Cells[x, y].IsAlive = true;
                        }
                        else
                        {
                            newBoard.Cells[x, y].IsAlive = false;
                        }
                    }
                }
            });

            GameBoard = newBoard;
            logger.LogState(GameBoard, generation);
        }

        private int CountAliveNeighbors(int x, int y)
        {
            int count = 0;
            int width = GameBoard.Width;
            int height = GameBoard.Height;

            for (int i = -1; i <= 1; i++)
            {
                int nx = (x + i + width) % width;
                for (int j = -1; j <= 1; j++)
                {
                    int ny = (y + j + height) % height;
                    if (i == 0 && j == 0) continue;
                    if (GameBoard.Cells[nx, ny].IsAlive)
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        private void RandomizeBoard()
        {
            Random rand = new Random();
            for (int x = 0; x < GameBoard.Width; x++)
            {
                for (int y = 0; y < GameBoard.Height; y++)
                {
                    GameBoard.Cells[x, y].IsAlive = rand.Next(2) == 0;
                }
            }
        }

        public void Reset()
        {
            GameBoard = new GameBoard(GameBoard.Width, GameBoard.Height);
            generation = 0;
            logger.ClearLog();
        }
    }
}

