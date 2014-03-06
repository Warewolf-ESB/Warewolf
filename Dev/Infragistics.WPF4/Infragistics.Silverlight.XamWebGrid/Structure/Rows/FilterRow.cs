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
using System.Windows.Data;

namespace Infragistics.Controls.Grids.Primitives
{
	/// <summary>
	/// An object that represents a filter row in the <see cref="XamGrid"/>.
	/// </summary>
	public class FilterRow : Row
	{
		#region static

		#region ChangeType

		/// <summary>
		/// Determines if the given type is a nullable value type.
		/// </summary>
		/// <param name="dataType"></param>
		/// <returns></returns>
		protected internal static bool IsNullableValueType(Type dataType)
		{
			if (dataType != null)
			{
				if (dataType.IsGenericType && dataType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
				{
					Type newType = Nullable.GetUnderlyingType(dataType);

					return newType.IsValueType;

				}
			}
			return false;
		}

		private static bool IsNullableVersionOfType(Type sourceType, Type comparisonType)
		{
			if (sourceType != null && comparisonType != null)
			{
				if (sourceType.IsGenericType && sourceType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
				{
					Type newType = Nullable.GetUnderlyingType(sourceType);

					return newType == comparisonType;
				}
			}
			return false;
		}


		/// <summary>
		/// Converts a value to the desired type, filtering out nullable types to overcome known MS limitation
		/// </summary>
		/// <param name="value"></param>
		/// <param name="dataType"></param>
		/// <returns></returns>
		protected internal static object ChangeType(object value, Type dataType)
		{
			if (dataType.IsGenericType && dataType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
			{
				if (value == null)
					return null;

				dataType = Nullable.GetUnderlyingType(dataType);
			}

			if (dataType.IsEnum && value != null)
			{
				return Enum.Parse(dataType, value.ToString(), true);
			}

			return Convert.ChangeType(value, dataType, CultureInfo.CurrentCulture);
		}

		#endregion // ChangeType

		#endregion // static

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="FilterRow"/> class.
		/// </summary>
		/// <param propertyName="manager">The <see cref="RowsManager"/> that owns the <see cref="FilterRow"/>.</param>
		protected internal FilterRow(RowsManager manager)
			: base(-1, manager, null)
		{
			this.FixedPositionSortOrder = 3;
		}

		#endregion // Constructor

		#region Overrides

		#region Public

		#region RowType

		/// <summary>
		/// Gets the <see cref="RowType"/> of this <see cref="RowBase"/>
		/// </summary>
		public override RowType RowType
		{
			get { return RowType.FilterRow; }
		}

		#endregion // RowType

		#region HeightResolved

		/// <summary>
		/// Resolves the <see cref="RowBase.Height"/> property for this Row.
		/// </summary>
		public override RowHeight HeightResolved
		{
			get
			{
				if (this.Height != null)
					return (RowHeight)this.Height;
				else
					return this.ColumnLayout.FilteringSettings.FilterRowHeightResolved;
			}
		}
		#endregion // HeightResolved

		#region HasChildren
		/// <summary>
		/// Gets whether or not <see cref="ExpandableRowBase"/> has any child rows.
		/// </summary>
		public override bool HasChildren
		{
			get
			{
				return false;
			}
		}
		#endregion // HasChildren

		#endregion // Public

		#region Protected

		#region AllowEditing

		/// <summary>
		/// Gets if the <see cref="RowBase"/> object should allow editing.
		/// </summary>
		protected internal override EditingType AllowEditing
		{
			get
			{
				return EditingType.Cell;
			}
		}

		#endregion // AllowEditing

		#region AllowSelection
		/// <summary>
		/// Gets whether selection will be allowed on the <see cref="RowBase"/>.
		/// </summary>
		protected internal override bool AllowSelection
		{
			get
			{
				return false;
			}
		}
		#endregion // AllowSelection

		#region AllowKeyboardNavigation
		/// <summary>
		/// Gets whether the <see cref="RowBase"/> will allow keyboard navigation.
		/// </summary>
		protected internal override bool AllowKeyboardNavigation
		{
			get
			{
				return true;
			}
		}
		#endregion // AllowKeyboardNavigation

		#region IsStandAloneRow
		/// <summary>
		/// Gets whether this <see cref="Row"/> can stand alone, when there are no other data rows.
		/// </summary>
		protected internal override bool IsStandAloneRow
		{
			get
			{
				return true;
			}
		}
		#endregion // IsStandAloneRow

		#region IsStandAloneRowResolved
		/// <summary>
		/// Used primarily by special rows, determimes if the stand alone row will force the showing of the child row island. 
		/// </summary>
		protected internal override bool IsStandAloneRowResolved
		{
			get
			{
				RowsManager rm = (RowsManager)this.Manager;
				if (rm.DataManager != null)
				{
					return rm.DataManager.TotalRecordCount > 0;
				}
				return this.IsStandAloneRow;
			}
		}
		#endregion //IsStandAloneRowResolved


		#region RequiresFixedRowSeparator
		/// <summary>
		/// Used to determine if a FixedRow separator is neccessary for this <see cref="RowBase"/>
		/// </summary>
		protected internal override bool RequiresFixedRowSeparator
		{
			get
			{
				return true;
			}
		}
		#endregion //RequiresFixedRowSeparator

		#endregion // Protected

		#endregion // Overrides

		#region Methods

		#region Protected

		#region SetData
		/// <summary>
		/// Sets the input object to the <see cref="RowBase.Data"/> value.
		/// </summary>
		/// <param propertyName="data"></param>
		protected internal void SetData(object data)
		{
			this.Data = data;
		}
		#endregion // SetData

		#region CellEditorValueChanged
		
		/// <summary>
		/// Called while the cell in a row is edited. 		
		/// </summary>
		/// <remarks>
		/// Designed to be used by rows who need to do actions while the editor control is being edited.
		/// </remarks>
		/// <param name="cellBase"></param>
		/// <param name="newValue"></param>
        protected internal override void CellEditorValueChanged(CellBase cellBase, object newValue)
        {
            Column col = cellBase.Column;

		    if (col.AllowCellEditorValueChangedFiltering)
            {
                BuildFilters(cellBase, newValue, true);
                this.RaiseFilteredEvent();
            }
        }

		#endregion // CellEditorValueChanged

		#region BuildFilters

		/// <summary>
		/// For a single column, based of the cell, clears any existing filtering conditions and creates a new condition based on current information.
		/// </summary>
		/// <param propertyName="cellBase"></param>
		/// <param propertyName="newValue"></param>
		protected internal bool BuildFilters(CellBase cellBase, object newValue, bool fireEvents)
		{
			RowsManager rm = ((RowsManager)cellBase.Row.Manager);
			FilterRowCell frc = (FilterRowCell)cellBase;
			return this.ColumnLayout.BuildFilters(rm.RowFiltersCollectionResolved, newValue, frc.Column, frc.FilteringOperandResolved, false, fireEvents);
		}

		#endregion // BuildFilters

		#region RemoveFilters
		/// <summary>
		/// Removes all filter conditions in a column based on the cell.
		/// </summary>
		/// <param propertyName="cellBase"></param>
		protected internal void RemoveFilters(FilterRowCell cellBase)
		{
			cellBase.Row.ColumnLayout.Grid.ExitEditMode(false);

			Column column = cellBase.Column;
			FilterRowCell frc = (FilterRowCell)cellBase;

			frc.SilentUpdate = true;
			frc.Column.FilterColumnSettings.SilentUpdate = true;
            object resetValue = null;
			//Binding b = cellBase.Control.ContentProvider.ResolveBinding(frc);
			//if (b != null)
			{                
                if (column.DataType == null)
                {
                    resetValue = null;
                }
                else if (typeof(bool) == column.DataType || FilterRow.IsNullableVersionOfType(column.DataType, typeof(bool)))
                {
                    resetValue = null;
                }
                else if (IsNullableValueType(column.DataType))
                {
                    resetValue = null;
                }
                else if (column.DataType.IsValueType)
                {

                    resetValue = null;



                }
                else if (column.DataType == typeof(string))
                {
                    resetValue = "";
                }
                else
                {
                    resetValue = null;
                }

                //frc.ExitEditMode(resetValue, false, true);
                //b.UpdateSourceTrigger = UpdateSourceTrigger.Default;
                //Infragistics.Controls.Grids.Cell.CellValueObject cvo = new Cell.CellValueObject();
                //if(b.Source == null)
                //    b.Source = this.Data;
                //cvo.SetBinding(Cell.CellValueObject.ValueProperty, b);
			}

			//if (cellBase.Column is TemplateColumn ||  cellBase.Column is UnboundColumn)
			{
				frc.FilterCellValue = resetValue;
                frc.Column.FilterColumnSettings.FilterCellValue = resetValue;				
			}
			frc.SilentUpdate = false;
			frc.Column.FilterColumnSettings.SilentUpdate = false;

            RowsManager rm = (RowsManager)this.Manager;
            RowsFilter rf = rm.RowFiltersCollectionResolved[cellBase.Column.Key];
            if (rf != null)
            {
                rf.Conditions.ClearSilently();
            }

			this.ColumnLayout.InvalidateFiltering();
		}
		#endregion // RemoveFilters

        #region RaiseFilteredEvent
        internal void RaiseFilteredEvent()
        {
            RowsManager rm = ((RowsManager)this.Manager);
            this.ColumnLayout.Grid.OnFiltered(rm.RowFiltersCollectionResolved);
        }
        #endregion // RaiseFilteredEvent

        #endregion // Protected

        #endregion // Methods

        #region Properties

        #region Protected

        #endregion // Protected

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