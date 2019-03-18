#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using Dev2.Activities;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.State;
using Dev2.Data.Util;
using Dev2.Diagnostics;
using Dev2.Interfaces;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Storage.Interfaces;
using WarewolfParserInterop;



namespace Dev2
{
    public class TestMockStep : DsfActivityAbstract<string>
    {
        readonly IDev2Activity _originalActivity;

        public TestMockStep()
        {
            DisplayName = "Mock Step";
        }

        public override IEnumerable<StateVariable> GetState()
        {
            //This Activity is only used as part of the Warewolf Test Exection Framework and is not used in normal exeuction.
            return new StateVariable[0];
        }

        public TestMockStep(IDev2Activity originalActivity , List<IServiceTestOutput> outputs)
        {
            _originalActivity = originalActivity;
            Outputs = outputs;
            Act = originalActivity as DsfNativeActivity<string>;
            if (Act != null)
            {
                DisplayName = Act.DisplayName;
                UniqueID = Act.UniqueID;
                ActualTypeName = originalActivity.GetType().Name;
            }

        }

        public DsfNativeActivity<string> Act { get; }
        public List<IServiceTestOutput> Outputs { get; }
        public string ActualTypeName { get; private set; }

        public override List<string> GetOutputs() => new List<string>();

        #region Overrides of DsfNativeActivity<string>

        protected override void OnExecute(NativeActivityContext context)
        {
        }

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates)
        {
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates)
        {
        }

        public override IList<DsfForEachItem> GetForEachInputs() => null;

        public override IList<DsfForEachItem> GetForEachOutputs() => null;

        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
        {
            if (dataObject.IsDebugMode())
            {
                InitializeDebug(dataObject);
            }
            AddRecordsetsOutputs(Outputs.Where(output => DataListUtil.IsValueRecordset(output.Variable) && !output.Variable.Contains("@")), dataObject.Environment);
            foreach (var output in Outputs)
            {
                if (!string.IsNullOrEmpty(output.Variable)) {
                    var variable = DataListUtil.AddBracketsToValueIfNotExist(output.Variable);
                    var value = output.Value;
                    if (variable.StartsWith("[[@"))
                    {
                        dataObject.Environment.AssignJson(new AssignValue(variable, value), update);
                    }
                    else
                    {
                        AssignNotJson(dataObject, output, variable, value);
                    }
                    if (dataObject.IsServiceTestExecution && dataObject.IsDebugMode())
                    {
                        var res = new DebugEvalResult(dataObject.Environment.ToStar(variable), "", dataObject.Environment, update, false, false, true);
                        AddDebugOutputItem(new DebugEvalResult(variable, "", dataObject.Environment, update));
                        AddDebugAssertResultItem(res);
                    }

                }
            }
            if (dataObject.IsDebugMode())
            {
                DispatchDebugState(dataObject, StateType.After, update);
            }
            NextNodes = _originalActivity.NextNodes;
        }

        private static void AssignNotJson(IDSFDataObject dataObject, IServiceTestOutput output, string variable, string value)
        {
            if (!DataListUtil.IsValueRecordset(output.Variable))
            {
                dataObject.Environment.Assign(variable, value, 0);
            }
        }

        #region Overrides of DsfNativeActivity<string>

        public override List<DebugItem> GetDebugOutputs(IExecutionEnvironment env, int update)
        {
            foreach (IDebugItem debugOutput in _debugOutputs)
            {
                debugOutput.FlushStringBuilder();
            }
            return _debugOutputs;
        }

        #endregion

        static void AddRecordsetsOutputs(IEnumerable<IServiceTestOutput> recSets, IExecutionEnvironment environment)
        {
            if (recSets != null)
            {
                var groupedRecsets = recSets.GroupBy(item => DataListUtil.ExtractRecordsetNameFromValue(item.Variable));
                foreach (var groupedRecset in groupedRecsets)
                {
                    AddEntireRecsetGroup(environment, groupedRecset);
                }
            }
        }

        private static void AddEntireRecsetGroup(IExecutionEnvironment environment, IGrouping<string, IServiceTestOutput> groupedRecset)
        {
            var dataListItems = groupedRecset.GroupBy(item => DataListUtil.ExtractIndexRegionFromRecordset(item.Variable));
            foreach (var dataListItem in dataListItems)
            {
                var recSetsToAssign = new List<IServiceTestOutput>();
                var empty = true;
                foreach (var listItem in dataListItem)
                {
                    if (!string.IsNullOrEmpty(listItem.Value))
                    {
                        empty = false;
                    }
                    recSetsToAssign.Add(listItem);
                }
                if (!empty)
                {
                    foreach (var serviceTestInput in recSetsToAssign)
                    {
                        environment.Assign(DataListUtil.AddBracketsToValueIfNotExist(serviceTestInput.Variable), serviceTestInput.Value, 0);
                    }
                }
            }
        }

        public bool Equals(TestMockStep other)
        {
            return ReferenceEquals(this, other);
        }
        public override bool Equals(object obj)
        {
            if (obj is TestMockStep instance)
            {
                return Equals(instance);
            }
            return false;
        }

        #endregion
    }
}