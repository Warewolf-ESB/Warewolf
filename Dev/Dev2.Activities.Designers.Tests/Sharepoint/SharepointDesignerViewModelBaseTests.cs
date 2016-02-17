using System;
using System.Activities.Presentation.Model;
using Caliburn.Micro;
using Dev2.Activities.Designers2.SharepointListRead;
using Dev2.Activities.Sharepoint;
using Dev2.Common.Interfaces.Threading;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.Interfaces;
using Dev2.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
// ReSharper disable InconsistentNaming

namespace Dev2.Activities.Designers.Tests.Sharepoint
{
    [TestClass]
    public class SharepointDesignerViewModelBaseTests
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SharepointListDesignerViewModelBase_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SharepointListDesignerViewModelBase_Constructor_NullModelItem_ThrowsException()
        {
            //------------Setup for test--------------------------
            
            
            //------------Execute Test---------------------------
            var sharepointListDesignerViewModelBase = new TestSharepointListDesignerViewModelBase(null, new SynchronousAsyncWorker(), new Mock<IEnvironmentModel>().Object, new Mock<IEventAggregator>().Object, false);
            //------------Assert Results-------------------------
            Assert.IsNull(sharepointListDesignerViewModelBase);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SharepointListDesignerViewModelBase_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SharepointListDesignerViewModelBase_Constructor_NullAsyncWorker_ThrowsException()
        {
            //------------Setup for test--------------------------


            //------------Execute Test---------------------------
            var sharepointListDesignerViewModelBase = new TestSharepointListDesignerViewModelBase(CreateModelItem(), null, new Mock<IEnvironmentModel>().Object, new Mock<IEventAggregator>().Object, false);
            //------------Assert Results-------------------------
            Assert.IsNull(sharepointListDesignerViewModelBase);
        }        
        
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SharepointListDesignerViewModelBase_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SharepointListDesignerViewModelBase_Constructor_NullEnvironmentModel_ThrowsException()
        {
            //------------Setup for test--------------------------


            //------------Execute Test---------------------------
            var sharepointListDesignerViewModelBase = new TestSharepointListDesignerViewModelBase(CreateModelItem(), new SynchronousAsyncWorker(), null, new Mock<IEventAggregator>().Object, false);
            //------------Assert Results-------------------------
            Assert.IsNull(sharepointListDesignerViewModelBase);
        }  
      
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SharepointListDesignerViewModelBase_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SharepointListDesignerViewModelBase_Constructor_NullEventAggregator_ThrowsException()
        {
            //------------Setup for test--------------------------


            //------------Execute Test---------------------------
            var sharepointListDesignerViewModelBase = new TestSharepointListDesignerViewModelBase(CreateModelItem(), new SynchronousAsyncWorker(), new Mock<IEnvironmentModel>().Object, null, false);
            //------------Assert Results-------------------------
            Assert.IsNull(sharepointListDesignerViewModelBase);
        }



        static ModelItem CreateModelItem()
        {
            return ModelItemUtils.CreateModelItem(new SharepointReadListActivity());
        }
    }

    public class TestSharepointListDesignerViewModelBase : SharepointListDesignerViewModelBase
    {
        public TestSharepointListDesignerViewModelBase(ModelItem modelItem, IAsyncWorker asyncWorker, IEnvironmentModel environmentModel, IEventAggregator eventPublisher, bool loadOnlyEditableFields)
            : base(modelItem, asyncWorker, environmentModel, eventPublisher, loadOnlyEditableFields)
        {
        }

        #region Overrides of ActivityDesignerViewModel

        public override void UpdateHelpDescriptor(string helpText)
        {
        }

        #endregion

        #region Overrides of ActivityCollectionDesignerViewModel

        public override string CollectionName
        {
            get
            {
                return "MyCollection";
            }
        }

        #endregion
    }
}
