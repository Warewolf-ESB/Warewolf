/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Activities.Presentation.Model;
using Dev2.Common.Interfaces.Help;
using Dev2.Interfaces;
using Dev2.Studio.Core.Activities.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Designers.Tests.DeleteRecords
{
    [TestClass]
    // ReSharper disable InconsistentNaming
    public class DeleteRecordsDesignerViewModelTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("DeleteRecordsDesignerViewModel_SetRecordsetNameValue")]
        public void DeleteRecordsDesignerViewModel_SetRecordsetNameValue_ModelItemIsValid_RecordSetOnModelItemIsSet()
        {
            var modelItem = CreateModelItem();
            var viewModel = new TestDeleteRecordsDesignerViewModel(modelItem);
            const string ExcpectedVal = "[[Table_Records()]]";
            viewModel.RecordsetName = ExcpectedVal;
            viewModel.Validate();
            Assert.AreEqual(ExcpectedVal, viewModel.RecordsetName);
            Assert.IsTrue(viewModel.HasLargeView);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DeleteRecordsDesignerViewModel_Handle")]
        public void DeleteRecordsDesignerViewModel_UpdateHelp_ShouldCallToHelpViewMode()
        {
            //------------Setup for test--------------------------      
            var mockMainViewModel = new Mock<IMainViewModel>();
            var mockHelpViewModel = new Mock<IHelpWindowViewModel>();
            mockHelpViewModel.Setup(model => model.UpdateHelpText(It.IsAny<string>())).Verifiable();
            mockMainViewModel.Setup(model => model.HelpViewModel).Returns(mockHelpViewModel.Object);
            CustomContainer.Register(mockMainViewModel.Object);
            var viewModel = new TestDeleteRecordsDesignerViewModel(CreateModelItem());
            //------------Execute Test---------------------------
            viewModel.UpdateHelpDescriptor("help");
            //------------Assert Results-------------------------
            mockHelpViewModel.Verify(model => model.UpdateHelpText(It.IsAny<string>()), Times.Once());
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Constructor_GivenIsNew_ShouldHaveTreatAsNullTrue()
        {
            //---------------Set up test pack-------------------
            var modelItem = CreateModelItem();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(modelItem);
            //---------------Execute Test ----------------------
            var modelProperty = modelItem.Properties["TreatNullAsZero"];
            var value = modelProperty?.Value;
            if (value != null)
            {
                var currentValue = value.GetCurrentValue();
                //---------------Test Result -----------------------
                Assert.IsTrue(bool.Parse(currentValue.ToString()));
            }

        }

        static ModelItem CreateModelItem()
        {
            return ModelItemUtils.CreateModelItem(new DsfDeleteRecordNullHandlerActivity());
        }
    }
}
