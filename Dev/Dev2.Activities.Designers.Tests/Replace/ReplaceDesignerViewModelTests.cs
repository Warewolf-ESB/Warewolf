
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Activities.Presentation.Model;
using Dev2.Activities.Designers2.Replace;
using Dev2.Studio.Core.Activities.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Designers.Tests.Replace
{
    [TestClass]
    public class ReplaceDesignerViewModelTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ReplaceDesignerViewModel_Constructor")]
        public void ReplaceDesignerViewModel_Constructor_PropertiesInitialized()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var viewModel = new ReplaceDesignerViewModel(CreateModelItem());

            //------------Assert Results-------------------------
            Assert.AreEqual(1, viewModel.TitleBarToggles.Count);
            StringAssert.Contains(viewModel.TitleBarToggles[0].ExpandToolTip, "Help");
        }


        static ModelItem CreateModelItem()
        {
            return ModelItemUtils.CreateModelItem(new DsfReplaceActivity());
        }

        static ReplaceDesignerViewModel CreateViewModel()
        {
            var viewModel = new ReplaceDesignerViewModel(ModelItemUtils.CreateModelItem(CreateModelItem()));
            return viewModel;
        }
    }
}
