using Dev2.Common;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Diagnostics;
using System;
using System.Activities;
using System.Collections.Generic;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities
{
    public class TestNativeActivity : DsfNativeActivity<string>
    {
        #region CTOR

        public TestNativeActivity(IDebugDispatcher debugDispatcher)
            : base(false, "TestNativeActivity", debugDispatcher)
        {
            InstanceID = "InstanceID";
            IsWorkflow = true;
            IsSimulationEnabled = false;
        }

        #endregion

        #region DsfNativeActivity implementation

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
        }

        protected override void OnExecute(NativeActivityContext context)
        {
        }

        #endregion

        public new IList<IDebugItem> GetDataListItems(IBinaryDataList dataList, bool includeIndexInRecordSetName = false)
        {
            return base.GetDataListItems(dataList, includeIndexInRecordSetName);
        }

        public static IBinaryDataList CreateBinaryDataList(string shape, string testData)
        {
            var compiler = DataListFactory.CreateDataListCompiler();
            ErrorResultTO errors;
            var dataListID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), testData, shape, out errors);
            return compiler.FetchBinaryDataList(dataListID, out errors);
        }
    }
}
