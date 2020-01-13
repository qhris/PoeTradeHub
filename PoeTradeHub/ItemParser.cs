using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace PoeTradeHub
{
    public class ItemParser
    {
        private static object _initializationLock = new object();
        private static IList<MapAffixLookupEntry> _mapAffixLookup;
        private static IReadOnlyDictionary<string, int> _mapAffixGroupLookup;

        class MapAffixLookupEntry
        {
            public string Pattern { get; set; }
            public string StatId { get; set; }
        }

        public ItemParser()
        {
            lock (_initializationLock)
            {
                if (_mapAffixLookup == null)
                {
                    var itemStatFile = Path.Combine(Directory.GetCurrentDirectory(), @"Data/stats.json");
                    var itemStats = JsonConvert.DeserializeObject<ItemStatDataResult>(File.ReadAllText(itemStatFile));
                    _mapAffixLookup = BuildMapAffixLookupTable(itemStats.Result.Where(x => x.Label == "Explicit").First());
                    _mapAffixGroupLookup = BuildMapAffixGroupLookup();
                }
            }
        }

        private IList<MapAffixLookupEntry> BuildMapAffixLookupTable(ItemStatDataGroup statList)
        {
            var lookupTable = new List<MapAffixLookupEntry>();

            foreach (var statInfo in statList.Entries)
            {
                var modDescription = Regex
                    .Escape(statInfo.Text)
                    .Replace("\\#", "(\\d+)");

                lookupTable.Add(new MapAffixLookupEntry
                {
                    Pattern = $"^{modDescription}$",
                    StatId = statInfo.Id,
                });
            }

            return lookupTable;
        }

        private IReadOnlyDictionary<string, int> BuildMapAffixGroupLookup()
        {
            var lookupTable = new Dictionary<string, int>();
            Action<IList<string>> addGroup = affixes =>
            {
                var groupId = lookupTable.Count + 1;
                foreach (var affix in affixes)
                {
                    lookupTable.Add(affix, groupId);
                }
            };

            addGroup(new string[]
            {
                // Monster crit chance and crit multi (Deadliness).
                "explicit.stat_57326096",
                "explicit.stat_2753083623",
            });

            return lookupTable;
        }

        public ItemInformation Parse(string itemText)
        {
            IList<string> itemData = itemText.Split(new string[] { "\r\n", "\n" },
                StringSplitOptions.RemoveEmptyEntries);
            IList<IList<string>> groups = ParseGroups(itemData);

            if (groups.Count < 2)
            {
                throw new InvalidItemException("Invalid item format");
            }

            var item = InitialPass(itemData, groups);

            if (item.ItemType == ItemType.Unknown)
            {
                ParseItemType(item, itemData, groups);
            }

            if (item.ItemType == ItemType.Map)
            {
                item.MapTier = GetMapTier(itemData);

                if (item.Rarity >= ItemRarity.Magic && item.IsIdentified)
                {
                    ParseMapAffixes(item, itemData, groups);

                    if (item.Rarity == ItemRarity.Magic)
                    {
                        // Strip off magic prefix names.
                        var hasSuffix = Regex.IsMatch(item.Name, @"^.*\s+of\s+.*$");
                        var prefixCount = item.GroupedAffixes.Count - Convert.ToInt32(hasSuffix);
                        if (prefixCount > 0)
                        {
                            item.BaseType = Regex.Replace(item.BaseType, @"^\w[\w\']+\s+", string.Empty);
                        }
                    }
                }

                item.IsBlighted = Regex.IsMatch(item.BaseType, @"^Blighted\s+.*$");
            }

            if (item.ItemType == ItemType.DivinationCard)
            {
                // ParseDivinationCardStack(item, groups);
            }

            return item;
        }

        private void ParseItemType(ItemInformation item, IList<string> itemData, IList<IList<string>> groups)
        {
            if (Regex.IsMatch(item.BaseType, @"Divination\s*Card"))
            {
                item.ItemType = ItemType.DivinationCard;
                return;
            }

            // Strip off quality names: "Superior Thicket Bow" becomes "Thicket Bow"
            // These names appear on normal rarity items or unidentifed items with quality.
            if ((item.Rarity == ItemRarity.Normal || !item.IsIdentified) && (item.Quality ?? 0) > 0)
            {
                item.BaseType = Regex.Replace(item.Name, @"^Superior\s+", string.Empty);
            }

            // Strip off magic suffix names: "Jungle Valley Map of Smothering" becomes "Jungle Valley Map"
            if (item.Rarity == ItemRarity.Magic && item.IsIdentified)
            {
                item.BaseType = Regex.Replace(item.Name, @"\s+of\s+\w[\w\']+$", "", RegexOptions.IgnoreCase);
            }

            const string isMapPattern = @"^.*\s+Map$";
            if (Regex.IsMatch(item.BaseType, isMapPattern))
            {
                item.ItemType = ItemType.Map;
            }
        }

        private void ParseMapAffixes(ItemInformation item, IList<string> itemData, IList<IList<string>> groups)
        {
            if (!item.IsIdentified)
            {
                return;
            }

            ParseRawMapAffixes(item, itemData, groups);

            if (!item.RawAffixes.Any())
            {
                return;
            }

            ParseAffixes(item);
            ParseAffixGroups(item);
        }

        private void ParseAffixes(ItemInformation item)
        {
            var itemAffixes = new List<ItemAffix>();

            foreach (var affix in item.RawAffixes)
            {
                foreach (var lookup in _mapAffixLookup)
                {
                    var match = Regex.Match(affix, lookup.Pattern, RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        var values = match.Groups
                            .Cast<Group>()
                            .Skip(1)
                            .Select(g =>
                            {
                                if (int.TryParse(g.Value, out int value))
                                {
                                    return value;
                                }

                                return 0;
                            })
                            .ToList();

                        itemAffixes.Add(new ItemAffix
                        {
                            Text = affix,
                            StatId = lookup.StatId,
                            Values = values,
                        });
                    }
                }
            }

            item.Affixes = itemAffixes;
        }

        private void ParseAffixGroups(ItemInformation item)
        {
            var affixGroups = new Dictionary<int, List<ItemAffix>>();
            var standalone = new List<ItemAffix>();

            foreach (var affix in item.Affixes)
            {
                if (_mapAffixGroupLookup.TryGetValue(affix.StatId, out int groupId))
                {
                    List<ItemAffix> groupAffixes;
                    if (!affixGroups.TryGetValue(groupId, out groupAffixes))
                    {
                        groupAffixes = new List<ItemAffix>();
                        affixGroups[groupId] = groupAffixes;
                    }

                    groupAffixes.Add(affix);
                }
                else
                {
                    standalone.Add(affix);
                }
            }

            var groupedAffixes = new List<IList<ItemAffix>>();
            foreach (var group in affixGroups)
            {
                groupedAffixes.Add(group.Value);
            }

            foreach (var affix in standalone)
            {
                groupedAffixes.Add(new ItemAffix[] { affix });
            }

            item.GroupedAffixes = groupedAffixes;
        }

        private void ParseRawMapAffixes(ItemInformation item, IList<string> itemData, IList<IList<string>> groups)
        {
            Func<IList<string>, bool> isPotentialAffixGroup = group =>
            {
                foreach (var line in group)
                {
                    if (line.StartsWith("Map Tier:"))
                    {
                        return false;
                    }

                    if (line.StartsWith("Item Level:"))
                    {
                        return false;
                    }

                    if (line.StartsWith("Corrupted"))
                    {
                        return false;
                    }

                    if (line.Contains("Map Device"))
                    {
                        return false;
                    }
                }

                return true;
            };

            Func<IEnumerable<IList<string>>, int> skipImplicitGroup = groupsToCheck =>
            {
                int currentIndex = 0;

                foreach (var group in groupsToCheck)
                {
                    foreach (var line in group)
                    {
                        if (line.EndsWith("(implicit)"))
                        {
                            return currentIndex + 1;
                        }
                    }

                    currentIndex += 1;
                }

                return 0;
            };

            var consideredGroups = groups
                .Skip(1)
                .Where(g => isPotentialAffixGroup(g));
            consideredGroups = consideredGroups
                .Skip(skipImplicitGroup(consideredGroups));

            if (consideredGroups.Any())
            {
                item.RawAffixes = consideredGroups.First();
            }
        }

        private IList<IList<string>> ParseGroups(IList<string> itemData)
        {
            var groups = new List<IList<string>>();
            var currentGroup = new List<string>();

            foreach (var line in itemData)
            {
                if (line.StartsWith("---"))
                {
                    if (currentGroup.Count > 0)
                    {
                        groups.Add(currentGroup);
                        currentGroup = new List<string>();
                    }

                    continue;
                }
                currentGroup.Add(line);
            }

            if (currentGroup.Count > 0)
            {
                groups.Add(currentGroup);
            }

            return groups;
        }

        /// <summary>
        /// Does the initial parsing pass to figure out the utmost basics before
        /// the item can be categorized for further parsing.
        /// </summary>
        private ItemInformation InitialPass(IList<string> itemData, IList<IList<string>> groups)
        {
            var item = new ItemInformation();

            item.IsIdentified = GetIdentified(itemData);
            item.ItemLevel = GetItemLevel(itemData);
            item.IsCorrupted = GetCorrupted(itemData);
            item.Quality = GetQuality(itemData);

            var itemHeader = groups[0];
            var rarityMatch = Regex.Match(itemHeader[0], @"^\s*Rarity:\s+(.*)\s*$");
            if (!rarityMatch.Success)
            {
                throw new InvalidItemException("Invalid item header");
            }

            var rarityText = rarityMatch.Groups[1].Value;
            if (Enum.TryParse(rarityText, out ItemRarity rarity))
            {
                item.Rarity = rarity;
            }

            if (item.Rarity != null)
            {
                item.ItemType = ItemType.Unknown;

                switch (groups[0].Count)
                {
                    case 2:
                        item.Name = itemData[1];
                        item.BaseType = itemData[1];
                        break;
                    case 3:
                        item.Name = itemData[1];
                        item.BaseType = itemData[2];
                        break;
                    default:
                        break;
                }
            }
            else
            {
                item.Name = itemData[1];
                item.BaseType = rarityText;
            }

            return item;
        }

        private void ParseDivinationCardStack(ItemInformation item, IList<IList<string>> groups)
        {
            if (item.ItemType != ItemType.DivinationCard)
            {
                throw new ArgumentException("Item must be a card type", nameof(item));
            }

            string stackInfo = groups
                .Skip(1)
                .Where(g => g.Count == 1)
                .Select(g => g.First())
                .Where(l => l.StartsWith("Stack Size:"))
                .FirstOrDefault();

            if (stackInfo != null)
            {
                var match = Regex.Match(stackInfo, @"Stack\s*Size\:\s+(\d+)\/(\d+)");
                if (match.Success)
                {
                    item.Stack = int.Parse(match.Groups[1].Value);
                    item.StackSize = int.Parse(match.Groups[2].Value);

                    return;
                }
            }

            item.Stack = 1;
            item.StackSize = 1;
        }

        private int? GetQuality(IList<string> itemData)
        {
            foreach (var line in itemData)
            {
                if (line.StartsWith("Quality:"))
                {
                    var match = Regex.Match(line, @"Quality:\s+\+(\d+)%.*");
                    if (match.Success)
                    {
                        if (int.TryParse(match.Groups[1].Value, out int result))
                        {
                            return result;
                        }
                    }
                }
            }

            return null;
        }

        private bool GetIdentified(IEnumerable<string> itemData)
        {
            return itemData.Contains("Unidentified") == false;
        }

        private bool GetCorrupted(IEnumerable<string> itemData)
        {
            return itemData.Contains("Corrupted");
        }

        private int? GetItemLevel(IEnumerable<string> itemData)
        {
            foreach (var line in itemData)
            {
                var match = Regex.Match(line, @"^\s*ItemLevel:\s+(\d+)\s*$");
                if (match.Success)
                {
                    if (int.TryParse(match.Groups[1].Value, out int value))
                    {
                        return value;
                    }
                }
            }

            return null;
        }

        private int? GetMapTier(IEnumerable<string> itemData)
        {
            foreach (var line in itemData)
            {
                var match = Regex.Match(line, @"^\s*Map\s?Tier:\s+(\d+)\s*$");
                if (match.Success)
                {
                    if (int.TryParse(match.Groups[1].Value, out int value))
                    {
                        return value;
                    }
                }
            }

            return null;
        }

        public static string DebugItem(ItemInformation item)
        {
            return JsonConvert.SerializeObject(item, Formatting.Indented);
        }
    }
}
