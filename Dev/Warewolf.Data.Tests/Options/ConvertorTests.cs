/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Linq;
using NUnit.Framework;
using Warewolf.Options;

namespace Warewolf.Data.Tests
{
    [TestFixture]
    public class ConvertorTests
    {
        [Test]
        public void OptionConvertor_GivenSimpleClass_ExpectListOfIOptions()
        {
            var result = ConvertDataToOptionsList();

            Assert.AreEqual(5, result.Length);
        }


        [Test]
        public void OptionConvertor_GivenSimpleClass_ExpectIntOption_Success()
        {
            var result = ConvertDataToOptionsList();

            Assert.AreEqual(nameof(TestData.MyInt), result[0].Name);
            Assert.AreEqual(12, ((OptionInt)result[0]).Value);
        }

        [Test]
        public void OptionConvertor_GivenSimpleClass_ExpectStringOption_Success()
        {
            var result = ConvertDataToOptionsList();

            Assert.AreEqual(nameof(TestData.MyString), result[1].Name);
            Assert.AreEqual("hello", ((OptionAutocomplete)result[1]).Value);
            var expected = new TestData.OptionsForS().Items;
            var suggestions = ((OptionAutocomplete)result[1]).Suggestions;
            Assert.IsTrue(expected.SequenceEqual(suggestions));
        }

        [Test]
        public void OptionConvertor_GivenSimpleClass_ExpectBoolOption_Success()
        {
            var result = ConvertDataToOptionsList();

            Assert.AreEqual(nameof(TestData.MyBool), result[2].Name);
            Assert.AreEqual(true, ((OptionBool)result[2]).Value);
        }

        [Test]
       
        public void OptionConvertor_GivenSimpleClass_ExpectedEnumOption_Success()
        {
            //----------------------Arrange----------------------
            //----------------------Act--------------------------
            var actual = ConvertDataToOptionsList();
            //----------------------Assert-----------------------
            Assert.AreEqual(nameof(TestData.MyEnum), actual[3].Name);
            Assert.AreEqual(0, ((OptionInt)actual[3]).Value);
        }

        [Test]
       
        public void OptionConvertor_Given_MultiDataProvider_WithEnum_ReturnSuccess()
        {
            //----------------------Arrange----------------------
            //----------------------Act--------------------------
            var result = ConvertDataToOptionsList();
            //----------------------Assert-----------------------

            var weetbixBreakfast = ((OptionCombobox)result[4]).Options[nameof(WeetbixBreakfast)].ToArray();
            var oatsBreakfast = ((OptionCombobox)result[4]).Options[nameof(OatsBreakfast)].ToArray();

            Assert.AreEqual(2, weetbixBreakfast.Count());
            Assert.AreEqual(2, oatsBreakfast.Count());

            Assert.AreEqual("NumPieces", weetbixBreakfast[0].Name);
            Assert.AreEqual("Milk", weetbixBreakfast[1].Name);

            Assert.AreEqual("SpoonCount", oatsBreakfast[0].Name);
            Assert.AreEqual("Temperature", oatsBreakfast[1].Name);
        }

        private static IOption[] ConvertDataToOptionsList()
        {
            var cls = new TestData
            {
                MyInt = 12,
                MyString = "hello",
                MyBool = true,
                MyEnum = (int)MyOptions.Option1,
                MyBreakfast = new OatsBreakfast()
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
            public int MyEnum { get; set; }


            [DataValue(nameof(BreakfastBase.BreakfastType))]
            [MultiDataProvider(typeof(OatsBreakfast), typeof(WeetbixBreakfast))]
            public BreakfastBase MyBreakfast { get; set; }

            public class OptionsForS : IOptionDataList<string>
            {
                public string[] Items => new string[] { "sopt1", "sopt2", "sopt3", "sopt4" };
            }
        }

        public class BreakfastBase
        {
            public BreakfastType BreakfastType { get; set; }
        }

        public enum BreakfastType
        {
            Weetbix,
            Oats
        }

        public class OatsBreakfast : BreakfastBase
        {
            public int SpoonCount { get; set; }
            public int Temperature { get; set; }
        }

        public class WeetbixBreakfast : BreakfastBase
        {
            public int NumPieces { get; set; }
            public bool Milk { get; set; }
        }

    }
}
