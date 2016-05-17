using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.SaveDialog;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class ManageOAuthSourceViewModelTests
    {
        private Mock<IManageOAuthSourceModel> updateManager;

        //private Mock<Task<IRequestServiceNameViewModel>> requestServiceNameViewModel;
        private Mock<IOAuthSource> oAuthSource;

        [TestInitialize]
        public void TestInitialize()
        {
            updateManager = new Mock<IManageOAuthSourceModel>();
            //requestServiceNameViewModel = new Mock<Task<IRequestServiceNameViewModel>>();
            oAuthSource = new Mock<IOAuthSource>();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestManageOAuthSourceViewModelConstructorNullIManageOAuthSourceModel()
        {
            Task<IRequestServiceNameViewModel> requestServiceNameViewModel = new Task<IRequestServiceNameViewModel>(null);
            IManageOAuthSourceModel nullParam = null;
            new ManageOAuthSourceViewModel(nullParam, requestServiceNameViewModel);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestManageOAuthSourceViewModelConstructorNullIRequestServiceNameViewModel()
        {
            Task<IRequestServiceNameViewModel> nullParam = null;
            new ManageOAuthSourceViewModel(updateManager.Object, nullParam);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestManageOAuthSourceViewModelConstructor2NullIManageOAuthSourceModel()
        {
            IManageOAuthSourceModel nullParam = null;
            new ManageOAuthSourceViewModel(nullParam, oAuthSource.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestManageOAuthSourceViewModelConstructorNullIOAuthSource()
        {
            IOAuthSource nullParam = null;
            new ManageOAuthSourceViewModel(updateManager.Object, nullParam);
        }

        [TestMethod]
        public void TestManageOAuthSourceViewModelConstructor()
        {
            oAuthSource.SetupProperty(p => p.ResourceName, "Test");
            new ManageOAuthSourceViewModel(updateManager.Object, oAuthSource.Object);
        }
    }
}