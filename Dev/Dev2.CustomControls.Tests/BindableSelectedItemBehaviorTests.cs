using System.Windows;
using Dev2.CustomControls.Behavior;
using Dev2.Studio.Core.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Controls;
using Moq;

namespace Dev2.CustomControls.Tests
{
    [TestClass]
    // ReSharper disable InconsistentNaming
    public class BindableSelectedItemBehaviorTests
    {
        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory(" BindableSelectedItemBehavior_Attach")]
        public void BindableSelectedItemBehavior_Attach_ToTreeView_AssociatedObjectIsSetToTreeView()
        {
            //------------Setup for test--------------------------
            var behavior = new SelectedBehavior();
            var treeView = new TreeView();
            //------------Execute Test---------------------------
            behavior.Attach(treeView);
            //------------Assert Results-------------------------
            Assert.AreEqual(treeView, behavior.GetAssociatedObject());
        }
    }


    public class SelectedBehavior : BindableSelectedItemBehavior
    {
        public TreeView GetAssociatedObject()
        {
            return AssociatedObject;
        }
    }
}
