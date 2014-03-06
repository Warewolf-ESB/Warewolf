using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Infragistics.Undo
{
	internal class StackList<T> : IEnumerable<T>
		, ICollection
		, INotifyCollectionChanged
		, IList<T>
		, INotifyPropertyChanged
	{
		#region Member Variables

		internal const int DefaultMaxCapacity = 0;
		private int _maxCapacity;
		private List<T> _stack;
		private int _version;

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="StackList&lt;T&gt;"/>
		/// </summary>
		/// <param name="maxCapacity">The maximum number of items to store</param>
		public StackList(int maxCapacity = DefaultMaxCapacity)
		{
			this.MaxCapacity = maxCapacity;

			_stack = new List<T>();
		}
		#endregion //Constructor

		#region Properties

		#region Count
		/// <summary>
		/// Returns the number of items in the stack.
		/// </summary>
		public int Count
		{
			get
			{
				return _stack.Count;
			}
		} 
		#endregion //Count

		#region MaxCapacity
		/// <summary>
		/// Returns or sets the maximum number of items that may be stored in the stack.
		/// </summary>
		public int MaxCapacity
		{
			get { return _maxCapacity; }
			set
			{
				if (value != _maxCapacity)
				{
					CoreUtilities.ValidateIsNotNegative(value, "value");

					this._maxCapacity = value > 0 ? value : int.MaxValue;

					if (null != _stack && _stack.Count > _maxCapacity)
					{
						_stack.RemoveRange(0, _stack.Count - _maxCapacity);
						_version++;

						this.OnCollectionReset();
					}
				}
			}
		} 
		#endregion //MaxCapacity

		#endregion //Properties

		#region Methods

		#region Clear
		/// <summary>
		/// Clears the stack
		/// </summary>
		public void Clear()
		{
			if (this.Count == 0)
				return;

			_stack.Clear();
			_version++;

			this.OnCollectionReset();
		} 
		#endregion //Clear

		#region GetEnumerator
		/// <summary>
		/// Returns an enumerator for the stack
		/// </summary>
		/// <returns></returns>
		public IEnumerator<T> GetEnumerator()
		{
			return new Enumerator(this);
		} 
		#endregion //GetEnumerator

		#region GetItem
		/// <summary>
		/// Returns the specified item that is <paramref name="offset"/> number of items from the top of the stack.
		/// </summary>
		/// <param name="offset">The number of items offset from the top of the stack from which the item should be obtained</param>
		/// <returns>An item offset by the specified amount from the top of the stack</returns>
		internal T GetItem(int offset)
		{
			return _stack[_stack.Count - ++offset];
		} 
		#endregion //GetItem

		#region OnAddOrRemove
		private void OnAddOrRemove(T item, int index, bool isAdd)
		{
			var propHandler = this.PropertyChanged;

			if (null != propHandler)
			{
				propHandler(this, new PropertyChangedEventArgs("Count"));
				propHandler(this, new PropertyChangedEventArgs("Item[]"));
			}

			var handler = this.CollectionChanged;

			if (null != handler)
			{
				var args = new NotifyCollectionChangedEventArgs(isAdd ? NotifyCollectionChangedAction.Add : NotifyCollectionChangedAction.Remove, item, index);
				handler(this, args);
			}
		} 
		#endregion //OnAddOrRemove

		#region OnCollectionReset
		private void OnCollectionReset()
		{
			var propHandler = this.PropertyChanged;

			if (null != propHandler)
			{
				propHandler(this, new PropertyChangedEventArgs("Count"));
				propHandler(this, new PropertyChangedEventArgs("Item[]"));
			}

			var handler = this.CollectionChanged;

			if (null != handler)
				handler(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		} 
		#endregion //OnCollectionReset

		#region Peek
		/// <summary>
		/// Returns the most recent item added to the stack without removing it.
		/// </summary>
		/// <returns>Returns the last item added to the stack</returns>
		/// <exception cref="InvalidOperationException">The 'Count' must be 1 or greater.</exception>
		public T Peek()
		{
			if (this.Count == 0)
				throw new InvalidOperationException();

			return _stack[this.Count - 1];
		} 
		#endregion //Peek

		#region Pop
		/// <summary>
		/// Removes the most recent item from the stack and returns it.
		/// </summary>
		/// <returns>The most recent item on the stack.</returns>
		public T Pop()
		{
			T action = this.Peek();
			_stack.RemoveAt(this.Count - 1);

			_version++;

			// when enumerating the 1st item is the last one pushed on so popping should return 0
			this.OnAddOrRemove(action, 0, false);

			return action;
		} 
		#endregion //Pop

		#region Push
		/// <summary>
		/// Adds a new item to the top of the stack.
		/// </summary>
		/// <param name="item">The item to add to the stack</param>
		public void Push(T item)
		{
			_version++;

			if (_maxCapacity > 0 && this.Count == _maxCapacity)
			{
				T old = _stack[0];
				_stack.RemoveAt(0);
				this.OnAddOrRemove(old, _maxCapacity - 1, false);
			}

			_stack.Add(item);
			this.OnAddOrRemove(item, 0, true);
		} 
		#endregion //Push

		#region RemoveAll
		internal void RemoveAll(Func<T, bool> match)
		{
			CoreUtilities.ValidateNotNull(match, "match");
			int version = _version;

			for (int i = _stack.Count - 1; i >= 0; i--)
			{
				T item = _stack[i];

				if (match(item))
				{
					_stack.RemoveAt(i);

					// raise a notification
					this.OnAddOrRemove(item, _stack.Count - i, false);

					if (version != _version)
						throw new InvalidOperationException(Utils.GetString("LE_RemoveAllFailedVersion"));
				}
			}
		}
		#endregion //RemoveAll

		#endregion //Methods

		#region IList<T> Members
		int IList<T>.IndexOf(T item)
		{
			int index = _stack.IndexOf(item);

			if (index >= 0)
				index = _stack.Count - ++index;

			return index;
		}

		void IList<T>.Insert(int index, T item)
		{
			throw new NotSupportedException();
		}

		void IList<T>.RemoveAt(int index)
		{
			throw new NotSupportedException();
		}

		T IList<T>.this[int index]
		{
			get
			{
				return this.GetItem(index);
			}
			set
			{
				throw new NotSupportedException();
			}
		} 
		#endregion //IList<T> Members

		#region ICollection<T> Members
		void ICollection<T>.Add(T item)
		{
			this.Push(item);
		}

		void ICollection<T>.Clear()
		{
			this.Clear();
		}

		bool ICollection<T>.Contains(T item)
		{
			return _stack.IndexOf(item) >= 0;
		}

		void ICollection<T>.CopyTo(T[] array, int arrayIndex)
		{
			var list = _stack;

			for (int i = list.Count - 1; i >= 0; i--)
				array[arrayIndex++] = list[i];
		}

		int ICollection<T>.Count
		{
			get { return this.Count; }
		}

		bool ICollection<T>.IsReadOnly
		{
			get { return false; }
		}

		bool ICollection<T>.Remove(T item)
		{
			throw new NotSupportedException();
		} 
		#endregion //ICollection<T> Members

		#region ICollection Members
		void ICollection.CopyTo(Array array, int index)
		{
			((ICollection)_stack).CopyTo(array, index);
		}

		bool ICollection.IsSynchronized
		{
			get { return false; }
		}

		object ICollection.SyncRoot
		{
			get { return ((ICollection)_stack).SyncRoot; }
		} 
		#endregion //ICollection Members

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		#endregion //IEnumerable

		#region INotifyCollectionChanged Members
		/// <summary>
		/// Invoked after the collection has been changed
		/// </summary>
		public event NotifyCollectionChangedEventHandler CollectionChanged; 
		#endregion //INotifyCollectionChanged Members

		#region INotifyPropertyChanged Members

		public event PropertyChangedEventHandler PropertyChanged; 

		#endregion //INotifyPropertyChanged Members

		#region Enumerator class
		private class Enumerator : IEnumerator<T>
		{
			private const int EnumeratorEnded = -1;
			private const int EnumeratorNotStarted = -2;

			private StackList<T> _stack;
			private int _version;
			private int _index;
			private T _current;

			internal Enumerator(StackList<T> stack)
			{
				_stack = stack;
				this.Reset();
			}

			public bool MoveNext()
			{
				if (_version != _stack._version)
					throw new InvalidOperationException(Utils.GetString("LE_EnumFailedVersion"));

				if (_index == EnumeratorEnded)
					return false;

				if (_index == EnumeratorNotStarted)
					_index = _stack.Count;

				_index--;

				if (_index >= 0)
				{
					_current = _stack._stack[_index];
					return true;
				}

				_current = default(T);
				return false;
			}

			public T Current
			{
				get
				{
					if (_index == EnumeratorNotStarted)
						throw new InvalidOperationException(Utils.GetString("LE_EnumNotStarted"));
					else if (_index == EnumeratorEnded)
						throw new InvalidOperationException(Utils.GetString("LE_EnumEnded"));

					return _current;
				}
			}

			public void Reset()
			{
				_version = _stack._version;
				_index = EnumeratorNotStarted;
				_current = default(T);
			}

			#region IDisposable

			void IDisposable.Dispose()
			{
				_index = EnumeratorEnded;
			}

			#endregion //IDisposable

			#region IEnumerator

			object System.Collections.IEnumerator.Current
			{
				get { return this.Current; }
			}

			#endregion //IEnumerator
		}
		#endregion //Enumerator class
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