
using Infragistics.Controls.Grids.Primitives;
namespace Infragistics.Controls.Grids
{
	/// <summary>
	/// A <see cref="RowsManagerBase"/> that manages <see cref="Row"/> objects for the root level of the <see cref="XamGrid"/>.
	/// </summary>
	public class XamGridRowsManager : RowsManager
	{
		#region Members

		XamGrid _grid;
		GroupByAreaRow _groupByAreaRow;
		GroupByColumn _groupByAreaColumn;


		#endregion // Members

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="XamGridRowsManager"/> class.
		/// </summary>
		/// <param propertyName="grid">The <see cref="XamGrid"/> that owns this <see cref="XamGridRowsManager"/></param>
		public XamGridRowsManager(XamGrid grid)
			: base(0, new ColumnLayout(grid) { Key = "XamWebGrid_Root_XamWebGrid" }, null)
		{
			this._grid = grid;
		}
		#endregion // Constructor

		#region Properties

		#region Public

		#region GroupByAreaRow

		/// <summary>
		/// Gets a reference to the <see cref="GroupByAreaRow"/> for this <see cref="XamGridRowsManager"/>
		/// </summary>
		public GroupByAreaRow GroupByAreaRow
		{
			get
			{
				if (this._groupByAreaRow == null)
					this._groupByAreaRow = new GroupByAreaRow(this);
				return this._groupByAreaRow;
			}
		}

		#endregion // GroupByAreaRow

		#region GroupByAreaColumn

		/// <summary>
		/// Gets the <see cref="GroupByColumn"/> that represents all <see cref="GroupByAreaCell"/> objects as this GroupByLevel.
		/// </summary>
		internal GroupByColumn GroupByAreaColumn
		{
			get
			{
				if (this._groupByAreaColumn == null)
				{
					this._groupByAreaColumn = new GroupByColumn();
					this._groupByAreaColumn.ColumnLayout = this.ColumnLayout;
				}
				return this._groupByAreaColumn;
			}
		}
		#endregion // GroupByAreaColumn

		#endregion // Public

        #region Internal

        internal XamGrid Grid
        {
            get { return this._grid; }
            set { this._grid = value; }
        }

        #endregion // Internal

        #endregion // Properties

        #region Overrides

        #region FooterSupported
        /// <summary>
		/// Gets if the <see cref="RowsManager"/> supports showing the footer in the general rows body.
		/// </summary>
		protected override bool FooterSupported
		{
			get
			{
				return false;
			}
		}
		#endregion // FooterSupported

		#region HeaderSupported
		/// <summary>
		/// Gets if the <see cref="RowsManager"/> supports showing the header in the general rows body.
		/// </summary>
		protected override bool HeaderSupported
		{
			get
			{
				return false;
			}
		}
		#endregion // HeaderSupported

		#region RegisterTopRow

		/// <summary>
		/// Registers a <see cref="RowBase"/> as a fixed row that should be displayed above all other rows, such as the <see cref="HeaderRow"/>
		/// </summary>
		/// <param propertyName="row"></param>
		protected override void RegisterTopRow(RowBase row)
		{
			if (this._grid != null && this._grid.Panel != null)
			{
				this._grid.Panel.RegisterFixedRow(row, FixedRowAlignment.Top);
				base.RegisterTopRow(row);
			}
		}
		#endregion // RegisterTopRow

		#region RegisterBottomRow

		/// <summary>
		/// Registers a <see cref="RowBase"/> as a fixed row that should be displayed below all other rows, such as the <see cref="FooterRow"/>
		/// </summary>
		/// <param propertyName="row"></param>
		protected override void RegisterBottomRow(RowBase row)
		{
			if (this._grid != null && this._grid.Panel != null)
			{
				this._grid.Panel.RegisterFixedRow(row, FixedRowAlignment.Bottom);
				base.RegisterBottomRow(row);
			}
		}
		#endregion // RegisterBottomRow

		#region UnregisterTopRow

		/// <summary>
		/// Unregisters a <see cref="RowBase"/> that was registered to be displayed above of all other rows..
		/// </summary>
		/// <param propertyName="row"></param>
		protected override void UnregisterTopRow(RowBase row)
		{
			if (this._grid != null && this._grid.Panel != null)
				this._grid.Panel.UnregisterFixedRow(row);
		}
		#endregion // UnregisterTopRow

		#region UnregisterBottomRow

		/// <summary>
		/// Unregisters a <see cref="RowBase"/> that was registered to be displayed below all other rows.
		/// </summary>
		/// <param propertyName="row"></param>
		protected override void UnregisterBottomRow(RowBase row)
		{
			if (this._grid != null && this._grid.Panel != null)
				this._grid.Panel.UnregisterFixedRow(row);
		}
		#endregion // UnregisterBottomRow

		#region ResolveRowForIndex

		/// <summary>
		/// Returns the <see cref="RowBase"/> for the given index. 
		/// </summary>
		/// <param propertyName="index">The index of the row to retrieve.</param>
		/// <returns></returns>
		protected internal override RowBase ResolveRowForIndex(int index)
		{
			return this.Rows[index];
		}
		#endregion // ResolveRowForIndex		

		#region InvalidateGroupBy

		/// <summary>
		/// Validates whether a <see cref="RowsManager"/> needs to group its rows.
		/// </summary>
		protected override void InvalidateGroupBy(bool reset)
		{
            if (this.Grid != null && this.Grid.IsLoaded)
            {
                base.InvalidateGroupBy(reset);

                if (this.GroupByLevel == -1)
                {
                    this.InvalidateHeaderRowVisibility();
                    this.InvalidateAddNewRowVisibility(false);
                    this.InvalidateFooterRowVisibility();
                    this.InvalidateFilterRowVisibility(false);
                    this.InvalidateSummaryRowVisibility();
                }
                else
                {
                    this.UnregisterTopRow(this.HeaderRow);
                    this.UnregisterTopRow(this.AddNewRowTop);
                    this.UnregisterTopRow(this.FilterRowTop);
                    this.UnregisterTopRow(this.SummaryRowTop);

                    this.UnregisterBottomRow(this.FooterRow);
                    this.UnregisterBottomRow(this.AddNewRowBottom);
                    this.UnregisterBottomRow(this.FilterRowBottom);
                    this.UnregisterBottomRow(this.SummaryRowBottom);

                }
            }
		}
		#endregion // InvalidateGroupBy

		#region InvalidateGroupByRowAreaVisibility

		/// <summary>
		/// Determines if the <see cref="GroupByAreaRow"/> should be visible, and registers/unregisters it accordingly.
		/// </summary>
		protected virtual void InvalidateGroupByAreaRowVisibility()
		{
			if (this._grid !=null)
			{
				switch (this._grid.GroupBySettings.AllowGroupByArea)
				{
					case GroupByAreaLocation.Top:
						this.UnregisterBottomRow(this.GroupByAreaRow);
						this.RegisterTopRow(this.GroupByAreaRow);
						break;
					case GroupByAreaLocation.Bottom:
						this.UnregisterTopRow(this.GroupByAreaRow);
						this.RegisterBottomRow(this.GroupByAreaRow);
						break;
					case GroupByAreaLocation.Hidden:
						this.UnregisterTopRow(this.GroupByAreaRow);
						this.UnregisterBottomRow(this.GroupByAreaRow);
						break;
				}
			}
		}

		#endregion // InvalidateGroupByRowAreaVisibility

		#region OnColumnLayoutPropertyChanged

		/// <summary>
		/// Raised when a property has changed on the ColumnLayout that this <see cref="XamGridRowsManager"/> represents.
		/// </summary>
		/// <param propertyName="layout"></param>
		/// <param propertyName="propertyName"></param>
		protected override void OnColumnLayoutPropertyChanged(ColumnLayout layout, string propertyName)
		{
			switch (propertyName)
			{
				case ("AllowGroupByArea"): 
					{
						this.InvalidateGroupByAreaRowVisibility();
						break;
					}
			}
			base.OnColumnLayoutPropertyChanged(layout, propertyName);
		}

		#endregion // OnColumnLayoutPropertyChanged

		#region InvalidateTopAndBottomRows

		/// <summary>
		/// Evaluates all header and footer rows, and determines if they should be hidden or visible.
		/// </summary>
		public override void InvalidateTopAndBottomRows()
		{
			if (this.GroupByLevel == -1)
			{
				base.InvalidateTopAndBottomRows();                
			}
			else
			{
				this.UnregisterTopRow(this.HeaderRow);
				this.UnregisterTopRow(this.AddNewRowTop);
				this.UnregisterTopRow(this.FilterRowTop);

				this.UnregisterBottomRow(this.FooterRow);
				this.UnregisterBottomRow(this.AddNewRowBottom);
				this.UnregisterBottomRow(this.FilterRowBottom);

                this.InvalidatePagerRowVisibility();
			}
			this.InvalidateGroupByAreaRowVisibility();
		}

		#endregion // InvalidateTopAndBottomRows

        #region OnCurrentItemChanged

        /// <summary>
        /// Raised when the underlying data sources current item changes.
        /// </summary>
        /// <param name="data"></param>
        protected override void OnCurrentItemChanged(object data)
        {
            if (this.Grid != null)
                this.Grid.ActiveItem = data;
        }

        #endregion // OnCurrentItemChanged

        #region ResetAddNewRows
        /// <summary>
        /// Assigns a new data object to the <see cref="AddNewRow"/> object.
        /// </summary>
        protected internal override void ResetAddNewRows(bool generateNewData)
        {
            base.ResetAddNewRows(generateNewData);
            if (generateNewData)
            {
                if (this.AddNewRowTop.IsActive || this.AddNewRowBottom.IsActive)
                {
                    this.Grid.IgnoreActiveItemChanging = true;
                    this.Grid.ActiveItem = this.AddNewRowTop.Data;
                    this.Grid.IgnoreActiveItemChanging = false;
                }

            }
        }
        #endregion // ResetAddNewRows

		#endregion // Overrides

        #region Methods

        #region Internal

        internal void InitData()
        {
            this.InitializeData();
        }

        internal void UpdateCurrentItem(object item)
        {
            DataManagerBase dm = this.DataManager;
            if (dm != null)
                dm.UpdateCurrentItem(item);
        }

        #endregion // Internal

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