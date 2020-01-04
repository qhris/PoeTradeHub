using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PoeTradeHub.CLI
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var items = new string[]
            {
                "currency_intrinsic_catalyst.txt",
                "card_her_mask.txt",
                "gem_vaal_impurity_of_fire.txt",
                "item_rare_gemini_claw.txt",
                "map_temple.txt",
                "map_unique_beachhead.txt",
                "unique_kaltenhalt.txt",
            };

            foreach (var item in items)
            {
                await ParseItem(item);
            }
        }

        class ItemGroupInfo
        {
            public ItemGroupInfo(int begin, int end)
            {
                BeginIndex = begin;
                EndIndex = end;
                Size = end - begin;
            }

            public int BeginIndex { get; }
            public int EndIndex { get; }
            public int Size { get; }
        }

        class ItemParseData
        {
            public ItemParseData(IReadOnlyList<string> lines)
            {
                Lines = lines;
                Groups = ParseGroupInfo(lines);
            }

            public IReadOnlyList<string> Lines { get; }
            public IReadOnlyList<ItemGroupInfo> Groups { get; }

            private IReadOnlyList<ItemGroupInfo> ParseGroupInfo(IReadOnlyList<string> lines)
            {
                var groups = new List<ItemGroupInfo>();
                var start = 0;

                for (int current = 0; current < lines.Count; current++)
                {
                    var line = lines[current];

                    if (line.StartsWith("---"))
                    {
                        groups.Add(new ItemGroupInfo(start, current));
                        start = current + 1;
                    }
                }

                if (start < lines.Count)
                {
                    groups.Add(new ItemGroupInfo(start, lines.Count));
                }

                return groups;
            }
        }

        private static async Task ParseItem(string fileName)
        {
            var path = @"G:\Projects\PoeTradeHub\Data\Items\" + fileName;

            try
            {
                IReadOnlyList<string> lines = await File.ReadAllLinesAsync(path, Encoding.UTF8);
                if (lines.Count < 3)
                {
                    Console.WriteLine("Invalid item data.");
                    return;
                }

                var data = new ItemParseData(lines);

                Console.WriteLine($"\nItem: {path}");
                Console.WriteLine($"Groups: {data.Groups.Count}");
                ParseItemType(data);

                Console.WriteLine();

                // foreach (var line in lines)
                // {
                //    Console.WriteLine(line);
                // }
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine($"Couldn't find the file: {path}");
            }
        }

        private static void ParseItemType(ItemParseData data)
        {
            var namedItems = new HashSet<string>
            {
                "Normal",
                "Magic",
                "Rare",
                "Unique",
            };

            var rarityMatch = Regex.Match(data.Lines[0], @"^Rarity: (.*)$");
            if (rarityMatch.Success)
            {
                if (namedItems.Contains(rarityMatch.Groups[1].Value))
                {
                    var itemName = data.Lines[1];
                    var baseType = data.Lines[2];

                    Console.WriteLine($"Item: {itemName} {baseType}");
                }
                else
                {
                    Console.WriteLine($"Type: {rarityMatch.Groups[1]}");
                }
            }
        }
    }
}
