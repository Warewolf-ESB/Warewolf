using System;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Diagnostics;
using System.Windows.Data;
using Infragistics.Controls.Menus;
using Infragistics.Controls.Menus.Primitives;


namespace Infragistics
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1064:ExceptionsShouldBePublic")]
    internal interface IHeaderedItemContainerGenerator
    {
        Panel ItemsHost
        {
            get;
        }
        ScrollViewer ScrollHost
        {
            get;
        }
        void OnApplyTemplate();
        void PrepareContainerForItemOverride(DependencyObject element, object item, Style parentItemContainerStyle);
        void ScrollIntoView(FrameworkElement element);
        void UpdateItemContainerStyle(Style itemContainerStyle);
        void ClearContainerForItemOverride(DependencyObject element, object item);
    }

    /// <summary>
    /// The HeaderedItemContainerGenerator provides useful utilities for mapping between
    /// the items of an XamHeaderedItemsControl and their generated containers.
    /// </summary>
    internal sealed class HeaderedItemContainerGenerator : IHeaderedItemContainerGenerator
    {
        #region Member variables

        /// <summary>
        /// Gets or sets the ItemsControl being tracked by the
        /// HeaderedItemContainerGenerator.
        /// </summary>
        private ItemsControl _itemsControl { get; set; }

        /// <summary>
        /// A Panel that is used as the ItemsHost of the ItemsControl.  This
        /// property will only be valid when the ItemsControl is live in the
        /// tree and has generated containers for some of its items.
        /// </summary>
        private Panel _itemsHost;

        /// <summary>
        /// A ScrollViewer that is used to scroll the items in the ItemsHost.
        /// </summary>
        private ScrollViewer _scrollHost;

        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the HeaderedItemContainerGenerator.
        /// </summary>
        /// <param name="control">
        /// The ItemsControl being tracked by the HeaderedItemContainerGenerator.
        /// </param>
        internal HeaderedItemContainerGenerator(ItemsControl control)
        {
            Debug.Assert(control != null, "control cannot be null!");
            _itemsControl = control;
            //_containersToItems = control.ItemContainerGenerator;
        }
        #endregion

        #region Properties

            #region Internal

                #region ItemsHost
        /// <summary>
        /// Gets a Panel that is used as the ItemsHost of the ItemsControl.
        /// This property will only be valid when the ItemsControl is live in
        /// the tree and has generated containers for some of its items.
        /// </summary>
        public Panel ItemsHost
        {
            get
            {
                // Lookup the ItemsHost if we haven't already cached it.
                if (_itemsHost == null)
                {
                    // Get any live container
                    if (_itemsControl.Items.Count <= 0)
                    {
                        return null;
                    }
                    DependencyObject container = _itemsControl.ItemContainerGenerator.ContainerFromIndex(0);
                    if (container == null)
                        return null;
                    // Get the parent of the container
                    _itemsHost = VisualTreeHelper.GetParent(container) as Panel;
                }

                return _itemsHost;
            }
        }
        #endregion

                #region ScrollHost
        /// <summary>
        /// Gets a ScrollViewer that is used to scroll the items in the
        /// ItemsHost.
        /// </summary>
        public ScrollViewer ScrollHost
        {
            get
            {
                if (_scrollHost == null)
                {
                    Panel itemsHost = ItemsHost;
                    if (itemsHost != null)
                    {
                        for (DependencyObject obj = itemsHost; obj != _itemsControl && obj != null; obj = VisualTreeHelper.GetParent(obj))
                        {
                            ScrollViewer viewer = obj as ScrollViewer;
                            if (viewer != null)
                            {
                                _scrollHost = viewer;
                                break;
                            }
                        }
                    }
                }
                return _scrollHost;
            }
        }
        #endregion

            #endregion
        #endregion

        #region Methods

            #region Internal

                #region OnApplyTemplate
        /// <summary>
        /// Apply a control template to the ItemsControl.
        /// </summary>
        public void OnApplyTemplate()
        {
            // Clear the cached ItemsHost, ScrollHost
            _itemsHost = null;
            _scrollHost = null;
        }
        #endregion

                #region PrepareContainer
        /// <summary>
        /// Prepares the specified container to display the specified item.
        /// </summary>
        /// <param name="element">
        /// Container element used to display the specified item.
        /// </param>
        /// <param name="item">Specified item to display.</param>
        /// <param name="parentItemContainerStyle">
        /// The ItemContainerStyle for the parent ItemsControl.
        /// </param>
        public void PrepareContainerForItemOverride(DependencyObject element, object item, Style parentItemContainerStyle)
        {
             // Apply the ItemContainerStyle to the item
            Control control = element as Control;
            if (parentItemContainerStyle != null && control != null && control.Style == null)
            {
                control.SetValue(Control.StyleProperty, parentItemContainerStyle);
            }

            XamHeaderedItemsControl headeredItemsControl = element as XamHeaderedItemsControl;
            if (headeredItemsControl != null)
            {
                Infragistics.Controls.HierarchicalDataTemplate hierarchicalItemTemplate = null;

                XamHeaderedItemsControl headeredParentControl = _itemsControl as XamHeaderedItemsControl;
                if (headeredParentControl != null)
                {
                    // Get HierarchicalDataTemplate from parent control
                    hierarchicalItemTemplate = headeredParentControl.HierarchicalItemTemplate;

                    if (headeredItemsControl.DefaultItemsContainer == null && headeredParentControl.DefaultItemsContainer != null)
                    {
                        headeredItemsControl.SetValue(XamHeaderedItemsControl.DefaultItemsContainerProperty,
                                                      headeredItemsControl.DefaultItemsContainer);
                    }
                }

                PrepareHeaderedItemsControlContainer(headeredItemsControl, item, _itemsControl,
                    parentItemContainerStyle, hierarchicalItemTemplate);
            }
        }

        /// <summary>
        /// Check whether a control has the default value for a property.
        /// </summary>
        /// <param name="control">The control to check.</param>
        /// <param name="property">The property to check.</param>
        /// <returns>
        /// True if the property has the default value; false otherwise.
        /// </returns>
        private static bool HasDefaultValue(Control control, DependencyProperty property)
        {
            Debug.Assert(control != null, "control should not be null!");
            Debug.Assert(property != null, "property should not be null!");
            return control.ReadLocalValue(property) == DependencyProperty.UnsetValue;
        }

        /// <summary>
        /// Prepare a PrepareHeaderedItemsControlContainer container for an
        /// item.
        /// </summary>
        /// <param name="control">Container to prepare.</param>
        /// <param name="item">Item to be placed in the container.</param>
        /// <param name="parentItemsControl">The parent ItemsControl.</param>
        /// <param name="parentItemContainerStyle"> The ItemContainerStyle for the parent ItemsControl. </param>
        /// <param name="hierarchicalTemplate">The HierarchicalTemplate for container</param>
        private static void PrepareHeaderedItemsControlContainer(XamHeaderedItemsControl control, object item, ItemsControl parentItemsControl, Style parentItemContainerStyle, Infragistics.Controls.HierarchicalDataTemplate hierarchicalTemplate)
        {
            if (control != item)
            {
                // Copy the ItemsControl properties from parent to child
                DataTemplate parentItemTemplate = parentItemsControl.ItemTemplate;
                if (parentItemTemplate != null)
                {
                    control.SetValue(XamHeaderedItemsControl.ItemTemplateProperty, parentItemTemplate);
                }
                if (parentItemContainerStyle != null && HasDefaultValue(control, XamHeaderedItemsControl.ItemContainerStyleProperty))
                {
                    control.SetValue(XamHeaderedItemsControl.ItemContainerStyleProperty, parentItemContainerStyle);
                }

                // Copy the Header properties from parent to child
                if (control.HeaderIsItem || HasDefaultValue(control, XamHeaderedItemsControl.HeaderProperty))
                {
                    if (!string.IsNullOrEmpty(control.DisplayMemberPath))
                    {
                        Binding b = new Binding(control.DisplayMemberPath);
                        b.Source = item;
                        b.Mode = BindingMode.OneWay;
                        control.SetBinding(XamHeaderedItemsControl.HeaderProperty, b);
                    }
                    else if (!string.IsNullOrEmpty(parentItemsControl.DisplayMemberPath))
                    {
                        Binding b = new Binding(parentItemsControl.DisplayMemberPath);
                        b.Source = item;
                        b.Mode = BindingMode.OneWay;
                        control.SetBinding(XamHeaderedItemsControl.HeaderProperty, b);
                    }
                    else
                    {
                        control.Header = item;// new Button() { Content = "lll" };
                    }
                    control.HeaderIsItem = true;
                }
                if (parentItemTemplate != null)
                {
                    control.SetValue(XamHeaderedItemsControl.HeaderTemplateProperty, parentItemTemplate);
                }
                if (parentItemContainerStyle != null && control.Style == null)
                {
                    control.SetValue(XamHeaderedItemsControl.StyleProperty, parentItemContainerStyle);
                }

                // Setup a hierarchical template
                Infragistics.Controls.HierarchicalDataTemplate headerTemplate = hierarchicalTemplate;
                if (headerTemplate != null)
                {
                    if (headerTemplate.ItemsSource != null && HasDefaultValue(control, XamHeaderedItemsControl.ItemsSourceProperty))
                    {
                        control.SetBinding(
                            XamHeaderedItemsControl.ItemsSourceProperty,
                            new Binding
                            {
                                Converter = headerTemplate.ItemsSource.Converter,
                                ConverterCulture = headerTemplate.ItemsSource.ConverterCulture,
                                ConverterParameter = headerTemplate.ItemsSource.ConverterParameter,
                                Mode = headerTemplate.ItemsSource.Mode,
                                NotifyOnValidationError = headerTemplate.ItemsSource.NotifyOnValidationError,
                                Path = headerTemplate.ItemsSource.Path,
                                Source = control.Header,
                                ValidatesOnExceptions = headerTemplate.ItemsSource.ValidatesOnExceptions,
                            });
                    }

                    if (headerTemplate.HierarchicalItemTemplate != null)
                    {
                        control.SetValue(XamHeaderedItemsControl.HierarchicalItemTemplateProperty, headerTemplate.HierarchicalItemTemplate);
                    }
                    else
                    {
                        control.SetValue(XamHeaderedItemsControl.HierarchicalItemTemplateProperty, headerTemplate);
                    }

                    if (headerTemplate.ItemTemplate != null && control.ItemTemplate == parentItemTemplate)
                    {
                        control.ClearValue(XamHeaderedItemsControl.ItemTemplateProperty);
                        if (headerTemplate.ItemTemplate != null)
                        {
                            control.ItemTemplate = headerTemplate.ItemTemplate;
                        }
                    }

                    if (headerTemplate.ItemContainerStyle != null && control.ItemContainerStyle == parentItemContainerStyle)
                    {
                        control.ClearValue(XamHeaderedItemsControl.ItemContainerStyleProperty);
                        if (headerTemplate.ItemContainerStyle != null)
                        {
                            control.ItemContainerStyle = headerTemplate.ItemContainerStyle;
                        }
                    }

                    if (control.DefaultItemsContainer == null && headerTemplate.DefaultItemsContainer != null)
                    {
                        control.SetValue(XamHeaderedItemsControl.DefaultItemsContainerProperty, headerTemplate.DefaultItemsContainer);
                    }
                }
            }
        }

        #endregion PrepareContainer

                #region ClearContainerForItemOverride
        /// <summary>
        /// Undoes the effects of PrepareContainerForItemOverride.
        /// </summary>
        /// <param name="element">The container element.</param>
        /// <param name="item">The contained item.</param>
        public void ClearContainerForItemOverride(DependencyObject element, object item)
        {
            //ContainersToItems.Remove(element);
        }
        #endregion

                #region UpdateItemContainerStyle
        /// <summary>
        /// Update the style of any generated items when the ItemContainerStyle
        /// has been changed.
        /// </summary>
        /// <param name="itemContainerStyle">The ItemContainerStyle.</param>
        public void UpdateItemContainerStyle(Style itemContainerStyle)
        {
            if (itemContainerStyle == null)
            {
                return;
            }

            Panel itemsHost = ItemsHost;
            if (itemsHost == null || itemsHost.Children == null)
            {
                return;
            }

            foreach (UIElement element in itemsHost.Children)
            {
                FrameworkElement obj = element as FrameworkElement;
                obj.Style = itemContainerStyle;
            }
        }
        #endregion

                #region void ScrollIntoView(FrameworkElement element)
        /// <summary>
        /// Scroll the desired element into the ScrollHost's viewport.
        /// </summary>
        /// <param name="element">Element to scroll into view.</param>
        public void ScrollIntoView(FrameworkElement element)
        {
            // Get the ScrollHost
            ScrollViewer scrollHost = ScrollHost;
            if (scrollHost == null)
            {
                return;
            }

            // Get the position of the element relative to the ScrollHost
            GeneralTransform transform = null;
            try
            {
                transform = element.TransformToVisual(scrollHost);
            }



            
            catch (InvalidOperationException)

            {
                // Ignore failures when not in the visual tree
                return;
            }
            Rect itemRect = new Rect(
                transform.Transform(new Point()),
                transform.Transform(new Point(element.ActualWidth, element.ActualHeight)));

            // Scroll vertically
            double verticalOffset = scrollHost.VerticalOffset;
            double verticalDelta = 0;
            double hostBottom = scrollHost.ViewportHeight;
            double itemBottom = itemRect.Bottom;
            if (hostBottom < itemBottom)
            {
                verticalDelta = itemBottom - hostBottom;
                verticalOffset += verticalDelta;
            }
            double itemTop = itemRect.Top;
            if (itemTop - verticalDelta < 0)
            {
                verticalOffset -= verticalDelta - itemTop;
            }
            scrollHost.ScrollToVerticalOffset(verticalOffset);

            // Scroll horizontally
            double horizontalOffset = scrollHost.HorizontalOffset;
            double horizontalDelta = 0;
            double hostRight = scrollHost.ViewportWidth;
            double itemRight = itemRect.Right;
            if (hostRight < itemRight)
            {
                //horizontalDelta = itemBottom - hostBottom;
                horizontalDelta = itemRight - hostRight;
                horizontalOffset += horizontalDelta;
            }
            double itemLeft = itemRect.Left;
            if (itemLeft - horizontalDelta < 0)
            {
                horizontalOffset -= horizontalDelta - itemLeft;
            }
            scrollHost.ScrollToHorizontalOffset(horizontalOffset);
        }
        #endregion

            #endregion

        #endregion
    }

}
#region Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved
/* ---------------------------------------------------------------------*
*                           Infragistics, Inc.                          *
*              Copyright (c) 2001-2012 All Rights reserved               *
*                                                                       *
*                                                                       *
* This file and its contents are protected by United States and         *
* International copyright laws.  Unauthorized reproduction and/or       *
* distribution of all or any portion of the code contained herein       *
* is strictly prohibited and will result in severe civil and criminal   *
* penalties.  Any violations of this copyright will be prosecuted       *
* to the fullest extent possible under law.                             *
*                                                                       *
* THE SOURCE CODE CONTAINED HEREIN AND IN RELATED FILES IS PROVIDED     *
* TO THE REGISTERED DEVELOPER FOR THE PURPOSES OF EDUCATION AND         *
* TROUBLESHOOTING. UNDER NO CIRCUMSTANCES MAY ANY PORTION OF THE SOURCE *
* CODE BE DISTRIBUTED, DISCLOSED OR OTHERWISE MADE AVAILABLE TO ANY     *
* THIRD PARTY WITHOUT THE EXPRESS WRITTEN CONSENT OF INFRAGISTICS, INC. *
*                                                                       *
* UNDER NO CIRCUMSTANCES MAY THE SOURCE CODE BE USED IN WHOLE OR IN     *
* PART, AS THE BASIS FOR CREATING A PRODUCT THAT PROVIDES THE SAME, OR  *
* SUBSTANTIALLY THE SAME, FUNCTIONALITY AS ANY INFRAGISTICS PRODUCT.    *
*                                                                       *
* THE REGISTERED DEVELOPER ACKNOWLEDGES THAT THIS SOURCE CODE           *
* CONTAINS VALUABLE AND PROPRIETARY TRADE SECRETS OF INFRAGISTICS,      *
* INC.  THE REGISTERED DEVELOPER AGREES TO EXPEND EVERY EFFORT TO       *
* INSURE ITS CONFIDENTIALITY.                                           *
*                                                                       *
* THE END USER LICENSE AGREEMENT (EULA) ACCOMPANYING THE PRODUCT        *
* PERMITS THE REGISTERED DEVELOPER TO REDISTRIBUTE THE PRODUCT IN       *
* EXECUTABLE FORM ONLY IN SUPPORT OF APPLICATIONS WRITTEN USING         *
* THE PRODUCT.  IT DOES NOT PROVIDE ANY RIGHTS REGARDING THE            *
* SOURCE CODE CONTAINED HEREIN.                                         *
*                                                                       *
* THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.              *
* --------------------------------------------------------------------- *
*/
#endregion Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved