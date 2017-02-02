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
using Dev2.Activities.Designers2.XPath;
using Dev2.Common.Interfaces.Help;
using Dev2.Interfaces;
using Dev2.Studio.Core.Activities.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Designers.Tests.XPath
{
    [TestClass]
    // ReSharper disable InconsistentNaming
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
        [Owner("Pieter Terblanche")]
        [TestCategory("XPathDesignerViewModel_Handle")]
        public void XPathDesignerViewModel_UpdateHelp_ShouldCallToHelpViewMode()
        {
            //------------Setup for test--------------------------      
            var mockMainViewModel = new Mock<IMainViewModel>();
            var mockHelpViewModel = new Mock<IHelpWindowViewModel>();
            mockHelpViewModel.Setup(model => model.UpdateHelpText(It.IsAny<string>())).Verifiable();
            mockMainViewModel.Setup(model => model.HelpViewModel).Returns(mockHelpViewModel.Object);
            CustomContainer.Register(mockMainViewModel.Object);

            var items = new List<XPathDTO> { new XPathDTO() };
            var viewModel = new XPathDesignerViewModel(CreateModelItem(items));
            //------------Execute Test---------------------------
            viewModel.UpdateHelpDescriptor("help");
            //------------Assert Results-------------------------
            mockHelpViewModel.Verify(model => model.UpdateHelpText(It.IsAny<string>()), Times.Once());
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

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("XPathDesignerViewModel_ValidateThis")]
        public void XPathDesignerViewModel_ValidateThis_SourceStringIsNotEmpty_DoesNotHaveErrors()
        {
            //------------Setup for test--------------------------
            var items = new List<XPathDTO> { new XPathDTO() };
            var mi = CreateModelItem(items);
            mi.SetProperty("SourceString", "<x></x>");
            var viewModel = CreateXPathDesignerViewModel(mi);

            //------------Execute Test---------------------------
            viewModel.Validate();
            //------------Assert Results-------------------------
            Assert.IsNull(viewModel.Errors);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("XPathDesignerViewModel_ValidateThis")]
        public void XPathDesignerViewModel_ValidateThis_XPathIsInvalid_DoesHaveErrors()
        {
            //------------Setup for test--------------------------
            var items = new List<XPathDTO> { new XPathDTO { XPath = "%", OutputVariable = "[[a]]" } };
            var mi = CreateModelItem(items);
            mi.SetProperty("SourceString", "<x></x>");
            var viewModel = CreateXPathDesignerViewModel(mi);
            //------------Execute Test---------------------------
            viewModel.Validate();
            //------------Assert Results-------------------------
            Assert.IsNotNull(viewModel.Errors);
            Assert.AreEqual(Warewolf.Resource.Errors.ErrorResource.XPathInvalidExpressionErrorTest, viewModel.Errors[0].Message);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("XPathDesignerViewModel_ValidateThis")]
        public void XPathDesignerViewModel_ValidateThis_OutputVariableHasSpecialCharacter_DoesHaveErrors()
        {
            //------------Setup for test--------------------------
            var items = new List<XPathDTO> { new XPathDTO { XPath = "a", OutputVariable = "[[a$]]" } };
            var mi = CreateModelItem(items);
            mi.SetProperty("SourceString", "<x></x>");
            var viewModel = CreateXPathDesignerViewModel(mi);
            //------------Execute Test---------------------------
            viewModel.Validate();
            //------------Assert Results-------------------------
            Assert.IsNotNull(viewModel.Errors);
            Assert.AreEqual("'Results' - Variable name [[a$]] contains invalid character(s). Only use alphanumeric _ and - ", viewModel.Errors[0].Message);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("XPathDesignerViewModel_ValidateThis")]
        public void XPathDesignerViewModel_ValidateThis_XPathIsValidButNoOuputVariable_DoesHaveErrors()
        {
            //------------Setup for test--------------------------
            var items = new List<XPathDTO> { new XPathDTO { XPath = "//root/number[@id='1']/text()", OutputVariable = "" } };
            var mi = CreateModelItem(items);
            mi.SetProperty("SourceString", "<x></x>");
            var viewModel = CreateXPathDesignerViewModel(mi);
            //------------Execute Test---------------------------
            viewModel.Validate();
            //------------Assert Results-------------------------
            Assert.IsNotNull(viewModel.Errors);
            Assert.AreEqual(Warewolf.Resource.Errors.ErrorResource.XPathResultsNotNullErrorTest, viewModel.Errors[0].Message);
        }


        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("XPathDesignerViewModel_ValidateThis")]
        public void XPathDesignerViewModel_ValidateThis_SourceStringIsEmpty_DoesHaveErrors()
        {
            //------------Setup for test--------------------------
            var items = new List<XPathDTO> { new XPathDTO() };
            var mi = CreateModelItem(items);
            mi.SetProperty("SourceString", "");
            var viewModel = CreateXPathDesignerViewModel(mi);
            //------------Execute Test---------------------------
            viewModel.Validate();
            //------------Assert Results-------------------------
            Assert.AreEqual(1, viewModel.Errors.Count);
            StringAssert.Contains(viewModel.Errors[0].Message, Warewolf.Resource.Errors.ErrorResource.XPathXmlNotNullErrorTest);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("XPathDesignerViewModel_ValidateThis")]
        public void XPathDesignerViewModel_ValidateThis_SourceStringIsInValidXml_DoesHaveErrors()
        {
            //------------Setup for test--------------------------
            var items = new List<XPathDTO> { new XPathDTO() };
            var mi = CreateModelItem(items);
            mi.SetProperty("SourceString", "$$@");
            var viewModel = CreateXPathDesignerViewModel(mi);
            //------------Execute Test---------------------------
            viewModel.Validate();
            //------------Assert Results-------------------------
            Assert.AreEqual(1, viewModel.Errors.Count);
            StringAssert.Contains(viewModel.Errors[0].Message, Warewolf.Resource.Errors.ErrorResource.XPathXmlInvalidExpressionErrorTest);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("XPathDesignerViewModel_ValidateThis")]
        public void XPathDesignerViewModel_ValidateThis_SourceStringIsValidRecordset_DoesNotHaveErrors()
        {
            //------------Setup for test--------------------------
            var items = new List<XPathDTO> { new XPathDTO() };
            var mi = CreateModelItem(items);
            mi.SetProperty("SourceString", "[[rec().set]]");
            var viewModel = CreateXPathDesignerViewModel(mi);
            //------------Execute Test---------------------------
            viewModel.Validate();
            //------------Assert Results-------------------------
            Assert.IsNull(viewModel.Errors);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("XPathDesignerViewModel_ValidateThis")]
        public void XPathDesignerViewModel_ValidateThis_SourceStringRecordsetHasANegativeIndex_DoesHaveErrors()
        {
            //------------Setup for test--------------------------
            var items = new List<XPathDTO> { new XPathDTO() };
            var mi = CreateModelItem(items);
            mi.SetProperty("SourceString", "[[rec(-1).set]]");
            var viewModel = CreateXPathDesignerViewModel(mi);
            //------------Execute Test---------------------------
            viewModel.Validate();
            //------------Assert Results-------------------------
            Assert.AreEqual(1, viewModel.Errors.Count);
            StringAssert.Contains(viewModel.Errors[0].Message, Warewolf.Resource.Errors.ErrorResource.XPathXmlRecordsetIndexErrorTest);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("XPathDesignerViewModel_ValidateThis")]
        public void XPathDesignerViewModel_ValidateThis_SourceStringRecordsetHasASpecialCharacter_DoesHaveErrors()
        {
            //------------Setup for test--------------------------
            var items = new List<XPathDTO> { new XPathDTO() };
            var mi = CreateModelItem(items);
            mi.SetProperty("SourceString", "[[rec(@).set]]");
            var viewModel = CreateXPathDesignerViewModel(mi);
            //------------Execute Test---------------------------
            viewModel.Validate();
            //------------Assert Results-------------------------
            Assert.AreEqual(1, viewModel.Errors.Count);
            StringAssert.Contains(viewModel.Errors[0].Message, "'XML' - Recordset index (@) contains invalid character(s)");
        }


        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("XPathDesignerViewModel_ValidateCollectionItem")]
        public void XPathDesignerViewModel_ValidateCollectionItem_ValidatesPropertiesOfDTO()
        {
            //------------Setup for test--------------------------
            var mi = ModelItemUtils.CreateModelItem(new DsfXPathActivity());
            mi.SetProperty("SourceString", "a,b");
            var viewModel = CreateXPathDesignerViewModel(mi);
            //------------Execute Test---------------------------
            viewModel.Validate();
            //------------Assert Results-------------------------
            Assert.AreEqual(1, viewModel.Errors.Count);
            StringAssert.Contains(viewModel.Errors[0].Message, Warewolf.Resource.Errors.ErrorResource.XPathXmlInvalidExpressionErrorTest);
        }

        static ModelItem CreateModelItem(IEnumerable<XPathDTO> items, string displayName = "XPath")
        {
            var modelItem = ModelItemUtils.CreateModelItem(new DsfXPathActivity());
            modelItem.SetProperty("DisplayName", displayName);

            var modelProperty = modelItem.Properties["ResultsCollection"];
            if(modelProperty != null)
            {
                var modelItemCollection = modelProperty.Collection;
                foreach(var dto in items)
                {
                    modelItemCollection?.Add(dto);
                }
            }
            return modelItem;
        }
        
        static XPathDesignerViewModel CreateXPathDesignerViewModel(ModelItem mi)
        {
            return new XPathDesignerViewModel(mi)
            {
                GetDatalistString = () =>
                {
                    const string trueString = "True";
                    const string noneString = "None";
                    var datalist = string.Format("<DataList><var Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" /><a Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" /><b Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" /><h Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" /><r Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" /><rec Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" ><set Description=\"\" IsEditable=\"{0}\" ColumnIODirection=\"{1}\" /></rec></DataList>", trueString, noneString);
                    return datalist;
                }
            };
        }
    }
}
