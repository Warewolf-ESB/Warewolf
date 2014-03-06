using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Infragistics.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Diagnostics;

namespace Infragistics.Undo
{
	/// <summary>
	/// Observable collection that supports adding collection change notifications to an <see cref="UndoManager"/>
	/// </summary>
	/// <typeparam name="T">The type of items in the collection</typeparam>
	public class ObservableCollectionExtendedWithUndo<T> : ObservableCollectionExtended<T>
	{
		#region Member Variables

		private UndoManager _undoManager;
		private T[] _originalItems;
		private string _itemTypeDisplayName;

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="ObservableCollectionExtendedWithUndo&lt;T&gt;"/> associated with the <see cref="Infragistics.Undo.UndoManager.Current"/>
		/// </summary>
		/// <param name="itemTypeDisplayName">A string representing the type of items in the collection as they should be presented to the end user or null to use the type name and/or item.</param>
		public ObservableCollectionExtendedWithUndo(string itemTypeDisplayName = null)
			: this(UndoManager.Current, itemTypeDisplayName)
		{
		}

		/// <summary>
		/// Initializes a new <see cref="ObservableCollectionExtendedWithUndo&lt;T&gt;"/>
		/// </summary>
		/// <param name="undoManager">The <see cref="UndoManager"/> with which to record the collection changes</param>
		/// <param name="itemTypeDisplayName">A string representing the type of items in the collection as they should be presented to the end user or null to use the type name and/or item.</param>
		public ObservableCollectionExtendedWithUndo(UndoManager undoManager, string itemTypeDisplayName = null)
		{
			CoreUtilities.ValidateNotNull(undoManager, "undoManager");

			_itemTypeDisplayName = itemTypeDisplayName;
			_undoManager = undoManager;
		} 
		#endregion //Constructor

		#region Base class overrides

		#region ClearItems
		/// <summary>
		/// Removes all the items from the collection.
		/// </summary>
		protected override void ClearItems()
		{
			if (!this.IsUpdating)
			{
				T[] newItems = new T[this.Count];
				this.Items.CopyTo(newItems, 0);
				this.UndoManager.AddCollectionChange(this, NotifyCollectionChangedAction.Remove, newItems, 0, _itemTypeDisplayName);
			}

			base.ClearItems();
		}
		#endregion //ClearItems

		#region InsertItem
		/// <summary>
		/// Inserts a new item at the specified index in the collection.
		/// </summary>
		/// <param name="index">The index at which to insert the <paramref name="item"/></param>
		/// <param name="item">The object to insert in the collection</param>
		protected override void InsertItem(int index, T item)
		{
			if (!this.IsUpdating)
				this.UndoManager.AddCollectionChange(this, NotifyCollectionChangedAction.Add, item, index, _itemTypeDisplayName);

			base.InsertItem(index, item);
		}
		#endregion //InsertItem

		#region InsertRange
		/// <summary>
		/// Inserts the elements of a collection into the collection at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index at which the new elements should be inserted.</param>
		/// <param name="collection">The collection whose elements should be inserted into the List. The collection itself cannot be null, but it can contain elements that are null, if type T is a reference type.</param>
		public override void InsertRange(int index, IEnumerable<T> collection)
		{
			if (!this.IsUpdating)
			{
				int count = CoreUtilities.GetCount(collection);
				T[] newItems = new T[count];
				CoreUtilities.CopyTo(collection, newItems, 0);
				this.UndoManager.AddCollectionChange(this, NotifyCollectionChangedAction.Add, newItems, index, _itemTypeDisplayName);
			}

			base.InsertRange(index, collection);
		}
		#endregion //InsertRange

		#region MoveItem



		/// <summary>
		/// Moves an item from one index in the collection to a new location.
		/// </summary>
		/// <param name="oldIndex">The index of the item to relocate</param>
		/// <param name="newIndex">The new index of the item currently located at index <paramref name="oldIndex"/></param>
		protected override void MoveItem(int oldIndex, int newIndex)
		{
			if (!this.IsUpdating)
			{
				T item = this[oldIndex];
				this.UndoManager.AddCollectionChange(this, item, oldIndex, newIndex, _itemTypeDisplayName);
			}

			base.MoveItem(oldIndex, newIndex);
		} 



		#endregion //MoveItem

		#region OnBeginUpdate
		/// <summary>
		/// Invoked when BeginUpdate is first called and <see cref="ObservableCollectionExtended&lt;T&gt;.IsUpdating"/> becomes true.
		/// </summary>
		protected override void OnBeginUpdate()
		{
			Debug.Assert(_originalItems == null, "EndUpdate was not correctly completed?");
			_originalItems = new T[this.Count];
			this.CopyTo(_originalItems, 0);

			base.OnBeginUpdate();
		}
		#endregion //OnBeginUpdate

		#region OnEndUpdate
		/// <summary>
		/// Invoked when EndUpdate is called and <see cref="ObservableCollectionExtended&lt;T&gt;.IsUpdating"/> becomes false.
		/// </summary>
		protected override void OnEndUpdate()
		{
			this.AddEndUpdateSnapshot();

			base.OnEndUpdate();
		}
		#endregion //OnEndUpdate

		#region RemoveItem
		/// <summary>
		/// Removes an item at the specified index.
		/// </summary>
		/// <param name="index">The index of the item in the collection to be removed.</param>
		protected override void RemoveItem(int index)
		{
			T item = this[index];

			if (!this.IsUpdating)
				this.UndoManager.AddCollectionChange(this, NotifyCollectionChangedAction.Remove, item, index, _itemTypeDisplayName);

			base.RemoveItem(index);
		}
		#endregion //RemoveItem

		#region RemoveRange
		/// <summary>
		/// Removes a contiguous block of items from the collection.
		/// </summary>
		/// <param name="index">The zero-based starting index of the range of elements to remove.</param>
		/// <param name="count">The number of elements to remove</param>
		public override void RemoveRange(int index, int count)
		{
			// to make sure we use consistent undo operations treat a remove of all as a clear
			if (index == 0 && count == this.Count)
			{
				this.Clear();
			}
			else
			{
				if (!this.IsUpdating)
				{
					T[] newItems = new T[count];
					List<T> list = this.Items as List<T>;

					if (null != list)
						list.CopyTo(index, newItems, 0, count);
					else
					{
						for (int i = 0; i < count; i++)
							newItems[i] = this[i + index];
					}

					this.UndoManager.AddCollectionChange(this, NotifyCollectionChangedAction.Remove, newItems, index, _itemTypeDisplayName);
				}

				base.RemoveRange(index, count);
			}
		}
		#endregion //RemoveRange

		#region SetItem
		/// <summary>
		/// Replaces an item at the specified index in the collection 
		/// </summary>
		/// <param name="index">Index of the item to replace</param>
		/// <param name="item">The item to insert into the collection.</param>
		protected override void SetItem(int index, T item)
		{
			if (!this.IsUpdating)
			{
				T oldItem = this[index];
				this.UndoManager.AddCollectionChange(this, oldItem, item, index, _itemTypeDisplayName);
			}

			base.SetItem(index, item);
		}
		#endregion //SetItem 

		#endregion //Base class overrides

		#region Properties

		#region Public Properties

		#region UndoManager
		/// <summary>
		/// Returns the <see cref="UndoManager"/> with which the collection change operations should be recorded for undo.
		/// </summary>
		public UndoManager UndoManager
		{
			get { return _undoManager; }
		}
		#endregion //UndoManager

		#endregion //Public Properties

		#endregion //Properties

		#region Methods

		#region Private Methods

		#region AddEndUpdateSnapshot
		private void AddEndUpdateSnapshot()
		{
			T[] items = _originalItems;
			_originalItems = null;

			if (items == null)
				return;

			if (CoreUtilities.AreEqual(items, this))
				return;

			this.UndoManager.AddCollectionChange(this, items, _itemTypeDisplayName);
		}
		#endregion //AddEndUpdateSnapshot

		#endregion //Private Methods

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