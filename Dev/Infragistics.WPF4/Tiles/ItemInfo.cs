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
using Infragistics.Windows.Tiles.Events;

namespace Infragistics.Windows.Tiles
{
    /// <summary>
    /// An object that maintains certain status information for an item in a <see cref="XamTilesControl"/>
    /// </summary>
    /// <seealso cref="XamTilesControl.GetItemInfo(object)"/>
    [InfragisticsFeature(Version = FeatureInfo.Version_10_1, FeatureName = FeatureInfo.FeatureName_TilesPanel)]
    public class ItemInfo : ItemInfoBase
    {
        #region Private Members

        private bool        _isClosed;
        private bool?       _isExpandedWhenMinimized;
        private bool?       _isExpandedWhenMinimizedLastReturned;
        private bool        _isMaximized;
        private LayoutManager.ItemSerializationInfo 
                            _serializationInfo;
        #endregion //Private Members	
 
        #region Constructor

        internal ItemInfo(TileManager manager, object item, int index) : base(manager, item, index)
        {
            Tile tile = this.Item as Tile;

            if ( tile != null )
                this.SyncFromTileState(tile);
        }

        #endregion //Constructor

        #region Base class overrides

            #region GetIsClosed

        /// <summary>
        /// Determines if the item is closed
        /// </summary>
        /// <returns>True if the tile is closed.</returns>
        internal protected override bool GetIsClosed()
        {
            return this._isClosed;
        }

            #endregion //GetIsClosed
    
            #region GetIsMaximized

            /// <summary>
            /// Determines if the tile is maximized
            /// </summary>
            /// <returns>True if the item is maximized.</returns>
            internal protected override bool GetIsMaximized() { return this._isMaximized; }

            #endregion //GetIsMaximized	
    
        #endregion //Base class overrides	
        
        #region Properties

            #region Public Properties
    
                #region IsClosed

        /// <summary>
        /// Gets/sets whether this item is closed.
        /// </summary>
        /// <seealso cref="XamTilesControl.TileCloseAction"/>
        /// <seealso cref="Tile.CloseAction"/>
        /// <seealso cref="Tile.IsClosed"/>
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
                
                TilesPanel panel = (TilesPanel)(this.Manager.Panel);

                if (synchronizePropsOnTile)
                {
                    Tile tile = panel != null ? panel.TileFromItem(this.Item) : null;

                    if (tile != null)
                        tile.IsClosed = newValue;

                    
                    
                    if (panel != null)
                    {
                        
                        
                        panel.OnItemIsClosedChanged(this.Item, this._isClosed, this.IsMaximized);
                    }
                }

                this.InvalidateScrollPosition();
            }
        }

                #endregion //IsClosed	

                #region IsExpandedWhenMinimized

        /// <summary>
        /// Gets/sets whether this item is expanded when it is minimized.
        /// </summary>
        /// <seealso cref="MaximizedModeSettings.MinimizedTileExpansionMode"/>
        /// <seealso cref="Tile.IsExpandedWhenMinimized"/>
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
                TilesPanel panel = (TilesPanel)(this.Manager.Panel);

                this._isExpandedWhenMinimized = newValue;
                this.RaisePropertyChangedEvent("IsExpandedWhenMinimized");

                bool resolvedValue = this.IsExpandedWhenMinimizedResolved;

                if (synchronizePropsOnTile && panel != null)
                {
                    Tile tile = panel.TileFromItem(this.Item);

                    if (tile != null)
                        tile.IsExpandedWhenMinimized = newValue;
                }
            }
        }

                #endregion //IsExpandedWhenMinimized	

                #region IsExpandedWhenMinimizedResolved

        /// <summary>
        /// Gets the whether this item is expanded when it is minimized (read-only).
        /// </summary>
        /// <seealso cref="MaximizedModeSettings.MinimizedTileExpansionMode"/>
        /// <seealso cref="Tile.IsExpandedWhenMinimized"/>
        public bool IsExpandedWhenMinimizedResolved
        {
            get
            {
                bool value = TilesPanel.GetIsExpandedWhenMinimizedHelper((TilesPanel)(this.Manager.Panel), null, this.Item, this.IsExpandedWhenMinimized);

                if (this._isExpandedWhenMinimizedLastReturned.HasValue)
                {
                    if (this._isExpandedWhenMinimizedLastReturned != value)
                    {
                        this._isExpandedWhenMinimizedLastReturned = value;
                        this.RaisePropertyChangedEvent("IsExpandedWhenMinimizedResolved");
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
        /// <seealso cref="XamTilesControl.MaximizedTileLimit"/>
        /// <seealso cref="Tile.AllowMaximize"/>
        /// <seealso cref="Tile.State"/>
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

        private void SetIsMaximizedInternal(bool newValue, bool synchronizePropsOnTile)
        {
            if (newValue != this._isMaximized)
            {
                this._isMaximized = newValue;
                this.RaisePropertyChangedEvent("IsMaximized");

                if (synchronizePropsOnTile)
                {
                    TilesPanel panel = (TilesPanel)(this.Manager.Panel);

                    Tile tile = panel != null ? panel.TileFromItem(this.Item) : null;

                    if (tile != null)
                    {
                        TileState newState = this._isMaximized
                            ? TileState.Maximized
                            : TileState.Normal;

                        panel.ChangeTileState(tile, newState);
                    }
                }

                this.InvalidateScrollPosition();
            }
        }

                #endregion //IsMaximized	
    
            #endregion //Public Properties	

            #region Internal Properties

                #region SerializationInfo

        internal LayoutManager.ItemSerializationInfo SerializationInfo
        {
            get { return this._serializationInfo; }
            set { this._serializationInfo = value; }
        }

                #endregion //SerializationInfo

                #region SizeOverride

        internal Size? SizeOverride
        {
            get { return base.PreferredSizeOverride; }
            set { base.PreferredSizeOverride = value; }
        }

                #endregion //SizeOverride	
    
            #endregion //Internal Properties	
        
        #endregion //Properties

        #region Methods

            #region GetResizeRangeInternal

        internal void GetResizeRangeInternal(out double maxDeltaLeft, out double maxDeltaRight,
                                                out double maxDeltaTop, out double maxDeltaBottom)
        {
            base.GetResizeRange( out maxDeltaLeft, out maxDeltaRight, out maxDeltaTop, out maxDeltaBottom);
        }

            #endregion //GetResizeRangeInternal	

            #region OnResizeInternal

        internal void OnResizeInternal(double deltaX, double deltaY)
        {
            this.OnResize(deltaX, deltaY);
        }

            #endregion //InternalOnResize	
    
            #region SyncFromTileState

        internal void SyncFromTileState(Tile tile)
        {
            if (tile != null)
            {
                this.SetIsClosedInternal(tile.IsClosed, false);
                this.SetIsExpandedWhenMinimizedInternal(tile.IsExpandedWhenMinimized, false);
                this.SetIsMaximizedInternal(tile.State == TileState.Maximized, false);
            }
        }

            #endregion //SyncFromTileState	

        #endregion //Methods
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