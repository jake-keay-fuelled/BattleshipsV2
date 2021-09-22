using System;
using System.Collections.Generic;
using System.Text;

namespace BattleshipsV2.Data
{
    public interface IGameHandler
    {
        void Initialise(IGridData gridData);
        bool CheckIsActive();
        MoveResult HandlePlayerMove(string coord, IGridData gridData);
    }
}
