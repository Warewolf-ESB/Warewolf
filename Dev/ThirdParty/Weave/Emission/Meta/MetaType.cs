
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
