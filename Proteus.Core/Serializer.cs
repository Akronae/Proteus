using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Chresimos.Core;

namespace Proteus.Core
{
    public class Serializer
    {
        private readonly Dictionary<Type, Dictionary<int, MemberInfo>> _cachedPacketSerializableMembers =
            new Dictionary<Type, Dictionary<int, MemberInfo>>();

        public readonly IGenericTypesProvider GenericTypesProvider;

        public Serializer (IGenericTypesProvider genericTypesProvider = null)
        {
            if (genericTypesProvider == null)
            {
                genericTypesProvider = new VoidGenericTypesProvider();
            }

            GenericTypesProvider = genericTypesProvider;
        }

        public byte[] Serialize (object obj)
        {
            return Serialize(obj, obj?.GetType());
        }

        public byte[] Serialize (object obj, Type objectType)
        {
            var writer = new BinaryWriter(this);
            var objGenericTypeId = GenericTypesProvider.GetTypeId(objectType);

            writer.WriteNumber(objGenericTypeId);

            if (obj == null && objectType == null)
            {
                writer.Write(obj);
            }
            else
            {
                var members = GetObjectSerializableMembers(objectType);

                if (members.Count == 0)
                {
                    writer.Write(obj);
                }

                foreach (var member in members)
                {
                    var value = member.Value.GetValue(obj);

                    // If value is not primitive type.
                    if (writer.Write(value) == false)
                    {
                        var serializedObj = Serialize(value);

                        if (serializedObj.Length == 0)
                        {
                            throw LogUtils.Throw($"Cannot write type {value.GetType().FullName}");
                        }

                        writer.WriteBytes(serializedObj);
                    }
                }
            }

            return writer.Buffer.ToArray();
        }

        public object Deserialize (Type type, IEnumerable<byte> data, out int bytesConsumed)
        {
            var reader = new BinaryReader(data.ToList(), this);

            var objGenericTypeId = reader.ReadNumber();

            if (objGenericTypeId != GenericTypesConsts.UndefinedTypeId)
            {
                type = GenericTypesProvider.GetType(objGenericTypeId);
            }

            var members = GetObjectSerializableMembers(type);

            object instance = null;
            if (members.Count == 0)
            {
                // As there is no serializable member in this type, it is either an empty class, or a native value.
                // If we can read the primitive value we assign it to the instance else we leave it as it is,
                // it's to say an empty class instance.
                var instanceValue = reader.Read(type);
                if (!(instanceValue is BinarySerializer.CannotRead))
                {
                    instance = instanceValue;
                }
            }

            if (instance == null && type != typeof(string))
            {
                if (!type.HasParameterlessConstructor())
                {
                    throw LogUtils.Throw(
                        $"{type} must have parameterless constructor in oder to be deserialized.");
                }

                instance = Activator.CreateInstance(type);
            }

            foreach (var member in members)
            {
                var memType = member.Value.GetValueType();
                var value = reader.Read(memType);

                if (value is BinarySerializer.CannotRead)
                {
                    value = Deserialize(memType, reader.RemainingBuffer.ToArray(), out var c);
                    reader.Index += c;

                    if (value == null)
                    {
                        throw LogUtils.Throw(new Exception($"Cannot read member of type {memType} of {type}"));
                    }
                }

                member.Value.SetValue(instance, value);
            }

            bytesConsumed = reader.Index;

            return instance;
        }

        public object Deserialize (Type type, byte[] data)
        {
            return Deserialize(type, data, out _);
        }

        public T Deserialize <T> (byte[] data, out int bytesConsumed)
        {
            return (T) Deserialize(typeof(T), data, out bytesConsumed);
        }

        public T Deserialize <T> (byte[] data)
        {
            return Deserialize<T>(data, out _);
        }

        public Dictionary<int, MemberInfo> GetObjectSerializableMembers (Type packetType)
        {
            var isCached = _cachedPacketSerializableMembers.ContainsKey(packetType);
            if (isCached) return _cachedPacketSerializableMembers[packetType];

            var members = new Dictionary<int, MemberInfo>();
            var inheritanceTree = packetType.GetInheritanceList(typeof(object), true);
            var packetTypeMembers = GetTypeSerializableMembers(packetType);

            foreach (var type in inheritanceTree)
            {
                // + 1 if first index is 0.
                var lastIndex = members.LastOrDefault().Key + 1;

                foreach (var pair in packetTypeMembers)
                {
                    if (pair.Value.DeclaringType == type)
                    {
                        var memberId = lastIndex + pair.Key;
                        if (members.ContainsKey(memberId))
                        {
                            LogUtils.Throw(new Exception(
                                $"{members[memberId]} and {pair.Value} have the same ID = {memberId} in packet {packetType}"));
                        }

                        members.Add(memberId, pair.Value);
                    }
                }
            }

            _cachedPacketSerializableMembers.Add(packetType, members);

            return members;
        }

        public static List<KeyValuePair<int, MemberInfo>> GetTypeSerializableMembers (Type type)
        {
            var values = new List<KeyValuePair<int, MemberInfo>>();

            foreach (var property in type.GetProperties())
            {
                var attr = property.GetCustomAttributes(typeof(SerializedMemberAttribute), false).SingleOrDefault();

                if (attr is SerializedMemberAttribute sma)
                {
                    values.Add(new KeyValuePair<int, MemberInfo>(sma.Index, property));
                }
            }

            foreach (var field in type.GetFields())
            {
                var attr = field.GetCustomAttributes(typeof(SerializedMemberAttribute), false).SingleOrDefault();

                if (attr is SerializedMemberAttribute sma)
                {
                    values.Add(new KeyValuePair<int, MemberInfo>(sma.Index, field));
                }
            }

            return values;
        }
    }
}