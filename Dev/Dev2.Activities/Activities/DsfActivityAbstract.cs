
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
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
using System.Windows;
using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Activity;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Data;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.Diagnostics.Debug;
using Microsoft.VisualBasic.Activities;
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

        public Activity Create(DependencyObject target)
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


                }
                finally
                {
                    // At resumption this is the root dl entry ;)

                    // Handle Errors
                    if(errorResultTO.HasErrors())
                    {
                        DisplayAndWriteError("Resumption", errorResultTO);
                        var errorString = errorResultTO.MakeDataListReady();
                        myDO.Environment.AddError(errorString);
                    }
                }
            }
            else
            {
                throw new Exception("Fatal Error : Cannot locate Root DataList for resumption!");
            }
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


        #region Protected Methods

        protected IWarewolfIterator CreateDataListEvaluateIterator(string expression, IExecutionEnvironment executionEnvironment)
        {
            var evalled = executionEnvironment.Eval(expression);
//            if(ExecutionEnvironment.IsNothing(evalled))
//                throw  new Exception("Invalid variable: "+expression);
            var expressionIterator = new WarewolfIterator(evalled);
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
