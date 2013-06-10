using System;
using System.Text;
using System.Collections.Generic;
using Dev2.Integration.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Integration.Tests.Dev2.Application.Server.Tests.Bpm_unit_tests
{
    /// <summary>
    /// Summary description for DBServiceTest
    /// </summary>
    [TestClass]
    public class DBServiceTest
    {
        public DBServiceTest()
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
        public void CanExecuteDBServiceAndReturnItsOutput()
        {
            string PostData = String.Format("{0}{1}?{2}", ServerSettings.WebserverURI, "bug9394DBServicesCall", "<ADL><Prefix>a</Prefix></ADL>");
            string expected = @"<DataList><dbo_spGetCountries><CountryID>1</CountryID><Description>Afghanistan</Description></dbo_spGetCountries><dbo_spGetCountries><CountryID>2</CountryID><Description>Albania</Description></dbo_spGetCountries><dbo_spGetCountries><CountryID>3</CountryID><Description>Algeria</Description></dbo_spGetCountries><dbo_spGetCountries><CountryID>4</CountryID><Description>Andorra</Description></dbo_spGetCountries><dbo_spGetCountries><CountryID>5</CountryID><Description>Angola</Description></dbo_spGetCountries><dbo_spGetCountries><CountryID>6</CountryID><Description>Argentina</Description></dbo_spGetCountries><dbo_spGetCountries><CountryID>7</CountryID><Description>Armenia</Description></dbo_spGetCountries><dbo_spGetCountries><CountryID>8</CountryID><Description>Australia</Description></dbo_spGetCountries><dbo_spGetCountries><CountryID>9</CountryID><Description>Austria</Description></dbo_spGetCountries><dbo_spGetCountries><CountryID>10</CountryID><Description>Azerbaijan</Description></dbo_spGetCountries></DataList>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            StringAssert.Contains(ResponseData, expected, "Expected [ " + expected + " ] But Got [ " + ResponseData + " ]");
        }


    }
}
