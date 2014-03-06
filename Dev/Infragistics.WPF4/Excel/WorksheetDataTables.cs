using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;




using Infragistics.Shared;


namespace Infragistics.Documents.Excel
{
	/// <summary>
	/// A collection of <see cref="WorksheetDataTable"/> instances on a worksheet.
	/// </summary>
	[DebuggerDisplay( "Count = {Count}" )]



	public

		 sealed class WorksheetDataTableCollection : 
		ICollection<WorksheetDataTable>
	{
		#region Member Variables

		private List<WorksheetDataTable> dataTables;
		private Worksheet worksheet;

		#endregion Member Variables

		#region Constructor

		internal WorksheetDataTableCollection( Worksheet worksheet )
		{
			this.worksheet = worksheet;
			this.dataTables = new List<WorksheetDataTable>();
		}

		#endregion Constructor

		#region Interfaces

		#region ICollection<WorksheetDataTable> Members

		void ICollection<WorksheetDataTable>.Add( WorksheetDataTable item )
		{
			throw new InvalidOperationException( SR.GetString( "LE_InvalidOperationException_CantAddDataTable" ) );
		}

		void ICollection<WorksheetDataTable>.Clear()
		{
			this.Clear();
		}

		bool ICollection<WorksheetDataTable>.Contains( WorksheetDataTable item )
		{
			return this.dataTables.Contains( item );
		}

		void ICollection<WorksheetDataTable>.CopyTo( WorksheetDataTable[] array, int arrayIndex )
		{
			this.dataTables.CopyTo( array, arrayIndex );
		}

		int ICollection<WorksheetDataTable>.Count
		{
			get { return this.Count; }
		}

		bool ICollection<WorksheetDataTable>.IsReadOnly
		{
			get { return false; }
		}

		bool ICollection<WorksheetDataTable>.Remove( WorksheetDataTable item )
		{
			return this.Remove( item );
		}

		#endregion

		#region IEnumerable<WorksheetDataTable> Members

		IEnumerator<WorksheetDataTable> IEnumerable<WorksheetDataTable>.GetEnumerator()
		{
			return this.dataTables.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.dataTables.GetEnumerator();
		}

		#endregion

		#endregion Interfaces

		#region Methods

		#region Add

		/// <summary>
		/// Creates a new data table and adds it to the collection.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The input cells specified must be different cell references and at least one must be non-null.
		/// See the <see cref="WorksheetDataTable"/> overview for more information on data tables.
		/// </p>
		/// </remarks>
		/// <param name="cellsInTable">The region of cells in the data table.</param>
		/// <param name="columnInputCell">The cell used as the column-input cell in the data table.</param>
		/// <param name="rowInputCell">The cell used as the row-input cell in the data table.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="cellsInTable"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="cellsInTable"/> is a region which does not belongs to the worksheet which owns this collection.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="columnInputCell"/> is not null but does not belong to the worksheet which owns this collection.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="rowInputCell"/> is not null but does not belong to the worksheet which owns this collection.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// Both <paramref name="columnInputCell"/> and <paramref name="rowInputCell"/> are null.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// <paramref name="columnInputCell"/> and <paramref name="rowInputCell"/> are the same cell.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// <paramref name="columnInputCell"/> or <paramref name="rowInputCell"/> are contained in the 
		/// <paramref name="cellsInTable"/> region.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// One or more of the interior cells of the <paramref name="cellsInTable"/> region (all cells except the left-most column 
		/// and top row) is an interior cell of another data table or is a cell in an array formula, and the entire 
		/// range of that other entity extends outside the interior cells of <paramref name="cellsInTable"/>.
		/// </exception>
		/// <returns>The newly created data table.</returns>
		public WorksheetDataTable Add( WorksheetRegion cellsInTable, WorksheetCell columnInputCell, WorksheetCell rowInputCell )
		{
			WorksheetDataTable.VerifyTableElements( 
				this.worksheet, 
				cellsInTable, "cellsInTable", 
				columnInputCell, "columnInputCell", 
				rowInputCell, "rowInputCell" );

			WorksheetDataTable dataTable = new WorksheetDataTable( this.worksheet, cellsInTable, columnInputCell, rowInputCell );
			this.dataTables.Add( dataTable );
			return dataTable;
		}

		#endregion Add

		#region Clear

		/// <summary>
		/// Clears all data tables from the collection.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// Once a data table is removed from the collection, it can no longer be used.
		/// </p>
		/// </remarks>
		public void Clear()
		{
			for ( int i = this.dataTables.Count - 1; i >= 0; i-- )
				this.RemoveAt( i );
		}

		#endregion Clear

		#region Remove

		/// <summary>
		/// Removes the specified data table from the collection.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// Once a data table is removed from the collection, it can no longer be used.
		/// </p>
		/// </remarks>
		/// <param name="dataTable">The data table to remove from the collection.</param>
        /// <returns>
        /// True if the dataTable was successfully removed; False if the dataTable was not in the collection.
        /// </returns>
		public bool Remove( WorksheetDataTable dataTable )
		{
			int index = this.dataTables.IndexOf( dataTable );

			if ( index < 0 )
				return false;

			this.RemoveAt( index );
			return true;
		}

		#endregion Remove

		#region RemoveAt

		/// <summary>
		/// Removes the data table at the specified index from the collection.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// Once a data table is removed from the collection, it can no longer be used.
		/// </p>
		/// </remarks>
		/// <param name="index">The zero-based index of the data table to remove from the collection.</param>
		public void RemoveAt( int index )
		{
			if ( index < 0 || this.Count <= index )
				throw new ArgumentOutOfRangeException( "index", index, SR.GetString( "LE_ArgumentOutOfRangeException_CollectionIndex" ) );

			WorksheetDataTable dataTable = this.dataTables[ index ];
			this.dataTables.RemoveAt( index );
			dataTable.OnRemovedFromCollection();
		}

		#endregion RemoveAt

		#endregion Methods

		#region Properties

		#region Count

		/// <summary>
		/// Gets the number of data tables in the collection.
		/// </summary>
		/// <value>The number of data tables in the collection.</value>
		public int Count
		{
			get { return this.dataTables.Count; }
		}

		#endregion Count

		#region Indexer [ int ]

		/// <summary>
		/// Gets the data table at the specified index in the collection.
		/// </summary>
		/// <param name="index">The zero-based index of the data table to get.</param>
		/// <value>The data table at the specified index.</value>
		public WorksheetDataTable this[ int index ]
		{
			get 
			{
				if ( index < 0 || this.Count <= index )
					throw new ArgumentOutOfRangeException( "index", index, SR.GetString( "LE_ArgumentOutOfRangeException_CollectionIndex" ) );

				return this.dataTables[ index ]; 
			}
		}

		#endregion Indexer [ int ]

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