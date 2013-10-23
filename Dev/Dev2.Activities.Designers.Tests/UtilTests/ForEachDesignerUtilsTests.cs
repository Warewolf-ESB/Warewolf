using Dev2.Activities.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;using System.Diagnostics.CodeAnalysis;

namespace Dev2.Core.Tests.Activities
{
    [TestClass, System.Runtime.InteropServices.GuidAttribute("2B16F3E1-F449-4F4F-BFF6-E6A489C09B10")]
    public class ForEachDesignerUtilsTests
    {
        [TestMethod]
        [TestCategory("ForEachUtils_UnitTest")]
        [Description("ForEach ForeachDropPointOnDragEnter util prevents decisions from being fropped into a foreach")]
        [Owner("Ashley Lewis")]
        // ReSharper disable InconsistentNaming
        public void ForEach_DropPointOnDragEnter_Decision_HandledSetToTrue()
        // ReSharper restore InconsistentNaming
        {
            //init
            var util = new ForeachActivityDesignerUtils();

            //exe
            var actual = util.ForeachDropPointOnDragEnter("Decision");

            //assert
            Assert.IsFalse(actual, "ForEach util allowed a decision to be dropped");
        }

        [TestMethod]
        [TestCategory("ForEachUtils_UnitTest")]
        [Description("ForEach ForeachDropPointOnDragEnter util prevents switches from being fropped into a foreach")]
        [Owner("Ashley Lewis")]
        // ReSharper disable InconsistentNaming
        public void ForEach_DropPointOnDragEnter_Switch_HandledSetToTrue()
        // ReSharper restore InconsistentNaming
        {
            //init
            var util = new ForeachActivityDesignerUtils();

            //exe
            var actual = util.ForeachDropPointOnDragEnter("Switch");

            //assert
            Assert.IsFalse(actual, "ForEach util allowed a switch to be dropped");
        }
    }
}
