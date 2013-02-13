using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Dev2.Runtime.ServiceModel.Data
{
    public class Recordset
    {
        #region CTOR

        public Recordset()
        {
            Fields = new List<RecordsetField>();
            Records = new List<RecordsetRecord>();
        }

        #endregion

        #region Properties

        public string Name { get; set; }

        public bool HasErrors { get; set; }

        public string ErrorMessage { get; set; }

        public List<RecordsetField> Fields { get; private set; }

        public List<RecordsetRecord> Records { get; private set; }

        #endregion

        #region AddRecord

        public RecordsetRecord AddRecord(Func<int, string> getFieldValue)
        {
            if(getFieldValue == null)
            {
                throw new ArgumentNullException("getFieldValue");
            }

            var record = new RecordsetRecord { Label = Name + "(" + (Records.Count + 1) + ")" };
            record.AddRange(Fields.Select((field, fieldIndex) => new RecordsetCell
            {
                Name = record.Label + "." + field.Alias,
                Value = getFieldValue(fieldIndex)
            }));

            Records.Add(record);
            return record;
        }

        #endregion

        #region ToString

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        #endregion

    }
}
