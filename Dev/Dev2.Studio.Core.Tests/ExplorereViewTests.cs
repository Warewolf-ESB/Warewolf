
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using Dev2.AppResources.Repositories;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.ConnectionHelpers;
using Dev2.Models;
using Dev2.Studio.Views.Explorer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests
{
    [TestClass]
    public class ExplorereViewTests
    {
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ExplorerView_Move")]
        // ReSharper disable once InconsistentNaming
        public void ExplorerView_Move_ExpectMove()
        {
            //------------Setup for test--------------------------
            var stud = new Mock<IStudioResourceRepository>();
            var model1 = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object,new Mock<IStudioResourceRepository>().Object) { ResourceType = ResourceType.DbService, ResourcePath = "bob" };
            var model2 = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object) { ResourceType = ResourceType.DbService, ResourcePath = "dave", Parent = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object) { ResourcePath = "moo" } };
            stud.Setup(a=>a.MoveItem(model1,"bob"));
            ExplorerView.MoveItem(model1, model2, stud.Object);
            
            //------------Execute Test---------------------------
            stud.Verify(a=>a.MoveItem(model1,"moo"));
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ExplorerView_Move")]
        // ReSharper disable once InconsistentNaming
        public void ExplorerView_Move_FolderExpectRename()
        {
            //------------Setup for test--------------------------
            var stud = new Mock<IStudioResourceRepository>();
            var model1 = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object) { ResourceType = ResourceType.Folder, ResourcePath = "bob" };
            var model2 = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object) { ResourceType = ResourceType.Folder, ResourcePath = "dave", Parent = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object) { ResourcePath = "moo" } };
            stud.Setup(a => a.MoveItem(model1, "bob"));
            ExplorerView.MoveItem(model1, model2, stud.Object);

            //------------Execute Test---------------------------
            stud.Verify(a => a.MoveItem(model1, "dave"));
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ExplorerView_Move")]
        // ReSharper disable once InconsistentNaming
        public void ExplorerView_Move_FolderExpectPopup()
        {
            //------------Setup for test--------------------------
            var stud = new Mock<IStudioResourceRepository>();
            var popup = new Mock<IPopupController>();
            CustomContainer.Register(popup.Object);
            var model1 = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object) { ResourceType = ResourceType.Folder, ResourcePath = "bob",EnvironmentId = Guid.NewGuid()};
            var model2 = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object) { ResourceType = ResourceType.Folder, ResourcePath = "dave", Parent = new ExplorerItemModel(new Mock<IConnectControlSingleton>().Object, new Mock<IStudioResourceRepository>().Object) { ResourcePath = "moo" } };
            stud.Setup(a => a.MoveItem(model1, "bob"));
            ExplorerView.MoveItem(model1, model2, stud.Object);

            //------------Execute Test---------------------------
            popup.Verify(a=>a.Show());
            popup.VerifySet(a => a.Description = "You are not allowed to move items between Servers using the explorer. Please use the deploy instead");
            //------------Assert Results-------------------------
        }

    }
}
