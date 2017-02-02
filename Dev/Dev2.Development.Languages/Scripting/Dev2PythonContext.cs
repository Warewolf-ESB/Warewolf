/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Linq;
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces.Scripting;
using IronPython.Hosting;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
// ReSharper disable NonLocalizedString

namespace Dev2.Development.Languages.Scripting
{
    public class Dev2PythonContext:IScriptingContext
    {
        private readonly IStringScriptSources _sources;

        /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
        public Dev2PythonContext(IStringScriptSources sources)
        {
            _sources = sources;
        }

        public string Execute(string scriptValue)
        {
            var pyEng = Python.CreateEngine();
            var scriptStatements = scriptValue.Split(new[] { '\r','\n' }, StringSplitOptions.RemoveEmptyEntries);
            var fixedScriptStatements = scriptStatements.Select(scriptStatement => "    " + scriptStatement).ToList();
            var fixedScript = String.Join(Environment.NewLine, fixedScriptStatements);
            string pyFunc =  @"def __result__(): "+Environment.NewLine + fixedScript;
            ScriptScope scope = pyEng.CreateScope();
            AddScriptToContext(pyEng, scope);
            ScriptSource source = pyEng.CreateScriptSourceFromString(pyFunc, SourceCodeKind.Statements);

            //create a scope to act as the context for the code
          
            //execute the source
            source.Execute(scope);

            //get a delegate to the python function
            var result = scope.GetVariable<Func<dynamic>>("__result__");

            var toReturn = result.Invoke();

            if(toReturn != null)
            {
                return toReturn.ToString();
            }

            return string.Empty;
        }

        private void AddScriptToContext(ScriptEngine pyEng, ScriptScope scope)
        {
            if(_sources?.GetFileScriptSources() != null)
                foreach(var fileScriptSource in _sources.GetFileScriptSources())
                    pyEng.Execute(fileScriptSource.GetReader().ReadToEnd(), scope);
        }

        public enScriptType HandlesType()
        {
            return enScriptType.Python;
        }

    }
}
