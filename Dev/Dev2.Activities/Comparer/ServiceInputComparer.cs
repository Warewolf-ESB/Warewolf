using System.Collections.Generic;
using Dev2.Common.Interfaces.DB;

namespace Dev2.Comparer
{
    internal class ServiceInputComparer : IEqualityComparer<IServiceInput>
    {
        public bool Equals(IServiceInput x, IServiceInput y)
        {
            if (x == null && y == null)
            {
                return true;
            }

            if (x == null || y == null)
            {
                return false;
            }

            var inputsAreTheSame = string.Equals(x.ActionName, y.ActionName)
                                   && string.Equals(x.Dev2ReturnType, y.Dev2ReturnType)
                                   && string.Equals(x.FullName, y.FullName)
                                   && string.Equals(x.Name, y.Name)
                                   && string.Equals(x.Value, y.Value)
                                   && string.Equals(x.ShortTypeName, y.ShortTypeName)
                                   && string.Equals(x.TypeName, y.TypeName)
                                   && Equals(x.RequiredField, y.RequiredField)
                                   && Equals(x.IsObject, y.IsObject)
                                   && Equals(x.EmptyIsNull, y.EmptyIsNull);
            return inputsAreTheSame;
        }

        public int GetHashCode(IServiceInput obj) => obj.GetHashCode();
    }
}