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
using System.Collections.Specialized;
using System.Collections.Generic;

namespace Infragistics.Collections
{
	/// <summary>
	/// A Collection that contains items that are bound to a DataSource. 
	/// Note: all items might not exist in the collection at a given time, as it only pulls down items as they're requested.
	/// </summary>
	/// <typeparam name="T"></typeparam>
    public class BindableItemCollection<T> : CollectionBase<T> where T : IBindableItem
    {
        #region Members

        IProvideDataItems<T> _owner;

        #endregion // Members

        #region Constructor
		/// <summary>
		/// Initializes a new instance of the <see cref="BindableItemCollection&lt;T&gt;"/> class.
		/// </summary>
		/// <param name="owner">The object that will actually be providing the items of the collection.</param>
        public BindableItemCollection(IProvideDataItems<T> owner)
        {
            this._owner = owner;
        }
        #endregion // Constructor

        #region Overrides

		#region GetCount

		/// <summary>
		/// Retrieves the amount of items in the collection.
		/// </summary>
		/// <returns></returns>
        protected override int GetCount()
        {
            int count = 0;
            if (this._owner != null)
                count = this._owner.DataCount;
			return count;
		}

		#endregion // GetCount

		#region GetItem
		/// <summary>
		/// Gets the item at the specified index.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		protected override T GetItem(int index)
        {
            if (index > -1 && index < this.Count)
            {
                T obj = base.GetItem(index);
                if (obj == null && this._owner != null)
                {
                    obj = (T)this._owner.GetDataItem(index);
                    if (obj != null)
                        obj.IsDataBound = true;
                    this.AddBoundItem(index, obj);
                }
                return obj;
            }
            return default(T);
		}
		#endregion // GetItem

		#region AddBoundItem

		/// <summary>
		/// Adds the item at the specified index. 
		/// </summary>
		/// <param name="index"></param>
		/// <param name="item"></param>
		protected void AddBoundItem(int index, T item)
        {
            this.AddItemSilently(index, item);
		}
		#endregion // AddBoundItem

		#region AddItem

		/// <summary>
		/// Adds the unbound item at the specified index. 
		/// </summary>
		/// <param name="index"></param>
		/// <param name="item"></param>
		protected override void AddItem(int index, T item)
		{
			this._owner.AddItem(item);
		}
		#endregion // AddItem

		#region InsertItem
		/// <summary>
		/// Adds an item to the collection at a given index.
		/// </summary>
		/// <param name="index"></param>
		/// <param name="item"></param>
		protected override void InsertItem(int index, T item)
		{
			this._owner.InsertItem(index, item);
		}
		#endregion // InsertItem

		#region RemoveItem

		/// <summary>
		/// Removes the item at the specified index
		/// </summary>
		/// <param name="index"></param>
		/// <returns>True if the item was removed.</returns>
		protected override bool RemoveItem(int index)
		{
			return this._owner.RemoveItem(this.GetItem(index));
		}
		#endregion // RemoveItem

		#endregion // Overrides

		#region Methods 

		#region Public

		#region CreateItem

		/// <summary>
		/// Creates a new object with a default underlying data object.
		/// </summary>
		/// <returns></returns>
		public T CreateItem()
		{
			return this._owner.CreateItem();
		}

		/// <summary>
		/// Creates a new object using the inputted data object.
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public T CreateItem(object data)
		{
			return this._owner.CreateItem(data);
		}

		#endregion // CreateItem

		#endregion // Public

		#region Protected

		#region RemoveRange

		/// <summary>
		/// Removes the specified list of items.
		/// </summary>
		/// <param name="itemsToRemove"></param>
		protected internal virtual void RemoveRange(IList<T> itemsToRemove)
		{
			this._owner.RemoveRange(itemsToRemove);
		}
		#endregion // RemoveRange

		#endregion // Protected

		#endregion // Methods
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