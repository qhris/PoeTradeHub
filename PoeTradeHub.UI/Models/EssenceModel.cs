using System.Collections.Generic;

namespace PoeTradeHub.UI.Models
{
    public class EssenceModel
    {
        public int? ItemLevelRestriction { get; set; }
        public int? DropLevelMin { get; set; }
        public int? DropLevelMax { get; set; }
        public bool IsDroppable { get; set; }

        public IList<EssenceModifierModel> Modifiers { get; set; }
    }
}
