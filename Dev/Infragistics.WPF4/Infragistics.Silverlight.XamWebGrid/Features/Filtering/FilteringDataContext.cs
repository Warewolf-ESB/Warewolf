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
using Infragistics.Controls.Grids;
using Infragistics.Controls.Grids.Primitives;
using System.ComponentModel;

namespace Infragistics
{
	/// <summary>
	/// An object that will be assigned to a <see cref="TemplateColumn"/>'s editor data context property for filtering.
	/// </summary>
	public class FilteringDataContext : INotifyPropertyChanged
	{
		#region Members
		FilterRowCell _cell;
		object _filterCellValue;
		#endregion // Members

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the <see cref="FilteringDataContext"/> class.
		/// </summary>
		/// <param name="frc"></param>
		public FilteringDataContext(FilterRowCell frc)
		{
			this.Cell = frc;
			this.RowData = frc.Row.Data;
			this._filterCellValue = frc.FilterCellValueResolved;
		}
		#endregion // Constructor

		#region Properties

		#region Internal

		#region Cell
		/// <summary>
		/// Gets / sets the <see cref="FilterRowCell"/> that is associated with this context
		/// </summary>
		internal FilterRowCell Cell
		{
			get
			{
				return this._cell;
			}
			set
			{
				if (this._cell != value)
				{
					if (this._cell != null)
						this._cell.PropertyChanged -= Cell_PropertyChanged;

					this._cell = value;

					if (this._cell != null)
						this._cell.PropertyChanged += Cell_PropertyChanged;
				}
			}
		}
		#endregion // Cell

		#endregion // Internal

		#region Value
		/// <summary>
		/// Gets / sets the resolved FilterCellValue for this binding.
		/// </summary>
		public object Value
		{
			get
			{
				return this._filterCellValue;
			}
			set
			{
				if (this._filterCellValue != value)
				{
					this._filterCellValue = value;
                    this.OnValueChanged();
				}
			}
		}

		#endregion // Value

		#region ColumnKey

		/// <summary>
		/// Gets the key of the column that that this DataContext is associated with.
		/// </summary>
		public string ColumnKey
		{
			get
			{
				return this._cell.Column.Key;
			}
		}

		#endregion // ColumnKey

		#region RowData
		/// <summary>
		/// Gets the row data that is associated with this object.
		/// </summary>
		public object RowData
		{
			get;
			protected set;
		}
		#endregion // RowData

		#endregion // Properties

		#region EventHandlers

		void Cell_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "FilterCellValueResolved")
			{
				this._filterCellValue = this.Cell.FilterCellValueResolved;
				this.OnPropertyChanged("Value");
			}
		}

		#endregion // EventHandlers

		#region INotifyPropertyChanged Members

		/// <summary>
		/// Fired when a property changes on the <see cref="FilteringDataContext"/>.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Invoked when a property changes on the <see cref="FilteringDataContext"/> object.
		/// </summary>
		/// <param propertyName="propertyName">The propertyName of the property that has changed.</param>
		protected virtual void OnPropertyChanged(string name)
		{
			if (this.PropertyChanged != null)
				this.PropertyChanged(this, new PropertyChangedEventArgs(name));
		}

		#endregion

        #region Methods

        #region Protected

        #region OnValueChanged
        
        /// <summary>
        /// Method called when the value is modified so that the underlying object can be updated as well.
        /// </summary>
        protected virtual void OnValueChanged()
        {
            if (this.Cell.Row.ColumnLayout.FilteringSettings.FilteringScopeResolved == FilteringScope.ChildBand)
            {
                this.Cell.FilterCellValue = this._filterCellValue;
            }
            else
            {
                this.Cell.Column.FilterColumnSettings.FilterCellValue = this._filterCellValue;
            }
        }

        #endregion // OnValueChanged

        #region SetValueSilent

        /// <summary>
        /// Sets the Value property without raising the ValueChanged.
        /// </summary>
        /// <param name="value"></param>
        protected void SetValueSilent(object value)
        {
            this._filterCellValue = value;
        }

        #endregion // SetValueSilent

        #endregion // Protected
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