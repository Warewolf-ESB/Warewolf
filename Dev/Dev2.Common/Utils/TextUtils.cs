/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
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
            int startIndex = 0;
            while (startIndex != -1 && startIndex < stringToReplaceIn.Length)
            {
                int indexOfReplacement = stringToReplaceIn.IndexOf('\n', startIndex);
                if (indexOfReplacement != -1)
                {
                    bool dontReplace = true;
                    int index = indexOfReplacement - 1;
                    if (index >= 0)
                    {
                        char backwardsLookup = stringToReplaceIn[index];
                        if (backwardsLookup == '\r')
                        {
                            dontReplace = false;
                            startIndex = indexOfReplacement + 2;
                        }
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
    }
}