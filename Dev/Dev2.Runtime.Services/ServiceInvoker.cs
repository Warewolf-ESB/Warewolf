/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;

namespace Dev2.Runtime
{
    public class ServiceInvoker
    {
        readonly string _typeNameFormat;

        #region CTOR
        
        public ServiceInvoker()
            : this("Dev2.Runtime.Services", "Dev2.Runtime.ServiceModel")
        {
        }
        
        public ServiceInvoker(string assemblyName, string namespaceName)
        {
            _typeNameFormat = string.Format("{0}.{{0}}, {1}", namespaceName, assemblyName);
        }

        #endregion

        #region Invoke
        
        public object Invoke(string className, string methodName, string args, Guid workspaceID, Guid dataListID)
        {
            return null;
        }

        #endregion
    }
}
