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

namespace Infragistics.Controls.Grids
{
	/// <summary>
	/// A Collection of RowFilter objects which will be used to limit the amount of data currently visible.
	/// </summary>
	public class RowFiltersCollection : RecordFilterCollection
	{
		#region Methods

		#region Protected

		#region GetRowFilterByColumnKey

		/// <summary>
		/// Returns a RowFilter by its Key value.
		/// </summary>
		/// <param propertyName="columnKey"></param>
		/// <returns></returns>
		public RowsFilter GetRowFilterByColumnKey(string columnKey)
		{
			foreach (IRecordFilter r in this.Items)
			{
				if (r.FieldName == columnKey)
					return r as RowsFilter;
			}
			return null;
		}

		#endregion // GetRowFilterByColumnKey

		#endregion // Protected

		#endregion // Methods

		#region Indexer

		/// <summary>
		/// Accesses the members of the collection via the Key value.
		/// </summary>
		/// <param propertyName="key"></param>
		/// <returns></returns>
		public RowsFilter this[string key]
		{
			get
			{
				return this.GetRowFilterByColumnKey(key);
			}
		}

		#endregion // Indexer

		#region Events

		#region OnItemAdded

		/// <summary>
		/// Invoked when a <see cref="IRecordFilter"/> is added at the specified index.
		/// </summary>
		/// <param propertyName="index"></param>
		/// <param propertyName="item"></param>
		protected override void OnItemAdded(int index, IRecordFilter item)
		{
            base.OnItemAdded(index, item);
            item.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(Item_PropertyChanged);
		}

		#endregion // OnItemAdded

        #region AddItem

        /// <summary>
        /// Adds the item at the specified index. 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        protected override void AddItem(int index, IRecordFilter item)
        {
            RowsFilter filter = item as RowsFilter;

            if (filter == null || filter.Column == null || filter.Column.CanBeFiltered)
            {
                base.AddItem(index, item);
            }
        }
        #endregion // AddItem

        #region OnItemRemoved

        /// <summary>
		/// Invoked when a <see cref="IRecordFilter"/> is removed at the specified index.
		/// </summary>
		/// <param propertyName="index"></param>
		/// <param propertyName="item"></param>
		protected override void OnItemRemoved(int index, IRecordFilter item)
		{
			base.OnItemRemoved(index, item);
			item.PropertyChanged -= Item_PropertyChanged;
		}
		#endregion // OnItemRemoved

		#region ResetItems
		/// <summary>
		/// Removes all items from the collection.
		/// </summary>
		protected override void ResetItems()
		{
			foreach (IRecordFilter item in this.Items)
			{
				item.PropertyChanged -= Item_PropertyChanged;
			}

			base.ResetItems();
		}
		#endregion // ResetItems


		#endregion // Events

		#region Event Handlers

		void Item_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			this.OnCollectionItemChanged();
		}

		#endregion // Event Handlers
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