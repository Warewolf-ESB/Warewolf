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
using System.Collections.Generic;
using Dev2.Activities.Designers2.GatherSystemInformation;
using Dev2.Common.Interfaces.Help;
using Dev2.Data.Enums;
using Dev2.Interfaces;
using Dev2.Studio.Core.Activities.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Activities.Designers.Tests.GatherSystemInformation
{
    [TestClass]
    public class GatherSystemInformationDesignerViewModelTests
    {
        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("GatherSystemInformationDesignerViewModel_Constructor")]
        public void GatherSystemInformationDesignerViewModel_Constructor_ModelItemIsValid_ListHasFourItems()
        {
            var items = new List<GatherSystemInformationTO> { new GatherSystemInformationTO() };
            var viewModel = new GatherSystemInformationDesignerViewModel(CreateModelItem(items));
            viewModel.Validate();
            Assert.AreEqual(29, viewModel.ItemsList.Count);
            Assert.IsTrue(viewModel.HasLargeView);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("GatherSystemInformationDesignerViewModel_Handle")]
        public void GatherSystemInformationDesignerViewModel_UpdateHelp_ShouldCallToHelpViewMode()
        {
            //------------Setup for test--------------------------      
            var mockMainViewModel = new Mock<IMainViewModel>();
            var mockHelpViewModel = new Mock<IHelpWindowViewModel>();
            mockHelpViewModel.Setup(model => model.UpdateHelpText(It.IsAny<string>())).Verifiable();
            mockMainViewModel.Setup(model => model.HelpViewModel).Returns(mockHelpViewModel.Object);
            CustomContainer.Register(mockMainViewModel.Object);

            var items = new List<GatherSystemInformationTO> { new GatherSystemInformationTO() };
            var viewModel = new GatherSystemInformationDesignerViewModel(CreateModelItem(items));
            //------------Execute Test---------------------------
            viewModel.UpdateHelpDescriptor("help");
            //------------Assert Results-------------------------
            mockHelpViewModel.Verify(model => model.UpdateHelpText(It.IsAny<string>()), Times.Once());
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("GatherSystemInformationDesignerViewModel_Constructor")]
        public void GatherSystemInformationDesignerViewModel_Constructor_ModelItemIsValid_CollectionNameIsSetToSystemInformationCollection()
        {
            var items = new List<GatherSystemInformationTO> { new GatherSystemInformationTO() };
            var viewModel = new GatherSystemInformationDesignerViewModel(CreateModelItem(items));
            Assert.AreEqual("SystemInformationCollection", viewModel.CollectionName);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("GatherSystemInformationDesignerViewModel_Constructor")]
        public void GatherSystemInformationDesignerViewModel_Constructor_ModelItemIsValid_SystemInformationCollectionHasTwoItems()
        {
            var modelItem = ModelItemUtils.CreateModelItem(new DsfGatherSystemInformationActivity());
            var viewModel = new GatherSystemInformationDesignerViewModel(modelItem);
            dynamic mi = viewModel.ModelItem;
            Assert.AreEqual(2, mi.SystemInformationCollection.Count);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("GatherSystemInformationDesignerViewModel_Constructor")]
        public void GatherSystemInformationDesignerViewModel_Constructor_ModelItemIsInitializedWith4Items_SystemInformationCollectionHasFourItems()
        {
            var items = new List<GatherSystemInformationTO>
            {
                new GatherSystemInformationTO(enTypeOfSystemInformationToGather.FullDateTime, "None", 0),
                new GatherSystemInformationTO(enTypeOfSystemInformationToGather.FullDateTime, "None", 0),
                new GatherSystemInformationTO(enTypeOfSystemInformationToGather.FullDateTime, "None", 0),
                new GatherSystemInformationTO(enTypeOfSystemInformationToGather.FullDateTime, "None", 0)
            };
            var viewModel = new GatherSystemInformationDesignerViewModel(CreateModelItem(items));
            dynamic mi = viewModel.ModelItem;
            Assert.AreEqual(5, mi.SystemInformationCollection.Count);
        }

        static ModelItem CreateModelItem(IEnumerable<GatherSystemInformationTO> items, string displayName = "Split")
        {
            var modelItem = ModelItemUtils.CreateModelItem(new DsfGatherSystemInformationActivity());
            modelItem.SetProperty("DisplayName", displayName);

            if(modelItem != null)
            {
                var modelProperty = modelItem.Properties["SystemInformationCollection"];
                if(modelProperty != null)
                {
                    var modelItemCollection = modelProperty.Collection;
                    foreach(var dto in items)
                    {
                        modelItemCollection?.Add(dto);
                    }
                }
            }
            return modelItem;
        }
    }
}
