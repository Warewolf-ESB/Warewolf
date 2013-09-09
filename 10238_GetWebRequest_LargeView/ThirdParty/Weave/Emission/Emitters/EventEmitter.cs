using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace System.Emission.Emitters
{
    internal sealed class EventEmitter : IEmissionMemberEmitter
    {
        #region Instance Fields
        private EventBuilder _eventBuilder;
        private Type _type;
        private AbstractTypeEmitter _typeEmitter;
        private MethodEmitter _addMethod;
        private MethodEmitter _removeMethod;
        #endregion

        #region Public Properties
        public MemberInfo Member { get { return null; } }
        public Type ReturnType { get { return _type; } }
        #endregion

        #region Constructor
        public EventEmitter(AbstractTypeEmitter typeEmitter, string name, EventAttributes attributes, Type type)
        {
            if (name == null) throw new ArgumentNullException("name");
            if (type == null) throw new ArgumentNullException("type");

            _typeEmitter = typeEmitter;
            _type = type;
            _eventBuilder = typeEmitter.TypeBuilder.DefineEvent(name, attributes, type);
        }
        #endregion

        #region Creation Handling
        public MethodEmitter CreateAddMethod(string addMethodName, MethodAttributes attributes, MethodInfo methodToOverride)
        {
            if (_addMethod != null) throw new InvalidOperationException("An add method exists");
            _addMethod = new MethodEmitter(_typeEmitter, addMethodName, attributes, methodToOverride);
            return _addMethod;
        }

        public MethodEmitter CreateRemoveMethod(string removeMethodName, MethodAttributes attributes, MethodInfo methodToOverride)
        {
            if (_removeMethod != null) throw new InvalidOperationException("A remove method exists");
            _removeMethod = new MethodEmitter(_typeEmitter, removeMethodName, attributes, methodToOverride);
            return _removeMethod;
        }
        #endregion

        #region Generation Handling
        public void Generate()
        {
            if (_addMethod == null) throw new InvalidOperationException("Event add method was not created");
            if (_removeMethod == null) throw new InvalidOperationException("Event remove method was not created");

            _addMethod.Generate();
            _eventBuilder.SetAddOnMethod(_addMethod.MethodBuilder);
            _removeMethod.Generate();
            _eventBuilder.SetRemoveOnMethod(_removeMethod.MethodBuilder);
        }

        public void EnsureValidCodeBlock()
        {
            _addMethod.EnsureValidCodeBlock();
            _removeMethod.EnsureValidCodeBlock();
        }
        #endregion
    }
}
