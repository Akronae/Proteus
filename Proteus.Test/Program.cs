using System;
using System.Collections.Generic;
using Proteus.Core;
using Proteus.Test.Serializer;

namespace Proteus.Test
{
    internal class Program
    {
        private static void Main (string[] args)
        {
            var valueToSerialize = (ushort) 12;
            var serializer = new Core.Serializer();
            var serialized = serializer.Serialize(valueToSerialize);
            var deserialized = serializer.Deserialize<ushort>(serialized);

            Console.WriteLine(deserialized);

            //new ComplexObject().ThreeLevelObject();
        }
    }
}