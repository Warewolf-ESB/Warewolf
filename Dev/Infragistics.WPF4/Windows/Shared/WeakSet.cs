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







using Infragistics.Collections;

namespace Infragistics.Collections

{
	#region WeakSet Class

	internal class WeakSet<T> : ISet<T>
		where T : class
	{
		#region Member Vars

		private static object Value = new object();
		private WeakDictionary<T, object> _items;

		#endregion // Member Vars

		#region Constructor

		/// <summary>
		/// Constructor. Initializes a new instance of <see cref="WeakSet&lt;T&gt;"/>.
		/// </summary>
		public WeakSet( )
		{
			_items = new WeakDictionary<T, object>( true, false );
		}

		#endregion // Constructor

		#region Properties

		#region Private Properties

		#endregion // Private Properties

		#region Public Properties

		#region Count

		public int Count
		{
			get
			{
				return _items.Count;
			}
		}

		#endregion // Count

		#endregion // Public Properties

		#endregion // Properties

		#region Methods

		#region Public Methods

		#region Add

		public bool Add( T item )
		{
			if ( !_items.ContainsKey( item ) )
			{
				_items.Add( item, Value );
				return true;
			}

			return false;
		}

		#endregion // Add

		#region Clear

		public void Clear( )
		{
			_items.Clear( );
		}

		#endregion // Clear

		#region Contains

		public bool Contains( T item )
		{
			return _items.ContainsKey( item );
		}

		#endregion // Contains

		#region CopyTo

		public void CopyTo( T[] array, int arrayIndex )
		{
			foreach ( T ii in this )
				array[arrayIndex++] = ii;
		}

		#endregion // CopyTo

		#region ExceptWith

		public void ExceptWith( IEnumerable<T> other )
		{
			if ( null != other )
			{
				foreach ( T ii in other )
					_items.Remove( ii );
			}
		}

		#endregion // ExceptWith

		#region IntersectWith

		public void IntersectWith( IEnumerable<T> other )
		{
			if ( null != other )
			{
				List<T> itemsToKeep = new List<T>( );

				foreach ( T ii in other )
				{
					if ( this.Contains( ii ) )
						itemsToKeep.Add( ii );
				}

				this.Clear( );
				this.UnionWith( itemsToKeep );
			}
		}

		#endregion // IntersectWith

		#region IsProperSubsetOf

		public bool IsProperSubsetOf( IEnumerable<T> other )
		{
			return IsSubsetSupersetOfHelper( this, ToSet( other ), true, false );
		}

		#endregion // IsProperSubsetOf

		#region IsProperSupersetOf

		public bool IsProperSupersetOf( IEnumerable<T> other )
		{
			return IsSubsetSupersetOfHelper( this, ToSet( other ), true, true );
		}

		#endregion // IsProperSupersetOf

		#region IsSubsetOf

		public bool IsSubsetOf( IEnumerable<T> other )
		{
			return IsSubsetSupersetOfHelper( this, ToSet( other ), false, false );
		}

		#endregion // IsSubsetOf

		#region IsSupersetOf

		public bool IsSupersetOf( IEnumerable<T> other )
		{
			return IsSubsetSupersetOfHelper( this, ToSet( other ), false, true );
		}

		#endregion // IsSupersetOf

		#region Overlaps

		public bool Overlaps( IEnumerable<T> other )
		{
			if ( null != other )
			{
				foreach ( T ii in other )
				{
					if ( this.Contains( ii ) )
						return true;
				}
			}

			return false;
		}

		#endregion // Overlaps

		#region Remove

		public bool Remove( T item )
		{
			return _items.Remove( item );
		}

		#endregion // Remove

		#region SetEquals

		public bool SetEquals( IEnumerable<T> other )
		{
			if ( null != other )
			{
				// Since this set is a weak set, it's count can't be relied upon and therefore we
				// need to check items from both sets.
				// 

				ISet<T> otherSet = ToSet( other );
				foreach ( T ii in this )
				{
					if ( !otherSet.Contains( ii ) )
						return false;
				}

				foreach ( T ii in otherSet )
				{
					if ( !this.Contains( ii ) )
						return false;
				}

				return true;
			}

			return false;
		}

		#endregion // SetEquals

		#region SymmetricExceptWith

		public void SymmetricExceptWith( IEnumerable<T> other )
		{
			if ( null != other )
			{
				List<T> itemsToRemove = new List<T>( );
				List<T> itemsToAdd = new List<T>( );

				foreach ( T ii in other )
				{
					if ( this.Contains( ii ) )
						itemsToRemove.Add( ii );
					else
						itemsToAdd.Add( ii );
				}

				foreach ( T ii in itemsToRemove )
					this.Remove( ii );

				foreach ( T ii in itemsToAdd )
					this.Add( ii );
			}
		}

		#endregion // SymmetricExceptWith

		#region UnionWith

		public void UnionWith( IEnumerable<T> other )
		{
			if ( null != other )
			{
				foreach ( T ii in other )
				{
					this.Add( ii );
				}
			}
		}

		#endregion // UnionWith

		#endregion // Public Methods

		#region Private Methods

		#region IsSubsetSupersetOfHelper

		private static bool IsSubsetSupersetOfHelper( ISet<T> s1, ISet<T> s2, bool proper, bool checkSuperset )
		{
			if ( null == s1 || null == s2 )
				return false;

			if ( proper )
			{
				bool hasItemsS1 = null != CoreUtilities.GetFirstItem(s1);
				bool hasItemsS2 = null != CoreUtilities.GetFirstItem(s2);

				if ( checkSuperset )
				{
					if ( !hasItemsS1 )
						return false;
				}
				else
				{
					if ( !hasItemsS1 )
						return hasItemsS2;
				}
			}

			if ( checkSuperset )
			{
				foreach ( T ii in s2 )
				{
					if ( !s1.Contains( ii ) )
						return false;
				}
			}
			else
			{
				foreach ( T ii in s1 )
				{
					if ( !s2.Contains( ii ) )
						return false;
				}
			}

			return true;
		}

		#endregion // IsSubsetSupersetOfHelper

		#region ToSet

		private static ISet<T> ToSet( IEnumerable<T> other )
		{
			ISet<T> set = other as ISet<T>;

			if (null == set && other != null)
				set = new HashSet<T>(other);

			return set;
		}

		#endregion // ToSet

		#endregion // Private Methods

		#endregion // Methods

		#region ICollection<T> Members

		#region Add

		void ICollection<T>.Add( T item )
		{
			this.Add( item );
		}

		#endregion // Add

		#region IsReadOnly

		bool ICollection<T>.IsReadOnly
		{
			get
			{
				return false;
			}
		}

		#endregion // IsReadOnly

		#endregion // ICollection<T> Members

		#region IEnumerable<T> Members

		public IEnumerator<T> GetEnumerator( )
		{
			IEnumerable<T> ii = _items.Keys;
			return ii.GetEnumerator( );
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator( )
		{
			return this.GetEnumerator( );
		}

		#endregion
	} 

	#endregion // WeakSet Class
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