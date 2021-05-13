/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/
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
            if (Equals(x, default(T)) && Equals(y, default(T)))
            {
                return true;
            }

            if (Equals(x, default(T)) || Equals(y, default(T)))
            {
                return false;
            }

            return x.Equals(y);
        }
        public static bool AreObjectsEqualUnSafe<T>(T x, T y)
        {
            if (Equals(x, default(T)) && Equals(y, default(T)))
            {
                return true;
            }

            if (Equals(x, default(T)) || Equals(y, default(T)))
            {
                return false;
            }

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
                temPpassword = pass1;
            }

            string temPpassword1;
            try
            {
                temPpassword1 = DpapiWrapper.DecryptIfEncrypted(pass2);
            }
            catch (Exception)
            {
                temPpassword1 = pass2;
            }

            return string.Equals(temPpassword, temPpassword1);
        }

        public static bool CollectionEquals<T>(IEnumerable<T> source, IEnumerable<T> source1, IEqualityComparer<T> equalityComparer)
        {
            if (source == null && source1 == null)
            {
                return true;
            }

            if (source == null || source1 == null)
            {
                return false;
            }

            var sequenceEqual = source.SequenceEqual(source1, equalityComparer);
            return sequenceEqual;
        }
    }
}
