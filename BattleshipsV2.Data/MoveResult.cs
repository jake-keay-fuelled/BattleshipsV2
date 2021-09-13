using System;
using System.Collections.Generic;
using System.Text;

namespace BattleshipsV2.Data
{
    public enum MoveResult
    {
        Success,
        InvalidCoords,
        SpaceIsOccupied,
        SalvoAlreadyFired
    }
}
