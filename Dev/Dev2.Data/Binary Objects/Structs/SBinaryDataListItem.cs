using System;

namespace Dev2.DataList.Contract.Binary_Objects.Structs
{
    [Serializable]
    public struct SBinaryDataListItem
    {

        public string TheValue { get; set;  }

        public int ItemCollectionIndex { get; set; }

        public string Namespace { get; set; }

        public string FieldName { get; set; }

        public string DisplayValue { get; set; }

    }
}
