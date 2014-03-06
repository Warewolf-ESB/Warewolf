using System;
using System.Collections.Generic;
using System.Text;
using Infragistics.Windows.Helpers;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Collections.ObjectModel;
using Infragistics.Collections;

namespace Infragistics.Windows.DockManager
{
	internal sealed class QueuedObservableCollection<T> : ObservableCollectionExtended<T>
	{
		#region Member Variables

		private Dictionary<T, int> _changedItems;

		#endregion //Member Variables

		#region Constructor
		public QueuedObservableCollection()
		{
			this._changedItems = new Dictionary<T, int>();
		}
		#endregion //Constructor

		#region Base class overrides
		protected override bool NotifyItemsChanged
		{
			get
			{
				return true;
			}
		}

		protected override void OnItemAdded(T itemAdded)
		{
			this.AdjustChangeCount(itemAdded, +1);
			base.OnItemAdded(itemAdded);
		}

		protected override void OnItemRemoved(T itemRemoved)
		{
			this.AdjustChangeCount(itemRemoved, -1);
			base.OnItemRemoved(itemRemoved);
		}

		protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			// FUTURE possibly snapshot the collection at a beginupdate and not raise the event if the collection is the same.

			e = new QueuedNotifyCollectionChangedEventArgs(GetChangedItems(true), GetChangedItems(false));

			this._changedItems.Clear();

			base.OnCollectionChanged(e);
		}
		#endregion //Base class overrides

		#region Private Methods
		private void AdjustChangeCount(T item, int offset)
		{
			Debug.Assert(item != null);

			if (null != item)
			{
				int changeCount;

				if (this._changedItems.TryGetValue(item, out changeCount))
					changeCount += offset;
				else
					changeCount = offset;

				this._changedItems[item] = changeCount;
			}
		}

		private IList<T> GetChangedItems(bool added)
		{
			IList<T> list = new List<T>();

			foreach (KeyValuePair<T, int> pair in this._changedItems)
			{
				if ((added && pair.Value > 0) || (false == added && pair.Value < 0))
					list.Add(pair.Key);
			}

			return list;
		}
		#endregion //Private Methods

		#region QueuedNotifyCollectionChangedEventArgs
		internal class QueuedNotifyCollectionChangedEventArgs : NotifyCollectionChangedEventArgs
		{
			#region Member Variables

			private IList<T> _itemsAdded;
			private IList<T> _itemsRemoved;

			#endregion //Member Variables

			#region Constructor
			internal QueuedNotifyCollectionChangedEventArgs(IList<T> itemsAdded, IList<T> itemsRemoved)
				: base(NotifyCollectionChangedAction.Reset)
			{
				this._itemsAdded = new ReadOnlyCollection<T>(itemsAdded);
				this._itemsRemoved = new ReadOnlyCollection<T>(itemsRemoved);
			}

			#endregion //Constructor

			#region Properties
			public IList<T> ItemsAdded
			{
				get { return this._itemsAdded; }
			}

			public IList<T> ItemsRemoved
			{
				get { return this._itemsRemoved; }
			}

			#endregion //Properties
		}
		#endregion //QueuedNotifyCollectionChangedEventArgs
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