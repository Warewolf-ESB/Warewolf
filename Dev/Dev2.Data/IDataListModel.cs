using System.Collections.Generic;

namespace Dev2.Data
{
    public interface IDataListModel
    {
        List<IScalar> Scalars { get; set; }
        List<IRecordSet> RecordSets { get; set; }

        void Create(string data, string shape);
        void PopulateWithData(string data);
        void CreateShape(string shape);
    }
}