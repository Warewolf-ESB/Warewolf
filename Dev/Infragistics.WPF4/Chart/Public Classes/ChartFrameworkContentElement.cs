
#region Using
using System;
using System.Windows;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
#endregion Using

namespace Infragistics.Windows.Chart
{
    /// <summary>
    /// This class hides public properties from FrameworkContentElement. FrameworkContentElement is the 
    /// Windows Presentation Foundation framework-level implementation of the ContentElement base class. 
    /// FrameworkContentElement adds support for additional input APIs (including tooltips and context menus), 
    /// animation, data context for databinding, and logical tree helper APIs. 
    /// </summary>
    public class ChartFrameworkContentElement : FrameworkContentElement
    {
        private const string MouseMoveEventName = "MouseMove";

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether this element 
        /// can be used as the target of a drag-and-drop operation. 
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new bool AllowDrop
        {
            get
            {
                return base.AllowDrop;
            }
            set
            {
                base.AllowDrop = value;
            }
        }

        /// <summary>
        /// Gets or sets the context menu element that should appear 
        /// whenever the context menu is requested through user 
        /// interface (UI) from within this element. 
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new ContextMenu ContextMenu
        {
            get
            {
                return base.ContextMenu;
            }
            set
            {
                base.ContextMenu = value;
            }
        }

        /// <summary>
        /// Gets or sets an arbitrary object value that can be used to 
        /// store custom information about this element. 
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Object Tag
        {
            get
            {
                return base.Tag;
            }
            set
            {
                base.Tag = value;
            }
        }

        /// <summary>
        /// Gets or sets the locally-defined resource dictionary.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new ResourceDictionary Resources
        {
            get
            {
                return base.Resources;
            }
        }

        /// <summary>
        /// Gets or sets the data context for an element when it participates in data binding.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Object DataContext
        {
            get
            {
                return base.DataContext;
            }
            set
            {
                base.DataContext = value;
            }
        }

        /// <summary>
        /// Gets or sets the context for input used by this FrameworkElement. 
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new InputScope InputScope
        {
            get
            {
                return base.InputScope;
            }
            set
            {
                base.InputScope = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this element is enabled in the user interface (UI).
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new bool IsEnabled
        {
            get
            {
                return base.IsEnabled;
            }
            set
            {
                base.IsEnabled = value;
            }
        }

        /// <summary>
        /// Gets or sets localization/globalization language information that applies to an individual element. 
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new XmlLanguage Language
        {
            get
            {
                return base.Language;
            }
            set
            {
                base.Language = value;
            }
        }

        /// <summary>
        /// Gets a collection of CommandBinding objects that are associated with this element.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new CommandBindingCollection CommandBindings
        {
            get
            {
                return base.CommandBindings;
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether this element incorporates style 
        /// properties from theme styles.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new bool OverridesDefaultStyle
        {
            get
            {
                return base.OverridesDefaultStyle;
            }
            set
            {
                base.OverridesDefaultStyle = value;
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether this FrameworkElement should force 
        /// the user interface (UI) to render the cursor as declared by the Cursor property. 
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new bool ForceCursor
        {
            get
            {
                return base.ForceCursor;
            }
            set
            {
                base.ForceCursor = value;
            }
        }

        /// <summary>
        /// Gets or sets a property that enables customization of appearance, 
        /// effects, or other style characteristics that will apply to this 
        /// element when it captures keyboard focus. 
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Style FocusVisualStyle
        {
            get
            {
                return base.FocusVisualStyle;
            }
            set
            {
                base.FocusVisualStyle = value;
            }
        }

        /// <summary>
        /// Gets or sets the cursor that displays when the mouse pointer is over this element.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Cursor Cursor
        {
            get
            {
                return base.Cursor;
            }
            set
            {
                base.Cursor = value;
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the element can receive focus. 
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new bool Focusable
        {
            get
            {
                return base.Focusable;
            }
            set
            {
                base.Focusable = value;
            }
        }

        #region ToolTip

        /// <summary>
        /// Gets or sets the tool-tip object that is displayed for this element in the user interface (UI). 
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Object ToolTip
        {
            get
            {
                return base.ToolTip;
            }
            set
            {
                base.ToolTip = value;
            }
        }

        #endregion ToolTip


        #endregion Properties

        #region Methods

        /// <summary>
        /// Initializes a new instance of the ChartFrameworkContentElement class. 
        /// </summary>
        public ChartFrameworkContentElement()
        {
        }

        /// <summary>
        /// Raises a specific routed event. The RoutedEvent to be raised is identified 
        /// within the RoutedEventArgs instance that is provided (as the  RoutedEvent 
        /// property of that event data). 
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">A RoutedEventArgs that contains the event data and also identifies the event to raise.</param>
        internal void RaiseEvent(object sender, RoutedEventArgs e)
        {   
            //[PK: bugs 19457 and 35837] raise the event only once
            this.RaiseEvent(e);

            //[PK: bug 58315] we should not stop the MouseMove event as it's used for crosshairsf
            if ((e.RoutedEvent.RoutingStrategy == RoutingStrategy.Bubble ||
                e.RoutedEvent.RoutingStrategy == RoutingStrategy.Tunnel) && 
                e.RoutedEvent.Name != MouseMoveEventName)
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// Adds the provided element as a child of this element. 
        /// </summary>
        /// <param name="child">Child element to be added.</param>
        protected void AddChild(object child)
        {
            if (child == null)
            {
                return;
            }

            if (child is FrameworkContentElement)
            {
                DependencyObject parent = ((FrameworkContentElement)child).Parent;
                if (parent != null)
                {
                    return;
                }
            }
            else if (child is FrameworkElement)
            {
                DependencyObject parent = ((FrameworkElement)child).Parent;
                if (parent != null)
                {
                    return;
                }
            }

            base.AddLogicalChild(child);
        }

        #endregion Methods
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