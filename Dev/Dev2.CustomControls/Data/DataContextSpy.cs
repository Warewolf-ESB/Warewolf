/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


// Copyright (C) Josh Smith - July 2008

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;

namespace WPF.JoshSmith.Data
{
    /// <summary>
    ///     When added to an element's Resources collection it's DataContext property
    ///     references the containing element's DataContext.  This enables DependencyObjects
    ///     not in the element tree to bind to the tree's DataContext.
    /// </summary>
    /// <remarks>
    ///     Documentation: http://www.codeproject.com/KB/WPF/ArtificialInheritanceCxt.aspx
    /// </remarks>
    public class DataContextSpy
        : Freezable // Enable ElementName and DataContext bindings
    {
        /// <summary>
        ///     Represents the DataContext property.
        /// </summary>
        public static readonly DependencyProperty DataContextProperty =
            FrameworkElement.DataContextProperty.AddOwner(
                typeof (DataContextSpy),
                new PropertyMetadata(null, null, OnCoerceDataContext));

        /// <summary>
        ///     Initializes a new instance.
        /// </summary>
        public DataContextSpy()
        {
            // This binding allows the spy to inherit a DataContext.
            BindingOperations.SetBinding(this, DataContextProperty, new Binding());

            IsSynchronizedWithCurrentItem = true;
        }

        /// <summary>
        ///     Gets/sets whether the spy will return the CurrentItem of the
        ///     ICollectionView that wraps the data context, assuming it is
        ///     a collection of some sort.  If the data context is not a
        ///     collection, this property has no effect.
        ///     The default value is true.
        /// </summary>
        public bool IsSynchronizedWithCurrentItem { get; set; }

        /// <summary>
        ///     Gets/sets the DataContext of an element in an element tree.
        ///     This is a dependency property.
        /// </summary>
        public object DataContext
        {
            get { return GetValue(DataContextProperty); }
            set { SetValue(DataContextProperty, value); }
        }

        private static object OnCoerceDataContext(DependencyObject depObj, object value)
        {
            var spy = depObj as DataContextSpy;
            if (spy == null)
                return value;

            if (spy.IsSynchronizedWithCurrentItem)
            {
                ICollectionView view = CollectionViewSource.GetDefaultView(value);
                if (view != null)
                    return view.CurrentItem;
            }

            return value;
        }

        /// <summary>
        ///     Do not invoke.
        /// </summary>
        protected override Freezable CreateInstanceCore()
        {
            //We are required to override this abstract method.
            throw new NotImplementedException();
        }
    }
}