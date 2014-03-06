using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Windows;

namespace Infragistics.Controls.Layouts
{
	/// <summary>
	/// Returns constraint information for tiles
	/// </summary>
	public interface ITileConstraints
	{
		/// <summary>
		/// Gets the horizontal alignment of the tile within its allocated slot (read-only)
		/// </summary>
		HorizontalAlignment? HorizontalAlignment { get; }

		/// <summary>
		/// Gets the margin used around a tile (read-only)
		/// </summary>
		Thickness? Margin { get; }

		/// <summary>
		/// Gets the maximum height of a tile (read-only)
		/// </summary>
		double MaxHeight { get; }

		/// <summary>
		/// Gets the maximum width of a tile (read-only)
		/// </summary>
		double MaxWidth { get; }

		/// <summary>
		/// Gets the minimum height of a tile (read-only)
		/// </summary>
		double MinHeight { get; }

		/// <summary>
		/// Gets the minimum width of a tile (read-only)
		/// </summary>
		double MinWidth { get; }

		/// <summary>
		/// Gets the preferred height of a tile (read-only)
		/// </summary>
		double PreferredHeight { get; }

		/// <summary>
		/// Gets the preferred width of a tile (read-only)
		/// </summary>
		double PreferredWidth { get; }

		/// <summary>
		/// Gets the vertical alignment of the tile within its allocated slot (read-only)
		/// </summary>
		VerticalAlignment? VerticalAlignment { get; }
	}

    /// <summary>
    /// Contains minimum, maximum and preferred size settings for a tile.
    /// </summary>

    [Infragistics.Windows.Internal.CloneBehavior(Infragistics.Windows.Internal.CloneBehavior.CloneObject)]

    public class TileConstraints : DependencyObjectNotifier, ITileConstraints
    {
        #region Private members

        private double _maxWidth        = (double)DependencyPropertyUtilities.GetDefaultValue(typeof(TileConstraints), MaxWidthProperty);
		private double _maxHeight		= (double)DependencyPropertyUtilities.GetDefaultValue(typeof(TileConstraints), MaxHeightProperty);
        private double _minWidth        = (double)DependencyPropertyUtilities.GetDefaultValue(typeof(TileConstraints), MinWidthProperty);
        private double _minHeight       = (double)DependencyPropertyUtilities.GetDefaultValue(typeof(TileConstraints), MinHeightProperty);
		private double _preferredWidth  = (double)DependencyPropertyUtilities.GetDefaultValue(typeof(TileConstraints), PreferredWidthProperty);
		private double _preferredHeight = (double)DependencyPropertyUtilities.GetDefaultValue(typeof(TileConstraints), PreferredHeightProperty);
		private Thickness?	_margin		= (Thickness?)DependencyPropertyUtilities.GetDefaultValue(typeof(TileConstraints), MarginProperty);
		private HorizontalAlignment?	_horizontalAlignment;
        private VerticalAlignment?      _verticalAlignment;

        #endregion //Private members	

        #region Base class overrides

            #region ToString

		///// <summary>
		///// Returns a string representation of the non-default settings
		///// </summary>
		///// <returns></returns>
		//public override string ToString()
		//{
		//    return Utilities.StringFromNonDefaultProperties(this);
		//}

            #endregion //ToString

        #endregion //Base class overrides	
        
        #region Properties

            #region Public Properties

                #region HorizontalAlignment

		/// <summary>
		/// Identifies the <see cref="HorizontalAlignment"/> dependency property
		/// </summary>
		public static readonly DependencyProperty HorizontalAlignmentProperty = DependencyPropertyUtilities.Register("HorizontalAlignment",
			typeof(HorizontalAlignment?), typeof(TileConstraints),
			null, new PropertyChangedCallback(OnHorizontalAlignmentChanged)
			);

		private static void OnHorizontalAlignmentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			TileConstraints instance = (TileConstraints)d;

			if ( e.NewValue != null )
				CoreUtilities.ValidateEnum(typeof(HorizontalAlignment), ((HorizontalAlignment?)( e.NewValue)).Value);

            instance._horizontalAlignment = (HorizontalAlignment?)e.NewValue;
			instance.RaisePropertyChangedEvent(DependencyPropertyUtilities.GetName(e.Property));

		}

        /// <summary>
        /// Gets/sets the horizontal alignment of the tile within its allocated slot.
        /// </summary>
        /// <seealso cref="HorizontalAlignmentProperty"/>





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
		public static readonly DependencyProperty MarginProperty = DependencyPropertyUtilities.Register("Margin",
			typeof(Thickness?), typeof(TileConstraints),
			null, new PropertyChangedCallback(OnMarginChanged)
			);

        private static void OnMarginChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            TileConstraints settings = target as TileConstraints;

            settings._margin = (Thickness?)e.NewValue;

			settings.RaisePropertyChangedEvent(DependencyPropertyUtilities.GetName(e.Property));
		}

        /// <summary>
        /// Gets/sets the margin used around a tile
        /// </summary>
        /// <seealso cref="MarginProperty"/>
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
		public static readonly DependencyProperty MaxHeightProperty = DependencyPropertyUtilities.Register("MaxHeight",
			typeof(double), typeof(TileConstraints),
			DependencyPropertyUtilities.CreateMetadata(double.PositiveInfinity, new PropertyChangedCallback(OnMaxHeightChanged))
			);

        private static void OnMaxHeightChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            TileConstraints settings = target as TileConstraints;

			ValidateMaxWidthHeight(e.NewValue);

            settings._maxHeight = (double)e.NewValue;

			settings.RaisePropertyChangedEvent(DependencyPropertyUtilities.GetName(e.Property));
		}

        /// <summary>
        /// Gets/sets the maximum height of a tile.
        /// </summary>
        /// <seealso cref="MaxHeightProperty"/>
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
		public static readonly DependencyProperty MaxWidthProperty = DependencyPropertyUtilities.Register("MaxWidth",
			typeof(double), typeof(TileConstraints),
			DependencyPropertyUtilities.CreateMetadata(double.PositiveInfinity, new PropertyChangedCallback(OnMaxWidthChanged))
			);

        private static void OnMaxWidthChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            TileConstraints settings = target as TileConstraints;

			ValidateMaxWidthHeight(e.NewValue);
			
			settings._maxWidth = (double)e.NewValue;
			settings.RaisePropertyChangedEvent(DependencyPropertyUtilities.GetName(e.Property));
		}


        /// <summary>
        /// Gets/sets the maximum width of a tile.
        /// </summary>
        /// <seealso cref="MaxWidthProperty"/>
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
		public static readonly DependencyProperty MinHeightProperty = DependencyPropertyUtilities.Register("MinHeight",
			typeof(double), typeof(TileConstraints),
			DependencyPropertyUtilities.CreateMetadata(0d, new PropertyChangedCallback(OnMinHeightChanged))
			);

        private static void OnMinHeightChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            TileConstraints settings = target as TileConstraints;

			ValidateMinWidthHeight(e.NewValue);

            settings._minHeight = (double)e.NewValue;
			settings.RaisePropertyChangedEvent(DependencyPropertyUtilities.GetName(e.Property));
		}

        /// <summary>
        /// Gets/sets the minimum height of a tile.
        /// </summary>
        /// <seealso cref="MinHeightProperty"/>
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
		public static readonly DependencyProperty MinWidthProperty = DependencyPropertyUtilities.Register("MinWidth",
			typeof(double), typeof(TileConstraints),
			DependencyPropertyUtilities.CreateMetadata(0d, new PropertyChangedCallback(OnMinWidthChanged))
			);

        private static void OnMinWidthChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            TileConstraints settings = target as TileConstraints;
			
			ValidateMinWidthHeight(e.NewValue);

            settings._minWidth = (double)e.NewValue;
			settings.RaisePropertyChangedEvent(DependencyPropertyUtilities.GetName(e.Property));
		}


        /// <summary>
        /// Gets/sets the minimum width of a tile.
        /// </summary>
        /// <seealso cref="MinWidthProperty"/>
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
		public static readonly DependencyProperty PreferredHeightProperty = DependencyPropertyUtilities.Register("PreferredHeight",
			typeof(double), typeof(TileConstraints),
			DependencyPropertyUtilities.CreateMetadata(double.NaN, new PropertyChangedCallback(OnPreferredHeightChanged))
			);

        private static void OnPreferredHeightChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            TileConstraints settings = target as TileConstraints;
			
			ValidatePreferredWidthHeight(e.NewValue);

            settings._preferredHeight = (double)e.NewValue;
			settings.RaisePropertyChangedEvent(DependencyPropertyUtilities.GetName(e.Property));
		}

        /// <summary>
        /// Gets/sets the preferred height of a tile.
        /// </summary>
        /// <seealso cref="PreferredHeightProperty"/>
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
		public static readonly DependencyProperty PreferredWidthProperty = DependencyPropertyUtilities.Register("PreferredWidth",
			typeof(double), typeof(TileConstraints),
			DependencyPropertyUtilities.CreateMetadata(double.NaN, new PropertyChangedCallback(OnPreferredWidthChanged))
			);

        private static void OnPreferredWidthChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            TileConstraints settings = target as TileConstraints;

			ValidatePreferredWidthHeight(e.NewValue);

            settings._preferredWidth = (double)e.NewValue;
			settings.RaisePropertyChangedEvent(DependencyPropertyUtilities.GetName(e.Property));
		}

        /// <summary>
        /// Gets/sets the preferred width of a tile.
        /// </summary>
        /// <seealso cref="PreferredWidthProperty"/>
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
		public static readonly DependencyProperty VerticalAlignmentProperty = DependencyPropertyUtilities.Register("VerticalAlignment",
			typeof(VerticalAlignment?), typeof(TileConstraints),
			null, new PropertyChangedCallback(OnVerticalAlignmentChanged)
			);

        private static void OnVerticalAlignmentChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            TileConstraints settings = target as TileConstraints;

			if (e.NewValue != null)
				CoreUtilities.ValidateEnum(typeof(VerticalAlignment), ((VerticalAlignment?)(e.NewValue)).Value);

            settings._verticalAlignment = (VerticalAlignment?)e.NewValue;
			settings.RaisePropertyChangedEvent(DependencyPropertyUtilities.GetName(e.Property));
		}

        /// <summary>
        /// Gets/sets the vertical alignment of the tile within its allocated slot.
        /// </summary>
        /// <seealso cref="VerticalAlignmentProperty"/>





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

        internal static void ValidateMaxWidthHeight(object objVal)
        {
            double val = (double)objVal;
			if (double.IsNaN(val) || val < 0d)
				throw new ArgumentOutOfRangeException(TileUtilities.GetString("LE_CantBeNegativeOrNan"));
		}

                #endregion // ValidateMaxWidthHeight

                #region ValidateMinWidthHeight

        internal static void ValidateMinWidthHeight(object objVal)
        {
            double val = (double)objVal;
            if ( double.IsNaN(val) || double.IsPositiveInfinity(val) || val < 0 )
				throw new ArgumentOutOfRangeException(TileUtilities.GetString("LE_CantBeNegativeOrNanOrInfinity"));
        }

                #endregion // ValidateMinWidthHeight

                #region ValidatePreferredWidthHeight

        internal static void ValidatePreferredWidthHeight(object objVal)
        {
            double val = (double)objVal;

            if ( double.IsNaN(val) )
                return;

            if (double.IsPositiveInfinity(val) || val < 0d)
				throw new ArgumentOutOfRangeException(TileUtilities.GetString("LE_CantBeNegativeOrInfinity"));
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