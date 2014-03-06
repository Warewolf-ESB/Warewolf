using System;
using System.Collections.Generic;
using System.Text;
using Infragistics.Windows.Internal;
using System.ComponentModel;
using System.Windows;
using Infragistics.Windows.Helpers;

namespace Infragistics.Windows.Tiles
{

    /// <summary>
    /// Contains minimum, maximum and preferred size settings for a tile.
    /// </summary>
    [CloneBehavior(CloneBehavior.CloneObject)]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    [InfragisticsFeature(Version = FeatureInfo.Version_10_1, FeatureName = FeatureInfo.FeatureName_TilesPanel)]
    public class TileConstraints : DependencyObjectNotifier, ITileConstraints
    {
        #region Private members

        private double _maxWidth        = (double)MaxWidthProperty.DefaultMetadata.DefaultValue;
        private double _maxHeight       = (double)MaxHeightProperty.DefaultMetadata.DefaultValue;
        private double _minWidth        = (double)MinWidthProperty.DefaultMetadata.DefaultValue;
        private double _minHeight       = (double)MinHeightProperty.DefaultMetadata.DefaultValue;
        private double _preferredWidth  = (double)PreferredWidthProperty.DefaultMetadata.DefaultValue;
        private double _preferredHeight = (double)PreferredHeightProperty.DefaultMetadata.DefaultValue;
        private Thickness?              _margin                 = (Thickness?)MarginProperty.DefaultMetadata.DefaultValue;
        private HorizontalAlignment?    _horizontalAlignment    = (HorizontalAlignment?)HorizontalAlignmentProperty.DefaultMetadata.DefaultValue;
        private VerticalAlignment?      _verticalAlignment      = (VerticalAlignment?)VerticalAlignmentProperty.DefaultMetadata.DefaultValue;

        #endregion //Private members	

        #region Base class overrides

			#region OnPropertyChanged

		/// <summary>
		/// Called when a property changes.
		/// </summary>
		/// <param name="e">A DependencyPropertyChangedEventArgs instance that contains information about the property that changed.</param>
		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);

			this.RaisePropertyChangedEvent(e.Property.Name);
		}

			#endregion //OnPropertyChanged

            #region ToString

        /// <summary>
        /// Returns a string representation of the non-default settings
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Utilities.StringFromNonDefaultProperties(this);
        }

            #endregion //ToString

        #endregion //Base class overrides	
        
        #region Properties

            #region Public Properties

                #region HorizontalAlignment

        /// <summary>
        /// Identifies the <see cref="HorizontalAlignment"/> dependency property
        /// </summary>
        public static readonly DependencyProperty HorizontalAlignmentProperty = DependencyProperty.Register("HorizontalAlignment",
            typeof(HorizontalAlignment?), typeof(TileConstraints), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnHorizontalAlignmentChanged)));

        private static void OnHorizontalAlignmentChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            TileConstraints settings = target as TileConstraints;

            settings._horizontalAlignment = (HorizontalAlignment?)e.NewValue;
        }

        /// <summary>
        /// Gets/sets the horizontal alignment of the tile within its allocated slot.
        /// </summary>
        /// <seealso cref="HorizontalAlignmentProperty"/>
        //[Description("Gets/sets the horizontal alignment of the tile within its allocated slot.")]
        //[Category("TilesControl Properties")]
        [TypeConverter(typeof(Infragistics.Windows.Helpers.NullableConverter<HorizontalAlignment>))]
        public HorizontalAlignment? HorizontalAlignment
        {
            get
            {
                return this._horizontalAlignment;
            }
            set
            {
                this.SetValue(TileConstraints.HorizontalAlignmentProperty, value);
            }
        }

                #endregion //HorizontalAlignment

                #region Margin

        /// <summary>
        /// Identifies the <see cref="Margin"/> dependency property
        /// </summary>
        public static readonly DependencyProperty MarginProperty = DependencyProperty.Register("Margin",
            typeof(Thickness?), typeof(TileConstraints), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnMarginChanged)));

        private static void OnMarginChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            TileConstraints settings = target as TileConstraints;

            settings._margin = (Thickness?)e.NewValue;
        }

        /// <summary>
        /// Gets/sets the margin used around a tile
        /// </summary>
        /// <seealso cref="MarginProperty"/>
        //[Description("Gets/sets the margin used around a tile")]
        //[Category("TilesControl Properties")]
        public Thickness? Margin
        {
            get
            {
                return this._margin;
            }
            set
            {
                this.SetValue(TileConstraints.MarginProperty, value);
            }
        }

                #endregion //Margin

                #region MaxHeight

        /// <summary>
        /// Identifies the <see cref="MaxHeight"/> dependency property
        /// </summary>
        public static readonly DependencyProperty MaxHeightProperty = DependencyProperty.Register("MaxHeight",
            typeof(double), typeof(TileConstraints), new FrameworkPropertyMetadata(double.PositiveInfinity, new PropertyChangedCallback(OnMaxHeightChanged)), new ValidateValueCallback(ValidateMaxWidthHeight));

        private static void OnMaxHeightChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            TileConstraints settings = target as TileConstraints;

            settings._maxHeight = (double)e.NewValue;
        }

        /// <summary>
        /// Gets/sets the maximum height of a tile.
        /// </summary>
        /// <seealso cref="MaxHeightProperty"/>
        //[Description("Gets/sets the maximum height of a tile.")]
        //[Category("TilesControl Properties")]
        public double MaxHeight
        {
            get
            {
                return this._maxHeight;
            }
            set
            {
                this.SetValue(TileConstraints.MaxHeightProperty, value);
            }
        }

                #endregion //MaxHeight

                #region MaxWidth

        /// <summary>
        /// Identifies the <see cref="MaxWidth"/> dependency property
        /// </summary>
        public static readonly DependencyProperty MaxWidthProperty = DependencyProperty.Register("MaxWidth",
            typeof(double), typeof(TileConstraints), new FrameworkPropertyMetadata(double.PositiveInfinity, new PropertyChangedCallback(OnMaxWidthChanged)), new ValidateValueCallback(ValidateMaxWidthHeight));

        private static void OnMaxWidthChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            TileConstraints settings = target as TileConstraints;

            settings._maxWidth = (double)e.NewValue;
        }


        /// <summary>
        /// Gets/sets the maximum width of a tile.
        /// </summary>
        /// <seealso cref="MaxWidthProperty"/>
        //[Description("Gets/sets the maximum width of a tile.")]
        //[Category("TilesControl Properties")]
        public double MaxWidth
        {
            get
            {
                return this._maxWidth;
            }
            set
            {
                this.SetValue(TileConstraints.MaxWidthProperty, value);
            }
        }

                #endregion //MaxWidth

                #region MinHeight

        /// <summary>
        /// Identifies the <see cref="MinHeight"/> dependency property
        /// </summary>
        public static readonly DependencyProperty MinHeightProperty = DependencyProperty.Register("MinHeight",
            typeof(double), typeof(TileConstraints), new FrameworkPropertyMetadata(0d , new PropertyChangedCallback(OnMinHeightChanged)), new ValidateValueCallback(ValidateMinWidthHeight));

        private static void OnMinHeightChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            TileConstraints settings = target as TileConstraints;

            settings._minHeight = (double)e.NewValue;
        }

        /// <summary>
        /// Gets/sets the minimum height of a tile.
        /// </summary>
        /// <seealso cref="MinHeightProperty"/>
        //[Description("Gets/sets the minimum height of a tile.")]
        //[Category("TilesControl Properties")]
        public double MinHeight
        {
            get
            {
                return this._minHeight;
            }
            set
            {
                this.SetValue(TileConstraints.MinHeightProperty, value);
            }
        }

                #endregion //MinHeight

                #region MinWidth

        /// <summary>
        /// Identifies the <see cref="MinWidth"/> dependency property
        /// </summary>
        public static readonly DependencyProperty MinWidthProperty = DependencyProperty.Register("MinWidth",
            typeof(double), typeof(TileConstraints), new FrameworkPropertyMetadata(0d, new PropertyChangedCallback(OnMinWidthChanged)), new ValidateValueCallback(ValidateMinWidthHeight));

        private static void OnMinWidthChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            TileConstraints settings = target as TileConstraints;

            settings._minWidth = (double)e.NewValue;
        }


        /// <summary>
        /// Gets/sets the minimum width of a tile.
        /// </summary>
        /// <seealso cref="MinWidthProperty"/>
        //[Description("Gets/sets the minimum width of a tile.")]
        //[Category("TilesControl Properties")]
        public double MinWidth
        {
            get
            {
                return this._minWidth;
            }
            set
            {
                this.SetValue(TileConstraints.MinWidthProperty, value);
            }
        }

                #endregion //MinWidth

                #region PreferredHeight

        /// <summary>
        /// Identifies the <see cref="PreferredHeight"/> dependency property
        /// </summary>
        public static readonly DependencyProperty PreferredHeightProperty = DependencyProperty.Register("PreferredHeight",
            typeof(double), typeof(TileConstraints), new FrameworkPropertyMetadata(double.NaN, new PropertyChangedCallback(OnPreferredHeightChanged)), new ValidateValueCallback(ValidatePreferredWidthHeight));

        private static void OnPreferredHeightChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            TileConstraints settings = target as TileConstraints;

            settings._preferredHeight = (double)e.NewValue;
        }

        /// <summary>
        /// Gets/sets the preferred height of a tile.
        /// </summary>
        /// <seealso cref="PreferredHeightProperty"/>
        //[Description("Gets/sets the preferred height of a tile.")]
        //[Category("TilesControl Properties")]
        public double PreferredHeight
        {
            get
            {
                return this._preferredHeight;
            }
            set
            {
                this.SetValue(TileConstraints.PreferredHeightProperty, value);
            }
        }

                #endregion //PreferredHeight

                #region PreferredWidth

        /// <summary>
        /// Identifies the <see cref="PreferredWidth"/> dependency property
        /// </summary>
        public static readonly DependencyProperty PreferredWidthProperty = DependencyProperty.Register("PreferredWidth",
            typeof(double), typeof(TileConstraints), new FrameworkPropertyMetadata(double.NaN, new PropertyChangedCallback(OnPreferredWidthChanged)), new ValidateValueCallback(ValidatePreferredWidthHeight));

        private static void OnPreferredWidthChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            TileConstraints settings = target as TileConstraints;

            settings._preferredWidth = (double)e.NewValue;
        }

        /// <summary>
        /// Gets/sets the preferred width of a tile.
        /// </summary>
        /// <seealso cref="PreferredWidthProperty"/>
        //[Description("Gets/sets the preferred width of a tile.")]
        //[Category("TilesControl Properties")]
        public double PreferredWidth
        {
            get
            {
                return this._preferredWidth;
            }
            set
            {
                this.SetValue(TileConstraints.PreferredWidthProperty, value);
            }
        }

                #endregion //PreferredWidth

                #region VerticalAlignment

        /// <summary>
        /// Identifies the <see cref="VerticalAlignment"/> dependency property
        /// </summary>
        public static readonly DependencyProperty VerticalAlignmentProperty = DependencyProperty.Register("VerticalAlignment",
            typeof(VerticalAlignment?), typeof(TileConstraints), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnVerticalAlignmentChanged)));

        private static void OnVerticalAlignmentChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            TileConstraints settings = target as TileConstraints;

            settings._verticalAlignment = (VerticalAlignment?)e.NewValue;
        }

        /// <summary>
        /// Gets/sets the vertical alignment of the tile within its allocated slot.
        /// </summary>
        /// <seealso cref="VerticalAlignmentProperty"/>
        //[Description("Gets/sets the vertical alignment of the tile within its allocated slot.")]
        //[Category("TilesControl Properties")]
        [TypeConverter(typeof(Infragistics.Windows.Helpers.NullableConverter<VerticalAlignment>))]
        public VerticalAlignment? VerticalAlignment
        {
            get
            {
                return this._verticalAlignment;
            }
            set
            {
                this.SetValue(TileConstraints.VerticalAlignmentProperty, value);
            }
        }

                #endregion //VerticalAlignment

            #endregion //Public Properties

        #endregion //Properties

        #region Methods

		    #region Private Methods

		    #endregion Private Methods

            #region Internal Methods

                #region ValidateMaxWidthHeight

        internal static bool ValidateMaxWidthHeight(object objVal)
        {
            double val = (double)objVal;
            return (!double.IsNaN(val)) && val >= 0d;
        }

                #endregion // ValidateMaxWidthHeight

                #region ValidateMinWidthHeight

        internal static bool ValidateMinWidthHeight(object objVal)
        {
            double val = (double)objVal;
            return (!double.IsNaN(val)) &&(!double.IsPositiveInfinity(val)) && val >= 0d;
        }

                #endregion // ValidateMinWidthHeight

                #region ValidatePreferredWidthHeight

        internal static bool ValidatePreferredWidthHeight(object objVal)
        {
            double val = (double)objVal;

            if ( double.IsNaN(val) )
                return true;

            return (!double.IsPositiveInfinity(val)) && val > 0d;
        }

                #endregion // ValidatePreferredWidthHeight

            #endregion //Internal Methods	

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