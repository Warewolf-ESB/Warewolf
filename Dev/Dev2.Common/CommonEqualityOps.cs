using System;
using System.Collections.Generic;
using System.Linq;
using Warewolf.Security.Encryption;

namespace Dev2.Common
{
    public static class CommonEqualityOps
    {

        public static bool AreObjectsEqual<T>(T x, T y)
            where T : IEquatable<T>
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;

            return x.Equals(y);
        }

        public static bool PassWordsCompare(string pass1, string pass2)
        {
            string temPpassword;
            try
            {
                temPpassword = DpapiWrapper.DecryptIfEncrypted(pass1);
            }
            catch (Exception)
            {
                // 
                temPpassword = pass1;
            }

            string temPpassword1;
            try
            {
                temPpassword1 = DpapiWrapper.DecryptIfEncrypted(pass2);
            }
            catch (Exception)
            {
                // 
                temPpassword1 = pass2;
            }

            return string.Equals(temPpassword, temPpassword1);
        }

        public static bool CollectionEquals<T>(IEnumerable<T> source, IEnumerable<T> source1, IEqualityComparer<T> equalityComparer)
        {
            if (source == null && source1 == null) return true;
            if (source == null || source1 == null) return false;
            var sequenceEqual = source.SequenceEqual(source1, equalityComparer);
            return sequenceEqual;
        }
    }
}
