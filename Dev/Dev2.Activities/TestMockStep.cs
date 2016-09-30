using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using Dev2.Activities;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Data.Util;
using Dev2.Interfaces;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Storage;
using WarewolfParserInterop;

// ReSharper disable UnusedMember.Global

namespace Dev2
{
    public class TestMockStep : DsfActivityAbstract<string>
    {
        private readonly IDev2Activity _originalActivity;
        private readonly List<IServiceTestOutput> _outputs;

        public TestMockStep()
        {
            DisplayName = "Mock Step";
        }

        public TestMockStep(IDev2Activity originalActivity , List<IServiceTestOutput> outputs)
        {
            _originalActivity = originalActivity;
            _outputs = outputs;
            var act = originalActivity as DsfNativeActivity<string>;
            if (act != null)
            {
                DisplayName = act.DisplayName;
            }

        }

        public override List<string> GetOutputs()
        {
            return new List<string>();
        }

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

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            return null;
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            return null;
        }

        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
        {
            InitializeDebug(dataObject);
            AddRecordsetsOutputs(_outputs.Where(output => DataListUtil.IsValueRecordset(output.Variable) && !output.Variable.Contains("@")), dataObject.Environment);
            foreach (var output in _outputs)
            {
                var variable = DataListUtil.AddBracketsToValueIfNotExist(output.Variable);
                var value = output.Value;
                if (variable.StartsWith("[[@"))
                {
                    dataObject.Environment.AssignJson(new AssignValue(variable,value),update);
                }
                else if (!DataListUtil.IsValueRecordset(output.Variable))
                {
                    dataObject.Environment.Assign(variable, value, 0);
                }
                var res = new DebugEvalResult(dataObject.Environment.ToStar(variable), "", dataObject.Environment, update);                
                //AddDebugOutputItem(res);
                if (dataObject.IsServiceTestExecution)
                {
                    AddDebugAssertResultItem(res);
                }
            }
            if (dataObject.IsDebugMode())
            {
                DispatchDebugState(dataObject, StateType.After, update);
            }
            NextNodes = _originalActivity.NextNodes;
        }


        private static void AddRecordsetsOutputs(IEnumerable<IServiceTestOutput> recSets, IExecutionEnvironment environment)
        {
            if(recSets != null)
            {
                var groupedRecsets = recSets.GroupBy(item => DataListUtil.ExtractRecordsetNameFromValue(item.Variable));
                foreach (var groupedRecset in groupedRecsets)
                {
                    var dataListItems = groupedRecset.GroupBy(item => DataListUtil.ExtractIndexRegionFromRecordset(item.Variable));
                    foreach (var dataListItem in dataListItems)
                    {
                        List<IServiceTestOutput> recSetsToAssign = new List<IServiceTestOutput>();
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
            }
        }

        #endregion
    }
}