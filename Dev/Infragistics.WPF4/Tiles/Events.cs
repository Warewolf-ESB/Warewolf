using System;
using System.Collections.Generic;
using System.Text;
using Infragistics.Windows.Controls.Events;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Media;
using System.IO;
using Infragistics.Shared;

namespace Infragistics.Windows.Tiles.Events
{
    #region LoadingItemMappingEventArgs

    /// <summary>
    /// Event arguments for routed event <see cref="XamTilesControl.LoadingItemMapping"/>
    /// </summary>
    /// <seealso cref="XamTilesControl.LoadLayout(Stream)"/>
    /// <seealso cref="XamTilesControl.LoadingItemMapping"/>
    /// <seealso cref="XamTilesControl.LoadingItemMappingEvent"/>
    [InfragisticsFeature(Version = FeatureInfo.Version_10_1, FeatureName = FeatureInfo.FeatureName_TilesPanel)]
    public class LoadingItemMappingEventArgs : RoutedEventArgs
    {
        #region Member Variables

        private object _item;
        private string _serializationId;
        private bool _isItemInitialized;
        private XamTilesControl _tilesControl;

        #endregion //Member Variables

        #region Constructor
        internal LoadingItemMappingEventArgs(XamTilesControl tilesControl, string serializationId)
        {
            this._tilesControl = tilesControl;
            this._serializationId = serializationId;
        }
        #endregion //Constructor

        #region Item
        /// <summary>
        /// Gets/sets the item whose information has just been de-serialized.
        /// </summary>
        public object Item
        {
            get 
            {
                if ( this._isItemInitialized )
                    return this._item;

                this._item = this._tilesControl.GetItemFromSerializationId(this._serializationId);

                this._isItemInitialized = true;
                return this._item; 
            }
            set 
            { 
                this._item = value;
                this._isItemInitialized = true;
            }
        }
        #endregion //Item

        #region SerializationId
        /// <summary>
        /// Represents a string that will be serialized to identify the item. (read-only).
        /// </summary>
        public string SerializationId
        {
            get { return this._serializationId; }
        }
        #endregion //SerializationId
    }

    #endregion //LoadingItemMappingEventArgs

    #region SavingItemMappingEventArgs

    /// <summary>
    /// Event arguments for routed event <see cref="XamTilesControl.SavingItemMapping"/>
    /// </summary>
    /// <seealso cref="XamTilesControl.SaveLayout()"/>
    /// <seealso cref="XamTilesControl.SavingItemMapping"/>
    /// <seealso cref="XamTilesControl.SavingItemMappingEvent"/>
    [InfragisticsFeature(Version = FeatureInfo.Version_10_1, FeatureName = FeatureInfo.FeatureName_TilesPanel)]
    public class SavingItemMappingEventArgs : RoutedEventArgs
    {
        #region Member Variables

        private object _item;
        private string _serializationId;
        private bool _isSerializationIdInitialized;
        private Tile _tile;
        private XamTilesControl _tilesControl;

        #endregion //Member Variables

        #region Constructor
        internal SavingItemMappingEventArgs(Tile tile, XamTilesControl tilesControl, object item)
        {
            TileUtilities.ThrowIfNull(item, "item");

            this._tile              = tile;
            this._tilesControl      = tilesControl;
            this._item              = item;

            if (this._tile == null)
                this._tile = item as Tile;
        }
        #endregion //Constructor

        #region Item
        /// <summary>
        /// Returns item whose information is about to be serialized. (read-only).
        /// </summary>
        public object Item
        {
            get { return this._item; }
        }
        #endregion //Item

        #region SerializationId
        /// <summary>
        /// Represents a string that will be serialized to identify the item.
        /// </summary>
        /// <remarks>
        /// <p class="body">By default this property is set to the associated <see cref="Tile"/>'s <see cref="Tile.SerializationId"/> or
        /// if that is not specified then the Tile's <see cref="FrameworkElement.Name"/>.</p>
        /// <p class="note"><b>Note:</b> If this property is still null after the event is processed then no information for this item will be serialized.</p>
        /// </remarks>
        public string SerializationId
        {
            get 
            {
                if ( this._isSerializationIdInitialized == true )
                    return this._serializationId;

                this._serializationId = this._tilesControl.GetSerializationIdFromItem(this._tile, this._item);

                this._isSerializationIdInitialized = true;
                
                return this._serializationId;
            }
            set 
            { 
                this._serializationId = value;
                this._isSerializationIdInitialized = true;
            }
        }
        #endregion //SerializationId
    }

    #endregion //SavingItemMappingEventArgs

    #region CancelableTileEventArgs

    /// <summary>
    /// Abstract base class for cancelable tile event args
    /// </summary>
    [InfragisticsFeature(Version = FeatureInfo.Version_10_1, FeatureName = FeatureInfo.FeatureName_TilesPanel)]
    public abstract class CancelableTileEventArgs : CancelableRoutedEventArgs
    {
        #region Private Members

        private Tile _tile;
        private object _item;
 
        #endregion //Private Members

        #region Constructor

        /// <summary>
        /// Instantiates a new instance of <see cref="CancelableTileEventArgs"/>
        /// </summary>
        /// <param name="tile">The affected tile</param>
        /// <param name="item">The affected item</param>
        protected CancelableTileEventArgs(Tile tile, object item)
        {
            this._tile = tile;
            this._item = item;
         }

        #endregion //Constructor

        #region Properties

        /// <summary>
        /// Returns the <see cref="Tile"/> that is affected (read-only).
        /// </summary>
        public Tile Tile { get { return this._tile; } }

        /// <summary>
        /// Returns the affected item (read-only).
        /// </summary>
        public object Item { get { return this._item; } }

        #endregion //Properties

    }

    #endregion //TileClosingEventArgs

    #region TileEventArgs

    /// <summary>
    /// Abstract base class for tile event args that are not cancelable
    /// </summary>
    [InfragisticsFeature(Version = FeatureInfo.Version_10_1, FeatureName = FeatureInfo.FeatureName_TilesPanel)]
    public abstract class TileEventArgs : RoutedEventArgs
    {
        #region Private Members

        private Tile _tile;
        private object _item;
 
        #endregion //Private Members

        #region Constructor

        /// <summary>
        /// Instantiates a new instance of <see cref="TileEventArgs"/>
        /// </summary>
        /// <param name="tile">The affected tile</param>
        /// <param name="item">The affected item</param>
        protected TileEventArgs(Tile tile, object item)
        {
            this._tile = tile;
            this._item = item;
         }

        #endregion //Constructor

        #region Properties

        /// <summary>
        /// Returns the <see cref="Tile"/> that is affected (read-only).
        /// </summary>
        public Tile Tile { get { return this._tile; } }

        /// <summary>
        /// Returns the affected item (read-only).
        /// </summary>
        public object Item { get { return this._item; } }

        #endregion //Properties

    }

    #endregion //TileClosingEventArgs

    #region TileClosingEventArgs

    /// <summary>
    /// Event arguments for routed event <see cref="XamTilesControl.TileClosing"/>
    /// </summary>
    [InfragisticsFeature(Version = FeatureInfo.Version_10_1, FeatureName = FeatureInfo.FeatureName_TilesPanel)]
    public class TileClosingEventArgs : CancelableTileEventArgs
    {
        #region Private Members
 
        #endregion //Private Members

        #region Constructor

        /// <summary>
        /// Instantiates a new instance of <see cref="TileClosingEventArgs"/>
        /// </summary>
        /// <param name="tile">The tile that is closing</param>
        /// <param name="item">The associated item</param>
        public TileClosingEventArgs(Tile tile, object item) : base(tile, item)
        {
        }

        #endregion //Constructor

    }

    #endregion //TileClosingEventArgs

    #region TileClosedEventArgs

    /// <summary>
    /// Event arguments for routed event <see cref="XamTilesControl.TileClosed"/>
    /// </summary>
    [InfragisticsFeature(Version = FeatureInfo.Version_10_1, FeatureName = FeatureInfo.FeatureName_TilesPanel)]
    public class TileClosedEventArgs : TileEventArgs
    {
        #region Private Members

        #endregion //Private Members

        #region Constructor

        /// <summary>
        /// Instantiates a new instance of <see cref="TileClosedEventArgs"/>
        /// </summary>
        /// <param name="tile">The tile that was closed</param>
        /// <param name="item">The associated item</param>
        public TileClosedEventArgs(Tile tile, object item) : base(tile, item)
        {
        }

        #endregion //Constructor

    }

    #endregion //TileClosedEventArgs

    #region TileDraggingEventArgs

    /// <summary>
    /// Event arguments for routed event <see cref="XamTilesControl.TileDragging"/>
    /// </summary>
    [InfragisticsFeature(Version = FeatureInfo.Version_10_1, FeatureName = FeatureInfo.FeatureName_TilesPanel)]
    public class TileDraggingEventArgs : CancelableTileEventArgs
    {
        #region Private Members
 
        #endregion //Private Members

        #region Constructor

        /// <summary>
        /// Instantiates a new instance of <see cref="TileDraggingEventArgs"/>
        /// </summary>
        /// <param name="tile">The tile that is about to be dragged</param>
        /// <param name="item">The associated item</param>
        public TileDraggingEventArgs(Tile tile, object item) : base(tile, item)
        {
        }

        #endregion //Constructor

    }

    #endregion //TileDraggingEventArgs

    #region TileStateChangingEventArgs

    /// <summary>
    /// Event arguments for routed event <see cref="XamTilesControl.TileStateChanging"/>
    /// </summary>
    /// <seealso cref="XamTilesControl.TileStateChanging"/>
    /// <seealso cref="XamTilesControl.TileStateChangingEvent"/>
    [InfragisticsFeature(Version = FeatureInfo.Version_10_1, FeatureName = FeatureInfo.FeatureName_TilesPanel)]
    public class TileStateChangingEventArgs : CancelableTileEventArgs
    {
        #region Private Members

        private TileState _newState;

        #endregion //Private Members

        #region Constructor

        /// <summary>
        /// Instantiates a new instance of <see cref="TileStateChangingEventArgs"/>
        /// </summary>
        /// <param name="tile">The tile whose state is changing</param>
        /// <param name="item">The associated item</param>
        /// <param name="newState">The new state being set.</param>
        public TileStateChangingEventArgs(Tile tile, object item, TileState newState) : base(tile, item)
        {
            this._newState = newState;
        }

        #endregion //Constructor

        #region Properties

        /// <summary>
        /// The state that the <see cref="Tile"/> is about to be changed to.
        /// </summary>
        public TileState NewState 
        { 
            get { return this._newState; }
            set { this._newState = value; }
        }

        #endregion //Properties

    }

    #endregion //TileStateChangingEventArgs

    #region TileStateChangedEventArgs

    /// <summary>
    /// Event arguments for routed event <see cref="XamTilesControl.TileStateChanged"/>
    /// </summary>
    /// <seealso cref="XamTilesControl.TileStateChanged"/>
    /// <seealso cref="XamTilesControl.TileStateChangedEvent"/>
    [InfragisticsFeature(Version = FeatureInfo.Version_10_1, FeatureName = FeatureInfo.FeatureName_TilesPanel)]
    public class TileStateChangedEventArgs : TileEventArgs
    {
        #region Private Members

        private TileState _newState;
        private TileState _oldState;

        #endregion //Private Members

        #region Constructor

        /// <summary>
        /// Instantiates a new instance of <see cref="TileStateChangedEventArgs"/>
        /// </summary>
        /// <param name="tile">The tile whose state was changed.</param>
        /// <param name="item">The associated item</param>
        /// <param name="newState">The new state of the tile.</param>
        /// <param name="oldState">The old state of the tile.</param>
        public TileStateChangedEventArgs(Tile tile, object item, TileState newState, TileState oldState)
            : base(tile, item)
        {
            this._newState  = newState;
            this._oldState  = oldState;
        }

        #endregion //Constructor

        #region Properties

        /// <summary>
        /// The new state of the <see cref="Tile"/>.
        /// </summary>
        public TileState NewState
        {
            get { return this._newState; }
        }

        /// <summary>
        /// The old state of the <see cref="Tile"/>.
        /// </summary>
        public TileState OldState
        {
            get { return this._oldState; }
        }

        #endregion //Properties

    }

    #endregion //TileStateChangedEventArgs

    #region TileSwappingEventArgs

    /// <summary>
    /// Event arguments for routed event <see cref="XamTilesControl.TileSwapping"/>
    /// </summary>
    /// <seealso cref="XamTilesControl.TileSwapping"/>
    /// <seealso cref="XamTilesControl.TileSwappingEvent"/>
    [InfragisticsFeature(Version = FeatureInfo.Version_10_1, FeatureName = FeatureInfo.FeatureName_TilesPanel)]
    public class TileSwappingEventArgs : CancelableTileEventArgs
    {
        #region Private Members

        private Tile _targetTile;
        private object _targetItem;

		// JJD 11/1/11 - TFS88171 - added
		private bool _swapIsExpandedWhenMinimized;

        #endregion //Private Members

        #region Constructor

        /// <summary>
        /// Instantiates a new instance of <see cref="TileSwappingEventArgs"/>
        /// </summary>
        /// <param name="tile">The tile that is being dragged</param>
        /// <param name="item">The associated item</param>
        /// <param name="targetTile">The tile that is the target of the swap</param>
        /// <param name="targetItem">he associated item of the target tile.</param>
        public TileSwappingEventArgs(Tile tile, object item, Tile targetTile, object targetItem) : base(tile, item)
        {
            this._targetTile = targetTile;
            this._targetItem = targetItem;
        }

        #endregion //Constructor

        #region Properties

		// JJD 11/1/11 - TFS88171 - added
		#region SwapIsExpandedWhenMinimized

		/// <summary>
		/// Gets/sets whether the <see cref="CancelableTileEventArgs.Tile"/>'s <see cref="Tile.IsExpandedWhenMinimized"/> setting will be swapped with the <see cref="TargetTile"/>'s setting.
		/// </summary>
		/// <remarks>
		/// <para class="note"><b>Note:</b> when swapping a maximized tile with a minimized tile this property will be defaulted to 'true'. Otherwise, it will be defaulted to 'false'.</para>
		/// </remarks>
		/// <seealso cref="Tile.IsExpandedWhenMinimized"/>
		public bool SwapIsExpandedWhenMinimized
		{
			get { return this._swapIsExpandedWhenMinimized; }
			set { _swapIsExpandedWhenMinimized = value; }
		}

		#endregion //SwapIsExpandedWhenMinimized	
    
		#region TargetItem

		/// <summary>
		/// Returns the associated item of the <see cref="TargetTile"/> (read-only).
		/// </summary>
		public object TargetItem
		{
			get { return this._targetItem; }
		}

		#endregion //TargetItem	
    
		#region TargetTile

		/// <summary>
		/// Returns the target of the swap (read-only).
		/// </summary>
		public Tile TargetTile
		{
			get { return this._targetTile; }
		}

		#endregion //TargetTile	
    
        #endregion //Properties

    }

    #endregion //TileSwappingEventArgs

    #region TileSwappedEventArgs

    /// <summary>
    /// Event arguments for routed event <see cref="XamTilesControl.TileSwapped"/>
    /// </summary>
    /// <seealso cref="XamTilesControl.TileSwapped"/>
    /// <seealso cref="XamTilesControl.TileSwappedEvent"/>
    [InfragisticsFeature(Version = FeatureInfo.Version_10_1, FeatureName = FeatureInfo.FeatureName_TilesPanel)]
    public class TileSwappedEventArgs : TileEventArgs
    {
        #region Private Members

        private Tile _targetTile;
        private object _targetItem;

        #endregion //Private Members

        #region Constructor

        /// <summary>
        /// Instantiates a new instance of <see cref="TileSwappedEventArgs"/>
        /// </summary>
        /// <param name="tile">The tile that was just dropped</param>
        /// <param name="item">The associated item</param>
        /// <param name="targetTile">The tile that was the target of the swap</param>
        /// <param name="targetItem">he associated item of the target tile.</param>
        public TileSwappedEventArgs(Tile tile, object item, Tile targetTile, object targetItem) : base(tile, item)
        {
            this._targetTile = targetTile;
            this._targetItem = targetItem;
        }

        #endregion //Constructor

        #region Properties

        /// <summary>
        /// Returns the associated item of the <see cref="TargetTile"/> (read-only).
        /// </summary>
        public object TargetItem 
        { 
            get { return this._targetItem; }
        }

        /// <summary>
        /// Returns the target of the swap (read-only).
        /// </summary>
        public Tile TargetTile 
        { 
            get { return this._targetTile; }
        }

        #endregion //Properties

    }

    #endregion //TileSwappedEventArgs

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