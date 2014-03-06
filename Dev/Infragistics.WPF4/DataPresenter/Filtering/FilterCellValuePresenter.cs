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
using Infragistics.Collections;

namespace Infragistics.Windows.DataPresenter
{

	#region FilterCellValuePresenter Class

	/// <summary>
	/// Element that represents a <see cref="FilterCell"/>.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// <b>FilterCellValuePresenter</b> represents a <see cref="FilterCell"/>.
	/// </para>
	/// </remarks>
	/// <seealso cref="FilterRecord"/>
	/// <seealso cref="FilterCell"/>
	/// <seealso cref="FilterCellValuePresenter"/>
	//[ToolboxItem( false )]
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class FilterCellValuePresenter : CellValuePresenter
	{
        #region Private Members

        private PropertyValueTracker _tracker;

        private ObservableCollectionExtended<FilterDropDownItem> _operands;
        private bool _areOperatorAndOperandsInitialized;
        private bool _wasChangedWhileOperandDropDownOpen;

        // JJD 04/28/10 - TFS31383
        private bool _areNonUniqueValueOperandsLoaded;
        private bool _uniqueValueOperandsLoadPending;

        // JJD 1/27/09
        // Added a flag so we know whether to ignore value change notifications
        private bool _ignoreValueChange;

		// SSP 3/31/09 TFS15861
		// We need to re-populate the list whenever a special operand is registered or un-registered
		// via SpecialFilterOperands.
		// 
		// SSP 7/17/09 TFS19258
		// We need to repopulate the drop-down items whenever data or filter criteria of some
		// other field changes.
		// 
		//private int _verifiedSpecialOperandsVersion;
		private int _verifiedDropDownItemsVersion;

		// AS 5/28/09 NA 2009.2 Undo/Redo
		private RecordFilter _originalFilter;
		
		// JJD 06/29/10 - TFS32174 - added
		private ResolvedRecordFilterCollection.FilterDropDownItemLoader _loader;

        #endregion //Private Members	
    
        #region Constructors

        static FilterCellValuePresenter()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(FilterCellValuePresenter), new FrameworkPropertyMetadata(typeof(FilterCellValuePresenter)));

            // JJD 8/20/09 - TFS19318
            // Add a coerce callback for the IsEnabled property
            FrameworkElement.IsEnabledProperty.OverrideMetadata(typeof(FilterCellValuePresenter), new FrameworkPropertyMetadata(null, new CoerceValueCallback(CoerceIsEnabled)));

            // JJD 9/24/09 - TFS22535
            // Add a coerce callback for the Visibility property
            FrameworkElement.VisibilityProperty.OverrideMetadata(typeof(FilterCellValuePresenter), new FrameworkPropertyMetadata(null, new CoerceValueCallback(CoerceVisibility)));

            CommandBinding commandBindingClearFilters = new CommandBinding(DataPresenterCommands.ClearActiveCellFilters,
                FilterCellValuePresenter.ExecuteCommandHandler, FilterCellValuePresenter.CanExecuteCommandHandler);
            CommandManager.RegisterClassCommandBinding(typeof(FilterCellValuePresenter), commandBindingClearFilters);

            CommandBinding commandToggleOperatorDropDown = new CommandBinding(DataPresenterCommands.ToggleFilterOperatorDropDown,
                FilterCellValuePresenter.ExecuteCommandHandler, FilterCellValuePresenter.CanExecuteCommandHandler);
            CommandManager.RegisterClassCommandBinding(typeof(FilterCellValuePresenter), commandToggleOperatorDropDown);


            // JJD 5/26/09 - TFS17564
            // Register a class handler for the ValueEditor.ValueChangedEvent instead of overriding 
            // OnValueChanged to ensure that evrything is in sync including the IsValueValid
            EventManager.RegisterClassHandler(typeof(FilterCellValuePresenter), ValueEditor.ValueChangedEvent, new RoutedEventHandler(OnEditorValueChanged), true); 

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FilterCellValuePresenter"/> class
        /// </summary>
        public FilterCellValuePresenter()
        {
        }

        #endregion //Constructors	

        #region CommandHandlers

			#region CanExecuteCommandHandler

		private static void CanExecuteCommandHandler(object sender, CanExecuteRoutedEventArgs e)
		{
            FilterCellValuePresenter fcvp = sender as FilterCellValuePresenter;

			e.CanExecute = false;

			if (fcvp != null && fcvp.Field != null && fcvp.Cell is FilterCell)
			{
                if (e.Command == DataPresenterCommands.ClearActiveCellFilters)
                {
                    e.CanExecute = ((FilterCell)(fcvp.Cell)).HasActiveFilters;
                    e.Handled = true;
                }
                else
                if (e.Command == DataPresenterCommands.ToggleFilterOperatorDropDown)
                {
                    e.CanExecute = fcvp.Field.FilterOperatorVisibilityResolved == Visibility.Visible;
                    e.Handled = true;
                }
			}
		}

			#endregion //CanExecuteCommandHandler	

			#region ExecuteCommandHandler

		private static void ExecuteCommandHandler(object sender, ExecutedRoutedEventArgs e)
		{
            FilterCellValuePresenter fcvp = sender as FilterCellValuePresenter;
			if (fcvp != null)
				fcvp.ExecuteCommandImpl(e.Command as RoutedCommand, e.Parameter, e.Source, e.OriginalSource);
		}

			#endregion //ExecuteCommandHandler

			#region ExecuteCommandImpl

		private bool ExecuteCommandImpl(RoutedCommand command, object parameter, object source, object originalSource)
		{
			// Make sure we have a command to execute.
			if (null == command)
				throw new ArgumentNullException("command");
                
            FilterCell fc = this.Cell as FilterCell;

            if (fc != null && !fc.IsInEditMode)
            {
                DataPresenterBase dp = this.DataPresenter;

                // If another cell is in edit mode then try to exit
                // if the exit was cancelled eat the command
                if (dp != null && dp.EndEditMode(true, false) == false)
                    return true;
            }

			bool handled = false;
 
            if (command == DataPresenterCommands.ClearActiveCellFilters)
            {
                if (fc != null)
                {
					fc.ClearActiveFilters(true );
                    handled = true;
                }
            }
            else
            {
                if (command == DataPresenterCommands.ToggleFilterOperatorDropDown)
                {
                    this.IsOperatorDropDownOpen = !this.IsOperatorDropDownOpen;
                    handled = true;
                }
            }

			return handled;
		}

			#endregion //ExecuteCommandImpl

		#endregion //CommandHandlers
    
		#region Base Overrides

            #region IsCurrentValueValid

        /// <summary>
        /// Performs any additional validation required.
        /// </summary>
        /// <param name="error"></param>
        /// <remarks>
        /// <para class="note"><b>Note:</b> the default implementaion always returns true. This method is intended 
        /// for use by derived classes that need to perform some additional validatio logic.</para>
        /// </remarks>
        /// <returns>True if the current value is valid, otherwise false.</returns>
        internal protected override bool IsCurrentValueValid(out Exception error)
        {
			// SSP 5/10/12 TFS111127
			// Check for null cell.
			// 
            //return this.Field.IsFilterCellEntryValid(this.Editor.Value, ((FilterCell)(this.Cell)).RecordFilter.CurrentUIOperator, out error);
			error = null;
			FilterCell cell = this.Cell as FilterCell;
			return null == cell || this.Field.IsFilterCellEntryValid( this.Editor.Value, cell.RecordFilter.CurrentUIOperator, out error );
        }

            #endregion //IsCurrentValueValid

		    #region IsEditingAllowed
		
#region Infragistics Source Cleanup (Region)
























#endregion // Infragistics Source Cleanup (Region)

		    #endregion //IsEditingAllowed

            #region OnApplyTemplate

        /// <summary>
        /// Called when the template is applied
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // JJD 8/20/09 - TFS19318
            // Force the IsEnabled property to be coreced so our callback gets called
            this.CoerceValue(IsEnabledProperty);

            FilterCell cell = this.Cell as FilterCell;

            // JJD 04/28/10 - TFS31383 
            // If there is a current FilterDropDownItem make sure we add it to
            // the Operands collection
            if (cell != null)
            {
                FilterDropDownItem fddi = cell.FieldFilterInfo.RecordFilter.CurrentFilterDropDownItem;

                if ( fddi != null && !this.Operands.Contains(fddi))
                     this.Operands.Add(fddi);
            }
       }

            #endregion //OnApplyTemplate	
    
            // JJD 12/24/08 - added
            #region OnCellSettingsInitialized

        /// <summary>
        /// Celled when all the cell settings have been initialized
        /// </summary>
        protected override void OnCellSettingsInitialized()
        {
            FilterCell cell = this.Cell as FilterCell;

            Debug.Assert(cell != null, "Should have a FilterCell allocate hete");

            if ( cell == null )
                return;

			// AS 10/27/11
			// This was actually causing some unit tests to fail that must have been relying 
			// upon the synchronous processing of the filter change. Since this change isn't 
			// required for this feature (the excel style filtering is mutually exclusive with 
			// the FilterRecord because it only relates to LabelIcons) we decided to just 
			// revert to the original behavior.
			//
			//// AS 5/17/11 NA 11.2 Excel Style Filtering
			//// The version number could be bumped multiple times. What's more if something is 
			//// setting the operator/operand and is about to call UpdatePendingChanges such that 
			//// it should be added to the undo history cannot since it will already have been 
			//// updated the pending changes because the FCVP is doing that.
			////
			////this._tracker = new PropertyValueTracker(cell, "RecordFilter.Version", this.OnFilterVersionChanged);
			//this._tracker = new PropertyValueTracker(cell, "RecordFilter.Version", this.OnFilterVersionChanged, true);
			//this._tracker.AsynchronousDispatcherPriority = DispatcherPriority.Send;
			this._tracker = new PropertyValueTracker(cell, "RecordFilter.Version", this.OnFilterVersionChanged);

            this.OnFilterVersionChanged();

            this._areOperatorAndOperandsInitialized = true;
        }

            #endregion //OnCellSettingsInitialized	

			// AS 5/28/09 NA 2009.2 Undo/Redo
			#region OnCommitEditValue
		internal override void OnCommitEditValue(object editedValue)
		{
			this.AddEditModeUndoAction();

			base.OnCommitEditValue(editedValue);
		}
			#endregion //OnCommitEditValue

			// AS 5/28/09 NA 2009.2 Undo/Redo
			#region AddEditModeUndoAction
		private void AddEditModeUndoAction()
		{
			DataPresenterBase dp = this.DataPresenter;

			if (null != dp && null != _originalFilter)
			{
				FilterCell cell = this.Cell as FilterCell;
				RecordFilter filter = cell.RecordFilter;
				RecordFilterCollection filters = filter.ParentCollection;
				Debug.Assert(null != filters);

				// if the operator/operand have changed since we started edit mode
				// then we want to create an undo action using the original filter
				if (null != filters && !filter.HasSameValues(_originalFilter))
				{
					DataPresenterAction action = new RecordFiltersAction(new RecordFilter[] { _originalFilter }, filters.Owner, cell, dp);

					// if the user was in autosize mode then store the original field sizes to restore when the edit is undo.
					action = dp.EditHelper.CreateCommitEditUndoAction(action);

					dp.History.AddUndoActionInternal(action);
				}

				// if the filter had been tracking an undo filter then make sure
				// it recreates that based on the current state
				filter.ResetUndoFilter();

				_originalFilter = null;
			}
		} 
			#endregion //AddEditModeUndoAction

            #region OnEditorCreated

        /// <summary>
        /// Called after the editor has been created but before its Content is set, its Style has been applied and before it has been sited.
        /// </summary>
        /// <param name="editor">The ValueEditor that was just created</param>
        protected override void OnEditorCreated(ValueEditor editor)
        {
            base.OnEditorCreated(editor);

            Field fld = this.Field;

            if (fld == null || editor == null)
                return;


            fld.InitializeFilterCellEditor(editor);

            XamComboEditor combo = editor as XamComboEditor;

            if (combo != null)
            {
                combo.SetValue(XamComboEditor.ItemsSourceProperty, this.Operands);
                combo.SetBinding(XamComboEditor.IsDropDownOpenProperty, Utilities.CreateBindingObject(IsOperandDropDownOpenProperty, BindingMode.TwoWay, this));

                ValueSource vs = DependencyPropertyHelper.GetValueSource(combo, XamComboEditor.DropDownResizeModeProperty);

                if (vs.BaseValueSource < BaseValueSource.Style)
                    combo.SetValue(XamComboEditor.DropDownResizeModeProperty, PopupResizeMode.None);

                vs = DependencyPropertyHelper.GetValueSource(combo, XamComboEditor.MinDropDownWidthProperty);

                if (vs.BaseValueSource < BaseValueSource.Style)
                    combo.SetValue(XamComboEditor.MinDropDownWidthProperty, 150d);

            }

        }

            #endregion //OnEditorCreated	

			#region OnEditModeEnded

		/// <summary>
		/// Called when the embedded editor has just exited edit mode
		/// </summary>
		/// <param name="e">The event arguments</param>
		/// <seealso cref="ValueEditor.EditModeEnded"/>
		/// <seealso cref="OnEditModeStarting"/>
		/// <seealso cref="OnEditModeStarted"/>
		protected override void OnEditModeEnded(Infragistics.Windows.Editors.Events.EditModeEndedEventArgs e)
		{
			// AS 5/28/09 NA 2009.2 Undo/Redo
			// If the operator was changed but the value was not then we would not have 
			// gotten a CommitEditValue call but we still want to store an undo action
			// for the changes since we wouldn't have stored any while the changes were
			// being made with the cell in edit mode.
			//
			this.AddEditModeUndoAction();

			base.OnEditModeEnded(e);
		}
			#endregion //OnEditModeEnded

            #region OnEditModeStarted

        /// <summary>
        /// Called after edit mode has started
        /// </summary>
        protected override void OnEditModeStarted(Infragistics.Windows.Editors.Events.EditModeStartedEventArgs e)
        {
            this.SetIsTextSearchEnabled();
            base.OnEditModeStarted(e);
        }

            #endregion //OnEditModeStarted	

			#region OnEditModeStarting

		/// <summary>
		/// Called when the embedded editor is about to enter edit mode
		/// </summary>
		/// <param name="e">The event arguments</param>
		/// <remarks>Setting the <see cref="EditModeStartingEventArgs.Cancel"/> property to true will prevent the editor from entering edit mode.</remarks>
		/// <seealso cref="ValueEditor.EditModeStarting"/>
		/// <seealso cref="OnEditModeStarted"/>
		/// <seealso cref="CellValuePresenter.OnEditModeEnding"/>
		/// <seealso cref="OnEditModeEnded"/>
		protected override void OnEditModeStarting(Infragistics.Windows.Editors.Events.EditModeStartingEventArgs e)
		{
			base.OnEditModeStarting(e);

			// AS 5/28/09 NA 2009.2 Undo/Redo
			// When the value is committed we need to put an entry in the undo stack that 
			// represents the entire record filter and not just the operand. To that end 
			// we'll make a clone of the record filter now since it can be changed while 
			// in edit mode (e.g. when the evaluation trigger is oncellchange).
			//
			if (!e.Cancel)
			{
				FilterCell cell = this.Cell as FilterCell;

				Debug.Assert(null != cell);

				if (null != cell)
				{
					_originalFilter = cell.RecordFilter.Clone(true, cell.RecordFilter.ParentCollection, true);
				}
			}
		}
			#endregion //OnEditModeStarting

			// MD 7/16/10 - TFS26592
			// Moved code from the OnEditorValueChanged static method.
			#region OnEditorValueChanged

		/// <summary>
		/// Called when the editor's value has changed but it's <seealso cref="ValueEditor.ValueChanged"/> event has been suppressed.
		/// </summary>
		/// <param name="args">The event arguments</param>
		/// <seealso cref="ValueEditor.ValueChanged"/>
		protected internal override void OnEditorValueChanged(RoutedEventArgs args)
		{
			base.OnEditorValueChanged(args);

			ValueEditor editor = this.Editor;

			Debug.Assert(editor != null, "Editor should not be null in FilterCellValuePresenter.OnEditorValueChanged");
			Debug.Assert(editor == args.OriginalSource, "Editor should be original source in FilterCellValuePresenter.OnEditorValueChanged");

			if (editor != null)
			{
				// JJD 1/27/09
				// Check the ignore flag. If true then return
				if (this._ignoreValueChange)
					return;

				// JJD 1/22/09
				// If the operand list is dropped down we want to 
				// delay processing of the value until it is closed up 
				// JJD 07/06/10 - TFS32174 
				// If we are in the middle of loading then process the value change synchronously
				//if (fcvp.IsOperandDropDownOpen)
				if (this.IsOperandDropDownOpen && (this._loader == null || this._loader.EndReached == true))
					this._wasChangedWhileOperandDropDownOpen = true;
				else
				{
					// only process the value change if the delay flag
					// is false since ProcessValueChange will be invoked
					// asynchronously when the dropdown closes up
					if (!this._wasChangedWhileOperandDropDownOpen)
						this.ProcessValueChange();
				}
			}
		} 

			#endregion // OnEditorValueChanged

			#region OnFieldPropertyChanged

		/// <summary>
		/// Called when a property on the associated <see cref="Field"/> object changes
		/// </summary>
		/// <param name="e"></param>
		protected override void OnFieldPropertyChanged(PropertyChangedEventArgs e)
		{
			// JJD 3/9/11 - TFS67970 - Optimization
			// Make sure we call the ase because we replaced bindings with logic in OnFieldPropertyChanged
			// in the base CellValuePresenter class
			base.OnFieldPropertyChanged(e);

            switch (e.PropertyName)
            {
                // JJD 9/24/09 - TFS22535
                case "AllowRecordFilteringResolved":

                    // see if the new value is false
                    if (this.Field.AllowRecordFilteringResolved == false)
                    {
                        // since filetering is no longer allowed on this cell we need
                        // to exit edit mode and clear the active cell if it is active
                        if (this.IsActive)
                        {
                            // Exit edit mode first
                            if (this.IsInEditMode)
                                this.EndEditMode(false, true);

                            // clear the active cell
                            this.DataPresenter.ActiveCell = null;
                        }
                    }

                    this.CoerceValue(VisibilityProperty);
                    break;

                // JJD 4/26/10 - TFS30833 
                case "ConverterCultureResolved":
                    this.OnCultureChanged();
                    break;

				// JJD 11/9/11 - TFS95718 - added
                case "FilterCellEditorStyleResolved":
					if ( this.Field != null )
						this.Field.SetFilterCellEditorStyle(this.Editor);
                    break;

                case "FilterCellValuePresenterStyleResolved":
                    this.InvalidateStyleSelector();
                    break;
                case "FilterOperatorDropDownItemsResolved":
                case "FilterOperatorDefaultValueResolved":
                    this.OnFilterVersionChanged();
                    break;
                case "FilterOperandUITypeResolved":
                    // JJD 2/17/09 - TFS14047
                    // Exit edit mode first
                    if (this.IsActive && this.IsInEditMode)
                        this.EndEditMode(false, true);

                    // JJD 2/17/09 - TFS14047
                    // Then call InitializeEditorSettings will nulls to
                    // clear our the old editor instance
                    this.InitializeEditorSettings(null, null, null);

                    // JJD 2/17/09 - TFS14047
                    // Finally re-initialize the editor
                    // JJD 11/12/09 - TFS24752
                    // Pass null as the cellValue
                    //this.InitializeEditor();
                    this.InitializeEditor(null);

                    // JJD 8/20/09 - TFS19318
                    // Force the IsEnabled property to be coerced so our callback gets called
                    this.CoerceValue(IsEnabledProperty);

                    break;
            }
		}

			#endregion //OnFieldPropertyChanged
    
            #region OnIsActiveChanged

        /// <summary>
        /// Called when the IsActive property has changed
        /// </summary>
        protected override void OnIsActiveChanged()
        {
            // JJD 12/23/08
            // Clear the list of operands
            if (this.IsActive == false)
            {
                this.ClearOperandList();
            }
        }

            #endregion //OnIsActiveChanged	

            #region OnPreviewKeyDown

        /// <summary>
        /// Called when a key is pressed
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                FilterCell cell = this.Cell as FilterCell;

                if (cell == null)
                    return;

                switch (cell.Field.FilterEvaluationTriggerResolved)
                {
                    case FilterEvaluationTrigger.OnEnterKey:
                    case FilterEvaluationTrigger.OnEnterKeyOrLeaveCell:
                    case FilterEvaluationTrigger.OnEnterKeyOrLeaveRecord:
						cell.RecordFilter.ParentCollection.ApplyPendingFilters();
                        break;
                }
            }

			// JJD 07/06/10 - TFS32174 
			// Diable arrow key navigation durng the loading process
			if (this._loader != null &&
				 this._loader.EndReached == false)
			{
				switch (e.Key)
				{
					case Key.Up:
					case Key.Down:
						e.Handled = true;
						return;
				}

			}

			base.OnPreviewKeyDown(e);

        }

            #endregion //OnPreviewKeyDown	

            #region OnPreviewMouseLeftButtonDown

        /// <summary>
        /// Called when the left mouse button is pressed
        /// </summary>
        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {

            FilterCell fc = this.Cell as FilterCell;

            if (fc != null && !fc.IsInEditMode)
            {
                DataPresenterBase dp = this.DataPresenter;

                // If another cell is in edit mode then try to exit
                // if the exit was cancelled eat the command
                if (dp != null && dp.EndEditMode(true, false) == false)
                {
                    e.Handled = true;
                    return;
                }
            }

            base.OnPreviewMouseLeftButtonDown(e);
        }

            #endregion //OnPreviewMouseLeftButtonDown	
    
            // JJD 04/28/10 - TFS31383 - added
            #region OnPreviewTextInput

        /// <summary>
        /// Called when the text has been input
        /// </summary>
        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            base.OnPreviewTextInput(e);


            FilterCell cell = this.Cell as FilterCell;

            if (cell != null && this.IsInEditMode)
            {
                XamComboEditor combo = this.Editor as XamComboEditor;

                if (combo != null)
                {
                    // JJD 04/28/10 - TFS31383
                    // Make sure the operands list is populated with all but the unique key values
                    // so a match can be performed by the XamComboEditor
                    this.VerifyNonUniqueKeyOperandsArePopulated();
                }
            }

        }

            #endregion //OnPreviewTextInput	
    
        
#region Infragistics Source Cleanup (Region)


































#endregion // Infragistics Source Cleanup (Region)


        #endregion // Base Overrides

        #region Properties

            #region Public Properties

                #region HasActiveFilters

        /// <summary>
        /// Identifies the <see cref="HasActiveFilters"/> dependency property
        /// </summary>
        public static readonly DependencyProperty HasActiveFiltersProperty =
            FilterButton.HasActiveFiltersProperty.AddOwner(typeof(FilterCellValuePresenter));

        /// <summary>
        /// Returns true if there are any active filters applied to this cell (read-only)
        /// </summary>
        /// <seealso cref="HasActiveFiltersProperty"/>
        [Bindable(true)]
        [ReadOnly(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool HasActiveFilters
        {
            get
            {
                return (bool)this.GetValue(FilterCellValuePresenter.HasActiveFiltersProperty);
            }
        }

                #endregion //HasActiveFilters

                #region IsOperandDropDownOpen

        /// <summary>
        /// Identifies the <see cref="IsOperandDropDownOpen"/> dependency property
        /// </summary>
        public static readonly DependencyProperty IsOperandDropDownOpenProperty = DependencyProperty.Register("IsOperandDropDownOpen",
            typeof(bool), typeof(FilterCellValuePresenter), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnIsOperandDropDownOpenChanged)));

        private static void OnIsOperandDropDownOpenChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {

            FilterCellValuePresenter fcvp = target as FilterCellValuePresenter;

            if (fcvp != null)
            {
                if ((bool)e.NewValue == true)
                    fcvp.OnOperandDropDownOpened();
                else
                {
					// JJD 07/06/10 - TFS32174 
					// If we are loading then abort the operation
					if (fcvp._loader != null && !fcvp._loader.EndReached)
						fcvp._loader.Abort();

                    // JJD 1/22/09
                    // If a value change was detected while the operand list was dropped down then
                    // asynchronously call ProcesValueChange
                    if (fcvp._wasChangedWhileOperandDropDownOpen)
                        fcvp.Dispatcher.BeginInvoke(DispatcherPriority.Render, new GridUtilities.MethodDelegate(fcvp.ProcessValueChange));
                }
            }

        }

        /// <summary>
        /// Gets or sets if the operand drop down is open.
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> setting this property to true raises the <see cref="DataPresenterBase.RecordFilterDropDownOpening"/> event.</para>
        /// </remarks>
        /// <seealso cref="IsOperandDropDownOpenProperty"/>
        /// <seealso cref="Operands"/>
        /// <seealso cref="DataPresenterBase.RecordFilterDropDownOpening"/>
        [Bindable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        //[Description("Gets or sets if the operand drop down is open.")]
        //[Category("Behavior")]
        public bool IsOperandDropDownOpen
        {
            get
            {
                return (bool)this.GetValue(FilterCellValuePresenter.IsOperandDropDownOpenProperty);
            }
            set
            {
                this.SetValue(FilterCellValuePresenter.IsOperandDropDownOpenProperty, value);
            }
        }

                #endregion //IsOperandDropDownOpen

                #region IsOperatorDropDownOpen

        /// <summary>
        /// Identifies the <see cref="IsOperatorDropDownOpen"/> dependency property
        /// </summary>
        public static readonly DependencyProperty IsOperatorDropDownOpenProperty = DependencyProperty.Register("IsOperatorDropDownOpen",
            typeof(bool), typeof(FilterCellValuePresenter), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        /// Gets or sets if the operator drop down is open.
        /// </summary>
        /// <seealso cref="IsOperatorDropDownOpenProperty"/>
        [Bindable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        //[Description("Gets or sets if the operator drop down is open.")]
        //[Category("Behavior")]
        public bool IsOperatorDropDownOpen
        {
            get
            {
                return (bool)this.GetValue(FilterCellValuePresenter.IsOperatorDropDownOpenProperty);
            }
            set
            {
                this.SetValue(FilterCellValuePresenter.IsOperatorDropDownOpenProperty, value);
            }
        }

                #endregion //IsOperatorDropDownOpen

                #region Operands

        /// <summary>
        /// Returns a list of possible operands (read-only)
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> The Operands list gets lazily populated when the when the <see cref="IsOperandDropDownOpen"/> is first set to true
        /// after this cell is made active. Populating the list raises the <see cref="DataPresenterBase.RecordFilterDropDownPopulating"/> event. The list is then cleared when the cell is de-activated.
        /// </para>
        /// </remarks>
        /// <seealso cref="IsOperandDropDownOpen"/>
        /// <seealso cref="DataPresenterBase.RecordFilterDropDownPopulating"/>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        [Bindable(true)]
        public ObservableCollectionExtended<FilterDropDownItem> Operands
        {
            get
            {
                if (this._operands == null)
                    this._operands = new ObservableCollectionExtended<FilterDropDownItem>();

                return this._operands;
            }
        }

                #endregion //Operands

                #region Operator

        /// <summary>
        /// Identifies the <see cref="Operator"/> dependency property
        /// </summary>
        public static readonly DependencyProperty OperatorProperty = DependencyProperty.Register("Operator",
            typeof(ComparisonOperator?), typeof(FilterCellValuePresenter), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnOperatorChanged), new CoerceValueCallback(CoerceOperator)));

        private static object CoerceOperator(DependencyObject target, object value)
        {
            FilterCellValuePresenter fcvp = target as FilterCellValuePresenter;

            if (fcvp != null)
            {
                if (value == null)
                {
                    FilterCell cell = fcvp.Cell as FilterCell;
                    if (cell != null)
                        value = cell.RecordFilter.CurrentUIOperator;
                }
            }

            return value;
        }

        private static void OnOperatorChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            FilterCellValuePresenter fcvp = target as FilterCellValuePresenter;

            if (fcvp != null)
            {
                FilterCell cell = fcvp.Cell as FilterCell;
                if (cell != null && e.NewValue != null)
                {
                    if (fcvp.IsInEditMode)
                        fcvp.SetIsTextSearchEnabled();

                    cell.RecordFilter.CurrentUIOperator = ((ComparisonOperator?)e.NewValue).Value;
                    fcvp.OnCriteriaChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the operator to use for the filter
        /// </summary>
        /// <seealso cref="OperatorProperty"/>
        [Bindable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        //[Description("Gets or sets the operator to use for the filter")]
        //[Category("Behavior")]
		[TypeConverter( typeof( Infragistics.Windows.Helpers.NullableConverter<ComparisonOperator> ) )]
        public ComparisonOperator? Operator
        {
            get
            {
                return (ComparisonOperator?)this.GetValue(FilterCellValuePresenter.OperatorProperty);
            }
            set
            {
                this.SetValue(FilterCellValuePresenter.OperatorProperty, value);
            }
        }

                #endregion //Operator

            #endregion //Public Properties	

			#region Private/Internal Properties

				#region VerifyDropDownItemsVersion

		// SSP 7/17/09 TFS19258
		// We need to repopulate the drop-down items whenever data or filter criteria of some
		// other field changes.
		// 
		/// <summary>
		/// A number used to re-populate the filter drop-down items.
		/// </summary>
		private int VerifyDropDownItemsVersion
		{
			get
			{
				FilterRecord filterRecord = this.Record as FilterRecord;
				ResolvedRecordFilterCollection filters = null != filterRecord ? filterRecord.Filters : null;

				return SpecialFilterOperands.Version
					+ ( null != filters ? filters.Version : 0 );
			}
		}

				#endregion // VerifyDropDownItemsVersion

			#endregion // Private/Internal Properties
    
        #endregion //Properties	
    
        #region Methods

            #region Internal Methods

                #region ClearOperandList

        internal void ClearOperandList()
        {
			// JJD 06/29/10 - TFS32174 
			// Abort the load process 
			if (this._loader != null)
			{
				this._loader.Abort();
				this._loader = null;
			}

            this.Operands.Clear();

            FilterCell cell = this.Cell as FilterCell;

            if (cell != null)
            {
                FilterDropDownItem item = cell.RecordFilter.CurrentFilterDropDownItem;

                if (item != null)
                    this.Operands.Add(item);
            }

            // JJD 04/28/10 - TFS31383
            // Clear the flags that let us know what is loaded
            this._areNonUniqueValueOperandsLoaded = false;
            this._uniqueValueOperandsLoadPending = false;
        }

                #endregion //ClearOperandList

            #endregion //Internal Methods	
        
            #region Private Methods

                // JJD 8/20/09 - TFS19318
                // Added a coerce callback for the IsEnabled property
                #region CoerceIsEnabled

        private static object CoerceIsEnabled(DependencyObject target, object value)
        {
            FilterCellValuePresenter fcvp = target as FilterCellValuePresenter;

            if (fcvp != null && (bool)value == true)
                return fcvp.IsEditingAllowed;

            return value;
        }

                #endregion //CoerceIsEnabled	

                // JJD 9/24/09 - TFS22535
                // Added a coerce callback for the Visibility property
                #region CoerceVisibility

        private static object CoerceVisibility(DependencyObject target, object value)
        {
            FilterCellValuePresenter fcvp = target as FilterCellValuePresenter;

            if (fcvp != null && (Visibility)value == Visibility.Visible)
            {
                Field fld = fcvp.Field;

                if (fld != null && fld.AllowRecordFilteringResolved == false)
                    return KnownBoxes.VisibilityCollapsedBox;
            }

            
            // Call the base class coerce logic before returning
            //return value;
            return GridUtilities.CoerceFieldElementVisibility(fcvp, value);
        }

                #endregion //CoerceVisibility	
    
                #region OnCriteriaChanged

        private void OnCriteriaChanged()
        {
            if ( this._areOperatorAndOperandsInitialized == false)
                return;

            FilterCell cell = this.Cell as FilterCell;

            if (cell == null)
                return;

			if ( cell.Field.FilterEvaluationTriggerResolved == FilterEvaluationTrigger.OnCellValueChange )
			{
				// SSP 7/17/09 TFS19258
				// We need to repopulate the drop-down items whenever data or filter criteria of some
				// other field changes. However we should not repopulate the drop-down when the criteria
				// is changed via this filter cell because there's no need for that and re-populating
				// the drop-down can be in-efficient if there are a lot of data records.
				// 
				// ----------------------------------------------------------------------------------------
				//cell.RecordFilter.ParentCollection.ApplyPendingFilters( );
				bool wasDropDownItemsDirty = _verifiedDropDownItemsVersion != this.VerifyDropDownItemsVersion;

				cell.RecordFilter.ParentCollection.ApplyPendingFilters( );

				if ( ! wasDropDownItemsDirty )
					_verifiedDropDownItemsVersion = this.VerifyDropDownItemsVersion;
				// ----------------------------------------------------------------------------------------
			}

			// AS 7/21/09 NA 2009.2 Field Sizing
			FieldLayout fl = cell.Field.Owner;

			if (null != fl)
				fl.AutoSizeInfo.OnFilterCellChanged(cell);
        }

                #endregion //OnCriteriaChanged	
                
                // JJD 4/26/10 - TFS30833 - added
                #region OnCultureChanged

        private void OnCultureChanged()
        {
            SpecialFilterOperandBase operand = this.Value as SpecialFilterOperandBase;

            Field fld = this.Field;
            FilterCell cell = this.Cell as FilterCell;

            FilterDropDownItem fddi = cell != null ? cell.RecordFilter.CurrentFilterDropDownItem : null;

            if (operand != null && fddi != null && fddi.Value == operand)
            {
                // update the display text with the new cultrue info
                fddi.DisplayText = GridUtilities.ToString(operand.DisplayContent, operand.Name, GridUtilities.GetDefaultCulture(fld));
            }

            // we should ceare the operan list after the display text was update above so the cell gets updated
            // The operand list will be re-built with the new culture specific operands the next time it is needed
            this.ClearOperandList();
        }

                #endregion //OnCultureChanged	

				// JJD 06/29/10 - TFS32174 - added
				#region OnDropdownLoadCompleted

		private void OnDropdownLoadCompleted(object sender, EventArgs e)
		{
			Debug.Assert(this._loader == sender, "FilterCellValuePresenter.OnDropdownLoadCompleted");
			if (this._loader == sender)
			{
				this._uniqueValueOperandsLoadPending = !this._loader.EndReached;
				this._loader.Phase2Completed -= new EventHandler(OnDropdownLoadCompleted);
				this._loader = null;
			}
		}

				#endregion //OnDropdownLoadCompleted	
        
                // JJD 5/26/09 - TFS17564 - added class handler
                #region OnEditorValueChanged


        private static void OnEditorValueChanged(object sender, RoutedEventArgs e)
        {
            FilterCellValuePresenter fcvp = sender as FilterCellValuePresenter;

            if (fcvp != null)
            {
				// MD 7/16/10 - TFS26592
				// Moved call code to the new OnEditorValueChanged instance method so the logic can be used when 
				// the ValueChanged event is suppressed.
				#region Old Code

				//ValueEditor editor = fcvp.Editor;
				//
				//Debug.Assert(editor != null, "Editor should not be null in FilterCellValuePresenter.OnEditorValueChanged");
				//Debug.Assert(editor == e.OriginalSource, "Editor should be original source in FilterCellValuePresenter.OnEditorValueChanged");
				//
				//if (editor != null)
				//{
				//    // JJD 1/27/09
				//    // Check the ignore flag. If true then return
				//    if (fcvp._ignoreValueChange)
				//        return;
				//
				//    // JJD 1/22/09
				//    // If the operand list is dropped down we want to 
				//    // delay processing of the value until it is closed up 
				//    // JJD 07/06/10 - TFS32174 
				//    // If we are in the middle of loading then process the value change synchronously
				//    //if (fcvp.IsOperandDropDownOpen)
				//    if (fcvp.IsOperandDropDownOpen && (fcvp._loader == null || fcvp._loader.EndReached == true))
				//        fcvp._wasChangedWhileOperandDropDownOpen = true;
				//    else
				//    {
				//        // only process the value change if the delay flag
				//        // is false since ProcessValueChange will be invoked
				//        // asynchronously when the dropdown closes up
				//        if (!fcvp._wasChangedWhileOperandDropDownOpen)
				//            fcvp.ProcessValueChange();
				//    }
				//}  

				#endregion // Old Code
				fcvp.OnEditorValueChanged(e);
            }
        }

                #endregion //OnEditorValueChanged	
    
                #region OnOperandDropDownOpened


        // JJD 12/22/08 - added to support filtering
        private void OnOperandDropDownOpened()
        {
            FilterCell fcell = this.Cell as FilterCell;

            if ( fcell == null )
                return;

            // JJD 04/28/10 - TFS31383 
            // make sure the spcial, non-unigue data value operands have been loaded
            this.VerifyNonUniqueKeyOperandsArePopulated();

            ObservableCollectionExtended<FilterDropDownItem> operands = this.Operands;

            // Check for count < 2 since we may have 1 item in there to handle the current operand value
			// SSP 3/31/09 TFS15861
			// We need to re-populate the list whenever a special operand is registered or un-registered
			// via SpecialFilterOperands.
			// 
            //if (operands.Count < 2 )
			// SSP 7/17/09 TFS19258
			// We need to repopulate the drop-down items whenever data or filter criteria of some
			// other field changes.
			// 
			//if ( operands.Count < 2 || _verifiedSpecialOperandsVersion != SpecialFilterOperands.Version )
            
            // JJD 04/28/10 - TFS31383 
            // Check the flag to determine if we need to append the list with unique data value entries
            
            if (_uniqueValueOperandsLoadPending == true)
            {
				// SSP 3/31/09 TFS15861
				// 
				// SSP 7/17/09 TFS19258
				// Related to change above.
				// 
				//_verifiedSpecialOperandsVersion = SpecialFilterOperands.Version;
				//_verifiedDropDownItemsVersion = this.VerifyDropDownItemsVersion;

                bool includeFilteredOutValues = (Keyboard.Modifiers & ModifierKeys.Shift) != 0 ||
                    fcell.Field.Owner.RecordFiltersLogicalOperatorResolved == LogicalOperator.Or;

                // JJD 04/28/10 - TFS31383
                // Start with an empty list
                
                
                
				
				// JJD 06/29/10 - TFS32174 
				// Use the new loader instead
                //List<FilterDropDownItem> list = new List<FilterDropDownItem>();
				if (this._loader == null)
				{
					this._loader = new ResolvedRecordFilterCollection.FilterDropDownItemLoader(this.Editor as XamComboEditor, fcell.FieldFilterInfo, operands, false);
					this._loader.PopulatePhase1();
				}

				this._loader.PopulatePhase2(includeFilteredOutValues);

				// JJD 06/29/10 - TFS32174 
				// If the loader isn't done wire the Phase2Completed event
				if (this._loader.EndReached)
					this._loader = null;
				else
					this._loader.Phase2Completed += new EventHandler(OnDropdownLoadCompleted);

                // JJD 04/28/10 - TFS31383 
                // Populate the list with the unique data values entries
                //fcell.FieldFilterInfo.GetFilterDropDownItemsPhase2(list, includeFilteredOutValues);

                this._uniqueValueOperandsLoadPending = false;

				// JJD 06/29/10 - TFS32174 
				// Use the new loader instead
				//// JJD 04/28/10 - TFS31383 
				//// Append the unique data values to the operands list
				//if (list.Count > 0)
				//{
				//    // update the operands list we expose
				//    operands.BeginUpdate();
				//    //~operands.Clear();
				//    operands.AddRange(list);
				//    operands.EndUpdate();
				//}
            }

            // Raise the RecordFilterDropDownOpening event
            RecordFilterDropDownOpeningEventArgs args = new RecordFilterDropDownOpeningEventArgs(fcell.Field, fcell.Record.RecordManager, operands, false);

            this.DataPresenter.RaiseRecordFilterDropDownOpening(args);

       }


                #endregion //OnOperandDropDownOpened

                #region OnFilterVersionChanged

        // AS 1/27/09
        // Optimization - only have 1 parameterless void delegate class defined.
        //
        //delegate void MethodImpl();

        private void OnFilterVersionChanged()
        {
            FilterCell cell = this.Cell as FilterCell;

            if ( cell == null )
                return;

            ValueEditor editor = this.Editor;


            XamComboEditor combo = editor as XamComboEditor;

            // JJD 1/27/09
            // If the editor is a combo and its selected item is a command
            // then first clear the slecteditem property so if the user
            // re-selects it we will get notified
            if (combo != null &&
                 combo.SelectedIndex >= 0 &&
                 (combo.SelectedItem is ICommand ||
                  (combo.SelectedItem is FilterDropDownItem &&
                   ((FilterDropDownItem)(combo.SelectedItem)).IsAction)))
            {
                try
                {
                    // JJD 1/27/09
                    // set a flag know to ignore value change notification from the Clear below
                    this._ignoreValueChange = true;

                    combo.ClearValue(XamComboEditor.SelectedIndexProperty);
                }
                finally
                {
                    // JJD 1/27/09
                    // Reset the ignore flag
                    this._ignoreValueChange = false;
                }
            }


			// AS 7/21/09 Optimization - Only get the RecordFilter once.
			//
			RecordFilter filter = cell.RecordFilter;

            this.Operator = filter.CurrentUIOperator;
			this.Value = filter.CurrentUIOperand;

			// JJD 02/21/12 - TFS99332
			// If we have a CurrentFilterDropDownItem but the SelectedIndex of the combo is -1
			// then we need to add the CurrentFilterDropDownItem to the Operannds collection
			// and select it
			if (combo != null)
			{
				// JJD 03/04/12 - TFS103722
				// If the value is null then clear the comboo's SelectedItem property 
				// as well as the CurrentFilterDropDownItem property on the RecordFilter
				if (this.Value == null)
				{
					combo.ClearValue(XamComboEditor.SelectedIndexProperty);
					filter.CurrentFilterDropDownItem = null;
				}
				else if (combo.SelectedIndex < 0 && filter.CurrentFilterDropDownItem != null)
				{
					// see if it is already in the operands list
					int index = this.Operands.IndexOf(filter.CurrentFilterDropDownItem);

					if (index < 0)
					{
						// if we are not in edit mode then we can clear the old entry out
						if (this.Operands.Count == 1 && !this.IsInEditMode)
							this.Operands.Clear();

						// add the CurrentFilterDropDownItem
						this.Operands.Add(filter.CurrentFilterDropDownItem);

						// use its index to set SelectIndex below
						index = this.Operands.Count - 1;
					}

					// set the combo's SelectedIndex
					combo.SelectedIndex = index;
				}
			}

            if (cell.HasActiveFilters)
                this.SetValue(FilterButton.HasActiveFiltersPropertyKey, KnownBoxes.TrueBox);
            else
                this.ClearValue(FilterButton.HasActiveFiltersPropertyKey);


            if (editor != null)
            {
                if ( editor.IsInEditMode )
					editor.Value = filter.CurrentUIOperand;

				object tooltip = filter.Tooltip;

                if ( tooltip == null || (tooltip is string && ((string)tooltip).Length == 0 ))
                    editor.ClearValue(ValueEditor.ToolTipProperty);
                else
                    editor.ToolTip = tooltip;
            }

        }

   	            #endregion //OnFilterVersionChanged	

                #region ProcessValueChange


        private void ProcessValueChange()
        {
            // clear the delayed process flag
            this._wasChangedWhileOperandDropDownOpen = false;
			
			// JJD 06/29/10 - TFS32174 
			// Abort the load process 
			if (this._loader != null)
			{
				this._loader.Abort();
				this._loader = null;
			}

            // make sure the value is valid
            if (this.Editor.IsValueValid)
            {
                FilterCell cell = this.Cell as FilterCell;

                if (cell != null)
                {
                    object tempValue;

                    // if we are in edit mode then get the value from the editor
                    // otherwise use this.Value
                    if (this.IsInEditMode)
                    {
                        tempValue = this.Editor.Value;

                        XamComboEditor combo = this.Editor as XamComboEditor;

                        if (combo != null)
                            cell.RecordFilter.CurrentFilterDropDownItem = combo.SelectedItem as FilterDropDownItem;
                    }
                    else
                        tempValue = this.Value;

                    // set the CurrentUIOperand 
                    cell.RecordFilter.CurrentUIOperand = tempValue;

                    // call OnCriteria Changed for anything other than a command
                    if (!(tempValue is ICommand))
                        this.OnCriteriaChanged();
                    else
                    {
                        // for commands asynchronuously call OnFilterVersionChanged
                        // so we can refresh the cell value
                        this.Dispatcher.BeginInvoke(DispatcherPriority.Send, new GridUtilities.MethodDelegate(this.OnFilterVersionChanged));
                    }
                }
            }

        }


                #endregion //ProcessValueChange	
    
                #region SetIsTextSearchEnabled

        private void SetIsTextSearchEnabled()
        {


            XamComboEditor combo = this.Editor as XamComboEditor;

            if (combo != null && combo.IsInEditMode && this.Operator.HasValue)
            {
                bool rtn = GridUtilities.SetIsTextSearchEnabled(combo, this.Operator.Value);

                Debug.Assert(rtn == true, "SetIsTextSearchEnabled failed in OnEditModeStarting");
            }

        }
                #endregion //SetIsTextSearchEnabled	

                // JJD 04/28/10 - TFS31383 - added
                #region VerifyNonUniqueKeyOperandsArePopulated

        private void VerifyNonUniqueKeyOperandsArePopulated()
        {
            if (this._areNonUniqueValueOperandsLoaded && _verifiedDropDownItemsVersion == this.VerifyDropDownItemsVersion)
                return;

            FilterCell fcell = this.Cell as FilterCell;

            if (fcell == null)
                return;

            ObservableCollectionExtended<FilterDropDownItem> operands = this.Operands;
            
            _verifiedDropDownItemsVersion = this.VerifyDropDownItemsVersion;

            _areNonUniqueValueOperandsLoaded = true;

			// JJD 06/29/10 - TFS32174 
			// Use the new FilterDropDownItemLoader instead
			//// Call GetFilterDropDownItems off FieldFilterInfo
			//// Note: this will raise the RecordFilterDropDownPopulating
			//List<FilterDropDownItem> list = fcell.FieldFilterInfo.GetFilterDropDownItemsPhase1(false, out this._uniqueValueOperandsLoadPending);

			//// update the operands list we expose
			//operands.BeginUpdate();
			//operands.Clear();
			//operands.AddRange(list);
			//operands.EndUpdate();
			if (this._loader != null)
				this._loader.Abort();

			this._loader = new ResolvedRecordFilterCollection.FilterDropDownItemLoader(this.Editor as XamComboEditor, fcell.FieldFilterInfo, operands, false);
			this._loader.PopulatePhase1();

			_uniqueValueOperandsLoadPending = this._loader.UniqueValuesLoadPending;

        }

                #endregion //VerifyNonUniqueKeyOperandsArePopulated	
    
            #endregion //Private Methods
        
        #endregion //Methods

    }

	#endregion // FilterCellValuePresenter Class

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