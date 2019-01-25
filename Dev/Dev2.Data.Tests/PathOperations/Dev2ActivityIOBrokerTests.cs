using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Data.Interfaces;
using Dev2.Data.Tests.Operations;
using Dev2.PathOperations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Data.Tests.PathOperations
{
    [TestClass]
    public class Dev2ActivityIOBrokerTests : Dev2ActivityIOBrokerTests_old
    {
        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(Dev2ActivityIOBroker))]
        public void Dev2ActivityIOBroker_Construct()
        {
            var mockFileWrapper = new Mock<IFile>();
            var mockCommonData = new Mock<ICommon>();

            var broker = new Dev2ActivityIOBroker(mockFileWrapper.Object, mockCommonData.Object);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(Dev2ActivityIOBroker))]
        public void Dev2ActivityIOBroker_Get()
        {
            var mockFileWrapper = new Mock<IFile>();
            var mockCommonData = new Mock<ICommon>();
            var mockPath = new Mock<IActivityIOOperationsEndPoint>();
            var mockIoPath = new Mock<IActivityIOPath>();
            var ioPath = mockIoPath.Object;
            mockPath.Setup(o => o.IOPath).Returns(ioPath);
            mockPath.Setup(o => o.Get(ioPath, It.IsAny<List<string>>()))
                .Callback<IActivityIOPath, List<string>>((path, filesToRemove) => Assert.AreEqual(0, filesToRemove.Count))
                .Returns(new MemoryStream(Encoding.UTF8.GetBytes("some string")));

            var broker = new Dev2ActivityIOBroker(mockFileWrapper.Object, mockCommonData.Object);
            var result = broker.Get(mockPath.Object);

            Assert.AreEqual("some string", result);

            mockPath.Verify(o => o.IOPath, Times.Exactly(1));
            mockPath.Verify(o => o.Get(ioPath, It.IsAny<List<string>>()), Times.Once);
        }

        // PutRaw
    }
}
