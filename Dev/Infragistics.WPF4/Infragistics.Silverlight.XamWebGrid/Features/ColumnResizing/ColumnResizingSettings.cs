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

namespace Infragistics.Controls.Grids
{
	/// <summary>
	/// An object that contains settings for using ColumnResizingSettings on the <see cref="XamGrid"/>
	/// </summary>
	public class ColumnResizingSettings : SettingsBase
	{
		#region IndicatorStyle

		/// <summary>
		/// Identifies the <see cref="IndicatorStyle"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty IndicatorStyleProperty = DependencyProperty.Register("IndicatorStyle", typeof(Style), typeof(ColumnResizingSettings), new PropertyMetadata(new PropertyChangedCallback(IndicatorStyleChanged)));

		/// <summary>
		/// Gets/sets the <see cref="Style"/> that will be used to identify where a <see cref="Column"/> will be resized.
		/// </summary>
		public Style IndicatorStyle
		{
			get { return (Style)this.GetValue(IndicatorStyleProperty); }
			set { this.SetValue(IndicatorStyleProperty, value); }
		}

		private static void IndicatorStyleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			ColumnResizingSettings settings = (ColumnResizingSettings)obj;
			settings.OnPropertyChanged("IndicatorStyle");
		}

		#endregion // IndicatorStyle

		#region AllowColumnResizing

		/// <summary>
		/// Identifies the <see cref="AllowColumnResizing"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty AllowColumnResizingTypeProperty = DependencyProperty.Register("AllowColumnResizing", typeof(ColumnResizingType), typeof(ColumnResizingSettings), new PropertyMetadata(ColumnResizingType.Immediate, new PropertyChangedCallback(AllowColumnResizingChanged)));

		/// <summary>
		/// Gets/sets how column resizing will work for all <see cref="ColumnLayout"/> objects in the <see cref="XamGrid"/>
		/// </summary>
		public ColumnResizingType AllowColumnResizing
		{
			get { return (ColumnResizingType)this.GetValue(AllowColumnResizingTypeProperty); }
			set { this.SetValue(AllowColumnResizingTypeProperty, value); }
		}

		private static void AllowColumnResizingChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			ColumnResizingSettings settings = (ColumnResizingSettings)obj;
			settings.OnPropertyChanged("AllowColumnResizing");
		}

		#endregion // AllowColumnMoving

		#region AllowDoubleClickToSize

		/// <summary>
		/// Identifies the <see cref="AllowDoubleClickToSize"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty AllowDoubleClickToSizeProperty = DependencyProperty.Register("AllowDoubleClickToSize", typeof(bool), typeof(ColumnResizingSettings), new PropertyMetadata(true, new PropertyChangedCallback(AllowDoubleClickToSizeChanged)));

		/// <summary>
		/// Gets/sets if double clicking on the column edge will resize for all <see cref="ColumnLayout"/> objects in the <see cref="XamGrid"/>
		/// </summary>
		public bool AllowDoubleClickToSize
		{
			get { return (bool)this.GetValue(AllowDoubleClickToSizeProperty); }
			set { this.SetValue(AllowDoubleClickToSizeProperty, value); }
		}

		private static void AllowDoubleClickToSizeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			ColumnResizingSettings settings = (ColumnResizingSettings)obj;
			settings.OnPropertyChanged("AllowDoubleClickToSize");
		}

		#endregion // AllowDoubleClickToSize

		#region AllowMultipleColumnResize
		/// <summary>
		/// Identifies the <see cref="AllowMultipleColumnResize"/> dependency property. 
		/// </summary>		
		public static readonly DependencyProperty AllowMultipleColumnResizeProperty = DependencyProperty.Register("AllowMultipleColumnResize", typeof(bool), typeof(ColumnResizingSettings), new PropertyMetadata(true, new PropertyChangedCallback(AllowMultipleColumnResizeChanged)));

		/// <summary>
		/// Gets/sets if multiple column resizing will work for all <see cref="ColumnLayout"/> objects in the <see cref="XamGrid"/>
		/// </summary>
		public bool AllowMultipleColumnResize
		{
			get { return (bool)this.GetValue(AllowMultipleColumnResizeProperty); }
			set { this.SetValue(AllowMultipleColumnResizeProperty, value); }
		}

		private static void AllowMultipleColumnResizeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			ColumnResizingSettings settings = (ColumnResizingSettings)obj;
			settings.OnPropertyChanged("AllowMultipleColumnResize");
		}

		#endregion // AllowMultipleColumnResize


		#region AllowCellAreaResizing
		/// <summary>
		/// Identifies the <see cref="AllowCellAreaResizing"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty AllowCellAreaResizingProperty = DependencyProperty.Register("AllowCellAreaResizing", typeof(bool), typeof(ColumnResizingSettings), new PropertyMetadata(false, new PropertyChangedCallback(AllowCellAreaResizingChanged)));

		/// <summary>
		/// Gets/sets cell area resizing will work for all <see cref="ColumnLayout"/> objects in the <see cref="XamGrid"/>
		/// </summary>
		public bool AllowCellAreaResizing
		{
			get { return (bool)this.GetValue(AllowCellAreaResizingProperty); }
			set { this.SetValue(AllowCellAreaResizingProperty, value); }
		}

		private static void AllowCellAreaResizingChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			ColumnResizingSettings settings = (ColumnResizingSettings)obj;
			settings.OnPropertyChanged("AllowCellAreaResizing");
		}

		#endregion // AllowCellAreaResizing
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