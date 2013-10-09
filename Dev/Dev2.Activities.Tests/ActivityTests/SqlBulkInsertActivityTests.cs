using System.Activities.Statements;
using System.Collections.Generic;
using System.Data.SqlClient;
using ActivityUnitTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities.ActivityTests
{

    [TestClass]
    public class SqlBulkInsertActivityTests : BaseActivityUnitTest
    {
        public TestContext TestContext { get; set; }

        
        #region Private Test Methods

        private void SetupArguments(string currentDL, string testData, string sourceString, IList<XPathDTO> resultCollection)
        {
            TestStartNode = new FlowStep
            {
                //Action = new DsfXPathActivity { SourceString = sourceString, ResultsCollection = resultCollection }
            };

            CurrentDl = testData;
            TestData = currentDL;
        }

        #endregion Private Test Methods
    }
}