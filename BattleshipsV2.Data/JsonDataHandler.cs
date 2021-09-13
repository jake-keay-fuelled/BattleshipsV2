using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Newtonsoft;
using Newtonsoft.Json;

namespace BattleshipsV2.Data
{
    public class JsonDataHandler : IJsonData
    {
        private const string FileName = @"C:\dev\BattleshipsV2\BattleshipsV2.Data\gamedata.json";
        private JsonData Data;

        public JsonDataHandler()
        {
            Data = JsonConvert.DeserializeObject<JsonData>(File.ReadAllText(FileName));
        }

        public CellContent[,] GetPlayerGridContent()
        {
            return Data.PlayerGridContent;
        }

        public CellContent[,] GetAIGridContent()
        {
            return Data.AIGridContent;
        }

        public int GetNumberOfTurns()
        {
            return Data.TurnNumber;
        }

        public int GetGameStage()
        {
            return Data.Stage;
        }

        public bool GetIsActive()
        {
            return Data.IsActive;
        }

        public bool ResetAllData()
        {
            Data = new JsonData();
            Data.IsActive = false;

            using (StreamWriter file = File.CreateText(FileName))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, Data);
            }

            return false;
        }

        public int Commit()
        {
            Data.PlayerGridContent = GridHandler.PlayerGrid.GetCells();
            Data.AIGridContent = GridHandler.AIGrid.GetCells();
            Data.TurnNumber = GridHandler.Turn;
            Data.Stage = GridHandler.GameStage;
            Data.IsActive = true;

            using (StreamWriter file = File.CreateText(FileName))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, Data);
            }
            return Data.Stage;
        }

        class JsonData
        {
            public CellContent[,] PlayerGridContent { get; set; }
            public CellContent[,] AIGridContent { get; set; }
            public int TurnNumber { get; set; }
            public int Stage { get; set; }
            public bool IsActive { get; set; }
        }
    }
}
