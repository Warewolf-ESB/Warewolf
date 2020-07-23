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
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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

            Assert.AreEqual(6, result.Length);
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
            var expected = new TestData.OptionsForS().Items;
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
        public void OptionConvertor_GivenSimpleClass_ExpectedEnumOption_Success()
        {
            //----------------------Arrange----------------------
            //----------------------Act--------------------------
            var actual = ConvertDataToOptionsList();
            //----------------------Assert-----------------------
            Assert.AreEqual(nameof(TestData.MyEnum), actual[3].Name);
            Assert.AreEqual(0, ((OptionInt)actual[3]).Value);
        }

        [TestMethod]

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

        [TestMethod]

        public void OptionConvertor_Given_NamedGuid_ReturnSuccess()
        {
            //----------------------Arrange----------------------
            //----------------------Act--------------------------
            var result = ConvertDataToOptionsList();

            var optionSourceCombobox = (OptionSourceCombobox)result[5];
            Assert.AreEqual("MyNamedGuid", optionSourceCombobox.Name);
            Assert.AreEqual(2, optionSourceCombobox.Options.Count());
            Assert.AreEqual(2, optionSourceCombobox.OptionNames.Count());

            Assert.AreEqual("sopt1", optionSourceCombobox.Options[0].Name);
            Assert.AreEqual(Guid.Empty, optionSourceCombobox.Options[0].Value);
            Assert.AreEqual("sopt2", optionSourceCombobox.Options[1].Name);
            Assert.AreNotEqual(Guid.Empty, optionSourceCombobox.Options[1].Value);

            Assert.AreEqual("OptionComboboxHelpText", optionSourceCombobox.HelpText);
            Assert.AreEqual("OptionComboboxTooltip", optionSourceCombobox.Tooltip);
        }

        private static IOption[] ConvertDataToOptionsList()
        {
            var cls = new TestData
            {
                MyInt = 12,
                MyString = "hello",
                MyBool = true,
                MyEnum = (int)MyOptions.Option1,
                MyBreakfast = new OatsBreakfast(),
                MyNamedGuid = new NamedGuid { Name = "Item", Value = Guid.Empty },
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

            [DataProvider(typeof(ResourceDataTest))]
            [HelpText(nameof(Studio.Resources.Languages.HelpText.OptionComboboxHelpText))]
            [Tooltip(nameof(Studio.Resources.Languages.Tooltips.OptionComboboxTooltip))]
            public NamedGuid MyNamedGuid { get; set; }



            public class OptionsForS : IOptionDataList<string>
            {
                public string[] Items => new string[] { "sopt1", "sopt2", "sopt3", "sopt4" };
            }

            public class ResourceDataTest : IOptionDataList<INamedGuid>
            {
                public INamedGuid[] Items => new NamedGuid[]
                {
                    new NamedGuid {Name = "sopt1", Value = Guid.Empty },
                    new NamedGuid {Name = "sopt2", Value = Guid.NewGuid() },
                };
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
