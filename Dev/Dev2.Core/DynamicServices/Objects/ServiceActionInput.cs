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
//  Description:    The Service Action Input type represents the inputs i.e. parameters that will be passed within every xml data request
//                  that will be serviced by the Dynamic Service Engine.
//                  This type is responsible for validating all input data

#endregion

using System.Collections.Generic;
using System.Linq;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.DynamicServices.Objects.Base;



namespace Dev2.DynamicServices
{

    #region Using Directives

    #endregion

    #region Service Action Input Class - Represents an for a service action
    
    public class ServiceActionInput : DynamicServiceObjectBase
    {
        #region Public Properties
        
        public string NativeType { get; set; }
        
        public bool EmptyToNull { get; set; }
        
        public List<Validator> Validators { get; set; }
        
        public string Source { get; set; }
        
        public object Value { get; set; }

        public string DefaultValue { get; set; }

        #endregion

        #region Constructors
        
        public ServiceActionInput()
            : base(enDynamicServiceObjectType.ServiceActionInput)
        {
            Validators = new List<Validator>();
        }

        #endregion
    }

    #endregion
}