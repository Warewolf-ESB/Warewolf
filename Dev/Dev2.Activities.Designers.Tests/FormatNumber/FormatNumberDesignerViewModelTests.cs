
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
using System.Diagnostics.CodeAnalysis;
using Dev2.Studio.Core.Activities.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Designers.Tests.FormatNumber
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class FormatNumberDesignerViewModelTests
    {
        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("FormatNumberDesignerViewModel_Constructor")]
        public void FormatNumberDesignerViewModel_Constructor_ModelItemIsValid_SelectedRoundingTypeIsInitialized()
        {
            var modelItem = CreateModelItem();
            var viewModel = new TestFormatNumberDesignerViewModel(modelItem);
            Assert.AreEqual("None", viewModel.SelectedRoundingType);
            Assert.AreEqual("None", viewModel.RoundingType);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("FormatNumberDesignerViewModel_Constructor")]
        public void FormatNumberDesignerViewModel_Constructor_ModelItemIsValid_RoundingTypesHasFourItems()
        {
            var modelItem = CreateModelItem();
            var viewModel = new TestFormatNumberDesignerViewModel(modelItem);
            Assert.AreEqual(4, viewModel.RoundingTypes.Count);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("FormatNumberDesignerViewModel_SetSelectedRoundingType")]
        public void FormatNumberDesignerViewModel_SetSelectedSelectedSort_ValidOrderType_RoundingTypeOnModelItemIsAlsoSet()
        {
            var modelItem = CreateModelItem();
            var viewModel = new TestFormatNumberDesignerViewModel(modelItem);
            const string ExpectedValue = "Normal";
            viewModel.SelectedRoundingType = ExpectedValue;
            Assert.AreEqual(ExpectedValue, viewModel.RoundingType);
        }

        static ModelItem CreateModelItem()
        {
            return ModelItemUtils.CreateModelItem(new DsfNumberFormatActivity());
        }
    }
}
