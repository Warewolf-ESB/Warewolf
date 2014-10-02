
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using Dev2.AppResources.Converters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows.Controls;

namespace Dev2.Core.Tests.ConverterTests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    // ReSharper disable InconsistentNaming
    public class GridRowNumberConverterTests
    {
        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("GridRowNumberConverter_Convert")]
        public void GridRowNumberConverter_Convert_WhenInputIsEmpty_ReturnsAZero()
        {
            //------------Setup for test--------------------------
            var converter = new GridRowNumberConverter();
            DataGrid dataGrid = new DataGrid { AutoGenerateColumns = true };
            var itemsSource = new List<string> { "Item 1 ", "Item 2" };
            dataGrid.ItemsSource = itemsSource;
            dataGrid.SelectedItem = itemsSource[0];
            var row = new DataGridRow();
            //------------Execute Test---------------------------
            var result = converter.Convert(row, typeof(string), null, CultureInfo.CurrentCulture);
            //------------Assert Results-------------------------
            Assert.AreEqual(0, result);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("GridRowNumberConverter_Convert")]
        public void GridRowNumberConverter_Convert_WhenInputNotDataGridRow_ReturnsMinus1()
        {
            //------------Setup for test--------------------------
            var converter = new GridRowNumberConverter();
            DataGrid dataGrid = new DataGrid { AutoGenerateColumns = true };
            var itemsSource = new List<string> { "Item 1 ", "Item 2" };
            dataGrid.ItemsSource = itemsSource;
            dataGrid.SelectedItem = itemsSource[0];
            var row = new Object();
            //------------Execute Test---------------------------
            var result = converter.Convert(row, typeof(string), null, CultureInfo.CurrentCulture);
            //------------Assert Results-------------------------
            Assert.AreEqual(-1, result);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("GridRowNumberConverter_Convert")]
        public void GridRowNumberConverter_Convert_WhenValidRow_ReturnsRowIndex()
        {
            var converter = new GridRowNumberConverter();
            DataGrid dataGrid = new DataGrid { AutoGenerateColumns = true };
            var itemsSource = new List<string> { "Item 1 ", "Item 2" };
            dataGrid.ItemsSource = itemsSource;
            dataGrid.SelectedItem = itemsSource[0];
            IItemContainerGenerator generator = dataGrid.ItemContainerGenerator;
            var position = generator.GeneratorPositionFromIndex(0);
            using(generator.StartAt(position, GeneratorDirection.Forward, true))
            {
                foreach(object o in dataGrid.Items)
                {
                    DependencyObject dp = generator.GenerateNext();
                    generator.PrepareItemContainer(dp);
                }
            }
            var row = (DataGridRow)dataGrid.ItemContainerGenerator.ContainerFromItem(dataGrid.SelectedItem);
            //------------Execute Test---------------------------
            var result = converter.Convert(row, typeof(string), null, CultureInfo.CurrentCulture);
            //------------Assert Results-------------------------
            Assert.AreEqual(1, result);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("GridRowNumberConverter_Convert")]
        public void GridRowNumberConverter_Convert_WhenNotFirstOne_ReturnsRowIndex()
        {
            var converter = new GridRowNumberConverter();
            DataGrid dataGrid = new DataGrid { AutoGenerateColumns = true };
            var itemsSource = new List<string> { "Item 1 ", "Item 2", "Item 3" };
            dataGrid.ItemsSource = itemsSource;
            dataGrid.SelectedItem = itemsSource[1];
            IItemContainerGenerator generator = dataGrid.ItemContainerGenerator;
            var position = generator.GeneratorPositionFromIndex(0);
            using(generator.StartAt(position, GeneratorDirection.Forward, true))
            {
                foreach(object o in dataGrid.Items)
                {
                    DependencyObject dp = generator.GenerateNext();
                    generator.PrepareItemContainer(dp);
                }
            }
            var row = (DataGridRow)dataGrid.ItemContainerGenerator.ContainerFromItem(dataGrid.SelectedItem);
            //------------Execute Test---------------------------
            var result = converter.Convert(row, typeof(string), null, CultureInfo.CurrentCulture);
            //------------Assert Results-------------------------
            Assert.AreEqual(2, result);
        }
    }
}
