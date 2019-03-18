#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Data;
using Dev2.Data.TO;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.Diagnostics.Debug;
using Dev2.Interfaces;
using Microsoft.VisualBasic.Activities;
using Newtonsoft.Json;
using Warewolf.Resource.Errors;
using Warewolf.Storage.Interfaces;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    public abstract class DsfActivityAbstract<T> : DsfNativeActivity<T>, IActivityTemplateFactory, INotifyPropertyChanged, IEquatable<DsfActivityAbstract<T>>
    {
        public string SimulationOutput { get; set; }

        [JsonIgnore]
        public OutArgument<bool> HasError { get; set; }
        [JsonIgnore]
        public OutArgument<bool> IsValid { get; set; }
        [JsonIgnore]
        public InArgument<string> ExplicitDataList { get; set; }
        [JsonIgnore]
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
        [JsonIgnore]
        public InOutArgument<string> ParentInstanceID { get; set; }
        public IRecordsetScopingObject ScopingObject { get => null; set => value = null; }

        #region Ctor

        protected DsfActivityAbstract()
            : this(null)
        {
        }

        protected DsfActivityAbstract(string displayName)
            : this(displayName, false)
        {
        }

        protected DsfActivityAbstract(string displayName, bool isAsync)
            : this(displayName, DebugDispatcher.Instance, isAsync)
        {
        }

        protected DsfActivityAbstract(string displayName, IDebugDispatcher debugDispatcher, bool isAsync)
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


        public Activity Create(DependencyObject target) => this;

        public virtual void Resumed(NativeActivityContext context, Bookmark bookmark, object value)
        {

            var myDO = context.GetExtension<IDSFDataObject>();
            var errorResultTO = new ErrorResultTO();
            var executionID = myDO.DataListID;

            if (value != null)
            {
                Guid.TryParse(value.ToString(), out Guid rootID);

                if (executionID == rootID)
                {
                    throw new Exception(ErrorResource.SameParentAndChildDataListId);
                }
                if (errorResultTO.HasErrors())
                {
                    DisplayAndWriteError("Resumption", errorResultTO);
                    var errorString = errorResultTO.MakeDataListReady();
                    myDO.Environment.AddError(errorString);
                }
            }
            else
            {
                throw new Exception(ErrorResource.CannotLocateRootDataList);
            }
        }

        #region INotifyPropertyChnaged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }

        #endregion INotifyPropertyChnaged


        #region Protected Methods

        protected IWarewolfIterator CreateDataListEvaluateIterator(string expression, IExecutionEnvironment executionEnvironment, int update)
        {
            var evalled = executionEnvironment.Eval(expression, update);
            var expressionIterator = new WarewolfIterator(evalled);
            return expressionIterator;
        }

        protected void ValidateRecordsetName(string recordsetName, ErrorResultTO errors)
        {
            if (string.IsNullOrEmpty(recordsetName))
            {
                errors.AddError(ErrorResource.NoRecordSet);
            }

            if (DataListCleaningUtils.SplitIntoRegions(recordsetName).Count > 1)
            {
                errors.AddError(ErrorResource.OneVariableAccepted);
            }
            else
            {
                if (DataListUtil.IsValueRecordset(recordsetName))
                {
                    if (DataListUtil.IsValueRecordsetWithFields(recordsetName))
                    {
                        errors.AddError(ErrorResource.RequiredRecordSetNameONLY);
                    }

                }
                else
                {
                    errors.AddError(ErrorResource.RequiredRecordSetName);
                }
            }
        }

        #endregion Protected Methods

        public bool Equals(DsfActivityAbstract<T> other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return base.Equals(other)
                   && string.Equals(UniqueID, other.UniqueID)
                   && string.Equals(DisplayName, other.DisplayName);

        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (UniqueID != null ? UniqueID.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (DisplayName != null ? DisplayName.GetHashCode() : 0);
             
                return hashCode;
            }
        }
    }
}
