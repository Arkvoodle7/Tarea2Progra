using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tarea2Progra.Models
{
    public class GameBoard
    {
        public int Width { get; }
        public int Height { get; }
        public Cell[,] Cells { get; set; }

        public GameBoard(int width, int height)
        {
            Width = width;
            Height = height;
            Cells = new Cell[Width, Height];
            InitializeCells();
        }

        private void InitializeCells()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    Cells[x, y] = new Cell();
                }
            }
        }
    }
}

