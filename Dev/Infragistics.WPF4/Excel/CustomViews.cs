using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Globalization;




using Infragistics.Shared;


namespace Infragistics.Documents.Excel
{
	/// <summary>
	/// A collection of <see cref="CustomView"/> instances in a workbook.
	/// </summary>
	/// <see cref="Workbook.CustomViews"/>
	[DebuggerDisplay( "Count = {Count}" )]



	public

		 sealed class CustomViewCollection : 
        // BF 8/6/08 - Excel 2007 Format
		//ICollection<Worksheet>
        IList<CustomView>
	{
		#region Member Variables

		private Workbook workbook;
		private List<CustomView> customViews;

		#endregion Member Variables

		#region Constructor

		internal CustomViewCollection( Workbook workbook )
		{
			this.workbook = workbook;
			this.customViews = new List<CustomView>();
		}

		#endregion Constructor

		#region Interfaces

		#region ICollection<CustomView> Members

		void ICollection<CustomView>.Add( CustomView item )
		{
			throw new InvalidOperationException( SR.GetString( "LE_InvalidOperationException_CantAddCustomView" ) );
		}

		void ICollection<CustomView>.Clear()
		{
			this.Clear();
		}

		bool ICollection<CustomView>.Contains( CustomView item )
		{
			return this.Contains( item );
		}

		void ICollection<CustomView>.CopyTo( CustomView[] array, int arrayIndex )
		{
			this.customViews.CopyTo( array, arrayIndex );
		}

		int ICollection<CustomView>.Count
		{
			get { return this.Count; }
		}

		bool ICollection<CustomView>.IsReadOnly
		{
			get { return false; }
		}

		bool ICollection<CustomView>.Remove( CustomView item )
		{
			return this.Remove( item );
		}

		#endregion

		#region IEnumerable<CustomView> Members

		IEnumerator<CustomView> IEnumerable<CustomView>.GetEnumerator()
		{
			return this.customViews.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.customViews.GetEnumerator();
		}

		#endregion

        // BF 8/6/08 - Excel 2007 Format
        #region IList<CustomView> Members

        void IList<CustomView>.Insert(int index, CustomView item)
        {
            throw new NotSupportedException();
        }

        CustomView IList<CustomView>.this[int index]
        {
            get
            {
                return this.customViews[index];
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        int IList<CustomView>.IndexOf( CustomView item )
        {
            return this.customViews.IndexOf( item );
        }

        #endregion

		#endregion Interfaces

		#region Methods

		#region Add

		// The exception comments should be similar to the exception comments on the 
		// Name setter of the CustomView.

		/// <summary>
		/// Adds a new custom view to the collection.
		/// </summary>
		/// <param name="name">The name to give the newly created custom view.</param>
		/// <param name="savePrintOptions">
		/// True to save print options for each worksheet with the custom view; False otherwise.
		/// </param>
		/// <param name="saveHiddenRowsAndColumns">
		/// True to save information about hidden rows and columns for each worksheet with the custom view; False otherwise.
		/// </param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="name"/> is a null or empty.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="name"/> is the name of another custom view in the collection (custom view names are compared 
		/// case-insensitively).
		/// </exception>
		/// <returns>The newly created <see cref="CustomView"/> instance.</returns>
		public CustomView Add( string name, bool savePrintOptions, bool saveHiddenRowsAndColumns )
		{
			CustomView customView = new CustomView( this.workbook, savePrintOptions, saveHiddenRowsAndColumns );
			customView.Name = name;

			this.customViews.Add( customView );

			return customView;
		}

        //  BF 8/6/08   Excel2007 Format
        internal void Add( CustomView customView )
        {
            this.customViews.Add( customView );
        }

		#endregion Add

		#region Clear

		/// <summary>
		/// Clears all custom views from the collection.
		/// </summary>
		public void Clear()
		{
			for ( int i = this.customViews.Count - 1; i >= 0; i-- )
				this.RemoveAt( i );
		}

		#endregion Clear

		#region Contains

		/// <summary>
		/// Determines whether a custom view is in this collection.
		/// </summary>
		/// <param name="customView">The custom view to locate in the collection.</param>
		/// <returns>True if the custom view is found; False otherwise.</returns>
		public bool Contains( CustomView customView )
		{
			return this.customViews.Contains( customView );
		}

		#endregion Contains

		#region Remove

		/// <summary>
		/// Removes the specified custom view from the collection.
		/// </summary>
		/// <param name="customView">The custom view to remove from the collection.</param>
		/// <returns>
		/// True if the custom view was successfully removed; False if the custom view was not 
		/// in the collection.
		/// </returns>
		public bool Remove( CustomView customView )
		{
			if ( customView == null )
				return false;

			int index = this.customViews.IndexOf( customView );

			if ( index < 0 )
				return false;

			this.RemoveAt( index );
			return true;
		}

		#endregion Remove

		#region RemoveAt

		/// <summary>
		/// Removes the custom view at the specified index from the collection.
		/// </summary>
		/// <param name="index">The zero-based index of the custom view in the collection.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="index"/> is less than zero or <paramref name="index"/> is greater than or 
		/// equal to <see cref="Count"/>.
		/// </exception>
		public void RemoveAt( int index )
		{
			if ( index < 0 || this.Count <= index )
				throw new ArgumentOutOfRangeException( "index", index, SR.GetString( "LE_ArgumentOutOfRangeException_CollectionIndex" ) );

			CustomView customView = this.customViews[ index ];

			this.customViews.RemoveAt( index );
			customView.OnRemovedFromCollection();
		}

		#endregion RemoveAt

		#endregion Methods

		#region Properties

		#region Public Properties

		#region Count

		/// <summary>
		/// Gets the number of custom views in the collection.
		/// </summary>
		/// <value>The number of custom views in the collection.</value>
		public int Count
		{
			get { return this.customViews.Count; }
		}

		#endregion Count

		#region Indexer [ int ]

		/// <summary>
		/// Gets the custom view at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the custom view to get.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="index"/> is less than zero or <paramref name="index"/> is greater than or 
		/// equal to <see cref="Count"/>.
		/// </exception>
		/// <value>The custom view at the specified index.</value>
		public CustomView this[ int index ]
		{
			get
			{
				if ( index < 0 || this.Count <= index )
					throw new ArgumentOutOfRangeException( "index", index, SR.GetString( "LE_ArgumentOutOfRangeException_CollectionIndex" ) );

				return this.customViews[ index ];
			}
		}

		#endregion Indexer [ int ]

		#endregion Public Properties

		#region Internal Properties

		#region Indexer [ Guid ]

		internal CustomView this[ Guid id ]
		{
			get
			{
				foreach ( CustomView customView in this.customViews )
				{
					if ( customView.Id == id )
						return customView;
				}

				return null;
			}
		}

		#endregion Indexer [ int ]

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