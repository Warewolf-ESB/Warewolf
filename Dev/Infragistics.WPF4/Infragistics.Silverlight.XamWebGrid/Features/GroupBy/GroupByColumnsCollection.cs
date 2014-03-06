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
using System.Collections.ObjectModel;
using System.Collections;
using System.Collections.Generic;
using Infragistics.Collections;

namespace Infragistics.Controls.Grids
{
	/// <summary>
	/// A collection that contains the <see cref="Column"/> objects that are Grouped.
	/// </summary>
	public class GroupByColumnsCollection : CollectionBase<Column>, IComparer<Column>, IProvideCustomPersistence
	{
		#region Members

		Dictionary<ColumnLayout, List<Column>> _groupByDictionary = new Dictionary<ColumnLayout, List<Column>>();

		#endregion // Members

		#region Methods

		#region Private

		private void RegisterColumn(Column column)
		{
			if (!this._groupByDictionary.ContainsKey(column.ColumnLayout))
				this._groupByDictionary.Add(column.ColumnLayout, new List<Column>());

			List<Column> cols = this._groupByDictionary[column.ColumnLayout];
			if (!cols.Contains(column))
				cols.Add(column);
			cols.Sort(this);

			this.DirtyFlag++;
		}

		private void UnregisterColumn(Column column)
		{
			this._groupByDictionary[column.ColumnLayout].Remove(column);
			this.DirtyFlag++;
		}

		#endregion // Private

		#region Protected

		#region OnGroupingChanged

		/// <summary>
		/// Called when the GroupBy collection has changed. 
		/// </summary>
		protected virtual void OnGroupingChanged(IList<Column> oldCollection, IList<Column> newCollection)
		{
			Collection<ColumnLayout> columnLayoutsToBeNotified = new Collection<ColumnLayout>();

			foreach (Column col in oldCollection)
			{
				if (!columnLayoutsToBeNotified.Contains(col.ColumnLayout))
					columnLayoutsToBeNotified.Add(col.ColumnLayout);
			}

			foreach (Column col in newCollection)
			{
				if (!columnLayoutsToBeNotified.Contains(col.ColumnLayout))
					columnLayoutsToBeNotified.Add(col.ColumnLayout);
			}

			foreach (ColumnLayout layout in columnLayoutsToBeNotified)
				layout.InvalidateGroupBy();

			this.Grid.OnGroupByCollectionChanged(oldCollection, newCollection);
		}

		#endregion // OnGroupingChanged

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
				val += col.Key + ":" + col.ColumnLayout.Key + ":" + col.IsSorted + ",";
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
				GroupBySettings settings = owner as GroupBySettings;

				if (settings != null && settings.Grid != null)
				{
                    this.Clear();

					string[] cols = ((string)value).Split(',');

					foreach (string col in cols)
					{
						if (col.Length > 0)
						{
							string[] keyPair = col.Split(':');

							if (keyPair.Length == 3)
							{
								ColumnLayout layout = this.Grid.ColumnLayouts[keyPair[1]];

								if (layout != null)
								{
									Column column = layout.Columns.AllColumns[keyPair[0]] as Column;
									if (column != null)
									{
										column.IsGroupBy = true;
										column.SetSortedColumnStateInternally((SortDirection)Enum.Parse(typeof(SortDirection), keyPair[2], true), true);
									}
								}
							}
						}
					}

				}
			}
		}
		#endregion // Load

		#endregion // Protected

		#endregion // Methods

		#region Properties

		#region Public

		#region Grid
		/// <summary>
		/// Gets a reference to the <see cref="XamGrid"/> that this collection belongs to. 
		/// </summary>
		public XamGrid Grid
		{
			get;
			protected internal set;
		}
		#endregion // Grid

		#region Indexer

		/// <summary>
		/// Gets a read-only collection of <see cref="Column"/> objects for the specified <see cref="ColumnLayout"/>.
		/// </summary>
		/// <param propertyName="layout"></param>
		/// <returns></returns>
		public ReadOnlyCollection<Column> this[ColumnLayout layout]
		{
			get
			{
				if (!this._groupByDictionary.ContainsKey(layout))
					this._groupByDictionary.Add(layout, new List<Column>());

				return new ReadOnlyCollection<Column>(this._groupByDictionary[layout]);
			}
		}
		#endregion // Indexer

		#endregion // Public

		#region Internal

		internal int DirtyFlag
		{
			get;
			private set;
		}

		#endregion // Internal

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
            if (item.CanBeGroupedBy)
            {
                if (!this.Items.Contains(item))
                {
                    List<Column> previousItems = new List<Column>();
                    previousItems.AddRange(this);

                    if (this.Grid != null)
                        item.SetGroupBy(true);

                    base.AddItem(index, item);

                    this.RegisterColumn(item);

                    if (this.Grid != null)
                    {
                        this.OnGroupingChanged(previousItems, this);
                        this.Grid.InvalidateScrollPanel(false);
                    }
                }
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
			this.AddItem(index, item);
		}
		#endregion // InsertItem

		#region RemoveItem

		/// <summary>
		/// Removes the item at the specified index.
		/// </summary>
		/// <param propertyName="index"></param>
		/// <returns></returns>
		protected override bool RemoveItem(int index)
		{
			List<Column> previousItems = new List<Column>();
			previousItems.AddRange(this);

			Column column = this.Items[index];

            bool removed = base.RemoveItem(index);

            this.UnregisterColumn(column);

			if (this.Grid != null)
				column.SetGroupBy(false);

			if (this.Grid != null)
			{
				this.OnGroupingChanged(previousItems, this);
				this.Grid.InvalidateScrollPanel(false);
				if (this.Grid.ConditionalFormattingSettings.AllowConditionalFormatting)
					this.Grid.ResetPanelRows();
			}

			return removed;
		}
		#endregion // RemoveItem

		#region ReplaceItem

		/// <summary>
		/// Replaces the item at the specified index with the specified item.
		/// </summary>
		/// <param propertyName="index"></param>
		/// <param propertyName="newItem"></param>
		protected override void ReplaceItem(int index, Column newItem)
		{
			List<Column> previousItems = new List<Column>();
			previousItems.AddRange(this);

			Column previousColumn = this.Items[index];
			if (this.Grid != null)
			{
				previousColumn.SetGroupBy(false);
				newItem.SetGroupBy(true);
			}

			base.ReplaceItem(index, newItem);

			this.UnregisterColumn(previousColumn);
			this.RegisterColumn(newItem);

			if (this.Grid != null)
			{
				this.OnGroupingChanged(previousItems, this);
				this.Grid.InvalidateScrollPanel(false);
			}
		}
		#endregion // ReplaceItem

		#region ResetItems

		/// <summary>
		/// Removes all items from the collection.
		/// </summary>
		protected override void ResetItems()
		{
			List<Column> previousItems = new List<Column>();
			previousItems.AddRange(this);

			if (this.Grid != null)
			{
                List<Column> cols = new List<Column>(this.Items);

				foreach (Column column in cols)
				{
					column.SetGroupBy(false);
					this.UnregisterColumn(column);
				}
			}

			base.ResetItems();

			if (this.Grid != null)
			{
				this.OnGroupingChanged(previousItems, this);
				this.Grid.InvalidateScrollPanel(false);
				if (this.Grid.ConditionalFormattingSettings.AllowConditionalFormatting)
					this.Grid.ResetPanelRows();
			}
		}
		#endregion // ResetItems

		#endregion // Overrides

		#region IComparer<Column> Members

		/// <summary>
		/// Compares the <see cref="Column"/> objects by looking at their position in the collection. 
		/// </summary>
		/// <param propertyName="x"></param>
		/// <param propertyName="y"></param>
		/// <returns></returns>
		protected virtual int Compare(Column x, Column y)
		{
			return this.IndexOf(x).CompareTo(this.IndexOf(y));
		}

		int IComparer<Column>.Compare(Column x, Column y)
		{
			return this.Compare(x, y);
		}

		#endregion

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