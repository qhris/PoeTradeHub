using Newtonsoft.Json;

namespace PoeTradeHub
{
    public static class JsonUtility
    {
        public static T Deserialize<T>(string value) =>
            JsonConvert.DeserializeObject<T>(value);

        public static string Serialize<T>(T value, bool isPretty = false) =>
            JsonConvert.SerializeObject(value, isPretty ? Formatting.Indented : Formatting.None);
    }
}
