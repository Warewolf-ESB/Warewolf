/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Diagnostics.Debug;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Dev2.Diagnostics.Test.Debug
{
    [TestClass]
    public class TestDebugMessageRepoTests
    {
        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(TestDebugMessageRepo))]
        public void TestDebugMessageRepo_AddDebugItem_Invalid_Guid()
        {
            Guid.TryParse("test", out Guid resourceId);
            var debugState = new DebugState { ID = resourceId };

            var instance = TestDebugMessageRepo.Instance;

            instance.AddDebugItem(resourceId, "test name", debugState);
            var list = instance.FetchDebugItems(resourceId, "test name");

            Assert.IsNull(list);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(TestDebugMessageRepo))]
        public void TestDebugMessageRepo_AddDebugItem()
        {
            var resourceId = Guid.NewGuid();
            var debugState = new DebugState { ID = resourceId };

            var instance = TestDebugMessageRepo.Instance;

            instance.AddDebugItem(resourceId, "test name", debugState);
            var list = instance.FetchDebugItems(resourceId, "test name");

            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(resourceId, list[0].ID);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(TestDebugMessageRepo))]
        public void TestDebugMessageRepo_AddDebugItem_AddToList()
        {
            var resourceId = Guid.NewGuid();
            var resourceIdOther = Guid.NewGuid();
            var debugState = new DebugState { ID = resourceId, Name = "name" };
            var debugStateOther = new DebugState { ID = resourceIdOther, Name = "otherName" };

            var instance = TestDebugMessageRepo.Instance;

            instance.AddDebugItem(resourceId, "test name", debugState);
            instance.AddDebugItem(resourceId, "test name", debugStateOther);
            var list = instance.FetchDebugItems(resourceId, "test name");

            Assert.AreEqual(2, list.Count);
            Assert.AreEqual(resourceId, list[0].ID);
            Assert.AreEqual("name", list[0].Name);
            Assert.AreEqual(resourceIdOther, list[1].ID);
            Assert.AreEqual("otherName", list[1].Name);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(TestDebugMessageRepo))]
        public void TestDebugMessageRepo_GetDebugItems()
        {
            var resourceId = Guid.NewGuid();
            var resourceIdOther = Guid.NewGuid();
            var debugState = new DebugState { ID = resourceId, Name = "name" };
            var debugStateOther = new DebugState { ID = resourceIdOther, Name = "otherName" };

            var instance = TestDebugMessageRepo.Instance;

            instance.AddDebugItem(resourceId, "test name", debugState);
            instance.AddDebugItem(resourceId, "test name", debugStateOther);
            var list = instance.GetDebugItems(resourceId, "test name");

            Assert.AreEqual(2, list.Count);
            Assert.AreEqual(resourceId, list[0].ID);
            Assert.AreEqual("name", list[0].Name);
            Assert.AreEqual(resourceIdOther, list[1].ID);
            Assert.AreEqual("otherName", list[1].Name);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(TestDebugMessageRepo))]
        public void TestDebugMessageRepo_GetDebugItems_NoItems()
        {
            var resourceId = Guid.NewGuid();
            var resourceIdOther = Guid.NewGuid();
            var debugState = new DebugState { ID = resourceId, Name = "name" };
            var debugStateOther = new DebugState { ID = resourceIdOther, Name = "otherName" };

            var instance = TestDebugMessageRepo.Instance;

            instance.AddDebugItem(resourceId, "test name", debugState);
            instance.AddDebugItem(resourceId, "test name", debugStateOther);
            var list = instance.GetDebugItems(resourceId, "some other name");

            Assert.IsNull(list);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(TestDebugMessageRepo))]
        public void TestDebugMessageRepo_AddDebugItem_Contains_Id()
        {
            var resourceId = Guid.NewGuid();

            var debugState = new DebugState { ID = resourceId };

            var instance = TestDebugMessageRepo.Instance;

            instance.AddDebugItem(resourceId, "test name", debugState);
            instance.AddDebugItem(resourceId, "test name", debugState);
            var list = instance.FetchDebugItems(resourceId, "test name");

            Assert.IsTrue(list.Count >= 1);
            Assert.AreEqual(resourceId, list[0].ID);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(TestDebugMessageRepo))]
        public void TestDebugMessageRepo_FetchDebugItems()
        {
            var resourceId = Guid.NewGuid();

            var instance = TestDebugMessageRepo.Instance;
            var list = instance.FetchDebugItems(resourceId, "test name");

            Assert.IsNull(list);
        }
    }
}
