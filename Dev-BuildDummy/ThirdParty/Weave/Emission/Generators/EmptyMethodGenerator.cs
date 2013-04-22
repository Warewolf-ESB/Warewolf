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
