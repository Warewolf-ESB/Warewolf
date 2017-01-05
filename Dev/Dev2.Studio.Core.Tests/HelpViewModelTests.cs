/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Threading.Tasks;
using System.Windows.Controls;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.ViewModels.Help;
using Dev2.Studio.Views.Help;
using Dev2.ViewModels.Help;
using Dev2.Webs.Callbacks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests
{
    // ReSharper disable InconsistentNaming
    [TestClass]
    public class HelpViewModelTests
    {
        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("HelpViewModel_OnViewLoaded")]
        public void HelpViewModel_OnViewLoaded_ValidHelpView_HelpViewIsSet()
        {
            //------------Setup for test--------------------------
           // const string uri = "http://community.warewolf.io/";
            var networkHelper = new Mock<INetworkHelper>();
            var helpViewModel = new HelpViewModel(networkHelper.Object,null, true);// { Uri = uri };
            var helpViewWrapper = new Mock<IHelpViewWrapper>(); 
            //------------Execute Test---------------------------
            helpViewModel.OnViewisLoaded(helpViewWrapper.Object);
            //------------Assert Results-------------------------
            Assert.AreEqual(helpViewWrapper.Object, helpViewModel.HelpViewWrapper);
            Assert.IsTrue(helpViewModel.IsViewAvailable);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("HelpViewModel_OnViewLoaded")]
        public void HelpViewModel_OnViewLoaded_HelpViewIsNull_IsViewAvailableIsFalse()
        {
            //------------Setup for test--------------------------
            //const string uri = "http://community.warewolf.io/";
            var networkHelper = new Mock<INetworkHelper>();
            var helpViewModel = new HelpViewModel(networkHelper.Object,null, false);
            //------------Execute Test---------------------------
            helpViewModel.OnViewisLoaded(null);
            //------------Assert Results-------------------------
            Assert.AreEqual(null, helpViewModel.HelpViewWrapper);
            Assert.IsFalse(helpViewModel.IsViewAvailable);
        }
        
        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("HelpViewModel_OnViewLoaded")]
        public async Task HelpViewModel_LoadBrowserUri_HasInternetConnection_NavigatesToUrl()
        {
            //------------Setup for test--------------------------
            const string uri = "http://community.warewolf.io/";
            var networkHelper = new Mock<INetworkHelper>();
            var task = new Task<bool>(() => true);
            task.RunSynchronously();
            networkHelper.Setup(m => m.HasConnectionAsync(It.IsAny<string>()))
                .Returns(task);
            var helpViewWrapper = new Mock<IHelpViewWrapper>();
            Frame webBrowser = new Frame();   
            helpViewWrapper.SetupGet(m => m.WebBrowser).Returns(webBrowser);
            helpViewWrapper.Setup(m => m.Navigate(It.IsAny<string>())).Verifiable();
            var helpViewModel = new HelpViewModel(networkHelper.Object, helpViewWrapper.Object, false);
            HelpView helpView = new HelpView();
            helpViewWrapper.SetupGet(m => m.HelpView).Returns(helpView);
            //------------Execute Test---------------------------
            await helpViewModel.LoadBrowserUri(uri);
            //------------Assert Results-------------------------
            helpViewWrapper.Verify(m => m.Navigate(It.IsAny<string>()), Times.Once());
            Assert.IsNotNull(helpViewModel.Uri);
            Assert.IsNull(helpViewModel.ResourcePath);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("HelpViewModel_Constructor")]
        public void HelpViewModel_Constructor_IsViewAvailableIsInitializedToTrue()
        {
            //------------Execute Test---------------------------
            var helpViewModel = new HelpViewModel();
            //------------Assert Results-------------------------
            Assert.IsTrue(helpViewModel.IsViewAvailable);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("HelpViewModel_Constructor")]
        public void HelpViewModel_Constructor_WorkSurfaceContextIsInitializedToHelp()
        {
            //------------Execute Test---------------------------
            var helpViewModel = new HelpViewModel();
            //------------Assert Results-------------------------
            Assert.AreEqual(WorkSurfaceContext.Help, helpViewModel.WorkSurfaceContext);
        }        
    }
}
    
