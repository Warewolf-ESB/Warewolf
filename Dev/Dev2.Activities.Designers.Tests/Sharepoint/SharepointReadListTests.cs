using System;
using System.Activities.Presentation.Model;
using Dev2.Activities.Designers2.SharepointListRead;
using Dev2.Activities.Sharepoint;
using Dev2.Studio.Core.Activities.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
// ReSharper disable InconsistentNaming

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
            var sharepointListReadDesignerViewModel = new SharepointListReadDesignerViewModel(CreateModelItem());
            
            //------------Execute Test---------------------------
            var collectionName = sharepointListReadDesignerViewModel.CollectionName;
            //------------Assert Results-------------------------
            Assert.AreEqual("FilterCriteria", collectionName);
        }

        static ModelItem CreateModelItem()
        {
            return ModelItemUtils.CreateModelItem(new SharepointReadListActivity());
        }
    }
}
