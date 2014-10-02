
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
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace System.Emission
{
    internal abstract class AbstractCodeBuilder
    {
        #region Instance Fields
        private ILGenerator _generator;
        private List<Reference> _ilMarkers;
        private List<Statement> _statements;
        private bool _isEmpty;
        #endregion

        #region Internal Properties
        internal bool IsEmpty { get { return _isEmpty; } }
        #endregion

        #region Public Properties
        public ILGenerator Generator { get { return _generator; } }
        #endregion

        #region Constructor
        protected AbstractCodeBuilder(ILGenerator generator)
        {
            _generator = generator;
            _statements = new List<Statement>();
            _ilMarkers = new List<Reference>();
            _isEmpty = true;
        }
        #endregion

        #region [Get/Set] Handling
        public void SetNonEmpty()
        {
            _isEmpty = false;
        }
        #endregion

        #region Add(...)
        public AbstractCodeBuilder AddExpression(Expression expression)
        {
            return AddStatement(new ExpressionStatement(expression));
        }

        public AbstractCodeBuilder AddStatement(Statement stmt)
        {
            SetNonEmpty();
            _statements.Add(stmt);
            return this;
        }

        public LocalReference DeclareLocal(Type type)
        {
            LocalReference local = new LocalReference(type);
            _ilMarkers.Add(local);
            return local;
        }
        #endregion

        #region Generation Handling
        internal void Generate(IEmissionMemberEmitter member, ILGenerator il)
        {
            foreach (Reference local in _ilMarkers) local.Generate(il);
            foreach (Statement statement in _statements) statement.Emit(member, il);
        }
        #endregion
    }

    #region ConstructorCodeBuilder
    internal sealed class ConstructorCodeBuilder : AbstractCodeBuilder
    {
        private Type _baseType;

        public ConstructorCodeBuilder(Type baseType, ILGenerator generator)
            : base(generator)
        {
            _baseType = baseType;
        }

        public void InvokeBaseConstructor()
        {
            Type type = _baseType;
            if (type.ContainsGenericParameters) type = type.GetGenericTypeDefinition();

            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            ConstructorInfo baseDefaultCtor = type.GetConstructor(flags, null, new Type[0], null);
            InvokeBaseConstructor(baseDefaultCtor);
        }

        public void InvokeBaseConstructor(ConstructorInfo constructor)
        {
            AddStatement(new ConstructorInvocationStatement(constructor));
        }

        public void InvokeBaseConstructor(ConstructorInfo constructor, params ArgumentReference[] arguments)
        {
            AddStatement(new ConstructorInvocationStatement(constructor, ArgumentsUtil.ConvertArgumentReferenceToExpression(arguments)));
        }
    }
    #endregion

    #region MethodCodeBuilder
    internal sealed class MethodCodeBuilder : AbstractCodeBuilder
    {
        public MethodCodeBuilder(ILGenerator generator)
            : base(generator)
        {
        }
    }
    #endregion
}
