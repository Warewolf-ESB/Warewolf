using System;
using System.Net;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Infragistics.AutomationPeers;
using System.ComponentModel;

namespace Infragistics.Controls.Grids
{
	/// <summary>
	/// An object that contains settings for selection on the <see cref="XamGrid"/>
	/// </summary>
	public class SelectionSettings : SettingsBase, IDisposable, IProvidePropertyPersistenceSettings
	{
		#region Members

		SelectedRowsCollection _selectedRows;
		SelectedCellsCollection _selectedCells;
		SelectedColumnsCollection _selectedColumns;
		List<string> _propertiesThatShouldntBePersisted;

		#endregion // Members

		#region Properties

		#region Public

		#region CellSelection

		/// <summary>
		/// Identifies the <see cref="CellSelection"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty CellSelectionProperty = DependencyProperty.Register("CellSelection", typeof(SelectionType), typeof(SelectionSettings), new PropertyMetadata(SelectionType.Single, new PropertyChangedCallback(CellSelectionChanged)));

		/// <summary>
		/// Gets/Sets how Cell selection will work on the <see cref="XamGrid"/>
		/// </summary>
		public SelectionType CellSelection
		{
			get { return (SelectionType)this.GetValue(CellSelectionProperty); }
			set { this.SetValue(CellSelectionProperty, value); }
		}

		private static void CellSelectionChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			SelectionSettings settings = (SelectionSettings)obj;
			settings.OnPropertyChanged("CellSelection");

			if (settings.Grid != null && settings.CellSelection == SelectionType.None)
				settings.Grid.SelectionSettings.SelectedCells.Clear();

		}

		#endregion // CellSelection

		#region RowSelection

		/// <summary>
		/// Identifies the <see cref="RowSelection"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty RowSelectionProperty = DependencyProperty.Register("RowSelection", typeof(SelectionType), typeof(SelectionSettings), new PropertyMetadata(SelectionType.Single, new PropertyChangedCallback(RowSelectionChanged)));

		/// <summary>
		/// Gets/Sets how Row selection will work on the <see cref="XamGrid"/>
		/// </summary>
		public SelectionType RowSelection
		{
			get { return (SelectionType)this.GetValue(RowSelectionProperty); }
			set { this.SetValue(RowSelectionProperty, value); }
		}

		private static void RowSelectionChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			SelectionSettings settings = (SelectionSettings)obj;
			settings.OnPropertyChanged("RowSelection");

			if (settings.Grid != null && settings.RowSelection == SelectionType.None)
				settings.Grid.SelectionSettings.SelectedRows.Clear();

			// Raise a PropertyChanged event
			settings.OnPropertyChanged("RowSelection");

		}

		#endregion // RowSelection

		#region ColumnSelection

		/// <summary>
		/// Identifies the <see cref="ColumnSelection"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty ColumnSelectionProperty = DependencyProperty.Register("ColumnSelection", typeof(SelectionType), typeof(SelectionSettings), new PropertyMetadata(SelectionType.None, new PropertyChangedCallback(ColumnSelectionChanged)));

		/// <summary>
		/// Gets/Sets how Column selection will work on the <see cref="XamGrid"/>
		/// </summary>
		public SelectionType ColumnSelection
		{
			get { return (SelectionType)this.GetValue(ColumnSelectionProperty); }
			set { this.SetValue(ColumnSelectionProperty, value); }
		}

		private static void ColumnSelectionChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			SelectionSettings settings = (SelectionSettings)obj;
			settings.OnPropertyChanged("ColumnSelection");

			if (settings.Grid != null && settings.ColumnSelection == SelectionType.None)
				settings.Grid.SelectionSettings.SelectedColumns.Clear();
		}

		#endregion // ColumnSelection

		#region CellClickAction

		/// <summary>
		/// Identifies the <see cref="CellClickAction"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty CellClickActionProperty = DependencyProperty.Register("CellClickAction", typeof(CellSelectionAction), typeof(SelectionSettings), new PropertyMetadata(CellSelectionAction.SelectCell, new PropertyChangedCallback(CellSelectionActionChanged)));

		/// <summary>
		/// Gets/Sets whether a row or cell should be selected when a Click occurs on the <see cref="XamGrid"/>
		/// </summary>
		public CellSelectionAction CellClickAction
		{
			get { return (CellSelectionAction)this.GetValue(CellClickActionProperty); }
			set { this.SetValue(CellClickActionProperty, value); }
		}

		private static void CellSelectionActionChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			SelectionSettings settings = (SelectionSettings)obj;
			settings.OnPropertyChanged("CellClickAction");
		}

		#endregion // CellClickAction

		#region SelectedRows

		/// <summary>
		/// Gets the collection of currently selected <see cref="Row"/> objects.
		/// </summary>
        [Browsable(false)]
		public SelectedRowsCollection SelectedRows
		{
			get
			{
				if (this._selectedRows == null)
				{
					this._selectedRows = new SelectedRowsCollection();
					this._selectedRows.Grid = this.Grid;
				}

				return this._selectedRows;
			}
		}

		#endregion // SelectedRows

		#region SelectedCells

		/// <summary>
		/// Gets the collection of currently selected <see cref="Cell"/> objects.
		/// </summary>
        [Browsable(false)]
		public SelectedCellsCollection SelectedCells
		{
			get
			{
				if (this._selectedCells == null)
				{
					this._selectedCells = new SelectedCellsCollection();
					this._selectedCells.Grid = this.Grid;
				}

				return this._selectedCells;
			}
		}

		#endregion // SelectedCells

		#region SelectedColumns

		/// <summary>
		/// Gets the collection of currently selected <see cref="Column"/> objects.
		/// </summary>
        [Browsable(false)]
		public SelectedColumnsCollection SelectedColumns
		{
			get
			{
				if (this._selectedColumns == null)
				{
					this._selectedColumns = new SelectedColumnsCollection();
					this._selectedColumns.Grid = this.Grid;
				}

				return this._selectedColumns;
			}
		}

		#endregion // SelectedColumns

		#endregion // Public

		#region Protected

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
						"SelectedRows",
						"SelectedCells"
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
			get { return null; }
		}

		#endregion // PriorityProperties

		#endregion // Protected

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

		#region IDisposable Members

		/// <summary>
		/// Releases the unmanaged resources used by the <see cref="SelectionSettings"/> and optionally
		/// releases the managed resources.
		/// </summary>
		/// <param propertyName="disposing">
		/// true to release both managed and unmanaged resources; 
		/// false to release only unmanaged resources.
		/// </param>
		protected virtual void Dispose(bool disposing)
		{
			if (this._selectedCells != null)
				this._selectedCells.Dispose();
			if (this._selectedRows != null)
				this._selectedRows.Dispose();
			if (this._selectedColumns != null)
				this._selectedColumns.Dispose();
		}

		/// <summary>
		/// Releases the unmanaged and managed resources used by the <see cref="SelectionSettings"/>.
		/// </summary>
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion

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