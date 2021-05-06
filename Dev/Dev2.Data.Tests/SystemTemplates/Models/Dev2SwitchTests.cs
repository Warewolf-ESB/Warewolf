/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Data.SystemTemplates.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data.Tests.SystemTemplates.Models
{
    [TestClass]
    public class Dev2SwitchTests
    {
        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(Dev2Switch))]
        public void Dev2Switch_SetProperties_AreEqual_ExpectTrue()
        {
            //-----------------------Arrange--------------------
            //-----------------------Act------------------------
            var dev2Switch = new Dev2Switch()
            {
                SwitchVariable = "TestSwitchVariable",
                SwitchExpression = "TestSwitchExpression",
                DisplayText = "TestDisplayText"
            };
            //-----------------------Assert---------------------
            Assert.AreEqual(Dev2ModelType.Dev2Switch, dev2Switch.ModelName);
            Assert.AreEqual("TestSwitchVariable", dev2Switch.SwitchVariable);
            Assert.AreEqual("TestSwitchExpression", dev2Switch.SwitchExpression);
            Assert.AreEqual("TestDisplayText", dev2Switch.DisplayText);
        }
    }
}
