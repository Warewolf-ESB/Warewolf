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
using System.Globalization;
using Infragistics.Controls.Grids.Primitives;
using System.ComponentModel;

namespace Infragistics.Controls.Grids
{
	/// <summary>
	/// An object that contains settings for using PagerSettings on the <see cref="XamGrid"/>
	/// </summary>
	[StyleTypedProperty(Property = "Style", StyleTargetType = typeof(PagerCellControl))]
	public class PagerSettings : StyleSettingsBase
	{
		#region AllowPaging

		/// <summary>
		/// Identifies the <see cref="AllowPaging"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty AllowPagingProperty = DependencyProperty.Register("AllowPaging", typeof(PagingLocation), typeof(PagerSettings), new PropertyMetadata(PagingLocation.None, new PropertyChangedCallback(AllowPagingChanged)));

		/// <summary>
		/// Gets / sets if paging will be allowed by default.
		/// </summary>
		public PagingLocation AllowPaging
		{
			get { return (PagingLocation)this.GetValue(AllowPagingProperty); }
			set { this.SetValue(AllowPagingProperty, value); }
		}

		private static void AllowPagingChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			PagerSettings settings = (PagerSettings)obj;
			settings.OnPropertyChanged("AllowPaging");
		}

		#endregion // AllowPaging

		#region DisplayPagerWhenOnlyOnePage

		/// <summary>
		/// Identifies the <see cref="DisplayPagerWhenOnlyOnePage"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty DisplayPagerWhenOnlyOnePageProperty = DependencyProperty.Register("DisplayPagerWhenOnlyOnePage", typeof(bool), typeof(PagerSettings), new PropertyMetadata(true, new PropertyChangedCallback(DisplayPagerWhenOnlyOnePageChanged)));

		/// <summary>
		/// Gets / sets if the Pager Row should be displayed, when there is only enough data for 1 page.
		/// </summary>
		public bool DisplayPagerWhenOnlyOnePage
		{
			get { return (bool)this.GetValue(DisplayPagerWhenOnlyOnePageProperty); }
			set { this.SetValue(DisplayPagerWhenOnlyOnePageProperty, value); }
		}

		private static void DisplayPagerWhenOnlyOnePageChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			PagerSettings settings = (PagerSettings)obj;
			settings.OnPropertyChanged("DisplayPagerWhenOnlyOnePage");
		}

		#endregion // DisplayPagerWhenOnlyOnePage

		#region PageSize

		/// <summary>
		/// Identifies the <see cref="PageSize"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty PageSizeProperty = DependencyProperty.Register("PageSize", typeof(int), typeof(PagerSettings), new PropertyMetadata(10, new PropertyChangedCallback(PageSizeChanged)));

		/// <summary>
		/// Gets / sets the maximum number of rows that will be available per page.
		/// </summary>
		public int PageSize
		{
			get { return (int)this.GetValue(PageSizeProperty); }
			set { this.SetValue(PageSizeProperty, value); }
		}

		private static void PageSizeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
            if ((int)e.NewValue < 1)
            {
                throw new ArgumentOutOfRangeException(string.Format(CultureInfo.CurrentCulture, SRGrid.GetString("PageSizeInvalid"), e.NewValue));
            }
			PagerSettings settings = (PagerSettings)obj;
			settings.OnPropertyChanged("PageSize");
		}

		#endregion // PageSize

		#region CurrentPageIndex

		/// <summary>
		/// Gets / sets what page of data will be displayed.
		/// </summary>
		public int CurrentPageIndex
		{
			get 
			{
				int index = 0;
				
				if(this.Grid != null)
					index = this.Grid.RowsManager.CurrentPageIndex;

				return index;
			}
			set 
			{
                if (value < 1)
                {
                    throw new InvalidPageIndexException(string.Format(CultureInfo.CurrentCulture, SRGrid.GetString("InvalidPageIndexException"), value));
                }
				if(this.Grid != null)
					this.Grid.RowsManager.CurrentPageIndex = value; 
			}
		}

		#endregion // CurrentPageIndex

		#region PagerRowHeight

		/// <summary>
		/// Identifies the <see cref="PagerRowHeight"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty PagerRowHeightProperty = DependencyProperty.Register("PagerRowHeight", typeof(RowHeight), typeof(PagerSettings), new PropertyMetadata(RowHeight.SizeToLargestCell, new PropertyChangedCallback(PagerRowHeightChanged)));

		/// <summary>
		/// Gets/Sets the RowHeight for the <see cref="PagerRow"/> for all <see cref="ColumnLayout"/> objects in the <see cref="XamGrid"/>
		/// </summary>
		[TypeConverter(typeof(RowHeightTypeConverter))]
		public RowHeight PagerRowHeight
		{
			get { return (RowHeight)this.GetValue(PagerRowHeightProperty); }
			set { this.SetValue(PagerRowHeightProperty, value); }
		}

		private static void PagerRowHeightChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			PagerSettings settings = (PagerSettings)obj;
			settings.OnPropertyChanged("PagerRowHeight");
		}

		#endregion // PagerRowHeight 
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