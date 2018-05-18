/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using Dev2.Common.Interfaces.Data;
using Dev2.Data.Interfaces;
using Dev2.Data.Util;
using Dev2.DataList.Contract;

namespace Dev2.Data.Builders
{
    public class RecordSetCollectionBuilder
    {

        public IList<IDev2Definition> ParsedOutput {get; private set;}

        public bool IsOutput { get; set; }
        public bool IsDbService { get; set; }

        public void SetParsedOutput(IList<IDev2Definition> parsedOutput)
        {
            ParsedOutput = parsedOutput ?? new List<IDev2Definition>();
        }

        public IRecordSetCollection Generate()
        {
            IDictionary<string, IList<IDev2Definition>> tmpCollections = new Dictionary<string, IList<IDev2Definition>>();
            IList<string> tmpNames = new List<string>();

            foreach(IDev2Definition tmp in ParsedOutput)
            {
                var rsName = DataListUtil.ExtractRecordsetNameFromValue(tmp.Value);
                var scanRsName = tmp.RecordSetName;

                if (IsOutput)
                {
                    if (IsDbService && !string.IsNullOrEmpty(rsName))
                    {
                        scanRsName = rsName;
                    }

                }
                else
                {
                    scanRsName = DataListUtil.ExtractRecordsetNameFromValue(tmp.Value);
                }

                AddTmpRecordsetNames(tmpCollections, tmpNames, tmp, scanRsName);
            }
            IList<IRecordSetDefinition> tmpDefs = new List<IRecordSetDefinition>();

            
            foreach(string setName in tmpNames)
            {
                
                var tmpOutput = tmpCollections[setName];
                tmpDefs.Add(new RecordSetDefinition(setName, tmpOutput));
            }

            IRecordSetCollection result = new RecordSetCollection(tmpDefs, tmpNames);

            return result;
        }

        static void AddTmpRecordsetNames(IDictionary<string, IList<IDev2Definition>> tmpCollections, IList<string> tmpNames, IDev2Definition tmp, string scanRsName)
        {
            if (tmp.IsRecordSet)
            {
                if (tmpCollections.ContainsKey(scanRsName))
                {
                    tmpCollections[scanRsName].Add(tmp);
                }
                else
                {
                    IList<IDev2Definition> newList = new List<IDev2Definition>();
                    newList.Add(tmp);
                    tmpCollections.Add(scanRsName, newList);
                    tmpNames.Add(scanRsName);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(scanRsName))
                {
                    if (tmpCollections.ContainsKey(scanRsName))
                    {
                        tmpCollections[scanRsName].Add(tmp);
                    }
                    else
                    {
                        IList<IDev2Definition> newList = new List<IDev2Definition>();
                        newList.Add(tmp);
                        tmpCollections.Add(scanRsName, newList);
                        tmpNames.Add(scanRsName);
                    }
                }
            }
        }
    }
}
