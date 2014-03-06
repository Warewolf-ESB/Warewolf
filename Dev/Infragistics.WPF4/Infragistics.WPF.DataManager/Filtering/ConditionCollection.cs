using System;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.ComponentModel;
using System.Collections.Specialized;
using Infragistics.Collections;

namespace Infragistics
{
	/// <summary>
	/// A collection of <see cref="IFilterCondition"/> objects which represent a group of conditions bound by a <see cref="LogicalOperator"/>
	/// </summary>
    public class ConditionCollection : CollectionBase<IFilterCondition>, IFilterCondition, IGroupFilterConditions, IProvidePersistenceLookupKeys
	{
		#region Members
		private IRecordFilter _parent;
		LogicalOperator _logicalOperator = LogicalOperator.And;
		#endregion

		#region Properties

		#region Public

		#region Parent
		/// <summary>
		/// Gets the <see cref="IRecordFilter"/> object which contains this object.
		/// </summary>
		public IRecordFilter Parent
		{
			get
			{
				return this._parent;
			}
			//protected internal set
            set
			{
				if (this._parent != value)
				{
					this._parent = value;
					foreach (IFilterCondition item in this.Items)
					{
						item.Parent = value;
					}
				}
			}
		}
		#endregion // Parent

		#region LogicalOperator
		/// <summary>
		/// Gets / sets the <see cref="LogicalOperator"/> that will be used to combine all the <see cref="IFilterCondition"/> objects in this collection.
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

		#endregion // Public

		#endregion // Properties

		#region Event Handlers

		void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			this.OnCollectionItemChanged();
		}

		#endregion // Event Handlers

		#region Overrides

		#region RemoveItem
		/// <summary>
		/// Removes the item at the specified index.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		protected override bool RemoveItem(int index)
		{
			this.Items[index].Parent = null;
			this.Items[index].PropertyChanged -= Item_PropertyChanged;
			this.OnNotifyCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, this.Items[index], index));
			return base.RemoveItem(index);
		}
		#endregion // RemoveItem

		#region RemoveItemSilently

		/// <summary>
		/// Removes the item at the specified index without raising any events.
		/// </summary>
		/// <param name="item"></param>
		protected internal bool RemoveItemSilently(IFilterCondition item)
		{
			int index = this.Items.IndexOf(item);
			if (index != -1)
			{				
				item.Parent = null;
				item.PropertyChanged -= Item_PropertyChanged;
				base.RemoveItemSilently(index);
			}
			return false;
		}

		#endregion // RemoveItemSilently

		#region AddItem
		/// <summary>
		/// Adds the item at the specified index. 
		/// </summary>
		/// <param name="index"></param>
		/// <param name="item"></param>
		protected override void AddItem(int index, IFilterCondition item)
		{
			base.AddItemSilently(index, item);
			item.Parent = this.Parent;
			item.PropertyChanged += new PropertyChangedEventHandler(Item_PropertyChanged);
			this.OnNotifyCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
		}

		#endregion // AddItem

		#region ResetItems
		/// <summary>
		/// Removes all items from the collection.
		/// </summary>
		protected override void ResetItems()
		{
			foreach (IFilterCondition item in this.Items)
			{
				item.PropertyChanged -= Item_PropertyChanged;
			}
			this.OnNotifyCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
			base.ResetItems();
		}
		#endregion // ResetItems

		#region InsertItem
		/// <summary>
		/// Adds an item to the collection at a given index.
		/// </summary>
		/// <param name="index"></param>
		/// <param name="item"></param>
		protected override void InsertItem(int index, IFilterCondition item)
		{
			this.AddItem(index, item);
		}
		#endregion // InsertItem

		#region ResetItemsSilently

		/// <summary>
		/// Removes all items from the collection without firing any events.
		/// </summary>
		protected override void ResetItemsSilently()
		{
			foreach (IFilterCondition item in this.Items)
			{
				item.PropertyChanged -= Item_PropertyChanged;
			}
			base.ResetItemsSilently();
		}

		#endregion // ResetItemsSilently

		#endregion // Overrides

		#region GetCurrentExpression
		/// <summary>
		/// Generates the current expression for this <see cref="ConditionCollection"/>.
		/// </summary>
		/// <returns></returns>
		protected virtual Expression GetCurrentExpression()
		{
			if (this.Parent != null)
			{
				FilterContext context = FilterContext.CreateGenericFilter(this.Parent.ObjectTypedInfo, this.Parent.FieldType, false, false);

				return GetCurrentExpression(context);
			}
			return null;
		}

		/// <summary>
		/// Generates the current expression for this <see cref="ConditionCollection"/> using the inputted context.
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		protected virtual Expression GetCurrentExpression(FilterContext context)
		{
			return context.CreateExpression(this);
		}
		#endregion // GetCurrentExpression

		#region IFilterCondition Members

		IRecordFilter IFilterCondition.Parent
		{
			get
			{
				return this.Parent;
			}
			set
			{
				this.Parent = value;
			}
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

		#region IGroupFilterConditions Members

		LogicalOperator IGroupFilterConditions.LogicalOperator
		{
			get { return this.LogicalOperator; }
		}

		/// <summary>
		/// Event raised when an Item in the Collection is changed.
		/// </summary>
		public event EventHandler<EventArgs> CollectionItemChanged;

		/// <summary>
		/// Raises the CollectionItemChanged event.
		/// </summary>
		protected virtual void OnCollectionItemChanged()
		{
			if (this.CollectionItemChanged != null)
				this.CollectionItemChanged(this, EventArgs.Empty);
		}

		#endregion // IGroupFilterConditions

		#region INotifyPropertyChanged Members

		/// <summary>
		/// Raises the <see cref="PropertyChanged"/> event.
		/// </summary>
		/// <param name="name"></param>
		protected virtual new void OnPropertyChanged(string name)
		{
			if (this.PropertyChanged != null)
				this.PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(name));
		}

		/// <summary>
		/// Event raised when a property on this object changes.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		#endregion

		#region Methods

		#region ClearSilently

		/// <summary>
		/// Removes all the elements of the collection without raising any events.
		/// </summary>
		protected internal void ClearSilently()
		{
			this.ResetItemsSilently();
		}

		#endregion // ClearSilently

		#region AddItemSilently

		/// <summary>
		/// Adds an element to the collection without raising any events.
		/// </summary>
		/// <param name="item"></param>
		protected internal void AddItemSilently(IFilterCondition item)
		{
			base.AddItemSilently(this.Items.Count, item);
			item.Parent = this.Parent;
			item.PropertyChanged += new PropertyChangedEventHandler(Item_PropertyChanged);
		}

		#endregion // AddItemSilently

		#endregion // Methods

        #region IProvidePersistenceLookupKeys Members

        #region GetLookupKeys

        /// <summary>
        /// Gets a list of keys that each object in the collection has. 
        /// </summary>
        /// <returns></returns>
        protected virtual Collection<string> GetLookupKeys()
        {
            return new Collection<string>() { this.LogicalOperator.ToString() };
        }

        #endregion // GetLookupKeys

        #region CanRehydrate

        /// <summary>
        /// Looks through the keys, and determines that all the keys are in the collection, and that the same about of objects are in the collection.
        /// If this isn't the case, false is returned, and the Control Persistence Framework, will not try to reuse the object that are already in the collection.
        /// </summary>
        /// <param name="lookupKeys"></param>
        /// <returns></returns>
        protected virtual bool CanRehydrate(Collection<string> lookupKeys)
        {
            // So yea, i'm not using my own framework properly :)
            // But the problem is that this is a collection, and the PS framework doesn't save properties on collections. 
            if (lookupKeys.Count > 0)
                this.LogicalOperator = (LogicalOperator)Enum.Parse(typeof(LogicalOperator), lookupKeys[0], true);

            return false;
        }

        #endregion // CanRehydrate

        Collection<string> IProvidePersistenceLookupKeys.GetLookupKeys()
        {
            return this.GetLookupKeys();
        }

        bool IProvidePersistenceLookupKeys.CanRehydrate(Collection<string> lookupKeys)
        {
            return this.CanRehydrate(lookupKeys);  
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