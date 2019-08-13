using System;

namespace Proteus.Core
{
    public interface IGenericTypesProvider
    {
        int GetTypeId (Type type);
        Type GetType (int id);
    }

    public static class GenericTypesConsts
    {
        public const int UndefinedTypeId = -1;
    }
}