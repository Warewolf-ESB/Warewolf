using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Collections;
using System.Diagnostics;
using System.Windows.Data;
using System.Linq;


using Infragistics.Services;

namespace Infragistics.Collections.Services



{
	#region ListSection Class

	internal class ListSection : IList
	{
		#region Nested Data Structures

		#region ListSectionEnumerator Class

		private class ListSectionEnumerator : IEnumerator
		{
			#region Member Vars

			private IList _list;
			private int _startIndex;
			private int _length;
			private int _currentIndex;

			#endregion // Member Vars

			#region Constructor

			internal ListSectionEnumerator( IList list, int startIndex, int length )
			{
				_list = list;
				_startIndex = startIndex;
				_length = length;

                this.Reset( );
			}

			#endregion // Constructor

			#region Current

			public object Current
			{
				get
				{
					return _list[_startIndex + _currentIndex];
				}
			}

			#endregion // Current

			#region MoveNext

			public bool MoveNext( )
			{
				return ++_currentIndex < _length;
			}

			#endregion // MoveNext

			#region Reset

			public void Reset( )
			{
				_currentIndex = -1;
			}

			#endregion // Reset
		}

		#endregion // ListSectionEnumerator Class

		#endregion // Nested Data Structures

		#region Member Vars

		private IList _source;
		private int _startIndex;
		private int _length;

		#endregion // Member Vars

		#region Constructor

		internal ListSection( IList source, int startIndex, int length )
		{
			_source = source;
			_startIndex = startIndex;
			_length = length;
		}

		#endregion // Constructor

		#region Indexer

		public object this[int index]
		{
			get
			{
				if ( index >= 0 && index < _length )
					return _source[index + _startIndex];

				throw new IndexOutOfRangeException( );
			}
			set
			{
                CoreUtilities.RaiseReadOnlyCollectionException( );
			}
		}

		#endregion // Indexer

		#region Methods

		#region Public Methods

		#region Add

		public int Add( object value )
		{
            CoreUtilities.RaiseReadOnlyCollectionException( );
			return -1;
		}

		#endregion // Add

		#region Clear

		public void Clear( )
		{
            CoreUtilities.RaiseReadOnlyCollectionException( );
		}

		#endregion // Clear

		#region Contains

		public bool Contains( object value )
		{
			return this.IndexOf( value ) >= 0;
		}

		#endregion // Contains

		#region CopyTo

		public void CopyTo( Array array, int index )
		{
            CoreUtilities.CopyTo( this, array, index );
		}

		#endregion // CopyTo

		#region GetEnumerator

		public IEnumerator GetEnumerator( )
		{
			return new ListSectionEnumerator( _source, _startIndex, _length );
		}

		#endregion // GetEnumerator

		#region IndexOf

		public int IndexOf( object value )
		{
			int index = _source.IndexOf( value );

			return index >= _startIndex && index - _startIndex < _length ? index - _startIndex : -1;
		}

		#endregion // IndexOf

		#region Insert

		public void Insert( int index, object value )
		{
            CoreUtilities.RaiseReadOnlyCollectionException( );
		}

		#endregion // Insert

		#region Remove

		public void Remove( object value )
		{
            CoreUtilities.RaiseReadOnlyCollectionException( );
		}

		#endregion // Remove

		#region RemoveAt

		public void RemoveAt( int index )
		{
            CoreUtilities.RaiseReadOnlyCollectionException( );
		}

		#endregion // RemoveAt

		#endregion // Public Methods

		#endregion // Methods

		#region Properties

		#region Public Properties

		#region Count

		public int Count
		{
			get
			{
				return _length;
			}
		}

		#endregion // Count

		#region IsFixedSize

		public bool IsFixedSize
		{
			get
			{
				return true;
			}
		}

		#endregion // IsFixedSize

		#region IsReadOnly

		public bool IsReadOnly
		{
			get
			{
				return true;
			}
		}

		#endregion // IsReadOnly

		#region IsSynchronized

		public bool IsSynchronized
		{
			get
			{
				return _source.IsSynchronized;
			}
		}

		#endregion // IsSynchronized

		#region SyncRoot

		public object SyncRoot
		{
			get
			{
				return _source.SyncRoot;
			}
		}

		#endregion // SyncRoot

		#endregion // Public Properties

		#endregion // Properties
	} 

	#endregion // ListSection Class
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