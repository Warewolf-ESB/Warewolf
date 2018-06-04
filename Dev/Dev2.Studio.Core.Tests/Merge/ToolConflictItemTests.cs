using Dev2.Activities.Designers2.MultiAssign;
using Dev2.Core.Tests.Merge.Utils;
using Dev2.ViewModels.Merge;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Windows;
using System.Windows.Media;

namespace Dev2.Core.Tests.Merge
{
    [TestClass]
    public class ToolConflictItemTests : MergeTestUtils
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        public void ToolConflictItem_NewFromActivity()
        {
            //------------Setup for test--------------------------
            var toolConflictItem = CreateToolConflictItem();
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.IsNotNull(toolConflictItem);
            Assert.AreEqual(_multiAssign.UniqueID, toolConflictItem.UniqueId.ToString());
            Assert.AreEqual(_multiAssign.DisplayName, toolConflictItem.MergeDescription);
            Assert.IsNotNull(toolConflictItem.FlowNode);
            Assert.AreEqual(_modelItem, toolConflictItem.ModelItem);
            Assert.AreEqual(_location, toolConflictItem.NodeLocation);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        public void ToolConflictItem_SetUserInterface()
        {
            //------------Setup for test--------------------------
            var toolConflictItem = CreateToolConflictItem();

            var imageSource = new DrawingImage();
            var instance = new MultiAssignDesignerViewModel(_modelItem);
            toolConflictItem.MergeIcon = imageSource;
            toolConflictItem.ActivityDesignerViewModel = instance;
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.AreEqual(imageSource, toolConflictItem.MergeIcon);
            Assert.AreEqual(instance, toolConflictItem.ActivityDesignerViewModel);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        public void ToolConflictItem_NewStartConflictItem()
        {
            //------------Setup for test--------------------------
            var imageSource = new DrawingImage();
            var startConflictItem = NewStartConflictItem(imageSource);
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.IsNotNull(startConflictItem);
            Assert.AreEqual("Start", startConflictItem.MergeDescription);
            Assert.AreEqual(imageSource, startConflictItem.MergeIcon);
            Assert.AreEqual(Guid.Empty, startConflictItem.UniqueId);
            Assert.IsNull(startConflictItem.ModelItem);
            Assert.IsNull(startConflictItem.FlowNode);
            Assert.AreEqual(default(Point), startConflictItem.NodeLocation);
            Assert.IsNull(startConflictItem.ActivityDesignerViewModel);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        public void ToolConflictItem_ShowCheckbox_ExpectedTrue()
        {
            //------------Setup for test--------------------------
            var toolConflictItem = CreateToolConflictItem();
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.IsTrue(toolConflictItem.ShowCheckbox);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        public void ToolConflictItem_ShowCheckbox_ExpectedFalse()
        {
            //------------Setup for test--------------------------
            var toolConflictItem = ToolConflictItem.EmptyConflictItem();
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.IsFalse(toolConflictItem.ShowCheckbox);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        public void ToolConflictItem_IsChecked_PropertyChanged()
        {
            //------------Setup for test--------------------------
            var toolConflictItem = CreateToolConflictItem();

            //------------Execute Test---------------------------
            var wasCalled = false;
            toolConflictItem.PropertyChanged += (a, b) =>
            {
                if (b.PropertyName == "IsChecked")
                {
                    wasCalled = true;
                }
            };

            //------------Assert Results-------------------------
            Assert.IsFalse(toolConflictItem.IsChecked);
            toolConflictItem.IsChecked = true;
            Assert.IsTrue(wasCalled);
        }
    }
}
