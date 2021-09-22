using BattleshipsV2.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BattleshipsV2.ViewModels
{
    public class GridViewModel
    {
        public CellContent[,] PlayerGrid { get; set; }
        public CellContent[,] AIGrid { get; set; }
        public string PlayerMessage { get; set; }
        public string AIMessage { get; set; }
        public List<ShipType> ValidShips { get; set; }
    }
}
