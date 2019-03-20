using System.Collections.Generic;
using Dev2.Common;
using Dev2.Common.Interfaces;

namespace Dev2.Comparer
{
    internal class PluginConstructorComparer : IEqualityComparer<IPluginConstructor>
    {
#pragma warning disable S1541 // Methods and properties should not be too complex
        public bool Equals(IPluginConstructor x, IPluginConstructor y)
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

            var methodsAreEqual = Equals(x.ID, y.ID)
                                  && string.Equals(x.ConstructorName, y.ConstructorName)
                                  && string.Equals(x.ReturnObject, y.ReturnObject)
                                  && Equals(x.IsExistingObject, y.IsExistingObject)
                                  && CommonEqualityOps.CollectionEquals(x.Inputs, y.Inputs, EqualityFactory.GetEqualityComparer<IConstructorParameter>(
                                      (parameter, constructorParameter) => string.Equals(parameter.Name, constructorParameter.Name)
                                                                           && string.Equals(parameter.Value, constructorParameter.Value)
                                                                           && Equals(parameter.EmptyToNull, constructorParameter.EmptyToNull)
                                                                           && Equals(parameter.Dev2ReturnType, constructorParameter.Dev2ReturnType), parameter => 1));
            return methodsAreEqual;
        }

        public int GetHashCode(IPluginConstructor obj) => obj.GetHashCode();
    }
}