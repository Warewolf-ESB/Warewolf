using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;
using System.Windows.Markup;
using System.Diagnostics;
using System.Windows.Threading;
using System.ComponentModel;
using System.Windows.Documents;
using System.Windows.Shapes;

using Infragistics.Windows.Selection;
using Infragistics.Windows.Licensing;
using Infragistics.Windows.Controls.Events;
using Infragistics.Windows.Helpers;
using Infragistics.Windows.Commands;
using Infragistics.Windows.Virtualization;
using System.Xml;
using Infragistics.Controls.Layouts.Primitives;
using Infragistics.Windows.Resizing;
using Infragistics.Windows.Internal;
using Infragistics.Collections;

namespace Infragistics.Windows.Tiles
{
    /// <summary>
    /// An object that maintains certain status information for an item in a tile
    /// </summary>
    [InfragisticsFeature(Version = FeatureInfo.Version_10_1, FeatureName = FeatureInfo.FeatureName_TilesPanel)]
    public class ItemInfoBase : PropertyChangeNotifier, ISparseArrayMultiItem
    {
        #region Private Members

        private TileManager  _manager;
        private object      _item;
        private int         _index;
        private object      _sparseArrayOwnerData;
        private Size?       _preferredSizeOverride;

        #endregion //Private Members	
 
        #region Constructor

        /// <summary>
        /// Instantiates and instance of a <see cref="ItemInfoBase"/>
        /// </summary>
        /// <param name="manager">The asociated managae</param>
        /// <param name="item">The item</param>
        /// <param name="index">The orginal index of the item (not required)</param>
        internal protected ItemInfoBase(TileManager manager, object item, int index)
        {
            this._manager   = manager;
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
                IList items = this._manager.Items;

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
        /// <value>A zero-based index reprresenting the item's relative position in the display or -1 if the item was not found.</value>
        /// <remarks>
        /// <para class="note"><b>Note:</b> this will return the same value as the <see cref="Index"/> property unless the user has re-positioned one or more <see cref="Tiles"/> in which case the logical index represents the position of this item relative to other items.</para>
        /// </remarks>
        [ReadOnly(true)]
        [Bindable(false)]
        public int LogicalIndex
        {
            get
            {
                return this._manager.GetLogicalIndexOfItem(this._item, false);
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
                return this.GetIsClosed() == false && this.GetIsMaximized() == false;
            }
        }

                #endregion //OccupiesScrollPosition

                #region ScrollPosition

        /// <summary>
        /// Gets the overall scroll position of this item (read-only).
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> ietms that are collapsed or maximized are not included in the scroll count.</para>
        /// </remarks>
        /// <value>The zero based scroll position or -1 for collapsed or maximized items.</value>
        [ReadOnly(true)]
        [Bindable(false)]
        public int ScrollPosition
        {
            get
            {
                return this._manager.GetLogicalIndexOfItem(this._item, true);
            }
        }

                #endregion //ScrollPosition
    
            #endregion //Public Properties	
 
            #region Internal Properties

                #region IsInSparseArray

            internal bool IsInSparseArray { get { return this._sparseArrayOwnerData != null; } }

                #endregion //IsInSparseArray	

            #endregion //Internal Properties
 
            #region Protected Properties
    
                #region Manager

            /// <summary>
            /// Returns the asscciated manager (read-only)
            /// </summary>
            protected TileManager Manager { get { return this._manager; } }

                #endregion //Manager	

                #region PreferredSizeOverride

            /// <summary>
            /// Gets/sets an explicit size to be returned as the item's preferred size.
            /// </summary>
            /// <remarks>
            /// <para class="note"><b>Note:</b> this setting may be ignored if a syncronized size is being used. Otherwise, if this property returns a width and/or height > 0 it will take precedence over all other settings but only when the item's state is normal.</para>
            /// </remarks>
            internal protected Size? PreferredSizeOverride
            {
                get { return this._preferredSizeOverride; }
                set { this._preferredSizeOverride = value; }
            }

                #endregion //PreferredSizeOverride	

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
                    this._manager.BringIndexIntoView(index);
            }

                #endregion //BringIntoView	
    
                #region ClearSizeCustomization

            /// <summary>
            /// This clears any individual resize operations for this item 
            /// </summary>
            public void ClearSizeCustomization()
            {
                this.PreferredSizeOverride = null;
            }

                #endregion //ClearSizeCustomization
    
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
                return this._manager.GetCurrentColumn(this);
            }

                #endregion //GetCurrentColumn
    
                #region GetCurrentColumn

            /// <summary>
            /// Returns the resolved constraints of this item in the current display 
            /// </summary>
            /// <returns>An object that implements the <see cref="ITileConstraints"/> interface for this item, if the item is currently displayed, otherwise it returns null.</returns>
            public ITileConstraints GetCurrentConstraints() 
            {
                if ( !this._manager.IsItemInView(this))
                    return null;

                TileManager.LayoutItem li = this._manager.GetLayoutItem(this);

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
                return this._manager.GetCurrentRow(this);
            }

            #endregion //GetCurrentRow
    
                #region GetCurrentTargetRect

            /// <summary>
            /// Returns the current target rect of the item 
            /// </summary>
            /// <returns>The current target rect, after animations have been completed. If the item will not be in view then Rect.Empty will be returned.</returns>
            public Rect GetCurrentTargetRect() 
            {
                TilesPanelBase panel = this._manager.Panel;

                if (panel != null)
                    return panel.GetTargetRectOfItem(this.Item);

                return Rect.Empty;
            }

                #endregion //GetCurrentTargetRect

            #endregion //Public Methods	
        
            #region Protected Methods

                #region GetIsClosed

            /// <summary>
            /// Determines if the item is closed
            /// </summary>
            /// <returns>True if the tile is closed</returns>
            internal protected virtual bool GetIsClosed() { return false; }

                #endregion //GetIsClosed

                #region GetIsMaximized

            /// <summary>
            /// Determines if the tile is maximized
            /// </summary>
            /// <returns>True if the item is maximized</returns>
            internal protected virtual bool GetIsMaximized() { return false; }

                #endregion //GetIsMaximized

                #region GetResizeRange

            /// <summary>
            /// Returns the resize range for this item
            /// </summary>
            /// <param name="maxDeltaLeft"></param>
            /// <param name="maxDeltaRight"></param>
            /// <param name="maxDeltaTop"></param>
            /// <param name="maxDeltaBottom"></param>
            protected void GetResizeRange(out double maxDeltaLeft, out double maxDeltaRight,
                                            out double maxDeltaTop, out double maxDeltaBottom)
            {
                this._manager.GetResizeRange(this, out maxDeltaLeft, out maxDeltaRight, out maxDeltaTop, out maxDeltaBottom);
            }

                #endregion //GetResizeRange

                #region OnResize

            /// <summary>
            /// Called when the item is resized
            /// </summary>
            /// <param name="deltaX"></param>
            /// <param name="deltaY"></param>
            protected void OnResize(double deltaX, double deltaY)
            {
                this._manager.OnItemResized(this, deltaX, deltaY);
            }

                #endregion //OnResize

                #region InvalidateScrollPosition

            /// <summary>
            /// Invalidates the scroll position.
            /// </summary>
            /// <remarks>
            /// <para class="note"><b>Note:</b> dervied classes should call this when their IsClosed or IsMaximized state has changed</para>
            /// </remarks>
            protected void InvalidateScrollPosition()
            {
                if (this.IsInSparseArray)
                    this._manager.SparseArray.InvalidateItem(this);
            }

                #endregion //InvalidateScrollPosition

            #endregion //Protected Methods	
    
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