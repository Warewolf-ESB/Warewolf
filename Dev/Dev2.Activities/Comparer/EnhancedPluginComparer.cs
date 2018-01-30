﻿using System.Collections.Generic;
using Dev2.Common;
using Dev2.Common.Interfaces;

namespace Dev2.Comparer
{
    internal class EnhancedPluginComparer : IEqualityComparer<IEnhancedPlugin>
    {
        public bool Equals(IEnhancedPlugin x, IEnhancedPlugin y)
        {
            if (x == null && y == null)
            {
                return true;
            }

            if (x == null || y == null)
            {
                return false;
            }

            bool nameSpacesAreEqual;

            if (x.Namespace != null && y.Namespace != null)
            {
                nameSpacesAreEqual = string.Equals(x.Namespace.AssemblyLocation, y.Namespace.AssemblyLocation)
                                     && string.Equals(x.Namespace.AssemblyName, y.Namespace.AssemblyName)
                                     && string.Equals(x.Namespace.FullName, y.Namespace.FullName)
                                     && string.Equals(x.Namespace.JsonObject, y.Namespace.JsonObject)
                                     && string.Equals(x.Namespace.MethodName, y.Namespace.MethodName);
            }
            else
            {
                nameSpacesAreEqual = x.Namespace == null && y.Namespace == null; ;
            }

            var serviceInputsEquals = CommonEqualityOps.CollectionEquals(x.ConstructorInputs, y.ConstructorInputs, new ServiceInputComparer());
            var methodsEquals = CommonEqualityOps.CollectionEquals(x.MethodsToRun, y.MethodsToRun, new PluginActionComparer());
            var constructorComparer = new PluginConstructorComparer();
            var constructorEquals = constructorComparer.Equals(x.Constructor, y.Constructor);
            return nameSpacesAreEqual && serviceInputsEquals && methodsEquals && constructorEquals;
        }

        public int GetHashCode(IEnhancedPlugin obj) => obj.GetHashCode();
    }
}