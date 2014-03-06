using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Infragistics.Controls.Primitives
{
    #region ISelectionHost interface

    /// <summary>
    /// This interface is implemented by controls that want to support selection. 
    /// </summary>
    internal interface ISelectionHost
    {
        #region Properties

            #region RootElement

        /// <summary>
        /// Returns the root element which normally is the control itself (read-only).
        /// </summary>
        FrameworkElement RootElement { get; }

            #endregion //RootElement	
    
        #endregion //Properties

        #region Methods

            #region ActivateItem

        /// <summary>
        /// Activates the specified item.
        /// </summary>
        /// <param name="item">The selectable item</param>
        /// <param name="preventScrollItemIntoView">If true don't scroll the item into view.</param>
        /// <returns>Returns true if successful, otherwise false.</returns>
        /// <seealso cref="ISelectableItem"/>
        // JJD 7/14/09 - TFS18784
        // Added preventScrollItemIntoView flag
        bool ActivateItem(ISelectableItem item, bool preventScrollItemIntoView);

            #endregion //ActivateItem

            #region ClearSnapshot

        /// <summary>
        /// Clears the selection snapshot 
        /// </summary>
        /// <seealso cref="SnapshotSelection"/>
        void ClearSnapshot();

            #endregion //ClearSnapshot	
    
            #region DeselectItem

        /// <summary>
        /// De-select the specified item.
        /// </summary>
        /// <param name="item">The item to de-select.</param>
        /// <returns>True if successful.</returns>
        bool DeselectItem(ISelectableItem item);

            #endregion //DeselectItem	
    
            #region DeselectRange

        /// <summary>
        /// De-select the range from the pivot item to the specified item.
        /// </summary>
        /// <param name="item">The item to de-select.</param>
        /// <returns>True if successful.</returns>
        bool DeselectRange(ISelectableItem item);

            #endregion //DeselectRange	
    
            #region DoAutoScrollHorizontal

        /// <summary>
        /// Perform a horizontal autoscroll operation
        /// </summary>
        /// <param name="item">The item that triggered the scroll operation</param>
        /// <param name="direction">The direction to scroll.</param>
        /// <param name="speed">The scroll speed.</param>
        void DoAutoScrollHorizontal(ISelectableItem item, ScrollDirection direction, ScrollSpeed speed);

            #endregion //DoAutoScrollHorizontal
    
            #region DoAutoScrollVertical

        /// <summary>
        /// Perform a vertical autoscroll operation
        /// </summary>
        /// <param name="item">The item that triggered the scroll operation</param>
        /// <param name="direction">The direction to scroll.</param>
        /// <param name="speed">The scroll speed.</param>
        void DoAutoScrollVertical(ISelectableItem item, ScrollDirection direction, ScrollSpeed speed);

            #endregion //DoAutoScrollVertical

            #region EnterSnakingMode

        /// <summary>
        /// Used to tell the host to enter snaking mode if appropriate for the item type being selected. 
        /// </summary>
        /// <param name="item"></param>
        /// <remarks>Snaking refers to range selection where the items being selected 
        /// form a snaking pattern instead of a rectangular pattern. For eample, if in 
        /// a grid that contained 5 columns the user was selecting a range of cells
        /// from different rows, the selection could be either rectangular or snake 
        /// from row to row in which case all cells from intervening rows would get selected.
        /// </remarks>
        void EnterSnakingMode(ISelectableItem item);

            #endregion //EnterSnakingMode	

            #region GetAutoScrollInfo

        /// <summary>
        /// Gets the potential auto scroll information.
        /// </summary>
        /// <param name="item">The selectable item.</param>
        /// <returns>An <see cref="AutoScrollInfo"/> struct that contains information about the possible auto scroll possibilies.</returns>
        AutoScrollInfo GetAutoScrollInfo(ISelectableItem item);

            #endregion //GetAutoScrollInfo	
    
            #region GetNearestCompatibleItem

        /// <summary>
        /// Returns the <see cref="ISelectableItem"/> compatible with the specified item that
        /// is nearest to the point identied by the mouse event args.
        /// </summary>
        /// <param name="item">The selectable item.</param>
        /// <param name="e">The mouse event arguments</param>
        ISelectableItem GetNearestCompatibleItem(ISelectableItem item, System.Windows.Input.MouseEventArgs e);

            #endregion //GetNearestCompatibleItem	

            #region GetPivotItem

        /// <summary>
        /// Returns the <see cref="ISelectableItem"/> that is the pivot item based on the type
        /// of item passed-in.
        /// </summary>
        /// <param name="item"></param>
        /// <returns>The current pivot item or null.</returns>
        ISelectableItem GetPivotItem(ISelectableItem item);

            #endregion //GetPivotItem	
            
            #region GetSelectionStrategyForItem

        /// <summary>
        /// Returns the selection strateg for a specific item.
        /// </summary>
        /// <param name="item">The selectable item.</param>
        /// <returns>The <see cref="SelectionStrategyBase"/> for this item type or null.</returns>
        SelectionStrategyBase GetSelectionStrategyForItem(ISelectableItem item);

            #endregion //GetSelectionStrategyForItem	
    
            #region IsItemSelectableWithCurrentSelection

        /// <summary>
        /// Returns true if the item can be selected without first clearing the current selection.
        /// </summary>
        /// <param name="item">The item to select.</param>
        /// <returns><b>True</b> if the item is selectable with current selection, <b>false</b> otherwise.</returns>
        /// <seealso cref="ISelectableItem"/>
        bool IsItemSelectableWithCurrentSelection(ISelectableItem item);

            #endregion //IsItemSelectableWithCurrentSelection	
            
            #region IsMaxSelectedItemsReached

        /// <summary>
        /// Returns true if selecting this item will exceed the maximum allowed.
        /// </summary>
        /// <param name="item"><see cref="ISelectableItem"/>The selectable item</param>
        /// <returns>True if the maximum number of items has already been selected.</returns>
        bool IsMaxSelectedItemsReached(ISelectableItem item);

            #endregion //IsMaxSelectedItemsReached

            #region OnDragStart

        /// <summary>
        /// Called when a dragging operation is about to begin.
        /// </summary>
        /// <param name="item">The selectable item</param>
        /// <param name="e">The mouse event arguments</param>
        /// <returns>Returning true means that the drag operation started successfully.</returns>
        bool OnDragStart(ISelectableItem item, System.Windows.Input.MouseEventArgs e);

            #endregion //OnDragStart	
            
            #region OnDragMove

        /// <summary>
        /// Called on a mouse move during a dragging operation
        /// </summary>
        /// <param name="e">The mouse event arguments</param>
        void OnDragMove(System.Windows.Input.MouseEventArgs e);

            #endregion //OnDragMove	
            
            #region OnDragEnd

        /// <summary>
        /// Called at the end of a dragging operation
        /// </summary>
        /// <param name="canceled"><b>True</b> if the drag was cancelled, <b>false</b> otherwise.</param>
        void OnDragEnd(bool canceled);

            #endregion //OnDragEnd	
            
			#region OnMouseUp

		/// <summary>
		/// Called when the mouse is released.
		/// </summary>
		/// <param name="e">The mouse event arguments</param>
		void OnMouseUp(System.Windows.Input.MouseEventArgs e);

			#endregion //OnMouseUp
    
            #region SelectItem

        /// <summary>
        /// Select the specified item.
        /// </summary>
        /// <param name="item">The item to select.</param>
        /// <param name="clearExistingSelection">True to clear the current selection.</param>
        /// <returns>True if successful.</returns>
        bool SelectItem(ISelectableItem item, bool clearExistingSelection);

            #endregion //SelectItem	
    
            #region SelectRange

        /// <summary>
        /// Select the range from the pivot item to the specified item.
        /// </summary>
        /// <param name="item">The item to select.</param>
        /// <param name="clearExistingSelection">True to clear the current selection.</param>
        /// <returns>True if successful.</returns>
        bool SelectRange(ISelectableItem item, bool clearExistingSelection);

            #endregion //SelectRange	
    
            #region SetPivotItem

        /// <summary>
        /// Set the specified item as the pivot item. 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="isRangeSelect">True if this is part of a range selection.</param>
        void SetPivotItem(ISelectableItem item, bool isRangeSelect);

            #endregion //SetPivotItem	
    
            #region SnapshotSelection

        /// <summary>
        /// Snapshots the current selection 
        /// </summary>
        /// <param name="item">The item to use to determine the which selection to snapshot.</param>
        /// <remarks>
        /// The 'item' parameter is necessary for controls that support selection of multiple item types.
        /// For example, the DataPresenter supports selection of Records, FieldLabels and Cells.
        /// </remarks>
        void SnapshotSelection(ISelectableItem item);

            #endregion //SnapshotSelection	
    
            #region TranslateItem

        /// <summary>
        /// Potentially translates the passed-in item to an <see cref="ISelectableItem"/> of a
        /// different type.  For instance, the DataPresenter translates a passed-in cell 
        /// into its parent DataRecord if record selection is specified.
        /// </summary>
        /// <param name="item">The item to translate</param>
        /// <returns>The translated item.</returns>
        ISelectableItem TranslateItem(ISelectableItem item);

            #endregion //TranslateItem	
    
        #endregion //Methods
    }

    #endregion //ISelectionHost interface	
    
    #region ISelectableElement interface

    /// <summary>
    /// Interface implemented by elements that represent selectable items (e.g. rows, cells, headers, nodes etc.)
    /// </summary>
    internal interface ISelectableElement
    {
        /// <summary>
        /// Returns the associated <see cref="ISelectableItem"/> (read-only).
        /// </summary>
        ISelectableItem SelectableItem { get; }
    }

    #endregion //ISelectableElement interface

    #region ISelectionStrategyFilter interface

    /// <summary>
    /// Interface implemented by the user of a control to supply
    /// custom selection strategies.
    /// </summary>
    /// <seealso cref="SelectionStrategyBase"/>
    internal interface ISelectionStrategyFilter
    {
        /// <summary>
        /// Called to get the selection strategy for a specific
        /// item. Returning null means that the control will supply
        /// its own implementation.
        /// </summary>
        SelectionStrategyBase GetSelectionStrategyForItem(ISelectableItem item);
    }

    #endregion //ISelectionStrategyFilter interface	
        
    #region ScrollSpeed enum

    /// <summary>
    /// Determines how fast a slection scroll should be
    /// </summary>
    internal enum ScrollSpeed
    {
        /// <summary>
        /// Scroll the slowest
        /// </summary>
        Slowest = 0,
        /// <summary>
        /// Scroll slower
        /// </summary>
        Slower = 1,
        /// <summary>
        /// Scroll slowly
        /// </summary>
        Slow = 2,
        /// <summary>
        /// Scroll a moderate amount
        /// </summary>
        Medium = 3,
        /// <summary>
        /// Scroll fast
        /// </summary>
        Fast = 4,
        /// <summary>
        /// Scroll faster
        /// </summary>
        Faster = 5,
        /// <summary>
        /// Scroll fastest
        /// </summary>
        Fastest = 6
    }

    #endregion //ScrollSpeed enum

    #region ScrollDirection enum

    /// <summary>
    /// Determines what direction to scroll
    /// </summary>
    internal enum ScrollDirection
    {
        /// <summary>
        /// Increment the scroll value
        /// </summary>
        Increment = 0,
        /// <summary>
        /// Decrement the scrcoll value
        /// </summary>
        Decrement = 1,
    }

    #endregion //ScrollDirection enum	

    // AS 8/28/08 NA 2008 Vol 2 ScrollOrientation
    #region ScrollOrientation
    /// <summary>
    /// Enumeration used to identify the orientation in which the auto scroll logic should scroll if the 
    /// mouse is positioned outside the element such that the element can be scroll both horizontally 
    /// and vertically.
    /// </summary>
    internal enum ScrollOrientation
    {
        /// <summary>
        /// The auto scroll logic should scroll both horizontally and vertically.
        /// </summary>
        Both,

        /// <summary>
        /// The auto scroll logic should prefer scrolling horizontally regardless of the distance between the mouse and the element bounds.
        /// </summary>
        Horizontal,

        /// <summary>
        /// The auto scroll logic should prefer scrolling vertically regardless of the distance between the mouse and the element bounds.
        /// </summary>
        Vertical,

        /// <summary>
        /// The auto scroll logic should prefer scrolling in the direction in which the mouse is furthest from the element.
        /// </summary>
        BasedOnDistance,
    } 
    #endregion //ScrollOrientation

    #region AutoScrollInfo struct

    /// <summary>
    /// Struct that returns information used by the auto scroll logic to determine if auto scrolling is possible
    /// </summary>
    internal struct AutoScrollInfo
    {
        #region Private Members

        private FrameworkElement		_scrollArea;
        private bool					_canScrollDown;
        private bool					_canScrollLeft;
        private bool					_canScrollRight;
        private bool					_canScrollUp;

        // AS 8/28/08 NA 2008 Vol 2 ScrollOrientation
        private ScrollOrientation       _scrollOrientation;

        #endregion //Private Members

        #region Constructor

        /// <summary>
		/// Initializes a new instance of the <see cref="AutoScrollInfo"/> struct
        /// </summary>
        /// <param name="scrollArea">The element that defines the scroll bounds</param>
        /// <param name="canScrollDown">True if a scroll down is possible.</param>
        /// <param name="canScrollLeft">True if a scroll left is possible.</param>
        /// <param name="canScrollRight">True if a scroll right is possible.</param>
        /// <param name="canScrollUp">True if a scroll up is possible.</param>
        public AutoScrollInfo(FrameworkElement scrollArea, bool canScrollDown, bool canScrollLeft, bool canScrollRight, bool canScrollUp)
		{
            this._scrollArea		= scrollArea;
            this._canScrollDown	= canScrollDown;
            this._canScrollLeft	= canScrollLeft;
            this._canScrollRight	= canScrollRight;
            this._canScrollUp		= canScrollUp;

            // AS 8/28/08 NA 2008 Vol 2 ScrollOrientation
            this._scrollOrientation = ScrollOrientation.Both;
        }

        #endregion //Constructor

        #region Properties

        #region CanScroll...

        /// <summary>
        /// Returns true if a scroll down is possible (read-only).  
        /// </summary>
        public bool CanScrollDown { get { return this._canScrollDown; } }
        /// <summary>
        /// Returns true if a scroll left is possible (read-only).  
        /// </summary>
        public bool CanScrollLeft { get { return this._canScrollLeft; } }
        /// <summary>
        /// Returns true if a scroll right is possible (read-only).  
        /// </summary>
        public bool CanScrollRight { get { return this._canScrollRight; } }
        /// <summary>
        /// Returns true if a scroll up is possible (read-only).  
        /// </summary>
        public bool CanScrollUp { get { return this._canScrollUp; } }

        #endregion //CanScroll...

        #region ScrollArea

        /// <summary>
        /// Returns the element that defines the scroll bounds (read-only). 
        /// </summary>
		public FrameworkElement ScrollArea { get { return this._scrollArea; } }

        #endregion //ScrollArea

        // AS 8/28/08 NA 2008 Vol 2 ScrollOrientation
        #region ScrollOrientation
        /// <summary>
        /// Returns or sets a value that indicates how the direction in which the auto scroll 
        /// logic should execute when it is possible to scroll both horizontally and vertically.
        /// </summary>
        public ScrollOrientation ScrollOrientation
        { 
            get { return this._scrollOrientation; }
            set { this._scrollOrientation = value; }
        }
        #endregion //ScrollOrientation

        #endregion //Properties
    }

    #endregion //AutoScrollInfo struct	
    
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