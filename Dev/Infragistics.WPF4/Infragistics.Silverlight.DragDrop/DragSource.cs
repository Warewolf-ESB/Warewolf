using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;

namespace Infragistics.DragDrop
{
    #region DragSource class
    /// <summary>
    /// This class is used to manage how element marked as a drag source behaves to.
    /// Element is marked as drag source as <see cref="DragDropManager.DragSourceProperty"/> attached 
    /// property is set to instance of <see cref="DragSource"/> class.
    /// </summary>
    public class DragSource : DependencyObject
    {
        #region Members

        private WeakReference _associatedObjectWeakRef;
        private double _draggingOffset = 2;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="DragSource"/> class.
        /// </summary>
        public DragSource()
        {
            this.SetValue(DragChannelsProperty, new ObservableCollection<string>());
        }

        #endregion

        #region Properties

        #region Public Properties

        #region AssociatedObject
        
        /// <summary>
        /// Gets the UIElement associated with this <see cref="DragSource"/> object.
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

        #region CopyCursorTemplate
        /// <summary>
        /// Identifies the <see cref="CopyCursorTemplate"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CopyCursorTemplateProperty =
            DependencyProperty.Register(
            "CopyCursorTemplate", 
            typeof(DataTemplate), 
            typeof(DragSource), 
            null);

        /// <summary>
        /// Gets or sets the data template used as a cursor while copy operation is performed during drag-and-drop operation.
        /// This is a dependency property.
        /// </summary>
        public DataTemplate CopyCursorTemplate
        {
            get
            {
                return (DataTemplate)GetValue(CopyCursorTemplateProperty);
            }

            set
            {
                SetValue(CopyCursorTemplateProperty, value);
            }
        }

        #endregion // CopyCursorTemplate

        #region DataObject

        /// <summary>
        /// Identifies the <see cref="DataObject"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DataObjectProperty =
            DependencyProperty.Register("DataObject", typeof(object), typeof(DragSource), null);

        /// <summary>
        /// Gets or sets the object that hold the meaningful for the drag-and-drop operation data.
        /// </summary>
        public object DataObject
        {
            get
            {
                return GetValue(DataObjectProperty);
            }

            set
            {
                SetValue(DataObjectProperty, value);
            }
        }

        #endregion // DataObject

        #region DragChannels

        /// <summary>
        /// Identifies the <see cref="DragChannels"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DragChannelsProperty =
            DependencyProperty.Register(
            "DragChannels",
            typeof(ObservableCollection<string>),
            typeof(DragSource),
            null);

        /// <summary>
        /// Gets or sets the channels that object can be dragged into.
        /// This is a dependency property.
        /// </summary>
        [TypeConverter(typeof(StringToDragDropChannelsCollectionConverter))]
        public ObservableCollection<string> DragChannels
        {
            get
            {
                return (ObservableCollection<string>)this.GetValue(DragChannelsProperty);
            }

            // altough code analysis warning CA2227 we need this setter
            // because of usage of type converter for setting this property from XAML
            set
            {
                this.SetValue(DragChannelsProperty, value);
            }
        }

        #endregion // DragChannels

        #region DraggingOffset

        /// <summary>
        /// Gets or sets the drag mouse offset after which dragging is initiated.
        /// The default and minimal value is 2 pixels.
        /// </summary>
        public double DraggingOffset
        {
            get
            {
                return this._draggingOffset;
            }

            set
            {
                this._draggingOffset = value > 2 ? value : 2;
            }
        }
        #endregion // DraggingOffset

        #region DragTemplate
        /// <summary>
        /// Identifies the <see cref="DragTemplate"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DragTemplateProperty =
            DependencyProperty.Register(
            "DragTemplate", 
            typeof(DataTemplate), 
            typeof(DragSource), 
            null);

        /// <summary>
        /// Gets or sets data template used by dragged element while drag-and-drop operation is performed. 
        /// This is a dependency property.
        /// </summary>
        public DataTemplate DragTemplate
        {
            get
            {
                return (DataTemplate)GetValue(DragTemplateProperty);
            }

            set
            {
                SetValue(DragTemplateProperty, value);
            }
        }
        #endregion // DragTemplate

        #region DropNotAllowedCursorTemplate
        /// <summary>
        /// Identifies the <see cref="DropNotAllowedCursorTemplate"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DropNotAllowedCursorTemplateProperty =
            DependencyProperty.Register(
            "DropNotAllowedCursorTemplate", 
            typeof(DataTemplate), 
            typeof(DragSource), 
            null);

        /// <summary>
        /// Gets or sets the data template used as a cursor if there drop target is not found 
        /// during drag-and-drop operation. This is a dependency property.
        /// </summary>
        public DataTemplate DropNotAllowedCursorTemplate
        {
            get
            {
                return (DataTemplate)GetValue(DropNotAllowedCursorTemplateProperty);
            }

            set
            {
                SetValue(DropNotAllowedCursorTemplateProperty, value);
            }
        }

        #endregion // DropNotAllowedCursorTemplate

        #region FindDropTargetMode

        /// <summary>
        /// Gets or sets the find drop target mode.
        /// </summary>
        public FindDropTargetMode FindDropTargetMode
        {
            get; set;
        }

        #endregion // FindDropTargetMode

        #region IsDraggable
        /// <summary>
        /// Identifies the <see cref="IsDraggable"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsDraggableProperty =
            DependencyProperty.Register(
            "IsDraggable",
            typeof(bool),
            typeof(DragSource),
            new PropertyMetadata(new PropertyChangedCallback(OnDraggableChanged)));

        private static void OnDraggableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DragSource dragSource = d as DragSource;
            if (dragSource != null && dragSource.AssociatedObject != null)
            {
                if ((bool)e.NewValue)
                {
                    DragDropManager.RegisterDragSource(dragSource.AssociatedObject);
                }
                else
                {
                    DragDropManager.UnregisterDragSource(dragSource.AssociatedObject);
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether marked element can be dragged. This is a dependency property.
        /// </summary>
        public bool IsDraggable
        {
            get
            {
                return (bool)GetValue(IsDraggableProperty);
            }

            set
            {
                SetValue(IsDraggableProperty, value);
            }
        }

        #endregion // IsDraggable

        #region MoveCursorTemplate
        /// <summary>
        /// Identifies the <see cref="MoveCursorTemplate"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MoveCursorTemplateProperty =
            DependencyProperty.Register(
            "MoveCursorTemplate", 
            typeof(DataTemplate), 
            typeof(DragSource), 
            null);

        /// <summary>
        /// Gets or sets the data template used as a cursor during regular drag-and-drop operation. 
        /// This is a dependency property.
        /// </summary>
        public DataTemplate MoveCursorTemplate
        {
            get
            {
                return (DataTemplate)GetValue(MoveCursorTemplateProperty);
            }

            set
            {
                SetValue(MoveCursorTemplateProperty, value);
            }
        }

        #endregion // MoveCursorTemplate        

        #endregion // Public Properties                

        #endregion // Class Properties

        #region Events

        #region DragCancel
        /// <summary>
        /// Occurs when drag-and-drop operation is canceled when <see cref="DragDropCancelEventArgs.Cancel"/> is set to <b>true</b>
        /// in some of event handlers for <see cref="DragStart"/> or <see cref="DragEnter"/> events
        /// or when <see cref="DragDropManager.EndDrag"/> method is called with <b>dragCancel</b> set to <b>true</b>.
        /// </summary>
        public event EventHandler<DragDropEventArgs> DragCancel;



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

        internal void OnDragCancel(DragDropEventArgs args)
        {
            if (this.DragCancel != null)
            {
                this.DragCancel(this, args);
            }
        }

        #endregion // DragCancel

        #region DragEnd
        /// <summary>
        /// Occurs at the very end of the drag-and-drop operation. This is the last event that is raised while drag-and-drop is performed.
        /// </summary>
        public event EventHandler<DragDropEventArgs> DragEnd;



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

        internal void OnDragEnd(DragDropEventArgs args)
        {
            if (this.DragEnd != null)
            {
                this.DragEnd(this, args);
            }
        }

        #endregion // DragEnd

        #region DragEnter
        /// <summary>
        /// Occurs when mouse pointer enter into UIElement marked as drop target while drag-and-drop is in progress.
        /// This event is cancelable.
        /// </summary>
        public event EventHandler<DragDropCancelEventArgs> DragEnter;



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

        internal void OnDragEnter(DragDropCancelEventArgs args)
        {
            if (this.DragEnter != null)
            {
                this.DragEnter(this, args);
            }
        }

        #endregion // DragEnter

        #region DragLeave
        /// <summary>
        /// Occurs when mouse pointer leaves the boundaries of the UIElement marked as drop target while drag-and-drop operation is in progress.
        /// </summary>
        public event EventHandler<DragDropEventArgs> DragLeave;



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

        internal void OnDragLeave(DragDropEventArgs args)
        {
            if (this.DragLeave != null)
            {
                this.DragLeave(this, args);
            }
        }

        #endregion // DragLeave

        #region DragOver
        /// <summary>
        /// Occurs when mouse pointer is moving over UIElement marked as drop target while drag-and-drop operation is in progress.
        /// This event occurs after <see cref="DragEnter"/> event is raised.
        /// </summary>
        public event EventHandler<DragDropMoveEventArgs> DragOver;



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

        internal void OnDragOver(DragDropMoveEventArgs args)
        {
            if (this.DragOver != null)
            {
                this.DragOver(this, args);
            }
        }

        #endregion // DragOver

        #region DragStart
        /// <summary>
        /// Occurs when UIElement marked as draggable initiate drag operation.
        /// This event is cancelable.
        /// </summary>
        public event EventHandler<DragDropStartEventArgs> DragStart;



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

        internal void OnDragStart(DragDropStartEventArgs args)
        {
            if (this.DragStart != null)
            {
                this.DragStart(this, args);
            }
        }

        #endregion // DragStart

        #region Drop
        /// <summary>
        /// Occurs when mouse is released while drag-and-drop operation is in progress, 
        /// mouse pointer is over UIElement marked as drop target and drag channels of the
        /// drag source matches the drop channels of the drop target.
        /// </summary>
        public event EventHandler<DropEventArgs> Drop;



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

        internal void OnDrop(DropEventArgs args)
        {
            if (this.Drop != null)
            {
                this.Drop(this, args);
            }
        }

        #endregion // Drop

        #endregion // Events  
    }

    #endregion // DragSource class

    #region BindingContext class

    /// <summary>
    /// This class is used to make possible <see cref="DragSource.DataObject"/> to be bound.
    /// </summary>
    internal class BindingContext : FrameworkElement
    { 
    }

    #endregion // BindingContext class
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