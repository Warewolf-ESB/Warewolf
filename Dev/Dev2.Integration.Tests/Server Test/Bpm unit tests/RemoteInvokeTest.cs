using System;
using Dev2.Integration.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            string PostData = String.Format("{0}{1}", ServerSettings.WebserverURI, "9139Local");
            string expected = @"</remoteResult><localResult>6</localResult><dbo_spGetCountries><CountryID>11</CountryID><Description>Bahrain</Description></dbo_spGetCountries><dbo_spGetCountries><CountryID>12</CountryID><Description>Bangladesh</Description></dbo_spGetCountries><dbo_spGetCountries><CountryID>13</CountryID><Description>Barbados</Description></dbo_spGetCountries><dbo_spGetCountries><CountryID>14</CountryID><Description>Belarus</Description></dbo_spGetCountries><dbo_spGetCountries><CountryID>15</CountryID><Description>Belgium</Description></dbo_spGetCountries><dbo_spGetCountries><CountryID>16</CountryID><Description>Belize</Description></dbo_spGetCountries><dbo_spGetCountries><CountryID>17</CountryID><Description>Benin</Description></dbo_spGetCountries><dbo_spGetCountries><CountryID>18</CountryID><Description>Bhutan</Description></dbo_spGetCountries><dbo_spGetCountries><CountryID>19</CountryID><Description>Bolivia</Description></dbo_spGetCountries><dbo_spGetCountries><CountryID>20</CountryID><Description>Bosnia and Herzegovina</Description></dbo_spGetCountries><dbo_spGetCountries><CountryID>21</CountryID><Description>Brazil</Description></dbo_spGetCountries><dbo_spGetCountries><CountryID>22</CountryID><Description>Brunei</Description></dbo_spGetCountries><dbo_spGetCountries><CountryID>23</CountryID><Description>Bulgaria</Description></dbo_spGetCountries></DataList>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            StringAssert.Contains(ResponseData, expected, "Expected [ " + expected + " ] But Got [ " + ResponseData + " ]");
        }

        [TestMethod]
        // Here because it is easy to test via the remote invoke ;)
        public void DoesWorkflowWithNoStartNodeEmitCorrectDebugInfo()
        {
            string PostData = String.Format("{0}{1}", ServerSettings.WebserverURI, "Bug9484");

            Guid id = Guid.NewGuid();
            TestHelper.PostDataToWebserverAsRemoteAgent(PostData, id);

            var debugItems = TestHelper.FetchRemoteDebugItems(ServerSettings.WebserverURI, id);

            Assert.AreEqual(1, debugItems.Count);
            Assert.AreEqual("The workflow must have at least one service or activity connected to the Start Node.", debugItems[0].ErrorMessage);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("DbServices_Execute")]
        public void DbServices_Execute_WithRecordsetOutput_CorrectDebugOutput()
        {
            //------------Setup for test--------------------------
            string PostData = String.Format("{0}{1}", ServerSettings.WebserverURI, "Bug_10996");

            Guid id = Guid.NewGuid();

            //------------Execute Test---------------------------
            TestHelper.PostDataToWebserverAsRemoteAgent(PostData, id);

            var debugItems = TestHelper.FetchRemoteDebugItems(ServerSettings.WebserverURI, id);
            //------------Assert Results-------------------------
            Assert.AreEqual("Afghanistan", debugItems[0].Outputs[1].ResultsList[3].Value);
            Assert.AreEqual("1", debugItems[0].Outputs[0].ResultsList[3].Value);
            Assert.AreEqual("Azerbaijan", debugItems[0].Outputs[1].ResultsList[30].Value);
            Assert.AreEqual("10", debugItems[0].Outputs[0].ResultsList[30].Value);
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

            Assert.AreEqual(5, debugItems.Count);
        }
    }
}
