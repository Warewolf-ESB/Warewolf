using System;
using System.Collections.ObjectModel;

namespace Infragistics.Controls.Grids.Primitives
{
	/// <summary>
	/// An object that represents a particular data item whose data has been grouped.
	/// </summary>
	public class GroupByRow : Row
	{
		#region Members
		GroupByDataContext _groupData;
		RowsManagerBase _childRowsManager;
		SingleCellBaseCollection<GroupByCell> _cells;
		#endregion // Members

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="GroupByRow"/> class.
		/// </summary>
		/// <param propertyName="index"></param>
		/// <param propertyName="manager">The <see cref="RowsManager"/> that owns the <see cref="GroupByRow"/>.</param>		
		/// <param propertyName="data">The <see cref="GroupByDataContext"/> that this row represents.</param>
		public GroupByRow(int index, RowsManager manager, GroupByDataContext data)
			: base(index, manager, data.Value)
		{
			this.GroupByData = data;
			this.ItemSource = data.Records;
		}
		#endregion // Constructor

		#region Properties

		#region Public

		#region GroupByData

		/// <summary>
		/// Gets the <see cref="GroupByDataContext"/> that the <see cref="GroupByRow"/> represents.
		/// </summary>
		public GroupByDataContext GroupByData
		{

			get { return this._groupData; }
			protected internal set { this._groupData = value; }
		}

		#endregion // GroupByData

		#region CurrentPageIndex
		/// <summary>
		/// Gets / sets the page of data for the given data island.
		/// </summary>
		public int CurrentPageIndex
		{
			get
			{
				return ((RowsManager)this.ChildRowsManager).CurrentPageIndex;
			}
			set
			{
				if (((RowsManager)this.ChildRowsManager).CurrentPageIndex != value)
					((RowsManager)this.ChildRowsManager).CurrentPageIndex = value;
			}
		}
		#endregion // CurrentPageIndex

		#region RowFiltersCollection

		/// <summary>
		/// Gets a <see cref="RowFiltersCollection"/> object that contains the filters being applied to this <see cref="ColumnLayout"/>.
		/// </summary>
		public RowFiltersCollection RowFiltersCollection
		{
			get
			{
				return ((RowsManager)this.ChildRowsManager).RowFiltersCollection;
			}
		}

		#endregion // RowFiltersCollection

		#region Rows

		/// <summary>
		/// Gets the child rows of this <see cref="GroupByRow"/>
		/// </summary>
		public virtual RowCollection Rows
		{
			get
			{
				if (this.ChildRowsManager != null)
				{
					return ((RowCollection)this.ChildRowsManager.Rows.ActualCollection);
				}

				return new RowCollection((RowsManager)this.Manager, this.ColumnLayout);
			}
		}

		#endregion // Rows

		#endregion // Public

		#endregion // Properties

		#region Overrides

		#region HasChildren

		/// <summary>
		/// Gets whether or not <see cref="GroupByRow"/> has any child rows.
		/// </summary>
		public override bool HasChildren
		{
			get 
			{
				return (this._groupData != null && this._groupData.Records.Count > 0);
			}
		}
		#endregion // HasChildren

		#region RecyclingElementType

		/// <summary>
		/// Gets the Type of control that should be created for the <see cref="GroupByRow"/>.
		/// </summary>
		protected override Type RecyclingElementType
		{
			get
			{
				return typeof(GroupByRowCellsPanel);
			}
		}
		#endregion // RecyclingElementType

		#region CreateInstanceOfRecyclingElement

		/// <summary>
		/// Creates a new instance of the <see cref="GroupByRowCellsPanel"/>. 
		/// Note: this method is only meant to be invoked via the RecyclingManager.
		/// </summary>
		/// <returns>A new <see cref="GroupByRowCellsPanel"/></returns>
		protected override CellsPanel CreateInstanceOfRecyclingElement()
		{
			return new GroupByRowCellsPanel();
		}
		#endregion // CreateInstanceOfRecyclingElement

		#region RowType

		/// <summary>
		/// Gets the <see cref="RowType"/> of this <see cref="RowBase"/>
		/// </summary>
		public override RowType RowType
		{
			get { return RowType.GroupByRow; }
		}

		#endregion // RowType

		#region ChildRowsManager

		/// <summary>
		/// Gets the <see cref="RowsManager"/> that the <see cref="GroupByRow"/> owns.
		/// </summary>
		protected internal override RowsManagerBase ChildRowsManager
		{
			get
			{
				if (this._childRowsManager == null)
				{
					RowsManager manager = new RowsManager(this.Manager.Level, this.ColumnLayout, this); 
					manager.CachedRows = ((RowsManager)this.Manager).CachedRows;
					this._childRowsManager = manager;
				}
				return this._childRowsManager;
			}
			set
			{
				throw new NotImplementedException();
			}
		}
		#endregion // ChildRowsManager

		#region Cells

		/// <summary>
		/// Gets the <see cref="CellBaseCollection"/> that belongs to the <see cref="GroupByRow"/>.
		/// </summary>
		public override CellBaseCollection Cells
		{
			get
			{
				if (this._cells == null)
					this._cells = new SingleCellBaseCollection<GroupByCell>(((RowsManager)this.Manager).GroupByColumn ,this.Columns, this);
				return this._cells;
			}
		}

		#endregion // Cells

		#region VisibleCells
		/// <summary>
		/// Gets a list of cells that are visible. 
		/// </summary>
		protected internal override Collection<CellBase> VisibleCells
		{
			get
			{
				return new Collection<CellBase>() { this.Cells[0] };
			}
		}
		#endregion // VisibleCells

		#region CanScrollHorizontally
		/// <summary>
		/// Gets whether or not a row will ever need to scroll horizontally. 
		/// </summary>
		protected internal override bool CanScrollHorizontally
		{
			get { return false; }
		}

		#endregion // CanScrollHorizontally

		#region ChildBands

		/// <summary>
		/// This property is not supported on a <see cref="GroupByRow"/>
		/// </summary>
		[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		public override ChildBandCollection ChildBands
		{
			get
			{
				return base.ChildBands;
			}
		}

		#endregion // ChildBands

		#region HeightResolved

		/// <summary>
		/// Resolves the <see cref="RowBase.Height"/> property for this Row.
		/// </summary>
		public override RowHeight HeightResolved
		{
			get
			{
				if (this.Height != null)
					return (RowHeight)this.Height;
				else
					return this.ColumnLayout.GroupBySettings.GroupByRowHeightResolved;
			}
		}
		#endregion // HeightResolved

		#region OnItemSourceChanged

		/// <summary>
		/// Invoked when the ItemSource property changes.
		/// </summary>
		protected override void OnItemSourceChanged()
		{
			base.OnItemSourceChanged();

			if (this.ChildRowsManager != null)
				((RowsManager)this.ChildRowsManager).ItemsSource = this.ItemSource;
		}

		#endregion // OnItemSourceChanged

        #region CanBeDeleted

        /// <summary>
        /// Gets whether the <see cref="Row"/> can actually be deleted. 
        /// </summary>
        protected internal override bool CanBeDeleted
        {
            get { return false; }
        }

        #endregion // CanBeDeleted

		#endregion // Overrides
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