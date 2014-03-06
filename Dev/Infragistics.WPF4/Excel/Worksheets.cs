using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Diagnostics;
using System.Globalization;




using Infragistics.Shared;


namespace Infragistics.Documents.Excel
{
	/// <summary>
	/// A collection of worksheets in a workbook.
	/// </summary>
	/// <seealso cref="Worksheet"/>
	/// <seealso cref="T:Workbook.Worksheets"/>
	[DebuggerDisplay( "Count = {Count}" )]



	public

		 sealed class WorksheetCollection : 
        // MBS 7/18/08 - Excel 2007 Format
		//ICollection<Worksheet>
        IList<Worksheet>
	{
		#region Member Variables

		private List<Worksheet> worksheets;
		private Workbook workbook;

		#endregion Member Variables

		#region Constructor

		internal WorksheetCollection( Workbook parentWorkbook )
		{
			this.workbook = parentWorkbook;
			this.worksheets = new List<Worksheet>();
		}

		#endregion Constructor

		#region Interfaces

		#region ICollection<Worksheet> Members

		void ICollection<Worksheet>.Add( Worksheet item )
		{
			throw new InvalidOperationException( SR.GetString( "LE_InvalidOperationException_CantAddWorksheet" ) );
		}

		void ICollection<Worksheet>.Clear()
		{
			this.Clear();
		}

		bool ICollection<Worksheet>.Contains( Worksheet item )
		{
			return this.Contains( item );
		}

		void ICollection<Worksheet>.CopyTo( Worksheet[] array, int arrayIndex )
		{
			this.worksheets.CopyTo( array, arrayIndex );
		}

		int ICollection<Worksheet>.Count
		{
			get { return this.Count; }
		}

		bool ICollection<Worksheet>.IsReadOnly
		{
			get { return false; }
		}

		bool ICollection<Worksheet>.Remove( Worksheet item )
		{
			return this.Remove( item );
		}

		#endregion

		#region IEnumerable<Worksheet> Members

		IEnumerator<Worksheet> IEnumerable<Worksheet>.GetEnumerator()
		{
			return this.worksheets.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.worksheets.GetEnumerator();
		}

		#endregion

        // MBS 7/18/08 - Excel 2007 Format
        #region IList<Worksheet> Members

        void IList<Worksheet>.Insert(int index, Worksheet item)
        {
            throw new NotSupportedException();
        }

        Worksheet IList<Worksheet>.this[int index]
        {
            get
            {
                return this.worksheets[index];
            }
            set
            {
                throw new NotSupportedException();
            }
        }
        #endregion

		#endregion Interfaces

		#region Methods

		#region Add

		/// <summary>
		/// Creates a new <see cref="Worksheet"/> and adds it to the collection.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// If the workbook originally had no worksheets, the newly added worksheet will become the selected worksheet of 
		/// the workbook.  This can be changed after more worksheets are added by setting the <see cref="WindowOptions.SelectedWorksheet"/> 
		/// of the Workbook.
		/// </p>
		/// </remarks>
		/// <param name="name">The name to give the new Worksheet.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="name"/> is null or empty.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="name"/> contains the invalid characters: ':', '\', '/', '?', '*', '[', or ']'.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="name"/> exceeds 31 characters in length.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="name"/> is being used as the name of another worksheet (worksheet names are case-insensitively compared).
		/// </exception>
		/// <returns>The Worksheet created with the specified name.</returns>
		public Worksheet Add( string name )
		{
			if ( String.IsNullOrEmpty( name ) )
				throw new ArgumentNullException( "name", SR.GetString( "LE_ArgumentNullException_WorksheetName" ) );

			// MD 1/9/08 - BR29299
			// The name could have invalid characters
			Worksheet.VerifyNameIsValid( name, "name" );

			this.workbook.VerifyWorksheetName( null, name, "name" );

			// MD 4/12/11
			// Found while fixing TFS67084
			// We use the Workbook more often than the parent collection, so we will store that instead.
			//Worksheet worksheet = new Worksheet( name, this );
			Worksheet worksheet = new Worksheet(name, this.workbook);

            // MBS 7/17/08 - Excel 2007 Format
            // Refactored into a helper method so that when we load the 'sheet' element, 
            // we can add the worksheet that was created with the Worksheet part/element.
            //
            //this.worksheets.Add( worksheet );
            //this.workbook.OnWorksheetAdded( worksheet );
            this.InternalAdd(worksheet);

			return worksheet;
		}

		#endregion Add

		#region Clear

		/// <summary>
		/// Clears all worksheets from the collection.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// If there are any <see cref="NamedReference"/> instances in the <see cref="Excel.Workbook.NamedReferences"/> collection
		/// with a worksheet for a scope, they will be removed from the <see cref="Workbook"/>.
		/// </p>
		/// </remarks>
		public void Clear()
		{
			for ( int i = this.worksheets.Count - 1; i >= 0; i-- )
				this.RemoveAt( i );
		}

		#endregion Clear

		#region Contains

		/// <summary>
		/// Determines whether a worksheet is in the collection.
		/// </summary>
		/// <param name="worksheet">The worksheet to locate in the collection.</param>
		/// <returns>True if the worksheet is found; False otherwise.</returns>
		public bool Contains( Worksheet worksheet )
		{
			return this.worksheets.Contains( worksheet );
		}

		#endregion Contains

		// MD 8/20/08 - Excel formula solving
		#region Exists

		/// <summary>
		/// Determines whether a worksheet with the specified name exists in the collection.
		/// </summary>
		/// <param name="name">The name of the worksheet to search for. The name is compared case-insensitively.</param>
		/// <returns>True if a worksheet with the specified name is found; False otherwise.</returns>
		public bool Exists( string name )
		{
			foreach ( Worksheet worksheet in this.worksheets )
			{
				// MD 4/6/12 - TFS101506
				//if ( String.Compare( worksheet.Name, name, StringComparison.CurrentCultureIgnoreCase ) == 0 )
				if (String.Compare(worksheet.Name, name, this.workbook.CultureResolved, CompareOptions.IgnoreCase) == 0)
					return true;
			}

			return false;
		} 

		#endregion Exists

		#region IndexOf

		/// <summary>
		/// Gets the index of the specified worksheet in the collection.
		/// </summary>
		/// <param name="worksheet">The worksheet of which to get the index.</param>
		/// <returns>The index of the specified worksheet in the collection.</returns>
		/// <seealso cref="Worksheet.Index"/>
		public int IndexOf( Worksheet worksheet )
		{
			return this.worksheets.IndexOf( worksheet );
		}

		#endregion IndexOf

        // MBS 7/17/08 - Excel 2007 Format
        #region InternalAdd

        internal void InternalAdd(Worksheet worksheet)
        {
            this.worksheets.Add(worksheet);
            this.workbook.OnWorksheetAdded(worksheet);
        }
        #endregion //InternalAdd

		// MD 9/9/08 - Worksheet Moving
		#region MoveWorksheet

		internal void MoveWorksheet( int index, int newIndex )
		{
			Worksheet worksheet = this.worksheets[ index ];
			this.worksheets.RemoveAt( index );
			this.worksheets.Insert( newIndex, worksheet );

			// MD 6/18/12 - TFS102878
			Debug.Assert(worksheet.Index == newIndex, "This is unexpected.");
			this.workbook.OnWorksheetMoved(worksheet, index);
		}

		#endregion MoveWorksheet

		#region Remove

		/// <summary>
		/// Removes the specified worksheet from the collection.
		/// </summary>
		/// <param name="worksheet">The worksheet to remove from the collection.</param>
		/// <remarks>
		/// <p class="body">
        /// If there are any <see cref="NamedReference"/> instances in the <see cref="Excel.Workbook.NamedReferences"/> collection
		/// with the worksheet to remove as their scope, they will be removed from the <see cref="Workbook"/>.
		/// </p>
		/// </remarks>
		/// <returns>
		/// True if the worksheet was successfully removed from the collection; 
		/// False if the worksheet did not exist in the collection.
		/// </returns>
		public bool Remove( Worksheet worksheet )
		{
			int index = this.worksheets.IndexOf( worksheet );

			if ( index < 0 )
				return false;

			this.RemoveAt( index );
			return true;
		}

		#endregion Remove

		#region RemoveAt

		/// <summary>
		/// Removes the worksheet at the specified index from the collection.
		/// </summary>
		/// <param name="index">The index of the worksheet to remove from the collection.</param>
		/// <remarks>
		/// <p class="body">
        /// If there are any <see cref="NamedReference"/> instances in the <see cref="Excel.Workbook.NamedReferences"/> collection
		/// with the worksheet to remove as their scope, they will be removed from the <see cref="Workbook"/>.
		/// </p>
		/// </remarks>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="index"/> is less than zero or <paramref name="index"/> is greater than or equal to <see cref="Count"/>.
		/// </exception>
		public void RemoveAt( int index )
		{
			if ( index < 0 || this.Count <= index )
				throw new ArgumentOutOfRangeException( "index", index, SR.GetString( "LE_ArgumentOutOfRangeException_CollectionIndex" ) );

			Worksheet worksheet = this.worksheets[ index ];

			this.worksheets.RemoveAt( index );

			// MD 8/20/08 - Excel formula solving
			this.workbook.OnWorksheetRemoving( worksheet );

			worksheet.OnRemovedFromCollection();
			this.workbook.OnWorksheetRemoved(worksheet, index);
		}

		#endregion RemoveAt

		// MD 7/2/08 - Excel 2007 Format
		#region VerifyFormatLimits

		internal void VerifyFormatLimits( FormatLimitErrors limitErrors, WorkbookFormat testFormat )
		{
			foreach ( Worksheet worksheet in this )
				worksheet.VerifyFormatLimits( limitErrors, testFormat );
		}

		#endregion VerifyFormatLimits

		#endregion Methods

		#region Properties

		#region Public Properties

		#region Count

		/// <summary>
		/// Gets the number of worksheets in the collection.
		/// </summary>
		/// <value>The number of worksheets in the collection.</value>
		public int Count
		{
			get { return this.worksheets.Count; }
		}

		#endregion Count

		#region Indexer [ int ]

		/// <summary>
		/// Gets the worksheet at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the worksheet to get.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="index"/> is less than zero or <paramref name="index"/> is greater than or equal to <see cref="Count"/>.
		/// </exception>
		/// <value>The worksheet at the specified index.</value>
		public Worksheet this[ int index ]
		{
			get
			{
				if ( index < 0 || this.Count <= index )
					throw new ArgumentOutOfRangeException( "index", index, SR.GetString( "LE_ArgumentOutOfRangeException_CollectionIndex" ) );

				return this.worksheets[ index ];
			}
		}

		#endregion Indexer [ int ]

		#region Indexer [ string ]

		/// <summary>
		/// Gets the worksheet with the specified name.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// Worksheet names are compared case-insensitively.
		/// </p>
		/// </remarks>
		/// <param name="name">The name of the worksheet to get.</param>
		/// <exception cref="InvalidOperationException">
		/// A worksheet with the specified name does not exist in the collection. 
		/// </exception>
		/// <value>The worksheet with the specified name.</value>
		/// <seealso cref="Worksheet.Name"/>
		public Worksheet this[ string name ]
		{
			get
			{
				foreach ( Worksheet worksheet in this.worksheets )
				{
					// MD 4/6/12 - TFS101506
					//if ( String.Compare( worksheet.Name, name, StringComparison.CurrentCultureIgnoreCase ) == 0 )
					if (String.Compare(worksheet.Name, name, this.workbook.CultureResolved, CompareOptions.IgnoreCase) == 0)
						return worksheet;
				}

				throw new InvalidOperationException( SR.GetString( "LER_Exception_KeyNotFound" ) );
			}
		}

		#endregion Indexer [ string ]

		#endregion Public Properties

		#region Internal Properties

		#region Workbook

		internal Workbook Workbook
		{
			get { return this.workbook; }
		}

		#endregion Workbook

		#endregion Internal Properties

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