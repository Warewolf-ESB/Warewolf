using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.DataList.Contract
{
    public interface IDev2DataLanguageIntellisensePart {

        string Name { get; }

        string Description { get; }

        IList<IDev2DataLanguageIntellisensePart> Children { get; }

    }
}
