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
using System.Collections.Generic;
using Infragistics.Collections;

namespace Infragistics.Controls.Grids
{
	/// <summary>
	/// A collection that contains the <see cref="Column"/> objects that are pinned in a specific direction.
	/// </summary>
	public class FixedColumnsCollection : CollectionBase<Column>, IProvideCustomPersistence
	{
		#region Constructor
		/// <summary>
		/// Creates a new instance of the <see cref="FixedColumnsCollection"/>
		/// </summary>
		/// <param propertyName="direction">The <see cref="FixedState"/> that a <see cref="Column"/> should be set to when added to the collection.</param>
		public FixedColumnsCollection(FixedState direction)
		{
			this.Direction = direction;
		}
		#endregion // Constructor

		#region Properties

		#region Protected

		#region Direction

		/// <summary>
		/// Gets/Sets the <see cref="FixedState"/> that items should be set to when added to this collection.
		/// </summary>
		protected FixedState Direction
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
		protected override void AddItem(int index, Column item)
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
                            if (item.ColumnLayout != null && item.ColumnLayout.Grid != null)
                                item.ColumnLayout.Grid.InvalidateScrollPanel(false);
                        }
                    }
                    else
                    {
                        if (item.ParentColumn != null)
                        {
                            item.IsFixed = FixedState.NotFixed;
                        }
                        else
                        {
                            base.AddItem(index, item);
                        }
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
		protected override void InsertItem(int index, Column item)
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
				Column item = this.Items[index];
				if (item == null)
					removed = false;
				else
					removed = item.SetFixedColumnState(FixedState.NotFixed);
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
		/// Adds a <see cref="Column"/> to the collection without setting it's IsFixed property.
		/// </summary>
		/// <param propertyName="item"></param>
		protected internal virtual void AddItemSilently(Column item)
		{
			if (!this.Items.Contains(item))
				this.AddItemSilently(this.Count, item);
		}
		#endregion // AddItemSilently

		#region RemoveItemSilently

		/// <summary>
		/// Removes a <see cref="Column"/> from the collection without setting it's IsFixed property.
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
				val += col.Key + ",";
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
				FixedColumnSettingsOverride settings = owner as FixedColumnSettingsOverride;

				if (settings != null && settings.ColumnLayout != null)
				{
                    this.Clear();

					string[] cols = ((string)value).Split(',');

					foreach (string key in cols)
					{
						if (key.Length > 0)
						{
							Column column = settings.ColumnLayout.Columns.DataColumns[key];
							if (column != null)
							{
								column.IsFixed = this.Direction;
							}
						}
					}

				}
			}
		}

		#endregion // Load

		#endregion // Protected

		#endregion // Methods

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