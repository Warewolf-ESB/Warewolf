using System;
using System.Collections.Generic;
using Dev2.Common;
using Dev2.Common.Interfaces.Data;

namespace Dev2.DataList.Contract
{
    public class RecordSetDefintionMutator {

        private IRecordSetDefinition _def;

        public void setDefinition(IRecordSetDefinition def) {
            _def = def;
        }

        public IRecordSetDefinition Generate() {
            IRecordSetDefinition result = null;

            IList<IDev2Definition> cols = _def.Columns;
            IList<IDev2Definition> newCols = new List<IDev2Definition>();

            if (cols.Count > 0) {
                string val = cols[0].Value;
                try
                {
                    string setName = val.Split('.')[0];
                    setName = setName.Replace("[", "");
                    for(int i = 0; i < cols.Count; i++)
                    {
                        string colName = cols[i].Value.Split('.')[1]; // extract DL column name
                        colName = colName.Replace("]", "");
                        newCols.Add(DataListFactory.CreateDefinition(cols[i].Name, cols[i].MapsTo, colName, true, String.Empty, false, colName));
                    }
                    result = new RecordSetDefinition(setName, newCols);
                }
                catch(Exception ex)
                {
                    this.LogError(ex);
                }
            }

            return result;
        }
    }
}
