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
using System.Windows;
using System.Windows.Controls;
using Dev2.Activities.Designers2.Core.Controls;
using Dev2.Studio.Core.Activities.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Designers.Tests.Designers2.Core.Controls
{
    [TestClass]
    public class Dev2DataGridTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("Dev2DataGrid_SetFocusToInserted")]
        public void Dev2DataGrid_SetFocusToInserted_ItemIsInserted_InvokesFocusOnGridRowChild()
        {
            //------------Setup for test--------------------------      
            var element = new FrameworkElement();
            var dataGrid = new Dev2DataGrid(r => element) { ItemsSource = CreateModelItemCollection(3, false, false, false) };

            var dataGridRow = new DataGridRow();
            dataGridRow.DataContext = dataGrid.Items[0]; // First item is not flagged as inserted

            //------------Execute Test---------------------------
            var result = dataGrid.SetFocusToInserted(dataGridRow);

            //------------Assert Results-------------------------
            Assert.IsTrue(result);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("Dev2DataGrid_SetFocusToInserted")]
        public void Dev2DataGrid_SetFocusToInserted_ItemIsNotInserted_DoesNotInvokeFocusOnGridRowChild()
        {
            //------------Setup for test--------------------------      
            var element = new FrameworkElement();
            var dataGrid = new Dev2DataGrid(r => element) { ItemsSource = CreateModelItemCollection(3, false, false, false) };

            var dataGridRow = new DataGridRow();
            dataGridRow.DataContext = dataGrid.Items[1];    // Second item is not flagged as inserted

            //------------Execute Test---------------------------
            var result = dataGrid.SetFocusToInserted(dataGridRow);

            //------------Assert Results-------------------------
            Assert.IsFalse(result);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("Dev2DataGrid_GetFocusElement")]
        public void Dev2DataGrid_GetFocusElement_RowIsNotNull_GetVisualChildResult()
        {
            //------------Setup for test--------------------------      
            var element = new FrameworkElement();
            var dataGrid = new Dev2DataGrid(r => element) { ItemsSource = CreateModelItemCollection(3, false, false, false) };
            var dataGridRow = new DataGridRow();

            //------------Execute Test---------------------------
            var result = dataGrid.GetFocusElement(dataGridRow);

            //------------Assert Results-------------------------
            Assert.AreSame(element, result);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("Dev2DataGrid_GetFocusElement")]
        public void Dev2DataGrid_GetFocusElement_RowIsNull_Null()
        {
            //------------Setup for test--------------------------      
            var element = new FrameworkElement();
            var dataGrid = new Dev2DataGrid(r => element) { ItemsSource = CreateModelItemCollection(3, false, false, false) };

            //------------Execute Test---------------------------
            var result = dataGrid.GetFocusElement(null);

            //------------Assert Results-------------------------
            Assert.IsNull(result);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("Dev2DataGrid_Init")]
        public void Dev2DataGrid_Init_VirtulizationIsOn()
        {
            //------------Setup for test--------------------------      
            var element = new FrameworkElement();
            var dataGrid = new Dev2DataGrid(r => element) { ItemsSource = CreateModelItemCollection(3, false, false, false) };

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.IsTrue(dataGrid.EnableRowVirtualization);
        }

        static ModelItemCollection CreateModelItemCollection(int itemCount, params bool[] blankFieldAndValues)
        {
            var modelItem = ModelItemUtils.CreateModelItem(new DsfMultiAssignActivity());
            // ReSharper disable PossibleNullReferenceException
            var modelItemCollection = modelItem.Properties["FieldsCollection"].Collection;
            for(var i = 0; i < itemCount; i++)
            {
                var indexNumber = i + 1;
                var dto = blankFieldAndValues[i]
                    ? new ActivityDTO("", "", indexNumber)
                    : new ActivityDTO("field" + indexNumber, "value" + indexNumber, indexNumber, i == 0);

                modelItemCollection.Add(dto);
            }
            // ReSharper restore PossibleNullReferenceException
            return modelItemCollection;
        }
    }
}
