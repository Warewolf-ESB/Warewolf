/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces.Enums.Enums;
using Dev2.Data.Interfaces.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Common.Tests
{
    [TestClass]
    public class Dev2EnumConverterTests
    {
        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(Dev2EnumConverter))]
        public void Dev2EnumConverter_ConvertEnumValueToString_AreEqual_ExpectTrue()
        {
            //----------------------Arrange-------------------------
            //----------------------Act-----------------------------
            var convertEnumValueToString = enRoundingType.Up.GetDescription();
            //----------------------Assert--------------------------
            Assert.AreEqual("Up", convertEnumValueToString);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(Dev2EnumConverter))]
        public void Dev2EnumConverter_GetEnumFromStringDiscription_WhenDiscriptionIsNotMacthedInEnum_IsNull_ExpectTrue()
        {
            //----------------------Arrange-------------------------
            //----------------------Act-----------------------------
            var convertEnumValueToString = Dev2EnumConverter.GetEnumFromStringDiscription("TestDiscription", typeof(enRoundingType));
            //----------------------Assert--------------------------
            Assert.IsNull(convertEnumValueToString);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(Dev2EnumConverter))]
        public void Dev2EnumConverter_GetEnumFromStringDiscription_WhenDiscriptionIsMacthedInEnum_AreEqual_ExpectTrue()
        {
            //----------------------Arrange-------------------------
            var discription = "Up";
            //----------------------Act-----------------------------
            var convertEnumValueToString = Dev2EnumConverter.GetEnumFromStringDiscription(discription, typeof(enRoundingType));
            //----------------------Assert--------------------------
            Assert.AreEqual(discription, convertEnumValueToString.ToString());
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(Dev2EnumConverter))]
        public void Dev2EnumConverter_GetEnumFromStringDiscription_WhenTypeIsNotEnum_ExpectInvalidOperationException()
        {
            //----------------------Arrange-------------------------
            //----------------------Act-----------------------------
            //----------------------Assert--------------------------
            Assert.ThrowsException<InvalidOperationException>(() => Dev2EnumConverter.GetEnumFromStringDiscription("Up", typeof(object)));
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(Dev2EnumConverter))]
        public void Dev2EnumConverter_ConvertEnumsTypeToStringList_GetType_AreEqual_ExpectTrue()
        {
            //----------------------Arrange-------------------------
            var testList = new List<string>();
            testList.Add("None");
            testList.Add("Normal");
            testList.Add("Up");
            testList.Add("Down");
            //----------------------Act-----------------------------
            var convertEnumsTypeToStringList = Dev2EnumConverter.ConvertEnumsTypeToStringList<enRoundingType>();
            //----------------------Assert--------------------------
            Assert.AreEqual(testList.GetType(), convertEnumsTypeToStringList.GetType());
        }
    }
}
