using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chresimos.Core;

namespace Proteus.Core
{
    public sealed class BinaryWriter : BinarySerializer
    {
        public BinaryWriter (Serializer serializer) : base(new List<byte>(), serializer)
        {
        }

        public bool Write (object value)
        {
            if (value == null)
            {
                WriteBool(NullFieldFlag);
                return true;
            }
            
            WriteBool(!NullFieldFlag);
            
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
                case int i32:
                    WriteNumber(i32);
                    return true;
                case float f:
                    WriteNumber(f);
                    return true;
                case string str:
                    WriteString(str);
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
            Buffer.Add(value);
        }
        
        public void WriteBool (bool value)
        {
            Buffer.Add(Convert.ToByte(value));
        }

        public void WriteShort (short value)
        {
            Buffer.AddRange(BitConverter.GetBytes(value));
        }

        public void WriteShort (ushort value)
        {
            Buffer.AddRange(BitConverter.GetBytes(value));
        }

        public void WriteInt32 (int value)
        {
            Buffer.AddRange(BitConverter.GetBytes(value));
        }

        public void WriteInt32 (uint value)
        {
            Buffer.AddRange(BitConverter.GetBytes(value));
        }

        public void WriteFloat (float value)
        {
            Buffer.AddRange(BitConverter.GetBytes(value));
        }

        public void WriteString (string value)
        {
            WriteInt32(Encoding.UTF8.GetByteCount(value));
            Buffer.AddRange(Encoding.UTF8.GetBytes(value));
        }

        public void WriteList (IList list, Type genericType = null)
        {
            WriteInt32(list.Count);

            var listGenericType = genericType ?? list.GetType().GetGenericArguments().SingleOrDefault();
            if (listGenericType == null)
            {
                throw LogUtils.Throw($"Cannot write {list} which is not of type List<>");
            }
            
            var useSerializer = listGenericType.IsSimpleType() is false;

            foreach (var obj in list)
                if (useSerializer)
                {
                    var objType = obj.GetType();
                    var objTypeId = Serializer.GenericTypesProvider.GetTypeId(objType);

                    if (objTypeId == GenericTypesConsts.UndefinedType) objType = listGenericType;

                    WriteShort((short) objTypeId);

                    WriteBytes(Serializer.Serialize(obj, objType));
                }
                else
                {
                    Write(obj);
                }
        }

        public void WriteDictionary (IDictionary dictionary)
        {
            var keys = new object[dictionary.Keys.Count];
            var values = new object[dictionary.Values.Count];

            dictionary.Keys.CopyTo(keys, 0);
            dictionary.Values.CopyTo(values, 0);

            var dictionaryGenerics = dictionary.GetType().GetGenericArguments();

            WriteList(keys.ToList(), dictionaryGenerics[0]);
            WriteList(values.ToList(), dictionaryGenerics[1]);
        }

        public void WriteBytes (IEnumerable<byte> bytes)
        {
            Buffer.AddRange(bytes);
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
                WriteShort((ushort) value);
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
            var multipliedBy10 = value * 10;
            var multipliedBy100 = value * 100;

            if (value == (int) value)
            {
                WriteNumber((int) value);

                return;
            }
            
            if (multipliedBy10 == (int) multipliedBy10)
            {
                if (multipliedBy10 > byte.MinValue && multipliedBy10 < byte.MaxValue)
                {
                    WriteByte((byte) NumberType.FloatMultipliedToByteBy10);
                    WriteByte((byte) multipliedBy10);

                    return;
                }
                else if (multipliedBy10 > sbyte.MinValue && multipliedBy10 < sbyte.MaxValue)
                {
                    WriteByte((byte) NumberType.FloatMultipliedToSByteBy10);
                    WriteByte((byte) Convert.ToSByte(multipliedBy10));

                    return;
                }
            }
            
            if (multipliedBy100 < short.MaxValue && multipliedBy100 == (int) multipliedBy100)
            {
                WriteByte((byte) NumberType.FloatMultipliedToShortBy100);
                WriteShort((short) multipliedBy100);

                return;
            }

            WriteByte((byte) NumberType.Float);
            WriteFloat(value);
        }
    }
}