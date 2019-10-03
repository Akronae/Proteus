using System;
using FluentAssertions;
using Xunit;

namespace Proteus.Test.Serializer
{
    public class Literal
    {
        [Theory]
        [InlineData(char.MaxValue)]
        [InlineData(char.MinValue)]
        [InlineData('\n')]
        [InlineData('À')]
        [InlineData('a')]
        [InlineData('z')]
        [InlineData('ز')]
        [InlineData('э')]
        [InlineData('Γ')]
        public void Char (char value)
        {
            var serializer = new Core.Serializer();
            var serialized = serializer.Serialize(value);
            var deserialized = serializer.Deserialize<char>(serialized);

            deserialized.Should().Be(value);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("\n\'\"\\\0\a\b\f\n\r\t\v")]
        [InlineData("👩👨🧑👧👦🧒🤹‍♂️🤝👊🤾‍♂️🎫🎉🎁⚾🎱⚙📚🚞🦼🌡🔃🕘♒")]
        [InlineData("À la pêche aux moules, moules, moules je ne veux plys y aller maman")]
        [InlineData("زبدة بالفراولة")]
        [InlineData("этот кодекс будет служить советам")]
        [InlineData("ΑΠΑΓΕ ΣΑΤΑΝΑ")]
        public void String (string value)
        {
            var serializer = new Core.Serializer();
            var serialized = serializer.Serialize(value);
            var deserialized = serializer.Deserialize<string>(serialized);

            deserialized.Should().Be(value);
        }

        [Theory]
        [InlineData("00000000-0000-0000-0000-000000000000")]
        [InlineData("6519cff9-8935-406b-8de2-a6a97e9144f6")]
        [InlineData("ad2563a1-25d6-42f2-a69d-3922dc79a443")]
        [InlineData("bae9c577-83f4-4720-8cc3-270d0a823fcc")]
        [InlineData("a54ea1b5-d355-40df-a11a-98e1e806976c")]
        public void Guid (string GuidString)
        {
            var guid = System.Guid.Parse(GuidString);
            var serializer = new Core.Serializer();
            var serialized = serializer.Serialize(guid);
            var deserialized = serializer.Deserialize<Guid>(serialized);

            deserialized.Should().Be(guid);
        }
    }
}