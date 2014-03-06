using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Infragistics.Controls.Grids.Primitives;
using System.Windows.Media.Imaging;
using System.Collections.Generic;

namespace Infragistics.Controls.Grids
{
	/// <summary>
	/// A column that uses a <see cref="Image"/> as the content for it's <see cref="CellBase"/>s
	/// </summary>
	public class ImageColumn : Column
    {
        #region Members

        Style _imageStyle;

        #endregion // Members

        #region Constructor
        /// <summary>
		/// Initializes a new instance of the <see cref="ImageColumn"/> class.
		/// </summary>
        public ImageColumn()
        {
            this.AddNewRowItemTemplateVerticalContentAlignment = this.VerticalContentAlignment = VerticalAlignment.Center;
            this.AddNewRowItemTemplateHorizontalContentAlignment = this.HorizontalContentAlignment = HorizontalAlignment.Center;
        }
		#endregion // Constructor

		#region Overrides

		#region Methods

		#region GenerateContentProvider

		/// <summary>
		/// Generates a new <see cref="ImageColumnContentProvider"/> that will be used to generate conent for <see cref="Cell"/> objects for this <see cref="Column"/>.
		/// </summary>
		/// <returns></returns>
		protected internal override ColumnContentProviderBase GenerateContentProvider()
		{
			return new ImageColumnContentProvider();
		}

		#endregion // GenerateContentProvider

        #region FillAvailableSummaries

        /// <summary>
        /// Fills the <see cref="SummaryOperandCollection"/> with the operands that the column expects as summary values.
        /// </summary>
        /// <param name="availableSummaries"></param>
        protected internal override void FillAvailableSummaries(SummaryOperandCollection availableSummaries)
        {
            ColumnLayout columnLayout = this.ColumnLayout;
            if (columnLayout != null)
            {
                XamGrid grid = columnLayout.Grid;

                if (grid != null)
                {
                    availableSummaries.Add(new CountSummaryOperand());                   
                }
            }
        }

        #endregion // FillAvailableSummaries

        #endregion // Methods

        #region IsFilterable

        /// <summary>
		/// Gets/sets if a column can be filtered via the UI.
		/// </summary>
		public override bool IsFilterable
		{
			get
			{
				return false;
			}
		}
		#endregion // IsFilterable

		#region DataType
		/// <summary>
		/// The DataType that the column's data is derived from.
		/// </summary>
        public override Type DataType
        {
            get
            {
                return base.DataType;
            }
            protected internal set
            {
                base.DataType = value;
                if (base.DataType == typeof(BitmapImage))
                {
                    this.GroupByComparer = this.SortComparer = new BitmapImageComparer();
                }
                else if (base.DataType == typeof(Uri) && this.SortComparer == null)
                {
                    this.SortComparer = new UriComparer();
                }
            }
        }
		#endregion // DataType

		#endregion // Overrides

		#region Properties

		#region ImageStyle

		/// <summary>
		/// Gets/sets the Style that will be applied to every <see cref="Image"/> in this <see cref="ImageColumn"/>.
		/// </summary>
		public Style ImageStyle
        {
            get
            {
                return this._imageStyle;
            }
            set
            {
                if (this._imageStyle != value)
                {
                    this._imageStyle = value;
                    this.OnPropertyChanged("ImageStyle");
                }
            }
        }

		#endregion // ImageStyle

		#region ImageHeight

		/// <summary>
		/// Identifies the <see cref="ImageHeight"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty ImageHeightProperty = DependencyProperty.Register("ImageHeight", typeof(double), typeof(ImageColumn), new PropertyMetadata(new PropertyChangedCallback(ImageHeightChanged)));

		/// <summary>
		/// Gets / sets a height to all images in an <see cref="ImageColumn"/>
		/// </summary>
		public double ImageHeight
		{
			get { return (double)this.GetValue(ImageHeightProperty); }
			set { this.SetValue(ImageHeightProperty, value); }
		}

		private static void ImageHeightChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			ImageColumn col = (ImageColumn)obj;
			col.OnPropertyChanged("ImageHeight");
		}

		#endregion // ImageHeight

		#region ImageWidth

		/// <summary>
		/// Identifies the <see cref="ImageWidth"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty ImageWidthProperty = DependencyProperty.Register("ImageWidth", typeof(double), typeof(ImageColumn), new PropertyMetadata(new PropertyChangedCallback(ImageWidthChanged)));

		/// <summary>
		/// Gets / sets a width to all images in an <see cref="ImageColumn"/>
		/// </summary>
		public double ImageWidth
		{
			get { return (double)this.GetValue(ImageWidthProperty); }
			set { this.SetValue(ImageWidthProperty, value); }
		}

		private static void ImageWidthChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			ImageColumn col = (ImageColumn)obj;
			col.OnPropertyChanged("ImageWidth");
		}

		#endregion // ImageWidth

		#endregion // Properties
	}

	/// <summary>
	/// A IComparer, IEqualityComparer implementation for BitmapImage datatypes
	/// </summary>
	internal class BitmapImageComparer : IComparer<BitmapImage>, IEqualityComparer<BitmapImage>
	{
		#region IComparer<BitmapImage> Members
		/// <summary>
		///     Compares two objects and returns a value indicating whether one is less than,
		///     equal to, or greater than the other.
		/// </summary>
		/// <param propertyName="x"> The first object to compare.</param>
		/// <param propertyName="y">The second object to compare.</param>
		/// <returns/>
		public int Compare(BitmapImage x, BitmapImage y)
		{
			if (x == null && y == null)
				return 0;

			if (x == null)
				return -1;

			if (y == null)
				return 1;

			int value = Uri.Compare(x.UriSource, y.UriSource, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);

			return value;
		}

		#endregion

		#region IEqualityComparer<BitmapImage> Members

		/// <summary>
		///     Compares two objects and returns a value indicating whether they equal one another.
		///     
		/// </summary>
		/// <param propertyName="x"> The first object to compare.</param>
		/// <param propertyName="y">The second object to compare.</param>
		/// <returns/>
		public bool Equals(BitmapImage x, BitmapImage y)
		{
			return Compare(x, y) == 0;
		}

		/// <summary>
		/// Returns a hash code for the object.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public int GetHashCode(BitmapImage obj)
		{
			int hash = 0;

			Uri uri = obj.UriSource;

			if (uri != null)
				hash = uri.GetHashCode();

			if (hash == 0)
				hash = obj.GetHashCode();

			return hash;
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