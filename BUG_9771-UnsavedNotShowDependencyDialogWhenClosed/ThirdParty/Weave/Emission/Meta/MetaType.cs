using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Emission.Meta
{
    internal sealed class MetaType
    {
        private ICollection<MetaEvent> _events = new TypeElementCollection<MetaEvent>();
        private ICollection<MetaMethod> _methods = new TypeElementCollection<MetaMethod>();
        private ICollection<MetaProperty> _properties = new TypeElementCollection<MetaProperty>();

        public IEnumerable<MetaMethod> Methods { get { return _methods; } }
        public IEnumerable<MetaProperty> Properties { get { return _properties; } }
        public IEnumerable<MetaEvent> Events { get { return _events; } }

        public void AddEvent(MetaEvent @event)
        {
            _events.Add(@event);
        }

        public void AddMethod(MetaMethod method)
        {
            _methods.Add(method);
        }

        public void AddProperty(MetaProperty property)
        {
            _properties.Add(property);
        }
    }
}
