/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using Dev2.Common.Interfaces.Data;
using Dev2.DataList.Contract;
using Dev2.Tests.Properties;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Unlimited.UnitTest.Framework.Parsing
{
    [TestClass]
    public class LanguageParserTest
    {

        #region Additional Test Attributes

        [TestInitialize]
        public void LanguageTestInitialize()
        {

        }

        [TestCleanup]
        public void LanguageTestCleanup()
        {
        }

        #endregion Additional Test Attributes

        #region Input Parse Tests

        // Travis Added : PBI 5779
        [TestMethod]
        public void Parse_InputMappingWithEmptyToNullTrue_Expected_InputCreateWithPropertyEmptyToNullSetTrue()
        {
            var inputs = DataListFactory.CreateInputParser().Parse(TestStrings.inputsWithEmptyToNullTrue);

            Assert.IsTrue(inputs.Count == 2 && inputs[0].EmptyToNull);
        }

        // Travis Added : PBI 5779
        [TestMethod]
        public void InputMappingWithEmptyToNullFalse_Expected_InputCreateWithPropertyEmptyToNullSetFalse()
        {
            var inputs = DataListFactory.CreateInputParser().Parse(TestStrings.inputsWithEmptyToNullFalse);

            Assert.IsTrue(inputs.Count == 2 && !inputs[0].EmptyToNull);
        }

        // Sashen Added : PBI 5779
        [TestMethod]
        public void Parse_EmptyToNullAttributeNotInXML_InputMappingWithEmptyToFalse()
        {
            var inputs = DataListFactory.CreateInputParser().Parse(TestStrings.inputsWithEmptyToNullNotInXML);

            Assert.IsTrue(inputs.Count == 2 && !inputs[0].EmptyToNull);
        }

        [TestMethod]
        public void TestInputMappingExtactScalars()
        {
            var inputs = DataListFactory.CreateInputParser().Parse(TestStrings.sampleActivityInputScalar);

            Assert.IsTrue(inputs.Count == 2 && inputs[0].Name == "fname");
        }

        [TestMethod]
        public void TestInputMappingExtractRecordSet()
        {
            var inputs = DataListFactory.CreateInputParser().Parse(TestStrings.sampleActivityInputRecordSet);

            Assert.IsTrue(inputs.Count == 1 && inputs[0].Name == "Person");
        }

        [TestMethod]
        public void TestInputMappingExtractMixed()
        {
            var inputs = DataListFactory.CreateInputParser().Parse(TestStrings.sampleActivityInputMixed);

            Assert.IsTrue(inputs.Count == 2);
        }

        [TestMethod]
        public void TestInputMappingExtractRequiredRegions()
        {
            var inputs = DataListFactory.CreateInputParser().Parse(TestStrings.inputMappingRequiredRegion);

            Assert.IsTrue(inputs.Count == 7 && inputs[0].Name == "Host" && inputs[0].DefaultValue == "mail.bellevuenet.co.za" && inputs[0].IsRequired);
        }

        #endregion Parse Tests
    }
}
