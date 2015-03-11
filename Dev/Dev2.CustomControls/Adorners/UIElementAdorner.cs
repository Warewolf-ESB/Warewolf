/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


// Copyright (C) Josh Smith - February 2007

using System;
using System.Collections;
using System.Windows;

namespace WPF.JoshSmith.Adorners
{
    /// <summary>
    ///     An adorner which can display one and only one UIElement.  The element
    ///     is added to the adorner's visual and logical trees, enabling it to
    ///     particpate in dependency property value inheritance, amongst other things.
    /// </summary>
    /// <remarks>
    ///     Used In: http://www.codeproject.com/KB/WPF/SmartTextBox.aspx
    /// </remarks>
    public class UIElementAdorner : SingleChildAdornerBase
    {
        #region Constructor

        /// <summary>
        ///     Constructor.  Adds 'childElement' to the adorner's visual and logical trees.
        /// </summary>
        /// <param name="adornedElement">The element to which the adorner will be bound.</param>
        /// <param name="childElement">The element to be displayed in the adorner.</param>
        public UIElementAdorner(UIElement adornedElement, UIElement childElement)
            : base(adornedElement)
        {
            if (childElement == null)
                throw new ArgumentNullException("childElement");

            child = childElement;
            AddLogicalChild(childElement);
            AddVisualChild(childElement);
        }

        #endregion // Constructor

        #region Base Class Overrides

        /// <summary>
        ///     Override.
        /// </summary>
        protected override IEnumerator LogicalChildren
        {
            get
            {
                var list = new ArrayList {child};
                return list.GetEnumerator();
            }
        }

        #endregion // Base Class Overrides
    }
}