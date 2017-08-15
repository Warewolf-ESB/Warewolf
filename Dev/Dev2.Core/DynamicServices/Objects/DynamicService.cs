/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

#region Change Log

//  Author:         Sameer Chunilall
//  Date:           2010-01-24
//  Log No:         9299
//  Description:    This type represents a dynamic service with all its metadata loaded from the service
//                  definition file or ad hoc using the LoadAndExecute ManagementDynamicService
//                  
//                  
//                  

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.DynamicServices.Objects.Base;


namespace Dev2.DynamicServices
{
    /// <summary>
    ///     Provides an representation of a service
    ///     A service can contain actions that define what the service can do
    ///     This class is hydrated from the service definition file.
    /// </summary>
    public class DynamicService : DynamicServiceObjectBase
    {
        /// <summary>
        ///     The actions that this service runs
        /// </summary>
        public List<ServiceAction> Actions { get; set; }
        
        public Guid ID { get; set; }

        public Guid ServiceId { get; set; }


        /// <summary>
        ///     Initializes the Dynamic Service
        /// </summary>
        public DynamicService()
            : base(enDynamicServiceObjectType.DynamicService)
        {
            //Initialize the Actions Property
            Actions = new List<ServiceAction>();
        }


        /// <summary>
        ///     Compiles this object
        /// </summary>
        /// <returns></returns>
        public override bool Compile()
        {
            base.Compile();

            if (Actions.Count == 0)
            {
                WriteCompileError(Resources.CompilerError_ServiceHasNoActions);
            }

            Actions.ForEach(c =>
            {
                c.Compile();
                c.CompilerErrors.ToList().ForEach(d => CompilerErrors.Add(d));
            });


            return IsCompiled;
        }
    }

}