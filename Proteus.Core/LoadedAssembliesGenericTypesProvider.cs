using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Chresimos.Core;

namespace Proteus.Core
{
    public class LoadedAssembliesGenericTypesProvider : IGenericTypesProvider
    {
        private static readonly Dictionary<int, Type> _genericTypes = new Dictionary<int, Type>();

        public LoadedAssembliesGenericTypesProvider ()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) RegisterAssembly(assembly);
        }

        public int GetTypeId (Type type)
        {
            var p = _genericTypes.SingleOrDefault(pair => pair.Value == type);

            return p.Value is null ? GenericTypesConsts.UndefinedType : p.Key;
        }

        public Type GetType (int id)
        {
            if (!_genericTypes.ContainsKey(id))
                LogUtils.Throw(new Exception($"{nameof(LoadedAssembliesGenericTypesProvider)} has no type associated with generic type Id {id}"));

            return _genericTypes[id];
        }

        private void RegisterAssembly (Assembly assembly)
        {
            foreach (var type in assembly.GetTypes())
            {
                if (_genericTypes.ContainsValue(type)) continue;

                var attr = type.GetCustomAttributes<SerializableAsGenericAttribute>(false).SingleOrDefault();
                if (attr is null) continue;

                if (_genericTypes.ContainsKey(attr.GenericTypeId))
                    throw new Exception(
                        $"{type} and {_genericTypes[attr.GenericTypeId]} have the same {nameof(attr.GenericTypeId)}");

                _genericTypes.Add(attr.GenericTypeId, type);
            }
        }
    }
}