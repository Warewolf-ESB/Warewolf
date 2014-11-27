
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
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;

// ReSharper disable once CheckNamespace
namespace Unlimited.Framework
{
    public class AutoLayoutGrid : Grid
    {

        public int Columns
        {
            get { return (int)GetValue(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Columns.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColumnsProperty =
            DependencyProperty.Register("Columns", typeof(int), typeof(AutoLayoutGrid), new UIPropertyMetadata(0, OnPropertyChangedCallback));

        public int Rows
        {
            get { return (int)GetValue(RowsProperty); }
            set { SetValue(RowsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Rows.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RowsProperty =
            DependencyProperty.Register("Rows", typeof(int), typeof(AutoLayoutGrid), new UIPropertyMetadata(0, OnPropertyChangedCallback));


        public int CellHeight
        {
            get { return (int)GetValue(CellHeightProperty); }
            set { SetValue(CellHeightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CellHeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CellHeightProperty =
            DependencyProperty.Register("CellHeight", typeof(int), typeof(AutoLayoutGrid), new UIPropertyMetadata(0, OnPropertyChangedCallback));



        public int CellWidth
        {
            get { return (int)GetValue(CellWidthProperty); }
            set { SetValue(CellWidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CellWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CellWidthProperty =
            DependencyProperty.Register("CellWidth", typeof(int), typeof(AutoLayoutGrid), new UIPropertyMetadata(0, OnPropertyChangedCallback));

        private static void OnPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var test = d as AutoLayoutGrid;
            if(test != null)
            {
                test.BindGridStructure();
            }
        }

        public void BindGridStructure()
        {


            RowDefinitions.Clear();
            ColumnDefinitions.Clear();


            Observable.Range(0, Rows)
                .Subscribe(c =>
                {
                    var rowDef = new RowDefinition();
                    if(CellHeight > 0)
                    {
                        rowDef.Height = new GridLength(CellHeight);
                    }
                    RowDefinitions.Add(rowDef);
                });

            Observable.Range(0, Columns)
                .Subscribe(c =>
                {
                    var def = new ColumnDefinition();
                    if(CellWidth > 0)
                    {
                        def.Width = new GridLength(CellWidth);
                    }
                    ColumnDefinitions.Add(def);
                });
        }

        public AutoLayoutGrid()
        {
            RowDefinitions.Clear();
            ColumnDefinitions.Clear();
        }

    }
}
