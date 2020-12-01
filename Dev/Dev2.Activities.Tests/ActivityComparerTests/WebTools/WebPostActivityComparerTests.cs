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
    public class WebPostActivityComparerTests
    {
        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WebPostActivity))]
        public void WebPostActivity_Equals_Given_SameUniqueID_EmptyWebPostTools_AreEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var webPostActivity = new WebPostActivity {UniqueID = uniqueId};
            var webPostActivity1 = new WebPostActivity {UniqueID = uniqueId};
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webPostActivity);
            //---------------Execute Test ----------------------
            var equals = webPostActivity.Equals(webPostActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WebPostActivity))]
        public void WebPostActivity_Equals_Given_DifferentWebPostToolIds_AreNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var uniqueId2 = Guid.NewGuid().ToString();
            var webPostActivity = new WebPostActivity {UniqueID = uniqueId};
            var webPostActivity1 = new WebPostActivity {UniqueID = uniqueId2};
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webPostActivity);
            //---------------Execute Test ----------------------
            var equals = webPostActivity.Equals(webPostActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WebPostActivity))]
        public void WebPostActivity_Equals_Given_SameWebPostTool_IsEqual()
        {
            //---------------Set up test pack-------------------
            var webPostActivity = new WebPostActivity();
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
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WebPostActivity))]
        public void WebPostActivity_Equals_Given_Same_DisplayName_Value_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var webPostActivity = new WebPostActivity {UniqueID = uniqueId, DisplayName = ""};
            var webPostActivity1 = new WebPostActivity {UniqueID = uniqueId, DisplayName = ""};
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webPostActivity);
            //---------------Execute Test ----------------------
            var equals = webPostActivity.Equals(webPostActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WebPostActivity))]
        public void WebPostActivity_Equals_Given_Different_DisplayName_Value_IsNOT_Equal()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var webPostActivity = new WebPostActivity {UniqueID = uniqueId, DisplayName = "A"};
            var webPostActivity1 = new WebPostActivity {UniqueID = uniqueId, DisplayName = ""};
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webPostActivity);
            //---------------Execute Test ----------------------
            var equals = webPostActivity.Equals(webPostActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WebPostActivity))]
        public void WebPostActivity_Equals_Given_Same_DisplayName_Value_Different_Casing_IsNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var webPostActivity = new WebPostActivity {UniqueID = uniqueId, DisplayName = "A"};
            var webPostActivity1 = new WebPostActivity {UniqueID = uniqueId, DisplayName = "a"};
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webPostActivity);
            //---------------Execute Test ----------------------
            var equals = webPostActivity.Equals(webPostActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WebPostActivity))]
        public void WebPostActivity_Equals_Given_Same_QueryString_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var webPostActivity = new WebPostActivity {UniqueID = uniqueId, QueryString = "A"};
            var webPostActivity1 = new WebPostActivity {UniqueID = uniqueId, QueryString = "A"};
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webPostActivity);
            //---------------Execute Test ----------------------
            var equals = webPostActivity.Equals(webPostActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WebPostActivity))]
        public void WebPostActivity_Equals_Given_Different_QueryString_IsNOTEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var webPostActivity = new WebPostActivity {UniqueID = uniqueId, QueryString = "A"};
            var webPostActivity1 = new WebPostActivity {UniqueID = uniqueId, QueryString = "B"};
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webPostActivity);
            //---------------Execute Test ----------------------
            var equals = webPostActivity.Equals(webPostActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WebPostActivity))]
        public void WebPostActivity_Equals_Given_Same_OutputDescription_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var outDesc = new OutputDescription();
            var webPostActivity = new WebPostActivity {UniqueID = uniqueId, OutputDescription = outDesc};
            var webPostActivity1 = new WebPostActivity {UniqueID = uniqueId, OutputDescription = outDesc};
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webPostActivity);
            //---------------Execute Test ----------------------
            var equals = webPostActivity.Equals(webPostActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WebPostActivity))]
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
            var webPostActivity = new WebPostActivity {UniqueID = uniqueId, OutputDescription = outDesc};
            var webPostActivity1 = new WebPostActivity {UniqueID = uniqueId, OutputDescription = outDesc2};
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webPostActivity);
            //---------------Execute Test ----------------------
            var equals = webPostActivity.Equals(webPostActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WebPostActivity))]
        public void WebPostActivity_Equals_Given_Different_Headers_IsNOTEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var headers = new List<INameValue>();
            var headers2 = new List<INameValue> {new NameValue("a", "x")};
            var webPostActivity = new WebPostActivity {UniqueID = uniqueId, Headers = headers};
            var webPostActivity1 = new WebPostActivity {UniqueID = uniqueId, Headers = headers2};
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webPostActivity);
            //---------------Execute Test ----------------------
            var equals = webPostActivity.Equals(webPostActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WebPostActivity))]
        public void WebPostActivity_Equals_Given_Same_Headers_DifferentIndexes_IsNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var headers = new List<INameValue> {new NameValue("b", "y"), new NameValue("a", "x")};
            var webPostActivity = new WebPostActivity {UniqueID = uniqueId, Headers = headers};
            var webPostActivity1 = new WebPostActivity {UniqueID = uniqueId, Headers = headers};
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webPostActivity);
            //---------------Execute Test ----------------------
            var equals = webPostActivity.Equals(webPostActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WebPostActivity))]
        public void WebPostActivity_Equals_Given_Same_Headers_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var headers = new List<INameValue> {new NameValue("a", "x")};
            var webPostActivity = new WebPostActivity {UniqueID = uniqueId, Headers = headers};
            var webPostActivity1 = new WebPostActivity {UniqueID = uniqueId, Headers = headers};
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webPostActivity);
            //---------------Execute Test ----------------------
            var equals = webPostActivity.Equals(webPostActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WebPostActivity))]
        public void WebPostActivity_Equals_Given_Same_IsPostDataBase64_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var webPostActivity = new WebPostActivity {UniqueID = uniqueId, IsPostDataBase64 = true};
            var webPostActivity1 = new WebPostActivity {UniqueID = uniqueId, IsPostDataBase64 = true};
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webPostActivity);
            //---------------Execute Test ----------------------
            var equals = webPostActivity.Equals(webPostActivity1);
            //---------------Test Result -----------------------
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WebPostActivity))]
        public void WebPostActivity_Equals_Given_Same_IsPostDataBase64_IsNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var webPostActivity = new WebPostActivity {UniqueID = uniqueId, IsPostDataBase64 = false};
            var webPostActivity1 = new WebPostActivity {UniqueID = uniqueId, IsPostDataBase64 = true};
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webPostActivity);
            //---------------Execute Test ----------------------
            var equals = webPostActivity.Equals(webPostActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WebPostActivity))]
        public void WebPostActivity_ReferenceEquals_Null_IsNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var webPostActivity = new WebPostActivity {UniqueID = uniqueId, IsPostDataBase64 = false};
            WebPostActivity webPostActivity1 = null;
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webPostActivity);
            //---------------Execute Test ----------------------
            var equals = webPostActivity.Equals(webPostActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WebPostActivity))]
        public void WebPostActivity_ReferenceEquals_Null_Object_IsNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var webPostActivity = new WebPostActivity {UniqueID = uniqueId, IsPostDataBase64 = false};
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
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WebPostActivity))]
        public void WebPostActivity_ReferenceEquals_Given_Same_Object_IsEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var webPostActivity = new WebPostActivity {UniqueID = uniqueId, IsPostDataBase64 = false};
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
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WebPostActivity))]
        public void WebPostActivity_ReferenceEquals_Different_Type_IsNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var webPostActivity = new WebPostActivity {UniqueID = uniqueId, IsPostDataBase64 = false};
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
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WebPostActivity))]
        public void WebPostActivity_ReferenceEquals_Given_Same_Object_IsNotEqual()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var webPostActivity = new WebPostActivity {UniqueID = uniqueId, IsPostDataBase64 = false};
            object webPostActivity1 = new WebPostActivity {UniqueID = uniqueId, IsPostDataBase64 = true};
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webPostActivity);
            //---------------Execute Test ----------------------
            var equals = webPostActivity.Equals(webPostActivity1);
            //---------------Test Result -----------------------
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WebPostActivity))]
        public void WebPostActivity_GetHashCode_IsNotNull_Expect_True()
        {
            //---------------Set up test pack-------------------
            var uniqueId = Guid.NewGuid().ToString();
            var webPostActivity = new WebPostActivity {UniqueID = uniqueId, IsPostDataBase64 = false};
            //---------------Execute Test ----------------------
            var hashCode = webPostActivity.GetHashCode();
            //---------------Test Result -----------------------
            Assert.IsNotNull(hashCode);
        }
    }
}