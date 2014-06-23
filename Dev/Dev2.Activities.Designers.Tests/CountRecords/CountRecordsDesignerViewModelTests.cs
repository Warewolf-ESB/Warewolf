using System.Activities.Presentation.Model;
using Dev2.Studio.Core.Activities.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Designers.Tests.CountRecords
{
    [TestClass]    
    public class CountRecordsDesignerViewModelTests
    {
        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("CountRecordsDesignerViewModel_SetRecordsetNameValue")]
        public void CountRecordsDesignerViewModel_SetRecordsetNameValue_ModelItemIsValid_RecordSetOnModelItemIsSet()
        {
            var modelItem = CreateModelItem();
            var viewModel = new TestCountRecordsDesignerViewModel(modelItem);
            const string ExcpectedVal = "[[Table_Records()]]";
            viewModel.RecordsetNameValue = ExcpectedVal;
            Assert.AreEqual(ExcpectedVal, viewModel.RecordsetName);
        }
        
        static ModelItem CreateModelItem()
        {
            return ModelItemUtils.CreateModelItem(new DsfCountRecordsetActivity());
        }
    }
}