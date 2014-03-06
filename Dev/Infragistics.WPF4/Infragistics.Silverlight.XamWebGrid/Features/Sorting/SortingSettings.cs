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
using System.ComponentModel;

namespace Infragistics.Controls.Grids
{
	/// <summary>
	/// An object that contains settings for using SortingSettings on the <see cref="XamGrid"/>
	/// </summary>
	public class SortingSettings : SettingsBase, IProvidePropertyPersistenceSettings
	{
		#region Members

		List<string> _propertiesThatShouldntBePersisted;

		#endregion // Members

		#region Properties

		#region AllowSorting

		/// <summary>
		/// Identifies the <see cref="AllowSorting"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty AllowSortingProperty = DependencyProperty.Register("AllowSorting", typeof(bool), typeof(SortingSettings), new PropertyMetadata(true, new PropertyChangedCallback(AllowSortingChanged)));

		/// <summary>
		/// Gets / sets if sorting will be allowed by default.
		/// </summary>
		public bool AllowSorting
		{
			get { return (bool)this.GetValue(AllowSortingProperty); }
			set { this.SetValue(AllowSortingProperty, value); }
		}

		private static void AllowSortingChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			SortingSettings settings = (SortingSettings)obj;
			settings.OnPropertyChanged("AllowSorting");
		}

		#endregion // AllowSorting

		#region ShowSortIndicator

		/// <summary>
		/// Identifies the <see cref="ShowSortIndicator"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty ShowSortIndicatorProperty = DependencyProperty.Register("ShowSortIndicator", typeof(bool), typeof(SortingSettings), new PropertyMetadata(true, new PropertyChangedCallback(ShowSortIndicatorChanged)));

		/// <summary>
		/// Gets / sets if the sort indicator will be visible by default.
		/// </summary>
		public bool ShowSortIndicator
		{
			get { return (bool)this.GetValue(ShowSortIndicatorProperty); }
			set { this.SetValue(ShowSortIndicatorProperty, value); }
		}

		private static void ShowSortIndicatorChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			SortingSettings settings = (SortingSettings)obj;
			settings.OnPropertyChanged("ShowSortIndicator");
		}

		#endregion // ShowSortIndicator

		#region AllowMultipleColumnSorting

		/// <summary>
		/// Identifies the <see cref="AllowMultipleColumnSorting"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty AllowMultipleColumnSortingProperty = DependencyProperty.Register("AllowMultipleColumnSorting", typeof(bool), typeof(SortingSettings), new PropertyMetadata(true, new PropertyChangedCallback(AllowMultipleColumnSortingChanged)));

		/// <summary>
		/// Gets / sets if sorting will be allowed on multiple columns by default.
		/// </summary>
		public bool AllowMultipleColumnSorting
		{
			get { return (bool)this.GetValue(AllowMultipleColumnSortingProperty); }
			set { this.SetValue(AllowMultipleColumnSortingProperty, value); }
		}

		private static void AllowMultipleColumnSortingChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			SortingSettings settings = (SortingSettings)obj;
			settings.OnPropertyChanged("AllowMultipleColumnSorting");
		}

		#endregion // AllowMultipleColumnSorting

		#region SortedColumns
		/// <summary>
		/// Gets the <see cref="SortedColumnsCollection"/> which will describe how the rows will be sorted.
		/// </summary>
        [Browsable(false)]
		public SortedColumnsCollection SortedColumns
		{
			get
			{
				SortedColumnsCollection collection = null;

				if (this.Grid != null)
					collection = this.Grid.RowsManager.ColumnLayout.SortingSettings.SortedColumns;

				return collection;
			}
		}
		#endregion // SortedColumns

		#region MulitSortingKey

		/// <summary>
		/// Identifies the <see cref="MultiSortingKey"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty MultiSortingKeyProperty = DependencyProperty.Register("MultiSortingKey", typeof(MultiSortingKey), typeof(SortingSettings), new PropertyMetadata(MultiSortingKey.Control, new PropertyChangedCallback(MultiSortingKeyChanged)));

		/// <summary>
		/// Gets / sets which keyboard key will be used to designate multiple column sorting.
		/// </summary>
		public MultiSortingKey MultiSortingKey
		{
			get { return (MultiSortingKey)this.GetValue(MultiSortingKeyProperty); }
			set { this.SetValue(MultiSortingKeyProperty, value); }
		}

		private static void MultiSortingKeyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			SortingSettings settings = (SortingSettings)obj;
			settings.OnPropertyChanged("MultiSortingKey");
		}

		#endregion // MulitSortingKey

		#region FirstSortDirection

		/// <summary>
		/// Identifies the <see cref="FirstSortDirection"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty FirstSortDirectionProperty = DependencyProperty.Register("FirstSortDirection", typeof(SortDirection), typeof(SortingSettings), new PropertyMetadata(SortDirection.Ascending, new PropertyChangedCallback(FirstSortDirectionChanged)));

		/// <summary>
		/// Gets/Sets FirstSortDirection for all <see cref="ColumnLayout"/> objects in the <see cref="XamGrid"/>
		/// </summary>
		public SortDirection FirstSortDirection
		{
			get { return (SortDirection)this.GetValue(FirstSortDirectionProperty); }
			set { this.SetValue(FirstSortDirectionProperty, value); }
		}

		private static void FirstSortDirectionChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			SortingSettings settings = (SortingSettings)obj;
			settings.OnPropertyChanged("FirstSortDirection");
		}

		#endregion // FirstSortDirection

		#region PropertiesToIgnore

		/// <summary>
		/// Gets a List of properties that shouldn't be saved when the PersistenceManager goes to save them.
		/// </summary>
		protected virtual List<string> PropertiesToIgnore
		{
			get
			{
				if (this._propertiesThatShouldntBePersisted == null)
				{
					this._propertiesThatShouldntBePersisted = new List<string>()
					{
						"SortedColumns"
					};
				}

				return this._propertiesThatShouldntBePersisted;
			}
		}

		#endregion // PropertiesToIgnore

		#region PriorityProperties

		/// <summary>
		/// Gets a List of properties that should be applied, before even trying to look at any other property on the object.
		/// </summary>
		protected virtual List<string> PriorityProperties
		{
			get
			{
				return null;
			}
		}

		#endregion // PriorityProperties

		#endregion // Properties

		#region Methods

		#region FinishedLoadingPersistence

		/// <summary>
		/// Allows an object to perform an operation, after it's been loaded.
		/// </summary>
		protected virtual void FinishedLoadingPersistence()
		{
		}

		#endregion // FinishedLoadingPersistence

		#endregion // Methods

		#region IProvidePropertyPersistenceSettings Members

		List<string> IProvidePropertyPersistenceSettings.PropertiesToIgnore
		{
			get
			{
				return this.PropertiesToIgnore;
			}
		}

		List<string> IProvidePropertyPersistenceSettings.PriorityProperties
		{
			get { return this.PriorityProperties; }
		}

		void IProvidePropertyPersistenceSettings.FinishedLoadingPersistence()
		{
			this.FinishedLoadingPersistence();
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