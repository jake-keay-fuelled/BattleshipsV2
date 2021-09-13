using System;
using System.Collections.Generic;
using System.Text;

namespace BattleshipsV2.Data
{
    public interface IJsonData
    {
        CellContent[,] GetPlayerGridContent();
        CellContent[,] GetAIGridContent();
        int GetNumberOfTurns();
        int GetGameStage();
        bool GetIsActive();
        bool ResetAllData();
        int Commit();
    }
}
