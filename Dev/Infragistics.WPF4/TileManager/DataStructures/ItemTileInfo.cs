using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;

using System.Xml;
using Infragistics.Controls.Layouts.Primitives;
using Infragistics.Collections;
using System.Windows;
using Infragistics.AutomationPeers;
using System.Windows.Automation;
using System.Windows.Automation.Provider;

namespace Infragistics.Controls.Layouts
{
    /// <summary>
    /// An object that maintains certain status information for an item in a tile
    /// </summary>
    public class ItemTileInfo : PropertyChangeNotifier, ISparseArrayMultiItem
    {
        #region Private Members

        private TileLayoutManager		_layoutManager;
        private object					_item;
        private int						_index;
        private object					_sparseArrayOwnerData;
        private Size?					_sizeOverride;
		private bool					_isClosed;
		private bool?					_isExpandedWhenMinimized;
		private bool?					_isExpandedWhenMinimizedLastReturned;
		private bool					_isMaximized;

		private TileItemAutomationPeer	_peer;
		private ExpandCollapseState		_lastExpandCollapseState;

		private TileItemPersistenceInfo _serializationInfo;

        #endregion //Private Members	
 
        #region Constructor



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

        internal ItemTileInfo(TileLayoutManager layoutManager, object item, int index)
        {
            this._layoutManager   = layoutManager;
            this._item      = item;
            this._index     = index;
        }

        #endregion //Constructor
        
        #region Properties

            #region Public Properties

                #region Index

        /// <summary>
        /// Returns the index of the item in the Items collection (read-only)
        /// </summary>
        [ReadOnly(true)]
        [Bindable(false)]
        public int Index
        {
            get
            {
                IList items = this._layoutManager.Items;

                // Optimization: use the cached index unless it is invalid or the item doesn't match
                if (this._index < 0 ||
                     this._index >= items.Count ||
                     !Object.Equals(this._item, items[this._index]))
                {
                    this._index = items.IndexOf(this._item);
                }

                return this._index;
            }
        }

                #endregion //Index	
    
                #region IsClosed

        /// <summary>
        /// Gets/sets whether this item is closed.
        /// </summary>
        /// <seealso cref="XamTileManager.TileCloseAction"/>
        /// <seealso cref="XamTile.CloseAction"/>
        /// <seealso cref="XamTile.IsClosed"/>
        public bool IsClosed
        {
            get
            {
                return this._isClosed;
            }
            set
            {
                this.SetIsClosedInternal(value, true);
            }
        }

        private void SetIsClosedInternal(bool newValue, bool synchronizePropsOnTile)
        {
            if (newValue != this._isClosed)
            {
                this._isClosed = newValue;
                this.RaisePropertyChangedEvent("IsClosed");

                XamTileManager tm  = _layoutManager.OwningManager;

                if (synchronizePropsOnTile)
                {
                    XamTile tile = tm != null ? tm.TileFromItem(this.Item) : null;

                    if (tile != null)
                        tile.IsClosed = newValue;

                    
                    
                    if (tm != null)
                    {
                        
                        
                        tm.OnItemIsClosedChanged(this.Item, this._isClosed, this.IsMaximized);
                    }
                }

                this.InvalidateScrollPosition();
				
				this.VerifyExpandCollapseStateOnPeer();
			}
        }

                #endregion //IsClosed	

                #region IsExpandedWhenMinimized

        /// <summary>
        /// Gets/sets whether this item is expanded when it is minimized.
        /// </summary>
        /// <seealso cref="MaximizedModeSettings.MinimizedTileExpansionMode"/>
        /// <seealso cref="XamTile.IsExpandedWhenMinimized"/>
        public bool? IsExpandedWhenMinimized
        {
            get
            {
                return this._isExpandedWhenMinimized;
            }
            set
            {
                this.SetIsExpandedWhenMinimizedInternal(value, true);
            }
        }

        private void SetIsExpandedWhenMinimizedInternal(bool? newValue, bool synchronizePropsOnTile)
        {
            if (newValue != this._isExpandedWhenMinimized)
            {
                this._isExpandedWhenMinimized = newValue;
                this.RaisePropertyChangedEvent("IsExpandedWhenMinimized");

                bool resolvedValue = this.IsExpandedWhenMinimizedResolved;

				XamTileManager tm = _layoutManager.OwningManager;

                if (synchronizePropsOnTile && tm != null)
                {
                    XamTile tile = tm.TileFromItem(this.Item);

                    if (tile != null)
                        tile.IsExpandedWhenMinimized = newValue;
                }
				
				this.VerifyExpandCollapseStateOnPeer();
			}
        }

                #endregion //IsExpandedWhenMinimized	

                #region IsExpandedWhenMinimizedResolved

        /// <summary>
        /// Gets the whether this item is expanded when it is minimized (read-only).
        /// </summary>
        /// <seealso cref="MaximizedModeSettings.MinimizedTileExpansionMode"/>
        /// <seealso cref="XamTile.IsExpandedWhenMinimized"/>
        public bool IsExpandedWhenMinimizedResolved
        {
            get
            {
                bool value = TileUtilities.GetIsExpandedWhenMinimizedHelper(_layoutManager.OwningManager, null, this.Item, this.IsExpandedWhenMinimized);

                if (this._isExpandedWhenMinimizedLastReturned.HasValue)
                {
                    if (this._isExpandedWhenMinimizedLastReturned != value)
                    {
                        this._isExpandedWhenMinimizedLastReturned = value;
                        this.RaisePropertyChangedEvent("IsExpandedWhenMinimizedResolved");

						this.VerifyExpandCollapseStateOnPeer();
                    }
                }
                else
                    this._isExpandedWhenMinimizedLastReturned = value;

                return value;
            }
        }

                #endregion //IsExpandedWhenMinimizedResolved	

                #region IsMaximized

        /// <summary>
        /// Gets/sets whether this item's state is 'Maximized'.
        /// </summary>
        /// <seealso cref="XamTileManager.MaximizedTileLimit"/>
        /// <seealso cref="XamTile.AllowMaximize"/>
        /// <seealso cref="XamTile.State"/>
        /// <seealso cref="TileState"/>
        public bool IsMaximized
        {
            get
            {
                return this._isMaximized;
            }
            set
            {
                this.SetIsMaximizedInternal(value, true);
            }
        }

        internal void SetIsMaximizedInternal(bool newValue, bool synchronizePropsOnTile)
        {
            if (newValue != this._isMaximized)
            {
                this._isMaximized = newValue;
                this.RaisePropertyChangedEvent("IsMaximized");

                if (synchronizePropsOnTile)
                {
                    XamTileManager tm = _layoutManager.OwningManager;

                    XamTile tile = tm != null ? tm.TileFromItem(this.Item) : null;

                    if (tile != null)
                    {
                        TileState newState = this._isMaximized
                            ? TileState.Maximized
                            : TileState.Normal;

                        tm.ChangeTileState(tile, newState);
                    }
                }

                this.InvalidateScrollPosition();
				this.VerifyExpandCollapseStateOnPeer();
			}
        }

                #endregion //IsMaximized	
    
                #region Item

        /// <summary>
        /// Returns the associated item (read-only).
        /// </summary>
        [ReadOnly(true)]
        public object Item { get { return this._item; } }

                #endregion //Item	

                #region LogicalIndex

        /// <summary>
        /// Returns the logical index of the item (read-only)
        /// </summary>
        /// <value>A zero-based index representing the item's relative position in the display or -1 if the item was not found.</value>
        /// <remarks>
        /// <para class="note"><b>Note:</b> this will return the same value as the <see cref="Index"/> property unless the user has re-positioned one or more <see cref="XamTile"/>s in which case the logical index represents the position of this item relative to other items.</para>
        /// </remarks>
        [ReadOnly(true)]
        [Bindable(false)]
        public int LogicalIndex
        {
            get
            {
                return this._layoutManager.GetLogicalIndexOfItem(this._item, false);
            }
        }

                #endregion //LogicalIndex	

                #region OccupiesScrollPosition

        /// <summary>
        /// Returns true if this item occupies a scroll position (read-only)
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> this will return false if the item is closed or maximized.</para>
        /// </remarks>
        public bool OccupiesScrollPosition
        {
            get
            {
                return this._isClosed == false && this._isMaximized == false;
            }
        }

                #endregion //OccupiesScrollPosition

                #region ScrollPosition

        /// <summary>
        /// Gets the overall scroll position of this item (read-only).
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> items that are collapsed or maximized are not included in the scroll count.</para>
        /// </remarks>
        /// <value>The zero based scroll position or -1 for collapsed or maximized items.</value>
        [ReadOnly(true)]
        [Bindable(false)]
        public int ScrollPosition
        {
            get
            {
                return this._layoutManager.GetLogicalIndexOfItem(this._item, true);
            }
        }

                #endregion //ScrollPosition

                #region SizeOverride

            /// <summary>
            /// Gets/sets an explicit size to be returned as the item's preferred size.
            /// </summary>
            /// <remarks>
            /// <para class="note"><b>Note:</b> this setting may be ignored if a synchronized size is being used. Otherwise, if this property returns a width and/or height > 0 it will take precedence over all other settings but only when the item's state is normal.</para>
            /// </remarks>
            public Size? SizeOverride
            {
                get { return this._sizeOverride; }
                set { this._sizeOverride = value; }
            }

                #endregion //SizeOverride	
    
            #endregion //Public Properties	
 
            #region Internal Properties

                #region IsInSparseArray

            internal bool IsInSparseArray { get { return this._sparseArrayOwnerData != null; } }

                #endregion //IsInSparseArray	
    
                #region Manager






            internal TileLayoutManager LayoutManager { get { return this._layoutManager; } }

                #endregion //Manager	

                #region SerializationInfo

		internal TileItemPersistenceInfo SerializationInfo
		{
		    get { return this._serializationInfo; }
		    set { this._serializationInfo = value; }
		}

                #endregion //SerializationInfo

            #endregion //Internal Properties
 
            #region Protected Properties

            #endregion //Protected Properties

        #endregion //Properties

        #region Methods

            #region Public Methods

                #region BringIntoView

            
            /// <summary>
            /// Brings the item into view
            /// </summary>
            public void BringIntoView()
            {
                int index = this.Index;

                if ( index >= 0 )
                    this._layoutManager.BringIndexIntoView(index);
            }

                #endregion //BringIntoView	
    
                #region GetCurrentColumn

            /// <summary>
            /// Returns the column of the item in the current display 
            /// </summary>
            /// <returns>The current zero-based column, if the item is currently displayed, otherwise it returns -1.</returns>
            /// <remarks>
            /// <para class="note"><b>Note:</b> this value is relative to the first column of items being currently displayed</para>
            /// </remarks>
            public int GetCurrentColumn() 
            { 
                return this._layoutManager.GetCurrentColumn(this);
            }

                #endregion //GetCurrentColumn

				#region GetCurrentConstraints

			/// <summary>
            /// Returns the resolved constraints of this item in the current display 
            /// </summary>
            /// <returns>An object that implements the <see cref="ITileConstraints"/> interface for this item, if the item is currently displayed, otherwise it returns null.</returns>
            public ITileConstraints GetCurrentConstraints() 
            {
                if ( !this._layoutManager.IsItemInView(this))
                    return null;

                TileLayoutManager.LayoutItem li = this._layoutManager.GetLayoutItem(this);

                Debug.Assert(li != null, "If the item is in view then a layoutItem should be cached");
                if (li == null)
                    return null;

                return (ITileConstraints)li;
            }

                #endregion //GetCurrentConstraints
    
                #region GetCurrentRow

            /// <summary>
            /// Returns the row of the item in the current display 
            /// </summary>
            /// <returns>The current zero-based row, if the item is currently displayed, otherwise it returns -1.</returns>
            /// <remarks>
            /// <para class="note"><b>Note:</b> this value is relative to the first row of items being currently displayed</para>
            /// </remarks>
            public int GetCurrentRow() 
            { 
                return this._layoutManager.GetCurrentRow(this);
            }

            #endregion //GetCurrentRow
    
                #region GetCurrentTargetRect

            /// <summary>
            /// Returns the current target rect of the item 
            /// </summary>
            /// <returns>The current target rect, after animations have been completed. If the item will not be in view then Rect.Empty will be returned.</returns>
            public Rect GetCurrentTargetRect() 
            {
                TileAreaPanel panel = this._layoutManager.Panel;

                if (panel != null)
                    return panel.GetTargetRectOfItem(this.Item);

                return Rect.Empty;
            }

                #endregion //GetCurrentTargetRect

            #endregion //Public Methods	
        
            #region Internal Methods

				#region GetAutomationPeer

			internal TileItemAutomationPeer GetAutomationPeer()
			{
				if (_peer == null)
				{
					_peer = new TileItemAutomationPeer(this);
					_lastExpandCollapseState = ((IExpandCollapseProvider)_peer).ExpandCollapseState;
				}

				return _peer;
			}

				#endregion //GetAutomationPeer	
    
                #region GetResizeRange



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

            internal void GetResizeRange(out double maxDeltaLeft, out double maxDeltaRight,
                                            out double maxDeltaTop, out double maxDeltaBottom)
            {
                this._layoutManager.GetResizeRange(this, out maxDeltaLeft, out maxDeltaRight, out maxDeltaTop, out maxDeltaBottom);
            }

                #endregion //GetResizeRange

                #region OnResize



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

            internal void OnResize(double deltaX, double deltaY)
            {
                this._layoutManager.OnItemResized(this, deltaX, deltaY);
            }

                #endregion //OnResize

                #region InvalidateScrollPosition






            internal void InvalidateScrollPosition()
            {
                if (this.IsInSparseArray)
                    this._layoutManager.SparseArray.InvalidateItem(this);
            }

                #endregion //InvalidateScrollPosition

				#region SyncFromTileState

			internal void SyncFromTileState(XamTile tile)
			{
				if (tile != null)
				{
					this.SetIsClosedInternal(tile.IsClosed, false);
					this.SetIsExpandedWhenMinimizedInternal(tile.IsExpandedWhenMinimized, false);
					this.SetIsMaximizedInternal(tile.State == TileState.Maximized, false);
				}
			}

				#endregion //SyncFromTileState	

				#region VerifyExpandCollapseStateOnPeer

			internal void VerifyExpandCollapseStateOnPeer()
			{
				if (_peer == null)
					return;

				ExpandCollapseState newState = ((IExpandCollapseProvider)_peer).ExpandCollapseState;

				if (newState != _lastExpandCollapseState)
				{
					_peer.RaisePropertyChangedEvent(ExpandCollapsePatternIdentifiers.ExpandCollapseStateProperty, _lastExpandCollapseState, newState);

					XamTileAutomationPeer tilePeer = _peer.GetTilePeer();

					if (tilePeer != null)
						tilePeer.RaisePropertyChangedEvent(ExpandCollapsePatternIdentifiers.ExpandCollapseStateProperty, _lastExpandCollapseState, newState);

					_lastExpandCollapseState = newState;
				}
			}

				#endregion //VerifyExpandCollapseStateOnPeer	
    
            #endregion //Internal Methods	
    
        #endregion //Methods

        #region ISparseArrayMultiItem Members

        object ISparseArrayMultiItem.GetItemAtScrollIndex(int scrollIndex)
        {
            Debug.Assert(scrollIndex == 0);
            return this;
        }

        int ISparseArrayMultiItem.ScrollCount
        {
            get { return this.OccupiesScrollPosition ? 1 : 0; }
        }

        #endregion

        #region ISparseArrayItem Members

        object ISparseArrayItem.GetOwnerData(SparseArray context)
        {
            return this._sparseArrayOwnerData;
        }

        void ISparseArrayItem.SetOwnerData(object ownerData, SparseArray context)
        {
            this._sparseArrayOwnerData = ownerData;
        }

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