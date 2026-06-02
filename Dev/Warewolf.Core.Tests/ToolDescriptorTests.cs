/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.Common.Interfaces.Toolbox;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Warewolf.Core.Tests
{
    [TestClass]
    public class ToolDescriptorTests
    {
        static IWarewolfType MockWarewolfType()
        {
            var mock = new Mock<IWarewolfType>();
            mock.Setup(t => t.FullyQualifiedName).Returns("Some.Type");
            mock.Setup(t => t.Version).Returns(new Version(1, 0));
            mock.Setup(t => t.ContainingAssemblyPath).Returns("path.dll");
            return mock.Object;
        }

        static ToolDescriptor NewDescriptor(Guid? id = null, Version version = null, string name = "MyTool")
        {
            return new ToolDescriptor(
                id ?? Guid.NewGuid(),
                MockWarewolfType(),
                MockWarewolfType(),
                name,
                "icon.png",
                version ?? new Version(1, 0, 0, 0),
                true,
                "Workflow",
                ToolType.Native,
                "icon-uri",
                "filter-tag",
                "tooltip-text",
                "help-text");
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(ToolDescriptor))]
        public void ToolDescriptor_Construct_AssignsAllProperties()
        {
            var id = Guid.NewGuid();
            var designer = MockWarewolfType();
            var activity = MockWarewolfType();
            var version = new Version(2, 3, 4, 5);

            var sut = new ToolDescriptor(id, designer, activity, "MyTool", "icon.png",
                version, false, "Workflow", ToolType.User, "icon-uri", "filter-tag",
                "tooltip", "help");

            Assert.AreEqual(id, sut.Id);
            Assert.AreSame(designer, sut.Designer);
            Assert.AreSame(activity, sut.Activity);
            Assert.AreEqual("MyTool", sut.Name);
            Assert.AreEqual("icon.png", sut.Icon);
            Assert.AreEqual(version, sut.Version);
            Assert.IsFalse(sut.IsSupported);
            Assert.AreEqual("Workflow", sut.Category);
            Assert.AreEqual(ToolType.User, sut.ToolType);
            Assert.AreEqual("icon-uri", sut.IconUri);
            Assert.AreEqual("filter-tag", sut.FilterTag);
            Assert.AreEqual("tooltip", sut.ResourceToolTip);
            Assert.AreEqual("help", sut.ResourceHelpText);
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(ToolDescriptor))]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ToolDescriptor_Construct_EmptyGuid_Throws()
        {
            NewDescriptor(id: Guid.Empty);
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(ToolDescriptor))]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ToolDescriptor_Construct_NullDesigner_Throws()
        {
            new ToolDescriptor(Guid.NewGuid(), null, MockWarewolfType(), "n", "i",
                new Version(1, 0), true, "c", ToolType.Native, "u", "f", "t", "h");
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(ToolDescriptor))]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ToolDescriptor_Construct_NullActivity_Throws()
        {
            new ToolDescriptor(Guid.NewGuid(), MockWarewolfType(), null, "n", "i",
                new Version(1, 0), true, "c", ToolType.Native, "u", "f", "t", "h");
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(ToolDescriptor))]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ToolDescriptor_Construct_NullName_Throws()
        {
            new ToolDescriptor(Guid.NewGuid(), MockWarewolfType(), MockWarewolfType(), null, "i",
                new Version(1, 0), true, "c", ToolType.Native, "u", "f", "t", "h");
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(ToolDescriptor))]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ToolDescriptor_Construct_NullIcon_Throws()
        {
            new ToolDescriptor(Guid.NewGuid(), MockWarewolfType(), MockWarewolfType(), "n", null,
                new Version(1, 0), true, "c", ToolType.Native, "u", "f", "t", "h");
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(ToolDescriptor))]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ToolDescriptor_Construct_NullVersion_Throws()
        {
            new ToolDescriptor(Guid.NewGuid(), MockWarewolfType(), MockWarewolfType(), "n", "i",
                null, true, "c", ToolType.Native, "u", "f", "t", "h");
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(ToolDescriptor))]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ToolDescriptor_Construct_NullCategory_Throws()
        {
            new ToolDescriptor(Guid.NewGuid(), MockWarewolfType(), MockWarewolfType(), "n", "i",
                new Version(1, 0), true, null, ToolType.Native, "u", "f", "t", "h");
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(ToolDescriptor))]
        public void ToolDescriptor_Properties_SettersWork()
        {
            var sut = NewDescriptor();

            sut.FilterTag = "new-filter";
            sut.ResourceToolTip = "new-tooltip";
            sut.ResourceHelpText = "new-help";

            Assert.AreEqual("new-filter", sut.FilterTag);
            Assert.AreEqual("new-tooltip", sut.ResourceToolTip);
            Assert.AreEqual("new-help", sut.ResourceHelpText);
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(ToolDescriptor))]
        public void ToolDescriptor_Equals_SameIdAndVersion_True()
        {
            var id = Guid.NewGuid();
            var v = new Version(1, 2, 3, 4);
            var a = NewDescriptor(id, v, "A");
            var b = NewDescriptor(id, v, "B"); // different name, equality keys are id+version

            Assert.IsTrue(a.Equals(b));
            Assert.IsTrue(a.Equals((object)b));
            Assert.IsTrue(a == b);
            Assert.IsFalse(a != b);
            Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(ToolDescriptor))]
        public void ToolDescriptor_Equals_DifferentId_False()
        {
            var v = new Version(1, 0);
            var a = NewDescriptor(Guid.NewGuid(), v);
            var b = NewDescriptor(Guid.NewGuid(), v);

            Assert.IsFalse(a.Equals(b));
            Assert.IsFalse(a == b);
            Assert.IsTrue(a != b);
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(ToolDescriptor))]
        public void ToolDescriptor_Equals_DifferentVersion_False()
        {
            var id = Guid.NewGuid();
            var a = NewDescriptor(id, new Version(1, 0));
            var b = NewDescriptor(id, new Version(2, 0));

            Assert.IsFalse(a.Equals(b));
            Assert.AreNotEqual(a.GetHashCode(), b.GetHashCode());
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(ToolDescriptor))]
        public void ToolDescriptor_Equals_Null_False()
        {
            var sut = NewDescriptor();

            Assert.IsFalse(sut.Equals((ToolDescriptor)null));
            Assert.IsFalse(sut.Equals((object)null));
            Assert.IsFalse(sut == null);
            Assert.IsFalse(null == sut);
            Assert.IsTrue(sut != null);
            Assert.IsTrue(null != sut);
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(ToolDescriptor))]
        public void ToolDescriptor_Equals_BothNull_True()
        {
            ToolDescriptor a = null;
            ToolDescriptor b = null;

            Assert.IsTrue(a == b);
            Assert.IsFalse(a != b);
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(ToolDescriptor))]
        public void ToolDescriptor_Equals_Self_True()
        {
            var sut = NewDescriptor();

            Assert.IsTrue(sut.Equals(sut));
            Assert.IsTrue(sut.Equals((object)sut));
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(ToolDescriptor))]
        public void ToolDescriptor_Equals_DifferentType_False()
        {
            var sut = NewDescriptor();

            Assert.IsFalse(sut.Equals("not a descriptor"));
            Assert.IsFalse(sut.Equals(new object()));
        }

        [TestMethod]
        [Owner("Warewolf Tester")]
        [TestCategory(nameof(ToolDescriptor))]
        public void ToolDescriptor_GetHashCode_Stable()
        {
            var sut = NewDescriptor();

            var first = sut.GetHashCode();
            var second = sut.GetHashCode();

            Assert.AreEqual(first, second);
        }
    }
}
