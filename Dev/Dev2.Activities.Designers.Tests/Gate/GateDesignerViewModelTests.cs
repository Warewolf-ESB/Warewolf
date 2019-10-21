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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Activities.Presentation.Model;
using System.Linq;
using Warewolf.Data;

namespace Dev2.Activities.Designers.Tests.Gate
{
    [TestClass]
    public class GateDesignerViewModelTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(GateDesignerViewModel))]
        public void GateDesignerViewModel_Constructor()
        {
            //------------Setup for test--------------------------
            var mockModelItem = new Mock<ModelItem>();
            var expectedGateFailure = "Retry";
            var retryStrategy = "NoBackoff";
            //------------Execute Test----------------------------
            var gateDesignerViewModel = new GateDesignerViewModel(mockModelItem.Object);
            Assert.IsTrue(gateDesignerViewModel.HasLargeView);
            Assert.AreEqual(expectedGateFailure, gateDesignerViewModel.GateFailure.ToString());
            Assert.AreEqual(retryStrategy, gateDesignerViewModel.GateRetryStrategy.ToString());
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(GateDesignerViewModel))]
        public void GateDesignerViewModel_GateFailureOptions_Retry()
        {
            //------------Setup for test--------------------------
            var mockModelItem = new Mock<ModelItem>();
            CustomContainer.Register(mockModelItem.Object);
            var gateDesignerViewModel = new GateDesignerViewModel(mockModelItem.Object);
            //------------Execute Test---------------------------
            gateDesignerViewModel.SelectedGateFailure = "Retry: Retry execution on error";
            //------------Assert Results-------------------------
            Assert.AreEqual("Retry: Retry execution on error", gateDesignerViewModel.SelectedGateFailure);
            Assert.AreEqual(GateFailureOptions.Retry, gateDesignerViewModel.GateFailure);
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(GateDesignerViewModel))]
        public void GateDesignerViewModel_GateFailureOptions_StopOnError()
        {
            //------------Setup for test--------------------------
            var mockModelItem = new Mock<ModelItem>();
            CustomContainer.Register(mockModelItem.Object);
            var gateDesignerViewModel = new GateDesignerViewModel(mockModelItem.Object);
            //------------Execute Test---------------------------
            gateDesignerViewModel.SelectedGateFailure = "StopOnError: Stop execution on error";
            //------------Assert Results-------------------------
            Assert.AreEqual("StopOnError: Stop execution on error", gateDesignerViewModel.SelectedGateFailure);
            Assert.AreEqual(GateFailureOptions.StopOnError, gateDesignerViewModel.GateFailure);
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(GateDesignerViewModel))]
        public void GateDesignerViewModel_GateFailureOptions_List()
        {
            //------------Setup for test--------------------------
            var mockModelItem = new Mock<ModelItem>();
            CustomContainer.Register(mockModelItem.Object);
            var gateDesignerViewModel = new GateDesignerViewModel(mockModelItem.Object);
            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            var list = gateDesignerViewModel.GateFailureOptions.ToList();
            Assert.AreEqual("Retry: Retry execution on error", list[0]);
            Assert.AreEqual("StopOnError: Stop execution on error", list[1]);
          
           
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(GateDesignerViewModel))]
        public void GateDesignerViewModel_GateRetryStrategies_List()
        {
            //------------Setup for test--------------------------
            var mockModelItem = new Mock<ModelItem>();
            CustomContainer.Register(mockModelItem.Object);
            var gateDesignerViewModel = new GateDesignerViewModel(mockModelItem.Object);
            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            var list = gateDesignerViewModel.GateRetryStrategies.ToList();
            Assert.AreEqual("NoBackoff: On Error Retry Immediately", list[0]);
            Assert.AreEqual("ConstantBackoff : Add a fixed delay after every attempt", list[1]);
            Assert.AreEqual("LinearBackoff : Delay increases along with every attempt on Linear curve", list[2]);
            Assert.AreEqual("FibonacciBackoff  : Delays based on the sum of the Fibonacci series", list[3]);
            Assert.AreEqual("QuadraticBackoff : Delay increases along with every attempt on Quadratic curve", list[4]);

        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(GateDesignerViewModel))]
        public void GateDesignerViewModel_GateRetryStrategies_SelectedGateFailure()
        {
            //------------Setup for test--------------------------
            var mockModelItem = new Mock<ModelItem>();
            CustomContainer.Register(mockModelItem.Object);
            var gateDesignerViewModel = new GateDesignerViewModel(mockModelItem.Object);
            //------------Execute Test---------------------------
            gateDesignerViewModel.SelectedRetryStrategy = "ConstantBackoff : Add a fixed delay after every attempt";
            //------------Assert Results-------------------------
            Assert.AreEqual("ConstantBackoff : Add a fixed delay after every attempt", gateDesignerViewModel.SelectedRetryStrategy);
            Assert.AreEqual(GateRetryStrategies.ConstantBackoff, gateDesignerViewModel.GateRetryStrategy);
        }
    }
}
