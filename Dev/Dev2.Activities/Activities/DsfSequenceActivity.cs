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
using System.Activities;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Diagnostics;
using Dev2.Interfaces;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Core;
using Warewolf.Resource.Messages;
using Warewolf.Storage.Interfaces;
using Dev2.Comparer;

namespace Dev2.Activities
{

    [ToolDescriptorInfo("ControlFlow-Sequence", "Sequence", ToolType.Native, "8999E59A-38A3-43BB-A98F-6090C5C9EA1E", "Dev2.Acitivities", "1.0.0.0", "Legacy", "Control Flow", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_Flow_Sequence")]
    public class DsfSequenceActivity : DsfActivityAbstract<string>,IEquatable<DsfSequenceActivity>
    {
        private readonly Sequence _innerSequence = new Sequence();
        string _previousParentID;
        private Guid _originalUniqueID;

        public DsfSequenceActivity()
        {
            DisplayName = "Sequence";
            Activities = new Collection<Activity>();
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);
            metadata.AddChild(_innerSequence);
        }


        public Collection<Activity> Activities
        {
            get;
            set;
        }

        public override List<string> GetOutputs()
        {
            return new List<string>();
        }
        #region Get Debug Inputs/Outputs

        public override List<DebugItem> GetDebugOutputs(IExecutionEnvironment dataList, int update)
        {
            return DebugItem.EmptyList;
        }

        #endregion Get Inputs/Outputs

        #region GetForEachInputs/Outputs

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates)
        {
            foreach(var activity in Activities)
            {
                var innerActivity = activity as DsfActivityAbstract<string>;
                if(innerActivity != null)
                {
                    innerActivity.UpdateForEachInputs(updates);
                }
            }
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates)
        {
            foreach(var activity in Activities)
            {
                var innerActivity = activity as DsfActivityAbstract<string>;
                if(innerActivity != null)
                {
                    innerActivity.UpdateForEachOutputs(updates);
                }
            }
        }

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            var forEachInputs = new List<DsfForEachItem>();

            foreach(var activity in Activities)
            {
                var innerActivity = activity as DsfActivityAbstract<string>;
                if(innerActivity != null)
                {
                    forEachInputs.AddRange(innerActivity.GetForEachInputs());
                }
            }

            return forEachInputs;
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            var forEachOutputs = new List<DsfForEachItem>();

            foreach(var activity in Activities)
            {
                var innerActivity = activity as DsfActivityAbstract<string>;
                if(innerActivity != null)
                {
                    forEachOutputs.AddRange(innerActivity.GetForEachOutputs());
                }
            }

            return forEachOutputs;
        }

        #endregion

        #region Overrides of DsfNativeActivity<string>

        protected override void OnBeforeExecute(NativeActivityContext context)
        {
            var dataObject = context.GetExtension<IDSFDataObject>();
            _previousParentID = dataObject.ParentInstanceID;
        }
        public override void UpdateDebugParentID(IDSFDataObject dataObject)
        {
            var isNestedForEach = dataObject.ForEachNestingLevel > 0;
            if (!isNestedForEach || _originalUniqueID == Guid.Empty)
            {
                _originalUniqueID = Guid.Parse(UniqueID);
            }
            if (!isNestedForEach && _originalUniqueID != Guid.Empty)
            {
                UniqueID = _originalUniqueID.ToString();
            }
            WorkSurfaceMappingId = Guid.Parse(UniqueID);
            UniqueID = isNestedForEach ? Guid.NewGuid().ToString() : UniqueID;
        }
        /// <summary>
        /// When overridden runs the activity's execution logic 
        /// </summary>
        /// <param name="context">The context to be used.</param>
        protected override void OnExecute(NativeActivityContext context)
        {
            IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();
            dataObject.ForEachNestingLevel++;
            InitializeDebug(dataObject);
            if (dataObject.IsDebugMode())
            {
                DispatchDebugState(dataObject, StateType.Before, 0);
            }
            dataObject.ParentInstanceID = UniqueID;
            dataObject.IsDebugNested = true;
            _innerSequence.Activities.Clear();
            foreach (var dsfActivity in Activities)
            {
                _innerSequence.Activities.Add(dsfActivity);
            }

            if (dataObject.IsDebugMode())
            {
                DispatchDebugState(dataObject, StateType.After, 0);
            }
        }

        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
        {
            _previousParentID = dataObject.ParentInstanceID;
            InitializeDebug(dataObject);
            dataObject.ForEachNestingLevel++;
            if (dataObject.IsDebugMode())
            {
                DispatchDebugState(dataObject, StateType.Before, update);
            }
            dataObject.ParentInstanceID = UniqueID;
            dataObject.IsDebugNested = true;
            if (dataObject.IsDebugMode())
            {
                DispatchDebugState(dataObject, StateType.After, update);
            }
            if (dataObject.IsServiceTestExecution)
            {
                if (_originalUniqueID == Guid.Empty)
                {
                    _originalUniqueID = Guid.Parse(UniqueID);
                }
            }
            var serviceTestStep = dataObject.ServiceTest?.TestSteps?.Flatten(step => step.Children)?.FirstOrDefault(step => step.UniqueId == _originalUniqueID);
            var serviceTestSteps = serviceTestStep?.Children;
            foreach (var dsfActivity in Activities)
            {
                var act = dsfActivity as IDev2Activity;
                if (act != null)
                {
                    act.Execute(dataObject, update);
                    if (dataObject.IsServiceTestExecution)
                    {
                        var contentId = Guid.Parse(act.UniqueID);
                        if (dsfActivity.GetType().Name == "DsfActivity")
                        {
                            var newAct = dsfActivity as DsfActivity;
                            if (newAct != null)
                            {
                                contentId = newAct.GetWorkSurfaceMappingId();
                            }
                        }
                        UpdateDebugStateWithAssertions(dataObject, serviceTestSteps?.ToList(), contentId);
                    }
                }
            }
            if (dataObject.IsServiceTestExecution)
            {
                if (serviceTestStep != null)
                {
                    var testRunResult = new TestRunResult();
                    GetFinalTestRunResult(serviceTestStep, testRunResult);
                    serviceTestStep.Result = testRunResult;
                }
            }
            OnCompleted(dataObject);
            if (dataObject.IsDebugMode())
            {
                _debugOutputs = new List<DebugItem>();
                DispatchDebugState(dataObject, StateType.Duration, update);
            }
        }

        private static void GetFinalTestRunResult(IServiceTestStep serviceTestStep, TestRunResult testRunResult)
        {
            ObservableCollection<TestRunResult> resultList = new ObservableCollection<TestRunResult>();
            foreach (var testStep in serviceTestStep.Children)
            {
                if (testStep.Result != null)
                {
                    resultList.Add(testStep.Result);
                }
            }

            if (resultList.Count == 0)
            {
                testRunResult.RunTestResult = RunResult.TestPassed;
            }
            else
            {
                testRunResult.RunTestResult = RunResult.TestInvalid;

                var testRunResults = resultList.Where(runResult => runResult.RunTestResult == RunResult.TestInvalid).ToList();
                if (testRunResults.Count>0)
                {
                    testRunResult.Message = string.Join(Environment.NewLine, testRunResults.Select(result => result.Message));
                    testRunResult.RunTestResult = RunResult.TestInvalid;
                }
                else
                {
                    var passed = resultList.All(runResult => runResult.RunTestResult == RunResult.TestPassed);
                    if (passed)
                    {
                        testRunResult.Message = Messages.Test_PassedResult;
                        testRunResult.RunTestResult = RunResult.TestPassed;
                    }
                    else
                    {
                        testRunResult.Message = Messages.Test_FailureResult;
                        testRunResult.RunTestResult = RunResult.TestFailed;
                    }
                }
            }
        }

        void OnCompleted(IDSFDataObject dataObject)
        {
            dataObject.IsDebugNested = false;
            dataObject.ParentInstanceID = _previousParentID;
            dataObject.ForEachNestingLevel--;
        }

        public override enFindMissingType GetFindMissingType()
        {
            return enFindMissingType.Sequence;
        }

        #endregion

        private void UpdateDebugStateWithAssertions(IDSFDataObject dataObject, List<IServiceTestStep> serviceTestTestSteps, Guid childId)
        {
            ServiceTestHelper.UpdateDebugStateWithAssertions(dataObject, serviceTestTestSteps, childId.ToString());
            
        }

        public bool Equals(DsfSequenceActivity other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) 
                && CommonEqualityOps.CollectionEquals(Activities, other.Activities, new ActivityComparer());
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DsfSequenceActivity) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (_innerSequence != null ? _innerSequence.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_previousParentID != null ? _previousParentID.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ _originalUniqueID.GetHashCode();
                hashCode = (hashCode * 397) ^ (Activities != null ? Activities.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
