
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
using System.Diagnostics;
using System.Emission.Emitters;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace System.Emission
{
    #region Expression
    internal abstract class Expression : IILEmitter
    {
        public abstract void Emit(IEmissionMemberEmitter member, ILGenerator gen);
    }
    #endregion

    internal sealed class ConvertExpression : Expression
    {
        private readonly Expression right;
        private Type fromType;
        private Type target;

        public ConvertExpression(Type targetType, Expression right)
            : this(targetType, typeof(object), right)
        {
        }

        public ConvertExpression(Type targetType, Type fromType, Expression right)
        {
            target = targetType;
            this.fromType = fromType;
            this.right = right;
        }

        public override void Emit(IEmissionMemberEmitter member, ILGenerator gen)
        {
            right.Emit(member, gen);

            if (fromType == target)
            {
                return;
            }

            if (fromType.IsByRef)
            {
                fromType = fromType.GetElementType();
            }

            if (target.IsByRef)
            {
                target = target.GetElementType();
            }

            if (target.IsValueType)
            {
                if (fromType.IsValueType)
                {
                    throw new NotImplementedException("Cannot convert between distinct value types");
                }
                else
                {
                    // Unbox conversion
                    // Assumes fromType is a boxed value
                    // if we can, we emit a box and ldind, otherwise, we will use unbox.any
                    if (LdindOpCodesDictionary.Instance[target] != LdindOpCodesDictionary.EmptyOpCode)
                    {
                        gen.Emit(OpCodes.Unbox, target);
                        OpCodeUtil.EmitLoadIndirectOpCodeForType(gen, target);
                    }
                    else
                    {
                        gen.Emit(OpCodes.Unbox_Any, target);
                    }
                }
            }
            else
            {
                if (fromType.IsValueType)
                {
                    // Box conversion
                    gen.Emit(OpCodes.Box, fromType);
                    EmitCastIfNeeded(typeof(object), target, gen);
                }
                else
                {
                    // Possible down-cast
                    EmitCastIfNeeded(fromType, target, gen);
                }
            }
        }

        private static void EmitCastIfNeeded(Type from, Type target, ILGenerator gen)
        {
            if (target.IsGenericParameter)
            {
                gen.Emit(OpCodes.Unbox_Any, target);
            }
            else if (from.IsGenericParameter)
            {
                gen.Emit(OpCodes.Box, from);
            }
            else if (target.IsGenericType && target != from)
            {
                gen.Emit(OpCodes.Castclass, target);
            }
            else if (target.IsSubclassOf(from))
            {
                gen.Emit(OpCodes.Castclass, target);
            }
        }
    }

    internal sealed class NewInstanceExpression : Expression
    {
        private Expression[] _arguments;
        private Type[] _constructorArgs;
        private Type _type;
        private ConstructorInfo _constructor;

        public NewInstanceExpression(ConstructorInfo constructor, params Expression[] args)
        {
            this._constructor = constructor;
            _arguments = args;
        }

        public NewInstanceExpression(Type target, Type[] constructor_args, params Expression[] args)
        {
            _type = target;
            _constructorArgs = constructor_args;
            _arguments = args;
        }

        public override void Emit(IEmissionMemberEmitter member, ILGenerator gen)
        {
            foreach (Expression exp in _arguments) exp.Emit(member, gen);

            if (_constructor == null) _constructor = _type.GetConstructor(_constructorArgs);
            if (_constructor == null) throw new InvalidOperationException("Could not find constructor matching specified arguments");
            gen.Emit(OpCodes.Newobj, _constructor);
        }
    }

    internal class TypeTokenExpression : Expression
    {
        public static readonly MethodInfo GetTypeFromHandle = typeof(Type).GetMethod("GetTypeFromHandle");

        private readonly Type type;

        public TypeTokenExpression(Type type)
        {
            this.type = type;
        }

        public override void Emit(IEmissionMemberEmitter member, ILGenerator gen)
        {
            gen.Emit(OpCodes.Ldtoken, type);
            gen.Emit(OpCodes.Call, GetTypeFromHandle);
        }
    }

    internal sealed class MethodInvocationExpression : Expression
    {
        #region Instance Fields
        private Expression[] _args;
        private MethodInfo _method;
        private Reference _owner;
        private bool _virtualCall;
        #endregion

        #region Public Properties
        public bool VirtualCall { get { return _virtualCall; } set { _virtualCall = value; } }
        #endregion

        #region Constructors
        public MethodInvocationExpression(MethodInfo method, params Expression[] args) :
            this(SelfReference.Self, method, args)
        {
        }

        public MethodInvocationExpression(MethodEmitter method, params Expression[] args) :
            this(SelfReference.Self, method.MethodBuilder, args)
        {
        }

        public MethodInvocationExpression(Reference owner, MethodEmitter method, params Expression[] args) :
            this(owner, method.MethodBuilder, args)
        {
        }

        public MethodInvocationExpression(Reference owner, MethodInfo method, params Expression[] args)
        {
            if (method == null) Debugger.Break();
            _owner = owner;
            _method = method;
            _args = args;
        }
        #endregion

        #region Emission Handling
        public override void Emit(IEmissionMemberEmitter member, ILGenerator gen)
        {
            ArgumentsUtil.EmitLoadOwnerAndReference(_owner, gen);

            foreach (Expression exp in _args) exp.Emit(member, gen);
            if (_virtualCall) gen.Emit(OpCodes.Callvirt, _method);
            else gen.Emit(OpCodes.Call, _method);
        }
        #endregion
    }

    internal sealed class DefaultValueExpression : Expression
    {
        #region Instance Fields
        private Type _type;
        #endregion

        #region Constructor
        public DefaultValueExpression(Type type)
        {
            _type = type;
        }
        #endregion

        #region Emission Handling
        public override void Emit(IEmissionMemberEmitter member, ILGenerator gen)
        {
            if (IsPrimitiveOrClass(_type)) OpCodeUtil.EmitLoadOpCodeForDefaultValueOfType(gen, _type);
            else if (_type.IsValueType || _type.IsGenericParameter)
            {
                LocalBuilder local = gen.DeclareLocal(_type);
                gen.Emit(OpCodes.Ldloca_S, local);
                gen.Emit(OpCodes.Initobj, _type);
                gen.Emit(OpCodes.Ldloc, local);
            }
            else if (_type.IsByRef) EmitByRef(gen);
            else throw new InvalidOperationException("Can't emit default value for type " + _type);
        }

        private void EmitByRef(ILGenerator gen)
        {
            Type elementType = _type.GetElementType();

            if (IsPrimitiveOrClass(elementType))
            {
                OpCodeUtil.EmitLoadOpCodeForDefaultValueOfType(gen, elementType);
                OpCodeUtil.EmitStoreIndirectOpCodeForType(gen, elementType);
            }
            else if (elementType.IsGenericParameter || elementType.IsValueType) gen.Emit(OpCodes.Initobj, elementType);
            else throw new InvalidOperationException("Can't emit default value for reference of type " + elementType);
        }

        private bool IsPrimitiveOrClass(Type type)
        {
            if ((type.IsPrimitive && type != typeof(IntPtr))) return true;
            return ((type.IsClass || type.IsInterface) && type.IsGenericParameter == false && type.IsByRef == false);
        }
        #endregion
    }

    #region AddressOfReferenceExpression
    internal sealed class AddressOfReferenceExpression : Expression
    {
        private readonly Reference _reference;

        public AddressOfReferenceExpression(Reference reference)
        {
            _reference = reference;
        }

        public override void Emit(IEmissionMemberEmitter member, ILGenerator gen)
        {
            ArgumentsUtil.EmitLoadOwnerAndReference(_reference.OwnerReference, gen);
            _reference.LoadAddressOfReference(gen);
        }
    }
    #endregion

    #region ReferenceExpression
    internal sealed class ReferenceExpression : Expression
    {
        private Reference _reference;

        public ReferenceExpression(Reference reference)
        {
            _reference = reference;
        }

        public override void Emit(IEmissionMemberEmitter member, ILGenerator gen)
        {
            ArgumentsUtil.EmitLoadOwnerAndReference(_reference, gen);
        }
    }
    #endregion
}
