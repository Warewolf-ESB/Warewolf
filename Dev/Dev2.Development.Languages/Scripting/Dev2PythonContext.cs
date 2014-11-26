
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces.Scripting;
using IronPython.Hosting;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;

namespace Dev2.Development.Languages.Scripting
{
    public class Dev2PythonContext:IScriptingContext
    {
        public string Execute(string scriptValue)
        {
            var pyEng = Python.CreateEngine();
            
            string pyFunc =  @"def __result__(): " + scriptValue;     

            ScriptSource source = pyEng.CreateScriptSourceFromString(pyFunc, SourceCodeKind.Statements);

            //create a scope to act as the context for the code
            ScriptScope scope = pyEng.CreateScope();

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

        public enScriptType HandlesType()
        {
            return enScriptType.Python;
        }

    }
}
