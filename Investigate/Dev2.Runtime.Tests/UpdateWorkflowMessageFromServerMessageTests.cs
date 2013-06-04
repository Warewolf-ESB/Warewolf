using System;
using System.Collections.Generic;
using Dev2.Network.Messaging.Messages;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.DynamicServices.Test
{
    /// <summary>
    /// Summary description for NetworkTest
    /// </summary>
    [TestClass]
    public class UpdateWorkflowMessageFromServerMessageTests
    {
        public UpdateWorkflowMessageFromServerMessageTests()
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
        public void WriteShouldWriteResourceIDToWriter()
        {
            //-------------------------Setup--------------------------------
            UpdateWorkflowFromServerMessage message = new UpdateWorkflowFromServerMessage();
            var resourceID = Guid.NewGuid();
            message.ResourceID = resourceID;
            var byteBuffer = new ByteBuffer();
            //-------------------------Execute --------------------------
            message.Write(byteBuffer);
            //-------------------------Assert Results------------------
            byteBuffer.Close();
            byteBuffer.Position = 0;
            var readGuid = byteBuffer.ReadGuid();
            Assert.AreEqual(resourceID,readGuid);
        }
        
        [TestMethod]
        public void ReadShouldSetResourceIDToReaderGuid()
        {
            //-------------------------Setup--------------------------------
            UpdateWorkflowFromServerMessage message = new UpdateWorkflowFromServerMessage();
            var resourceID = Guid.NewGuid();
            
            var byteBuffer = new ByteBuffer();
            byteBuffer.Write(resourceID);
            byteBuffer.Close();
            byteBuffer.Position = 0;
            //-------------------------Execute --------------------------
            message.Read(byteBuffer);
            //-------------------------Assert Results------------------
            Assert.AreEqual(resourceID,message.ResourceID);
        }        
        
        [TestMethod]
        public void ReadWhenNoGuidValueInReaderShouldSetResourceIDToEmptyGuid()
        {
            //-------------------------Setup--------------------------------
            UpdateWorkflowFromServerMessage message = new UpdateWorkflowFromServerMessage();
            var byteBuffer = new ByteBuffer();
            //-------------------------Execute --------------------------
            message.Read(byteBuffer);
            //-------------------------Assert Results------------------
            Assert.AreEqual(Guid.Empty,message.ResourceID);
        }


        [TestMethod]
        public void WriteWhenNoGuidInResourceIDShouldWriteEmptyGuidToWriter()
        {
            //-------------------------Setup--------------------------------
            UpdateWorkflowFromServerMessage message = new UpdateWorkflowFromServerMessage();
            var byteBuffer = new ByteBuffer();
            //-------------------------Execute --------------------------
            message.Write(byteBuffer);
            //-------------------------Assert Results------------------
            byteBuffer.Close();
            byteBuffer.Position = 0;
            var readGuid = byteBuffer.ReadGuid();
            Assert.AreEqual(Guid.Empty, readGuid);
        }
    }
}