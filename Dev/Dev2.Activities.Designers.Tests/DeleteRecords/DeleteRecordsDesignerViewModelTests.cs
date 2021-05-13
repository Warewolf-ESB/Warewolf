/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Activities.Presentation.Model;
using Dev2.Activities.Designers2.DeleteRecords;
using Dev2.Common.Interfaces.Help;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Designers.Tests.DeleteRecords
{
    [TestClass]
    public class DeleteRecordsDesignerViewModelTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DeleteRecordsDesignerViewModel))]
        public void DeleteRecordsDesignerViewModel_ShouldCall_UpdateHelpDescriptor()
        {
            //------------Setup for test--------------------------
            var mockMainViewModel = new Mock<IShellViewModel>();
            var mockHelpViewModel = new Mock<IHelpWindowViewModel>();
            mockMainViewModel.Setup(model => model.HelpViewModel).Returns(mockHelpViewModel.Object);
            using (var viewModel = new DeleteRecordsDesignerViewModel(CreateModelItem(), mockMainViewModel.Object))
            {
                //------------Execute Test---------------------------
                viewModel.UpdateHelpDescriptor("help");
                //------------Assert Results-------------------------
                mockHelpViewModel.Verify(model => model.UpdateHelpText("help"), Times.Once());
                Assert.AreEqual(Warewolf.Studio.Resources.Languages.HelpText.Tool_Recordset_Delete, viewModel.HelpText);
                Assert.IsTrue(viewModel.HasLargeView);
            }
        }

        static ModelItem CreateModelItem()
        {
            return ModelItemUtils.CreateModelItem(new DsfDeleteRecordNullHandlerActivity());
        }
    }
}
