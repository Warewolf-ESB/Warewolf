using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Globalization;




using Infragistics.Shared;


namespace Infragistics.Documents.Excel
{
	/// <summary>
	/// A collection of hidden <see cref="WorksheetColumn"/> instances.
	/// </summary>
	/// <remarks>
	/// <p class="body">
	/// The columns in this collection do not necessarily have their <see cref="RowColumnBase.Hidden"/> property
	/// set to True. This collection applies to a <see cref="Worksheet"/>, but belongs to a <see cref="CustomView"/>.
	/// When the CustomView which owns this collection is applied, the columns in this collection will have their
	/// <see cref="RowColumnBase.Hidden"/> property set to True. All other columns in the associated worksheet will
	/// be made visible.
	/// </p>
	/// </remarks>
	/// <seealso cref="CustomView.GetHiddenColumns"/>
	[DebuggerDisplay( "Count = {Count}" )]



	public

		 sealed class HiddenColumnCollection : 
		ICollection<WorksheetColumn>
	{
		#region Member Variables

		private Worksheet worksheet;
		private CustomView customView;

		private List<int> columnIndices;

		#endregion Member Variables

		#region Constructor

		internal HiddenColumnCollection( Worksheet worksheet, CustomView customView )
		{
			this.worksheet = worksheet;
			this.customView = customView;

			this.columnIndices = new List<int>();
		}

		#endregion Constructor

		#region Interfaces

		#region ICollection<WorksheetRow> Members

		void ICollection<WorksheetColumn>.Add( WorksheetColumn item )
		{
			this.Add( item );
		}

		void ICollection<WorksheetColumn>.Clear()
		{
			this.Clear();
		}

		bool ICollection<WorksheetColumn>.Contains( WorksheetColumn item )
		{
			return this.Contains( item );
		}

		void ICollection<WorksheetColumn>.CopyTo( WorksheetColumn[] array, int arrayIndex )
		{
			for ( int i = 0; i < this.columnIndices.Count; i++ )
				array[ i + arrayIndex ] = this.worksheet.Columns[ this.columnIndices[ i ] ];
		}

		int ICollection<WorksheetColumn>.Count
		{
			get { return this.Count; }
		}

		bool ICollection<WorksheetColumn>.IsReadOnly
		{
			get { return false; }
		}

		bool ICollection<WorksheetColumn>.Remove( WorksheetColumn item )
		{
			return this.Remove( item );
		}

		#endregion

		#region IEnumerable<WorksheetRow> Members

		IEnumerator<WorksheetColumn> IEnumerable<WorksheetColumn>.GetEnumerator()
		{
			foreach ( int columnIndex in this.columnIndices )
				yield return this.worksheet.Columns[ columnIndex ];
		}

		#endregion

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return ( (IEnumerable<WorksheetColumn>)this ).GetEnumerator();
		}

		#endregion

		#endregion Interfaces

		#region Methods

		#region Public Methods

		#region Add

		/// <summary>
		/// Adds a column to the collection.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// This collection does not allow duplicate values. If the specified column already exists in the collection,
		/// nothing will happen. This collection also keeps itself sorted, so the column added will not necessarily be
		/// added at the end of the collection.
		/// </p>
		/// </remarks>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="column"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="column"/> does not belong to the same worksheet this collection is associated with.
		/// </exception>
		/// <param name="column">The column to be added to the hidden columns collection.</param>
		public void Add( WorksheetColumn column )
		{
			if ( column == null )
				throw new ArgumentNullException( "column", SR.GetString( "LE_ArgumentNullException_HiddenColumn" ) );

			if ( column.Worksheet != this.worksheet )
				throw new ArgumentException( SR.GetString( "LE_ArgumentException_ColumnFromDifferentWorksheet" ), "column" );

			this.AddIndex( column.Index );
		}

		#endregion Add

		#region Clear

		/// <summary>
		/// Clears all columns from the collection.
		/// </summary>
		public void Clear()
		{
			for ( int i = this.columnIndices.Count - 1; i >= 0; i-- )
				this.RemoveAt( i );
		}

		#endregion Clear

		#region Contains

		/// <summary>
		/// Determines whether the specified column exists in the collection.
		/// </summary>
		/// <param name="column">The column to search for in the collection.</param>
		/// <returns>
		/// True if the column exists in the collection; False otherwise or if the specified column is null.
		/// </returns>
		public bool Contains( WorksheetColumn column )
		{
			if ( column == null )
				return false;

			return this.columnIndices.BinarySearch( column.Index ) >= 0;
		}

		#endregion Contains

		#region Remove

		/// <summary>
		/// Removes the specified column from the collection if it exists.
		/// </summary>
		/// <param name="column">The column to removed from the collection.</param>
		/// <returns>
		/// True if the column existed in the collection and was removed; False otherwise or if the 
		/// specified column is null.
		/// </returns>
		public bool Remove( WorksheetColumn column )
		{
			if ( column == null )
				return false;

			int index = this.columnIndices.BinarySearch( column.Index );

			if ( index < 0 )
				return false;

			this.RemoveAt( index );
			return true;
		}

		#endregion Remove

		#region RemoveAt

		/// <summary>
		/// Removes the column at the specified index in the collection.
		/// </summary>
		/// <param name="index">The zero-based index of the column in the collection.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="index"/> is less than zero or <paramref name="index"/> is greater than or 
		/// equal to <see cref="Count"/>.
		/// </exception>
		public void RemoveAt( int index )
		{
			if ( index < 0 || this.Count <= index )
				throw new ArgumentOutOfRangeException( "index", index, SR.GetString( "LE_ArgumentOutOfRangeException_CollectionIndex" ) );

			this.columnIndices.RemoveAt( index );
		}

		#endregion RemoveAt

		#endregion Public Methods

		#region Internal Methods

		#region CreateNamedReference






		internal NamedReference CreateNamedReference()
		{
			// If there are no hidden columns in the collection, don't return anything
			if ( this.Count == 0 )
				return null;

			// The formula must start with an equal sign
			StringBuilder formulaBuilder = new StringBuilder( "=" );

			// Find all grousp of contiguous columns and create a cell reference ranges to describe these groups
			int firstColumnIndex = -1;
			for ( int i = 0; i < this.columnIndices.Count; i++ )
			{
				int columnIndex = this.columnIndices[ i ];

				// If the first column in the current group has not been recorded, this is the first column
				if ( firstColumnIndex < 0 )
					firstColumnIndex = columnIndex;

				// If this is the last column in the hidden collection or the next column is not directly after the
				// current column in the worksheet, add the reference range for the current group to the formula
				// and start a new group
				if ( i == this.columnIndices.Count - 1 || columnIndex + 1 != this.columnIndices[ i + 1 ] )
				{
					// Append the column range as well as a list operator (,) so the next column range will be unioned
					// with the rest of the ranges
					formulaBuilder.AppendFormat( "{0}C{1}:C{2},", 
						Utilities.CreateReferenceString( null, this.worksheet.Name ), 
						firstColumnIndex + 1, 
						columnIndex + 1 );

					// Reset the column group
					firstColumnIndex = -1;
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
				Utilities.DebugFail( "The formula could not be parsed.\n:" + exc );
				return null;
			}

			// The create the name for the named reference, which contains the custom view id
			// MD 4/6/12 - TFS101506
			//string name = String.Format( 
			//    CultureInfo.CurrentCulture, 
			//    "Z_{0}_.wvu.Cols", 
			//    this.customView.Id.ToString( "D" ).Replace( "-", "_" ).ToUpper( CultureInfo.CurrentCulture ) );
			string name = String.Format(
				CultureInfo.InvariantCulture,
				"Z_{0}_.wvu.Cols",
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
			// MD 3/15/12 - TFS104581
			//if ( worksheet.HasColumns )
			//{
			//    foreach ( WorksheetColumn column in worksheet.Columns )
			//    {
			//        if ( column.Hidden )
			//            this.Add( column );
			//    }
			//}
			foreach (WorksheetColumnBlock columnBlock in worksheet.ColumnBlocks.Values)
			{
				if (columnBlock.Hidden)
				{
					for (int i = columnBlock.FirstColumnIndex; i <= columnBlock.LastColumnIndex; i++)
						this.Add(worksheet.Columns[i]);
				}
			}
		}

		#endregion InitializeFrom

		#endregion Internal Methods

		#region Private Methods

		#region AddIndex

		private void AddIndex( int columnIndex )
		{
			int collectionIndex = this.columnIndices.BinarySearch( columnIndex );

			if ( collectionIndex >= 0 )
				return;

			this.columnIndices.Insert( ~collectionIndex, columnIndex );
		}

		#endregion AddIndex

		#endregion Private Methods

		#endregion Methods

		#region Properties

		#region Public Properties

		#region Count

		/// <summary>
		/// Gets the number of columns in the collection.
		/// </summary>
		/// <value>The number of columns in the collection.</value>
		public int Count
		{
			get { return this.columnIndices.Count; }
		}

		#endregion Count

		#region Indexer [ int ]

		/// <summary>
		/// Gets the column at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the column to get.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="index"/> is less than zero or <paramref name="index"/> is greater than or 
		/// equal to <see cref="Count"/>.
		/// </exception>
		/// <value>The column at the specified index.</value>
		public WorksheetColumn this[ int index ]
		{
			get
			{
				if ( index < 0 || this.Count <= index )
					throw new ArgumentOutOfRangeException( "index", index, SR.GetString( "LE_ArgumentOutOfRangeException_CollectionIndex" ) );

				return this.worksheet.Columns[ this.columnIndices[ index ] ];
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