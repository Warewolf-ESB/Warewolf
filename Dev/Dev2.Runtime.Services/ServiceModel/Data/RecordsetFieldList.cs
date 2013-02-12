using System.Collections.Generic;
using Newtonsoft.Json;

namespace Dev2.Runtime.ServiceModel.Data
{
    public class RecordsetFieldList : List<RecordsetField>
    {
        public void Add(string name)
        {
            Add(new RecordsetField { Name = name, Alias = name });
        }

        #region ToString

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        #endregion
    }
}