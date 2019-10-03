using FluentAssertions;
using Xunit;

namespace Proteus.Test.Serializer
{
    public class Numeric
    {
        [Theory]
        [InlineData(byte.MaxValue)]
        [InlineData(byte.MinValue)]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(23)]
        [InlineData(214)]
        public void Byte (byte value)
        {
            var serializer = new Core.Serializer();
            var serialized = serializer.Serialize(value);
            var deserialized = serializer.Deserialize<byte>(serialized);

            deserialized.Should().Be(value);
        }

        [Theory]
        [InlineData(short.MaxValue)]
        [InlineData(short.MinValue)]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(23)]
        [InlineData(-856)]
        [InlineData(5423)]
        [InlineData(-21321)]
        public void Short (short value)
        {
            var serializer = new Core.Serializer();
            var serialized = serializer.Serialize(value);
            var deserialized = serializer.Deserialize<short>(serialized);

            deserialized.Should().Be(value);
        }

        [Theory]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(23)]
        [InlineData(856)]
        [InlineData(-2654)]
        [InlineData(203302)]
        [InlineData(-2334354)]
        [InlineData(84654654)]
        [InlineData(-325648477)]
        public void Int (int value)
        {
            var serializer = new Core.Serializer();
            var serialized = serializer.Serialize(value);
            var deserialized = serializer.Deserialize<int>(serialized);

            deserialized.Should().Be(value);
        }

        [Theory]
        [InlineData(float.MaxValue)]
        [InlineData(float.MinValue)]
        [InlineData(float.Epsilon)]
        [InlineData(float.NaN)]
        [InlineData(float.NegativeInfinity)]
        [InlineData(float.PositiveInfinity)]
        [InlineData(0)]
        [InlineData(-1.0f)]
        [InlineData(-0.00f)]
        [InlineData(16.201f)]
        [InlineData(-1608.21f)]
        [InlineData(6587.4107f)]
        public void Float (float value)
        {
            var serializer = new Core.Serializer();
            var serialized = serializer.Serialize(value);
            var deserialized = serializer.Deserialize<float>(serialized);

            deserialized.Should().Be(value);
        }
    }
}