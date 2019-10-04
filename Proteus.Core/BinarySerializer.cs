using System;
using System.Collections.Generic;
using System.Linq;
using Chresimos.Core;

namespace Proteus.Core
{
    public abstract class BinarySerializer
    {
        public const int DefaultBufferSize = 512;
        public const int BufferExpansionFactor = 2;
        public const bool MemberIsNullFlag = true;
        public static readonly CannotRead CannotReadValue = new CannotRead();

        public byte[] Buffer { get; private set; }
        public int BufferIndex;

        protected readonly Serializer Serializer;

        public byte[] RemainingBuffer => Buffer.GetRange(BufferIndex, Buffer.Length - BufferIndex);
        public byte[] UsedBuffer => Buffer.GetRange(0, BufferIndex);

        protected BinarySerializer (Serializer serializer)
        {
            Buffer = new byte[DefaultBufferSize];
            Serializer = serializer;
        }

        protected BinarySerializer (byte[] buffer, Serializer serializer)
        {
            Buffer = buffer;
            Serializer = serializer;
        }

        public void AppendToBuffer (byte data)
        {
            MakeSureBufferIsLargeEnoughToAppend(sizeof(byte));
            
            Buffer[BufferIndex] = data;
            BufferIndex += sizeof(byte);
        }

        public void AppendToBuffer (byte[] data)
        {
            AppendToBuffer(data, data.Length);
        }

        public void AppendToBuffer (byte[] data, int length)
        {
            MakeSureBufferIsLargeEnoughToAppend(length);
            
            Array.Copy(data, 0, Buffer, BufferIndex, length);
            BufferIndex += length;
        }

        public byte ReadBuffer ()
        {
            var data = Buffer[BufferIndex];
            BufferIndex += sizeof(byte);

            return data;
        }

        private void MakeSureBufferIsLargeEnoughToAppend (int dataLength)
        {
            if (Buffer.Length - BufferIndex > dataLength) return;

            var newBufferSize = Buffer.Length;
            while (newBufferSize - BufferIndex < dataLength)
            {
                newBufferSize *= BufferExpansionFactor;
            }
            
            var newBuffer = new byte[newBufferSize];
            Array.Copy(Buffer, 0, newBuffer, 0, Buffer.Length);

            Buffer = newBuffer;
        }
        
        public struct CannotRead
        {
        }
    }
}