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
using System.Collections.Generic;
using System.Linq;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Warewolf.Resource.Errors;

namespace Dev2.Providers.Validation.Rules
{
    public class HasNoDuplicateEntriesRule : Rule<string>
    {
        public HasNoDuplicateEntriesRule(Func<string> getValue)
            : base(getValue)
        {
            ErrorText = ErrorResource.CannotHaveDuplicateFields;
        }

        public override IActionableErrorInfo Check()
        {
            var value = GetValue();
            var fields = value.Split(',');
            for (int i = 0; i < fields.Length; i++)
            {
                fields[i] = ReplaceRecordsetIndexWithBlank(fields[i]);
            }

            var enumerable = fields.Distinct();

            if (enumerable.Count() != fields.Length)
            {
                return CreatError();
            }

            return null;
        }

        /// <summary>
        /// Replaces the index of a recordset with a blank index.
        /// </summary>
        /// <param name="expression">The expession.</param>
        /// <returns></returns>
        public static string ReplaceRecordsetIndexWithBlank(string expression)
        {
            var extractIndexRegionFromRecordset = ExtractIndexRegionFromRecordset(expression);
            return expression.Replace("(" + extractIndexRegionFromRecordset + ")", "()");
        }

        /// <summary>
        /// Used to extract an index in the recordset notation
        /// </summary>
        /// <param name="rs">The rs.</param>
        /// <returns></returns>
        public static string ExtractIndexRegionFromRecordset(string rs)
        {
            var result = string.Empty;

            var start = rs.IndexOf("(", StringComparison.Ordinal);
            if (start > 0)
            {
                var end = rs.LastIndexOf(")", StringComparison.Ordinal);
                if (end < 0)
                {
                    end = rs.Length;
                }

                start += 1;
                result = rs.Substring(start, end - start);
            }

            return result;
        }
    }
}
