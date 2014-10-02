
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
using System.Emission.Meta;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace System.Emission.Collectors
{
    internal sealed class NetworkInterfaceProxyMemberCollector : EmissionMemberCollector
    {
        public NetworkInterfaceProxyMemberCollector(string dynamicAssemblyName, Type interfaceType)
            : base(interfaceType, dynamicAssemblyName)
        {
        }

        protected override MetaMethod GetMethodToGenerate(MethodInfo method, IEmissionProxyHook hook, bool isStandalone, MetaMethodSource source)
        {
            if (method.IsAccessible(DynamicAsssemblyName) == false)
            {
                return null;
            }

            var proxyable = AcceptMethod(method, false, hook);
            return new MetaMethod(DynamicAsssemblyName, method, method, isStandalone, proxyable, false, source);
        }
    }
}
