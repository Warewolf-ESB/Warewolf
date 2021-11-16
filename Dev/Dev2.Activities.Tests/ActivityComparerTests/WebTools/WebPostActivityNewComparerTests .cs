/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using Dev2.Activities;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.Graph;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Framework.Converters.Graph.Ouput;

namespace Dev2.Tests.Activities.ActivityComparerTests.WebTools
{
    [TestClass]
    public class WebPostActivityNewComparerTests
    {
        [TestMethod]
        [Timeout(60000)]
        [Owner("Njabulo Nxele")]
        [TestCategory(nameof(WebPostActivityNew))]
        public void WebPostActivity_Equals_Given_SameUniqueID_EmptyWebPostTools_AreEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var webPostActivity = new WebPostActivityNew {UniqueID = uniqueId};
            var webPostActivity1 = new WebPostActivityNew {UniqueID = uniqueId};
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webPostActivity);
            //---------------Execute Test ----------------------
            var equals = webPostActivity.Equals(webPostActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Njabulo Nxele")]
        [TestCategory(nameof(WebPostActivityNew))]
        public void WebPostActivity_Equals_Given_DifferentWebPostToolIds_AreNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var uniqueId2 = Guid.NewGuid().ToString();
            var webPostActivity = new WebPostActivityNew {UniqueID = uniqueId};
            var webPostActivity1 = new WebPostActivityNew {UniqueID = uniqueId2};
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webPostActivity);
            //---------------Execute Test ----------------------
            var equals = webPostActivity.Equals(webPostActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Njabulo Nxele")]
        [TestCategory(nameof(WebPostActivityNew))]
        public void WebPostActivity_Equals_Given_SameWebPostTool_IsEqual()
        {
            //---------------Set up test pack-------------------
            var webPostActivity = new WebPostActivityNew();
            var webPostActivity1 = webPostActivity;
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webPostActivity);
            //---------------Execute Test ----------------------
            var equals = webPostActivity.Equals(webPostActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Njabulo Nxele")]
        [TestCategory(nameof(WebPostActivityNew))]
        public void WebPostActivity_Equals_Given_Same_DisplayName_Value_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var webPostActivity = new WebPostActivityNew {UniqueID = uniqueId, DisplayName = ""};
            var webPostActivity1 = new WebPostActivityNew {UniqueID = uniqueId, DisplayName = ""};
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webPostActivity);
            //---------------Execute Test ----------------------
            var equals = webPostActivity.Equals(webPostActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Njabulo Nxele")]
        [TestCategory(nameof(WebPostActivityNew))]
        public void WebPostActivity_Equals_Given_Different_DisplayName_Value_IsNOT_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var webPostActivity = new WebPostActivityNew {UniqueID = uniqueId, DisplayName = "A"};
            var webPostActivity1 = new WebPostActivityNew {UniqueID = uniqueId, DisplayName = ""};
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webPostActivity);
            //---------------Execute Test ----------------------
            var equals = webPostActivity.Equals(webPostActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Njabulo Nxele")]
        [TestCategory(nameof(WebPostActivityNew))]
        public void WebPostActivity_Equals_Given_Same_DisplayName_Value_Different_Casing_IsNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var webPostActivity = new WebPostActivityNew {UniqueID = uniqueId, DisplayName = "A"};
            var webPostActivity1 = new WebPostActivityNew {UniqueID = uniqueId, DisplayName = "a"};
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webPostActivity);
            //---------------Execute Test ----------------------
            var equals = webPostActivity.Equals(webPostActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Njabulo Nxele")]
        [TestCategory(nameof(WebPostActivityNew))]
        public void WebPostActivity_Equals_Given_Same_QueryString_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var webPostActivity = new WebPostActivityNew {UniqueID = uniqueId, QueryString = "A"};
            var webPostActivity1 = new WebPostActivityNew {UniqueID = uniqueId, QueryString = "A"};
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webPostActivity);
            //---------------Execute Test ----------------------
            var equals = webPostActivity.Equals(webPostActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Njabulo Nxele")]
        [TestCategory(nameof(WebPostActivityNew))]
        public void WebPostActivity_Equals_Given_Different_QueryString_IsNOTEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var webPostActivity = new WebPostActivityNew {UniqueID = uniqueId, QueryString = "A"};
            var webPostActivity1 = new WebPostActivityNew {UniqueID = uniqueId, QueryString = "B"};
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webPostActivity);
            //---------------Execute Test ----------------------
            var equals = webPostActivity.Equals(webPostActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Njabulo Nxele")]
        [TestCategory(nameof(WebPostActivityNew))]
        public void WebPostActivity_Equals_Given_Same_OutputDescription_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var outDesc = new OutputDescription();
            var webPostActivity = new WebPostActivityNew {UniqueID = uniqueId, OutputDescription = outDesc};
            var webPostActivity1 = new WebPostActivityNew {UniqueID = uniqueId, OutputDescription = outDesc};
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webPostActivity);
            //---------------Execute Test ----------------------
            var equals = webPostActivity.Equals(webPostActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Njabulo Nxele")]
        [TestCategory(nameof(WebPostActivityNew))]
        public void WebPostActivity_Equals_Given_Different_OutputDescription_IsNOTEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var outDesc = new OutputDescription
            {
                Format = OutputFormats.Unknown
            };
            var outDesc2 = new OutputDescription
            {
                Format = OutputFormats.ShapedXML
            };
            var webPostActivity = new WebPostActivityNew {UniqueID = uniqueId, OutputDescription = outDesc};
            var webPostActivity1 = new WebPostActivityNew {UniqueID = uniqueId, OutputDescription = outDesc2};
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webPostActivity);
            //---------------Execute Test ----------------------
            var equals = webPostActivity.Equals(webPostActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Njabulo Nxele")]
        [TestCategory(nameof(WebPostActivityNew))]
        public void WebPostActivity_Equals_Given_Different_Headers_IsNOTEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var headers = new List<INameValue>();
            var headers2 = new List<INameValue> {new NameValue("a", "x")};
            var webPostActivity = new WebPostActivityNew {UniqueID = uniqueId, Headers = headers};
            var webPostActivity1 = new WebPostActivityNew {UniqueID = uniqueId, Headers = headers2};
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webPostActivity);
            //---------------Execute Test ----------------------
            var equals = webPostActivity.Equals(webPostActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Njabulo Nxele")]
        [TestCategory(nameof(WebPostActivityNew))]
        public void WebPostActivity_Equals_Given_Same_Headers_DifferentIndexes_IsNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var headers = new List<INameValue> {new NameValue("b", "y"), new NameValue("a", "x")};
            var webPostActivity = new WebPostActivityNew {UniqueID = uniqueId, Headers = headers};
            var webPostActivity1 = new WebPostActivityNew {UniqueID = uniqueId, Headers = headers};
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webPostActivity);
            //---------------Execute Test ----------------------
            var equals = webPostActivity.Equals(webPostActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Njabulo Nxele")]
        [TestCategory(nameof(WebPostActivityNew))]
        public void WebPostActivity_Equals_Given_Same_Headers_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var headers = new List<INameValue> {new NameValue("a", "x")};
            var webPostActivity = new WebPostActivityNew {UniqueID = uniqueId, Headers = headers};
            var webPostActivity1 = new WebPostActivityNew {UniqueID = uniqueId, Headers = headers};
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webPostActivity);
            //---------------Execute Test ----------------------
            var equals = webPostActivity.Equals(webPostActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WebPostActivityNew))]
        public void WebPostActivity_Equals_Given_Same_IsFormDataChecked_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var settings = new List<INameValue>();
            settings.Add(new NameValue("IsFormDataChecked", "true"));
            var webPostActivity = new WebPostActivityNew {UniqueID = uniqueId,  Settings = settings};
            var webPostActivity1 = new WebPostActivityNew {UniqueID = uniqueId, Settings = settings};
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webPostActivity);
            //---------------Execute Test ----------------------
            var equals = webPostActivity.Equals(webPostActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Njabulo Nxele")]
        [TestCategory(nameof(WebPostActivityNew))]
        public void WebPostActivity_Equals_Given_Same_IsFormDataChecked_IsNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var settingsTrue = new List<INameValue>();
            settingsTrue.Add(new NameValue("IsFormDataChecked", "true"));
            var settingsFalse = new List<INameValue>();
            settingsFalse.Add(new NameValue("IsFormDataChecked", "false"));
            var webPostActivity = new WebPostActivityNew {UniqueID = uniqueId, Settings = settingsFalse};
            var webPostActivity1 = new WebPostActivityNew {UniqueID = uniqueId, Settings = settingsTrue};
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webPostActivity);
            //---------------Execute Test ----------------------
            var equals = webPostActivity.Equals(webPostActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Njabulo Nxele")]
        [TestCategory(nameof(WebPostActivityNew))]
        public void WebPostActivity_ReferenceEquals_Null_IsNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var settings = new List<INameValue>();
            settings.Add(new NameValue("IsFormDataChecked", "false"));
            var webPostActivity = new WebPostActivityNew {UniqueID = uniqueId, Settings = settings};
#pragma warning disable 618
            WebPostActivity webPostActivity1 = null;
#pragma warning restore 618
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webPostActivity);
            //---------------Execute Test ----------------------
            var equals = webPostActivity.Equals(webPostActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Njabulo Nxele")]
        [TestCategory(nameof(WebPostActivityNew))]
        public void WebPostActivity_ReferenceEquals_Null_Object_IsNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var settings = new List<INameValue>();
            settings.Add(new NameValue("IsFormDataChecked", "false"));
            var webPostActivity = new WebPostActivityNew {UniqueID = uniqueId, Settings = settings};
            object webPostActivity1 = null;
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webPostActivity);
            //---------------Execute Test ----------------------
            var equals = webPostActivity.Equals(webPostActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Njabulo Nxele")]
        [TestCategory(nameof(WebPostActivityNew))]
        public void WebPostActivity_ReferenceEquals_Given_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var settings = new List<INameValue>();
            settings.Add(new NameValue("IsFormDataChecked", "false"));
            var webPostActivity = new WebPostActivityNew {UniqueID = uniqueId, Settings = settings};
            object webPostActivity1 = webPostActivity;
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webPostActivity);
            //---------------Execute Test ----------------------
            var equals = webPostActivity.Equals(webPostActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Njabulo Nxele")]
        [TestCategory(nameof(WebPostActivityNew))]
        public void WebPostActivity_ReferenceEquals_Different_Type_IsNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var settings = new List<INameValue>();
            settings.Add(new NameValue("IsFormDataChecked", "false"));
            var webPostActivity = new WebPostActivityNew {UniqueID = uniqueId, Settings = settings};
            object webGetActivity = new WebGetActivity {UniqueID = uniqueId};
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webPostActivity);
            //---------------Execute Test ----------------------
            var equals = webPostActivity.Equals(webGetActivity);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Njabulo Nxele")]
        [TestCategory(nameof(WebPostActivityNew))]
        public void WebPostActivity_ReferenceEquals_Given_Same_Object_IsNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var settingsTrue = new List<INameValue>();
            settingsTrue.Add(new NameValue("IsFormDataChecked", "true"));
            var settingsFalse = new List<INameValue>();
            settingsFalse.Add(new NameValue("IsFormDataChecked", "false"));
            var webPostActivity = new WebPostActivityNew {UniqueID = uniqueId, Settings = settingsFalse};
            object webPostActivity1 = new WebPostActivityNew {UniqueID = uniqueId, Settings = settingsTrue};
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webPostActivity);
            //---------------Execute Test ----------------------
            var equals = webPostActivity.Equals(webPostActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Njabulo Nxele")]
        [TestCategory(nameof(WebPostActivityNew))]
        public void WebPostActivity_GetHashCode_IsNotNull_Expect_True()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var settings = new List<INameValue>();
            settings.Add(new NameValue("IsFormDataChecked", "true"));
            var webPostActivity = new WebPostActivityNew {UniqueID = uniqueId, Settings = settings};
            //---------------Execute Test ----------------------
            var hashCode = webPostActivity.GetHashCode();
            //---------------Test Result -----------------------
            Assert.IsNotNull(hashCode);
        }
    }
}