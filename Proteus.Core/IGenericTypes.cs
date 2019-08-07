using System;

namespace Proteus.Core
{
    public interface IGenericTypes
    {
        int GetTypeId (Type type);
        Type GetType (int id);
    }

    public static class GenericTypesConsts
    {
        public const int UndefinedType = -1;
    }
}