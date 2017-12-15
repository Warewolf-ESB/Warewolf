using System.Collections.Generic;
using Dev2.Common;
using Dev2.Common.Interfaces;

namespace Dev2.Comparer
{
    internal class EnhancedPluginComparer : IEqualityComparer<IEnhancedPlugin>
    {
        public bool Equals(IEnhancedPlugin p1, IEnhancedPlugin p2)
        {
            if (p1 == null && p2 == null) return true;
            if (p1 == null || p2 == null) return false;

            bool nameSpacesAreEqual;

            if (p1.Namespace != null && p2.Namespace != null)
            {
                nameSpacesAreEqual = string.Equals(p1.Namespace.AssemblyLocation, p2.Namespace.AssemblyLocation)
                                     && string.Equals(p1.Namespace.AssemblyName, p2.Namespace.AssemblyName)
                                     && string.Equals(p1.Namespace.FullName, p2.Namespace.FullName)
                                     && string.Equals(p1.Namespace.JsonObject, p2.Namespace.JsonObject)
                                     && string.Equals(p1.Namespace.MethodName, p2.Namespace.MethodName);
            }
            else
            {
                nameSpacesAreEqual = p1.Namespace == null && p2.Namespace == null; ;
            }

            var serviceInputsEquals = CommonEqualityOps.CollectionEquals(p1.ConstructorInputs, p2.ConstructorInputs, new ServiceInputComparer());
            var methodsEquals = CommonEqualityOps.CollectionEquals(p1.MethodsToRun, p2.MethodsToRun, new PluginActionComparer());
            var constructorComparer = new PluginConstructorComparer();
            var constructorEquals = constructorComparer.Equals(p1.Constructor, p2.Constructor);
            return nameSpacesAreEqual && serviceInputsEquals && methodsEquals && constructorEquals;
        }

        public int GetHashCode(IEnhancedPlugin obj)
        {
            return obj.GetHashCode();
        }
    }
}