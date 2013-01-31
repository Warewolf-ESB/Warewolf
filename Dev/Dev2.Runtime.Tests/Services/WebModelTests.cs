using System;
using Dev2.Common;
using Dev2.Data.SystemTemplates.Models;
using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.Runtime.Services;
using Dev2.Runtime.Services.Data;
using System.Collections.Generic;

namespace Dev2.Tests.Runtime.Dev2.Runtime.Services.Tests
{

    // Sashen : 31-01-2012 : Testing Feedback
    // Tests Missing an Author comment
    // Missing Tests:
    // Fetching a Decision Model with a Null DataList (Guid.Empty)
    // Fetching a Switch Decision Model - all tests
    // Fetching a switch case decision model tests
    // No test cases for SaveModel

    [TestClass]
    public class WebModelTests
    {
        [TestMethod]
        public void FetchDecisionModel_Expected_WebModel()
        {
            var testWebModel = new WebModel();

            IDev2DataModel testModel = new Dev2Decision();

            var stack = new Dev2DecisionStack { TheStack = new List<Dev2Decision> { new Dev2Decision() } };

            string expected = stack.ToWebModel();
            string actual = testWebModel.FetchDecisionModel("", Guid.Empty, generateGuid());

            Assert.AreEqual(expected, actual);
        }

        #region private method region

        private Guid generateGuid()
        {
            IDataListCompiler testCompiler = DataListFactory.CreateDataListCompiler();

            var stack = new Dev2DecisionStack();
            stack.TheStack = new List<Dev2Decision> { new Dev2Decision() };

            var error = new ErrorResultTO();
            Guid MyModel = testCompiler.PushSystemModelToDataList(stack, out error);

            return MyModel;
        }

        #endregion
    }
}
