namespace Dev2.Data.Interfaces {
    public interface IDataListVerifyPart {

        string DisplayValue { get; }

        string Recordset { get; }

        string Field { get; }

        string Description { get; }

        bool IsScalar { get; }

        string RecordsetIndex { get; }

        bool HasRecordsetIndex { get; }
    }
}
