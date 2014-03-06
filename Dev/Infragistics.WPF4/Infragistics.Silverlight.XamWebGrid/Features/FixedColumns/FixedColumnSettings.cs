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
using System.ComponentModel;
using System.Windows.Controls.Primitives;
using Infragistics.Controls.Grids.Primitives;

namespace Infragistics.Controls.Grids
{
	/// <summary>
	/// An object that contains settings for using FixedColumns on the <see cref="XamGrid"/>
	/// </summary>
	public class FixedColumnSettings : SettingsBase
	{
		#region AllowFixedColumns

		/// <summary>
		/// Identifies the <see cref="AllowFixedColumns"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty AllowFixedColumnsProperty = DependencyProperty.Register("AllowFixedColumns", typeof(FixedColumnType), typeof(FixedColumnSettings), new PropertyMetadata(FixedColumnType.Disabled, new PropertyChangedCallback(AllowFixedColumnsChanged)));

		/// <summary>
		/// Gets/Sets how Fixed columns will be enabled via the UI for all <see cref="ColumnLayout"/> objects in the <see cref="XamGrid"/>
		/// </summary>
		public FixedColumnType AllowFixedColumns
		{
			get { return (FixedColumnType)this.GetValue(AllowFixedColumnsProperty); }
			set { this.SetValue(AllowFixedColumnsProperty, value); }
		}

		private static void AllowFixedColumnsChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			FixedColumnSettings settings = (FixedColumnSettings)obj;
			settings.OnPropertyChanged("AllowFixedColumns");
		}

		#endregion // AllowFixedColumns

		#region FixedIndicatorDirection

		/// <summary>
		/// Identifies the <see cref="FixedIndicatorDirection"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty FixedIndicatorDirectionProperty = DependencyProperty.Register("FixedIndicatorDirection", typeof(FixedIndicatorDirection), typeof(FixedColumnSettings), new PropertyMetadata(new PropertyChangedCallback(FixedDirectionChanged)));

		/// <summary>
		/// Gets/Sets what side of the <see cref="XamGrid"/> a column should be locked to if the <see cref="Column.IsFixed"/> property is set to true for all <see cref="ColumnLayout"/> objects in the <see cref="XamGrid"/>.
		/// </summary>
		public FixedIndicatorDirection FixedIndicatorDirection
		{
			get { return (FixedIndicatorDirection)this.GetValue(FixedIndicatorDirectionProperty); }
			set { this.SetValue(FixedIndicatorDirectionProperty, value); }
		}

		private static void FixedDirectionChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			FixedColumnSettings settings = (FixedColumnSettings)obj;
			settings.OnPropertyChanged("FixedIndicatorDirection");
		}

		#endregion // FixedIndicatorDirection 

		#region FixedDropAreaLocation

		/// <summary>
		/// Identifies the <see cref="FixedDropAreaLocation"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty FixedDropAreaLocationProperty = DependencyProperty.Register("FixedDropAreaLocation", typeof(FixedDropAreaLocation), typeof(FixedColumnSettings), new PropertyMetadata(new PropertyChangedCallback(FixedDropAreaLocationChanged)));

		/// <summary>
		/// Gets/Sets where a drop area will be displayed on the <see cref="XamGrid"/> when a <see cref="Column"/> is dragged,
		/// so that user can drop a column in order to lock it in place so that it can'type be scrolled.
		/// </summary>
		public FixedDropAreaLocation FixedDropAreaLocation
		{
			get { return (FixedDropAreaLocation)this.GetValue(FixedDropAreaLocationProperty); }
			set { this.SetValue(FixedDropAreaLocationProperty, value); }
		}

		private static void FixedDropAreaLocationChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			FixedColumnSettings settings = (FixedColumnSettings)obj;
			settings.OnPropertyChanged("FixedDropAreaLocation");
		}

		#endregion // FixedDropAreaLocation 

		#region FixedBorderStyle

		/// <summary>
		/// Identifies the <see cref="FixedBorderStyle"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty FixedBorderStyleProperty = DependencyProperty.Register("FixedBorderStyle", typeof(Style), typeof(FixedColumnSettings), new PropertyMetadata(new PropertyChangedCallback(FixedBorderStyleChanged)));

		/// <summary>
		/// Gets/sets the <see cref="Style"/> that will be used on the <see cref="FixedBorderCellControl"/> objects of the <see cref="ColumnLayout"/>.
		/// </summary>
		public Style FixedBorderStyle
		{
			get { return (Style)this.GetValue(FixedBorderStyleProperty); }
			set { this.SetValue(FixedBorderStyleProperty, value); }
		}

		private static void FixedBorderStyleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			FixedColumnSettings settings = (FixedColumnSettings)obj;
			settings.OnPropertyChanged("FixedBorderStyle");
		}

		#endregion // FixedBorderStyle

		#region FixedBorderHeaderStyle

		/// <summary>
		/// Identifies the <see cref="FixedBorderHeaderStyle"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty FixedBorderHeaderStyleProperty = DependencyProperty.Register("FixedBorderHeaderStyle", typeof(Style), typeof(FixedColumnSettings), new PropertyMetadata(new PropertyChangedCallback(FixedBorderHeaderStyleChanged)));

		/// <summary>
		/// Gets/sets the <see cref="Style"/> that will be used on the <see cref="FixedBorderHeaderCellControl"/> objects of the <see cref="ColumnLayout"/>.
		/// </summary>
		public Style FixedBorderHeaderStyle
		{
			get { return (Style)this.GetValue(FixedBorderHeaderStyleProperty); }
			set { this.SetValue(FixedBorderHeaderStyleProperty, value); }
		}

		private static void FixedBorderHeaderStyleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			FixedColumnSettings settings = (FixedColumnSettings)obj;
			settings.OnPropertyChanged("FixedBorderHeaderStyle");
		}

		#endregion // FixedBorderHeaderStyle

		#region FixedBorderFooterStyle

		/// <summary>
		/// Identifies the <see cref="FixedBorderFooterStyle"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty FixedBorderFooterStyleProperty = DependencyProperty.Register("FixedBorderFooterStyle", typeof(Style), typeof(FixedColumnSettings), new PropertyMetadata(new PropertyChangedCallback(FixedBorderFooterStyleChanged)));

		/// <summary>
		/// Gets/sets the <see cref="Style"/> that will be used on the <see cref="FixedBorderFooterCellControl"/> objects of the <see cref="ColumnLayout"/>.
		/// </summary>
		public Style FixedBorderFooterStyle
		{
			get { return (Style)this.GetValue(FixedBorderFooterStyleProperty); }
			set { this.SetValue(FixedBorderFooterStyleProperty, value); }
		}

		private static void FixedBorderFooterStyleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			FixedColumnSettings settings = (FixedColumnSettings)obj;
			settings.OnPropertyChanged("FixedBorderFooterStyle");
		}

		#endregion // FixedBorderFooterStyle

		#region FixedDropAreaLeftStyle

		/// <summary>
		/// Identifies the <see cref="FixedDropAreaLeftStyle"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty FixedDropAreaLeftStyleProperty = DependencyProperty.Register("FixedDropAreaLeftStyle", typeof(Style), typeof(FixedColumnSettings), new PropertyMetadata(new PropertyChangedCallback(FixedDropAreaLeftStyleChanged)));

		/// <summary>
		/// Gets/sets the <see cref="Style"/> that will be for the Left DropArea for fixed columns.
		/// </summary>
		public Style FixedDropAreaLeftStyle
		{
			get { return (Style)this.GetValue(FixedDropAreaLeftStyleProperty); }
			set { this.SetValue(FixedDropAreaLeftStyleProperty, value); }
		}

		private static void FixedDropAreaLeftStyleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			FixedColumnSettings settings = (FixedColumnSettings)obj;
			settings.OnPropertyChanged("FixedDropAreaLeftStyle");
		}

		#endregion // FixedDropAreaLeftStyle

		#region FixedDropAreaRightStyle

		/// <summary>
		/// Identifies the <see cref="FixedDropAreaRightStyle"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty FixedDropAreaRightStyleProperty = DependencyProperty.Register("FixedDropAreaRightStyle", typeof(Style), typeof(FixedColumnSettings), new PropertyMetadata(new PropertyChangedCallback(FixedDropAreaRightStyleChanged)));

		/// <summary>
		/// Gets/sets the <see cref="Style"/> that will be for the Left DropArea for fixed columns.
		/// </summary>
		public Style FixedDropAreaRightStyle
		{
			get { return (Style)this.GetValue(FixedDropAreaRightStyleProperty); }
			set { this.SetValue(FixedDropAreaRightStyleProperty, value); }
		}

		private static void FixedDropAreaRightStyleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			FixedColumnSettings settings = (FixedColumnSettings)obj;
			settings.OnPropertyChanged("FixedDropAreaRightStyle");
		}

		#endregion // FixedDropAreaRightStyle

		#region FixedColumnsLeft

		/// <summary>
		/// Gets a collection of <see cref="Column"/> objects that are pinned to the left side of a particular <see cref="ColumnLayout"/>
		/// </summary>
        [Browsable(false)]
		public FixedColumnsCollection FixedColumnsLeft
		{
			get
			{
				FixedColumnsCollection collection = null;

				if (this.Grid != null)
					collection =  this.Grid.RowsManager.ColumnLayout.FixedColumnSettings.FixedColumnsLeft;
				
				return collection;
			}
		}

		#endregion // FixedColumnsLeft

		#region FixedColumnsRight

		/// <summary>
		/// Gets a collection of <see cref="Column"/> objects that are pinned to the right side of a particular <see cref="ColumnLayout"/>
		/// </summary>
        [Browsable(false)]
		public FixedColumnsCollection FixedColumnsRight
		{
			get
			{
				FixedColumnsCollection collection = null;

				if (this.Grid != null)
					collection = this.Grid.RowsManager.ColumnLayout.FixedColumnSettings.FixedColumnsRight;
					
				return collection;
			}
		}

		#endregion // FixedColumnsRight

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