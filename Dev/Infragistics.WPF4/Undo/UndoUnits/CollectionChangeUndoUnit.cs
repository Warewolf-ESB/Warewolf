using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections.Specialized;
using System.Collections.ObjectModel;


using Infragistics.Collections;


namespace Infragistics.Undo
{
	/// <summary>
	/// Custom <see cref="UndoUnit"/> for undoing changes to an <see cref="ICollection&lt;T&gt;"/>.
	/// </summary>
	/// <typeparam name="T">The type of item in the collection</typeparam>
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public class CollectionChangeUndoUnit<T> : UndoUnit
	{
		#region Member Variables

		private ICollection<T> _collection;
		private CollectionChange _change;
		private string _itemTypeDisplayName;

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="CollectionChangeUndoUnit&lt;T&gt;"/> based upon the specified <see cref="NotifyCollectionChangedEventArgs"/>
		/// </summary>
		/// <param name="collection">The collection that was modified</param>
		/// <param name="changedArgs">An object that describes the changes made to the collection</param>
		/// <param name="itemTypeDisplayName">A string representing the type of items in the collection as they should be presented to the end user or null to use the type name and/or item.</param>
		/// <exception cref="ArgumentNullException">The <paramref name="collection"/> and <paramref name="changedArgs"/> must not be null.</exception>
		/// <exception cref="ArgumentException">The <paramref name="collection"/> cannot be ReadOnly.</exception>
		/// <exception cref="InvalidOperationException">The <see cref="NotifyCollectionChangedEventArgs.Action"/> cannot be 'Reset'.</exception>
		public CollectionChangeUndoUnit(ICollection<T> collection, NotifyCollectionChangedEventArgs changedArgs, string itemTypeDisplayName)
			: this(collection, CreateChange(changedArgs), itemTypeDisplayName)
		{
		}

		/// <summary>
		/// Initializes a new <see cref="CollectionChangeUndoUnit&lt;T&gt;"/> for an add or remove of a specific item.
		/// </summary>
		/// <param name="collection">The collection that was modified</param>
		/// <param name="action">Must be either 'Add' or 'Remove' to indicate if the item was added or removed.</param>
		/// <param name="changedItem">The item that was added or removed.</param>
		/// <param name="index">The index at which the item was inserted for an Add or the old index of the item for a Remove <paramref name="action"/>.</param>
		/// <param name="itemTypeDisplayName">A string representing the type of items in the collection as they should be presented to the end user or null to use the type name and/or item.</param>
		/// <exception cref="ArgumentNullException">The <paramref name="collection"/> must not be null.</exception>
		/// <exception cref="ArgumentException">The <paramref name="collection"/> cannot be ReadOnly.</exception>
		/// <exception cref="ArgumentException">The <paramref name="action"/> must be 'Add' or 'Remove'.</exception>
		public CollectionChangeUndoUnit(ICollection<T> collection, NotifyCollectionChangedAction action, T changedItem, int index, string itemTypeDisplayName)
			: this(collection, CreateChange(changedItem, index, IsAddRemove(action)), itemTypeDisplayName)
		{
		}

		/// <summary>
		/// Initializes a new <see cref="CollectionChangeUndoUnit&lt;T&gt;"/> for an add or remove of multiple items.
		/// </summary>
		/// <param name="collection">The collection that was modified</param>
		/// <param name="action">Must be either 'Add' or 'Remove' to indicate if the item was added or removed.</param>
		/// <param name="items">The items that were added or removed.</param>
		/// <param name="index">The index at which the items were inserted for an Add or the old index of the item for a Remove <paramref name="action"/>.</param>
		/// <param name="itemTypeDisplayName">A string representing the type of items in the collection as they should be presented to the end user or null to use the type name and/or item.</param>
		/// <exception cref="ArgumentNullException">The <paramref name="collection"/> must not be null.</exception>
		/// <exception cref="ArgumentException">The <paramref name="collection"/> cannot be ReadOnly.</exception>
		/// <exception cref="ArgumentException">The <paramref name="action"/> must be 'Add' or 'Remove'.</exception>
		public CollectionChangeUndoUnit(ICollection<T> collection, NotifyCollectionChangedAction action, T[] items, int index, string itemTypeDisplayName)
			: this(collection, new AddRemoveBlock(items, index, IsAddRemove(action)), itemTypeDisplayName)
		{
		}

		/// <summary>
		/// Initializes a new <see cref="CollectionChangeUndoUnit&lt;T&gt;"/> for a reset of the specified collection.
		/// </summary>
		/// <param name="collection">The collection that was modified</param>
		/// <param name="items">The old items previously in the collection.</param>
		/// <param name="itemTypeDisplayName">A string representing the type of items in the collection as they should be presented to the end user or null to use the type name and/or item.</param>
		/// <exception cref="ArgumentNullException">The <paramref name="collection"/> must not be null.</exception>
		/// <exception cref="ArgumentException">The <paramref name="collection"/> cannot be ReadOnly.</exception>
		public CollectionChangeUndoUnit(ICollection<T> collection, T[] items, string itemTypeDisplayName)
			: this(collection, new ResetChange(items), itemTypeDisplayName)
		{
		}

		/// <summary>
		/// Initializes a new <see cref="CollectionChangeUndoUnit&lt;T&gt;"/> for a replacement of a specific item.
		/// </summary>
		/// <param name="collection">The collection that was modified</param>
		/// <param name="oldItem">The item that was removed/replaced.</param>
		/// <param name="newItem">The item that was added as a replacement for <paramref name="oldItem"/>.</param>
		/// <param name="index">The index of the item that was replaced which is also the index of the replacement.</param>
		/// <param name="itemTypeDisplayName">A string representing the type of items in the collection as they should be presented to the end user or null to use the type name and/or item.</param>
		/// <exception cref="ArgumentNullException">The <paramref name="collection"/> must not be null.</exception>
		/// <exception cref="ArgumentException">The <paramref name="collection"/> cannot be ReadOnly.</exception>
		public CollectionChangeUndoUnit(ICollection<T> collection, T oldItem, T newItem, int index, string itemTypeDisplayName)
			: this(collection, new Replacement(oldItem, newItem, index), itemTypeDisplayName)
		{
		}


		/// <summary>
		/// Initializes a new <see cref="CollectionChangeUndoUnit&lt;T&gt;"/> for a move of a specific item.
		/// </summary>
		/// <param name="collection">The collection that was modified</param>
		/// <param name="item">The item that was moved.</param>
		/// <param name="oldIndex">The previous index of the item.</param>
		/// <param name="newIndex">The new index of the item.</param>
		/// <param name="itemTypeDisplayName">A string representing the type of items in the collection as they should be presented to the end user or null to use the type name and/or item.</param>
		/// <exception cref="ArgumentNullException">The <paramref name="collection"/> must not be null.</exception>
		/// <exception cref="ArgumentException">The <paramref name="collection"/> cannot be ReadOnly.</exception>
		public CollectionChangeUndoUnit(ICollection<T> collection, T item, int oldIndex, int newIndex, string itemTypeDisplayName)
			: this(collection, new MoveItem(item, oldIndex, newIndex), itemTypeDisplayName)
		{
		}


		private CollectionChangeUndoUnit(ICollection<T> collection, CollectionChange change, string itemTypeDisplayName)
		{
			CoreUtilities.ValidateNotNull(collection);

			if (collection.IsReadOnly)
				throw new ArgumentException(Utils.GetString("LE_TargetCollectionIsReadOnly", collection));

			_collection = collection;
			_change = change;
			_itemTypeDisplayName = itemTypeDisplayName;
		} 
		#endregion //Constructor

		#region Base class overrides

		#region Execute
		/// <summary>
		/// Used to perform the associated action.
		/// </summary>
		/// <param name="executeInfo">Provides information about the undo/redo operation being executed.</param>
		/// <returns>Returns true if some action was taken. Otherwise false is returned. In either case the object was removed from the undo stack.</returns>
		protected internal override bool Execute(UndoExecuteContext executeInfo)
		{
			return _change != null && _change.Execute(_collection, executeInfo);
		}
		#endregion //Execute

		#region GetDescription
		/// <summary>
		/// Returns a string representation of the action based on whether this is for an undo or redo operation.
		/// </summary>
		/// <param name="itemType">The type of history for which the description is being requested.</param>
		/// <param name="detailed">A boolean indicating if a detailed description should be returned. For example, when false one may return "Typing" but for verbose one may return "Typing 'qwerty'".</param>
		public override string GetDescription(UndoHistoryItemType itemType, bool detailed)
		{
			if (_change == null)
				return null;

			return _change.GetDescription(_collection, itemType, detailed, _itemTypeDisplayName);
		}
		#endregion //GetDescription

		#region Merge
		/// <summary>
		/// Used to allow multiple consecutive undo units to be merged into a single operation.
		/// </summary>
		/// <param name="mergeInfo">Provides information about the unit to evaluate for a merge operation</param>
		/// <returns>Returns an enumeration used to provide identify how the unit was merged.</returns>
		protected internal override UndoMergeAction Merge(UndoMergeContext mergeInfo)
		{
			var other = mergeInfo.UnitBeingAdded as CollectionChangeUndoUnit<T>;

			if (other != null && other._collection == _collection && _change != null)
			{
				var singleThis = _change as AddRemoveSingle;
				var singleOther = other._change as AddRemoveSingle;

				if (singleThis != null && singleOther != null)
				{
					// if the change was an add (undo would be remove) followed by a remove 
					// (undo would be an add) of that item (without changes in between or 
					// this wouldn't be the head of the undo stack) then just remove the item
					if (singleThis.WasAdd == true &&
						singleThis.WasAdd != singleOther.WasAdd &&
						singleThis.Index == singleOther.Index &&
						EqualityComparer<T>.Default.Equals(singleThis.Item, singleOther.Item))
					{
						_change = null;
						return UndoMergeAction.MergedRemoveUnit;
					}
				}
			}

			return base.Merge(mergeInfo);
		}
		#endregion //Merge

		#region Target
		/// <summary>
		/// Returns the target <see cref="Collection"/>
		/// </summary>
		public override object Target
		{
			get { return _collection; }
		} 
		#endregion //Target

		#endregion //Base class overrides

		#region Properties

		#region Public Properties

		#region Collection
		/// <summary>
		/// Returns the associated collection that will be updated if the unit is executed.
		/// </summary>
		public ICollection<T> Collection
		{
			get { return _collection; }
		}
		#endregion //Collection

		#endregion //Public Properties

		#region Private Properties

		#region DebuggerDisplay
		private string DebuggerDisplay
		{
			get { return _change.ToString(); }
		}
		#endregion //DebuggerDisplay

		#endregion //Private Properties

		#endregion //Properties

		#region Methods

		#region CreateChange
		private static CollectionChange CreateChange(NotifyCollectionChangedEventArgs changedArgs)
		{
			CoreUtilities.ValidateNotNull(changedArgs, "changeArgs");

			CollectionChange change;

			switch (changedArgs.Action)
			{
				default:
				case NotifyCollectionChangedAction.Reset:
					throw new InvalidOperationException(Utils.GetString("LE_ResetCollectionAction"));

				case NotifyCollectionChangedAction.Move:
					{
						if (changedArgs.NewItems.Count != 1 || changedArgs.OldItems.Count != 1)
							throw new NotSupportedException(Utils.GetString("LE_RangeCollectionAction"));

						return new MoveItem((T)changedArgs.OldItems[0], changedArgs.OldStartingIndex, changedArgs.NewStartingIndex);
					}

				case NotifyCollectionChangedAction.Replace:
					{
						if (changedArgs.NewItems.Count != 1 || changedArgs.OldItems.Count != 1)
							throw new NotSupportedException(Utils.GetString("LE_RangeCollectionAction"));

						Debug.Assert(changedArgs.OldStartingIndex == changedArgs.NewStartingIndex, "Indices are different for a replace?");
						return new Replacement((T)changedArgs.OldItems[0], (T)changedArgs.NewItems[0], changedArgs.OldStartingIndex);
					}
				case NotifyCollectionChangedAction.Add:
					{
						if (changedArgs.NewItems.Count != 1)
							throw new NotSupportedException(Utils.GetString("LE_RangeCollectionAction"));

						T item = (T)changedArgs.NewItems[0];
						change = CreateChange(item, changedArgs.NewStartingIndex, true);
						break;
					}
				case NotifyCollectionChangedAction.Remove:
					{
						if (changedArgs.OldItems.Count != 1)
							throw new NotSupportedException(Utils.GetString("LE_RangeCollectionAction"));

						T item = (T)changedArgs.OldItems[0];
						change = CreateChange(item, changedArgs.OldStartingIndex, false);
						break;
					}
			}

			return change;
		}

		private static CollectionChange CreateChange(T item, int index, bool wasAdd)
		{
			return new AddRemoveSingle { Item = item, Index = index, WasAdd = wasAdd };
		}

		#endregion //CreateChange

		#region IsAddRemove
		private static bool IsAddRemove(NotifyCollectionChangedAction action)
		{
			if (action == NotifyCollectionChangedAction.Add)
				return true;
			else if (action == NotifyCollectionChangedAction.Remove)
				return false;

			throw new ArgumentException(Utils.GetString("LE_NeedAddRemoveAction"), "action");
		}
		#endregion //IsAddRemove

		#endregion //Methods

		#region CollectionChange class
		private abstract class CollectionChange
		{
			internal abstract bool Execute(ICollection<T> collection, UndoExecuteContext executeInfo);
			internal abstract string GetDescription(ICollection<T> collection, UndoHistoryItemType itemType, bool detailed, string itemTypeDisplayName);

			protected void ExecuteInTransaction(ICollection<T> collection, UndoExecuteContext executeInfo, Action action)
			{
				CoreUtilities.ValidateNotNull(action, "action");

				if (executeInfo.Reason == UndoExecuteReason.Rollback)
				{
					// if we're rolling back a transaction then we don't need a nested transaction
					action();
				}
				else
				{
					var itemType = executeInfo.Reason == UndoExecuteReason.Undo ? UndoHistoryItemType.Undo : UndoHistoryItemType.Redo;
					var ownerCollectionUnit = executeInfo.GetUnit(0) as CollectionChangeUndoUnit<T>;
					Debug.Assert(ownerCollectionUnit != null, "Should be called while executing the unit!");
					string itemTypeDisplayName = ownerCollectionUnit != null ? ownerCollectionUnit._itemTypeDisplayName : null;
					var desc = this.GetDescription(collection, itemType, false, itemTypeDisplayName);
					var descFull = this.GetDescription(collection, itemType, true, itemTypeDisplayName);
					executeInfo.UndoManager.ExecuteInTransaction(desc, descFull, action);
				}
			}
		} 
		#endregion //CollectionChange class

		#region AddRemoveSingle class
		private class AddRemoveSingle : CollectionChange
		{
			#region Member Variables

			internal T Item;
			internal int Index;
			internal bool WasAdd;

			#endregion //Member Variables

			#region Execute
			internal override bool Execute(ICollection<T> collection, UndoExecuteContext executeInfo)
			{
				var list = collection as IList<T>;
				int index = Index;

				if (WasAdd)
				{
					Debug.Assert(list == null || index < 0 || list.Count > index && EqualityComparer<T>.Default.Equals(list[index], Item), "List isn't back in the original state?");

					if (list != null && index >= 0 && list.Count > index && EqualityComparer<T>.Default.Equals(list[index], Item))
					{
						list.RemoveAt(index);
						return true;
					}
					else
					{
						return collection.Remove(Item);
					}
				}
				else
				{
					Debug.Assert(list == null || index < 0 || index <= list.Count, "Can't insert at the original spot");
					if (null != list && index >= 0 && index <= list.Count)
						list.Insert(index, Item);
					else
						collection.Add(Item);

					return true;
				}
			} 
			#endregion //Execute

			#region GetDescription
			internal override string GetDescription(ICollection<T> collection, UndoHistoryItemType itemType, bool detailed, string itemTypeDisplayName)
			{
				bool wasAdd = WasAdd;

				if (itemType == UndoHistoryItemType.Redo)
					wasAdd = !wasAdd;

				string resourceName = detailed
					? wasAdd ? "AddItemDescriptionDetailed" : "RemoveItemDescriptionDetailed"
					: wasAdd ? "AddItemDescription" : "RemoveItemDescription";

				return Utils.GetString(resourceName, collection, itemTypeDisplayName ?? (object)Item);
			} 
			#endregion //GetDescription

			#region ToString
			public override string ToString()
			{
				return string.Format("{0}: Index={0}, Item='{1}'", this.WasAdd ? "Add" : "Remove", this.Index, this.Item);
			} 
			#endregion //ToString
		} 
		#endregion //AddRemoveSingle class

		#region Replacement class
		private class Replacement : CollectionChange
		{
			#region Member Variables

			private T _oldItem;
			private T _newItem;
			private int _index;

			#endregion //Member Variables

			#region Constructor
			internal Replacement(T oldItem, T newItem, int index)
			{
				_oldItem = oldItem;
				_newItem = newItem;
				_index = index;
			}
			#endregion //Constructor

			#region Execute
			internal override bool Execute(ICollection<T> collection, UndoExecuteContext executeInfo)
			{
				var list = collection as IList<T>;

				// try to use list to do a replacement
				if (list != null)
				{
					int index = _index;

					Debug.Assert(index < 0 || (index < list.Count && EqualityComparer<T>.Default.Equals(_newItem, list[index])), "Collection out of sync from when the operation was performed?");

					// if the index was invalid or is now outside the range
					if (index < 0 || index >= list.Count || !EqualityComparer<T>.Default.Equals(_newItem, list[index]))
						index = list.IndexOf(_newItem);

					// if the new item isn't there then we can't really do a replacement
					if (index < 0)
						return false;

					list[index] = _oldItem;
				}
				else
				{
					this.ExecuteInTransaction(collection, executeInfo, () =>
					{
						if (collection.Remove(_newItem))
						{
							collection.Add(_oldItem);
						}
					});
				}

				return true;
			}
			#endregion //Execute

			#region GetDescription
			internal override string GetDescription(ICollection<T> collection, UndoHistoryItemType itemType, bool detailed, string itemTypeDisplayName)
			{
				string resourceName = detailed ? "ReplaceItemDescriptionDetailed" : "ReplaceItemDescription";
				T from = itemType == UndoHistoryItemType.Redo ? _newItem : _oldItem;
				T to = itemType == UndoHistoryItemType.Redo ? _oldItem : _newItem;

				return Utils.GetString(resourceName, collection, itemTypeDisplayName ?? (object)from, itemTypeDisplayName ?? (object)to);
			}
			#endregion //GetDescription

			#region ToString
			public override string ToString()
			{
				return string.Format("Replace: Index={0}, Old='{1}', New='{2}'", _index, _oldItem, _newItem);
			}
			#endregion //ToString
		} 
		#endregion //Replacement class

		#region MoveItem class

		private class MoveItem : CollectionChange
		{
			#region Member Variables

			private T _item;
			private int _oldIndex;
			private int _newIndex;

			#endregion //Member Variables

			#region Constructor
			internal MoveItem(T item, int oldIndex, int newIndex)
			{
				_item = item;
				_oldIndex = oldIndex;
				_newIndex = newIndex;
			}
			#endregion //Constructor

			#region Execute
			internal override bool Execute(ICollection<T> collection, UndoExecuteContext executeInfo)
			{
				var list = collection as IList<T>;

				// try to use list to do a replacement
				if (list != null)
				{
					int index = _newIndex;

					Debug.Assert(index < 0 || (index < list.Count && EqualityComparer<T>.Default.Equals(_item, list[index])), "Collection out of sync from when the operation was performed?");

					// if the index was invalid or is now outside the range
					if (index < 0 || index >= list.Count || !EqualityComparer<T>.Default.Equals(_item, list[index]))
						index = list.IndexOf(_item);

					// if the new item isn't there then we can't really do a replacement
					if (index < 0)
						return false;

					int newIndex = _oldIndex;

					if (newIndex >= list.Count)
						newIndex = list.Count - 1;

					var oc = collection as ObservableCollection<T>;

					if (oc != null)
						oc.Move(index, newIndex);
					else
					{
						this.ExecuteInTransaction(collection, executeInfo, () =>
						{
							list.RemoveAt(index);
							list.Insert(newIndex, _item);
						});
					}
				}
				else
				{
					return false;
				}

				return true;
			}
			#endregion //Execute

			#region GetDescription
			internal override string GetDescription(ICollection<T> collection, UndoHistoryItemType itemType, bool detailed, string itemTypeDisplayName)
			{
				string resourceName = detailed ? "MoveItemDescriptionDetailed" : "MoveItemDescription";
				int from = itemType == UndoHistoryItemType.Redo ? _newIndex : _oldIndex;
				int to = itemType == UndoHistoryItemType.Redo ? _oldIndex : _newIndex;

				return Utils.GetString(resourceName, collection, itemTypeDisplayName ?? (object)_item, from, to);
			}
			#endregion //GetDescription

			#region ToString
			public override string ToString()
			{
				return string.Format("Move: Old Index={0}, New Index={1}", _oldIndex, _newIndex);
			}
			#endregion //ToString
		}

		#endregion //MoveItem class

		#region AddRemoveBlock class
		private class AddRemoveBlock : CollectionChange
		{
			#region Member Variables

			private T[] _items;
			private int _index;
			private bool _wasAdd;

			#endregion //Member Variables

			#region Constructor
			internal AddRemoveBlock(T[] items, int index, bool wasAdd)
			{
				_items = items;
				_index = index;
				_wasAdd = wasAdd;
			} 
			#endregion //Constructor

			#region Execute
			internal override bool Execute(ICollection<T> collection, UndoExecuteContext executeInfo)
			{
				var list = collection as IList<T>;
				int index = _index;

				if (_wasAdd)
				{
					#region RemoveRange
					if (list != null)
					{
						// remove the range of items that were added
						bool isConsecutiveBlock = index + _items.Length <= list.Count;

						if (isConsecutiveBlock)
						{
							var comparer = EqualityComparer<T>.Default;

							for (int i = 0; i < _items.Length; i++)
							{
								if (!comparer.Equals(_items[i], list[index + i]))
								{
									isConsecutiveBlock = false;
									break;
								}
							}
						}

						if (isConsecutiveBlock)
						{

							var oc = list as ObservableCollectionExtended<T>;

							if (null != oc)
								oc.RemoveRange(index, _items.Length);
							else

							{
								this.ExecuteInTransaction(collection, executeInfo, () =>
								{
									for (int i = 0; i < _items.Length; i++)
										list.RemoveAt(index);
								});
							}
						}
						else
						{
							int[] indexes = new int[_items.Length];

							for (int i = 0; i < _items.Length; i++)
							{
								int currentIndex = list.IndexOf(_items[i]);

								if (currentIndex < 0)
									return false;

								indexes[i] = currentIndex;
							}

							Array.Sort(indexes);

							this.ExecuteInTransaction(collection, executeInfo, () =>
							{
								for (int i = indexes.Length - 1; i >= 0; i--)
									list.RemoveAt(indexes[i]);
							});
						}

						return true; 
					}
					else
					{
						int itemsRemoved = 0;

						this.ExecuteInTransaction(collection, executeInfo, () =>
						{
							foreach (var item in _items)
							{
								if (collection.Remove(item))
									itemsRemoved++;
							}
						});

						Debug.Assert(itemsRemoved == _items.Length, "The collection was not back in the original state such that every item was in the collection?");
						return itemsRemoved > 0;
					}
					#endregion //RemoveRange
				}
				else
				{
					#region InsertRange
					if (list != null)
					{
						Debug.Assert(index < 0 || index <= collection.Count, "Cannot insert at the original location");

						if (index > collection.Count)
							index = collection.Count;


						var oc = list as ObservableCollectionExtended<T>;

						if (null != oc)
							oc.InsertRange(index, _items);
						else

						{
							this.ExecuteInTransaction(collection, executeInfo, () =>
							{
								for (int i = 0; i < _items.Length; i++)
									list.Insert(index + i, _items[i]);
							});
						}

						return true;
					}
					else
					{
						this.ExecuteInTransaction(collection, executeInfo, () =>
						{
							foreach (var item in _items)
								collection.Add(item);
						});

						return true;
					} 
					#endregion //InsertRange
				}
			}
			#endregion //Execute

			#region GetDescription
			internal override string GetDescription(ICollection<T> collection, UndoHistoryItemType itemType, bool detailed, string itemTypeDisplayName)
			{
				bool wasAdd = _wasAdd;

				if (itemType == UndoHistoryItemType.Redo)
					wasAdd = !wasAdd;

				string resourceName = detailed
					? wasAdd ? "AddRangeDescriptionDetailed" : "RemoveRangeDescriptionDetailed"
					: wasAdd ? "AddRangeDescription" : "RemoveRangeDescription";

				return Utils.GetString(resourceName, collection, _items.Length, itemTypeDisplayName ?? typeof(T).Name);
			}
			#endregion //GetDescription

			#region ToString
			public override string ToString()
			{
				return string.Format("{0}Range: Count={1}", this._wasAdd ? "Add" : "Remove",  _items == null ? 0 : _items.Length);
			}
			#endregion //ToString
		}
		#endregion //AddRemoveBlock class

		#region ResetChange class
		private class ResetChange : CollectionChange
		{
			#region Member Variables

			private T[] _items;

			#endregion //Member Variables

			#region Constructor
			internal ResetChange(T[] items)
			{
				_items = items;
			}
			#endregion //Constructor

			#region Execute
			internal override bool Execute(ICollection<T> collection, UndoExecuteContext executeInfo)
			{

				var oc = collection as ObservableCollectionExtended<T>;

				if (null != oc)
				{
					oc.ReInitialize(_items);
				}
				else 

				if (_items == null || _items.Length == 0)
				{
					collection.Clear();
				}
				else
				{
					this.ExecuteInTransaction(collection, executeInfo, () =>
					{
						collection.Clear();

						var list = collection as List<T>;

						if (list != null)
						{
							list.AddRange(_items);
						}
						else
						{
							foreach (var item in _items)
								collection.Add(item);
						}
					});
				}

				return true;
			}
			#endregion //Execute

			#region GetDescription
			internal override string GetDescription(ICollection<T> collection, UndoHistoryItemType itemType, bool detailed, string itemTypeDisplayName)
			{
				string resourceName = detailed ? "ReinitializeCollectionDescriptionDetailed" : "ReinitializeCollectionDescription";
				return Utils.GetString(resourceName, collection, _items, itemTypeDisplayName ?? typeof(T).Name);
			}
			#endregion //GetDescription

			#region ToString
			public override string ToString()
			{
				return string.Format("Reset: Count={0}", _items == null ? 0 : _items.Length);
			}
			#endregion //ToString
		} 
		#endregion //ResetChange class
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