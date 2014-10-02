
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
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace System.Emission
{
    internal abstract class Reference
    {
        #region Instance Fields
        protected Reference _owner = SelfReference.Self;
        #endregion

        #region Public Properties
        public Reference OwnerReference { get { return _owner; } set { _owner = value; } }
        #endregion

        #region Constructors
        protected Reference()
        {
        }

        protected Reference(Reference owner)
        {
            _owner = owner;
        }
        #endregion

        #region Emission Handling
        public abstract void LoadAddressOfReference(ILGenerator gen);
        public abstract void LoadReference(ILGenerator gen);
        public abstract void StoreReference(ILGenerator gen);

        public virtual void Generate(ILGenerator gen)
        {
        }

        public virtual Expression ToAddressOfExpression()
        {
            return new AddressOfReferenceExpression(this);
        }

        public virtual Expression ToExpression()
        {
            return new ReferenceExpression(this);
        }
        #endregion
    }

    [DebuggerDisplay("this")]
    internal sealed class SelfReference : Reference
    {
        #region Readonly Members
        public static readonly SelfReference Self = new SelfReference();
        #endregion

        #region Constructor
        protected SelfReference()
            : base(null)
        {
        }
        #endregion

        #region Emission Handling
        public override void LoadAddressOfReference(ILGenerator gen)
        {
            throw new NotSupportedException();
        }

        public override void LoadReference(ILGenerator gen)
        {
            gen.Emit(OpCodes.Ldarg_0);
        }

        public override void StoreReference(ILGenerator gen)
        {
            gen.Emit(OpCodes.Ldarg_0);
        }
        #endregion
    }

    [DebuggerDisplay("{_fieldBuilder.Name} ({_fieldBuilder.FieldType})")]
    internal sealed class FieldReference : Reference
    {
        #region Instance Fields
        private readonly FieldInfo _field;
        private readonly FieldBuilder _fieldBuilder;
        private readonly bool _isStatic;
        #endregion

        #region Public Properties
        public FieldBuilder Fieldbuilder { get { return _fieldBuilder; } }
        public FieldInfo Reference { get { return _field; } }
        #endregion

        #region Constructors
        public FieldReference(FieldInfo field)
        {
            _field = field;

            if ((field.Attributes & FieldAttributes.Static) != 0)
            {
                _isStatic = true;
                _owner = null;
            }
        }

        public FieldReference(FieldBuilder fieldbuilder)
        {
            _fieldBuilder = fieldbuilder;
            _field = fieldbuilder;

            if ((fieldbuilder.Attributes & FieldAttributes.Static) != 0)
            {
                _isStatic = true;
                _owner = null;
            }
        }
        #endregion

        #region Emission Handling
        public override void LoadAddressOfReference(ILGenerator gen)
        {
            if (_isStatic) gen.Emit(OpCodes.Ldsflda, Reference);
            else gen.Emit(OpCodes.Ldflda, Reference);
        }

        public override void LoadReference(ILGenerator gen)
        {
            if (_isStatic) gen.Emit(OpCodes.Ldsfld, Reference);
            else gen.Emit(OpCodes.Ldfld, Reference);
        }

        public override void StoreReference(ILGenerator gen)
        {
            if (_isStatic) gen.Emit(OpCodes.Stsfld, Reference);
            else gen.Emit(OpCodes.Stfld, Reference);
        }
        #endregion
    }

    #region TypeReference
    internal abstract class TypeReference : Reference
    {
        private Type _type;

        public Type Type { get { return _type; } }

        protected TypeReference(Type argumentType)
            : this(null, argumentType)
        {
        }

        protected TypeReference(Reference owner, Type type)
            : base(owner)
        {
            _type = type;
        }
    }
    #endregion

    #region LocalReference
    [DebuggerDisplay("local {Type}")]
    internal sealed class LocalReference : TypeReference
    {
        private LocalBuilder _localBuilder;

        public LocalReference(Type type)
            : base(type)
        {
        }

        public override void Generate(ILGenerator gen)
        {
            _localBuilder = gen.DeclareLocal(base.Type);
        }

        public override void LoadAddressOfReference(ILGenerator gen)
        {
            gen.Emit(OpCodes.Ldloca, _localBuilder);
        }

        public override void LoadReference(ILGenerator gen)
        {
            gen.Emit(OpCodes.Ldloc, _localBuilder);
        }

        public override void StoreReference(ILGenerator gen)
        {
            gen.Emit(OpCodes.Stloc, _localBuilder);
        }
    }
    #endregion

    #region ArgumentReference
    [DebuggerDisplay("argument {Type}")]
    internal sealed class ArgumentReference : TypeReference
    {
        private int _position;

        internal int Position { get { return _position; } set { _position = value; } }

        public ArgumentReference(Type argumentType)
            : base(argumentType)
        {
            _position = -1;
        }

        public ArgumentReference(Type argumentType, int position)
            : base(argumentType)
        {
            _position = position;
        }

        public override void LoadAddressOfReference(ILGenerator gen)
        {
            throw new NotSupportedException();
        }

        public override void LoadReference(ILGenerator gen)
        {
            if (_position == -1) throw new InvalidOperationException("ArgumentReference unitialized");

            switch (_position)
            {
                case 0: gen.Emit(OpCodes.Ldarg_0); break;
                case 1: gen.Emit(OpCodes.Ldarg_1); break;
                case 2: gen.Emit(OpCodes.Ldarg_2); break;
                case 3: gen.Emit(OpCodes.Ldarg_3); break;
                default: gen.Emit(OpCodes.Ldarg_S, Position); break;
            }
        }

        public override void StoreReference(ILGenerator gen)
        {
            if (_position == -1) throw new InvalidOperationException("ArgumentReference unitialized");
            gen.Emit(OpCodes.Starg, Position);
        }
    }
    #endregion
}
