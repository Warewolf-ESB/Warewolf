using System;
using System.Collections.Generic;
using Dev2.Common;
using Dev2.Common.Interfaces.Data;
using Dev2.DataList.Contract;

namespace Dev2.Data.Builders
{
    public class RecordSetDefintionMutator {

        private IRecordSetDefinition _def;

        public void SetDefinition(IRecordSetDefinition def) {
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
                    foreach(IDev2Definition t in cols)
                    {
                        string colName = t.Value.Split('.')[1]; // extract DL column name
                        colName = colName.Replace("]", "");
                        newCols.Add(DataListFactory.CreateDefinition(t.Name, t.MapsTo, colName, true, String.Empty, false, colName));
                    }
                    result = new RecordSetDefinition(setName, newCols);
                }
                catch(Exception ex)
                {
                    Dev2Logger.Log.Error(ex);
                }
            }

            return result;
        }
    }
}
