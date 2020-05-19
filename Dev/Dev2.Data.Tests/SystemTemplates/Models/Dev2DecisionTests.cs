/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Data.Decisions.Operations;
using Dev2.Data.SystemTemplates.Models;
using Dev2.Data.TO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using Warewolf.Storage.Interfaces;

namespace Dev2.Data.Tests.SystemTemplates.Models
{
    [TestClass]
    public class Dev2DecisionTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2Decision))]
        public void Dev2Decision_PopulatedColumnCount_Default()
        {
            var dev2Decision = new Dev2Decision();

            Assert.AreEqual(0, dev2Decision.PopulatedColumnCount);
            Assert.IsNull(dev2Decision.Col1);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2Decision))]
        public void Dev2Decision_PopulatedColumnCount_Col1()
        {
            var dev2Decision = new Dev2Decision
            {
                Col1 = "[[a]]"
            };

            Assert.AreEqual(1, dev2Decision.PopulatedColumnCount);
            Assert.AreEqual("[[a]]", dev2Decision.Col1);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2Decision))]
        public void Dev2Decision_PopulatedColumnCount_Col2()
        {
            var dev2Decision = new Dev2Decision
            {
                Col2 = "="
            };

            Assert.AreEqual(1, dev2Decision.PopulatedColumnCount);
            Assert.AreEqual("=", dev2Decision.Col2);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2Decision))]
        public void Dev2Decision_PopulatedColumnCount_Col3()
        {
            var dev2Decision = new Dev2Decision
            {
                Col3 = "bob"
            };

            Assert.AreEqual(1, dev2Decision.PopulatedColumnCount);
            Assert.AreEqual("bob", dev2Decision.Col3);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2Decision))]
        public void Dev2Decision_PopulatedColumnCount_AllColumns()
        {
            var dev2Decision = new Dev2Decision
            {
                Col1 = "[[a]]",
                Col2 = "=",
                Col3 = "bob"
            };

            Assert.AreEqual(3, dev2Decision.PopulatedColumnCount);
            Assert.AreEqual("[[a]]", dev2Decision.Col1);
            Assert.AreEqual("=", dev2Decision.Col2);
            Assert.AreEqual("bob", dev2Decision.Col3);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2Decision))]
        public void Dev2Decision_GenerateUserFriendlyModel_PopulatedColumnCount_Zero()
        {
            var dev2Decision = new Dev2Decision
            {
                EvaluationFn = enDecisionType.IsEqual
            };

            var mockExecutionEnvironment = new Mock<IExecutionEnvironment>();

            var result = dev2Decision.GenerateToolLabel(mockExecutionEnvironment.Object, Dev2DecisionMode.AND, out var error);

            Assert.AreEqual("If = ", result);
            Assert.AreEqual(0, error.FetchErrors().Count);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2Decision))]
        public void Dev2Decision_GenerateUserFriendlyModel_PopulatedColumnCount_One_Scalar()
        {
            var dev2Decision = new Dev2Decision
            {
                Col1 = "[[field]]",
                EvaluationFn = enDecisionType.IsBetween
            };

            var mockExecutionEnvironment = new Mock<IExecutionEnvironment>();

            var result = dev2Decision.GenerateToolLabel(mockExecutionEnvironment.Object, Dev2DecisionMode.AND, out var error);

            Assert.AreEqual("If [[field]] Is Between ", result);
            Assert.AreEqual(0, error.FetchErrors().Count);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2Decision))]
        public void Dev2Decision_GenerateUserFriendlyModel_PopulatedColumnCount_One_Recordset()
        {
            var dev2Decision = new Dev2Decision
            {
                Col1 = "[[recset(*).field]]",
                EvaluationFn = enDecisionType.IsBetween
            };

            var mockExecutionEnvironment = new Mock<IExecutionEnvironment>();
            mockExecutionEnvironment.Setup(env => env.EvalAsListOfStrings(It.IsAny<string>(), It.IsAny<int>())).Returns(new List<string> { "[[a]]" });
            var result = dev2Decision.GenerateToolLabel(mockExecutionEnvironment.Object, Dev2DecisionMode.AND, out var error);

            Assert.AreEqual("If [[a]] Is Between", result);
            Assert.AreEqual(0, error.FetchErrors().Count);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2Decision))]
        public void Dev2Decision_GenerateUserFriendlyModel_PopulatedColumnCount_One_Recordset_ExpectedError()
        {
            var dev2Decision = new Dev2Decision
            {
                Col1 = "[[recset(*).field]]",
                EvaluationFn = enDecisionType.IsBetween
            };

            var result = dev2Decision.GenerateToolLabel(null, Dev2DecisionMode.AND, out var error);

            Assert.AreEqual("If ", result);
            Assert.AreEqual(1, error.FetchErrors().Count);
            Assert.AreEqual("Object reference not set to an instance of an object.", error.FetchErrors()[0]);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2Decision))]
        public void Dev2Decision_GenerateUserFriendlyModel_PopulatedColumnCount_One_Recordset_Multiple_Values()
        {
            var dev2Decision = new Dev2Decision
            {
                Col1 = "[[recset(*).field]]",
                EvaluationFn = enDecisionType.IsBetween
            };

            var mockExecutionEnvironment = new Mock<IExecutionEnvironment>();
            mockExecutionEnvironment.Setup(env => env.EvalAsListOfStrings(It.IsAny<string>(), It.IsAny<int>())).Returns(new List<string> { "[[a]]", "[[b]]" });
            var result = dev2Decision.GenerateToolLabel(mockExecutionEnvironment.Object, Dev2DecisionMode.AND, out var error);

            Assert.AreEqual("If [[a]] Is Between AND [[b]] Is Between", result);
            Assert.AreEqual(0, error.FetchErrors().Count);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2Decision))]
        public void Dev2Decision_GenerateUserFriendlyModel_PopulatedColumnCount_Two_Col2_Recordset_ExpectedError()
        {
            var dev2Decision = new Dev2Decision
            {
                Col1 = "[[field]]",
                Col2 = "[[recset(*).field]]",
                EvaluationFn = enDecisionType.IsBetween
            };

            var result = dev2Decision.GenerateToolLabel(null, Dev2DecisionMode.AND, out var error);

            Assert.AreEqual("If ", result);
            Assert.AreEqual(1, error.FetchErrors().Count);
            Assert.AreEqual("Object reference not set to an instance of an object.", error.FetchErrors()[0]);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2Decision))]
        public void Dev2Decision_GenerateUserFriendlyModel_PopulatedColumnCount_Two_Col2_IsOnly_Recordset()
        {
            var dev2Decision = new Dev2Decision
            {
                Col1 = "[[field]]",
                Col2 = "[[recset(*).field]]",
                EvaluationFn = enDecisionType.IsBetween
            };

            var mockExecutionEnvironment = new Mock<IExecutionEnvironment>();
            mockExecutionEnvironment.Setup(env => env.EvalAsListOfStrings(It.IsAny<string>(), It.IsAny<int>())).Returns(new List<string> { "[[a]]", "[[b]]" });
            var result = dev2Decision.GenerateToolLabel(mockExecutionEnvironment.Object, Dev2DecisionMode.AND, out var error);

            Assert.AreEqual("If [[field]] Is Between [[a]] AND [[field]] Is Between [[b]]", result);
            Assert.AreEqual(0, error.FetchErrors().Count);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2Decision))]
        public void Dev2Decision_GenerateUserFriendlyModel_PopulatedColumnCount_Two_Col1_Recordset_ExpectedError()
        {
            var dev2Decision = new Dev2Decision
            {
                Col1 = "[[recset(*).field]]",
                Col2 = "[[field]]",
                EvaluationFn = enDecisionType.IsBetween
            };

            var result = dev2Decision.GenerateToolLabel(null, Dev2DecisionMode.AND, out var error);

            Assert.AreEqual("If ", result);
            Assert.AreEqual(1, error.FetchErrors().Count);
            Assert.AreEqual("Object reference not set to an instance of an object.", error.FetchErrors()[0]);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2Decision))]
        public void Dev2Decision_GenerateUserFriendlyModel_PopulatedColumnCount_Two_Col1_IsOnly_Recordset()
        {
            var dev2Decision = new Dev2Decision
            {
                Col1 = "[[recset(*).field]]",
                Col2 = "[[field]]",
                EvaluationFn = enDecisionType.IsBetween
            };

            var mockExecutionEnvironment = new Mock<IExecutionEnvironment>();
            mockExecutionEnvironment.Setup(env => env.EvalAsListOfStrings(It.IsAny<string>(), It.IsAny<int>())).Returns(new List<string> { "[[a]]", "[[b]]" });
            var result = dev2Decision.GenerateToolLabel(mockExecutionEnvironment.Object, Dev2DecisionMode.AND, out var error);

            Assert.AreEqual("If [[a]] Is Between [[field]] AND [[b]] Is Between [[field]]", result);
            Assert.AreEqual(0, error.FetchErrors().Count);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2Decision))]
        public void Dev2Decision_GenerateUserFriendlyModel_PopulatedColumnCount_Two_Col1_And_Col2_Recordset_ExpectedError()
        {
            var dev2Decision = new Dev2Decision
            {
                Col1 = "[[recset(*).field]]",
                Col2 = "[[recset(*).field1]]",
                EvaluationFn = enDecisionType.IsBetween
            };

            var result = dev2Decision.GenerateToolLabel(null, Dev2DecisionMode.AND, out var error);

            Assert.AreEqual("If ", result);
            Assert.AreEqual(1, error.FetchErrors().Count);
            Assert.AreEqual("Object reference not set to an instance of an object.", error.FetchErrors()[0]);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2Decision))]
        public void Dev2Decision_GenerateUserFriendlyModel_PopulatedColumnCount_Two_Col1_And_Col2_Recordset()
        {
            var dev2Decision = new Dev2Decision
            {
                Col1 = "[[recset(*).field]]",
                Col2 = "[[recset(*).field1]]",
                EvaluationFn = enDecisionType.IsBetween
            };

            var mockExecutionEnvironment = new Mock<IExecutionEnvironment>();
            mockExecutionEnvironment.Setup(env => env.EvalAsListOfStrings(It.IsAny<string>(), It.IsAny<int>())).Returns(new List<string> { "[[a]]", "[[b]]" });
            var result = dev2Decision.GenerateToolLabel(mockExecutionEnvironment.Object, Dev2DecisionMode.AND, out var error);

            Assert.AreEqual("If [[a]] Is Between [[a]]", result);
            Assert.AreEqual(0, error.FetchErrors().Count);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2Decision))]
        public void Dev2Decision_GenerateUserFriendlyModel_PopulatedColumnCount_Two_Col1_And_Col2_IsNotRecordset_ExpectedError()
        {
            var dev2Decision = new Dev2Decision
            {
                Col1 = "[[field]]",
                Col2 = "[[field1]]",
                EvaluationFn = enDecisionType.IsBetween
            };

            var result = dev2Decision.GenerateToolLabel(null, Dev2DecisionMode.AND, out var error);

            Assert.AreEqual("If ", result);
            Assert.AreEqual(1, error.FetchErrors().Count);
            Assert.AreEqual("Object reference not set to an instance of an object.", error.FetchErrors()[0]);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2Decision))]
        public void Dev2Decision_GenerateUserFriendlyModel_PopulatedColumnCount_Two_Col1_And_Col2_IsNotRecordset()
        {
            var dev2Decision = new Dev2Decision
            {
                Col1 = "[[field]]",
                Col2 = "[[field1]]",
                EvaluationFn = enDecisionType.IsBetween
            };

            var mockExecutionEnvironment = new Mock<IExecutionEnvironment>();
            mockExecutionEnvironment.Setup(env => env.EvalAsListOfStrings(It.IsAny<string>(), It.IsAny<int>())).Returns(new List<string> { "[[a]]", "[[b]]" });
            var result = dev2Decision.GenerateToolLabel(mockExecutionEnvironment.Object, Dev2DecisionMode.AND, out var error);

            Assert.AreEqual("If [[a]] Is Between [[a]]", result);
            Assert.AreEqual(0, error.FetchErrors().Count);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(Dev2Decision))]
        public void Dev2Decision_GenerateUserFriendlyModel_PopulatedColumnCount_Two_NoMatching_Options()
        {
            var dev2Decision = new Dev2Decision
            {
                Col1 = "[[recset(*).field]]",
                Col2 = "[[recset(*).field1]]",
                EvaluationFn = enDecisionType.IsBetween
            };

            var mockExecutionEnvironment = new Mock<IExecutionEnvironment>();
            mockExecutionEnvironment.Setup(env => env.EvalAsListOfStrings(It.IsAny<string>(), It.IsAny<int>())).Returns(new List<string> { "[[a]]", "[[b]]", "[[c]]" });
            var result = dev2Decision.GenerateToolLabel(mockExecutionEnvironment.Object, Dev2DecisionMode.AND, out var error);

            Assert.AreEqual("If [[a]] Is Between [[a]] AND [[c]] Is Between [[c]]", result);
            Assert.AreEqual(0, error.FetchErrors().Count);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(Dev2Decision))]
        public void Dev2Decision_GenerateUserFriendlyModel_PopulatedColumnCount_DynamicDecisionType()
        {
          var dev2Decision = new Dev2Decision
                             {
                               Col1 = "Some Expression",
                               EvaluationFn = enDecisionType.Dynamic
                             };

          var mockExecutionEnvironment = new Mock<IExecutionEnvironment>();

          var result = dev2Decision.GenerateToolLabel(mockExecutionEnvironment.Object, Dev2DecisionMode.AND, out var error);

          Assert.AreEqual("If Dynamic Expression", result);
          Assert.AreEqual(0, error.FetchErrors().Count);
        }
  }
}
