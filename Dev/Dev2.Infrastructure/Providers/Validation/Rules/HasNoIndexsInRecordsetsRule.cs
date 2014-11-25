
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
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;

namespace Dev2.Providers.Validation.Rules
{
    public class HasNoIndexsInRecordsetsRule : Rule<string>
    {

        public HasNoIndexsInRecordsetsRule(Func<string> getValue)
            : base(getValue)
        {
            ErrorText = "Cannot have index's for recordsets in this field";
        }

        public override IActionableErrorInfo Check()
        {
            var value = GetValue();

            string[] fields = value.Split(',');
            for(int i = 0; i < fields.Length; i++)
            {
                if(!string.IsNullOrEmpty(ExtractIndexRegionFromRecordset(fields[i])))
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
            string result = string.Empty;

            int start = rs.IndexOf("(", StringComparison.Ordinal);
            if(start > 0)
            {
                int end = rs.LastIndexOf(")", StringComparison.Ordinal);
                if(end < 0)
                {
                    end = rs.Length;
                }

                start += 1;
                result = rs.Substring(start, (end - start));
            }

            return result;
        }
    }
}
