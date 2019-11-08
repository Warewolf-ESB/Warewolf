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
using System.Collections.Generic;
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

            Assert.AreEqual("OptionBoolHelpText", optionBool.HelpText);
            Assert.AreEqual("OptionBoolTooltip", optionBool.Tooltip);
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

            Assert.AreEqual("OptionIntHelpText", optionInt.HelpText);
            Assert.AreEqual("OptionIntTooltip", optionInt.Tooltip);
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

            Assert.AreEqual("OptionAutocompleteHelpText", optionAutocomplete.HelpText);
            Assert.AreEqual("OptionAutocompleteTooltip", optionAutocomplete.Tooltip);
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

        [TestMethod]
        [TestCategory(nameof(OptionEnum))]
        [Owner("Siphamandla Dube")]
        public void OptionEnum_Default()
        {
            var optionEnum = new OptionEnum();

            Assert.IsNull(optionEnum.Name);
            optionEnum.Name = "MyEnum";
            Assert.AreEqual("MyEnum", optionEnum.Name);

            Assert.IsNotNull(optionEnum.Value);
            optionEnum.Value = (int)MyEnum.Option2;
            Assert.AreEqual((int)MyEnum.Option2, optionEnum.Value);

            Assert.IsNotNull(optionEnum.Default);
            optionEnum.Default = (int)MyEnum.Option1;
            Assert.AreEqual((int)MyEnum.Option1, optionEnum.Default);

            Assert.AreEqual("OptionEnumHelpText", optionEnum.HelpText);
            Assert.AreEqual("OptionEnumTooltip", optionEnum.Tooltip);
        }

        [TestMethod]
        [TestCategory(nameof(OptionEnum))]
        [Owner("Pieter Terblanche")]
        public void OptionEnum_OptionNames()
        {
            var optionEnum = new OptionEnum();

            var values = new List<KeyValuePair<string, int>>
            {
                new KeyValuePair<string, int>("Yes", 0),
                new KeyValuePair<string, int>("No", 1)
            };

            optionEnum.Values = values;

            Assert.AreEqual(2, optionEnum.OptionNames.Count);
            Assert.AreEqual("Yes", optionEnum.OptionNames[0].ToString());
            Assert.AreEqual("No", optionEnum.OptionNames[1].ToString());

            Assert.IsNull(optionEnum.OptionName);

            optionEnum.OptionName = "No";

            Assert.AreEqual(1, optionEnum.Value);
        }

        [TestMethod]
        [TestCategory(nameof(OptionEnum))]
        [Owner("Siphamandla Dube")]
        public void OptionEnum_Clone()
        {
            var optionEnum = new OptionEnum
            {
                Name = "MyEnum",
                Value = (int)MyEnum.Option2
            };

            var cloneOptionEnum = optionEnum.Clone() as OptionEnum;
            Assert.AreEqual(optionEnum.Name, cloneOptionEnum.Name);
            Assert.AreEqual(optionEnum.Value, cloneOptionEnum.Value);
        }

        [TestMethod]
        [TestCategory(nameof(OptionEnum))]
        [Owner("Siphamandla Dube")]
        public void OptionEnum_CompareTo()
        {
            var optionEnum = new OptionEnum
            {
                Name = "MyEnum",
                Value = (int)MyEnum.Option2
            };

            var expectedValue = optionEnum.CompareTo(null);
            Assert.AreEqual(-1, expectedValue);

            expectedValue = optionEnum.CompareTo(new object { });
            Assert.AreEqual(-1, expectedValue);

            expectedValue = optionEnum.CompareTo(optionEnum);
            Assert.AreEqual(0, expectedValue);
        }

        [TestMethod]
        [TestCategory(nameof(OptionEnumGen))]
        [Owner("Siphamandla Dube")]
        public void OptionEnumGen_Default()
        {
            var optionEnum = new OptionEnumGen();

            Assert.IsNull(optionEnum.Name);
            optionEnum.Name = "MyEnum";
            Assert.AreEqual("MyEnum", optionEnum.Name);

            Assert.AreEqual(new KeyValuePair<string, int>(), optionEnum.Value);
            optionEnum.Value = new KeyValuePair<string, int>(MyEnum.Option2.ToString(), (int)MyEnum.Option2);
            Assert.AreEqual(MyEnum.Option2.ToString(), optionEnum.Value.Key);
            Assert.AreEqual(1, optionEnum.Value.Value);

            Assert.AreEqual("OptionEnumGenHelpText", optionEnum.HelpText);
            Assert.AreEqual("OptionEnumGenTooltip", optionEnum.Tooltip);
        }

        [TestMethod]
        [TestCategory(nameof(OptionEnumGen))]
        [Owner("Siphamandla Dube")]
        public void OptionEnumGen_Clone()
        {
            var optionEnumGen = new OptionEnumGen
            {
                Name = "MyEnum",
                Value = new KeyValuePair<string, int>(MyEnum.Option2.ToString(), (int)MyEnum.Option2)
            };

            var cloneOptionEnumGen = optionEnumGen.Clone() as OptionEnumGen;
            Assert.AreEqual(optionEnumGen.Name, cloneOptionEnumGen.Name);
            Assert.AreEqual(optionEnumGen.Value, cloneOptionEnumGen.Value);
        }

        [TestMethod]
        [TestCategory(nameof(OptionEnumGen))]
        [Owner("Siphamandla Dube")]
        public void OptionEnumGen_CompareTo()
        {
            var optionEnumGen = new OptionEnumGen
            {
                Name = "MyEnum",
                Value = new KeyValuePair<string, int>(MyEnum.Option2.ToString(), (int)MyEnum.Option2)
            };

            var expectedValue = optionEnumGen.CompareTo(null);
            Assert.AreEqual(-1, expectedValue);

            expectedValue = optionEnumGen.CompareTo(new object { });
            Assert.AreEqual(-1, expectedValue);

            expectedValue = optionEnumGen.CompareTo(optionEnumGen);
            Assert.AreEqual(0, expectedValue);
        }

        [TestMethod]
        [TestCategory(nameof(OptionEnumGen))]
        [Owner("Pieter Terblanche")]
        public void OptionCombobox_Default()
        {
            var optionCombobox = new OptionCombobox();

            Assert.IsNotNull(optionCombobox.Options);
            Assert.IsNotNull(optionCombobox.OptionNames);
            Assert.IsNotNull(optionCombobox.SelectedOptions);

            Assert.IsNull(optionCombobox.Name);
            optionCombobox.Name = "Name";
            Assert.AreEqual("Name", optionCombobox.Name);

            Assert.IsNull(optionCombobox.Value);
            optionCombobox.Value = "Item1";
            Assert.AreEqual("Item1", optionCombobox.Value);

            Assert.AreEqual("OptionComboboxHelpText", optionCombobox.HelpText);
            Assert.AreEqual("OptionComboboxTooltip", optionCombobox.Tooltip);
        }

        [TestMethod]
        [TestCategory(nameof(OptionCombobox))]
        [Owner("Siphamandla Dube")]
        public void OptionCombobox_Clone()
        {
            var optionCombobox = new OptionCombobox
            {
                Name = "MyEnum",
                Value = MyEnum.Option1.ToString()
            };

            Assert.ThrowsException<NotImplementedException>(() => optionCombobox.Clone() as OptionCombobox);
        }

        [TestMethod]
        [TestCategory(nameof(OptionCombobox))]
        [Owner("Siphamandla Dube")]
        public void OptionCombobox_CompareTo()
        {
            var optionCombobox = new OptionCombobox
            {
                Name = "MyEnum",
                Value = MyEnum.Option1.ToString()
            };

            var expectedValue = optionCombobox.CompareTo(null);
            Assert.AreEqual(-1, expectedValue);

            expectedValue = optionCombobox.CompareTo(new object { });
            Assert.AreEqual(-1, expectedValue);

            expectedValue = optionCombobox.CompareTo(optionCombobox);
            Assert.AreEqual(0, expectedValue);
        }

        [TestMethod]
        [TestCategory(nameof(OptionWorkflow))]
        [Owner("Pieter Terblanche")]
        public void OptionWorkflow_Default()
        {
            var guid = Guid.NewGuid();
            var optionWorkflow = new OptionWorkflow();

            Assert.IsNull(optionWorkflow.Name);
            optionWorkflow.Name = "Name";
            Assert.AreEqual("Name", optionWorkflow.Name);

            Assert.IsNotNull(optionWorkflow.Value);
            Assert.AreEqual(Guid.Empty, optionWorkflow.Value);
            optionWorkflow.Value = guid;
            Assert.AreEqual(guid, optionWorkflow.Value);

            Assert.IsNull(optionWorkflow.WorkflowName);
            optionWorkflow.WorkflowName = "WorkflowName";
            Assert.AreEqual("WorkflowName", optionWorkflow.WorkflowName);

            Assert.AreEqual(Guid.Empty, ((IOptionBasic<Guid>)optionWorkflow).Default);

            Assert.IsNull(optionWorkflow.Inputs);
            optionWorkflow.Inputs = new List<IServiceInputBase>();
            Assert.IsNotNull(optionWorkflow.Inputs);

            Assert.AreEqual("OptionWorkflowHelpText", optionWorkflow.HelpText);
            Assert.AreEqual("OptionWorkflowTooltip", optionWorkflow.Tooltip);
        }

        [TestMethod]
        [TestCategory(nameof(OptionWorkflow))]
        [Owner("Pieter Terblanche")]
        public void OptionWorkflow_Clone()
        {
            var guid = Guid.NewGuid();
            var optionWorkflow = new OptionWorkflow
            {
                Name = "Suggestions",
                Value = guid
            };

            var cloneWorkflow = optionWorkflow.Clone() as OptionWorkflow;
            Assert.AreEqual(optionWorkflow.Name, cloneWorkflow.Name);
            Assert.AreEqual(optionWorkflow.Value, cloneWorkflow.Value);
        }

        [TestMethod]
        [TestCategory(nameof(OptionWorkflow))]
        [Owner("Pieter Terblanche")]
        public void OptionWorkflow_CompareTo()
        {
            var guid = Guid.NewGuid();
            var optionWorkflow = new OptionWorkflow
            {
                Name = "Suggestions",
                Value = guid
            };

            var expectedValue = optionWorkflow.CompareTo(null);
            Assert.AreEqual(-1, expectedValue);

            expectedValue = optionWorkflow.CompareTo(new object { });
            Assert.AreEqual(-1, expectedValue);

            expectedValue = optionWorkflow.CompareTo(optionWorkflow);
            Assert.AreEqual(0, expectedValue);
        }

        enum MyEnum
        {
            Option1,
            Option2
        }

    }
}
