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
        }
    }
}
