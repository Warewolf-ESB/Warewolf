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
using System.Collections.Generic;
using System.Text;
using Dev2.Common.Interfaces.Enums;
using Dev2.Runtime.ESB.Management.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class TestRedisSourceTests
    {
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(TestRedisSource))]
        public void TestRedisSource_GetResourceID_ShouldReturnEmptyGuid()
        {
            //------------Setup for test--------------------------
            var testRedisSource = new TestRedisSource();
            //------------Execute Test---------------------------
            var resId = testRedisSource.GetResourceID(new Dictionary<string, StringBuilder>());
            //------------Assert Results-------------------------
            Assert.AreEqual(Guid.Empty, resId);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(TestRedisSource))]
        public void TestRedisSource_GetAuthorizationContextForService_ShouldReturnContext()
        {
            //------------Setup for test--------------------------
            var testRedisSource = new TestRedisSource();
            //------------Execute Test---------------------------
            var resId = testRedisSource.GetAuthorizationContextForService();
            //------------Assert Results-------------------------
            Assert.AreEqual(AuthorizationContext.Contribute, resId);
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(TestRedisSource))]
        public void TestRedisSource_HandlesType_ExpectName()
        {
            //------------Setup for test--------------------------
            var testRedisSource = new TestRedisSource();
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.AreEqual("TestRedisSource", testRedisSource.HandlesType());
        }
    }
}
