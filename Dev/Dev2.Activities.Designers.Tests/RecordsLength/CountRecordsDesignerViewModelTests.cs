using Dev2.Studio.Core.Activities.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Activities.Presentation.Model;
using System.Diagnostics.CodeAnalysis;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Designers.Tests.RecordsLength
{
    [TestClass]    
    // ReSharper disable InconsistentNaming
    public class RecordsLengthDesignerViewModelTests
    {
        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("RecordsLengthDesignerViewModel_SetRecordsetNameValue")]
        public void RecordsLengthDesignerViewModel_SetRecordsetNameValue_ModelItemIsValid_RecordSetOnModelItemIsSet()
        {
            var modelItem = CreateModelItem();
            var viewModel = new TestRecordsLengthDesignerViewModel(modelItem);
            const string ExcpectedVal = "[[Table_Records()]]";
            viewModel.RecordsetNameValue = ExcpectedVal;
            Assert.AreEqual(ExcpectedVal, viewModel.RecordsetName);
        }

        static ModelItem CreateModelItem()
        {
            return ModelItemUtils.CreateModelItem(new DsfRecordsetLengthActivity());
        }
    }
}