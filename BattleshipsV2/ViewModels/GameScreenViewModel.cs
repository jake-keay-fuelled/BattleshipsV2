using BattleshipsV2.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BattleshipsV2.ViewModels
{
    public class GameScreenViewModel
    {
        public CellContent[,] PlayerGrid { get; set; }
        public CellContent[,] AIGrid { get; set; }
        public IEnumerable<ShipType> ValidShips { get; set; }
        public int GameStage { get; set; }
        public string PrimaryMessage { get; set; }
        public string TrackingMessage { get; set; }
        public string PlayerHitPoints { get; set; }
        public string AIHitPoints { get; set; }
        public string Turn { get; set; }
    }
}
