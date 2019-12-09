/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;

namespace Warewolf.Streams
{
    [TestClass]
    public class SerializeDeserializeTests
    {
        private const string ExpectedSerializedData = "{\"MyInt\":123,\"MyString\":\"kuihhj:\"}";

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ISerializer))]
        public void ISerializer_GivenValidObject_ExpectJsonString()
        {
            ISerializer serializer = new JsonSerializer();

            var testValue = new MyValue();
            var bytes = serializer.Serialize(testValue);

            Assert.AreEqual(ExpectedSerializedData, UTF8Encoding.UTF8.GetString(bytes));
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ISerializer))]
        public void IDeserializer_GivenValidJson_ExpectValidObject()
        {
            IDeserializer deserializer = new JsonSerializer();
            var bytes = UTF8Encoding.UTF8.GetBytes(ExpectedSerializedData);
            var testValue = new MyValue();

            var result = deserializer.Deserialize<MyValue>(bytes);

            Assert.AreEqual(testValue.MyInt, result.MyInt);
            Assert.AreEqual(testValue.MyString, result.MyString);
        }
    }

    internal class MyValue
    {
        public int MyInt { get; set; } = 123;
        public string MyString { get; set; } = "kuihhj:";
    }
}
