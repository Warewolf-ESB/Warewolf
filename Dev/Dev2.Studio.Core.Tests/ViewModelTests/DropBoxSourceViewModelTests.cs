using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using Dev2.Views.DropBox;
using Dev2.Webs.Callbacks;
using DropNet;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests.ViewModelTests
{
    [TestClass]
    public class DropBoxSourceViewModelTests
    {

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DropBoxSourceViewModel_Ctor")]
        [ExpectedException(typeof(ArgumentNullException))]
        // ReSharper disable InconsistentNaming
        public void DropBoxSourceViewModel_Ctor_NullParams_ExpectException()
           
        {
            //------------Setup for test--------------------------
            // ReSharper disable once ObjectCreationAsStatement
            new DropBoxSourceViewModel(null, new Mock<IDropBoxHelper>().Object,new DropboxFactory(),false );

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DropBoxSourceViewModel_Ctor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DropBoxSourceViewModel_Ctor_NullParams_ExpectException_helper()
        {
            //------------Setup for test--------------------------
            // ReSharper disable once ObjectCreationAsStatement
            new DropBoxSourceViewModel(new Mock<INetworkHelper>().Object, null, new DropboxFactory(), false);

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DropBoxSourceViewModel_Ctor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DropBoxSourceViewModel_Ctor_NullParams_ExpectException_factory()
        {
            //------------Setup for test--------------------------
            // ReSharper disable once ObjectCreationAsStatement
            new DropBoxSourceViewModel(new Mock<INetworkHelper>().Object, new Mock<IDropBoxHelper>().Object, null, false);

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadg")]
        [TestCategory("HelpViewModel_OnViewLoaded")]
        public async Task DropBoxSourceViewModel_LoadBrowserUri_HasInternetConnection_Navigates()
        {
            //------------Setup for test--------------------------
            const string uri = "http://community.warewolf.io/";
            var networkHelper = new Mock<INetworkHelper>();
            var dropFactory = new Mock<IDropboxFactory>();
            dropFactory.Setup(a => a.Create()).Returns(new Mock<IDropNetClient>().Object);
            var task = new Task<bool>(() => true);
            task.RunSynchronously();
            networkHelper.Setup(m => m.HasConnectionAsync(It.IsAny<string>()))
                .Returns(task);
            var helpViewWrapper = new Mock<IDropBoxHelper>();
            helpViewWrapper.Setup(m => m.Navigate(It.IsAny<string>())).Verifiable();
            helpViewWrapper.Setup(a => a.WebBrowser).Returns(new WebBrowser());
            var helpViewModel = new DropBoxSourceViewModel(networkHelper.Object, helpViewWrapper.Object, dropFactory.Object,false);
            var helpView = new DropBoxViewWindow();
            helpViewWrapper.SetupGet(m => m.DropBoxViewWindow).Returns(helpView);
            //------------Execute Test---------------------------
            await helpViewModel.LoadBrowserUri(uri);
            //------------Assert Results-------------------------
            helpViewWrapper.Verify(m => m.Navigate(It.IsAny<string>()), Times.AtLeastOnce());

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadg")]
        [TestCategory("DropSource_OnViewLoaded")]
        public async Task DropBoxSourceViewModel_LoadBrowserUri_HasNoInternetConnection_NavigatesNowhere()
        {
            //------------Setup for test--------------------------
            const string uri = "http://community.warewolf.io/";
            var networkHelper = new Mock<INetworkHelper>();
            var dropFactory = new Mock<IDropboxFactory>();
            dropFactory.Setup(a => a.Create()).Returns(new Mock<IDropNetClient>().Object);
            var task = new Task<bool>(() => false);
            task.RunSynchronously();
            networkHelper.Setup(m => m.HasConnectionAsync(It.IsAny<string>()))
                .Returns(task);
            var helpViewWrapper = new Mock<IDropBoxHelper>();
            helpViewWrapper.Setup(m => m.Navigate(It.IsAny<string>())).Verifiable();
            helpViewWrapper.Setup(a => a.WebBrowser).Returns(new WebBrowser());
            var helpViewModel = new DropBoxSourceViewModel(networkHelper.Object, helpViewWrapper.Object, dropFactory.Object,false);
            var helpView = new DropBoxViewWindow();
            helpViewWrapper.SetupGet(m => m.DropBoxViewWindow).Returns(helpView);
            //------------Execute Test---------------------------
            await helpViewModel.LoadBrowserUri(uri);
            //------------Assert Results-------------------------
            helpViewWrapper.Verify(m => m.Navigate(It.IsAny<string>()), Times.Never());

        }
        // ReSharper restore InconsistentNaming
    }
}
