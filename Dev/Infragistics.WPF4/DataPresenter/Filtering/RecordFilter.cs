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
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Infragistics.Windows.DataPresenter
{
	/// <summary>
	/// RecordFilter contains filter criteria information that will be used to filter data records.
	/// </summary>
	[ ContentProperty("Conditions") ]
	[ DefaultProperty("Conditions") ]
	public class RecordFilter : PropertyChangeNotifier, ICloneable
	{
		#region Private Vars

		private RecordFilterCollection _parentCollection;
		private ConditionGroup _conditions;
		private string _fieldName;
		private Field _field;
        private int _version;
        private bool _filterUpdatePending;
        private ComparisonOperator? _currentUiOperator;
        private object _currentUiOperand;
        private FilterDropDownItem _currentFilterDropDownItem;

		// AS 5/28/09
		private bool _applyingPendingFilter;

		// JJD 02/29/12 - TFS103486
		// Keep track of pending command so we don't execute it asynchronusly more than once
		private ICommand _pendingCommand;

		// JJD 03/29/12 - TFS106889 
		// Keep track of the condition until the fieldlayout has been initialized
		// so we can raise the RecordFilterDropDownItemInitializing event
		private ComparisonCondition _conditionPendingInitialization;

		// AS 5/28/09 NA 2009.2 Undo/Redo
		private bool _isInitializing;
		private RecordFilter _undoFilter;

		#endregion // Private Vars

		#region Constructor

		/// <summary>
		/// Constructor. Initializes a new instance of <see cref="RecordFilter"/>.
		/// </summary>
		public RecordFilter( )
		{
		}

		/// <summary>
		/// Constructor. Initializes a new instance of <see cref="RecordFilter"/>.
		/// </summary>
		/// <param name="field">Field for which the RecordFilter is being created.</param>
		public RecordFilter( Field field )
		{
			this.Field = field;
		}

		/// <summary>
		/// Constructor. Initializes a new instance of <see cref="RecordFilter"/>.
		/// </summary>
		/// <param name="fieldName">Name of the field for which the RecordFilter is being created.</param>
		internal RecordFilter( string fieldName )
		{
			_fieldName = fieldName;
		}

		#endregion // Constructor

		#region Properties

		#region Public Properties

		#region Conditions

		/// <summary>
		/// Specifies filter conditions with which to filter data records.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// Specifies one or more conditions that will be used to determine if a data record passes
		/// the record filter or not. The cell data associated with the record filter's <see cref="Field"/>
		/// will be evaluated against the conditions to determine if the data record should be filtered
		/// out. If the cell data does not satisfy these conditions, the data record will get filtered out.
		/// The <see cref="DataRecord.IsFilteredOut"/> property of the data record will reflect whether
		/// the data record met the filter conditions or not. By default filtered out data records are
		/// hidden - however <see cref="FieldLayoutSettings.FilterAction"/> property lets you specify what
		/// action to take on filtered out records.
		/// </para>
		/// <para class="body">
		/// You can add any <see cref="ICondition"/> derived objects to the condition group. Built-in
		/// classes that implement this interface include <see cref="ComparisonCondition"/>, 
		/// <see cref="ComplementCondition"/> and the <see cref="ConditionGroup"/>. 
		/// The <i>ComparisonCondition</i> lets you specify 
		/// commonly used filter criteria. The <i>ComplementCondition</i> complements results of another
		/// condition. Since <see cref="ConditionGroup"/> itself implements <i>ICondition</i>, you can
		/// essentially create condition groups within condition groups - essentially allowing you create
		/// arbitrarily nested conditions.
		/// </para>
		/// <para class="body">
		/// In addition, <see cref="ComparisonCondition"/> supports <see cref="SpecialFilterOperandBase"/>
		/// instances as its compare values. Built-in special operands are exposed off <see cref="SpecialFilterOperands"/>
		/// class as static properties. See <see cref="SpecialFilterOperands"/> for more information and listing of
		/// available special operands. These special operands (for example, <b>FirstQuater</b>, <b>Today</b>, 
		/// <b>ThisWeek</b>, <b>AboveAverage</b>, <b>Top10</b> etc...) let you quicky filter data by commonly used
		/// filter criteria. They are also extensible. You can create your own special operands (or override logic
		/// of any built-in special operand) by deriving a class from <i>SpecialFilterOperandBase</i>. You can
		/// also easily integrate such custom operands in the data presenter filtering UI by using 
		/// <i>SpecialFilterOperands</i>'s <see cref="SpecialFilterOperands.Register"/> method.
		/// </para>
		/// </remarks>
		/// <seealso cref="ConditionGroup"/>
		/// <seealso cref="ComplementCondition"/>
		/// <seealso cref="ICondition"/>
		/// <seealso cref="SpecialFilterOperandBase"/>
		/// <seealso cref="SpecialFilterOperands"/>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ConditionGroup Conditions
		{
			get
			{
				if ( null == _conditions )
				{
					_conditions = new ConditionGroup( );
					((INotifyPropertyChanged)_conditions).PropertyChanged += new PropertyChangedEventHandler( OnConditions_PropertyChanged );
				}

				return _conditions;
			}
		}

		#endregion // Conditions

		#region Field

		/// <summary>
		/// Gets or sets the field whose cell data will be matched against filter conditions of this RecordFilter.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>Field</b> property specifies the field whose cell data will be matched against filter conditions 
		/// of this RecordFilter to determine if the a particular DataRecord should be filtered out.
		/// </para>
		/// <para class="body">
		/// <b>Note</b> that the <b>FieldName</b> and the <see cref="Field"/> properties will be syncrhonized.
		/// When <i>FieldName</i> is set, <i>Field</i> property will be updated to reflect the matching Field
		/// object. If no Field with that name exists then the <i>Field</i> property will return null. 
		/// Likewise when you set the <i>Field</i> property, the <i>FieldName</i> will be updated to reflect
		/// the name of the set Field. If <i>Field</i> is set to null, then <i>FieldName</i> will be set to
		/// null as well.
		/// </para>
		/// </remarks>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public Field Field
		{
			get
			{
				if ( null == _field )
					this.SyncFieldWithFieldName( );

				return _field;
			}
			set
			{
				if ( _field != value )
				{
					_field = value;

					_fieldName = null != _field ? _field.Name : null;

 					this.RaisePropertyChangedEvent( "Field" );
					this.RaisePropertyChangedEvent( "FieldName" );

                    this.WireFieldTracker();
				}
			}
		}

		#endregion // Field

		#region FieldName

		/// <summary>
		/// Gets or sets the name of the field whose cell data will be matched against filter conditions of 
		/// this RecordFilter.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>FieldName</b> property specifies the field name whose cell data will be matched against filter 
		/// conditions of this RecordFilter to determine if the a particular DataRecord should be filtered out.
		/// </para>
		/// <para class="body">
		/// <b>Note</b> that the <b>FieldName</b> and the <see cref="Field"/> properties will be syncrhonized.
		/// When <i>FieldName</i> is set, <i>Field</i> property will be updated to reflect the matching Field
		/// object. If no Field with that name exists then the <i>Field</i> property will return null. 
		/// Likewise when you set the <i>Field</i> property, the <i>FieldName</i> will be updated to reflect
		/// the name of the set Field. If <i>Field</i> is set to null, then <i>FieldName</i> will be set to
		/// null as well.
		/// </para>
		/// </remarks>
        [DefaultValue(null)]
        //[Description("Gets or sets the name of the field whose cell data will be matched against filter conditions.")]
        //[Category("Behavior")]
		public string FieldName
		{
			get
			{
				return _fieldName;
			}
			set
			{
				if ( _fieldName != value )
				{
					_fieldName = value;

					this.RaisePropertyChangedEvent( "FieldName" );

					this.SyncFieldWithFieldName( );
				}
			}
		}

		#endregion // FieldName

        #region Version

        /// <summary>
        /// Returns a version number that gets bumped if any criteria change (read-only)
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public int Version { get { return this._version; } }

        #endregion //Version	
    
		#endregion // Public Properties

		#region Private/Internal Properties

        #region CurrentFilterDropDownItem

        internal FilterDropDownItem CurrentFilterDropDownItem 
        { 
            get { return this._currentFilterDropDownItem; }
            set { this._currentFilterDropDownItem = value; }
        }

        #endregion //CurrentFilterDropDownItem	
    
        #region CurrentUIOperator
    
        internal ComparisonOperator CurrentUIOperator
        {
            get
            {
                Field fld = this.Field;

                if (this._currentUiOperator.HasValue)
                    return this._currentUiOperator.Value;

                if (fld != null)
                    return fld.FilterOperatorDefaultValueResolved;

                return ComparisonOperator.Equals;
            }
            set 
            {
                //if ( value != this._currentUiOperator )
                if ( value != this.CurrentUIOperator )
                {
					// AS 5/29/09 NA 2009.2 Undo/Redo
					// When the filter is being edited, we need to make a copy of the 
					// record filter so we can add the original state to the undo stack
					// if the filter change is committed.
					//
					if (_undoFilter == null)
						_undoFilter = this.Clone(true, _parentCollection, true);

                    this._currentUiOperator = value;
                    this._filterUpdatePending = true;

                    SpecialFilterOperandBase specialOperand = this._currentUiOperand as SpecialFilterOperandBase;
					
					// JJD 12/29/08
					// If the currentUiOperand is a SpecialOperand make sure that it supports
					// the operator. Otherwisw clear the operand
                    if (specialOperand != null)
                    {
                        if (!specialOperand.SupportsOperator(value))
                        {
                            this._currentUiOperand = null;
                            this._currentFilterDropDownItem = null;
                        }
                    }
                    else
                    {
                        // JJD 04/28/10 - TFS31383 
                        // If the operator is specified check if the operand is a condition
                        if (this._currentUiOperator.HasValue && this._currentUiOperand is ICondition)
                        {
                            ComplementCondition complement;
                            ComparisonCondition cc = GetUnderlyingComparisonCondition(this._currentUiOperand, out complement);

                            // JJD 04/28/10 - TFS31383 
                            // for ComparisonConditions make sure the current operator makes sense
                            if (cc != null)
                            {
                                // JJD 04/28/10 - TFS31383 
                                // If the current operator is not a regular expression operator
                                // then clear the operand 
                                if (!IsOperatorCompatibleWithCondition(cc, this._currentUiOperator.Value))
                                {
                                    this._currentUiOperand = null;
                                    this._currentFilterDropDownItem = null;
                                }
                            }
                            else
                            {
                                // JJD 04/28/10 - TFS31383
                                // clear the operand since the operator can't work with a custom ICondition
                                this._currentUiOperand = null;
                                this._currentFilterDropDownItem = null;
                            }
                        }
                    }

					// SSP 2/20/09 TFS14093
					// We also need to make sure that any operand, not just the special operand,
					// is appropriate for the new operator. For example, for Top/Bottom operators,
					// only numeric operands are valid. In such a case we also need to clear the
					// operand.
					// 
					// ------------------------------------------------------------------------------
					Exception error;
					if ( null != _currentUiOperand && null != this.Field 
						&& ! this.Field.IsFilterCellEntryValid( _currentUiOperand, value, out error ) )
					{
						this._currentUiOperand = null;
						this._currentFilterDropDownItem = null;
					}
					// ------------------------------------------------------------------------------
 
                    this.BumpVersionNumber();
                }
            }
        }

   	    #endregion //CurrentUIOperator	

        #region CurrentUIOperand
    
        internal object CurrentUIOperand
        {
            get
            {
                return this._currentUiOperand;
            }
            set
            {
                if (value != this._currentUiOperand )
                {
                    FilterDropDownItem filterItem = value as FilterDropDownItem;

                    // if the valu is a FilterDropDownItem then
                    // get it's value
                    if (filterItem != null)
                         value = filterItem.Value;
 
                    // if the item is a command then execute is asyncronously
                    ICommand command = value as ICommand;

                    if (command != null)
                    {
						// JJD 02/29/12 - TFS103486
						// Check the cammand against the pending command so we don't execute it asynchronusly more than once
                        //if (this._field != null)
                        if (this._field != null && command != _pendingCommand)
                        {
                            DataPresenterBase dp = this._field.DataPresenter;

							if (dp != null)
							{
								// JJD 02/29/12 - TFS103486
								// Keep track of pending command so we don't execute it asynchronusly more than once
								_pendingCommand = command;

								dp.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new ExecuteCommand(this.OnExecuteCommand), command);
							}
                        }
                        return;
                    }

                    // JJD 1/28/09 - TFS12637
                    // If the value is an empty string or DBNull then treat it as null
                    if (GridUtilities.IsNullOrEmpty(value))
                        value = null;

					// AS 5/28/09
					// The value coming in could have been wrapped in which case the equality 
					// check above would indicate they're different but they're really not.
					//
					if (value == _currentUiOperand)
						return;

					// AS 5/29/09 NA 2009.2 Undo/Redo
					// When the filter is being edited, we need to make a copy of the 
					// record filter so we can add the original state to the undo stack
					// if the filter change is committed.
					//
					if (_undoFilter == null)
						_undoFilter = this.Clone(true, _parentCollection, true);

                    this._currentUiOperand = value;

					SpecialFilterOperandBase specialOperand = value as SpecialFilterOperandBase;

					// JJD 12/29/08
					// If the value is a SpecialOperand make sure that it supports
					// the current operator
                    if (specialOperand != null)
                    {
                        if (!specialOperand.SupportsOperator(this.CurrentUIOperator))
                        {
                            // first clear the operator to pick up the default
                            this._currentUiOperator = null;

                            // If the default doesn't work try the equals operator
                            if (!specialOperand.SupportsOperator(this.CurrentUIOperator))
                            {
                                if (specialOperand.SupportsOperator(ComparisonOperator.Equals))
                                    this._currentUiOperator = ComparisonOperator.Equals;
                            }
                        }
                    }
					else
					{
						// JJD 04/28/10 - TFS31383 
						// If the operator is specified check if the operand is a condition
						// JJD 02/17/12 - TFS101703 
						// We always want to go into this block now, even if _currentUiOperator is null
						//if (this._currentUiOperator.HasValue && this._currentUiOperand is ICondition)
						if (this._currentUiOperand is ICondition)
						{
							ComplementCondition complement;
							ComparisonCondition cc = GetUnderlyingComparisonCondition(this._currentUiOperand, out complement);

							// JJD 04/28/10 - TFS31383 
							// for ComparisonConditions make sure the current operator makes sense
							if (cc != null)
							{
								// JJD 02/17/12 - TFS101703 
								// Special case ComparisonCondition with epxlicit display text. For this case we only want to allow 
								// the 'Equals' operator.
								if (!string.IsNullOrEmpty(cc.DisplayText))
									this._currentUiOperator = ComparisonOperator.Equals;
								else
								{
									// JJD 04/28/10 - TFS31383 
									// If the current operator is not compatible with the ComparisonCondition
									// then clear it so it will pick up the appropriate operator from
									// the condition
									if (this._currentUiOperator.HasValue && !IsOperatorCompatibleWithCondition(cc, this._currentUiOperator.Value))
										this._currentUiOperator = null;
								}
							}
							else
							{
								// JJD 04/28/10 - TFS31383
								// Since the operator is not used for a custom condition then just clear it out
								this._currentUiOperator = null;
							}
						}
					}

					// SSP 2/20/09 TFS14093
					// We also need to make sure that any operand, not just the special operand,
					// is appropriate for the new operator. For example, for Top/Bottom operators,
					// only numeric operands are valid. In such a case we also need to clear the
					// operand.
					// 
					// ------------------------------------------------------------------------------
					Exception error;
					if ( null != value && null != this.Field
						&& !this.Field.IsFilterCellEntryValid( value, this.CurrentUIOperator, out error ) )
					{
						// first clear the operator to pick up the default
						this._currentUiOperator = null;

						// If the default doesn't work try the equals operator
						if ( !this.Field.IsFilterCellEntryValid( value, this.CurrentUIOperator, out error ) )
						{
							if ( this.Field.IsFilterCellEntryValid( value, ComparisonOperator.Equals, out error ) )
								this._currentUiOperator = ComparisonOperator.Equals;
						}
					}
					// ------------------------------------------------------------------------------

                    this._filterUpdatePending = true;
                    this.BumpVersionNumber();
                }
            }
        }

   	    #endregion //CurrentUIOperand	
    
		#region HasConditions

		internal bool HasConditions
		{
			get
			{
				return null != _conditions && _conditions.Count > 0;
			}
		}

		#endregion // HasConditions

        #region HasPendingChanges

        internal bool HasPendingChanges { get { return this._filterUpdatePending; } }

        #endregion //HasPendingChanges	
        
		// AS 5/28/09 NA 2009.2 Undo/Redo
		#region Initializing
		internal bool IsInitializing
		{
			get { return _isInitializing; }
		} 
		#endregion //Initializing

		#region ParentCollection

		/// <summary>
		/// Returns the parent RecordFilterCollection to which this RecordFilter belongs to.
		/// </summary>
		internal RecordFilterCollection ParentCollection
		{
			get
			{
				return _parentCollection;
			}
		}

		#endregion // ParentCollection

        #region Tooltip

        internal object Tooltip
        {
            get
            {
				// JJD 07/19/12 - TFS117413
				// Moved block of code that was returning the conditon's text
				// from inside the "if (this._filterUpdatePending)" block
				// below so that we always return it even if an update isn't pending
 				if (this._currentUiOperand is ICondition)
                {
                    if (this._currentFilterDropDownItem != null)
                    {
                        string text = this._currentFilterDropDownItem.DisplayText;

                        if (text != null && text.Length > 0)
                            return text;
                    }

                    ConditionGroup group = this._currentUiOperand as ConditionGroup;

                    if (group != null)
                        return group.ToolTip;

                    return this._currentUiOperand.ToString();
                }

                if (this._filterUpdatePending)
                {
                    if (this._currentUiOperand == null)
                        return null;

					// JJD 07/19/12 - TFS117413
					// Moved if block above
					//if (this._currentUiOperand is ICondition)
					//{
					//    if (this._currentFilterDropDownItem != null)
					//    {
					//        string text = this._currentFilterDropDownItem.DisplayText;

					//        if (text != null && text.Length > 0)
					//            return text;
					//    }

					//    ConditionGroup group = this._currentUiOperand as ConditionGroup;

					//    if (group != null)
					//        return group.ToolTip;

					//    return this._currentUiOperand.ToString();
					//}


					// SSP 2/21/09 TFS14093
					// ComparisonCondition constructor can throw an exception if the operand and
					// operator are incompatible. In such a case return null for the tooltip. 
					// An appropriate error message will be displayed when the user leaves the
					// cell.
					// ----------------------------------------------------------------------------
                    //ComparisonCondition cc = new ComparisonCondition(this.CurrentUIOperator, this._currentUiOperand);
					//return cc.ToString( );
					try
					{
						ComparisonCondition cc = new ComparisonCondition( this.CurrentUIOperator, this._currentUiOperand );
						return cc.ToString( );
					}
					catch
					{
						return null;
					}
					// ----------------------------------------------------------------------------
                }
                else
                {
                    // JJD 1/31/09
                    // If we don't have any conditions then return null for the tooltip
                    if (this.Conditions.Count == 0)
                        return null;

                    return this.Conditions.ToolTip;
                }
            }
        }

        #endregion //Tooltip	
    
		#endregion // Private/Internal Properties

		#endregion // Properties

		#region Methods

		#region Public Methods

        #region ApplyPendingFilter

        /// <summary>
        /// Applies any pending filter changes
        /// </summary>
        public void ApplyPendingFilter()
        {
			this.ApplyPendingFilter(true, true);
		}

		// AS 5/28/09 NA 2009.2 Undo/Redo
		// Added an overload so we can get the record filter that represents the 
		// original state should the recordfilter be changed. I also added a raiseEvents
		// parameter since this could be called from a method where raising the changing|ed
		// events is optional.
		//
		internal RecordFilter ApplyPendingFilter(bool raiseEvents, bool addToUndo)
		{
            // if there are no pending filter changes then return
            if (this._filterUpdatePending == false)
                return null;

			// AS 5/28/09
			// Added an anti-recursion flag. During the processing of the 
			// record filter we were getting a property change from the 
			// ConditionGroup providing the Conditions. This was ultimately
			// causing us to get back into into this method while we were 
			// still processing the pending filter change.
			//
			Debug.Assert(!_applyingPendingFilter);

			bool wasApplying = _applyingPendingFilter;
			bool result = false;

			// AS 5/28/09 NA 2009.2 Undo/Redo
			// Get the snapshot of the pre-edited state for return to the caller.
			// Since the apply will clear the current operator/operand, we should 
			// get rid of the snapshot now in case its manipulated in one of the 
			// events raised.
			//
			RecordFilter undoFilter = _undoFilter;
			_undoFilter = null;

			// SSP 3/22/10 TFS27514
			// 
			bool raiseRecordFilterChangedEvent;

			try
			{
				_applyingPendingFilter = true;

				// SSP 3/22/10 TFS27514
				// 
				//result = ApplyPendingFilterImpl(raiseEvents);
				result = ApplyPendingFilterImpl( raiseEvents, out raiseRecordFilterChangedEvent );
			}
			finally
			{
				_applyingPendingFilter = wasApplying;
			}

			// since we are not processing the OnConditions_PropertyChanged
			// while the pending filter change is applied we need to notify
			// the parent collection now
			if (result && null != _parentCollection)
				_parentCollection.OnRecordFilterChanged(this);

			// AS 5/28/09 NA 2009.2 Undo/Redo
			// If the change was cancelled or its the same as the saved state then 
			// do not put an entry in the undo stack.
			//
			if (!result || this.HasSameValues(_undoFilter))
				undoFilter = null;

			// if we have an undo filter and the filter itself should put that 
			// entry on the undo stack (as opposed to the caller), then do that now
			if (null != undoFilter && addToUndo)
			{
				DataPresenterBase dp = this.Field.DataPresenter;

				if (null != dp)
				{
					FilterCell cell = dp.ActiveCell as FilterCell;
					dp.History.AddUndoActionInternal(new RecordFiltersAction(new RecordFilter[] { undoFilter }, _parentCollection.Owner, cell, dp));
				}
			}

			// SSP 3/22/10 TFS27514
			// Moved this here from ApplyPendingFilterImpl method because we need to raise
			// the event after filter version number is bumped so anyone accessing record's
			// filter state from that event gets the correct state based on the new filter
			// conditions.
			// 
			// ------------------------------------------------------------------------------
			if ( raiseRecordFilterChangedEvent )
			{
				DataPresenterBase dp = null != _field ? _field.DataPresenter : null;

				if ( null != dp )
					dp.RaiseRecordFilterChanged( new RecordFilterChangedEventArgs( this ) );
			}
			// ------------------------------------------------------------------------------

			return undoFilter;
		}

		// AS 5/28/09
		//
		// AS 5/29/09 NA 2009.2 Undo/Redo
		// Added raiseEvents parameter since the caller may not want the changing/ed.
		//
		// SSP 3/22/10 TFS27514
		// Added raiseRecordFilterChanged parameter.
		// 
		//private bool ApplyPendingFilterImpl(bool raiseEvents)
		private bool ApplyPendingFilterImpl( bool raiseEvents, out bool raiseRecordFilterChanged )
		{
			raiseRecordFilterChanged = false;
            Field field = this.Field;

            if (field == null)
                return false;

            DataPresenterBase dp = field.DataPresenter;

            if (dp == null)
                return false;

            // reset the pending flag
            this._filterUpdatePending = false;

            RecordFilter clone = this.Clone(false);

            // see if an operand was set
            if (this._currentUiOperand != null)
            {
                ConditionGroup newGroup = this._currentUiOperand as ConditionGroup;

                // If the operand is a ConditionGroup then initialize the clone from it
                if (newGroup != null)
                {
                    clone.Conditions.LogicalOperator = newGroup.LogicalOperator;

                    foreach (ICondition condition in newGroup)
                        clone.Conditions.Add(condition);
                }
                // if the operand is an ICondition then add it to the conditions collection
                else if (this._currentUiOperand is ICondition)
                {
                    ComplementCondition complement;
                    ComparisonCondition cc = GetUnderlyingComparisonCondition(this._currentUiOperand, out complement);

                    // JJD 04/28/10 - TFS31383
                    // If the comparison condition's operator doesn't math the current operator
                    // then clone the comparison and set its Operator to the current operator.
                    // Otherwise use it as is.
					//if (cc != null &&
					//    this._currentUiOperator.HasValue &&
					//    cc.Operator != this._currentUiOperator.Value)
					if (cc != null)
					{
						// JJD 02/17/12 - TFS101703 
						// Special case ComparisonCondition with explicit display text. For this case we always
						// want to use the condition 'as is'
						if (string.IsNullOrEmpty(cc.DisplayText) &&
							this._currentUiOperator.HasValue		&&
							cc.Operator != this._currentUiOperator.Value)
						{
							ComparisonCondition ccClone = ((ICloneable)cc).Clone() as ComparisonCondition;
							ccClone.Operator = this._currentUiOperator.Value;

							if (complement != null)
							{
								clone._conditions.Add(new ComplementCondition(cc));
							}
							else
							{
								clone._conditions.Add(ccClone);
							}
						}
						else
						{
							clone._conditions.Add(this._currentUiOperand as ICondition);
						}
					}
					else
						clone._conditions.Add(this._currentUiOperand as ICondition);
                }
                else
                {
                    // since the operand wasn't an ICondition then create a ComparisonCondition
                    // with the operator and operand
                    // SSP 2/21/09 TFS14093
                    // ComparisonCondition constructor can throw an exception if the operand and
                    // operator are incompatible. In such a case we can't apply pending filters.
                    // Instead we need to display an error message to the user indicating that 
                    // the operand is invalid for the particular operator. Such an message will 
                    // be displayed when the user leaves the cell. For now we need to return 
                    // here without doing anything.
                    // 
                    // --------------------------------------------------------------------------
                    //ComparisonCondition cc = new ComparisonCondition(this.CurrentUIOperator, this._currentUiOperand);
                    //clone._conditions.Add(cc);
                    try
                    {
                        ComparisonCondition cc = new ComparisonCondition(this.CurrentUIOperator, this._currentUiOperand);
                        clone._conditions.Add(cc);
                    }
                    catch
                    {
                        return false;
                    }
                    // --------------------------------------------------------------------------
                }
            }

			
#region Infragistics Source Cleanup (Region)





















#endregion // Infragistics Source Cleanup (Region)

			// SSP 3/22/10 TFS27514
			// Moved this into the ApplyPendingFilter which calls this method. In the ApplyPendingFilter
			// method where we are setting a flag that prevents us from bumping filters version number
			// (which we do below in this method) and thus anyone accessing any information that relies 
			// on record's filter state (like RecordManager's GetFilteredInDataRecords method) in the 
			// RecordFilterChanged event will not get the correct information since the filter version
			// number haven't been bumped yet. We need to raise this event after we bump the version 
			// number.
			// 
			//bool changed = this.CopyConditionGroup(clone, false, raiseEvents);
			bool changed = this.CopyConditionGroupHelper( clone, false, raiseEvents, out raiseRecordFilterChanged );

            // reset the operator and operands from the new settings
            this.InitializeUIOperatorAndOperand();

            this.BumpVersionNumber();

			// AS 5/29/09 NA 2009.2 Undo/Redo
			//return !args.Cancel;
			return changed;
        }

        #endregion //ApplyPendingFilter	
    
		#region Clear

		/// <summary>
		/// Clears conditions of this record filter.
		/// </summary>
		/// <seealso cref="Conditions"/>
		public void Clear( )
		{
			this.Clear(false);
		}

		// AS 5/28/09
		// Added an overload so we can raise the changing/changed events when the 
		// clear method is invoked via the ui.
		//
		internal void Clear(bool raiseEvents)
		{
			// AS 5/28/09 NA 2009.2 Undo/Redo
			this.Clear(raiseEvents, raiseEvents);
		}

		// AS 5/28/09 NA 2009.2 Undo/Redo
		// Added an overload to optionally add the source filter to the undo stack.
		//
		internal RecordFilter Clear(bool raiseEvents, bool addToUndo)
		{
			if (raiseEvents)
			{
				// if we're supposed to raise the event but we have an update
				// pending then don't explicitly raise the events. they will happen
				// when the change is committed
				if (!_filterUpdatePending)
				{
					// AS 5/28/09 NA 2009.2 Undo/Redo
					// Make sure that we have created a copy of the filter before making
					// any changes.
					//
					if (null == _undoFilter)
						_undoFilter = this.Clone(true, _parentCollection, true);

					// treat this as though the user had modified the operand 
					// to be empty and go through the same logic we would have
					// when applying pending filters
					_filterUpdatePending = true;
					_currentUiOperand = null;

					// AS 5/28/09 NA 2009.2 Undo/Redo
					//this.ApplyPendingFilter();
					return this.ApplyPendingFilter(raiseEvents, addToUndo);
				}

				// AS 5/28/09 NA 2009.2 Undo/Redo
				// if we're supposed to raise events but we have an update pending
				// (e.g. you're still in edit mode and some custom action tries to 
				// clear) then we probably just want to clear the operand and wait
				// for the explicit apply call
				//
				this.CurrentUIOperand = null;
				return null;
			}

			Debug.Assert(!_filterUpdatePending);
			_undoFilter = null;

			if ( null != _conditions )
				_conditions.Clear( );

            // JJD 1/22/094
            // Only clear the operand
            //this._currentUiOperator = null;
            this._currentUiOperand = null;

            this.InitializeUIOperatorAndOperand();

            this.BumpVersionNumber();

			return null;
		}

		#endregion // Clear

		#region Clone

		object ICloneable.Clone( )
		{
			return this.Clone( true );
		}

		internal RecordFilter Clone( bool cloneConditions )
		{
            return this.Clone(cloneConditions, this._parentCollection );
        }

		internal RecordFilter Clone( bool cloneConditions, RecordFilterCollection parentCollection)
		{
			// AS 5/28/09 NA 2009.2 Undo/Redo
			return this.Clone(cloneConditions, parentCollection, false);
		}

		// AS 5/28/09 NA 2009.2 Undo/Redo
		// Added an overload so we can include the current modified operator/operand.
		//
		internal RecordFilter Clone(bool cloneConditions, RecordFilterCollection parentCollection, bool includeCurrentState)
		{
			RecordFilter f = new RecordFilter( );

            // JJD 1/20/9 - NA 2009 vol 1 - Filtering in reports
            // Only copy the file over if the parentcollection matches
            if (parentCollection == this._parentCollection)
            {
                f._parentCollection = _parentCollection;
                f._field = _field;
            }

			f._fieldName = _fieldName;
            if (cloneConditions)
            {
                f._conditions = (ConditionGroup)((ICondition)_conditions).Clone();
            }
            else
            {
                f._conditions = new ConditionGroup();
                f._conditions.LogicalOperator = this.Conditions.LogicalOperator;
            }

			// AS 5/28/09 NA 2009.2 Undo/Redo
			// If we need to include the current state and we have an update pending...
			if (includeCurrentState)
			{
				f._currentUiOperand = _currentUiOperand;
				f._currentUiOperator = _currentUiOperator;
				f._filterUpdatePending = _filterUpdatePending;

				// include the tooltip since it is checked in the equals check of the group
				f._conditions.ToolTip = this.Conditions.ToolTip;
			}

			return f;
		}

		#endregion // Clone

		#region MeetsCriteria

		/// <summary>
		/// Returns true if the specified DataRecord meets conditions of this RecordFilter.
		/// </summary>
		/// <param name="record">Record to check.</param>
		/// <returns>True if the record filter's conditions evaluate to true for the cell value of the record associated 
		/// with the <see cref="RecordFilter.Field"/>.</returns>
		public bool MeetsCriteria( DataRecord record )
		{
			FilterConditionEvaluationContext evaluationContext = 
				ResolvedRecordFilterCollection.CreateEvaluationContext( record.RecordManager, this.Field );

			return this.MeetsCriteria( record, evaluationContext );
		}

		#endregion // MeetsCriteria

		#endregion // Public Methods

		#region Private/Internal Methods

        #region ApplyNewCoonditions

        // called from the CustomFilterDialog
        internal void ApplyNewConditions(ConditionGroup newGroup)
        {
            this._currentFilterDropDownItem = null;
            this.CurrentUIOperand = newGroup;


			// AS 5/28/09 NA 2009.2 Undo/Redo
			//this._parentCollection.ApplyPendingFilters(); 
			DataPresenterBase dp = _parentCollection.DataPresenter;
			FilterCell cell = null == dp ? null : dp.ActiveCell as FilterCell;
            this._parentCollection.ApplyPendingFilters(true, true, cell);
        }

        #endregion //ApplyNewCoonditions	
            
        #region BumpVersionNumber

        internal void BumpVersionNumber()
        {
            this._version++;
            this.RaisePropertyChangedEvent("Version");
        }

        #endregion //BumpVersionNumber	

		// AS 5/28/09 NA 2009.2 Undo/Redo
		// This method was based on the ApplyPendingFilter 
		#region CopyConditionGroup

		// SSP 3/22/10 TFS27514
		// Added raiseRecordFilterChanged parameter.
		// 
		//private bool CopyConditionGroup(RecordFilter source, bool cloneConditions, bool raiseEvents)
		private bool CopyConditionGroupHelper( RecordFilter source, bool cloneConditions, bool raiseEvents, out bool raiseRecordFilterChanged )
		{
			raiseRecordFilterChanged = false;

			Debug.Assert(null != source);
			DataPresenterBase dp = _field != null ? _field.DataPresenter : null;

			if (null != dp && raiseEvents)
			{
				RecordFilterChangingEventArgs args = new RecordFilterChangingEventArgs(source);
				dp.RaiseRecordFilterChanging(args);

				if (args.Cancel)
					return false;
			}

			bool wasInitializing = _isInitializing;

			try
			{
				_isInitializing = true;

				ConditionGroup sourceGroup = source.Conditions;

				this.Conditions.Clear();

				// move the contions from the clone to our collection
				foreach (ICondition condition in sourceGroup)
				{
					ICondition newCondition = cloneConditions ? (ICondition)condition.Clone() : condition;
					this.Conditions.Add(newCondition);
				}

				this.Conditions.LogicalOperator = sourceGroup.LogicalOperator;

				// JM 02-08-10 TFS26529
				//this.Conditions.ToolTip = sourceGroup.LogicalOperator;
				this.Conditions.ToolTip = sourceGroup.ToolTip;
			}
			finally
			{
				_isInitializing = wasInitializing;
			}

			if ( null != dp && raiseEvents )
			{
				// SSP 3/22/10 TFS27514
				// We need to raise the RecordFilterChanged event after bumping any filter version numbers
				// which the caller does after calling this method. Therefore we need to move it out of
				// here and put it in the appropriate place in the caller.
				// 
				//dp.RaiseRecordFilterChanged( new RecordFilterChangedEventArgs( this ) );
				raiseRecordFilterChanged = true;
			}

			return true;
		}
		#endregion //CopyConditionGroup

		// AS 5/28/09 NA 2009.2 Undo/Redo
		#region HasSameValues
		/// <summary>
		/// Helper method used to compare a cloned filter against the current settings.
		/// </summary>
		internal bool HasSameValues(RecordFilter other)
		{
			if (null == other)
				return false;

			if (!object.Equals(_filterUpdatePending, other._filterUpdatePending))
				return false;

			if (!object.Equals(_currentUiOperator, other._currentUiOperator))
				return false;

			if (!object.Equals(_currentUiOperand, other._currentUiOperand))
				return false;

			if (!object.Equals(this.Conditions, other.Conditions))
				return false;

			return true;
		}
		#endregion //HasSameValues
 
        // JJD 04/28/10 - TFS31383 - added
        #region GetUnderlyingComparisonCondition

        private static ComparisonCondition GetUnderlyingComparisonCondition(object value, out ComplementCondition complement)
        {
            ConditionGroup group = value as ConditionGroup;

            if (group != null)
            {
                if (group.Count == 1)
                    return GetUnderlyingComparisonCondition(group[0], out complement);

                complement = null;

                return null;
            }

            complement = value as ComplementCondition;

            if (complement != null)
                return complement.SourceCondition as ComparisonCondition;

            return value as ComparisonCondition;
        }

        #endregion //GetUnderlyingComparisonCondition	
  
		// JJD 02/21/12 - TFS99332 - added
		#region InitializeFilterDropDownItem

		private void InitializeFilterDropDownItem(ComparisonCondition condition)
		{
			if (condition == null || this._field == null)
				return;

			FieldLayout fl = _field.Owner;
			if (fl == null)
			{
				// JJD 03/29/12 - TFS106889
				// If the fieldLayout is null that means that the field hasn't been added 
				// to the fieldlayout so cache the condition until it is and then the
				// field layout is initialized
				_conditionPendingInitialization = condition;
				return;
			}

			// if the FilterUITypeResolved is not 'FilterRecord' then bail
			// JJD 03/29/12 - TFS106889
			// Fl checked for null above
			//if (fl == null || fl.FilterUITypeResolved != FilterUIType.FilterRecord)
			if (fl.FilterUITypeResolved != FilterUIType.FilterRecord)
			{
				// JJD 03/29/12 - TFS106889
				// Clear any cached condition
				_conditionPendingInitialization = null;
				return;
			}

			DataPresenterBase dp = this._field.DataPresenter;

			if (dp == null)
			{
				// JJD 03/29/12 - TFS106889
				// If the dp is null that means that the fieldLayout hasn't been initialized yet
				// so cache the condition until it is
				_conditionPendingInitialization = condition;
				return;
			}

			// JJD 03/29/12 - TFS106889
			// Clear any cached condition
			_conditionPendingInitialization = null;

			// create a new FilterDropDownItem to wrap the condition
			FilterDropDownItem ddItem = new FilterDropDownItem(condition.Value, condition.DisplayText, true);

			RecordManager rm = _parentCollection != null ? this._parentCollection.Owner as RecordManager : null;

			// raise the new RecordFilterDropDownItemInitializing event
			dp.RaiseRecordFilterDropDownItemInitializing(new RecordFilterDropDownItemInitializingEventArgs(_field, rm, ddItem));

			// Only set the _currentFilterDropDownItem if display text is non null or if the Image property
			// was set inside the RecordFilterDropDownItemInitializing event
			if (false == string.IsNullOrEmpty(ddItem.DisplayText) || ddItem.Image != null)
			{
				this._currentFilterDropDownItem = ddItem;
			}
		}

		#endregion //InitializeFilterDropDownItem	
    
		// AS 5/28/09 NA 2009.2 Undo/Redo
		#region InitializeFrom
		internal bool InitializeFrom(RecordFilter recordFilter, bool cloneConditions, bool raiseEvents)
		{
			bool wasInitializing = _isInitializing;

			try
			{
				_isInitializing = true;

				bool notifyParent = false;

				// SSP 3/22/10 TFS27514
				// 
				bool raiseRecordFilterChanged = false;

				DataPresenterBase dp = _field != null ? _field.DataPresenter : null;

				if (!object.Equals(recordFilter.Conditions, this.Conditions))
				{
					if (null != dp)
					{
						// SSP 3/22/10 TFS27514
						// 
						//if (!this.CopyConditionGroup(recordFilter, cloneConditions, raiseEvents))
						if ( !this.CopyConditionGroupHelper( recordFilter, cloneConditions, raiseEvents, out raiseRecordFilterChanged ) )
							return false;

						notifyParent = true;
						this.InitializeUIOperatorAndOperand();
					}
				}

				_filterUpdatePending = recordFilter._filterUpdatePending;
				_currentUiOperator = recordFilter._currentUiOperator;
				_currentUiOperand = recordFilter._currentUiOperand;
				_currentFilterDropDownItem = null;

				this.BumpVersionNumber();

				if (notifyParent && null != _parentCollection)
					_parentCollection.OnRecordFilterChanged(this);

				// SSP 3/22/10 TFS27514
				// 
				if ( raiseRecordFilterChanged && null != dp )
					dp.RaiseRecordFilterChanged( new RecordFilterChangedEventArgs( this ) );

				return true;
			}
			finally
			{
				_isInitializing = wasInitializing;
			}
		}
		#endregion //InitializeFrom

		#region InitializeParentCollection

		/// <summary>
		/// Initializes this RecordFilter with the backward pointer to the parent collection to which
		/// this RecordFilter belongs. This method does validation to ensure that the same RecordFilter
		/// instance is not added to more than one RecordFilterCollection.
		/// </summary>
		/// <param name="parentCollection">The RecordFilterCollection to which this RecordFilter belongs.</param>
		internal void InitializeParentCollection( RecordFilterCollection parentCollection )
		{
			if ( _parentCollection != parentCollection )
			{
				// Disallow adding same record filter into multiple record filters collections. This is
				// because the Field property is syncrhonized with the FieldName via parentCollection.
				// This syncrhonization would not work if the same record filter is in two parent 
				// collections that belong to two different field layouts.
				// 
				if ( null != _parentCollection )
					throw new InvalidOperationException( DataPresenterBase.GetString( "LE_InvalidOperationException_27" ) );

				_parentCollection = parentCollection;

				this.SyncFieldWithFieldName( );

				// AS 5/27/09
				// When the record filter is hooked up, we need to initialize the
				// operator and operand. The operand in particular since it is used 
				// as the value for the cell. Otherwise when the filter cell enters 
				// edit mode it uses an uninitialized operand member variable.
				//
				this.InitializeUIOperatorAndOperand();
			}
		}

		#endregion // InitializeParentCollection

		// JJD 03/29/12 - TFS106889 - added
		#region InitializePendingCondition

		private void InitializePendingCondition()
		{
			if (_conditionPendingInitialization != null &&
				_conditions.Count == 1 &&
				_conditions[0] == _conditionPendingInitialization)
			{
				this.InitializeFilterDropDownItem(_conditionPendingInitialization);
			}
			else
			{
				_conditionPendingInitialization = null;
			}

		}

		#endregion //InitializePendingCondition	

        #region InitializeUIOperatorAndOperand

		// JJD 02/23/12 - TFS102487
		// Added overload that takes externalChangeOfCriteria param
		private void InitializeUIOperatorAndOperand()
		{
			// JJD 02/23/12 - TFS102487
			// By default the externalChangeOfCriteria parameter should be null
			this.InitializeUIOperatorAndOperand(false);
		}
        private void InitializeUIOperatorAndOperand(bool externalChangeOfCriteria)
        {
            if (this._filterUpdatePending)
                return;

            object newOperand = null;
            ComparisonOperator? newOperator = null;
            int count = this.Conditions.Count;

            if (count > 0)
            {
                if (count == 1)
                {
                    newOperand = this.Conditions[0];


                    // JJD 04/28/10 - TFS31383
                    // Get the underlying ComparisonCondition
                    
                    ComplementCondition dummy;
                    ComparisonCondition cc = GetUnderlyingComparisonCondition(newOperand, out dummy);

					if (cc != null)
					{
						newOperator = cc.Operator;

						// JJD 02/23/12 - TFS102487
						// Fixed regression caused by fix for TFS99332 below.
						// We should only be calling InitializeFilterDropDownItem if we are
						// called as the result of an external filter criteria change (i.e. from code).
						if (this._currentFilterDropDownItem == null && externalChangeOfCriteria)
						{
							// JJD 02/21/12 - TFS99332 
							// call InitializeFilterDropDownItem to create a FilterDropDownItem wrapper and 
							// raise the new RecordFilterDropDownItemInitializing event
							this.InitializeFilterDropDownItem(cc);
							newOperand = cc.Value;
						}

						// JJD 02/17/12 - TFS101703 
						// Special case ComparisonCondition with explicit display text. For this case we only  
						// want to use the 'Equals' operator.
						if (!string.IsNullOrEmpty(cc.DisplayText))
						{
							newOperand = cc;
							newOperator = ComparisonOperator.Equals;
						}
						else
						{
							// JJD 2/18/09
							// Only use the comparison condition's value if the condition wasn't selected from
							// the operand list (i.e it is the value of the _currentFilterDropDownItem).
							
							
							if (this._currentFilterDropDownItem == null)
							{
								newOperand = cc.Value;
							}
							else if (cc != this._currentFilterDropDownItem.Value)
							{
								// JJD 04/28/10 - TFS31383 
								// chek if the dropdown item's value is a Comparison condition
								ComplementCondition complement;
								ComparisonCondition ccFromDdi = GetUnderlyingComparisonCondition(this._currentFilterDropDownItem.Value, out complement);

								if (complement != null)
									ccFromDdi = complement.SourceCondition as ComparisonCondition;
								else
									ccFromDdi = this._currentFilterDropDownItem.Value as ComparisonCondition;

								// JJD 04/28/10 - TFS31383 
								// Ignoring the operator chekc if the value is the same.
								// If so use the dropdown item's value instead so the
								// display text doesn't change
								if (ccFromDdi != null &&
									(ccFromDdi == cc || ccFromDdi.Value == cc.Value))
									newOperand = this._currentFilterDropDownItem.Value;
								else
									newOperand = cc.Value;
							}
						}
					}
					else
						newOperator = null;
				}
                else
                {
                    newOperand = this.Conditions;
                    newOperator = null;
                }
            }
            else
                newOperator = this._currentUiOperator;

            bool bumpVersion = false;

			// AS 5/27/09
			// Check for equality rather than compare the boxed values
			//
            //if (this._currentUiOperand != newOperand)
            if (!object.Equals(this._currentUiOperand,newOperand))
            {
                this._currentUiOperand = newOperand;
                bumpVersion = true;
            }

			// AS 5/27/09
			// Check for equality rather than compare the boxed values
			//
			//if (this._currentUiOperator != newOperator)
			if (!object.Equals(this._currentUiOperator, newOperator))
            {
                this._currentUiOperator = newOperator;
                bumpVersion = true;
            }

            if (bumpVersion)
                this.BumpVersionNumber();
        }

        #endregion //InitializeUIOperatorAndOperand	

        // JJD 04/28/10 - TFS31383 - added
        #region IsOperatorCompatibleWithCondition

        internal static bool IsOperatorCompatibleWithCondition(ComparisonCondition condition, ComparisonOperator oper)
        {
			// JJD 02/17/12 - TFS101703 
			// Special case ComparisonCondition with explicit display text. For this case we only 
			// want to allow the 'Equals' operator 
			if (!string.IsNullOrEmpty(condition.DisplayText))
				return oper == ComparisonOperator.Equals;

			switch (condition.Operator)
            {
                case ComparisonOperator.Match:
                case ComparisonOperator.DoesNotMatch:
                    return (oper == ComparisonOperator.Match || oper == ComparisonOperator.DoesNotMatch);

                case ComparisonOperator.Like:
                case ComparisonOperator.NotLike:
                    return (oper == ComparisonOperator.Like || oper == ComparisonOperator.NotLike);

				// JJD 10/07/10 - TFS37236
				// Allow all the other valid complements
                case ComparisonOperator.Contains:
                case ComparisonOperator.DoesNotContain:
                    return (oper == ComparisonOperator.Contains || oper == ComparisonOperator.DoesNotContain);

                case ComparisonOperator.StartsWith:
                case ComparisonOperator.DoesNotStartWith:
                    return (oper == ComparisonOperator.StartsWith || oper == ComparisonOperator.DoesNotStartWith);

                case ComparisonOperator.EndsWith:
                case ComparisonOperator.DoesNotEndWith:
                    return (oper == ComparisonOperator.EndsWith || oper == ComparisonOperator.DoesNotEndWith);

                case ComparisonOperator.Equals:
                case ComparisonOperator.NotEquals:
                    return (oper == ComparisonOperator.Equals || oper == ComparisonOperator.NotEquals);
            }

			// JJD 10/07/10 - TFS37236
			// We should default to comparing the operators 
            //return true;
            return oper == condition.Operator;
        }

        #endregion //IsOperatorCompatibleWithCondition	
    
		#region MeetsCriteria

		/// <summary>
		/// Checks to see if the specified data record meets all the filter conditions of this RecordFilter.
		/// </summary>
		/// <param name="record">Data record to check for filter criteria.</param>
		/// <param name="evaluationContext">Evaluation context that provides further information
		/// to the condition matching logic, regarding things like culture, format etc...</param>
		/// <returns>Returns true if the record meets filter criteria, false otherwise.</returns>
		internal bool MeetsCriteria( DataRecord record, FilterConditionEvaluationContext evaluationContext )
		{
			Field field = this.Field;

			// If the field is null specified field name is not found, or the field is from a different
			// field layout than the record, then don't apply the filter to the record.
			// 
			if ( null == field || field.Owner != record.FieldLayout )
			{
				Debug.Assert( null != field );
				return true;
			}

			// SSP 6/16/09 - TFS18467
			// If a combo editor is being used to map values then the filtering should be done on the
			// mapped values, not the underlying values, because the mapped values are what the end
			// user sees.
			// 
			// --------------------------------------------------------------------------------
			// Initialize the context with the record being evaluated so it can correctly
			// initialize its CurrentValue structure with the right value, cell context etc...
			// 
			evaluationContext.Initialize( record );
			object cellVal = evaluationContext.CurrentCellValueForFilterComparison;
			
#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)

			// --------------------------------------------------------------------------------

			return null == _conditions || _conditions.IsMatch( cellVal, evaluationContext );
		}

		#endregion // MeetsCriteria

		#region OnConditions_PropertyChanged

		/// <summary>
		/// Event handler for Conditions' PropertyChanged event.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnConditions_PropertyChanged( object sender, PropertyChangedEventArgs e )
		{
			// AS 5/28/09
			// We can (and will) get in here from within the apply pending
			// filters. We need to wait until the apply is done before notifying
			// the collection.
			//
			if (_applyingPendingFilter)
				return;

			// AS 5/28/09 NA 2009.2 Undo/Redo
			if (_isInitializing)
				return;

			// SSP 1/5/10
			// Copied this from 9.2 to 9.1. This is necessary to initialize the
			// filter cell's value and the filter cvp's value correctly when a condition
			// is added in code, for example in the window's load. This is tested
			// by TestRecordFilter_CurrentUIOperand unit test.
			// 
			// AS 8/4/09 NA 2009.2 Field Sizing
			// When a field's filter is manipulated and we are autosizing we need to be 
			// able to notify the filtercell so it can get remeasured. I was originally 
			// going to bump the record filters version directly but the CurrentUIOperand
			// and therefore the ValueInternal of the FilterCell was still the old value.
			// Instead we will update the ui operator/operand which will in turn bump the 
			// Version if either has changed. In this way the FieldFilterInfo will see that 
			// this filter has changed and send out a notification that the FilterCell can 
			// receive and dirty itself. Likewise when the filtercell is later measured, the 
			// ValueInternal (which just returns the CurrentUIOperand) will return the new 
			// value based on the changes to the Conditions.
			//
			if (e.PropertyName == "Item[]")
			{
				// JJD 02/23/12 - TFS102487
				// Pass trus in as the externalChangeOfCriteria param
				//this.InitializeUIOperatorAndOperand();
				this.InitializeUIOperatorAndOperand(true);
			}

			// AS 8/4/09 NA 2009.2 Optimization
			// Whenever the ObservableCollection, it sends out an indexer change (i.e. "Item[]")
			// and sometimes a Count so we were sometimes bumping the parent twice even though 
			// we didn't need to so we can ignore a Count change. If we get a Count change then 
			// we would have gotten a Item[] change.
			//
			//if ( null != _parentCollection )
			if ( null != _parentCollection && e.PropertyName != "Count")
				_parentCollection.OnRecordFilterChanged( this );
		}

		#endregion // OnConditions_PropertyChanged

		//#region OnDefaultOperatorChanged

        //private void OnDefaultOperatorChanged()
        //{
        //    if (this._field == null)
        //        return;

        //    ComparisonOperator newDefaultOperator = this._field.FilterOperatorDefaultValueResolved;

        //    if (this._currentUiOperator == null)
        //        this.BumpVersionNumber();
        //}

        //#endregion //OnDefaultOperatorChanged	

        #region OnExecuteCommand

        delegate void ExecuteCommand(ICommand command);
        private void OnExecuteCommand(ICommand command)
        {
			// JJD 02/29/12 - TFS103486
			// Clear the cached command
			_pendingCommand = null;

			if (command.CanExecute(this))
                command.Execute(this);

        }

        #endregion //OnExecuteCommand	
		
		// JJD 03/29/12 - TFS106889 - added
		#region OnFieldLayoutInitialized

		internal void OnFieldLayoutInitialized()
		{
			this.InitializePendingCondition();
		}

		#endregion //OnFieldLayoutInitialized	
    
		// AS 5/28/09 NA 2009.2 Undo/Redo
		#region ResetUndoFilter
		internal void ResetUndoFilter()
		{
			if (_filterUpdatePending)
				_undoFilter = this.Clone(true, _parentCollection, true);
			else
				_undoFilter = null;
		}
		#endregion //ResetUndoFilter
        
		#region ShouldSerialize

		/// <summary>
		/// Returns if any of this RecordFilter should be serialized.
		/// </summary>
		/// <returns></returns>
		internal bool ShouldSerialize( )
		{
			return this.HasConditions;
		}

		#endregion // ShouldSerialize

		#region SyncFieldWithFieldName

		/// <summary>
		/// Syncrhonizes the Field property with the FieldName property.
		/// </summary>
		private void SyncFieldWithFieldName( )
		{
			FieldLayout fl;
			
			object collectionOwner = null != _parentCollection ? _parentCollection.Owner : null;

			if ( collectionOwner is FieldLayout )
				fl = (FieldLayout)collectionOwner;
			else if ( collectionOwner is RecordManager )
			{
				// AS 2/9/11 TFS65727
				// I worked through this one with Sandip. Basically when using LabelIcons and the 
				// parent collection has heterogeneous data, we cannot assume that we should use 
				// the parent field layout since that is the default field layout. So if we have 
				// a Field reference then we should continue to use that field layout.
				//
				//fl = ( (RecordManager)collectionOwner ).FieldLayout;
				if ( _field != null )
					fl = _field.Owner;
				else
					fl = ((RecordManager)collectionOwner).FieldLayout;
			}
			else
				fl = null;

			Field oldField = _field;
			FieldCollection fieldCollection = GridUtilities.GetFields( fl );

			// SSP 6/15/09 TFS18465
			// We allow fields with empty names as well as multiple fields with the 
			// same field name in the field collection. Therefore we should only change
			// the field if the _fieldName doesn't match the old field's name.
			// 
			if ( null != oldField && oldField.Name == _fieldName && oldField.Owner == fl )
				return;

			_field = GridUtilities.GetField( fieldCollection, _fieldName, false );
			if ( _field != oldField )
			{
				this.RaisePropertyChangedEvent( "Field" );

				if ( null != _parentCollection )
					_parentCollection.OnRecordFilterChanged( this );

                this.WireFieldTracker();
			}
		}

		#endregion // SyncFieldWithFieldName

        #region WireFieldTracker

        private void WireFieldTracker()
        {
            if (this._field != null)
            {
 //               this._tracker = new PropertyValueTracker(this._field, "FilterOperatorDefaultValueResolved", this.OnDefaultOperatorChanged);
                this.InitializeUIOperatorAndOperand();

				// JJD 03/29/12 - TFS106889
				// If the DataPresenter returns a value call InitializePendingCondition
				if (this._field.DataPresenter != null)
					this.InitializePendingCondition();

            }
            //else
            //    this._tracker = null;
        }

        #endregion //WireFieldTracker	
    
		#endregion // Private/Internal Methods

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