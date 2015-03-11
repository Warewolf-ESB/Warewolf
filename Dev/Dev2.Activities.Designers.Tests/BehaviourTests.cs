
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


//using System.Windows.Controls;

using System.Activities.Presentation.Model;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Controls;
using Dev2.Core.Tests.Activities;
using Dev2.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Designers.Tests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class BehaviourTests
    {
        #region DataGridFocusTextOnLoadBehavior

        [TestMethod]
        [TestCategory("DataGridFocusTextOnLoadBehavior_UnitTest")]
        [Description("The DataGridFocusTextOnLoad behavior can initialize")]
        [Owner("Ashley Lewis")]
        // ReSharper disable InconsistentNaming
        public void DataGridFocusTextOnLoadBehavior_Constructor_DesignerClassInitializes()
        // ReSharper restore InconsistentNaming
        {
            var behaviour = new testDataGridFocusTextOnLoadBehavior();
            Assert.IsInstanceOfType(behaviour, typeof(DataGridFocusTextOnLoadBehavior), "DataGridFocusTextOnLoad behavior cannot initialize");
        }

        [TestMethod]
        [TestCategory("DataGridFocusTextOnLoadBehavior_UnitTest")]
        [Description("The DataGridFocusTextOnLoad behavior sets focus of the correct datagrid textbox on load")]
        [Owner("Ashley Lewis")]
        // ReSharper disable InconsistentNaming
        public void DataGridFocusTextOnLoadBehavior_OnAssociatedObjectLoaded_TextBoxGetsFocus()
        // ReSharper restore InconsistentNaming
        {
            //init
            var mockDatagrid = new Mock<DataGrid>();
            var mockModelItem = new Mock<ModelItem>();
            var mockContext = new Mock<IDev2TOFn>();
            var mockTextbox = new Mock<TextBox>();
            var behaviour = new Mock<testDataGridFocusTextOnLoadBehavior>();
            var dto = new ActivityDTO();

            mockContext.Setup(c => c.Inserted).Returns(true);
            mockModelItem.Setup(c => c.GetCurrentValue()).Returns(mockContext.Object);
            dto.Inserted = true;
            mockDatagrid.Object.Items.Add(dto);
            behaviour.Setup(c => c.GetVisualChild<TextBox>(mockDatagrid.Object)).Returns(mockTextbox.Object);
            behaviour.Setup(c => c.GetVisualChild<TextBox>(mockDatagrid.Object)).Verifiable();

            //exe
            behaviour.Object.TestAssociatedObject_Loaded(mockDatagrid);

            //assert
            behaviour.Verify(c => c.GetVisualChild<TextBox>(mockDatagrid.Object), Times.Once());
        }

        #endregion
    }
}
