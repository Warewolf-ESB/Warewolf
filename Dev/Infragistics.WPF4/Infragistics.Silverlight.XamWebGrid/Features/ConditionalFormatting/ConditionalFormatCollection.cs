using System;
using Infragistics.Collections;
namespace Infragistics.Controls.Grids
{
	/// <summary>
	/// A collection of <see cref="IConditionalFormattingRule"/> objects.
	/// </summary>
	public class ConditionalFormatCollection : CollectionBase<IConditionalFormattingRule>
	{
		#region Members

		Column _column;

		#endregion // Members

		#region Properties

		#region Column

		/// <summary>
		/// Gets the <see cref="Column"/> which the <see cref="IConditionalFormattingRule"/> applies.
		/// </summary>
		public Column Column
		{
			get
			{
				return this._column;
			}
			protected internal set
			{
				if (this._column != value)
				{
					this._column = value;

					foreach (IConditionalFormattingRule rule in this.Items)
					{
						rule.Column = value;
					}
				}
			}
		}

		#endregion // Column

		#endregion // Properties

		#region Overrides

		#region OnItemAdded

		/// <summary>
		/// Invoked when a <see cref="IConditionalFormattingRuleProxy"/> is added at the specified index.
		/// </summary>
		/// <param propertyName="index"></param>
		/// <param propertyName="item"></param>
		protected override void OnItemAdded(int index, IConditionalFormattingRule item)
		{
			if (this.Column != null)
			{
				item.Column = this.Column;
				item.PropertyChanged += Item_PropertyChanged;

			}
			base.OnItemAdded(index, item);
		}

		#endregion // OnItemAdded

		#region OnItemRemoved

		/// <summary>
		/// Invoked when a <see cref="IConditionalFormattingRuleProxy"/> is removed at the specified index.
		/// </summary>
		/// <param propertyName="index"></param>
		/// <param propertyName="item"></param>
		protected override void OnItemRemoved(int index, IConditionalFormattingRule item)
		{
			base.OnItemRemoved(index, item);
			item.PropertyChanged -= Item_PropertyChanged;
            if (item.Column == this.Column)
                item.Column = null;
		}

		#endregion // OnItemRemoved

		#region ResetItems

		/// <summary>
		/// Removes all items from the collection.
		/// </summary>
		protected override void ResetItems()
		{
			foreach (IConditionalFormattingRule item in this.Items)
			{
				item.Column = null;
			}

			base.ResetItems();
		}

		#endregion // ResetItems

		#endregion // Overrides

		#region Event Handlers

		void Item_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			this.OnCollectionItemChanged();
		}

		#endregion // Event Handlers

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