using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.ComponentModel;

namespace Infragistics.Controls.Primitives
{
	/// <summary>
	/// A custom panel that positions each item in a separate cell within a grid of evenly sized cells.
	/// </summary>
	/// <remarks>
	/// <p class="body">The number of rows and columns of cells are based on the <see cref="Rows"/> and 
	/// <see cref="Columns"/> properties respectively. The first item is positioned based on the 
	/// <see cref="FirstRow"/> and <see cref="FirstColumn"/>. Each subsequent item is positioned in the 
	/// next column. When there are no more columns within the row, the next item will be positioned in 
	/// the first cell of the next row.</p>
	/// </remarks>

	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!

	public class UniformGrid : Panel
    {
		#region Member Variables

		private int _columns;
		private int _rows;
		private int _firstCol;
		private int _firstRow;

		#endregion //Member Variables

        #region Constructor
        /// <summary>
        /// Initializes a new <see cref="UniformGrid"/>
        /// </summary>
        public UniformGrid()
        {
        } 
        #endregion //Constructor

        #region Properties

		#region Columns

		/// <summary>
		/// Identifies the <see cref="Columns"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ColumnsProperty = DependencyPropertyUtilities.Register("Columns",
			typeof(int), typeof(UniformGrid),
			DependencyPropertyUtilities.CreateMetadata(0, new PropertyChangedCallback(OnColumnRowPropertyChanged))
			);

        /// <summary>
        /// Gets or sets the number of columns in the grid
        /// </summary>
        /// <seealso cref="ColumnsProperty"/>
        //[Description("Gets or sets the number of columns in the grid")]
        //[Category("Layout")]
        [Bindable(true)]
		public int Columns
		{
			get
			{
				return (int)this.GetValue(UniformGrid.ColumnsProperty);
			}
			set
			{
				this.SetValue(UniformGrid.ColumnsProperty, value);
			}
		}

        #endregion //Columns

		#region FirstColumn

		/// <summary>
		/// Identifies the <see cref="FirstColumn"/> dependency property
		/// </summary>
		public static readonly DependencyProperty FirstColumnProperty = DependencyPropertyUtilities.Register("FirstColumn",
			typeof(int), typeof(UniformGrid),
			DependencyPropertyUtilities.CreateMetadata(0, new PropertyChangedCallback(OnColumnRowPropertyChanged))
			);

		/// <summary>
		/// Gets or sets the number of leading blank columns in the first row of the grid
		/// </summary>
		/// <seealso cref="FirstColumnProperty"/>
		//[Description("Gets or sets the number of leading blank columns in the first row of the grid")]
		//[Category("Layout")]
		[Bindable(true)]
		public int FirstColumn
		{
			get
			{
				return (int)this.GetValue(UniformGrid.FirstColumnProperty);
			}
			set
			{
				this.SetValue(UniformGrid.FirstColumnProperty, value);
			}
		}

        #endregion //FirstColumn

		#region FirstRow

		/// <summary>
		/// Identifies the <see cref="FirstRow"/> dependency property
		/// </summary>
		public static readonly DependencyProperty FirstRowProperty = DependencyPropertyUtilities.Register("FirstRow",
			typeof(int), typeof(UniformGrid),
			DependencyPropertyUtilities.CreateMetadata(0, new PropertyChangedCallback(OnColumnRowPropertyChanged))
			);

		/// <summary>
		/// Gets or sets the number of blank rows to display before the first item is arranged.
		/// </summary>
		/// <seealso cref="FirstRowProperty"/>
		//[Description("Gets or sets the number of blank rows to display before the first item is arranged.")]
		//[Category("Layout")]
		[Bindable(true)]
		public int FirstRow
		{
			get
			{
				return (int)this.GetValue(UniformGrid.FirstRowProperty);
			}
			set
			{
				this.SetValue(UniformGrid.FirstRowProperty, value);
			}
		}

        #endregion //FirstRow

		#region Rows

		/// <summary>
		/// Identifies the <see cref="Rows"/> dependency property
		/// </summary>
		public static readonly DependencyProperty RowsProperty = DependencyPropertyUtilities.Register("Rows",
			typeof(int), typeof(UniformGrid),
			DependencyPropertyUtilities.CreateMetadata(0, new PropertyChangedCallback(OnColumnRowPropertyChanged))
			);

		/// <summary>
		/// Gets or sets the number of rows in the grid
		/// </summary>
		/// <seealso cref="RowsProperty"/>
		//[Description("Gets or sets the number of rows in the grid")]
		//[Category("Layout")]
		[Bindable(true)]
		public int Rows
		{
			get
			{
				return (int)this.GetValue(UniformGrid.RowsProperty);
			}
			set
			{
				this.SetValue(UniformGrid.RowsProperty, value);
			}
		}

        #endregion //Rows

		#endregion //Properties

        #region Methods

		// JJD 11/7/11 - TFS79499 - added
		#region CalculateLayoutRoundingOffset

		private static double CalculateLayoutRoundingOffset(int firstElementIndex, ref double extraAvailableExtent)
		{
			// if the first element is at index 0 or there is no extra extent to allocate then return 0;
			if (firstElementIndex == 0 || extraAvailableExtent == 0)
				return 0;

			// offset a pixel for each row/column that ia being skipped
			double offset = Math.Min(firstElementIndex, extraAvailableExtent);

			// Decrement the extra extent left after the offset is taken out
			extraAvailableExtent = Math.Max(extraAvailableExtent - offset, 0);

			return offset;
		}

		#endregion //CalculateLayoutRoundingOffset	

		#region CalculateMetrics
		private void CalculateMetrics(bool preferColumns)
		{
			// get the explicitly set values
			this._columns = this.Columns;
			this._rows = this.Rows;
			this._firstCol = this.FirstColumn;
			this._firstRow = this.FirstRow;

			// make sure the first column value is less than the total number of columns
			if (this._columns > 0 && this._firstCol > this._columns)
				this._firstCol = this._columns;

			// make sure the first row value is less than the total number of rows
			if (this._rows > 0 && this._firstRow > this._rows)
				this._firstRow = this._rows;

			// if neither the rows nor columns are specified
			if (this._rows == 0 || this._columns == 0)
			{
				int visibleChildCount = 0;

				for(int i = 0, count = this.Children.Count; i < count; i++)
				{
					if (this.Children[i].Visibility != Visibility.Collapsed)
						visibleChildCount++;
				}

				if (visibleChildCount == 0)
					visibleChildCount = 1;

				if (this._rows == 0)
				{
					if (this._columns == 0)
					{
						this._rows = (int)Math.Sqrt(visibleChildCount);

						if (this._rows * this._rows < visibleChildCount)
						{
							this._columns = this._rows;
							if (preferColumns)
							{
								this._columns++;

								if (this._columns * this._rows < visibleChildCount)
									this._rows++;
							}
							else
							{
								this._rows++;

								if (this._rows * this._columns < visibleChildCount)
									this._columns++;
							}
						}
						else
							this._columns = this._rows;
					}
					else
					{
						this._rows = (visibleChildCount + this._firstCol + (this._columns - 1)) / this._columns;
						this._rows += this._firstRow;
					}
				}
				else
				{
					// the columns must be 0 but the rows are not
					this._columns = (visibleChildCount + this._firstRow + (this._rows - 1)) / this._rows;
					this._columns += this._firstCol;
				}
			}
		} 
		#endregion //CalculateMetrics

		#region OnColumnRowPropertyChanged

		private static void OnColumnRowPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			UniformGrid instance = (UniformGrid)d;

			CoreUtilities.ValidateIsNotNegative((double)(int)e.NewValue, DependencyPropertyUtilities.GetName(e.Property));

			instance.InvalidateMeasure();
		}

		#endregion //OnColumnRowPropertyChanged	
    
        #endregion //Methods    

		#region Base class overrides

		#region MeasureOverride
		/// <summary>
		/// Invoked to measure the element and its children.
		/// </summary>
		/// <param name="availableSize">The size that reflects the available size that this element can give to its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> that represents the desired size of the element.</returns>
        protected override Size MeasureOverride(Size availableSize)
        {
			
			//bool preferColumns = double.IsInfinity(availableSize.Width) ||
			//	(false == double.IsInfinity(availableSize.Height) && availableSize.Width > availableSize.Height);
			bool preferColumns = true;
			this.CalculateMetrics(preferColumns);

			Size itemSize = new Size(availableSize.Width / this._columns, availableSize.Height / this._rows);
			double width = 0;
			double height = 0;


			// JJD 11/4/11 - TFS79499
			// If UseLayoutRounding is true we need to make sure the sizes we allocate to each item
			// are on pixel boundaries so strip off the decimal portion of the width and height.
			// Note: we will allocate any extra pixels in the ArrangeOverride method
			if (this.UseLayoutRounding)
			{
				itemSize.Width = Math.Floor(itemSize.Width);
				itemSize.Height = Math.Floor(itemSize.Height);
			}


			for(int i = 0, count = this.Children.Count; i < count; i++)
			{
				UIElement element = this.Children[i];

				element.Measure(itemSize);

				Size desiredSize = element.DesiredSize;

				if (desiredSize.Width > width)
					width = desiredSize.Width;
				if (desiredSize.Height > height)
					height = desiredSize.Height;
			}

			Size tempSize = new Size(this._columns * width, this._rows * height);

			if (preferColumns && tempSize.Width > availableSize.Width)
			{
				this.CalculateMetrics(false);
				tempSize = new Size(this._columns * width, this._rows * height);
			}

			return tempSize;
        } 
		#endregion //MeasureOverride
    
		#region ArrangeOverride
		/// <summary>
		/// Positions child elements and determines a size for this element.
		/// </summary>
		/// <param name="finalSize">The size available to this element for arranging its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> used by this element to arrange its children.</returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
			double itemWidth = finalSize.Width / this._columns;
			double itemHeight = finalSize.Height / this._rows;

			// JJD 11/4/11 - TFS79499
			// If UseLayoutRounding is true we need to make sure the sizes we allocate to each item
			// are on pixel boundaries so create stack variables to hold any extra width and height
			double extraWidth = 0;
			double extraHeight = 0;


			if (this.UseLayoutRounding)
			{
				// JJD 11/4/11 - TFS79499
				// strip off the decimal portion of the width and height
				itemWidth = Math.Floor(itemWidth);
				itemHeight = Math.Floor(itemHeight);

				// JJD 11/7/11 - TFS79499
				// figure out the extra size, if any 
				// Note: we will allocate this to the first few items in the rows and columns, 1 pixel at a time
				// Use the floor of the calculation
				extraWidth = Math.Floor( finalSize.Width - (itemWidth * _columns));
				extraHeight = Math.Floor( finalSize.Height - (itemHeight * _rows));
			}


			Rect finalRect = new Rect(itemWidth * this._firstCol, itemHeight * this._firstRow, itemWidth, itemHeight);
            int column = this._firstCol;

			// JJD 11/4/11 - TFS79499
			double extraAvailableWidth = extraWidth;
			double extraAvailableHeight = extraHeight;

			// JJD 11/7/11 - TFS79499
			// If the first column is not 0 and we have extra width 
			// make sure we offset the first item's X to account for any adjustments that
			// would have been made to the previous columns in a full row
			finalRect.X += CalculateLayoutRoundingOffset(_firstCol, ref extraAvailableWidth);

			// JJD 11/7/11 - TFS79499
			// If the first row is not 0 and we have extra height 
			// make sure we offset the first item's Y to account for any adjustments that
			// would have been made to the previous rows
			finalRect.Y += CalculateLayoutRoundingOffset(_firstRow, ref extraAvailableHeight);

			for(int i = 0, count = this.Children.Count; i < count; i++)
			{
				UIElement element = this.Children[i];

				// JJD 11/4/11 - TFS79499
				// Moved arrange below
				//element.Arrange(finalRect);

				if (element.Visibility != Visibility.Collapsed)
				{
                    column++;

					// JJD 11/4/11 - TFS79499
					// initialize the rect size to the one calculated above
					finalRect.Width = itemWidth;
					finalRect.Height = itemHeight;

					// JJD 11/4/11 - TFS79499
					// if we have extra height to allocate then add 1 to the rect's height
					if (extraAvailableHeight > 0)
						finalRect.Height += Math.Min(1.0, extraAvailableHeight);

					// JJD 11/4/11 - TFS79499
					// if we have extra width to allocate then add 1 to the rect's width
					if (extraAvailableWidth > 0)
					{
						finalRect.Width += Math.Min(1.0, extraAvailableWidth);

						// JJD 11/4/11 - TFS79499
						// Since this loop snakes over columns then rows
						// we should immediately decrement the extra available width
						// by 1
						extraAvailableWidth = Math.Max(extraAvailableWidth - 1, 0);
					}
				
					// JJD 11/4/11 - TFS79499
					// Arrange the element with the adjusted rect
					element.Arrange(finalRect);

                    if (column < this._columns)
					{
						// JJD 11/7/11 - TFS79499
						// Use the adjusted width to do the offset
						//finalRect.X += itemWidth;
						finalRect.X += finalRect.Width;
					}
					else
					{
                        column = 0;
						finalRect.X = 0;
						// JJD 11/7/11 - TFS79499
						// Use the adjusted height to do the offset
						//finalRect.Y += itemHeight;
						finalRect.Y += finalRect.Height;

						// JJD 11/4/11 - TFS79499
						// Since we are about to move to a new row we need to decrement
						// the extraAvailableHeight by 1 and reset the extra available width for the next row
						// since this loop snakes over columns then rows
						extraAvailableHeight = Math.Max(extraAvailableHeight - 1, 0);
						extraAvailableWidth = extraWidth;
					}
				}
			}

			return finalSize;
        } 
		#endregion //ArrangeOverride

		#endregion //Base class overrides
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