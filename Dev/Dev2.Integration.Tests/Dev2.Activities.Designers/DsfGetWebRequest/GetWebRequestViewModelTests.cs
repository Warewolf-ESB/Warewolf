using System.Activities.Presentation.Model;
using System.Collections.Generic;
using Dev2.Activities.Designers2.GetWebRequest;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;

namespace Dev2.Integration.Tests.Dev2.Activities.Designers.DsfGetWebRequest
{
    [TestClass]
    public class GetWebRequestViewModelTests
    {
        #region Constructor

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("GetWebRequestDesignerViewModel_ExecutePreview")]
        public void GetWebRequestDesignerViewModel_ExecutePreviewWhenUrlIsValid_WebRequestIsInvoked()
        {
            var properties = new Dictionary<string, Mock<ModelProperty>>();
            var propertyCollection = new Mock<ModelPropertyCollection>();

            var url = new Mock<ModelProperty>();
            url.Setup(p => p.ComputedValue).Returns("http://rsaklfsvrdevstats:1234/services/ping");
            properties.Add("Url", url);
            propertyCollection.Protected().Setup<ModelProperty>("Find", "Url", true).Returns(url.Object);

            var modelItemMock = new Mock<ModelItem>();
            modelItemMock.Setup(s => s.Properties).Returns(propertyCollection.Object);

            var sut = new GetWebRequestDesignerViewModel(modelItemMock.Object);

            sut.Url = "";

            sut.PreviewViewModel.PreviewCommand.Execute(null);

            Assert.IsFalse(string.IsNullOrWhiteSpace(sut.PreviewViewModel.Output));
            Assert.IsNull(sut.Errors);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("GetWebRequestDesignerViewModel_ExecutePreview")]
        public void GetWebRequestDesignerViewModel_ExecutePreviewWhenUrlIsValidButDoesNotExist_WebRequestIsInvoked()
        {
            var properties = new Dictionary<string, Mock<ModelProperty>>();
            var propertyCollection = new Mock<ModelPropertyCollection>();

            var url = new Mock<ModelProperty>();
            url.Setup(p => p.ComputedValue).Returns("http://www.nonexistingwebsiteinpretoria.com");
            properties.Add("Url", url);
            propertyCollection.Protected().Setup<ModelProperty>("Find", "Url", true).Returns(url.Object);

            var modelItemMock = new Mock<ModelItem>();
            modelItemMock.Setup(s => s.Properties).Returns(propertyCollection.Object);

            var sut = new GetWebRequestDesignerViewModel(modelItemMock.Object);

            sut.Url = "";

            sut.PreviewViewModel.PreviewCommand.Execute(null);

            Assert.IsTrue(string.IsNullOrWhiteSpace(sut.PreviewViewModel.Output));
            Assert.IsTrue(sut.Errors.Count == 1);
        }


        #endregion
    }
}