using System;
using System.Collections.Generic;
using System.Text;

namespace BattleshipsV2.Data
{
    public interface IJsonData
    {
        void Commit(GameDataFormat gameData);
        GameDataFormat Retrieve();
        void Reset();
    }
}
