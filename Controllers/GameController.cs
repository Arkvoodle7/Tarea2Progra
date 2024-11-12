using Tarea2Progra.Models;
using Tarea2Progra.Utilities;
using System;
using System.Threading;
using System.Collections.Generic;

namespace Tarea2Progra.Controllers
{
    public class GameController
    {
        public GameBoard GameBoard { get; private set; }
        private int threadCount;
        private Logger logger;
        private int generation = 0;
        private List<Thread> threads;
        private Cell[,] newCells;
        private ManualResetEvent[] resetEvents;

        public GameController(int width, int height, int threadCount, bool randomize, Logger logger)
        {
            GameBoard = new GameBoard(width, height);
            this.threadCount = threadCount;
            this.logger = logger;

            if (randomize)
            {
                RandomizeBoard();
            }

            logger.LogInitialState(GameBoard);
        }

        public void NextGeneration()
        {
            generation++;
            newCells = new Cell[GameBoard.Width, GameBoard.Height];
            for (int x = 0; x < GameBoard.Width; x++)
            {
                for (int y = 0; y < GameBoard.Height; y++)
                {
                    newCells[x, y] = new Cell();
                }
            }

            int regionHeight = GameBoard.Height / threadCount;
            resetEvents = new ManualResetEvent[threadCount];

            threads = new List<Thread>();

            for (int i = 0; i < threadCount; i++)
            {
                int threadIndex = i;
                resetEvents[threadIndex] = new ManualResetEvent(false);

                Thread thread = new Thread(() => ProcessRegion(threadIndex, regionHeight));
                threads.Add(thread);
                thread.Start();
            }

            // Esperar a que todos los hilos terminen su procesamiento
            WaitHandle.WaitAll(resetEvents);

            // Actualizar el tablero con los nuevos estados
            GameBoard.Cells = newCells;

            // Registrar el estado después de la generación actual
            logger.LogState(GameBoard, generation);
        }

        private void ProcessRegion(int threadIndex, int regionHeight)
        {
            int yStart = threadIndex * regionHeight;
            int yEnd = (threadIndex == threadCount - 1) ? GameBoard.Height : yStart + regionHeight;

            for (int x = 0; x < GameBoard.Width; x++)
            {
                for (int y = yStart; y < yEnd; y++)
                {
                    int aliveNeighbors = CountAliveNeighbors(x, y);
                    bool isAlive = GameBoard.Cells[x, y].IsAlive;

                    if (!isAlive && aliveNeighbors == 3)
                    {
                        newCells[x, y].IsAlive = true;
                    }
                    else if (isAlive && (aliveNeighbors == 2 || aliveNeighbors == 3))
                    {
                        newCells[x, y].IsAlive = true;
                    }
                    else
                    {
                        newCells[x, y].IsAlive = false;
                    }
                }
            }

            // Señalar que este hilo ha terminado su procesamiento
            resetEvents[threadIndex].Set();
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
