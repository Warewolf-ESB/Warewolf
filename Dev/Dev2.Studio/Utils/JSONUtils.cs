using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.Studio.Utils
{
    public static class JSONUtils
    {
        public static string ScrubJSON(string stringToScrub)
        {
            if(!string.IsNullOrEmpty(stringToScrub))
            {
                if(stringToScrub.StartsWith("\""))
                {
                    stringToScrub = stringToScrub.Remove(0, 1);
                }

                if(stringToScrub.EndsWith("\""))
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
    }
}
