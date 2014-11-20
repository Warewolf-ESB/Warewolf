
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Dev2.Activities.Designers2.CaseConvert;
using Dev2.Studio.Core.Activities.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Designers.Tests.CaseConvert
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class CaseConvertDesignerViewModelTests
    {
        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("CaseConvertDesignerViewModel_Constructor")]
        public void CaseConvertDesignerViewModel_Constructor__ModelItemIsValid_ListHasFourItems()
        {
            var items = new List<CaseConvertTO> { new CaseConvertTO() };
            var viewModel = new CaseConvertDesignerViewModel(CreateModelItem(items));
            Assert.AreEqual(4, viewModel.ItemsList.Count);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("CaseConvertDesignerViewModel_Constructor")]
        public void CaseConvertDesignerViewModel_Constructor__ModelItemIsValid_CollectionNameIsSetToConvertCollection()
        {
            var items = new List<CaseConvertTO> { new CaseConvertTO() };
            var viewModel = new CaseConvertDesignerViewModel(CreateModelItem(items));
            Assert.AreEqual("ConvertCollection", viewModel.CollectionName);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("CaseConvertDesignerViewModel_Constructor")]
        public void CaseConvertDesignerViewModel_Constructor_ModelItemIsValid_ConvertCollectionHasTwoItems()
        {
            var modelItem = ModelItemUtils.CreateModelItem(new DsfCaseConvertActivity());
            var viewModel = new CaseConvertDesignerViewModel(modelItem);
            dynamic mi = viewModel.ModelItem;
            Assert.AreEqual(2, mi.ConvertCollection.Count);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("CaseConvertDesignerViewModel_Constructor")]
        public void CaseConvertDesignerViewModel_Constructor_ModelItemIsInitializedWith4Items_ConvertCollectionHasFourItems()
        {
            var items = new List<CaseConvertTO>
            {
                new CaseConvertTO("", "None", "", 0),
                new CaseConvertTO("", "None", "", 0),
                new CaseConvertTO("", "None", "", 0),
                new CaseConvertTO("", "None", "", 0)
            };
            var viewModel = new CaseConvertDesignerViewModel(CreateModelItem(items));
            dynamic mi = viewModel.ModelItem;
            Assert.AreEqual(4, mi.ConvertCollection.Count);
        }
        
        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("CaseConvertDesignerViewModel_Constructor")]
        public void CaseConvertDesignerViewModel_Constructor_ModelItemIsEmpty_ConvertCollectionHasTwoItems()
        {
            var viewModel = new CaseConvertDesignerViewModel(CreateModelItem(null));
            dynamic mi = viewModel.ModelItem;
            Assert.AreEqual(2, mi.ConvertCollection.Count);
        }


        static ModelItem CreateModelItem(IEnumerable<CaseConvertTO> items, string displayName = "Case Convert")
        {
            var modelItem = ModelItemUtils.CreateModelItem(new DsfCaseConvertActivity());
            modelItem.SetProperty("DisplayName", displayName);

            var modelItemCollection = modelItem.Properties["ConvertCollection"].Collection;
            if(items != null)
            {
                foreach(var dto in items)
                {
                    modelItemCollection.Add(dto);
                }
            }
            return modelItem;
        }
    }
}
