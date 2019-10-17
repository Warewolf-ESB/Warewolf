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
using Dev2.Common.Interfaces.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Activities.Presentation.Model;
using System.Linq;

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
            //------------Execute Test----------------------------
            var gateDesignerViewModel = new GateDesignerViewModel(mockModelItem.Object);
            Assert.IsTrue(gateDesignerViewModel.HasLargeView);
            Assert.AreEqual(expectedGateFailure, gateDesignerViewModel.GateFailure.ToString());
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
    }
}
