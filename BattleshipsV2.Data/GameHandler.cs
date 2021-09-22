using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BattleshipsV2.Data
{
    public class GameHandler : IGameHandler
    {
        private bool[,] AIHasFiredAtCell;
        private readonly Random Rand = new Random();
        private bool IsActive = false;

        public void Initialise(IGridData gridData)
        {
            IsActive = true;
            AIHasFiredAtCell = new bool[10, 10];

            for (int x = 0; x < 10; x++)
            {
                for (int y = 0; y < 10; y++)
                {
                    CellContent content = gridData.GetPlayerGrid()[gridData.GetPlayerCoords().FirstOrDefault(c => c.X == x && c.Y == y)];
                    AIHasFiredAtCell[x, y] = content == CellContent.Hit || content == CellContent.Miss;
                }
            }
        }

        public bool CheckIsActive()
        {
            return IsActive;
        }

        public MoveResult HandlePlayerMove(string coord, IGridData gridData)
        {
            if (!StringIsValid(coord)) return MoveResult.InvalidCoords;

            MoveResult result = gridData.FireAtCellOnAIGrid(coord[0].ToString(), coord[1..]);

            if (result == MoveResult.LastAIHitPointTaken)
            {
                // AI CANNOT fire back
                return MoveResult.LastAIHitPointTaken;
            }
            else if (result == MoveResult.SpaceIsOccupied)
            {
                // AI CANNOT fire back
                return MoveResult.SpaceIsOccupied;
            }
            else
            {
                // AI CAN fire back
                MoveResult aiResult = AIReturnFire(gridData);
                if (aiResult == MoveResult.LastPlayerHitPointTaken) return MoveResult.LastPlayerHitPointTaken;
            }

            return MoveResult.Success;
        }

        private MoveResult AIReturnFire(IGridData gridData)
        {
            int x, y;
            do
            {
                x = Rand.Next(10);
                y = Rand.Next(10);
            } while (AIHasFiredAtCell[x, y]);

            AIHasFiredAtCell[x, y] = true;
            return gridData.FireAtCellOnPlayerGrid(x, y);
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
    }
}
