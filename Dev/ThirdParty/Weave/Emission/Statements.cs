
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
    internal abstract class Statement : IILEmitter
    {
        public abstract void Emit(IEmissionMemberEmitter member, ILGenerator gen);
    }

    internal class AssignStatement : Statement
    {
        private Expression _expression;
        private Reference _target;

        public AssignStatement(Reference target, Expression expression)
        {
            _target = target;
            _expression = expression;
        }

        public override void Emit(IEmissionMemberEmitter member, ILGenerator gen)
        {
            ArgumentsUtil.EmitLoadOwnerAndReference(_target.OwnerReference, gen);
            _expression.Emit(member, gen);
            _target.StoreReference(gen);
        }
    }

    internal class AssignArgumentStatement : Statement
    {
        private ArgumentReference _argument;
        private Expression _expression;

        public AssignArgumentStatement(ArgumentReference argument, Expression expression)
        {
            _argument = argument;
            _expression = expression;
        }

        public override void Emit(IEmissionMemberEmitter member, ILGenerator gen)
        {
            ArgumentsUtil.EmitLoadOwnerAndReference(_argument, gen);
            _expression.Emit(member, gen);
        }
    }

    internal class ReturnStatement : Statement
    {
        private Expression _expression;
        private Reference _reference;

        public ReturnStatement()
        {
        }

        public ReturnStatement(Reference reference)
        {
            _reference = reference;
        }

        public ReturnStatement(Expression expression)
        {
            _expression = expression;
        }

        public override void Emit(IEmissionMemberEmitter member, ILGenerator gen)
        {
            if (_reference != null) ArgumentsUtil.EmitLoadOwnerAndReference(_reference, gen);
            else if (_expression != null) _expression.Emit(member, gen);
            else if (member.ReturnType != typeof(void)) OpCodeUtil.EmitLoadOpCodeForDefaultValueOfType(gen, member.ReturnType);
            gen.Emit(OpCodes.Ret);
        }
    }

    internal class NopStatement : Statement
    {
        public override void Emit(IEmissionMemberEmitter member, ILGenerator gen)
        {
            gen.Emit(OpCodes.Nop);
        }
    }

    internal class ExpressionStatement : Statement
    {
        private Expression _expression;

        public ExpressionStatement(Expression expression)
        {
            _expression = expression;
        }

        public override void Emit(IEmissionMemberEmitter member, ILGenerator gen)
        {
            _expression.Emit(member, gen);
        }
    }

    internal class ConstructorInvocationStatement : Statement
    {
        private Expression[] _args;
        private ConstructorInfo _constructor;

        public ConstructorInvocationStatement(ConstructorInfo method, params Expression[] args)
        {
            if (method == null) throw new ArgumentNullException("method");
            if (args == null) throw new ArgumentNullException("args");

            _constructor = method;
            _args = args;
        }

        public override void Emit(IEmissionMemberEmitter member, ILGenerator gen)
        {
            gen.Emit(OpCodes.Ldarg_0);
            foreach (Expression exp in _args) exp.Emit(member, gen);
            gen.Emit(OpCodes.Call, _constructor);
        }
    }
}
