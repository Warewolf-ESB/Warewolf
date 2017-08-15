using System;
using System.Activities.Presentation.Model;
using Dev2.Activities.Designers2.Core;
using Dev2.Activities.Designers2.Core.Extensions;
using Dev2.Activities.DropBox2016.DeleteActivity;
using Dev2.Activities.DropBox2016.DownloadActivity;
using Dev2.Activities.DropBox2016.DropboxFileActivity;
using Dev2.Activities.DropBox2016.UploadActivity;
using Dev2.Studio.Core.Activities.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Dev2.Activities.Designers.Tests.Core
{
    public class TestVm : ActivityDesignerViewModel
    {
        public TestVm(ModelItem modelItem)
            : base(modelItem)
        {
            this.RunViewSetup();
        }

        public TestVm(ModelItem modelItem, Action<Type> showExampleWorkflow)
            : base(modelItem)
        {
        }

        #region Overrides of ActivityDesignerViewModel

        public override void Validate()
        {
        }

        public override void UpdateHelpDescriptor(string helpText)
        {
        }

        #endregion
    }

    [TestClass]
    public class ActivityDesignerViewModelExentionTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void OnCreation_GivenIsDraggedTrue_ShouldShowLarge()
        {
            //---------------Set up test pack-------------------
            IsItemDragged.Instance.IsDragged = true;
            var testVm = new TestVm(ModelItemUtils.CreateModelItem(new DsfDatabaseActivity()));
            //---------------Assert Precondition----------------
            Assert.IsNotNull(testVm);
            //---------------Execute Test ----------------------
            var showLarge = testVm.ShowLarge;
            //---------------Test Result -----------------------
            Assert.IsTrue(showLarge);
            
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void OnCreation_GivenIsDraggedTrue_ShouldShowLargeOnAllTools()
        {
            //---------------Set up test pack-------------------
            IsItemDragged.Instance.IsDragged = true;
            var testVm = new TestVm(ModelItemUtils.CreateModelItem(new DsfDatabaseActivity()));
            IsItemDragged.Instance.IsDragged = true;
            var testVm1 = new TestVm(ModelItemUtils.CreateModelItem(new DsfDotNetDllActivity()));
            IsItemDragged.Instance.IsDragged = true;
            var testVm2 = new TestVm(ModelItemUtils.CreateModelItem(new DsfMySqlDatabaseActivity()));
            IsItemDragged.Instance.IsDragged = true;
            var testVm3 = new TestVm(ModelItemUtils.CreateModelItem(new DsfOracleDatabaseActivity()));
            IsItemDragged.Instance.IsDragged = true;
            var testVm4 = new TestVm(ModelItemUtils.CreateModelItem(new DsfDropBoxDeleteActivity()));
            IsItemDragged.Instance.IsDragged = true;
            var testVm5 = new TestVm(ModelItemUtils.CreateModelItem(new DsfDropBoxDownloadActivity()));
            IsItemDragged.Instance.IsDragged = true;
            var testVm6 = new TestVm(ModelItemUtils.CreateModelItem(new DsfDropBoxUploadActivity()));
            IsItemDragged.Instance.IsDragged = true;
            var testVm7 = new TestVm(ModelItemUtils.CreateModelItem(new DsfDropboxFileListActivity()));
            //---------------Assert Precondition----------------
            Assert.IsNotNull(testVm);
            Assert.IsNotNull(testVm1);
            Assert.IsNotNull(testVm2);
            Assert.IsNotNull(testVm3);
            Assert.IsNotNull(testVm4);
            Assert.IsNotNull(testVm5);
            Assert.IsNotNull(testVm6);
            Assert.IsNotNull(testVm7);
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.IsTrue(testVm.ShowLarge);
            Assert.IsTrue(testVm1.ShowLarge);
            Assert.IsTrue(testVm2.ShowLarge);
            Assert.IsTrue(testVm3.ShowLarge);
            Assert.IsTrue(testVm4.ShowLarge);
            Assert.IsTrue(testVm5.ShowLarge);
            Assert.IsTrue(testVm6.ShowLarge);
            Assert.IsTrue(testVm7.ShowLarge);
            
        }
        
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void OnCreation_GivenIsDraggedFalse_ShouldNotShowLarge()
        {
            //---------------Set up test pack-------------------
            IsItemDragged.Instance.IsDragged = false;
            var testVm = new TestVm(ModelItemUtils.CreateModelItem(new DsfDatabaseActivity()));
            //---------------Assert Precondition----------------
            Assert.IsNotNull(testVm);
            //---------------Execute Test ----------------------
            var showLarge = testVm.ShowLarge;
            //---------------Test Result -----------------------
            Assert.IsFalse(showLarge);
        }
        
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void OnTabChanged_GivenIsDraggedFalse_ShouldNotShowLarge()
        {
            //---------------Set up test pack-------------------
            IsItemDragged.Instance.IsDragged = true;
            var testVm = new TestVm(ModelItemUtils.CreateModelItem(new DsfDatabaseActivity()));
            //---------------Assert Precondition----------------
            Assert.IsNotNull(testVm);
            var showLarge = testVm.ShowLarge;
            Assert.IsTrue(showLarge);
            //---------------Execute Test ----------------------
            testVm = new TestVm(ModelItemUtils.CreateModelItem(new DsfDatabaseActivity()));
            //---------------Test Result -----------------------
            Assert.IsFalse(testVm.ShowLarge);
        }
    }
}
