
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
using System.Windows.Data;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Dock
{
    /// <summary>
    /// Class that provides information about a binding.
    /// </summary>
    public class ItemBinding
    {
        #region Properties
        /// <summary>
        /// Returns or sets the binding to the underlying item that will be set as the binding for the <see cref="TargetProperty"/>
        /// </summary>
        public Binding Binding
        {
            get;
            set;
        }

        /// <summary>
        /// Returns or sets the base type for the item to which the TargetProperty will be bound.
        /// </summary>
        /// <remarks>
        /// <para>This property defaults to null which means that it can be applied to any item. This property 
        /// is intended to be used in a situation where the source collection can items of different types and 
        /// the binding only applies to items of a given source type.</para>
        /// </remarks>
        public Type SourceType
        {
            get;
            set;
        }

        /// <summary>
        /// Returns or sets the base type for the container on which this binding may be applied.
        /// </summary>
        public Type TargetContainerType
        {
            get;
            set;
        }

        /// <summary>
        /// Returns or sets the property that will be set to the specified <see cref="Binding"/>
        /// </summary>
        public DependencyProperty TargetProperty
        {
            get;
            set;
        }
        #endregion //Properties

        #region Methods
        internal bool CanApply(DependencyObject container, object item)
        {
            if(TargetProperty == null || Binding == null)
                return false;

            if(container != null && TargetContainerType != null && !TargetContainerType.IsAssignableFrom(container.GetType()))
                return false;

            if(item != null && SourceType != null && !SourceType.IsAssignableFrom(item.GetType()))
                return false;

            return true;
        }
        #endregion //Methods
    }
}
