
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
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace System.Emission.Emitters
{
    internal class ConstructorEmitter : IEmissionMemberEmitter
    {
        #region Instance Fields
        private ConstructorBuilder _builder;
        private AbstractTypeEmitter _mainType;
        private ConstructorCodeBuilder _codeBuilder;
        #endregion

        #region Private Properties
        private bool ImplementedByRuntime { get { return (_builder.GetMethodImplementationFlags() & MethodImplAttributes.Runtime) != 0; } }
        #endregion

        #region Public Properties
        public virtual ConstructorCodeBuilder CodeBuilder { get { if (_codeBuilder == null) _codeBuilder = new ConstructorCodeBuilder(_mainType.BaseType, _builder.GetILGenerator()); return _codeBuilder; } }
        public ConstructorBuilder ConstructorBuilder { get { return _builder; } }
        public MemberInfo Member { get { return _builder; } }
        public Type ReturnType { get { return typeof(void); } }
        #endregion

        #region Constructors
        protected ConstructorEmitter(AbstractTypeEmitter maintype, ConstructorBuilder builder)
        {
            _mainType = maintype;
            _builder = builder;
        }

        internal ConstructorEmitter(AbstractTypeEmitter maintype, params ArgumentReference[] arguments)
        {
            _mainType = maintype;
            Type[] args = ArgumentsUtil.InitializeAndConvert(arguments);
            _builder = maintype.TypeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, args);
        }
        #endregion

        #region Generation Handling
        public virtual void Generate()
        {
            if (ImplementedByRuntime) return;
            CodeBuilder.Generate(this, _builder.GetILGenerator());
        }

        public virtual void EnsureValidCodeBlock()
        {
            if (!ImplementedByRuntime && CodeBuilder.IsEmpty)
            {
                CodeBuilder.InvokeBaseConstructor();
                CodeBuilder.AddStatement(new ReturnStatement());
            }
        }
        #endregion
    }

    internal sealed class TypeConstructorEmitter : ConstructorEmitter
    {
        internal TypeConstructorEmitter(AbstractTypeEmitter maintype)
            : base(maintype, maintype.TypeBuilder.DefineTypeInitializer())
        {
        }

        public override void EnsureValidCodeBlock()
        {
            if (CodeBuilder.IsEmpty) CodeBuilder.AddStatement(new ReturnStatement());
        }
    }
}
