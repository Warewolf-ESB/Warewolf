/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Data.Interfaces;
using Dev2.Data.Parsers;
using Dev2.Data.Util;


namespace Dev2.DataList.Contract
{
    public static class DataListCleaningUtils
    {
        public static List<string> SplitIntoRegions(string result)
        {
            if(!String.IsNullOrEmpty(result))
            {
                try
                {
                    var allRegions = new List<string>();
                    var parser = new Dev2DataLanguageParser();
                    var makeParts = parser.MakeParts(result);
                    foreach (var makePart in makeParts.Where(c => !c.HangingOpen))
                    {
                        if(makePart.Child != null)
                        {
                            var indexOfBracket = makePart.Payload.IndexOf("(", StringComparison.Ordinal);
                            var tmpresult = makePart.Payload.Insert(indexOfBracket + 1, DataListUtil.AddBracketsToValueIfNotExist(makePart.Child.Payload));
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

        static IEnumerable<string> AddChildrenPart(IParseTO child)
        {
            var results = new List<string>();
            if (child != null)
            {
                results.Add(DataListUtil.AddBracketsToValueIfNotExist(child.Payload));
                if (child.Child != null)
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
                    var parser = new Dev2DataLanguageParser();
                    var makeParts = parser.MakeParts(result);
                    foreach (var makePart in makeParts.Where(c => !c.HangingOpen))
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

        static IEnumerable<string> AddChildrenPartForFindMissing(IParseTO child)
        {
            var results = new List<string>();
            if (child != null)
            {
                results.Add(DataListUtil.AddBracketsToValueIfNotExist(child.Payload));
                if (child.Child != null)
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
