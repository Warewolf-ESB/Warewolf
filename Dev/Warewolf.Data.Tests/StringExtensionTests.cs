/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


namespace Warewolf.Data.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json.Linq;
    using System.Collections.Generic;
    using System.Xml.Linq;

    namespace Warewolf.Data.Tests
    {
        [TestClass]
        public class StringExtensionTests
        {
            [TestMethod]
            [Owner("Siphamandla Dube")]
            [TestCategory(nameof(StringExtension))]
            public void StringExtension_IsXml_Given_EmptyString_ShouldReturnFalse()
            {
                var stringData = string.Empty;

                var result = stringData.IsXml(out XDocument ouput);

                Assert.IsFalse(result);
                Assert.IsInstanceOfType(ouput, typeof(XDocument));
            }

            [TestMethod]
            [Owner("Siphamandla Dube")]
            [TestCategory(nameof(StringExtension))]
            public void StringExtension_IsXml_Given_IncorrectXML_ShouldReturnFalse()
            {
                var stringData = "<tag1<=test data</tag1>";

                var result = stringData.IsXml(out XDocument ouput);

                Assert.IsFalse(result);
                Assert.IsInstanceOfType(ouput, typeof(XDocument));
            }

            [TestMethod]
            [Owner("Siphamandla Dube")]
            [TestCategory(nameof(StringExtension))]
            public void StringExtension_IsXml_Given_CorrectXML_ShouldReturnTrue()
            {
                var stringData = "<tag1>test data</tag1>";

                var result = stringData.IsXml(out XDocument output);

                Assert.IsTrue(result);
                Assert.AreEqual(XDocument.Parse(stringData).ToString(), output.ToString());
            }

            [TestMethod]
            [Owner("Siphamandla Dube")]
            [TestCategory(nameof(StringExtension))]
            public void StringExtension_CleanXmlSOAP_Given_CorrectSoapXML_ShouldReturnTrue()
            {
                int a = 333, b = 555;

                var soap = @"<?xml version=""1.0"" encoding=""utf-8""?>  
                                <soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-   instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">  
                                 <soap:Body>  
                                    <Addition xmlns=""http://tempuri.org/"">  
                                      <a>" + a + @"</a>  
                                      <b>" + b + @"</b>  
                                    </Addition>  
                                  </soap:Body>  
                                </soap:Envelope>";

                Assert.IsTrue(soap.IsXml(out XDocument _), "The SOAP xml given is not valid xml, if it cannot be XDocument.Parse'd");

                var result = soap.CleanXmlSOAP();

                Assert.IsTrue(result.IsXml(out XDocument _), "XML should still be valid after the CleanXmlSOAP call, Warewolf still expects xml after this call");

                var expected = "<Envelope >\r\n" +
                                "  <Body>\r\n" +
                                "    <Addition >\r\n" +
                                "      <a>333</a>\r\n" +
                                "      <b>555</b>\r\n" +
                                "    </Addition>\r\n" +
                                "  </Body>\r\n" +
                                "</Envelope>";

                Assert.AreEqual(expected, result.ToString());
            }

            [TestMethod]
            [Owner("Siphamandla Dube")]
            [TestCategory(nameof(StringExtension))]
            public void StringExtension_IsXElement_Given_IsEmpty_ShouldReturnFalse()
            {
                var stringData = string.Empty;

                var result = stringData.IsXElement(out XElement output);

                Assert.IsNull(output);
                Assert.IsFalse(result);
            }

            [TestMethod]
            [Owner("Siphamandla Dube")]
            [TestCategory(nameof(StringExtension))]
            public void StringExtension_IsXElement_Given_IsNotEmpty_And_StartsWithAngleBracket_ShouldReturnFalse()
            {
                var stringData = "<tag like string that should not work.";

                var result = stringData.IsXElement(out XElement output);

                Assert.IsNull(output);
                Assert.IsFalse(result);
            }

            [TestMethod]
            [Owner("Siphamandla Dube")]
            [TestCategory(nameof(StringExtension))]
            public void StringExtension_IsXElement_Given_InCorrectXML_ShouldReturnFalse()
            {
                var stringData = "<tag1>test data<tag1>";

                var result = stringData.IsXElement(out XElement output);

                Assert.IsNull(output);
                Assert.IsFalse(result);
            }

            [TestMethod]
            [Owner("Siphamandla Dube")]
            [TestCategory(nameof(StringExtension))]
            public void StringExtension_IsXElement_Given_CorrectXML_ShouldReturnTrue()
            {
                var stringData = "<tag1>test data</tag1>";

                var result = stringData.IsXElement(out XElement output);

                Assert.IsTrue(result);
                Assert.IsNotNull(output);
                Assert.AreEqual(XElement.Parse(stringData).ToString(), output.ToString());
            }

            [TestMethod]
            [Owner("Siphamandla Dube")]
            [TestCategory(nameof(StringExtension))]
            public void StringExtension_IsXElement_Given_MultipleXElements_ShouldReturnFalse()
            {
                var stringData = "<tag1>test data one</tag1><tag2>test data two</tag2>";

                var result = stringData.IsXElement(out XElement _);

                Assert.IsFalse(result);
            }

            [TestMethod]
            [Owner("Siphamandla Dube")]
            [TestCategory(nameof(StringExtension))]
            public void StringExtension_IsMultipleXElement_Given_MultipleXElements_ShouldReturnTrue()
            {
                var stringData = "<tag1>test data one</tag1><tag2>test data two</tag2>";
                var expectedStringData = string.Concat("<Error>", stringData, "</Error>");

                var result = stringData.IsMultipleXElement(out XElement output);

                Assert.IsTrue(result);
                Assert.IsNotNull(output);
                Assert.AreEqual(XElement.Parse(expectedStringData).ToString(), output.ToString());
            }

            [TestMethod]
            [Owner("Siphamandla Dube")]
            [TestCategory(nameof(StringExtension))]
            public void StringExtension_IsJSON_Given_IsEmpty_ShouldReturnFalse()
            {
                var stringData = string.Empty;

                var result = stringData.IsJToken(out JToken output);

                Assert.IsNull(output);
                Assert.IsFalse(result);
            }

            [TestMethod]
            [Owner("Siphamandla Dube")]
            [TestCategory(nameof(StringExtension))]
            public void StringExtension_IsJSON_Given_IsInCorrectJSONObject_ShouldReturnFalse()
            {
                var stringData = "{this is not a correct json object}";

                var result = stringData.IsJToken(out JToken output);

                Assert.IsNull(output);
                Assert.IsFalse(result);
            }

            [TestMethod]
            [Owner("Siphamandla Dube")]
            [TestCategory(nameof(StringExtension))]
            public void StringExtension_IsJSON_Given_IsInCorrectJSONArray_ShouldReturnFalse()
            {
                var stringData = "[this is not a correct json array]";

                var result = stringData.IsJToken(out JToken output);

                Assert.IsNull(output);
                Assert.IsFalse(result);
            }

            [TestMethod]
            [Owner("Siphamandla Dube")]
            [TestCategory(nameof(StringExtension))]
            public void StringExtension_IsJSON_Given_IsCorrectJSONObject_ShouldReturnTrue()
            {
                var stringData = "{errorTag:'test error'}";

                var result = stringData.IsJToken(out JToken output);

                Assert.IsNotNull(output);
                Assert.IsTrue(result);
            }


            [TestMethod]
            [Owner("Siphamandla Dube")]
            [TestCategory(nameof(StringExtension))]
            public void StringExtension_IsJSON_Given_IsCorrectJSONArray_ShouldReturnTrue()
            {
                var stringData = "['test error one', 'test error two']";

                var result = stringData.IsJToken(out JToken output);


                Assert.IsNotNull(output);
                Assert.IsTrue(result);

                List<string> list = output.ToObject(typeof(List<string>)) as List<string>;
                Assert.IsNotNull(list);
            }
        }
    }
}
