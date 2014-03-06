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

namespace Infragistics.Windows.Tiles
{
    /// <summary>
    /// Contains settings that are used to lay out Tiles inside a <see cref="TilesPanel"/> or <see cref="XamTilesControl"/> when not in maximized mode.
    /// </summary>
    /// <seealso cref="XamTilesControl.IsInMaximizedMode"/>
    /// <seealso cref="XamTilesControl.NormalModeSettings"/>
    [InfragisticsFeature(Version = FeatureInfo.Version_10_1, FeatureName = FeatureInfo.FeatureName_TilesPanel)]
    public class NormalModeSettings : ModeSettingsBase
    {
        #region Private members
        
        private TileConstraints _tileConstraints;

        #endregion //Private members	

        #region Properties

            #region Public Properties

                #region AllowTileSizing

        /// <summary>
        /// Identifies the <see cref="AllowTileSizing"/> dependency property
        /// </summary>
        public static readonly DependencyProperty AllowTileSizingProperty = DependencyProperty.Register("AllowTileSizing",
            typeof(AllowTileSizing), typeof(NormalModeSettings), new FrameworkPropertyMetadata(AllowTileSizing.Synchronized));

        /// <summary>
        /// Gets/sets whether the user will be allowed to re-size tiles.
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> the 'Synchronized' setting (the default) is ignored in maximized mode or if <see cref="TileLayoutOrder"/> is set to 'UseExplicitRowColumnOnTile'. 
        /// Also, if TileLayoutOrder is set to 'HorizontalVariable' only the synchronized height will be used. Likewise, if it is set to 'VerticalVaraible' only
        /// the synchronized width will be used.</para>
        /// </remarks>
        /// <seealso cref="AllowTileSizingProperty"/>
        //[Description("Gets/sets whether the user will be allowed to re-size tiles.")]
        //[Category("TilesControl Properties")]
        public AllowTileSizing AllowTileSizing
        {
            get
            {
                return (AllowTileSizing)this.GetValue(NormalModeSettings.AllowTileSizingProperty);
            }
            set
            {
                this.SetValue(NormalModeSettings.AllowTileSizingProperty, value);
            }
        }

                #endregion //AllowTileSizing

				// JJD 5/9/11 - TFS74206 - added 
                #region ExplicitLayoutTileSizeBehavior

        /// <summary>
        /// Identifies the <see cref="ExplicitLayoutTileSizeBehavior"/> dependency property
        /// </summary>
        public static readonly DependencyProperty ExplicitLayoutTileSizeBehaviorProperty = DependencyProperty.Register("ExplicitLayoutTileSizeBehavior",
            typeof(ExplicitLayoutTileSizeBehavior), typeof(NormalModeSettings), new FrameworkPropertyMetadata(ExplicitLayoutTileSizeBehavior.SynchronizeTileWidthsAndHeights), new ValidateValueCallback(ValidateExplicitLayoutTileSizeBehavior));

        private static bool ValidateExplicitLayoutTileSizeBehavior(object value)
        {
            return Enum.IsDefined(typeof(ExplicitLayoutTileSizeBehavior), value);
        }

		/// <summary>
		/// Gets/sets whether tile heights are synchronized across columns and whether tile widths are synchronized across rows when <see cref="TileLayoutOrder"/> is set to 'UseExplicitRowColumnOnTile'
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// If there are ColumnsSpan values specified > 1 on one or more tiles then all tiles in intersecting columns will behave as if this setting was 'SynchronizeTileWidthsAndHeights' with respect to all other tiles in those intersecting columns.</para>
		/// <para class="body">
		/// Likewise, if there are RowSpan values specified > 1 on one or more tiles then all tiles in intersecting rows will behave as if this setting was 'SynchronizeTileWidthsAndHeights' with respect to all other tiles in those intersecting rows.</para>
		/// <para class="note"><b>Note:</b> regardless of the value of this setting if the overall size is constrained (e.g. if the HorizontalTileAreaAlignment and/or VerticalTileAreaAlignment is set to 'Stretch') 
		/// then resizing a tile's width may indirectly affect the width of all tiles and resizing its height may indirectly affect the height of all tiles respectively.
		/// </para>
		/// </remarks>
		/// <seealso cref="TileLayoutOrder"/>
        /// <seealso cref="ExplicitLayoutTileSizeBehaviorProperty"/>
		//[Description("Gets/sets whether tile heights are synchronized across columns and whether tile widths are synchronized across rows when TileLayoutOrder is set to 'UseExplicitRowColumnOnTile'.")]
        //[Category("TilesControl Properties")]
        public ExplicitLayoutTileSizeBehavior ExplicitLayoutTileSizeBehavior
        {
            get
            {
                return (ExplicitLayoutTileSizeBehavior)this.GetValue(NormalModeSettings.ExplicitLayoutTileSizeBehaviorProperty);
            }
            set
            {
                this.SetValue(NormalModeSettings.ExplicitLayoutTileSizeBehaviorProperty, value);
            }
        }

                #endregion //ExplicitLayoutTileSizeBehavior

                #region MaxColumns

        /// <summary>
        /// Identifies the <see cref="MaxColumns"/> dependency property
        /// </summary>
        public static readonly DependencyProperty MaxColumnsProperty = DependencyProperty.Register("MaxColumns",
            typeof(int), typeof(NormalModeSettings), new FrameworkPropertyMetadata(0), new ValidateValueCallback(ValidateMaxRowColumn));

        /// <summary>
        /// Gets/sets the maximum number of columns to use when arranging tiles in 'Normal' mode..
        /// </summary>
        /// <seealso cref="MaxColumnsProperty"/>
        //[Description("Gets/sets the maximum number of columns to use when arranging tiles in 'Normal' mode..")]
        //[Category("TilesControl Properties")]
        public int MaxColumns
        {
            get
            {
                return (int)this.GetValue(NormalModeSettings.MaxColumnsProperty);
            }
            set
            {
                this.SetValue(NormalModeSettings.MaxColumnsProperty, value);
            }
        }

                #endregion //MaxColumns

                #region MaxRows

        /// <summary>
        /// Identifies the <see cref="MaxRows"/> dependency property
        /// </summary>
        public static readonly DependencyProperty MaxRowsProperty = DependencyProperty.Register("MaxRows",
            typeof(int), typeof(NormalModeSettings), new FrameworkPropertyMetadata(0), new ValidateValueCallback(ValidateMaxRowColumn));

        /// <summary>
        /// Gets/sets the maximum number of rows to use when arranging tiles in 'Normal' mode..
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note</b>: a value of zero, which is the default, represents unlimited.</para>
        /// </remarks>
        /// <seealso cref="MaxRowsProperty"/>
        //[Description("Gets/sets the maximum number of rows to use when arranging tiles in 'Normal' mode..")]
        //[Category("TilesControl Properties")]
        public int MaxRows
        {
            get
            {
                return (int)this.GetValue(NormalModeSettings.MaxRowsProperty);
            }
            set
            {
                this.SetValue(NormalModeSettings.MaxRowsProperty, value);
            }
        }

                #endregion //MaxRows

                #region MinColumns

        /// <summary>
        /// Identifies the <see cref="MinColumns"/> dependency property
        /// </summary>
        public static readonly DependencyProperty MinColumnsProperty = DependencyProperty.Register("MinColumns",
            typeof(int), typeof(NormalModeSettings), new FrameworkPropertyMetadata(1), new ValidateValueCallback(ValidateMinRowColumn));

        /// <summary>
        /// Gets/sets the minimum number of columns to use when arranging tiles in 'Normal' mode..
        /// </summary>
        /// <seealso cref="MinColumnsProperty"/>
        //[Description("Gets/sets the minimum number of columns to use when arranging tiles in 'Normal' mode..")]
        //[Category("TilesControl Properties")]
        public int MinColumns
        {
            get
            {
                return (int)this.GetValue(NormalModeSettings.MinColumnsProperty);
            }
            set
            {
                this.SetValue(NormalModeSettings.MinColumnsProperty, value);
            }
        }

                #endregion //MinColumns

                #region MinRows

        /// <summary>
        /// Identifies the <see cref="MinRows"/> dependency property
        /// </summary>
        public static readonly DependencyProperty MinRowsProperty = DependencyProperty.Register("MinRows",
            typeof(int), typeof(NormalModeSettings), new FrameworkPropertyMetadata(1), new ValidateValueCallback(ValidateMinRowColumn));

        /// <summary>
        /// Gets/sets the minimum number of rows to use when arranging tiles in 'Normal' mode..
        /// </summary>
        /// <seealso cref="MinRowsProperty"/>
        //[Description("Gets/sets the minimum number of rows to use when arranging tiles in 'Normal' mode..")]
        //[Category("TilesControl Properties")]
        public int MinRows
        {
            get
            {
                return (int)this.GetValue(NormalModeSettings.MinRowsProperty);
            }
            set
            {
                this.SetValue(NormalModeSettings.MinRowsProperty, value);
            }
        }

                #endregion //MinRows

                #region ShowAllTiles

        /// <summary>
        /// Identifies the <see cref="ShowAllTiles"/> dependency property
        /// </summary>
        public static readonly DependencyProperty ShowAllTilesProperty = DependencyProperty.Register("ShowAllTiles",
            typeof(bool), typeof(NormalModeSettings), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

        /// <summary>
        /// Gets/sets whether all tiles are arranged in view
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> even if this property is set to true it is still possible that all the tiles don't fit, e.g. if there isn't enough space to satisfy their minimum size constraints. In this case, scrollbars will appear.</para>
        /// </remarks>
        /// <seealso cref="ShowAllTilesProperty"/>
        /// <seealso cref="MaximizedModeSettings.ShowAllMinimizedTiles"/>
        /// <seealso cref="NormalModeSettings.TileConstraints"/>
        //[Description("Gets/sets whether all tiles are arranged in view")]
        //[Category("TilesControl Properties")]
        public bool ShowAllTiles
        {
            get
            {
                return (bool)this.GetValue(NormalModeSettings.ShowAllTilesProperty);
            }
            set
            {
                this.SetValue(NormalModeSettings.ShowAllTilesProperty, value);
            }
        }

                #endregion //ShowAllTiles

                #region TileConstraints

        /// <summary>
        /// Identifies the <see cref="TileConstraints"/> dependency property
        /// </summary>
        public static readonly DependencyProperty TileConstraintsProperty = DependencyProperty.Register("TileConstraints",
            typeof(TileConstraints), typeof(NormalModeSettings), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnTileConstraintsChanged)));

        private static void OnTileConstraintsChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            NormalModeSettings settings = target as NormalModeSettings;

            // unwire the property changed of the old constraints
            if (settings._tileConstraints != null)
                settings._tileConstraints.PropertyChanged -= new PropertyChangedEventHandler(settings.OnTileConstraintsPropertyChanged);

            settings._tileConstraints = e.NewValue as TileConstraints;

            // wire up the property changed of the new constraints
            if (settings._tileConstraints != null)
                settings._tileConstraints.PropertyChanged += new PropertyChangedEventHandler(settings.OnTileConstraintsPropertyChanged);
        }

        private void OnTileConstraintsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.RaisePropertyChangedEvent("TileConstraints");
        }

        /// <summary>
        /// Gets/sets the constraints use for sizing a tile when it's <see cref="Tile.State"/> is 'Normal'
        /// </summary>
        /// <seealso cref="TileConstraintsProperty"/>
        //[Description("Gets/sets the constraints use for sizing a tile when it's Stae is 'Normal'")]
        //[Category("TilesControl Properties")]
        [RefreshProperties(RefreshProperties.Repaint)]
        public TileConstraints TileConstraints
        {
            get
            {
                return this._tileConstraints;
            }
            set
            {
                this.SetValue(NormalModeSettings.TileConstraintsProperty, value);
            }
        }

                #endregion //TileConstraints

                #region TileLayoutOrder

        /// <summary>
        /// Identifies the <see cref="TileLayoutOrder"/> dependency property
        /// </summary>
        public static readonly DependencyProperty TileLayoutOrderProperty = DependencyProperty.Register("TileLayoutOrder",
            typeof(TileLayoutOrder), typeof(NormalModeSettings), new FrameworkPropertyMetadata(TileLayoutOrder.Vertical), new ValidateValueCallback(ValidateTileLayoutOrder));

        private static bool ValidateTileLayoutOrder(object value)
        {
            return Enum.IsDefined(typeof(TileLayoutOrder), value);
        }

        /// <summary>
        /// Gets/sets how the panel will layout the tiles.
        /// </summary>
        /// <seealso cref="TileLayoutOrderProperty"/>
        //[Description("Gets/sets how the panel will layout the tiles.")]
        //[Category("TilesControl Properties")]
        public TileLayoutOrder TileLayoutOrder
        {
            get
            {
                return (TileLayoutOrder)this.GetValue(NormalModeSettings.TileLayoutOrderProperty);
            }
            set
            {
                this.SetValue(NormalModeSettings.TileLayoutOrderProperty, value);
            }
        }

                #endregion //TileLayoutOrder

            #endregion //Public Properties

        #endregion //Properties

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