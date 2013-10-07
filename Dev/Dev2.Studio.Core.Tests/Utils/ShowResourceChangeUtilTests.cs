using System;
using System.Collections.Generic;
using Caliburn.Micro;
using Dev2.Providers.Events;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

// ReSharper disable InconsistentNaming
namespace Dev2.Core.Tests.Utils
{
    [TestClass]
    public class ShowResourceChangeUtilTests
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ShowResourceChangedUtil_Construct")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShowResourceChangedUtil_Constructor_WithNullEventPublisher_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            new ShowResourceChangedUtil(null);
            //------------Assert Results-------------------------
        }     
        
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ShowResourceChangedUtil_ShowResourceChanged")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShowResourceChangedUtil_ShowResourceChanged_WithNullResource_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------
            var showResourceChangedUtil = CreateShowResourceChangedUtil();
            //------------Execute Test---------------------------
            showResourceChangedUtil.ShowResourceChanged(null,new List<string>());
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ShowResourceChangedUtil_ShowResourceChanged")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShowResourceChangedUtil_ShowResourceChanged_WithNullList_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------
            var showResourceChangedUtil = CreateShowResourceChangedUtil();
            //------------Execute Test---------------------------
            showResourceChangedUtil.ShowResourceChanged(new Mock<IContextualResourceModel>().Object,null);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ShowResourceChangedUtil_ShowResourceChanged")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShowResourceChangedUtil_ShowResourceChanged_WithOneDependant_FiresMessage()
        {
            //------------Setup for test--------------------------
            var mockAggregator = new Mock<IEventAggregator>();
            var showResourceChangedUtil = CreateShowResourceChangedUtil(mockAggregator.Object);
            var mockResource = new Mock<IContextualResourceModel>();
            var mockEnvironmentModel = new Mock<IEnvironmentModel>();
            var mockResourceRepository = new Mock<IResourceRepository>();
            var mockResourceModelDependant = new Mock<IResourceModel>();
            mockResourceRepository.Setup(repository => repository.FindSingle(model => true)).Returns(mockResourceModelDependant.Object);
            mockEnvironmentModel.Setup(model => model.ResourceRepository).Returns(mockResourceRepository.Object);
            mockResource.Setup(model => model.Environment).Returns(mockEnvironmentModel.Object);
            //------------Execute Test---------------------------
            showResourceChangedUtil.ShowResourceChanged(mockResource.Object,null);
            //------------Assert Results-------------------------
            mockAggregator.Verify(aggregator => aggregator.Publish(It.IsAny<AddWorkSurfaceMessage>()),Times.Once());
        }

        static ShowResourceChangedUtil CreateShowResourceChangedUtil()
        {
            return CreateShowResourceChangedUtil(new Mock<IEventAggregator>().Object);
        }
        
        static ShowResourceChangedUtil CreateShowResourceChangedUtil(IEventAggregator eventAggregator)
        {
            return new ShowResourceChangedUtil(eventAggregator);
        }
    }
}
