using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace Infragistics.DragDrop
{
    #region DropTarget class

    /// <summary>
    /// This class is used to manage how element marked as a drop target behaves to.
    /// Element is marked as drop target as <see cref="DragDropManager.DropTargetProperty"/> attached 
    /// property is set to instance of <see cref="DropTarget"/> class.
    /// </summary>
    public class DropTarget : DependencyObject
    {
        #region Members

        private WeakReference _associatedObjectWeakRef;
        private Style _originalTargetStyle;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="DropTarget"/> class.
        /// </summary>
        public DropTarget()
        {
            this.SetValue(DropChannelsProperty, new ObservableCollection<string>());
        }

        #endregion // Constructor

        #region Properties

        #region Public Properties

        #region AssociatedObject

        /// <summary>
        /// Gets the UIElement associated with this <see cref="DropTarget"/> object.
        /// </summary>
        public UIElement AssociatedObject
        {
            get
            {
                if (this._associatedObjectWeakRef == null)
                {
                    return null;
                }

                return this._associatedObjectWeakRef.Target as UIElement;
            }

            internal set
            {
                this._associatedObjectWeakRef = new WeakReference(value);
            }
        }

        #endregion // AssociatedObject

        #region DropChannels
        /// <summary>
        /// Identifies the <see cref="DropChannels"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DropChannelsProperty =
            DependencyProperty.Register(
            "DropChannels",
            typeof(ObservableCollection<string>),
            typeof(DropTarget),
            null);

        /// <summary>
        /// Gets or sets the channels that drop target is listening to.
        /// This is a dependency property.
        /// </summary>
        [TypeConverter(typeof(StringToDragDropChannelsCollectionConverter))]
        public ObservableCollection<string> DropChannels
        {
            get
            {
                return (ObservableCollection<string>)this.GetValue(DropChannelsProperty);
            }

            // altough code analysis warning CA2227 we need this setter
            // because of usage of type converter for setting this property from XAML
            set
            {
                this.SetValue(DropChannelsProperty, value);
            }
        }

        #endregion // DropChannels

        #region DropTargetMarkerBrush

        /// <summary>
        /// Identifies the <see cref="DropTargetMarkerBrush"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DropTargetMarkerBrushProperty =
            DependencyProperty.Register(
            "DropTargetMarkerBrush",
            typeof(Brush),
            typeof(DropTarget),
            new PropertyMetadata(new SolidColorBrush(Colors.Transparent)));

        /// <summary>
        /// Gets or sets the brush used to fill shape that marks the drop target.
        /// The default brush color is Transparent.
        /// This property is relevant just when <see cref="DropTargetStyle"/> is null.
        /// This is a dependency property.
        /// </summary>
        public Brush DropTargetMarkerBrush
        {
            get
            {
                return (Brush)GetValue(DropTargetMarkerBrushProperty);
            }

            set
            {
                SetValue(DropTargetMarkerBrushProperty, value);
            }
        }

        #endregion // DropTargetMarkerColor

        #region DropTargetStyle
        /// <summary>
        /// Identifies the <see cref="DropTargetStyle"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DropTargetStyleProperty =
            DependencyProperty.Register(
            "DropTargetStyle",
            typeof(Style),
            typeof(DropTarget),
            null);

        /// <summary>
        /// Gets or sets style used by the drop target when dragged item is over it.
        /// This is a dependency property.
        /// </summary>
        public Style DropTargetStyle
        {
            get
            {
                return (Style)GetValue(DropTargetStyleProperty);
            }

            set
            {
                SetValue(DropTargetStyleProperty, value);
            }
        }

        #endregion // DropTargetStyle

        #region IsDropTarget
        /// <summary>
        /// Identifies the <see cref="IsDropTarget"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsDropTargetProperty =
            DependencyProperty.Register(
            "IsDropTarget",
            typeof(bool),
            typeof(DropTarget),
            new PropertyMetadata(new PropertyChangedCallback(OnDropTargetChanged)));

        private static void OnDropTargetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DropTarget dt = d as DropTarget;

            if (dt != null && dt.AssociatedObject != null)
            {
                if ((bool)e.NewValue)
                {
                    DragDropManager.RegisterDropTarget(dt.AssociatedObject);
                }
                else
                {
                    DragDropManager.UnregisterDropTarget(dt.AssociatedObject);
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether marked element can be drop target. This is a dependency property.
        /// </summary>
        public bool IsDropTarget
        {
            get
            {
                return (bool)GetValue(IsDropTargetProperty);
            }

            set
            {
                SetValue(IsDropTargetProperty, value);
            }
        }

        #endregion // IsDropTarget

        #region HighlightOnDragStart

        /// <summary>
        /// Gets or sets a value indicating whether drop target element should be highlighted when 
        /// drag source element with appropriate <see cref="DragSource.DragChannels"/> is dragged.
        /// </summary>
        public bool HighlightOnDragStart
        {
            get; set;
        }

        #endregion // HighlightOnDragStart

        #endregion // Public Properties

        #endregion // Properties

        #region Methods

        /// <summary>
        /// Highlights the drop target as apply <see cref="DropTargetStyle"/> to the <see cref="AssociatedObject"/>.
        /// </summary>
        /// <param name="highlight">if set to <c>true</c> the specified style is set to the drop target element, otherwise the style is reverted to its original value.</param>
        /// <returns><c>true</c> when the drop target element can be highlighted.</returns>
        public bool HighlightDropTarget(bool highlight)
        {
            FrameworkElement frameworkElement = this.AssociatedObject as FrameworkElement;
            if (frameworkElement == null)
            {
                return false;
            }

            if (highlight)
            {
                if (this.DropTargetStyle == null || frameworkElement.Style == this.DropTargetStyle)
                {
                    return false;
                }

                this._originalTargetStyle = frameworkElement.Style;
                frameworkElement.Style = this.DropTargetStyle;
            }
            else
            {
                if (this._originalTargetStyle == null)
                {
                    return false;
                }

                frameworkElement.Style = this._originalTargetStyle;
                this._originalTargetStyle = null;
            }

            return true;
        }

        #endregion // Methods
    }

    #endregion // DropTarget class
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