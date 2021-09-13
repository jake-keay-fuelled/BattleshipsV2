using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleshipsV2.Data
{
    public static class AI
    {
        private readonly static Random Rand = new Random(Guid.NewGuid().GetHashCode());
        private static int GridSizeRef;

        // Targeting Solution Data
        private static bool isSearching = true;
        private static bool hasVector = false;
        private static int vectorX;
        private static int vectorY;
        private static int targetX;
        private static int targetY;
        private static bool vectorIsVertical;
        private static bool hasReachedFirstEndOfVector;
        private static Dictionary<(int x, int y), bool> searchedVector = new Dictionary<(int x, int y), bool>();

        public static void HandleShipPlacement(List<ShipType> shipTypes)
        {
            foreach (ShipType type in shipTypes)
            {
                int placesInLine = 5 - (int)type;
                bool shipIsPlaced = false;
                while (!shipIsPlaced)
                {
                    shipIsPlaced = AttemptPlacement(placesInLine);
                }
            }
        }

        #region Game Management Methods
        public static void CommenceTurn()
        {
            // Has not scored a hit
            if (isSearching && !hasVector)
            {
                FireAtRandomCell();
            }
            // Scored a hit, moving clockwise
            else if (!isSearching && !hasVector)
            {
                FireAtNextClockwiseCell();
            }
            // Scored two hits, extrapolated vector
            else
            {
                FireAtNextCellOnVector();
            }
        }

        public static void Reset()
        {
            isSearching = true;
            hasVector = false;
            hasReachedFirstEndOfVector = false;
            searchedVector = new Dictionary<(int x, int y), bool>();
        }

        public static void SetGridSizeRef(int gridSizeRef)
        {
            GridSizeRef = gridSizeRef;
            for (int x = 0; x < GridSizeRef; x++)
            {
                for (int y = 0; y < GridSizeRef; y++)
                {
                    searchedVector.Add((x, y), false);
                }
            }
        }

        public static void HasCheckedRef(int x, int y)
        {
            searchedVector[(x, y)] = true;
        }
        #endregion

        #region Salvo Methods
        public static void FireAtRandomCell()
        {
            Grid grid = GridHandler.PlayerGrid;

            int randX;
            int randY;

            bool coordIsInMemory;

            do
            {
                randX = Rand.Next(GridSizeRef);
                randY = Rand.Next(GridSizeRef);
                coordIsInMemory = searchedVector[(randX, randY)];
            } while (coordIsInMemory);

            searchedVector[(randX, randY)] = true;

            CellContent salvoResult = grid.FireSalvoAtCell(randX, randY);

            if (salvoResult == CellContent.Hit)
            {
                vectorX = randX;
                vectorY = randY;
                isSearching = false;
            }
        }

        public static void FireAtNextClockwiseCell()
        {
            Grid grid = GridHandler.PlayerGrid;

            // Cell above
            if (CoordIsWithinBounds(vectorY + 1) && !searchedVector[(vectorX, vectorY + 1)])
            {
                searchedVector[(vectorX, vectorY + 1)] = true;
                CellContent salvoResult = grid.FireSalvoAtCell(vectorX, vectorY + 1);

                if (salvoResult == CellContent.Hit)
                {
                    targetX = vectorX;
                    targetY = vectorY + 2 < GridSizeRef ? vectorY + 2 : vectorY - 1;
                    hasVector = true;
                    vectorIsVertical = true;
                }
            }

            // Cell right
            else if (CoordIsWithinBounds(vectorX + 1) && !searchedVector[(vectorX + 1, vectorY)])
            {
                searchedVector[(vectorX + 1, vectorY)] = true;
                CellContent salvoResult = grid.FireSalvoAtCell(vectorX + 1, vectorY);

                if (salvoResult == CellContent.Hit)
                {
                    targetX = vectorX + 2 < GridSizeRef ? vectorX + 2 : vectorX - 1;
                    targetY = vectorY;
                    hasVector = true;
                    vectorIsVertical = false;
                }
            }

            // Cell below
            else if (CoordIsWithinBounds(vectorY - 1) && !searchedVector[(vectorX, vectorY - 1)])
            {
                searchedVector[(vectorX, vectorY - 1)] = true;
                CellContent salvoResult = grid.FireSalvoAtCell(vectorX, vectorY - 1);

                if (salvoResult == CellContent.Hit)
                {
                    targetX = vectorX;
                    targetY = vectorY - 1 >= 0 ? vectorY - 1 : vectorY + 2;
                    hasVector = true;
                    vectorIsVertical = true;
                }
            }

            // Cell left
            else if (CoordIsWithinBounds(vectorX - 1) && !searchedVector[(vectorX - 1, vectorY)])
            {
                searchedVector[(vectorX - 1, vectorY)] = true;
                CellContent salvoResult = grid.FireSalvoAtCell(vectorX - 1, vectorY);

                if (salvoResult == CellContent.Hit)
                {
                    targetX = vectorX - 2 >= 0 ? vectorX - 2 : vectorX + 1;
                    targetY = vectorY;
                    hasVector = true;
                    vectorIsVertical = true;
                }
            }

            // No more cells found, return to random firing
            else
            {
                isSearching = true;
                hasVector = false;
                hasReachedFirstEndOfVector = false;
                FireAtRandomCell();
            }
        }

        public static void FireAtNextCellOnVector()
        {
            Grid grid = GridHandler.PlayerGrid;

            if (!CoordIsWithinBounds(targetX) || !CoordIsWithinBounds(targetY) || searchedVector[(targetX, targetY)])
            {
                // There has been a targeting error; reset targeting
                isSearching = true;
                hasVector = false;
                hasReachedFirstEndOfVector = false;
                FireAtRandomCell();
            }
            else
            {
                searchedVector[(targetX, targetY)] = true;
                if (vectorIsVertical)
                {
                    // Vertical vector
                    CellContent salvoResult = grid.FireSalvoAtCell(targetX, targetY);

                    if (salvoResult == CellContent.Hit)
                    {
                        if (targetY < vectorY)
                        {
                            // Target may be below vector or out of bounds
                            if (CoordIsWithinBounds(targetY - 1))
                            {
                                // Target is below vector
                                targetX = vectorX;
                                targetY -= 1;
                            }
                            else
                            {
                                // Reverse direction and fire above vector
                                targetX = vectorX;
                                targetY = vectorY + 1;
                                hasReachedFirstEndOfVector = true;
                            }
                        }
                        else
                        {
                            // Target may be above vector or out of bounds
                            if (CoordIsWithinBounds(targetY + 1))
                            {
                                // Target is above vector
                                targetX = vectorX;
                                targetY += 1;
                            }
                            else
                            {
                                // Reverse direction and fire below vector
                                targetX = vectorX;
                                targetY = vectorY - 1;
                                hasReachedFirstEndOfVector = true;
                            }
                        }
                    }
                    else
                    {
                        // Salvo has missed, check if has reached first end of vector
                        if (hasReachedFirstEndOfVector)
                        {
                            // Ship has been sunk, resume random firing
                            isSearching = true;
                            hasVector = false;
                            hasReachedFirstEndOfVector = false;
                        }
                        else
                        {
                            // Reverse direction based on whether coord above vector has been searched
                            hasReachedFirstEndOfVector = true;
                            if (CoordIsWithinBounds(vectorY + 1) && searchedVector[(vectorX, vectorY + 1)])
                            {
                                // Fire below vector
                                targetX = vectorX;
                                targetY = vectorY - 1;
                            }
                            else
                            {
                                if (!CoordIsWithinBounds(vectorY - 1) || searchedVector[(vectorY - 1, vectorY)])
                                {
                                    isSearching = true;
                                    hasVector = false;
                                    hasReachedFirstEndOfVector = false;
                                }
                                else
                                {
                                    // Fire above vector
                                    targetX = vectorX;
                                    targetY = vectorY + 1;
                                }
                            }
                        }
                    }
                }
                else
                {
                    // Horizontal vector
                    CellContent salvoResult = grid.FireSalvoAtCell(targetX, targetY);

                    if (salvoResult == CellContent.Hit)
                    {
                        if (targetX < vectorX)
                        {
                            // Target may be left of current target or out of bounds
                            if (CoordIsWithinBounds(targetX - 1))
                            {
                                // Target is left of vector
                                targetX -= 1;
                                targetY = vectorY;
                            }
                            else
                            {
                                // Reverse direction and fire right of vector
                                targetX = targetX + 1;
                                targetY = vectorY;
                                hasReachedFirstEndOfVector = true;
                            }
                        }
                        else
                        {
                            // Target may be right of vector or out of bounds
                            if (CoordIsWithinBounds(targetX + 1))
                            {
                                // Target is right of vector
                                targetX += 1;
                                targetY = vectorY;
                            }
                            else
                            {
                                // Reverse direction and fire left of vector
                                targetX = vectorX - 1;
                                targetY = vectorY;
                                hasReachedFirstEndOfVector = true;
                            }
                        }
                    }
                    else
                    {
                        // Salvo has missed, check if has reached first end of vector
                        if (hasReachedFirstEndOfVector)
                        {
                            // Ship has been sunk, resume random firing
                            isSearching = true;
                            hasVector = false;
                            hasReachedFirstEndOfVector = false;
                        }
                        else
                        {
                            // Reverse direction based on whether coord right of vector has been searched
                            hasReachedFirstEndOfVector = true;
                            if (CoordIsWithinBounds(vectorX + 1) && searchedVector[(vectorX + 1, vectorY)])
                            {
                                // Fire left of vector
                                targetX = vectorX - 1;
                                targetY = vectorY;
                            }
                            else
                            {
                                if (!CoordIsWithinBounds(vectorX - 1) || searchedVector[(vectorX - 1, vectorY)])
                                {
                                    isSearching = true;
                                    hasVector = false;
                                    hasReachedFirstEndOfVector = false;
                                }
                                else
                                {
                                    // Fire right of vector
                                    targetX = vectorX + 1;
                                    targetY = vectorY;
                                }
                            }
                        }
                    }
                }
            }
        }
        #endregion

        private static bool CoordIsWithinBounds(int coord)
        {
            return (coord >= 0 && coord < GridSizeRef);
        }

        public static bool AttemptPlacement(int placesInLine)
        {
            Grid grid = GridHandler.AIGrid;

            int randX = Rand.Next(GridSizeRef);
            int randY = Rand.Next(GridSizeRef);

            bool verticalAlign = Rand.Next(0, 2) < 1;
            if (verticalAlign && randY + placesInLine < GridSizeRef)
            {
                for (int i = 0; i < placesInLine; i++)
                {
                    if (!grid.CellIsWater(randX, randY + i)) return false;
                }
                for (int i = 0; i < placesInLine; i++)
                {
                    grid.PlaceShip(randX, randY + i);
                }
                return true;
            }
            else if (!verticalAlign && randX + placesInLine < GridSizeRef)
            {
                for (int i = 0; i < placesInLine; i++)
                {
                    if (!grid.CellIsWater(randX + i, randY)) return false;
                }

                for (int i = 0; i < placesInLine; i++)
                {
                    grid.PlaceShip(randX + i, randY);
                }
                return true;
            }
            else return false;
        }
    }
}
    