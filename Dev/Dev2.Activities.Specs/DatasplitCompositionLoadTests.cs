
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dev2.DynamicServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Specs
{
    [TestClass]
    public class DatasplitCompositionLoadTests
    {
        protected string CurrentDl { get; set; }

        [TestMethod]
        [DeploymentItem("LargeRowsDataSplit.txt")]
        [TestCategory("CompositionLoadTests")]
        public void LargeRows_SplitOnNewLine_ShouldSplitCorrectly()
        {
            IList<DataSplitDTO> resultsCollection = new List<DataSplitDTO>();
            resultsCollection.Add(new DataSplitDTO("[[rec().data]]", "New Line", "", 1));
            var sourceString = "";
            if (File.Exists("LargeRowsDataSplit.txt"))
            {
                sourceString = File.ReadAllText("LargeRowsDataSplit.txt");
            }
            else if (File.Exists("Out\\LargeRowsDataSplit.txt"))
            {
                sourceString = File.ReadAllText("Out\\LargeRowsDataSplit.txt");
            }
            Assert.IsFalse(string.IsNullOrEmpty(sourceString), "Cannot find Deployment Item LargeRowsDataSplit.txt");
            var act = new DsfDataSplitActivity { SourceString = sourceString, ResultsCollection = resultsCollection, SkipBlankRows = true };
            var dataObject = new DsfDataObject("", Guid.Empty)
            {
                IsDebug = false,
            };
            act.Execute(dataObject, 0);

            var totalCount = dataObject.Environment.GetCount("rec");
            var res = dataObject.Environment.Eval("[[rec().data]]", 0) as CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult;
            Assert.AreEqual("0827373254", res.Item.First().ToString());
            Assert.AreEqual(8300000, totalCount, CurrentDl);
        }
    }
}
