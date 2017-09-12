using System.Collections.Generic;
using Dev2.Common.Interfaces;

namespace Dev2.Comparer
{
    internal class SimplePluginComparer : IEqualityComparer<ISimpePlugin>
    {
        public bool Equals(ISimpePlugin p1, ISimpePlugin p2)
        {
            if (p1 == null && p2 == null) return true;
            if (p1 == null || p2 == null) return false;

            bool methodsAreEqual;
            bool nameSpacesAreEqual;
            bool outPutsEqual;


            if (p1.Method != null && p2.Method != null)
            {
                var actionComparer = new PluginActionComparer();
                methodsAreEqual= actionComparer.Equals(p1.Method, p2.Method);}
            else
            {
                methodsAreEqual = p1.Method == null && p2.Method == null;
            }

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

            if (p1.OutputDescription != null && p2.OutputDescription != null)
            {
                outPutsEqual = p1.OutputDescription.Equals(p2.OutputDescription);
            }
            else
            {
                outPutsEqual = p1.OutputDescription == null && p2.OutputDescription == null;
            }
            return methodsAreEqual && nameSpacesAreEqual && outPutsEqual;
        }

        public int GetHashCode(ISimpePlugin obj)
        {
            int hashCode = base.GetHashCode();
            hashCode = (hashCode * 397) ^ ((int)obj?.GetHashCode());
            hashCode = (hashCode * 397) ^ (obj.Method?.GetHashCode() ?? 0);
            hashCode = (hashCode * 397) ^ (obj.Namespace?.GetHashCode() ?? 0);
            return hashCode;
        }
    }

}
