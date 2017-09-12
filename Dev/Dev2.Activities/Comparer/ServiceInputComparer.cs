using System.Collections.Generic;
using Dev2.Common.Interfaces.DB;

namespace Dev2.Comparer
{
    public class ServiceInputComparer : IEqualityComparer<IServiceInput>
    {
        public bool Equals(IServiceInput input1, IServiceInput input2)
        {
            if (input1 == null && input2 == null) return true;
            if (input1 == null || input2 == null) return false;
            var inputsAreTheSame = string.Equals(input1.ActionName, input2.ActionName)
                                   && string.Equals(input1.Dev2ReturnType, input2.Dev2ReturnType)
                                   && string.Equals(input1.FullName, input2.FullName)
                                   && string.Equals(input1.Name, input2.Name)
                                   && string.Equals(input1.Value, input2.Value)
                                   && string.Equals(input1.ShortTypeName, input2.ShortTypeName)
                                   && string.Equals(input1.TypeName, input2.TypeName)
                                   && Equals(input1.RequiredField, input2.RequiredField)
                                   && Equals(input1.IsObject, input2.IsObject)
                                   && Equals(input1.EmptyIsNull, input2.EmptyIsNull);
            return inputsAreTheSame;
        }

        public int GetHashCode(IServiceInput obj)
        {
            return obj.GetHashCode();
        }
    }
}