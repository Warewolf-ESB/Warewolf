using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Diagnostics;
using Infragistics.Collections;
using Infragistics.Controls.Schedules.Primitives;

namespace Infragistics.Controls.Schedules
{
	internal class TimeslotCollection : IList<TimeslotBase>
		, ICreateItemCallback
	{
		#region Member Variables

		private SparseArray _list;
		private Func<DateTime, DateTime, TimeslotBase> _creatorFunc;
		private TimeslotRange[] _groupTemplates;
		private Action<TimeslotBase> _initializer;
		private Func<DateTime, DateTime> _modifyDateFunc;

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="TimeslotCollection"/>
		/// </summary>
		/// <param name="creatorFunc">The delegate invoked when a TimeslotBase is to be allocated</param>
		/// <param name="modifyDateFunc">Optional method used to adjust a datetime for a timeslot</param>
		/// <param name="groupTemplates">Array of objects that provide the time information for the timeslots</param>
		/// <param name="initializer">The delegate invoked when an item is created or Reinitialize is invoked. This is meant to be used to update state on a given timeslot</param>
		internal TimeslotCollection(
			Func<DateTime, DateTime, TimeslotBase> creatorFunc, 
			Func<DateTime, DateTime> modifyDateFunc,
			TimeslotRange[] groupTemplates, 
			Action<TimeslotBase> initializer)
		{


#region Infragistics Source Cleanup (Region)










#endregion // Infragistics Source Cleanup (Region)


			Debug.Assert(creatorFunc != null);

			_list = new SparseArray(20, 0.5f, true);
			_groupTemplates = groupTemplates;
			_creatorFunc = creatorFunc;
			_initializer = initializer;
			_modifyDateFunc = modifyDateFunc;

			long totalCount = 0;

			foreach (var item in groupTemplates)
			{
				totalCount += item.TimeslotCount;
			}

			_list.Expand((int)totalCount);

		} 
		#endregion //Constructor

		#region Properties

		#region Public
		/// <summary>
		/// Returns the timeslot at the specified index.
		/// </summary>
		/// <param name="index">The index of the item to return</param>
		/// <returns></returns>
		public TimeslotBase this[int index]
		{
			get
			{
				return _list.GetItem(index, this) as TimeslotBase;
			}
		}

		/// <summary>
		/// Returns the number of items in the collection.
		/// </summary>
		public int Count
		{
			get { return _list.Count; }
		}

		#endregion // Public

		#region Internal
		internal Func<DateTime, DateTime, TimeslotBase> CreatorFunc
		{
			get { return _creatorFunc; }
		}

		internal TimeslotRange[] GroupTemplates
		{
			get { return _groupTemplates; }
		}

		internal Action<TimeslotBase> Initializer
		{
			get { return _initializer; }
		}

		internal Func<DateTime, DateTime> ModifyDateFunc
		{
			get { return _modifyDateFunc; }
		}

		internal IEnumerable NonNullItems
		{
			get { return _list.NonNullItems; }
		}

		internal TimeSpan TimeslotInterval
		{
			get
			{
				if (_groupTemplates.Length == 0)
					return TimeSpan.FromMinutes(15);

				return _groupTemplates[0].TimeslotInterval;
			}
		}
		#endregion // Internal

		#endregion //Properties

		#region Methods

		#region Public Methods

		#region BinarySearch
		/// <summary>
		/// Returns the index of the specified item.
		/// </summary>
		/// <param name="date">The date to look for.</param>
		/// <returns>The index of the </returns>
		public int BinarySearch(DateTime date)
		{
			int si = 0, ei = this.Count - 1;
			int mi = 0;

			while (si <= ei)
			{
				mi = (si + ei) / 2;

				DateRange? range = this.GetDateRange(mi);

				if (range.Value.Start > date)
					ei = mi - 1;
				else if (range.Value.End <= date) // note we're doing <= because the end date is exclusive
					si = mi + 1;
				else
					return mi;
			}

			return ~si;
		}
		#endregion // BinarySearch

		#region Contains
		/// <summary>
		/// Returns a boolean indicating if the specified item exists in the collection.
		/// </summary>
		/// <param name="item">The item to check</param>
		/// <returns>True if the item exists in the collection otherwise false</returns>
		public bool Contains(TimeslotBase item)
		{
			return _list.Contains(item);
		} 
		#endregion // Contains

		#region CopyTo
		/// <summary>
		/// Copies the items to the specified array.
		/// </summary>
		/// <param name="array">The array to update</param>
		/// <param name="arrayIndex">The starting index at which to copy the items</param>
		public void CopyTo(TimeslotBase[] array, int arrayIndex)
		{
			_list.CopyTo(array, arrayIndex, this);
		} 
		#endregion // CopyTo

		#region GetDateRange
		/// <summary>
		/// Returns a DateRange that represents the <see cref="TimeslotBase.Start"/> and <see cref="TimeslotBase.End"/> of the <see cref="TimeslotBase"/> at the specified index.
		/// </summary>
		/// <param name="index">The index of the timeslot in the collection whose date range is being requested</param>
		/// <returns>The date range for the timeslot at the specified index</returns>
		public DateRange? GetDateRange(int index)
		{
			// if its allocated then just get its information
			TimeslotBase timeslot = _list.GetItem(index, null) as TimeslotBase;
			
			if (null != timeslot)
			    return new DateRange(timeslot.Start, timeslot.End);
			
			return CalculateDateRange(index, true);
		}
		#endregion // GetDateRange

		#region IndexOf
		/// <summary>
		/// Returns the index of the specified item
		/// </summary>
		/// <param name="item">The item to locate</param>
		/// <returns>The index of the item or -1 if it is not in the collection.</returns>
		public int IndexOf(TimeslotBase item)
		{
			return _list.IndexOf(item);
		} 
		#endregion // IndexOf

		#endregion // Public Methods

		#region Internal Methods

		#region CalculateDateRange
		/// <summary>
		/// Returns the date range for the specified timeslot index
		/// </summary>
		/// <param name="index">Index of the timeslot</param>
		/// <param name="useModifyDateFunc">True to return the range taking the modify date function provided to the collection</param>
		/// <returns>The range for that index</returns>
		internal DateRange? CalculateDateRange(int index, bool useModifyDateFunc)
		{
			if (index < 0 || index >= this.Count)
				return null;

			return ScheduleUtilities.CalculateDateRange(_groupTemplates, index, useModifyDateFunc ? _modifyDateFunc : null);
		}

		#endregion // CalculateDateRange

		#region Reinitialize
		internal void Reinitialize()
		{
			if (null != _initializer)
			{
				foreach (TimeslotBase timeslot in _list.NonNullItems)
				{
					this.InitializeItem(timeslot);
				}
			}
		}
		#endregion // Reinitialize

		#endregion // Internal Methods

		#region Private Methods

		#region InitializeItem
		private void InitializeItem(TimeslotBase timeslot)
		{
			if (_initializer != null)
				_initializer(timeslot);
		}
		#endregion // InitializeItem

		#endregion // Private Methods

		#endregion //Methods

		#region IList<TimeslotBase> Members

		void IList<TimeslotBase>.Insert(int index, TimeslotBase item)
		{
			CoreUtilities.RaiseReadOnlyCollectionException();
		}

		void IList<TimeslotBase>.RemoveAt(int index)
		{
			CoreUtilities.RaiseReadOnlyCollectionException();
		}

		TimeslotBase IList<TimeslotBase>.this[int index]
		{
			get
			{
				return _list.GetItem(index, this) as TimeslotBase;
			}
			set
			{
				CoreUtilities.RaiseReadOnlyCollectionException();
			}
		}

		#endregion //IList<TimeslotBase> Members

		#region ICollection<TimeslotBase> Members

		void ICollection<TimeslotBase>.Add(TimeslotBase item)
		{
			CoreUtilities.RaiseReadOnlyCollectionException();
		}

		void ICollection<TimeslotBase>.Clear()
		{
			CoreUtilities.RaiseReadOnlyCollectionException();
		}

		/// <summary>
		/// Returns true to indicate that the list is read-only
		/// </summary>
		bool ICollection<TimeslotBase>.IsReadOnly
		{
			get { return true; }
		}

		bool ICollection<TimeslotBase>.Remove(TimeslotBase item)
		{
			CoreUtilities.RaiseReadOnlyCollectionException();
			return false;
		}

		#endregion //ICollection<TimeslotBase> Members

		#region IEnumerable<TimeslotBase> Members

		IEnumerator<TimeslotBase> IEnumerable<TimeslotBase>.GetEnumerator()
		{
			return new TypedEnumerable<TimeslotBase>.Enumerator(_list.GetEnumerator(this));
		}

		#endregion //IEnumerable<TimeslotBase> Members

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return _list.GetEnumerator(this);
		}

		#endregion //IEnumerable Members

		#region ICreateItemCallback Members

		object ICreateItemCallback.CreateItem(SparseArray array, int relativeIndex)
		{
			DateRange? range = this.CalculateDateRange(relativeIndex, true);

			if (null != range)
			{
				TimeslotBase timeslot = _creatorFunc(range.Value.Start, range.Value.End);

				this.InitializeItem(timeslot);

				return timeslot;
			}

			Debug.Assert(false, "Index outside range?");
			return null;
		}

		#endregion // ICreateItemCallback Members
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