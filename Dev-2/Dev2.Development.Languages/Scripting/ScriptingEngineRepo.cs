using Dev2.Common;
using Dev2.Common.Enums;

namespace Dev2.Development.Languages.Scripting
{
    public class ScriptingEngineRepo : SpookyAction<IScriptingContext,enScriptType>
    {

        public IScriptingContext CreateFindMissingStrategy(enScriptType typeOf)
        {
            return FindMatch(typeOf);
        }

    }
}
