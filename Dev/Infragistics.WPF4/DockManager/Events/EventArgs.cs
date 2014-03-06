using System;
using System.Collections.Generic;
using System.Text;
using Infragistics.Windows.Controls.Events;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using Infragistics.Windows.DockManager.Dragging;
using System.Windows.Media;
using Infragistics.Shared;

namespace Infragistics.Windows.DockManager.Events
{
	// AS 9/29/09 NA 2010.1 - FilesMenuOpening
	#region FilesMenuOpeningEventArgs
	/// <summary>
	/// Event arguments for the <see cref="TabGroupPane.FilesMenuOpening"/>
	/// </summary>
	public class FilesMenuOpeningEventArgs : RoutedEventArgs
	{
		private ItemCollection _items;

		/// <summary>
		/// Initializes a new <see cref="FilesMenuOpeningEventArgs"/>
		/// </summary>
		/// <param name="items">Modifiable items collection that represents the menu items for the pane.</param>
		public FilesMenuOpeningEventArgs(ItemCollection items)
		{
			DockManagerUtilities.ThrowIfNull(items, "items");

			this._items = items;
		}

		/// <summary>
		/// Returns a modifiable collection of MenuItems that represents the list of files presented to the end user for the pane.
		/// </summary>
		public ItemCollection Items
		{
			get { return this._items; }
		}
	} 
	#endregion //FilesMenuOpeningEventArgs

	#region InitializePaneContentEventArgs

	/// <summary>
	/// Event arguments for routed event <see cref="XamDockManager.InitializePaneContent"/>
	/// </summary>
	/// <seealso cref="XamDockManager.InitializePaneContent"/>
	/// <seealso cref="XamDockManager.InitializePaneContentEvent"/>
	public class InitializePaneContentEventArgs : RoutedEventArgs
	{
		#region Member Variables

		private ContentPane _pane;

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new InitializePaneContentEventArgs
		/// </summary>
		public InitializePaneContentEventArgs(ContentPane newPane)
		{
			DockManagerUtilities.ThrowIfNull(newPane, "newPane");

			this._pane = newPane;
		}
		#endregion //Constructor

		#region NewPane
		/// <summary>
		/// Returns or sets the new <see cref="ContentPane"/> representing a pane in the layout being loaded that does not currently exist within the <see cref="XamDockManager"/>.
		/// </summary>
        /// <remarks>
        /// <p class="body">By default this property is set to a new <see cref="ContentPane"/> whose 
        /// <see cref="ContentPane.SerializationId"/> and <see cref="FrameworkElement.Name"/> have been 
        /// initialized with the information from the layout being loaded. In order to use the pane in the 
        /// layout, you must set the <see cref="ContentControl.Content"/> property to a value or set the 
        /// NewPane to a new ContentPan instance. The latter may be used to provide a derived ContentPane 
        /// if needed.</p>
        /// <p class="note"><b>Note:</b> If you set the NewPane to a new ContentPane, you must initialize the 
        /// <b>Name</b> and <b>SerializationId</b> if you need these properties. If you leave the Name unset 
        /// and you try to save the layout, an exception will be generated because all panes whose 
        /// <see cref="ContentPane.SaveInLayout"/> is true, which is the default value, must have a unique 
        /// name.</p>
        /// </remarks>
		public ContentPane NewPane
		{
			get { return this._pane; }
            // AS 10/17/08 TFS8130
            // Allow the NewPane to be set in case the customer is using a derived pane.
            //
            set { this._pane = value; }
        } 
		#endregion //NewPane
	}

	#endregion //InitializePaneContentEventArgs

	#region PaneOptionsMenuOpeningEventArgs
	/// <summary>
	/// Event arguments for the <see cref="ContentPane.OptionsMenuOpening"/>
	/// </summary>
	public class PaneOptionsMenuOpeningEventArgs : RoutedEventArgs
	{
		private ItemCollection _items;

		/// <summary>
		/// Initializes a new <see cref="PaneOptionsMenuOpeningEventArgs"/>
		/// </summary>
		/// <param name="items">Modifiable items collection that represents the menu items for the pane.</param>
		public PaneOptionsMenuOpeningEventArgs(ItemCollection items)
		{
			DockManagerUtilities.ThrowIfNull(items, "items");

			this._items = items;
		}

		/// <summary>
		/// Returns a modifiable collection of MenuItems that represents the options presented to the end user for the pane.
		/// </summary>
		public ItemCollection Items
		{
			get { return this._items; }
		}
	} 
	#endregion //PaneOptionsMenuOpeningEventArgs

	#region PaneClosingEventArgs

	/// <summary>
	/// Event arguments for routed event <see cref="ContentPane.Closing"/>
	/// </summary>
	/// <seealso cref="ContentPane.Closing"/>
	/// <seealso cref="ContentPane.ClosingEvent"/>
	public class PaneClosingEventArgs : CancelableRoutedEventArgs
	{
		/// <summary>
		/// Initializes a new <see cref="PaneClosingEventArgs"/>
		/// </summary>
		public PaneClosingEventArgs()
		{
		}
	}

	#endregion //PaneClosingEventArgs

	#region PaneClosedEventArgs

	/// <summary>
	/// Event arguments for routed event <see cref="ContentPane.Closed"/>
	/// </summary>
	/// <seealso cref="ContentPane.Closed"/>
	/// <seealso cref="ContentPane.ClosedEvent"/>
	public class PaneClosedEventArgs : RoutedEventArgs
	{

		/// <summary>
		/// Initializes a new <see cref="PaneClosedEventArgs"/>
		/// </summary>
		public PaneClosedEventArgs()
		{
		}
	}

	#endregion //PaneClosedEventArgs

	#region PaneToolWindowEventArgs
	/// <summary>
	/// Event arguments for the <see cref="XamDockManager.ToolWindowLoaded"/> and <see cref="XamDockManager.ToolWindowUnloaded"/> events
	/// </summary>
	/// <seealso cref="XamDockManager.ToolWindowLoaded"/>
	/// <seealso cref="XamDockManager.ToolWindowUnloaded"/>
	public class PaneToolWindowEventArgs : RoutedEventArgs
	{
		#region Member Variables

		private PaneToolWindow _window;

		#endregion //Member Variables

		/// <summary>
		/// Initializes a new <see cref="PaneToolWindowEventArgs"/>
		/// </summary>
		/// <param name="window"></param>
		public PaneToolWindowEventArgs(PaneToolWindow window)
		{
			DockManagerUtilities.ThrowIfNull(window, "window");

			this._window = window;
		}

		/// <summary>
		/// Returns the <see cref="PaneToolWindow"/> associated with the event.
		/// </summary>
		public PaneToolWindow Window
		{
			get { return this._window; }
		}
	} 
	#endregion //PaneToolWindowEventArgs

	#region PaneDragStartingEventArgs
	/// <summary>
	/// Event arguments for the <see cref="XamDockManager.PaneDragStarting"/> event
	/// </summary>
	/// <seealso cref="XamDockManager.PaneDragStarting"/>
	public class PaneDragStartingEventArgs : CancelableRoutedEventArgs
	{
		#region Member Variables

		private FrameworkElement _rootPane;
		private FrameworkElement _rootContainer;
		private ReadOnlyCollection<ContentPane> _panes;

		// AS 5/28/08 RaisePaneDragOverForInvalidLocations
		private bool _raisePaneDragOverForInvalidLocations;
		private Cursor _invalidDragActionCursor;
		private Cursor _validDragActionCursor;

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="PaneDragStartingEventArgs"/>
		/// </summary>
		/// <param name="rootPane">The element being dragged</param>
		public PaneDragStartingEventArgs(FrameworkElement rootPane)
		{
			DockManagerUtilities.ThrowIfNull(rootPane, "rootPane");

			IList<ContentPane> panes = DragManager.GetPanesForDragElement(rootPane, out this._rootContainer);

			this._rootPane = rootPane;
			this._panes = new ReadOnlyCollection<ContentPane>(panes);
		}
		#endregion //Constructor

		#region Properties

		#region Public Properties

		// AS 5/28/08 RaisePaneDragOverForInvalidLocations
		/// <summary>
		/// The cursor that should be displayed by default when the mouse is over an invalid drop location during the drag operation.
		/// </summary>
		public Cursor InvalidDragActionCursor
		{
			get { return this._invalidDragActionCursor; }
			set { this._invalidDragActionCursor = value; }
		}

		/// <summary>
		/// Returns the collection of panes being dragged.
		/// </summary>
		public ReadOnlyCollection<ContentPane> Panes
		{
			get { return this._panes; }
		}

		// AS 5/28/08 RaisePaneDragOverForInvalidLocations
		/// <summary>
		/// Indicates if the <see cref="XamDockManager.PaneDragOver"/> event should be raised even when the mouse is moved over an invalid pane location based on the properties of the panes being dragged.
		/// </summary>
		public bool RaisePaneDragOverForInvalidLocations 
		{
			get { return this._raisePaneDragOverForInvalidLocations; }
			set { this._raisePaneDragOverForInvalidLocations = value; } 
		}

		/// <summary>
		/// Returns the root element that is being dragged.
		/// </summary>
		public FrameworkElement RootPane
		{
			get { return this._rootPane; }
		}

		// AS 5/28/08 RaisePaneDragOverForInvalidLocations
		/// <summary>
		/// The cursor that should be displayed by default when the mouse is over a valid drop location during the drag operation.
		/// </summary>
		public Cursor ValidDragActionCursor
		{
			get { return this._validDragActionCursor; }
			set { this._validDragActionCursor = value; }
		}
		#endregion //Public Properties

		#region Internal Properties

		internal FrameworkElement RootContainerElement
		{
			get { return this._rootContainer; }
		}

		#endregion //Internal Properties

		#endregion //Properties
	} 
	#endregion //PaneDragStartingEventArgs

	#region PaneDragEndedEventArgs
	/// <summary>
	/// Event arguments for the <see cref="XamDockManager.PaneDragEnded"/> event
	/// </summary>
	/// <seealso cref="XamDockManager.PaneDragEnded"/>
	public class PaneDragEndedEventArgs : RoutedEventArgs
	{
		#region Member Variables

		private ReadOnlyCollection<ContentPane> _panes;

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="PaneDragEndedEventArgs"/>
		/// </summary>
		/// <param name="panes">The collection of panes that were repositioned</param>
		public PaneDragEndedEventArgs(ReadOnlyCollection<ContentPane> panes)
		{
			DockManagerUtilities.ThrowIfNull(panes, "panes");

			this._panes = panes;
		}
		#endregion //Constructor

		#region Properties
		/// <summary>
		/// Returns the collection of panes that were dragged.
		/// </summary>
		public ReadOnlyCollection<ContentPane> Panes
		{
			get { return this._panes; }
		}
		#endregion //Properties
	} 
	#endregion //PaneDragEndedEventArgs

	#region PaneDragOverEventArgs
	/// <summary>
	/// Event arguments for the <see cref="XamDockManager.PaneDragOver"/> event
	/// </summary>
	/// <seealso cref="XamDockManager.PaneDragOver"/>
	public class PaneDragOverEventArgs : RoutedEventArgs
	{
		#region Member Variables

		private bool _isValidDragAction = true;
		private ReadOnlyCollection<ContentPane> _panes;
		private Cursor _cursor;
		private PaneDragAction _dragAction;

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="PaneDragOverEventArgs"/>
		/// </summary>
		/// <param name="panes">The panes being dragged</param>
		/// <param name="dragAction">The type of action that will result</param>
		public PaneDragOverEventArgs(ReadOnlyCollection<ContentPane> panes, PaneDragAction dragAction)
		{
			DockManagerUtilities.ThrowIfNull(panes, "panes");
			DockManagerUtilities.ThrowIfNull(dragAction, "dragAction");

			this._panes = panes;
			this._dragAction = dragAction;
		}
		#endregion //Constructor

		#region Properties
		/// <summary>
		/// Returns or sets a boolean indicating whether the specified drag action is valid.
		/// </summary>
		public bool IsValidDragAction
		{
			get { return this._isValidDragAction; }
			set { this._isValidDragAction = value; }
		}

		/// <summary>
		/// Returns or sets the cursor that should be shown.
		/// </summary>
		public Cursor Cursor
		{
			get { return this._cursor; }
			set { this._cursor = value; }
		}

		/// <summary>
		/// Returns a class that provides information about the operation being performed. This must be upcast to the appropriate type to obtain specific information about the operation.
		/// </summary>
		public PaneDragAction DragAction
		{
			get { return this._dragAction; }
		}

		/// <summary>
		/// Returns the collection of panes that were dragged.
		/// </summary>
		public ReadOnlyCollection<ContentPane> Panes
		{
			get { return this._panes; }
		}
		#endregion //Properties
	} 
	#endregion //PaneDragOverEventArgs
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