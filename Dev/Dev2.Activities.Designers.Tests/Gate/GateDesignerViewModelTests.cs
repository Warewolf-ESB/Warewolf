/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Activities.Designers2.Gate;
using Dev2.Studio.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Linq;
using Warewolf.Options;

namespace Dev2.Activities.Designers.Tests.Gate
{
    [TestClass]
    public class GateDesignerViewModelTests
    {
        [TestInitialize]
        public void SetupForTest()
        {
            var optionBool = new OptionBool();
            var optionsList = new List<IOption> { optionBool };

            var mockServer = new Mock<IServer>();

            var mockResourceRepository = new Mock<IResourceRepository>();
            mockResourceRepository.Setup(resourceRepository => resourceRepository.FindOptionsBy(mockServer.Object, "")).Returns(optionsList);

            mockServer.Setup(server => server.ResourceRepository).Returns(mockResourceRepository.Object);

            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(shellViewModel => shellViewModel.ActiveServer).Returns(mockServer.Object);

            CustomContainer.Register(mockShellViewModel.Object);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(GateDesignerViewModel))]
        public void GateDesignerViewModel_Constructor()
        {
            //------------Setup for test--------------------------
            var mockModelItem = new Mock<ModelItem>();
            var expectedGateFailure = "StopOnError: Stop execution on error";
            var retryStrategy = "NoBackoff";
            //------------Execute Test----------------------------
            var gateDesignerViewModel = new GateDesignerViewModel(mockModelItem.Object);
            //------------Assert Results-------------------------
            Assert.IsTrue(gateDesignerViewModel.HasLargeView);
            Assert.AreEqual(expectedGateFailure, gateDesignerViewModel.SelectedGateFailure);
            Assert.AreEqual(retryStrategy, gateDesignerViewModel.SelectedRetryStrategy);
            Assert.IsFalse(gateDesignerViewModel.GateSelectionVisible);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(GateDesignerViewModel))]
        public void GateDesignerViewModel_GateFailureOptions_Retry()
        {
            //------------Setup for test--------------------------
            var mockModelItem = new Mock<ModelItem>();
            CustomContainer.Register(mockModelItem.Object);
            //------------Execute Test---------------------------
            var gateDesignerViewModel = new GateDesignerViewModel(mockModelItem.Object)
            {
                SelectedGateFailure = "Retry: Retry execution on error"
            };
            //------------Assert Results-------------------------
            Assert.AreEqual("Retry: Retry execution on error", gateDesignerViewModel.SelectedGateFailure);
            Assert.IsTrue(gateDesignerViewModel.GateSelectionVisible);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(GateDesignerViewModel))]
        public void GateDesignerViewModel_GateFailureOptions_StopOnError()
        {
            //------------Setup for test--------------------------
            var mockModelItem = new Mock<ModelItem>();
            CustomContainer.Register(mockModelItem.Object);
            //------------Execute Test---------------------------
            var gateDesignerViewModel = new GateDesignerViewModel(mockModelItem.Object)
            {
                SelectedGateFailure = "StopOnError: Stop execution on error"
            };
            //------------Assert Results-------------------------
            Assert.AreEqual("StopOnError: Stop execution on error", gateDesignerViewModel.SelectedGateFailure);
            Assert.IsFalse(gateDesignerViewModel.GateSelectionVisible);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(GateDesignerViewModel))]
        public void GateDesignerViewModel_GateFailureOptions_List()
        {
            //------------Setup for test--------------------------
            var mockModelItem = new Mock<ModelItem>();
            CustomContainer.Register(mockModelItem.Object);
            //------------Execute Test---------------------------
            var gateDesignerViewModel = new GateDesignerViewModel(mockModelItem.Object);
            var gateFailureOptions = gateDesignerViewModel.GateFailureOptions.ToList();
            //------------Assert Results-------------------------
            Assert.AreEqual(2, gateFailureOptions.Count);
            Assert.AreEqual("Retry: Retry execution on error", gateFailureOptions[0]);
            Assert.AreEqual("StopOnError: Stop execution on error", gateFailureOptions[1]);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(GateDesignerViewModel))]
        public void GateDesignerViewModel_GateRetryStrategies_List()
        {
            //------------Setup for test--------------------------
            var mockModelItem = new Mock<ModelItem>();
            CustomContainer.Register(mockModelItem.Object);
            //------------Execute Test---------------------------
            var gateDesignerViewModel = new GateDesignerViewModel(mockModelItem.Object);
            var gateRetryStrategies = gateDesignerViewModel.GateRetryStrategies.ToList();
            //------------Assert Results-------------------------
            Assert.AreEqual(5, gateRetryStrategies.Count);
            Assert.AreEqual("NoBackoff: On Error Retry Immediately", gateRetryStrategies[0]);
            Assert.AreEqual("ConstantBackoff: Add a fixed delay after every attempt", gateRetryStrategies[1]);
            Assert.AreEqual("LinearBackoff: Delay increases along with every attempt on Linear curve", gateRetryStrategies[2]);
            Assert.AreEqual("FibonacciBackoff: Delays based on the sum of the Fibonacci series", gateRetryStrategies[3]);
            Assert.AreEqual("QuadraticBackoff: Delay increases along with every attempt on Quadratic curve", gateRetryStrategies[4]);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(GateDesignerViewModel))]
        public void GateDesignerViewModel_GateRetryStrategies_SelectedGateFailure()
        {
            //------------Setup for test--------------------------
            var mockModelItem = new Mock<ModelItem>();
            CustomContainer.Register(mockModelItem.Object);
            //------------Execute Test---------------------------
            var gateDesignerViewModel = new GateDesignerViewModel(mockModelItem.Object)
            {
                SelectedRetryStrategy = "ConstantBackoff: Add a fixed delay after every attempt"
            };
            //------------Assert Results-------------------------
            Assert.AreEqual("ConstantBackoff: Add a fixed delay after every attempt", gateDesignerViewModel.SelectedRetryStrategy);
        }
    }
}
