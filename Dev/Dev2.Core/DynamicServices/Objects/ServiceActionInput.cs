/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
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

// ReSharper disable CheckNamespace

namespace Dev2.DynamicServices
{

    #region Using Directives

    #endregion

    #region Service Action Input Class - Represents an for a service action

    /// <summary>
    ///     Provides a representation of the input of a dynamic service
    ///     Validates the input
    /// </summary>
    public class ServiceActionInput : DynamicServiceObjectBase
    {
        #region Public Properties

        /// <summary>
        ///     Used to invoke the correct Plugin Method with parm type and order
        /// </summary>
        public string NativeType { get; set; }

        /// <summary>
        ///     Do we convert Empty strings to null?
        /// </summary>
        public bool EmptyToNull { get; set; }

        /// <summary>
        ///     The validators that must execute to validate this input
        /// </summary>
        public List<Validator> Validators { get; set; }

        /// <summary>
        ///     The name of the field from the source system - this provides a mapping between
        ///     the client request field and the data source field - they may have different names
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        ///     The value of the field to validate from the client request
        /// </summary>
        public object Value { get; set; }

        public string DefaultValue { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes the service action input
        /// </summary>
        public ServiceActionInput()
            : base(enDynamicServiceObjectType.ServiceActionInput)
        {
            Validators = new List<Validator>();
        }

        #endregion

        #region Public Methods

        public override bool Compile()
        {
            base.Compile();

            if (string.IsNullOrEmpty(Source))
            {
                WriteCompileError(Resources.CompilerError_SourceNotFound);
            }

            Validators.ForEach(c =>
            {
                c.Compile();
                c.CompilerErrors.ToList().ForEach(d => CompilerErrors.Add(d));
            });

            return IsCompiled;
        }


        /// <summary>
        ///     Runs all the validators in the validator list against the input value
        /// </summary>
        /// <returns>Boolean indicating whether the validation was successful</returns>
        public virtual bool Validate()
        {
            bool isValid = !Validators.Any();
            foreach (Validator validator in Validators)
            {
                switch (validator.ValidatorType)
                {
                    case enValidationType.Required:
                        isValid = ValidateRequired();
                        break;
                }
            }

            return isValid;
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Validates a required field against an input value
        /// </summary>
        /// <returns>Boolean indicating whether the field passed required field validation</returns>
        private bool ValidateRequired()
        {
            bool isValid = !string.IsNullOrEmpty(Value.ToString());

            return isValid;
        }

        #endregion
    }

    #endregion
}