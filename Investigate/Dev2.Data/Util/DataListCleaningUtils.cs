using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Dev2.DataList.Contract
{
    public class DataListCleaningUtils {

        public static string stripDoubleBracketsAndRecordsetNotation(string canidate) {
            string result = canidate;
            bool isCanidate = isDoubleBracketCanidate(canidate);


            if (canidate.Contains("[[") && isCanidate) {
                result = result.Replace("[[", "");
            }
            
            if (canidate.Contains("]]") && isCanidate) {
                result = result.Replace("]]", "");
            }

            if (result.Contains("(") && result.Contains(")") && isCanidate) {
                result = result.Remove(result.IndexOf("("));
            }

            return result;
        }

        //2013.06.03: Ashley Lewis for bug 9498 - handle multiple regions in result
        public static List<string> SplitIntoRegions(string result)
        {
            if(!String.IsNullOrEmpty(result))
            {
                var allRegions = new List<string>();
                string[] openParts = Regex.Split(result, @"\[\[");
                string[] closeParts = Regex.Split(result, @"\]\]");
                if(openParts.Length == closeParts.Length && openParts.Length > 2 && closeParts.Length > 2)
                {
                    foreach(var newCountNumber in openParts)
                    {
                        if(!string.IsNullOrEmpty(newCountNumber))
                        {
                            string cleanRegion = null;
                            if(newCountNumber.IndexOf("]]") + 2 < newCountNumber.Length)
                            {
                                cleanRegion = "[[" + newCountNumber.Remove(newCountNumber.IndexOf("]]") + 2);
                            }
                            else
                            {
                                cleanRegion = "[[" + newCountNumber;
                            }
                            allRegions.Add(cleanRegion);
                        }
                    }
                }
                else
                {
                    allRegions.Add(result);
                }
                return allRegions;
            }
            else
            {
                return new List<string>(){ null };
            }
        }

        #region Private Method

        private static bool isDoubleBracketCanidate(string canidate) {
            bool result = false;
            char[] tokens = {']'};

            string[] parts = canidate.Split(tokens);

            if (parts.Count() == 3) {
                result = true;
            }


            return result;
        }

        #endregion
    }
}
