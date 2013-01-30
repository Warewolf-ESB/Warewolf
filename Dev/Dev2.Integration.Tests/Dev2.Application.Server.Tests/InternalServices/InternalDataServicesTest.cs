using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.Integration.Tests.Helpers;

namespace Dev2.Integration.Tests.Dev2.Application.Server.Tests.InternalServices
{
    /// <summary>
    /// Summary description for InternalDataServicesTest
    /// </summary>
    [TestClass]
    public class InternalDataServicesTest
    {
        public InternalDataServicesTest()
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
        public void CallProcedureWithNoInput_Executes_Normally()
        {
            // http://127.0.0.1:1234/services/CallProcedureService?ServerName=RSAKLFSVRGENDEV&DatabaseName=Dev2TestingDB&Procedure=dbo.NoInputExecution&Parameters=&Mode=interrogate&Username=testuser&Password=test123

            string path = ServerSettings.WebserverURI + "CallProcedureService?ServerName=RSAKLFSVRGENDEV&DatabaseName=Dev2TestingDB&Procedure=dbo.NoInputExecution&Parameters=&Mode=interrogate&Username=testuser&Password=test123";

            string result = TestHelper.PostDataToWebserver(path);

            // 1 == 1, else an error will be thrown
            Assert.IsTrue( (result.IndexOf("No_Input_Result</SampleData>") >= 0) );
        }
         
    }
}
