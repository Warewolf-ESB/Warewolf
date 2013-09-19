using System.Activities.Presentation.Model;
using System.Collections.Generic;
using Dev2.Activities.Adorners;
using Dev2.Activities.Designers.DsfGetWebRequest;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;

namespace Dev2.Integration.Tests.Dev2.Activities.Designers.DsfGetWebRequest
{
    [TestClass][Ignore]//Ashley: One of these tests may be causing the server to hang in a background thread, preventing windows 7 build server from performing any more builds
    public class GetWebRequestViewModelTests
    {
        #region Constructor

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DsfGetWebRequestActivityViewModel_ExecutePreview")]
        public void DsfGetWebRequestActivityViewModel_ExecutePreviewWhenUrlIsValid_WebRequestIsInvoked()
        {
            var properties = new Dictionary<string, Mock<ModelProperty>>();
            var propertyCollection = new Mock<ModelPropertyCollection>();

            var url = new Mock<ModelProperty>();
            url.Setup(p => p.ComputedValue).Returns("http://rsaklfsvrdevstats:1234/services/ping");
            properties.Add("Url", url);
            propertyCollection.Protected().Setup<ModelProperty>("Find", "Url", true).Returns(url.Object);

            var modelItemMock = new Mock<ModelItem>();
            modelItemMock.Setup(s => s.Properties).Returns(propertyCollection.Object);

            var sut = new DsfGetWebRequestActivityViewModel(modelItemMock.Object);

            sut.HelpViewModel = new HelpViewModel();
            sut.Url = "";

            sut.PreviewViewModel.PreviewCommand.Execute(null);

            Assert.IsFalse(string.IsNullOrWhiteSpace(sut.PreviewViewModel.Output));
            Assert.IsTrue(sut.HelpViewModel.Errors.Count == 0);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DsfGetWebRequestActivityViewModel_ExecutePreview")]
        public void DsfGetWebRequestActivityViewModel_ExecutePreviewWhenUrlIsValidButDoesNotExist_WebRequestIsInvoked()
        {
            var properties = new Dictionary<string, Mock<ModelProperty>>();
            var propertyCollection = new Mock<ModelPropertyCollection>();

            var url = new Mock<ModelProperty>();
            url.Setup(p => p.ComputedValue).Returns("http://www.nonexistingwebsiteinpretoria.com");
            properties.Add("Url", url);
            propertyCollection.Protected().Setup<ModelProperty>("Find", "Url", true).Returns(url.Object);

            var modelItemMock = new Mock<ModelItem>();
            modelItemMock.Setup(s => s.Properties).Returns(propertyCollection.Object);

            var sut = new DsfGetWebRequestActivityViewModel(modelItemMock.Object);

            sut.HelpViewModel = new HelpViewModel();
            sut.Url = "";

            sut.PreviewViewModel.PreviewCommand.Execute(null);

            Assert.IsTrue(string.IsNullOrWhiteSpace(sut.PreviewViewModel.Output));
            Assert.IsTrue(sut.HelpViewModel.Errors.Count == 1);
        }


        #endregion
    }
}