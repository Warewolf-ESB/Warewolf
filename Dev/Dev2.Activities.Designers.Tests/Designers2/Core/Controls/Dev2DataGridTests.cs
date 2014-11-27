
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
using System.Windows;
using System.Windows.Controls;
using Dev2.Activities.Designers2.Core.Controls;
using Dev2.Studio.Core.Activities.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Designers.Tests.Designers2.Core.Controls
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class Dev2DataGridTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("Dev2DataGrid_RemoveFirstDuplicateBlankRow")]
        public void Dev2DataGrid_RemoveFirstDuplicateBlankRow_NullCollection_DoesNothing()
        {
            //------------Setup for test--------------------------
            var dataGrid = new Dev2DataGrid();

            //------------Execute Test---------------------------
            dataGrid.RemoveFirstDuplicateBlankRow();

            //------------Assert Results-------------------------
            Assert.AreEqual(0, dataGrid.Items.Count);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("Dev2DataGrid_RemoveFirstDuplicateBlankRow")]
        public void Dev2DataGrid_RemoveFirstDuplicateBlankRow_SelectedValueIsNull_DoesNothing()
        {
            //------------Setup for test--------------------------            
            var dataGrid = CreateDataGrid(2, false, false);
            dataGrid.SelectedItem = null;

            //------------Execute Test---------------------------
            dataGrid.RemoveFirstDuplicateBlankRow();

            //------------Assert Results-------------------------
            Assert.AreEqual(2, dataGrid.Items.Count);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("Dev2DataGrid_RemoveFirstDuplicateBlankRow")]
        public void Dev2DataGrid_RemoveFirstDuplicateBlankRow_ItemCountIsTwo_DoesNothing()
        {
            //------------Setup for test--------------------------            
            var dataGrid = CreateDataGrid(2, false, false);
            dataGrid.SelectedItem = dataGrid.Items[0];          // select first row

            //------------Execute Test---------------------------
            dataGrid.RemoveFirstDuplicateBlankRow();

            //------------Assert Results-------------------------
            Assert.AreEqual(2, dataGrid.Items.Count);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("Dev2DataGrid_RemoveFirstDuplicateBlankRow")]
        public void Dev2DataGrid_RemoveFirstDuplicateBlankRow_ItemCountIsOne_DoesNothing()
        {
            //------------Setup for test--------------------------            
            var dataGrid = CreateDataGrid(1, false, false);
            dataGrid.SelectedItem = dataGrid.Items[0];          // select first row

            //------------Execute Test---------------------------
            dataGrid.RemoveFirstDuplicateBlankRow();

            //------------Assert Results-------------------------
            Assert.AreEqual(1, dataGrid.Items.Count);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("Dev2DataGrid_RemoveFirstDuplicateBlankRow")]
        public void Dev2DataGrid_RemoveFirstDuplicateBlankRow_ItemsContainsOneBlankRow_DoesNothing()
        {
            //------------Setup for test--------------------------            
            var dataGrid = CreateDataGrid(3, true, false, false);   // first row is blank
            dataGrid.SelectedItem = dataGrid.Items[1];              // select non-blank row

            //------------Execute Test---------------------------
            dataGrid.RemoveFirstDuplicateBlankRow();

            //------------Assert Results-------------------------
            Assert.AreEqual(3, dataGrid.Items.Count);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("Dev2DataGrid_RemoveFirstDuplicateBlankRow")]
        public void Dev2DataGrid_RemoveFirstDuplicateBlankRow_ItemsContainsTwoOrMoreBlankRow_RemovesFirstBlankRow()
        {
            //------------Setup for test--------------------------            
            var dataGrid = CreateDataGrid(4, true, false, true, true);  // first and last 2 rows are blank
            dataGrid.SelectedItem = dataGrid.Items[1];                  // select non-blank row

            //------------Execute Test---------------------------
            dataGrid.RemoveFirstDuplicateBlankRow();

            //------------Assert Results-------------------------
            Assert.AreEqual(3, dataGrid.Items.Count);
            VerifyItem(dataGrid.Items[0], 2, 1);
            VerifyItem(dataGrid.Items[1], 3, 2, true);
            VerifyItem(dataGrid.Items[2], 4, 3, true);
        }


        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("Dev2DataGrid_RemoveRow")]
        public void Dev2DataGrid_RemoveRow_NullCollection_DoesNothing()
        {
            //------------Setup for test--------------------------
            var dataGrid = new Dev2DataGrid();

            //------------Execute Test---------------------------
            dataGrid.RemoveRow(0);

            //------------Assert Results-------------------------
            Assert.AreEqual(0, dataGrid.Items.Count);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("Dev2DataGrid_RemoveRow")]
        public void Dev2DataGrid_RemoveRow_ItemCountIsTwo_RemovesAtIndexAndAddsBlankRow()
        {
            //------------Setup for test--------------------------            
            var dataGrid = CreateDataGrid(2, false, false);
            var oldItem1 = dataGrid.Items[0];
            var oldItem2 = dataGrid.Items[1];

            //------------Execute Test---------------------------
            dataGrid.RemoveRow(0);

            //------------Assert Results-------------------------
            Assert.AreEqual(2, dataGrid.Items.Count);
            Assert.AreNotSame(oldItem1, dataGrid.Items[0]);
            Assert.AreSame(oldItem2, dataGrid.Items[1]);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("Dev2DataGrid_RemoveRow")]
        public void Dev2DataGrid_RemoveRow_ItemCountIsThreeOrMore_RemovesAtIndexAndRenumbersSubsequentIndexNumbers()
        {
            //------------Setup for test--------------------------            
            var dataGrid = CreateDataGrid(4, false, false, false, false);
            var oldItem1 = dataGrid.Items[0];
            var oldItem2 = dataGrid.Items[1];   // item to be removed
            var oldItem3 = dataGrid.Items[2];
            var oldItem4 = dataGrid.Items[3];

            //------------Execute Test---------------------------
            dataGrid.RemoveRow(1);

            //------------Assert Results-------------------------
            Assert.AreEqual(3, dataGrid.Items.Count);
            Assert.AreSame(oldItem1, dataGrid.Items[0]);
            Assert.AreSame(oldItem3, dataGrid.Items[1]);
            Assert.AreSame(oldItem4, dataGrid.Items[2]);

            VerifyItem(dataGrid.Items[0], 1, 1);
            VerifyItem(dataGrid.Items[1], 3, 2);
            VerifyItem(dataGrid.Items[2], 4, 3);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("Dev2DataGrid_AddRow")]
        public void Dev2DataGrid_AddRow_ItemsContainsABlankRow_DoesNothing()
        {
            //------------Setup for test--------------------------            
            var dataGrid = CreateDataGrid(3, false, true, false);
            var oldItem1 = dataGrid.Items[0];
            var oldItem2 = dataGrid.Items[1];   // blank row
            var oldItem3 = dataGrid.Items[2];

            //------------Execute Test---------------------------
            dataGrid.AddRow();

            //------------Assert Results-------------------------
            Assert.AreEqual(3, dataGrid.Items.Count);
            Assert.AreSame(oldItem1, dataGrid.Items[0]);
            Assert.AreSame(oldItem2, dataGrid.Items[1]);
            Assert.AreSame(oldItem3, dataGrid.Items[2]);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("Dev2DataGrid_AddRow")]
        public void Dev2DataGrid_AddRow_ItemsDoesNotContainABlankRow_AddsBlankItem()
        {
            //------------Setup for test--------------------------            
            var dataGrid = CreateDataGrid(3, false, false, false);
            var oldItem1 = dataGrid.Items[0];
            var oldItem2 = dataGrid.Items[1];
            var oldItem3 = dataGrid.Items[2];

            //------------Execute Test---------------------------
            dataGrid.AddRow();

            //------------Assert Results-------------------------
            Assert.AreEqual(4, dataGrid.Items.Count);
            Assert.AreSame(oldItem1, dataGrid.Items[0]);
            Assert.AreSame(oldItem2, dataGrid.Items[1]);
            Assert.AreSame(oldItem3, dataGrid.Items[2]);

            VerifyItem(dataGrid.Items[3], 4, 4, true);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("Dev2DataGrid_InsertRow")]
        public void Dev2DataGrid_InsertRow_InsertsAtIndexAndRenumbersSubsequentIndexNumbers()
        {
            //------------Setup for test--------------------------            
            var dataGrid = CreateDataGrid(3, false, false, false);
            var oldItem1 = dataGrid.Items[0];
            var oldItem2 = dataGrid.Items[1];
            var oldItem3 = dataGrid.Items[2];
            //var oldItem4 = dataGrid.Items[3];

            const int InsertIndex = 1;

            //------------Execute Test---------------------------
            dataGrid.InsertRow(InsertIndex);

            //------------Assert Results-------------------------
            Assert.AreEqual(4, dataGrid.Items.Count);
            Assert.AreSame(oldItem1, dataGrid.Items[0]);
            Assert.AreSame(oldItem2, dataGrid.Items[2]);
            Assert.AreSame(oldItem3, dataGrid.Items[3]);

            VerifyItem(dataGrid.Items[0], 1, 1);
            VerifyItem(dataGrid.Items[1], 2, 2, true);
            VerifyItem(dataGrid.Items[2], 2, 3);
            VerifyItem(dataGrid.Items[3], 3, 4);

            Assert.AreEqual(InsertIndex, dataGrid.SelectedIndex);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("Dev2DataGrid_CountRows")]
        public void Dev2DataGrid_CountRows_ItemsCount()
        {
            //------------Setup for test--------------------------            
            var dataGrid = CreateDataGrid(3, false, false, false);

            //------------Execute Test---------------------------
            var actualCount = dataGrid.CountRows();

            //------------Assert Results-------------------------
            Assert.AreEqual(3, actualCount);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("Dev2DataGrid_OnDataGridRowLoaded")]
        public void Dev2DataGrid_OnDataGridRowLoaded_WiredUp()
        {
            //------------Setup for test--------------------------            

            //------------Execute Test---------------------------
            var dataGrid = new Dev2DataGrid();

            //------------Assert Results-------------------------
            Assert.IsNotNull(dataGrid.RowStyle);
            Assert.AreEqual(1, dataGrid.RowStyle.Setters.Count);

            var setter = (EventSetter)dataGrid.RowStyle.Setters[0];
            Assert.AreEqual("Loaded", setter.Event.Name);
            Assert.AreEqual("OnDataGridRowLoaded", setter.Handler.Method.Name);
            Assert.AreEqual(typeof(Dev2DataGrid), setter.Handler.Method.DeclaringType);
        }

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


        static void VerifyItem(dynamic item, int oldIndexNumber, int newIndexNumber, bool isBlank = false)
        {
            var dto = (ActivityDTO)item.GetCurrentValue();
            Assert.AreEqual(newIndexNumber, dto.IndexNumber);
            if(isBlank)
            {
                Assert.AreEqual(string.Empty, dto.FieldName);
                Assert.AreEqual(string.Empty, dto.FieldValue);
            }
            else
            {
                Assert.AreEqual("field" + oldIndexNumber, dto.FieldName);
                Assert.AreEqual("value" + oldIndexNumber, dto.FieldValue);
            }
        }

        static Dev2DataGrid CreateDataGrid(int itemCount, params bool[] blankFieldAndValues)
        {
            var modelItemCollection = CreateModelItemCollection(itemCount, blankFieldAndValues);

            var dg = new Dev2DataGrid { ItemsSource = modelItemCollection };

            return dg;
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
