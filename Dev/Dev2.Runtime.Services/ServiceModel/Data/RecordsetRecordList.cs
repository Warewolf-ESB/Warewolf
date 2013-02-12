using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Dev2.Runtime.ServiceModel.Data
{
    public class RecordsetRecordList : List<RecordsetRecord>
    {
        public void Add(Recordset recordset, Func<int, string> getFieldValue)
        {
            if(recordset == null || getFieldValue == null)
            {
                return;
            }

            var rowIndex = Count;
            var record = new RecordsetRecord();
            record.AddRange(recordset.Fields.Select((fieldName, i) => new RecordsetCell
            {
                Name = "[[" + recordset.Name + "(" + (rowIndex + 1) + ")." + fieldName + "]]",
                Value = getFieldValue(i)
            }));
            Add(record);
        }

        #region ToString

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        #endregion
    }
}