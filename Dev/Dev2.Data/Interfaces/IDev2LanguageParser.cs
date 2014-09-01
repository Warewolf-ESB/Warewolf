using System.Collections.Generic;
using Dev2.Common.Interfaces.Data;

namespace Dev2.DataList.Contract
{
    public interface IDev2LanguageParser {

        IList<IDev2Definition> Parse(string OutputDefinition);

        IList<IDev2Definition> ParseAndAllowBlanks(string OutputDefinition);

    }
}
