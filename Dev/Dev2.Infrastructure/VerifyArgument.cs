/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Warewolf.Resource.Errors;
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedParameter.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Dev2
{
    /// <summary>
    /// Performs verification of arguments
    /// </summary>
    public static class VerifyArgument
    {
        /// <summary>
        /// Verifies that the argument is not null, otherwise throws an ArgumentNullException.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="argument">The argument.</param>
        [AssertionMethod]
        public static void IsNotNull([NotNull] string name, [AssertionCondition(AssertionConditionType.IS_NOT_NULL)] object argument)
        {
            if(argument == null)
            {
                throw new ArgumentNullException(name);
            }
        }

        [AssertionMethod]
        public static void AreNotNull([AssertionCondition(AssertionConditionType.IS_NOT_NULL)] IDictionary<string, object> args)
        {
            if(args.Any(a => a.Value == null))
            {
                throw new ArgumentNullException(@"", String.Format(ErrorResource.ArgumentsNotAllowedToBeNull, args.Where(a => a.Value == null).Aggregate(@"", (a, b) => $"{a}{b.Key}{Environment.NewLine}")));
            }
        }

        /// <summary>
        /// Verifies that the given string is not null, empty or whitespace.
        /// </summary>
        /// <param name="name">The argument name.</param>
        /// <param name="value">The value.</param>
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public static void IsNotNullOrWhitespace([NotNull] string name, string value)
        {
            if(string.IsNullOrWhiteSpace(value) || string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException(name, ErrorResource.NullArgument);
            }
        }
    }

    /*
 * Copyright 2007-2011 JetBrains s.r.o.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

    /// <summary>
    /// Indicates that the marked method is assertion method, i.e. it halts control flow if one of the conditions is satisfied. 
    /// To set the condition, mark one of the parameters with <see cref="AssertionConditionAttribute"/> attribute
    /// </summary>
    /// <seealso cref="AssertionConditionAttribute"/>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class AssertionMethodAttribute : Attribute
    {
    }

    /// <summary>
    /// Indicates the condition parameter of the assertion method. 
    /// The method itself should be marked by <see cref="AssertionMethodAttribute"/> attribute.
    /// The mandatory argument of the attribute is the assertion type.
    /// </summary>
    /// <seealso cref="AssertionConditionType"/>
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class AssertionConditionAttribute : Attribute
    {
        /// <summary>
        /// Initializes new instance of AssertionConditionAttribute
        /// </summary>
        /// <param name="conditionType">Specifies condition type</param>
        public AssertionConditionAttribute(AssertionConditionType conditionType)
        {
            ConditionType = conditionType;
        }

        /// <summary>
        /// Gets condition type
        /// </summary>
        public AssertionConditionType ConditionType { get; private set; }
    }

    /// <summary>
    /// Specifies assertion type. If the assertion method argument satisifes the condition, then the execution continues. 
    /// Otherwise, execution is assumed to be halted
    /// </summary>
    public enum AssertionConditionType
    {
        /// <summary>
        /// Indicates that the marked parameter should be evaluated to not null value
        /// </summary>
        IS_NOT_NULL = 3
    }

    /// <summary>
    /// Indicates that the value of marked element could never be <c>null</c>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.Delegate | AttributeTargets.Field)]
    public sealed class NotNullAttribute : Attribute
    {
    }
}