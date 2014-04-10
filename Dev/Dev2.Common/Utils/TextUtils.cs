using System;

namespace Dev2.Common.Utils
{
    public static class TextUtils
    {
        public static string ReplaceWorkflowNewLinesWithEnvironmentNewLines(string stringToReplaceIn)
        {

            if(stringToReplaceIn.Contains("\n"))
            {
                int startIndex = 0;
                while(startIndex != -1 && startIndex < stringToReplaceIn.Length)
                {
                    int indexOfReplacement = stringToReplaceIn.IndexOf("\n", startIndex, StringComparison.Ordinal);
                    if(indexOfReplacement != -1)
                    {
                        bool dontReplace = true;
                        if(indexOfReplacement > 1)
                        {
                            string backwardsLookup = stringToReplaceIn.Substring(indexOfReplacement - 1, 1);
                            if(backwardsLookup == "\r" || backwardsLookup[0] == '\\')
                            {
                                dontReplace = false;
                                startIndex = indexOfReplacement + 2;
                            }
                        }
                        else if(indexOfReplacement == 1)
                        {
                            string backwardsLookup = stringToReplaceIn.Substring(indexOfReplacement - 1, 1);
                            if(backwardsLookup == "\\")
                            {
                                dontReplace = false;
                                startIndex = indexOfReplacement + 2;
                            }
                        }

                        if(dontReplace)
                        {
                            stringToReplaceIn = stringToReplaceIn.Insert(indexOfReplacement, "\r");
                            startIndex = indexOfReplacement + 4;
                        }
                    }
                    else
                    {
                        startIndex = -1;
                    }
                }
            }

            return stringToReplaceIn;
        }
    }
}
