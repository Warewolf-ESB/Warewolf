using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
