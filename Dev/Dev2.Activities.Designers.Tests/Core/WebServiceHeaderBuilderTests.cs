using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.ToolBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Dev2.Activities.Designers.Tests.Core
{
    [TestClass]
    public class WebServiceHeaderBuilderTests
    {
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("WebServiceHeaderBuilder")]
        public void WebServiceHeaderBuilder_GivenNoHeadersNoContent_PassThrouh()
        {
            //------------Setup for test--------------------------
            var mod = new WebServiceHeaderBuilder();
            var newMock = new Mock<IHeaderRegion>();
            newMock.Setup(region => region.Headers).Returns(default(ObservableCollection<INameValue>));
            //------------Execute Test---------------------------
            mod.BuildHeader(newMock.Object, null);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("WebServiceHeaderBuilder")]
        public void WebServiceHeaderBuilder_GivenNormalText_PassAddNoHeaders()
        {
            //------------Setup for test--------------------------
            var mod = new WebServiceHeaderBuilder();
            var newMock = new Mock<IHeaderRegion>();
            newMock.SetupProperty(region => region.Headers);
            var content = "NormalText";
            //---------------Assert Precondition----------------
            Assert.IsNull(newMock.Object.Headers);
            //------------Execute Test---------------------------
            mod.BuildHeader(newMock.Object, content);
            //------------Assert Results-------------------------
            Assert.IsNull(newMock.Object.Headers);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("WebServiceHeaderBuilder")]
        public void WebServiceHeaderBuilder_GivenNormalTextJson_PassAddHeaders()
        {
            //------------Setup for test--------------------------
            var mod = new WebServiceHeaderBuilder();
            var newMock = new Mock<IHeaderRegion>();
            newMock.SetupProperty(region => region.Headers);
            var content = "{\"NormalText\":\"\"}";
            //---------------Assert Precondition----------------
            Assert.IsNull(newMock.Object.Headers);
            //------------Execute Test---------------------------
            mod.BuildHeader(newMock.Object, content);
            //------------Assert Results-------------------------
            Assert.IsNotNull(newMock.Object.Headers);
            Assert.AreEqual(2, newMock.Object.Headers.Count);
            var countContentTypes = newMock.Object.Headers.Count(value => value.Name.Equals(GlobalConstants.ContentType));
            var countContentTypesValues = newMock.Object.Headers.Count(value => value.Value.Equals("application/json"));
            Assert.AreEqual(1, countContentTypesValues);
            Assert.AreEqual(1, countContentTypes);
        }


        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("WebServiceHeaderBuilder")]
        public void WebServiceHeaderBuilder_GivenNormalTextXml_PassAddHeaders()
        {
            //------------Setup for test--------------------------
            var mod = new WebServiceHeaderBuilder();
            var newMock = new Mock<IHeaderRegion>();
            newMock.SetupProperty(region => region.Headers);
            var content = "<DataList><a>2</a></DataList>";
            //---------------Assert Precondition----------------
            Assert.IsNull(newMock.Object.Headers);
            //------------Execute Test---------------------------
            mod.BuildHeader(newMock.Object, content);
            //------------Assert Results-------------------------
            Assert.IsNotNull(newMock.Object.Headers);
            Assert.AreEqual(2, newMock.Object.Headers.Count);
            var countContentTypes = newMock.Object.Headers.Count(value => value.Name.Equals(GlobalConstants.ContentType));
            var countContentTypesValues = newMock.Object.Headers.Count(value => value.Value.Equals(GlobalConstants.ApplicationXmlHeader));
            Assert.AreEqual(1, countContentTypesValues);
            Assert.AreEqual(1, countContentTypes);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("WebServiceHeaderBuilder")]
        public void WebServiceHeaderBuilder_GivenHasExistingXmlHeaderAndCOntentIsXml_PassAddNoHeaders()
        {
            //------------Setup for test--------------------------
            var mod = new WebServiceHeaderBuilder();
            var newMock = new Mock<IHeaderRegion>();
            newMock.SetupProperty(region => region.Headers);
            var jsonHeader = new NameValue(GlobalConstants.ContentType, GlobalConstants.ApplicationXmlHeader);
            newMock.Object.Headers = new ObservableCollection<INameValue> { jsonHeader, new NameValue() };
            var content = "<DataList><a>2</a></DataList>";
            //---------------Assert Precondition----------------
            Assert.IsNotNull(newMock.Object.Headers);
            Assert.AreEqual(2, newMock.Object.Headers.Count);
            //------------Execute Test---------------------------
            mod.BuildHeader(newMock.Object, content);
            //------------Assert Results-------------------------
            Assert.IsNotNull(newMock.Object.Headers);
            Assert.AreEqual(2, newMock.Object.Headers.Count);
            var countContentTypes = newMock.Object.Headers.Count(value => value.Name.Equals(GlobalConstants.ContentType));
            var countContentTypesValues = newMock.Object.Headers.Count(value => value.Value.Equals(GlobalConstants.ApplicationXmlHeader));
            Assert.AreEqual(1, countContentTypesValues);
            Assert.AreEqual(1, countContentTypes);

        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("WebServiceHeaderBuilder")]
        public void WebServiceHeaderBuilder_GivenHasExistingJsonHeaderAndContentIsJson_PassAddNoHeaders()
        {
            //------------Setup for test--------------------------
            var mod = new WebServiceHeaderBuilder();
            var newMock = new Mock<IHeaderRegion>();
            newMock.SetupProperty(region => region.Headers);
            var jsonHeader = new NameValue(GlobalConstants.ContentType, GlobalConstants.ApplicationJsonHeader);
            newMock.Object.Headers = new ObservableCollection<INameValue> { jsonHeader, new NameValue() };
            var content = "{\"NormalText\":\"\"}";
            //---------------Assert Precondition----------------
            Assert.IsNotNull(newMock.Object.Headers);
            Assert.AreEqual(2, newMock.Object.Headers.Count);
            //------------Execute Test---------------------------
            mod.BuildHeader(newMock.Object, content);
            //------------Assert Results-------------------------
            Assert.IsNotNull(newMock.Object.Headers);
            Assert.AreEqual(2, newMock.Object.Headers.Count);
            var countContentTypes = newMock.Object.Headers.Count(value => value.Name.Equals(GlobalConstants.ContentType));
            var countContentTypesValues = newMock.Object.Headers.Count(value => value.Value.Equals(GlobalConstants.ApplicationJsonHeader));
            Assert.AreEqual(1, countContentTypesValues);
            Assert.AreEqual(1, countContentTypes);

        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("WebServiceHeaderBuilder")]
        public void WebServiceHeaderBuilder_GivenEmptyHeaders_SetHeaders()
        {
            //------------Setup for test--------------------------
            var mod = new WebServiceHeaderBuilder();
            var newMock = new Mock<IHeaderRegion>();
            newMock.SetupProperty(region => region.Headers);
            var jsonHeader = new NameValue();
            newMock.Object.Headers = new ObservableCollection<INameValue> { jsonHeader, new NameValue() };
            var content = "{\"NormalText\":\"\"}";
            //---------------Assert Precondition----------------
            Assert.IsNotNull(newMock.Object.Headers);
            Assert.AreEqual(2, newMock.Object.Headers.Count);
            //------------Execute Test---------------------------
            mod.BuildHeader(newMock.Object, content);
            //------------Assert Results-------------------------
            Assert.IsNotNull(newMock.Object.Headers);
            Assert.AreEqual(2, newMock.Object.Headers.Count);
            var countContentTypes = newMock.Object.Headers.Count(value => value.Name.Equals(GlobalConstants.ContentType));
            var countContentTypesValues = newMock.Object.Headers.Count(value => value.Value.Equals(GlobalConstants.ApplicationJsonHeader));
            Assert.AreEqual(1, countContentTypesValues);
            Assert.AreEqual(1, countContentTypes);

        }


    }
}
