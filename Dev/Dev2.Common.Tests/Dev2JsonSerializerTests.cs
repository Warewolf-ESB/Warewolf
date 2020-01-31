/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Serializers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Dev2.Common.Tests
{
    [TestClass]
    public class Dev2JsonSerializerTests
    {
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(Dev2JsonSerializer))]
        public void Dev2JsonSerializer_DeserializeXNode_JSON_TO_XML_Empty()
        {
            var dev2Serializer = new Dev2JsonSerializer();
            var dataListString = "{}";
            var xml = dev2Serializer.DeserializeXNode(dataListString, "DataList");
            Assert.AreEqual("<DataList />", xml.ToString());
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(Dev2JsonSerializer))]
        public void Dev2JsonSerializer_DeserializeXNode_JSON_TO_XML_List()
        {
            var dev2Serializer = new Dev2JsonSerializer();
            var dataListString = "{\"test\": \r\n \"rec2value\" \r\n }";
            var xml = dev2Serializer.DeserializeXNode(dataListString, "DataList");
            Assert.AreEqual("<DataList>\r\n  <test>rec2value</test>\r\n</DataList>", xml.ToString());
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(Dev2JsonSerializer))]
        public void Dev2JsonSerializer_DeserializeXNode_JSON_TO_XML_NestedList()
        {
            var dev2Serializer = new Dev2JsonSerializer();            
            var dataListString = "{\r\n  \"Employees\": \"All\"\r\n ,\r\n  \"Person\": {\r\n    \"Age\": \"50\",\r\n    \"Name\": \"Bob\"\r\n  }\r\n}";
            var xml = dev2Serializer.DeserializeXNode(dataListString, "DataList");
            Assert.AreEqual("<DataList>\r\n  <Employees>All</Employees>\r\n  <Person>\r\n    <Age>50</Age>\r\n    <Name>Bob</Name>\r\n  </Person>\r\n</DataList>", xml.ToString());
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(Dev2JsonSerializer))]
        public void Dev2JsonSerializer_DeserializeXNode_JSON_TO_XML_Complex_NestedList_1()
        {
            var dev2Serializer = new Dev2JsonSerializer();
            //var dataListString = "{\r\n  \"Person\": {\r\n    \"Age\": \"50\",\r\n    \"Name\": \"bob\"\r\n ,\r\n  \"addr\": {\r\n    \"line1\": \"14\",\r\n    \"line2\": \"winston\"\r\n  }\r\n }\r\n}";
            var dataListString = "{\"world\": {\"hello\": \"woo\", \"foo\": {\"key\":\"bar\"}}}";
            var xml = dev2Serializer.DeserializeXNode(dataListString, "DataList");
            Assert.AreEqual("<DataList>\r\n  <world>\r\n    <hello>woo</hello>\r\n    <foo>\r\n      <key>bar</key>\r\n    </foo>\r\n  </world>\r\n</DataList>", xml.ToString());
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(Dev2JsonSerializer))]
        public void Dev2JsonSerializer_DeserializeXNode_JSON_TO_XML_Complex_NestedList_2()
        {
            var dev2Serializer = new Dev2JsonSerializer();
            var dataListString = "{\r\n  \"Person\": {\r\n    \"Age\": \"50\",\r\n    \"Name\": \"bob\"\r\n ,\r\n  \"addr\": {\r\n    \"line1\": \"14\",\r\n    \"line2\": \"winston\"\r\n  }\r\n }\r\n}";
            var xml = dev2Serializer.DeserializeXNode(dataListString, "DataList");
            Assert.AreEqual("<DataList>\r\n  <Person>\r\n    <Age>50</Age>\r\n    <Name>bob</Name>\r\n    <addr>\r\n      <line1>14</line1>\r\n      <line2>winston</line2>\r\n    </addr>\r\n  </Person>\r\n</DataList>", xml.ToString());
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(Dev2JsonSerializer))]
        public void Dev2JsonSerializer_DeserializeXNode_JSON_TO_XML_EmptyArray()
        {
            var dev2Serializer = new Dev2JsonSerializer();
            var dataListString = "{\"world\": {\"hello\": \"woo\", \"foo\": {\"key\":\"bar\"}, \"arr\": []}}";
            var xml = dev2Serializer.DeserializeXNode(dataListString, "DataList");
            Assert.AreEqual("<DataList>\r\n  <world>\r\n    <hello>woo</hello>\r\n    <foo>\r\n      <key>bar</key>\r\n    </foo>\r\n    <arr array=\"true\" />\r\n  </world>\r\n</DataList>", xml.ToString());
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(Dev2JsonSerializer))]
        public void Dev2JsonSerializer_DeserializeXNode_FromJSON_WithArray()
        {
            var dev2Serializer = new Dev2JsonSerializer();
            var dataListString = "{\"world\": {\"hello\": \"woo\", \"foo\": {\"key\":\"bar\"}, \"arr\": [\"one\", \"two\"]}}";
            var xml = dev2Serializer.DeserializeXNode(dataListString, "DataList");

            Assert.AreEqual("<DataList>\r\n  <world>\r\n    <hello>woo</hello>\r\n    <foo>\r\n      <key>bar</key>\r\n    </foo>\r\n    <arr array=\"true\">\r\n      <arr>one</arr>\r\n      <arr>two</arr>\r\n    </arr>\r\n  </world>\r\n</DataList>", xml.ToString());
        }
    }
}
