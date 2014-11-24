﻿
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Diagnostics.CodeAnalysis;
using Dev2.Common.ExtMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace
namespace Dev2.Common.Test
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class StringExtensionTests
    {


        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("StringExtensions_IsJSON")]
        public void StringExtensionss_IsJSON_WhenValidJSON_ExpectTrue()
        {
            //------------Setup for test--------------------------
            const string fragment = "{}";

            //------------Execute Test---------------------------
            var result = fragment.IsJSON();

            //------------Assert Results-------------------------
            Assert.IsTrue(result);
        }


        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("StringExtensions_IsJSON")]
        public void StringExtensionss_IsJSON_WhenValidXML_ExpectFalse()
        {
            //------------Setup for test--------------------------
            const string fragment = "<x></x>";

            //------------Execute Test---------------------------
            var result = fragment.IsJSON();

            //------------Assert Results-------------------------
            Assert.IsFalse(result);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("StringExtensions_IsJSON")]
        public void StringExtensionss_IsJSON_WhenValidText_ExpectFalse()
        {
            //------------Setup for test--------------------------
            const string fragment = "{ hello } { name }";

            //------------Execute Test---------------------------
            var result = fragment.IsJSON();

            //------------Assert Results-------------------------
            Assert.IsFalse(result);
        }

        [TestMethod]
        [TestCategory("StringExtensionUnitTest")]
        [Description("Test for 'ValidateCategoryName' string extension method: A valid resource category name ('new_category.var') is passed to it and true is expected to be returned back")]
        [Owner("Ashley Lewis")]
        // ReSharper disable InconsistentNaming
        public void StringExtension_StringExtensionUnitTest_ValidateCategoryName_TrueIsReturned()
        // ReSharper restore InconsistentNaming
        {
            Assert.IsTrue("new_category.var".IsValidCategoryName(), "Valid category name was rejected by the validation function");
        }

        [TestMethod]
        [TestCategory("StringExtensionUnitTest")]
        [Description("Test for 'ValidateCategoryName' string extention method: An invalid resource category name ('new/<category>') is passed to it and true is expected to be returned back")]
        [Owner("Ashley Lewis")]
        // ReSharper disable InconsistentNaming
        public void StringExtension_StringExtensionUnitTest_ValidateCategoryName_FalseIsReturned()
        // ReSharper restore InconsistentNaming
        {
            Assert.IsFalse("new/<category>".IsValidCategoryName(), "Invalid category name passed validation");
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("StringExtensions_Escape")]
        public void StringExtensions_Escape_EscapeXml_XmlIsEscaped()
        {
            //------------Setup for test--------------------------
            const string xml = @"<ResourceXml><Service ID=""9e62d8ec-41f1-4613-8439-1f9f7c6a2c68"" Version=""1.0"" ServerID=""51a58300-7e9d-4927-a57b-e5d700b11b55"" Name=""ForEachUpgradeTest"" ResourceType=""WorkflowService"" IsValid=""true"">
  <DisplayName>ForEachUpgradeTest</DisplayName>
  <Category>Mo</Category>
  <IsNewWorkflow>false</IsNewWorkflow>
  <AuthorRoles></AuthorRoles>
  <Comment></Comment>
  <Tags></Tags>
  <IconPath>pack://application:,,,/Warewolf Studio;component/images/Workflow-32.png</IconPath>
  <HelpLink></HelpLink>
  <UnitTestTargetWorkflowService></UnitTestTargetWorkflowService></ResourceXml>";

            //------------Execute Test---------------------------

            string actual = xml.Escape();

            //------------Assert Results-------------------------

            StringAssert.Contains(actual, "&gt;");
            StringAssert.Contains(actual, "&lt;");
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("StringExtension_Text")]
        public void StringExtensions_Text_NotLatinCharacter_ShowMessageBox_TextMadeEmpty()
        {
            //------------Setup for test-------------------------
            const string Text = "أَبْجَدِي";
            //------------Execute Test---------------------------
            var checkHasUnicodeInText = Text.ContainsUnicodeCharacter();
            //------------Assert Results-------------------------
            Assert.IsTrue(checkHasUnicodeInText);
        }
    }
}
