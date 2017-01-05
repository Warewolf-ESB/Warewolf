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
using System.Xml;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Warewolf.Resource.Errors;

namespace Dev2.Providers.Validation.Rules
{
    public class IsValidXmlRule : Rule<string>
    {
        public IsValidXmlRule(Func<string> getValue)
            : base(getValue)
        {
            ErrorText = ErrorResource.InvalidExpression;
        }

        public override IActionableErrorInfo Check()
        {
            var value = GetValue();
            bool isValid;

            XmlDocument doc = new XmlDocument();
            try
            {
                doc.LoadXml(value);
                isValid = true;
            }
            catch(Exception)
            {
                isValid = false;
            }
            return !isValid ? CreatError() : null;
        }
    }
}
