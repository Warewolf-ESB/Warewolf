/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/



using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Data.Serializers;

namespace Warewolf.Data.Tests.Serializers
{
    [TestClass]
    [TestCategory(nameof(WarewolfXmlSerializer))]
    public class WarewolfXmlSerializerTests
    {
        const string personIndentedXML = 
            "<Person xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">\r\n" +
            "  <FName>TestFName</FName>\r\n" +
            "  <LName>TestLName</LName>\r\n" +
            "</Person>";
        const string personNonIndentedXML = "<Person xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><FName>TestFName</FName><LName>TestLName</LName></Person>";


        private Person _person = new Person
        {
            FName = "TestFName",
            LName = "TestLName",
        };

        [TestMethod]
        [Owner("Siphamandla Dube")]
        public void WarewolfXmlSerializer_SerializeToXml_GivenIndentIsTrue_ShouldSuccess()
        {
            var sut = WarewolfXmlSerializer.SerializeToXml(_person);

            Assert.AreEqual(personIndentedXML, sut);

            sut = _person.SerializeToXml();
         
            Assert.AreEqual(personIndentedXML, sut);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        public void WarewolfXmlSerializer_SerializeToXml_GivenIndentIsFalse_ShouldSuccess()
        {
            var sut = WarewolfXmlSerializer.SerializeToXml(_person, false);

            Assert.AreEqual(personNonIndentedXML, sut);

            sut = _person.SerializeToXml(false);

            Assert.AreEqual(personNonIndentedXML, sut);
        }
    }

    public class Person
    {
        public string FName { get; set; }
        public string LName { get; set; }
    }
}
