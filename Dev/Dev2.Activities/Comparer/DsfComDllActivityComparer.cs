using System.Collections.Generic;
using Dev2.Activities;
using Dev2.Common;

namespace Dev2.Comparer
{
    internal class DsfComDllActivityComparer : IEqualityComparer<DsfComDllActivity>
    {
        public bool Equals(DsfComDllActivity p1, DsfComDllActivity p2)
        {
            if (p1 == null && p2 == null) return true;
            if (p1 == null || p2 == null) return false;

            bool methodsAreEqual;
            bool nameSpacesAreEqual;
            bool outPutsEqual;


            if (p1.Method != null && p2.Method != null)
            {
                methodsAreEqual = string.Equals(p1.Method.Method, p2.Method.Method)
                                  && string.Equals(p1.Method.Dev2ReturnType, p2.Method.Dev2ReturnType)
                                  && string.Equals(p1.Method.ErrorMessage, p2.Method.ErrorMessage)
                                  && string.Equals(p1.Method.FullName, p2.Method.FullName)
                                  && string.Equals(p1.Method.MethodResult, p2.Method.MethodResult)
                                  && string.Equals(p1.Method.OutputVariable, p2.Method.OutputVariable)
                                  && p1.Method.ReturnType == p2.Method.ReturnType
                                  && Equals(p1.Method.HasError, p2.Method.HasError)
                                  && Equals(p1.Method.IsObject, p2.Method.IsObject)
                                  && Equals(p1.Method.IsProperty, p2.Method.IsProperty)
                                  && Equals(p1.Method.IsVoid, p2.Method.IsVoid)
                                  && Equals(p1.Method.ID, p2.Method.ID)
                                  && CommonEqualityOps.CollectionEquals(p1.Method.Inputs, p2.Method.Inputs, new ServiceInputComparer());

            }
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

        public int GetHashCode(DsfComDllActivity obj)
        {
            int hashCode = base.GetHashCode();
            hashCode = (hashCode * 397) ^ ((int)obj?.GetHashCode());
            hashCode = (hashCode * 397) ^ (obj.Method?.GetHashCode() ?? 0);
            hashCode = (hashCode * 397) ^ (obj.Namespace?.GetHashCode() ?? 0);
            return hashCode;
        }
    }

}
