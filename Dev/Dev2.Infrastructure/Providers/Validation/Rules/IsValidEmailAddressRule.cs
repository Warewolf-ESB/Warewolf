#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.Common.ExtMethods;

namespace Dev2.Providers.Validation.Rules
{
    public class IsValidEmailAddressRule : IsValidCollectionRule
    {
        public IsValidEmailAddressRule(Func<string> getValue)
            : this(getValue, ';')
        {
        }

        public IsValidEmailAddressRule(Func<string> getValue, char splitToken)
            : base(getValue, "email address", splitToken)
        {
        }

        protected override bool IsValid(string item) => item.IsEmail();
    }
}
