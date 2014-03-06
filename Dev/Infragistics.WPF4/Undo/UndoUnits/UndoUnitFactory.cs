using System;
using System.Net;
//using System.Windows;
//using System.Windows.Controls;
//using System.Windows.Documents;
//using System.Windows.Ink;
//using System.Windows.Input;
//using System.Windows.Media;
//using System.Windows.Media.Animation;
//using System.Windows.Shapes;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Infragistics.Undo
{
	/// <summary>
	/// Factory class for creating common <see cref="UndoUnit"/> instances for use in an <see cref="UndoManager"/>
	/// </summary>
	/// <see cref="UndoManager.UndoUnitFactory"/>
	/// <see cref="UndoManager.UndoUnitFactoryResolved"/>
	public partial class UndoUnitFactory
	{
		#region Member Variables

		private static UndoUnitFactory _current;

		#endregion //Member Variables

		#region Constructor
		static UndoUnitFactory()
		{
			_current = new UndoUnitFactory();
		}

		/// <summary>
		/// Initializes a new <see cref="UndoUnitFactory"/>
		/// </summary>
		public UndoUnitFactory()
		{
		} 
		#endregion //Constructor

		#region Properties

		#region Current
		/// <summary>
		/// Returns the default factory used to create <see cref="UndoUnit"/> instances for use in an <see cref="UndoManager"/>
		/// </summary>
		/// <remarks>
		/// <p class="note"><b>Note:</b> This class should be thread safe since it will be stored statically.</p>
		/// </remarks>
		/// <exception cref="ArgumentNullException">The property cannot be set to a null value.</exception>
		public static UndoUnitFactory Current
		{
			get
			{
				return _current;
			}
			set
			{
				CoreUtilities.ValidateNotNull(value);
				_current = value;
			}
		}
		#endregion //Current

		#endregion //Properties

		#region Methods

		#region CreateChange
		/// <summary>
		/// Creates an <see cref="UndoUnit"/> for the specified method.
		/// </summary>
		/// <param name="description">The description for the transaction.</param>
		/// <param name="detailedDescription">The more detailed description for the operation.</param>
		/// <param name="undoMethod">The method to be invoked when an Undo is being performed. The method returns a boolean indicating if the operation was successful.</param>
		/// <param name="redoMethod">The method to be invoked when an Redo is being performed. The method returns a boolean indicating if the operation was successful.</param>
		/// <param name="target">The object that is the target of the function. This information is exposed via the <see cref="UndoUnit.Target"/> property.</param>
		/// <returns>An <see cref="UndoUnit"/> that can be used to undo the operation</returns>
		public virtual UndoUnit CreateChange(string description, string detailedDescription, Func<UndoExecuteContext, bool> undoMethod, Func<UndoExecuteContext, bool> redoMethod, object target)
		{
			Func<UndoExecuteContext, bool> executeMethod = (c) => 
			{
				var method = c.ExecuteItemType == UndoHistoryItemType.Redo ? redoMethod : undoMethod;

				if (null == method || !method(c))
					return false;

				// otherwise readd this undo unit back into the history
				c.UndoManager.AddChange(c.GetUnit(0));
				return true;
			};

			return new CustomUndoUnit(description, detailedDescription, executeMethod, target);
		}
		#endregion //CreateChange

		#region CreateCollectionChange<T>
		/// <summary>
		/// Creates an <see cref="UndoUnit"/> for a collection change based upon a specified <see cref="NotifyCollectionChangedEventArgs"/>.
		/// </summary>
		/// <typeparam name="T">The type of item in the collection</typeparam>
		/// <param name="collection">The collection that was modified</param>
		/// <param name="changedArgs">An object that describes the changes made to the collection</param>
		/// <param name="itemTypeDisplayName">A string representing the type of items in the collection as they should be presented to the end user or null to use the type name and/or item.</param>
		/// <returns>An <see cref="UndoUnit"/> that can be used to undo the operation</returns>
		public virtual UndoUnit CreateCollectionChange<T>(ICollection<T> collection, NotifyCollectionChangedEventArgs changedArgs, string itemTypeDisplayName)
		{
			return new CollectionChangeUndoUnit<T>(collection, changedArgs, itemTypeDisplayName);
		}

		/// <summary>
		/// Creates an <see cref="UndoUnit"/> for an Add/Remove operation of a specific item.
		/// </summary>
		/// <typeparam name="T">The type of item in the collection</typeparam>
		/// <param name="collection">The collection that was modified</param>
		/// <param name="action">Either 'Add' or 'Remove' to indicate the type of operation that occurred</param>
		/// <param name="changedItem">The item that was added or removed</param>
		/// <param name="index">The index of the new item for an Add operation or the index where the item existed for a Remove operation.</param>
		/// <param name="itemTypeDisplayName">A string representing the type of items in the collection as they should be presented to the end user or null to use the type name and/or item.</param>
		/// <returns>An <see cref="UndoUnit"/> that can be used to undo the operation</returns>
		public virtual UndoUnit CreateCollectionChange<T>(ICollection<T> collection, NotifyCollectionChangedAction action, T changedItem, int index, string itemTypeDisplayName)
		{
			return new CollectionChangeUndoUnit<T>(collection, action, changedItem, index, itemTypeDisplayName);
		}

		/// <summary>
		/// Creates an <see cref="UndoUnit"/> for an Add/Remove operation of multiple items.
		/// </summary>
		/// <typeparam name="T">The type of item in the collection</typeparam>
		/// <param name="collection">The collection that was modified</param>
		/// <param name="action">Either 'Add' or 'Remove' to indicate the type of operation that occurred</param>
		/// <param name="items">The item that were added or removed</param>
		/// <param name="index">The index of the new item for an Add operation or the index where the item existed for a Remove operation.</param>
		/// <param name="itemTypeDisplayName">A string representing the type of items in the collection as they should be presented to the end user or null to use the type name and/or item.</param>
		/// <returns>An <see cref="UndoUnit"/> that can be used to undo the operation</returns>
		public virtual UndoUnit CreateCollectionChange<T>(ICollection<T> collection, NotifyCollectionChangedAction action, T[] items, int index, string itemTypeDisplayName)
		{
			return new CollectionChangeUndoUnit<T>(collection, action, items, index, itemTypeDisplayName);
		}

		/// <summary>
		/// Creates an <see cref="UndoUnit"/> for replacing the entire contents of a collection.
		/// </summary>
		/// <typeparam name="T">The type of item in the collection</typeparam>
		/// <param name="collection">The collection that was modified</param>
		/// <param name="items">The item to restore to the collection upon undo.</param>
		/// <param name="itemTypeDisplayName">A string representing the type of items in the collection as they should be presented to the end user or null to use the type name and/or item.</param>
		/// <returns>An <see cref="UndoUnit"/> that can be used to undo the operation</returns>
		public virtual UndoUnit CreateCollectionChange<T>(ICollection<T> collection, T[] items, string itemTypeDisplayName)
		{
			return new CollectionChangeUndoUnit<T>(collection, items, itemTypeDisplayName);
		}

		/// <summary>
		/// Creates an <see cref="UndoUnit"/> for a Replace operation of a specific item.
		/// </summary>
		/// <typeparam name="T">The type of item in the collection</typeparam>
		/// <param name="collection">The collection that was modified</param>
		/// <param name="oldItem">The item that was removed/replaced</param>
		/// <param name="newItem">The item that was add as the replacement for <paramref name="oldItem"/></param>
		/// <param name="index">The index of the where the old item existed that is where the new item has been added.</param>
		/// <param name="itemTypeDisplayName">A string representing the type of items in the collection as they should be presented to the end user or null to use the type name and/or item.</param>
		/// <returns>An <see cref="UndoUnit"/> that can be used to undo the operation</returns>
		public virtual UndoUnit CreateCollectionChange<T>(ICollection<T> collection, T oldItem, T newItem, int index, string itemTypeDisplayName)
		{
			return new CollectionChangeUndoUnit<T>(collection, oldItem, newItem, index, itemTypeDisplayName);
		}


		/// <summary>
		/// Creates an <see cref="UndoUnit"/> for Move operation of a specific item.
		/// </summary>
		/// <typeparam name="T">The type of item in the collection</typeparam>
		/// <param name="collection">The collection that was modified</param>
		/// <param name="item">The item that was moved.</param>
		/// <param name="oldIndex">The previous index of the item.</param>
		/// <param name="newIndex">The new index of the item.</param>
		/// <param name="itemTypeDisplayName">A string representing the type of items in the collection as they should be presented to the end user or null to use the type name and/or item.</param>
		/// <returns>An <see cref="UndoUnit"/> that can be used to undo the operation</returns>
		public virtual UndoUnit CreateCollectionChange<T>(ICollection<T> collection, T item, int oldIndex, int newIndex, string itemTypeDisplayName)
		{
			return new CollectionChangeUndoUnit<T>(collection, item, oldIndex, newIndex, itemTypeDisplayName);
		}

		#endregion //CreateCollectionChange<T>

		#region CreatePropertyChange<TOwner, TProperty>
		/// <summary>
		/// Creates an <see cref="PropertyChangeUndoUnitBase"/> for the specified property change.
		/// </summary>
		/// <typeparam name="TOwner">The type of class whose value was changed</typeparam>
		/// <typeparam name="TProperty">The type of the property that was changed</typeparam>
		/// <param name="owner">The instance whose property was changed</param>
		/// <param name="getter">An expression for the property of the <typeparamref name="TOwner"/> that was changed</param>
		/// <param name="oldValue">The old value of the property that should be restored when the action is undone.</param>
		/// <param name="newValue">The new value of the property</param>
		/// <param name="propertyDisplayName">The preferred name of the property as it should be displayed to the end user. If this is not specified the actual name of the property will be used.</param>
		/// <param name="typeDisplayName">The preferred name of the object whose property is being changed as it should be displayed to the end user.</param>
		/// <returns>Returns an undo unit that can be used to undo the specified property change.</returns>
		public virtual PropertyChangeUndoUnitBase CreatePropertyChange<TOwner, TProperty>(TOwner owner, Expression<Func<TProperty>> getter, TProperty oldValue, TProperty newValue, string propertyDisplayName = null, string typeDisplayName = null)
			where TOwner : class
		{
			var getSet = GetSetHelper.Get<TOwner, TProperty>(getter);
			return new PropertyChangeUndoUnit<TOwner, TProperty>(owner, oldValue, newValue, getSet, propertyDisplayName, typeDisplayName);
		}

		/// <summary>
		/// Creates an <see cref="PropertyChangeUndoUnitBase"/> for the specified property change.
		/// </summary>
		/// <typeparam name="TOwner">The type of class whose value was changed</typeparam>
		/// <typeparam name="TProperty">The type of the property that was changed</typeparam>
		/// <param name="owner">The instance whose property was changed</param>
		/// <param name="getter">An expression for the property that was changed</param>
		/// <param name="oldValue">The old value of the property that should be restored when the action is undone.</param>
		/// <param name="newValue">The new value of the property</param>
		/// <param name="propertyDisplayName">The preferred name of the property as it should be displayed to the end user. If this is not specified the actual name of the property will be used.</param>
		/// <param name="typeDisplayName">The preferred name of the object whose property is being changed as it should be displayed to the end user.</param>
		/// <returns>Returns an undo unit that can be used to undo the specified property change.</returns>
		public virtual PropertyChangeUndoUnitBase CreatePropertyChange<TOwner, TProperty>(TOwner owner, Expression<Func<TOwner, TProperty>> getter, TProperty oldValue, TProperty newValue, string propertyDisplayName = null, string typeDisplayName = null)
			where TOwner : class
		{
			var getSet = GetSetHelper.Get<TOwner, TProperty>(getter);
			return new PropertyChangeUndoUnit<TOwner, TProperty>(owner, oldValue, newValue, getSet, propertyDisplayName, typeDisplayName);
		}

		/// <summary>
		/// Creates an <see cref="PropertyChangeUndoUnitBase"/> for the specified property change.
		/// </summary>
		/// <typeparam name="TOwner">The type of class whose value was changed</typeparam>
		/// <typeparam name="TProperty">The type of the property that was changed</typeparam>
		/// <param name="owner">The instance whose property was changed</param>
		/// <param name="propertyName">The string name of the public property that was changed. This is used to find the PropertyInfo for the property to be affected when the operation is undone.</param>
		/// <param name="oldValue">The old value of the property that should be restored when the action is undone.</param>
		/// <param name="newValue">The new value of the property</param>
		/// <param name="propertyDisplayName">The preferred name of the property as it should be displayed to the end user. If this is not specified the actual name of the property will be used.</param>
		/// <param name="typeDisplayName">The preferred name of the object whose property is being changed as it should be displayed to the end user.</param>
		/// <returns>Returns an undo unit that can be used to undo the specified property change.</returns>
		public virtual PropertyChangeUndoUnitBase CreatePropertyChange<TOwner, TProperty>(TOwner owner, string propertyName, TProperty oldValue, TProperty newValue, string propertyDisplayName = null, string typeDisplayName = null)
			where TOwner : class
		{
			var getSet = GetSetHelper.Get<TOwner, TProperty>(propertyName);
			return new PropertyChangeUndoUnit<TOwner, TProperty>(owner, oldValue, newValue, getSet, propertyDisplayName, typeDisplayName);
		}
		#endregion //CreatePropertyChanged<TOwner, TProperty>

		#region CreateTransaction
		/// <summary>
		/// Creates an <see cref="UndoTransaction"/>
		/// </summary>
		/// <param name="description">The description for the transaction.</param>
		/// <param name="detailedDescription">The detailed description for the transaction.</param>
		/// <returns>Returns a new <see cref="UndoTransaction"/> instance</returns>
		public virtual UndoTransaction CreateTransaction(string description, string detailedDescription)
		{
			return new UndoTransaction(description, detailedDescription);
		} 
		#endregion //CreateTransaction

		#endregion //Methods
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