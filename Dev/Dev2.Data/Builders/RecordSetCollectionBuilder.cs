using System.Collections.Generic;
using Dev2.Data.Util;

namespace Dev2.DataList.Contract
{
    public class RecordSetCollectionBuilder
    {

        private IList<IDev2Definition> _parsedOutput;

        public bool IsOutput { get; set; }

        public void setParsedOutput(IList<IDev2Definition> parsedOutput)
        {
            _parsedOutput = parsedOutput;
        }

        public IRecordSetCollection Generate()
        {
            IDictionary<string, IList<IDev2Definition>> tmpCollections = new Dictionary<string, IList<IDev2Definition>>();
            IList<string> tmpNames = new List<string>();

            foreach(IDev2Definition tmp in _parsedOutput)
            {
                var rsName = DataListUtil.ExtractRecordsetNameFromValue(tmp.Value); // last .Name
                var scanRsName = tmp.RecordSetName;

                if(IsOutput)
                {
                    if(!string.IsNullOrEmpty(rsName))
                    {
                        scanRsName = rsName;
                    }
                    else
                    {
                        // ReSharper disable RedundantAssignment
                        rsName = scanRsName;
                        // ReSharper restore RedundantAssignment
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
