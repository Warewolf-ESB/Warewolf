using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Globalization;




using Infragistics.Shared;


namespace Infragistics.Documents.Excel
{
	/// <summary>
	/// A collection of hidden <see cref="WorksheetRow"/> instances.
	/// </summary>
	/// <remarks>
	/// <p class="body">
	/// The rows in this collection do not necessarily have their <see cref="RowColumnBase.Hidden"/> property
	/// set to True. This collection applies to a <see cref="Worksheet"/>, but belongs to a <see cref="CustomView"/>.
	/// When the CustomView which owns this collection is applied, the rows in this collection will have their
	/// <see cref="RowColumnBase.Hidden"/> property set to True. All other rows in the associated worksheet will
	/// be made visible.
	/// </p>
	/// </remarks>
	/// <seealso cref="CustomView.GetHiddenRows"/>
	[DebuggerDisplay( "Count = {Count}" )]



	public

		 sealed class HiddenRowCollection : 
		ICollection<WorksheetRow>
	{
		#region Member Variables

		private Worksheet worksheet;
		private CustomView customView;

		private List<int> rowIndices;

		#endregion Member Variables

		#region Constructor

		internal HiddenRowCollection( Worksheet worksheet, CustomView customView )
		{
			this.worksheet = worksheet;
			this.customView = customView;

			this.rowIndices = new List<int>();
		}

		#endregion Constructor

		#region Interfaces

		#region ICollection<WorksheetRow> Members

		void ICollection<WorksheetRow>.Add( WorksheetRow item )
		{
			this.Add( item );
		}

		void ICollection<WorksheetRow>.Clear()
		{
			this.Clear();
		}

		bool ICollection<WorksheetRow>.Contains( WorksheetRow item )
		{
			return this.Contains( item );
		}

		void ICollection<WorksheetRow>.CopyTo( WorksheetRow[] array, int arrayIndex )
		{
			for ( int i = 0; i < this.rowIndices.Count; i++ )
				array[ i + arrayIndex ] = this.worksheet.Rows[ this.rowIndices[ i ] ];
		}

		int ICollection<WorksheetRow>.Count
		{
			get { return this.Count; }
		}

		bool ICollection<WorksheetRow>.IsReadOnly
		{
			get { return false; }
		}

		bool ICollection<WorksheetRow>.Remove( WorksheetRow item )
		{
			return this.Remove( item );
		}

		#endregion

		#region IEnumerable<WorksheetRow> Members

		IEnumerator<WorksheetRow> IEnumerable<WorksheetRow>.GetEnumerator()
		{
			foreach ( int rowIndex in this.rowIndices )
				yield return this.worksheet.Rows[ rowIndex ];
		}

		#endregion

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return ( (IEnumerable<WorksheetRow>)this ).GetEnumerator();
		}

		#endregion

		#endregion Interfaces

		#region Methods

		#region Public Methods

		#region Add

		/// <summary>
		/// Adds a row to the collection.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// This collection does not allow duplicate values. If the specified row already exists in the collection,
		/// nothing will happen. This collection also keeps itself sorted, so the row added will not necessarily be
		/// added at the end of the collection.
		/// </p>
		/// </remarks>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="row"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="row"/> does not belong to the same worksheet this collection is associated with.
		/// </exception>
		/// <param name="row">The row to be added to the hidden rows collection.</param>
		public void Add( WorksheetRow row )
		{
			if ( row == null )
				throw new ArgumentNullException( "row", SR.GetString( "LE_ArgumentNullException_HiddenRow" ) );

			if ( row.Worksheet != this.worksheet )
				throw new ArgumentException( SR.GetString( "LE_ArgumentException_RowFromDifferentWorksheet" ), "row" );

			this.AddIndex( row.Index );
		}

		#endregion Add

		#region Clear

		/// <summary>
		/// Clears all rows from the collection.
		/// </summary>
		public void Clear()
		{
			for ( int i = this.rowIndices.Count - 1; i >= 0; i-- )
				this.RemoveAt( i );
		}

		#endregion Clear

		#region Contains

		/// <summary>
		/// Determines whether the specified row exists in the collection.
		/// </summary>
		/// <param name="row">The row to search for in the collection.</param>
		/// <returns>
		/// True if the row exists in the collection; False otherwise or if the specified row is null.
		/// </returns>
		public bool Contains( WorksheetRow row )
		{
			if ( row == null )
				return false;

			return this.rowIndices.BinarySearch( row.Index ) >= 0;
		}

		#endregion Contains

		#region Remove

		/// <summary>
		/// Removes the specified row from the collection if it exists.
		/// </summary>
		/// <param name="row">The row to removed from the collection.</param>
		/// <returns>
		/// True if the row existed in the collection and was removed; False otherwise or if the 
		/// specified row is null.
		/// </returns>
		public bool Remove( WorksheetRow row )
		{
			if ( row == null )
				return false;

			int index = this.rowIndices.BinarySearch( row.Index );

			if ( index < 0 )
				return false;

			this.RemoveAt( index );
			return true;
		}

		#endregion Remove

		#region RemoveAt

		/// <summary>
		/// Removes the row at the specified index in the collection.
		/// </summary>
		/// <param name="index">The zero-based index of the row in the collection.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="index"/> is less than zero or <paramref name="index"/> is greater than or 
		/// equal to <see cref="Count"/>.
		/// </exception>
		public void RemoveAt( int index )
		{
			if ( index < 0 || this.Count <= index )
				throw new ArgumentOutOfRangeException( "index", index, SR.GetString( "LE_ArgumentOutOfRangeException_CollectionIndex" ) );

			this.rowIndices.RemoveAt( index );
		}

		#endregion RemoveAt

		#endregion Public Methods

		#region Internal Methods

		#region CreateNamedReference






		internal NamedReference CreateNamedReference()
		{
			// If there are no hidden rows in the collection, don't return anything
			if ( this.Count == 0 )
				return null;

			// The formula must start with an equal sign
			StringBuilder formulaBuilder = new StringBuilder( "=" );

			// Find all grousp of contiguous rows and create a cell reference ranges to describe these groups
			int firstRowIndex = -1;
			for ( int i = 0; i < this.rowIndices.Count; i++ )
			{
				int rowIndex = this.rowIndices[ i ];

				// If the first row in the current group has not been recorded, this is the first row
				if ( firstRowIndex < 0 )
					firstRowIndex = rowIndex;

				// If this is the last row in the hidden collection or the next row is not directly after the
				// current row in the worksheet, add the reference range for the current group to the formula
				// and start a new group
				if ( i == this.rowIndices.Count - 1 || rowIndex + 1 != this.rowIndices[ i + 1 ] )
				{
					// Append the row range as well as a list operator (,) so the next row range will be unioned
					// with the rest of the ranges
					formulaBuilder.AppendFormat( "{0}R{1}:R{2},", 
						Utilities.CreateReferenceString( null, this.worksheet.Name ), 
						firstRowIndex + 1, 
						rowIndex + 1 );

					// Reset the row group
					firstRowIndex = -1;
				}
			}

			// Remove the last list operator (,)
			formulaBuilder.Remove( formulaBuilder.Length - 1, 1 );

			// Parse the formula string in R1C1 cell reference mode.
			Formula formula;
			FormulaParseException exc;
			
			// MD 7/9/08 - Excel 2007 Format
			//if ( Formula.TryParse( formulaBuilder.ToString(), CellReferenceMode.R1C1, FormulaType.NamedReferenceFormula, out formula, out exc ) == false )
			if ( Formula.TryParse( 
				formulaBuilder.ToString(), 
				CellReferenceMode.R1C1, 
				FormulaType.NamedReferenceFormula,
				this.Worksheet.CurrentFormat,
				CultureInfo.InvariantCulture,
				// MD 2/23/12 - TFS101504
				// Pass along null for the new indexedReferencesDuringLoad parameter.
				null,
				out formula, 
				out exc ) == false )
			{
				Utilities.DebugFail( "The formula could not be parsed:\n" + exc );
				return null;
			}

			// The create the name for the named reference, which contains the custom view id
			// MD 4/6/12 - TFS101506
			//string name = String.Format( 
			//    CultureInfo.CurrentCulture, 
			//    "Z_{0}_.wvu.Rows", 
			//    this.customView.Id.ToString( "D" ).Replace( "-", "_" ).ToUpper( CultureInfo.CurrentCulture ) );
			string name = String.Format(
				CultureInfo.InvariantCulture,
				"Z_{0}_.wvu.Rows",
				this.customView.Id.ToString("D").Replace("-", "_").ToUpper(CultureInfo.InvariantCulture));

			// Create a hidden named reference with the formula
			// MD 5/25/11 - Data Validations / Page Breaks
			// We shouldn't allow a null workbook in the constructor.
			//NamedReference namedReference = new NamedReference( null, this.worksheet, true );
			NamedReference namedReference = new NamedReference(this.worksheet.Workbook.NamedReferences, this.worksheet, true);

			namedReference.FormulaInternal = formula;
			namedReference.Name = name;
			return namedReference;
		}

		#endregion CreateNamedReference

		#region InitializeFrom

		internal void InitializeFrom( Worksheet worksheet )
		{
			if ( worksheet.DefaultRowHidden )
			{
				// MD 6/31/08 - Excel 2007 Format
				//for ( int rowIndex = 0; rowIndex < Workbook.MaxExcelRowCount - 1; rowIndex++ )
				for ( int rowIndex = 0; rowIndex < worksheet.Workbook.MaxRowCount - 1; rowIndex++ )
				{
					WorksheetRow row = worksheet.Rows.GetIfCreated( rowIndex );

					if ( row == null || row.Hidden )
						this.AddIndex( rowIndex );
				}
			}
			else
			{
				if ( worksheet.HasRows )
				{
					foreach ( WorksheetRow row in worksheet.Rows )
					{
						if ( row.Hidden )
							this.Add( row );
					}
				}
			}
		}

		#endregion InitializeFrom

		#endregion Internal Methods

		#region Private Methods

		#region AddIndex

		private void AddIndex( int rowIndex )
		{
			int collectionIndex = this.rowIndices.BinarySearch( rowIndex );

			if ( collectionIndex >= 0 )
				return;

			// Insert the index sorted into the collection
			this.rowIndices.Insert( ~collectionIndex, rowIndex );
		}

		#endregion AddIndex

		#endregion Private Methods

		#endregion Methods

		#region Properties

		#region Public Properties

		#region Count

		/// <summary>
		/// Gets the number of rows in the collection.
		/// </summary>
		/// <value>The number of rows in the collection.</value>
		public int Count
		{
			get { return this.rowIndices.Count; }
		}

		#endregion Count

		#region Indexer [ int ]

		/// <summary>
		/// Gets the row at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the row to get.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="index"/> is less than zero or <paramref name="index"/> is greater than or 
		/// equal to <see cref="Count"/>.
		/// </exception>
		/// <value>The row at the specified index.</value>
		public WorksheetRow this[ int index ]
		{
			get
			{
				if ( index < 0 || this.Count <= index )
					throw new ArgumentOutOfRangeException( "index", index, SR.GetString( "LE_ArgumentOutOfRangeException_CollectionIndex" ) );

				return this.worksheet.Rows[ this.rowIndices[ index ] ];
			}
		}

		#endregion Indexer [ int ]

		#region Worksheet

		/// <summary>
		/// Gets the worksheet associated with this collection.
		/// </summary>
		/// <value>The worksheet associated with this collection.</value>
		public Worksheet Worksheet
		{
			get { return this.worksheet; }
		}

		#endregion Worksheet

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