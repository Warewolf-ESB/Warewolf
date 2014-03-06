using System;
using System.Linq.Expressions;
using System.ComponentModel;
using Infragistics.Collections;

namespace Infragistics
{
	/// <summary>
	/// A CollectionBase of <see cref="IRecordFilter"/> objects which combine to give the the current filter.
	/// </summary>
	public class RecordFilterCollection : CollectionBase<IRecordFilter>, IGroupFilterConditions, INotifyPropertyChanged
	{
		#region Properties

		#region Members
		LogicalOperator _logicalOperator = LogicalOperator.And;
		#endregion // Members

		#region LogicalOperator
		/// <summary>
		/// The <see cref="LogicalOperator"/> which will be used to combine all the terms in the <see cref="RecordFilterCollection"/>.
		/// </summary>
		public LogicalOperator LogicalOperator
		{
			get
			{
				return this._logicalOperator;
			}
			set
			{
				if (this._logicalOperator != value)
				{
					this._logicalOperator = value;
					this.OnPropertyChanged("LogicalOperator");
				}
			}
		}
		#endregion // LogicalOperator

		#endregion // Properties

		#region IGroupFilterConditions Members

		LogicalOperator IGroupFilterConditions.LogicalOperator
		{
			get { return this.LogicalOperator; }
		}

		Expression IExpressConditions.GetCurrentExpression(FilterContext context)
		{
			return this.GetCurrentExpression(context);
		}

		Expression IExpressConditions.GetCurrentExpression()
		{
			return this.GetCurrentExpression();
		}
		#endregion

		#region Methods

		#region Protected

		#region GetCurrentExpression
		/// <summary>
		/// Creates an <see cref="Expression"/> based on the objects current values.
		/// </summary>
		/// <param name="context">The <see cref="FilterContext"/> object which will be used as a basis for the Expression being built.</param>
		/// <returns></returns>
		protected virtual Expression GetCurrentExpression(FilterContext context)
		{
			return context.CreateExpression(this);
		}

		/// <summary>
		/// Creates an <see cref="Expression"/> based on the objects current values.
		/// </summary>
		/// <remarks>
		/// Not used by this object.
		/// </remarks>
		/// <exception cref="NotImplementedException">Will be raised if this method is used.</exception>
		protected virtual Expression GetCurrentExpression()
		{
			throw new NotImplementedException("");
		}
		#endregion // GetCurrentExpression

		#region AddItemSilently
		/// <summary>
		/// Adds an element to the collection without invoking any events.
		/// </summary>
		/// <param name="item"></param>
		protected internal void AddItemSilently(IRecordFilter item)
		{
			base.AddItemSilently(this.Items.Count, item);

            item.PropertyChanged -= ProvidesChanged_PropertyChanged;

            item.PropertyChanged += ProvidesChanged_PropertyChanged;
		}
		#endregion // AddItemSilently

		#endregion // Protected

		#endregion // Methods

		#region Overrides

		#region AddItem
		/// <summary>
		/// Adds the item at the specified index. 
		/// </summary>
		/// <param name="index"></param>
		/// <param name="item"></param>
		protected override void AddItem(int index, IRecordFilter item)
		{
			base.AddItem(index, item);

            item.PropertyChanged -= ProvidesChanged_PropertyChanged;

            item.PropertyChanged += ProvidesChanged_PropertyChanged;
		}
		#endregion // AddItem

		#region RemoveItem
		/// <summary>
		/// Removes the item at the specified index.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		protected override bool RemoveItem(int index)
		{
			this[index].PropertyChanged -= ProvidesChanged_PropertyChanged;
			return base.RemoveItem(index);
		}
		#endregion // RemoveItem

		#region ResetItems
		/// <summary>
		/// Removes all items from the collection.
		/// </summary>
		protected override void ResetItems()
		{
			foreach (IRecordFilter item in this.Items)
			{
				item.PropertyChanged -= ProvidesChanged_PropertyChanged;
			}
			base.ResetItems();
		}
		#endregion // ResetItems

		#region InsertItem
		/// <summary>
		/// Adds an item to the collection at a given index.
		/// </summary>
		/// <param name="index"></param>
		/// <param name="item"></param>
		protected override void InsertItem(int index, IRecordFilter item)
		{
			this.AddItem(index, item);
		}
		#endregion // InsertItem
		#endregion // Overrides

		#region Events
		/// <summary>
		/// Raised when an Item in collection is modified.
		/// </summary>
		public event EventHandler<EventArgs> CollectionItemChanged;

		/// <summary>
		/// Raises the <see cref="CollectionItemChanged"/> event.
		/// </summary>
		protected virtual void OnCollectionItemChanged()
		{
			if (this.CollectionItemChanged != null)
				this.CollectionItemChanged(this, EventArgs.Empty);
		}
		#endregion // Events

		#region EventHandlers
		void ProvidesChanged_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			this.OnCollectionItemChanged();
		}
		#endregion // EventHandlers

		#region INotifyPropertyChanged Members

		/// <summary>
		/// Fired when a property changes on the <see cref="RecordFilterCollection"/>.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Raises the <see cref="PropertyChanged"/> event.
		/// </summary>
		/// <param name="propertyName"></param>
        protected virtual new void OnPropertyChanged(string propertyName)
		{
			if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
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