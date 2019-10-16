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
using Dev2.Activities.Designers2.Gate;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            var expectedImagePath = "pack://application:,,,/Warewolf Studio;component/Images/gate-open.png";
            var expectedGateFailure = "Retry [Gate]";
            //------------Execute Test----------------------------
            var gateDesignerViewModel = new GateDesignerViewModel();
            //------------Assert Results--------------------------
            Assert.AreEqual(expectedImagePath, gateDesignerViewModel.ImagePath);
            Assert.AreEqual(expectedGateFailure, gateDesignerViewModel.GateFailure);
        }
    }
}
