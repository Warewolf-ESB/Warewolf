using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Dev2.Activities.Designers2.XPath;
using Dev2.Studio.Core.Activities.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Designers.Tests.XPath
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class XPathDesignerViewModelTests
    {
        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("XPathDesignerViewModel_Constructor")]
        public void XPathDesignerViewModel_Constructor_ModelItemIsValid_CollectionNameIsSetToResultsCollection()
        {
            var items = new List<XPathDTO> { new XPathDTO() };
            var viewModel = new XPathDesignerViewModel(CreateModelItem(items));
            Assert.AreEqual("ResultsCollection", viewModel.CollectionName);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("XPathDesignerViewModel_Constructor")]
        public void XPathDesignerViewModel_Constructor_ModelItemIsValid_ResultsCollectionHasTwoItems()
        {
            var modelItem = ModelItemUtils.CreateModelItem(new DsfXPathActivity());
            var viewModel = new XPathDesignerViewModel(modelItem);
            dynamic mi = viewModel.ModelItem;
            Assert.AreEqual(2, mi.ResultsCollection.Count);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("XPathDesignerViewModel_Constructor")]
        public void XPathDesignerViewModel_Constructor_ModelItemIsInitializedWith4Items_ResultsCollectionHasFourItems()
        {
            var items = new List<XPathDTO>
            {
                new XPathDTO("", "None", 0),
                new XPathDTO("", "None", 0),
                new XPathDTO("", "None", 0),
                new XPathDTO("", "None", 0)
            };
            var viewModel = new XPathDesignerViewModel(CreateModelItem(items));
            dynamic mi = viewModel.ModelItem;
            Assert.AreEqual(5, mi.ResultsCollection.Count);
        }

        static ModelItem CreateModelItem(IEnumerable<XPathDTO> items, string displayName = "XPath")
        {
            var modelItem = ModelItemUtils.CreateModelItem(new DsfXPathActivity());
            modelItem.SetProperty("DisplayName", displayName);

            var modelItemCollection = modelItem.Properties["ResultsCollection"].Collection;
            foreach(var dto in items)
            {
                modelItemCollection.Add(dto);
            }
            return modelItem;
        }
    }
}
