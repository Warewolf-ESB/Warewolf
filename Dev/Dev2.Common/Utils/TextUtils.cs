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

namespace Dev2.Common.Utils
{
    public static class TextUtils
    {
        public static string ReplaceWorkflowNewLinesWithEnvironmentNewLines(string stringToReplaceIn)
        {
            var startIndex = 0;
            while (startIndex != -1 && startIndex < stringToReplaceIn.Length)
            {
                var indexOfReplacement = stringToReplaceIn.IndexOf('\n', startIndex);
                if (indexOfReplacement != -1)
                {
                    var dontReplace = true;
                    var index = indexOfReplacement - 1;
                    if (index >= 0)
                    {
                        BackwardsLookup(stringToReplaceIn, ref startIndex, indexOfReplacement, ref dontReplace, index);
                    }

                    if (dontReplace)
                    {
                        stringToReplaceIn = stringToReplaceIn.Insert(indexOfReplacement, "\r");
                        startIndex = indexOfReplacement + 2;
                    }
                }
                else
                {
                    startIndex = -1;
                }
            }

            return stringToReplaceIn;
        }

        private static void BackwardsLookup(string stringToReplaceIn, ref int startIndex, int indexOfReplacement, ref bool dontReplace, int index)
        {
            var backwardsLookup = stringToReplaceIn[index];
            if (backwardsLookup == '\r')
            {
                dontReplace = false;
                startIndex = indexOfReplacement + 2;
            }
        }
    }
}