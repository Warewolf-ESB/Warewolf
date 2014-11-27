
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
using Dev2.Common;
using Infragistics.Calculations;
using Infragistics.Calculations.CalcManager;

// ReSharper disable CheckNamespace
namespace Dev2.MathOperations
// ReSharper restore CheckNamespace
{

    // PBI 1214: This class is used to create a dev2 function used by the Calculate Tool
    //           This contains a set of values that can fully describe a function
    //           from it's Name, to the Argument List, to the description of what the function actually does.
    public class Function : IFunction
    {

        #region Private Members

        private string _functionName;
        private IList<string> _arguments;
        private IList<string> _argumentDescriptions;
        private string _description;

        #endregion Private Members

        #region Properties

        public string FunctionName
        {
            get { return _functionName; }
        }

        public IList<string> arguments
        {
            get { return _arguments; }
        }

        public IList<string> ArgumentDescriptions
        {
            get { return _argumentDescriptions; }
        }

        public string Description
        {
            get { return _description; }
        }

        #endregion Properties

        #region Ctor

        internal Function(string functionName, IList<string> arguments, IList<string> argumentDescriptions, string description)
        {
            SetFunctionName(functionName);
            SetArguments(arguments);
            SetArgumentDescriptions(argumentDescriptions);
            SetDescription(description);
        }

        internal Function()
        {

        }

        #endregion Ctor

        #region Public Methods

        public void CreateCustomFunction(string functionName, List<string> args, string description, Func<double[], double> function, IDev2CalculationManager calcManager)
        {
            CustomCalculationFunction calcFunction;
            if(CreateCustomFunction(functionName, function, out calcFunction))
            {
                if(calcManager != null)
                {
                    calcManager.RegisterUserDefinedFunction(calcFunction);
                    _functionName = functionName;
                    _arguments = args;
                    _argumentDescriptions = new List<string>();
                    _description = description;
                }
                else
                {
                    throw new NullReferenceException("Calculation Manager is currently null");
                }
            }

            else
            {
                throw new InvalidOperationException("Unable to create the defined function");
            }



        }

        public void CreateCustomFunction(string functionName, List<string> args, List<string> argumentDescriptions, string description, Func<double[], double> function, IDev2CalculationManager calcManager)
        {
            CustomCalculationFunction calcFunction;
            if(CreateCustomFunction(functionName, function, out calcFunction))
            {
                if(calcManager != null)
                {
                    calcManager.RegisterUserDefinedFunction(calcFunction);
                    SetFunctionName(functionName);
                    SetArguments(args);
                    SetArgumentDescriptions(argumentDescriptions);
                    SetDescription(description);
                }
                else
                {
                    throw new NullReferenceException("Calculation Manager is currently null");
                }
            }

            else
            {
                throw new InvalidOperationException("Unable to create the defined function");
            }



        }

        #endregion Public Methods

        #region Private Methods

        private static bool CreateCustomFunction(string functionName, Func<double[], double> func, out CustomCalculationFunction custCalculation)
        {
            bool isSucessfullyCreated;            
            try
            {
                custCalculation = new CustomCalculationFunction(functionName, func, 0, 1);
                isSucessfullyCreated = true;
            }
            catch(Exception ex)
            {
                Dev2Logger.Log.Error("Function", ex);
                custCalculation = null;
                isSucessfullyCreated = false;
            }
            return isSucessfullyCreated;

        }

        private void SetFunctionName(string functionName)
        {
            if(!(string.IsNullOrEmpty(functionName)))
            {
                _functionName = functionName;
            }
            else
            {
                // ReSharper disable once NotResolvedInText
                throw new ArgumentNullException("Cannot set Function Name to an empty string");
            }
        }

        private void SetArguments(IList<string> args)
        {
            _arguments = args ?? new List<string>();
        }

        private void SetArgumentDescriptions(IList<string> argumentDescriptions)
        {
            _argumentDescriptions = argumentDescriptions ?? new List<string>();
        }

        private void SetDescription(string description)
        {
            _description = !(string.IsNullOrEmpty(description)) ? description : string.Empty;
        }

        #endregion Private Methods
    }
}
