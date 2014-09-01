using System;
using Dev2.Common;
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces.Scripting;

namespace Dev2.Development.Languages.Scripting
{
    public class ScriptingEngineRepo : SpookyAction<IScriptingContext,enScriptType>
    {

        public IScriptingContext CreateFindMissingStrategy(enScriptType typeOf)
        {
            return FindMatch(typeOf);
        }


        public IScriptingContext CreateEngine(enScriptType ScriptType)
        {
            switch(ScriptType)
            {
                case enScriptType.JavaScript :
                    return  new JavaScriptContext();
                case enScriptType.Python:
                    return new Dev2PythonContext();
                case enScriptType.Ruby:
                    return new RubyContext();
                default : throw new Exception("Invalid scripting context");


            }
        }
    }
}
