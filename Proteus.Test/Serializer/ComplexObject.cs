using System;
using System.Collections.Generic;
using Chresimos.Core;
using FluentAssertions;
using Proteus.Core;
using Xunit;

namespace Proteus.Test.Serializer
{
    public class ComplexObject
    {
        public class EmptyObjectClass
        {
        }

        public class OneLevelObjectClass
        {
            [SerializedMember(0)]
            public string StringField = RandUtils.RandomId();

            [SerializedMember(1)]
            public bool BoolField = RandUtils.EquiProb();

            [SerializedMember(2)]
            public int IntField = RandUtils.NextInt();

            [SerializedMember(3)]
            public Guid GuidField = Guid.NewGuid();

            [SerializedMember(4)]
            public List<int> ListField = new List<int> {RandUtils.Rand.Next(), RandUtils.Rand.Next()};

            [SerializedMember(5)]
            public Dictionary<string, float> DictionaryField = new Dictionary<string, float>
            {
                {RandUtils.RandomId(), RandUtils.NextFloat()},
                {RandUtils.RandomId(), RandUtils.NextFloat()},
                {RandUtils.RandomId(), RandUtils.NextFloat()},
                {RandUtils.RandomId(), RandUtils.NextFloat()}
            };
        }

        public class TwoLevelObjectClass
        {
            [SerializedMember(0)]
            public OneLevelObjectClass Object1 = new OneLevelObjectClass();

            [SerializedMember(1)]
            public OneLevelObjectClass Object2 = new OneLevelObjectClass();

            [SerializedMember(2)]
            public OneLevelObjectClass Object3 = new OneLevelObjectClass();
        }

        public class ThreeLevelObjectClass
        {
            [SerializedMember(0)]
            public TwoLevelObjectClass Object1 = new TwoLevelObjectClass();

            [SerializedMember(1)]
            public TwoLevelObjectClass Object2 = new TwoLevelObjectClass();

            public string ThisFieldWillNotBe = "Serialized";
        }

        [Fact]
        public void EmptyObject ()
        {
            AssertObject(new EmptyObjectClass());
        }

        [Fact]
        public void TwoLevelObject ()
        {
            AssertObject(new TwoLevelObjectClass());
        }

        [Fact]
        public void ThreeLevelObject ()
        {
            AssertObject(new ThreeLevelObjectClass());
        }

        private static void AssertObject <T> (T value)
        {
            var serializer = new Core.Serializer();
            var serialized = serializer.Serialize(value);
            var deserialized = serializer.Deserialize<T>(serialized);

            // Although this method can't 100% prove the deserialized value is equal to the original unserialized object,
            // the probability of a false positive is very low - according with the other tests -.
            var serializedDeserialized = serializer.Serialize(deserialized);

            serializedDeserialized.Should().Equal(serialized);
        }
    }
}