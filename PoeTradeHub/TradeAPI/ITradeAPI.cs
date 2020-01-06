using System.Collections.Generic;
using System.Threading.Tasks;
using PoeTradeHub.TradeAPI.Models;

namespace PoeTradeHub.TradeAPI
{
    public interface ITradeAPI
    {
        Task<IList<ItemRecord>> QueryPrice(ItemQuery query);
    }
}
