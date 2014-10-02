
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

namespace System.Network
{
    public abstract class IOCPExtensionHandler<T> where T : NetworkContext
    {
        private PacketTemplate _template;

        public PacketTemplate Template { get { return _template; } }

        public IOCPExtensionHandler(PacketTemplate template)
        {
            if (template == null) throw new ArgumentNullException("template");
            _template = template;
        }

        protected internal abstract void Dispatch(IOCPPacketHandlerCollection<T> channel, T context, PacketData extension, PacketData packet);
        protected internal abstract void Response(IOCPPacketHandlerCollection<T> channel, T context, PacketData extension, PacketData packet);
    }
}
