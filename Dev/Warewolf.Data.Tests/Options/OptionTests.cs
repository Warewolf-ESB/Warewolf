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
using Warewolf.Options;

namespace Warewolf.Data.Tests
{
    [TestClass]
    public class OptionTests
    {
        [TestMethod]
        [TestCategory(nameof(OptionBool))]
        [Owner("Pieter Terblanche")]
        public void OptionBool_Default()
        {
            var optionBool = new OptionBool();

            Assert.IsNull(optionBool.Name);
            optionBool.Name = "Durable";
            Assert.AreEqual("Durable", optionBool.Name);

            Assert.IsFalse(optionBool.Value);
            optionBool.Value = true;
            Assert.IsTrue(optionBool.Value);

            Assert.IsTrue(optionBool.Default);
        }

        [TestMethod]
        [TestCategory(nameof(OptionBool))]
        [Owner("Pieter Terblanche")]
        public void OptionBool_Clone()
        {
            var optionBool = new OptionBool
            {
                Name = "Durable",
                Value = true
            };

            var cloneOptionBool = optionBool.Clone() as OptionBool;
            Assert.AreEqual(optionBool.Name, cloneOptionBool.Name);
            Assert.AreEqual(optionBool.Value, cloneOptionBool.Value);
        }

        [TestMethod]
        [TestCategory(nameof(OptionBool))]
        [Owner("Pieter Terblanche")]
        public void OptionBool_CompareTo()
        {
            var optionBool = new OptionBool
            {
                Name = "Durable",
                Value = true
            };

            var expectedValue = optionBool.CompareTo(null);
            Assert.AreEqual(-1, expectedValue);

            expectedValue = optionBool.CompareTo(new object { });
            Assert.AreEqual(-1, expectedValue);

            expectedValue = optionBool.CompareTo(optionBool);
            Assert.AreEqual(0, expectedValue);
        }

        [TestMethod]
        [TestCategory(nameof(OptionInt))]
        [Owner("Pieter Terblanche")]
        public void OptionInt_Default()
        {
            var optionInt = new OptionInt();

            Assert.IsNull(optionInt.Name);
            optionInt.Name = "MaxAllowed";
            Assert.AreEqual("MaxAllowed", optionInt.Name);

            Assert.AreEqual(0, optionInt.Value);
            optionInt.Value = 10;
            Assert.AreEqual(10, optionInt.Value);

            Assert.AreEqual(0, optionInt.Default);
        }

        [TestMethod]
        [TestCategory(nameof(OptionInt))]
        [Owner("Pieter Terblanche")]
        public void OptionInt_Clone()
        {
            var optionInt = new OptionInt
            {
                Name = "MaxAllowed",
                Value = 10
            };

            var cloneOptionBool = optionInt.Clone() as OptionInt;
            Assert.AreEqual(optionInt.Name, cloneOptionBool.Name);
            Assert.AreEqual(optionInt.Value, cloneOptionBool.Value);
        }

        [TestMethod]
        [TestCategory(nameof(OptionInt))]
        [Owner("Pieter Terblanche")]
        public void OptionInt_CompareTo()
        {
            var optionInt = new OptionInt
            {
                Name = "MaxAllowed",
                Value = 10
            };

            var expectedValue = optionInt.CompareTo(null);
            Assert.AreEqual(-1, expectedValue);

            expectedValue = optionInt.CompareTo(new object { });
            Assert.AreEqual(-1, expectedValue);

            expectedValue = optionInt.CompareTo(optionInt);
            Assert.AreEqual(0, expectedValue);
        }

        [TestMethod]
        [TestCategory(nameof(OptionAutocomplete))]
        [Owner("Pieter Terblanche")]
        public void OptionAutocomplete_Default()
        {
            var optionAutocomplete = new OptionAutocomplete();

            Assert.IsNull(optionAutocomplete.Name);
            optionAutocomplete.Name = "Suggestions";
            Assert.AreEqual("Suggestions", optionAutocomplete.Name);

            Assert.IsNull(optionAutocomplete.Value);
            optionAutocomplete.Value = "Item1";
            Assert.AreEqual("Item1", optionAutocomplete.Value);

            Assert.AreEqual(string.Empty, optionAutocomplete.Default);
            Assert.IsNull(optionAutocomplete.Suggestions);
        }

        [TestMethod]
        [TestCategory(nameof(OptionAutocomplete))]
        [Owner("Pieter Terblanche")]
        public void OptionAutocomplete_Clone()
        {
            var optionAutocomplete = new OptionAutocomplete
            {
                Name = "Suggestions",
                Value = "Item1"
            };

            var cloneOptionBool = optionAutocomplete.Clone() as OptionAutocomplete;
            Assert.AreEqual(optionAutocomplete.Name, cloneOptionBool.Name);
            Assert.AreEqual(optionAutocomplete.Value, cloneOptionBool.Value);
        }

        [TestMethod]
        [TestCategory(nameof(OptionAutocomplete))]
        [Owner("Pieter Terblanche")]
        public void OptionAutocomplete_CompareTo()
        {
            var optionAutocomplete = new OptionAutocomplete
            {
                Name = "Suggestions",
                Value = "Item1"
            };

            var expectedValue = optionAutocomplete.CompareTo(null);
            Assert.AreEqual(-1, expectedValue);

            expectedValue = optionAutocomplete.CompareTo(new object { });
            Assert.AreEqual(-1, expectedValue);

            expectedValue = optionAutocomplete.CompareTo(optionAutocomplete);
            Assert.AreEqual(0, expectedValue);
        }
    }
}
