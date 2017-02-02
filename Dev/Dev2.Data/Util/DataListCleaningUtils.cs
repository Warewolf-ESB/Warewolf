/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Data.Parsers;
using Dev2.Data.TO;
using Dev2.Data.Util;

// ReSharper disable CheckNamespace
namespace Dev2.DataList.Contract
// ReSharper restore CheckNamespace
{
    public class DataListCleaningUtils
    {

        // ReSharper disable InconsistentNaming

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

        #endregion
    }
}
