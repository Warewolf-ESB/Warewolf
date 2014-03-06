using System.Collections.Specialized;
using System;
using Infragistics.Collections;

namespace Infragistics.Controls.Grids
{
	/// <summary>
	/// A collection of <see cref="Column"/> objects which define how sorting will be applied to the 
	/// <see cref="XamGrid"/>
	/// </summary>
	/// <remarks>
	/// The order that the sort is applied is driven by the order of the SortedColumn objects in this collection.
	/// </remarks>
	public class SortedColumnsCollection : CollectionBase<Column>, IProvideCustomPersistence
	{
		#region Indexers
		/// <summary>
		/// Gets the <see cref="Column"/> from the collection based on the inputted key.
		/// </summary>
		/// <param propertyName="columnKey">The <see cref="Column"/> Key value that should be paired in the collection.</param>
		/// <returns>The SortedColumn with the matching key, null if not found.</returns>
		public Column this[string columnKey]
		{
			get
			{
				if (!string.IsNullOrEmpty(columnKey) || this.Count == 0)
				{
					foreach (Column sc in this)
					{
						if (sc.Key == columnKey)
							return sc;
					}
				}
				return null;
			}
		}
		#endregion // Indexers

		#region Methods

		#region AddItemSilently

		/// <summary>
		/// Adds a <see cref="Column"/> to the collection without setting it's IsSorted property.
		/// </summary>
		/// <param propertyName="item"></param>
		protected internal virtual void AddItemSilently(Column item)
		{
			if (!this.Items.Contains(item))
				this.AddItemSilently(this.Count, item);
		}

		#endregion // AddItemSilently

        #region InsertItemSilently

        /// <summary>
        /// Adds a <see cref="Column"/> to the collection without setting it's IsSorted property.
        /// </summary>
        /// <param propertyName="item"></param>
        protected internal virtual void InsertItemSilently(Column item, int index)
        {
            if (!this.Items.Contains(item))
                this.AddItemSilently(index, item);           
        }

        #endregion // InsertItemSilently


        #region RemoveItemSilently

        /// <summary>
		/// Removes a <see cref="Column"/> from the collection without sett it's IsSorted property.
		/// </summary>
		/// <param propertyName="item"></param>
		protected internal virtual void RemoveItemSilently(Column item)
		{
			int index = this.Items.IndexOf(item);
			if (index != -1)
			{
				this.RemoveItemSilently(index);
			}
		}

		#endregion // RemoveItemSilently

		#region ClearSilently

		/// <summary>
		/// Removes a <see cref="Column"/> from the collection setting it's IsSorted property silently.
		/// </summary>	
		protected internal virtual void ClearSilently()
		{
			int count = this.Items.Count;
			for (int i = count - 1; i >= 0; i--)
			{
				Column c = this.Items[i];
				c.SetSortedColumnStateSilent(SortDirection.None);
				this.RemoveItemSilently(c);
			}
		}

		#endregion // ClearSilently

		#region Save

		/// <summary>
		/// Gets the string representation of the object, that can be later be passed into the Load method of this object, in order to rehydrate.
		/// </summary>
		/// <returns></returns>
		protected virtual string Save()
		{
			string val = "";

			foreach (Column col in this)
			{
				val += col.Key + ":" + col.IsSorted + ",";
			}
			return val;
		}

		#endregion // Save

		#region Load

		/// <summary>
		/// Takes the string that was created in the Save method, and rehydrates the object. 
		/// </summary>
		/// <param name="owner">This is the object who owns this object as a property.</param>
		/// <param name="value"></param>
		protected virtual void Load(object owner, string value)
		{
			if (value != null)
			{
				SortingSettingsOverride settings = owner as SortingSettingsOverride;

				if (settings != null && settings.ColumnLayout != null)
				{
                    this.Clear();

					string[] cols = ((string)value).Split(',');

					foreach (string col in cols)
					{
						if (col.Length > 0)
						{
							string[] keyPair = col.Split(':');

							if (keyPair.Length == 2)
							{
								Column column = settings.ColumnLayout.Columns.DataColumns[keyPair[0]];
								if (column != null)
								{
									column.IsSorted = (SortDirection)Enum.Parse(typeof(SortDirection), keyPair[1], true);
								}
							}
						}
					}
				}
			}
		}

		#endregion // Load

		#endregion // Methods

		#region Overrides

		#region RemoveItem

		/// <summary>
		/// Removes the item at the specified index.
		/// </summary>
		/// <param propertyName="index"></param>
		/// <returns></returns>
		protected override bool RemoveItem(int index)
		{
			Column item = this.Items[index];
			bool retVal = item.SetSortedColumnState(SortDirection.None);
			this.OnNotifyCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
			return retVal;
		}

		#endregion // RemoveItem

		#region ResetItems

		/// <summary>
		/// Removes all items from the collection.
		/// </summary>
		protected override void ResetItems()
		{
			int count = this.Items.Count;
			for (int i = count - 1; i >= 0; i--)
				this.RemoveItem(i);
			if (count > 0)
				this.OnNotifyCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		#endregion // ResetItems

		#region AddItem

		/// <summary>
		/// Adds the item at the specified index. 
		/// </summary>
		/// <param propertyName="index"></param>
		/// <param propertyName="item"></param>
		protected override void AddItem(int index, Column item)
		{
			if (item != null && item.CanBeSorted)
			{
				item.SetSortedColumnState(item.NextSortDirection);
				this.OnNotifyCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
			}
		}

		#endregion // AddItem

		#region InsertItem

		/// <summary>
		/// Adds the item at the specified index. 
		/// </summary>
		/// <param propertyName="index"></param>
		/// <param propertyName="item"></param>
        protected override void InsertItem(int index, Column item)
        {
            if (item != null)
            {
                item.SetSortedColumnStateInternally(item.NextSortDirection, true, index);
                this.OnNotifyCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
            }
        }

		#endregion // InsertItem

		#endregion // Overrides

		#region IProvideCustomPersistence Members

		string IProvideCustomPersistence.Save()
		{
			return this.Save();
		}

		void IProvideCustomPersistence.Load(object owner, string value)
		{
			this.Load(owner, value);
		}

		#endregion
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