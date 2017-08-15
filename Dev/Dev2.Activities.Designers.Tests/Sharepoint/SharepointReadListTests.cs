using System;
using System.Activities.Presentation.Model;
using Dev2.Activities.Designers2.SharepointListRead;
using Dev2.Activities.Sharepoint;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Interfaces;
using Dev2.Threading;
using Dev2.TO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;



namespace Dev2.Activities.Designers.Tests.Sharepoint
{
    [TestClass]
    public class SharepointReadListTests
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SharepointListReadDesignerViewModel_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SharepointListReadDesignerViewModel_Constructor_NullModelItem_ThrowArgumentNullException()
        {
            //------------Setup for test--------------------------
            
            
            //------------Execute Test---------------------------
            var sharepointReadListViewModel = new SharepointListReadDesignerViewModel(null);
            //------------Assert Results-------------------------
            Assert.IsNull(sharepointReadListViewModel);
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SharepointListReadDesignerViewModel_CollectionName")]
        public void SharepointListReadDesignerViewModel_CollectionName_Property_ReturnsFilterCriteria()
        {
            //------------Setup for test--------------------------
            var sharepointListReadDesignerViewModel = new SharepointListReadDesignerViewModel(CreateModelItem(),new SynchronousAsyncWorker(), new Mock<IServer>().Object);
            
            //------------Execute Test---------------------------
            var collectionName = sharepointListReadDesignerViewModel.CollectionName;
            //------------Assert Results-------------------------
            Assert.AreEqual("FilterCriteria", collectionName);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SharepointListReadDesignerViewModel_WhereOptions")]
        public void SharepointListReadDesignerViewModel_WhereOptions_Constructor_ShouldBePopulatedWithCorrectOptions()
        {
            //------------Setup for test--------------------------
            
            
            //------------Execute Test---------------------------
            var sharepointListReadDesignerViewModel = new SharepointListReadDesignerViewModel(CreateModelItem(), new SynchronousAsyncWorker(), new Mock<IServer>().Object);
            //------------Assert Results-------------------------
            Assert.IsNotNull(sharepointListReadDesignerViewModel);
            Assert.IsNotNull(sharepointListReadDesignerViewModel.WhereOptions);
            Assert.AreEqual(9,sharepointListReadDesignerViewModel.WhereOptions.Count);
            CollectionAssert.AreEqual(SharepointSearchOptions.SearchOptions(),sharepointListReadDesignerViewModel.WhereOptions);
        }

        static ModelItem CreateModelItem()
        {
            return ModelItemUtils.CreateModelItem(new SharepointReadListActivity());
        }
    }
}
