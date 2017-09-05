using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities.ActivityComparerTests.Assigns

{
    [TestClass]
    public class MultiAssignComparerTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void SetCredentialOnHandler_GivenHandlerIsNotNull_ShouldReturnHttpHandler()
        {
            //---------------Set up test pack-------------------
            DsfMultiAssignActivity multiAssign = new DsfMultiAssignActivity();
            //---------------Assert Precondition----------------
            Assert.IsInstanceOfType(multiAssign,typeof(IEqualityComparer<>));
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
        }
    }
}
