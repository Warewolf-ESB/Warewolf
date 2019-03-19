#pragma warning disable
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
using Dev2.Data.Interfaces.Enums;

namespace Dev2.Data
{
    public interface IScalar
    {
        string Name { get; set; }
        enDev2ColumnArgumentDirection IODirection { get; set; }
        string Description { get; set; }
        bool IsEditable { get; set; }
        string Value { get; set; }

    }

    public class ScalarEqualityComparer : IEqualityComparer<IScalar>
    {
        public bool Equals(IScalar x, IScalar y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }
            if (x is null || y is null)
            {
                return false;
            }
            return string.Equals(x.Name, y.Name);
        }

        public int GetHashCode(IScalar obj) => obj.Name?.GetHashCode() ?? 0;

    }
    public class Scalar : IScalar, IEquatable<IScalar>
    {
        public string Name { get; set; }
        public enDev2ColumnArgumentDirection IODirection { get; set; }
        public string Description { get; set; }
        public bool IsEditable { get; set; }
        public string Value { get; set; }

        public bool Equals(IScalar other)
        {
            if (other is null)
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            return string.Equals(Name, other.Name);
        }

        public override bool Equals(object obj)
        {
            if (obj is Scalar scalarObj)
            {
                return Equals((Scalar)obj);
            }
            return false;
        }

        public override int GetHashCode() => Name?.GetHashCode() ?? 0;

        public static bool operator ==(Scalar left, Scalar right) => Equals(left, right);

        public static bool operator !=(Scalar left, Scalar right) => !Equals(left, right);
       
        static readonly IEqualityComparer<IScalar> ComparerInstance = new ScalarEqualityComparer();
        public static IEqualityComparer<IScalar> Comparer => ComparerInstance;

    }
}