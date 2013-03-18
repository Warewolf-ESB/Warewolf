using System;
using System.Collections.Generic;

namespace Dev2.DataList.Contract.Binary_Objects.Structs
{
    [Serializable]
    public struct SBinaryDataList
    {
        public Guid UID { get; set; }
        public Guid ParentUID { get; set; }

        // Template dictionary
        public IDictionary<string, IBinaryDataListEntry> _templateDict;
        // Intellisesne parts to return 
        public IList<IDev2DataLanguageIntellisensePart> _intellisenseParts;
        // Catelog for intelisense namespaces
        public IList<string> _intellisensedNamespace;
    }
}
