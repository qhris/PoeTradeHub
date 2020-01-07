using System;
using System.Collections.Generic;
using Caliburn.Micro;
using PoeTradeHub.TradeAPI.Models;
using PoeTradeHub.UI.Models;

namespace PoeTradeHub.UI.ViewModels
{
    public class ItemListingViewModel : Screen
    {
        public ItemListingViewModel(IList<ItemRecord> listings)
        {
            Listings = TransformListings(listings);
        }

        public IList<ItemListingModel> Listings { get; }

        private IList<ItemListingModel> TransformListings(IList<ItemRecord> oldListings)
        {
            var listings = new List<ItemListingModel>();

            foreach (var listing in oldListings)
            {
                ItemListingPrice price = listing.Listing.Price;
                string priceTag = null;

                if (price != null)
                {
                    priceTag = $"{price.Amount} {price.Currency}";
                }

                listings.Add(new ItemListingModel
                {
                    Account = listing.Listing.Account,
                    Item = listing.Item,
                    CurrencyType = price?.Currency,
                    HasPriceTag = price != null,
                    PriceTag = priceTag,
                });
            }

            return listings;
        }
    }
}
