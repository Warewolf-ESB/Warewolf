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
using Infragistics.Controls.Grids.Primitives;
using System.Collections.Generic;
using Infragistics.Collections;

namespace Infragistics.Controls.Grids
{
    /// <summary>
    /// A <see cref="Column"/> that organizes other Columns under a single header. 
    /// </summary>
    public class GroupColumn : Column
    {
        #region Members

        GroupColumnsCollection _columns;

        #endregion // Members

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupColumn"/> class.
        /// </summary>
        public GroupColumn()
        {
            this.HeaderTextHorizontalAlignment = HorizontalAlignment.Center;
        }

        #endregion // Constructor

        #region Overrides

        #region ResizeColumnResolved

        /// <summary>
        /// Resolves the <see cref="Column"/> that should be resized when dragging the right edge of this Column.
        /// </summary>
        protected internal override Column ResizeColumnResolved
        {
            get
            {
                Column resizeCol = this;

                if (this.Columns.Count > 0)
                {
                    bool starCol = false;

                    if (this.Columns.Count > 0)
                    {
                        Column lastVisCol = null;
                        foreach (Column col in this.Columns)
                        {
                            if (col.WidthResolved.WidthType == ColumnWidthType.Star)
                            {
                                starCol = true;
                            }

                            if (col.Visibility == Visibility.Visible && col.IsResizable)
                                lastVisCol = col;
                        }

                        if (!starCol)
                        {
                            if(lastVisCol != null)
                                resizeCol = lastVisCol.ResizeColumnResolved;
                        }
                    }
                }

                return resizeCol;
            }
        }

        #endregion // ResizeColumnResolved

        #region RequiresBoundDataKey
        /// <summary>
        /// This Column type doesn't require a Key that is bound to the underlying Data.
        /// </summary>
        protected internal override bool RequiresBoundDataKey
        {
            get
            {
                return false;
            }
        }
        #endregion // RequiresBoundDataKey

        #region IsEditable

        /// <summary>
        /// This Column type is not Editable.
        /// </summary>
        protected internal override bool IsEditable
        {
            get
            {
                return false;
            }
        }
        #endregion // IsEditable

        #region IsFilterable

        /// <summary>
        /// A <see cref="GroupColumn"/> is not Filterable.
        /// </summary>
        public override bool IsFilterable
        {
            get
            {
                return false;
            }
            set
            {
                base.IsFilterable = value;
            }
        }

        #endregion // IsFilterable

        #region IsSortable

        /// <summary>
        /// This Column is not sortable.
        /// </summary>
        public override bool IsSortable
        {
            get
            {
                return false;
            }
            set
            {
                base.IsSortable = value;
            }
        }
        #endregion //IsSortable

        #region IsSummable

        /// <summary>
        /// The <see cref="GroupColumn"/> is not Summable
        /// </summary>
        public override bool IsSummable
        {
            get
            {
                return false;
            }
            set
            {
                base.IsSummable = value;
            }
        }
        #endregion // IsSummable

        #region IsGroupable

        /// <summary>
        /// This column is not Groupable.
        /// </summary>
        public override bool IsGroupable
        {
            get
            {
                return false;
            }
            set
            {
                base.IsGroupable = value;
            }
        }
        #endregion // IsGroupable

        #region CanBeGroupedBy

        /// <summary>
        /// A <see cref="GroupColumn"/> can not be Grouped.
        /// </summary>
        protected internal override bool CanBeGroupedBy
        {
            get
            {
                return false;
            }
        }

        #endregion // CanBeGroupedBy

        #region CanBeSorted

        /// <summary>
        /// Determines if a <see cref="Column"/> can be Sorted.
        /// </summary>
        protected internal override bool CanBeSorted
        {
            get
            {
                return false;
            }
        }

        #endregion // CanBeSorted

        #region CanBeFiltered

        /// <summary>
        /// Determines if a <see cref="Column"/> can be Filtered.
        /// </summary>
        protected internal override bool CanBeFiltered
        {
            get
            {
                return false;
            }
        }

        #endregion // CanBeFiltered

        #region OnColumnLayoutChanged
        /// <summary>
        /// Raised when the a <see cref="ColumnLayout"/> is assigned or removed from this <see cref="GroupColumn"/>
        /// </summary>
        protected override void OnColumnLayoutChanged()
        {
            base.OnColumnLayoutChanged();

            foreach (Column col in this.Columns)
            {
                col.ColumnLayout = this.ColumnLayout;
            }

            this.Columns.ColumnLayout = this.ColumnLayout;
        }
        #endregion // OnColumnLayoutChanged

        #region GenerateContentProvider

        /// <summary>
        /// This Column does not need a <see cref="ColumnContentProviderBase"/>
        /// </summary>
        /// <returns></returns>
        protected internal override ColumnContentProviderBase GenerateContentProvider()
        {
            return null;
        }
        #endregion // GenerateContentProvider

        #region GenerateDataCell

        /// <summary>
        /// Generates a new <see cref="GroupCell"/> which will be used for all data rows in this <see cref="GroupColumn"/>
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        protected override CellBase GenerateDataCell(RowBase row)
        {
            return new GroupCell(row, this);
        }
        #endregion // GenerateDataCell

        #region GenerateHeaderCell
        /// <summary>
        /// Generates a new <see cref="GroupHeaderCell"/> which will be used for all header rows in this <see cref="GroupColumn"/>
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        protected override CellBase GenerateHeaderCell(RowBase row)
        {
            return new GroupHeaderCell(row, this);
        }

        #endregion // GenerateHeaderCell

        #region GenerateFooterCell
        /// <summary>
        /// Generates a new <see cref="GroupFooterCell"/> which will be used for all Footer rows in this <see cref="GroupColumn"/>
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        protected override CellBase GenerateFooterCell(RowBase row)
        {
            return new GroupFooterCell(row, this);
        }

        #endregion // GenerateFooterCell

        #region ResolveChildColumns

        /// <summary>
        /// Returns a collection of <see cref="Column"/> objects that belong to this <see cref="GroupColumn"/>
        /// </summary>
        /// <returns></returns>
        protected internal override CollectionBase<Column> ResolveChildColumns()
        {
            return this.Columns;
        }

        #endregion // ResolveChildColumns

        #region AllColumns

        /// <summary>
        /// Gets a ReadOnly collection of all <see cref="Column"/> objects that belong to a particular <see cref="Column"/>. 
        /// </summary>
        /// <remarks>
        /// This includes Columns that are children of other Columns. 
        /// </remarks>
        public override ReadOnlyKeyedColumnBaseCollection<Column> AllColumns
        {
            get
            {
                List<Column> allColumns = new List<Column>();
                foreach (Column col in this.Columns)
                {
                    allColumns.Add(col);
                    allColumns.AddRange(col.AllColumns);
                }

                return new ReadOnlyKeyedColumnBaseCollection<Column>(allColumns);
            }
        }
        #endregion // AllColumns

        #region AllVisibleChildColumns

        /// <summary>
        /// Gets a ReadOnly collection of all visible <see cref="Column"/> objects that have no children columns.
        /// </summary>
        /// <remarks>
        /// This includes Columns that are children of other Columns. 
        /// </remarks>
        public override ReadOnlyKeyedColumnBaseCollection<Column> AllVisibleChildColumns
        {
            get
            {
                List<Column> allColumns = new List<Column>();
                foreach (Column col in this.Columns)
                {
                    if (col.Visibility == System.Windows.Visibility.Visible)
                    {
                        ReadOnlyKeyedColumnBaseCollection<Column> children = col.AllVisibleChildColumns;
                        if (children.Count == 0)
                            allColumns.Add(col);
                        else
                            allColumns.AddRange(children);
                    }
                }

                return new ReadOnlyKeyedColumnBaseCollection<Column>(allColumns);
            }
        }
        #endregion // AllVisibleChildColumns

        #region SupportsActivationAndSelection

        /// <summary>
        /// A <see cref="GroupColumn"/> can neither be selected or Activated.
        /// </summary>
        protected internal override bool SupportsActivationAndSelection
        {
            get
            {
                return false;
            }
        }

        #endregion // SupportsActivationAndSelection

        #region HeaderStyleResolved


        /// <summary>
        /// Resolves the actual Style that will be applied to the <see cref="GroupHeaderCellControl"/>
        /// </summary>
        public override Style HeaderStyleResolved
        {
            get
            {
                return this.HeaderStyle;
            }
        }

        #endregion //HeaderStyleResolved

        #endregion // Overrides

        #region Properties

        #region Public

        #region Columns
        /// <summary>
        /// Gets the Collection of <see cref="Column"/> objects that are grouped in to this Column.
        /// </summary>
        public GroupColumnsCollection Columns
        {
            get
            {
                if (this._columns == null)
                {
                    this._columns = new GroupColumnsCollection();
                    this._columns.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Columns_CollectionChanged);
                }

                return this._columns;
            }
        }

        #endregion // Columns

        #endregion // Public

        #endregion // Properties       

        #region EventHandlers 

        #region Columns_CollectionChanged

        void Columns_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (Column col in e.NewItems)
                {
                    if (!col.IsMoving)
                    {
                        col.ColumnLayout = this.ColumnLayout;
                        col.ParentColumn = this;
                        col.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(Col_PropertyChanged);
                    }
                }
            }

            if (e.OldItems != null)
            {
                bool update = false;
                foreach (Column col in e.OldItems)
                {
                    if (!col.IsMoving)
                    {
                        col.PropertyChanged -= Col_PropertyChanged;
                        col.ParentColumn = null;
                        update = true;
                    }
                }

                if (update && this.ColumnLayout != null && this.ColumnLayout.Grid != null)
                    this.ColumnLayout.Grid.ResetPanelRows(true);
            }
        }

        #endregion // Columns_CollectionChanged

        #region Col_PropertyChanged

        void Col_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            this.OnPropertyChanged(e.PropertyName);
        }

        #endregion // Col_PropertyChanged

        #endregion // EventHandlers
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