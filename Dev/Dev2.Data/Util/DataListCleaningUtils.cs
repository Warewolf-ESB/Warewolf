using Dev2.Data.Parsers;
using Dev2.Data.TO;
using Dev2.Data.Util;
using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable CheckNamespace
namespace Dev2.DataList.Contract
// ReSharper restore CheckNamespace
{
    public class DataListCleaningUtils
    {

        // ReSharper disable InconsistentNaming
        public static string stripDoubleBracketsAndRecordsetNotation(string canidate)
        // ReSharper restore InconsistentNaming
        {
            string result = canidate;
            bool isCanidate = IsDoubleBracketCanidate(canidate);


            if(canidate.Contains("[[") && isCanidate)
            {
                result = result.Replace("[[", "");
            }

            if(canidate.Contains("]]") && isCanidate)
            {
                result = result.Replace("]]", "");
            }

            if(result.Contains("(") && result.Contains(")") && isCanidate)
            {
                result = result.Remove(result.IndexOf("(", StringComparison.Ordinal));
            }

            return result;
        }

        //2013.06.03: Ashley Lewis for bug 9498 - handle multiple regions in result
        public static List<string> SplitIntoRegions(string result)
        {
            if(!String.IsNullOrEmpty(result))
            {
                try
                {
                    var allRegions = new List<string>();
                    Dev2DataLanguageParser parser = new Dev2DataLanguageParser();
                    IList<ParseTO> makeParts = parser.MakeParts(result);
                    foreach(var makePart in makeParts.Where(c => !c.HangingOpen))
                    {
                        if(makePart.Child != null)
                        {
                            int indexOfBracket = makePart.Payload.IndexOf("(", StringComparison.Ordinal);
                            string tmpresult = makePart.Payload.Insert(indexOfBracket + 1, DataListUtil.AddBracketsToValueIfNotExist(makePart.Child.Payload));
                            allRegions.Add(string.Concat("[[", tmpresult, "]]"));
                        }
                        else
                        {
                            allRegions.Add(string.Concat("[[", makePart.Payload, "]]"));
                        }
                    }
                    return allRegions;
                }
                catch(Exception)
                {

                    return new List<string> { null };
                }
            }
            return new List<string> { null };
        }

        private static IEnumerable<string> AddChildrenPart(ParseTO child)
        {
            List<string> results = new List<string>();
            if(child != null)
            {
                results.Add(DataListUtil.AddBracketsToValueIfNotExist(child.Payload));
                if(child.Child != null)
                {
                    results.AddRange(AddChildrenPart(child.Child).Select(DataListUtil.AddBracketsToValueIfNotExist));
                }
            }
            return results;
        }

        public static List<string> SplitIntoRegionsForFindMissing(string result)
        {
            if(!String.IsNullOrEmpty(result))
            {
                try
                {
                    var allRegions = new List<string>();
                    Dev2DataLanguageParser parser = new Dev2DataLanguageParser();
                    IList<ParseTO> makeParts = parser.MakeParts(result);
                    foreach(var makePart in makeParts.Where(c => !c.HangingOpen))
                    {
                        allRegions.Add(DataListUtil.AddBracketsToValueIfNotExist(makePart.Payload));
                        allRegions.AddRange(AddChildrenPartForFindMissing(makePart.Child));
                    }
                    return allRegions;
                }
                catch(Exception)
                {
                    return new List<string> { null };
                }
            }
            return new List<string> { null };

        }

        private static IEnumerable<string> AddChildrenPartForFindMissing(ParseTO child)
        {
            List<string> results = new List<string>();
            if(child != null)
            {
                results.Add(DataListUtil.AddBracketsToValueIfNotExist(child.Payload));
                if(child.Child != null)
                {
                    results.AddRange(AddChildrenPart(child.Child).Select(DataListUtil.AddBracketsToValueIfNotExist));
                }
            }
            return results;
        }

        #region Private Method

        private static bool IsDoubleBracketCanidate(string canidate)
        {
            bool result = false;
            char[] tokens = { ']' };

            string[] parts = canidate.Split(tokens);

            if(parts.Count() == 3)
            {
                result = true;
            }


            return result;
        }

        #endregion
    }
}
