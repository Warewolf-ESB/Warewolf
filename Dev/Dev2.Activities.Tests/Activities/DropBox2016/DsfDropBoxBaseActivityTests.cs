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
using System.Net.Http;
using Dev2.Activities.DropBox2016;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.State;
using Dev2.Common.Wrappers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Tests.Activities.Activities.DropBox2016
{
    [TestClass]
    public class DsfDropBoxBaseActivityTests
    {
        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(DsfDropBoxBaseActivity))]
        public void DsfDropBoxBaseActivity_SetupDropboxClient_DropboxClient_IsNotNull_ExpectTrue()
        {
            //----------------------------Arrange------------------------------
            var mockDropboxClientFactory = new Mock<IDropboxClientFactory>();
            var mockDropboxClient= new Mock<IDropboxClient>();

            mockDropboxClientFactory.Setup(o => o.New(It.IsAny<string>(), It.IsAny<HttpClient>())).Returns(mockDropboxClient.Object);

            //----------------------------Act----------------------------------
            using (var testDsfDropBoxBaseActivity = new TestDsfDropBoxBaseActivity(mockDropboxClientFactory.Object))
            {
                testDsfDropBoxBaseActivity.TestSetupDropboxClient("TestAccessToken");

                Assert.IsNotNull(testDsfDropBoxBaseActivity.UniqueID);
            }
            //----------------------------Assert-------------------------------
            mockDropboxClientFactory.VerifyAll();
        }

        class TestDsfDropBoxBaseActivity : DsfDropBoxBaseActivity
        {
            public TestDsfDropBoxBaseActivity(IDropboxClientFactory dropboxClientFactory)
                : base(dropboxClientFactory)
            {
            }

            public void TestSetupDropboxClient(string accessToken)
            {
                SetupDropboxClient(accessToken);
            }

            public override IEnumerable<StateVariable> GetState()
            {
                throw new System.NotImplementedException();
            }

            protected override List<string> PerformExecution(Dictionary<string, string> evaluatedValues)
            {
                throw new System.NotImplementedException();
            }
        }
    }
}
