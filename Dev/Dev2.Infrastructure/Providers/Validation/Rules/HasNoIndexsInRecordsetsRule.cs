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
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Warewolf.Resource.Errors;

namespace Dev2.Providers.Validation.Rules
{
    public class HasNoIndexsInRecordsetsRule : Rule<string>
    {

        public HasNoIndexsInRecordsetsRule(Func<string> getValue)
            : base(getValue)
        {
            ErrorText = ErrorResource.CannotHaveIndexsForRecordsets;
        }

        public override IActionableErrorInfo Check()
        {
            var value = GetValue();

            var fields = value.Split(',');
            for (int i = 0; i < fields.Length; i++)
            {
                var indexValue = ExtractIndexRegionFromRecordset(fields[i]);
                if (!string.IsNullOrEmpty(indexValue) && indexValue!="*")
                {
                    return CreatError();
                }
            }

            return null;
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
