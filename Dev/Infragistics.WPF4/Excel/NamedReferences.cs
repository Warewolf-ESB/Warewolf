using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;
using System.Globalization;
using Infragistics.Documents.Excel.CalcEngine;





using Infragistics.Shared;


namespace Infragistics.Documents.Excel
{
	/// <summary>
	/// A collection of <see cref="NamedReference"/> instances in a workbook.
	/// </summary>
	/// <seealso cref="NamedReference"/>
	/// <seealso cref="T:Workbook.NamedReferences"/>
	[DebuggerDisplay( "Count = {Count}" )]



	public

		 sealed class NamedReferenceCollection : 
		ICollection<NamedReference>
	{
		#region Member Variables

		private Workbook workbook;
		private List<NamedReference> namedReferences;

		#endregion Member Variables

		#region Constructor

		internal NamedReferenceCollection( Workbook workbook )
		{
			this.workbook = workbook;
			this.namedReferences = new List<NamedReference>();
		}

		#endregion Constructor

		#region Interfaces

		#region ICollection<NamedReference> Members

		[System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "reference" ), System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "reference" )]
		void ICollection<NamedReference>.Add( NamedReference item )
		{
			throw new InvalidOperationException( SR.GetString( "LE_InvalidOperationException_CantAddNamedReference" ) );
		}

		void ICollection<NamedReference>.Clear()
		{
			this.Clear();
		}

		bool ICollection<NamedReference>.Contains( NamedReference item )
		{
			return this.Contains( item );
		}

		void ICollection<NamedReference>.CopyTo( NamedReference[] array, int arrayIndex )
		{
			this.namedReferences.CopyTo( array, arrayIndex );
		}

		int ICollection<NamedReference>.Count
		{
			get { return this.Count; }
		}

		bool ICollection<NamedReference>.IsReadOnly
		{
			get { return false; }
		}

		bool ICollection<NamedReference>.Remove( NamedReference item )
		{
			return this.Remove( item );
		}

		#endregion

		#region IEnumerable<NamedReference> Members

		IEnumerator<NamedReference> IEnumerable<NamedReference>.GetEnumerator()
		{
			return this.namedReferences.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.namedReferences.GetEnumerator();
		}

		#endregion

		#endregion Interfaces

		#region Methods

		#region Public Methods

		#region Add( string, string )

		/// <summary>
		/// Adds a named reference with a scope of the collection's associated <see cref="Workbook"/> to the collection.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The <see cref="CellReferenceMode"/> of the owning <see cref="Workbook"/> will be used to parse the formula.
		/// </p>
		/// </remarks>
		/// <param name="name">The name to give the named reference.</param>
		/// <param name="formula">The formula to give the named reference.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="name"/> is null or empty.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="name"/> is longer than 255 characters.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="name"/> is invalid. The name must begin with a letter, underscore (_), or a backslash (\).
		/// All other characters in the name must be letters, numbers, periods, underscores (_), or backslashes (\).
		/// The name cannot be a an A1 cell reference (1 to 3 letters followed by 1 to 6 numbers). In addition, the name
		/// cannot be 'r', 'R', 'c', or 'C' or start with a row or column reference in R1C1 cell reference mode 
		/// ('R' followed by 1 to 6 numbers or 'C' followed by 1 to 6 numbers).
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// <paramref name="name"/> is used by another named reference which also has a scope of the workbook. 
		/// Named reference names are compared case-insensitively.
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="formula"/> is null or empty.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="formula"/> is not a valid formula. The inner exception will contain the <see cref="FormulaParseException"/>
		/// describing the reason the formula was not valid.
		/// </exception>
		/// <returns>The named reference which was added to the collection.</returns>
		/// <seealso cref="NamedReferenceBase.Scope"/>
		public NamedReference Add( string name, string formula )
		{
			return this.Add( name, formula, this.workbook.CellReferenceMode );
		}

		#endregion Add( string, string )

		#region Add( string, string, CellReferenceMode )

		/// <summary>
		/// Adds a named reference with a scope of the collection's associated <see cref="Workbook"/> to the collection.
		/// </summary>
		/// <param name="name">The name to give the named reference.</param>
		/// <param name="formula">The formula to give the named reference.</param>
		/// <param name="cellReferenceMode">The mode used to interpret cell references in the formula.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="name"/> is null or empty.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="name"/> is longer than 255 characters.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="name"/> is invalid. The name must begin with a letter, underscore (_), or a backslash (\).
		/// All other characters in the name must be letters, numbers, periods, underscores (_), or backslashes (\).
		/// The name cannot be a an A1 cell reference (1 to 3 letters followed by 1 to 6 numbers). In addition, the name
		/// cannot be 'r', 'R', 'c', or 'C' or start with a row or column reference in R1C1 cell reference mode 
		/// ('R' followed by 1 to 6 numbers or 'C' followed by 1 to 6 numbers).
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// <paramref name="name"/> is used by another named reference which also has a scope of the workbook. 
		/// Named reference names are compared case-insensitively.
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="formula"/> is null or empty.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="formula"/> is not a valid formula. The inner exception will contain the <see cref="FormulaParseException"/>
		/// describing the reason the formula was not valid.
		/// </exception>
		/// <exception cref="InvalidEnumArgumentException">
		/// <paramref name="cellReferenceMode"/> is not defined in the <see cref="CellReferenceMode"/> enumeration.
		/// </exception>
		/// <returns>The named reference which was added to the collection.</returns>
		/// <seealso cref="NamedReferenceBase.Scope"/>
		public NamedReference Add( string name, string formula, CellReferenceMode cellReferenceMode )
		{
			return this.Add( name, formula, cellReferenceMode, this.workbook );
		}

		#endregion Add( string, string, CellReferenceMode )

		#region Add( string, string, Worksheet )

		/// <summary>
		/// Adds a named reference with a scope of a worksheet to the collection.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The <see cref="CellReferenceMode"/> of the owning <see cref="Workbook"/> will be used to parse the formula.
		/// </p>
		/// </remarks>
		/// <param name="name">The name to give the named reference.</param>
		/// <param name="formula">The formula to give the named reference.</param>
		/// <param name="worksheet">The scope of the named reference.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="name"/> is null or empty.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="name"/> is longer than 255 characters.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="name"/> is invalid. The name must begin with a letter, underscore (_), or a backslash (\).
		/// All other characters in the name must be letters, numbers, periods, underscores (_), or backslashes (\).
		/// The name cannot be a an A1 cell reference (1 to 3 letters followed by 1 to 6 numbers). In addition, the name
		/// cannot be 'r', 'R', 'c', or 'C' or start with a row or column reference in R1C1 cell reference mode 
		/// ('R' followed by 1 to 6 numbers or 'C' followed by 1 to 6 numbers).
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// <paramref name="name"/> is used by another named reference which also has a scope of 
		/// the specified <paramref name="worksheet"/>. Named reference names are compared case-insensitively.
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="formula"/> is null or empty.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="formula"/> is not a valid formula. The inner exception will contain the <see cref="FormulaParseException"/>
		/// describing the reason the formula was not valid.
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="worksheet"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="worksheet"/> does not belong to the workbook owning this collection.
		/// </exception>
		/// <returns>The named reference which was added to the collection.</returns>
		/// <seealso cref="NamedReferenceBase.Scope"/>
		public NamedReference Add( string name, string formula, Worksheet worksheet )
		{
			return this.Add( name, formula, this.workbook.CellReferenceMode, worksheet );
		}

		#endregion Add( string, string, Worksheet )

		#region Add( string, string, CellReferenceMode, Worksheet )

		/// <summary>
		/// Adds a named reference with a scope of a worksheet to the collection.
		/// </summary>
		/// <param name="name">The name to give the named reference.</param>
		/// <param name="formula">The formula to give the named reference.</param>
		/// <param name="cellReferenceMode">The mode used to interpret cell references in the formula.</param>
		/// <param name="worksheet">The scope of the named reference.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="name"/> is null or empty.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="name"/> is longer than 255 characters.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="name"/> is invalid. The name must begin with a letter, underscore (_), or a backslash (\).
		/// All other characters in the name must be letters, numbers, periods, underscores (_), or backslashes (\).
		/// The name cannot be a an A1 cell reference (1 to 3 letters followed by 1 to 6 numbers). In addition, the name
		/// cannot be 'r', 'R', 'c', or 'C' or start with a row or column reference in R1C1 cell reference mode 
		/// ('R' followed by 1 to 6 numbers or 'C' followed by 1 to 6 numbers).
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// <paramref name="name"/> is used by another named reference which also has a scope of 
		/// the specified <paramref name="worksheet"/>. Named reference names are compared case-insensitively.
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="formula"/> is null or empty.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="formula"/> is not a valid formula. The inner exception will contain the <see cref="FormulaParseException"/>
		/// describing the reason the formula was not valid.
		/// </exception>
		/// <exception cref="InvalidEnumArgumentException">
		/// <paramref name="cellReferenceMode"/> is not defined in the <see cref="CellReferenceMode"/> enumeration.
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="worksheet"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="worksheet"/> does not belong to the workbook owning this collection.
		/// </exception>
		/// <returns>The named reference which was added to the collection.</returns>
		/// <seealso cref="NamedReferenceBase.Scope"/>
		public NamedReference Add( string name, string formula, CellReferenceMode cellReferenceMode, Worksheet worksheet )
		{
			if ( worksheet == null )
				throw new ArgumentNullException( "worksheet", SR.GetString( "LE_ArgumentNullException_WorksheetScope" ) );

			if ( worksheet.Workbook != this.workbook )
				throw new ArgumentException( SR.GetString( "LE_ArgumentException_WorksheetScopeFromOtherWorkbook" ), "worksheet" );

			return this.Add( name, formula, cellReferenceMode, (object)worksheet );
		}

		#endregion Add( string, string, CellReferenceMode, Worksheet )

		#region Clear

		/// <summary>
		/// Clears all named references from the collection.
		/// </summary>
		public void Clear()
		{
			for ( int i = this.namedReferences.Count - 1; i >= 0; i-- )
				this.RemoveAt( i );
		}

		#endregion Clear

		#region Contains

		/// <summary>
		/// Determines whether a named reference is in the collection.
		/// </summary>
		/// <param name="namedReference">The named reference to locate in the collection.</param>
		/// <returns>True if the named reference is found; False otherwise.</returns>
		public bool Contains( NamedReference namedReference )
		{
			return this.namedReferences.Contains( namedReference );
		}

		#endregion Contains

		#region Find( string )

		/// <summary>
		/// Finds a named reference in the collection with a scope of the collection's associated <see cref="Excel.Workbook"/>.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// Named reference names are compared case-insensitively.
		/// </p>
		/// </remarks>
		/// <param name="name">The name of the named reference to find.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="name"/> is null or empty.
		/// </exception>
		/// <returns>The named reference with the specified name or null if the named reference was not found.</returns>
		public NamedReference Find( string name )
		{
			// MD 2/23/12 - 12.1 - Table Support
			//return this.Find( name, this.workbook );
			if (String.IsNullOrEmpty(name))
				throw new ArgumentNullException("name", SR.GetString("LE_ArgumentNullException_FindNamedReference"));

			return this.Workbook.GetWorkbookScopedNamedItem(name) as NamedReference;
		}

		#endregion Find( string )

		#region Find( string, Worksheet )

		/// <summary>
		/// Finds a named reference in the collection with a scope of the specified worksheet.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// Named reference names are compared case-insensitively.
		/// </p>
		/// </remarks>
		/// <param name="name">The name of the named reference to find.</param>
		/// <param name="worksheetScope">The worksheet that the named reference found must have a scope of.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="name"/> is null or empty.
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="worksheetScope"/> is null.
		/// </exception>
		/// <returns>The named reference with the specified name or null if the named reference was not found.</returns>
		public NamedReference Find( string name, Worksheet worksheetScope )
		{
			if ( worksheetScope == null )
				throw new ArgumentNullException( "worksheetScope", SR.GetString( "LE_ArgumentNullException_WorksheetScope" ) );

			// MD 7/15/08
			// Found while implementing Excel formula solving
			// This was causing a StackOverflowException. It needs to be casted to an object first.
			//return this.Find( name, worksheetScope );
			return this.Find( name, (object)worksheetScope );
		}

		#endregion Find( string, Worksheet )

		#region FindAll

		/// <summary>
		/// Finds all named references in the collection with the specified name.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// Named reference names are compared case-insensitively.
		/// </p>
		/// </remarks>
		/// <param name="name">The name of the named references to find.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="name"/> is null or empty.
		/// </exception>
		/// <returns>An array of <see cref="NamedReference"/> instances with the specified name.</returns>
		public NamedReference[] FindAll( string name )
		{
			if ( String.IsNullOrEmpty( name ) )
				throw new ArgumentNullException( "name", SR.GetString( "LE_ArgumentNullException_FindNamedReference" ) );

			List<NamedReference> namedReferences = new List<NamedReference>();

			foreach ( NamedReference namedReference in this.namedReferences )
			{
				// MD 4/6/12 - TFS101506
				//if ( String.Compare( name, namedReference.Name, StringComparison.CurrentCultureIgnoreCase ) == 0 )
				if (String.Compare(name, namedReference.Name, this.workbook.CultureResolved, CompareOptions.IgnoreCase) == 0)
					namedReferences.Add( namedReference );
			}

			return namedReferences.ToArray();
		}

		#endregion FindAll

		#region Remove

		/// <summary>
		/// Removes the specified named reference from the collection.
		/// </summary>
		/// <param name="namedReference">The named reference to remove fro the collection.</param>
		/// <returns>True if the named reference existed in the collection and was removed; False otherwise.</returns>
		public bool Remove( NamedReference namedReference )
		{
			if ( namedReference == null )
				return false;

			int index = this.namedReferences.IndexOf( namedReference );

			if ( index < 0 )
				return false;

			this.RemoveAt( index );
			return true;
		}

		#endregion Remove

		#region RemoveAt

		/// <summary>
		/// Removes the named reference at the specified index in the collection.
		/// </summary>
		/// <param name="index">The zero-based index of the named reference in the collection.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="index"/> is less than zero or <paramref name="index"/> is greater than or 
		/// equal to <see cref="Count"/>.
		/// </exception>
		public void RemoveAt( int index )
		{
			if ( index < 0 || this.Count <= index )
				throw new ArgumentOutOfRangeException( "index", index, SR.GetString( "LE_ArgumentOutOfRangeException_CollectionIndex" ) );

			NamedReference namedReference = this.namedReferences[ index ];

			this.namedReferences.RemoveAt( index );
			namedReference.OnRemovedFromCollection();

			this.Workbook.OnNamedReferenceRemoved(namedReference);
		}

		#endregion RemoveAt

		#endregion Public Methods

		#region Internal Methods

		#region Add( NamedReference )

		internal void Add( NamedReference namedReference )
		{
			// MD 5/25/11 - Data Validations / Page Breaks
			// Moved all code to the new overload.
			this.Add(namedReference, true);
		}

		// MD 5/25/11 - Data Validations / Page Breaks
		// Added a new overload.
		internal void Add(NamedReference namedReference, bool updateDataFromBuiltInReferences)
		{
			this.namedReferences.Add(namedReference);

			// MD 7/19/12 - TFS116808 (Table resizing)
			// Moved from below. We should let the workbook know the named reference exists before we do anything else.
			// The reason for this is we no longer defer adding calc references to the network, so when we request the 
			// calc reference below, it will immediately be added and we will try to find the named reference in the 
			// workbook to link up unconnected references to this name.
			this.Workbook.OnNamedReferenceAdded(namedReference);

			// MD 8/22/08 - Excel formula solving
			// New named references should have their calc references lazily created so they are added to the calc network
			// and unresolved named references from pre-existing formulas are resolved.
			IExcelCalcReference reference = namedReference.CalcReference;

			if (updateDataFromBuiltInReferences && namedReference.IsBuiltIn)
			{
				Worksheet worksheet = namedReference.Scope as Worksheet;

				if (worksheet != null)
					NamedReferenceBase.ParseBuiltInNameInfo(namedReference, worksheet.PrintOptions);
			}

			// MD 7/19/12 - TFS116808 (Table resizing)
			// Moved above.
			//this.Workbook.OnNamedReferenceAdded(namedReference);
		}

		#endregion Add( NamedReference )

		// MD 7/2/08 - Excel 2007 Format
		#region VerifyFormatLimits

		internal void VerifyFormatLimits( FormatLimitErrors limitErrors, WorkbookFormat testFormat )
		{
			foreach ( NamedReference namedReference in this )
				namedReference.VerifyFormatLimits( limitErrors, testFormat );
		} 

		#endregion VerifyFormatLimits

		#endregion Internal Methods

		#region Private Methods

		#region Add( string, string, CellReferenceMode, object )

		private NamedReference Add( string name, string formula, CellReferenceMode cellReferenceMode, object scope )
		{
			NamedReference namedReference = new NamedReference( this, scope );

			// MD 7/19/12 - TFS116808 (Table resizing)
			// Let the named reference know its initializing so it doesn't try to compile its formula in the OnFormulaChanged
			// override.
			namedReference.IsInitializing = true;

			// MD 7/2/08 - Excel 2007 Format
			// We should only add the named reference after the formula is set. Setting the formula might throw and exception
			// and then we would have an invalid named reference instance in the collection.
			//this.Add( namedReference );

			// MD 7/23/08 - Excel formula solving
			// We have to set the formula after we set the name. The formula is calculated right when it is set, so the name 
			// must be assigned or the reference's absolute name will be incorrect.
			//namedReference.SetFormula( formula, cellReferenceMode );

			// MD 7/9/08 - Excel 2007 Format
			//namedReference.SetNameInternal( name, "name" );
			namedReference.SetNameInternal( name, "name", this.Workbook.CurrentFormat );

			// MD 7/23/08 - Excel formula solving
			// We have to set the formula after we set the name. The formula is calculated right when it is set, so the name 
			// must be assigned or the reference's absolute name will be incorrect.
			namedReference.SetFormula( formula, cellReferenceMode );

			// MD 7/2/08 - Excel 2007 Format
			// We should only add the named reference after the formula is set. 
			this.Add(namedReference);

			// MD 7/19/12 - TFS116808 (Table resizing)
			// Tell the named reference it is no longer initializing and re-call OnFormulaChanged so the formula can be 
			// compiled and added to the calc network.
			namedReference.IsInitializing = false;
			namedReference.OnFormulaChanged();

			return namedReference;
		}

		#endregion Add( string, string, CellReferenceMode, object )

		#region Find( string, object )

		private NamedReference Find( string name, object scope )
		{
			if ( String.IsNullOrEmpty( name ) )
				throw new ArgumentNullException( "name", SR.GetString( "LE_ArgumentNullException_FindNamedReference" ) );

			foreach ( NamedReference namedReference in this.namedReferences )
			{
				if ( namedReference.Scope == scope &&
					// MD 4/6/12 - TFS101506
					//String.Compare( name, namedReference.Name, StringComparison.CurrentCultureIgnoreCase ) == 0 )
					String.Compare(name, namedReference.Name, this.workbook.CultureResolved, CompareOptions.IgnoreCase) == 0)
				{
					return namedReference;
				}
			}

			return null;
		}

		#endregion Find( string, object )

		#endregion Private Methods

		#endregion Methods

		#region Properties

		#region Count

		/// <summary>
		/// Gets the number of named references in the collection.
		/// </summary>
		/// <value>The number of named references in the collection.</value>
		public int Count
		{
			get { return this.namedReferences.Count; }
		}

		#endregion Count

		#region Indexer [ int ]

		/// <summary>
		/// Gets the named reference at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the named reference to get.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="index"/> is less than zero or <paramref name="index"/> is greater than or 
		/// equal to <see cref="Count"/>.
		/// </exception>
		/// <value>The named reference at the specified index.</value>
		public NamedReference this[ int index ]
		{
			get 
			{
				if ( index < 0 || this.Count <= index )
					throw new ArgumentOutOfRangeException( "index", index, SR.GetString( "LE_ArgumentOutOfRangeException_CollectionIndex" ) );

				return this.namedReferences[ index ]; 
			}
		}

		#endregion Indexer [ int ]

		#region Workbook

		/// <summary>
		/// Gets the workbook associated with this collection.
		/// </summary>
		/// <value>The workbook associated with this collection.</value>
		public Workbook Workbook
		{
			get { return this.workbook; }
		}

		#endregion Workbook

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