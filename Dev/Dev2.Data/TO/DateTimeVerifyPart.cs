using Dev2.Data.Binary_Objects;
using Dev2.Data.Interfaces;

namespace Dev2.DataList.Contract
{
    public class DateTimeVerifyPart : IDataListVerifyPart
    {

        public string DisplayValue { get; private set; }

        public string Recordset { get; private set; }

        public string Field { get; private set; }

        public string Description { get; set; }

        public string RecordsetIndex { get; private set; }

        public enDev2ColumnArgumentDirection ColumnIODirection { get; private set; }

        public bool HasRecordsetIndex
        {

            get { return (RecordsetIndex != string.Empty); }

        }

        public bool IsScalar
        {

            get { return (Recordset.Length == 0); }
        }

        internal DateTimeVerifyPart(string displayValue, string description)
        {
            Description = description;
            DisplayValue = displayValue;
            Recordset = "";
            Field = "";
            RecordsetIndex = "";
            ColumnIODirection = enDev2ColumnArgumentDirection.None;
        }


    }
}
