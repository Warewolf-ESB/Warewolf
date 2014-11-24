
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Studio.Core.Activities.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Designers.Tests.Move
{
    [TestClass]
    // ReSharper disable InconsistentNaming
    public class MoveDesignerViewModelTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("MoveDesignerViewModel_Constructor")]
        public void MoveDesignerViewModel_Constructor_Properties_Initialized()
        {
            //------------Setup for test-------------------------

            //------------Execute Test---------------------------
            var viewModel = MoveViewModel();

            //------------Assert Results-------------------------
            Assert.AreEqual("File or Folder", viewModel.InputPathLabel);
            Assert.AreEqual("Destination", viewModel.OutputPathLabel);
            Assert.IsNull(viewModel.InputPathValue);
            Assert.IsNull(viewModel.OutputPathValue);
            Assert.IsNull(viewModel.Errors);
            Assert.AreEqual(1, viewModel.TitleBarToggles.Count);
            StringAssert.Contains(viewModel.TitleBarToggles[0].ExpandToolTip, "Help");
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("MoveDesignerViewModel_Validate")]
        public void MoveDesignerViewModel_Validate_CorrectFieldsAreValidated()
        {
            //------------Setup for test-------------------------
            var viewModel = MoveViewModel();

            //------------Execute Test---------------------------
            viewModel.Validate();

            //------------Assert Results-------------------------
            Assert.AreEqual(1, viewModel.ValidateInputAndOutputPathHitCount);
            Assert.AreEqual(1, viewModel.ValidateInputPathHitCount);
            Assert.AreEqual(1, viewModel.ValidateOutputPathHitCount);
            Assert.AreEqual(1, viewModel.ValidateUserNameAndPasswordHitCount);
            Assert.AreEqual(1, viewModel.ValidateDestinationUsernameAndPasswordHitCount);
        }

        static TestMoveDesignerViewModel MoveViewModel()
        {
            var viewModel = new TestMoveDesignerViewModel(ModelItemUtils.CreateModelItem(new DsfPathMove()));
            return viewModel;
        }
    }
}
