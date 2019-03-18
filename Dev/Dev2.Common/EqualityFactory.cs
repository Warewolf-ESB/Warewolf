#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
﻿/*
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

namespace Dev2.Common
{
    
    public static class EqualityFactory
    {

        private class InternalComparer<T> : IComparer<T>, IEqualityComparer<T>, IComparable<T>, IEquatable<T>
        {

            private readonly Func<T, T, int> _compareMethod;
            private readonly Func<T, T, bool> _equalityEqualsMethod;
            private readonly Func<T, int> _getHashCodeMethod;
            private readonly Func<T, int> _compareToMethod;
            private readonly Func<T, bool> _equitableEqualsMethod;


            public InternalComparer(Func<T, T, int> compareMethod)
            {
                _compareMethod = compareMethod;
            }

            int IComparer<T>.Compare(T x, T y)
            {
                ThrowIfNotImplemented(_compareMethod);
                return _compareMethod(x, y);
            }
            
            public InternalComparer(Func<T, T, bool> equalsMethod, Func<T, int> getHashCodeMethod)
            {
                _equalityEqualsMethod = equalsMethod;
                _getHashCodeMethod = getHashCodeMethod;
            }

            bool IEqualityComparer<T>.Equals(T x, T y)
            {
                ThrowIfNotImplemented(_equalityEqualsMethod);
                return _equalityEqualsMethod(x, y);
            }

            int IEqualityComparer<T>.GetHashCode(T obj)
            {
                ThrowIfNotImplemented(_getHashCodeMethod);
                return _getHashCodeMethod(obj);
            }
            
            public InternalComparer(Func<T, int> compareToMethod)
            {
                _compareToMethod = compareToMethod;
            }

            int IComparable<T>.CompareTo(T other)
            {
                ThrowIfNotImplemented(_compareToMethod);
                return _compareToMethod(other);
            }

            public InternalComparer(Func<T, bool> equalsMethod)
            {
                _equitableEqualsMethod = equalsMethod;
            }

            bool IEquatable<T>.Equals(T other)
            {
                ThrowIfNotImplemented(_equitableEqualsMethod);
                return _equitableEqualsMethod(other);
            }

            private static void ThrowIfNotImplemented(Delegate method)
            {
                if (method == null)
                {
                    throw new NotImplementedException("This method is not implemented for this instance.");
                }
            }

        }

        public static IComparer<T> GetComparer<T>(Func<T, T, int> compareMethod) => new InternalComparer<T>(compareMethod);

        public static IEqualityComparer<T> GetEqualityComparer<T>(Func<T, T, bool> equalsMethod, Func<T, int> getHashCodeMethod)
        {
            return new InternalComparer<T>(equalsMethod, getHashCodeMethod);
        }

        public static IComparable<T> GetComparable<T>(Func<T, int> compareToMethod) => new InternalComparer<T>(compareToMethod);

        public static IEquatable<T> GetEquitable<T>(Func<T, bool> equalsMethod)
        {
            return new InternalComparer<T>(equalsMethod);
        }
    }
}