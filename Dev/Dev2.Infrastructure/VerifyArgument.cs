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
using System.Linq;
using Warewolf.Resource.Errors;


namespace Dev2
{
    public static class VerifyArgument
    {
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
    }
}