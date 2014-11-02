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
using System.Reflection;

namespace Dev2.Runtime
{
    /// <summary>
    ///     A class for invoking methods on classes.
    /// </summary>
    public class ServiceInvoker
    {
        private readonly string _typeNameFormat;

        #region CTOR

        /// <summary>
        ///     Initializes a new instance of the <see cref="ServiceInvoker" /> class
        ///     where the assembly and namespace names are <b>Dev2.Runtime.Services</b>.
        ///     <seealso cref="ServiceInvoker(string, string)" />
        /// </summary>
        public ServiceInvoker()
            : this("Dev2.Runtime.Services", "Dev2.Runtime.ServiceModel")
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ServiceInvoker" /> class.
        /// </summary>
        /// <param name="assemblyName">The name of the assembly in which the classes containing the methods to be invoked reside.</param>
        /// <param name="namespaceName">The name of the namespace in which the classes containing the methods to be invoked reside.</param>
        public ServiceInvoker(string assemblyName, string namespaceName)
        {
            _typeNameFormat = string.Format("{0}.{{0}}, {1}", namespaceName, assemblyName);
        }

        #endregion

        #region Invoke

        /// <summary>
        ///     Invokes the given method on the given class.
        /// </summary>
        /// <param name="className">The name of the class to be used.</param>
        /// <param name="methodName">The name of the method to be invoked.</param>
        /// <param name="args">The arguments to the method; this is typically a JSON string.</param>
        /// <param name="workspaceID">The workspace ID.</param>
        /// <param name="dataListID">The data list ID.</param>
        /// <returns>
        ///     The result of the operation; this is typically a JSON string.
        /// </returns>
        public object Invoke(string className, string methodName, string args, Guid workspaceID, Guid dataListID)
        {
            Type serviceType = Type.GetType(string.Format(_typeNameFormat, className));
            if (serviceType != null)
            {
                MethodInfo method = serviceType.GetMethod(methodName);
                if (method != null)
                {
                    object service = method.IsStatic ? null : Activator.CreateInstance(serviceType);
                    object actionResult = method.Invoke(service, new object[] {args, workspaceID, dataListID});
                    if (actionResult != null)
                    {
                        return actionResult;
                    }
                }
            }
            return null;
        }

        #endregion
    }
}