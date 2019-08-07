using System;

namespace Proteus.Core
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SerializableAsGenericAttribute : Attribute
    {
        public readonly int GenericTypeId;

        public SerializableAsGenericAttribute (int genericTypeId)
        {
            GenericTypeId = genericTypeId;
        }
    }
}