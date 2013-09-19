using System;
using Dev2.Development.Languages.Scripting;
using Dev2.Integration.Tests.Helpers;
using IronPython.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Integration.Tests.Dev2.Application.Server.Tests.Bpm_unit_tests
{
    /// <summary>
    /// Summary description for RemoteInvokeTest
    /// </summary>
    [TestClass][Ignore]//Ashley: One of these tests may be causing the server to hang in a background thread, preventing windows 7 build server from performing any more builds
    public class RemoteInvokeTest
    {
        public RemoteInvokeTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        [Ignore]
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
        public void CanInvokePythonScriptWithFunction()
        {
            var script = @"
    def Add(x,y):
        return x + y;
    return Add(1,1);";

            Dev2PythonContext dev2PythonContext = new Dev2PythonContext();
            string val = dev2PythonContext.Execute(script);
            
            Assert.AreEqual("2", val, "Valid Python with nested function did not evaluate");
           
        }

        [TestMethod]
        public void CanInvokePythonScriptWithoutFunction()
        {
                var script = @"
            return 1 + 1;";

            Dev2PythonContext dev2PythonContext = new Dev2PythonContext();
            string val = dev2PythonContext.Execute(script);

            Assert.AreEqual("2", val, "Valid Python with nested function did not evaluate");

        }

        [TestMethod]
        [ExpectedException(typeof(UnboundNameException))]
        public void CanInvokePythonScriptWithDoggyScriptAndReturnSensableError()
        {
            var script = @"
            return x;";

            Dev2PythonContext dev2PythonContext = new Dev2PythonContext();

            string val = dev2PythonContext.Execute(script);

            Assert.Fail("the python world has issues!");

        }

    }
}   
