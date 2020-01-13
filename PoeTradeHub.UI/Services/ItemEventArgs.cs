using System;

namespace PoeTradeHub.UI.Services
{
    public class ItemEventArgs : EventArgs
    {
        public ItemEventArgs(ItemInformation item)
        {
            Item = item;
        }

        public ItemInformation Item { get; }
    }
}
