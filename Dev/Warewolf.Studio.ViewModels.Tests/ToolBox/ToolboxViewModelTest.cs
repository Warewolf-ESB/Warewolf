using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Dev2.Common.Interfaces.Help;
using Dev2.Common.Interfaces.Toolbox;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Core;
using Warewolf.Studio.ViewModels.ToolBox;
using Warewolf.UnittestingUtils;

namespace Warewolf.Studio.ViewModels.Tests.ToolBox
{
    [TestClass]
    public class ToolboxViewModelTest
    {
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ToolBoxViewModel_Ctor")]
        // ReSharper disable InconsistentNaming
        public void ToolBoxViewModel_Ctor_NullParams_ExpectExceptions()

        {
            //------------Setup for test--------------------------
            var localTools = new Mock<IToolboxModel>();
            var remoteTools = new Mock<IToolboxModel>();
            NullArgumentConstructorHelper.AssertNullConstructor(new object[] { localTools.Object, remoteTools.Object},typeof(ToolboxViewModel) );
            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ToolBoxViewModel_Ctor")]
        public void ToolBoxViewModel_Ctor_ValidParams_ExpectExceptions()
        {
            //------------Setup for test--------------------------
            var localTools = new Mock<IToolboxModel>();
            // ReSharper disable CoVariantArrayConversion
            IList<IToolDescriptor> tools = new[] { new ToolDescriptor(Guid.NewGuid(), new WarewolfType("bob",new Version(),"dave" ), new WarewolfType("bob",new Version(),"dave" ), "bob", "", new Version(1, 2, 3), true, "cat", ToolType.Native, "") };
           
            var remoteTools = new Mock<IToolboxModel>();
            localTools.Setup(a => a.GetTools()).Returns(tools);
            remoteTools.Setup(a => a.GetTools()).Returns(tools);
            //------------Execute Test---------------------------
            var vm = new ToolboxViewModel(localTools.Object, remoteTools.Object, new Mock<IEventAggregator>().Object);

            //------------Assert Results-------------------------
            localTools.Verify(a => a.GetTools());
            remoteTools.Verify(a => a.GetTools());
            Assert.AreEqual(1,vm.Tools.Count);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ToolBoxViewModel_IsEnabled")]
        public void ToolBoxViewModel_IsEnabled_CanBeSetWithVariable()
        {
            //------------Setup for test--------------------------
            var localTools = new Mock<IToolboxModel>();
            
            var tools = new[] { new ToolDescriptor(Guid.NewGuid(), new WarewolfType("bob",new Version(),"dave" ), new WarewolfType("bob",new Version(),"dave" ), "bob", "", new Version(1, 2, 3), true, "cat", ToolType.Native, "") };
            var remoteTools = new Mock<IToolboxModel>();
            remoteTools.Setup(a => a.IsEnabled()).Returns(true);
            localTools.Setup(a => a.IsEnabled()).Returns(true);
            localTools.Setup(a => a.GetTools()).Returns(tools);
            remoteTools.Setup(a => a.GetTools()).Returns(tools);
            //------------Execute Test---------------------------
            var vm = new ToolboxViewModel(localTools.Object, remoteTools.Object, new Mock<IEventAggregator>().Object) { IsDesignerFocused = true };
            //------------Assert Results-------------------------
            Assert.AreEqual(true,vm.IsEnabled);

            vm.IsDesignerFocused = false;
            Assert.AreEqual(false, vm.IsEnabled);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ToolBoxViewModel_IsEnabled")]
        public void ToolBoxViewModel_IsEnabled_NotEnabledIfNotConnectedLocal()
        {
            //------------Setup for test--------------------------
            var localTools = new Mock<IToolboxModel>();
            var tools = new[] { new ToolDescriptor(Guid.NewGuid(), new WarewolfType("bob",new Version(),"dave" ), new WarewolfType("bob",new Version(),"dave" ), "bob", "", new Version(1, 2, 3), true, "cat", ToolType.Native, "") };
            var remoteTools = new Mock<IToolboxModel>();
            remoteTools.Setup(a => a.IsEnabled()).Returns(true);
            localTools.Setup(a => a.IsEnabled()).Returns(false);
            localTools.Setup(a => a.GetTools()).Returns(tools);
            remoteTools.Setup(a => a.GetTools()).Returns(tools);
            //------------Execute Test---------------------------
            var vm = new ToolboxViewModel(localTools.Object, remoteTools.Object, new Mock<IEventAggregator>().Object) { IsDesignerFocused = true };
            //------------Assert Results-------------------------
            Assert.AreEqual(false, vm.IsEnabled);
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ToolBoxViewModel_IsEnabled")]
        public void ToolBoxViewModel_IsEnabled_NotEnabledIfNotConnectedRemote()
        {
            //------------Setup for test--------------------------
            var localTools = new Mock<IToolboxModel>();
            var tools = new[] { new ToolDescriptor(Guid.NewGuid(), new WarewolfType("bob",new Version(),"dave" ), new WarewolfType("bob",new Version(),"dave" ), "bob", "", new Version(1, 2, 3), true, "cat", ToolType.Native, "") };
            var remoteTools = new Mock<IToolboxModel>();
            remoteTools.Setup(a => a.IsEnabled()).Returns(false);
            localTools.Setup(a => a.IsEnabled()).Returns(true);
            localTools.Setup(a => a.GetTools()).Returns(tools);
            remoteTools.Setup(a => a.GetTools()).Returns(tools);
            //------------Execute Test---------------------------
            var vm = new ToolboxViewModel(localTools.Object, remoteTools.Object, new Mock<IEventAggregator>().Object) { IsDesignerFocused = true };
            //------------Assert Results-------------------------
            Assert.AreEqual(false, vm.IsEnabled);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ToolBoxViewModel_Filter")]
        public void ToolBoxViewModel_Filter_FiltersWithContains()
        {
            //------------Setup for test--------------------------
            var localTools = new Mock<IToolboxModel>();
            var tools = new[]
            {
                new ToolDescriptor(Guid.NewGuid(), new WarewolfType("bob",new Version(),"dave" ), new WarewolfType("bob",new Version(),"dave" ), "bob", "", new Version(1, 2, 3), true, "cat", ToolType.Native, ""),
                new ToolDescriptor(Guid.NewGuid(), new WarewolfType("bob",new Version(),"dave" ), new WarewolfType("bob",new Version(),"dave" ), "ded", "", new Version(1, 2, 3), true, "cat", ToolType.Native, "")
            };
            var remoteTools = new Mock<IToolboxModel>();
            remoteTools.Setup(a => a.IsEnabled()).Returns(false);
            localTools.Setup(a => a.IsEnabled()).Returns(true);
            localTools.Setup(a => a.GetTools()).Returns(tools);
            remoteTools.Setup(a => a.GetTools()).Returns(tools);
            //------------Execute Test---------------------------
            var vm = new ToolboxViewModel(localTools.Object, remoteTools.Object, new Mock<IEventAggregator>().Object) { IsDesignerFocused = true };
            Assert.AreEqual(2, vm.Tools.Count);
            vm.Filter("d");
            //------------Assert Results-------------------------
            Assert.AreEqual(1,vm.Tools.Count);
            Assert.AreEqual(vm.Tools.First().Tool,tools[1]);
        } 
        
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ToolBoxViewModel_Search")]
        public void ToolBoxViewModel_SetSearch_ShouldCallFiltersWithContains()
        {
            //------------Setup for test--------------------------
            var localTools = new Mock<IToolboxModel>();
            var tools = new[]
            {
                new ToolDescriptor(Guid.NewGuid(), new WarewolfType("bob",new Version(),"dave" ), new WarewolfType("bob",new Version(),"dave" ), "bob", "", new Version(1, 2, 3), true, "cat", ToolType.Native, ""),
                new ToolDescriptor(Guid.NewGuid(), new WarewolfType("bob",new Version(),"dave" ), new WarewolfType("bob",new Version(),"dave" ), "ded", "", new Version(1, 2, 3), true, "cat", ToolType.Native, "")
            };
            var remoteTools = new Mock<IToolboxModel>();
            remoteTools.Setup(a => a.IsEnabled()).Returns(false);
            localTools.Setup(a => a.IsEnabled()).Returns(true);
            localTools.Setup(a => a.GetTools()).Returns(tools);
            remoteTools.Setup(a => a.GetTools()).Returns(tools);
            //------------Execute Test---------------------------
            var vm = new ToolboxViewModel(localTools.Object, remoteTools.Object, new Mock<IEventAggregator>().Object) { IsDesignerFocused = true };
            Assert.AreEqual(2, vm.Tools.Count);
            vm.SearchTerm = "d";
            //------------Assert Results-------------------------
            Assert.AreEqual(1,vm.Tools.Count);
            Assert.AreEqual(vm.Tools.First().Tool,tools[1]);
        }
  
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ToolBoxViewModel_Search")]
        public void ToolBoxViewModel_SetSearchSame_ShouldNotCallFiltersWithContains()
        {
            //------------Setup for test--------------------------
            var localTools = new Mock<IToolboxModel>();
            var tools = new[]
            {
                new ToolDescriptor(Guid.NewGuid(), new WarewolfType("bob",new Version(),"dave" ), new WarewolfType("bob",new Version(),"dave" ), "bob", "", new Version(1, 2, 3), true, "cat", ToolType.Native, ""),
                new ToolDescriptor(Guid.NewGuid(), new WarewolfType("bob",new Version(),"dave" ), new WarewolfType("bob",new Version(),"dave" ), "ded", "", new Version(1, 2, 3), true, "cat", ToolType.Native, "")
            };
            var remoteTools = new Mock<IToolboxModel>();
            remoteTools.Setup(a => a.IsEnabled()).Returns(false);
            localTools.Setup(a => a.IsEnabled()).Returns(true);
            localTools.Setup(a => a.GetTools()).Returns(tools);
            remoteTools.Setup(a => a.GetTools()).Returns(tools);
            var vm = new ToolboxViewModel(localTools.Object, remoteTools.Object, new Mock<IEventAggregator>().Object) { IsDesignerFocused = true };
            var hitCount = 0;
            vm.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "SearchTerm")
                {
                    hitCount++;
                }
            };
            //------------Assert Precondition--------------------
            Assert.AreEqual(2, vm.Tools.Count);
            vm.SearchTerm = "d";           
            //------------Execute Test---------------------------
            vm.SearchTerm = "d";
            //------------Assert Results-------------------------
            Assert.AreEqual(1, vm.Tools.Count);
            Assert.AreEqual(vm.Tools.First().Tool, tools[1]);
            Assert.AreEqual(1,hitCount);
           
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ToolBoxViewModel_Filter")]
        public void ToolBoxViewModel_Filter_Unmatched_ReturnsEmpty()
        {
            //------------Setup for test--------------------------
            var localTools = new Mock<IToolboxModel>();
            var tools = new[]
            {
                new ToolDescriptor(Guid.NewGuid(), new WarewolfType("bob",new Version(),"dave" ), new WarewolfType("bob",new Version(),"dave" ), "bob", "", new Version(1, 2, 3), true, "cat", ToolType.Native, ""),
                new ToolDescriptor(Guid.NewGuid(), new WarewolfType("bob",new Version(),"dave" ), new WarewolfType("bob",new Version(),"dave" ), "ded", "", new Version(1, 2, 3), true, "cat", ToolType.Native, "")
            };
            var remoteTools = new Mock<IToolboxModel>();
            remoteTools.Setup(a => a.IsEnabled()).Returns(false);
            localTools.Setup(a => a.IsEnabled()).Returns(true);
            localTools.Setup(a => a.GetTools()).Returns(tools);
            remoteTools.Setup(a => a.GetTools()).Returns(tools);
            //------------Execute Test---------------------------
            var vm = new ToolboxViewModel(localTools.Object, remoteTools.Object, new Mock<IEventAggregator>().Object) { IsDesignerFocused = true };
            Assert.AreEqual(2, vm.Tools.Count);
            Assert.AreEqual(1, vm.CategorisedTools.Count);
            vm.Filter("dz");
            //------------Assert Results-------------------------
            Assert.AreEqual(0, vm.Tools.Count);
            Assert.AreEqual(0, vm.CategorisedTools.Count);
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ToolBoxViewModel_Categories")]
        public void ToolBoxViewModel_Categories_ReturnsTwo()
        {
            //------------Setup for test--------------------------
            var localTools = new Mock<IToolboxModel>();
            var tools = new[]
            {
                new ToolDescriptor(Guid.NewGuid(), new WarewolfType("bob",new Version(),"dave" ), new WarewolfType("bob",new Version(),"dave" ), "bob", "", new Version(1, 2, 3), true, "cat", ToolType.Native, ""),
                new ToolDescriptor(Guid.NewGuid(), new WarewolfType("bob",new Version(),"dave" ), new WarewolfType("bob",new Version(),"dave" ), "ded", "", new Version(1, 2, 3), true, "cat2", ToolType.Native, "")
            };
            var remoteTools = new Mock<IToolboxModel>();
            remoteTools.Setup(a => a.IsEnabled()).Returns(false);
            localTools.Setup(a => a.IsEnabled()).Returns(true);
            localTools.Setup(a => a.GetTools()).Returns(tools);
            remoteTools.Setup(a => a.GetTools()).Returns(tools);
            //------------Execute Test---------------------------
            var vm = new ToolboxViewModel(localTools.Object, remoteTools.Object, new Mock<IEventAggregator>().Object) { IsDesignerFocused = true };
            Assert.AreEqual(2, vm.Tools.Count);
            Assert.AreEqual(2, vm.CategorisedTools.Count);

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ToolBoxViewModel_Filter")]
        public void ToolBoxViewModel_Clear_Clears_Filter()
        {
            //------------Setup for test--------------------------
            var localTools = new Mock<IToolboxModel>();
            var tools = new[]
            {
                new ToolDescriptor(Guid.NewGuid(), new WarewolfType("bob",new Version(),"dave" ), new WarewolfType("bob",new Version(),"dave" ), "bob", "", new Version(1, 2, 3), true, "cat", ToolType.Native, ""),
                new ToolDescriptor(Guid.NewGuid(), new WarewolfType("bob",new Version(),"dave" ), new WarewolfType("bob",new Version(),"dave" ), "ded", "", new Version(1, 2, 3), true, "cat", ToolType.Native, "")
            };
            var remoteTools = new Mock<IToolboxModel>();
            remoteTools.Setup(a => a.IsEnabled()).Returns(false);
            localTools.Setup(a => a.IsEnabled()).Returns(true);
            localTools.Setup(a => a.GetTools()).Returns(tools);
            remoteTools.Setup(a => a.GetTools()).Returns(tools);
            //------------Execute Test---------------------------
            var vm = new ToolboxViewModel(localTools.Object, remoteTools.Object, new Mock<IEventAggregator>().Object) { IsDesignerFocused = true };
            Assert.AreEqual(2, vm.Tools.Count);
            vm.Filter("dz");
            //------------Assert Results-------------------------
            Assert.AreEqual(0, vm.Tools.Count);

            vm.ClearFilter();
            Assert.AreEqual(2, vm.Tools.Count);

        }

        // ReSharper restore InconsistentNaming
        // ReSharper restore CoVariantArrayConversion
    }
}
