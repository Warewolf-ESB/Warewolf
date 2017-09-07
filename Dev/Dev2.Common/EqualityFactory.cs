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
        
        public static IComparer<T> GetComparer<T>(Func<T, T, int> compareMethod)
        {
            return new InternalComparer<T>(compareMethod);
        }

      public static IEqualityComparer<T> GetEqualityComparer<T>(Func<T, T, bool> equalsMethod, Func<T, int> getHashCodeMethod)
        {
            return new InternalComparer<T>(equalsMethod, getHashCodeMethod);
        }
        
        public static IComparable<T> GetComparable<T>(Func<T, int> compareToMethod)
        {
            return new InternalComparer<T>(compareToMethod);
        }
        
        public static IEquatable<T> GetEquitable<T>(Func<T, bool> equalsMethod)
        {
            return new InternalComparer<T>(equalsMethod);
        }
    }
}