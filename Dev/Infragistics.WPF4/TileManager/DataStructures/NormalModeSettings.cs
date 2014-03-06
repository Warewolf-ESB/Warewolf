using System;
using System.Windows;
using System.ComponentModel;

namespace Infragistics.Controls.Layouts
{
    /// <summary>
    /// Contains settings that are used to lay out Tiles inside a <see cref="XamTileManager"/> when not in maximized mode.
    /// </summary>
    /// <seealso cref="XamTileManager.IsInMaximizedMode"/>
    /// <seealso cref="XamTileManager.NormalModeSettings"/>
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
		public static readonly DependencyProperty AllowTileSizingProperty = DependencyPropertyUtilities.Register("AllowTileSizing",
			typeof(AllowTileSizing), typeof(NormalModeSettings),
			DependencyPropertyUtilities.CreateMetadata(AllowTileSizing.Synchronized, new PropertyChangedCallback(OnAllowTileSizingChanged))
			);

		private static void OnAllowTileSizingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			CoreUtilities.ValidateEnum(typeof(AllowTileSizing), e.NewValue);

			NormalModeSettings instance = d as NormalModeSettings;

			instance.RaisePropertyChangedEvent(DependencyPropertyUtilities.GetName(e.Property));
		}

        /// <summary>
        /// Gets/sets whether the user will be allowed to re-size tiles.
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> the 'Synchronized' setting (the default) is ignored in maximized mode or if <see cref="TileLayoutOrder"/> is set to 'UseExplicitRowColumnOnTile'. 
        /// Also, if TileLayoutOrder is set to 'HorizontalVariable' only the synchronized height will be used. Likewise, if it is set to 'VerticalVaraible' only
        /// the synchronized width will be used.</para>
        /// </remarks>
        /// <seealso cref="AllowTileSizingProperty"/>
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

				#region ExplicitLayoutTileSizeBehavior

		/// <summary>
		/// Identifies the <see cref="ExplicitLayoutTileSizeBehavior"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ExplicitLayoutTileSizeBehaviorProperty = DependencyPropertyUtilities.Register("ExplicitLayoutTileSizeBehavior",
			typeof(ExplicitLayoutTileSizeBehavior), typeof(NormalModeSettings),
			DependencyPropertyUtilities.CreateMetadata(ExplicitLayoutTileSizeBehavior.SynchronizeTileWidthsAndHeights, new PropertyChangedCallback(OnExplicitLayoutTileSizeBehaviorChanged))
			);

		private static void OnExplicitLayoutTileSizeBehaviorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			CoreUtilities.ValidateEnum(typeof(ExplicitLayoutTileSizeBehavior), e.NewValue);

			NormalModeSettings instance = d as NormalModeSettings;

			instance.RaisePropertyChangedEvent(DependencyPropertyUtilities.GetName(e.Property));
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

				#region HorizontalTileAreaAlignment

		/// <summary>
		/// Identifies the <see cref="HorizontalTileAreaAlignment"/> dependency property
		/// </summary>
		public static readonly DependencyProperty HorizontalTileAreaAlignmentProperty = DependencyPropertyUtilities.Register("HorizontalTileAreaAlignment",
			typeof(HorizontalAlignment), typeof(NormalModeSettings),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.HorizontalAlignmentLeftBox, new PropertyChangedCallback(OnHorizontalTileAreaAlignmentChanged))
			);

		private static void OnHorizontalTileAreaAlignmentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			CoreUtilities.ValidateEnum(typeof(HorizontalAlignment), e.NewValue);
			
			NormalModeSettings instance = d as NormalModeSettings;

			instance.RaisePropertyChangedEvent(DependencyPropertyUtilities.GetName(e.Property));
		}

        /// <summary>
        /// Determines the horizontal alignment of the complete block of visible tiles within the control.
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> For <see cref="MaximizedModeSettings"/>, this property applies to the minimized tile area only.</para>
        /// </remarks>
        /// <seealso cref="HorizontalTileAreaAlignmentProperty"/>
       public HorizontalAlignment HorizontalTileAreaAlignment
        {
            get
            {
                return (HorizontalAlignment)this.GetValue(NormalModeSettings.HorizontalTileAreaAlignmentProperty);
            }
            set
            {
                this.SetValue(NormalModeSettings.HorizontalTileAreaAlignmentProperty, value);
            }
        }

                #endregion //HorizontalTileAreaAlignment

				#region MaxColumns

		/// <summary>
		/// Identifies the <see cref="MaxColumns"/> dependency property
		/// </summary>
		public static readonly DependencyProperty MaxColumnsProperty = DependencyPropertyUtilities.Register("MaxColumns",
			typeof(int), typeof(NormalModeSettings),
			DependencyPropertyUtilities.CreateMetadata(0, new PropertyChangedCallback(OnMaxColumnsChanged))
			);

		private static void OnMaxColumnsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ValidateMaxRowColumn(e.NewValue);

			NormalModeSettings instance = d as NormalModeSettings;

			instance.RaisePropertyChangedEvent(DependencyPropertyUtilities.GetName(e.Property));
		}

        /// <summary>
        /// Gets/sets the maximum number of columns to use when arranging tiles in 'Normal' mode..
        /// </summary>
        /// <seealso cref="MaxColumnsProperty"/>
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
		public static readonly DependencyProperty MaxRowsProperty = DependencyPropertyUtilities.Register("MaxRows",
			typeof(int), typeof(NormalModeSettings),
			DependencyPropertyUtilities.CreateMetadata(0, new PropertyChangedCallback(OnMaxRowsChanged))
			);

		private static void OnMaxRowsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ValidateMaxRowColumn(e.NewValue);

			NormalModeSettings instance = d as NormalModeSettings;

			instance.RaisePropertyChangedEvent(DependencyPropertyUtilities.GetName(e.Property));
		}

        /// <summary>
        /// Gets/sets the maximum number of rows to use when arranging tiles in 'Normal' mode..
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note</b>: a value of zero, which is the default, represents unlimited.</para>
        /// </remarks>
        /// <seealso cref="MaxRowsProperty"/>
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
		public static readonly DependencyProperty MinColumnsProperty = DependencyPropertyUtilities.Register("MinColumns",
			typeof(int), typeof(NormalModeSettings),
			DependencyPropertyUtilities.CreateMetadata((int)1, new PropertyChangedCallback(OnMinColumnsChanged))
			);

		private static void OnMinColumnsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ValidateMinRowColumn(e.NewValue);

			NormalModeSettings instance = d as NormalModeSettings;

			instance.RaisePropertyChangedEvent(DependencyPropertyUtilities.GetName(e.Property));
		}

        /// <summary>
        /// Gets/sets the minimum number of columns to use when arranging tiles in 'Normal' mode..
        /// </summary>
        /// <seealso cref="MinColumnsProperty"/>
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
		public static readonly DependencyProperty MinRowsProperty = DependencyPropertyUtilities.Register("MinRows",
			typeof(int), typeof(NormalModeSettings),
			DependencyPropertyUtilities.CreateMetadata((int)1, new PropertyChangedCallback(OnMinRowsChanged))
			);

		private static void OnMinRowsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ValidateMinRowColumn(e.NewValue);

			NormalModeSettings instance = d as NormalModeSettings;

			instance.RaisePropertyChangedEvent(DependencyPropertyUtilities.GetName(e.Property));
		}

        /// <summary>
        /// Gets/sets the minimum number of rows to use when arranging tiles in 'Normal' mode..
        /// </summary>
        /// <seealso cref="MinRowsProperty"/>
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
		public static readonly DependencyProperty ShowAllTilesProperty = DependencyPropertyUtilities.Register("ShowAllTiles",
			typeof(bool), typeof(NormalModeSettings),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnShowAllTilesChanged))
			);

		private static void OnShowAllTilesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			NormalModeSettings instance = d as NormalModeSettings;

			instance.RaisePropertyChangedEvent(DependencyPropertyUtilities.GetName(e.Property));

		}

        /// <summary>
        /// Gets/sets whether all tiles are arranged in view
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> even if this property is set to true it is still possible that all the tiles don't fit, e.g. if there isn't enough space to satisfy their minimum size constraints. In this case, scrollbars will appear.</para>
        /// </remarks>
        /// <seealso cref="ShowAllTilesProperty"/>
        /// <seealso cref="MaximizedModeSettings.ShowAllMinimizedTiles"/>
        /// <seealso cref="NormalModeSettings.TileConstraints"/>
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
		public static readonly DependencyProperty TileConstraintsProperty = DependencyPropertyUtilities.Register("TileConstraints",
			typeof(TileConstraints), typeof(NormalModeSettings),
			null, 
			new PropertyChangedCallback(OnTileConstraintsChanged)
			);

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

			settings.RaisePropertyChangedEvent(DependencyPropertyUtilities.GetName(e.Property));
		}

        private void OnTileConstraintsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.RaisePropertyChangedEvent("TileConstraints");
        }

        /// <summary>
        /// Gets/sets the constraints use for sizing a tile when it's <see cref="XamTile.State"/> is 'Normal'
        /// </summary>
        /// <seealso cref="TileConstraintsProperty"/>
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
		public static readonly DependencyProperty TileLayoutOrderProperty = DependencyPropertyUtilities.Register("TileLayoutOrder",
			typeof(TileLayoutOrder), typeof(NormalModeSettings),
			DependencyPropertyUtilities.CreateMetadata(TileLayoutOrder.Vertical, new PropertyChangedCallback(OnTileLayoutOrderChanged))
			);

		private static void OnTileLayoutOrderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			CoreUtilities.ValidateEnum(typeof(TileLayoutOrder), e.NewValue);

			NormalModeSettings instance = d as NormalModeSettings;

			instance.RaisePropertyChangedEvent(DependencyPropertyUtilities.GetName(e.Property));

		}

        /// <summary>
        /// Gets/sets how the panel will layout the tiles.
        /// </summary>
        /// <seealso cref="TileLayoutOrderProperty"/>
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

				#region VerticalTileAreaAlignment

		/// <summary>
		/// Identifies the <see cref="VerticalTileAreaAlignment"/> dependency property
		/// </summary>
		public static readonly DependencyProperty VerticalTileAreaAlignmentProperty = DependencyPropertyUtilities.Register("VerticalTileAreaAlignment",
			typeof(VerticalAlignment), typeof(NormalModeSettings),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.VerticalAlignmentTopBox, new PropertyChangedCallback(OnVerticalTileAreaAlignmentChanged))
			);

		private static void OnVerticalTileAreaAlignmentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			CoreUtilities.ValidateEnum(typeof(VerticalAlignment), e.NewValue);

			NormalModeSettings instance = d as NormalModeSettings;

			instance.RaisePropertyChangedEvent(DependencyPropertyUtilities.GetName(e.Property));
		}

        /// <summary>
        /// Determines the vertical alignment of the complete block of visible tiles within the control.
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> For <see cref="MaximizedModeSettings"/>, this property applies to the minimized tile area only.</para>
        /// </remarks>
        /// <seealso cref="VerticalTileAreaAlignmentProperty"/>
        public VerticalAlignment VerticalTileAreaAlignment
        {
            get
            {
                return (VerticalAlignment)this.GetValue(NormalModeSettings.VerticalTileAreaAlignmentProperty);
            }
            set
            {
                this.SetValue(NormalModeSettings.VerticalTileAreaAlignmentProperty, value);
            }
        }

                #endregion //VerticalTileAreaAlignment

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