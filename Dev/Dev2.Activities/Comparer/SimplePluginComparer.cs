#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
﻿using System.Collections.Generic;
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
