namespace Dev2.DataList.Contract
{
    public interface IDev2Definition {

        #region Properties
        string Name { get; }

        string MapsTo { get; }

        string Value { get; }

        bool IsRecordSet { get; }

        string RecordSetName { get; }

        bool IsEvaluated { get; }

        string DefaultValue { get; }

        bool IsRequired { get; }

        string RawValue { get; }

        bool EmptyToNull { get; }

        #endregion
    }
}

