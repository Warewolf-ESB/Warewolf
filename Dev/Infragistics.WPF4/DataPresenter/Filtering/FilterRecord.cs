using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Shapes;
using System.Windows.Navigation;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Automation;
using System.Windows.Automation.Provider;
using System.Windows.Threading;
using System.Diagnostics;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Infragistics.Shared;
using Infragistics.Windows.DataPresenter;
using Infragistics.Windows.Helpers;
using Infragistics.Windows.Selection;
using Infragistics.Windows.Themes;
using Infragistics.Windows.Controls;
using Infragistics.Windows.Internal;
using Infragistics.Windows.DataPresenter.Internal;
using Infragistics.Windows.Reporting;
using Infragistics.Windows.DataPresenter.Events;
using Infragistics.Windows.Editors;
using System.Windows.Markup;
using System.ComponentModel;

namespace Infragistics.Windows.DataPresenter
{
	#region FilterRecord Class

	/// <summary>
	/// A filter record lets the user filter data records by specifying filter criteria for one or more fields.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// <b>FilterRecord</b> lets the user filter data records by specifying filter criteria for one or more fields.
	/// Filter criteria for a field is specified via the associated cell in the filter record.
	/// Filter record displays a filter cell for each field in the associated field layout. Each filter cell lets you 
	/// select filter criteria for the associated field. Each cell typically has UI
	/// for selecting a filter operator (like Equals, GreaterThan, StartsWith etc...) and UI for entering
	/// filter operand, which is the filter value to compare against. An operator combined with an operand forms a
	/// filter condition. Data records with cell values that do not match the condition will be filtered-out 
	/// (hidden) by default. A filter cell can also have a button to clear previously entered filter criteria.
	/// </para>
	/// <para class="body">
	/// Filter record is enabled by setting the <see cref="FieldSettings.AllowRecordFiltering"/> property to
	/// true and <see cref="FieldLayoutSettings.FilterUIType"/> to <b>FilterRecord</b>.
	/// </para>
	/// <para class="body">
	/// Settings are available to control various aspects of the filter record and filter cells.
	/// <see cref="FieldSettings.FilterOperatorDropDownItems"/> controls the filter operators that the filter cell
	/// will make available to the user for selection. <see cref="FieldSettings.FilterOperandUIType"/> controls
	/// type of UI used for the operand portion in the filter cell. <see cref="FieldSettings.FilterOperatorDefaultValue"/>
	/// controls the initial value that will be pre-selected in the filter operator UI.
	/// </para>
	/// </remarks>
	/// <seealso cref="FieldSettings.AllowRecordFiltering"/>
	/// <seealso cref="FieldLayoutSettings.FilterUIType"/>
	/// <seealso cref="FieldSettings.FilterOperandUIType"/>
	/// <seealso cref="FieldSettings.FilterOperatorDefaultValue"/>
	/// <seealso cref="FieldSettings.FilterOperatorDropDownItems"/>
	/// <seealso cref="FieldSettings.FilterClearButtonVisibility"/>
	/// <seealso cref="FieldLayoutSettings.FilterAction"/>
	/// <seealso cref="FieldLayoutSettings.FilterClearButtonLocation"/>
	/// <seealso cref="RecordFilterCollection"/>
	/// <seealso cref="FieldLayout.RecordFilters"/>
	/// <seealso cref="RecordManager.RecordFilters"/>
	/// <seealso cref="DataRecord.IsFilteredOut"/>
	/// <seealso cref="GroupByRecord.IsFilteredOut"/>
	/// <seealso cref="FieldLayoutSettings.RecordFilterScope"/>
	public class FilterRecord : DataRecord
	{
		#region Member Vars

		private bool _cachedHasActiveFilters;

		// JJD 2/14/11 - TFS56924
		private PropertyValueTracker _filterVersionTracker;

		#endregion // Member Vars

		#region Constructors

		/// <summary>
		/// Constructor. Initializes a new instance of <see cref="FilterRecord"/> class.
		/// </summary>
		/// <param name="fieldLayout">Associated field layout.</param>
		/// <param name="parentCollection">Parent record collection.</param>
        internal FilterRecord( FieldLayout fieldLayout, RecordCollectionBase parentCollection )
			: base( fieldLayout, parentCollection )
        {
        }

        #endregion //Constructors

        #region Base class overrides

			// JJD 09/22/11  - TFS84708 - Optimization
			#region ChildRecordsIfNeeded

		internal override RecordCollectionBase ChildRecordsIfNeeded { get { return null; } }

			#endregion //ChildRecordsIfNeeded

            #region ChildRecordsInternal

        internal override RecordCollectionBase ChildRecordsInternal { get { return null; } }

            #endregion //ChildRecordsInternal	

			#region CreateCellCollection

		/// <summary>
		/// Overridden. Creates a new FilterCellCollection.
		/// </summary>
		/// <returns>A new FilterCellCollection instance.</returns>
		internal override CellCollection CreateCellCollection( )
		{
			return new FilterCellCollection( this );
		}

			#endregion // CreateCellCollection

            #region Description

        /// <summary>
        /// Gets/sets the description for the row
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override string Description
        {
            get
            {
				string ret = base.Description;
				if ( !string.IsNullOrEmpty( ret ) )
					return ret;

				return DataPresenterBase.GetString( "SR_FilterRecordDescription" );
            }
            set
            {
                base.Description = value;
            }
        }

            #endregion // Description

            #region GetAssociatedRecord


        // JJD 11/24/09 - TFS25215 - made public 
        /// <summary>
        /// Returns the associated record from the UI <see cref="DataPresenterBase"/> during a print or export operation. 
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> during a print or export operation clones of records are made that are used only during the operation. This method returns the source record this record was cloned from.</para>
        /// </remarks>
        /// <returns>The associated record from the UI DataPresenter or null.</returns>
        //internal override Record GetAssociatedRecord()
        public override Record GetAssociatedRecord()
        {
			ViewableRecordCollection vrc = this.ParentCollection.ViewableRecords;

			if ( vrc != null )
				return vrc.GetAssociatedFilterRecord( this );

			return null;
        }

            #endregion //GetAssociatedRecord

			#region GetCellClickActionResolved

		// SSP 9/9/09 TFS19158
		// Added GetCellClickActionResolved virtual method on the DataRecord.
		// 
		/// <summary>
		/// Overridden. Returns the resolved cell click action for the cell associated with
		/// this record and the specified field.
		/// </summary>
		/// <param name="field">Returned cell click action is for the cell of this field.</param>
		/// <returns>A resolved CellClickAction value.</returns>
		internal override CellClickAction GetCellClickActionResolved( Field field )
		{
			return CellClickAction.EnterEditModeIfAllowed;
		}

			#endregion // GetCellClickActionResolved

			#region GetCellValue

		/// <summary>
		/// Returns the value of a filter cell.
		/// </summary>
		/// <param name="field">Value of the filter cell associated with this field will be returned.</param>
		/// <param name="useConverter">Not used by the FilterRecord.</param>
		public override object GetCellValue( Field field, bool useConverter )
		{
			if ( field == null )
				throw new ArgumentNullException( "Field" );

			if ( field.Owner != this.FieldLayout )
				throw new ArgumentException( DataPresenterBase.GetString( "LE_ArgumentException_28" ) );

			FilterCell filterCell = (FilterCell)this.Cells[field];
            // JJD 2/17/09 - TFS14029
            // Use the new ValueInternal property instead of Value since that will now
            // simply call back into this method and cause a stack overflow
            //return filterCell.Value;
            return filterCell.ValueInternal;
		}

        // JJD 5/29/09 - TFS18063 - added
        /// <summary>
        /// Returns the value of a specific cell 
        /// </summary>
        /// <param name="field">The field whose value to get.</param>
        /// <param name="cellValueType">The type of value to return</param>
        /// <seealso cref="Field"/>
        /// <seealso cref="Field.Converter"/>
        /// <seealso cref="Infragistics.Windows.DataPresenter.FieldLayout.Fields"/>
        public override object GetCellValue(Field field, CellValueType cellValueType)
        {
            return this.GetCellValue(field, true);
        }

			#endregion // GetCellValue

			// AS 5/1/09
			// Since we have the GetEditorTypeResolved I thought we should probably get
			// the edit as type as well.
			//
			#region GetEditAsTypeResolved
		internal override Type GetEditAsTypeResolved(Field field)
		{
			if (null == field)
				return null;

			if (field.FilterOperandUITypeResolved == FilterOperandUIType.UseFieldEditor)
				return base.GetEditAsTypeResolved(field);

			return field.FilterEditAsTypeResolved;
		}
			#endregion //GetEditAsTypeResolved

			// AS 5/1/09
			// I took part of the GetIsEditingAllowed from the Field/Record and 
			// moved it into this helper method.
			//
			#region GetEditorTypeResolved
		internal override Type GetEditorTypeResolved(Field field)
		{
			if (null == field)
				return null;

			if (field.FilterOperandUITypeResolved == FilterOperandUIType.UseFieldEditor)
				return base.GetEditorTypeResolved(field);

			return field.FilterEditorTypeResolved;
		}
			#endregion //GetEditorTypeResolved 

			// AS 5/1/09
			// Originally GetIsEditingAllowed was defined on the Field and then the 
			// FilterCellValuePresenter overrode IsEditingAllowed instead of reworking 
			// the field's GetIsEditingAllowed. Now we have a virtual on the record and
			// can override it here.
			// 
			#region GetIsEditingAllowed
		internal override bool GetIsEditingAllowed(Field field)
		{
			if (null == field)
				return false;

			switch (field.FilterOperandUITypeResolved)
			{
				case FilterOperandUIType.Disabled:
				case FilterOperandUIType.None:
					return false;
			}

			if (null == this.GetEditorTypeResolved(field))
				return false;

			if (!this.IsEnabledResolved)
				return false;

			return true;
		} 
			#endregion //GetIsEditingAllowed

			#region HasChildrenInternal

        internal override bool HasChildrenInternal 
        { 
            get
            {
                return false;
            } 
        }

            #endregion //HasChildrenInternal	

			#region IsDataRecord

			// SSP 12/11/08 - NAS9.1 Record Filtering
			// 
			/// <summary>
			/// Overridden. Returns false since this is not a data record.
			/// </summary>
			// JJD 10/26/11 - TFS91364 - Make property public
			//internal override bool IsDataRecord
			public override bool IsDataRecord
			{
				get
				{
					return false;
				}
			}

			#endregion // IsDataRecord

			#region IsSelectable

            /// <summary>
            /// Property: Returns true only if the record can be selected
            /// </summary>
            internal protected override bool IsSelectable
            {
                get
                {
                     return false;
                }
            }

            #endregion // IsSelectable

			#region IsSpecialRecord

			// SSP 8/5/09 - NAS9.2 Enhanced grid view
			// Made public.
			// 
			//internal override bool IsSpecialRecord
			/// <summary>
			/// Overridden. Returns true since the filter record is a special record.
			/// </summary>
			public override bool IsSpecialRecord
			{
				get
				{
					return true;
				}
			}

			#endregion // IsSpecialRecord

            #region OnActiveRecordChanged

        internal override void OnActiveRecordChanged()
        {
            base.OnActiveRecordChanged();

            if (this.IsActive == false)
            {
                foreach (FilterCell cell in this.Cells)
                {
                    bool applied = false;

                    switch (cell.Field.FilterEvaluationTriggerResolved)
                    {
                        case FilterEvaluationTrigger.OnLeaveRecord:
                        case FilterEvaluationTrigger.OnEnterKeyOrLeaveRecord:
                            cell.RecordFilter.ParentCollection.ApplyPendingFilters();
                            applied = true;
                            break;
                    }

                    if (applied)
                        break;
                }
            }
        }

            #endregion //OnActiveRecordChanged	

			// JJD 2/14/11 - TFS56924 - added
			#region OnHasListenersChanged

		/// <summary>
		/// Called when the number of listeners to the property changed event goes to or from 0.
		/// </summary>
		protected override void OnHasListenersChanged()
		{
			base.OnHasListenersChanged();

			// JJD 2/14/11 - TFS56924
			// If  anyone is listening then create a tracker to get notified when the filter version changes
			// so we can keep the HasActiveFilters property in sync.
			if (this.HasListeners)
			{
				if (this._filterVersionTracker == null)
					this._filterVersionTracker = new PropertyValueTracker(this.Filters, ResolvedRecordFilterCollection.VersionProperty, this.VerifyActiveFilters);

				this.VerifyActiveFilters();
			}
			else
			{
				// JJD 2/14/11 - TFS56924
				// Since no one is listening null out the tracker
				this._filterVersionTracker = null;
			}
		}

			#endregion //OnHasListenersChanged	
    
            #region RecordType

        /// <summary>
        /// Returns the type of the record (read-only)
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override RecordType RecordType 
        { 
            get 
            {
				return RecordType.FilterRecord;
            } 
        }

            #endregion //RecordType

			#region SetCellValue

		/// <summary>
		/// Overridden. Sets the value of the filter cell.
		/// </summary>
		/// <param name="field">Value on the filter cell of this field will be set.</param>
		/// <param name="value">The new value.</param>
		/// <param name="useConverter">Not used for filter record.</param>
		/// <param name="errorInfo">Set to null since the filter record doesn't generated data errors</param>
		/// <param name="dontSetDataChanged">True to avoid setting the data changed flag</param>
        //// AS 4/15/09 Field.(Get|Set)CellValue
        //// Needed to return a boolean value.
        ////
        ////public override void SetCellValue( Field field, object value, bool useConverter )
		// AS 5/5/09 NA 2009.2 ClipboardSupport
		//public override bool SetCellValue( Field field, object value, bool useConverter )
		// SSP 3/19/12 - Calc manager support
		// Added an overload that takes 'dontSetDataChanged' flag.
		// 
		//internal override bool SetCellValue( Field field, object value, bool useConverter, out DataErrorInfo errorInfo )
		internal override bool SetCellValue( Field field, object value, bool useConverter, out DataErrorInfo errorInfo, bool dontSetDataChanged )
		{
			if ( field == null )
				throw new ArgumentNullException( "Field" );

			if ( field.Owner != this.FieldLayout )
				throw new ArgumentException(DataPresenterBase.GetString("LE_ArgumentException_28"));

			// AS 5/5/09 NA 2009.2 ClipboardSupport
			errorInfo = null;

			FilterCell filterCell = (FilterCell)this.Cells[field];
            // JJD 2/17/09 - TFS14029
            // Use the new ValueInternal property instead of Value since that will now
            // simply call back into this method and cause a stack overflow
            //filterCell.Value = value;
            filterCell.ValueInternal = value;

            // AS 4/15/09 Field.(Get|Set)CellValue
            return true;
		}

			#endregion //SetCellValue	

			// JM 10-13-10 TFS42835
			#region ShouldCollapseCell
		internal override bool ShouldCollapseCell(Field field)
		{
			if (field == null || field.Index < 0)
				return false;

			// AS 12/2/10 TFS61073
			// The fix for TFS42835 introduced this issue. Basically we started collapsing filter 
			// cells that didn't support filtering even when empty cells weren't support (e.g. in 
			// grid view) so the filter cells wouldn't line up with the regular cells. I think the 
			// right thing to do is to only collapse filter cells when filtering is not supported 
			// and the record has not been told to keep them expanded - which would only happen 
			// now if the user sets the ShouldCollapseEmptyCells to false. Also we should based 
			// this on the ui state which is represented by FilterOperandUITypeResolved being 
			// none since filtering may be allowed but the ui hidden for a given field.
			//
			//if ( false == field.AllowRecordFilteringResolved )
			//    return true;
			//
			//FieldLayout fl = field.Owner;
			//if ( fl == null || !fl.IsEmptyCellCollapsingSupportedByView )
			//    return false;
			//
			//return base.ShouldCollapseCell(field);
			if (!this.ShouldCollapseEmptyCellsResolved)
				return false;

			if (field.FilterOperandUITypeResolved != FilterOperandUIType.None)
				return false;

			return true;
		}
			#endregion ShouldCollapseCell

			// AS 12/2/10 TFS61073
			// The base impl will return false for special records. However, with the fix for 
			// TFS42835 we started allowing collapsing of filter cells - specifically cells 
			// for which there was filtering was not allowed.
			//
			#region ShouldCollapseEmptyCellsResolved
		/// <summary>
		/// Returns whether <see cref="Cell"/>s contained in this <see cref="Record"/> should be collapsed if they contain empty values.
		/// </summary>
		/// <remarks>
		/// <para class="note"><b>Note: </b> this property returns true when used in a View that supports collapsing empty cells.</para>
		/// </remarks>
		/// <seealso cref="CardView"/>
		/// <seealso cref="CardViewCard"/>
		/// <seealso cref="CardViewSettings.ShouldCollapseEmptyCells"/>
		/// <seealso cref="Record.ShouldCollapseEmptyCells"/>
		[InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_CardView, Version = FeatureInfo.Version_10_1)]
		public override bool ShouldCollapseEmptyCellsResolved
		{
			get
			{
				FieldLayout fl = this.FieldLayout;

				if ( fl == null || !fl.IsEmptyCellCollapsingSupportedByView )
					return false;

				return this.ShouldCollapseEmptyCells ?? true;
			}
		} 
			#endregion // ShouldCollapseEmptyCellsResolved

			#region ToString

		/// <summary>
		/// Returns a string representation of the object
		/// </summary>
		public override string ToString( )
		{
			return "FilterRecord: " + this.Description;
		}

			#endregion //ToString	
 
			// JJD 10/20/11  - TFS84708 - added
			#region ViewableChildRecordsIfNeeded

		internal override ViewableRecordCollection ViewableChildRecordsIfNeeded
		{
			get
			{
				return null;
			}
		}

			#endregion //ViewableChildRecordsIfNeeded	

			#region VisibilityResolved

		/// <summary>
		/// Gets the resolved visibility of the record (read-only)
		/// </summary>
		public override Visibility VisibilityResolved
		{
			get
			{
				return base.VisibilityResolved;
			}
		}

			#endregion //VisibilityResolved	

        #endregion Base class overrides

        #region Properties

        #region Public Properties

        #region Cells

        /// <summary>
        /// Returns a collection of <see cref="FilterCell"/> objects.
        /// </summary>
        /// <remarks>The <see cref="FilterCell"/> objects in this collection are maintained in the same order as their associated <see cref="Field"/>s in the <see cref="FieldLayout.Fields"/> collection.</remarks>
        public new FilterCellCollection Cells
        {
            get
            {
                return base.Cells as FilterCellCollection;
            }
        }

        #endregion //Cells	

		#region HasActiveFilters

		/// <summary>
		/// Returns true if there are active filters.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>HasActiveFilters</b> property indicates whether there are active filters with which records are
		/// currently being filtered. It doesn't necessarily mean that any records are actually filtered out 
		/// (for example if all records match the filter criteria). It just means that there are current filter 
		/// criteria.
		/// </para>
		/// </remarks>
        /// <seealso cref="ClearActiveFilters()"/>
        /// <seealso cref="RefreshActiveFilters"/>
		public bool HasActiveFilters
		{
			get
			{
				return _cachedHasActiveFilters;
			}
		}

		#endregion // HasActiveFilters
        
        #endregion //Public Properties

        #region Internal Properties

		#region Filters

		/// <summary>
		/// Returns the record filters associated with this filter record. The filter record will 
		/// manipulate these filters when its cells are modified.
		/// </summary>
		internal ResolvedRecordFilterCollection Filters
		{
			get
			{
				RecordManager rm = this.RecordManager;
				Debug.Assert( null != rm );
				return null != rm ? rm.RecordFiltersResolved : null;
			}
		}

		#endregion // Filters

		#region VisibilityResolvedBase

		/// <summary>
		/// Returns the value returned by the VisibilityResolved implementation of the base class.
		/// </summary>
		internal Visibility VisibilityResolvedBase
		{
			get
			{
				return base.VisibilityResolved;
			}
		}

		#endregion // VisibilityResolvedBase

        #endregion //Internal Properties	

        #endregion //Properties

        #region Methods

        // JJD 12/30/08 NA 2009 Vol 1 - Record filtering
        #region ClearActiveFilters

        /// <summary>
        /// Clears all active filters
        /// </summary>
        /// <seealso cref="HasActiveFilters"/>
        /// <seealso cref="RefreshActiveFilters"/>
		public void ClearActiveFilters()
		{
			this.ClearActiveFilters(false);
		}

		// AS 5/28/09
		// Added an overload so we can raise the changing/ed events when cleared by the ui commands.
		//
		internal void ClearActiveFilters(bool raiseEvents)
        {
            if (this.HasActiveFilters)
            {
                if (this.IsActive)
                {
                    FilterCell cell = this.DataPresenter.ActiveCell as FilterCell;

                    // first exit edit mode on a filter cell in this record
                    if (cell != null)
                    {
                        if (cell.IsInEditMode)
                            cell.EndEditMode(false, true);

                        // JJD 1/27/09
                        // Also clear the operand list so it can get re-loaded on the next drop down
                        // We need to do this because the old operands might have oly included
                        // values that had been restricted by other field filters
                        FilterCellValuePresenter fcvp = cell.AssociatedCellValuePresenter as FilterCellValuePresenter;
                        if ( fcvp != null )
                            fcvp.ClearOperandList();
                    }
                }

                FilterCellCollection cells = this.Cells;

				// AS 5/28/09 NA 2009.2 Undo/Redo
				// For undo/redo, we need to get the cloned recordfilter that each filter 
				// made when its pending changes was set to true.
				//
				DataPresenterBase dp = this.DataPresenter;
				List<RecordFilter> undoFilters = new List<RecordFilter>();
				RecordFilterCollection filters = null;
				FilterCell firstCell = null;

                // loop over all the cells and clear any active filters
                for (int i = 0, count = this.Cells.Count; i < count; i++)
                {
                    FilterCell cell = cells[i];
					if (cell.HasActiveFilters)
					{
						// AS 5/28/09 NA 2009.2 Undo/Redo
						//cell.RecordFilter.Clear(raiseEvents);
						if (null == firstCell)
						{
							firstCell = cell;
							filters = cell.RecordFilter.ParentCollection;
						}

						RecordFilter undoFilter = cell.RecordFilter.Clear(raiseEvents, false);

						if (null != undoFilter && raiseEvents)
							undoFilters.Add(undoFilter);
					}
                }

				if (null != dp && undoFilters.Count > 0)
				{
					Debug.Assert(null != filters);
					RecordFiltersAction action = new RecordFiltersAction(undoFilters.ToArray(), filters.Owner, firstCell, dp);
					dp.History.AddUndoActionInternal(action);
				}
            }
        }

        #endregion //ClearActiveFilters	

        // JJD 12/30/08 NA 2009 Vol 1 - Record filtering
        #region RefreshActiveFilters

        /// <summary>
        /// Refreshes the active filters
        /// </summary>
        /// <remarks>
        /// <para class="body">This will re-evaluate each record to determine if it is filtered out.</para>
        /// </remarks>
        /// <seealso cref="ClearActiveFilters()"/>
        /// <seealso cref="HasActiveFilters"/>
        /// <seealso cref="DataRecord.IsFilteredOut"/>
        public void RefreshActiveFilters()
        {
            this.Filters.BumpVersion();
        }

        #endregion //RefreshActiveFilters

        #region Internal Methods

		#region DoesAnyFieldAllowFiltering

		/// <summary>
		/// Returns true if any visible field allows filtering. Used to determine whether the filter record
		/// should be visible.
		/// </summary>
		/// <param name="fieldLayout">Field layout.</param>
		/// <param name="hasVisibleFields">Will be set to true if there are any visible fields in the field layout.</param>
		/// <returns>Returns true if one or more fields allow filtering.</returns>
		// SSP 9/8/09 TFS21710
		// Renamed AllFieldsDisallowFilteringHelper to DoesAnyFieldAllowFiltering and changed 
		// the code accordingly.
		// 
		//internal static bool AllFieldsDisallowFilteringHelper( FieldLayout fieldLayout )
		internal static bool DoesAnyFieldAllowFiltering( FieldLayout fieldLayout, out bool hasVisibleFields )
		{
			FieldCollection fields = fieldLayout.Fields;

			hasVisibleFields = false;

			for ( int i = 0, count = fields.Count; i < count; i++ )
			{
				Field field = fields[i];

				// Skip hidden fields.
				// 
				if ( ! field.IsVisibleInCellArea )
					continue;

				hasVisibleFields = true;

				// If a field has filtering enabled for it then return true.
				// 
				if ( field.AllowRecordFilteringResolved
					&& FilterOperandUIType.None != field.FilterOperandUITypeResolved )
					// SSP 9/8/09 TFS21710
					// Renamed AllFieldsDisallowFilteringHelper to DoesAnyFieldAllowFiltering and changed 
					// the code accordingly.
					// 
					//return false;
					return true;
			}

			// SSP 9/8/09 TFS21710
			// Renamed AllFieldsDisallowFilteringHelper to DoesAnyFieldAllowFiltering and changed 
			// the code accordingly.
			// 
			// ------------------------------------------------------------------------------------
			return false;
			
#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

			// ------------------------------------------------------------------------------------
		}

		#endregion // DoesAnyFieldAllowFiltering

		#region VerifyActiveFilters

		/// <summary>
		/// Updates the HasActiveFilters property and raises the property changed notification if the
		/// property has changed.
		/// </summary>
		internal void VerifyActiveFilters( )
		{
			FilterCellCollection cells = this.Cells;

			bool hasActiveFilters = false;

			for ( int i = 0, count = this.Cells.Count; i < count; i++ )
			{
				FilterCell cell = cells[i];
				if ( cell.HasActiveFilters )
				{
					hasActiveFilters = true;
					break;
				}
			}

			if ( hasActiveFilters != _cachedHasActiveFilters )
			{
				_cachedHasActiveFilters = hasActiveFilters;
				this.RaisePropertyChangedEvent( "HasActiveFilters" );
			}
		}

		#endregion // VerifyActiveFilters

		#endregion //Internal Methods

		#endregion //Methods
	}

	#endregion // FilterRecord Class

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