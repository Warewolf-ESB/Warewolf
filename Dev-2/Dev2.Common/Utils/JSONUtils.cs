
using System;

namespace Dev2.Common.Utils
{
    public static class JSONUtils
    {
        public static string ScrubJSON(string stringToScrub)
        {
            if (!string.IsNullOrEmpty(stringToScrub))
            {
                if (stringToScrub.StartsWith("\""))
                {
                    stringToScrub = stringToScrub.Remove(0, 1);
                }

                if (stringToScrub.EndsWith("\""))
                {
                    int indexTORemoveFrom = stringToScrub.Length - 1;
                    stringToScrub = stringToScrub.Remove(indexTORemoveFrom, 1);
                }

                //if (stringToScrub.Contains("\\\\"))
                //{
                //    stringToScrub = stringToScrub.Replace("\\\\", "\\");
                //}
            }
            return stringToScrub;
        }

        public static string ReplaceSlashes(string stringToReplaceIn)
        {
            if(stringToReplaceIn.Contains("\\") || stringToReplaceIn.Contains("//"))
            {
                int indexOfSlash = stringToReplaceIn.IndexOf("\\", StringComparison.InvariantCulture);
                if(indexOfSlash != -1)
                {
                    if(stringToReplaceIn[indexOfSlash+1] != '"')
                    {
                        stringToReplaceIn = stringToReplaceIn.Remove(indexOfSlash, 1);
                        stringToReplaceIn = stringToReplaceIn.Insert(indexOfSlash, "/");
                        stringToReplaceIn = ReplaceSlashes(stringToReplaceIn);
                    }
                }
            }            
            return stringToReplaceIn.Replace("//", "/");
        }
    }
}
