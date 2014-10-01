
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
using System.Linq;
using Dev2.Common.Interfaces.Core.Graph;

namespace Unlimited.Framework.Converters.Graph.Poco
{
    public class PocoInterrogator : IInterrogator
    {
        #region Methods

        public IMapper CreateMapper(object data)
        {
            return new PocoMapper();
        }

        public INavigator CreateNavigator(object data, Type pathType)
        {
            if (!pathType.GetInterfaces().Contains(typeof(IPath)))
            {
                throw new Exception("'" + pathType + "' doesn't implement '" + typeof(IPath) + "'");
            }

            return new PocoNavigator(data);
        }

        #endregion Methods
    }
}
