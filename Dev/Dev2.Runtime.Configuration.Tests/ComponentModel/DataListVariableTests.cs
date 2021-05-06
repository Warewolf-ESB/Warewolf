/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/
using Dev2.Runtime.Configuration.ComponentModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel;

namespace Dev2.Runtime.Configuration.Tests.ComponentModel
{
    [TestClass]
    public class DataListVariableTests
    {
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(DataListVariable))]
        public void DataListVariable_Name()
        {
            var called = false;
            var dataListVariable = new DataListVariable() { Name = "test" };
            dataListVariable.PropertyChanged += (s, e) => called = true;
            dataListVariable.Name = "testChange";
            Assert.AreEqual("testChange", dataListVariable.Name);
            Assert.IsTrue(called);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(DataListVariable))]
        public void DataListVariable_Name_Rename_Same()
        {
            var called = false;
            var dataListVariable = new DataListVariable() { Name = "test" };
            dataListVariable.PropertyChanged += (s, e) => called = true;
            dataListVariable.Name = "test";
            Assert.AreEqual("test", dataListVariable.Name);
            Assert.IsFalse(called);
        }
    }
}
