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
using System.Collections.Specialized;

namespace Infragistics.Controls.Grids
{
	/// <summary>
	/// An object that contains settings for using SortingSettings on a particular <see cref="ColumnLayout"/>
	/// </summary>
	public class SortingSettingsOverride : SettingsOverrideBase, IDisposable
	{
		#region Members

		SortedColumnsCollection _sortedColumns;

		#endregion // Members

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
					settings = this.ColumnLayout.Grid.SortingSettings;
				return settings;
			}
		}

		#endregion // SettingsObject

		#endregion // Overrides

		#region Properties

		#region AllowSorting

		/// <summary>   
		/// Identifies the <see cref="AllowSorting"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty AllowSortingProperty = DependencyProperty.Register("AllowSorting", typeof(bool?), typeof(SortingSettingsOverride), new PropertyMetadata(new PropertyChangedCallback(AllowSortingChanged)));

		/// <summary>
		/// Gets / sets if sorting should be allowed at this level.  A null value will use the setting from the <see cref="SortingSettings.AllowSorting"/>.
		/// </summary>
		[TypeConverter(typeof(NullableBoolConverter))]
		public bool? AllowSorting
		{
			get { return (bool?)this.GetValue(AllowSortingProperty); }
			set { this.SetValue(AllowSortingProperty, value); }
		}

		private static void AllowSortingChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			SortingSettingsOverride settings = (SortingSettingsOverride)obj;
			settings.OnPropertyChanged("AllowSorting");
		}

		#endregion // AllowSorting

		#region AllowSortingResolved
		/// <summary>
		/// Gets if sorting is enabled on the ColumnLayout object.  If it is not explicitly set will take the value from the <see cref="SortingSettings.AllowSorting"/> value.
		/// </summary>
		public bool AllowSortingResolved
		{
			get
			{
                if (this.AllowSorting == null)
                {
                    if (this.SettingsObject != null)
                        return ((SortingSettings)this.SettingsObject).AllowSorting;
                }
                else
                    return (bool)this.AllowSorting;

                return (bool)SortingSettings.AllowSortingProperty.GetMetadata(typeof(SortingSettings)).DefaultValue;
			}
		}
		#endregion //AllowSortingResolved

		#region ShowSortIndicator

		/// <summary>   
		/// Identifies the <see cref="ShowSortIndicator"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty ShowSortIndicatorProperty = DependencyProperty.Register("ShowSortIndicator", typeof(bool?), typeof(SortingSettingsOverride), new PropertyMetadata(new PropertyChangedCallback(ShowSortIndicatorChanged)));

		/// <summary>
		/// Gets / sets if sorting should be allowed at this level.  A null value will use the setting from the <see cref="SortingSettings.ShowSortIndicator"/>.
		/// </summary>
		[TypeConverter(typeof(NullableBoolConverter))]
		public bool? ShowSortIndicator
		{
			get { return (bool?)this.GetValue(ShowSortIndicatorProperty); }
			set { this.SetValue(ShowSortIndicatorProperty, value); }
		}

		private static void ShowSortIndicatorChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			SortingSettingsOverride settings = (SortingSettingsOverride)obj;
			settings.OnPropertyChanged("ShowSortIndicator");
		}

		#endregion // ShowSortIndicator

		#region ShowSortIndicatorResolved
		/// <summary>
		/// Gets if sorting is enabled on the ColumnLayout object.  If it is not explicitly set will take the value from the <see cref="SortingSettings.ShowSortIndicator"/> value.
		/// </summary>
		public bool ShowSortIndicatorResolved
		{
			get
			{
                if (this.ShowSortIndicator == null)
                {
                    if (this.SettingsObject != null)
                        return ((SortingSettings)this.SettingsObject).ShowSortIndicator;
                }
                else
                    return (bool)this.ShowSortIndicator;

                return (bool)SortingSettings.ShowSortIndicatorProperty.GetMetadata(typeof(SortingSettings)).DefaultValue;
			}
		}
		#endregion //ShowSortIndicatorResolved

		#region AllowMultipleColumnSorting

		/// <summary>   
		/// Identifies the <see cref="AllowMultipleColumnSorting"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty AllowMultipleColumnSortingProperty = DependencyProperty.Register("AllowMultipleColumnSorting", typeof(bool?), typeof(SortingSettingsOverride), new PropertyMetadata(new PropertyChangedCallback(AllowMultipleColumnSortingChanged)));

		/// <summary>
		/// Gets / sets if multiple column sorting will be allowed at this level.  A null value will use the setting from the <see cref="SortingSettings.AllowMultipleColumnSorting"/>.
		/// </summary>
		[TypeConverter(typeof(NullableBoolConverter))]
		public bool? AllowMultipleColumnSorting
		{
			get { return (bool?)this.GetValue(AllowMultipleColumnSortingProperty); }
			set { this.SetValue(AllowMultipleColumnSortingProperty, value); }
		}

		private static void AllowMultipleColumnSortingChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			SortingSettingsOverride settings = (SortingSettingsOverride)obj;
			settings.OnPropertyChanged("AllowMultipleColumnSorting");
		}

		#endregion // AllowMultipleColumnSorting

		#region AllowMultipleColumnSortingResolved
		/// <summary>
		/// Gets if multiple column sorting is enabled on the ColumnLayout object.  If it is not explicitly set will take the value from the <see cref="SortingSettings.AllowMultipleColumnSorting"/> value.
		/// </summary>
		public bool AllowMultipleColumnSortingResolved
		{
			get
			{
                if (this.AllowMultipleColumnSorting == null)
                {
                    if (this.SettingsObject != null)
                        return ((SortingSettings)this.SettingsObject).AllowMultipleColumnSorting;
                }
                else
                    return (bool)this.AllowMultipleColumnSorting;

                return (bool)SortingSettings.AllowMultipleColumnSortingProperty.GetMetadata(typeof(SortingSettings)).DefaultValue;
			}
		}
		#endregion //AllowMultipleColumnSortingResolved

		#region SortedColumns

		/// <summary>
		/// Gets the <see cref="SortedColumnsCollection"/> which will describe how the rows will be sorted.
		/// </summary>
		public SortedColumnsCollection SortedColumns
		{
			get
			{
				if (null == _sortedColumns)
				{
					_sortedColumns = new SortedColumnsCollection();
					_sortedColumns.CollectionChanged += new NotifyCollectionChangedEventHandler(SortedColumns_CollectionChanged);
				}
				return this._sortedColumns;
			}
		}

		#endregion // SortedColumns

		#region MultiSortingKeyResolved
		/// <summary>
		/// Gets what keyboard key will designate multiple column sorting.
		/// </summary>		
		protected internal MultiSortingKey MultiSortingKeyResolved
		{
			




			get
			{
				if (this.SettingsObject != null)
				{
					return ((SortingSettings)this.SettingsObject).MultiSortingKey;
				}
				return MultiSortingKey.Control;
			}
		}
		#endregion // MultiSortingKeyResolved

		#region FirstSortDirection

		/// <summary>
		/// Identifies the <see cref="FirstSortDirection"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty FirstSortDirectionProperty = DependencyProperty.Register("FirstSortDirection", typeof(SortDirection?), typeof(SortingSettingsOverride), new PropertyMetadata(null, new PropertyChangedCallback(FirstSortDirectionChanged)));

		/// <summary>
		/// Gets/Sets FirstSortDirection for a particular <see cref="ColumnLayout"/>
		/// </summary>
		[TypeConverter(typeof(NullableEnumTypeConverter<SortDirection>))]
		public SortDirection? FirstSortDirection
		{
			get { return (SortDirection?)this.GetValue(FirstSortDirectionProperty); }
			set { this.SetValue(FirstSortDirectionProperty, value); }
		}

		private static void FirstSortDirectionChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			SortingSettingsOverride settings = (SortingSettingsOverride)obj;
			settings.OnPropertyChanged("FirstSortDirection");
		}

		#endregion // FirstSortDirection

		#region FirstSortDirectionResolved

		/// <summary>
		/// Resolves the <see cref="SortingSettingsOverride.FirstSortDirection"/> property for a particular <see cref="ColumnLayout"/>
		/// </summary>
		public SortDirection FirstSortDirectionResolved
		{
			get
			{
                if (this.FirstSortDirection == null)
                {
                    if (this.SettingsObject != null)
                        return ((SortingSettings)this.SettingsObject).FirstSortDirection;
                }
                else
                    return (SortDirection)this.FirstSortDirection;

                return (SortDirection)SortingSettings.FirstSortDirectionProperty.GetMetadata(typeof(SortingSettings)).DefaultValue;
			}
		}

		#endregion //FirstSortDirectionResolved

		#endregion // Properties

		#region Events
		void SortedColumns_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if(this.ColumnLayout != null)
				this.ColumnLayout.InvalidateSorting();
		}
		#endregion // Events

		#region IDisposable Members

		/// <summary>
		/// Releases the unmanaged resources used by the <see cref="SortingSettingsOverride"/> and optionally
		/// releases the managed resources.
		/// </summary>
		/// <param propertyName="disposing">
		/// true to release both managed and unmanaged resources; 
		/// false to release only unmanaged resources.
		/// </param>
		protected virtual void Dispose(bool disposing)
		{
			if (this._sortedColumns != null)
				this._sortedColumns.Dispose();
		}

		/// <summary>
		/// Releases the unmanaged and managed resources used by the <see cref="SortingSettingsOverride"/>.
		/// </summary>
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
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