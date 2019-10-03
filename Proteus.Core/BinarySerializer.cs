using System.Collections.Generic;
using System.Linq;

namespace Proteus.Core
{
    public abstract class BinarySerializer
    {
        public const bool MemberIsNullFlag = true;
        public static readonly CannotRead CannotReadValue = new CannotRead();

        public readonly List<byte> Buffer;
        public int Index;

        protected readonly Serializer Serializer;

        public IEnumerable<byte> RemainingBuffer => Buffer.ToList().GetRange(Index, Buffer.Count - Index);


        protected BinarySerializer (List<byte> buffer, Serializer serializer)
        {
            Buffer = buffer;
            Serializer = serializer;
        }

        public struct CannotRead
        {
        }
    }
}