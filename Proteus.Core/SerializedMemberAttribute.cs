using System;

namespace Proteus.Core
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class SerializedMemberAttribute : Attribute
    {
        public int Index;

        public SerializedMemberAttribute (int index)
        {
            Index = index;
        }
    }
}