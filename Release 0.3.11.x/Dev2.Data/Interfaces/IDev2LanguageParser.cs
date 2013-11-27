using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.DataList.Contract
{
    public interface IDev2LanguageParser {

        IList<IDev2Definition> Parse(string OutputDefinition);

        IList<IDev2Definition> ParseAndAllowBlanks(string OutputDefinition);

    }
}
