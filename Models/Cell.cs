using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tarea2Progra.Models
{
    public class Cell
    {
        public bool IsAlive { get; set; }

        public Cell(bool isAlive = false)
        {
            IsAlive = isAlive;
        }
    }
}

