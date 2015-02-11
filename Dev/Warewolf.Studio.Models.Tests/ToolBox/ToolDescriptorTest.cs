using System;
using System.Windows.Media;
using Dev2.Common.Interfaces.Help;
using Dev2.Common.Interfaces.Toolbox;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Core;
using Warewolf.UnittestingUtils;

namespace Warewolf.Studio.Models.Tests.ToolBox
{
    [TestClass]
    public class ToolDescriptorTest
    {
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ToolDescriptor_Ctor")]
        // ReSharper disable InconsistentNaming
        public void ToolDescriptor_Ctor_NullParams_ExpectExceptions()

        {

            NullArgumentConstructorHelper.AssertNullConstructor(new object[] { Guid.NewGuid(), new WarewolfType("bob", new Version(), "dave"), new WarewolfType("bob", new Version(), "dave"), "bob", "", new Version(1, 2, 3), true, "cat", ToolType.Native, "" }, typeof(ToolDescriptor));

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ToolDescriptor_Ctor")]
        public void ToolDescriptor_Ctor_ValidParams_ExpectPropertiesSet()
        {
            //------------Setup for test--------------------------
            var help = new Mock<IHelpDescriptor>().Object;
            ToolDescriptor t = new ToolDescriptor(Guid.NewGuid(), new WarewolfType("bob", new Version(), "dave"), new WarewolfType("bob", new Version(), "dave"), "bob", "icon", new Version(1, 2, 3), true, "cat", ToolType.Native, "");

            //------------Assert Results-------------------------
            //Assert.AreEqual(typeof(Guid),t.Activity);
            //Assert.AreEqual(typeof(Mock), t.Model);
            //Assert.AreEqual(typeof(String), t.Designer);
            Assert.AreNotEqual(t.Id,Guid.Empty);
            Assert.AreEqual("bob",t.Name);
            Assert.AreEqual("icon",t.Icon);
            Assert.AreEqual(t.Version,new Version(1,2,3));
            //Assert.AreEqual(t.Helpdescriptor,help);

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ToolDescriptor_Equality")]
        public void ToolDescriptor_Equality_CheckEqualityOptions()
        {
            //------------Setup for test--------------------------
            var help = new Mock<IHelpDescriptor>().Object;
            ToolDescriptor t = new ToolDescriptor(Guid.NewGuid(), new WarewolfType("bob", new Version(), "dave"), new WarewolfType("bob",new Version(),"dave" ), "bob", "", new Version(1, 2, 3), true, "cat", ToolType.Native, "");
            ToolDescriptor u = new ToolDescriptor(Guid.NewGuid(), new WarewolfType("bob", new Version(), "dave"), new WarewolfType("bob",new Version(),"dave" ), "bob", "", new Version(1, 2, 3), true, "cat", ToolType.Native, "");
            ToolDescriptor v = new ToolDescriptor(t.Id, new WarewolfType("bob", new Version(), "dave"), new WarewolfType("bob",new Version(),"dave" ), "bob", "", new Version(1, 2, 3), true, "cat", ToolType.Native, "");
            ToolDescriptor w = new ToolDescriptor(t.Id, new WarewolfType("bob",new Version(),"dave" ), new WarewolfType("bob",new Version(),"dave" ), "bob", "", new Version(1, 2, 4), true, "cat", ToolType.Native, "");

            //------------Assert Results-------------------------
            //basic
            Assert.AreNotEqual(t,null);
            Assert.IsFalse(t==null);
            Assert.IsTrue(t!=null);
            Assert.IsTrue(t.Equals(t));
            //different guid
            Assert.IsTrue(t!=u);
            Assert.IsFalse(t==u);
            //same guid and same version
            Assert.IsFalse(t != v);
            Assert.IsTrue(t == v);
            Assert.IsTrue(t.Equals(v));

            //same guid and diff version
            Assert.IsTrue(t != w);
            Assert.IsFalse(t == w);
            Assert.IsFalse(t.Equals(w));

            Assert.AreNotEqual(t.GetHashCode(),w.GetHashCode());
            Assert.AreNotEqual(0,t.GetHashCode());

        }
        // ReSharper restore InconsistentNaming
    }
}
