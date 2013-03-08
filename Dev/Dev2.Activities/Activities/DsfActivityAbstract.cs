using Dev2;
using Dev2.Common;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Value_Objects;
using Dev2.Diagnostics;
using Dev2.Network.Execution;
using Microsoft.VisualBasic.Activities;
using System;
using System.Activities;
using System.Activities.Presentation;
using System.Collections.Generic;
using System.ComponentModel;
using Unlimited.Applications.BusinessDesignStudio.Activities.Interfaces;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    public abstract class DsfActivityAbstract<T> : DsfNativeActivity<T>, IActivityTemplateFactory, INotifyPropertyChanged, IWizardEditable
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
        public IRecordsetScopingObject ScopingObject { get { return null; } set { value = null; } }
        //public string OutputMapping { get; set; }             

        #region Ctor

        protected DsfActivityAbstract()
            : this(null) {
        }

        protected DsfActivityAbstract(string displayName, bool isAsync = false)
            : this(displayName, DebugDispatcher.Instance, isAsync) {
        }

        protected DsfActivityAbstract(string displayName, IDebugDispatcher debugDispatcher, bool isAsync = false)
            : base(isAsync, displayName, debugDispatcher) 
        {

            AmbientDataList = new InOutArgument<List<string>>();
            ParentInstanceID = new InOutArgument<string>();

            InstructionList = new VisualBasicReference<List<string>> {
                ExpressionText = "InstructionList"
            };
            IsValid = new VisualBasicReference<bool> {
                ExpressionText = "IsValid"
            };
            HasError = new VisualBasicReference<bool> {
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

        public virtual void Resumed(NativeActivityContext context, Bookmark bookmark, object value) {

            IDSFDataObject myDO = context.GetExtension<IDSFDataObject>();
            //IDataListCompiler compiler = context.GetExtension<IDataListCompiler>();
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
            ErrorResultTO errors = new ErrorResultTO();
            ErrorResultTO tmpErrors = new ErrorResultTO();
            Guid executionID = myDO.DataListID;

            if(value != null){
                Guid rootID = GlobalConstants.NullDataListID;
                Guid.TryParse(value.ToString(), out rootID);

                if (executionID == rootID)
                {
                    throw new Exception("Parent and Child DataList IDs are the same, aborting resumption!");
                }

                try {
                    // remove SystemTags before the shape
                    compiler.UpsertSystemTag(executionID, enSystemTag.FormView, string.Empty, out tmpErrors);
                    errors.MergeErrors(tmpErrors);

                    compiler.UpsertSystemTag(executionID, enSystemTag.Service, string.Empty, out tmpErrors);
                    errors.MergeErrors(tmpErrors);

                    compiler.UpsertSystemTag(executionID, enSystemTag.WebServerUrl, string.Empty, out tmpErrors);
                    errors.MergeErrors(tmpErrors);

                    /* Now perform the shape.... */

                    // First set the parentID on executionID to rootID.. so the shape happens correctly ;)
                    compiler.SetParentID(rootID, executionID);
                    // Next shape the execution result into the root datalist ;)
                    Guid shapeID = compiler.Shape(rootID, enDev2ArgumentType.Output, OutputMapping, out tmpErrors);
                    errors.MergeErrors(tmpErrors);

                    // set parent instanceID
                    myDO.DataListID = executionID; // reset the DataListID accordingly

                    if (shapeID != executionID) {
                        throw new Exception("Fatal Error : Cannot merge resumed data");
                    }


                    compiler.ConditionalMerge(DataListMergeFrequency.Always | DataListMergeFrequency.OnResumption,
                        myDO.DatalistOutMergeID, myDO.DataListID, myDO.DatalistOutMergeFrequency, myDO.DatalistOutMergeType, myDO.DatalistOutMergeDepth);
                    ExecutionStatusCallbackDispatcher.Instance.Post(myDO.BookmarkExecutionCallbackID, ExecutionStatusCallbackMessageType.ResumedCallback);

                } finally {
                   // At resumption this is the root dl entry ;)
                   
                    // Handle Errors
                    if (errors.HasErrors()) {
                        DisplayAndWriteError("Resumption", errors);
                        compiler.UpsertSystemTag(myDO.DataListID, enSystemTag.Error, errors.MakeDataListReady(), out errors);
                    }
                }
            } else {
                throw new Exception("Fatal Error : Cannot locate Root DataList for resumption!");
            }
        }

        /// <summary>
        /// this method exist for the datalist server port
        /// it is a crap way to upserting since it replicates all the existing functionality of the server, but it is the quickest
        /// and with an activity re-write coming, it is as good as it needs to be...
        /// </summary>
        /// <param name="outputs"></param>
        /// <param name="dataListID"></param>
        /// <param name="compiler"></param>
        /// <param name="parser"></param>
        /// <param name="errors"></param>
        /// <returns></returns>
        public Guid UpsertOutputs(IList<OutputTO> outputs, Guid dataListID, IDataListCompiler compiler, out ErrorResultTO errors) {

            errors = new ErrorResultTO();
            ErrorResultTO allErrors = new ErrorResultTO();
            ActivityUpsertTO toUpsert = BinaryDataListEntryBuilder.CreateEntriesFromOutputTOs(outputs, compiler, dataListID, out errors);
            if (errors.HasErrors()) {
                allErrors.MergeErrors(errors);
            }
            Guid result = compiler.Upsert(dataListID, toUpsert.FetchExpressions(), toUpsert.FetchBinaryEntries(), out errors);
            if (errors.HasErrors()) {
                allErrors.MergeErrors(errors);
            }

            errors = allErrors;

            return result;
        }


        //protected override void CacheMetadata(NativeActivityMetadata metadata)
        //{
        ////public OutArgument<bool> HasError { get; set; }
        ////public OutArgument<bool> IsValid { get; set; }
        ////public InArgument<string> ExplicitDataList { get; set; }
        ////public InOutArgument<List<string>> AmbientDataList { get; set; }
        ////public InOutArgument<List<string>> InstructionList { get; set; }

        //    RuntimeArgument argHasError = new RuntimeArgument("HasError", typeof(bool), Dev2ColumnArgumentDirection.Out, true);
        //    metadata.Bind(this.HasError, argHasError);

        //    RuntimeArgument argIsValid = new RuntimeArgument("IsValid", typeof(bool), Dev2ColumnArgumentDirection.Out, true);
        //    metadata.Bind(this.IsValid, argIsValid);

        //    RuntimeArgument argExplicitDataList = new RuntimeArgument("ExplicitDataList", typeof(string), Dev2ColumnArgumentDirection.In, true);
        //    metadata.Bind(this.ExplicitDataList, argExplicitDataList);

        //    RuntimeArgument argAmbientDataList = new RuntimeArgument("AmbientDataList", typeof(List<string>), Dev2ColumnArgumentDirection.InOut, true);
        //    metadata.Bind(this.AmbientDataList, argAmbientDataList);

        //    RuntimeArgument argInstructionList = new RuntimeArgument("InstructionList", typeof(List<string>), Dev2ColumnArgumentDirection.InOut, true);
        //    metadata.Bind(this.InstructionList, argInstructionList);

        //    base.CacheMetadata(metadata);
        //}



        #region INotifyPropertyChnaged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string PropertyName) {
            if (PropertyChanged != null) {
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

        #region Get Wizard Data


        /// <summary>
        /// Gets the data for the wizard.
        /// </summary>
        /// <returns></returns>
        public virtual IBinaryDataList GetWizardData() {
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
            
            ErrorResultTO errors;
            IBinaryDataList inputsAndOutputs = compiler.Merge(ActivityInputOutputUtils.GetSimpleInputs(this), ActivityInputOutputUtils.GetSimpleOutputs(this), enDataListMergeTypes.Union, enTranslationDepth.Data, true, out errors);
            return inputsAndOutputs;
        }

        #endregion Get Wizard Data

        #region Get General Settings Data

        /// <summary>
        /// Gets the general setting data.
        /// </summary>
        /// <returns></returns>
        public virtual IBinaryDataList GetGeneralSettingData() {
            return ActivityInputOutputUtils.GetGeneralSettings(this);
        }

        #endregion Get Settings Data

        #region Protected Methods

        protected IDev2DataListEvaluateIterator CreateDataListEvaluateIterator(string expression, Guid executionId, IDataListCompiler compiler, IDev2IteratorCollection iteratorCollection, ErrorResultTO allErrors)
        {
            ErrorResultTO errors = new ErrorResultTO();

            IBinaryDataListEntry expressionEntry = compiler.Evaluate(executionId, enActionType.User, expression, false, out errors);
            allErrors.MergeErrors(errors);
            IDev2DataListEvaluateIterator expressionIterator = Dev2ValueObjectFactory.CreateEvaluateIterator(expressionEntry);
            iteratorCollection.AddIterator(expressionIterator);

            return expressionIterator;
        }

        #endregion Protected Methods
    }
}
