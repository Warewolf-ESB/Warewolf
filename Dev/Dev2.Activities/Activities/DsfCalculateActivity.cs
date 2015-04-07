
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
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using Dev2;
using Dev2.Activities;
using Dev2.Common;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Data;
using Dev2.DataList.Contract;
using Dev2.Diagnostics;
using Dev2.Util;
using Dev2.Validation;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Warewolf.Storage;

// ReSharper disable CheckNamespace
namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    public class DsfCalculateActivity : DsfActivityAbstract<string>
    {

        #region Properties

        /// <summary>
        /// The property that holds the Expression string the user enters into the "fx" box
        /// </summary>
        [Inputs("Expression")]
        [FindMissing]
        public string Expression { get; set; }

        /// <summary>
        /// The property that holds the Result string the user enters into the "Result" box
        /// </summary>
        [Outputs("Result")]
        [FindMissing]
        public new string Result { get; set; }

        #endregion Properties

        #region Ctor

        public DsfCalculateActivity()
            : base("Calculate")
        {
            Expression = string.Empty;
            Result = string.Empty;
        }

        #endregion Ctor

        #region Override Abstract Methods

        /// <summary>
        /// The execute method that is called when the activity is executed at run time and will hold all the logic of the activity
        /// </summary> 
        protected override void OnExecute(NativeActivityContext context)
        {
            _debugInputs = new List<DebugItem>();
            _debugOutputs = new List<DebugItem>();
            IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();

            ErrorResultTO allErrors = new ErrorResultTO();
            ErrorResultTO errors = new ErrorResultTO();
            allErrors.MergeErrors(errors);
            InitializeDebug(dataObject);
            try
            {
                IsSingleValueRule.ApplyIsSingleValueRule(Result, allErrors);

                if(dataObject.IsDebugMode())
                {
                    AddDebugInputItem(dataObject.Environment);
                }

                string input = string.IsNullOrEmpty(Expression) ? Expression : Expression.Replace("\\r", string.Empty).Replace("\\n", string.Empty).Replace(Environment.NewLine, "");
                var warewolfListIterator = new WarewolfListIterator();
                var calc = String.Format(GlobalConstants.CalculateTextConvertFormat,input);
                var inputIterator = new WarewolfIterator(dataObject.Environment.Eval(calc));
                warewolfListIterator.AddVariableToIterateOn(inputIterator);
                while(warewolfListIterator.HasMoreData())
                {
                    var result = warewolfListIterator.FetchNextValue(inputIterator);
                    dataObject.Environment.Assign(Result, result);
                }
                
                if(dataObject.IsDebugMode() && !allErrors.HasErrors())
                {
                    AddDebugOutputItem(Result, dataObject.Environment);
                }
                allErrors.MergeErrors(errors);
            }
            catch(Exception ex)
            {

                Dev2Logger.Log.Error("Calculate Exception", ex);
                allErrors.AddError(ex.Message);
            }
            finally
            {
                // Handle Errors
                var hasErrors = allErrors.HasErrors();
                if(hasErrors)
                {
                    DisplayAndWriteError("DsfCalculateActivity", allErrors);
                    var errorString = allErrors.MakeDisplayReady();
                    dataObject.Environment.AddError(errorString);
                    dataObject.Environment.Assign(Result, null);
                }
                if(dataObject.IsDebugMode())
                {
                    if(hasErrors)
                    {
                        AddDebugOutputItem(Result, dataObject.Environment);
                    }
                    DispatchDebugState(context, StateType.Before);
                    DispatchDebugState(context, StateType.After);
                }
            }

        }

        #endregion Override Abstract Methods

        #region Private Methods

        private void AddDebugInputItem(IExecutionEnvironment environment)
        {
            AddDebugInputItem(new DebugEvalResult(Expression, "fx =", environment));
        }

        private void AddDebugOutputItem(string expression, IExecutionEnvironment environment)
        {
            AddDebugOutputItem(new DebugEvalResult(expression, "", environment));
        }

        #endregion Private Methods

        #region Get Debug Inputs/Outputs

        public override List<DebugItem> GetDebugInputs(IExecutionEnvironment dataList)
        {
            foreach(IDebugItem debugInput in _debugInputs)
            {
                debugInput.FlushStringBuilder();
            }
            return _debugInputs;
        }

        public override List<DebugItem> GetDebugOutputs(IExecutionEnvironment dataList)
        {
            foreach(IDebugItem debugOutput in _debugOutputs)
            {
                debugOutput.FlushStringBuilder();
            }
            return _debugOutputs;
        }

        #endregion Get Inputs/Outputs

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {

            if(updates != null && updates.Count == 1)
            {
                Expression = updates[0].Item2;
            }
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            if(updates != null)
            {
                var itemUpdate = updates.FirstOrDefault(tuple => tuple.Item1 == Result);
                if(itemUpdate != null)
                {
                    Result = itemUpdate.Item2;
                }
            }
        }


        #region GetForEachInputs/Outputs

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            return GetForEachItems(Expression);
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            return GetForEachItems(Result);
        }

        #endregion
    }
}

