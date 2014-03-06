using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace Infragistics.DragDrop
{
    #region DragDropEventArgs class
    /// <summary>
    /// This class is a base class for all event arguments for events raised by <see cref="DragSource"/> objects.
    /// </summary>
    public class DragDropEventArgs : EventArgs
    {
        #region Properties

        #region Public Properties

        #region CopyCursorTemplate
        /// <summary>
        /// Gets or sets the data template that will be used as cursor 
        /// when coping operation is performed during dragging.
        /// </summary>
        public DataTemplate CopyCursorTemplate
        {
            get;
            set;
        }
        #endregion // CopyCursorTemplate

        #region Data
        /// <summary>
        /// Gets or sets the instance of the object that represents the dragged data.
        /// </summary>
        public object Data
        {
            get;
            set;
        }

        #endregion // Data

        #region DragSource
        /// <summary>
        /// Gets the UIElement that initiates drag operation.
        /// </summary>
        public UIElement DragSource
        {
            get;
            internal set;
        }
        #endregion // DragSource

        #region DragTemplate
        /// <summary>
        /// Gets or sets the data template that will be applied to dragged element representation.
        /// </summary>
        public DataTemplate DragTemplate
        {
            get;
            set;
        }

        #endregion // DragTemplate

        #region DropNotAllowedCursorTemplate
        /// <summary>
        /// Gets or sets the data template that will be used as cursor 
        /// when drag operation is performed but drop dragged item is not over appropriate target.
        /// </summary>
        public DataTemplate DropNotAllowedCursorTemplate
        {
            get;
            set;
        }
        #endregion // DropNotAllowedCursorTemplate

        #region DropTarget

        /// <summary>
        /// Gets the instance of the object marked as drop target.
        /// </summary>
        public UIElement DropTarget
        {
            get;
            internal set;
        }

        #endregion // DropTarget

        #region MoveCursorTemplate
        /// <summary>
        /// Gets or sets the data template that will be used as cursor 
        /// when item is draged over appropriate drop target.
        /// </summary>
        public DataTemplate MoveCursorTemplate
        {
            get; set;
        }
        #endregion // MoveCursorTemplate

        #region OperationType

        /// <summary>
        /// Gets or sets the type of intended action during drag-and-drop operation.
        /// Setting this property will enforce the applying of related cursor.
        /// </summary>
        public OperationType? OperationType
        {
            get; set;
        }

        #endregion // OperationType

        #region OriginalDragSource
        /// <summary>
        /// Gets the original reporting source as determined by pure hit testing. 
        /// </summary>
        public UIElement OriginalDragSource
        {
            get; internal set;
        }

        #endregion // OriginalDragSource

        #region CustomMouseHolder
        /// <summary>
        /// Gets or sets the <see cref="UIElement"/> suggested by consumer of the event as the element which has to capture the mouse during the drag-drop.
        /// </summary>
        public UIElement CustomMouseHolder
        {
            get;
            set;
        }

        #endregion // CustomMouseHolder

        #endregion // Public Properties

        #endregion // Properties
    }

    #endregion // DragDropEventArgs class

    #region DragDropMoveEventArgs class
    /// <summary>
    /// Event argumens for <see cref="DragSource.DragOver"/> event of the <see cref="DragSource"/>.
    /// </summary>
    public class DragDropMoveEventArgs : DragDropEventArgs
    {
        #region Members

        private MouseEventArgs _mouseEventArgs;

        #endregion // Members

        #region Constructors

        internal DragDropMoveEventArgs(DragDropEventArgs baseArgs, MouseEventArgs mouseEventArgs)
        {
            this.CopyCursorTemplate = baseArgs.CopyCursorTemplate;
            this.Data = baseArgs.Data;
            this.DragTemplate = baseArgs.DragTemplate;
            this.DragSource = baseArgs.DragSource;
            this.OriginalDragSource = baseArgs.OriginalDragSource;            
            this.MoveCursorTemplate = baseArgs.MoveCursorTemplate;
            this.DropNotAllowedCursorTemplate = baseArgs.DropNotAllowedCursorTemplate;
            this.OperationType = baseArgs.OperationType;

            this._mouseEventArgs = mouseEventArgs;
        }

        #endregion // Constructors

        #region Methods

        #region Public Methods

        #region GetPosition

        /// <summary>
        /// Returns mouse pointer position relative to supplied UIElement-derived object.
        /// </summary>
        /// <param name="relativeTo">Any UIElement-derived object connected to the object tree.</param>
        /// <returns>The x- and y-coordinates of the mouse pointer position relative to the specified object.</returns>
        public Point GetPosition(UIElement relativeTo)
        {
            if (this._mouseEventArgs != null)
            {
                return this._mouseEventArgs.GetPosition(relativeTo);
            }

            return new Point();
        }

        #endregion // GetPosition

        #endregion // Public Methods

        #endregion // Methods
    }

    #endregion // DragDropMoveEventArgs class

    #region DragDropCancelEventArgs class
    /// <summary>
    /// Class for event argumens for <see cref="DragSource.DragEnter"/> cancelable event of the <see cref="DragSource"/>.
    /// </summary>
    public class DragDropCancelEventArgs : DragDropEventArgs
    {
        #region Constructors

        internal DragDropCancelEventArgs(DragDropEventArgs baseArgs)
        {
            this.CopyCursorTemplate = baseArgs.CopyCursorTemplate;
            this.Data = baseArgs.Data;
            this.DragTemplate = baseArgs.DragTemplate;
            this.DragSource = baseArgs.DragSource;
            this.OriginalDragSource = baseArgs.OriginalDragSource;
            this.MoveCursorTemplate = baseArgs.MoveCursorTemplate;
            this.DropNotAllowedCursorTemplate = baseArgs.DropNotAllowedCursorTemplate;
            this.OriginalDragSource = baseArgs.OriginalDragSource;
            this.OperationType = baseArgs.OperationType;

            this.Cancel = false;
        }

        #endregion // Constructors

        #region Properties

        #region Public Properties

        #region Cancel

        /// <summary>
        /// Gets or sets a value indicating whether drag-and-drop operation should be canceled.
        /// </summary>
        public bool Cancel
        {
            get;
            set;
        }

        #endregion // Cancel        

        #endregion // Public Properties

        #endregion // Properties
    }

    #endregion // DragDropCancelEventArgs class

    #region DragStartEventArgs class

    /// <summary>
    /// Class for event arguments for <see cref="DragSource.DragStart"/> cancelable event of the <see cref="DragSource"/>.
    /// </summary>
    public class DragDropStartEventArgs : DragDropCancelEventArgs
    {
        #region Constructors

        internal DragDropStartEventArgs(DragDropEventArgs baseArgs) :
            base(baseArgs)
        {
        }

        #endregion // Constructors

        #region Properties
        /// <summary>
        /// Gets or sets the UI element that will be dragged.
        /// </summary>
        public UIElement DragSnapshotElement
        {
            get; set;
        }

        #endregion // Properties
    }

    #endregion // DragStartEventArgs class

    #region DropEventArgs class
    /// <summary>
    /// Event arguments for <see cref="DragSource.Drop"/> event.
    /// </summary>
    public class DropEventArgs : DragDropMoveEventArgs
    {
        #region Constructors

        internal DropEventArgs(DragDropEventArgs baseArgs, MouseEventArgs mouseEventArgs) : 
            base(baseArgs, mouseEventArgs)
            {
            }

        #endregion // Constructors

        #region Properties

        #region Public Properties

        #region DropTargetElements
        /// <summary>
        /// Gets the collection of visual childs of drop target over which the dragged item is dropped.
        /// </summary>
        public ReadOnlyCollection<UIElement> DropTargetElements
        {
            get;
            internal set;
        }

        #endregion // DropTargetElements

        #endregion // Public Properties

        #endregion // Properties
    }
    #endregion // DropEventArgs
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