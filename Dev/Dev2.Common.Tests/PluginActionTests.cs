/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.DB;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;

namespace Dev2.Common.Tests
{
    [TestClass]
    public class PluginActionTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(PluginAction))]
        public void PluginAction_Validate()
        {
            const string expectedFullName = "testFullName";
            const string expectedMethod = "testMethod";

            var mockServiceInput = new Mock<IServiceInput>();
            mockServiceInput.Setup(serviceInput => serviceInput.Name).Returns("testName");
            mockServiceInput.Setup(serviceInput => serviceInput.Value).Returns("1");

            var mockServiceInputOther = new Mock<IServiceInput>();
            mockServiceInputOther.Setup(serviceInput => serviceInput.Name).Returns("testNewName");
            mockServiceInputOther.Setup(serviceInput => serviceInput.Value).Returns("2");

            var expectedInputs = new List<IServiceInput>
            {
                mockServiceInput.Object,
                mockServiceInputOther.Object
            };

            var expectedReturnType = typeof(string);

            var mockNameValue = new Mock<INameValue>();
            mockNameValue.Setup(nameValue => nameValue.Name).Returns("[[a]]");
            mockNameValue.Setup(nameValue => nameValue.Value).Returns("1");

            var mockNameValueOther = new Mock<INameValue>();
            mockNameValueOther.Setup(nameValue => nameValue.Name).Returns("[[b]]");
            mockNameValueOther.Setup(nameValue => nameValue.Value).Returns("2");

            var expectedVariables = new List<INameValue>
            {
                mockNameValue.Object,
                mockNameValueOther.Object
            };

            const string expectedDev2ReturnType = "testDev2ReturnType";
            const string expectedMethodResult = "testMethodResult";
            const string expectedOutputVariable = "testOutputVariable";
            const bool expectedIsObject = false;
            const bool expectedIsVoid = false;
            const string expectedErrorMessage = "This is an error message";
            const bool expectedHasError = false;
            const bool expectedIsProperty = false;
            var expectedID = Guid.NewGuid();

            var pluginAction = new PluginAction
            {
                FullName = expectedFullName,
                Method = expectedMethod,
                Inputs = expectedInputs,
                ReturnType = expectedReturnType,
                Variables = expectedVariables,
                Dev2ReturnType = expectedDev2ReturnType,
                MethodResult = expectedMethodResult,
                OutputVariable = expectedOutputVariable,
                IsObject = expectedIsObject,
                IsVoid = expectedIsVoid,
                ErrorMessage = expectedErrorMessage,
                HasError = expectedHasError,
                IsProperty = expectedIsProperty,
                ID = expectedID
            };

            Assert.AreEqual(expectedFullName, pluginAction.FullName);
            Assert.AreEqual(expectedMethod, pluginAction.Method);

            Assert.AreEqual(2, pluginAction.Inputs.Count);
            Assert.AreEqual("testName", pluginAction.Inputs[0].Name);
            Assert.AreEqual("1", pluginAction.Inputs[0].Value);
            Assert.AreEqual("testNewName", pluginAction.Inputs[1].Name);
            Assert.AreEqual("2", pluginAction.Inputs[1].Value);

            Assert.AreEqual(expectedReturnType, pluginAction.ReturnType);

            Assert.AreEqual(2, pluginAction.Variables.Count);
            Assert.AreEqual("[[a]]", pluginAction.Variables[0].Name);
            Assert.AreEqual("1", pluginAction.Variables[0].Value);
            Assert.AreEqual("[[b]]", pluginAction.Variables[1].Name);
            Assert.AreEqual("2", pluginAction.Variables[1].Value);

            Assert.AreEqual(expectedDev2ReturnType, pluginAction.Dev2ReturnType);
            Assert.AreEqual(expectedMethodResult, pluginAction.MethodResult);
            Assert.AreEqual(expectedOutputVariable, pluginAction.OutputVariable);
            Assert.AreEqual(expectedIsObject, pluginAction.IsObject);
            Assert.AreEqual(expectedIsVoid, pluginAction.IsVoid);
            Assert.AreEqual(expectedErrorMessage, pluginAction.ErrorMessage);
            Assert.AreEqual(expectedHasError, pluginAction.HasError);
            Assert.AreEqual(expectedIsProperty, pluginAction.IsProperty);

            Assert.AreEqual(expectedFullName + expectedMethod, pluginAction.GetIdentifier());
            Assert.AreEqual(expectedMethod, pluginAction.ToString());

            Assert.AreEqual(expectedID, pluginAction.ID);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(PluginAction))]
        public void PluginAction_Equals_PluginAction_Null_Expected_False()
        {
            var pluginAction = new PluginAction();

            const PluginAction nullPluginAction = null;

            var isEqual = pluginAction.Equals(nullPluginAction);
            Assert.IsFalse(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(PluginAction))]
        public void PluginAction_ReferenceEquals_PluginAction_Expected_True()
        {
            const string expectedMethod = "testMethod";

            var pluginAction = new PluginAction
            {
                Method = expectedMethod
            };

            var isEqual = pluginAction.Equals(pluginAction);
            Assert.IsTrue(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(PluginAction))]
        public void PluginAction_Equals_PluginAction_Expected_True()
        {
            const string expectedMethod = "testMethod";

            var pluginAction = new PluginAction
            {
                Method = expectedMethod
            };
            var pluginActionDup = new PluginAction
            {
                Method = expectedMethod
            };

            var isEqual = pluginAction.Equals(pluginActionDup);
            Assert.IsTrue(isEqual);
            Assert.IsTrue(pluginAction == pluginActionDup);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(PluginAction))]
        public void PluginAction_Equals_PluginAction_Expected_False()
        {
            const string expectedMethod = "testMethod";

            var pluginAction = new PluginAction
            {
                Method = expectedMethod
            };

            const string expectedMethodDup = "testMethodDup";

            var pluginActionObj = new PluginAction
            {
                Method = expectedMethodDup
            };

            var isEqual = pluginAction.Equals(pluginActionObj);
            Assert.IsFalse(isEqual);
            Assert.IsTrue(pluginAction != pluginActionObj);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(PluginAction))]
        public void PluginAction_Equals_Object_Null_Expected_False()
        {
            var pluginAction = new PluginAction();

            const object pluginActionObj = null;

            var isEqual = pluginAction.Equals(pluginActionObj);
            Assert.IsFalse(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(PluginAction))]
        public void PluginAction_Equals_Object_Expected_True()
        {
            const string expectedMethod = "testMethod";

            var pluginAction = new PluginAction
            {
                Method = expectedMethod
            };

            object pluginActionObj = pluginAction;

            var isEqual = pluginAction.Equals(pluginActionObj);
            Assert.IsTrue(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(PluginAction))]
        public void PluginAction_Equals_Object_Expected_False()
        {
            const string expectedMethod = "testMethod";

            var pluginAction = new PluginAction
            {
                Method = expectedMethod
            };

            const string expectedMethodDup = "testMethodDup";

            object pluginActionObj = new PluginAction
            {
                Method = expectedMethodDup
            };

            var isEqual = pluginAction.Equals(pluginActionObj);
            Assert.IsFalse(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(PluginAction))]
        public void PluginAction_Equals_Object_GetType_Expected_False()
        {
            const string expectedMethod = "testMethod";

            var pluginAction = new PluginAction
            {
                Method = expectedMethod
            };

            var pluginActionObj = new object();

            var isEqual = pluginAction.Equals(pluginActionObj);
            Assert.IsFalse(isEqual);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(PluginAction))]
        public void PluginAction_GetHashCode_Not_Equal_To_Zero()
        {
            const string expectedMethod = "testMethod";

            var mockServiceInput = new Mock<IServiceInput>();
            mockServiceInput.Setup(serviceInput => serviceInput.Name).Returns("testName");
            mockServiceInput.Setup(serviceInput => serviceInput.Value).Returns("1");

            var mockServiceInputOther = new Mock<IServiceInput>();
            mockServiceInputOther.Setup(serviceInput => serviceInput.Name).Returns("testNewName");
            mockServiceInputOther.Setup(serviceInput => serviceInput.Value).Returns("2");

            var expectedInputs = new List<IServiceInput>
            {
                mockServiceInput.Object,
                mockServiceInputOther.Object
            };

            var pluginAction = new PluginAction
            {
                Method = expectedMethod,
                Inputs = expectedInputs
            };

            var hashCode = pluginAction.GetHashCode();

            Assert.AreNotEqual(0, hashCode);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(PluginAction))]
        public void PluginAction_GetHashCode_Expect_Zero()
        {
            var pluginAction = new PluginAction();

            var hashCode = pluginAction.GetHashCode();

            Assert.AreEqual(0, hashCode);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(PluginAction))]
        public void PluginAction_OnPropertyChanged()
        {
            bool called = false;
            var action = new TestPluginAction();
            action.PropertyChanged += (s, e) => { called = true; };

            Assert.IsFalse(called);

            action.TestOnPropertyChanged();

            Assert.IsTrue(called);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(PluginAction))]
        public void PluginAction_OnPropertyChanged_Null_DoesNotThrow()
        {
            bool called = false;
            var action = new TestPluginAction();

            Assert.IsFalse(called);

            action.TestOnPropertyChanged();
        }

        class TestPluginAction : PluginAction
        {
            public void TestOnPropertyChanged()
            {
                OnPropertyChanged("SomeProperty");
            }
        }

    }
}
