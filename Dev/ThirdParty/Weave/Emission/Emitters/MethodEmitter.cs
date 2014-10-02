
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Reflection.Emit;
using System.Diagnostics;

namespace System.Emission.Emitters
{
    [DebuggerDisplay("{_builder.Name}")]
    internal class MethodEmitter : IEmissionMemberEmitter
    {
        #region Instance Fields
        private MethodBuilder _builder;
        private GenericTypeParameterBuilder[] _genericTypeParams;
        private ArgumentReference[] _arguments;
        private MethodCodeBuilder _codeBuilder;
        #endregion

        #region Private Properties
        private bool ImplementedByRuntime { get { return (_builder.GetMethodImplementationFlags() & MethodImplAttributes.Runtime) != 0; } }
        #endregion

        #region Public Properties
        public ArgumentReference[] Arguments { get { return _arguments; } }
        public virtual MethodCodeBuilder CodeBuilder { get { if (_codeBuilder == null) _codeBuilder = new MethodCodeBuilder(_builder.GetILGenerator()); return _codeBuilder; } }
        public GenericTypeParameterBuilder[] GenericTypeParams { get { return _genericTypeParams; } }
        public MethodBuilder MethodBuilder { get { return _builder; } }
        public MemberInfo Member { get { return _builder; } }
        public Type ReturnType { get { return _builder.ReturnType; } }
        #endregion

        #region Constructors
        protected MethodEmitter(MethodBuilder builder)
        {
            _builder = builder;
        }

        internal MethodEmitter(AbstractTypeEmitter owner, String name, MethodAttributes attributes)
            : this(owner.TypeBuilder.DefineMethod(name, attributes))
        {
        }

        internal MethodEmitter(AbstractTypeEmitter owner, String name, MethodAttributes attributes, Type returnType, params Type[] argumentTypes)
            : this(owner, name, attributes)
        {
            SetParameters(argumentTypes);
            SetReturnType(returnType);
        }

        internal MethodEmitter(AbstractTypeEmitter owner, String name, MethodAttributes attributes, MethodInfo methodToUseAsATemplate)
            : this(owner, name, attributes)
        {
            Dictionary<string, GenericTypeParameterBuilder> name2GenericType = GenericUtil.GetGenericArgumentsMap(owner);
            Type returnType = GenericUtil.ExtractCorrectType(methodToUseAsATemplate.ReturnType, name2GenericType);
            ParameterInfo[] baseMethodParameters = methodToUseAsATemplate.GetParameters();
            Type[] parameters = GenericUtil.ExtractParametersTypes(baseMethodParameters, name2GenericType);

            _genericTypeParams = GenericUtil.CopyGenericArguments(methodToUseAsATemplate, _builder, name2GenericType);

            SetParameters(parameters);
            SetReturnType(returnType);
            SetSignature(returnType, methodToUseAsATemplate.ReturnParameter, parameters, baseMethodParameters);
            DefineParameters(baseMethodParameters);
        }
        #endregion

        #region [Get/Set] Handling
        public void SetParameters(Type[] paramTypes)
        {
            _builder.SetParameters(paramTypes);
            _arguments = ArgumentsUtil.ConvertToArgumentReference(paramTypes);
            ArgumentsUtil.InitializeArgumentsByPosition(_arguments, MethodBuilder.IsStatic);
        }

        private void SetReturnType(Type returnType)
        {
            _builder.SetReturnType(returnType);
        }

        private void SetSignature(Type returnType, ParameterInfo returnParameter, Type[] parameters, ParameterInfo[] baseMethodParameters)
        {
            _builder.SetSignature(returnType, returnParameter.GetRequiredCustomModifiers(), returnParameter.GetOptionalCustomModifiers(), parameters, baseMethodParameters.Select(x => x.GetRequiredCustomModifiers()).ToArray(), baseMethodParameters.Select(x => x.GetOptionalCustomModifiers()).ToArray());
        }
        #endregion

        #region Define(...)
        public void DefineCustomAttribute(CustomAttributeBuilder attribute)
        {
            _builder.SetCustomAttribute(attribute);
        }

        private void DefineParameters(ParameterInfo[] parameters)
        {
            foreach (ParameterInfo parameter in parameters)
            {
                ParameterBuilder parameterBuilder = _builder.DefineParameter(parameter.Position + 1, parameter.Attributes, parameter.Name);

                foreach (CustomAttributeBuilder attribute in parameter.GetNonInheritableAttributes())
                    parameterBuilder.SetCustomAttribute(attribute);
            }
        }
        #endregion

        #region Generation Handling
        public virtual void Generate()
        {
            if (ImplementedByRuntime) return;
            _codeBuilder.Generate(this, _builder.GetILGenerator());
        }

        public virtual void EnsureValidCodeBlock()
        {
            if (ImplementedByRuntime == false && CodeBuilder.IsEmpty)
            {
                CodeBuilder.AddStatement(new NopStatement());
                CodeBuilder.AddStatement(new ReturnStatement());
            }
        }
        #endregion




    }
}
