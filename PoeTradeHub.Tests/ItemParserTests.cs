using Xunit;

namespace PoeTradeHub.Tests
{
    public class ItemParserTests
    {
        [Theory]
        [ItemTestData("Data/maps.json")]
        public void Parse_ParsesSuccessfully_GivenMaps(string data, ItemInformation expected)
        {
            var parser = new ItemParser();
            var result = parser.Parse(data);

            Assert.Equal(ItemType.Map, result.ItemType);
            Assert.Equal(expected.Name, result.Name);
            Assert.Equal(expected.IsCorrupted, result.IsCorrupted);
            Assert.Equal(expected.MapTier, result.MapTier);
            Assert.Equal(expected.BaseType, result.BaseType);
            Assert.Equal(expected.IsBlighted ?? false, result.IsBlighted ?? false);
        }

        [Theory]
        [ItemTestData("Data/cards.json")]
        public void Parse_ParsesSuccessfully_GivenCards(string data, ItemInformation expected)
        {
            var parser = new ItemParser();
            var result = parser.Parse(data);

            Assert.Equal(ItemType.DivinationCard, result.ItemType);
            Assert.Equal(expected.BaseType, result.BaseType);
            Assert.Equal(expected.Name, result.Name);
            Assert.Equal(expected.Stack, result.Stack);
            Assert.Equal(expected.StackSize, result.StackSize);
        }

        [Theory]
        [ItemTestData("Data/fragments.json")]
        public void Parse_ParsesSuccessfully_GivenFragments(string data, ItemInformation expected)
        {
            var parser = new ItemParser();
            var result = parser.Parse(data);

            Assert.Equal(expected.ItemType, result.ItemType);
            Assert.Equal(expected.Name, result.Name);

            // Name and basetypes should match.
            Assert.Equal(result.Name, result.BaseType);
        }
    }
}
