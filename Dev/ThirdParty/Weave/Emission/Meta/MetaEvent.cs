
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
using System.Text;
using System.Threading.Tasks;

namespace System.Emission.Meta
{
    internal sealed class MetaEvent : MetaTypeElement, IEquatable<MetaEvent>
    {
        #region Instance Fields
        private MetaMethod _adder;
        private MetaMethod _remover;
        private Type _type;
        private EventEmitter _emitter;
        private EventAttributes _attributes;
        private string _name;
        #endregion

        #region Public Properties
        public EventAttributes Attributes { get { return _attributes; } }
        public MetaMethod Adder { get { return _adder; } }
        public MetaMethod Remover { get { return _remover; } }
        public EventEmitter Emitter { get { if (_emitter == null) throw new InvalidOperationException("Emitter is not built. You have to build it first using 'BuildEventEmitter' method"); return _emitter; } }
        #endregion

        public MetaEvent(string dynamicAssemblyName, string name, Type declaringType, Type eventDelegateType, MetaMethod adder, MetaMethod remover, EventAttributes attributes)
            : base(declaringType, dynamicAssemblyName)
        {
            if (adder == null) throw new ArgumentNullException("adder");
            if (remover == null) throw new ArgumentNullException("remover");
            _name = name;
            _type = eventDelegateType;
            _adder = adder;
            _remover = remover;
            _attributes = attributes;
        }


        #region Build Handling
        public void BuildEventEmitter(ClassEmitter classEmitter)
        {
            if (_emitter != null) throw new InvalidOperationException();
            _emitter = classEmitter.CreateEvent(_name, Attributes, _type);
        }
        #endregion

        #region Overrides
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(MetaEvent)) return false;
            return Equals((MetaEvent)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = (_adder.Method != null ? _adder.Method.GetHashCode() : 0);
                result = (result * 397) ^ (_remover.Method != null ? _remover.Method.GetHashCode() : 0);
                result = (result * 397) ^ Attributes.GetHashCode();
                return result;
            }
        }
        #endregion

        #region Equality Handling
        public bool Equals(MetaEvent other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            if (!_type.Equals(other._type)) return false;
            if (!StringComparer.OrdinalIgnoreCase.Equals(_name, other._name)) return false;
            return true;
        }
        #endregion

        #region Switch Handling
        internal override void SwitchToExplicitImplementation()
        {
            _name = string.Format("{0}.{1}", sourceType.Name, _name);
            _adder.SwitchToExplicitImplementation();
            _remover.SwitchToExplicitImplementation();
        }
        #endregion
    }
}
