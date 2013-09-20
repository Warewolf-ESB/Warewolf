using System;
using System.Security.Principal;
using Caliburn.Micro;
using Dev2.Composition;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Integration.Tests.MEF;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Core.Network;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Integration.Tests.Dev2.Application.Server.Tests.Channel_Tests
{
    [TestClass][Ignore]//Ashley: round 2 hunting the evil test
    public class DataListChannel
    {

        [TestMethod]
        public void ReadNonExistentDataList_Expected_Null()
        {
            //
            // Setup test data
            //
            var errors = new ErrorResultTO();

            //
            // Connect to the server
            //
            var conn = CreateConnection();
            conn.Connect();

            //
            // Get datalist from the server
            //
            IBinaryDataList resultDataList = conn.DataListChannel.ReadDatalist(Guid.Empty, errors);

            //
            // Clean up
            //
            conn.Disconnect();

            Assert.IsNull(resultDataList);
        }

        [TestMethod]
        public void WriteAndReadDataList_Expected_UIDsAreEqual()
        {
            //
            // Setup test data
            //
            ErrorResultTO errors = new ErrorResultTO();
            IBinaryDataList dataList = Dev2BinaryDataListFactory.CreateDataList();

            //
            // Connect to the server
            //
            var conn = CreateConnection();
            conn.Connect();

            //
            // Write the transfere datalist to the server over the datalist channel
            //
            conn.DataListChannel.WriteDataList(dataList.UID, dataList, errors);

            //
            // Get datalist from the server
            //
            IBinaryDataList resultDataList = conn.DataListChannel.ReadDatalist(dataList.UID, errors);

            //
            // Clean up
            //
            conn.Disconnect();

            Assert.AreEqual(dataList.UID, resultDataList.UID);
        }

        [TestMethod]
        public void WriteDeleteAndReadDataList_Expected_NullOnRead()
        {
            //
            // Setup test data
            //
            ErrorResultTO errors = new ErrorResultTO();
            IBinaryDataList dataList = Dev2BinaryDataListFactory.CreateDataList();

            //
            // Connect to the server
            //
            var conn = CreateConnection();
            conn.Connect();

            //
            // Write the transfere datalist to the server over the datalist channel
            //
            conn.DataListChannel.WriteDataList(dataList.UID, dataList, errors);

            //
            // Delete datalist from the server
            //
            conn.DataListChannel.DeleteDataList(dataList.UID, true);

            //
            // Try read datalist from the server
            //
            IBinaryDataList resultDataList = conn.DataListChannel.ReadDatalist(dataList.UID, errors);

            //
            // Clean up
            //
            conn.Disconnect();

            Assert.IsNull(resultDataList);
        }


        #region CreateConnection

        static TcpConnection CreateConnection(bool isAuxiliary = false)
        {
            return CreateConnection(ServerSettings.DsfAddress, isAuxiliary);
        }

        static TcpConnection CreateConnection(string appServerUri, bool isAuxiliary = false)
        {
            var securityContetxt = new Mock<IFrameworkSecurityContext>();
            securityContetxt.Setup(c => c.UserIdentity).Returns(WindowsIdentity.GetCurrent());

            return new TcpConnection(securityContetxt.Object, new Uri(appServerUri), Int32.Parse(ServerSettings.WebserverPort), isAuxiliary);
        }

        #endregion
    }
}
