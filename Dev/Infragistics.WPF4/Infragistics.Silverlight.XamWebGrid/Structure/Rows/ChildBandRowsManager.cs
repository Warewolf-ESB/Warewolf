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
using System.Reflection;
using System.ComponentModel;
using Infragistics.Controls.Grids;
using Infragistics.Controls.Grids.Primitives;
using System.Collections.Generic;

namespace Infragistics.Controls.Grids
{
	/// <summary>
	/// A <see cref="RowsManagerBase"/> that manages <see cref="ChildBand"/>s for a particular <see cref="Row"/> object.
	/// </summary>
	public class ChildBandRowsManager : RowsManagerBase
	{
		#region Members

		RowBaseCollection _rows;
		ChildBandCollection _childLayoutRows;
		ChildBandColumn _column;

		#endregion // Members

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="ChildBandRowsManager"/> class.
		/// </summary>
		/// <param propertyName="parentRow">The <see cref="Row"/> object that owns this <see cref="RowsManagerBase"/></param>
		public ChildBandRowsManager(Row parentRow)
			: base(parentRow.ColumnLayout)
		{
			this.ParentRow = parentRow;
			this.Level = parentRow.Manager.Level + 1;
			object data = parentRow.Data;

			this._childLayoutRows = new ChildBandCollection();
			this._rows = new RowBaseCollection(this._childLayoutRows);

			if (data != null)
			{
				foreach (ColumnLayout column in this.ColumnLayout.Columns.ColumnLayouts)
					this.AddChildBand(column);

                this._childLayoutRows.Sort(new ChildBandComparer());
			}
		}

		#endregion // Constructor

		#region Properties

		/// <summary>
		/// Gets the Column that represents all cells for reach ChildBand in this RowsManager.
		/// </summary>
		public ChildBandColumn Column
		{
			get
			{
				if (this._column == null)
					this._column = new ChildBandColumn();
				return this._column;
			}
		}

		#endregion // Properties

		#region Methods

		private void AddChildBand(ColumnLayout layout)
		{
			layout.Grid = this.ColumnLayout.Grid;
			ChildBand cb = new ChildBand(this, layout, (Row)this.ParentRow);
			this._childLayoutRows.Add(cb);            
			layout.PropertyChanged += new PropertyChangedEventHandler(ChildColumnLayout_PropertyChanged);
		}

        internal void InvalidateManager()
        {
            if (this.ParentRow != null)
            {
                bool isRowVisible = false;
                // Child bands have Rows, that are visible, when there are Siblings, or the ColumnLayoutHeaderVisbility is set to always
                // So we need to take them into account. 				
                foreach (ChildBand row in this.Rows)
                {
                    if (row.ResolveIsVisible)
                    {
                        isRowVisible = true;
                        break;
                    }
                }

                for (int j = this.VisibleChildManagers.Count - 1; j >= 0; j--)
                {
                    RowsManagerBase childManager = this.VisibleChildManagers[j] as RowsManagerBase;

                    if (childManager.ColumnLayout == null || childManager.ColumnLayout.Visibility == Visibility.Collapsed)
                    {
                        this.UnregisterChildRowsManager(childManager);
                    }
                }

                if (this.VisibleChildManagers.Count == 0)
                {
                    // If there is a row visible, then we shouldn't unregister the manager, b/c there is still a sibling that is viisble. 
                    if (!isRowVisible)
                        this.ParentRow.Manager.UnregisterChildRowsManager(this);
                }
            }
        }

		#endregion // Methods

		#region Overrides

		#region Rows

		/// <summary>
		/// A collection of <see cref="ChildBand"/> objects that this object manages. 
		/// </summary>
		public override RowBaseCollection Rows
		{
			get
			{
				return this._rows;
			}
		}
		#endregion // Rows

		#region CompareTo

		/// <summary>
		/// Compares the index of the parent row of the manager, to the parent row index of the other manager. 
		/// </summary>
		/// <param propertyName="other"></param>
		/// <returns>
		/// A signed number indicating the relative values of this instance and value. 
		/// Return FilterActionValue Description: 
		/// Less than zero This instance is less than value. 
		/// Zero This instance is equal to value. 
		/// Greater than zero This instance is greater than value. -or- value is null. 
		/// </returns>
		protected override int CompareTo(RowsManagerBase other)
		{
			return this.ParentRow.Index.CompareTo(other.ParentRow.Index);
		}
		#endregion // CompareTo

		#region ResolveIndentation
		/// <summary>
		/// Resolves the indentation for a particular <see cref="ChildBand"/>. 
		/// </summary>
		/// <param propertyName="row">The ChildBand whose indentation should be calculated.</param>
		/// <returns></returns>
		protected internal override double ResolveIndentation(RowBase row)
		{
			double indendation = 0;

			if (((ChildBand)row).ResolveIsVisible)
			{
				indendation = base.ResolveIndentation(row);
			}
			else if (this.ParentRow != null)
			{
				indendation = this.ParentRow.Manager.ResolveIndentation(this.ParentRow);
			}

			return indendation;
		}
		#endregion // ResolveIndentation

        #region UnregisterRowsManager

        protected internal override void UnregisterRowsManager(bool removeColumnLayout, bool clearChildRowsManager, bool clearSelection)
	    {
	        if (clearChildRowsManager)
	        {
	            foreach (ChildBand childLayoutRow in this._childLayoutRows)
	            {
	                if (childLayoutRow.Manager == this)
	                {
	                    childLayoutRow.ChildRowsManager.UnregisterRowsManager(removeColumnLayout, clearChildRowsManager, clearSelection);
	                }
	            }

	            this._childLayoutRows.Clear();
	        }

	        base.UnregisterRowsManager(removeColumnLayout, clearChildRowsManager, clearSelection);
	    }

        #endregion // UnregisterRowsManager

        #region OnRegisteredAsVisibleChildManager

        /// <summary>
		/// Invoked when a <see cref="RowsManagerBase"/> is now visible, meaning it's Parent row is expanded. 
		/// </summary>
		protected override void OnRegisteredAsVisibleChildManager()
		{
			// This Manager's parent has been expanded, making this manager visible
			// So, loop through all the ChildBands of this RowsManager
			foreach (ChildBand row in this._childLayoutRows)
			{
				// If the ChildBand can be displayed
				if (row.ResolveIsVisible)
				{
					// So the ChildBand is visible, if it's expanded, register it's Children
					if (row.IsExpanded)
						this.RegisterChildRowsManager(row.ChildRowsManager);
				}
				else
				{
                    if (row.ColumnLayout.Visibility == Visibility.Visible)
                    {
                        // This ChildBand itself isn'type visible, which means that it's children should be
                        this.RegisterChildRowsManager(row.ChildRowsManager);
                    }
				}
			}

			base.OnRegisteredAsVisibleChildManager();
		}
		#endregion // OnRegisteredAsVisibleChildManager

		#region OnUnregisteredAsVisibleChildManager

		/// <summary>
		/// Invoked when a <see cref="RowsManagerBase"/> is no longer visible, meaning it's Parent row is collapsed. 
		/// </summary>
		protected override void OnUnregisteredAsVisibleChildManager()
		{
			// This Manager's parent has been collapsed, making this manager hidden
			// So, loop through all the ChildBands of this RowsManager
			foreach (ChildBand row in this._childLayoutRows)
			{
				// if the ChildBand, wasn'type visible, we need to unregister it's children.
				if (!row.ResolveIsVisible)
				{
					this.UnregisterChildRowsManager(row.ChildRowsManager);
				}
			}

			base.OnUnregisteredAsVisibleChildManager();
		}
		#endregion // OnUnregisteredAsVisibleChildManager

		#region FullRowCount

		/// <summary>
		/// Gets the total number of <see cref="ChildBand"/> objects that can be physically displayed. 
		/// </summary>
		protected internal override int FullRowCount
		{
			get
			{
				int count = 0;

				foreach (ChildBand row in this._childLayoutRows)
				{
					if (row.ResolveIsVisible)
						count++;
				}

				return count;
			}
		}
		#endregion // FullRowCount

		#region ResolveRowForIndex

		/// <summary>
		/// Returns the <see cref="ChildBand"/> for the given index. 
		/// </summary>
		/// <param propertyName="index"></param>
		/// <returns></returns>
		protected internal override RowBase ResolveRowForIndex(int index)
		{
			int current = 0;
			RowBase r = null;
			foreach (ChildBand row in this._childLayoutRows)
			{
				if (row.ResolveIsVisible)
				{
					if (current == index)
					{
						r = row;
						break;
					}
					current++;
				}
			}

			return r;
		}
		#endregion // ResolveRowForIndex

		#region ResolveIndexForRow

		/// <summary>
		/// Resolves the index of the given <see cref="ChildBand"/>.
		/// </summary>
		/// <param propertyName="row"></param>
		/// <returns>
		/// If the specified <see cref="ChildBand"/> is not currently visible, then index of the row that comes before it will be returned. 
		/// If the specified row is the only row in the collection, then -1 will be returned. 
		/// </returns>
		protected internal override int ResolveIndexForRow(RowBase row)
		{
			int index = 0;
			foreach (ChildBand childRow in this._childLayoutRows)
			{
				if (childRow.ResolveIsVisible)
				{
					if (row == childRow)
						return index;

					index++;
				}
				else if (row == childRow)
					break;
			}
			return index - 1;
		}
		#endregion // ResolveIndexForRow

		#region OnChildColumnLayoutRemoved

		/// <summary>
		/// Raised when a <see cref="ColumnLayout"/> is removed from the owning ColumnLayout's Columns collection.
		/// </summary>
		/// <param propertyName="layout">The <see cref="ColumnLayout"/> being removed.</param>
		protected override void OnChildColumnLayoutRemoved(ColumnLayout layout)
		{
			ChildBand cb = this._childLayoutRows[layout.Key];
			if (cb != null)
			{
                this._childLayoutRows.Remove(cb);
                cb.ColumnLayout.PropertyChanged -= new PropertyChangedEventHandler(ChildColumnLayout_PropertyChanged);
			}
		}
		#endregion // OnChildColumnLayoutRemoved

		#region OnChildColumnLayoutAdded

		/// <summary>
		/// Raised when a <see cref="ColumnLayout"/> is added to the owning ColumnLayout's Columns collection.
		/// </summary>
		/// <param propertyName="layout">The <see cref="ColumnLayout"/> being added.</param>
		protected override void OnChildColumnLayoutAdded(ColumnLayout layout)
		{
			ChildBand cb = this._childLayoutRows[layout.Key];

            if (cb == null)
            {
                this.AddChildBand(layout);                
            }
            else
            {
                this._childLayoutRows.Sort(new ChildBandComparer());
                this.SortVisibleChildManagers();
            }

            if(!layout.IsMoving)
                ((RowsManager)this.ParentRow.Manager).ClearRows();
		}
		#endregion // OnChildColumnLayoutAdded

		#endregion // Overrides

		#region EventHandlers

		void ChildColumnLayout_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "ColumnLayoutHeaderVisibility")
			{
				Row parentRow = ((Row)this.ParentRow);
				if (parentRow.IsExpanded)
				{
					foreach (ChildBand row in this._childLayoutRows)
						this.UnregisterChildRowsManager(row.ChildRowsManager);

					this.OnRegisteredAsVisibleChildManager();
				}
			}
		}

		#endregion // EventHandlers
	}

    /// <summary>
    /// A IComparer implementation for ChildBand datatypes
    /// </summary>
    internal class ChildBandComparer : IComparer<ChildBand>
    {
        #region IComparer<Uri> Members
        /// <summary>
        ///     Compares two objects and returns a value indicating whether one is less than,
        ///     equal to, or greater than the other.
        /// </summary>
        /// <param propertyName="x"> The first object to compare.</param>
        /// <param propertyName="y">The second object to compare.</param>
        /// <returns/>
        public int Compare(ChildBand x, ChildBand y)
        {
            ColumnLayout parent = x.ParentRow.ColumnLayout;
            int xIndex = parent.Columns.IndexOf(x.ColumnLayout);
            int yIndex = parent.Columns.IndexOf(y.ColumnLayout);
            return xIndex.CompareTo(yIndex);
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