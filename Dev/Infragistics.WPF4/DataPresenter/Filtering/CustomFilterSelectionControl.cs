using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Infragistics.Windows.Controls;
using System.Windows.Input;
using Infragistics.Windows.Helpers;
using System.Drawing;
using System.Windows.Media;
using System.Diagnostics;
using System.Windows.Data;
using Infragistics.Windows.Editors;
using Infragistics.Shared;
using System.Collections;
using Infragistics.Collections;

namespace Infragistics.Windows.DataPresenter
{
	/// <summary>
	/// Provides a custom filtering UI that allows the user to specify multiple filter conditions for a single <see cref="Field"/>
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// This control is displayed modally in an appropriate top level window when the user selects '(Custom)' from the  filter dropdown list.
	/// </para>
	/// </remarks>
	/// <seealso cref="Field"/>
	/// <seealso cref="RecordFilter"/>
	[TemplatePart(Name="PART_ConditionsGrid", Type=typeof(XamDataGrid))]
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class CustomFilterSelectionControl : Control
	{

		#region Member Variables

		private bool													_initializeMethodCalled;
		private XamDataGrid												_conditionsGrid;
		private ConditionInfoCollection									_conditionInfos;
		private ConditionInfoGroup										_rootConditionInfoGroup;
		private int														_totalGroupLevels;
		private List<Field>												_unboundFields;
		private bool													_currentSelectionCanBeGrouped;
		private bool													_currentSelectionCanBeUngrouped;
		private bool													_currentSelectionSpansMultipleGroups;
		private int														_currentSelectionCount;
		private ObservableCollectionExtended<FilterDropDownItem>		_operands;
		private bool													_isDirty;
		private ConditionGroup											_tempRootConditionGroup;

        // AS 3/12/09 TFS15327
        private ComparisonOperatorSelector                              _comparisonOperatorSelector;

		// JJD 06/29/10 - TFS32174 - added
		private ResolvedRecordFilterCollection.FilterDropDownItemLoader _loader;

		// AS 1/14/11 TFS63183
		private ComboEditorInitializer									_operatorInitializer;
		private ComboEditorInitializer									_operandInitializer;

		#endregion //Member Variables

		#region Constants

		private const int								MAX_GROUPLEVELS = 50;

		#endregion //Constants

		#region Constructors

		static CustomFilterSelectionControl()
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomFilterSelectionControl), new FrameworkPropertyMetadata(typeof(CustomFilterSelectionControl)));

            // AS 3/12/09 TFS15327
            // This isn't specific to this but this control really shouldn't get focus by default.
            //
            UIElement.FocusableProperty.OverrideMetadata(typeof(CustomFilterSelectionControl), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

			#region Create Commands and CommandBindings

			#region AddConditionCommand
			AddConditionCommand							= new RoutedCommand("AddCondition", typeof(CustomFilterSelectionControl));
			CommandBinding commandBindingAddCondition	= new CommandBinding(AddConditionCommand,
				CustomFilterSelectionControl.ExecuteCommandHandler, CustomFilterSelectionControl.CanExecuteCommandHandler);

			CommandManager.RegisterClassCommandBinding(typeof(CustomFilterSelectionControl), commandBindingAddCondition);
			#endregion // AddConditionCommand

			#region CancelChangesCommand
			CancelChangesCommand = new RoutedCommand("CancelChanges", typeof(CustomFilterSelectionControl));
			CommandBinding commandBindingCancelChanges = new CommandBinding(CancelChangesCommand,
				CustomFilterSelectionControl.ExecuteCommandHandler, CustomFilterSelectionControl.CanExecuteCommandHandler);

			CommandManager.RegisterClassCommandBinding(typeof(CustomFilterSelectionControl), commandBindingCancelChanges);
			#endregion // CancelChangesCommand

			#region CommitChangesCommand
			CommitChangesCommand = new RoutedCommand("CommitChanges", typeof(CustomFilterSelectionControl));
			CommandBinding commandBindingCommitChanges = new CommandBinding(CommitChangesCommand,
				CustomFilterSelectionControl.ExecuteCommandHandler, CustomFilterSelectionControl.CanExecuteCommandHandler);

			CommandManager.RegisterClassCommandBinding(typeof(CustomFilterSelectionControl), commandBindingCommitChanges);
			#endregion // CommitChangesCommand

			#region GroupSelectedConditionsAsAndGroupCommand
			GroupSelectedConditionsAsAndGroupCommand = new RoutedCommand("GroupSelectedConditionsAsAndGroup", typeof(CustomFilterSelectionControl));
			CommandBinding commandBindingGroupSelectedConditionsAsAndGroup = new CommandBinding(GroupSelectedConditionsAsAndGroupCommand,
				CustomFilterSelectionControl.ExecuteCommandHandler, CustomFilterSelectionControl.CanExecuteCommandHandler);

			CommandManager.RegisterClassCommandBinding(typeof(CustomFilterSelectionControl), commandBindingGroupSelectedConditionsAsAndGroup);
			#endregion // GroupSelectedConditionsAsAndGroupCommand

			#region GroupSelectedConditionsAsOrGroupCommand
			GroupSelectedConditionsAsOrGroupCommand = new RoutedCommand("GroupSelectedConditionsAsOrGroup", typeof(CustomFilterSelectionControl));
			CommandBinding commandBindingGroupSelectedConditionsAsOrGroup = new CommandBinding(GroupSelectedConditionsAsOrGroupCommand,
				CustomFilterSelectionControl.ExecuteCommandHandler, CustomFilterSelectionControl.CanExecuteCommandHandler);

			CommandManager.RegisterClassCommandBinding(typeof(CustomFilterSelectionControl), commandBindingGroupSelectedConditionsAsOrGroup);
			#endregion // GroupSelectedConditionsAsOrGroupCommand

			#region RemoveSelectedConditionsCommand
			RemoveSelectedConditionsCommand							= new RoutedCommand("RemoveSelectedConditions", typeof(CustomFilterSelectionControl));
			CommandBinding commandBindingRemoveSelectedConditions	= new CommandBinding(RemoveSelectedConditionsCommand,
				CustomFilterSelectionControl.ExecuteCommandHandler, CustomFilterSelectionControl.CanExecuteCommandHandler);

			CommandManager.RegisterClassCommandBinding(typeof(CustomFilterSelectionControl), commandBindingRemoveSelectedConditions);
			#endregion // RemoveSelectedConditionsCommand

			#region ToggleSelectedGroupLogicalOperatorCommand
			ToggleSelectedGroupLogicalOperatorCommand = new RoutedCommand("ToggleSelectedGroupLogicalOperator", typeof(CustomFilterSelectionControl));
			CommandBinding commandBindingToggleSelectedGroupLogicalOperator = new CommandBinding(ToggleSelectedGroupLogicalOperatorCommand,
				CustomFilterSelectionControl.ExecuteCommandHandler, CustomFilterSelectionControl.CanExecuteCommandHandler);

			CommandManager.RegisterClassCommandBinding(typeof(CustomFilterSelectionControl), commandBindingToggleSelectedGroupLogicalOperator);
			#endregion // ToggleSelectedGroupLogicalOperatorCommand

			#region UngroupSelectedConditionsCommand
			UngroupSelectedConditionsCommand						= new RoutedCommand("UngroupSelectedConditionsAsAndGroup", typeof(CustomFilterSelectionControl));
			CommandBinding commandBindingUngroupSelectedConditionsAsAndGroup	= new CommandBinding(UngroupSelectedConditionsCommand,
				CustomFilterSelectionControl.ExecuteCommandHandler, CustomFilterSelectionControl.CanExecuteCommandHandler);

			CommandManager.RegisterClassCommandBinding(typeof(CustomFilterSelectionControl), commandBindingUngroupSelectedConditionsAsAndGroup);
			#endregion // UngroupSelectedConditionsCommand

			#endregion // Create Commands and CommandBindings
		}

		/// <summary>
		/// Creates an instance of the CustomFilterSelectionControl.  When using this constructor you must call the Initialize method to supply needed parameters and context. 
		/// </summary>
		public CustomFilterSelectionControl()
		{
			CustomFilterSelectionControl.SetControl(this, this); // AS 1/14/11 TFS63183
		}

		/// <summary>
		/// Creates an instance of the CustomFilterSelectionControl.
		/// </summary>
		/// <param name="recordFilter">The <see cref="RecordFilter"/> containing the <see cref="Infragistics.Windows.Controls.ICondition"/>s to be customized for a specific <see cref="Field"/></param>
		/// <param name="recordManager">The <see cref="RecordManager"/> associated with the records being filtered.</param>
		/// <seealso cref="RecordFilter"/>
		/// <seealso cref="RecordManager"/>
		/// <seealso cref="Infragistics.Windows.Controls.ICondition"/>
		public CustomFilterSelectionControl(RecordFilter recordFilter, RecordManager recordManager)
			: this() // AS 1/14/11 TFS63183
		{
			this.Initialize(recordFilter, recordManager);
		}

		#endregion //Constructors

		#region Commands

			#region AddConditionCommand

        /// <summary>
		/// Adds a <see cref="Infragistics.Windows.Controls.ComparisonCondition"/> to the list of <see cref="RecordFilter"/> conditions being managed by the <see cref="CustomFilterSelectionControl"/> for a specific <see cref="Field"/>.
        /// </summary>
		public static readonly RoutedCommand AddConditionCommand;

			#endregion //AddConditionCommand

			#region CancelChangesCommand

        /// <summary>
		/// Cancels any changes made to the list of <see cref="RecordFilter"/> conditions and hides the control.
        /// </summary>
		public static readonly RoutedCommand CancelChangesCommand;

			#endregion //CancelChangesCommand

			#region CommitChangesCommand

        /// <summary>
		/// Commits any changes made to the list of <see cref="RecordFilter"/> conditions and hides the control.
        /// </summary>
		public static readonly RoutedCommand CommitChangesCommand;

			#endregion //CommitChangesCommand

			#region GroupSelectedConditionsAsAndGroupCommand

        /// <summary>
		/// Groups (using an 'And' LogicalOperator) the currently selected <see cref="Infragistics.Windows.Controls.ComparisonCondition"/>s (if any) in the list of <see cref="RecordFilter"/> conditions being managed by the <see cref="CustomFilterSelectionControl"/> for a specific <see cref="Field"/>.
        /// </summary>
		public static readonly RoutedCommand GroupSelectedConditionsAsAndGroupCommand;

			#endregion //GroupSelectedConditionsAsAndGroupCommand

			#region GroupSelectedConditionsAsOrGroupCommand

		/// <summary>
		/// Groups (using an 'Or' LogicalOperator) the currently selected <see cref="Infragistics.Windows.Controls.ComparisonCondition"/>s (if any) in the list of <see cref="RecordFilter"/> conditions being managed by the <see cref="CustomFilterSelectionControl"/> for a specific <see cref="Field"/>.
		/// </summary>
		public static readonly RoutedCommand GroupSelectedConditionsAsOrGroupCommand;

			#endregion //GroupSelectedConditionsAsOrGroupCommand

			#region RemoveSelectedConditionsCommand

		/// <summary>
		/// Removes the currently selected <see cref="Infragistics.Windows.Controls.ComparisonCondition"/>s (if any) from the list of <see cref="RecordFilter"/> conditions being managed by the <see cref="CustomFilterSelectionControl"/> for a specific <see cref="Field"/>.
        /// </summary>
		public static readonly RoutedCommand RemoveSelectedConditionsCommand;

			#endregion //RemoveSelectedConditionsCommand

			#region ToggleSelectedGroupLogicalOperatorCommand

		/// <summary>
		/// Toggles the <see cref="LogicalOperator"/> of the group associated with the currently selected <see cref="Infragistics.Windows.Controls.ComparisonCondition"/>s between 'Or' and 'And'./>.
		/// </summary>
		public static readonly RoutedCommand ToggleSelectedGroupLogicalOperatorCommand;

			#endregion //ToggleSelectedGroupLogicalOperatorCommand

			#region UngroupSelectedConditionsCommand

		/// <summary>
		/// Ungroups the currently selected <see cref="Infragistics.Windows.Controls.ComparisonCondition"/>s (if any) in the list of <see cref="RecordFilter"/> conditions being managed by the <see cref="CustomFilterSelectionControl"/> for a specific <see cref="Field"/>.
        /// </summary>
		public static readonly RoutedCommand UngroupSelectedConditionsCommand;

			#endregion //UngroupSelectedConditionsCommand

		#endregion //Commands

		#region CommandHandlers

			#region CanExecuteCommandHandler

		private static void CanExecuteCommandHandler(object sender, CanExecuteRoutedEventArgs e)
		{
			CustomFilterSelectionControl customFilterSelectionControl = sender as CustomFilterSelectionControl;

			e.CanExecute = false;

			if (customFilterSelectionControl != null)
			{
				if (e.Command == AddConditionCommand)
                {
                    // AS 3/12/09 TFS15327
                    // This isn't specific to this bug but I noticed that focus doesn't start 
                    // in the first button. When I tried to put focus into it it failed because 
                    // the add button (which is the first in our default template) is using this 
                    // command and we return false because we only set the _conditionInfos in the 
                    // loaded. I tried to call InvalidateRequerySuggested but that happens on the 
                    // Background call which is a bit late.
                    //
					//e.CanExecute = customFilterSelectionControl._conditionInfos != null;
					e.CanExecute = true;
                }
				else
				if (e.Command == RemoveSelectedConditionsCommand)
				{
					// JM 05-8-12 TFS110624 - Allow ActiveRecord if no records are selected. 
					//e.CanExecute = customFilterSelectionControl._conditionsGrid			!= null		&&
					//               customFilterSelectionControl.IsConditionInEditMode	== false	&&
					//               customFilterSelectionControl._conditionsGrid.SelectedItems.Records.Count > 0;
					e.CanExecute = customFilterSelectionControl._conditionsGrid			!= null &&
								   customFilterSelectionControl.IsConditionInEditMode	== false &&
								  (customFilterSelectionControl._conditionsGrid.SelectedItems.Records.Count > 0 ||
								   customFilterSelectionControl._conditionsGrid.ActiveRecord				!= null);
				}
				else
				if (e.Command == GroupSelectedConditionsAsAndGroupCommand ||
					e.Command == GroupSelectedConditionsAsOrGroupCommand)
				{
					e.CanExecute = customFilterSelectionControl._currentSelectionCanBeGrouped &&
								   customFilterSelectionControl.IsConditionInEditMode == false;
				}
				else
				if (e.Command == UngroupSelectedConditionsCommand)
				{
					e.CanExecute = customFilterSelectionControl._currentSelectionCanBeUngrouped &&
								   customFilterSelectionControl.IsConditionInEditMode == false;
				}
				else
				if (e.Command == CancelChangesCommand)
				{
					e.CanExecute = true;
				}
				else
				if (e.Command == CommitChangesCommand)
				{
					e.CanExecute = customFilterSelectionControl._conditionInfos != null && customFilterSelectionControl._isDirty == true;
				}
				else
				if (e.Command == ToggleSelectedGroupLogicalOperatorCommand)
				{
					// JM 05-8-12 TFS110624 - Allow ActiveRecord if no records are selected. 
					//e.CanExecute = customFilterSelectionControl._conditionsGrid != null &&
					//               customFilterSelectionControl.IsConditionInEditMode						== false	&&
					//               customFilterSelectionControl._conditionsGrid.SelectedItems.Records.Count > 0			&&
					//               customFilterSelectionControl._currentSelectionSpansMultipleGroups		== false;
					e.CanExecute = customFilterSelectionControl._conditionsGrid								!= null		&&
								   customFilterSelectionControl.IsConditionInEditMode						== false	&&
								  (customFilterSelectionControl._conditionsGrid.SelectedItems.Records.Count > 0 ||
								   customFilterSelectionControl._conditionsGrid.ActiveRecord				!= null)	&&
								   customFilterSelectionControl._currentSelectionSpansMultipleGroups		== false;
				}
			}
		}

			#endregion //CanExecuteCommandHandler	

			#region ExecuteCommandHandler

		private static void ExecuteCommandHandler(object sender, ExecutedRoutedEventArgs e)
		{
			CustomFilterSelectionControl customFilterSelectionControl= sender as CustomFilterSelectionControl;
			if (customFilterSelectionControl != null)
				customFilterSelectionControl.ExecuteCommandImpl(e.Command as RoutedCommand, e.Parameter, e.Source, e.OriginalSource);
		}

			#endregion //ExecuteCommandHandler

			#region ExecuteCommandImpl

		private bool ExecuteCommandImpl(RoutedCommand command, object parameter, object source, object originalSource)
		{
			// Make sure we have a command to execute.
			if (null == command)
				throw new ArgumentNullException(DataPresenterBase.GetString("LE_ArgumentNullException_6"));

			bool handled = false;

			if (command == AddConditionCommand)
				handled = this.ExecuteAddConditionCommand(source, originalSource, parameter);
			else
			if (command == RemoveSelectedConditionsCommand)
				handled = this.ExecuteRemoveSelectedConditionsCommand(source, originalSource, parameter);
			else
			if (command == GroupSelectedConditionsAsAndGroupCommand)
				handled = this.ExecuteGroupSelectedConditionsAsAndGroupCommand(source, originalSource, parameter);
			else
			if (command == GroupSelectedConditionsAsOrGroupCommand)
				handled = this.ExecuteGroupSelectedConditionsAsOrGroupCommand(source, originalSource, parameter);
			else
			if (command == UngroupSelectedConditionsCommand)
				handled = this.ExecuteUngroupSelectedConditionsCommand(source, originalSource, parameter);
			else
			if (command == CancelChangesCommand)
				handled = this.ExecuteCancelChangesCommand(source, originalSource, parameter);
			else
			if (command == CommitChangesCommand)
				handled = this.ExecuteCommitChangesCommand(source, originalSource, parameter);
			else
			if (command == ToggleSelectedGroupLogicalOperatorCommand)
				handled = this.ExecuteToggleSelectedGroupLogicalOperatorCommand(source, originalSource, parameter);

			return handled;
		}

			#endregion //ExecuteCommandImpl

			#region ExecuteAddConditionCommand

		private bool ExecuteAddConditionCommand(object source, object originalSource, object parameter)
		{
			if (this.IsConditionInEditMode == true && false == this.TryEndEditMode())
				return true;


			if (this._conditionInfos != null)
			{
				// If conditions are currently selected and they are all from the same group, add the new condition to that group.
				// Otherwise add it to the root condition group.
				ConditionInfoGroup groupToAddConditionTo = this._rootConditionInfoGroup;

				if (this._currentSelectionCount > 0 && this._currentSelectionSpansMultipleGroups == false)
				{
					List<ConditionInfo> selectedConditionInfos = this.GetSelectedConditionInfos();
					groupToAddConditionTo = selectedConditionInfos[0].Group;
				}
				else // JM 02-17-09 TFS 13987 - Use the ActiveRecord if there is one.
				{
					if (this._conditionsGrid.ActiveRecord != null)
					{
						ConditionInfo activeConditionInfo = ((DataRecord)this._conditionsGrid.ActiveRecord).DataItem as ConditionInfo;
						if (activeConditionInfo != null)
							groupToAddConditionTo = activeConditionInfo.Group;
					}
				}



				// JJD 02/17/12 - TFS101703 
				// Added DisplayText parameter. Pass in null for groups 
				//ConditionInfo newConditionInfo = new ConditionInfo(groupToAddConditionTo, this.RecordFilter.Field.FilterOperatorDefaultValueResolved, string.Empty);
				ConditionInfo newConditionInfo = new ConditionInfo(groupToAddConditionTo, this.RecordFilter.Field.FilterOperatorDefaultValueResolved, string.Empty, null);
				groupToAddConditionTo.ChildConditions.Add(newConditionInfo);


				// Refresh some status and force the conditions grid to update it's unbound fields to reflect the new grouping.
				this.UpdateSelectionDependentFlags();
				this.AddRemoveUnboundFieldsUsedAsGroupingIndicators();
				this.RefreshConditionsGrid();


				// JM 01-22-09 TFS12620 - place the Operand cell of the newly added record into edit mode.
				DataRecord newConditionDataRecord = this._conditionsGrid.RecordManager.Unsorted[this._conditionInfos.IndexOf(newConditionInfo)];							
				if (newConditionDataRecord != null)
				{
					this._conditionsGrid.ActiveCell = newConditionDataRecord.Cells["Value"];
					this._conditionsGrid.ExecuteCommand(DataPresenterCommands.StartEditMode);
				}

				return true;
			}

			return false;
		}

			#endregion //ExecuteAddConditionCommand

			#region ExecuteCancelChangesCommand

		private bool ExecuteCancelChangesCommand(object source, object originalSource, object parameter)
		{
			if (this._isDirty)
			{
				MessageBoxResult result = Infragistics.Windows.Utilities.ShowMessageBox(this,
                                                                                        // AS 3/9/09 TFS13972
                                                                                        //SR.GetString("CustomFilterSelectionControl_CancelMessageTextProperty"), 
																						//SR.GetString("CustomFilterSelectionControl_CancelMessageTitleProperty"), 
                                                                                        this.CancelMessageText,
                                                                                        this.CancelMessageTitle,
																						MessageBoxButton.YesNo, 
																						MessageBoxImage.Warning);
				if (result == MessageBoxResult.No)
					return true;
			}


			ToolWindow toolWindow = ToolWindow.GetToolWindow(this);
			if (toolWindow != null)
				toolWindow.DialogResult = false;

			return true;
		}

			#endregion //ExecuteCancelChangesCommand

			#region ExecuteCommitChangesCommand

		private bool ExecuteCommitChangesCommand(object source, object originalSource, object parameter)
		{
			if (this._tempRootConditionGroup == null || this._isDirty == false)
				return false;
			

			// Check to see if we have pending errors in the conditions.
			if (this.ConditionsContainErrors)
			{
				MessageBoxResult result = Infragistics.Windows.Utilities.ShowMessageBox(this,
																						DataPresenterBase.GetString("CustomFilterSelectionControl_ConditionErrorMessageTextProperty"),
																						DataPresenterBase.GetString("CustomFilterSelectionControl_ConditionErrorMessageTitleProperty"),
																						MessageBoxButton.OK,
																						MessageBoxImage.Error);
				
				return true;
			}


			// Update the RecordFilter that was passed to us with the new ConditionGroup.
			// AS - NA 11.2 Excel Style Filtering
			// ApplyNewConditions will result in an exception if the RecordFilter provided is 
			// not associated with a parentCollection. Since I want to be able to pass in 
			// arbitrary record filters we need to apply the changes to the actual source 
			// filter.
			//
			//if (this.RecordFilter != null)
			//    this.RecordFilter.ApplyNewConditions(this._tempRootConditionGroup);
			var recordFilter = this.GetTargetRecordFilter();
			if (recordFilter != null)
				recordFilter.ApplyNewConditions(this._tempRootConditionGroup);

			// Dismiss the ToolWindow that is hosting us.
			ToolWindow toolWindow = ToolWindow.GetToolWindow(this);
			if (toolWindow != null)
				toolWindow.DialogResult = true;

			return true;
		}

			#endregion //ExecuteCommitChangesCommand

			#region ExecuteGroupSelectedConditionsAsAndGroupCommand

		private bool ExecuteGroupSelectedConditionsAsAndGroupCommand(object source, object originalSource, object parameter)
		{
			return this.GroupSelectedConditionsHelper(LogicalOperator.And);
		}

			#endregion //ExecuteGroupSelectedConditionsAsAndGroupCommand

			#region ExecuteGroupSelectedConditionsAsOrGroupCommand

		private bool ExecuteGroupSelectedConditionsAsOrGroupCommand(object source, object originalSource, object parameter)
		{
			return this.GroupSelectedConditionsHelper(LogicalOperator.Or);
		}

			#endregion //ExecuteGroupSelectedConditionsAsOrGroupCommand

			#region ExecuteRemoveSelectedConditionsCommand

		private bool ExecuteRemoveSelectedConditionsCommand(object source, object originalSource, object parameter)
		{
			// JM 06-02-09 TFS 18008 - Moved the code below into a separate routine (RemoveSelectedConditions) so it
			//						   can also be called from OnConditionsGridRecordsDeleting.
			return this.RemoveSelectedConditions();

			#region Following code moved to RemoveSelectedConditions method
			
#region Infragistics Source Cleanup (Region)





































#endregion // Infragistics Source Cleanup (Region)

			#endregion //Following code moved to RemoveSelectedConditions method
		}

			#endregion //ExecuteRemoveSelectedConditionsCommand

			#region ExecuteToggleSelectedGroupLogicalOperatorCommand

		private bool ExecuteToggleSelectedGroupLogicalOperatorCommand(object source, object originalSource, object parameter)
		{
			// Check the flag which indicates whether the current selection spans multiple groups (theoretically we shouldn't have to do this
			// since the 'CanExecute' for this command should prevent us from getting here if the flag is false)
			if (this._currentSelectionSpansMultipleGroups)
				return false;


			// JM 05-8-12 TFS110624 - Call the new overload with true.
			//List<ConditionInfo> selectedConditionInfos = this.GetSelectedConditionInfos();
			List<ConditionInfo> selectedConditionInfos = this.GetSelectedConditionInfos(true);
			if (selectedConditionInfos.Count > 0)
			{
				ConditionInfoGroup	currentConditionInfoGroup	= selectedConditionInfos[0].Group;
				LogicalOperator		newLogicalOperator			= currentConditionInfoGroup.LogicalOperator == LogicalOperator.And ? LogicalOperator.Or : LogicalOperator.And;

				currentConditionInfoGroup.LogicalOperator		= newLogicalOperator;

				// Force the conditions grid to update it's unbound fields to reflect the grouping's new operator.
				this.RefreshConditionsGrid();

				return true;
			}

			return false;
		}

			#endregion //ExecuteToggleSelectedGroupLogicalOperatorCommand

			#region ExecuteUngroupSelectedConditionsCommand

		private bool ExecuteUngroupSelectedConditionsCommand(object source, object originalSource, object parameter)
		{
			// Check the flag which indicates whether the current selection can be ungrouped (theoretically we shouldn't have to do this
			// since the 'CanExecute' for this command should prevent us from getting here if the flag is false)
			if (false == this._currentSelectionCanBeUngrouped)
				return false;


			// Make sure we have at least 1 selected ConditionInfo.
			List<ConditionInfo> selectedConditionInfos = this.GetSelectedConditionInfos();
			if (selectedConditionInfos.Count < 1)
				return false;


			// First move all selected conditions that are not at level zero up one level
			foreach (ConditionInfo conditionInfo in selectedConditionInfos)
			{
				// Only process conditions that are at level 1 or greater.
				if (conditionInfo.Group.Level < 1)
					continue;

				// Remove from current group and add to current group's parent group.
				ConditionInfoGroup conditionInfoGroup = conditionInfo.Group;
				conditionInfoGroup.ChildConditions.Remove(conditionInfo);
				conditionInfoGroup.Group.ChildConditions.Insert(conditionInfoGroup.Group.ChildConditions.IndexOf(conditionInfoGroup), conditionInfo);
			}

			// Then cleanup groups that contain empty groups or groups with a single group and no conditions.
			this.ProcessConditionInfoGroupForCleanup(this._rootConditionInfoGroup);


			// Refresh some status and force the conditions grid to update it's unbound fields to reflect the new grouping.
			this.UpdateSelectionDependentFlags();
			this.AddRemoveUnboundFieldsUsedAsGroupingIndicators();
			this.RefreshConditionsGrid();

			return true;
		}

			#endregion //ExecuteUngroupSelectedConditionsCommand

		#endregion //CommandHandlers

		#region Base Class Overrides

            #region LogicalChildren
        /// <summary>
        /// Gets an enumerator for this element's logical child elements.
        /// </summary>
        protected override IEnumerator LogicalChildren
        {
            get
            {
                // AS 3/12/09 TFS15327
                // We need the ComparisonOperatorSelector to be in the logical tree
                // so we can pick up resources for its images that may be used.
                //
                System.Collections.IEnumerator baseEnum = base.LogicalChildren;

                if (baseEnum != null)
                    return new MultiSourceEnumerator(base.LogicalChildren, new SingleItemEnumerator(_comparisonOperatorSelector));

                return new SingleItemEnumerator(_comparisonOperatorSelector);
            }
        } 
            #endregion //LogicalChildren

			// JM 05-8-12 TFS110624 Added.
			#region OnKeyUp

		/// <summary>
		/// Raised when a key is released.
		/// </summary>
		/// <param name="e">Information about the event.</param>
		protected override void OnKeyUp(KeyEventArgs e)
		{
			// JM 05-8-12 TFS110624 - Since we are now allowing the removal of conditions when there is an ActiveRecord
			// in the conditions grid (not just when record(s) are selected), we need to process the Delete key for the 
			// ActiveRecord case since the XamDataGrid will not process the Delete key for an ActiveRecord.
			if (e.Key								== Key.Delete	&& 
				this._conditionsGrid				!= null			&&  
				this._conditionsGrid.ActiveRecord	!= null			&&
				this._conditionsGrid.IsKeyboardFocusWithin)
			{
				this.RemoveSelectedConditions();

				e.Handled = true;
			}

			base.OnKeyUp(e);
		}

			#endregion //OnKeyUp

			#region OnApplyTemplate

		/// <summary>
		/// Called when the template is applied.
		/// </summary>
		public override void OnApplyTemplate()
		{
			// Make sure we have all the Template parts we expect.
			this.VerifyParts();

            
#region Infragistics Source Cleanup (Region)














#endregion // Infragistics Source Cleanup (Region)


            base.OnApplyTemplate();
		}

			#endregion //OnApplyTemplate

		#endregion //Base Class Overrides
		
		#region Properties

			#region Public Properties
	
				#region AddConditionLabel

		/// <summary>
		/// Identifies the <see cref="AddConditionLabel"/> dependency property
		/// </summary>
		public static readonly DependencyProperty AddConditionLabelProperty = DependencyProperty.Register("AddConditionLabel",
			typeof(string), typeof(CustomFilterSelectionControl), new FrameworkPropertyMetadata());

		/// <summary>
		/// Returns the prompt used for the 'Add Condition' button.  Use Infragistics.Windows.DataPresenter.Resources.Customizer.SetCustomizedString("CustomFilterSelectionControl_AddConditionLabelProperty", "Custom text") to change the contents of the string.
		/// </summary>
		/// <seealso cref="AddConditionLabelProperty"/>
		//[Description("Returns the prompt used for the 'Add Condition' button.  Use Infragistics.Windows.DataPresenter.Resources.Customizer.SetCustomizedString("CustomFilterSelectionControl_AddConditionLabelProperty", "Custom text") to change the contents of the string.")]
		//[Category("Appearance")]
		[Bindable(true)]
		public string AddConditionLabel
		{
			get
			{
				return (string)this.GetValue(CustomFilterSelectionControl.AddConditionLabelProperty);
			}
            set
            {
                this.SetValue(CustomFilterSelectionControl.AddConditionLabelProperty, value);
            }
		}

				#endregion //AddConditionLabel

				#region AndGroupLegendDescription

		/// <summary>
		/// Identifies the <see cref="AndGroupLegendDescription"/> dependency property
		/// </summary>
		public static readonly DependencyProperty AndGroupLegendDescriptionProperty = DependencyProperty.Register("AndGroupLegendDescription",
			typeof(string), typeof(CustomFilterSelectionControl), new FrameworkPropertyMetadata());

		/// <summary>
		/// Returns the description used for 'And' groups in the legend.  Use Infragistics.Windows.DataPresenter.Resources.Customizer.SetCustomizedString("CustomFilterSelectionControl_AndGroupLegendDescriptionProperty", "Custom text") to change the contents of the string.
		/// </summary>
		/// <seealso cref="AndGroupLegendDescriptionProperty"/>
		//[Description("Returns the description used for 'And' groups in the legend.  Use Infragistics.Windows.DataPresenter.Resources.Customizer.SetCustomizedString("CustomFilterSelectionControl_AndGroupLegendDescriptionProperty", "Custom text") to change the contents of the string.")]
		//[Category("Appearance")]
		[Bindable(true)]
		public string AndGroupLegendDescription
		{
			get
			{
				return (string)this.GetValue(CustomFilterSelectionControl.AndGroupLegendDescriptionProperty);
			}
            set
            {
                this.SetValue(CustomFilterSelectionControl.AndGroupLegendDescriptionProperty, value);
            }
		}

				#endregion //AndGroupLegendDescription

				#region AndLogicalOperatorBrush

		/// <summary>
		/// Identifies the <see cref="AndLogicalOperatorBrush"/> dependency property
		/// </summary>
		public static readonly DependencyProperty AndLogicalOperatorBrushProperty = DependencyProperty.Register("AndLogicalOperatorBrush",
			typeof(System.Windows.Media.Brush), typeof(CustomFilterSelectionControl), new FrameworkPropertyMetadata(System.Windows.Media.Brushes.Blue));

		/// <summary>
		/// Returns/sets the brush to use when coloring conditions that belong to groups with an 'AND' LogicalOperator.
		/// </summary>
		//[Description("Returns/sets the brush to use when coloring conditions that belong to groups with an 'AND' LogicalOperator.")]
		//[Category("Appearance")]
		[Bindable(true)]
		public System.Windows.Media.Brush AndLogicalOperatorBrush
		{
			get
			{
				return (System.Windows.Media.Brush)this.GetValue(CustomFilterSelectionControl.AndLogicalOperatorBrushProperty);
			}
			set
			{
				this.SetValue(CustomFilterSelectionControl.AndLogicalOperatorBrushProperty, value);
			}
		}

				#endregion //AndLogicalOperatorBrush

				#region CancelButtonLabel

		/// <summary>
		/// Identifies the <see cref="CancelButtonLabel"/> dependency property
		/// </summary>
		public static readonly DependencyProperty CancelButtonLabelProperty = DependencyProperty.Register("CancelButtonLabel",
			typeof(string), typeof(CustomFilterSelectionControl), new FrameworkPropertyMetadata());

		/// <summary>
		/// Returns the prompt used for the Cancel button.  Use Infragistics.Windows.DataPresenter.Resources.Customizer.SetCustomizedString("CustomFilterSelectionControl_CancelButtonLabelProperty", "Custom text") to change the contents of the string.
		/// </summary>
		/// <seealso cref="CancelButtonLabelProperty"/>
		//[Description("Returns the prompt used for the Cancel button.  Use Infragistics.Windows.DataPresenter.Resources.Customizer.SetCustomizedString("CustomFilterSelectionControl_CancelButtonLabelProperty", "Custom text") to change the contents of the string.")]
		//[Category("Appearance")]
		[Bindable(true)]
		public string CancelButtonLabel
		{
			get
			{
				return (string)this.GetValue(CustomFilterSelectionControl.CancelButtonLabelProperty);
			}
            set
            {
                this.SetValue(CustomFilterSelectionControl.CancelButtonLabelProperty, value);
            }
		}

				#endregion //CancelButtonLabel

				#region CancelMessageText

		/// <summary>
		/// Identifies the <see cref="CancelMessageText"/> dependency property
		/// </summary>
		public static readonly DependencyProperty CancelMessageTextProperty = DependencyProperty.Register("CancelMessageText",
			typeof(string), typeof(CustomFilterSelectionControl), new FrameworkPropertyMetadata());

		/// <summary>
		/// Returns the text of the message displayed when the user clicks the Cancel button after making changes to the Filters.  Use Infragistics.Windows.DataPresenter.Resources.Customizer.SetCustomizedString("CustomFilterSelectionControl_CancelMessageTextProperty", "Custom text") to change the contents of the string.
		/// </summary>
		/// <seealso cref="CancelMessageTextProperty"/>
		//[Description("Returns the text of the message displayed when the user clicks the Cancel button after making changes to the Filters.  Use Infragistics.Windows.DataPresenter.Resources.Customizer.SetCustomizedString("CustomFilterSelectionControl_CancelMessageTextProperty", "Custom text") to change the contents of the string.")]
		//[Category("Appearance")]
		[Bindable(true)]
		public string CancelMessageText
		{
			get
			{
				return (string)this.GetValue(CustomFilterSelectionControl.CancelMessageTextProperty);
			}
            set
            {
                this.SetValue(CustomFilterSelectionControl.CancelMessageTextProperty, value);
            }
		}

				#endregion //CancelMessageText

				#region CancelMessageTitle

		/// <summary>
		/// Identifies the <see cref="CancelMessageTitle"/> dependency property
		/// </summary>
		public static readonly DependencyProperty CancelMessageTitleProperty = DependencyProperty.Register("CancelMessageTitle",
			typeof(string), typeof(CustomFilterSelectionControl), new FrameworkPropertyMetadata());

		/// <summary>
		/// Returns the Title of the MessageBox displayed when the user clicks the Cancel button after making changes to the Filters.  Use Infragistics.Windows.DataPresenter.Resources.Customizer.SetCustomizedString("CustomFilterSelectionControl_CancelMessageTitleProperty", "Custom text") to change the contents of the string.
		/// </summary>
		/// <seealso cref="CancelMessageTitleProperty"/>
		//[Description("Returns the Title of the MessageBox displayed when the user clicks the Cancel button after making changes to the Filters.  Use Infragistics.Windows.DataPresenter.Resources.Customizer.SetCustomizedString("CustomFilterSelectionControl_CancelMessageTitleProperty", "Custom text") to change the contents of the string.")]
		//[Category("Appearance")]
		[Bindable(true)]
		public string CancelMessageTitle
		{
			get
			{
				return (string)this.GetValue(CustomFilterSelectionControl.CancelMessageTitleProperty);
			}
            set
            {
                this.SetValue(CustomFilterSelectionControl.CancelMessageTitleProperty, value);
            }
		}

				#endregion //CancelMessageTitle

				#region FieldDescription

		/// <summary>
		/// Identifies the <see cref="FieldDescription"/> dependency property
		/// </summary>
		public static readonly DependencyProperty FieldDescriptionProperty = DependencyProperty.Register("FieldDescription",
			typeof(string), typeof(CustomFilterSelectionControl), new FrameworkPropertyMetadata());

		/// <summary>
		/// Returns the description of the Field whose filters are being customized by the control.  Use Infragistics.Windows.DataPresenter.Resources.Customizer.SetCustomizedString("CustomFilterSelectionControl_FieldDescriptionProperty", "Custom text") to change the contents of the string.
		/// </summary>
		/// <seealso cref="FieldDescriptionProperty"/>
		//[Description("Returns the description of the Field whose filters are being customized by the control.  Use Infragistics.Windows.DataPresenter.Resources.Customizer.SetCustomizedString("CustomFilterSelectionControl_FieldDescriptionProperty", "Custom text") to change the contents of the string.")]
		//[Category("Appearance")]
		[Bindable(true)]
		public string FieldDescription
		{
			get
			{
				return (string)this.GetValue(CustomFilterSelectionControl.FieldDescriptionProperty);
			}
            set
            {
                this.SetValue(CustomFilterSelectionControl.FieldDescriptionProperty, value);
            }
		}

				#endregion //FieldDescription

				#region FilterSummaryDescription

		private static readonly DependencyPropertyKey FilterSummaryDescriptionPropertyKey =
			DependencyProperty.RegisterReadOnly("FilterSummaryDescription",
			typeof(string), typeof(CustomFilterSelectionControl), new FrameworkPropertyMetadata());

		/// <summary>
		/// Identifies the <see cref="FilterSummaryDescription"/> dependency property
		/// </summary>
		public static readonly DependencyProperty FilterSummaryDescriptionProperty =
			FilterSummaryDescriptionPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns a string that describes the current set of filter conditions in human readable form.
		/// </summary>
		/// <seealso cref="FilterSummaryDescriptionProperty"/>
		//[Description("Returns a string that describes the current set of filter conditions in human readable form.")]
		//[Category("Behavior")]
		[Bindable(true)]
		public string FilterSummaryDescription
		{
			get
			{
				return (string)this.GetValue(CustomFilterSelectionControl.FilterSummaryDescriptionProperty);
			}
		}

				#endregion //FilterSummaryDescription

				#region GroupSelectedLabel

		/// <summary>
		/// Identifies the <see cref="GroupSelectedLabel"/> dependency property
		/// </summary>
		public static readonly DependencyProperty GroupSelectedLabelProperty = DependencyProperty.Register("GroupSelectedLabel",
			typeof(string), typeof(CustomFilterSelectionControl), new FrameworkPropertyMetadata());

		/// <summary>
		/// Returns the prompt used for the 'Group Selected' label.  Use Infragistics.Windows.DataPresenter.Resources.Customizer.SetCustomizedString("CustomFilterSelectionControl_GroupSelectedLabelProperty", "Custom text") to change the contents of the string.
		/// </summary>
		/// <seealso cref="GroupSelectedLabelProperty"/>
		//[Description("Returns the prompt used for the 'Group Selected' label.  Use Infragistics.Windows.DataPresenter.Resources.Customizer.SetCustomizedString("CustomFilterSelectionControl_GroupSelectedLabelProperty", "Custom text") to change the contents of the string.")]
		//[Category("Appearance")]
		[Bindable(true)]
		public string GroupSelectedLabel
		{
			get
			{
				return (string)this.GetValue(CustomFilterSelectionControl.GroupSelectedLabelProperty);
			}
            set
            {
                this.SetValue(CustomFilterSelectionControl.GroupSelectedLabelProperty, value);
            }
		}

				#endregion //GroupSelectedLabel

				#region GroupSelectedConditionsAsAndGroupLabel

		/// <summary>
		/// Identifies the <see cref="GroupSelectedConditionsAsAndGroupLabel"/> dependency property
		/// </summary>
		public static readonly DependencyProperty GroupSelectedConditionsAsAndGroupLabelProperty = DependencyProperty.Register("GroupSelectedConditionsAsAndGroupLabel",
			typeof(string), typeof(CustomFilterSelectionControl), new FrameworkPropertyMetadata());

		/// <summary>
		/// Returns the prompt used for the 'Group Selected Conditions As And Group' button.  Use Infragistics.Windows.DataPresenter.Resources.Customizer.SetCustomizedString("CustomFilterSelectionControl_GroupSelectedConditionsAsAndGroupLabelProperty", "Custom text") to change the contents of the string.
		/// </summary>
		/// <seealso cref="GroupSelectedConditionsAsAndGroupLabelProperty"/>
		//[Description("Returns the prompt used for the 'Group Selected Conditions as As And Group' button.  Use Infragistics.Windows.DataPresenter.Resources.Customizer.SetCustomizedString("CustomFilterSelectionControl_GroupSelectedConditionsAsAndGroupLabelProperty", "Custom text") to change the contents of the string.")]
		//[Category("Appearance")]
		[Bindable(true)]
		public string GroupSelectedConditionsAsAndGroupLabel
		{
			get
			{
				return (string)this.GetValue(CustomFilterSelectionControl.GroupSelectedConditionsAsAndGroupLabelProperty);
			}
            set
            {
                this.SetValue(CustomFilterSelectionControl.GroupSelectedConditionsAsAndGroupLabelProperty, value);
            }
		}

				#endregion //GroupSelectedConditionsAsAndGroupLabel

				#region GroupSelectedConditionsAsOrGroupLabel

		/// <summary>
		/// Identifies the <see cref="GroupSelectedConditionsAsOrGroupLabel"/> dependency property
		/// </summary>
		public static readonly DependencyProperty GroupSelectedConditionsAsOrGroupLabelProperty = DependencyProperty.Register("GroupSelectedConditionsAsOrGroupLabel",
			typeof(string), typeof(CustomFilterSelectionControl), new FrameworkPropertyMetadata());

		/// <summary>
		/// Returns the prompt used for the 'Group Selected Conditions As Or Group' button.  Use Infragistics.Windows.DataPresenter.Resources.Customizer.SetCustomizedString("CustomFilterSelectionControl_GroupSelectedConditionsAsOrGroupLabelProperty", "Custom text") to change the contents of the string.
		/// </summary>
		/// <seealso cref="GroupSelectedConditionsAsOrGroupLabelProperty"/>
		//[Description("Returns the prompt used for the 'Group Selected Conditions as As Or Group' button.  Use Infragistics.Windows.DataPresenter.Resources.Customizer.SetCustomizedString("CustomFilterSelectionControl_GroupSelectedConditionsAsOrGroupLabelProperty", "Custom text") to change the contents of the string.")]
		//[Category("Appearance")]
		[Bindable(true)]
		public string GroupSelectedConditionsAsOrGroupLabel
		{
			get
			{
				return (string)this.GetValue(CustomFilterSelectionControl.GroupSelectedConditionsAsOrGroupLabelProperty);
			}
            set
            {
                this.SetValue(CustomFilterSelectionControl.GroupSelectedConditionsAsOrGroupLabelProperty, value);
            }
		}

				#endregion //GroupSelectedConditionsAsOrGroupLabel

				// JM 04-06-12 Added for Metro Theme changes
				#region LogicalOperatorColumnWidth

		/// <summary>
		/// Identifies the <see cref="LogicalOperatorColumnWidth"/> dependency property
		/// </summary>
		public static readonly DependencyProperty LogicalOperatorColumnWidthProperty = DependencyProperty.Register("LogicalOperatorColumnWidth",
			typeof(double), typeof(CustomFilterSelectionControl), new FrameworkPropertyMetadata(12d));

		/// <summary>
		/// Returns/sets the width of the column that displays colored bars which represents a logical operator type (i.e., 'And' 'Or)
		/// </summary>
		/// <seealso cref="LogicalOperatorColumnWidthProperty"/>
		[Bindable(true)]
		public double LogicalOperatorColumnWidth
		{
			get
			{
				return (double)this.GetValue(CustomFilterSelectionControl.LogicalOperatorColumnWidthProperty);
			}
            set
            {
                this.SetValue(CustomFilterSelectionControl.LogicalOperatorColumnWidthProperty, value);
            }
		}

				#endregion //LogicalOperatorColumnWidth

				#region OkButtonLabel

		/// <summary>
		/// Identifies the <see cref="OkButtonLabel"/> dependency property
		/// </summary>
		public static readonly DependencyProperty OkButtonLabelProperty = DependencyProperty.Register("OkButtonLabel",
			typeof(string), typeof(CustomFilterSelectionControl), new FrameworkPropertyMetadata());

		/// <summary>
		/// Returns the prompt used for the OK button.  Use Infragistics.Windows.DataPresenter.Resources.Customizer.SetCustomizedString("CustomFilterSelectionControl_OkButtonLabelProperty", "Custom text") to change the contents of the string.
		/// </summary>
		/// <seealso cref="OkButtonLabelProperty"/>
		//[Description("Returns the prompt used for the OK button.  Use Infragistics.Windows.DataPresenter.Resources.Customizer.SetCustomizedString("CustomFilterSelectionControl_OkButtonLabelProperty", "Custom text") to change the contents of the string.")]
		//[Category("Appearance")]
		[Bindable(true)]
		public string OkButtonLabel
		{
			get
			{
				return (string)this.GetValue(CustomFilterSelectionControl.OkButtonLabelProperty);
			}
            set
            {
                this.SetValue(CustomFilterSelectionControl.OkButtonLabelProperty, value);
            }
		}

				#endregion //OkButtonLabel

				#region OrGroupLegendDescription

		/// <summary>
		/// Identifies the <see cref="OrGroupLegendDescription"/> dependency property
		/// </summary>
		public static readonly DependencyProperty OrGroupLegendDescriptionProperty = DependencyProperty.Register("OrGroupLegendDescription",
			typeof(string), typeof(CustomFilterSelectionControl), new FrameworkPropertyMetadata());

		/// <summary>
		/// Returns the description used for 'Or' groups in the legend.  Use Infragistics.Windows.DataPresenter.Resources.Customizer.SetCustomizedString("CustomFilterSelectionControl_OrGroupLegendDescriptionProperty", "Custom text") to change the contents of the string.
		/// </summary>
		/// <seealso cref="OrGroupLegendDescriptionProperty"/>
		//[Description("Returns the description used for 'Or' groups in the legend.  Use Infragistics.Windows.DataPresenter.Resources.Customizer.SetCustomizedString("CustomFilterSelectionControl_OrGroupLegendDescriptionProperty", "Custom text") to change the contents of the string.")]
		//[Category("Appearance")]
		[Bindable(true)]
		public string OrGroupLegendDescription
		{
			get
			{
				return (string)this.GetValue(CustomFilterSelectionControl.OrGroupLegendDescriptionProperty);
			}
            set
            {
                this.SetValue(CustomFilterSelectionControl.OrGroupLegendDescriptionProperty, value);
            }
		}

				#endregion //OrGroupLegendDescription

				#region OrLogicalOperatorBrush

		/// <summary>
		/// Identifies the <see cref="OrLogicalOperatorBrush"/> dependency property
		/// </summary>
		public static readonly DependencyProperty OrLogicalOperatorBrushProperty = DependencyProperty.Register("OrLogicalOperatorBrush",
			typeof(System.Windows.Media.Brush), typeof(CustomFilterSelectionControl), new FrameworkPropertyMetadata(System.Windows.Media.Brushes.Red));

		/// <summary>
		/// Returns/sets the brush to use when coloring conditions that belong to groups with an 'OR' LogicalOperator.
		/// </summary>
		//[Description("Returns/sets the brush to use when coloring conditions that belong to groups with an 'OR' LogicalOperator.")]
		//[Category("Appearance")]
		[Bindable(true)]
		public System.Windows.Media.Brush OrLogicalOperatorBrush
		{
			get
			{
				return (System.Windows.Media.Brush)this.GetValue(CustomFilterSelectionControl.OrLogicalOperatorBrushProperty);
			}
			set
			{
				this.SetValue(CustomFilterSelectionControl.OrLogicalOperatorBrushProperty, value);
			}
		}

				#endregion //OrLogicalOperatorBrush

				#region RecordFilter

		private static readonly DependencyPropertyKey RecordFilterPropertyKey =
			DependencyProperty.RegisterReadOnly("RecordFilter",
			typeof(RecordFilter), typeof(CustomFilterSelectionControl), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Identifies the <see cref="RecordFilter"/> dependency property
		/// </summary>
		public static readonly DependencyProperty RecordFilterProperty =
			RecordFilterPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the <see cref="RecordFilter"/> being manipulated by the CustomFilterSelectionControl (read only).
		/// </summary>
		/// <seealso cref="RecordFilterProperty"/>
		[Bindable(true)]
		[ReadOnly(true)]
		public RecordFilter RecordFilter
		{
			get
			{
				return (RecordFilter)this.GetValue(CustomFilterSelectionControl.RecordFilterProperty);
			}
		}

				#endregion //RecordFilter

				#region RecordManager

		private static readonly DependencyPropertyKey RecordManagerPropertyKey =
			DependencyProperty.RegisterReadOnly("RecordManager",
			typeof(RecordManager), typeof(CustomFilterSelectionControl), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Identifies the <see cref="RecordManager"/> dependency property
		/// </summary>
		public static readonly DependencyProperty RecordManagerProperty =
			RecordManagerPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the <see cref="RecordManager"/> associated with the records being filtered (read only).
		/// </summary>
		/// <seealso cref="RecordManagerProperty"/>
		[Bindable(true)]
		[ReadOnly(true)]
		public RecordManager RecordManager
		{
			get
			{
				return (RecordManager)this.GetValue(CustomFilterSelectionControl.RecordManagerProperty);
			}
		}

				#endregion //RecordManager

				#region RemoveSelectedConditionsLabel

		/// <summary>
		/// Identifies the <see cref="RemoveSelectedConditionsLabel"/> dependency property
		/// </summary>
		public static readonly DependencyProperty RemoveSelectedConditionsLabelProperty = DependencyProperty.Register("RemoveSelectedConditionsLabel",
			typeof(string), typeof(CustomFilterSelectionControl), new FrameworkPropertyMetadata());

		/// <summary>
		/// Returns the prompt used for the 'Remove Selected Conditions' button.  Use Infragistics.Windows.DataPresenter.Resources.Customizer.SetCustomizedString("CustomFilterSelectionControl_RemoveSelectedConditionsLabelProperty", "Custom text") to change the contents of the string.
		/// </summary>
		/// <seealso cref="RemoveSelectedConditionsLabelProperty"/>
		//[Description("Returns the prompt used for the 'Remove Selected Conditions' button.  Use Infragistics.Windows.DataPresenter.Resources.Customizer.SetCustomizedString("CustomFilterSelectionControl_RemoveSelectedConditionsLabelProperty", "Custom text") to change the contents of the string.")]
		//[Category("Appearance")]
		[Bindable(true)]
		public string RemoveSelectedConditionsLabel
		{
			get
			{
				return (string)this.GetValue(CustomFilterSelectionControl.RemoveSelectedConditionsLabelProperty);
			}
            set
            {
                this.SetValue(CustomFilterSelectionControl.RemoveSelectedConditionsLabelProperty, value);
            }
		}

				#endregion //RemoveSelectedConditionsLabel

				#region TitleDescription

		/// <summary>
		/// Identifies the <see cref="TitleDescription"/> dependency property
		/// </summary>
		public static readonly DependencyProperty TitleDescriptionProperty = DependencyProperty.Register("TitleDescription",
			typeof(string), typeof(CustomFilterSelectionControl), new FrameworkPropertyMetadata());

		/// <summary>
		/// Returns the description used for the Title of the container hosting the control.  Use Infragistics.Windows.DataPresenter.Resources.Customizer.SetCustomizedString("CustomFilterSelectionControl_TitleDescriptionProperty", "Custom text") to change the contents of the string.
		/// </summary>
		/// <seealso cref="TitleDescriptionProperty"/>
		//[Description("Returns the description used for the Title of the container hosting the control.  Use Infragistics.Windows.DataPresenter.Resources.Customizer.SetCustomizedString("CustomFilterSelectionControl_TitleDescriptionProperty", "Custom text") to change the contents of the string.")]
		//[Category("Appearance")]
		[Bindable(true)]
		public string TitleDescription
		{
			get
			{
				return (string)this.GetValue(CustomFilterSelectionControl.TitleDescriptionProperty);
			}
            set
            {
                this.SetValue(CustomFilterSelectionControl.TitleDescriptionProperty, value);
            }
		}

				#endregion //TitleDescription

				#region ToggleOperatorOfSelectedConditionsLabel

		/// <summary>
		/// Identifies the <see cref="ToggleOperatorOfSelectedConditionsLabel"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ToggleOperatorOfSelectedConditionsLabelProperty = DependencyProperty.Register("ToggleOperatorOfSelectedConditionsLabel",
			typeof(string), typeof(CustomFilterSelectionControl), new FrameworkPropertyMetadata());

		/// <summary>
		/// Returns the prompt used for the 'Toggle Operator Of Selected Conditions' button.  Use Infragistics.Windows.DataPresenter.Resources.Customizer.SetCustomizedString("CustomFilterSelectionControl_ToggleOperatorOfSelectedConditionsLabelProperty", "Custom text") to change the contents of the string.
		/// </summary>
		/// <seealso cref="ToggleOperatorOfSelectedConditionsLabelProperty"/>
		//[Description("Returns the prompt used for the 'Toggle Operator Of Selected Conditions' button.  Use Infragistics.Windows.DataPresenter.Resources.Customizer.SetCustomizedString("CustomFilterSelectionControl_ToggleOperatorOfSelectedConditionsLabelProperty", "Custom text") to change the contents of the string.")]
		//[Category("Appearance")]
		[Bindable(true)]
		public string ToggleOperatorOfSelectedConditionsLabel
		{
			get
			{
				return (string)this.GetValue(CustomFilterSelectionControl.ToggleOperatorOfSelectedConditionsLabelProperty);
			}
            set
            {
                this.SetValue(CustomFilterSelectionControl.ToggleOperatorOfSelectedConditionsLabelProperty, value);
            }
		}

				#endregion //ToggleOperatorOfSelectedConditionsLabel

				#region UngroupSelectedConditionsLabel

		/// <summary>
		/// Identifies the <see cref="UngroupSelectedConditionsLabel"/> dependency property
		/// </summary>
		public static readonly DependencyProperty UngroupSelectedConditionsLabelProperty = DependencyProperty.Register("UngroupSelectedConditionsLabel",
			typeof(string), typeof(CustomFilterSelectionControl), new FrameworkPropertyMetadata());

		/// <summary>
		/// Returns the prompt used for the 'Ungroup Selected Conditions' button.  Use Infragistics.Windows.DataPresenter.Resources.Customizer.SetCustomizedString("CustomFilterSelectionControl_UngroupSelectedConditionsLabelProperty", "Custom text") to change the contents of the string.
		/// </summary>
		/// <seealso cref="UngroupSelectedConditionsLabelProperty"/>
		//[Description("Returns the prompt used for the 'Ungroup Selected Conditions' button.  Use Infragistics.Windows.DataPresenter.Resources.Customizer.SetCustomizedString("CustomFilterSelectionControl_UngroupSelectedConditionsLabelProperty", "Custom text") to change the contents of the string.")]
		//[Category("Appearance")]
		[Bindable(true)]
		public string UngroupSelectedConditionsLabel
		{
			get
			{
				return (string)this.GetValue(CustomFilterSelectionControl.UngroupSelectedConditionsLabelProperty);
			}
            set
            {
                this.SetValue(CustomFilterSelectionControl.UngroupSelectedConditionsLabelProperty, value);
            }
		}

				#endregion //UngroupSelectedConditionsLabel

			#endregion //Public Properties

            #region Internal Properties

				// AS 1/14/11 TFS63183
                #region Control

        internal static readonly DependencyProperty ControlProperty =
            DependencyProperty.RegisterAttached("Control", typeof(CustomFilterSelectionControl), typeof(CustomFilterSelectionControl),
                new FrameworkPropertyMetadata((CustomFilterSelectionControl)null,
                    FrameworkPropertyMetadataOptions.Inherits,
                    new PropertyChangedCallback(OnControlChanged)));

        internal static CustomFilterSelectionControl GetControl(DependencyObject d)
        {
            return (CustomFilterSelectionControl)d.GetValue(ControlProperty);
        }

        internal static void SetControl(DependencyObject d, CustomFilterSelectionControl value)
        {
            d.SetValue(ControlProperty, value);
        }

        private static void OnControlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CustomFilterSelectionControl ctrl = e.NewValue as CustomFilterSelectionControl;
            XamComboEditor cbo = d as XamComboEditor;

            if (ctrl != null && cbo != null)
            {
                ctrl.InitializeComboEditor(cbo);
            }
        }

                #endregion // Control

                // AS 3/12/09 TFS15327
                #region IsDirty
        internal bool IsDirty
        {
            get { return this._isDirty; }
        }
                #endregion //IsDirty 

            #endregion //Internal Properties

			#region Private Properties

				#region ConditionsContainErrors

		private bool ConditionsContainErrors
		{
			get
			{
				if (this._conditionInfos == null)
					return false;

				foreach (ConditionInfo conditionInfo in this._conditionInfos)
				{
					if (false == string.IsNullOrEmpty(conditionInfo.ErrorDescription))
						return true;
				}

				return false;
			}
		}

				#endregion //ConditionsContainErrors

                // AS 3/12/09 TFS15327
                #region ComparisonOperatorSelector
        private ComparisonOperatorSelector ComparisonOperatorSelector
        {
            get
            {
                if (null == _comparisonOperatorSelector)
                {
                    _comparisonOperatorSelector = new ComparisonOperatorSelector();
                    this.AddLogicalChild(_comparisonOperatorSelector);

                    // get the allowable operators from the associated field
                    _comparisonOperatorSelector.SetBinding(ComparisonOperatorSelector.AllowableOperatorsProperty, Utilities.CreateBindingObject("RecordFilter.Field.FilterOperatorDropDownItemsResolved", BindingMode.OneWay, this));
                }

                return _comparisonOperatorSelector;
            }
        } 
                #endregion //ComparisonOperatorSelector

				#region IsConditionInEditMode

		private bool IsConditionInEditMode
		{
			get
			{
				return this._conditionsGrid				!= null &&
					   this._conditionsGrid.ActiveCell	!= null &&
					   this._conditionsGrid.ActiveCell.IsInEditMode;
			}
		}

				#endregion //IsConditionInEditMode

				#region Operands

		private ObservableCollectionExtended<FilterDropDownItem> Operands
		{
			get
			{
				if (this._operands == null)
				{
					this._operands = new ObservableCollectionExtended<FilterDropDownItem>();

					if (this.RecordFilter != null && this.RecordManager != null)
					{
						bool includeFilteredOutValues = (Keyboard.Modifiers & ModifierKeys.Shift) != 0 ||
							this.RecordFilter.Field.Owner.RecordFiltersLogicalOperatorResolved == LogicalOperator.Or;


						// Call GetFilterDropDownItems off FieldFilterInfo (note that this will raise the RecordFilterDropDownPopulating event)
						ResolvedRecordFilterCollection.FieldFilterInfo	fieldFilterInfo			= new ResolvedRecordFilterCollection.FieldFilterInfo(this.RecordManager.RecordFiltersResolved, this.RecordFilter.Field);
						//// JM 01/22/09 - TFS12622 - pass true to ensure that the 'Custom' option is not included in the operand list.
						//List<FilterDropDownItem>						filterDropDownItemList	= fieldFilterInfo.GetFilterDropDownItems(true, includeFilteredOutValues);


						//// Update the operands list we expose
						//this._operands.BeginUpdate();
						//this._operands.AddRange(filterDropDownItemList);
						//this._operands.EndUpdate();
						if (this._loader != null)
							this._loader.Abort();

						Cell activeCell = this._conditionsGrid != null ? this._conditionsGrid.ActiveCell : null;
						CellValuePresenter cvp = activeCell != null ? CellValuePresenter.FromCell(activeCell) : null;
						XamComboEditor editor = cvp != null ? Utilities.GetDescendantFromType<XamComboEditor>(cvp, true, null) : null;

						this._loader = new ResolvedRecordFilterCollection.FilterDropDownItemLoader(editor, fieldFilterInfo, this._operands, true);
						this._loader.PopulatePhase1();
						this._loader.PopulatePhase2(includeFilteredOutValues);

						// JJD 06/29/10 - TFS32174 
						// If the loader isn't done wire the Phase2Completed event
						if (this._loader.EndReached)
							this._loader = null;
						else
							this._loader.Phase2Completed += new EventHandler(OnDropdownLoadCompleted);
					}
				}

				return this._operands;
			}
		}

				#endregion //Operands

				#region UnboundFields

		private List<Field> UnboundFields
		{
			get
			{
				if (this._unboundFields == null)
					this._unboundFields = new List<Field>(MAX_GROUPLEVELS);

				return this._unboundFields;
			}
		}

				#endregion //UnboundFields	
    
			#endregion //Private Properties

		#endregion //Properties

		#region Methods

			#region Public Methods

				#region Initialize

		/// <summary>
		/// Initializes an instance of the CustomFilterSelectionControl.  This method should be called when the parameterless constructor is used to create the control instance.
		/// </summary>
		/// <param name="recordFilter">The <see cref="RecordFilter"/> containing the <see cref="Infragistics.Windows.Controls.ICondition"/>s to be customized for a specific <see cref="Field"/></param>
		/// <param name="recordManager">The <see cref="RecordManager"/> associated with the records being filtered.</param>
		/// <seealso cref="RecordFilter"/>
		/// <seealso cref="RecordManager"/>
		/// <seealso cref="Infragistics.Windows.Controls.ICondition"/>
		public void Initialize(RecordFilter recordFilter, RecordManager recordManager)
		{
			if (this._initializeMethodCalled)
				return;

			if (recordFilter == null)
				throw new ArgumentNullException(DataPresenterBase.GetString("LE_ArgumentException_37"));

			if (recordManager == null)
				throw new ArgumentNullException(DataPresenterBase.GetString("LE_ArgumentException_38"));


			// Save the RecordFilter and ReordManager
			this.SetValue(CustomFilterSelectionControl.RecordFilterPropertyKey, recordFilter);
			this.SetValue(CustomFilterSelectionControl.RecordManagerPropertyKey, recordManager);

			this.Loaded += new RoutedEventHandler(OnLoaded);


			// Initialize UI prompt strings
			this.SetBinding(CustomFilterSelectionControl.AddConditionLabelProperty, Utilities.CreateBindingObject("Value", BindingMode.OneWay, Infragistics.Windows.DataPresenter.Resources.GetDynamicResourceString("CustomFilterSelectionControl_AddConditionLabelProperty")));
			this.SetBinding(CustomFilterSelectionControl.RemoveSelectedConditionsLabelProperty, Utilities.CreateBindingObject("Value", BindingMode.OneWay, Infragistics.Windows.DataPresenter.Resources.GetDynamicResourceString("CustomFilterSelectionControl_RemoveSelectedConditionsLabelProperty")));
			this.SetBinding(CustomFilterSelectionControl.GroupSelectedConditionsAsAndGroupLabelProperty, Utilities.CreateBindingObject("Value", BindingMode.OneWay, Infragistics.Windows.DataPresenter.Resources.GetDynamicResourceString("CustomFilterSelectionControl_GroupSelectedConditionsAsAndGroupLabelProperty")));
			this.SetBinding(CustomFilterSelectionControl.GroupSelectedConditionsAsOrGroupLabelProperty, Utilities.CreateBindingObject("Value", BindingMode.OneWay, Infragistics.Windows.DataPresenter.Resources.GetDynamicResourceString("CustomFilterSelectionControl_GroupSelectedConditionsAsOrGroupLabelProperty")));
			this.SetBinding(CustomFilterSelectionControl.UngroupSelectedConditionsLabelProperty, Utilities.CreateBindingObject("Value", BindingMode.OneWay, Infragistics.Windows.DataPresenter.Resources.GetDynamicResourceString("CustomFilterSelectionControl_UngroupSelectedConditionsLabelProperty")));
			this.SetBinding(CustomFilterSelectionControl.ToggleOperatorOfSelectedConditionsLabelProperty, Utilities.CreateBindingObject("Value", BindingMode.OneWay, Infragistics.Windows.DataPresenter.Resources.GetDynamicResourceString("CustomFilterSelectionControl_ToggleOperatorOfSelectedConditionsLabelProperty")));
			this.SetBinding(CustomFilterSelectionControl.AndGroupLegendDescriptionProperty, Utilities.CreateBindingObject("Value", BindingMode.OneWay, Infragistics.Windows.DataPresenter.Resources.GetDynamicResourceString("CustomFilterSelectionControl_AndGroupLegendDescriptionProperty")));
			this.SetBinding(CustomFilterSelectionControl.OrGroupLegendDescriptionProperty, Utilities.CreateBindingObject("Value", BindingMode.OneWay, Infragistics.Windows.DataPresenter.Resources.GetDynamicResourceString("CustomFilterSelectionControl_OrGroupLegendDescriptionProperty")));
			this.SetBinding(CustomFilterSelectionControl.TitleDescriptionProperty, Utilities.CreateBindingObject("Value", BindingMode.OneWay, Infragistics.Windows.DataPresenter.Resources.GetDynamicResourceString("CustomFilterSelectionControl_TitleDescriptionProperty")));
			this.SetBinding(CustomFilterSelectionControl.FieldDescriptionProperty, Utilities.CreateBindingObject("Value", BindingMode.OneWay, Infragistics.Windows.DataPresenter.Resources.GetDynamicResourceString("CustomFilterSelectionControl_FieldDescriptionProperty", new object [] { recordFilter.Field.Label })));
			this.SetBinding(CustomFilterSelectionControl.OkButtonLabelProperty, Utilities.CreateBindingObject("Value", BindingMode.OneWay, Infragistics.Windows.DataPresenter.Resources.GetDynamicResourceString("CustomFilterSelectionControl_OkButtonLabelProperty")));
			this.SetBinding(CustomFilterSelectionControl.CancelButtonLabelProperty, Utilities.CreateBindingObject("Value", BindingMode.OneWay, Infragistics.Windows.DataPresenter.Resources.GetDynamicResourceString("CustomFilterSelectionControl_CancelButtonLabelProperty")));
			this.SetBinding(CustomFilterSelectionControl.CancelMessageTextProperty, Utilities.CreateBindingObject("Value", BindingMode.OneWay, Infragistics.Windows.DataPresenter.Resources.GetDynamicResourceString("CustomFilterSelectionControl_CancelMessageTextProperty")));
			this.SetBinding(CustomFilterSelectionControl.CancelMessageTitleProperty, Utilities.CreateBindingObject("Value", BindingMode.OneWay, Infragistics.Windows.DataPresenter.Resources.GetDynamicResourceString("CustomFilterSelectionControl_CancelMessageTitleProperty")));
			this.SetBinding(CustomFilterSelectionControl.GroupSelectedLabelProperty, Utilities.CreateBindingObject("Value", BindingMode.OneWay, Infragistics.Windows.DataPresenter.Resources.GetDynamicResourceString("CustomFilterSelectionControl_GroupSelectedLabelProperty")));

			// JM 07-21-11 TFS79821 - Since we no longer have a ComparisonOperatorSelector in the tree as a 
			// result of the fix for TFS15327, we no longer need these handlers.
			//// AS 9/14/09 TFS22126
			//// We shouldn't be registering a class handler in the instance constructor but that really 
			//// shouldn't root the element either. Somehow this is causing the WPF framework to maintain 
			//// an instance of a SelectedOperatorChangedEventArgs whose _target is this instance. Changing 
			//// this to adding an instance handler.
			////
			////// JM 02-24-09 TFS14444
			////EventManager.RegisterClassHandler(typeof(CustomFilterSelectionControl), ComparisonOperatorSelector.SelectedOperatorChangedEvent, new EventHandler<Infragistics.Windows.Controls.Events.SelectedOperatorChangedEventArgs>(OnComparisonOperatorSelectedOperatorChanged), true);
			//EventHandler<Infragistics.Windows.Controls.Events.SelectedOperatorChangedEventArgs> handler = new EventHandler<Infragistics.Windows.Controls.Events.SelectedOperatorChangedEventArgs>(OnComparisonOperatorSelectedOperatorChanged);
			//this.RemoveHandler(ComparisonOperatorSelector.SelectedOperatorChangedEvent, handler);
			//this.AddHandler(ComparisonOperatorSelector.SelectedOperatorChangedEvent, handler, true);

			this._initializeMethodCalled = true;
		}

				#endregion //Initialize

			#endregion //Public Methods

			#region Private Methods

				#region AddRemoveUnboundFieldsUsedAsGroupingIndicators

		private void AddRemoveUnboundFieldsUsedAsGroupingIndicators()
		{
			int highestGroupLevel = 0;
			this.ProcessConditionInfoGroupForGroupLevel(this._rootConditionInfoGroup, ref highestGroupLevel);


			// Make sure we have 1 UnboundField for each group level (note that 'GroupLevel' is zero-based)
			this._totalGroupLevels = highestGroupLevel + 1;

			if (this._totalGroupLevels > this.UnboundFields.Count)
			{
				// Add UnboundFields
				int originalUnboundFieldCount = this.UnboundFields.Count;
				for (int i = originalUnboundFieldCount; i < this._totalGroupLevels; i++)
				{
					Field field										= new UnboundField();
					field.Name										= string.Format("Field {0}", i.ToString());
					field.Settings.AllowEdit						= false;
					field.Settings.AllowResize						= false;
					field.Settings.CellClickAction					= CellClickAction.SelectRecord;
					field.Settings.CellMinWidth						= this.LogicalOperatorColumnWidth; // 12;  JM 04-06-12 Metro Theme changes
					field.Settings.CellMaxWidth						= this.LogicalOperatorColumnWidth; // 12;  JM 04-06-12 Metro Theme changes
					field.Settings.CellValuePresenterStyleSelector	= new GroupFieldStyleSelector(this);
					field.Tag										= i;
					field.Label										= " ";	// JM 02-17-09 TFS 14001

					this._conditionsGrid.DefaultFieldLayout.Fields.Insert(i, field);
					this.UnboundFields.Insert(i, field);
				}
			}
			else if (this._totalGroupLevels < this.UnboundFields.Count)
			{
				// Remove UnboundFields.
				int originalUnboundFieldCount = this.UnboundFields.Count;
				for (int i = originalUnboundFieldCount; i > this._totalGroupLevels; i--)
				{
					this._conditionsGrid.DefaultFieldLayout.Fields.Remove(this.UnboundFields[i - 1]);
					this.UnboundFields.RemoveAt(i - 1);
				}
			}
		}

				#endregion //AddRemoveUnboundFieldsUsedAsGroupingIndicators	

				#region IsGroupCompletelySelected

		private bool IsGroupCompletelySelected(ConditionInfoGroup group)
		{
			bool allConditionsInGroupAreSelected = true;
			if (group != null && group.ChildConditions != null)
			{
				foreach (ConditionInfo conditionInfo in group.ChildConditions)
				{
					// JM 02-20-09 - If there are any ConditionInfoGroups in the group, then we should never
					//				 consider all conditions to be selected even if all non-groups are selected.
					//if (false == conditionInfo is ConditionInfoGroup && conditionInfo.IsSelected == false)
					if (true == conditionInfo is ConditionInfoGroup || conditionInfo.IsSelected == false)
					{
						allConditionsInGroupAreSelected = false;
						break;
					}
				}
			}


			return allConditionsInGroupAreSelected;
		}

				#endregion //IsGroupCompletelySelected

				// JM 07-21-11 TFS79821 Added.
				#region EditComparisonOperatorValue

		private void EditComparisonOperatorValue(Cell cell)
		{
			DataRecord record = cell.Record;
			if (record == null)
				return;

			Cell operatorCell	= record.Cells["ComparisonOperator"];
			Cell operandCell	= record.Cells["Value"];
			SpecialFilterOperandBase	specialOperand		= operandCell.Value as SpecialFilterOperandBase;
			ComparisonOperator			comparisonOperator	= (ComparisonOperator)operatorCell.Value;

			// If the current Operand is a SpecialOperand make sure that it supports the operator.
			if (specialOperand != null)
			{

				if (!specialOperand.SupportsOperator(comparisonOperator))
				{
					((ConditionInfo)cell.Record.DataItem).SetErrorDescription(DataPresenterBase.GetString("CustomFilterSelectionControl_ConditionError_IncompatibleOperator"));
					return;
				}
			}
			else
			{
				// JJD 02/17/12 - TFS101703 
				// Special case ComparisonCondition with epxlicit display text. For this case we only want to allow 
				// the 'Equals' operator.
				ComparisonCondition cc = operandCell.Value as ComparisonCondition;

				if (cc != null && false == string.IsNullOrEmpty(cc.DisplayText) && comparisonOperator != ComparisonOperator.Equals)
				{
					((ConditionInfo)cell.Record.DataItem).SetErrorDescription(DataPresenterBase.GetString("CustomFilterSelectionControl_ConditionError_IncompatibleOperator"));
					return;		
				}
			}

			// JM 2/24/09 TFS14444
			// We also need to make sure that any operand, not just the special operand,
			// is appropriate for the new operator. For example, for Top/Bottom operators,
			// only numeric operands are valid.
			Exception error;
			object currentUIOperand = operandCell.Value;
			if (null != currentUIOperand &&
				null != operatorCell.Field &&
				null != this.RecordFilter &&
				null != this.RecordFilter.Field &&
				!this.RecordFilter.Field.IsFilterCellEntryValid(currentUIOperand, comparisonOperator, out error))
			{
				((ConditionInfo)cell.Record.DataItem).SetErrorDescription(DataPresenterBase.GetString("CustomFilterSelectionControl_ConditionError_IncompatibleOperator"));
				return;
			}

			((ConditionInfo)cell.Record.DataItem).SetErrorDescription(string.Empty);
		}

				#endregion //EditComparisonOperatorValue

				// JM 02-24-09 TFS 14444 - Added.
				#region EditOperandValue

		private void EditOperandValue(Cell cell)
		{
			if (cell.Field.Name != "Value")
				return;


			// If the Operand value is a SpecialOperand, ensure that it is compatible with the operator.  If not, reset the operator to 
			// a compatible operator.
			SpecialFilterOperandBase specialOperand = cell.Value as SpecialFilterOperandBase;
			if (specialOperand != null)
			{
				Cell				operatorCell				= cell.Record.Cells["ComparisonOperator"];
				ComparisonOperator	currentComparisonOperator	= (ComparisonOperator)operatorCell.Value;

				if (!specialOperand.SupportsOperator(currentComparisonOperator))
				{
					// Get the default operator for the Field
					ComparisonOperator defaultComparisonOperator = this.RecordFilter.Field.FilterOperatorDefaultValueResolved;

					// If the default doesn't work try the equals operator
					if (!specialOperand.SupportsOperator(defaultComparisonOperator))
					{
						if (specialOperand.SupportsOperator(ComparisonOperator.Equals))
							operatorCell.Value = ComparisonOperator.Equals;
						else
							operatorCell.Value = defaultComparisonOperator;
					}
					else
						operatorCell.Value = defaultComparisonOperator;
				}


				// JM 02-24-09 TFS 14444
				Exception error;
				if (null != specialOperand			&&
					null != this.RecordFilter.Field	&&
					!this.RecordFilter.Field.IsFilterCellEntryValid(specialOperand, currentComparisonOperator, out error))
				{
					// Get the default operator for the Field
					ComparisonOperator defaultComparisonOperator = this.RecordFilter.Field.FilterOperatorDefaultValueResolved;

					// If the default doesn't work try the equals operator
					if (!specialOperand.SupportsOperator(defaultComparisonOperator))
					{
						if (specialOperand.SupportsOperator(ComparisonOperator.Equals))
							operatorCell.Value = ComparisonOperator.Equals;
						else
							operatorCell.Value = defaultComparisonOperator;
					}
					else
						operatorCell.Value = defaultComparisonOperator;
				}
			}
			else
			{
				// JJD 02/17/12 - TFS101703 
				// Special case ComparisonCondition with explicit display text. For this case we only want to allow 
				// the 'Equals' operator so set it here
				ComparisonCondition cc = cell.Value as ComparisonCondition;
				if (cc != null && false == string.IsNullOrEmpty(cc.DisplayText))
				{
					Cell operatorCell = cell.Record.Cells["ComparisonOperator"];
					operatorCell.Value = ComparisonOperator.Equals;
				}
			}



			// Make sure the value is valid with respect to the ComparisonOperator, and set/clear the error message as necessary.
			Exception	exception;
			bool		isValid = this.RecordFilter.Field.IsFilterCellEntryValid(cell.Value, (ComparisonOperator)cell.Record.Cells["ComparisonOperator"].Value, out exception);
			if (isValid == false)
			{
				// JM 01-22-09 TFS12620 - 
				//Infragistics.Windows.Utilities.ShowMessageBox(this,
				//                                            SR.GetString("CustomFilterSelectionControl_ConditionErrorMessageTextProperty", new object[] { exception.Message }),
				//                                            SR.GetString("CustomFilterSelectionControl_ConditionErrorMessageTitleProperty"),
				//                                            MessageBoxButton.OK,
				//                                            MessageBoxImage.Error);

				//e.Cancel = true;
				((ConditionInfo)cell.Record.DataItem).SetErrorDescription(exception.Message);
			}
			else if (cell.Value == null || (cell.Value is string && string.IsNullOrEmpty((string)cell.Value)))
			{
				// JM 01-22-09 TFS12620 - 
				//Infragistics.Windows.Utilities.ShowMessageBox(this, SR.GetString("CustomFilterSelectionControl_ConditionErrorMessageTextProperty", new object[] { SR.GetString("CustomFilterSelectionControl_ConditionError_Empty") }),
				//                                                    SR.GetString("CustomFilterSelectionControl_ConditionErrorMessageTitleProperty"),
				//                                                    MessageBoxButton.OK,
				//                                                    MessageBoxImage.Error);

				//e.Cancel = true;
				((ConditionInfo)cell.Record.DataItem).SetErrorDescription(DataPresenterBase.GetString("CustomFilterSelectionControl_ConditionError_Empty"));
			}
			else
				((ConditionInfo)cell.Record.DataItem).SetErrorDescription(string.Empty);
		}

				#endregion //EditOperandValue

				#region GetIndexOfAdjacentRecordAtGroupLevel

		private int GetIndexOfAdjacentRecordAtSpecificGroupLevel(int rangeStartIndex, int rangeEndIndex, int groupLevel)
		{
			// First look for an adjacent record in thespecified group that precedes the specified range.
			if (rangeStartIndex > 0)
			{
				for (int i = rangeStartIndex - 1; i >= 0; i--)
				{
					if (this._conditionInfos[i].Group.Level == groupLevel)
						return i;
				}
			}
			else if (rangeEndIndex < this._conditionInfos.Count - 1)
			{
				for (int i = rangeEndIndex + 1; i < this._conditionInfos.Count; i++)
				{
					if (this._conditionInfos[i].Group.Level == groupLevel)
						return i;
				}
			}

			return -1;
		}

				#endregion //GetIndexOfAdjacentRecordAtGroupLevel	

				#region GetSelectedConditionInfos

		// JM 05-8-12 TFS110624 - Add an overload that takes an option that determines whether to include active
		// records if no records are selected.
		private List<ConditionInfo> GetSelectedConditionInfos()
		{
			return this.GetSelectedConditionInfos(false);
		}

		private List<ConditionInfo> GetSelectedConditionInfos(bool includeActiveRecordIfNoRecordsAreSelected)
		{
			if (this._conditionsGrid == null)
				return new List<ConditionInfo>();


			List<ConditionInfo> selectedConditionInfos = new List<ConditionInfo>(this._conditionsGrid.SelectedItems.Records.Count);
			foreach (ConditionInfo conditionInfo in this._conditionInfos)
			{
				if (conditionInfo.IsSelected)
					selectedConditionInfos.Add(conditionInfo);
			}

			// JM 05-8-12 TFS110624.  If there are no selected ConditionInfos, add the condition info associated
			// with the active record if we have been asked to do so.
			if (selectedConditionInfos.Count				< 1		&& 
				includeActiveRecordIfNoRecordsAreSelected	== true	&& 
				this._conditionsGrid.ActiveRecord			!= null)
			{
				DataRecord activeRecord = this._conditionsGrid.ActiveRecord as DataRecord;
				if (activeRecord != null && activeRecord.DataItem is ConditionInfo)
					selectedConditionInfos.Add(activeRecord.DataItem as ConditionInfo);
			}

			return selectedConditionInfos;
		}

				#endregion //GetSelectedConditionInfos	
    
				#region GetSelectedRecordsInVisibleIndexOrder

		private List<Record> GetSelectedRecordsInVisibleIndexOrder(SelectedRecordCollection selectedRecords)
		{
			List<Record> selectedRecordsSorted = new List<Record>(selectedRecords.ToArray());
			selectedRecordsSorted.Sort(new RecordVisibleIndexComparer());

			return selectedRecordsSorted;
		}

				#endregion //GetSelectedRecordsInVisibleIndexOrder	
    
				#region GroupSelectedConditionsHelper

		private bool GroupSelectedConditionsHelper(LogicalOperator logicalOperator)
		{
			// Check the flag which indicates whether the current selection can be grouped (theoretically we shouldn't have to do this
			// since the 'CanExecute' for this command should prevent us from getting here if the flag is false)
			if (false == this._currentSelectionCanBeGrouped)
				return false;


			// Make sure we have at least 1 selected ConditionInfo.
			List<ConditionInfo> selectedConditionInfos	= this.GetSelectedConditionInfos();
			if (selectedConditionInfos.Count < 1)
				return false;


			// Establish the old ConditionInfoGroup that held the selected ConditionInfos and:
			//	1. create a new ConditionInfoGroup and insert it in the ChildConditions collection of the old ConditionInfoGroup in the same slot as the first selected condition
			//	2. for each selected ConditionInfo, remove it from the old group and add it to the new group
			ConditionInfoGroup	oldConditionInfoGroup			= selectedConditionInfos[0].Group;

			ConditionInfoGroup	newConditionInfoGroup			= new ConditionInfoGroup(oldConditionInfoGroup, logicalOperator);
			int					indexOfFirstSelectedCondition	= oldConditionInfoGroup.ChildConditions.IndexOf(selectedConditionInfos[0]);
			oldConditionInfoGroup.ChildConditions.Insert(indexOfFirstSelectedCondition, newConditionInfoGroup);

			foreach (ConditionInfo conditionInfo in selectedConditionInfos)
			{
				oldConditionInfoGroup.ChildConditions.Remove(conditionInfo);
				newConditionInfoGroup.ChildConditions.Add(conditionInfo);
			}


			// Refresh some status and force the conditions grid to update it's unbound fields to reflect the new grouping.
			this.UpdateSelectionDependentFlags();
			this.AddRemoveUnboundFieldsUsedAsGroupingIndicators();
			this.RefreshConditionsGrid();

			return true;
		}

				#endregion //GroupSelectedConditionsHelper

				// AS - NA 11.2 Excel Style Filtering
				#region GetTargetRecordFilter
		private RecordFilter GetTargetRecordFilter()
		{
			var recordFilter = this.RecordFilter;

			if (recordFilter != null)
			{
				Field field = recordFilter.Field;
				Debug.Assert(recordFilter.Field != null, "Record filter is not associated with a field");

				if (null != field && null != field.Owner)
				{
					var recordManager = this.RecordManager;
					var recordFilterCollection = recordManager.GetRecordFiltersResolved(field.Owner);
					recordFilter = recordFilterCollection[field];
				}
			}

			return recordFilter;
		}
				#endregion //GetTargetRecordFilter

				// AS 1/14/11 TFS63183
				#region InitializeComboEditor
		private void InitializeComboEditor(XamComboEditor editor)
		{
			CellValuePresenter cvp = editor.Host as CellValuePresenter;

			if (cvp == null)
				return;

			if (cvp.Field.Name == "ComparisonOperator")
			{
				if (null != _operatorInitializer)
					_operatorInitializer.Initialize(editor);
			}
			else if (cvp.Field.Name == "Value")
			{
				if (null != _operandInitializer)
					_operandInitializer.Initialize(editor);
			}
		} 
				#endregion //InitializeComboEditor

				// AS - NA 11.2 Excel Style Filtering
				#region InitializeErrorDescriptions
		private void InitializeErrorDescriptions()
		{
			if (_isDirty)
			{
				Field valueField = _conditionsGrid.DefaultFieldLayout.Fields["Value"];
				Field errorField = _conditionsGrid.DefaultFieldLayout.Fields["ErrorDescription"];
				Cell errorCell = null;

				foreach (Record record in _conditionsGrid.Records)
				{
					DataRecord dr = record as DataRecord;

					if (dr != null)
					{
						Cell valueCell = dr.Cells[valueField];

						this.EditOperandValue(valueCell);

						if (null == errorCell && !string.IsNullOrEmpty(dr.Cells[errorField].Value as string))
						{
							errorCell = valueCell;
						}
					}
				}

				if (null != errorCell)
				{
					_conditionsGrid.ActiveCell = errorCell;
					_conditionsGrid.ExecuteCommand(DataPresenterCommands.StartEditMode);
				}
			}
		}
				#endregion //InitializeErrorDescriptions

				// JM 07-21-11 TFS79821 - Since we no longer have a ComparisonOperatorSelector in the tree as a 
				// result of the fix for TFS15327, we no longer need this handler.  However, since this processing
				// still needs to occur we have created a new method called EditComparisonOperatorValue that includes
				// a refactored version of this logic.  EditComparisonOperatorValue is called from OnConditionsGridEditModeEnded.
				// JM 02-24-09 TFS14444 - Added.
				#region OnComparisonOperatorSelectedOperatorChanged

#region Infragistics Source Cleanup (Region)

















































#endregion // Infragistics Source Cleanup (Region)

				#endregion //OnComparisonOperatorSelectedOperatorChanged

				#region OnConditionInfosCollectionChanged

		void OnConditionInfosCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			this._isDirty = true;

			this.RefreshTempRootConditionGroup();
			this.UpdateSelectionDependentFlags();
			this.AddRemoveUnboundFieldsUsedAsGroupingIndicators();
		}

				#endregion //OnConditionInfosCollectionChanged	

				#region OnConditionInfosItemPropertyChanged

		void OnConditionInfosItemPropertyChanged(object sender, ItemPropertyChangedEventArgs e)
		{
			this._isDirty = true;

			this.RefreshTempRootConditionGroup();
			this.AddRemoveUnboundFieldsUsedAsGroupingIndicators();
		}

				#endregion //OnConditionInfosItemPropertyChanged	

				// JJD 07/06/10 - TFS32174 - added
				#region OnConditionsGridCellActivated

		void OnConditionsGridCellActivated(object sender, Infragistics.Windows.DataPresenter.Events.CellActivatedEventArgs e)
		{
			if (e.Cell.Field.Name == "Value")
			{
				if (this._loader != null)
				{
					CellValuePresenter cvp = CellValuePresenter.FromCell(e.Cell);

					// re-initialize the editor on the loader so that the loading msg can 
					// get initialized
					this._loader.Editor = cvp != null ? Utilities.GetDescendantFromType<XamComboEditor>(cvp, true, null) : null;
				}
			}

		}

				#endregion //OnConditionsGridCellActivated	
    
				#region OnConditionsGridEditModeStarted

		void OnConditionsGridEditModeStarted(object sender, Infragistics.Windows.DataPresenter.Events.EditModeStartedEventArgs e)
		{
			// If the Operand is the value that has just been edited, enable/disable AutoComplete based on the value of the operator field.
			if (e.Cell.Field.Name == "Value")
				GridUtilities.SetIsTextSearchEnabled(e.Editor as XamComboEditor, (ComparisonOperator)e.Cell.Record.Cells["ComparisonOperator"].Value);
		}

				#endregion //OnConditionsGridEditModeStarted

				// JM 02-24-09 TFS 14444 - Removed. Code moved to new EditOperandValue method
				#region OnConditionsGridEditModeEnding

#region Infragistics Source Cleanup (Region)
































#endregion // Infragistics Source Cleanup (Region)

				#endregion //OnConditionsGridEditModeEnding	
    
				#region OnConditionsGridEditModeEnded

		void OnConditionsGridEditModeEnded(object sender, Infragistics.Windows.DataPresenter.Events.EditModeEndedEventArgs e)
		{
			// JM 02-24-09 TFS 14444 - Move code to separate method and call method.
			if (e.Cell.Field.Name == "Value")
				this.EditOperandValue(e.Cell);

			#region Old Code - moved to new method EditOperandValue

#region Infragistics Source Cleanup (Region)



















































#endregion // Infragistics Source Cleanup (Region)
			
			#endregion //Old Code - moved to new method EditOperandValue

			// JM 07-21-11 TFS79821
			if (e.Cell.Field.Name == "ComparisonOperator")
				this.EditComparisonOperatorValue(e.Cell);
		}

				#endregion //OnConditionsGridEditModeEnded	
				
				// JJD 06/29/10 - TFS32174 - added
				#region OnDropdownLoadCompleted

		private void OnDropdownLoadCompleted(object sender, EventArgs e)
		{
			Debug.Assert(this._loader == sender, "FilterButton.OnDropdownLoadCompleted");
			if (this._loader == sender)
			{
				this._loader.Phase2Completed -= new EventHandler(OnDropdownLoadCompleted);
				this._loader = null;
			}
		}

				#endregion //OnDropdownLoadCompleted	
				
				// JM 06-02-09 TFS 18008 - Added.
				#region OnConditionsGridRecordsDeleting

		void OnConditionsGridRecordsDeleting(object sender, Infragistics.Windows.DataPresenter.Events.RecordsDeletingEventArgs e)
		{
			// Cancel the event since we will handle the deletion ourselves.
			e.Cancel = true;

			this.RemoveSelectedConditions();
		}

				#endregion //OnConditionsGridRecordsDeleting

				#region OnFieldLayoutInitialized

		void OnFieldLayoutInitialized(object sender, Infragistics.Windows.DataPresenter.Events.FieldLayoutInitializedEventArgs e)
		{
			if (this.RecordFilter == null)
				return;


			Field recordFilterTargetField = this.RecordFilter.Field;

			#region Setup the ComparisonOperator Field

			Field comparisonOperatorField = e.FieldLayout.Fields["ComparisonOperator"];
			Debug.Assert(comparisonOperatorField != null, "ComparisonOperator Field definition not found in FieldLayout!");
			if (comparisonOperatorField != null)
			{
				comparisonOperatorField.Label			= DataPresenterBase.GetString("CustomFilterSelectionControl_OperatorFieldLabel");

                
#region Infragistics Source Cleanup (Region)






























#endregion // Infragistics Source Cleanup (Region)

                comparisonOperatorField.Settings.EditorType = typeof(XamComboEditor);
                comparisonOperatorField.Settings.EditAsType = typeof(ComparisonOperatorListItem);

                // create the style to use for the EditorStyle
				// AS 1/14/11 TFS63183
				// WPF only supports a single local style. Since we needed to set the ItemsProvider to 
				// indicate the items for the xamComboEditor we used the EditorStyle but by doing so we 
				// provided a local style to the editor. This means that it would not pick up any styling 
				// from any implicit styles such as when the Theme property was set. To get around this 
				// we have to get a reference to the xamComboEditor and set the properties we wanted to 
				// set via the Style directly on the element. To accomplish that we're using an attached 
				// inherited property on CustomFilterSelectionControl. When that gets a reference to a 
				// XamComboEditor for either the Value or ComparisonOperator field, we set the ItemsProvider,
				// etc as needed.
				//
				//Style style = new Style(typeof(XamComboEditor));
				//ComboBoxItemsProvider ip = new ComboBoxItemsProvider();
				//ip.ItemsSource = this.ComparisonOperatorSelector.Items;
				//style.Setters.Add(new Setter(XamComboEditor.ItemsProviderProperty, ip));
				//style.Setters.Add(new Setter(XamComboEditor.DisplayValueSourceProperty, DisplayValueSource.Value));
				//
				////~ SSP 3/16/09 Display Value Task
				////~ Added DisplayValue and DisplayValueSource properties to XamComboEditor. Also added
				////~ a workaround in the XamComboEditor that works around the bug in ComboBox where it
				////~ fallbacks to using a string template for dependency object. As a result, the following
				////~ lines aren't necessary. Commented out the following.
				////~ 
				////~ create the custom combobox style since we need to set the item template
				////~ otherwise it will use a string conversion
				////~Style cbStyle = new Style(typeof(ComboBox));
				////~cbStyle.Setters.Add(new Setter(ComboBox.ItemTemplateProperty, new DynamicResourceExtension(new DataTemplateKey(typeof(ComparisonOperatorListItem)))));
				////~style.Setters.Add(new Setter(XamComboEditor.ComboBoxStyleProperty, cbStyle));
				//
				//comparisonOperatorField.Settings.EditorStyle = style;
				ComboBoxItemsProvider ip = new ComboBoxItemsProvider();
				ip.ItemsSource = this.ComparisonOperatorSelector.Items;
				_operatorInitializer = new ComboEditorInitializer(ip, null, DisplayValueSource.Value);

                // we need to use a custom converter on the field. basically we need the editor 
                // to deal with ComparisonOperatorListItem but the field is actually of type 
                // ComparisonOperator
                comparisonOperatorField.Converter = new ComparisonOperatorToListItemConverter(this.ComparisonOperatorSelector);

            }

			#endregion //Setup the ComparisonOperator Field

			#region Setup the Value (i.e., Operand) Field

			Field valueField = e.FieldLayout.Fields["Value"];
			Debug.Assert(valueField != null, "Value Field definition not found in FieldLayout!");
			if (valueField != null)
			{
				valueField.Label				= DataPresenterBase.GetString("CustomFilterSelectionControl_OperandFieldLabel");

				valueField.Settings.EditorType	= recordFilterTargetField.FilterEditorTypeResolved;
				valueField.Settings.EditAsType	= recordFilterTargetField.FilterEditAsTypeResolved;

				// JM 02-14-12 TFS100423 - Handle FilterOperandUIType.DropDownList
				Field recordFilterField = this.RecordFilter.Field;
				//if (this.RecordFilter.Field								!= null &&
				//    this.RecordFilter.Field.FilterOperandUITypeResolved == FilterOperandUIType.Combo)
				if (recordFilterField != null && (recordFilterField.FilterOperandUITypeResolved == FilterOperandUIType.Combo ||
												  recordFilterField.FilterOperandUITypeResolved == FilterOperandUIType.DropDownList))
				{
					// AS 1/14/11 TFS63183
					//Style style			= new Style();
					//style.TargetType	= typeof(XamComboEditor);
					//
					//Setter setter		= new Setter();
					//setter.Property		= XamComboEditor.IsEditableProperty;
					//setter.Value		= true;
					//style.Setters.Add(setter);
					//
					//setter = new Setter();
					//setter.Property		= XamComboEditor.ItemsSourceProperty;
					//setter.Value		= this.Operands;
					//style.Setters.Add(setter);
					//
					//valueField.Settings.EditorStyle	= style;
					ComboBoxItemsProvider ip = new ComboBoxItemsProvider();
					ip.ItemsSource = this.Operands;

					// JM 02-14-12 TFS100423 - Handle FilterOperandUIType.DropDownList
					//_operandInitializer = new ComboEditorInitializer(ip, true, null);
					_operandInitializer = new ComboEditorInitializer(ip, recordFilterField.FilterOperandUITypeResolved == FilterOperandUIType.Combo, null);
				}
			}

			#endregion //Setup the Value (i.e., Operand) Field

			#region Setup the ErrorDescription Field

			Field errorDescriptionField = e.FieldLayout.Fields["ErrorDescription"];
			Debug.Assert(errorDescriptionField != null, "Value Field definition not found in FieldLayout!");
			if (errorDescriptionField != null)
			{
				errorDescriptionField.Label		= "";

				errorDescriptionField.Settings.EditorType 
												= typeof(XamTextEditor);
				errorDescriptionField.Settings.EditAsType 
												= typeof(string);

				Style style						= new Style();
				style.TargetType				= typeof(XamTextEditor);

				Setter setter					= new Setter();
				setter.Property					= TextBlock.ForegroundProperty;
				setter.Value					= System.Windows.Media.Brushes.Red;
				style.Setters.Add(setter);

				setter							= new Setter();
				setter.Property					= XamTextEditor.TextWrappingProperty;
				setter.Value					= TextWrapping.Wrap;
				style.Setters.Add(setter);

				setter							= new Setter();
				setter.Property					= XamTextEditor.HorizontalContentAlignmentProperty;
				setter.Value					= HorizontalAlignment.Stretch;
				style.Setters.Add(setter);

				errorDescriptionField.Settings.EditorStyle = style;
			}

			#endregion //Setup the ErrorDescription Field

			#region Hide/show Fields for debugging




			e.FieldLayout.Fields["Group"].Visibility	= Visibility.Collapsed;


			#endregion //Hide/show Fields for debugging
		}

				#endregion //OnFieldLayoutInitialized	
        
				#region OnLoaded

		void OnLoaded(object sender, RoutedEventArgs e)
		{
			// Create ConditionInfoCollection from the Conditions in the RecordFilter.
			this._conditionInfos = new ConditionInfoCollection(new List<ConditionInfo>(5));

			if (this.RecordFilter == null)
				return;

			// AS - NA 11.2 Excel Style Filtering
			// If we're using a different record filter then consider the dialog dirty.
			//
			if (this.RecordFilter != this.GetTargetRecordFilter())
				this._isDirty = true;

			// Process all the conditions in the RecordFilter's conditions collection.  If we encounter any conditions that are not
			// supported in this custom filter selection control (we currently only support ComparisonConditions), clear the conditionInfos
			// collection to remove any supported conditions that may have been added.
			this._rootConditionInfoGroup	= new ConditionInfoGroup(null, this.RecordFilter.Conditions.LogicalOperator);
			bool allConditionsSupported		= this.ProcessConditionGroup(this.RecordFilter.Conditions, this._rootConditionInfoGroup);
			if (allConditionsSupported == false)
			{
				this._rootConditionInfoGroup.ChildConditions.Clear();
				this._conditionInfos.Clear();

				// Initialize an empty _rootConditionInfoGroup.
				this._rootConditionInfoGroup = new ConditionInfoGroup(null, this.RecordFilter.Conditions.LogicalOperator);

				// Initialize an empty _tempRootConditionGroup.
				this._tempRootConditionGroup					= new ConditionGroup();
				this._tempRootConditionGroup.LogicalOperator	= this.RecordFilter.Conditions.LogicalOperator;
			}
			else
			{
				this.RefreshConditionInfosCollection();
				this.RefreshTempRootConditionGroup();
			}


			// Set the DataSource of the XamDataGrid we are expecting in our template to the ConditionInfoCollection we have created, and
			// listen to some events.
			if (this._conditionsGrid != null)
			{
				// AS 9/14/09 TFS22126
				// This is not getting cleared and so the RD from the ToolWindow (which is set from the 
				// owning DP and therefore is the RD of the owning DP) has a reference to this grid. This 
				// doesn't seem to be needed anymore to fix the original issue which may have been an 
				// issue with the headerpresenter or something.
				//
				//// JM 02-19-09 TFS14172
				//this._conditionsGrid.Resources				= ToolWindow.GetToolWindow(this).Resources;

				// JM 11-11-11 TFS94718 - Set a new instance of FieldLayoutSettings and FieldSettings on our grid to ensure that any settings
				// contained in an implicit style for the grid specified in the hosting application does not cause us to use settings that we don't want.
				this._conditionsGrid.FieldLayoutSettings	= new FieldLayoutSettings();
				this._conditionsGrid.FieldSettings			= new FieldSettings();

				this._conditionsGrid.RecordContainerGenerationMode
															= ItemContainerGenerationMode.PreLoad;
				this._conditionsGrid.CellContainerGenerationMode
															= CellContainerGenerationMode.PreLoad;
				this._conditionsGrid.FieldLayoutSettings.SelectionTypeCell 
															= SelectionType.None;
				this._conditionsGrid.FieldLayoutSettings.AllowFieldMoving
															= AllowFieldMoving.No;		// TFS13922
				this._conditionsGrid.FieldSettings.LabelClickAction
															= LabelClickAction.Nothing;
				this._conditionsGrid.UpdateMode				= UpdateMode.OnCellChange;
				this._conditionsGrid.ScrollingMode			= ScrollingMode.Immediate;
				this._conditionsGrid.AutoFit				= true;
				this._conditionsGrid.SelectedItemsChanged	+= new EventHandler<Infragistics.Windows.DataPresenter.Events.SelectedItemsChangedEventArgs>(OnSelectedConditionsChanged);
				this._conditionsGrid.FieldLayoutInitialized += new EventHandler<Infragistics.Windows.DataPresenter.Events.FieldLayoutInitializedEventArgs>(OnFieldLayoutInitialized);
				this._conditionsGrid.EditModeStarted		+= new EventHandler<Infragistics.Windows.DataPresenter.Events.EditModeStartedEventArgs>(OnConditionsGridEditModeStarted);
				this._conditionsGrid.EditModeEnded			+= new EventHandler<Infragistics.Windows.DataPresenter.Events.EditModeEndedEventArgs>(OnConditionsGridEditModeEnded);
				this._conditionsGrid.DataSource				= this._conditionInfos;

				// JM 06-02-09 TFS 18008
				this._conditionsGrid.RecordsDeleting		+= new EventHandler<Infragistics.Windows.DataPresenter.Events.RecordsDeletingEventArgs>(OnConditionsGridRecordsDeleting);

				// JJD 07/06/10 - TFS32174 - added
				this._conditionsGrid.CellActivated			+= new EventHandler<Infragistics.Windows.DataPresenter.Events.CellActivatedEventArgs>(OnConditionsGridCellActivated);
			}


			this._conditionInfos.CollectionChanged			+= new System.Collections.Specialized.NotifyCollectionChangedEventHandler(OnConditionInfosCollectionChanged);
			this._conditionInfos.ItemPropertyChanged		+= new EventHandler<ItemPropertyChangedEventArgs>(OnConditionInfosItemPropertyChanged);

			this.UpdateSelectionDependentFlags();
			this.AddRemoveUnboundFieldsUsedAsGroupingIndicators();

			// AS - NA 11.2 Excel Style Filtering
			// If the filter was different then we want to check if there are any 
			// errors and if so focus that cell to start.
			//
			if (_isDirty)
				this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded, new GridUtilities.MethodDelegate(this.InitializeErrorDescriptions));
			// JM 05-08-12 TFS110589
			else if (this._conditionsGrid.ViewableRecords.Count > 0)
			{
				// JM 05-21-12 TFS112214 - in certain themes this is too early to set the selection.
				//this._conditionsGrid.SelectedItems.Records.Add(this._conditionsGrid.ViewableRecords[0]);
				this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded, new GridUtilities.MethodDelegate(this.SelectFirstRecord));
			}
		}

		// JM 05-21-12 TFS112214
		private void SelectFirstRecord()
		{
			if (this._conditionsGrid.ViewableRecords.Count > 0)
				this._conditionsGrid.SelectedItems.Records.Add(this._conditionsGrid.ViewableRecords[0]);
		}

				#endregion //OnLoaded	

				#region OnSelectedConditionsChanged

		void OnSelectedConditionsChanged(object sender, Infragistics.Windows.DataPresenter.Events.SelectedItemsChangedEventArgs e)
		{
			this.UpdateSelectedStatusOfAllConditionInfos();
			this.UpdateSelectionDependentFlags();
		}

				#endregion //OnSelectedConditionsChanged	

				#region OnToolWindowClosing

        
#region Infragistics Source Cleanup (Region)






















#endregion // Infragistics Source Cleanup (Region)

				#endregion //OnToolWindowClosing

				#region ProcessConditionGroup

		private bool ProcessConditionGroup(ConditionGroup conditions, ConditionInfoGroup parentConditionInfoGroup)
		{
			// Process all conditions.
			foreach (ICondition condition in conditions)
			{
				if (condition is ConditionGroup)
				{
					ConditionInfoGroup conditionInfoGroup = new ConditionInfoGroup(parentConditionInfoGroup, ((ConditionGroup)condition).LogicalOperator);
					parentConditionInfoGroup.ChildConditions.Add(conditionInfoGroup);

					bool allConditionsSupported = this.ProcessConditionGroup(condition as ConditionGroup, conditionInfoGroup);
					if (allConditionsSupported == false)
						return false;

					continue;
				}

				// We are only supporting ComparisonConditions in the CustomFilterSelectionControl.
				// If we encounter a consition that is not a ComparisonCondition, stop processing conditions and return false;
				if (condition is ComparisonCondition)
				{
					ComparisonCondition cc = condition as ComparisonCondition;
					// JJD 02/17/12 - TFS101703 
					// Special case ComparisonCondition with explicit display text. For this case we only want to keep the 
 					// value as the ComparisonCondition and use the 'Equals' operator and the explicit DisplayText
					//parentConditionInfoGroup.ChildConditions.Add(new ConditionInfo(parentConditionInfoGroup, cc.Operator, cc.Value));
					if ( !string.IsNullOrEmpty(cc.DisplayText))
						parentConditionInfoGroup.ChildConditions.Add(new ConditionInfo(parentConditionInfoGroup, ComparisonOperator.Equals, cc, cc.DisplayText));
					else
						parentConditionInfoGroup.ChildConditions.Add(new ConditionInfo(parentConditionInfoGroup, cc.Operator, cc.Value, null));
				}
				else
					return false;
			}

			return true;
		}

				#endregion //ProcessConditionGroup	

				#region ProcessConditionInfoGroupForCleanup

		private void ProcessConditionInfoGroupForCleanup(ConditionInfoGroup conditionInfoGroup)
		{
			for (int i = 0; i < conditionInfoGroup.ChildConditions.Count; i++)
			{
				ConditionInfoGroup childConditionInfoGroup = conditionInfoGroup.ChildConditions[i] as ConditionInfoGroup;
				if (childConditionInfoGroup != null)
				{
					if (childConditionInfoGroup.ContainsOneGroupAndNoConditions)
					{
						this.UngroupGroupAndItsChildrenHelper(childConditionInfoGroup);

						// Restart processing at the beginning.
						i = -1;
						continue;
					}

					if (childConditionInfoGroup.ChildConditions.Count == 0)
					{
						conditionInfoGroup.ChildConditions.Remove(childConditionInfoGroup);

						// Restart processing at the beginning.
						i = -1;
						continue;
					}

					this.ProcessConditionInfoGroupForCleanup(childConditionInfoGroup);
				}
			}
		}

				#endregion //ProcessConditionInfoGroupForCleanup

				#region ProcessConditionInfoGroupForCommit

		private void ProcessConditionInfoGroupForCommit(ConditionInfoGroup conditionInfoGroup, ConditionGroup conditionGroup)
		{
			foreach (ConditionInfo conditionInfo in conditionInfoGroup.ChildConditions)
			{
				if (conditionInfo is ConditionInfoGroup)
				{
					ConditionGroup newConditionGroup = new ConditionGroup();
					newConditionGroup.LogicalOperator = ((ConditionInfoGroup)conditionInfo).LogicalOperator;
					conditionGroup.Add(newConditionGroup);

					this.ProcessConditionInfoGroupForCommit(conditionInfo as ConditionInfoGroup, newConditionGroup);
				}
				else
				{
					Exception e;

					// JJD 10/07/10 - TFS37236
					// If the value is a ComparionCondition then use it as is 
					ComparisonCondition cc = conditionInfo.Value as ComparisonCondition;
					if (cc != null)
					{
						// If the comparison condition's operator doesn't math the current operator
						// then clone the comparison and set its Operator to the current operator.
						// Otherwise use it as is.
						// JJD 02/17/12 - TFS101703 
						// Special case ComparisonCondition with explicit display text. For this case we want
						// to keep the ComparisonCondition 'as is'
						//if (cc.Operator != conditionInfo.ComparisonOperator)
						if (cc.Operator != conditionInfo.ComparisonOperator && string.IsNullOrEmpty(cc.DisplayText))
						{
							ComparisonCondition ccClone = ((ICloneable)cc).Clone() as ComparisonCondition;
							ccClone.Operator = conditionInfo.ComparisonOperator;
							conditionGroup.Add(ccClone);
						}
						else
							conditionGroup.Add(cc);
					}
					else
						conditionGroup.Add(ComparisonCondition.Create(conditionInfo.ComparisonOperator, conditionInfo.Value, false, out e));
				}
			}
		}

				#endregion //ProcessConditionInfoGroupForCommit	

				#region ProcessConditionInfoGroupForGroupLevel

		private void ProcessConditionInfoGroupForGroupLevel(ConditionInfoGroup conditionInfoGroup, ref int highestGroupLevel)
		{
			foreach (ConditionInfo conditionInfo in conditionInfoGroup.ChildConditions)
			{
				if (conditionInfo is ConditionInfoGroup)
				{
					highestGroupLevel = Math.Max(highestGroupLevel, ((ConditionInfoGroup)conditionInfo).Level);
					this.ProcessConditionInfoGroupForGroupLevel(conditionInfo as ConditionInfoGroup, ref highestGroupLevel);
				}
			}
		}

				#endregion //ProcessConditionInfoGroupForGroupLevel

				#region ProcessConditionInfoGroupForConditionInfoRefresh

		private void ProcessConditionInfoGroupForConditionInfoRefresh(ConditionInfoGroup conditionInfoGroup)
		{
			foreach (ConditionInfo conditionInfo in conditionInfoGroup.ChildConditions)
			{
				if (conditionInfo is ConditionInfoGroup)
					this.ProcessConditionInfoGroupForConditionInfoRefresh(conditionInfo as ConditionInfoGroup);
				else
					this._conditionInfos.Add(conditionInfo);
			}
		}

				#endregion //ProcessConditionInfoGroupForConditionInfoRefresh	
    
				#region RefreshConditionInfosCollection

		private void RefreshConditionInfosCollection()
		{
			this._conditionInfos.BeginUpdate();

			this._conditionInfos.Clear();
			this.ProcessConditionInfoGroupForConditionInfoRefresh(this._rootConditionInfoGroup);

			this._conditionInfos.EndUpdate();
		}

				#endregion //RefreshConditionInfosCollection	
    
				#region RefreshConditionsGrid

		private void RefreshConditionsGrid()
		{
			// JM 07-01-10 TFS27422
			this._conditionsGrid.ExecuteCommand(DataPresenterCommands.EndEditModeAndAcceptChanges);

			// JM 05-8-12 TFS110624 - Call the new overload with true.
			//List<ConditionInfo> selectedConditionInfos = this.GetSelectedConditionInfos();
			List<ConditionInfo> selectedConditionInfos = this.GetSelectedConditionInfos(true);
			this._conditionsGrid.SelectedItems.ClearAllSelected();

			this.RefreshConditionInfosCollection();
			this.RefreshTempRootConditionGroup();

			this._conditionsGrid.DataSource = null;
			this._conditionsGrid.DataSource	= this._conditionInfos;

			// Select the same records that were selected before we reset the DataSource above.
			foreach (DataRecord record in this._conditionsGrid.RecordManager.Current)
			{
				int index = selectedConditionInfos.IndexOf(record.DataItem as ConditionInfo);
				if (index > -1)
					this._conditionsGrid.SelectedItems.Records.Add(record);
			}
		}

				#endregion //RefreshConditionsGrid	
    
				#region RefreshTempRootConditionGroup



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		private void RefreshTempRootConditionGroup()
		{
			// Process all ConditionInfos and reconstruct the conditions in a ConditionGroup that we can eventually assign to the RecordFilter that was originally passed to us.
			this._tempRootConditionGroup					= new ConditionGroup();
			this._tempRootConditionGroup.LogicalOperator	= this._rootConditionInfoGroup.LogicalOperator;

			this.ProcessConditionInfoGroupForCommit(this._rootConditionInfoGroup, this._tempRootConditionGroup);


			// Update the FilterSummaryDescription.
			this.SetValue(FilterSummaryDescriptionPropertyKey, this._tempRootConditionGroup.ToolTip);
		}

				#endregion //RefreshTempRootConditionGroup

				// JM 06-02-09 TFS 18008 - Added.
				#region RemoveSelectedConditions

		private bool RemoveSelectedConditions()
		{
			// The following code was moved from the ExecuteRemoveSelectedConditionsCommand method so it could be called
			// from multiple places.
			// JM 05-8-12 TFS110624 - Call the new overload with true.
			//List<ConditionInfo> selectedConditionInfos = this.GetSelectedConditionInfos();
			List<ConditionInfo> selectedConditionInfos = this.GetSelectedConditionInfos(true);
			if (selectedConditionInfos.Count > 0)
			{
				// JM 05-08-12 TFS110589
				int holdRecordIndex = -1;
				if (this._conditionsGrid.ViewableRecords.Count > 0 && this._conditionsGrid.SelectedItems.Records.Count > 0)
					holdRecordIndex = this._conditionsGrid.ViewableRecords.IndexOf(this._conditionsGrid.SelectedItems.Records[0]);
				else
				{
					DataRecord activeRecord = this._conditionsGrid.ActiveRecord as DataRecord;
					if (activeRecord != null && activeRecord.DataItem is ConditionInfo)
						holdRecordIndex = this._conditionsGrid.ViewableRecords.IndexOf(activeRecord);
				}

				this._conditionInfos.BeginUpdate();
				foreach (ConditionInfo conditionInfo in selectedConditionInfos)
				{
					ConditionInfoGroup currentConditionInfoGroup = conditionInfo.Group;
					currentConditionInfoGroup.ChildConditions.Remove(conditionInfo);

					// Even though the _conditionInfos collection will eventually reflect the removal of this conditionInfo from the Group when
					// we call RefreshConditionsGrid below, explicitly remove it here so that intervening code which looks at the _conditionInfos collection
					// does not 'see it'.
					this._conditionInfos.Remove(conditionInfo);

					// If the condition we just removed was the last condition in its Group, the remove the Group from it's
					// parent group.
					if (currentConditionInfoGroup.ChildConditions.Count		== 0	&&
						currentConditionInfoGroup.Group						!= null &&
						currentConditionInfoGroup.Group.ChildConditions.Contains(currentConditionInfoGroup))
						currentConditionInfoGroup.Group.ChildConditions.Remove(currentConditionInfoGroup);
					else
					{
						// If the current group we just removed the condition from contains a single child and that child is a group, 'ungroup'
						// that child group.
						if (currentConditionInfoGroup.ContainsOneGroupAndNoConditions)
							this.UngroupGroupAndItsChildrenHelper(currentConditionInfoGroup.ChildConditions[0] as ConditionInfoGroup);
					}
				}

				this._conditionInfos.EndUpdate();


				// Force the conditions grid to update it's unbound fields to reflect the grouping's new operator.
				this.UpdateSelectionDependentFlags();
				this.AddRemoveUnboundFieldsUsedAsGroupingIndicators();
				this.RefreshConditionsGrid();

				// JM 05-08-12 TFS110589 - Since we just removed all the records in the selection, select a new record.
				if (holdRecordIndex > -1)
				{
					holdRecordIndex = Math.Min(holdRecordIndex, this._conditionsGrid.ViewableRecords.Count - 1);
					if (holdRecordIndex > -1)
						this._conditionsGrid.SelectedItems.Records.Add(this._conditionsGrid.ViewableRecords[holdRecordIndex]);
				}

				return true;
			}

			return false;
		}

				#endregion //RemoveSelectedConditions

				#region TryEndEditMode

		private bool TryEndEditMode()
		{
			if (this.IsConditionInEditMode)
			{
				this._conditionsGrid.ActiveCell.EndEditMode(true);

				return !this._conditionsGrid.ActiveCell.IsInEditMode;
			}

			return true;
		}

				#endregion //TryEndEditMode

				#region UngroupGroupAndItsChildrenHelper

		// Ungroups a group and all of its children.
		private void UngroupGroupAndItsChildrenHelper(ConditionInfoGroup group)
		{
			ConditionInfoGroup	parentConditionInfoGroup				= group.Group;
			int					indexInParentGroup						= parentConditionInfoGroup.ChildConditions.IndexOf(group);
			bool				assignChildGroupOperatorToParentGroup	= parentConditionInfoGroup.ChildConditions.Count == 1;
			foreach (ConditionInfo conditionInfo in group.ChildConditions)
			{
				parentConditionInfoGroup.ChildConditions.Insert(indexInParentGroup, conditionInfo);
				indexInParentGroup++;
			}

			if (assignChildGroupOperatorToParentGroup)
				parentConditionInfoGroup.LogicalOperator = group.LogicalOperator;

			parentConditionInfoGroup.ChildConditions.Remove(group);
		}

				#endregion //UngroupGroupAndItsChildrenHelper

				#region UpdateSelectionDependentFlags

		private void UpdateSelectionDependentFlags()
		{
			// Set flag that indicates whether the current selection spans multiple groups.
			this._currentSelectionSpansMultipleGroups	= false;

			List<ConditionInfo> selectedConditionInfos	= this.GetSelectedConditionInfos();
			ConditionInfoGroup	group					= null;
			foreach (ConditionInfo conditionInfo in selectedConditionInfos)
			{
				if (group == null)
				{
					group = conditionInfo.Group;
					continue;
				}

				if (conditionInfo.Group != group)
				{
					this._currentSelectionSpansMultipleGroups = true;
					break;
				}
			}


			// Initialize the 'allow grouping/ungrouping' flags to false and then update below based on what we find.
			this._currentSelectionCanBeGrouped			= false;
			this._currentSelectionCanBeUngrouped		= false;


			// Update the current selection count.
			this._currentSelectionCount					= selectedConditionInfos.Count;


			// If we don't have a conditionsGrid PART, there is no reason to continue.
			if (this._conditionsGrid == null)
				return;


			List<Record>	selectedRecords			= this.GetSelectedRecordsInVisibleIndexOrder(this._conditionsGrid.SelectedItems.Records);
			int				totalSelectedRecords	= selectedRecords.Count;
			if (totalSelectedRecords < 1)
				return;


			// Setup some context.
			ConditionInfo		firstSelectedCondition			= ((DataRecord)selectedRecords[0]).DataItem as ConditionInfo;
			ConditionInfoGroup	currentGroup					= firstSelectedCondition.Group;


			// Set the CanBeGrouped and CanBeUngrouped flags
			if (this._currentSelectionSpansMultipleGroups == false)
			{
				bool currentGroupIsCompletelySelected = this.IsGroupCompletelySelected(currentGroup);

				if (currentGroupIsCompletelySelected)
				{
					this._currentSelectionCanBeGrouped		= false;
					this._currentSelectionCanBeUngrouped	= currentGroup != null && currentGroup.Level != 0;
				}
				else
				{
					this._currentSelectionCanBeGrouped		= this._currentSelectionCount > 1;	// JM 01-22-09 TFS12632
					this._currentSelectionCanBeUngrouped	= currentGroup != null && currentGroup.Level != 0;
				}
			}
			else
			{
				bool selectionContainsAtLeastOneNonLevelZeroCondition = false;
				foreach (ConditionInfo conditionInfo in selectedConditionInfos)
				{
					if (conditionInfo.Group.Level > 0)
					{
						selectionContainsAtLeastOneNonLevelZeroCondition = true;
						break;
					}
				}

				this._currentSelectionCanBeGrouped			= false;
				this._currentSelectionCanBeUngrouped		= selectionContainsAtLeastOneNonLevelZeroCondition;
			}
		}

				#endregion //UpdateSelectionDependentFlags	
    
				#region UpdateSelectedStatusOfAllConditionInfos

		private void UpdateSelectedStatusOfAllConditionInfos()
		{
			if (this._conditionsGrid == null)
				return;

			if (this._conditionInfos.Count < 1)
				return;


			// Create a temporary list of the selected ConditionInfos
			SelectedRecordCollection	selectedRecords			= this._conditionsGrid.SelectedItems.Records;
			List<ConditionInfo>			selectedConditionInfos	= new List<ConditionInfo>(selectedRecords.Count);
			foreach (DataRecord record in selectedRecords)
				selectedConditionInfos.Add(record.DataItem as ConditionInfo);


			// Update each Condition Info in the list with its selected state.
			foreach (ConditionInfo conditionInfo in this._conditionInfos)
			{
				conditionInfo.IsSelected = selectedConditionInfos.Contains(conditionInfo);
			}
		}

				#endregion //UpdateSelectedStatusOfAllConditionInfos	
            
				#region VerifyParts

		internal void VerifyParts()
		{
            Debug.Assert(_conditionsGrid == null || this.GetTemplateChild("PART_ConditionsGrid") == _conditionsGrid, "We should probably update the conditions grid");

			if (this._conditionsGrid == null)
				this._conditionsGrid = this.GetTemplateChild("PART_ConditionsGrid") as XamDataGrid;
		}

				#endregion //VerifyParts

			#endregion //Private Methods

		#endregion //Methods

		#region Nested Private Class GroupFieldStyleSelector

	private class GroupFieldStyleSelector : StyleSelector
	{
		#region Member Variables

		private CustomFilterSelectionControl					_customFilterSelectionControl;

		#endregion //Member Variables	
    
		#region Constructor

		internal GroupFieldStyleSelector(CustomFilterSelectionControl customFilterSelectionControl)
		{
			Debug.Assert(customFilterSelectionControl != null, "customFilterSelectionControl is null in GroupFieldStyleSelector!");
			if (customFilterSelectionControl == null)
				throw new ArgumentException();

			this._customFilterSelectionControl = customFilterSelectionControl;
		}

		#endregion //Constructor	
    
		#region Base Class Overrides

			#region SelectStyle

		public override Style SelectStyle(object item, DependencyObject container)
		{
			CellValuePresenter cvp = container as CellValuePresenter;
			if (cvp == null)
				return null;

			int				unboundFieldGroupLevel	= (int)cvp.Field.Tag;
			ConditionInfo	conditionInfo			= cvp.Record.DataItem as ConditionInfo;
			if (conditionInfo != null)
			{
				ConditionInfoGroup	recordConditionInfoGroup= conditionInfo.Group;
				int					recordGroupLevel		= recordConditionInfoGroup.Level;
				LogicalOperator		recordLogicalOperator	= recordConditionInfoGroup.LogicalOperator;
				if (unboundFieldGroupLevel <= recordGroupLevel)
				{
					Style style = new Style();
					style.TargetType = typeof(CellValuePresenter);

					ControlTemplate template = new ControlTemplate(typeof(CellValuePresenter));
					FrameworkElementFactory fefBorder = new FrameworkElementFactory(typeof(Border));
					template.VisualTree = fefBorder;

					FrameworkElementFactory fefCardPanel = new FrameworkElementFactory(typeof(CardPanel));
					FrameworkElementFactory fefRectangle = new FrameworkElementFactory(typeof(System.Windows.Shapes.Rectangle));

					ConditionInfoGroup unboundFieldConditionInfoGroup;
					if (unboundFieldGroupLevel == recordGroupLevel)
						unboundFieldConditionInfoGroup = recordConditionInfoGroup;
					else
						unboundFieldConditionInfoGroup = recordConditionInfoGroup.GetParentGroupAtLevel(unboundFieldGroupLevel);

					fefRectangle.SetValue(System.Windows.Shapes.Rectangle.FillProperty,
										  unboundFieldConditionInfoGroup.LogicalOperator == LogicalOperator.And ?
													this._customFilterSelectionControl.AndLogicalOperatorBrush :
													this._customFilterSelectionControl.OrLogicalOperatorBrush);

					double topMargin = conditionInfo.IsFirstInGroup && unboundFieldGroupLevel == recordGroupLevel ? 2 : 0;
					double bottomMargin = conditionInfo.IsLastInGroup && unboundFieldGroupLevel == recordGroupLevel ? 2 : 0;
					fefRectangle.SetValue(System.Windows.Shapes.Rectangle.MarginProperty, new Thickness(3, topMargin, 3, bottomMargin));

					fefCardPanel.AppendChild(fefRectangle);
					fefBorder.AppendChild(fefCardPanel);


					// Set the template property.
					Setter templateSetter = new Setter();
					templateSetter.Property = Control.TemplateProperty;
					templateSetter.Value = template;
					style.Setters.Add(templateSetter);


					// Set the cursor property
					Setter setter = new Setter();
					setter.Property = FrameworkElement.CursorProperty;
					setter.Value = Cursors.Hand;
					style.Setters.Add(setter);

					// Set the ToolTip property
					setter = new Setter();
					setter.Property = FrameworkElement.ToolTipProperty;
					setter.Value = unboundFieldConditionInfoGroup.LogicalOperator == LogicalOperator.And ?
														DataPresenterBase.GetString("CustomFilterSelectionControl_ChangeGroupLogicalOperatorTo_Or") :
														DataPresenterBase.GetString("CustomFilterSelectionControl_ChangeGroupLogicalOperatorTo_And");
					style.Setters.Add(setter);

					// Set the Tag property to the ConditionInfoGroup associated with this CellValuePresenter
					setter = new Setter();
					setter.Property = FrameworkElement.TagProperty;
					setter.Value = unboundFieldConditionInfoGroup;
					style.Setters.Add(setter);

					// Set and Event Handler for the MouseDown event
					EventSetter eventSetter = new EventSetter();
					eventSetter.Event = UIElement.MouseDownEvent;
					eventSetter.Handler = new MouseButtonEventHandler(OnMouseDown);
					style.Setters.Add(eventSetter);

					return style;
				}
				// JM 02-17-09 TFS 14001 - For CellValuePresenters that do not contain an indicator, return a simple Style so the default style does 
				//						   not get picked up.  This will ensure that all CellValuePresenters in our UnboundField columns look the same
				//						   regardless of what styling is provided by the current Theme.
				//return null;
				else
				{
					Style style								= new Style();
					style.TargetType						= typeof(CellValuePresenter);

					ControlTemplate			template		= new ControlTemplate(typeof(CellValuePresenter));
					FrameworkElementFactory fefBorder		= new FrameworkElementFactory(typeof(Border));
					template.VisualTree = fefBorder;

					// Set the template property.
					Setter templateSetter					= new Setter();
					templateSetter.Property					= Control.TemplateProperty;
					templateSetter.Value					= template;
					style.Setters.Add(templateSetter);

					return style;
				}
			}
			else
				return null;
		}

			#endregion //SelectStyle

		#endregion //Base Class Overrides

		#region Methods

			#region OnMouseDown

		private static void OnMouseDown(object sender, MouseButtonEventArgs e)
		{
			CellValuePresenter cvp = sender as CellValuePresenter;
			if (cvp != null)
			{
				ConditionInfoGroup conditionInfoGroup = cvp.Tag as ConditionInfoGroup;
				if (conditionInfoGroup != null)
				{
					CustomFilterSelectionControl customFilterSelectionControl = Utilities.GetAncestorFromType(cvp, typeof(CustomFilterSelectionControl), true) as CustomFilterSelectionControl;
					if (customFilterSelectionControl != null)
					{
						LogicalOperator newLogicalOperator = conditionInfoGroup.LogicalOperator == LogicalOperator.And ? LogicalOperator.Or : LogicalOperator.And;
						conditionInfoGroup.LogicalOperator = newLogicalOperator;

						// Force the conditions grid to update it's unbound fields to reflect the grouping's new operator.
						customFilterSelectionControl.RefreshConditionsGrid();
					}
				}
			}
		}

			#endregion //OnMouseDown

		#endregion // Methods
	}

		#endregion //Nested Private Class GroupFieldStyleSelector

        // AS 3/12/09 TFS15327
        #region ComparisonOperatorToListItemConverter
    private class ComparisonOperatorToListItemConverter : IValueConverter
    {
        #region Member Variables

        private ComparisonOperatorSelector _selector;

        #endregion //Member Variables

        #region Constructor
        internal ComparisonOperatorToListItemConverter(ComparisonOperatorSelector selector)
        {
            _selector = selector;
        }
        #endregion //Constructor

        #region IValueConverter Members

        object IValueConverter.Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Debug.Assert(value == null || value is DBNull || value is ComparisonOperator);

            if (value is ComparisonOperator)
            {
                ComparisonOperator op = (ComparisonOperator)value;

                // first iterate the items to see if we have an item in the dropdown
                // for the specified operator
                foreach (ComparisonOperatorListItem item in _selector.Items)
                {
                    if (item.Operator == op)
                        return item;
                }

                // if not then this one has been filtered out (likely because its not 
                // allowed) so let the comparisonoperator selector find it amongst
                // its internal list which contains all the list items
                _selector.SelectedOperator = op;

                return _selector.SelectedOperatorInfo;
            }

            return value;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Debug.Assert(value == null || value is DBNull || value is ComparisonOperatorListItem);

            ComparisonOperatorListItem li = value as ComparisonOperatorListItem;

            if (null != li)
                return li.Operator;

            return value;
        }

        #endregion
    } 
        #endregion //ComparisonOperatorToListItemConverter

		// AS 1/14/11 TFS63183
		#region ComboEditorInitializer
	private class ComboEditorInitializer
	{
		private ComboBoxItemsProvider _itemsProvider;
		private object _isEditable;
		private object _displayValueSource;

		internal ComboEditorInitializer(ComboBoxItemsProvider itemsProvider, bool? isEditable, DisplayValueSource? displayValueSource)
		{
			_itemsProvider = itemsProvider;
			_isEditable = isEditable;
			_displayValueSource = displayValueSource;
		}

		public void Initialize(XamComboEditor editor)
		{
			if (null != editor)
			{
				editor.ItemsProvider = _itemsProvider;

				if (null != _isEditable)
					editor.SetValue(XamComboEditor.IsEditableProperty, _isEditable);

				if (null != _displayValueSource)
					editor.SetValue(XamComboEditor.DisplayValueSourceProperty, _displayValueSource);
			}
		}
	} 
		#endregion //ComboEditorInitializer



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

	}



	#region ConditionInfoCollection Internal Class

	// JM 07-15-09 TFS 18595, 18637 - Add support for ITypedList.
	internal class ConditionInfoCollection : ObservableCollectionExtended<ConditionInfo>,
											 ITypedList
	{
		internal ConditionInfoCollection(List<ConditionInfo> list) : base(list)
		{
		}

		// JM 07-15-09 TFS 18595, 18637 - Added.
		#region ITypedList Members

		PropertyDescriptorCollection ITypedList.GetItemProperties(PropertyDescriptor[] listAccessors)
		{
			return new PropertyDescriptorCollection( new PropertyDescriptor [] { new CustomPropertyDescriptor("ComparisonOperator"),
																				 new CustomPropertyDescriptor("Group"),
																				 new CustomPropertyDescriptor("Value"),
																				 new CustomPropertyDescriptor("ErrorDescription")});
		}

		string ITypedList.GetListName(PropertyDescriptor[] listAccessors)
		{
			return string.Empty;
		}

		#endregion
	}

		// JM 07-15-09 TFS 18595, 18637 - Added.
		#region CustomPropertyDescriptor for the ConditionInfoCollection ITypedList implementation

	internal class CustomPropertyDescriptor : PropertyDescriptor
	{
		internal CustomPropertyDescriptor(string name)
			: base(name, null)
		{
		}

		public override bool CanResetValue(object component)
		{
			return true;
		}

		public override object GetValue(object component)
		{
			switch (this.Name)
			{
				case "ComparisonOperator":
					return ((ConditionInfo)component).ComparisonOperator;
				case "Group":
					return ((ConditionInfo)component).Group;
				case "Value":
					return ((ConditionInfo)component).Value;
				case "ErrorDescription":
					return ((ConditionInfo)component).ErrorDescription;
			}

			return null;
		}

		public override void ResetValue(object component)
		{
		}

		public override void SetValue(object component, object value)
		{
			switch (this.Name)
			{
				case "ComparisonOperator":
					((ConditionInfo)component).ComparisonOperator	= (ComparisonOperator)value;
					break;
				case "Group":
					((ConditionInfo)component).Group				= (ConditionInfoGroup)value;
					break;
				case "Value":
					((ConditionInfo)component).Value				= value;
					break;
			}
		}

		public override bool ShouldSerializeValue(object component)
		{
			return true;
		}

		public override Type ComponentType
		{
			get	{ return typeof(ConditionInfo); }
		}

		public override bool IsReadOnly
		{
			get 
			{
				switch (this.Name)
				{
					case "ComparisonOperator":
					case "Group":
					case "Value":
						return false;
					case "ErrorDescription":
						return true;
				}

				return false;
			}
		}

		public override Type PropertyType
		{
			get
			{
				switch (this.Name)
				{
					case "ComparisonOperator":
						return typeof(ComparisonOperator);
					case "Group":
						return typeof(ConditionInfoGroup);
					case "Value":
						return typeof(object);
					case "ErrorDescription":
						return typeof(string);
				}

				return typeof(object);
			}
		}
	}

		#endregion //CustomPropertyDescriptor for the ConditionInfoCollection ITypedList implementation

	#endregion //ConditionInfoCollection Internal Class

	#region ConditionInfo Internal Class

	internal class ConditionInfo : PropertyChangeNotifier
	{
		#region Member Variables

		private ConditionInfoGroup			_group;
		private ComparisonOperator			_comparisonOperator;
		private object						_value;
		private string						_errorDescription;
		// JJD 02/17/12 - TFS101703 - added
		private string						_displayText;

		#endregion //Member Variables

		#region Constructor

		internal ConditionInfo()
		{
		}

		// JJD 02/17/12 - TFS101703 - added displayText parameter
		//internal ConditionInfo(ConditionInfoGroup group, ComparisonOperator comparisonOperator, object value)
		internal ConditionInfo(ConditionInfoGroup group, ComparisonOperator comparisonOperator, object value, string displayText)
		{
			this._group						= group;
			this._comparisonOperator		= comparisonOperator;
			this._value						= value;

			// JJD 02/17/12 - TFS101703 - added displayText parameter
			this._displayText				= displayText;
		}

		#endregion //Constructor

		#region Properties

			#region Public Properties

				#region ComparisonOperator

		public ComparisonOperator ComparisonOperator
		{
			get { return this._comparisonOperator; }
			set
			{
				this._comparisonOperator = value;
				this.RaisePropertyChangedEvent("ComparisonOperator");

				// JJD 10/07/10 - TFS37236
				// If the value is a ComparisonCondition with an incompatible operator
				// then  clear the Value property
				ComparisonCondition cc = this._value as ComparisonCondition;
				if (cc != null && !RecordFilter.IsOperatorCompatibleWithCondition( cc, _comparisonOperator))
					this.Value = null;
			}
		}

				#endregion //ComparisonOperator	

				#region Group

		public ConditionInfoGroup Group
		{
			get { return this._group; }
			set
			{
				this._group = value;
				this.RaisePropertyChangedEvent("Group");
			}
		}

				#endregion //Group

				#region Value

		public object Value
		{
			get { return this._value; }
			set
			{
				this._value = value;
				this.RaisePropertyChangedEvent("Value");

				// JJD 10/07/10 - TFS37236
				// If the value is a ComparisonCondition with an incompatible operator
				// then set the Operator to the condition's operator
				ComparisonCondition cc = this._value as ComparisonCondition;
				if (cc != null && !RecordFilter.IsOperatorCompatibleWithCondition(cc, _comparisonOperator))
				{
					// JJD 02/17/12 - TFS101703 
					// Special case ComparisonCondition with explicit display text. For this case we only  
					// want to use the 'Equals' operator.
					if (!string.IsNullOrEmpty(cc.DisplayText))
						this.ComparisonOperator = Controls.ComparisonOperator.Equals;
					else
						this.ComparisonOperator = cc.Operator;
				}
			}
		}

				#endregion //Value	
    
				// JJD 02/17/12 - TFS101703 - added
				#region DisplayText

		public string DisplayText
		{
			get { return this._displayText; }
		}

				#endregion //DisplayText
    
				#region ErrorDescription

		public string ErrorDescription
		{
			get { return this._errorDescription; }
		}

				#endregion //ErrorDescription

			#endregion //Public Properties

			#region Internal Properties

				#region IsFirstInGroup

		internal bool IsFirstInGroup
		{
			get
			{
				if (this.Group != null && this.Group.ChildConditions.Count > 0 && this.Group.ChildConditions.Contains(this))
					return this.Group.ChildConditions.IndexOf(this) == 0;

				return false;
			}
		}

				#endregion //IsFirstInGroup	
    
				#region IsLastInGroup

		internal bool IsLastInGroup
		{
			get
			{
				if (this.Group != null && this.Group.ChildConditions.Count > 0 && this.Group.ChildConditions.Contains(this))
					return this.Group.ChildConditions.IndexOf(this) == this.Group.ChildConditions.Count - 1;

				return false;
			}
		}

				#endregion //IsLastInGroup	
    
				#region IsSelected

		internal bool IsSelected { get; set; }

				#endregion //IsSelected

			#endregion //Internal Properties

		#endregion //Properties

		#region Methods

			#region SetErrorDescription

		internal void SetErrorDescription(string errorDescription)
		{
			this._errorDescription = errorDescription;
			this.RaisePropertyChangedEvent("ErrorDescription");
		}

			#endregion //SetErrorDescription

		#endregion //Methods

		#region Base Class Overrides

		public override string ToString()
		{
			// JJD 02/17/12 - TFS101703 
			// If it is a Comparison condition with DisplayText then return that.
			if (!string.IsNullOrEmpty(_displayText))
				return _displayText;

			// JM 06-24-11 TFS79644
			//return string.Format("ComparisonOperator: {0}, Value: {1}, Group: <<<{5}>>>", new object [] {ComparisonOperator.ToString(), Value.ToString(), Group.ToString()});
			// AS 2/7/12 TFS101122
			// Removed the ToString on the Value since the Value can be null. The Format will automatically call ToString
			// on the value as needed so it's not needed anyway.
			//
			//return string.Format("ComparisonOperator: {0}, Value: {1}, Group: <<<{2}>>>", new object[] { ComparisonOperator.ToString(), Value.ToString(), Group.ToString() });
			return string.Format("ComparisonOperator: {0}, Value: {1}, Group: <<<{2}>>>", new object[] { ComparisonOperator.ToString(), Value, Group.ToString() });
		}

		#endregion //Base Class Overrides
	}

	#endregion //ConditionInfo Internal Class

	#region ConditionInfoGroup Internal Class

	internal class ConditionInfoGroup : ConditionInfo
	{
		#region Member Variables

		private ConditionInfoCollection					_childConditions;

		#endregion //Member Variables

		#region Constructors

		internal ConditionInfoGroup(ConditionInfoGroup parentGroup, LogicalOperator logicalOperator)
		{
			this.Group				= parentGroup;
			this.LogicalOperator	= logicalOperator;
		}

		#endregion //Constructors

		#region Base Class Overrides

		public override string ToString()
		{
			return string.Format("Level: {0}, LogicalOperator: {1}, ParentGroup: <<<{2}>>>, ChildConditionCount: {3}", new object[] { Level.ToString(), LogicalOperator.ToString(), Group == null ? "??" : Group.ToString(), ChildConditions.Count.ToString() });
		}

		#endregion //Base Class Overrides

		#region Properties

		internal int Level
		{
			get
			{
				int					level		=	0;
				ConditionInfoGroup	parentGroup = this.Group;

				while (parentGroup != null)
				{
					level++;
					parentGroup = parentGroup.Group;
				}

				return level;
			}
		}

		internal LogicalOperator LogicalOperator
		{
			get; set;
		}

		internal ConditionInfoCollection ChildConditions
		{
			get
			{
				if (this._childConditions == null)
				{
					this._childConditions					= new ConditionInfoCollection(new List<ConditionInfo>(5));
					this._childConditions.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(OnChildConditionsCollectionChanged);
				}

				return this._childConditions;
			}
		}

		internal bool ContainsOneGroupAndNoConditions
		{
			get
			{
				int		totalContainedGroups	= 0;
				bool	containsCondition		= false;

				foreach (ConditionInfo conditionInfo in this.ChildConditions)
				{
					if (conditionInfo is ConditionInfoGroup)
						totalContainedGroups++;
					else
					{
						containsCondition = true;
						break;
					}
				}

				return totalContainedGroups == 1 && containsCondition == false;
			}
		}

		#endregion //Properties

		#region Methods

			#region GetParentGroupAtLevel

		internal ConditionInfoGroup GetParentGroupAtLevel(int level)
		{
			ConditionInfoGroup parentGroup = this.Group;

			while (parentGroup != null)
			{
				if (parentGroup.Level == level)
					return parentGroup;

				parentGroup = parentGroup.Group;
			}

			return null;
		}

			#endregion //GetParentGroupAtLevel

			#region OnChildConditionsCollectionChanged

		private void OnChildConditionsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
					foreach (ConditionInfo conditionInfo in e.NewItems)
					{
						conditionInfo.Group = this;
					}

					break;
				case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
					foreach (ConditionInfo conditionInfo in e.OldItems)
					{
						conditionInfo.Group = null;
					}

					break;
				case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
					foreach (ConditionInfo conditionInfo in e.OldItems)
					{
						conditionInfo.Group = this;
					}

					break;
			}
		}

			#endregion //OnChildConditionsCollectionChanged	
    
		#endregion //Methods
	}

	#endregion //ConditionInfoGroup Internal Class

	#region GroupNumberManager Internal Class

	internal class GroupNumberManager
	{
		private int				_nextGroupNumber;

		internal GroupNumberManager()
		{
		}

		internal int NextGroupNumber
		{
			get { return this._nextGroupNumber++; }
		}
	}

	#endregion //GroupNumberManager Internal Class

	#region RecordVisibleIndexComparer Class

	internal class RecordVisibleIndexComparer : IComparer<Record>
	{
		#region IComparer<Record> Members

		int IComparer<Record>.Compare(Record x, Record y)
		{
			return x.VisibleIndex - y.VisibleIndex;
		}

		#endregion
	}

	#endregion //RecordVisibleIndexComparer Class	
    

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