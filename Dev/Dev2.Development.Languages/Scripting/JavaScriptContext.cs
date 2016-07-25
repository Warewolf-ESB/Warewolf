/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
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
        private ScriptEngine jsContext;

        public JavaScriptContext()
        {
            jsContext = new ScriptEngine();
            AddScriptSourcesToContext();
        }

        public string Execute(string scriptValue)
        {
            jsContext.Evaluate("function __result__() {" + scriptValue + "}");
            return jsContext.CallGlobalFunction("__result__").ToString();
        }

        public IList<FileScriptSource> ScriptSources()
        {
            return StringScriptSources.GetFileScriptSources();
        }
        public enScriptType HandlesType()
        {
            return enScriptType.JavaScript;
        }

        public void AddScriptSourcesToContext()
        {
            if (jsContext == null) return;
            foreach (var scriptSource in ScriptSources())
                jsContext.Evaluate(scriptSource);
        }
    }
}
