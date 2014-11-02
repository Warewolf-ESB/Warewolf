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
using Dev2.Common;
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces.Scripting;

namespace Dev2.Development.Languages.Scripting
{
    public class ScriptingEngineRepo : SpookyAction<IScriptingContext, enScriptType>
    {
        public IScriptingContext CreateFindMissingStrategy(enScriptType typeOf)
        {
            return FindMatch(typeOf);
        }


        public IScriptingContext CreateEngine(enScriptType ScriptType)
        {
            switch (ScriptType)
            {
                case enScriptType.JavaScript:
                    return new JavaScriptContext();
                case enScriptType.Python:
                    return new Dev2PythonContext();
                case enScriptType.Ruby:
                    return new RubyContext();
                default:
                    throw new Exception("Invalid scripting context");
            }
        }
    }
}