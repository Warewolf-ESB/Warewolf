using System.Collections.Generic;
using Dev2.Common.Interfaces;

namespace Dev2.Comparer
{
    internal class SimplePluginComparer : IEqualityComparer<ISimpePlugin>
    {
#pragma warning disable S1541 // Methods and properties should not be too complex
        public bool Equals(ISimpePlugin x, ISimpePlugin y)
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

            bool methodsAreEqual;
            bool nameSpacesAreEqual;
            bool outPutsEqual;


            if (x.Method != null && y.Method != null)
            {
                var actionComparer = new PluginActionComparer();
                methodsAreEqual= actionComparer.Equals(x.Method, y.Method);}
            else
            {
                methodsAreEqual = x.Method == null && y.Method == null;
            }

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
                nameSpacesAreEqual = x.Namespace == null && y.Namespace == null;
            }

            if (x.OutputDescription != null && y.OutputDescription != null)
            {
                outPutsEqual = x.OutputDescription.Equals(y.OutputDescription);
            }
            else
            {
                outPutsEqual = x.OutputDescription == null && y.OutputDescription == null;
            }
            return methodsAreEqual && nameSpacesAreEqual && outPutsEqual;
        }

        public int GetHashCode(ISimpePlugin obj)
        {
            var hashCode = base.GetHashCode();
            hashCode = (hashCode * 397) ^ ((int)obj?.GetHashCode());
            hashCode = (hashCode * 397) ^ (obj.Method?.GetHashCode() ?? 0);
            hashCode = (hashCode * 397) ^ (obj.Namespace?.GetHashCode() ?? 0);
            return hashCode;
        }
    }

}
