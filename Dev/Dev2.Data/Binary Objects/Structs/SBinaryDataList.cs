using System;
using System.Collections.Generic;

namespace Dev2.DataList.Contract.Binary_Objects.Structs
{
    [Serializable]
    // ReSharper disable InconsistentNaming
    public struct SBinaryDataList
    {
        public Guid UID { get; set; }
        public Guid ParentUID { get; set; }

        // Template dictionary
        public IDictionary<string, IBinaryDataListEntry> _templateDict;
        // Intellisesne parts to return 
        public IList<IDev2DataLanguageIntellisensePart> _intellisenseParts;
        // Catalog for intellisense namespaces
        public IList<string> _intellisensedNamespace;
    }
}
