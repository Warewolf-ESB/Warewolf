/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
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
                throw new ArgumentNullException("", String.Format(ErrorResource.ArgumentsNotAllowedToBeNull, args.Where(a => a.Value == null).Aggregate("", (a, b) => string.Format("{0}{1}{2}", a, b.Key, Environment.NewLine))));
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
        /// Indicates that the marked parameter should be evaluated to true
        /// </summary>
        IS_TRUE = 0,

        /// <summary>
        /// Indicates that the marked parameter should be evaluated to false
        /// </summary>
        IS_FALSE = 1,

        /// <summary>
        /// Indicates that the marked parameter should be evaluated to null value
        /// </summary>
        IS_NULL = 2,

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

    /// <summary>
    /// Indicates that the marked symbol is used implicitly (e.g. via reflection, in external library),
    /// so this symbol will not be marked as unused (as well as by other usage inspections)
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public sealed class UsedImplicitlyAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UsedImplicitlyAttribute"/> class.
        /// </summary>
        [UsedImplicitly]
        public UsedImplicitlyAttribute()
            : this(ImplicitUseKindFlags.Default, ImplicitUseTargetFlags.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UsedImplicitlyAttribute"/> class.
        /// </summary>
        /// <param name="useKindFlags">The use kind flags.</param>
        /// <param name="targetFlags">The target flags.</param>
        [UsedImplicitly]
        public UsedImplicitlyAttribute(ImplicitUseKindFlags useKindFlags, ImplicitUseTargetFlags targetFlags)
        {
            UseKindFlags = useKindFlags;
            TargetFlags = targetFlags;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UsedImplicitlyAttribute"/> class.
        /// </summary>
        /// <param name="useKindFlags">The use kind flags.</param>
        [UsedImplicitly]
        public UsedImplicitlyAttribute(ImplicitUseKindFlags useKindFlags)
            : this(useKindFlags, ImplicitUseTargetFlags.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UsedImplicitlyAttribute"/> class.
        /// </summary>
        /// <param name="targetFlags">The target flags.</param>
        [UsedImplicitly]
        public UsedImplicitlyAttribute(ImplicitUseTargetFlags targetFlags)
            : this(ImplicitUseKindFlags.Default, targetFlags)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        [UsedImplicitly]
        public ImplicitUseKindFlags UseKindFlags { get; private set; }

        /// <summary>
        /// Gets value indicating what is meant to be used
        /// </summary>
        [UsedImplicitly]
        public ImplicitUseTargetFlags TargetFlags { get; private set; }
    }

    /// <summary>
    /// 
    /// </summary>
    [Flags]
    public enum ImplicitUseKindFlags
    {
        /// <summary>
        /// 
        /// </summary>
        Default = Access | Assign | InstantiatedWithFixedConstructorSignature,

        /// <summary>
        /// Only entity marked with attribute considered used
        /// </summary>
        Access = 1,

        /// <summary>
        /// Indicates implicit assignment to a member
        /// </summary>
        Assign = 2,

        /// <summary>
        /// Indicates implicit instantiation of a type with fixed constructor signature.
        /// That means any unused constructor parameters won't be reported as such.
        /// </summary>
        InstantiatedWithFixedConstructorSignature = 4,

        /// <summary>
        /// Indicates implicit instantiation of a type
        /// </summary>
        InstantiatedNoFixedConstructorSignature = 8
    }

    /// <summary>
    /// Specify what is considered used implicitly when marked with <see cref="MeansImplicitUseAttribute"/> or <see cref="UsedImplicitlyAttribute"/>
    /// </summary>
    [Flags]
    public enum ImplicitUseTargetFlags
    {
        /// <summary>
        /// 
        /// </summary>
        Default = Itself,

        /// <summary>
        /// 
        /// </summary>
        Itself = 1,

        /// <summary>
        /// Members of entity marked with attribute are considered used
        /// </summary>
        Members = 2,

        /// <summary>
        /// Entity marked with attribute and all its members considered used
        /// </summary>
        WithMembers = Itself | Members
    }
}