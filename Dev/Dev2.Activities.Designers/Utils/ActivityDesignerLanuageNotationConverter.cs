
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
using Dev2.Data.Util;
using Dev2.DataList.Contract;

namespace Dev2.Activities.Utils
{
    /// <summary>
    /// Supports the Activity Designers with Lanuage Bits ;)
    /// </summary>
    public static class ActivityDesignerLanuageNotationConverter
    {

        /// <summary>
        /// Converts the a data lang fragment to top level rs notation.
        /// </summary>
        /// <param name="val">The value.</param>
        /// <returns></returns>
        public static string ConvertToTopLevelRSNotation(string val)
        {
            var text = val;

            if(!String.IsNullOrEmpty(text))
            {
                var idxType = DataListUtil.GetRecordsetIndexType(text);

                // extract name if fields where entered 
                text = DataListUtil.ExtractRecordsetNameFromValue(text);

                // must be in scalar format, use it and conver to RS notation
                if (string.IsNullOrEmpty(text))
                {
                    text = val;
                }

                // convert to rs notation already ;)
                text = DataListUtil.MakeValueIntoHighLevelRecordset(text);

                // finally add brackets if required ;)
                text = DataListUtil.AddBracketsToValueIfNotExist(text);

                // be nice about it and keep the star if they put it there ;)
                if (idxType == enRecordsetIndexType.Star)
                {
                    text = text.Replace("()", "(*)");
                }

                return text;
            }

            return string.Empty;
        }

    }
}
