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
using Dev2.Activities.Designers2.CaseConvert;
using Dev2.Common.Interfaces.Help;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using System;
using Dev2.TO;

namespace Dev2.Activities.Designers.Tests.CaseConvert
{
    [TestClass]
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
            Assert.IsTrue(viewModel.HasLargeView);
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
            viewModel.Validate();
            dynamic mi = viewModel.ModelItem;
            Assert.AreEqual(4, mi.ConvertCollection.Count);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("CaseConvertDesignerViewModel_Handle")]
        public void CaseConvertDesignerViewModel_UpdateHelp_ShouldCallToHelpViewMode()
        {
            //------------Setup for test--------------------------      
            var mockMainViewModel = new Mock<IShellViewModel>();
            var mockHelpViewModel = new Mock<IHelpWindowViewModel>();
            mockHelpViewModel.Setup(model => model.UpdateHelpText(It.IsAny<string>())).Verifiable();
            mockMainViewModel.Setup(model => model.HelpViewModel).Returns(mockHelpViewModel.Object);
            CustomContainer.Register(mockMainViewModel.Object);

            var items = new List<CaseConvertTO>
            {
                new CaseConvertTO("", "None", "", 0),
                new CaseConvertTO("", "None", "", 0),
                new CaseConvertTO("", "None", "", 0),
                new CaseConvertTO("", "None", "", 0)
            };
            var viewModel = new CaseConvertDesignerViewModel(CreateModelItem(items));
            //------------Execute Test---------------------------
            viewModel.UpdateHelpDescriptor("help");
            //------------Assert Results-------------------------
            mockHelpViewModel.Verify(model => model.UpdateHelpText(It.IsAny<string>()), Times.Once());
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

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DataMergeDesignerViewModel_ValidateCollectionItem")]
        public void DataMergeDesignerViewModel_ValidateCollectionItem_ValidatesPropertiesOfDTO()
        {
            //------------Setup for test--------------------------
            var mi = ModelItemUtils.CreateModelItem(new DsfCaseConvertActivity());
            mi.SetProperty("DisplayName", "Merge");

            var dto = new CaseConvertTO("a&]]", "Upper", "", 0, true);


            var miCollection = mi.Properties["ConvertCollection"].Collection;
            var dtoModelItem = miCollection.Add(dto);


            var viewModel = new CaseConvertDesignerViewModel(mi);
            viewModel._getDatalistString = () =>
            {
                const string trueString = "True";
                const string noneString = "None";
                var datalist = string.Format("<DataList><var Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" /><a Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" /><b Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" /><h Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" /><r Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" /><rec Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" ><set Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" /></rec></DataList>", trueString, noneString);
                return datalist;
            };

            //------------Execute Test---------------------------
            viewModel.Validate();

            //------------Assert Results-------------------------
            Assert.AreEqual(4, viewModel.Errors.Count);

            StringAssert.Contains(viewModel.Errors[0].Message, Warewolf.Resource.Errors.ErrorResource.DataMergeInvalidExpressionErrorTest);
            Verify_IsFocused(dtoModelItem, viewModel.Errors[0].Do, "IsFieldNameFocused");

            StringAssert.Contains(viewModel.Errors[1].Message, Warewolf.Resource.Errors.ErrorResource.DataMergeUsingNullErrorTest);
            Verify_IsFocused(dtoModelItem, viewModel.Errors[1].Do, "IsAtFocused");
        }

        void Verify_IsFocused(ModelItem modelItem, Action doError, string isFocusedPropertyName)
        {
            Assert.IsFalse(modelItem.GetProperty<bool>(isFocusedPropertyName));
            doError.Invoke();
            Assert.IsTrue(modelItem.GetProperty<bool>(isFocusedPropertyName));
        }

        static ModelItem CreateModelItem(IEnumerable<CaseConvertTO> items, string displayName = "Case Convert")
        {
            var modelItem = ModelItemUtils.CreateModelItem(new DsfCaseConvertActivity());
            modelItem.SetProperty("DisplayName", displayName);

            var modelProperty = modelItem.Properties["ConvertCollection"];
            if(modelProperty != null)
            {
                var modelItemCollection = modelProperty.Collection;
                if(items != null)
                {
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
