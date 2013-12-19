using System.Collections.Generic;
using Dev2.Data.Util;

namespace Dev2.DataList.Contract
{
    public class RecordSetCollectionBuilder {

        private IList<IDev2Definition> _parsedOutput;

        public bool IsOutput { get; set; }

        public void setParsedOutput(IList<IDev2Definition> parsedOutput) {
            _parsedOutput = parsedOutput;
        }

        public IRecordSetCollection Generate() {
            IRecordSetCollection result;

            IDictionary<string, IList<IDev2Definition>> tmpCollections = new Dictionary<string, IList<IDev2Definition>>();
            IList<string> tmpNames = new List<string>();

            for (int i = 0; i < _parsedOutput.Count; i++) {
                IDev2Definition tmp = _parsedOutput[i];
                var rsName = DataListUtil.ExtractRecordsetNameFromValue(tmp.Value); // last .Name
                var scanRsName = tmp.RecordSetName;

                if (IsOutput)
                {
                    if(!string.IsNullOrEmpty(rsName))
                    {
                        scanRsName = rsName;
                    }else
                    {
                        rsName = scanRsName;
                    }
                }
                else
                {
                    scanRsName = DataListUtil.ExtractRecordsetNameFromValue(tmp.Value);
                }

                if (tmp.IsRecordSet) {
                    // is already present in the record set?
                    if (tmpCollections.ContainsKey(scanRsName)) {
                        tmpCollections[scanRsName].Add(tmp);
                    }
                    else { // first time adding for this record set
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

            foreach (string setName in tmpNames) {
                IList<IDev2Definition> tmpOutput = tmpCollections[setName];
                tmpDefs.Add(new RecordSetDefinition(setName, tmpOutput));
            }

            result = new RecordSetCollection(tmpDefs, tmpNames);

            return result;
        }

    }
}
