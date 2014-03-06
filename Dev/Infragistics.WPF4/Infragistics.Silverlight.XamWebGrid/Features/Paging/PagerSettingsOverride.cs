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
using System.Globalization;
using Infragistics.Controls.Grids.Primitives;

namespace Infragistics.Controls.Grids
{
	/// <summary>
	/// An object that contains settings for using PagerSettings on a particular <see cref="ColumnLayout"/>
	/// </summary>
	[StyleTypedProperty(Property = "Style", StyleTargetType = typeof(PagerCellControl))]
	public class PagerSettingsOverride : StyleSettingsOverrideBase
	{
		#region Overrides

		#region SettingsObject

		/// <summary>
		/// Gets the <see cref="SettingsBase"/> that is the counterpart to this <see cref="SettingsOverrideBase"/>
		/// </summary>
		protected override SettingsBase SettingsObject
		{
			get
			{
				SettingsBase settings = null;
				if (this.ColumnLayout != null && this.ColumnLayout.Grid != null)
					settings = this.ColumnLayout.Grid.PagerSettings;
				return settings;
			}
		}

		#endregion // SettingsObject

		#endregion // Overrides

		#region Properties

		#region AllowPaging

		/// <summary>   
		/// Identifies the <see cref="AllowPaging"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty AllowPagingProperty = DependencyProperty.Register("AllowPaging", typeof(PagingLocation?), typeof(PagerSettingsOverride), new PropertyMetadata(new PropertyChangedCallback(AllowPagingChanged)));

		/// <summary>
		/// Gets / sets if paging should be allowed at this level.  A null value will use the setting from the <see cref="PagerSettings.AllowPaging"/>.
		/// </summary>
		[TypeConverter(typeof(NullableEnumTypeConverter<PagingLocation>))]
		public PagingLocation? AllowPaging
		{
			get { return (PagingLocation?)this.GetValue(AllowPagingProperty); }
			set { this.SetValue(AllowPagingProperty, value); }
		}

		private static void AllowPagingChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			PagerSettingsOverride settings = (PagerSettingsOverride)obj;
			settings.OnPropertyChanged("AllowPaging");
		}

		#endregion // AllowPaging

		#region AllowPagingResolved
		/// <summary>
		/// Gets if PagingLocation is enabled on the ColumnLayout object.  If it is not explicitly set will take the value from the <see cref="PagerSettings.AllowPaging"/> value.
		/// </summary>
		public PagingLocation AllowPagingResolved
		{
            get
            {
                if (this.AllowPaging == null)
                {
                    if (this.SettingsObject != null)
                        return ((PagerSettings)this.SettingsObject).AllowPaging;
                }
                else
                    return (PagingLocation)this.AllowPaging;

                return (PagingLocation)PagerSettings.AllowPagingProperty.GetMetadata(typeof(PagerSettings)).DefaultValue;
            }
		}
		#endregion //AllowPagingResolved

		#region PageSize

		/// <summary>   
		/// Identifies the <see cref="PageSize"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty PageSizeProperty = DependencyProperty.Register("PageSize", typeof(int), typeof(PagerSettingsOverride), new PropertyMetadata(0, new PropertyChangedCallback(PageSizeChanged)));

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
			PagerSettingsOverride settings = (PagerSettingsOverride)obj;
			settings.OnPropertyChanged("PageSize");
		}

		#endregion // PageSize

		#region PageSizeResolved
		/// <summary>
		/// Gets what the page size will be.  If it is not explicitly set will take the value from the <see cref="SortingSettings.AllowMultipleColumnSorting"/> value.
		/// </summary>
		public int PageSizeResolved
		{
			get
			{
                if (this.PageSize < 1)
                {
                    if (this.SettingsObject != null)
                    {
                        return ((PagerSettings)this.SettingsObject).PageSize;
                    }
                }
                else
                    return (int)this.PageSize;

                return (int)PagerSettings.PageSizeProperty.GetMetadata(typeof(PagerSettings)).DefaultValue;
			}
		}
		#endregion //PageSizeResolved

		#region PagerRowHeight

		/// <summary>
		/// Identifies the <see cref="PagerRowHeight"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty PagerRowHeightProperty = DependencyProperty.Register("PagerRowHeight", typeof(RowHeight?), typeof(PagerSettingsOverride), new PropertyMetadata(null, new PropertyChangedCallback(PagerRowHeightChanged)));

		/// <summary>
		/// Gets/Sets the RowHeight for the <see cref="PagerRow"/> for a particular <see cref="ColumnLayout"/>
		/// </summary>
		[TypeConverter(typeof(RowHeightTypeConverter))]
		public RowHeight? PagerRowHeight
		{
			get { return (RowHeight?)this.GetValue(PagerRowHeightProperty); }
			set { this.SetValue(PagerRowHeightProperty, value); }
		}

		private static void PagerRowHeightChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			PagerSettingsOverride settings = (PagerSettingsOverride)obj;
			settings.OnPropertyChanged("PagerRowHeight");
		}

		#endregion // PagerRowHeight

		#region PagerRowHeightResolved

		/// <summary>
		/// Resolves the <see cref="PagerSettingsOverride.PagerRowHeight"/> property for a particular <see cref="ColumnLayout"/>
		/// </summary>
		public RowHeight PagerRowHeightResolved
		{
            get
            {
                if (this.PagerRowHeight == null)
                {
                    if (this.SettingsObject != null)
                        return ((PagerSettings)this.SettingsObject).PagerRowHeight;
                }
                else
                    return (RowHeight)this.PagerRowHeight;

                return (RowHeight)PagerSettings.PagerRowHeightProperty.GetMetadata(typeof(PagerSettings)).DefaultValue;
            }
		}

		#endregion //PagerRowHeightResolved

		#endregion // Properties
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