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
using System.Reflection;

namespace Infragistics.Controls.Grids.Primitives
{
	/// <summary>
	/// A <see cref="RowBase"/> which acts as a top level row for the <see cref="XamGrid"/> to add rows.
	/// </summary>
	public class AddNewRow : Row
	{
		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="AddNewRow"/> class.
		/// </summary>
		/// <param propertyName="manager">The <see cref="RowsManager"/> that owns the <see cref="AddNewRow"/>.</param>
		protected internal AddNewRow(RowsManager manager)
			: base(-1, manager, null)
		{
			this.FixedPositionSortOrder = 2;
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
			get { return RowType.AddNewRow; }
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
					return this.ColumnLayout.AddNewRowSettings.AddNewRowHeightResolved;
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
				return EditingType.Row;
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
				return true;
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

		/// <summary>
		/// Sets the input object to the <see cref="RowBase.Data"/> value.
		/// </summary>
		/// <param propertyName="data"></param>
		protected internal void SetData(object data)
		{
			this.Data = data;
		}

		#endregion // Methods

		#region Properties
		#region Protected
		#region IsRowDirty
		/// <summary>
		/// Gets if the <see cref="AddNewRow"/> contains changes to it's default values.
		/// </summary>
		protected internal bool IsRowDirty
		{
			get
			{
				if (this.Data != null)
				{
					object defaultObject = this.ColumnLayout.DefaultDataObject;
					if (defaultObject == null)
					{
						defaultObject = ((RowsManager)this.Manager).GenerateNewObject(RowType.DataRow);
						this.ColumnLayout.DefaultDataObject = defaultObject;
					}					
					if (defaultObject != null && this.Data != null)
					{
						foreach (CellBase cb in this.VisibleCells)
						{
							Cell c = cb as Cell;
							if (c != null && c.IsEditable)
							{
								object value = DataManagerBase.ResolveValueFromPropertyPath(c.Column.Key, this.Data);
                                object valueDefault = DataManagerBase.ResolveValueFromPropertyPath(c.Column.Key, defaultObject);

								if (value == valueDefault)
									continue;

								if (c.Column.DataType == typeof(string))
								{
									if (string.IsNullOrEmpty((string)value) && string.IsNullOrEmpty((string)valueDefault))
									{
										continue;
									}
								}

								if (value == null && valueDefault != null)
									return true;

								if (value != null && valueDefault == null)
									return true;

								bool isEqual = value.Equals(valueDefault);

								if (!isEqual)
									return true;
							}
						}
					}
				}
				return false;
			}
		}
		#endregion // IsRowDirty

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