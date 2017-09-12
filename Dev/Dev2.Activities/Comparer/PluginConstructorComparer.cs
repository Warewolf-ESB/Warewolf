using System.Collections.Generic;
using Dev2.Common;
using Dev2.Common.Interfaces;

namespace Dev2.Comparer
{
    internal class PluginConstructorComparer : IEqualityComparer<IPluginConstructor>
    {
        public bool Equals(IPluginConstructor p1, IPluginConstructor p2)
        {
            if (p1 == null && p2 == null) return true;
            if (p1 == null || p2 == null) return false;
            var methodsAreEqual = Equals(p1.ID, p2.ID)
                                  && string.Equals(p1.ConstructorName, p2.ConstructorName)
                                  && string.Equals(p1.ReturnObject, p2.ReturnObject)
                                  && CommonEqualityOps.CollectionEquals(p1.Inputs, p2.Inputs, EqualityFactory.GetEqualityComparer<IConstructorParameter>(
                                      (parameter, constructorParameter) => string.Equals(parameter.Name, constructorParameter.Name)
                                                                           && string.Equals(parameter.Value, constructorParameter.Value)
                                                                           && Equals(parameter.EmptyToNull, constructorParameter.EmptyToNull)
                                                                           && Equals(parameter.Dev2ReturnType, constructorParameter.Dev2ReturnType), parameter => 1));
            return methodsAreEqual;
        }

        public int GetHashCode(IPluginConstructor obj)
        {
            return obj.GetHashCode();
        }
    }
}