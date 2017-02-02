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
using Dev2.Interfaces;
using Dev2.Studio.Core.Activities.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Unlimited.Applications.BusinessDesignStudio.Activities;

// ReSharper disable InconsistentNaming 
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
            viewModel.Validate();
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
            var mockMainViewModel = new Mock<IMainViewModel>();
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


        static ModelItem CreateModelItem(IEnumerable<BaseConvertTO> items, string displayName = "Base Convert")
        {
            var modelItem = ModelItemUtils.CreateModelItem(new DsfBaseConvertActivity());
            modelItem.SetProperty("DisplayName", displayName);

            // ReSharper disable PossibleNullReferenceException
            var modelItemCollection = modelItem.Properties["ConvertCollection"].Collection;
            foreach(var dto in items)
            {
                modelItemCollection.Add(dto);
            }
            // ReSharper restore PossibleNullReferenceException

            return modelItem;
        }
    }
}
