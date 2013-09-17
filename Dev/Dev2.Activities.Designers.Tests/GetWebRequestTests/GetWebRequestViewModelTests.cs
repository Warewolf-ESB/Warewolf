using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Windows;
using Dev2.Activities.Adorners;
using Dev2.Activities.Designers.DsfGetWebRequest;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;

namespace Dev2.Activities.Designers.Tests.GetWebRequestTests
{
    [TestClass]
    public class GetWebRequestViewModelTests
    {
        #region Constructor

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DsfGetWebRequestActivityViewModel_Constructor")]
        public void DsfGetWebRequestActivityViewModel_Constructor_PreviewViewModel_NotNull()
        {
            var modelItemMock = new Mock<ModelItem>();
            var sut = new DsfGetWebRequestActivityViewModel(modelItemMock.Object);
            Assert.IsNotNull(sut.PreviewViewModel);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DsfGetWebRequestActivityViewModel_UrlSet")]
        public void DsfGetWebRequestActivityViewModel_SetUrl_EmptyString_CanPreviewIsFalse()
        {
            var properties = new Dictionary<string, Mock<ModelProperty>>();
            var propertyCollection = new Mock<ModelPropertyCollection>();

            var url = new Mock<ModelProperty>();
            url.Setup(p => p.ComputedValue).Returns("");
            properties.Add("Url", url);
            propertyCollection.Protected().Setup<ModelProperty>("Find", "Url", true).Returns(url.Object);

            var modelItemMock = new Mock<ModelItem>();
            modelItemMock.Setup(s => s.Properties).Returns(propertyCollection.Object);

            var sut = new DsfGetWebRequestActivityViewModel(modelItemMock.Object);
            sut.Url = "";

            Assert.IsFalse(sut.PreviewViewModel.CanPreview);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DsfGetWebRequestActivityViewModel_UrlSet")]
        public void DsfGetWebRequestActivityViewModel_SetUrl_StringWithoutVariables_CanPreviewIsTrueButPreviewInputsCountIsZero()
        {
            var properties = new Dictionary<string, Mock<ModelProperty>>();
            var propertyCollection = new Mock<ModelPropertyCollection>();

            var url = new Mock<ModelProperty>();
            url.Setup(p => p.ComputedValue).Returns("http://www.google.com");
            properties.Add("Url", url);
            propertyCollection.Protected().Setup<ModelProperty>("Find", "Url", true).Returns(url.Object);

            var modelItemMock = new Mock<ModelItem>();
            modelItemMock.Setup(s => s.Properties).Returns(propertyCollection.Object);

            var sut = new DsfGetWebRequestActivityViewModel(modelItemMock.Object);
            sut.Url = "";

            Assert.IsTrue(sut.PreviewViewModel.CanPreview);
            Assert.IsTrue(sut.PreviewViewModel.Inputs.Count == 0);
            Assert.AreEqual(sut.PreviewViewModel.InputsVisibility, Visibility.Collapsed);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DsfGetWebRequestActivityViewModel_UrlSet")]
        public void DsfGetWebRequestActivityViewModel_SetUrl_StringWithTwoVariables_CanPreviewIsTrueAndPreviewInputsCountIsTwo()
        {
            var properties = new Dictionary<string, Mock<ModelProperty>>();
            var propertyCollection = new Mock<ModelPropertyCollection>();

            var url = new Mock<ModelProperty>();
            url.Setup(p => p.ComputedValue).Returns("http://www.[[mysite]].com?[[queryString]]");
            properties.Add("Url", url);
            propertyCollection.Protected().Setup<ModelProperty>("Find", "Url", true).Returns(url.Object);

            var modelItemMock = new Mock<ModelItem>();
            modelItemMock.Setup(s => s.Properties).Returns(propertyCollection.Object);

            var sut = new DsfGetWebRequestActivityViewModel(modelItemMock.Object);
            sut.Url = "";

            Assert.IsTrue(sut.PreviewViewModel.CanPreview);
            Assert.IsTrue(sut.PreviewViewModel.Inputs.Count == 2);
            Assert.AreEqual(sut.PreviewViewModel.InputsVisibility, Visibility.Visible);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DsfGetWebRequestActivityViewModel_HeadersSet")]
        public void DsfGetWebRequestActivityViewModel_SetHeaders_EmptyString_CanPreviewIsFalse()
        {
            var properties = new Dictionary<string, Mock<ModelProperty>>();
            var propertyCollection = new Mock<ModelPropertyCollection>();

            var url = new Mock<ModelProperty>();
            url.Setup(p => p.ComputedValue).Returns("");
            properties.Add("Headers", url);
            propertyCollection.Protected().Setup<ModelProperty>("Find", "Headers", true).Returns(url.Object);

            var modelItemMock = new Mock<ModelItem>();
            modelItemMock.Setup(s => s.Properties).Returns(propertyCollection.Object);

            var sut = new DsfGetWebRequestActivityViewModel(modelItemMock.Object);
            sut.Headers = "";

            Assert.IsFalse(sut.PreviewViewModel.CanPreview);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DsfGetWebRequestActivityViewModel_HeadersSet")]
        public void DsfGetWebRequestActivityViewModel_SetHeaders_StringWithoutVariables_PreviewInputsCountIsZero()
        {
            var properties = new Dictionary<string, Mock<ModelProperty>>();
            var propertyCollection = new Mock<ModelPropertyCollection>();

            var url = new Mock<ModelProperty>();
            url.Setup(p => p.ComputedValue).Returns("ContentType=text/xml");
            properties.Add("Headers", url);
            propertyCollection.Protected().Setup<ModelProperty>("Find", "Headers", true).Returns(url.Object);

            var modelItemMock = new Mock<ModelItem>();
            modelItemMock.Setup(s => s.Properties).Returns(propertyCollection.Object);

            var sut = new DsfGetWebRequestActivityViewModel(modelItemMock.Object);
            sut.Headers = "";

            Assert.IsTrue(sut.PreviewViewModel.Inputs.Count == 0);
            Assert.AreEqual(sut.PreviewViewModel.InputsVisibility, Visibility.Collapsed);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DsfGetWebRequestActivityViewModel_HeadersSet")]
        public void DsfGetWebRequestActivityViewModel_SetHeaders_StringWithOneVariables_PreviewInputsCountIsOne()
        {
            var properties = new Dictionary<string, Mock<ModelProperty>>();
            var propertyCollection = new Mock<ModelPropertyCollection>();

            var url = new Mock<ModelProperty>();
            url.Setup(p => p.ComputedValue).Returns("ContentType=[[contenttype]]");
            properties.Add("Headers", url);
            propertyCollection.Protected().Setup<ModelProperty>("Find", "Headers", true).Returns(url.Object);

            var modelItemMock = new Mock<ModelItem>();
            modelItemMock.Setup(s => s.Properties).Returns(propertyCollection.Object);

            var sut = new DsfGetWebRequestActivityViewModel(modelItemMock.Object);
            sut.Headers = "";

            Assert.IsTrue(sut.PreviewViewModel.Inputs.Count == 1);
            Assert.AreEqual(sut.PreviewViewModel.InputsVisibility, Visibility.Visible);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DsfGetWebRequestActivityViewModel_ExecutePreview")]
        public void DsfGetWebRequestActivityViewModel_ExecutePreviewWhenUrlIsValid_WebRequestIsInvoked()
        {
            var properties = new Dictionary<string, Mock<ModelProperty>>();
            var propertyCollection = new Mock<ModelPropertyCollection>();

            var url = new Mock<ModelProperty>();
            url.Setup(p => p.ComputedValue).Returns("http://www.google.com");
            properties.Add("Url", url);
            propertyCollection.Protected().Setup<ModelProperty>("Find", "Url", true).Returns(url.Object);

            var modelItemMock = new Mock<ModelItem>();
            modelItemMock.Setup(s => s.Properties).Returns(propertyCollection.Object);

            var sut = new DsfGetWebRequestActivityViewModel(modelItemMock.Object);

            sut.HelpViewModel = new HelpViewModel();
            sut.WebInvoke = (m, u) => { return "Was Called"; };
            sut.Url = "";

            sut.PreviewViewModel.PreviewCommand.Execute(null);

            Assert.IsFalse(string.IsNullOrWhiteSpace(sut.PreviewViewModel.Output));
            Assert.AreEqual(sut.PreviewViewModel.Output, "Was Called");
            Assert.IsTrue(sut.HelpViewModel.Errors.Count == 0);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DsfGetWebRequestActivityViewModel_ExecutePreview")]
        public void DsfGetWebRequestActivityViewModel_ExecutePreviewWhenUrlIsValidWithoutHttpPrefix_WebRequestIsInvoked()
        {
            var properties = new Dictionary<string, Mock<ModelProperty>>();
            var propertyCollection = new Mock<ModelPropertyCollection>();

            var url = new Mock<ModelProperty>();
            url.Setup(p => p.ComputedValue).Returns("www.google.com");
            properties.Add("Url", url);
            propertyCollection.Protected().Setup<ModelProperty>("Find", "Url", true).Returns(url.Object);

            var modelItemMock = new Mock<ModelItem>();
            modelItemMock.Setup(s => s.Properties).Returns(propertyCollection.Object);

            var sut = new DsfGetWebRequestActivityViewModel(modelItemMock.Object);

            sut.HelpViewModel = new HelpViewModel();
            sut.WebInvoke = (m, u) => { return "Was Called"; };
            sut.Url = "";

            sut.PreviewViewModel.PreviewCommand.Execute(null);

            Assert.IsFalse(string.IsNullOrWhiteSpace(sut.PreviewViewModel.Output));
            Assert.AreEqual(sut.PreviewViewModel.Output, "Was Called");
            Assert.IsTrue(sut.HelpViewModel.Errors.Count == 0);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DsfGetWebRequestActivityViewModel_ExecutePreview")]
        public void DsfGetWebRequestActivityViewModel_ExecutePreviewWhenUrlIsNull_WebRequestIsNotInvoked()
        {
            var properties = new Dictionary<string, Mock<ModelProperty>>();
            var propertyCollection = new Mock<ModelPropertyCollection>();

            var url = new Mock<ModelProperty>();
            url.Setup(p => p.ComputedValue).Returns(string.Empty);
            properties.Add("Url", url);
            propertyCollection.Protected().Setup<ModelProperty>("Find", "Url", true).Returns(url.Object);

            var modelItemMock = new Mock<ModelItem>();
            modelItemMock.Setup(s => s.Properties).Returns(propertyCollection.Object);

            var sut = new DsfGetWebRequestActivityViewModel(modelItemMock.Object);

            sut.HelpViewModel = new HelpViewModel();
            var isInvoked = false;
            sut.WebInvoke = (m, u) =>
            {
                isInvoked = true;
                return "Was Called";
            };
            sut.Url = "";

            sut.PreviewViewModel.PreviewCommand.Execute(null);

            Assert.IsTrue(string.IsNullOrWhiteSpace(sut.PreviewViewModel.Output));
            Assert.IsFalse(isInvoked);
            Assert.IsTrue(sut.HelpViewModel.Errors.Count > 0);
        }

        #endregion
    }
}