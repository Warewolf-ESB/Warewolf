using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Infragistics.Controls.Grids.Primitives;

namespace Infragistics.Controls.Grids
{
	/// <summary>
	/// The base class for managament of all <see cref="RowBase"/> objects of the <see cref="XamGrid"/>.
	/// </summary>
	public abstract class RowsManagerBase : IDisposable, IComparable<RowsManagerBase> 
	{
		#region Members

		List<RowsManagerBase> _visibleChildManagers;
		ReadOnlyCollection<RowsManagerBase> _readOnlyVisibleChildManagers;
		ColumnLayout _columnLayout;
		bool _isDisposed;
        Dictionary<Column, double> _starColumnWidths;
		
		#endregion // Members

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="RowsManagerBase"/> class.
		/// </summary>
		/// <param propertyName="columnLayout">The <see cref="ColumnLayout"/> that this <see cref="RowsManagerBase"/> represents.</param>
		protected RowsManagerBase(ColumnLayout columnLayout)
		{
			this.ColumnLayout = columnLayout;
			this._visibleChildManagers = new List<RowsManagerBase>();
			this._readOnlyVisibleChildManagers = new ReadOnlyCollection<RowsManagerBase>(this._visibleChildManagers);
			this.OverrideHorizontalMax = -1;

            this._starColumnWidths = new Dictionary<Column, double>();
		}

		#endregion // Constructor

		#region Properties

		#region Public

		#region ParentRow

		/// <summary>
		/// The <see cref="ParentRow"/> that owns this <see cref="RowsManagerBase"/>.
		/// </summary>
		public virtual RowBase ParentRow
		{
			get;
			protected set;
		}

		#endregion // ParentRow

		#region ColumnLayout

		/// <summary>
		/// Gets the <see cref="ColumnLayout"/> object that is associated with the <see cref="RowsManagerBase"/>.
		/// </summary>
		public ColumnLayout ColumnLayout
		{
			get { return this._columnLayout; }
			protected internal set
			{
				if (value != this.ColumnLayout)
				{
					this._isDisposed = false;
					this.OnColumnLayoutAssigned(value);
				}			
			}
		}
		
		#endregion // ColumnLayout

		#region Level

		/// <summary>
		/// Gets the level in the hierarchy of the <see cref="RowsManagerBase"/>.
		/// </summary>
		public virtual int Level
		{
			get;
			protected set;
		}

		#endregion // Level

		#region Rows

		/// <summary>
		/// Gets the collection of <see cref="RowBase"/> objects that this <see cref="RowsManagerBase"/> represents.
		/// </summary>
		public abstract RowBaseCollection Rows
		{
			get;
		}
		#endregion // Rows

		#endregion // Public

		#region Protected

		#region FullRowCount

		/// <summary>
		/// Gets the total amount of rows that can be displayed for the <see cref="RowsManagerBase"/>.
		/// </summary>
		protected internal abstract int FullRowCount
		{
			get;
		}

		#endregion // FullRowCount

		#region VisibleChildRowsManagers

		/// <summary>
		/// Gets a list of currently visible child <see cref="RowsManagerBase"/> objects.
		/// </summary>
		protected internal ReadOnlyCollection<RowsManagerBase> VisibleChildManagers
		{
			get
			{
				return this._readOnlyVisibleChildManagers;
			}
		}

		#endregion // VisibleChildManagers

		#endregion // Protected

		#region Internal

		internal int VisibleCellCount
		{
			get;
			set;
		}

		internal int ScrollableCellCount
		{
			get;
			set;
		}

        internal bool InvalidateOverrideHorizontalMax
        {
            get;
            set;
        }

		internal double OverrideHorizontalMax
		{
			get;
			set;
		}

		internal double OverflowAdjustment
		{
			get;
			set;
		}

		internal bool IsFirstRowRenderingInThisLayoutCycle
		{
			get;
			set;
		}

		internal int IndexOfFirstColumnRendered
		{
			get;
			set;
		}

		internal int TotalColumnsRendered
		{
			get;
			set;
		}

		internal double RowWidth
		{
			get;
			set;
		}

		internal double CachedIndentation
		{
			get;
			set;
		}

		internal double CurrentVisibleWidth
		{
			get;
			set;
		}

        internal Dictionary<Column, double> StarColumnWidths
        {
            get { return this._starColumnWidths; }
        }

        Dictionary<Column, List<RenderMergeInfo>> _renderedMergedCells = new Dictionary<Column, List<RenderMergeInfo>>();
        internal Dictionary<Column, List<RenderMergeInfo>> RenderedMergedCells
        {
            get { return this._renderedMergedCells; }
        }

        #endregion // Internal

		#endregion // Properties

		#region Methods

		#region Public

		#region InvalidateRows
		/// <summary>
		/// Clears rows and child rows and lets the grid rerender.
		/// </summary>
		public virtual void InvalidateRows()
		{

		}
		#endregion

        #region RefreshSummaries

        /// <summary>
        /// Reevaluates the data for the summaries on the rows.
        /// </summary>
        public virtual void RefreshSummaries()
        {

        }

        #endregion // RefreshSummaries

        #endregion // Public

        #region Protected

        #region ResolveRowForIndex

        /// <summary>
		/// Returns the <see cref="RowBase"/> for the given index. 
		/// </summary>
		/// <param propertyName="index">The index of the row to retrieve.</param>
		/// <returns></returns>
		protected internal abstract RowBase ResolveRowForIndex(int index);

		#endregion // ResolveRowForIndex

		#region ResolveIndexForRow

		/// <summary>
		/// Returns the index for a given row.
		/// </summary>
		/// <param propertyName="row">The row whose index should be returned.</param>
		/// <returns></returns>
		protected internal abstract int ResolveIndexForRow(RowBase row);

		#endregion // ResolveIndexForRow

		#region ResolveIndentation

		/// <summary>
		/// Returns the amount of indentation that should be allocated for the <see cref="RowsManagerBase"/>.
		/// </summary>
		/// <param propertyName="row">The row that is currently being indented.</param>
		/// <returns></returns>
		protected internal virtual double ResolveIndentation(RowBase row)
		{
			double indentation = 0;
			if (this.ParentRow != null)
				indentation += this.ColumnLayout.IndentationResolved + this.ParentRow.Manager.ResolveIndentation(this.ParentRow);

			return indentation;
		}

		#endregion // ResolveIndentation

		#region RegisterChildRowsManager

		/// <summary>
		/// Adds the specified <see cref="RowsManagerBase"/> as a visible child manager, so that it will be considered
		/// in the rendering of rows.
		/// </summary>
		/// <param propertyName="manager"></param>
		protected internal void RegisterChildRowsManager(RowsManagerBase manager)
		{
			if (!this.VisibleChildManagers.Contains(manager))
			{
				this._visibleChildManagers.Add(manager);
				this._visibleChildManagers.Sort();
				manager.OnRegisteredAsVisibleChildManager();
			}
		}

		#endregion // RegisterChildRowsManager               

		#region UnregisterChildRowsManager

		/// <summary>
		/// Removes the specified <see cref="RowsManagerBase"/> as a visible child manager, so that it will no longer be considered
		/// in the rendering of rows.
		/// </summary>
		/// <param propertyName="manager"></param>
		protected internal void UnregisterChildRowsManager(RowsManagerBase manager)
		{
			if(this.VisibleChildManagers.Contains(manager))
			{
				this._visibleChildManagers.Remove(manager);
				manager.OnUnregisteredAsVisibleChildManager();
			}
		}

		#endregion // UnregisterChildRowsManager

		#region UnregisterAllChildRowsManager

		/// <summary>
		/// Removes all visible child managers, so that they will no longer be considered
		/// in the rendering of rows.
		/// </summary>
		protected internal void UnregisterAllChildRowsManager()
		{
			for (int i = this.VisibleChildManagers.Count - 1; i >= 0; i--)
				this.UnregisterChildRowsManager(this.VisibleChildManagers[i]);
		}

		#endregion // UnregisterAllChildRowsManager

		#region OnRegisteredAsVisibleChildManager

		/// <summary>
		/// Invoked when a <see cref="RowsManagerBase"/> is now visible, meaning it's Parent row is expanded. 
		/// </summary>
		protected virtual void OnRegisteredAsVisibleChildManager()
		{
		}

		#endregion // OnRegisteredAsVisibleChildManager

		#region OnUnregisteredAsVisibleChildManager

		/// <summary>
		/// Invoked when a <see cref="RowsManagerBase"/> is no longer visible, meaning it's Parent row is collapsed. 
		/// </summary>
		protected virtual void OnUnregisteredAsVisibleChildManager()
		{
		}

		#endregion // OnUnregisteredAsVisibleChildManager
		
		#region OnColumnLayoutAssigned

		/// <summary>
		/// Called when the ColumnLayout assigned to this <see cref="RowsManagerBase"/> changes.
		/// </summary>
		/// <param propertyName="layout"></param>
		protected virtual void OnColumnLayoutAssigned(ColumnLayout layout)
		{
			if (this._columnLayout != null)
			{
				this._columnLayout.PropertyChanged -= OnColumnLayoutPropertyChanged;
				this._columnLayout.ChildColumnLayoutRemoved -= ColumnLayout_ChildColumnLayoutRemoved;
				this._columnLayout.ChildColumnLayoutAdded -= ColumnLayout_ChildColumnLayoutAdded;
				this._columnLayout.ChildColumnLayoutVisibilityChanged -= ColumnLayout_ChildColumnLayoutVisibilityChanged;
				this._columnLayout.ColumnLayoutDisposing -= ColumnLayout_ColumnLayoutDisposing;
                this._columnLayout.ColumnLayoutReset -= ColumnLayout_ColumnLayoutReset;

			}

			this._columnLayout = layout;

			if (this._columnLayout != null)
			{
				this._columnLayout.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(OnColumnLayoutPropertyChanged);
				this._columnLayout.ChildColumnLayoutRemoved += new EventHandler<ColumnLayoutEventArgs>(ColumnLayout_ChildColumnLayoutRemoved);
				this._columnLayout.ChildColumnLayoutAdded += new EventHandler<ColumnLayoutEventArgs>(ColumnLayout_ChildColumnLayoutAdded);
				this._columnLayout.ChildColumnLayoutVisibilityChanged += new EventHandler<ColumnLayoutEventArgs>(ColumnLayout_ChildColumnLayoutVisibilityChanged);
				this._columnLayout.ColumnLayoutDisposing += new EventHandler<EventArgs>(ColumnLayout_ColumnLayoutDisposing);
                this._columnLayout.ColumnLayoutReset += ColumnLayout_ColumnLayoutReset;
			}
		}

		#endregion // OnColumnLayoutAssigned

		#region OnColumnLayoutPropertyChanged

		/// <summary>
		/// Raised when a property has changed on the ColumnLayout that this <see cref="RowsManagerBase"/> represents.
		/// </summary>
		/// <param propertyName="layout"></param>
		/// <param propertyName="propertyName"></param>
		protected virtual void OnColumnLayoutPropertyChanged(ColumnLayout layout, string propertyName)
		{
			if (propertyName == "InvalidateData")
				this.InvalidateData();
		}

		#endregion // OnColumnLayoutPropertyChanged

		#region OnChildColumnLayoutRemoved

		/// <summary>
		/// Raised when a <see cref="ColumnLayout"/> is removed from the owning ColumnLayout's Columns collection.
		/// </summary>
		/// <param propertyName="layout">The <see cref="ColumnLayout"/> being removed.</param>
		protected virtual void OnChildColumnLayoutRemoved(ColumnLayout layout)
		{

		}
		#endregion // OnChildColumnLayoutRemoved

		#region OnChildColumnLayoutAdded

		/// <summary>
		/// Raised when a <see cref="ColumnLayout"/> is added to the owning ColumnLayout's Columns collection.
		/// </summary>
		/// <param propertyName="layout">The <see cref="ColumnLayout"/> being added.</param>
		protected virtual void OnChildColumnLayoutAdded(ColumnLayout layout)
		{

		}
		#endregion // OnChildColumnLayoutAdded

		#region OnChildColumnLayoutVisibilityChanged

		/// <summary>
		/// Raised when a child <see cref="ColumnLayout"/> of the owning ColumnLayout, visibility changes.
		/// </summary>
		/// <param propertyName="layout">The <see cref="ColumnLayout"/> that had it's Visibility changed.</param>
		protected virtual void OnChildColumnLayoutVisibilityChanged(ColumnLayout layout)
		{

		}
		#endregion // OnChildColumnLayoutVisibilityChanged

		#region InvalidateData

		/// <summary>
		/// Triggers all Data operations such as sorting and GroupBy to be invalidated. 
		/// </summary>
		protected virtual void InvalidateData()
		{

		}

		#endregion // InvalidateData

		#region UnregisterRowsManager

		/// <summary>
		/// When a RowsManager is no longer needed, this method should be called, to detach all events that are hooked up. 
		/// To avoid Memory leaks.
		/// </summary>
		/// <param name="removeColumnLayout">Whether the ColumnLayout should be removed, or just its events.</param>
		/// <param name="clearChildRowsManager">Whether the ChildRowsManager should be disposed of on each row.</param>
        /// <param name="clearSelection">Whether the selected items should be unselected</param>
		protected internal virtual void UnregisterRowsManager(bool removeColumnLayout, bool clearChildRowsManager, bool clearSelection)
		{
			if (this._columnLayout != null)
			{
				this._columnLayout.PropertyChanged -= OnColumnLayoutPropertyChanged;
				this._columnLayout.ChildColumnLayoutRemoved -= ColumnLayout_ChildColumnLayoutRemoved;
				this._columnLayout.ChildColumnLayoutAdded -= ColumnLayout_ChildColumnLayoutAdded;
				this._columnLayout.ChildColumnLayoutVisibilityChanged -= ColumnLayout_ChildColumnLayoutVisibilityChanged;

				if (removeColumnLayout)
				{
					this._columnLayout.ColumnLayoutDisposing -= ColumnLayout_ColumnLayoutDisposing;
                    this._columnLayout.ColumnLayoutReset -= ColumnLayout_ColumnLayoutReset;
					this._columnLayout = null;
					this._isDisposed = true;
				}

			}
		}

		#endregion // UnregisterRowsManager

		#region GetRowScopedConditions

		/// <summary>
		/// Creates a collection of <see cref="IConditionalFormattingRuleProxy"/> objects scoped to <see cref="StyleScope"/>.Row.
		/// </summary>
		/// <returns></returns>
		public virtual ReadOnlyCollection<IConditionalFormattingRuleProxy> GetRowScopedConditions()
		{
			return null;
		}

		#endregion // GetRowScopedConditions

		#region GetCellScopedConditions

		/// <summary>
		/// Creates a collection of <see cref="IConditionalFormattingRuleProxy"/> objects scoped to <see cref="StyleScope"/>.Cell for a given <see cref="Column"/>.
		/// </summary>
		/// <returns></returns>
		public virtual ReadOnlyCollection<IConditionalFormattingRuleProxy> GetCellScopedConditions(string columnKey)
		{
			return null;
		}

		#endregion // GetCellScopedConditions

        #region SortVisibleChildManagers
        /// <summary>
        /// Sorts the VisibleChildmanagers
        /// </summary>
        protected void SortVisibleChildManagers()
        {
            this._visibleChildManagers.Sort();
        }
        #endregion // SortVisibleChildManagers

        #region OnColumnLayoutReset

        /// <summary>
        /// Raised when the ColumnLayout wasn't removed, but it's data has been reset.
        /// </summary>
        protected virtual void OnColumnLayoutReset()
        {

        }

        #endregion // OnColumnLayoutReset

        #endregion // Protected

        #endregion // Methods

        #region Overrides

        #region ToString


#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

        #endregion // ToString

        #endregion // Overrides

        #region EventHandlers

        #region OnColumnLayoutPropertyChanged

        private void OnColumnLayoutPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (!this._isDisposed)
			{
			    this.OnColumnLayoutPropertyChanged((ColumnLayout)sender, e.PropertyName);
			}
		}

		#endregion // OnColumnLayoutPropertyChanged

		#region ColumnLayout_ChildColumnLayoutAdded

		void ColumnLayout_ChildColumnLayoutAdded(object sender, ColumnLayoutEventArgs e)
		{
			this.OnChildColumnLayoutAdded(e.ColumnLayout);
		}

		#endregion // ColumnLayout_ChildColumnLayoutAdded

		#region ColumnLayout_ChildColumnLayoutRemoved

		void ColumnLayout_ChildColumnLayoutRemoved(object sender, ColumnLayoutEventArgs e)
		{
			this.OnChildColumnLayoutRemoved(e.ColumnLayout);
		}

		#endregion // ColumnLayout_ChildColumnLayoutRemoved

		#region ColumnLayout_ChildColumnLayoutVisibilityChanged

		void ColumnLayout_ChildColumnLayoutVisibilityChanged(object sender, ColumnLayoutEventArgs e)
		{
			this.OnChildColumnLayoutVisibilityChanged(e.ColumnLayout);
		}

		#endregion // ColumnLayout_ChildColumnLayoutVisibilityChanged

        #region ColumnLayout_ColumnLayoutDisposing

        void ColumnLayout_ColumnLayoutDisposing(object sender, EventArgs e)
		{
			this.UnregisterRowsManager(true, true, true);
		}

        #endregion // ColumnLayout_ColumnLayoutDisposing

        #region ColumnLayout_ColumnLayoutReset

        void ColumnLayout_ColumnLayoutReset(object sender, EventArgs e)
        {
            this.OnColumnLayoutReset();
        }

        #endregion // ColumnLayout_ColumnLayoutReset

        #endregion // EventHandlers

        #region IDisposable Members

        /// <summary>
		/// Releases the unmanaged resources used by the <see cref="RowsManagerBase"/> and optionally
		/// releases the managed resources.
		/// </summary>
		/// <param propertyName="disposing">
		/// true to release both managed and unmanaged resources; 
		/// false to release only unmanaged resources.
		/// </param>
		protected virtual void Dispose(bool disposing)
		{

		}

		/// <summary>
		/// Releases the unmanaged and managed resources used by the <see cref="RowsManagerBase"/>.
		/// </summary>
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion

		#region IComparable<RowsManagerBase> Members

		/// <summary>
		/// Should be overriden by a base class to determine which RowsManager should be displayed first.
		/// </summary>
		/// <param propertyName="other"></param>
		/// <returns>
		/// A signed number indicating the relative values of this instance and value. 
		/// Return FilterActionValue Description: 
		/// Less than zero This instance is less than value. 
		/// Zero This instance is equal to value. 
		/// Greater than zero This instance is greater than value. -or- value is null. 
		/// </returns>
		protected abstract int CompareTo(RowsManagerBase other);

		int IComparable<RowsManagerBase>.CompareTo(RowsManagerBase other)
		{
			return this.CompareTo(other);
		}

		#endregion // IComparable<RowsManagerBase> Members			
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