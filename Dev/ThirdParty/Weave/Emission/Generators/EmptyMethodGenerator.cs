
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
using System.Collections.Generic;
using System.Emission.Emitters;
using System.Emission.Meta;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace System.Emission.Generators
{
    internal class EmptyMethodGenerator : MethodGenerator
    {
        public EmptyMethodGenerator(MetaMethod method, OverrideMethodDelegate overrideMethod)
            : base(method, overrideMethod)
        {
        }

        protected override MethodEmitter BuildProxiedMethodBody(MethodEmitter emitter, ClassEmitter @class, EmissionProxyOptions options, IDesignatingScope designatingScope, string dynamicAssemblyName)
        {
            ParameterInfo[] parameters = MethodToOverride.GetParameters();
            InitOutParameters(emitter, parameters);
            if (emitter.ReturnType == typeof(void))
                emitter.CodeBuilder.AddStatement(new ReturnStatement());
            else
                emitter.CodeBuilder.AddStatement(new ReturnStatement(new DefaultValueExpression(emitter.ReturnType)));

            return emitter;
        }

        private void InitOutParameters(MethodEmitter emitter, ParameterInfo[] parameters)
        {
            for (int index = 0; index < parameters.Length; index++)
            {
                ParameterInfo parameter = parameters[index];

                if (parameter.IsOut)
                {
                    emitter.CodeBuilder.AddStatement(
                        new AssignArgumentStatement(new ArgumentReference(parameter.ParameterType, index + 1),
                                                    new DefaultValueExpression(parameter.ParameterType)));
                }
            }
        }
    }
}
