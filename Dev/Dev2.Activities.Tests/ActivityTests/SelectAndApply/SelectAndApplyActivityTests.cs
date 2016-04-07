using Dev2.Activities.SelectAndApply;
using Microsoft.VisualStudio.TestTools.UnitTesting;
// ReSharper disable InconsistentNaming

namespace Dev2.Tests.Activities.ActivityTests.SelectAndApply
{
    [TestClass]
    public class SelectAndApplyActivityTests
    {
        private DsfSelectAndApplyActivity CreateActivity()
        {
            return new DsfSelectAndApplyActivity();
        }
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("SelectAndApplyActivity_Construct")]
        public void SelectAndApplyActivity_Construct_GivenInstance_ShouldNotBeNull()
        {
            //------------Setup for test--------------------------
            var selectAndApplyActivity = CreateActivity();
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.IsNotNull(selectAndApplyActivity);
        }
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("SelectAndApplyActivity_DisplayName")]
        public void SelectAndApplyActivity_DisplayName_GivenIsCreated_ShouldBeSelectAndApply()
        {
            //------------Setup for test--------------------------
            var selectAndApplyActivity = CreateActivity();
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.IsNotNull(selectAndApplyActivity);
            Assert.AreEqual("Select and apply", selectAndApplyActivity.DisplayName);
        }
    }
}
