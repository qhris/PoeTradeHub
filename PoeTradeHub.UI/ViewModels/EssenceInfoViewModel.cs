using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Caliburn.Micro;
using PoeTradeHub.UI.Models;

namespace PoeTradeHub.UI.ViewModels
{
    public class EssenceInfoViewModel : Screen
    {
        private static IDictionary<string, EssenceModel> s_essenceDataLookup;

        public EssenceInfoViewModel(ItemInformation item)
        {
            Item = item;
            Data = LookupEssenceData(item);
        }

        public ItemInformation Item { get; }
        public EssenceModel Data { get; }

        private EssenceModel LookupEssenceData(ItemInformation item)
        {
            if (item == null || item.ItemType != ItemType.Currency && !item.Name.Contains("Essence of"))
            {
                throw new ArgumentException("Invalid item", nameof(item));
            }

            EnsureEssenceDataIsLoaded();

            if (s_essenceDataLookup.TryGetValue(item.Name, out EssenceModel value))
            {
                return value;
            }

            throw new NotImplementedException($"Information for essence type '{item.Name}' is not implemented");
        }

        private static void EnsureEssenceDataIsLoaded()
        {
            if (s_essenceDataLookup != null)
            {
                return;
            }

            string dataPath = $@"{AppDomain.CurrentDomain.BaseDirectory}\Data\processed_essences.json";
            string dataText = File.ReadAllText(dataPath, Encoding.UTF8);
            s_essenceDataLookup = JsonUtility.Deserialize<IDictionary<string, EssenceModel>>(dataText);
        }

        public class EssenceDataFormatModifiers
        {
            public string Type { get; set; }
            public int ItemLevel { get; set; }
        }
    }
}
