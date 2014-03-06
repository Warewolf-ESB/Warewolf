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
using Infragistics.Collections;

namespace Infragistics.Controls.Editors
{
    /// <summary>
    /// A collection that contains the <see cref="ComboColumn"/> objects that are pinned in a specific direction.
    /// </summary>
    public class FixedComboColumnsCollection: CollectionBase<ComboColumn>
	{
		#region Constructor
		/// <summary>
		/// Creates a new instance of the <see cref="FixedComboColumnsCollection"/>
		/// </summary>
		/// <param propertyName="direction">The <see cref="ComboColumnFixedState"/> that a <see cref="ComboColumn"/> should be set to when added to the collection.</param>
        public FixedComboColumnsCollection(ComboColumnFixedState direction)
		{
			this.Direction = direction;
		}
		#endregion // Constructor

		#region Properties

		#region Protected

		#region Direction

		/// <summary>
		/// Gets/Sets the <see cref="ComboColumnFixedState"/> that items should be set to when added to this collection.
		/// </summary>
		protected ComboColumnFixedState Direction
		{
			get;
			set;
		}

		#endregion // Direction

		#endregion // Protected

		#endregion // Properties

		#region Overrides

		#region AddItem

		/// <summary>
		/// Adds the item at the specified index. 
		/// </summary>
		/// <param propertyName="index"></param>
		/// <param propertyName="item"></param>
		protected override void AddItem(int index, ComboColumn item)
		{
			if (item != null)
			{
				if (item.IsFixed == this.Direction)
				{
					int currentIndex = this.Items.IndexOf(item);
                    if (currentIndex != -1)
                    {
                        if (index < this.Items.Count)
                        {
                            this.Items.Remove(item);
                            this.Items.Insert(index, item);
                            if (item.ComboEditor != null)
                                item.ComboEditor.InvalidateScrollPanel(false);
                        }
                    }
                    else
                    {
                        base.AddItem(index, item);
                    }
				}
				else
					item.SetFixedColumnState(this.Direction);
			}
		}

		#endregion // AddItem

		#region AddItem

		/// <summary>
		/// Adds the item at the specified index. 
		/// </summary>
		/// <param propertyName="index"></param>
		/// <param propertyName="item"></param>
        protected override void InsertItem(int index, ComboColumn item)
		{
			this.AddItem(index, item);
		}

		#endregion // AddItem

		#region RemoveItem

		/// <summary>
		/// Removes the item at the specified index.
		/// </summary>
		/// <param propertyName="index"></param>
		/// <returns></returns>
		protected override bool RemoveItem(int index)
		{
			bool removed = false;

			if (index >= 0 && index < this.Items.Count)
			{
                ComboColumn item = this.Items[index];
				if (item == null)
					removed = false;
				else
					removed = item.SetFixedColumnState(ComboColumnFixedState.NotFixed);
			}
			return removed;
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
		}

		#endregion // ResetItems

		#endregion // Overrides

		#region Methods

		#region Protected

		#region AddItemSilently

		/// <summary>
        /// Adds a <see cref="ComboColumn"/> to the collection without setting it's IsFixed property.
		/// </summary>
		/// <param propertyName="item"></param>
		protected internal virtual void AddItemSilently(ComboColumn item)
		{
			if (!this.Items.Contains(item))
				this.AddItemSilently(this.Count, item);
		}
		#endregion // AddItemSilently

		#region RemoveItemSilently

		/// <summary>
        /// Removes a <see cref="ComboColumn"/> from the collection without setting it's IsFixed property.
		/// </summary>
		/// <param propertyName="item"></param>
        protected internal virtual void RemoveItemSilently(ComboColumn item)
		{
			int index = this.Items.IndexOf(item);
			if (index != -1)
			{
				this.RemoveItemSilently(index);
			}
		}
		#endregion // RemoveItemSilently

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