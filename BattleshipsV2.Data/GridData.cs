using System;
using System.Collections.Generic;
using System.Linq;

namespace BattleshipsV2.Data
{
    public class GridData : IGridData
    {
        private Dictionary<Coord, CellContent> PlayerGrid;
        private Dictionary<Coord, CellContent> AIGrid;
        private List<Coord> PlayerCoords;
        private List<Coord> AICoords;
        private int PlayerHitpoints;
        private int AIHitpoints;
        private List<ShipType> ValidShips;
        private readonly Random Rand = new Random();

        #region Interface Methods

        public void SetElementsFromJson(GameDataFormat dataFormat)
        {
            if (dataFormat == null || dataFormat.ValidShips == null)
            {
                InitialiseElements();
            }
            else
            {
                PlayerGrid = dataFormat.PlayerGrid.ToDictionary(x => GetCoord(x.Key.X, x.Key.Y, dataFormat.PlayerCoords), x => x.Value);
                PlayerCoords = dataFormat.PlayerCoords;
                PlayerHitpoints = dataFormat.PlayerHitpoints;
                AIGrid = dataFormat.AIGrid.ToDictionary(x => GetCoord(x.Key.X, x.Key.Y, dataFormat.AICoords), x => x.Value);
                AICoords = dataFormat.AICoords;
                AIHitpoints = dataFormat.AIHitpoints;
                ValidShips = dataFormat.ValidShips;
            }
        }

        public GameDataFormat FormatElements()
        {
            GameDataFormat dataFormat = new GameDataFormat();
            dataFormat.PlayerGrid = PlayerGrid.ToList();
            dataFormat.PlayerCoords = PlayerCoords;
            dataFormat.PlayerHitpoints = PlayerHitpoints;
            dataFormat.AIGrid = AIGrid.ToList();
            dataFormat.AICoords = AICoords;
            dataFormat.AIHitpoints = AIHitpoints;
            dataFormat.ValidShips = ValidShips;
            return dataFormat;
        }

        public List<Coord> GetPlayerCoords()
        {
            return PlayerCoords;
        }

        public List<Coord> GetAICoords()
        {
            return AICoords;
        }

        public List<ShipType> GetValidShips()
        {
            return ValidShips;
        }

        public Dictionary<Coord, CellContent> GetPlayerGrid()
        {
            return PlayerGrid;
        }

        public Dictionary<Coord, CellContent> GetAIGrid()
        {
            return AIGrid;
        }

        public MoveResult FireAtCellOnAIGrid(string x, string y)
        {
            Coord coord = GetCoord(ConvertXStringToInt(x), ConvertYStringToInt(y), AICoords);
            if (AIGrid[coord] == CellContent.Ship)
            {
                AIHitpoints--;
                SetCell(coord, CellContent.Hit, AIGrid);
                if (AIHitpoints == 0) return MoveResult.LastAIHitPointTaken;
                else return MoveResult.Success;
            }
            else if (AIGrid[coord] != CellContent.Water)
            {
                return MoveResult.SpaceIsOccupied;
            }
            else
            {
                SetCell(coord, CellContent.Miss, AIGrid);
                return MoveResult.Success;
            }
        }

        public MoveResult FireAtCellOnPlayerGrid(int x, int y)
        {
            Coord coord = GetCoord(x, y, PlayerCoords);
            if (PlayerGrid[coord] == CellContent.Ship)
            {
                PlayerHitpoints--;
                SetCell(coord, CellContent.Hit, PlayerGrid);
                if (PlayerHitpoints == 0) return MoveResult.LastPlayerHitPointTaken;
                else return MoveResult.Success;
            }
            else
            {
                SetCell(coord, CellContent.Miss, PlayerGrid);
                return MoveResult.AIHasMissed;
            }
        }

        public MoveResult PlaceShipOnPlayerGrid(ShipType type, string startString, string endString)
        {
            // Coord has not been entered
            if (startString == null || endString == null) return MoveResult.InvalidCoords;
            // Coord is too short / too long
            if (startString.Length < 2 || (startString.Length > 2 && startString[1..] != "10")) return MoveResult.InvalidCoords;
            if (endString.Length < 2 || (endString.Length > 2 && endString[1..] != "10")) return MoveResult.InvalidCoords;
            // Coord is out of bounds
            if (!StringIsValid(startString) || !StringIsValid(endString)) return MoveResult.InvalidCoords;

            int placesInLine = 5 - (int)type;
            Coord startCoord = GetCoord(ConvertXStringToInt(startString[0].ToString()), ConvertYStringToInt(startString[1..]), PlayerCoords);
            Coord endCoord = GetCoord(ConvertXStringToInt(endString[0].ToString()), ConvertYStringToInt(endString[1..]), PlayerCoords);
            List<Coord> coordsToPlace = new List<Coord>();

            // Vector is not horizontal or vertical
            if (startCoord.X != endCoord.X && startCoord.Y != endCoord.Y) return MoveResult.InvalidSize;
            // Vector is vertical
            else if (startCoord.X == endCoord.X)
            {
                // Vector does not match ship size
                if (Math.Abs(startCoord.Y - endCoord.Y) + 1 != placesInLine) return MoveResult.InvalidSize;

                int direction = startCoord.Y < endCoord.Y ? 1 : -1;
                for (int i = 0; i < placesInLine; i++)
                {
                    int x = startCoord.X;
                    int y = startCoord.Y + (i * direction);
                    // Cell is occupied; break loop before adding
                    if (CheckCellType(x, y, true) == CellContent.Ship) return MoveResult.SpaceIsOccupied;
                    coordsToPlace.Add(GetCoord(x, y, PlayerCoords));
                }
            }
            // Vector is horizontal
            else
            {
                // Vector does not match ship size
                if (Math.Abs(startCoord.X - endCoord.X) + 1 != placesInLine) return MoveResult.InvalidSize;

                int direction = startCoord.X < endCoord.X ? 1 : -1;
                for (int i = 0; i < placesInLine; i++)
                {
                    int x = startCoord.X + (i * direction);
                    int y = startCoord.Y;
                    // Cell is occupied; break loop before adding
                    if (CheckCellType(x, y, true) == CellContent.Ship) return MoveResult.SpaceIsOccupied;
                    coordsToPlace.Add(GetCoord(x, y, PlayerCoords));
                }
            }

            foreach (Coord C in coordsToPlace)
            {
                SetCell(C, CellContent.Ship, PlayerGrid);
            }

            ValidShips.Remove(type);
            return MoveResult.Success;
        }

        public void PlaceAllShipsRandomly(Dictionary<Coord, CellContent> grid)
        {
            List<Coord> coords;
            List<ShipType> shipTypes;
            if (grid == PlayerGrid)
            {
                coords = PlayerCoords;
                shipTypes = ValidShips;
            }
            else
            {
                coords = AICoords;
                shipTypes = new List<ShipType>()
                {
                    ShipType.Carrier,
                    ShipType.Battleship,
                    ShipType.Cruiser,
                    ShipType.Destroyer,
                    ShipType.Destroyer,
                    ShipType.Submarine,
                    ShipType.Submarine
                };
            }

            while (shipTypes.Count > 0)
            {
                ShipType ship = shipTypes[0];
                int placesInLine = 5 - (int)ship;
                Coord start = GetCoord(Rand.Next(10), Rand.Next(10), coords);
                Coord end = GetCoord(Rand.Next(10), Rand.Next(10), coords);
                List<Coord> coordsToPlace = new List<Coord>();

                // Determining vector validity
                bool cellsAreUnoccupied = true;
                bool vectorIsNotDiagonal = start.X == end.X || start.Y == end.Y;
                bool vectorIsVertical = start.X == end.X;
                bool vectorMatchesShipSize;
                if (vectorIsVertical) vectorMatchesShipSize = (Math.Abs(start.Y - end.Y) + 1 == placesInLine);
                else vectorMatchesShipSize = (Math.Abs(start.X - end.X) + 1 == placesInLine);

                if (vectorIsNotDiagonal && vectorMatchesShipSize)
                {
                    if (vectorIsVertical)
                    {
                        int direction = start.Y < end.Y ? 1 : -1;
                        for (int i = 0; i < placesInLine; i++)
                        {
                            int x = start.X;
                            int y = start.Y + (i * direction);
                            // Cell is occupied; break loop before adding
                            if (CheckCellType(x, y, grid == PlayerGrid) == CellContent.Ship) cellsAreUnoccupied = false;
                            coordsToPlace.Add(GetCoord(x, y, coords));
                        }
                    }
                    else
                    {
                        int direction = start.X < end.X ? 1 : -1;
                        for (int i = 0; i < placesInLine; i++)
                        {
                            int x = start.X + (i * direction);
                            int y = start.Y;
                            // Cell is occupied; break loop before adding
                            if (CheckCellType(x, y, grid == PlayerGrid) == CellContent.Ship) cellsAreUnoccupied = false;
                            coordsToPlace.Add(GetCoord(x, y, coords));
                        }
                    }
                    if (cellsAreUnoccupied)
                    {
                        shipTypes.Remove(ship);
                        foreach (Coord C in coordsToPlace)
                        {
                            SetCell(C, CellContent.Ship, grid);
                        }
                    }
                }
            }
        }

        #endregion Interface Methods

        #region Internal Methods

        private void InitialiseElements()
        {
            PlayerGrid = new Dictionary<Coord, CellContent>();
            PlayerCoords = new List<Coord>();
            PlayerHitpoints = 18;
            AIGrid = new Dictionary<Coord, CellContent>();
            AICoords = new List<Coord>();
            AIHitpoints = 18;
            ValidShips = new List<ShipType>()
            {
                ShipType.Carrier,
                ShipType.Battleship,
                ShipType.Cruiser,
                ShipType.Destroyer,
                ShipType.Destroyer,
                ShipType.Submarine,
                ShipType.Submarine
            };

            for (int x = 0; x < 10; x++)
            {
                for (int y = 0; y < 10; y++)
                {
                    Coord coord = new Coord(x, y);
                    PlayerCoords.Add(coord);
                    PlayerGrid.Add(coord, CellContent.Water);

                    coord = new Coord(x, y);
                    AICoords.Add(coord);
                    AIGrid.Add(coord, CellContent.Water);
                }
            }

            PlaceAllShipsRandomly(AIGrid);
        }

        private CellContent CheckCellType(int x, int y, bool isPlayer)
        {
            if (isPlayer) return PlayerGrid[GetCoord(x, y, PlayerCoords)];
            else return AIGrid[GetCoord(x, y, AICoords)];
        }

        private void SetCell(Coord coord, CellContent content, Dictionary<Coord, CellContent> grid)
        {
            grid[coord] = content;
        }

        private Coord GetCoord(int x, int y, List<Coord> coords)
        {
            return coords.FirstOrDefault(c => c.X == x && c.Y == y);
        }

        private bool StringIsValid(string xy)
        {
            List<string> validXStrings = new List<string>() { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J" };
            List<string> validYStrings = new List<string>() { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" };
            return validXStrings.Contains(xy[0].ToString().ToUpper()) && validYStrings.Contains(xy[1..]);
        }

        private int ConvertXStringToInt(string x)
        {
            List<string> validStrings = new List<string>() { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J" };
            return validStrings.IndexOf(x.ToUpper());
        }

        private int ConvertYStringToInt(string y)
        {
            List<string> validStrings = new List<string>() { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" };
            return validStrings.IndexOf(y);
        }

        #endregion Internal Methods
    }
}