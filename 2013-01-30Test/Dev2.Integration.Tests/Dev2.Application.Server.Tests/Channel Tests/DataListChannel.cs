using System;
using Dev2.Composition;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Integration.Tests.MEF;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Integration.Tests.Dev2.Application.Server.Tests.Channel_Tests
{
    [TestClass]
    public class DataListChannel
    {
        [TestMethod]
        public void ReadNonExistentDataList_Expected_Null()
        {
            //
            // Setup MEF context
            //
            ImportService.CurrentContext = CompositionInitializer.DefaultInitialize();

            //
            // Setup test data
            //
            ErrorResultTO errors = new ErrorResultTO();

            //
            // Connect to the server
            //
            IEnvironmentConnection conn = new EnvironmentConnection(Guid.NewGuid().ToString(), "asd");
            conn.Address = new Uri(ServerSettings.DsfAddress);
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
            // Setup MEF context
            //
            ImportService.CurrentContext = CompositionInitializer.DefaultInitialize();

            //
            // Setup test data
            //
            ErrorResultTO errors = new ErrorResultTO();
            IBinaryDataList dataList = Dev2BinaryDataListFactory.CreateDataList();

            //
            // Connect to the server
            //
            IEnvironmentConnection conn = new EnvironmentConnection(Guid.NewGuid().ToString(), "asd");
            conn.Address = new Uri(ServerSettings.DsfAddress);
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
            // Setup MEF context
            //
            ImportService.CurrentContext = CompositionInitializer.DefaultInitialize();

            //
            // Setup test data
            //
            ErrorResultTO errors = new ErrorResultTO();
            IBinaryDataList dataList = Dev2BinaryDataListFactory.CreateDataList();

            //
            // Connect to the server
            //
            IEnvironmentConnection conn = new EnvironmentConnection(Guid.NewGuid().ToString(), "asd");
            conn.Address = new Uri(ServerSettings.DsfAddress);
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
    }
}
