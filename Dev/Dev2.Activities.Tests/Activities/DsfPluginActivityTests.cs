/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Activities;
using Dev2.Data.TO;
using Dev2.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Tests.Activities.Activities
{
    [TestClass]
    public class DsfPluginActivityTests
    {
        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfPluginActivity))]
        public void DsfPluginActivity_ExecutionImpl_tempErrors_IsNotNull_Expect_True()
        {
            //-----------------Arrange-----------------------
            var mockEsbChannel = new Mock<IEsbChannel>().Object;
            var mockDSFDataObject = new Mock<IDSFDataObject>().Object;

            var dsfPluginActivity = new TestDsfPluginActivity();
            //-----------------Act---------------------------
            dsfPluginActivity.TestExecutionImpl(mockEsbChannel, mockDSFDataObject, "TestInput", "TestOutput", out ErrorResultTO tempErrors, 0);
            //-----------------Assert------------------------
            Assert.IsNotNull(tempErrors);
            Assert.AreEqual(0, tempErrors.FetchErrors().Count);
        }

        class TestDsfPluginActivity : DsfPluginActivity
        {
            public void TestExecutionImpl(IEsbChannel esbChannel, IDSFDataObject dataObject, string inputs, string outputs, out ErrorResultTO tmpErrors, int update)
            {
                base.ExecutionImpl(esbChannel, dataObject, inputs, outputs, out tmpErrors, update);
            }
        }
    }
}
