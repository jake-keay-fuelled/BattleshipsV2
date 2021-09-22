using System;
using System.Collections.Generic;
using System.Text;

namespace BattleshipsV2.Data
{
    public class GameDataFormat
    {
        public List<KeyValuePair<Coord, CellContent>> PlayerGrid { get; set; }
        public List<KeyValuePair<Coord, CellContent>> AIGrid { get; set; }
        public List<Coord> PlayerCoords { get; set; }
        public List<Coord> AICoords { get; set; }
        public List<Coord> PlayerMoves { get; set; }
        public List<Coord> AIMoves { get; set; }
        public int PlayerHitpoints { get; set; }
        public int AIHitpoints { get; set; }
        public List<ShipType> ValidShips { get; set; }
    }
}
