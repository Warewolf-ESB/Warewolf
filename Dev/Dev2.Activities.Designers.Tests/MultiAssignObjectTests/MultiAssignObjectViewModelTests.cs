/*
*  Warewolf - The Easy Service Bus
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Activities.Designers2.Core;
using Dev2.Activities.Designers2.MultiAssignObject;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.TO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Activities.Presentation.Model;
using Unlimited.Applications.BusinessDesignStudio.Activities;

// ReSharper disable InconsistentNaming
namespace Dev2.Activities.Designers.Tests.MultiAssignObjectTests
{
    [TestClass]
    public class MultiAssignObjectViewModelTests
    {
        [TestMethod]
        [Owner("Clint Stedman")]
        [TestCategory("DsfMultiAssignObjectActivityViewModel_Construct")]
        public void DsfMultiAssignObjectActivityViewModel_Construct_IsInstanceOfActivityViewModelBase_True()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var dsfMultiAssignObjectActivityViewModel = CreateDsfMultiAssignObjectActivityViewModel();
            //------------Assert Results-------------------------
            Assert.IsInstanceOfType(dsfMultiAssignObjectActivityViewModel, typeof(ActivityDesignerViewModel));
        }

        [TestMethod]
        [Owner("Clint Stedman")]
        [TestCategory("DsfMultiAssignObjectActivityViewModel_Construct")]
        public void DsfMultiAssignObjectActivityViewModel_Construct_IsInstanceOfActivityCollectionViewModelBaseOfActivityDTO_True()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var dsfMultiAssignObjectActivityViewModel = CreateDsfMultiAssignObjectActivityViewModel();
            //------------Assert Results-------------------------
            Assert.IsInstanceOfType(dsfMultiAssignObjectActivityViewModel, typeof(ActivityCollectionDesignerViewModel<AssignObjectDTO>));
        }

        [TestMethod]
        [Owner("Clint Stedman")]
        [TestCategory("MultiAssignObjectActivityViewModel_Constructor")]
        // ReSharper disable InconsistentNaming
        public void MultiAssignObjectActivityViewModel_Constructor_CollectionNameInitialized()
        // ReSharper restore InconsistentNaming
        {
            //init
            const string ExpectedCollectionName = "FieldsCollection";
            var mockModel = new Mock<ModelItem>();
            var propertyCollection = new Mock<ModelPropertyCollection>();

            mockModel.Setup(s => s.Properties).Returns(propertyCollection.Object);

            //exe
            var vm = CreateDsfMultiAssignObjectActivityViewModel();

            //assert
            Assert.AreEqual(ExpectedCollectionName, vm.CollectionName, "Collection Name not initialized on Multi Assign load");
        }

        [TestMethod]
        [Owner("Clint Stedman")]
        [TestCategory("MultiAssignObjectActivityViewModel_ErrorsProperty")]
        public void MultiAssignObjectActivityViewModel_ErrorsProperty_Constructor_IsNull()
        {
            //------------Setup for test--------------------------
            var MultiAssignObjectActivityViewModel = CreateDsfMultiAssignObjectActivityViewModel();
            //------------Execute Test---------------------------
            var errorInfos = MultiAssignObjectActivityViewModel.Errors;
            //------------Assert Results-------------------------
            Assert.IsNull(errorInfos);
        }

        public static MultiAssignObjectDesignerViewModel CreateDsfMultiAssignObjectActivityViewModel()
        {
            var dsfMultiAssignObjectActivityViewModel = new MultiAssignObjectDesignerViewModel(ModelItemUtils.CreateModelItem(new DsfMultiAssignObjectActivity()));
            return dsfMultiAssignObjectActivityViewModel;
        }
    }
}