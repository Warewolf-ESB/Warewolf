using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Dev2.Activities.Designers2.GatherSystemInformation;
using Dev2.Data.Enums;
using Dev2.Studio.Core.Activities.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Activities.Designers.Tests.GatherSystemInformation
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class GatherSystemInformationDesignerViewModelTests
    {
        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("GatherSystemInformationDesignerViewModel_Constructor")]
        public void GatherSystemInformationDesignerViewModel_Constructor_ModelItemIsValid_ListHasFourItems()
        {
            var items = new List<GatherSystemInformationTO> { new GatherSystemInformationTO() };
            var viewModel = new GatherSystemInformationDesignerViewModel(CreateModelItem(items));
            Assert.AreEqual(17, viewModel.ItemsList.Count);
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

            var modelItemCollection = modelItem.Properties["SystemInformationCollection"].Collection;
            foreach(var dto in items)
            {
                modelItemCollection.Add(dto);
            }
            return modelItem;
        }
    }
}
