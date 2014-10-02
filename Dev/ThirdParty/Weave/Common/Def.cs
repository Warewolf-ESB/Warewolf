
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace System
{
    [StructLayout(LayoutKind.Explicit, Size = SizeInBytes)]
    internal struct TPKey : IEquatable<TPKey>, IComparable<TPKey>
    {
        #region Constants
        public const int SizeInBytes = 4;
        #endregion

        #region Readonly Fields
        public static readonly TPKey Invalid = new TPKey();
        #endregion

        #region Instance Fields
        [FieldOffset(0)]
        internal int Value;
        #endregion

        #region Public Properties
        public bool Valid { get { return Value != 0; } }
        #endregion

        #region Constructor
        internal TPKey(int value)
        {
            Value = value;
        }
        #endregion

        #region Overrides
        public override string ToString()
        {
            return "TPKey " + (Valid ? "[Valid]" : "[Invalid]");
        }

        public override int GetHashCode()
        {
            return Value;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is TPKey)) return false;
            TPKey other = (TPKey)obj;
            return Value == other.Value;
        }

        public bool Equals(TPKey other)
        {
            return Value == other.Value;
        }
        #endregion

        #region Comparison Handling
        public int CompareTo(TPKey comparand)
        {
            return Value.CompareTo(comparand.Value);
        }
        #endregion

        #region Operator Overloads
        public static bool operator ==(TPKey l, TPKey r)
        {
            return l.Value == r.Value;
        }

        public static bool operator !=(TPKey l, TPKey r)
        {
            return l.Value != r.Value;
        }
        #endregion
    }
}
