using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using Infragistics.Windows.Controls.Events;

namespace Infragistics.Windows.Ribbon.Events
{

	#region CancelableRoutedToolEventArgs

	/// <summary>
	/// Event arguments for routed events that expose a Tool property that are cancelable
	/// </summary>
	public class CancelableRoutedToolEventArgs : CancelableRoutedEventArgs
	{
		private FrameworkElement _tool;

		/// <summary>
		/// Initializes a new instance of the <see cref="CancelableRoutedToolEventArgs"/> class
		/// </summary>
		/// <param name="tool">The tool instance.</param>
		public CancelableRoutedToolEventArgs(FrameworkElement tool)
		{
			this._tool = tool;
		}

		/// <summary>
		/// Returns the tool instance (read-only)
		/// </summary>
		public FrameworkElement Tool { get { return this._tool; } }

	}

	#endregion //CancelableRoutedToolEventArgs

	#region GalleryItemEventArgs

	/// <summary>
	/// Event arguments for the <see cref="Infragistics.Windows.Ribbon.GalleryTool.ItemActivated"/>, <see cref="Infragistics.Windows.Ribbon.GalleryTool.ItemClicked"/>
	/// and <see cref="Infragistics.Windows.Ribbon.GalleryTool.ItemSelected"/>routed events. 
	/// </summary>
	/// <seealso cref="Infragistics.Windows.Ribbon.GalleryTool.ItemActivated"/>
	/// <seealso cref="Infragistics.Windows.Ribbon.GalleryTool.ItemActivatedEvent"/>
	/// <seealso cref="Infragistics.Windows.Ribbon.GalleryTool.ItemClicked"/>
	/// <seealso cref="Infragistics.Windows.Ribbon.GalleryTool.ItemClickedEvent"/>
	/// <seealso cref="Infragistics.Windows.Ribbon.GalleryTool.ItemSelected"/>
	/// <seealso cref="Infragistics.Windows.Ribbon.GalleryTool.ItemSelectedEvent"/>
	public class GalleryItemEventArgs : RoutedEventArgs
	{
		private GalleryTool											_galleryTool = null;
		private GalleryItemGroup									_galleryItemGroup = null;
		private GalleryItem											_galleryItem = null;

		private GalleryItemPresenter _presenter;

		/// <summary>
		/// Initializes a new instance of the <see cref="GalleryItemEventArgs"/> class
		/// </summary>
		/// <param name="galleryTool">The GalleryTool instance associated with the event.</param>
		/// <param name="galleryItemGroup">The GalleryItemGroup instance associated with the event.</param>
		/// <param name="galleryItem">The GalleryItem instance associated with the event.</param>
		public GalleryItemEventArgs(GalleryTool galleryTool, GalleryItemGroup galleryItemGroup, GalleryItem galleryItem)
		{
			this._galleryTool				= galleryTool;
			this._galleryItemGroup			= galleryItemGroup;
			this._galleryItem				= galleryItem;
		}

		// JJD 12/9/11 - added internal ctor that takes GalleryItemPresenter so we can expose it for TestAdvantage use
		internal GalleryItemEventArgs(GalleryTool galleryTool, GalleryItemGroup galleryItemGroup, GalleryItem galleryItem, GalleryItemPresenter presenter) 
			: this(galleryTool, galleryItemGroup, galleryItem)
		{
			this._presenter = presenter;
		}
		/// <summary>
		/// Returns the <see cref="GalleryTool"/> instance associated with the <see cref="GalleryItem"/> (read-only).
		/// </summary>
		public GalleryTool GalleryTool { get { return this._galleryTool; } }

		/// <summary>
		/// Returns the <see cref="GalleryItemGroup"/> instance associated with the <see cref="GalleryItem"/> (read-only).
		/// </summary>
		public GalleryItemGroup Group { get { return this._galleryItemGroup; } }

		/// <summary>
		/// Returns the <see cref="GalleryItem"/> (read-only).
		/// </summary>
		public GalleryItem Item { get { return this._galleryItem; } }

		// JJD 12/9/11 - added Presenter internal property for TestAdvantage use
		internal GalleryItemPresenter GalleryItemPresenter { get { return _presenter; } }
	}

	#endregion //GalleryItemEventArgs

	#region RibbonGroupOpeningEventArgs

	/// <summary>
	/// Event arguments for <see cref="RibbonGroup.Opening"/> routed event 
	/// </summary>
	/// <seealso cref="RibbonGroup.Opening"/>
	// AS 10/15/07
	// We changed the "Opening" events of the tools to not be cancelable because we are not always in control
	// of the Popup being opened and therefore we cannot support this functionality. The inbox controls don't have
	// this notion either. In any case, to be consistent, I'm changing the ribbon group's opening to also not be
	// cancelable.
	//
	//public class RibbonGroupOpeningEventArgs : CancelableRoutedEventArgs
	public class RibbonGroupOpeningEventArgs : RoutedEventArgs
	{
		#region Member Variables

		private RibbonGroup _group;

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the <see cref="RibbonGroupOpeningEventArgs"/> class
		/// </summary>
		/// <param name="group">The <see cref="RibbonGroup"/> instance.</param>
		public RibbonGroupOpeningEventArgs(RibbonGroup group)
			: base()
		{
			this._group = group;
		} 
		#endregion //Constructor

		#region Properties
		/// <summary>
		/// Returns the associated <see cref="RibbonGroup"/>.
		/// </summary>
		public RibbonGroup Group
		{
			get { return this._group; }
		} 
		#endregion //Properties
	}

	#endregion //RibbonGroupOpeningEventArgs

	// JM 10-24-07
	#region RibbonTabItemOpeningEventArgs

	/// <summary>
	/// Event args for the <see cref="XamRibbon.RibbonTabItemOpening"/> event of the <see cref="XamRibbon"/>
	/// </summary>
	public class RibbonTabItemOpeningEventArgs : RoutedEventArgs
	{
		/// <summary>
		/// Initializes a new <see cref="RibbonTabItemOpeningEventArgs"/>
		/// </summary>
		public RibbonTabItemOpeningEventArgs()
		{
		}
	}

	#endregion //RibbonTabItemOpeningEventArgs

	// JM 10-24-07
	#region RibbonTabItemSelectedEventArgs

	/// <summary>
	/// Event args for the <see cref="XamRibbon.RibbonTabItemOpening"/> event of the <see cref="XamRibbon"/>
	/// </summary>
	public class RibbonTabItemSelectedEventArgs : RoutedEventArgs
	{
		private RibbonTabItem								_previousSelectedTabItem;
		private RibbonTabItem								_newSelectedTabItem;

		/// <summary>
		/// Initializes a new <see cref="RibbonTabItemSelectedEventArgs"/>
		/// </summary>
		public RibbonTabItemSelectedEventArgs(RibbonTabItem previousSelectedTabItem, RibbonTabItem newSelectedTabItem)
		{
			this._previousSelectedTabItem	= previousSelectedTabItem;
			this._newSelectedTabItem		= newSelectedTabItem;
		}

		/// <summary>
		/// Returns the <see cref="RibbonTabItem"/> that was previously selected.
		/// </summary>
		public RibbonTabItem PreviousSelectedRibbonTabItem
		{
			get { return this._previousSelectedTabItem; }
		}

		/// <summary>
		/// Returns the newly selected <see cref="RibbonTabItem"/>.
		/// </summary>
		public RibbonTabItem NewSelectedRibbonTabItem
		{
			get { return this._newSelectedTabItem; }
		}
	}

	#endregion //RibbonTabItemSelectedEventArgs

	#region RoutedToolEventArgs

	/// <summary>
	/// Event arguments for routed events that expose a Tool property
	/// </summary>
	public class RoutedToolEventArgs : RoutedEventArgs
	{
		private FrameworkElement _tool;

		/// <summary>
		/// Initializes a new instance of the <see cref="RoutedToolEventArgs"/> class
		/// </summary>
		/// <param name="tool">The tool instance.</param>
		public RoutedToolEventArgs(FrameworkElement tool)
		{
			this._tool = tool;
		}

		/// <summary>
		/// Returns the tool instance (read-only)
		/// </summary>
		public FrameworkElement Tool { get { return this._tool; } }

	}

	#endregion //RoutedToolEventArgs

	#region ItemActivatedEventArgs

	/// <summary>
	/// Event arguments for <b>ItemActivated</b> routed event 
	/// </summary>
	/// <seealso cref="ButtonTool.Activated"/>
	// AS 10/31/07
	// Changed to internal for now since there is no defined use case and they are not consistently raised.
	//
	//public class ItemActivatedEventArgs : RoutedEventArgs
	internal class ItemActivatedEventArgs : RoutedEventArgs
	{
		private FrameworkElement _item;

		/// <summary>
		/// Initializes a new instance of the <see cref="ItemActivatedEventArgs"/> class
		/// </summary>
		/// <param name="item">The item (e.g. tool) that was activated.</param>
		public ItemActivatedEventArgs(FrameworkElement item)
			: base()
		{
			this._item = item;
		}

		/// <summary>
		/// Returns the item (e.g. tool) that is being activated.
		/// </summary>
		public FrameworkElement Item
		{
			get { return this._item; }
		}
	}

	#endregion //ItemActivatedEventArgs

	#region ToolClonedEventArgs

	/// <summary>
	/// Event arguments for <b>ToolCloned</b> routed event 
	/// </summary>
	/// <seealso cref="ButtonTool.Cloned"/>
	public class ToolClonedEventArgs : RoutedEventArgs
	{
		private FrameworkElement _clonedTool;
		private FrameworkElement _originalTool;
		private bool _eventHandlersAttached;

		/// <summary>
		/// Initializes a new instance of the <see cref="ToolClonedEventArgs"/> class
		/// </summary>
		/// <param name="clonedTool">The cloned tool instance.</param>
		/// <param name="originalTool">The original tool instance.</param>
		/// <param name="eventHandlersAttached">A boolean indicating whether events handlers on the source tool were copied to the clone tool.</param>
		public ToolClonedEventArgs(FrameworkElement clonedTool, FrameworkElement originalTool, bool eventHandlersAttached)
		{
			this._clonedTool = clonedTool;
			this._originalTool = originalTool;
			this._eventHandlersAttached = eventHandlersAttached;
		}

		/// <summary>
		/// Returns the cloned tool instance (read-only)
		/// </summary>
		public FrameworkElement ClonedTool { get { return this._clonedTool; } }

		/// <summary>
		/// Returns the cloned tool instance (read-only)
		/// </summary>
		public FrameworkElement OriginalTool { get { return this._originalTool; } }

		/// <summary>
		/// Returns a boolean indicating if the event handlers for routed events of the <see cref="OriginalTool"/> were associated with the same events for the <see cref="ClonedTool"/>
		/// </summary>
		public bool EventHandlersAttached { get { return this._eventHandlersAttached; } }

	}

	#endregion //ToolClonedEventArgs

	#region ToolCloneDiscardedEventArgs

	/// <summary>
	/// Event arguments for <b>ToolCloneDiscarded</b> routed event 
	/// </summary>
	/// <seealso cref="ButtonTool.CloneDiscarded"/>
	public class ToolCloneDiscardedEventArgs : RoutedEventArgs
	{
		private FrameworkElement _clonedTool;
		private FrameworkElement _originalTool;

		/// <summary>
		/// Initializes a new instance of the <see cref="ToolCloneDiscardedEventArgs"/> class
		/// </summary>
		/// <param name="clonedTool">The cloned tool instance.</param>
		/// <param name="originalTool">The original tool instance.</param>
		public ToolCloneDiscardedEventArgs(FrameworkElement clonedTool, FrameworkElement originalTool)
		{
			this._clonedTool = clonedTool;
			this._originalTool = originalTool;
		}

		/// <summary>
		/// Returns the cloned tool instance (read-only)
		/// </summary>
		public FrameworkElement ClonedTool { get { return this._clonedTool; } }

		/// <summary>
		/// Returns the cloned tool instance (read-only)
		/// </summary>
		public FrameworkElement OriginalTool { get { return this._originalTool; } }

	}

	#endregion //ToolCloneDiscardedEventArgs

	#region ItemDeactivatedEventArgs

	/// <summary>
	/// Event arguments for <b>ItemDeactivated</b> routed event 
	/// </summary>
	/// <seealso cref="ButtonTool.Deactivated"/>
	// AS 10/31/07
	// Changed to internal for now since there is no defined use case and they are not consistently raised.
	//
	//public class ItemDeactivatedEventArgs : RoutedEventArgs
	internal class ItemDeactivatedEventArgs : RoutedEventArgs
	{
		private FrameworkElement _item;

		/// <summary>
		/// Initializes a new instance of the <see cref="ItemDeactivatedEventArgs"/> class
		/// </summary>
		/// <param name="item">The item (e.g. tool) that was deactivated</param>
		public ItemDeactivatedEventArgs(FrameworkElement item)
			: base()
		{
			this._item = item;
		}

		/// <summary>
		/// Returns the item (e.g. tool) that is being activated.
		/// </summary>
		public FrameworkElement Item
		{
			get { return this._item; }
		}
	}

	#endregion //ItemDeactivatedEventArgs

	#region ToolOpeningEventArgs

	/// <summary>
	/// Event arguments for <b>ToolOpening</b> routed event 
	/// </summary>
	/// <seealso cref="MenuToolBase.Opening"/>
	// JJD 10/10/07 - BR26870
	// Prevent the ToolOpeningEventArgs from being cancelable
	//public class ToolOpeningEventArgs : CancelableRoutedToolEventArgs
	public class ToolOpeningEventArgs : RoutedToolEventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ToolOpeningEventArgs"/> class
		/// </summary>
		/// <param name="tool">The tool instance.</param>
		public ToolOpeningEventArgs(FrameworkElement tool)
			: base(tool)
		{
		}

	}

	#endregion //ToolOpeningEventArgs

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