
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Collections.Generic;
using Dev2.Common.Interfaces.Data;
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
                
                if(IsOutput)
                {
                    if(IsDbService)
                    {
                        if(!string.IsNullOrEmpty(rsName))
                        {
                            scanRsName = rsName;
                        }
                    }
                }
                else
                {
                    scanRsName = DataListUtil.ExtractRecordsetNameFromValue(tmp.Value);
                }

                if(tmp.IsRecordSet)
                {
                    // is already present in the record set?
                    if(tmpCollections.ContainsKey(scanRsName))
                    {
                        tmpCollections[scanRsName].Add(tmp);
                    }
                    else
                    { // first time adding for this record set
                        IList<IDev2Definition> newList = new List<IDev2Definition>();
                        newList.Add(tmp);
                        tmpCollections.Add(scanRsName, newList);
                        tmpNames.Add(scanRsName);
                    }
                }
                // Handle scalars that are really recordsets ;)
                else if(!string.IsNullOrEmpty(scanRsName))
                {
                    // is already present in the record set?
                    if(tmpCollections.ContainsKey(scanRsName))
                    {
                        tmpCollections[scanRsName].Add(tmp); // ???
                    }
                    else
                    { // first time adding for this record set
                        IList<IDev2Definition> newList = new List<IDev2Definition>();
                        newList.Add(tmp);
                        tmpCollections.Add(scanRsName, newList);
                        tmpNames.Add(scanRsName);
                    }
                }
            }
            IList<IRecordSetDefinition> tmpDefs = new List<IRecordSetDefinition>();

            // ReSharper disable LoopCanBeConvertedToQuery
            foreach(string setName in tmpNames)
            {
                // ReSharper restore LoopCanBeConvertedToQuery
                IList<IDev2Definition> tmpOutput = tmpCollections[setName];
                tmpDefs.Add(new RecordSetDefinition(setName, tmpOutput));
            }

            IRecordSetCollection result = new RecordSetCollection(tmpDefs, tmpNames);

            return result;
        }

    }
}
