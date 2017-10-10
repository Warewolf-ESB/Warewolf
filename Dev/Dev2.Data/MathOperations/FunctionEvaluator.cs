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
using System.Globalization;
using Dev2.Common;
using Infragistics.Calculations.CalcManager;
using Infragistics.Calculations.Engine;
using Warewolf.Resource.Errors;


namespace Dev2.MathOperations

{
    public class FunctionEvaluator : IFunctionEvaluator
    {

        #region Private Members
        private readonly IDev2CalculationManager _manager;

        #endregion Private Members

        #region Ctor

        public FunctionEvaluator()
        {
            _manager = new Dev2CalculationManager();
        }

        #endregion Ctor

        #region Public Methods

        /// <summary>
        /// Evaluate the expression according to the operation specified and pass this to the CalculationManager
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="evaluation">The evaluation.</param>
        /// <param name="error">The error.</param>
        /// <returns></returns>
        public bool TryEvaluateFunction(string expression, out string evaluation, out string error)
        {
            bool evaluationState = false;
            error = String.Empty;
            evaluation = String.Empty;
            if(!String.IsNullOrEmpty(expression))
            {

                try
                {
                    CalculationValue value = _manager.CalculateFormula(expression);
                    if(value.IsError)
                    {
                        error = value.ToErrorValue().Message;
                    }
                    else
                    {
                        if(value.IsDateTime)
                        {
                            DateTime dateTime = value.ToDateTime();                            
                            evaluation = dateTime.ToString(GlobalConstants.Dev2DotNetDefaultDateTimeFormat);
                        }
                        else
                        {
                            evaluation = value.GetResolvedValue().ToString();
                        }
                        evaluationState = true;
                    }
                }
                catch(Exception ex)
                {
                    Dev2Logger.Error(ErrorResource.FunctionEvaluationError, ex, GlobalConstants.WarewolfError);
                    error = ex.Message;
                    evaluationState = false;
                }
            }
            else
            {
                error = ErrorResource.NothingToEvaluate;
            }

            return evaluationState;
        }

        // It expects a List of Type To (either strings or any type of object that is IComparable).
        // And evaluates the whole list against the expression.

        #endregion Public Methods

        #region Private Methods

        #endregion Private Methods

    }
}
