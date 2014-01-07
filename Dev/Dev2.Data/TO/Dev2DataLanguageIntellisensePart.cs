using System;
using System.Collections.Generic;

namespace Dev2.DataList.Contract
{
    [Serializable]
    public class Dev2DataLanguageIntellisensePart : IDev2DataLanguageIntellisensePart
    {

        public string Name { get; private set; }

        public string Description { get; private set; }

        public IList<IDev2DataLanguageIntellisensePart> Children { get; private set; }

        public Dev2DataLanguageIntellisensePart(string name, string desc, IList<IDev2DataLanguageIntellisensePart> children)
        {
            Name = name;
            Children = children;
            Description = desc;
        }

    }
}
