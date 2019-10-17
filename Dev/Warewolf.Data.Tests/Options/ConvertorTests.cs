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
using System;
using System.Linq;
using Warewolf.Options;

namespace Warewolf.Data.Tests
{
    [TestClass]
    public class ConvertorTests
    {
        [TestMethod]
        public void OptionConvertor_GivenSimpleClass_ExpectListOfIOptions()
        {
            var result = ConvertDataToOptionsList();

            Assert.AreEqual(4, result.Length);
        }


        [TestMethod]
        public void OptionConvertor_GivenSimpleClass_ExpectIntOption_Success()
        {
            var result = ConvertDataToOptionsList();

            Assert.AreEqual(nameof(TestData.MyInt), result[0].Name);
            Assert.AreEqual(12, ((OptionInt)result[0]).Value);
        }

        [TestMethod]
        public void OptionConvertor_GivenSimpleClass_ExpectStringOption_Success()
        {
            var result = ConvertDataToOptionsList();

            Assert.AreEqual(nameof(TestData.MyString), result[1].Name);
            Assert.AreEqual("hello", ((OptionAutocomplete)result[1]).Value);
            var expected = new TestData.OptionsForS().Options;
            var suggestions = ((OptionAutocomplete)result[1]).Suggestions;
            Assert.IsTrue(expected.SequenceEqual(suggestions));
        }

        [TestMethod]
        public void OptionConvertor_GivenSimpleClass_ExpectBoolOption_Success()
        {
            var result = ConvertDataToOptionsList();

            Assert.AreEqual(nameof(TestData.MyBool), result[2].Name);
            Assert.AreEqual(true, ((OptionBool)result[2]).Value);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(OptionConvertor))]
        public void OptionConvertor_GivenSimpleClass_ExpectedEnumOption_Success()
        {
            //----------------------Arrange----------------------
            //----------------------Act--------------------------
            var actual = ConvertDataToOptionsList();
            //----------------------Assert-----------------------
            Assert.AreEqual(nameof(TestData.MyEnum), actual[3].Name);
            Assert.AreEqual(MyOptions.Option1, ((OptionEnum)actual[3]).Value);
        }

        private static IOption[] ConvertDataToOptionsList()
        {
            var cls = new TestData
            {
                MyInt = 12,
                MyString = "hello",
                MyBool = true,
                MyEnum = MyOptions.Option1
            };

            return OptionConvertor.Convert(cls);
        }

        enum MyOptions
        {
            Option1,
            Option2
        }

        public class TestData
        {
            public int MyInt { get; set; }
            [DataProvider(typeof(OptionsForS))]
            public string MyString { get; set; }
            public bool MyBool { get; set; }
            public Enum MyEnum { get; set; }

            public class OptionsForS : IOptionDataList
            {
                public string[] Options => new string[] { "sopt1", "sopt2", "sopt3", "sopt4" };
            }
        }

    }
}
