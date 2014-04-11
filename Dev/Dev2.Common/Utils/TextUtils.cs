
namespace Dev2.Common.Utils
{
    public static class TextUtils
    {
        public static string ReplaceWorkflowNewLinesWithEnvironmentNewLines(string stringToReplaceIn)
        {

            int startIndex = 0;
            while(startIndex != -1 && startIndex < stringToReplaceIn.Length)
            {
                int indexOfReplacement = stringToReplaceIn.IndexOf('\n', startIndex);
                if(indexOfReplacement != -1)
                {
                    bool dontReplace = true;
                    var index = indexOfReplacement - 1;
                    if(index >= 0)
                    {
                        char backwardsLookup = stringToReplaceIn[index];
                        if(backwardsLookup == '\r')
                        {
                            dontReplace = false;
                            startIndex = indexOfReplacement + 2;
                        }
                    }

                    if(dontReplace)
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
