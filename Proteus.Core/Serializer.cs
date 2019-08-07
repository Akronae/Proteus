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

        public Serializer (IGenericTypesProvider genericTypesProvider)
        {
            GenericTypesProvider = genericTypesProvider;
        }

        public byte[] Serialize (object obj)
        {
            return Serialize(obj, obj.GetType());
        }

        public byte[] Serialize (object obj, Type objectType)
        {
            var members = GetPacketSerializableMembers(objectType);
            var writer = new BinaryWriter(this);

            if (members.Count == 0)
            {
                writer.Write(obj);
            }
            
            foreach (var member in members)
            {
                var value = member.Value.GetValue(obj);

                if (value is Enum) value = (short) (int) value;

                // If value is not primitive type.
                if (writer.Write(value) == false)
                {
                    var serializedObj = Serialize(value);

                    if (serializedObj.Length == 0)
                    {
                        throw LogUtils.Throw($"Can't write type {value.GetType().FullName}");
                    }
                    
                    writer.WriteBytes(serializedObj);
                }
            }

            return writer.Buffer.ToArray();
        }

        public object Deserialize (Type type, IEnumerable<byte> data, out int bytesConsumed)
        {
            if (!type.HasParameterlessConstructor())
            {
                throw LogUtils.Throw(
                    $"{type} must have parameterless constructor in oder to be deserialized.");
            }
            
            var members = GetPacketSerializableMembers(type);
            var instance = Activator.CreateInstance(type);

            var reader = new BinaryReader(data.ToList(), this);

            if (members.Count == 0)
            {
                instance = reader.Read(type);
            }

            foreach (var member in members)
            {
                var memType = member.Value.GetValueType();
                var value = reader.Read(memType);

                if (value is BinarySerializer.CannotRead)
                {
                    value = Deserialize(memType, reader.RemainingBuffer.ToArray(), out var c);
                    reader.Index += c;

                    if (value is null) throw LogUtils.Throw(new Exception($"Can't read type {type.FullName}"));
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
        
        public Dictionary<int, MemberInfo> GetPacketSerializableMembers (Type packetType)
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
                    values.Add(new KeyValuePair<int, MemberInfo>(sma.Index, property));
            }

            foreach (var field in type.GetFields())
            {
                var attr = field.GetCustomAttributes(typeof(SerializedMemberAttribute), false).SingleOrDefault();

                if (attr is SerializedMemberAttribute sma)
                    values.Add(new KeyValuePair<int, MemberInfo>(sma.Index, field));
            }

            return values;
        }
    }
}