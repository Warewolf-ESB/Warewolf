using Dev2.Activities.Designers2.MultiAssign;
using Dev2.Common.Interfaces;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Interfaces;
using Dev2.ViewModels.Merge;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Activities.Presentation.Model;
using System.Activities.Statements;
using System.Windows;
using System.Windows.Media;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Core.Tests.Merge
{
    [TestClass]
    public class ToolConflictItemTests
    {
        Guid uniqueId = Guid.NewGuid();
        const string description = "MultiAssign";
        Point location = new Point(10, 10);
        ModelItem modelItem;

        private ToolConflictItem CreateToolConflictItem()
        {
            var multiAssign = new Mock<DsfMultiAssignActivity>();
            multiAssign.Setup(ma => ma.UniqueID).Returns(uniqueId.ToString());
            multiAssign.Setup(ma => ma.DisplayName).Returns(description);

            modelItem = ModelItemUtils.CreateModelItem(multiAssign);

            var toolConflictItem = ToolConflictItem.NewFromActivity(multiAssign.Object, modelItem, location);
            return toolConflictItem;
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        public void ToolConflictItem_NewFromActivity()
        {
            //------------Setup for test--------------------------
            var toolConflictItem = CreateToolConflictItem();
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.IsNotNull(toolConflictItem);
            Assert.AreEqual(uniqueId, toolConflictItem.UniqueId);
            Assert.AreEqual(description, toolConflictItem.MergeDescription);
            Assert.AreEqual(null, toolConflictItem.FlowNode);
            Assert.AreEqual(modelItem, toolConflictItem.ModelItem);
            Assert.AreEqual(location, toolConflictItem.NodeLocation);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        public void ToolConflictItem_SetUserInterface()
        {
            //------------Setup for test--------------------------
            var toolConflictItem = CreateToolConflictItem();

            var imageSource = Application.Current.TryFindResource("System-StartNode") as ImageSource;
            var instance = new MultiAssignDesignerViewModel(modelItem);

            toolConflictItem.SetUserInterface(imageSource, instance);
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.AreEqual(imageSource, toolConflictItem.MergeIcon);
            Assert.AreEqual(instance, toolConflictItem.ActivityDesignerViewModel);
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
