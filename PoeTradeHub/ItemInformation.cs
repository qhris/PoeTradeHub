using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PoeTradeHub
{
    public class ItemInformation
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public ItemType ItemType { get; set; }
        public string Name { get; set; }
        public string BaseType { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public ItemRarity? Rarity { get; set; }
        public bool IsIdentified { get; set; }
        public int? ItemLevel { get; set; }
        public int? MapTier { get; set; }
        public bool IsCorrupted { get; set; }
        public string FlavorText { get; set; }
        public IList<string> RawAffixes { get; set; }
        public int? Quality { get; set; }
        public bool? IsBlighted { get; set; }
        internal IList<ItemAffix> Affixes { get; set; }
        internal IList<IList<ItemAffix>> GroupedAffixes { get; set; }
    }
}
