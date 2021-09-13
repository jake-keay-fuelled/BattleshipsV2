using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleshipsV2.Data
{
    public static class GridHandler
    {
        private readonly static Random Rand = new Random(Guid.NewGuid().GetHashCode());
        private const int GridSize = 10;
        private static bool[,] PlayerMoves;
        public static List<ShipType> ValidPlayerShips;

        public static Grid PlayerGrid;
        public static Grid AIGrid;
        private static int PlayerHitPoints = 18;
        private static int AIHitPoints = 18;
        public static int Turn = 1;
        public static int GameStage = 1;
        public static string TrackingMessage = "";
        public static string PrimaryMessage = "";

        public static void HandleGameStart()
        {
            PrimaryMessage = "";
            AI.Reset();
            PlayerGrid = new Grid(GridSize, true);
            AIGrid = new Grid(GridSize, false);
            AI.SetGridSizeRef(GridSize);
            ValidPlayerShips = ProvideStartingShipList();
            AI.HandleShipPlacement(ProvideStartingShipList());
            PlayerHitPoints = 18;
            AIHitPoints = 18;
            Turn = 1;
            GameStage = 1;

            PlayerMoves = new bool[GridSize, GridSize];
            for (int x = 0; x < GridSize; x++)
            {
                for (int y = 0; y < GridSize; y++)
                {
                    PlayerMoves[x, y] = false;
                }
            }
        }

        private static (int x, int y) GetCoordPair(string coordA, string coordB)
        {
            List<string> aCoords = new List<string>() { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J" };
            List<string> bCoords = new List<string>() { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" };
            return (aCoords.IndexOf(coordA), bCoords.IndexOf(coordB));
        }

        public static (string player, string ai) GetHitPoints()
        {
            return (PlayerHitPoints.ToString(), AIHitPoints.ToString());
        }

        public static string GetTurn()
        {
            return Turn.ToString();
        }

        public static void UpdateDetails(int turn, int stage)
        {
            Turn = turn;
            GameStage = stage;
            ValidPlayerShips = new List<ShipType>();
        }

        public static void SetAllGridCells(CellContent[,] playerCells, CellContent[,] aiCells)
        {
            PlayerHitPoints = 18;
            AIHitPoints = 18;
            for (int x = 0; x < GridSize; x++)
            {
                for (int y = 0; y < GridSize; y++)
                {
                    if (playerCells[x, y] == CellContent.Miss) AI.HasCheckedRef(x, y);
                    if (playerCells[x, y] == CellContent.Hit)
                    {
                        PlayerHitPoints--;
                        AI.HasCheckedRef(x, y);
                    }
                    if (aiCells[x, y] == CellContent.Hit) AIHitPoints--;
                    if (aiCells[x, y] != CellContent.Water && aiCells[x, y] != CellContent.Ship) PlayerMoves[x, y] = true;
                    PlayerGrid.SetCell(x, y, playerCells[x, y]);
                    AIGrid.SetCell(x, y, aiCells[x, y]);
                }
            }
        }

        #region Salvo Methods
        public static MoveResult AttemptPlayerMove(string coordA, string coordB)
        {
            coordA = coordA.ToUpper();
            coordB = coordB.ToUpper();
            if (CheckMoveIsValid(coordA, coordB))
            {
                (int x, int y) coords = GetCoordPair(coordA, coordB);
                if (PlayerMoves[coords.x, coords.y]) return MoveResult.SalvoAlreadyFired;
                else
                {
                    PlayerMoves[coords.x, coords.y] = true;
                    AIGrid.FireSalvoAtCell(coords.x, coords.y);
                    AI.CommenceTurn();
                    Turn += 1;
                    return MoveResult.Success;
                }
            }
            else
            {
                return MoveResult.InvalidCoords;
            }
        }

        public static void HandlePlayerHit()
        {
            PlayerHitPoints -= 1;
            if (PlayerHitPoints == 0) GameStage = 3;
        }

        public static void HandleAIHit()
        {
            AIHitPoints -= 1;
            if (AIHitPoints == 0) GameStage = 4;
        }

        private static bool CheckMoveIsValid(string coordA, string coordB)
        {
            List<string> aCoords = new List<string>() { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J" };
            List<string> bCoords = new List<string>() { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" };
            return aCoords.Contains(coordA) && bCoords.Contains(coordB);
        }
        #endregion

        #region Ship Placement Rules
        public static List<ShipType> ProvideStartingShipList()
        {
            return new List<ShipType>()
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

        public static MoveResult PlacePlayerShip(ShipType type, string pairAX, string pairAY, string pairBX, string pairBY)
        {
            if (CheckMoveIsValid(pairAX.ToUpper(), pairAY) && CheckMoveIsValid(pairBX.ToUpper(), pairBY))
            {
                // Coordinates are both valid
                if ((pairAX == pairBX) || (pairAY == pairBY))
                {
                    // Coordinates are on the same vector
                    (int x, int y) coordA = GetCoordPair(pairAX.ToUpper(), pairAY);
                    (int x, int y) coordB = GetCoordPair(pairBX.ToUpper(), pairBY);
                    int placesInLine = 5 - (int)type;

                    if (Math.Abs(coordA.x - coordB.x) + 1 == placesInLine || Math.Abs(coordA.y - coordB.y) + 1 == placesInLine)
                    {
                        if (!PlayerGrid.CellIsWater(coordA.x, coordA.y)) return MoveResult.SpaceIsOccupied;
                        if (!PlayerGrid.CellIsWater(coordB.x, coordB.y)) return MoveResult.SpaceIsOccupied;

                        if (coordA.x > coordB.x)
                        {
                            // X vector, A > B
                            for (int i = 0; i < placesInLine; i++)
                            {
                                if (!PlayerGrid.CellIsWater(coordA.x - i, coordA.y)) return MoveResult.SpaceIsOccupied;
                            }
                            for (int i = 0; i < placesInLine; i++)
                            {
                                PlayerGrid.PlaceShip(coordA.x - i, coordA.y);
                            }
                        }
                        else if (coordA.x < coordB.x)
                        {
                            // X vector, A < B
                            for (int i = 0; i < placesInLine; i++)
                            {
                                if (!PlayerGrid.CellIsWater(coordA.x + i, coordA.y)) return MoveResult.SpaceIsOccupied;
                            }
                            for (int i = 0; i < placesInLine; i++)
                            {
                                PlayerGrid.PlaceShip(coordA.x + i, coordA.y);
                            }
                        }
                        else if (coordA.y > coordB.y)
                        {
                            // Y vector, A > B
                            for (int i = 0; i < placesInLine; i++)
                            {
                                if (!PlayerGrid.CellIsWater(coordA.x, coordA.y - 1)) return MoveResult.SpaceIsOccupied;
                            }
                            for (int i = 0; i < placesInLine; i++)
                            {
                                PlayerGrid.PlaceShip(coordA.x, coordA.y - i);
                            }
                        }
                        else
                        {
                            // Y vector, A < B
                            for (int i = 0; i < placesInLine; i++)
                            {
                                if (!PlayerGrid.CellIsWater(coordA.x, coordA.y + 1)) return MoveResult.SpaceIsOccupied;
                            }
                            for (int i = 0; i < placesInLine; i++)
                            {
                                PlayerGrid.PlaceShip(coordA.x, coordA.y + i);
                            }
                        }

                        ValidPlayerShips.Remove(type);
                        if (ValidPlayerShips.Count == 0) GameStage = 2;
                        return MoveResult.Success;
                    }
                    else return MoveResult.InvalidCoords;
                }
                else return MoveResult.InvalidCoords;
            }
            else return MoveResult.InvalidCoords;
        }

        public static void PlaceRandomPlayerShips()
        {
            GameStage = 2;

            foreach (ShipType type in ValidPlayerShips)
            {
                int placesInLine = 5 - (int)type;
                bool shipIsPlaced = false;
                while (!shipIsPlaced)
                {
                    shipIsPlaced = AttemptPlacement(placesInLine);
                }
            }
        }

        private static bool AttemptPlacement(int placesInLine)
        {
            int randX = Rand.Next(GridSize);
            int randY = Rand.Next(GridSize);

            bool verticalAlign = Rand.Next(0, 2) < 1;
            if (verticalAlign && randY + placesInLine < GridSize)
            {
                for (int i = 0; i < placesInLine; i++)
                {
                    if (!PlayerGrid.CellIsWater(randX, randY + i)) return false;
                }
                for (int i = 0; i < placesInLine; i++)
                {
                    PlayerGrid.PlaceShip(randX, randY + i);
                }
                return true;
            }
            else if (!verticalAlign && randX + placesInLine < GridSize)
            {
                for (int i = 0; i < placesInLine; i++)
                {
                    if (!PlayerGrid.CellIsWater(randX + i, randY)) return false;
                }

                for (int i = 0; i < placesInLine; i++)
                {
                    PlayerGrid.PlaceShip(randX + i, randY);
                }
                return true;
            }
            else return false;
        }
        #endregion
    }
}
