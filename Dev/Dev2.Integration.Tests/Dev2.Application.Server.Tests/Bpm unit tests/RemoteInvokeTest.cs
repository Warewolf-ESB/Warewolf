using System;
using System.Text;
using System.Collections.Generic;
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
        public void CanInvokeARemoteService()
        {
            string PostData = String.Format("{0}{1}", ServerSettings.WebserverURI, "9139Local");
            string expected = @"</remoteResult><localResult>6</localResult><dbo_spGetCountries><CountryID>11</CountryID><Description>Bahrain</Description></dbo_spGetCountries><dbo_spGetCountries><CountryID>12</CountryID><Description>Bangladesh</Description></dbo_spGetCountries><dbo_spGetCountries><CountryID>13</CountryID><Description>Barbados</Description></dbo_spGetCountries><dbo_spGetCountries><CountryID>14</CountryID><Description>Belarus</Description></dbo_spGetCountries><dbo_spGetCountries><CountryID>15</CountryID><Description>Belgium</Description></dbo_spGetCountries><dbo_spGetCountries><CountryID>16</CountryID><Description>Belize</Description></dbo_spGetCountries><dbo_spGetCountries><CountryID>17</CountryID><Description>Benin</Description></dbo_spGetCountries><dbo_spGetCountries><CountryID>18</CountryID><Description>Bhutan</Description></dbo_spGetCountries><dbo_spGetCountries><CountryID>19</CountryID><Description>Bolivia</Description></dbo_spGetCountries><dbo_spGetCountries><CountryID>20</CountryID><Description>Bosnia and Herzegovina</Description></dbo_spGetCountries><dbo_spGetCountries><CountryID>21</CountryID><Description>Brazil</Description></dbo_spGetCountries><dbo_spGetCountries><CountryID>22</CountryID><Description>Brunei</Description></dbo_spGetCountries><dbo_spGetCountries><CountryID>23</CountryID><Description>Bulgaria</Description></dbo_spGetCountries></DataList>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            StringAssert.Contains(ResponseData, expected, "Expected [ " + expected + " ] But Got [ " + ResponseData + " ]");
        }
    }
}
