
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Diagnostics.CodeAnalysis;
using Dev2.Data.SystemTemplates.Models;
using Dev2.DataList.Contract;
using Dev2.Runtime.ServiceModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Dev2.Data.SystemTemplates;

namespace Dev2.Tests.Runtime.ServiceModel
{

    // Sashen : 31-01-2012 : Testing Feedback
    // Tests Missing an Author comment
    // Missing Tests:
    // Fetching a Decision Model with a Null DataList (Guid.Empty) - Fixed
    // Fetching a Switch Decision Model - all tests - Fixed
    // Fetching a switch case decision model tests - Un-used have commented it out
    // No test cases for SaveModel - Fixed

    [TestClass]
    [ExcludeFromCodeCoverage]
    public class WebModelTests
    {
        /// <summary>
        /// Travis.Frisinger - Fetches the decision model as a web model
        /// </summary>
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



        /// <summary>
        /// Travis.Frisinger - Fetches the decision model with invalid DataList ID
        /// </summary>
        [TestMethod]
        public void FetchDecisionModel_Expected_EmptyJsonString()
        {
            var testWebModel = new WebModel();

            IDev2DataModel testModel = new Dev2Decision();

            var stack = new Dev2DecisionStack { TheStack = new List<Dev2Decision> { new Dev2Decision() } };

            string expected = "{}";
            string actual = testWebModel.FetchDecisionModel("", Guid.Empty, Guid.Empty);

            Assert.AreEqual(expected, actual);
        }


        /// <summary>
        /// Travis.Frisinger - Fetch a switch model as a web model
        /// </summary>
        [TestMethod]
        public void FetchSwitchModel_Expected_WebModel()
        {
            var testWebModel = new WebModel();
            string expected = DataListConstants.DefaultSwitch.ToWebModel();

            string actual = testWebModel.FetchSwitchExpression(expected, Guid.Empty, generateGuidSwitch());

            Assert.AreEqual(expected, actual);

        }

        /// <summary>
        /// Travis.Frisinger - Fetch a switch model with an invalid DataList ID 
        /// </summary>
        [TestMethod]
        public void FetchSwitchModel_Expected_EmptyJson()
        {
            var testWebModel = new WebModel();
            string expected = DataListConstants.DefaultSwitch.ToWebModel();

            string actual = testWebModel.FetchSwitchExpression(expected, Guid.Empty, Guid.Empty);

            Assert.AreEqual("{}", actual);

        }

        /// <summary>
        /// Travis.Frisinger - Can it save a web model to the DataList
        /// </summary>
        [TestMethod]
        public void SaveModelData_Expected_SavedMessage()
        {
            var testWebModel = new WebModel();

            string actual = testWebModel.SaveModel("Dummy Model Data", Guid.Empty, generateGuidSwitch());

            string expected = "{  \"message\" : \"Saved Model\"} ";

            Assert.AreEqual(expected, actual);

        }


        /// <summary>
        /// Travis.Frisinger - Can it return error message with invalid DataList ID
        /// </summary>
        [TestMethod]
        public void SaveModelData_Null_DataListID_Expected_ErrorMessage()
        {
            var testWebModel = new WebModel();

            string actual = testWebModel.SaveModel("Dummy Model Data", Guid.Empty, Guid.Empty);

            string expected = "{ \"message\" : \"Error Saving Model\"} ";

            Assert.AreEqual(expected, actual);

        }


        #region private method region

        private Guid generateGuid()
        {
            IDataListCompiler testCompiler = DataListFactory.CreateDataListCompiler();

            var stack = new Dev2DecisionStack();
            stack.TheStack = new List<Dev2Decision> { new Dev2Decision() };

            ErrorResultTO error;
            Guid MyModel = testCompiler.PushSystemModelToDataList(stack, out error);

            return MyModel;
        }


        private Guid generateGuidSwitch()
        {
            IDataListCompiler testCompiler = DataListFactory.CreateDataListCompiler();

            var stack = new Dev2Switch() { SwitchVariable = "" };

            ErrorResultTO error;
            Guid MyModel = testCompiler.PushSystemModelToDataList(stack, out error);

            return MyModel;
        }

        #endregion
    }
}
