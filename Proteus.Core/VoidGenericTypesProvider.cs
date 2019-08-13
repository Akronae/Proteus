using System;
using Chresimos.Core;

namespace Proteus.Core
{
    public class VoidGenericTypesProvider : IGenericTypesProvider
    {
        public int GetTypeId (Type type)
        {
            return GenericTypesConsts.UndefinedTypeId;
        }

        public Type GetType (int id)
        {
            throw LogUtils.Throw($"As no generic types provider has been given, can not retrieve type " +
                                 $"associated with id {id}");
        }
    }
}