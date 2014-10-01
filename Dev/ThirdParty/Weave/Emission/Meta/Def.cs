
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace System.Emission.Meta
{
    #region TypeElementCollection
    internal sealed class TypeElementCollection<TElement> : ICollection<TElement> where TElement : MetaTypeElement, IEquatable<TElement>
    {
        private readonly ICollection<TElement> _items = new List<TElement>();

        public int Count { get { return _items.Count; } }
        bool ICollection<TElement>.IsReadOnly { get { return false; } }

        public void Add(TElement item)
        {
            if (!item.CanBeImplementedExplicitly)
            {
                _items.Add(item);
                return;
            }

            if (Contains(item))
            {
                item.SwitchToExplicitImplementation();
                if (Contains(item)) throw new InvalidOperationException("Duplicate element: " + item);
            }

            _items.Add(item);
        }

        public bool Contains(TElement item)
        {
            foreach (TElement element in _items)
                if (element.Equals(item))
                    return true;

            return false;
        }

        public IEnumerator<TElement> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        void ICollection<TElement>.Clear()
        {
            throw new NotSupportedException();
        }

        void ICollection<TElement>.CopyTo(TElement[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        bool ICollection<TElement>.Remove(TElement item)
        {
            throw new NotSupportedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
    #endregion

    #region MethodSignatureComparer
    internal sealed class MethodSignatureComparer : IEqualityComparer<MethodInfo>
    {
        public static readonly MethodSignatureComparer Instance = new MethodSignatureComparer();

        public bool EqualGenericParameters(MethodInfo x, MethodInfo y)
        {
            if (x.IsGenericMethod != y.IsGenericMethod) return false;

            if (x.IsGenericMethod)
            {
                Type[] xArgs = x.GetGenericArguments();
                Type[] yArgs = y.GetGenericArguments();

                if (xArgs.Length != yArgs.Length) return false;

                for (var i = 0; i < xArgs.Length; ++i)
                {
                    if (xArgs[i].IsGenericParameter != yArgs[i].IsGenericParameter) return false;
                    if (!xArgs[i].IsGenericParameter && !xArgs[i].Equals(yArgs[i])) return false;
                }
            }

            return true;
        }

        public bool EqualParameters(MethodInfo x, MethodInfo y)
        {
            ParameterInfo[] xArgs = x.GetParameters();
            ParameterInfo[] yArgs = y.GetParameters();

            if (xArgs.Length != yArgs.Length) return false;

            for (var i = 0; i < xArgs.Length; ++i)
                if (!EqualSignatureTypes(xArgs[i].ParameterType, yArgs[i].ParameterType))
                    return false;

            return true;
        }

        public bool EqualSignatureTypes(Type x, Type y)
        {
            if (x.IsGenericParameter != y.IsGenericParameter)
                return false;

            if (x.IsGenericParameter)
            {
                if (x.GenericParameterPosition != y.GenericParameterPosition)
                    return false;
            }
            else if (!x.Equals(y)) return false;

            return true;
        }

        public bool Equals(MethodInfo x, MethodInfo y)
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;
            return EqualNames(x, y) && EqualGenericParameters(x, y) && EqualSignatureTypes(x.ReturnType, y.ReturnType) && EqualParameters(x, y);
        }

        public int GetHashCode(MethodInfo obj)
        {
            return obj.Name.GetHashCode() ^ obj.GetParameters().Length;
        }

        private bool EqualNames(MethodInfo x, MethodInfo y)
        {
            return x.Name == y.Name;
        }
    }
    #endregion
}
