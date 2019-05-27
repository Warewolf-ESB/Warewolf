#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces.Scripting;
using Jurassic;

namespace Dev2.Development.Languages.Scripting
{
    public class JavaScriptContext : IScriptingContext
    {
        readonly IStringScriptSources _scriptSources;
        readonly ScriptEngine _jsContext;

        public JavaScriptContext(IStringScriptSources sources)
        {
            _jsContext = new ScriptEngine();
            _scriptSources = sources;
            AddScriptSourcesToContext();
        }

        public string Execute(string scriptValue)
        {
            _jsContext.Evaluate("function __result__() {" + scriptValue + "}");
            return _jsContext.CallGlobalFunction("__result__").ToString();
        }

        public IList<FileScriptSource> ScriptSources() => _scriptSources.GetFileScriptSources();

        public enScriptType HandlesType() => enScriptType.JavaScript;

        public void AddScriptSourcesToContext()
        {
            if (_jsContext == null)
            {
                return;
            }

            foreach (var scriptSource in ScriptSources())
            {
                _jsContext.Evaluate(scriptSource);
            }
        }
    }
}
