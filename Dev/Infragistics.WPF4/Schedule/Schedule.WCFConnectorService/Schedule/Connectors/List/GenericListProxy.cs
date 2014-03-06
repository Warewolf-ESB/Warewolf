using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Data;
using System.Linq;
using System.Reflection;


using Infragistics.Collections.Services;
using Infragistics.Controls.Schedules.Services;

namespace Infragistics.Services






{
	#region ItemFactory Class

	/// <summary>
	/// Used for creating objects of a specific type.
	/// </summary>
	public abstract class ItemFactory
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public ItemFactory( )
		{
		}

		/// <summary>
		/// Creates new object.
		/// </summary>
		/// <returns>The newly created object.</returns>
		internal abstract object CreateNew( );

		internal static ItemFactory CreateItemFactory( Type type )
		{
			Type nicType = typeof( ItemFactory<> ).MakeGenericType( type );
			ItemFactory nic = (ItemFactory)Activator.CreateInstance( nicType );

			return nic;
		}

		internal static object CreateNew( Type type )
		{
			return CreateItemFactory( type ).CreateNew( );
		}
	}

	#endregion // ItemFactory Class

	#region ItemFactory<T> Class

	/// <summary>
	/// Used for creating objects of type specified by the generic template type parameter <typeparamref name="T"/>.
	/// </summary>
	/// <typeparam name="T">Objects of this type will be created.</typeparam>
	public class ItemFactory<T> : ItemFactory
		where T : new( )
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public ItemFactory( )
		{
		}

		/// <summary>
		/// Creates new object.
		/// </summary>
		/// <returns>The newly created object.</returns>
		internal override object CreateNew( )
		{
			return new T( );
		}
	}

	#endregion // ItemFactory<T> Class

	#region GenericListProxy Class

	internal abstract class GenericListProxy
	{
		#region Member Vars

		protected object _listObj;
		protected IList _listNonGeneric;
		protected Type _listElemType;
		protected ItemFactory _itemFactory; 

		#endregion // Member Vars

		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		public GenericListProxy( )
		{
		} 

		#endregion // Constructor

		#region Methods

		#region Protected Methods

		#region Initialize

		protected virtual void Initialize( object list, Type listElemType )
		{
			_listObj = list;
			_listNonGeneric = list as IList;
			_listElemType = listElemType;
		}

		#endregion // Initialize 

		#endregion // Protected Methods

		#region Internal Methods

		#region Create

		internal static GenericListProxy Create( object list, Type listItemType )
		{
			if ( null == listItemType )
				return null;

			Type proxyType = typeof( GenericListPropxy<> ).MakeGenericType( listItemType );

			try
			{
				GenericListProxy proxy = (GenericListProxy)ItemFactory.CreateNew( proxyType );

				if ( null != proxy )
					proxy.Initialize( list, listItemType );

				return proxy;
			}
			catch
			{
			}

			return null;
		}

		#endregion // Create 

		#endregion // Internal Methods

		#region Public Methods

		#region Add

		public abstract bool Add( object item, out DataErrorInfo error );

		#endregion // Add

		#region CreateNew

		public virtual object CreateNew( out DataErrorInfo error )
		{
			error = null;

			try
			{
				if ( null != _itemFactory )
					return _itemFactory.CreateNew( );

				error = ScheduleUtilities.CreateErrorFromId(_listElemType, "LE_CantCreateObject", null != _listElemType ? _listElemType.Name : "data object"); //"Unable to create {0} object. Public parameterless constructor is required and type must be public as well."
			}
			catch ( Exception exception )
			{
				error = new DataErrorInfo( exception );
			}

			return null;
		}

		#endregion // CreateNew

		#region Remove

		public abstract bool Remove( object item, out DataErrorInfo error );

		#endregion // Remove  

		#endregion // Public Methods

		#endregion // Methods

		#region Properties

		#region Public Properties

		#region CanCreateNew

		public virtual bool CanCreateNew
		{
			get
			{
				return null != _itemFactory;
			}
		}

		#endregion // CanCreateNew

		#region IsAddAllowed

		public abstract bool IsAddAllowed { get; }

		#endregion // IsAddAllowed

		#region IsRemoveAllowed

		public abstract bool IsRemoveAllowed { get; }

		#endregion // IsRemoveAllowed  

		#endregion // Public Properties

		#endregion // Properties
	} 

	#endregion // GenericListProxy Class

	#region GenericListProxy<T> Class

	internal class GenericListPropxy<T> : GenericListProxy
	{
		#region Member Vars

		private IList<T> _listGeneric;

		#endregion // Member Vars

		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		public GenericListPropxy( )
			: base( )
		{
		}

		#endregion // Constructor

		#region Methods

		#region Protected Methods

		#region Initialize

		protected override void Initialize( object list, Type listElemType )
		{
			base.Initialize( list, listElemType );

			_listGeneric = list as IList<T>;

			try
			{
				_itemFactory = ItemFactory.CreateItemFactory( listElemType );
			}
			catch
			{
			}
		}

		#endregion // Initialize 

		#endregion // Protected Methods

		#region Public Methods

		#region Add

		public override bool Add( object item, out DataErrorInfo error )
		{
			error = null;

			try
			{
				if ( null != _listNonGeneric )
				{
					_listNonGeneric.Add( item );
					return true;
				}
				else if ( null != _listGeneric )
				{
					_listGeneric.Add( (T)item );
					return true;
				}

				error = ScheduleUtilities.CreateErrorFromId( item, "LE_IListNotImplemented", typeof( T ).Name ); //"Data source list does not implement IList<T> or IList necessary to support adding."
			}
			catch ( Exception exception )
			{
				error = new DataErrorInfo( exception );
			}

			return false;
		}

		#endregion // Add

		#region Remove

		public override bool Remove( object item, out DataErrorInfo error )
		{
			error = null;

			try
			{
				if ( null != _listNonGeneric )
				{
					_listNonGeneric.Remove( item );
					return true;
				}
				else if ( null != _listGeneric )
				{
					_listGeneric.Remove( (T)item );
					return true;
				}

				error = ScheduleUtilities.CreateErrorFromId( item, "LE_IListNotImplemented", typeof( T ).Name ); //"Data source list does not implement IList<T> or IList necessary to support adding."
			}
			catch ( Exception exception )
			{
				new DataErrorInfo( exception );
			}

			return false;
		}

		#endregion // Remove

		#endregion // Public Methods 

		#region Private Methods

		#region IsAddOrRemoveAllowed

		private bool IsAddOrRemoveAllowed( bool checkAdd )
		{
			if ( null != _listNonGeneric )
				return !_listNonGeneric.IsFixedSize && !_listNonGeneric.IsReadOnly;
			else if ( null != _listGeneric )
				return !_listGeneric.IsReadOnly;

			return false;
		}

		#endregion // IsAddOrRemoveAllowed

		#endregion // Private Methods

		#endregion // Methods

		#region Properties

		#region Public Properties

		#region IsAddAllowed

		public override bool IsAddAllowed
		{
			get
			{
				return this.IsAddOrRemoveAllowed( true );
			}
		}

		#endregion // IsAddAllowed

		#region IsRemoveAllowed

		public override bool IsRemoveAllowed
		{
			get
			{
				return this.IsAddOrRemoveAllowed( false );
			}
		}

		#endregion // IsRemoveAllowed

		#endregion // Public Properties 

		#endregion // Properties
	}

	#endregion // GenericListProxy<T> Class

	internal class ITableProxy
	{
		private object _list;
		private MethodInfo _insertMethod;
		private MethodInfo _deleteMethod;
		private PropertyInfo _isReadOnlyProperty;
		private PropertyInfo _dataContextProperty;
		private MethodInfo _submitChangesMethod;

		private ITableProxy( )
		{

		}

		public bool IsReadOnly
		{
			get
			{
				return (bool)_isReadOnlyProperty.GetValue( _list, null );
			}
		}

		public void Insert( object item, bool submit, out DataErrorInfo error )
		{
			error = null;
			try
			{
				_insertMethod.Invoke( _list, new object[] { item } );
			}
			catch ( Exception exception )
			{
				error = ScheduleUtilities.CreateErrorFromId( item, "LE_AddOperationFailed", item );
				error.Exception = exception;
			}

			if ( submit )
				this.SubmitChanges( out error );
		}

		public void Remove( object item, bool submit, out DataErrorInfo error )
		{
			error = null;

			try
			{
				_deleteMethod.Invoke( _list, new object[] { item } );
			}
			catch ( Exception exception )
			{
				error = DataErrorInfo.CreateError( item, "Unable to remove the item. An error occurred in the underlying data source." );
				error.Exception = exception;
			}

			if ( null == error && submit )
				this.SubmitChanges( out error );
		}

		public void SubmitChanges( out DataErrorInfo error )
		{
			error = null;

			try
			{
				object dataContext = _dataContextProperty.GetValue( _list, null );
				if ( null != dataContext )
					_submitChangesMethod.Invoke( dataContext, null );
				else
					Debug.Assert( false, "Table doesn't have DataContext!" );
			}
			catch ( Exception exception )
			{
				error = DataErrorInfo.CreateError( null, "Unable to remove the item. An error occurred in the underlying data source." );
				error.Exception = exception;
			}
		}

		private bool CreateHelper( Type iTableType )
		{
			_insertMethod = iTableType.GetMethod( "InsertOnSubmit", new Type[] { typeof( object ) } );
			_deleteMethod = iTableType.GetMethod( "DeleteOnSubmit", new Type[] { typeof( object ) } );
			_isReadOnlyProperty = iTableType.GetProperty( "IsReadOnly" );
			_dataContextProperty = iTableType.GetProperty( "Context" );
			_submitChangesMethod = null != _dataContextProperty
				? _dataContextProperty.PropertyType.GetMethod( "SubmitChanges", new Type[] { } ) : null;

			return null != _insertMethod && null != _deleteMethod && null != _isReadOnlyProperty && null != _submitChangesMethod;
		}

		public static ITableProxy Create( object list )
		{
			Type iTableType = GetITableType( list );
			if ( null != iTableType )
			{
				ITableProxy proxy = new ITableProxy( )
				{
					_list = list
				};

				if ( proxy.CreateHelper( iTableType ) )
					return proxy;
				else
					Debug.Assert( false );
			}

			return null;
		}

		private static Type GetITableType( object list )
		{
			Type t = list.GetType( );
			Type[] interfaces = t.GetInterfaces( );
			if ( null != interfaces )
			{
				foreach ( Type i in interfaces )
				{
					if ( i.FullName == "System.Data.Linq.ITable" )
						return i;
				}
			}

			return null;
		}
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