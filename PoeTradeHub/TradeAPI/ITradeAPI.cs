using System.Collections.Generic;
using System.Threading.Tasks;
using PoeTradeHub.TradeAPI.Models;

namespace PoeTradeHub.TradeAPI
{
    public interface ITradeAPI
    {
        /// <summary>
        /// Queries the official API for a list of current league names.
        /// Private leagues won't be returned unless the user is logged in.
        /// </summary>
        /// <returns>A list of available leagues. List is empty if request failed.</returns>
        Task<IList<LeagueInfo>> QueryLeagues();

        Task<IList<ItemRecord>> QueryPrice(string leagueName, ItemInformation item);
    }
}
