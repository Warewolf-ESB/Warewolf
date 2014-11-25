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
using System.Linq.Expressions;

namespace Dev2.Reflection
{
    /// <summary>
    ///     Note in the case of a non-null Nullable&lt;T&gt; as well as a null Nullable&lt;T&gt;;
    ///     the first is over fifteen times slower than checking against a value type that is not Nullable&lt;T&gt;
    ///     while the second is at least eight times as slow. The reason for this is boxing.
    ///     For every instance of Nullable&lt;T&gt; that is passed in, when casting to object for a
    ///     comparison, the value type has to be boxed, which means an allocation on the heap, etc.
    ///     This can be improved upon, however, by compiling code on the fly. This helper class can be
    ///     defined which will provide the implementation of a call to IsNull, assigned on the fly
    ///     when the type is created.
    ///     See
    ///     http://stackoverflow.com/questions/12396457/checking-instance-of-non-class-constrained-type-parameter-for-null-in-generic-me
    /// </summary>
    /// <typeparam name="T">The type to be checked</typeparam>
    internal static class IsNullHelper<T>
    {
        public static readonly Predicate<T> IsNull = CreatePredicate();

        private static Predicate<T> CreatePredicate()
        {
            // If the default is not null, then set to false.
            // ReSharper disable RedundantCast
            if (((object) default(T)) != null)
            {
                return t => false;
            }
            // ReSharper restore RedundantCast

            // Create the expression that checks and return.
            ParameterExpression p = Expression.Parameter(typeof (T), "t");

            // Compare to null.
            BinaryExpression equals = Expression.Equal(p, Expression.Constant(null, typeof (T)));

            // Create the lambda and return.
            return Expression.Lambda<Predicate<T>>(equals, p).Compile();
        }
    }
}