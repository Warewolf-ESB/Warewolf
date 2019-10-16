/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities.Presentation.Model;
using Dev2.Activities.Designers2.Gate;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

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
            var expectedGateFailure = "Retry [Gate]";
            var mockModelItem = new Mock<ModelItem>();
            //------------Execute Test----------------------------
            var gateDesignerViewModel = new GateDesignerViewModel(mockModelItem.Object);
            //------------Assert Results--------------------------
            Assert.AreEqual(expectedGateFailure, gateDesignerViewModel.GateFailure);
            Assert.IsTrue(gateDesignerViewModel.HasLargeView);
        }
    }
}
