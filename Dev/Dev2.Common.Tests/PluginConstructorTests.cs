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
using Dev2.Common.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Common.Tests
{
    [TestClass]
    public class PluginConstructorTests
    {
        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(PluginConstructor))]
        public void PluginConstructor_Construct()
        {
            var p = GetDefaultInstance();

            Assert.AreEqual(null, p.ConstructorName);
            Assert.AreEqual(Guid.Empty, p.ID);
            Assert.AreEqual(0, p.Inputs.Count);
            Assert.AreEqual(false, p.IsExistingObject);
            Assert.AreEqual(null, p.ReturnObject);

            Assert.AreEqual(null, p.GetIdentifier());
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(PluginConstructor))]
        public void PluginConstructor_ConstructWithValues()
        {
            var id = Guid.NewGuid();
            var p = GetInstance(id);

            Assert.AreEqual("constr Name", p.ConstructorName);
            Assert.AreEqual("constr Name", p.ToString());
            Assert.AreEqual(id, p.ID);
            Assert.AreEqual(1, p.Inputs.Count);
            Assert.AreEqual(true, p.IsExistingObject);
            Assert.AreEqual("return ob", p.ReturnObject);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(PluginConstructor))]
        public void PluginConstructor_Equals_IsFalse()
        {
            var id = Guid.NewGuid();

            var ob1 = GetDefaultInstance();
            var ob2 = GetInstance(id);

            Assert.IsFalse(ob1.Equals(ob2));
            Assert.IsFalse(ob1.Equals((object)ob2));
            Assert.IsFalse(ob1 == ob2);
            Assert.IsTrue(ob1 != ob2);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(PluginConstructor))]
        public void PluginConstructor_Equals_IsTrue()
        {
            var id = Guid.NewGuid();

            var ob1 = GetInstance(id);
            var ob2 = GetInstance(id);

            Assert.IsTrue(ob1.Equals(ob2));
            Assert.IsTrue(ob1.Equals((object)ob2));
            Assert.IsTrue(ob1 == ob2);
            Assert.IsFalse(ob1 != ob2);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(PluginConstructor))]
        public void PluginConstructor_SameEquals_IsTrue()
        {
            var id = Guid.NewGuid();

            var ob1 = GetInstance(id);

            Assert.IsTrue(ob1.Equals(ob1));
            Assert.IsTrue(ob1.Equals((object)ob1));
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(PluginConstructor))]
        public void PluginConstructor_Equals_Null_IsFalse()
        {
            var id = Guid.NewGuid();

            var ob1 = GetInstance(id);

            Assert.IsFalse(ob1.Equals(null));
            Assert.IsFalse(ob1.Equals((object)null));
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(PluginConstructor))]
        public void PluginConstructor_Equals_DifferentType_IsFalse()
        {
            var id = Guid.NewGuid();

            var ob1 = GetInstance(id);

            Assert.IsFalse(ob1.Equals((object)1234));
        }

        private static PluginConstructor GetDefaultInstance() => new PluginConstructor();
        private static PluginConstructor GetInstance(Guid id)
        {
            var p = new PluginConstructor
            {
                ConstructorName = "constr Name",
                ID = id,
                IsExistingObject = true,
                ReturnObject = "return ob",
            };
            p.Inputs.Add(new Mock<IConstructorParameter>().Object);
            return p;
        }
    }
}
