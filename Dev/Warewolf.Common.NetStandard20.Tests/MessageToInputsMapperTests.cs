/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Common;
using Warewolf.Web;

namespace Warewolf.Web.Tests
{
    [TestClass]
    public class MessageToInputsMapperTests
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(MessageToInputsMapper))]
        public void MessageToInputsMapper_JsonMessage_WithExactProperties_ShouldMap()
        {
            //-----------------------------Arrange------------------------------
            const string message = "{\"FirstName\":\"Bob\",\"Surname\":\"The Builder\"}";
            var inputs = new List<(string name, string value)> {
                                                                  (name: "FirstName", value: "FirstName"),
                                                                  (name: "Surname", value: "Surname")
                                                               };
            var messageToInputsMapper = new MessageToInputsMapper();
            //-----------------------------Act----------------------------------
            var mappedData = messageToInputsMapper.Map(message, inputs,true,false,false);
            //-----------------------------Assert-------------------------------
            Assert.AreEqual(message, mappedData);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(MessageToInputsMapper))]
        public void MessageToInputsMapper_JsonMessage_WithMappableProperties_ShouldMap()
        {
            //-----------------------------Arrange------------------------------
            const string message = "{\"FirstName\":\"Bob\",\"Surname\":\"The Builder\"}";
            var inputs = new List<(string name, string value)> {
                                                                  (name: "Name", value: "FirstName"),
                                                                  (name: "LastName", value: "Surname")
                                                               };
            var messageToInputsMapper = new MessageToInputsMapper();
            var expectedString = "{\"Name\":\"Bob\",\"LastName\":\"The Builder\"}";
            //-----------------------------Act----------------------------------
            var mappedData = messageToInputsMapper.Map(message, inputs,true,false, false);
            //-----------------------------Assert-------------------------------
            Assert.AreEqual(expectedString, mappedData);
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(MessageToInputsMapper))]
        public void MessageToInputsMapper_XMLMessage_WithMappableProperties_ShouldMap()
        {
            //-----------------------------Arrange------------------------------
            const string message = "<Root><FirstName>Bob</FirstName><Surname>The Builder</Surname></Root>";
            var inputs = new List<(string name, string value)> {
                                                                  (name: "Name", value: "FirstName"),
                                                                  (name: "LastName", value: "Surname")
                                                               };
            var messageToInputsMapper = new MessageToInputsMapper();
            var expectedString = "{\"Name\":\"Bob\",\"LastName\":\"The Builder\"}";
            //-----------------------------Act----------------------------------
            var mappedData = messageToInputsMapper.Map(message, inputs,false,true, false);
            //-----------------------------Assert-------------------------------
            Assert.AreEqual(expectedString, mappedData);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(MessageToInputsMapper))]
        public void MessageToInputsMapper_NoInputs_ShouldReturnTrue()
        {
            //-----------------------------Arrange------------------------------
            const string message = "<Root><FirstName>Bob</FirstName><Surname>The Builder</Surname></Root>";
            var inputs = new List<(string name, string value)>();
            var messageToInputsMapper = new MessageToInputsMapper();
            var expectedString = "{}";
            //-----------------------------Act----------------------------------
            var mappedData = messageToInputsMapper.Map(message, inputs,false,true, false);
            //-----------------------------Assert-------------------------------
            Assert.AreEqual(expectedString, mappedData);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(MessageToInputsMapper))]
        public void MessageToInputsMapper_UnMappableData_Xml_Should_NotMap()
        {
            //-----------------------------Arrange------------------------------
            const string message = "<Root><Something>Bob</Something><SomethingElse>The Builder</SomethingElse></Root>";
            var inputs = new List<(string name, string value)> {
                                                                  (name: "Name", value: "FirstName"),
                                                                  (name: "LastName", value: "Surname")
                                                               };
            var messageToInputsMapper = new MessageToInputsMapper();
            var expectedString = "{}";
            //-----------------------------Act----------------------------------
            var mappedData = messageToInputsMapper.Map(message, inputs, false, true, false); ;
            //-----------------------------Assert-------------------------------
            Assert.AreEqual(expectedString, mappedData);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(MessageToInputsMapper))]
        public void MessageToInputsMapper_JsonMessage_UnMappableProperties_ShouldNotMap()
        {
            //-----------------------------Arrange------------------------------
            const string message = "{\"Something\":\"Bob\",\"SomethingElse\":\"The Builder\"}";
            var inputs = new List<(string name, string value)> {
                                                                  (name: "Name", value: "FirstName"),
                                                                  (name: "LastName", value: "Surname")
                                                               };
            var messageToInputsMapper = new MessageToInputsMapper();
            var expectedString = "{}";
            //-----------------------------Act----------------------------------
            var mappedData = messageToInputsMapper.Map(message, inputs, true, false, false);
            //-----------------------------Assert-------------------------------
            Assert.AreEqual(expectedString, mappedData);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(MessageToInputsMapper))]
        public void MessageToInputsMapper_WhenMapWholeMessage_ShouldSetWholeMessageToVariable()
        {
            //-----------------------------Arrange------------------------------
            const string message = "<Root><FirstName>Bob</FirstName><Surname>The Builder</Surname></Root>";
            var inputs = new List<(string name, string value)>{
                                                                  (name: "msg", value: ""),
                                                               }; 
            var messageToInputsMapper = new MessageToInputsMapper();
            var expectedString = "{\"msg\":\""+message+"\"}";
            //-----------------------------Act----------------------------------
            var mappedData = messageToInputsMapper.Map(message, inputs, false, true,true);
            //-----------------------------Assert-------------------------------
            Assert.AreEqual(expectedString, mappedData);
        }
    }
}
