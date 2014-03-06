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
    /// Contains settings that are used to lay out Tiles inside a <see cref="XamTilesControl"/> or <see cref="XamTilesControl"/> when in maximized mode.
    /// </summary>
    /// <seealso cref="XamTilesControl.IsInMaximizedMode"/>
    /// <seealso cref="XamTilesControl.MaximizedModeSettings"/>
    /// <seealso cref="XamTilesControl.MaximizedModeSettings"/>
    [InfragisticsFeature(Version = FeatureInfo.Version_10_1, FeatureName = FeatureInfo.FeatureName_TilesPanel)]
    public class MaximizedModeSettings : ModeSettingsBase
    {
        #region Private members
        
        private TileConstraints _maximizedTileConstraints;
        private TileConstraints _minimizedTileConstraints;
        private TileConstraints _minimizedExpandedTileConstraints;

		#endregion //Base class overrides

        #region Constructors

        static MaximizedModeSettings()
        {
            HorizontalTileAreaAlignmentProperty.OverrideMetadata(typeof(MaximizedModeSettings), new FrameworkPropertyMetadata(KnownBoxes.HorizontalAlignmentStretchBox));
            VerticalTileAreaAlignmentProperty.OverrideMetadata(typeof(MaximizedModeSettings), new FrameworkPropertyMetadata(KnownBoxes.VerticalAlignmentStretchBox));
        }

        #endregion //Constructors	
    
        #region Properties

            #region Public Properties

                #region InterTileAreaSpacing

        /// <summary>
        /// Identifies the <see cref="InterTileAreaSpacing"/> dependency property
        /// </summary>
        public static readonly DependencyProperty InterTileAreaSpacingProperty = DependencyProperty.Register("InterTileAreaSpacing",
            typeof(double), typeof(MaximizedModeSettings), new FrameworkPropertyMetadata(double.NaN), new ValidateValueCallback(ModeSettingsBase.ValidateSpacing));

        /// <summary>
        /// Gets/sets the amount of spacing between the maximized tile area and the miminized tile area when in maximized mode.
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> if this property is not set then the <see cref="XamTilesControl.InterTileSpacingX"/> or <see cref="XamTilesControl.InterTileSpacingY"/> value will be used based on the <see cref="MaximizedTileLocation"/>.</para>
        /// </remarks>
        /// <seealso cref="XamTilesControl.InterTileAreaSpacingResolved"/>
        /// <seealso cref="XamTilesControl.IsInMaximizedMode"/>
        /// <seealso cref="XamTilesControl.MaximizedItems"/>
        //[Description("Gets/sets the amount of spacing between the maximized tile area and the miminized tile area when in maximized mode")]
        //[Category("TilesControl Properties")]
        public double InterTileAreaSpacing
        {
            get
            {
                return (double)this.GetValue(MaximizedModeSettings.InterTileAreaSpacingProperty);
            }
            set
            {
                this.SetValue(MaximizedModeSettings.InterTileAreaSpacingProperty, value);
            }
        }

                #endregion //InterTileAreaSpacing

                #region InterTileSpacingXMaximized

        /// <summary>
        /// Identifies the <see cref="InterTileSpacingXMaximized"/> dependency property
        /// </summary>
        public static readonly DependencyProperty InterTileSpacingXMaximizedProperty = DependencyProperty.Register("InterTileSpacingXMaximized",
            typeof(double), typeof(MaximizedModeSettings), new FrameworkPropertyMetadata(double.NaN), new ValidateValueCallback(ModeSettingsBase.ValidateSpacing));

        /// <summary>
        /// Gets/sets the amount of spacing between maximized tiles horizontally.
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> if this property is not set the <see cref="XamTilesControl.InterTileSpacingX"/> setting will be used.</para>
        /// </remarks>
        /// <seealso cref="XamTilesControl.InterTileSpacingXMaximizedResolved"/>
        /// <seealso cref="XamTilesControl.InterTileSpacingX"/>
        /// <seealso cref="InterTileAreaSpacing"/>
        /// <seealso cref="InterTileSpacingXMinimized"/>
        /// <seealso cref="InterTileSpacingYMinimized"/>
        /// <seealso cref="InterTileSpacingXMaximizedProperty"/>
        //[Description("Gets/sets the amount of spacing between maximized tiles horizontally.")]
        //[Category("TilesControl Properties")]
        public double InterTileSpacingXMaximized
        {
            get
            {
                return (double)this.GetValue(MaximizedModeSettings.InterTileSpacingXMaximizedProperty);
            }
            set
            {
                this.SetValue(MaximizedModeSettings.InterTileSpacingXMaximizedProperty, value);
            }
        }

                #endregion //InterTileSpacingXMaximized

                #region InterTileSpacingXMinimized

        /// <summary>
        /// Identifies the <see cref="InterTileSpacingXMinimized"/> dependency property
        /// </summary>
        public static readonly DependencyProperty InterTileSpacingXMinimizedProperty = DependencyProperty.Register("InterTileSpacingXMinimized",
            typeof(double), typeof(MaximizedModeSettings), new FrameworkPropertyMetadata(double.NaN), new ValidateValueCallback(ModeSettingsBase.ValidateSpacing));

        /// <summary>
        /// Gets/sets the amount of spacing between minimized tiles horizontally.
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> if this property is not set the <see cref="XamTilesControl.InterTileSpacingX"/> setting will be used.</para>
        /// </remarks>
        /// <seealso cref="XamTilesControl.InterTileSpacingXMinimizedResolved"/>
        /// <seealso cref="XamTilesControl.InterTileSpacingX"/>
        /// <seealso cref="InterTileSpacingXMaximized"/>
        /// <seealso cref="InterTileSpacingXMinimizedProperty"/>
        //[Description("Gets/sets the amount of spacing between minimized tiles horizonatlly.")]
        //[Category("TilesControl Properties")]
        public double InterTileSpacingXMinimized
        {
            get
            {
                return (double)this.GetValue(MaximizedModeSettings.InterTileSpacingXMinimizedProperty);
            }
            set
            {
                this.SetValue(MaximizedModeSettings.InterTileSpacingXMinimizedProperty, value);
            }
        }

                #endregion //InterTileSpacingXMinimized

                #region InterTileSpacingYMaximized

        /// <summary>
        /// Identifies the <see cref="InterTileSpacingYMaximized"/> dependency property
        /// </summary>
        public static readonly DependencyProperty InterTileSpacingYMaximizedProperty = DependencyProperty.Register("InterTileSpacingYMaximized",
            typeof(double), typeof(MaximizedModeSettings), new FrameworkPropertyMetadata(double.NaN), new ValidateValueCallback(ModeSettingsBase.ValidateSpacing));

        /// <summary>
        /// Gets/sets the amount of spacing between maximized tiles vertically.
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> if this property is not set the <see cref="XamTilesControl.InterTileSpacingX"/> setting will be used.</para>
        /// </remarks>
        /// <seealso cref="XamTilesControl.InterTileSpacingYMaximizedResolved"/>
        /// <seealso cref="XamTilesControl.InterTileSpacingX"/>
        /// <seealso cref="InterTileAreaSpacing"/>
        /// <seealso cref="InterTileSpacingYMinimized"/>
        /// <seealso cref="InterTileSpacingYMinimized"/>
        /// <seealso cref="InterTileSpacingYMaximizedProperty"/>
        //[Description("Gets/sets the amount of spacing between maximized tiles vertically.")]
        //[Category("TilesControl Properties")]
        public double InterTileSpacingYMaximized
        {
            get
            {
                return (double)this.GetValue(MaximizedModeSettings.InterTileSpacingYMaximizedProperty);
            }
            set
            {
                this.SetValue(MaximizedModeSettings.InterTileSpacingYMaximizedProperty, value);
            }
        }

                #endregion //InterTileSpacingYMaximized

                #region InterTileSpacingYMinimized

        /// <summary>
        /// Identifies the <see cref="InterTileSpacingYMinimized"/> dependency property
        /// </summary>
        public static readonly DependencyProperty InterTileSpacingYMinimizedProperty = DependencyProperty.Register("InterTileSpacingYMinimized",
            typeof(double), typeof(MaximizedModeSettings), new FrameworkPropertyMetadata(double.NaN), new ValidateValueCallback(ModeSettingsBase.ValidateSpacing));

        /// <summary>
        /// Gets/sets the amount of spacing between minimized tiles vertically.
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> if this property is not set the <see cref="XamTilesControl.InterTileSpacingY"/> setting will be used.</para>
        /// </remarks>
        /// <seealso cref="XamTilesControl.InterTileSpacingYMinimizedResolved"/>
        /// <seealso cref="XamTilesControl.InterTileSpacingY"/>
        /// <seealso cref="InterTileSpacingYMaximized"/>
        /// <seealso cref="InterTileSpacingYMinimizedProperty"/>
        //[Description("Gets/sets the amount of spacing between minimized tiles horizonatlly.")]
        //[Category("TilesControl Properties")]
        public double InterTileSpacingYMinimized
        {
            get
            {
                return (double)this.GetValue(MaximizedModeSettings.InterTileSpacingYMinimizedProperty);
            }
            set
            {
                this.SetValue(MaximizedModeSettings.InterTileSpacingYMinimizedProperty, value);
            }
        }

                #endregion //InterTileSpacingYMinimized

                #region MaximizedTileLocation

        /// <summary>
        /// Identifies the <see cref="MaximizedTileLocation"/> dependency property
        /// </summary>
        public static readonly DependencyProperty MaximizedTileLocationProperty = DependencyProperty.Register("MaximizedTileLocation",
            typeof(MaximizedTileLocation), typeof(MaximizedModeSettings), new FrameworkPropertyMetadata(MaximizedTileLocation.Left), new ValidateValueCallback(ValidateMaximizedTileLocation));

        private static bool ValidateMaximizedTileLocation(object value)
        {
            return Enum.IsDefined(typeof(MaximizedTileLocation), value);
        }

        /// <summary>
        /// Gets/sets where the maximized tiles will be arranged
        /// </summary>
        /// <seealso cref="MaximizedTileLocationProperty"/>
        //[Description("Gets/sets where the maximized tiles will be arranged")]
        //[Category("TilesControl Properties")]
        public MaximizedTileLocation MaximizedTileLocation
        {
            get
            {
                return (MaximizedTileLocation)this.GetValue(MaximizedModeSettings.MaximizedTileLocationProperty);
            }
            set
            {
                this.SetValue(MaximizedModeSettings.MaximizedTileLocationProperty, value);
            }
        }

                #endregion //MaximizedTileLocation

                #region MaximizedTileLayoutOrder

        /// <summary>
        /// Identifies the <see cref="MaximizedTileLayoutOrder"/> dependency property
        /// </summary>
        /// <seealso cref="MaximizedTileLayoutOrder"/>
        public static readonly DependencyProperty MaximizedTileLayoutOrderProperty = DependencyProperty.Register("MaximizedTileLayoutOrder",
            typeof(MaximizedTileLayoutOrder), typeof(MaximizedModeSettings), new FrameworkPropertyMetadata(MaximizedTileLayoutOrder.VerticalWithLastTileFill), new ValidateValueCallback(ValidateMaximizedTileLayoutOrder));

        private static bool ValidateMaximizedTileLayoutOrder(object value)
        {
            return Enum.IsDefined(typeof(MaximizedTileLayoutOrder), value);
        }

        /// <summary>
        /// Gets/sets how multiple maximized tiles are laid out relative to one another
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note</b>: This property only has meaning if the <see cref="XamTilesControl.MaximizedTileLimit"/> is set to a value greater than 1.</para>
        /// </remarks>
        /// <seealso cref="MaximizedTileLayoutOrderProperty"/>
        /// <seealso cref="XamTilesControl.MaximizedTileLimit"/>
        //[Description("Gets/sets how multiple maximized tiles are laid out relative to one another")]
        //[Category("TilesControl Properties")]
        public MaximizedTileLayoutOrder MaximizedTileLayoutOrder
        {
            get
            {
                return (MaximizedTileLayoutOrder)this.GetValue(MaximizedModeSettings.MaximizedTileLayoutOrderProperty);
            }
            set
            {
                this.SetValue(MaximizedModeSettings.MaximizedTileLayoutOrderProperty, value);
            }
        }

                #endregion //MaximizedTileLayoutOrder

                #region MaximizedTileConstraints

        /// <summary>
        /// Identifies the <see cref="MaximizedTileConstraints"/> dependency property
        /// </summary>
        public static readonly DependencyProperty MaximizedTileConstraintsProperty = DependencyProperty.Register("MaximizedTileConstraints",
            typeof(TileConstraints), typeof(MaximizedModeSettings), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnMaximizedTileConstraintsChanged)));

        private static void OnMaximizedTileConstraintsChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            MaximizedModeSettings settings = target as MaximizedModeSettings;

            // unwire the property changed of the old constraints
            if ( settings._maximizedTileConstraints != null )
                settings._maximizedTileConstraints.PropertyChanged -= new PropertyChangedEventHandler(settings.OnMaximizedTileConstraintsPropertyChanged);

            settings._maximizedTileConstraints = e.NewValue as TileConstraints;

            // wire up the property changed of the new constraints
            if ( settings._maximizedTileConstraints != null )
                settings._maximizedTileConstraints.PropertyChanged += new PropertyChangedEventHandler(settings.OnMaximizedTileConstraintsPropertyChanged);
        }


        private void OnMaximizedTileConstraintsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.RaisePropertyChangedEvent("MaximizedTileConstraints");
        }

        /// <summary>
        /// Gets/sets the constraints use for sizing a tile when it's <see cref="Tile.State"/> is 'Maximized'
        /// </summary>
        /// <seealso cref="MaximizedTileConstraintsProperty"/>
        //[Description("Gets/sets the constraints use for sizing a tile when it's Stae is 'Maximized'")]
        //[Category("TilesControl Properties")]
        [RefreshProperties(RefreshProperties.Repaint)]
        public TileConstraints MaximizedTileConstraints
        {
            get
            {
                return this._maximizedTileConstraints;
            }
            set
            {
                this.SetValue(MaximizedModeSettings.MaximizedTileConstraintsProperty, value);
            }
        }

                #endregion //MaximizedTileConstraints

                #region MinimizedTileConstraints

        /// <summary>
        /// Identifies the <see cref="MinimizedTileConstraints"/> dependency property
        /// </summary>
        public static readonly DependencyProperty MinimizedTileConstraintsProperty = DependencyProperty.Register("MinimizedTileConstraints",
            typeof(TileConstraints), typeof(MaximizedModeSettings), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnMinimizedTileConstraintsChanged)));

        private static void OnMinimizedTileConstraintsChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            MaximizedModeSettings settings = target as MaximizedModeSettings;

            // unwire the property changed of the old constraints
            if (settings._minimizedTileConstraints != null)
                settings._minimizedTileConstraints.PropertyChanged -= new PropertyChangedEventHandler(settings.OnMinimizedTileConstraintsPropertyChanged);

            settings._minimizedTileConstraints = e.NewValue as TileConstraints;

            // wire up the property changed of the new constraints
            if (settings._minimizedTileConstraints != null)
                settings._minimizedTileConstraints.PropertyChanged += new PropertyChangedEventHandler(settings.OnMinimizedTileConstraintsPropertyChanged);
        }


        private void OnMinimizedTileConstraintsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.RaisePropertyChangedEvent("MinimizedTileConstraints");
        }

        /// <summary>
        /// Gets/sets the constraints use for sizing a tile when it's <see cref="Tile.State"/> is 'Minimized'
        /// </summary>
        /// <seealso cref="MinimizedTileConstraintsProperty"/>
        //[Description("Gets/sets the constraints use for sizing a tile when it's Stae is 'Minimized'")]
        //[Category("TilesControl Properties")]
        [RefreshProperties(RefreshProperties.Repaint)]
        public TileConstraints MinimizedTileConstraints
        {
            get
            {
                return this._minimizedTileConstraints;
            }
            set
            {
                this.SetValue(MaximizedModeSettings.MinimizedTileConstraintsProperty, value);
            }
        }

                #endregion //MinimizedTileConstraints
        
                #region MinimizedTileExpandButtonVisibility

        /// <summary>
        /// Identifies the <see cref="MinimizedTileExpandButtonVisibility"/> dependency property
        /// </summary>
        public static readonly DependencyProperty MinimizedTileExpandButtonVisibilityProperty = DependencyProperty.Register("MinimizedTileExpandButtonVisibility",
            typeof(Visibility), typeof(MaximizedModeSettings), new FrameworkPropertyMetadata(KnownBoxes.VisibilityVisibleBox));

        /// <summary>
        /// Gets/sets the default visibility of the expand button in a <see cref="Tile"/>'s header.
        /// </summary>
        /// <seealso cref="MinimizedTileExpandButtonVisibilityProperty"/>
        /// <seealso cref="MaximizedModeSettings.MinimizedTileExpansionMode"/>
        //[Description("Gets/sets the default visibility of the expand button in a tile's header.")]
        //[Category("Behavior")]
        public Visibility MinimizedTileExpandButtonVisibility
        {
            get
            {
                return (Visibility)this.GetValue(MaximizedModeSettings.MinimizedTileExpandButtonVisibilityProperty);
            }
            set
            {
                this.SetValue(MaximizedModeSettings.MinimizedTileExpandButtonVisibilityProperty, value);
            }
        }

                #endregion //MinimizedTileExpandButtonVisibility
	
                #region MinimizedExpandedTileConstraints

        /// <summary>
        /// Identifies the <see cref="MinimizedExpandedTileConstraints"/> dependency property
        /// </summary>
        public static readonly DependencyProperty MinimizedExpandedTileConstraintsProperty = DependencyProperty.Register("MinimizedExpandedTileConstraints",
            typeof(TileConstraints), typeof(MaximizedModeSettings), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnMinimizedExpandedTileConstraintsChanged)));

        private static void OnMinimizedExpandedTileConstraintsChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            MaximizedModeSettings settings = target as MaximizedModeSettings;

            // unwire the property changed of the old constraints
            if (settings._minimizedExpandedTileConstraints != null)
                settings._minimizedExpandedTileConstraints.PropertyChanged -= new PropertyChangedEventHandler(settings.OnMinimizedExpandedTileConstraintsPropertyChanged);

            settings._minimizedExpandedTileConstraints = e.NewValue as TileConstraints;

            // wire up the property changed of the new constraints
            if (settings._minimizedExpandedTileConstraints != null)
                settings._minimizedExpandedTileConstraints.PropertyChanged += new PropertyChangedEventHandler(settings.OnMinimizedExpandedTileConstraintsPropertyChanged);
        }


        private void OnMinimizedExpandedTileConstraintsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.RaisePropertyChangedEvent("MinimizedExpandedTileConstraints");
        }

        /// <summary>
        /// Gets/sets the constraints use for sizing a tile when it's <see cref="Tile.State"/> is 'MinimizedExpanded'
        /// </summary>
        /// <seealso cref="MinimizedExpandedTileConstraintsProperty"/>
        //[Description("Gets/sets the constraints use for sizing a tile when it's Stae is 'MinimizedExpanded'")]
        //[Category("TilesControl Properties")]
        [RefreshProperties(RefreshProperties.Repaint)]
        public TileConstraints MinimizedExpandedTileConstraints
        {
            get
            {
                return this._minimizedExpandedTileConstraints;
            }
            set
            {
                this.SetValue(MaximizedModeSettings.MinimizedExpandedTileConstraintsProperty, value);
            }
        }

                #endregion //MinimizedExpandedTileConstraints

                #region MinimizedTileExpansionMode

        /// <summary>
        /// Identifies the <see cref="MinimizedTileExpansionMode"/> dependency property
        /// </summary>
        public static readonly DependencyProperty MinimizedTileExpansionModeProperty = DependencyProperty.Register("MinimizedTileExpansionMode",
            typeof(MinimizedTileExpansionMode), typeof(MaximizedModeSettings), new FrameworkPropertyMetadata(MinimizedTileExpansionMode.AllowMultipleExpandAllInitially), new ValidateValueCallback(ValidateMinimizedTileExpansionMode));

        private static bool ValidateMinimizedTileExpansionMode(object value)
        {
            return Enum.IsDefined(typeof(MinimizedTileExpansionMode), value);
        }

        /// <summary>
        /// Gets/sets how many minimized tiles may be expanded.
        /// </summary>
        /// <seealso cref="MinimizedTileExpansionModeProperty"/>
        /// <seealso cref="MinimizedTileExpandButtonVisibility"/>
        //[Description("Gets/sets how many minimized tiles may be expanded.")]
        //[Category("TilesControl Properties")]
        public MinimizedTileExpansionMode MinimizedTileExpansionMode
        {
            get
            {
                return (MinimizedTileExpansionMode)this.GetValue(MaximizedModeSettings.MinimizedTileExpansionModeProperty);
            }
            set
            {
                this.SetValue(MaximizedModeSettings.MinimizedTileExpansionModeProperty, value);
            }
        }

                #endregion //MinimizedTileExpansionMode

                #region ShowAllMinimizedTiles

        /// <summary>
        /// Identifies the <see cref="ShowAllMinimizedTiles"/> dependency property
        /// </summary>
        public static readonly DependencyProperty ShowAllMinimizedTilesProperty = DependencyProperty.Register("ShowAllMinimizedTiles",
            typeof(bool), typeof(MaximizedModeSettings), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

        /// <summary>
        /// Gets/sets whether all minimized tiles are arranged in view
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> even if this property is set to true it is still possible that all the minimized tiles don't fit, e.g. if there isn't enough space to satisfy their minimum size constraints. In this case, scrollbars will appear.</para>
        /// </remarks>
        /// <seealso cref="ShowAllMinimizedTilesProperty"/>
        /// <seealso cref="NormalModeSettings.ShowAllTiles"/>
        /// <seealso cref="NormalModeSettings.TileConstraints"/>
        //[Description("Gets/sets whether all minimized tiles are arranged in view")]
        //[Category("TilesControl Properties")]
        public bool ShowAllMinimizedTiles
        {
            get
            {
                return (bool)this.GetValue(MaximizedModeSettings.ShowAllMinimizedTilesProperty);
            }
            set
            {
                this.SetValue(MaximizedModeSettings.ShowAllMinimizedTilesProperty, value);
            }
        }

                #endregion //ShowAllMinimizedTiles

                #region ShowTileAreaSplitter

        /// <summary>
        /// Identifies the <see cref="ShowTileAreaSplitter"/> dependency property
        /// </summary>
        public static readonly DependencyProperty ShowTileAreaSplitterProperty = DependencyProperty.Register("ShowTileAreaSplitter",
            typeof(bool), typeof(MaximizedModeSettings), new FrameworkPropertyMetadata(KnownBoxes.TrueBox));

        /// <summary>
        /// Gets/sets whether a splitter will be shown between the maximized and minimized tile areas which can be used to control the relative size of the these areas.
        /// </summary>
        /// <seealso cref="ShowTileAreaSplitterProperty"/>
        /// <seealso cref="TileAreaSplitter"/>
        //[Description("Gets/sets whether a splitter will be shown between the maximized and minimized tile areas which can be used to control the relative size of the these areas.")]
        //[Category("Behavior")]
        public bool ShowTileAreaSplitter
        {
            get
            {
                return (bool)this.GetValue(MaximizedModeSettings.ShowTileAreaSplitterProperty);
            }
            set
            {
                this.SetValue(MaximizedModeSettings.ShowTileAreaSplitterProperty, value);
            }
        }

                #endregion //ShowTileAreaSplitter

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