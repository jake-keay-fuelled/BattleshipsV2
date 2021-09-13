using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleshipsV2.Data
{
    public class Grid
    {
        private bool IsPlayer;
        private CellContent[,] Cells;

        public Grid(int gridSize, bool isPlayer)
        {
            IsPlayer = isPlayer;
            Cells = new CellContent[gridSize, gridSize];

            for (int x = 0; x < gridSize; x++)
            {
                for (int y = 0; y < gridSize; y++)
                {
                    SetCell(x, y, CellContent.Water);
                }
            }
        }

        public CellContent[,] GetCells()
        {
            return Cells;
        }

        public void SetCell(int targetX, int targetY, CellContent content)
        {
            Cells[targetX, targetY] = content;
        }

        public bool CellIsWater(int targetX, int targetY)
        {
            return Cells[targetX, targetY] == CellContent.Water;
        }

        public CellContent FireSalvoAtCell(int targetX, int targetY)
        {
            if (Cells[targetX, targetY] == CellContent.Ship)
            {
                SetCell(targetX, targetY, CellContent.Hit);
                if (IsPlayer) GridHandler.HandlePlayerHit();
                else GridHandler.HandleAIHit();
                return CellContent.Hit;
            }
            else
            {
                SetCell(targetX, targetY, CellContent.Miss);
                return CellContent.Miss;
            }
        }

        public void PlaceShip(int targetX, int targetY)
        {
            SetCell(targetX, targetY, CellContent.Ship);
        }
    }
}
