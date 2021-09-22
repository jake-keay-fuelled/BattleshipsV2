using Newtonsoft.Json;
using System.IO;

namespace BattleshipsV2.Data
{
    public class JsonData : IJsonData
    {
        private readonly string FileName = @"C:\dev\BattleshipsV2\BattleshipsV2.Data\data.json";

        public async void Commit(GameDataFormat gameData)
        {
            string json = JsonConvert.SerializeObject(gameData, Formatting.Indented);
            await File.WriteAllTextAsync(FileName, json);
        }

        public GameDataFormat Retrieve()
        {
            GameDataFormat gameData = JsonConvert.DeserializeObject<GameDataFormat>(File.ReadAllText(FileName));
            return gameData;
        }

        public async void Reset()
        {
            string json = JsonConvert.SerializeObject(new GameDataFormat(), Formatting.Indented);
            await File.WriteAllTextAsync(FileName, json);
        }
    }
}