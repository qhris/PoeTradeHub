using System.Collections.Generic;

namespace PoeTradeHub
{
    public class ItemStatDataGroup
    {
        public string Label { get; set; }
        public IList<ItemStatData> Entries { get; set; }
    }
}
