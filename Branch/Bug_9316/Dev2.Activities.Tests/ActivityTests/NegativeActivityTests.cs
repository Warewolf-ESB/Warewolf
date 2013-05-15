using System;
using System.Activities;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ActivityUnitTests;
using ActivityUnitTests.ActivityTest;
using ActivityUnitTests.ActivityTests;
using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.Tests.Activities;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities.ActivityTests
{
    [TestClass]
    public class NegativeActivityTests : BaseActivityUnitTest
    {

/*      Bug 7821: Throws Exception but its swallowed
        [TestMethod]
        [ExpectedException(typeof(NotImplementedException))]
        public void HttpActivityExecute_Expected_NotImplementedException()
        {
            SetupArguments();
            IDSFDataObject result = ExecuteProcess();
        }
*/
        #region Private Test Methods
         
        private void SetupArguments()
        {
            TestStartNode = new FlowStep
            {
                Action = new DsfHttpActivity {  }
            };
            TestData = "";
            CurrentDl = "<ADL></ADL>";
        }

        #endregion
    }
}
