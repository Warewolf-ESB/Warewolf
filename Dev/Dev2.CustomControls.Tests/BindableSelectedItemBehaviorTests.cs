
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Windows.Controls;
using Dev2.CustomControls.Behavior;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
