using Newtonsoft.Json;

namespace PoeTradeHub.TradeAPI.Models
{
    public class ItemData
    {
        public bool Verified { get; set; }
        [JsonProperty("w")]
        public int Width { get; set; }
        [JsonProperty("h")]
        public int Height { get; set; }
        public string Icon { get; set; }
        public string League { get; set; }
        public string Name { get; set; }
        public string TypeLine { get; set; }
        public bool Identified { get; set; }
        [JsonProperty("ilvl")]
        public int ItemLevel { get; set; }
    }
}
