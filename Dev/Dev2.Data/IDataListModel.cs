using System.Collections.Generic;

namespace Dev2.Data
{
    public interface IDataListModel
    {
        List<IScalar> Scalars { get; set; }
        List<IRecordSet> RecordSets { get; set; }
        List<IScalar> ShapeScalars { get; set; }
        List<IRecordSet> ShapeRecordSets { get; set; }
        List<IComplexObject> ShapeComplexObjects { get; set; }
        List<IComplexObject> ComplexObjects { get; set; }

        void Create(string data, string shape);
        void PopulateWithData(string data);
        void CreateShape(string shape);
    }
}