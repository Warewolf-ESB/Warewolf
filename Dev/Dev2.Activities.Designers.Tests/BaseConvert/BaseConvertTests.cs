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
using System.Linq;
using Dev2.Activities.Designers2.BaseConvert;
using Dev2.Common.Interfaces.Enums.Enums;
using Dev2.Common.Interfaces.Help;
using Dev2.Converters;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using System;

namespace Dev2.Activities.Designers.Tests.BaseConvert
{
    [TestClass]
    public class BaseConvertTests
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("BaseConvertViewModel_Constructor")]
        public void BaseConvertViewModel_Constructor_PropertiesInitialized()
        {
            //------------Setup for test--------------------------
            var items = new List<BaseConvertTO>
            {
                new BaseConvertTO("xxxx","Text" ,"Binary","", 1),
                new BaseConvertTO("yyyy","Text" ,"Text","", 2)
            };

            //------------Execute Test---------------------------
            var viewModel = new BaseConvertDesignerViewModel(CreateModelItem(items));
            //------------Assert Results-------------------------
            Assert.IsNotNull(viewModel.ModelItem);
            Assert.IsNotNull(viewModel.ModelItemCollection);
            Assert.AreEqual("ConvertCollection", viewModel.CollectionName);

            var expectedOptions = Dev2EnumConverter.ConvertEnumsTypeToStringList<enDev2BaseConvertType>().ToList();
            CollectionAssert.AreEqual(expectedOptions, viewModel.ConvertTypes.ToList());

            Assert.AreEqual(1, viewModel.TitleBarToggles.Count);
            Assert.IsTrue(viewModel.HasLargeView);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("BaseConvertViewModel_Handle")]
        public void BaseConvertViewModel_UpdateHelp_ShouldCallToHelpViewMode()
        {
            //------------Setup for test--------------------------      
            var items = new List<BaseConvertTO>
            {
                new BaseConvertTO("xxxx","Text" ,"Binary","", 1),
                new BaseConvertTO("yyyy","Text" ,"Text","", 2)
            };
            var mockMainViewModel = new Mock<IShellViewModel>();
            var mockHelpViewModel = new Mock<IHelpWindowViewModel>();
            mockHelpViewModel.Setup(model => model.UpdateHelpText(It.IsAny<string>())).Verifiable();
            mockMainViewModel.Setup(model => model.HelpViewModel).Returns(mockHelpViewModel.Object);
            CustomContainer.Register(mockMainViewModel.Object);
            var viewModel = new BaseConvertDesignerViewModel(CreateModelItem(items));
            //------------Execute Test---------------------------
            viewModel.UpdateHelpDescriptor("help");
            //------------Assert Results-------------------------
            mockHelpViewModel.Verify(model => model.UpdateHelpText(It.IsAny<string>()), Times.Once());
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("BaseConvertDesignerViewModel_ValidateCollectionItem")]
        public void BaseConvertDesignerViewModel_ValidateCollectionItem_ValidatesPropertiesOfDTO()
        {
            //------------Setup for test--------------------------
            var mi = ModelItemUtils.CreateModelItem(new DsfBaseConvertActivity());
            mi.SetProperty("DisplayName", "Case Convert");

            var dto = new BaseConvertTO("a&]]", "Text", "Base64","",1, true);


            var miCollection = mi.Properties["ConvertCollection"].Collection;
            var dtoModelItem = miCollection.Add(dto);


            var viewModel = new BaseConvertDesignerViewModel(mi);
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
            Assert.AreEqual(1, viewModel.Errors.Count);

            StringAssert.Contains(viewModel.Errors[0].Message, Warewolf.Resource.Errors.ErrorResource.BaseConvertInputInvalidExpressionErrorTest);
            Verify_IsFocused(dtoModelItem, viewModel.Errors[0].Do, "IsFromExpressionFocused");
        }

        void Verify_IsFocused(ModelItem modelItem, Action doError, string isFocusedPropertyName)
        {
            Assert.IsFalse(modelItem.GetProperty<bool>(isFocusedPropertyName));
            doError.Invoke();
            Assert.IsTrue(modelItem.GetProperty<bool>(isFocusedPropertyName));
        }

        static ModelItem CreateModelItem(IEnumerable<BaseConvertTO> items, string displayName = "Base Convert")
        {
            var modelItem = ModelItemUtils.CreateModelItem(new DsfBaseConvertActivity());
            modelItem.SetProperty("DisplayName", displayName);

            
            var modelItemCollection = modelItem.Properties["ConvertCollection"].Collection;
            foreach(var dto in items)
            {
                modelItemCollection.Add(dto);
            }
            

            return modelItem;
        }
    }
}
