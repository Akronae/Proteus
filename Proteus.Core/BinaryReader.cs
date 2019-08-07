using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chresimos.Core;

namespace Proteus.Core
{
    public sealed class BinaryReader : BinarySerializer
    {
        public BinaryReader (List<byte> buffer, Serializer serializer) : base(buffer, serializer)
        {
        }


        public object Read (Type type)
        {
            if (ReadBool() == NullFieldFlag)
            {
                return null;
            }
            
            object value;

            if (type == typeof(byte))
                value = ReadByte();
            else if (type == typeof(int))
                value = ReadNumber();
            else if (type == typeof(float))
                value = ReadFloatNumber();
            else if (type == typeof(string))
                value = ReadString();
            else if (type.IsSubclassOf(typeof(Enum)))
                value = ReadEnum(type);
            else if (type == typeof(Guid))
                value = ReadGuid();
            else if (type.GetInterfaces().Contains(typeof(IList)))
                value = ReadList(type);
            else if (type.GetInterfaces().Contains(typeof(IDictionary)))
                value = ReadDictionary(type);
            else value = null;

            return value ?? CannotReadValue;
        }

        public byte ReadByte ()
        {
            var value = Buffer[Index];
            Index += sizeof(byte);

            return value;
        }
        
        public bool ReadBool ()
        {
            return Convert.ToBoolean(ReadByte());
        }

        public short ReadShort ()
        {
            var value = BitConverter.ToInt16(Buffer.ToArray(), Index);
            Index += sizeof(short);

            return value;
        }

        public ushort ReadUShort ()
        {
            var value = BitConverter.ToUInt16(Buffer.ToArray(), Index);
            Index += sizeof(short);

            return value;
        }

        public int ReadInt32 ()
        {
            var value = BitConverter.ToInt32(Buffer.ToArray(), Index);
            Index += sizeof(int);

            return value;
        }

        public uint ReadUInt32 ()
        {
            var value = BitConverter.ToUInt32(Buffer.ToArray(), Index);
            Index += sizeof(int);

            return value;
        }

        public float ReadFloat ()
        {
            var value = BitConverter.ToSingle(Buffer.ToArray(), Index);
            Index += sizeof(float);

            return value;
        }

        public byte[] ReadBytes (int len)
        {
            var bytes = Buffer.GetRange(Index, len);
            Index += len;

            return bytes.ToArray();
        }

        public string ReadString ()
        {
            var len = ReadInt32();
            var value = Encoding.UTF8.GetString(Buffer.ToArray(), Index, len);
            Index += len;

            return value;
        }

        public object ReadEnum (Type type)
        {
            return Enum.ToObject(type, ReadShort());
        }

        public Guid ReadGuid ()
        {
            const int len = 16;
            var guid = new Guid(ReadBytes(len));

            return guid;
        }

        public IList ReadList (Type listType)
        {
            var count = ReadInt32();
            var listGenericType = listType.GetGenericArguments().Single();
            var list = ListUtils.CreateListOfType(listGenericType);

            var useSerializer = listGenericType.IsSimpleType() is false;

            for (var i = 0; i < count; i++)
                if (useSerializer)
                {
                    var typeId = ReadShort();
                    Type itemType;

                    if (typeId == GenericTypesConsts.UndefinedType)
                        itemType = listGenericType;
                    else itemType = Serializer.GenericTypes.GetType(typeId);

                    var item = Serializer.Deserialize(itemType, RemainingBuffer.ToArray(), out var consumed);
                    Index += consumed;

                    list.Add(item);
                }
                else
                {
                    var value = Read(listGenericType);
                    if (value is CannotRead)
                    {
                        LogUtils.Throw(
                            new Exception($"Tried to add to list but can't read value of type {listGenericType}"));
                    }
                    
                    list.Add(value);
                }

            return list;
        }

        public IDictionary ReadDictionary (Type dictionaryType)
        {
            var keyType = dictionaryType.GetGenericArguments()[0];
            var valueType = dictionaryType.GetGenericArguments()[1];
            var keys = ReadList(typeof(List<>).MakeGenericType(keyType));
            var values = ReadList(typeof(List<>).MakeGenericType(valueType));

            var dictionary = DictionaryUtils.CreateDictionaryOfTypes(keyType, valueType);
            for (var index = 0; index < keys.Count; index++)
            {
                var key = keys[index];
                dictionary.Add(key, values[index]);
            }

            return dictionary;
        }

        public int ReadNumber ()
        {
            var type = (NumberType) ReadByte();
            return ReadIntegerType(type);
        }

        private int ReadIntegerType (NumberType type)
        {
            switch (type)
            {
                case NumberType.Byte:
                    return ReadByte();
                case NumberType.SByte:
                    return (sbyte) ReadByte();
                case NumberType.Short:
                    return ReadShort();
                case NumberType.UShort:
                    return ReadUShort();
                case NumberType.Int32:
                    return ReadInt32();

                default:
                    throw LogUtils.Throw(new ArgumentOutOfRangeException(type.ToString()));
            }
        }

        public float ReadFloatNumber ()
        {
            var type = (NumberType) ReadByte();

            switch (type)
            {
                case NumberType.Float:
                    return ReadFloat();
                case NumberType.FloatMultipliedToByteBy10:
                    return (float) ReadByte() / 10;
                case NumberType.FloatMultipliedToSByteBy10:
                    return (float) (sbyte) ReadByte() / 10;
                case NumberType.FloatMultipliedToShortBy100:
                    return (float) ReadShort() / 100;

                default:
                    return ReadIntegerType(type);
            }
        }
    }
}