using System.Collections.Generic;
using Dev2.Common;
using Dev2.Common.Interfaces;

namespace Dev2.Comparer
{
    internal class PluginActionComparer : IEqualityComparer<IPluginAction>
    {
#pragma warning disable S1541 // Methods and properties should not be too complex
        public bool Equals(IPluginAction x, IPluginAction y)
#pragma warning restore S1541 // Methods and properties should not be too complex
        {
            if (x == null && y == null)
            {
                return true;
            }

            if (x == null || y == null)
            {
                return false;
            }

            var methodsAreEqual = string.Equals(x.Method, y.Method)
                                 && string.Equals(x.Dev2ReturnType, y.Dev2ReturnType)
                                 && string.Equals(x.ErrorMessage, y.ErrorMessage)
                                 && string.Equals(x.FullName, y.FullName)
                                 && string.Equals(x.MethodResult, y.MethodResult)
                                 && string.Equals(x.OutputVariable, y.OutputVariable)
                                 && x.ReturnType == y.ReturnType
                                 && Equals(x.HasError, y.HasError)
                                 && Equals(x.IsObject, y.IsObject)
                                 && Equals(x.IsProperty, y.IsProperty)
                                 && Equals(x.IsVoid, y.IsVoid)
                                 && Equals(x.ID, y.ID)
                                 && CommonEqualityOps.CollectionEquals(x.Inputs, y.Inputs, new ServiceInputComparer());
            return methodsAreEqual;
        }

        public int GetHashCode(IPluginAction obj) => obj.GetHashCode();
    }
}
