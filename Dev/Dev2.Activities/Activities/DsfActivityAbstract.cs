
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
using System.Activities.Presentation;
using System.Collections.Generic;
using System.ComponentModel;
using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Activity;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Data;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Diagnostics.Debug;
using Dev2.Network.Execution;
using Microsoft.VisualBasic.Activities;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Warewolf.Storage;

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
namespace Unlimited.Applications.BusinessDesignStudio.Activities
// ReSharper restore CheckNamespace
{
    public abstract class DsfActivityAbstract<T> : DsfNativeActivity<T>, IActivityTemplateFactory, INotifyPropertyChanged
    {
        // TODO: Remove legacy properties - when we've figured out how to load files when these are not present
        public string SimulationOutput { get; set; }
        // END TODO: Remove legacy properties 

        public OutArgument<bool> HasError { get; set; }
        public OutArgument<bool> IsValid { get; set; }
        public InArgument<string> ExplicitDataList { get; set; }
        public InOutArgument<List<string>> InstructionList { get; set; }
        public string ResultTransformation { get; set; }
        public string InputTransformation { get; set; }
        public bool Add { get; set; }

        public bool OnResumeClearAmbientDataList { get; set; }
        public string OnResumeClearTags { get; set; }
        public string OnResumeKeepList { get; set; }
        public bool IsUIStep { get; set; }
        public bool DatabindRecursive { get; set; }
        public string CurrentResult { get; set; }
        public InOutArgument<string> ParentInstanceID { get; set; }
        // ReSharper disable RedundantAssignment
        public IRecordsetScopingObject ScopingObject { get { return null; } set { value = null; } }
        // ReSharper restore RedundantAssignment

        #region Ctor

        protected DsfActivityAbstract()
            : this(null)
        {
        }

        protected DsfActivityAbstract(string displayName, bool isAsync = false)
            : this(displayName, DebugDispatcher.Instance, isAsync)
        {
        }

        protected DsfActivityAbstract(string displayName, IDebugDispatcher debugDispatcher, bool isAsync = false)
            : base(isAsync, displayName, debugDispatcher)
        {

            AmbientDataList = new InOutArgument<List<string>>();
            ParentInstanceID = new InOutArgument<string>();

            InstructionList = new VisualBasicReference<List<string>>
            {
                ExpressionText = "InstructionList"
            };
            IsValid = new VisualBasicReference<bool>
            {
                ExpressionText = "IsValid"
            };
            HasError = new VisualBasicReference<bool>
            {
                ExpressionText = "HasError"
            };

            OnResumeClearTags = "FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage";
        }
        #endregion

        public void Notify(IApplicationMessage messageNotifier, string message)
        {
            if(messageNotifier != null && !string.IsNullOrEmpty(message))
            {
                messageNotifier.SendMessage(message);
            }
        }

        public Activity Create(System.Windows.DependencyObject target)
        {
            return this;
        }

        /// <summary>
        /// Resumed the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="bookmark">The bookmark.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="System.Exception">
        /// Parent and Child DataList IDs are the same, aborting resumption!
        /// or
        /// Fatal Error : Cannot merge resumed data
        /// or
        /// Fatal Error : Cannot locate Root DataList for resumption!
        /// </exception>
        public virtual void Resumed(NativeActivityContext context, Bookmark bookmark, object value)
        {

            IDSFDataObject myDO = context.GetExtension<IDSFDataObject>();
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
            ErrorResultTO errorResultTO = new ErrorResultTO();
            Guid executionID = myDO.DataListID;

            if(value != null)
            {
                Guid rootID;
                Guid.TryParse(value.ToString(), out rootID);

                if(executionID == rootID)
                {
                    throw new Exception("Parent and Child DataList IDs are the same, aborting resumption!");
                }

                try
                {

                    /* Now perform the shape.... */

                    // First set the parentID on executionID to rootID.. so the shape happens correctly ;)
                    compiler.SetParentID(rootID, executionID);
                    // Next shape the execution result into the root datalist ;)
                    ErrorResultTO tmpErrors;
                    Guid shapeID = compiler.Shape(rootID, enDev2ArgumentType.Output, OutputMapping, out tmpErrors);
                    errorResultTO.MergeErrors(tmpErrors);

                    // set parent instanceID
                    myDO.DataListID = executionID; // reset the DataListID accordingly

                    if(shapeID != executionID)
                    {
                        throw new Exception("Fatal Error : Cannot merge resumed data");
                    }


                    compiler.ConditionalMerge(DataListMergeFrequency.Always | DataListMergeFrequency.OnResumption,
                        myDO.DatalistOutMergeID, myDO.DataListID, myDO.DatalistOutMergeFrequency, myDO.DatalistOutMergeType, myDO.DatalistOutMergeDepth);
                    ExecutionStatusCallbackDispatcher.Instance.Post(myDO.BookmarkExecutionCallbackID, ExecutionStatusCallbackMessageType.ResumedCallback);

                }
                finally
                {
                    // At resumption this is the root dl entry ;)

                    // Handle Errors
                    if(errorResultTO.HasErrors())
                    {
                        DisplayAndWriteError("Resumption", errorResultTO);
                        compiler.UpsertSystemTag(myDO.DataListID, enSystemTag.Dev2Error, errorResultTO.MakeDataListReady(), out errorResultTO);
                    }
                }
            }
            else
            {
                throw new Exception("Fatal Error : Cannot locate Root DataList for resumption!");
            }
        }

        /// <summary>
        /// this method exist for the datalist server port
        /// it is a crap way to upserting since it replicates all the existing functionality of the server, but it is the quickest
        /// and with an activity re-write coming, it is as good as it needs to be...
        /// </summary>
        /// <param name="outputs">The outputs.</param>
        /// <param name="dataListID">The data list unique identifier.</param>
        /// <param name="compiler">The compiler.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public Guid UpsertOutputs(IList<OutputTO> outputs, Guid dataListID, IDataListCompiler compiler, out ErrorResultTO errors)
        {
            ErrorResultTO allErrors = new ErrorResultTO();
            ActivityUpsertTO toUpsert = BinaryDataListEntryBuilder.CreateEntriesFromOutputTOs(outputs, compiler, dataListID, out errors);
            if(errors.HasErrors())
            {
                allErrors.MergeErrors(errors);
            }
            Guid result = compiler.Upsert(dataListID, toUpsert.FetchExpressions(), toUpsert.FetchBinaryEntries(), out errors);
            if(errors.HasErrors())
            {
                allErrors.MergeErrors(errors);
            }

            errors = allErrors;

            return result;
        }

        #region INotifyPropertyChnaged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string PropertyName)
        {
            if(PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(PropertyName));
            }
        }

        #endregion INotifyPropertyChnaged

        #region Get Inputs/Outputs


        /// <summary>
        /// Gets the inputs.
        /// </summary>
        /// <returns></returns>
        public virtual IBinaryDataList GetInputs()
        {
            return ActivityInputOutputUtils.GetSimpleInputs(this);
        }

        /// <summary>
        /// Gets the outputs.
        /// </summary>
        /// <returns></returns>
        public virtual IBinaryDataList GetOutputs()
        {
            return ActivityInputOutputUtils.GetSimpleOutputs(this);
        }

        #endregion Get Inputs/Outputs

        #region Get General Settings Data

        /// <summary>
        /// Gets the general setting data.
        /// </summary>
        /// <returns></returns>
        public virtual IBinaryDataList GetGeneralSettingData()
        {
            return ActivityInputOutputUtils.GetGeneralSettings(this);
        }

        #endregion Get Settings Data

        #region Protected Methods

        protected IWarewolfIterator CreateDataListEvaluateIterator(string expression, IExecutionEnvironment executionEnvironment)
        {
            var expressionIterator = new WarewolfIterator(executionEnvironment.Eval(expression));
            return expressionIterator;
        }

        protected void ValidateRecordsetName(string recordsetName, ErrorResultTO errors)
        {
            if(string.IsNullOrEmpty(recordsetName))
            {
                errors.AddError("No recordset given");
            }

            if(DataListCleaningUtils.SplitIntoRegions(recordsetName).Count > 1)
            {
                errors.AddError("Can only accept one variable");
            }
            else
            {
                if(DataListUtil.IsValueRecordset(recordsetName))
                {
                    if(DataListUtil.IsValueRecordsetWithFields(recordsetName))
                    {
                        errors.AddError("Must only be a recordset name");
                    }

                }
                else
                {
                    errors.AddError("Value must be a recordset name");
                }
            }
        }

        #endregion Protected Methods
    }
}
