
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using Dev2.Integration.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
namespace Dev2.Integration.Tests.Dev2.Application.Server.Tests.Bpm_unit_tests
{
    /// <summary>
    /// Summary description for RemoteInvokeTest
    /// </summary>
    [TestClass]
    public class RemoteInvokeTest
    {
        [TestMethod]
        // TFS Migration Issue
        public void CanInvokeARemoteService()
        {
            string PostData = String.Format("{0}{1}", ServerSettings.WebserverURI, "TestCategory/9139Local");
            const string expected = @"<DataList><localResult>6</localResult><remoteResult>Success</remoteResult><localError></localError><remoteError></remoteError><dbo_spGetCountries index=""1""><CountryID>11</CountryID><Description>Bahrain</Description></dbo_spGetCountries><dbo_spGetCountries index=""2""><CountryID>12</CountryID><Description>Bangladesh</Description></dbo_spGetCountries><dbo_spGetCountries index=""3""><CountryID>13</CountryID><Description>Barbados</Description></dbo_spGetCountries><dbo_spGetCountries index=""4""><CountryID>14</CountryID><Description>Belarus</Description></dbo_spGetCountries><dbo_spGetCountries index=""5""><CountryID>15</CountryID><Description>Belgium</Description></dbo_spGetCountries><dbo_spGetCountries index=""6""><CountryID>16</CountryID><Description>Belize</Description></dbo_spGetCountries><dbo_spGetCountries index=""7""><CountryID>17</CountryID><Description>Benin</Description></dbo_spGetCountries><dbo_spGetCountries index=""8""><CountryID>18</CountryID><Description>Bhutan</Description></dbo_spGetCountries><dbo_spGetCountries index=""9""><CountryID>19</CountryID><Description>Bolivia</Description></dbo_spGetCountries><dbo_spGetCountries index=""10""><CountryID>20</CountryID><Description>Bosnia and Herzegovina</Description></dbo_spGetCountries><dbo_spGetCountries index=""11""><CountryID>21</CountryID><Description>Brazil</Description></dbo_spGetCountries><dbo_spGetCountries index=""12""><CountryID>22</CountryID><Description>Brunei</Description></dbo_spGetCountries><dbo_spGetCountries index=""13""><CountryID>23</CountryID><Description>Bulgaria</Description></dbo_spGetCountries></DataList>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            StringAssert.Contains(ResponseData, expected, "Expected [ " + expected + " ] But Got [ " + ResponseData + " ]");
        }

        [TestMethod]
        // Here because it is easy to test via the remote invoke ;)
        public void DoesWorkflowWithNoStartNodeEmitCorrectDebugInfo()
        {
            string PostData = String.Format("{0}{1}", ServerSettings.WebserverURI, "TestCategory/Bug9484");

            Guid id = Guid.NewGuid();
            TestHelper.PostDataToWebserverAsRemoteAgent(PostData, id);

            var debugItems = TestHelper.FetchRemoteDebugItems(ServerSettings.WebserverURI, id);

            Assert.AreEqual(1, debugItems.Count);
            Assert.AreEqual("The workflow must have at least one service or activity connected to the Start Node.", debugItems[0].ErrorMessage);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("Webservice_Invoke")]
        public void Webservice_Invoke_WhenDataReturnedIsDifferentThenTestedDataType_DataFormatErrorMessage()
        {
            //------------Setup for test--------------------------
            string PostData = String.Format("{0}{1}", ServerSettings.WebserverURI, "INTEGRATION TEST SERVICES/Bug_11271_Test_Bad");

            Guid id = Guid.NewGuid();

            //------------Execute Test---------------------------
            TestHelper.PostDataToWebserverAsRemoteAgent(PostData, id);

            var debugItems = TestHelper.FetchRemoteDebugItems(ServerSettings.WebserverURI, id);

            //------------Assert Results-------------------------
            Assert.AreEqual(2, debugItems.Count);
            StringAssert.Contains(debugItems[1].ErrorMessage, "1 Data Format Error : It is likely that you tested with one format yet the service is returning another. IE you tested with XML and it now returns JSON");
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("Webservice_Invoke")]
        public void Webservice_Invoke_WhenDataReturnedIsSameAsTestedDataType_NoDataFormatErrorMessage()
        {
            //------------Setup for test--------------------------
            string PostData = String.Format("{0}{1}", ServerSettings.WebserverURI, "INTEGRATION TEST SERVICES/Bug_11271_Test_Ok");

            Guid id = Guid.NewGuid();

            //------------Execute Test---------------------------
            TestHelper.PostDataToWebserverAsRemoteAgent(PostData, id);

            var debugItems = TestHelper.FetchRemoteDebugItems(ServerSettings.WebserverURI, id);

            //------------Assert Results-------------------------
            Assert.AreEqual("TRUE", debugItems[2].Outputs[0].FetchResultsList()[1].Value);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("RemoteInvoke_CanFetchDebugItems")]
        public void RemoteInvoke_CanFetchDebugItems_WhenRemoteInvokeWorkflow_ExpectAllDebugItems()
        {
            string PostData = String.Format("{0}{1}", ServerSettings.WebserverURI, "Remote Debug Test");

            Guid id = Guid.NewGuid();
            TestHelper.PostDataToWebserverAsRemoteAgent(PostData, id);

            var debugItems = TestHelper.FetchRemoteDebugItems(ServerSettings.WebserverURI, id);

            Assert.AreEqual(2, debugItems.Count);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("RemoteInvoke_CanExecuteRemoteNested")]
        public void RemoteInvoke_CanFetchResults_WhenRemoteNestedWF_ExpectCorrectMappings()
        {
            string PostData = String.Format("{0}{1}", ServerSettings.WebserverURI, "Tests/11466_RemoteExecution");

            Guid id = Guid.NewGuid();
            var output = TestHelper.PostDataToWebserverAsRemoteAgent(PostData, id);
            Assert.AreEqual("<DataList><Output>PASS</Output></DataList>", output);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("RemoteInvoke_CanExecuteRemoteNested")]
        public void RemoteInvoke_CanFetchResults_WhenRemoteNestedWF_SameName_ExpectCorrectMappings()
        {
            string PostData = String.Format("{0}{1}", ServerSettings.WebserverURI, "Tests/11466_RemoteExecution_SameName");

            Guid id = Guid.NewGuid();
            var output = TestHelper.PostDataToWebserverAsRemoteAgent(PostData, id);
            Assert.AreEqual(@"<DataList><Output>PASS</Output><RemoteWorkflowErrors></RemoteWorkflowErrors><hero index=""1""><pushups>All of them</pushups><name>Chuck Norris</name></hero></DataList>", output);
        }
    }
}
