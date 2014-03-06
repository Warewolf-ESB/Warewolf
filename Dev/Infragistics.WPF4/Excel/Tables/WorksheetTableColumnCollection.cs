using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using System.Security;
using System.ComponentModel;
using System.Collections;


using Infragistics.Shared;


namespace Infragistics.Documents.Excel
{
	// MD 12/7/11 - 12.1 - Table Support



	/// <summary>
	/// A read-only collection of all <see cref="WorksheetTableColumn"/> instances which exist in a <see cref="WorksheetTable"/>.
	/// </summary>
	/// <seealso cref="WorksheetTable.Columns"/>
	[DebuggerDisplay("Count = {Count}")]
	[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelTables)]
	public

 class WorksheetTableColumnCollection :
		IEnumerable<WorksheetTableColumn>
	{
		#region Member Variables

		private List<WorksheetTableColumn> _columns;
		private WorksheetTable _table;

		#endregion // Member Variables

		#region Constructor

		internal WorksheetTableColumnCollection(WorksheetTable table)
		{
			_columns = new List<WorksheetTableColumn>();
			_table = table;
		}

		#endregion Constructor

		#region Interfaces

		#region IEnumerable<WorksheetTableColumn> Members

		IEnumerator<WorksheetTableColumn> IEnumerable<WorksheetTableColumn>.GetEnumerator()
		{
			return _columns.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _columns.GetEnumerator();
		}

		#endregion

		#endregion Interfaces

		#region Methods

		#region Public Methods

		#region Contains

		/// <summary>
		/// Determines whether the specified <see cref="WorksheetTableColumn"/> is in the collection.
		/// </summary>
		/// <param name="column">The column to find in the collection.</param>
		/// <returns>True if the specified column is in the collection; False otherwise.</returns>
		/// <seealso cref="WorksheetTableColumn"/>
		public bool Contains(WorksheetTableColumn column)
		{
			return _columns.Contains(column);
		}

		#endregion Contains

		#region IndexOf

		/// <summary>
		/// Gets the index of the specified <see cref="WorksheetTableColumn"/> in the collection.
		/// </summary>
		/// <param name="column">The WorksheetTableColumn to find in the collection.</param>
		/// <returns>
		/// The 0-based index of the specified WorksheetTableColumn in the collection or -1 if the item is not in the collection.
		/// </returns>
		public int IndexOf(WorksheetTableColumn column)
		{
			return _columns.IndexOf(column);
		}

		#endregion // IndexOf

		#endregion // Public Methods

		#region Internal Methods

		#region GetColumnById

		internal WorksheetTableColumn GetColumnById(uint id)
		{
			foreach (WorksheetTableColumn column in _columns)
			{
				if (column.Id == id)
					return column;
			}

			return null;
		}

		#endregion // GetColumnById

		// MD 7/19/12 - TFS116808 (Table resizing)
		#region InternalInsert

		internal void InternalInsert(int index, WorksheetTableColumn column)
		{
			_columns.Insert(index, column);
		}

		#endregion //InternalInsert

		// MD 7/19/12 - TFS116808 (Table resizing)
		#region InternalRemoveAt

		internal void InternalRemoveAt(int index)
		{
			WorksheetTableColumn column = _columns[index];
			column.OnRemovedFromTable();
			_columns.RemoveAt(index);
		}

		#endregion //InternalRemoveAt

		#endregion // Internal Methods

		#endregion Methods

		#region Properties

		#region Public Properties

		#region Count

		/// <summary>
		/// Gets the number of <see cref="WorksheetTableColumn"/> instances in the collection.
		/// </summary>
		/// <value>The number of columns in the collection.</value>
		public int Count
		{
			get { return _columns.Count; }
		}

		#endregion Count

		#region Indexer [ int ]

		/// <summary>
		/// Gets the <see cref="WorksheetTableColumn"/> at the specified index.
		/// </summary>
		/// <param name="index">The 0-based index of the column to get.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="index"/> is less than 0 or greater than or equal to the number of columns in the collection.
		/// </exception>
		/// <returns>The WorksheetTableColumn at the specified index.</returns>
		public WorksheetTableColumn this[int index]
		{
			get
			{
				if (index < 0 || this.Count <= index)
					throw new ArgumentOutOfRangeException("index", SR.GetString("LE_ArgumentOutOfRangeException_CollectionIndex"));

				return _columns[index];
			}
		}

		#endregion Indexer [ int ]

		#region Indexer [ string ]

		/// <summary>
		/// Gets the <see cref="WorksheetTableColumn"/> with the specified name or null if it doesn't exist.
		/// </summary>
		/// <param name="name">The name of the column to get.</param>
		/// <remarks>
		/// <p class="body">
		/// Column names are compared case-insensitively.
		/// </p>
		/// </remarks>
		/// <returns>The WorksheetTableColumn with the specified name or null a column with the specified name doesn't exist.</returns>
		/// <seealso cref="WorksheetTableColumn.Name"/>
		public WorksheetTableColumn this[string name]
		{
			get
			{
				// MD 4/9/12 - TFS101506
				CultureInfo culture = _table.Culture;

				foreach (WorksheetTableColumn column in _columns)
				{
					// MD 4/9/12 - TFS101506
					//if (String.Compare(column.Name, name, StringComparison.CurrentCultureIgnoreCase) == 0)
					if (String.Compare(column.Name, name, culture, CompareOptions.IgnoreCase) == 0)
						return column;
				}

				return null;
			}
		}

		#endregion Indexer [ string ]

		#endregion Public Properties

		#endregion Properties
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