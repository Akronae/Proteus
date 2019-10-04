using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Chresimos.Core;

namespace Proteus.Core
{
    public sealed class BinaryWriter : BinarySerializer
    {
        public BinaryWriter (Serializer serializer) : base(serializer)
        {
        }

        public bool Write (object value)
        {
            if (value == null)
            {
                WriteBool(MemberIsNullFlag);
                return true;
            }

            WriteBool(!MemberIsNullFlag);

            switch (value)
            {
                case bool b:
                    WriteBool(b);
                    return true;
                case byte bt:
                    WriteByte(bt);
                    return true;
                case short i16:
                    WriteShort(i16);
                    return true;
                case ushort ui16:
                    WriteUShort(ui16);
                    return true;
                case char char16:
                    WriteChar(char16);
                    return true;
                case int i32:
                    WriteNumber(i32);
                    return true;
                case float f:
                    WriteNumber(f);
                    return true;
                case string str:
                    WriteString(str);
                    return true;
                case Enum @enum:
                    WriteEnum(@enum);
                    return true;
                case Guid guid:
                    WriteGuid(guid);
                    return true;
                case IList list:
                    WriteList(list);
                    return true;
                case IDictionary dictionary:
                    WriteDictionary(dictionary);
                    return true;
            }

            return false;
        }

        public void WriteByte (byte value)
        {
            AppendToBuffer(value);
        }

        public void WriteBool (bool value)
        {
            AppendToBuffer(value ? (byte) 1 : (byte) 0);
        }

        public void WriteShort (short value)
        {
            AppendToBuffer((byte) (value & 255));
            AppendToBuffer((byte) (value >> 8));
        }

        public void WriteUShort (ushort value)
        {
            WriteShort((short) value);
        }

        public void WriteChar (char value)
        {
           WriteUShort(value);
        }

        public void WriteInt32 (int value)
        {
            AppendToBuffer((byte) value);
            AppendToBuffer((byte) (value >> 8));
            AppendToBuffer((byte) (value >> 16));
            AppendToBuffer((byte) (value >> 24));
        }

        public void WriteFloat (float value)
        {
            AppendToBuffer(BitConverter.GetBytes(value));
        }

        public void WriteString (string value)
        {
            WriteNumber(Encoding.UTF8.GetByteCount(value));
            AppendToBuffer(Encoding.UTF8.GetBytes(value));
        }

        public void WriteEnum (Enum value)
        {
            WriteNumber((int) (object) value);
        }

        public void WriteList (IList list)
        {
            var listElementType = list.GetListElementTypeOrDefault();
            if (listElementType == null)
            {
                throw LogUtils.Throw($"Cannot write {list} which is not of type List<>");
            }

            WriteNumber(list.Count);

            foreach (var obj in list)
            {
                var objSerializationType = obj?.GetType();
                var objGenericTypeId = Serializer.GenericTypesProvider.GetTypeId(objSerializationType);

                // If the type of this element has a unique ID, we can serialize it as it is because the type will later
                // be retrieved during the deserialization; If not we stick to the list generic element type.
                if (objGenericTypeId == GenericTypesConsts.UndefinedTypeId)
                {
                    objSerializationType = listElementType;
                }

                var serialized = Serializer.Serialize(obj, objSerializationType, out var serializedLength);
                WriteBytes(serialized, serializedLength);
            }
        }

        public void WriteDictionary (IDictionary dictionary)
        {
            var dictionaryGenerics = dictionary.GetType().GetGenericArguments();

            var keys = ListUtils.CreateListOfType(dictionaryGenerics[0]);
            var values = ListUtils.CreateListOfType(dictionaryGenerics[1]);

            dictionary.Keys.CopyToList(keys);
            dictionary.Values.CopyToList(values);

            WriteList(keys);
            WriteList(values);
        }

        public void WriteBytes (byte[] bytes)
        {
            AppendToBuffer(bytes, bytes.Length);
        }
        
        public void WriteBytes (byte[] bytes, int length)
        {
            AppendToBuffer(bytes, length);
        }

        public void WriteGuid (Guid guid)
        {
            WriteBytes(guid.ToByteArray());
        }

        public void WriteNumber (int value)
        {
            if (value <= byte.MaxValue && value >= byte.MinValue)
            {
                WriteByte((byte) NumberType.Byte);
                WriteByte((byte) value);
            }
            else if (value <= sbyte.MaxValue && value >= sbyte.MinValue)
            {
                WriteByte((byte) NumberType.SByte);
                WriteByte((byte) Convert.ToSByte(value));
            }
            else if (value <= ushort.MaxValue && value >= ushort.MinValue)
            {
                WriteByte((byte) NumberType.UShort);
                WriteUShort((ushort) value);
            }
            else if (value <= short.MaxValue && value >= short.MinValue)
            {
                WriteByte((byte) NumberType.Short);
                WriteShort((short) value);
            }
            else
            {
                WriteByte((byte) NumberType.Int32);
                WriteInt32(value);
            }
        }

        public void WriteNumber (float value)
        {
            if (value == (int) value)
            {
                WriteNumber((int) value);

                return;
            }

            var multipliedBy10 = value * 10;
            var multipliedBy100 = value * 100;

            if (multipliedBy10 == (int) multipliedBy10)
            {
                if (multipliedBy10 >= byte.MinValue && multipliedBy10 <= byte.MaxValue)
                {
                    WriteByte((byte) NumberType.FloatMultipliedToByteBy10);
                    WriteByte((byte) multipliedBy10);

                    return;
                }

                if (multipliedBy10 >= sbyte.MinValue && multipliedBy10 <= sbyte.MaxValue)
                {
                    WriteByte((byte) NumberType.FloatMultipliedToSByteBy10);
                    WriteByte((byte) Convert.ToSByte(multipliedBy10));

                    return;
                }
            }

            if (multipliedBy100 == (int) multipliedBy100)
            {
                if (multipliedBy100 >= short.MinValue && multipliedBy100 <= short.MaxValue)
                {
                    WriteByte((byte) NumberType.FloatMultipliedToShortBy100);
                    WriteShort((short) multipliedBy100);

                    return;
                }

                if (multipliedBy100 >= ushort.MinValue && multipliedBy100 <= ushort.MinValue)
                {
                    WriteByte((byte) NumberType.FloatMultipliedToShortBy100);
                    WriteShort((short) multipliedBy100);

                    return;
                }
            }

            WriteByte((byte) NumberType.Float);
            WriteFloat(value);
        }
    }
}