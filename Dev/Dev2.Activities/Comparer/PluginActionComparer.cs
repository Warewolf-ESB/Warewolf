using System.Collections.Generic;
using Dev2.Common;
using Dev2.Common.Interfaces;

namespace Dev2.Comparer
{
    internal class PluginActionComparer : IEqualityComparer<IPluginAction>
    {
        public bool Equals(IPluginAction p1, IPluginAction p2)
        {
            if (p1 == null && p2 == null) return true;
            if (p1 == null || p2 == null) return false;
            var methodsAreEqual = string.Equals(p1.Method, p2.Method)
                                 && string.Equals(p1.Dev2ReturnType, p2.Dev2ReturnType)
                                 && string.Equals(p1.ErrorMessage, p2.ErrorMessage)
                                 && string.Equals(p1.FullName, p2.FullName)
                                 && string.Equals(p1.MethodResult, p2.MethodResult)
                                 && string.Equals(p1.OutputVariable, p2.OutputVariable)
                                 && p1.ReturnType == p2.ReturnType
                                 && Equals(p1.HasError, p2.HasError)
                                 && Equals(p1.IsObject, p2.IsObject)
                                 && Equals(p1.IsProperty, p2.IsProperty)
                                 && Equals(p1.IsVoid, p2.IsVoid)
                                 && Equals(p1.ID, p2.ID)
                                 && CommonEqualityOps.CollectionEquals(p1.Inputs, p2.Inputs, new ServiceInputComparer());
            return methodsAreEqual;
        }

        public int GetHashCode(IPluginAction obj)
        {
            return obj.GetHashCode();
        }
    }
}
