using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Warewolf.Storage
{
    internal interface IIndexMapBuilder
    {
        void BuildIndexMap(LanguageAST.JsonIdentifierExpression var, string exp, List<string> indexMap,
            JContainer container);
    }
}