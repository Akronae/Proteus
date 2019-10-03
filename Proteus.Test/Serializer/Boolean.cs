using FluentAssertions;
using Xunit;

namespace Proteus.Test.Serializer
{
    public class Boolean
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Bool (bool value)
        {
            var serializer = new Core.Serializer();
            var serialized = serializer.Serialize(value);
            var deserialized = serializer.Deserialize<bool>(serialized);

            deserialized.Should().Be(value);
        }
    }
}