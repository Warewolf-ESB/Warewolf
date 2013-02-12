using Newtonsoft.Json;

namespace Dev2.Runtime.ServiceModel.Data
{
    public class Recordset
    {
        #region CTOR

        public Recordset()
        {
            Fields = new RecordsetFieldList();
            Records = new RecordsetRecordList();
        }

        #endregion

        #region Properties

        public string Name { get; set; }

        public bool HasErrors { get; set; }

        public string ErrorMessage { get; set; }

        public RecordsetFieldList Fields { get; private set; }

        public RecordsetRecordList Records { get; private set; }

        #endregion

        #region ToString

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        #endregion
    }
}
