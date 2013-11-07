using System;
using Dev2.Providers.Logs;

namespace Dev2.Common.Utils
{
    public static class JSONUtils
    {
        /// <summary>
        /// Scrubs the JSON.
        /// </summary>
        /// <param name="stringToScrub">The string to scrub.</param>
        /// <returns></returns>
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

            }
            return stringToScrub;
        }

        /// <summary>
        /// Replaces the slashes.
        /// </summary>
        /// <param name="stringToReplaceIn">The string to replace in.</param>
        /// <returns></returns>
        public static string ReplaceSlashes(string stringToReplaceIn)
        {
            var replaceSlashes = stringToReplaceIn.Replace(@"\", @"\\");
            return replaceSlashes;
        }
    }
}
