using System;
using System.Collections.Generic;
using Chresimos.Core;
using FluentAssertions;
using Proteus.Core;
using Xunit;

namespace Proteus.Test.Serializer
{
    public class Generic
    {
        [SerializableAsGeneric(0)]
        public class GenericBaseClass
        {
            [SerializedMember(0)]
            public int Field1 = RandUtils.NextInt();

            [SerializedMember(1)]
            public string Field2 = RandUtils.RandomId();
        }

        [SerializableAsGeneric(1)]
        public class GenericBaseInheritedClass : GenericBaseClass
        {
            [SerializedMember(0)]
            public bool Field3 = RandUtils.EquiProb();
        }

        [SerializableAsGeneric(2)]
        public class GenericBaseInheritedInheritedClass : GenericBaseInheritedClass
        {
            [SerializedMember(0)]
            public float Field4 = RandUtils.NextFloat();
        }

        [Fact]
        public void PostDeserializationCast ()
        {
            var serializer = new Core.Serializer(new LoadedAssembliesGenericTypesProvider());
            var value = new GenericBaseInheritedInheritedClass();

            var serialized = serializer.Serialize(value);
            var deserialized = serializer.Deserialize<GenericBaseClass>(serialized);
            var deserializedCastedAsGenericBaseInherited = deserialized as GenericBaseInheritedClass;
            var deserializedCastedAsGenericBaseInheritedInherited = deserialized as GenericBaseInheritedInheritedClass;

            deserializedCastedAsGenericBaseInherited.Should().NotBeNull();
            deserializedCastedAsGenericBaseInheritedInherited.Should().NotBeNull();
            deserializedCastedAsGenericBaseInherited.Field3.Should().Be(value.Field3);
            deserializedCastedAsGenericBaseInheritedInherited.Field4.Should().Be(value.Field4);

            AssertObject(value);
        }

        [Fact]
        public void GenericList ()
        {
            var serializer = new Core.Serializer(new LoadedAssembliesGenericTypesProvider());
            var value = new List<GenericBaseClass>
                {new GenericBaseClass(), new GenericBaseInheritedClass(), new GenericBaseInheritedInheritedClass()};

            var serialized = serializer.Serialize(value);
            var deserialized = serializer.Deserialize<List<GenericBaseClass>>(serialized);

            deserialized.Should().SatisfyRespectively
            (
                first =>
                {
                    first.Should().BeOfType<GenericBaseClass>();
                    first.Field1.Should().Be(value[0].Field1);
                },
                second =>
                {
                    var casted = second as GenericBaseInheritedClass;
                    casted.Should().NotBeNull();
                    casted.Field3.Should().Be((value[1] as GenericBaseInheritedClass).Field3);
                },
                third =>
                {
                    var casted = third as GenericBaseInheritedInheritedClass;
                    casted.Should().NotBeNull();
                    casted.Field4.Should().Be((value[2] as GenericBaseInheritedInheritedClass).Field4);
                }
            );

            AssertObject(value);
        }

        private static void AssertObject <T> (T value)
        {
            var serializer = new Core.Serializer();
            var serialized = serializer.Serialize(value);
            var deserialized = serializer.Deserialize<T>(serialized);

            // Although this method can't 100% prove the deserialized value is equal to the original unserialized object,
            // the probability of a false positive is very low - according with the other tests -.
            var serializedDeserialized = serializer.Serialize(deserialized);
            Console.WriteLine(BitConverter.ToString(serialized));
            Console.WriteLine(BitConverter.ToString(serializedDeserialized));

            serializedDeserialized.Should().Equal(serialized);
        }
    }
}