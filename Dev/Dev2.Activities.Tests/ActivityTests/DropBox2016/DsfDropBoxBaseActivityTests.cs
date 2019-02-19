/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using Dev2.Activities.DropBox2016;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.State;
using Dev2.Common.Wrappers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Tests.Activities.ActivityTests.DropBox2016
{
    [TestClass]
    public class DsfDropBoxBaseActivityTests
    {
        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxBaseActivity))]
        public void DsfDropBoxBaseActivity_SetupDropboxClient_DropboxClient_IsNull()
        {
            //--------------------------Arrange---------------------------
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();

            var testDsfDropBoxBaseActivity = new TestDsfDropBoxBaseActivity(mockDropboxClientFactory.Object);
            //--------------------------Act-------------------------------
            testDsfDropBoxBaseActivity.TestSetupDropboxClient("testToken");
            //--------------------------Assert----------------------------
            Assert.IsNotNull(testDsfDropBoxBaseActivity.UniqueID);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxBaseActivity))]
        public void DsfDropBoxBaseActivity_SetupDropboxClient_DropboxClient_IsNotNull()
        {
            //--------------------------Arrange---------------------------
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            var mockDropboxClient = new Mock<IDropboxClient>();

            //--------------------------Act-------------------------------
            var testDsfDropBoxBaseActivity = new TestDsfDropBoxBaseActivity(mockDropboxClientFactory.Object, mockDropboxClient.Object);

            testDsfDropBoxBaseActivity.TestSetupDropboxClient("testToken");
            //--------------------------Assert----------------------------
            testDsfDropBoxBaseActivity.Dispose();
            Assert.IsNotNull(testDsfDropBoxBaseActivity.UniqueID);
        }

        class TestDsfDropBoxBaseActivity : DsfDropBoxBaseActivity
        {
            public TestDsfDropBoxBaseActivity(IDropboxClientFactory dropboxClientFactory) 
                : base(dropboxClientFactory)
            {
            }
            public TestDsfDropBoxBaseActivity(IDropboxClientFactory dropboxClientFactory, IDropboxClient _dropboxClient)
                : base(dropboxClientFactory)
            {
            }

            public override IEnumerable<StateVariable> GetState()
            {
                return GetState();
            }

            protected override List<string> PerformExecution(Dictionary<string, string> evaluatedValues)
            {
                return PerformExecution(evaluatedValues);
            }

            public void TestSetupDropboxClient(string accessToken)
            {
                SetupDropboxClient(accessToken);
            }
        }
    }
}
