/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


// Copyright (C) Josh Smith - May 2007

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace WPF.JoshSmith.Controls
{

    #region RoutedDataTemplateSelector

    /// <summary>
    ///     A DataTemplateSelector which raises the bubbling TemplateRequested routed event
    ///     on the templated element when a DataTemplate must be chosen.
    /// </summary>
    /// <remarks>
    ///     Documentation: http://www.codeproject.com/KB/WPF/RoutedTemplateSelection.aspx
    /// </remarks>
    public class RoutedDataTemplateSelector : DataTemplateSelector
    {
        #region TemplateRequested [routed event]

        /// <summary>
        ///     Represents the TemplateRequested bubbling routed event.
        /// </summary>
        public static readonly RoutedEvent TemplateRequestedEvent =
            EventManager.RegisterRoutedEvent(
                "TemplateRequested",
                RoutingStrategy.Bubble,
                typeof (TemplateRequestedEventHandler),
                typeof (RoutedDataTemplateSelector));

        // This event declaration is only here so that the compiler allows
        // the TemplateRequested event to be assigned a handler in XAML.
        // Since DataTemplateSelector does not derive from UIElement it 
        // does not have the AddHandler/RemoveHandler methods typically
        // used within an explicit event declaration.
        /// <summary>
        ///     Raised when a DataTemplate is requested on an element.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public event TemplateRequestedEventHandler TemplateRequested
        {
            add { throw new InvalidOperationException("Do not directly hook the TemplateRequested event."); }
            remove { throw new InvalidOperationException("Do not directly unhook the TemplateRequested event."); }
        }

        #endregion // TemplateRequested [routed event]

        #region SelectTemplate [override]

        /// <summary>
        ///     Raises the TemplateRequested event up the 'container' element's logical tree
        ///     so that the DataTemplate to return can be determined.
        /// </summary>
        /// <param name="item">The data object being templated.</param>
        /// <param name="container">The element which contains the data object.</param>
        /// <returns>The DataTemplate to apply.</returns>
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            // We need 'container' to be a UIElement because that class
            // exposes the RaiseEvent method.
            var templatedElement = container as UIElement;
            if (templatedElement == null)
                throw new ArgumentException("RoutedDataTemplateSelector only works with UIElements.");

            // Bubble the TemplateRequested event up the logical tree, starting at the templated element.
            // This allows the outside world to determine what template to use.
            var args = new TemplateRequestedEventArgs(TemplateRequestedEvent, templatedElement, item);
            templatedElement.RaiseEvent(args);

            // Return the DataTemplate selected by the outside world.
            return args.TemplateToUse;
        }

        #endregion // SelectTemplate [override]
    }

    #endregion // RoutedDataTemplateSelector

    #region TemplateRequestedEventArgs

    /// <summary>
    ///     Event argument used by the RoutedDataTemplateSelector's TemplateRequested event.
    /// </summary>
    public class TemplateRequestedEventArgs : RoutedEventArgs
    {
        private readonly object dataObject;

        /// <summary>
        ///     Initializes a new instance.
        /// </summary>
        internal TemplateRequestedEventArgs(RoutedEvent routedEvent, UIElement templatedElement, object dataObject)
            : base(routedEvent, templatedElement)
        {
            this.dataObject = dataObject;
        }

        /// <summary>
        ///     Returns the data item being templated.
        /// </summary>
        public object DataObject
        {
            get { return dataObject; }
        }

        /// <summary>
        ///     Gets/sets the DataTemplate to apply to the templated element.
        /// </summary>
        public DataTemplate TemplateToUse { get; set; }

        /// <summary>
        ///     The UIElement which contains the data object for which a template must be specified.
        /// </summary>
        public UIElement TemplatedElement
        {
            get { return OriginalSource as UIElement; }
        }
    }

    #endregion // TemplateRequestedEventArgs

    #region TemplateRequestedEventHandler

    /// <summary>
    ///     Delegate used to handle the RoutedDataTemplateSelector's TemplateRequested event.
    /// </summary>
    /// <param name="sender">The element on which the event was raised.</param>
    /// <param name="e">Set the SelectedTemplate property to the DataTemplate to apply to the Source element.</param>
    public delegate void TemplateRequestedEventHandler(object sender, TemplateRequestedEventArgs e);

    #endregion // TemplateRequestedEventHandler
}