
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using Dev2.Activities.Designers2.GetWebRequest;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;

namespace Dev2.Activities.Designers.Tests.GetWebRequestTests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class GetWebRequestViewModelTests
    {
        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("GetWebRequestDesignerViewModel_Constructor")]
        public void GetWebRequestDesignerViewModel_Constructor_PreviewViewModel_NotNull()
        {
            var properties = new Dictionary<string, Mock<ModelProperty>>();
            var propertyCollection = new Mock<ModelPropertyCollection>();

            var url = new Mock<ModelProperty>();
            url.SetupProperty(p => p.ComputedValue, null);
            properties.Add("Url", url);
            propertyCollection.Protected().Setup<ModelProperty>("Find", "Url", true).Returns(url.Object);

            var modelItemMock = new Mock<ModelItem>();
            modelItemMock.Setup(s => s.Properties).Returns(propertyCollection.Object);

            var sut = new GetWebRequestDesignerViewModel(modelItemMock.Object);
            Assert.IsNotNull(sut.PreviewViewModel);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("GetWebRequestDesignerViewModel_UrlSet")]
        public void GetWebRequestDesignerViewModel_SetUrl_EmptyString_CanPreviewIsFalse()
        {
            var properties = new Dictionary<string, Mock<ModelProperty>>();
            var propertyCollection = new Mock<ModelPropertyCollection>();

            var url = new Mock<ModelProperty>();
            url.SetupProperty(p => p.ComputedValue, "xxxxx"); // start "tracking" sets/gets to this property
            properties.Add("Url", url);
            propertyCollection.Protected().Setup<ModelProperty>("Find", "Url", true).Returns(url.Object);

            var modelItemMock = new Mock<ModelItem>();
            modelItemMock.Setup(s => s.Properties).Returns(propertyCollection.Object);

            var sut = new GetWebRequestDesignerViewModel(modelItemMock.Object);
            url.Object.ComputedValue = "";
            modelItemMock.Raise(mi => mi.PropertyChanged += null, new PropertyChangedEventArgs("Url"));

            Assert.IsFalse(sut.PreviewViewModel.CanPreview);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("GetWebRequestDesignerViewModel_UrlSet")]
        public void GetWebRequestDesignerViewModel_SetUrl_StringWithoutVariables_CanPreviewIsTrueButPreviewInputsCountIsZero()
        {
            var properties = new Dictionary<string, Mock<ModelProperty>>();
            var propertyCollection = new Mock<ModelPropertyCollection>();

            var url = new Mock<ModelProperty>();
            url.SetupProperty(p => p.ComputedValue, ""); // start "tracking" sets/gets to this property
            properties.Add("Url", url);
            propertyCollection.Protected().Setup<ModelProperty>("Find", "Url", true).Returns(url.Object);

            var modelItemMock = new Mock<ModelItem>();
            modelItemMock.Setup(s => s.Properties).Returns(propertyCollection.Object);

            var sut = new GetWebRequestDesignerViewModel(modelItemMock.Object);
            url.Object.ComputedValue = "http://www.google.com";
            modelItemMock.Raise(mi => mi.PropertyChanged += null, new PropertyChangedEventArgs("Url"));

            Assert.IsTrue(sut.PreviewViewModel.CanPreview);
            Assert.IsTrue(sut.PreviewViewModel.Inputs.Count == 0);
            Assert.AreEqual(sut.PreviewViewModel.InputsVisibility, Visibility.Collapsed);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("GetWebRequestDesignerViewModel_UrlSet")]
        public void GetWebRequestDesignerViewModel_SetUrl_StringWithTwoVariables_CanPreviewIsTrueAndPreviewInputsCountIsTwo()
        {
            var properties = new Dictionary<string, Mock<ModelProperty>>();
            var propertyCollection = new Mock<ModelPropertyCollection>();

            var url = new Mock<ModelProperty>();
            url.SetupProperty(p => p.ComputedValue, ""); // start "tracking" sets/gets to this property
            properties.Add("Url", url);
            propertyCollection.Protected().Setup<ModelProperty>("Find", "Url", true).Returns(url.Object);

            var modelItemMock = new Mock<ModelItem>();
            modelItemMock.Setup(s => s.Properties).Returns(propertyCollection.Object);

            var sut = new GetWebRequestDesignerViewModel(modelItemMock.Object);
            url.Object.ComputedValue = "http://www.[[mysite]].com?[[queryString]]";
            modelItemMock.Raise(mi => mi.PropertyChanged += null, new PropertyChangedEventArgs("Url"));

            Assert.IsTrue(sut.PreviewViewModel.CanPreview);
            Assert.IsTrue(sut.PreviewViewModel.Inputs.Count == 2);
            Assert.AreEqual(sut.PreviewViewModel.InputsVisibility, Visibility.Visible);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("GetWebRequestDesignerViewModel_HeadersSet")]
        public void GetWebRequestDesignerViewModel_SetHeaders_EmptyString_CanPreviewIsFalse()
        {
            var properties = new Dictionary<string, Mock<ModelProperty>>();
            var propertyCollection = new Mock<ModelPropertyCollection>();

            var headers = new Mock<ModelProperty>();
            headers.SetupProperty(p => p.ComputedValue, ""); // start "tracking" sets/gets to this property
            properties.Add("Headers", headers);
            propertyCollection.Protected().Setup<ModelProperty>("Find", "Headers", true).Returns(headers.Object);

            var modelItemMock = new Mock<ModelItem>();
            modelItemMock.Setup(s => s.Properties).Returns(propertyCollection.Object);

            var sut = new GetWebRequestDesignerViewModel(modelItemMock.Object);
            headers.Object.ComputedValue = "";
            modelItemMock.Raise(mi => mi.PropertyChanged += null, new PropertyChangedEventArgs("Headers"));

            Assert.IsFalse(sut.PreviewViewModel.CanPreview);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("GetWebRequestDesignerViewModel_HeadersSet")]
        public void GetWebRequestDesignerViewModel_SetHeaders_StringWithoutVariables_PreviewInputsCountIsZero()
        {
            var properties = new Dictionary<string, Mock<ModelProperty>>();
            var propertyCollection = new Mock<ModelPropertyCollection>();

            var headers = new Mock<ModelProperty>();
            headers.SetupProperty(p => p.ComputedValue, ""); // start "tracking" sets/gets to this property
            properties.Add("Headers", headers);
            propertyCollection.Protected().Setup<ModelProperty>("Find", "Headers", true).Returns(headers.Object);

            var modelItemMock = new Mock<ModelItem>();
            modelItemMock.Setup(s => s.Properties).Returns(propertyCollection.Object);

            var sut = new GetWebRequestDesignerViewModel(modelItemMock.Object);
            headers.Object.ComputedValue = "ContentType=text/xml";
            modelItemMock.Raise(mi => mi.PropertyChanged += null, new PropertyChangedEventArgs("Headers"));

            Assert.IsTrue(sut.PreviewViewModel.Inputs.Count == 0);
            Assert.AreEqual(sut.PreviewViewModel.InputsVisibility, Visibility.Collapsed);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("GetWebRequestDesignerViewModel_HeadersSet")]
        public void GetWebRequestDesignerViewModel_SetHeaders_StringWithOneVariables_PreviewInputsCountIsOne()
        {

            var properties = new Dictionary<string, Mock<ModelProperty>>();
            var propertyCollection = new Mock<ModelPropertyCollection>();

            var headers = new Mock<ModelProperty>();
            headers.SetupProperty(p => p.ComputedValue, ""); // start "tracking" sets/gets to this property
            properties.Add("Headers", headers);
            propertyCollection.Protected().Setup<ModelProperty>("Find", "Headers", true).Returns(headers.Object);

            var modelItemMock = new Mock<ModelItem>();
            modelItemMock.Setup(s => s.Properties).Returns(propertyCollection.Object);

            var sut = new GetWebRequestDesignerViewModel(modelItemMock.Object);

            headers.Object.ComputedValue = "ContentType=[[contenttype]]";
            modelItemMock.Raise(mi => mi.PropertyChanged += null, new PropertyChangedEventArgs("Headers"));

            Assert.IsTrue(sut.PreviewViewModel.Inputs.Count == 1);
            Assert.AreEqual(sut.PreviewViewModel.InputsVisibility, Visibility.Visible);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("GetWebRequestDesignerViewModel_ExecutePreview")]
        public void GetWebRequestDesignerViewModel_ExecutePreviewWhenUrlIsValid_WebRequestIsInvoked()
        {
            var properties = new Dictionary<string, Mock<ModelProperty>>();
            var propertyCollection = new Mock<ModelPropertyCollection>();

            var url = new Mock<ModelProperty>();
            url.SetupProperty(p => p.ComputedValue, "http://www.google.com"); // start "tracking" sets/gets to this property
            properties.Add("Url", url);
            propertyCollection.Protected().Setup<ModelProperty>("Find", "Url", true).Returns(url.Object);

            var modelItemMock = new Mock<ModelItem>();
            modelItemMock.Setup(s => s.Properties).Returns(propertyCollection.Object);

            var sut = new GetWebRequestDesignerViewModel(modelItemMock.Object);

            sut.WebInvoke = (m, u, h) => { return "Was Called"; };

            sut.PreviewViewModel.PreviewCommand.Execute(null);

            Assert.IsFalse(string.IsNullOrWhiteSpace(sut.PreviewViewModel.Output));
            Assert.AreEqual(sut.PreviewViewModel.Output, "Was Called");
            Assert.IsNull(sut.Errors);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("GetWebRequestDesignerViewModel_ExecutePreview")]
        public void GetWebRequestDesignerViewModel_ExecutePreviewWhenUrlIsValidWithoutHttpPrefix_WebRequestIsInvoked()
        {
            var properties = new Dictionary<string, Mock<ModelProperty>>();
            var propertyCollection = new Mock<ModelPropertyCollection>();

            var url = new Mock<ModelProperty>();
            url.SetupProperty(p => p.ComputedValue, "www.google.com"); // start "tracking" sets/gets to this property
            properties.Add("Url", url);
            propertyCollection.Protected().Setup<ModelProperty>("Find", "Url", true).Returns(url.Object);

            var modelItemMock = new Mock<ModelItem>();
            modelItemMock.Setup(s => s.Properties).Returns(propertyCollection.Object);

            var sut = new GetWebRequestDesignerViewModel(modelItemMock.Object);

            sut.WebInvoke = (m, u, h) => { return "Was Called"; };

            sut.PreviewViewModel.PreviewCommand.Execute(null);

            Assert.IsFalse(string.IsNullOrWhiteSpace(sut.PreviewViewModel.Output));
            Assert.AreEqual(sut.PreviewViewModel.Output, "Was Called");
            Assert.IsNull(sut.Errors);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("GetWebRequestDesignerViewModel_ExecutePreview")]
        public void GetWebRequestDesignerViewModel_ExecutePreviewWhenUrlIsNull_WebRequestIsNotInvoked()
        {
            var modelItem = GenerateMockModelItem(string.Empty);

            var sut = new GetWebRequestDesignerViewModel(modelItem.Object);

            var isInvoked = false;
            sut.WebInvoke = (m, u, h) =>
            {
                isInvoked = true;
                return "Was Called";
            };

            sut.PreviewViewModel.PreviewCommand.Execute(null);

            Assert.IsTrue(string.IsNullOrWhiteSpace(sut.PreviewViewModel.Output));
            Assert.IsFalse(isInvoked);
            Assert.AreEqual(1, sut.Errors.Count);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("GetWebRequestDesignerViewModel_Validate")]
        public void GetWebRequestDesignerViewModel_Validate_InvalidExpression_IsValidFalse()
        {
            //------------Setup for test--------------------------
            var modelItem = GenerateMockModelItem("[[asdf]asdf]]asdf]]");

            var viewModel = new GetWebRequestDesignerViewModel(modelItem.Object);

            //------------Execute Test---------------------------
            viewModel.Validate();

            //------------Assert Results-------------------------
            Assert.IsFalse(viewModel.IsValid);
            Assert.AreEqual(1, viewModel.Errors.Count);
            Assert.AreEqual("Invalid expression: opening and closing brackets don't match.", viewModel.Errors[0].Message);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("GetWebRequestDesignerViewModel_Validate")]
        public void GetWebRequestDesignerViewModel_Validate_ValidExpression_IsValidTrue()
        {
            //------------Setup for test--------------------------
            var modelItem = GenerateMockModelItem("http://[[asdf]]?[[asdf]]");

            var viewModel = new GetWebRequestDesignerViewModel(modelItem.Object);

            //------------Execute Test---------------------------
            viewModel.Validate();

            //------------Assert Results-------------------------
            Assert.IsTrue(viewModel.IsValid);
            Assert.IsNull(viewModel.Errors);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("GetWebRequestDesignerViewModel_Validate")]
        public void GetWebRequestDesignerViewModel_Validate_ValidString_IsValidTrue()
        {
            //------------Setup for test--------------------------
            var modelItem = GenerateMockModelItem("http://www.search.com?p=5");

            var viewModel = new GetWebRequestDesignerViewModel(modelItem.Object);

            //------------Execute Test---------------------------
            viewModel.Validate();

            //------------Assert Results-------------------------
            Assert.IsTrue(viewModel.IsValid);
            Assert.IsNull(viewModel.Errors);
        }

        static Mock<ModelItem> GenerateMockModelItem(string urlValue, string headersValue = null, string resultValue = null)
        {
            var properties = new Dictionary<string, Mock<ModelProperty>>();
            var propertyCollection = new Mock<ModelPropertyCollection>();


            var url = new Mock<ModelProperty>();
            url.Setup(p => p.ComputedValue).Returns(urlValue);
            properties.Add("Url", url);
            propertyCollection.Protected().Setup<ModelProperty>("Find", "Url", true).Returns(url.Object);

            var headers = new Mock<ModelProperty>();
            headers.Setup(p => p.ComputedValue).Returns(headersValue);
            properties.Add("Headers", url);
            propertyCollection.Protected().Setup<ModelProperty>("Find", "Headers", true).Returns(headers.Object);

            var result = new Mock<ModelProperty>();
            result.Setup(p => p.ComputedValue).Returns(resultValue);
            properties.Add("Result", url);
            propertyCollection.Protected().Setup<ModelProperty>("Find", "Result", true).Returns(result.Object);

            var displayName = new Mock<ModelProperty>();
            displayName.Setup(p => p.ComputedValue).Returns("Web Request");
            properties.Add("DisplayName", displayName);
            propertyCollection.Protected().Setup<ModelProperty>("Find", "DisplayName", true).Returns(displayName.Object);

            var mockModelItem = new Mock<ModelItem>();
            mockModelItem.Setup(mi => mi.ItemType).Returns(typeof(DsfWebGetRequestActivity));
            mockModelItem.Setup(s => s.Properties).Returns(propertyCollection.Object);

            return mockModelItem;
        }

    }
}
