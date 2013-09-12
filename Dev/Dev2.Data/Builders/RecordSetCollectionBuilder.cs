using System.Collections.Generic;

namespace Dev2.DataList.Contract
{
    public class RecordSetCollectionBuilder {

        private IList<IDev2Definition> _parsedOutput;

        public void setParsedOutput(IList<IDev2Definition> parsedOutput) {
            _parsedOutput = parsedOutput;
        }

        public IRecordSetCollection Generate() {
            IRecordSetCollection result;

            IDictionary<string, IList<IDev2Definition>> _tmpCollections = new Dictionary<string, IList<IDev2Definition>>();
            IList<string> _tmpNames = new List<string>();

            for (int i = 0; i < _parsedOutput.Count; i++) {
                IDev2Definition tmp = _parsedOutput[i];
                var rsName = DataListUtil.ExtractRecordsetNameFromValue(tmp.Value);

                if (tmp.IsRecordSet) {
                    // is already present in the record set?
                    if (_tmpCollections.ContainsKey(tmp.RecordSetName)) {
                        _tmpCollections[tmp.RecordSetName].Add(tmp);
                    }
                    else { // first time adding for this record set
                        IList<IDev2Definition> newList = new List<IDev2Definition>();
                        newList.Add(tmp);
                        _tmpCollections.Add(tmp.RecordSetName, newList);
                        _tmpNames.Add(tmp.RecordSetName);
                    }
                }
                // Handle scalars that are really recordsets ;)
                else if(!string.IsNullOrEmpty(rsName))
                {
                    // is already present in the record set?
                    if (_tmpCollections.ContainsKey(rsName))
                    {
                        _tmpCollections[rsName].Add(tmp); // ???
                    }
                    else
                    { // first time adding for this record set
                        IList<IDev2Definition> newList = new List<IDev2Definition>();
                        newList.Add(tmp);
                        _tmpCollections.Add(rsName, newList);
                        _tmpNames.Add(rsName);
                    }
                }
            }
            IList<IRecordSetDefinition> tmpDefs = new List<IRecordSetDefinition>();

            foreach (string setName in _tmpNames) {
                IList<IDev2Definition> tmpOutput = _tmpCollections[setName];
                tmpDefs.Add(new RecordSetDefinition(setName, tmpOutput));
            }

            result = new RecordSetCollection(tmpDefs, _tmpNames);

            return result;
        }

    }
}
