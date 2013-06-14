using System;
using System.Text;
using System.Collections.Generic;
using Dev2.Studio.AppResources.Comparers;
using Dev2.Studio.Factory;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Core.Tests.AppResources.Comparers
{
    /// <summary>
    /// Summary description for WorkSurfaceKeyEqualityComparerTests
    /// </summary>
    [TestClass]
    public class WorkSurfaceKeyEqualityComparerTests
    {
        public WorkSurfaceKeyEqualityComparerTests()
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
        public void CreateKeysExpectedKeysCreated()
        {
            var resId = Guid.NewGuid();
            var serverId = Guid.NewGuid();
            var enviroId = Guid.NewGuid();

            var resource1 = Dev2MockFactory.SetupResourceModelMock();
            resource1.Setup(c=>c.ID).Returns(resId);
            resource1.Setup(c=>c.ServerID).Returns(serverId);
            resource1.Setup(c=>c.Environment.ID).Returns(enviroId);
            
            var key1 = WorkSurfaceKeyFactory.CreateKey(resource1.Object);
                        
            var enviroId2 = Guid.NewGuid();

            var resource2 = Dev2MockFactory.SetupResourceModelMock();
            resource2.Setup(c => c.ID).Returns(resId);
            resource2.Setup(c => c.ServerID).Returns(serverId);
            resource2.Setup(c => c.Environment.ID).Returns(enviroId2);

            var key2 = WorkSurfaceKeyFactory.CreateKey(resource2.Object);            
            if (WorkSurfaceKeyEqualityComparer.Current.Equals(key1,key2))
             {
                 Assert.Fail("The keys should not be the same as they are from two different environments.");
             }            
        }
    }
}
