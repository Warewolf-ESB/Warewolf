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
using System.Collections.Generic;
using System.Diagnostics;

namespace Infragistics.Controls.Primitives
{
	/// <summary>
	/// A custom wrap panel used to arrange items horizontally or vertically in as few columns as possible.
	/// </summary>
	/// <remarks>
	/// <p class="body">The CompactWrapPanel is a specialized type of WrapPanel that will fit as many items as possible into a single 
	/// "row" down to the <see cref="MinItemExtent"/>. The items are arranged based on the <see cref="Orientation"/> so they will be arranged 
	/// from left to right when set to Horizontal and from top to bottom when set to Vertical. The number of items on each row is based on 
	/// the extent with which the panel is measured in comparison to the <see cref="MinItemExtent"/> (and also considering the spacing between 
	/// the items - <see cref="InterItemSpacingX"/> and <see cref="InterItemSpacingY"/>). So for example if the <see cref="Orientation"/> is 
	/// set to Horizontal, the <see cref="MinItemExtent"/> is set to 100, the <see cref="InterItemSpacingX"/> is 0, there are 5 visible items 
	/// in the Children collection and the panel is measured with width of 360, the items will be arranged horizontally such that 3 items 
	/// (with a width of 120 since they must be at least 100 pixels) in the first row and 2 items (with a width of 180) in the second row. 
	/// As the width decreased to 300 and increased to 400, the width of the items would decrease/increase respectively. If the panel is 
	/// resized beyond that range, the items will be rearranged. For example, if the panel was then resized to 400 pixels, there would be 
	/// 4 items in the first row (with a width of 100 pixels) and 1 item in the last row (with a width of 400 pixels).</p>
	/// </remarks>
	public class CompactWrapPanel : Panel
	{
		#region Member Variables

		private List<Rect> _itemPositions;

		#endregion // Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="CompactWrapPanel"/>
		/// </summary>
		public CompactWrapPanel()
		{
			_itemPositions = new List<Rect>();
		}
		#endregion // Constructor

		#region Base class overrides

		#region ArrangeOverride

		/// <summary>
		/// Positions child elements and determines a size for this element.
		/// </summary>
		/// <param name="finalSize">The size available to this element for arranging its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> used by this element to arrange its children.</returns>
		protected override System.Windows.Size ArrangeOverride( System.Windows.Size finalSize )
		{
			var children = this.Children;

			Debug.Assert(children.Count == this._itemPositions.Count, "Item counts are out of sync");

			if ( null != _itemPositions )
			{
				int count = Math.Min(children.Count, _itemPositions.Count);

				for ( int i = 0; i < count; i++ )
				{
					UIElement element = children[i];
					element.Arrange(_itemPositions[i]);
				}
			}

			return finalSize;
		}
		#endregion //ArrangeOverride

		#region MeasureOverride
		/// <summary>
		/// Invoked to measure the element and its children.
		/// </summary>
		/// <param name="availableSize">The size that reflects the available size that this element can give to its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> that represents the desired size of the element.</returns>
		protected override Size MeasureOverride( Size availableSize )
		{
			return this.MeasureItems(availableSize);
		}
		#endregion //MeasureOverride

		#endregion // Base class overrides

		#region Properties

		#region InterItemSpacingX

		/// <summary>
		/// Identifies the <see cref="InterItemSpacingX"/> dependency property
		/// </summary>
		public static readonly DependencyProperty InterItemSpacingXProperty = DependencyPropertyUtilities.Register("InterItemSpacingX",
			typeof(double), typeof(CompactWrapPanel),
			DependencyPropertyUtilities.CreateMetadata(2d, new PropertyChangedCallback(OnMetricInvalidated))
			);

		/// <summary>
		/// Returns or sets the amount of horizontal space between items.
		/// </summary>
		/// <seealso cref="InterItemSpacingXProperty"/>
		public double InterItemSpacingX
		{
			get
			{
				return (double)this.GetValue(CompactWrapPanel.InterItemSpacingXProperty);
			}
			set
			{
				this.SetValue(CompactWrapPanel.InterItemSpacingXProperty, value);
			}
		}

		#endregion //InterItemSpacingX

		#region InterItemSpacingY

		/// <summary>
		/// Identifies the <see cref="InterItemSpacingY"/> dependency property
		/// </summary>
		public static readonly DependencyProperty InterItemSpacingYProperty = DependencyPropertyUtilities.Register("InterItemSpacingY",
			typeof(double), typeof(CompactWrapPanel),
			DependencyPropertyUtilities.CreateMetadata(2d, new PropertyChangedCallback(OnMetricInvalidated))
			);

		/// <summary>
		/// Returns or sets the amount of vertical space between items.
		/// </summary>
		/// <seealso cref="InterItemSpacingYProperty"/>
		public double InterItemSpacingY
		{
			get
			{
				return (double)this.GetValue(CompactWrapPanel.InterItemSpacingYProperty);
			}
			set
			{
				this.SetValue(CompactWrapPanel.InterItemSpacingYProperty, value);
			}
		}

		#endregion //InterItemSpacingY

		#region MinItemExtent

		/// <summary>
		/// Identifies the <see cref="MinItemExtent"/> dependency property
		/// </summary>
		public static readonly DependencyProperty MinItemExtentProperty = DependencyPropertyUtilities.Register("MinItemExtent",
			typeof(double), typeof(CompactWrapPanel),
			DependencyPropertyUtilities.CreateMetadata(75d, new PropertyChangedCallback(OnMetricInvalidated))
			);

		/// <summary>
		/// Returns or sets the minimum width of the items when the Orientation is horizontal and the minimum height when the Orientation is vertical.
		/// </summary>
		/// <seealso cref="MinItemExtentProperty"/>
		public double MinItemExtent
		{
			get
			{
				return (double)this.GetValue(CompactWrapPanel.MinItemExtentProperty);
			}
			set
			{
				this.SetValue(CompactWrapPanel.MinItemExtentProperty, value);
			}
		}

		#endregion //MinItemExtent

		#region Orientation

		/// <summary>
		/// Identifies the <see cref="Orientation"/> dependency property
		/// </summary>
		public static readonly DependencyProperty OrientationProperty = DependencyPropertyUtilities.Register("Orientation",
			typeof(Orientation), typeof(CompactWrapPanel),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.OrientationHorizontalBox, new PropertyChangedCallback(OnMetricInvalidated))
			);

		/// <summary>
		/// Returns or sets the orientation in which the children will be arranged.
		/// </summary>
		/// <seealso cref="OrientationProperty"/>
		public Orientation Orientation
		{
			get
			{
				return (Orientation)this.GetValue(CompactWrapPanel.OrientationProperty);
			}
			set
			{
				this.SetValue(CompactWrapPanel.OrientationProperty, value);
			}
		}

		#endregion //Orientation

		#endregion // Properties

		#region Methods

		#region AdjustRowHeights
		private void AdjustRowHeights( List<Rect> positions, int start, int end, double rowHeight, bool isVertical )
		{
			for ( int i = start; i <= end; i++ )
			{
				var rect = positions[i];
				double currentHeight = isVertical ? rect.Width : rect.Height;

				if ( currentHeight > 0 )
				{
					if ( isVertical )
						rect.Width = rowHeight;
					else
						rect.Height = rowHeight;

					positions[i] = rect;
				}
			}
		}
		#endregion // AdjustRowHeights

		#region CalculateInterItemSpacing
		private static double CalculateInterItemSpacing( double spacing, bool useLayoutRounding )
		{
			return useLayoutRounding ? Math.Round(spacing) : spacing;
		}
		#endregion // CalculateInterItemSpacing

		#region CalculateItemExtent
		private static double CalculateItemExtent( double availableExtent, int columnCount, double interItemSpacing, bool useLayoutRounding, out int extraSpaceItems )
		{
			extraSpaceItems = 0;
			double itemExtent;

			Debug.Assert(columnCount > 0);

			if ( double.IsPositiveInfinity(availableExtent) )
				itemExtent = 100d;
			else
			{
				// remove the space needed for the interitemspacing
				availableExtent -= interItemSpacing * (columnCount - 1);

				// assuming we have a finite space give each item an equal portion
				itemExtent = availableExtent / columnCount;

				if ( useLayoutRounding )
				{
					// if we're using layout rounding then give the items integral values
					itemExtent = Math.Floor(itemExtent);

					// and give the last X number of items an extra pixel
					extraSpaceItems = (int)(availableExtent - (itemExtent * columnCount));
				}
			}

			return itemExtent;
		}
		#endregion // CalculateItemExtent

		#region MeasureItems
		private Size MeasureItems( Size availableSize )
		{
			bool isVertical = this.Orientation == Orientation.Vertical;

			if ( isVertical )
				Swap(ref availableSize);

			var children = this.Children;
			int childCount = children.Count;
			List<Rect> itemPositions = _itemPositions;
			bool useLayoutRounding = this.UseLayoutRounding;

			#region Initialize ItemPositions

			itemPositions.Clear();
			int visibleItemCount = 0;

			foreach ( UIElement item in children )
			{
				Rect itemRect;

				// we don't need to calculate/include information for collapsed elements
				if ( item.Visibility == Visibility.Collapsed )
				{
					itemRect = new Rect();
				}
				else
				{
					visibleItemCount++;
					itemRect = Rect.Empty;
				}

				itemPositions.Add(itemRect);
			} 
			#endregion // Initialize ItemPositions

			if ( visibleItemCount == 0 )
				return new Size();

			double availableColumnExtent = availableSize.Width;
			double availableRowExtent = availableSize.Height;
			double minExtent = this.MinItemExtent;

			if ( minExtent > availableColumnExtent )
				minExtent = availableColumnExtent;
			else if ( minExtent < 1d || double.IsNaN(minExtent) || double.IsInfinity(minExtent) )
				minExtent = 1d;

			// round the spacing if needed
			double interColumnSpacing = CalculateInterItemSpacing(Math.Max(this.InterItemSpacingX, 0), useLayoutRounding);
			double interRowSpacing = CalculateInterItemSpacing(Math.Max(this.InterItemSpacingY, 0), useLayoutRounding);

			if ( isVertical )
				Swap(ref interColumnSpacing, ref interRowSpacing);

			// figure out how many "columns" we have having at least 1 per column but no more than the number of items
			// then we can know how many "rows" we have putting as many items as possible in each row.
			int columnCount = double.IsPositiveInfinity(availableColumnExtent) ? visibleItemCount : (int)Math.Min(Math.Max((availableColumnExtent + interColumnSpacing) / (minExtent + interColumnSpacing), 1), visibleItemCount);
			int rowCount = (int)Math.Min(((visibleItemCount - 1) / columnCount) + 1, visibleItemCount);

			int row = 0;
			int column = 0;
			int rowItemCount = columnCount;
			int extraSpaceColumns = 0;
			double columnExtent = double.IsPositiveInfinity(availableColumnExtent) ? minExtent : CalculateItemExtent(availableColumnExtent, columnCount, interColumnSpacing, useLayoutRounding, out extraSpaceColumns);
			double rowExtent = double.PositiveInfinity;

			Debug.Assert(extraSpaceColumns < columnCount);

			Size baseMeasureSize = new Size(columnExtent, rowExtent);
			double x = 0;
			double y = 0;
			double rowHeight = 0;
			int rowStartIndex = 0;

			Size desiredSize = new Size(availableSize.Width, 0);
			bool isNewRow = false;

			for ( int i = 0; i < childCount; i++ )
			{
				var child = children[i];

				Rect elementRect = itemPositions[i];

				// skip the items that are collapsed. we already calculated their size at 
				if ( elementRect.IsEmpty )
				{
					elementRect = new Rect(x, y, baseMeasureSize.Width, baseMeasureSize.Height);

					if ( column < extraSpaceColumns )
						elementRect.Width++;

					// adjust the x for the next item
					x += elementRect.Width + interColumnSpacing;

					// increment the column count so we know how many items in the row we've used
					column++;

					// if we've filled the row then prepare for the next row
					if ( column == rowItemCount )
					{
						isNewRow = true;
					}

					if ( isVertical )
						Swap(ref elementRect);

					itemPositions[i] = elementRect;
				}

				child.Measure(new Size(elementRect.Width, elementRect.Height));

				Size desired = child.DesiredSize;

				if ( isVertical )
					Swap(ref desired);

				// keep track of the tallest item in the "row"
				rowHeight = Math.Max(rowHeight, desired.Height);

				if ( isNewRow )
				{
					isNewRow = false;

					// update the "height" of the items in this row
					this.AdjustRowHeights(itemPositions, rowStartIndex, i, rowHeight, isVertical);
					desiredSize.Height += rowHeight;

					// prepare for the next row
					x = 0;
					y += rowHeight + interRowSpacing;
					rowHeight = 0;
					rowStartIndex = i + 1;
					row++;
					column = 0;

					// for the last row we may have less than the number of items in the previous rows
					if ( row == rowCount - 1 )
					{
						rowItemCount = visibleItemCount % columnCount;

						if ( rowItemCount == 0 )
							rowItemCount = columnCount;
						else
						{
							columnExtent = CalculateItemExtent(availableColumnExtent, rowItemCount, interColumnSpacing, useLayoutRounding, out extraSpaceColumns);
							baseMeasureSize = new Size(columnExtent, rowExtent);
						}
					}
				}
			}

			// handle the row height for the last row
			if ( rowStartIndex < childCount )
			{
				this.AdjustRowHeights(itemPositions, rowStartIndex, childCount - 1, rowHeight, isVertical);
				desiredSize.Height += rowHeight;
			}

			desiredSize.Height += interRowSpacing * (rowCount - 1);
			desiredSize.Width = (columnExtent * columnCount) + (interColumnSpacing * (columnCount - 1));

			if ( isVertical )
				Swap(ref desiredSize);

			return desiredSize;
		}
		#endregion // MeasureItems

		#region OnMetricInvalidated
		private static void OnMetricInvalidated( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			((UIElement)d).InvalidateMeasure();
		}
		#endregion // OnMetricInvalidated

		#region Swap
		private static void Swap( ref Rect rect )
		{
			rect = new Rect(rect.Y, rect.X, rect.Height, rect.Width);
		}

		private static void Swap( ref Size size )
		{
			size = new Size(size.Height, size.Width);
		}

		private static void Swap( ref double x, ref double y )
		{
			double temp = y;
			y = x;
			x = temp;
		}
		#endregion // Swap

		#endregion // Methods
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