using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Shapes;
using System.Windows.Navigation;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Automation;
using System.Windows.Automation.Provider;
using System.Windows.Threading;
using System.Diagnostics;
//using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Xml;
using System.Text;
using System.Windows.Media.Imaging;
using Infragistics.Controls.Layouts;
using Infragistics.Controls.Primitives;




namespace Infragistics.Controls.Layouts.Primitives
{
	#region DropLocation Enum






	internal enum DropLocation
	{





		None = 0,



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		OverTarget = 5
	}

	#endregion // DropLocation Enum

	#region TileDragManager Class

	/// <summary>
	/// Class with the logic for starting and managing dragging and dropping of a tile.
	/// </summary>
	internal class TileDragManager
	{
		#region Nested Data Structures

		#region DropInfo Class

		/// <summary>
		/// Contains information about a specific drop. It contains info on the new layout.
		/// </summary>
		private class DropInfo
		{
			#region Member Vars

			private TileDragManager _dragManager;
			private TileLayoutManager.LayoutItem _dropTargetItem;
			private DropLocation _dropLocation;
			private RectInfo _dropTargetRect;

			private bool _isProcessed;
			private bool _isDropValid;
			private bool _isDropNOOP;
			private bool _isDragAreaInvalid;
			#endregion // Member Vars

			#region Constructor

			/// <summary>
			/// Constructs an invalid drop info. Used for figuring out the drag indicator location.
			/// </summary>
			/// <param name="dragManager"></param>
			/// <param name="isDragAreaInvalid"></param>
			internal DropInfo( TileDragManager dragManager, bool isDragAreaInvalid )
			{
				_isDragAreaInvalid = isDragAreaInvalid;
				this.Initialize( dragManager, DropLocation.None, null, null );
			}

			internal DropInfo( TileDragManager dragManager, TileLayoutManager.LayoutItem dropTargetItem, bool isDropNOOP) :
                this(dragManager, new RectInfo(dragManager, dropTargetItem.TargetRect, dragManager.Panel), DropLocation.OverTarget, isDropNOOP)
            {
				_isDragAreaInvalid = false;
				_isDropValid = true;
                _dropTargetItem = dropTargetItem;
			}

			internal DropInfo(TileDragManager dragManager, RectInfo targetRect, DropLocation dropLocation, bool isDropNOOP)
			{
				_dragManager					= dragManager;
				_dropTargetRect					= targetRect;
				_dropLocation					= dropLocation;
				_isDropNOOP						= isDropNOOP;
			}

			#endregion // Constructor

			#region Methods

			#region Private/Internal Methods

			#region ApplyDrop

			internal bool ApplyDrop( )
			{
				this.EnsureProcessed( );

                if (this._dropTargetItem == null)
                    return false;

                if (this._dragManager._isSwapping &&
                    !this._dragManager._canSwapTarget)
                    return false;

                return _dragManager._tileManager.MoveTileHelper( _dragManager._layoutItem, _dropTargetItem, true);
			}

			#endregion // ApplyDrop

			#region EnsureProcessed

			private void EnsureProcessed( )
			{
				if ( !_isProcessed )
				{
					this.ProcessHelper( );
					_isProcessed = true;
				}
			}

			#endregion // EnsureProcessed

			#region Initialize

			private void Initialize( 
				TileDragManager dragManager,
				DropLocation dropLocation,
				TileLayoutManager.LayoutItem dropTargetItem,
				RectInfo dropTargetRect )
			{
				_dragManager = dragManager;
				_dropLocation = dropLocation;
				_dropTargetItem = dropTargetItem;
				_dropTargetRect = dropTargetRect;
			}

			#endregion // Initialize

			#region ProcessHelper

			private void ProcessHelper( )
			{
			}

			#endregion // ProcessHelper

			#endregion // Private/Internal Methods

			#endregion // Methods

			#region Properties

			#region Private/Internal Properties

            #region DropTargetItem
    
            internal TileLayoutManager.LayoutItem DropTargetItem{ get { return this._dropTargetItem; } }

   	        #endregion //DropTargetItem	
    
			#region IsDragAreaInvalid

			/// <summary>
			/// Indicates if the item is currently being dragged outside of valid drop area.
			/// For example, if the item is 
			/// </summary>
			internal bool IsDragAreaInvalid
			{
				get
				{
					return _isDragAreaInvalid;
				}
			}

			#endregion // IsDragAreaInvalid

			#region IsDropNOOP

			internal bool IsDropNOOP
			{
				get
				{
					this.EnsureProcessed( );

					return _isDropNOOP;
				}
			}

			#endregion // IsDropNOOP

			#region IsDropValid

			internal bool IsDropValid
			{
				get
				{
					this.EnsureProcessed( );

					return _isDropValid;
				}
			}

			#endregion // IsDropValid

			#endregion // Private/Internal Properties

			#endregion // Properties
		}

		#endregion // DropInfo Class

		#region RectInfo Class

		/// <summary>
		/// Contains rect information and the element its relative to.
		/// </summary>
		internal class RectInfo
		{
			private TileDragManager _dragManager;
			private Rect _rect;
			private FrameworkElement _relativeTo;

			/// <summary>
			/// Constructor
			/// </summary>
			/// <param name="dragManager">Drag manager</param>
			/// <param name="rect">Rectangle</param>
			/// <param name="relativeTo">This is the element that the specified rectangle is relative to. If null then
			/// the rect is taken to be relative to the screen.</param>
			internal RectInfo( TileDragManager dragManager, Rect rect, FrameworkElement relativeTo )
			{
				CoreUtilities.ValidateNotNull( dragManager );

				_dragManager = dragManager;
				_rect = rect;
				_relativeTo = relativeTo;
			}

			internal Rect GetRectInScreen( )
			{
				// Pass in null which means convert to screen coordinates.
				// 
				return this.GetRect( null );
			}

			/// <summary>
			/// Gets the rect relative to data presenter.
			/// </summary>
			/// <returns>Rect relative to data presenter.</returns>
			internal Rect GetRect( )
			{
				return this.GetRect( _dragManager.Panel );
			}

			/// <summary>
			/// Gets the rect relative to specified element. If null then gets the rect in screen coordinates.
			/// </summary>
			/// <param name="relativeTo"></param>
			/// <returns></returns>
			internal Rect GetRect( UIElement relativeTo )
			{
                Point topLeft = PointInfo.TranslatePointHelper(new Point(_rect.Top, _rect.Left), _relativeTo, relativeTo);
 
				return new Rect(topLeft.X, topLeft.Y, _rect.Width, _rect.Height);
             }

			/// <summary>
			/// Checks to see if the rect contains specified point.
			/// </summary>
			/// <param name="point">Point to check.</param>
			/// <returns>True if the rect contains the point, false otherwise.</returns>
			internal bool Contains( PointInfo point )
			{
				Rect r = this.GetRect( );
				Point p = point.GetPosition( );

				return r.Contains( p );
			}
		}

		#endregion // RectInfo Class

		#region PointInfo Class

		/// <summary>
		/// Contains point info and which element its relative to.
		/// </summary>
		internal class PointInfo
		{
			#region Private Members

			private TileDragManager _dragManager;
			private Point _point;
			private FrameworkElement _relativeTo;

			#endregion //Private Members	
    
			#region Constructor

			internal PointInfo(TileDragManager dragManager, Point point, FrameworkElement relativeTo)
			{
				CoreUtilities.ValidateNotNull(dragManager);

				_dragManager = dragManager;
				_point = point;
				_relativeTo = relativeTo;
			}

			#endregion //Constructor	
    
			#region Properties

			internal FrameworkElement RelativeTo { get { return _relativeTo; } }

			#endregion //Properties	
    
			#region Methods

			#region GetPosition...

			/// <summary>
			/// Returns the mouse position relative to data presenter.
			/// </summary>
			/// <returns></returns>
			internal Point GetPosition()
			{
				return this.GetPosition(_dragManager.Panel);
			}

			internal Point GetPositionInScreen()
			{
				return this.GetPosition(null);
			}

			internal Point GetPosition(UIElement relativeTo)
			{
				return TranslatePointHelper(_point, _relativeTo, relativeTo);
			}

			#endregion //GetPosition...	
    
			#region IsInsideTilesPanel

			internal bool IsInsideTilesPanel()
			{
				return TileUtilities.DoesElementContainPoint(_dragManager.Panel, this.GetPosition());
			}

			#endregion //IsInsideTilesPanel	
    
			#region TranslatePointHelper

			internal static Point TranslatePointHelper(Point p, UIElement currentRelativeTo, UIElement relativeTo)
			{
				// If relativeTo parameter is null then return rect in screen coordinates.
				// 
				if (currentRelativeTo != relativeTo)
				{



					if (null == relativeTo)
					{

						p = Infragistics.Windows.Utilities.PointToScreenSafe(currentRelativeTo, p);





					}
					else
					{

						if (null != currentRelativeTo)
							p = currentRelativeTo.TranslatePoint(p, relativeTo);
						else // Convert from screen to element.
							p = Infragistics.Windows.Utilities.PointFromScreenSafe(relativeTo, p);
					}


#region Infragistics Source Cleanup (Region)









#endregion // Infragistics Source Cleanup (Region)


				}

				return p;
			}

			#endregion //TranslatePointHelper	
    
			#endregion //Methods
		}

		#endregion // PointInfo Class

		#endregion // Nested Data Structures

		#region Member Vars

		private TileLayoutManager _tileManager;
        private TileLayoutManager.LayoutItem _layoutItem;
		private TileAreaPanel _panel;
		private FrameworkElement _tile;
		private ItemTileInfo _itemInfo;
		private FrameworkElement _dragElement;

		private PopupDragManager _dragIndicatorPopupMgr;
		private Point _dragIndicatorWindow_OffsetFromMouse;
        private TileLayoutManager.LayoutItem _lastSwapTarget;
        private bool _canSwapTarget;
        private bool _isSwapping;

		// JJD 9/12/11 - TFS87312
		// Listen for an unload of the panel during a drag operation
		private bool _panelWasUnloaded;

		private bool _dragIndicatorShown;
        private AllowTileDragging _allowTileDragging;

		

		#endregion // Member Vars

		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		internal TileDragManager( TileAreaPanel panel, FrameworkElement tile, ItemTileInfo itemInfo, TileLayoutManager.LayoutItem layoutItem, FrameworkElement dragElement, AllowTileDragging allowTileDragging )
		{
			CoreUtilities.ValidateNotNull( panel );

			_tile = tile;
			_itemInfo = itemInfo;
            _layoutItem = layoutItem;
            _panel = panel;
			_tileManager = panel.LayoutManager;
			_dragElement = dragElement;
            _allowTileDragging = allowTileDragging;

			// JJD 9/12/11 - TFS87312
			// Listen for an unload of the panel during a drag operation
			panel.Unloaded += new RoutedEventHandler(OnPanelUnloaded);

		}

		#endregion // Constructor

		#region Properties

		#region Private/Internal Properties

		#region Panel

		/// <summary>
		/// Returns the associated panel.
		/// </summary>
		internal TileAreaPanel Panel
		{
			get
			{
				return _panel;
			}
		}

		#endregion // Panel

		// JJD 9/12/11 - TFS87312 - added
		#region PanelWasUnloaded

		internal bool PanelWasUnloaded { get { return _panelWasUnloaded; } }

		#endregion //PanelWasUnloaded	

		#region TileLayoutManager

		/// <summary>
		/// Returns the associated tiles manager.
		/// </summary>
		internal TileLayoutManager TileLayoutManager
		{
			get
			{
				return _tileManager;
			}
		}

		#endregion // TileLayoutManager

		#region Tile

		/// <summary>
		/// Returns the field being dragged.
		/// </summary>
		internal FrameworkElement Tile
		{
			get
			{
				return _tile;
			}
		}

		#endregion // Tile
		
		#endregion // Private/Internal Properties

		#endregion // Properties

		#region Methods

		#region Private/Internal Methods
        
        internal RectInfo GetLayoutAreaRect()
        {
            TileAreaPanel panel = this.Panel;

            Rect rect = new Rect(new Point(), panel.TileAreaSize);

            Thickness tileAreaPadding = panel.Manager.TileAreaPadding;
            rect.X += tileAreaPadding.Left;
			rect.Y += tileAreaPadding.Top;

            return new RectInfo(this, rect, panel);
        }

		#region CalcScrollSpeed

		private ScrollSpeed CalcScrollSpeed( double distance )
		{
			double range = 200;

			int minScrollSpeed = (int)ScrollSpeed.Slowest;
			int maxScrollSpeed = (int)ScrollSpeed.Fastest;

			int scrollSpeed = Math.Max( minScrollSpeed,
				Math.Min( maxScrollSpeed, (int)( maxScrollSpeed * ( distance / range ) ) ) );

			return (ScrollSpeed)scrollSpeed;
		}

		#endregion // CalcScrollSpeed

		#region DragHelper

		private void DragHelper( PointInfo mouseLoc, bool apply )
		{
			// If the mouse is outside the grid then we need to scroll.
			// 
            //if ( ! apply )
            //    this.ManagerScrollTimerHelper( mouseLoc );

			// JM 07-07-09 - Move this code here from below the following 'if statement
			// so we can pass along the dragIndicatorLocation to the ShowDragIndicator Method.  
			Point dragIndicatorLocation;
			DropInfo dropInfo = this.DragHelper_GetDropInfoHelper(mouseLoc, out dragIndicatorLocation);

			if ( !_dragIndicatorShown )
			{
				// Set the offset of the cursor within the drag element.
				// 
				_dragIndicatorWindow_OffsetFromMouse = null != _dragElement
					? TileUtilities.EnsureInElementBounds( mouseLoc.GetPosition( _dragElement ), _dragElement )
					: new Point( 0, 0 );

                // if right-to-left then adjust offset to compensate
                if (_dragElement.FlowDirection == FlowDirection.RightToLeft)
                    _dragIndicatorWindow_OffsetFromMouse.X = _dragElement.ActualWidth - _dragIndicatorWindow_OffsetFromMouse.X;
 
				// Reset the dragIndicatorLocation now that we have the _dragIndicatorWindow_OffsetFromMouse
				dropInfo = this.DragHelper_GetDropInfoHelper(mouseLoc, out dragIndicatorLocation);

				// Pass the drag indicator location to the ShowDragIndicator method so it can position 
				// the drag indicator before showing it.
				this.ShowDragIndicator(dragIndicatorLocation);
			}

			// Move the drag indicator window to new drop location.
			// 
			if ( null != _dragIndicatorPopupMgr )
			{
                // AS 2/12/09 TFS11410
                //_dragIndicatorPopupMgr.Left = dragIndicatorLocation.X;
				//_dragIndicatorPopupMgr.Top = dragIndicatorLocation.Y;
                this.PositionPopup(_dragIndicatorPopupMgr, dragIndicatorLocation);
			}

			bool isDropNoop = null != dropInfo && dropInfo.IsDropNOOP;
			bool isDropValid = null != dropInfo && dropInfo.IsDropValid;
			bool isDragAreaInvalid = null != dropInfo && dropInfo.IsDragAreaInvalid;

			Debug.WriteLine( string.Format( "Drop Info = {0} {1} {2}",
				null == dropInfo ? "null" : "",
				isDropNoop ? "NOOP" : "",
				null != dropInfo && !isDropValid ? "Invalid" : "Valid" ) );


			// If the item is dragged outside of a valid drop area (outside 
			// of the layout container), then display appropriate cursor.
			// 

			if ( isDragAreaInvalid )
				Mouse.OverrideCursor = Cursors.No;
			else
				Mouse.OverrideCursor = null;


			if ( !isDropNoop && isDropValid )
			{
                TileLayoutManager.LayoutItem liTarget = dropInfo.DropTargetItem;

                this._isSwapping = _allowTileDragging == AllowTileDragging.Swap;

                // if the maximized state of the source and target are different then
                // we can only do a swap
                if (this._isSwapping == false && liTarget != null && liTarget.ItemInfo != null)
                    this._isSwapping = liTarget.ItemInfo.IsMaximized != this._itemInfo.IsMaximized;

                if ((apply == true && this._isSwapping) ||
                     (apply == false && !this._isSwapping))
                {
                    dropInfo.ApplyDrop();
                }
                else
                {
                    if (apply == false)
                    {
                        if (liTarget != null)
                            this.SetSwapTarget(liTarget);
                    }
                }
			}
		}

		#endregion // DragHelper

		#region DragHelper_GetDropInfoHelper

		// SSP 7/2/09 - NAS9.2 Tile Chooser
		// Refactored the code in the DragHelper_GetDropInfoHelper method. It's mostly the same
		// code except the field chooser related logic was added to it.
		// 
		
		/// <summary>
		/// Gets the drop info and also displays/moves the drag indicator.
		/// </summary>
		/// <param name="mouseLoc"></param>
		/// <param name="dragIndicatorLocation"></param>
		/// <returns></returns>
		private DropInfo DragHelper_GetDropInfoHelper( PointInfo mouseLoc, out Point dragIndicatorLocation )
		{
			DropInfo returnValue = null;

			bool isMouseOverTilesPanel = mouseLoc.IsInsideTilesPanel( );

            dragIndicatorLocation = mouseLoc.GetPositionInScreen();
            dragIndicatorLocation.X -= this._dragIndicatorWindow_OffsetFromMouse.X;
			dragIndicatorLocation.Y -= this._dragIndicatorWindow_OffsetFromMouse.Y;

            Thickness targetRectAdjustment = _panel.Manager.TileAreaPadding;

            // adjust for right to left flow
            if (_panel.FlowDirection == FlowDirection.RightToLeft)
                targetRectAdjustment.Left += _layoutItem.TargetRect.Width;           

            // update the target and current so that when we release the mouse the
            // element being dragged will animate from its last drag position back
            // to its appropriate position in the layout

			// JJD 03/28/12 - TFS103699
			// Instead of caling PointFromScreen (which will throw an exception in a low-trust xbap application)
			// tranform directly to the panel's coordinates from the 'relativeTo' in the mouseLoc struct 
			// Point panelPt = _panel.PointFromScreen(dragIndicatorLocation);
			GeneralTransform transform = mouseLoc.RelativeTo.TransformToVisual(_panel);
			Point panelPt = transform.Transform(dragIndicatorLocation);


#region Infragistics Source Cleanup (Region)















#endregion // Infragistics Source Cleanup (Region)


            _layoutItem.SetTargetRect( new Rect( new Point( panelPt.X - targetRectAdjustment.Left, panelPt.Y - targetRectAdjustment.Top), new Size(_layoutItem.TargetRect.Width, _layoutItem.TargetRect.Height)), true);
            _layoutItem.CurrentRect = _layoutItem.TargetRect;

            TileLayoutManager.LayoutItem li = _tileManager.GetLayoutItemFromPoint(mouseLoc.GetPosition(_panel), _layoutItem);

            if (li != null)
            {
                returnValue = new DropInfo(this, li, li.Container == _tile);
            }
            else
            {
                this.SetSwapTarget(null);
            }
			// ------------------------------------------------------------------------------------------
			


			if ( null == returnValue )
			{
				if ( !isMouseOverTilesPanel )
				{
					
					// Return an drop info instance that represents invalid drop.
					// 
					returnValue = new DropInfo( this, true );
					// ------------------------------------------------------------
				}
			}

			return returnValue;
		}


		#endregion // DragHelper_GetDropInfoHelper

		#region HideDragIndicator

		internal void HideDragIndicator( )
		{
			if ( null != _dragIndicatorPopupMgr )
			{
				_dragIndicatorPopupMgr.Close();
				_dragIndicatorShown = false;
			}
		}

		#endregion // HideDragIndicator

#region Infragistics Source Cleanup (Region)


























#endregion // Infragistics Source Cleanup (Region)

		#region Close

		/// <summary>
		/// Called to indicate that the drag operation has ended. Drop will be processed if 'cancel' parameter
		/// is false.
		/// </summary>
		/// <param name="mouseEventArgs">Mouse event args associated with the drag end.</param>
		/// <param name="cancel">Indicates whether to process the drop operation.</param>
		internal void OnDragEnd( MouseEventArgs mouseEventArgs, bool cancel )
		{
			try
			{
				// JJD 9/12/11 - TFS87312
				// Unwire the unloaded event
				_panel.Unloaded -= new RoutedEventHandler(OnPanelUnloaded);

				this.SetSwapTarget(null);

				this.OnDragEndHelper( mouseEventArgs, cancel );

                this._panel.OnEndDrag(_tile);

                this._panel.InvalidateMeasure();
			}
			finally
			{
					//_panel.SetTileDragManagerHelper( null );
				// --------------------------------------------------------------------------------
			}

			Debug.WriteLine( "Drag End" );
		}

		private void OnDragEndHelper( MouseEventArgs mouseEventArgs, bool cancel )
		{
            
#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)



			// JJD 9/12/11 - TFS87312
			// Bypass mouse moves if the panel was unloaded during a drag operation
			//if ( !cancel )
			if (!cancel && false == _panelWasUnloaded)
			{
				PointInfo mouseLoc = new PointInfo( this, mouseEventArgs.GetPosition( _panel ), _panel );
				this.DragHelper( mouseLoc, true );
			}

			// We set the OverrideCursor when the item is dragged outside of a valid 
			// drop area (outside of the layout container), in which case we have to 
			// reset it here since the drag operation is being ended.
			//

			Mouse.OverrideCursor = null;


			this.HideDragIndicator( );

			if ( null != _dragIndicatorPopupMgr )
			{
				_dragIndicatorPopupMgr.Close( );
				_dragIndicatorPopupMgr = null;
			}
		}

		#endregion // Close

		#region OnMouseMove

		/// <summary>
		/// Called whenever mouse is moved while drag operation is in progress.
		/// </summary>
		internal void OnMouseMove( MouseEventArgs mouseEventArgs )
		{
			// JJD 9/12/11 - TFS87312
			// Bypass mouse moves if the panel was unloaded during a drag operation
			if (_panelWasUnloaded)
				return;

			PointInfo mouseLoc = new PointInfo(this, mouseEventArgs.GetPosition(_panel), _panel);

			this.DragHelper( mouseLoc, false );
		}

		#endregion // OnMouseMove

        
#region Infragistics Source Cleanup (Region)









































































#endregion // Infragistics Source Cleanup (Region)


		#region OnPanelUnloaded

		// JJD 9/12/11 - TFS87312
		// Listen for an unload of the panel during a drag operation
		private void OnPanelUnloaded(object sender, RoutedEventArgs e)
		{
			this._panelWasUnloaded = true;
		}

		#endregion //OnPanelUnloaded

		#region PositionPopup
        // The top/left are relative to the logical screen used by the popup
        // which may be within the adorner layer.
        //
		private void PositionPopup(PopupDragManager popupMgr, Point screenPoint)
        {
			popupMgr.PositionPopup(screenPoint);
        } 
        #endregion //PositionPopup

        #region SetSwapTarget

        private void SetSwapTarget(TileLayoutManager.LayoutItem swapTarget)
        {
            if (swapTarget == this._lastSwapTarget)
                return;

            if (this._lastSwapTarget != null && this._lastSwapTarget.Container != null)
				this._lastSwapTarget.Container.ClearValue(XamTileManager.IsSwapTargetPropertyKey);

            this._lastSwapTarget = swapTarget;

            if (this._lastSwapTarget != null)
            {
                this._canSwapTarget = this._panel.CanSwapContainers(this._layoutItem.Container as FrameworkElement, this._layoutItem.ItemInfo, this._lastSwapTarget.Container as FrameworkElement, this._lastSwapTarget.ItemInfo);

                if ( this._canSwapTarget )
					this._lastSwapTarget.Container.SetValue(XamTileManager.IsSwapTargetPropertyKey, KnownBoxes.TrueBox);
            }
        }

        #endregion //SetSwapTarget	

		#region ShowDragIndicator

		internal void ShowDragIndicator(Point dragIndicatorLocation)
		{
			this._dragIndicatorPopupMgr = new PopupDragManager();

			TileDragIndicator dragIndicator = new TileDragIndicator( );

			dragIndicator.SetValue(TileDragIndicator.ContainerPropertyKey, _tile);

            
            // Make sure the IsDragging property is set before we make the
            // snapshot so any template triggers can be invoked
			_tile.SetValue(XamTileManager.IsDraggingPropertyKey, KnownBoxes.TrueBox);

            
            // Call UpdateLayout to make sure any IsDragging property triggers get reflected
            // in the 
            _tile.UpdateLayout();

            
            // Create an image snapshot of the tile that we will use while dragging
            Size size = _tile.RenderSize;

			// JJD 4/22/11 - TFS29160
			// Add half a pixel in both dimensions to eliminate any layout rounding issues
			// so the right and bottom borders of the tile show up in the drag indicator
			// and aren't clipped out.
			size.Width += .5;
			size.Height += .5;


            RenderTargetBitmap bmp = new RenderTargetBitmap((int)(size.Width),
                (int)(size.Height),
                96, 96, PixelFormats.Default);

            if (VisualTreeHelper.GetChildrenCount(_tile) > 0 )
                bmp.Render((Visual)VisualTreeHelper.GetChild( _tile, 0));
            else
                bmp.Render(_tile);

            bmp.Freeze();

			if (!Infragistics.Windows.Controls.PopupHelper.PopupAllowsTransparency)
				dragIndicator.Background = Brushes.White;



            Image image;

            image = new Image();
            image.Source = bmp;
            image.Stretch = Stretch.None;
            image.Width = size.Width;
            image.Height = size.Height;



#region Infragistics Source Cleanup (Region)









#endregion // Infragistics Source Cleanup (Region)


            dragIndicator.Content = image;

			Debug.WriteLine( "Showing drag indicator window" );

			//this.PositionPopup(_dragIndicatorPopupMgr, dragIndicatorLocation);

			_dragIndicatorPopupMgr.Open(dragIndicatorLocation, _panel, dragIndicator, null );

			_dragIndicatorShown = true;
		}

		#endregion // ShowDragIndicator

		#region StartDrag

		/// <summary>
		/// Starts dragging operation. It displays the drag indicator to indicate that dragging operation is in progress.
		/// </summary>
		// JJD 03/06/12 - TFS100150 - Added touch support for scrolling
		// Changed signature to take a point instead of the MouseEventArgs
		//internal void StartDrag( MouseEventArgs mouseEventArgs )
		internal void StartDrag( Point point )
		{
			// --------------------------------------------------------------------------------

			// JJD 03/06/12 - TFS100150 - Added touch support for scrolling
			// Pass point parameter into PointInfo ctor
			//PointInfo mouseLoc = new PointInfo( this, mouseEventArgs.GetPosition( _panel ), _panel );
			PointInfo mouseLoc = new PointInfo(this, point, _panel );

			Debug.WriteLine( "StartDrag" );

			// Call DragHelper which will display drag indicator window.
			// 
			this.DragHelper( mouseLoc, false );
		}

		#endregion // StartDrag

		#endregion // Private/Internal Methods

		#endregion // Methods
	}

	#endregion // TileDragManager Class
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