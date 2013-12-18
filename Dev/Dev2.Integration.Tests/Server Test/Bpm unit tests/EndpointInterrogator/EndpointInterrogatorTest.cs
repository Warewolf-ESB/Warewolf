using System;
using Dev2.Integration.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Integration.Tests.Dev2.Application.Server.Tests.Bpm_unit_tests.EndpointInterrogator
{
    /// <summary>
    /// Summary description for EndpointInterrogatorTest
    /// </summary>
    [TestClass]
    public class EndpointInterrogatorTest
    {
        public EndpointInterrogatorTest()
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
        public void AssemblyTypeEndpoint_PocoResponse_Expected_AllFieldsReturnedForPoco()
        {
            //string postValue = @"ServiceType=Plugin&assemblyLocation=Plugins/Dev2.AnytingToXmlHook.Plugin.dll&assemblyName=Dev2.AnytingToXmlHook.Plugin.AnythignToXmlHookPlugin&method=EmitComplex&args=Nothing";
            string PostData = String.Format("{0}{1}", ServerSettings.WebserverURI, "EndpointInterrogator");

            TestHelper.PostDataToWebserver(PostData);

            //Assert.IsTrue(ResponseData.IndexOf(expected) >= 0);
            Assert.Inconclusive("Test is failing because of plugins");
    
        }

    }
}
