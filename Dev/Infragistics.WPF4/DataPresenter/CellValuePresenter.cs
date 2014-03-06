using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Shapes;
using System.Windows.Navigation;
using System.Windows.Media;
//using System.Windows.Events;
using System.Windows.Media.Animation;
using System.Windows.Automation;
using System.Windows.Automation.Provider;
using System.Windows.Markup;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.Text;
using System.Windows.Input;
using System.Windows.Data;
using Infragistics.Shared;
using Infragistics.Windows.Helpers;
using Infragistics.Windows.Resizing;
using Infragistics.Windows.Selection;
using Infragistics.Windows.Editors;
using Infragistics.Windows.Controls;
using Infragistics.Windows.Editors.Events;
using Infragistics.Windows.Automation.Peers.DataPresenter;
using System.Windows.Automation.Peers;
using Infragistics.Windows.Commands;
using Infragistics.Windows.Internal;
using System.Collections.Generic;

namespace Infragistics.Windows.DataPresenter
{
	#region CellValuePresenter

	/// <summary>
	/// An element used to display the value of a Cell.
	/// </summary>
	/// <remarks>
	/// <p class="body">Refer to the <a href="xamData_Terms_Fields.html">Fields</a> topic in the Developer's Guide for an explanation of fields.</p>
	/// <p class="body">Refer to the <a href="xamData_TheoryOfOperation.html">Theory of Operation</a> topic in the Developer's Guide for an explanation of how this object is used.</p>
	/// </remarks>
	/// <seealso cref="Infragistics.Windows.DataPresenter.Cell"/>
	/// <seealso cref="Infragistics.Windows.DataPresenter.Field"/>
	/// <seealso cref="LabelPresenter"/>
	//[Description("An element used in the visual tree of the CellPresenter control to display the value associated with the cell.")]

    // JJD 4/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
    [TemplateVisualState(Name = VisualStateUtilities.StateActive,          GroupName = VisualStateUtilities.GroupActive)]
    [TemplateVisualState(Name = VisualStateUtilities.StateInactive,        GroupName = VisualStateUtilities.GroupActive)]
    
    [TemplateVisualState(Name = VisualStateUtilities.StateDragging,        GroupName = VisualStateUtilities.GroupDrag)]
    [TemplateVisualState(Name = VisualStateUtilities.StateNotDragging,     GroupName = VisualStateUtilities.GroupDrag)]

    [TemplateVisualState(Name = VisualStateUtilities.StateEditable,        GroupName = VisualStateUtilities.GroupEdit)]
    [TemplateVisualState(Name = VisualStateUtilities.StateUneditable,      GroupName = VisualStateUtilities.GroupEdit)]

    [TemplateVisualState(Name = VisualStateUtilities.StateFocused,         GroupName = VisualStateUtilities.GroupFocus)]
    [TemplateVisualState(Name = VisualStateUtilities.StateUnfocused,       GroupName = VisualStateUtilities.GroupFocus)]

    [TemplateVisualState(Name = VisualStateUtilities.StateDisplay,         GroupName = VisualStateUtilities.GroupInteraction)]
    [TemplateVisualState(Name = VisualStateUtilities.StateEditing,         GroupName = VisualStateUtilities.GroupInteraction)]

    [TemplateVisualState(Name = VisualStateUtilities.StateSelected,        GroupName = VisualStateUtilities.GroupSelection)]
    [TemplateVisualState(Name = VisualStateUtilities.StateFieldSelected,   GroupName = VisualStateUtilities.GroupSelection)]
    [TemplateVisualState(Name = VisualStateUtilities.StateUnselected,      GroupName = VisualStateUtilities.GroupSelection)]

    [StyleTypedProperty(Property = "ForegroundStyle", StyleTargetType = typeof(ContentPresenter))]	// AS 5/3/07
	[StyleTypedProperty(Property = "ForegroundActiveStyle", StyleTargetType = typeof(ContentPresenter))]	// AS 5/3/07
	[StyleTypedProperty(Property = "ForegroundAlternateStyle", StyleTargetType = typeof(ContentPresenter))]	// AS 5/3/07
	[StyleTypedProperty(Property = "ForegroundSelectedStyle", StyleTargetType = typeof(ContentPresenter))]	// AS 5/3/07
	[StyleTypedProperty(Property = "ForegroundFieldSelectedStyle", StyleTargetType = typeof(ContentPresenter))]	// AS 5/3/07
	[StyleTypedProperty(Property = "ForegroundHoverStyle", StyleTargetType = typeof(ContentPresenter))]	// AS 5/3/07
	[StyleTypedProperty(Property = "ForegroundPrimaryStyle", StyleTargetType = typeof(ContentPresenter))]	// AS 5/3/07
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class CellValuePresenter : DataItemPresenter, ISelectableElement
	{
		#region Nested Data Structures

		#region CVPValueWrapper Class

		// SSP 1/21/09 TFS12327
		// We need to be able to prevent ValueProperty's OnCoerceValue from updating 
		// the record with the new value when ValueProperty's value is being set to 
		// what's in the record to begin with. For example when we get a change 
		// notification from the data item, we will set the cvp's value to the new 
		// value however the cvp doesn't need to update the record's value in that
		// case since the record already has the correct value. Although in most 
		// cases this should not be a problem, in some cases, especially with the 
		// use of a converter, can result in infinite recursion where the data item 
		// keeps raising change notification even though the value being set is the 
		// same and in response we keep updating the ValueProperty which in turn 
		// sets the value on data item again and thus resulting in the recursion.
		// This can be due to converter always creating a new instance of custom
		// object and the custom object doesn't implement equals and thus even 
		// though the new instance created by the converter represents the same 
		// value, as far as the data source or field's SetCellValue is concerned,
		// the value has changed (since equals fails) and it will again try to
		// update the cvp value, resulting in infinite recursion.
		// 
		internal class CVPValueWrapper
		{
			internal object _value;
			internal bool _updateRecord;

			internal CVPValueWrapper( object value, bool updateRecord )
			{
				_value = value;
				_updateRecord = updateRecord;
			}
		}

		#endregion // CVPValueWrapper Class

		#endregion // Nested Data Structures

		#region Private Members

		private DataRecord _dataRecord;
		private Cell _cell;
        // AS 12/17/08 NA 2009 Vol 1 - Fixed Fields
		//private bool _individuallySized;
		private bool _initializingValue;
		private bool _isSynchronizingEditorValue;
        
        // JJD 4/30/09 - TFS17157 - added
		// JJD 3/11/11 - TFS67970 - Optimization 
		// Wired DataRecordCellArea for mouse over logic from the VirtualizingDataRecordCellPanel instead
		//private PropertyValueTracker _recordMouseOverTracker;
		//private DataRecordCellArea _recordCellArea;
		
		// JJD 3/9/11 - TFS67970 - Optimization
		// Cache the content binding
		private Binding _contentBinding;

		// JJD 3/11/11 - TFS67970 - Optimization
		// Cache a weak reference to this CellValuePresenter to be used by its associated Cell
		// This prevents heap fragmentation when this object is recycled
		private WeakReference _weakRef;		
		
		// JJD 3/11/11 - TFS67970 - Optimization
		// Cache the value wrapper
		private CVPValueWrapper _valueWrapper;

		// JJD 3/11/11 - TFS67970 - Optimization 
		// Maintain a flag so we know if CoerceValue was called
		[ThreadStatic()]
		private static bool _WasCoerceValueCalled;

		// SSP 4/29/09 NAS9.2 IDataErrorInfo Support
		// While in edit mode, do not change the visibility status of the data error in the cell
		// as the user types in the value because otherwise the error icon could constantly keep
		// changing its visibility based on the current value that the user has typed so far.
		// This would be an undesirable behavior.
		// 
		private ValidationErrorInfo _editorInvalidValueInfoOverride;
		private bool _pendingIsDataErrorTemplateActive;

		// JJD 7/05/11 - TFS80466 - added
		private bool _invalidateCVPStylePending;
		private bool _invalidateEditorStylePending;

		// JJD 04/12/12 - TFS108549 - Optimization
		// Keep a flag to detect whether the cell's value changed while the rp was de-activated
		private bool _isValueDirty;

		// MD 6/22/11 - TFS62384
		private bool _isInitializingRecord;

		#endregion //Private Members

		#region Constructors

		static CellValuePresenter()
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(CellValuePresenter), new FrameworkPropertyMetadata(typeof(CellValuePresenter)));

			// This will manage FocusWithinManager.IsFocusWithin property for this type.
			// 
			FocusWithinManager.RegisterType( typeof( CellValuePresenter ) );

            // JJD 2/7/08 - BR30444
            // Change the default ContentTemplateSelector to our instance so we 
            // can provide appropriate default temples based on data type.
            CellValuePresenter.ContentTemplateSelectorProperty.OverrideMetadata(typeof(CellValuePresenter), new FrameworkPropertyMetadata(CVPContentTemplateSelector.Instance));

			// AS 8/24/09 TFS19532
			UIElement.VisibilityProperty.OverrideMetadata(typeof(CellValuePresenter), new FrameworkPropertyMetadata(null, new CoerceValueCallback(GridUtilities.CoerceFieldElementVisibility)));

			// MD 6/22/11 - TFS62384
			EventManager.RegisterClassHandler(typeof(CellValuePresenter), FrameworkElement.RequestBringIntoViewEvent, new RequestBringIntoViewEventHandler(OnRequestBringIntoView));
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CellValuePresenter"/> class
		/// </summary>
		public CellValuePresenter()
		{
		}

		#endregion //Constructors

		#region Base class overrides

		#region AlwaysValidate

		// SSP 2/6/09 TFS10586
		// Added AlwaysValidate property to base ValuePresenter and overrode it here.
		// 
		/// <summary>
		/// Overridden. Returns true if the cell is from an add-record.
		/// </summary>
		internal protected override bool AlwaysValidate
		{
			get
			{
				DataRecord record = this.Record;

				return null != record && record.IsAddRecord;
			}
		}

		#endregion // AlwaysValidate

		#region CommitEditValue

		/// <summary>
		/// Overridden. Called after OnEditModeEnding however before OnEditModeEnded. This is 
		/// called after input validation succeeds to let the host know to commit the value.
		/// </summary>
		/// <param name="editedValue">The edited value that is to be committed.</param>
		/// <param name="stayInEditMode">Whether the editor should cancel the operation of exitting edit mode.</param>
		/// <returns>Returns true if commit succeeded, false otherwise.</returns>
		internal protected override bool CommitEditValue( object editedValue, out bool stayInEditMode )
		{
			stayInEditMode = false;

			bool success = this.CommitEditValueHelper( editedValue );
            if ( !success )
                stayInEditMode = true;

			return success;
		}

		#endregion // CommitEditValue

		// AS 5/25/07 DefaultInvalidValueBehavior
		#region DefaultInvalidValueBehavior
		/// <summary>
		/// Returns the resolved <see cref="InvalidValueBehavior"/> from the associated field's <see cref="Field.InvalidValueBehaviorResolved"/> that the editor should use when its <see cref="ValueEditor.InvalidValueBehavior"/> has not been explicitly specified.
		/// </summary>
		internal protected override InvalidValueBehavior DefaultInvalidValueBehavior
		{
			get
			{
				Field field = this.Field;

				return null != field ? field.InvalidValueBehaviorResolved : InvalidValueBehavior.Default;
			}
		}
		#endregion //DefaultInvalidValueBehavior

		#region IsEditingAllowed

		/// <summary>
		/// Returns true is editing is allowed
		/// </summary>
		public override bool IsEditingAllowed
		{
			get
			{
				Field fld = this.Field;
				DataRecord rcd = this.Record;

				
#region Infragistics Source Cleanup (Region)









#endregion // Infragistics Source Cleanup (Region)

                
#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)

                return null != fld && fld.GetIsEditingAllowed(rcd);
            }
		}

		#endregion //IsEditingAllowed

        #region OnApplyTemplate

		// JJD 3/11/11 - TFS67970 - Optimization 
		// Wired DataRecordCellArea for mouse over logic from the VirtualizingDataRecordCellPanel instead
		///// <summary>
		///// Called after the template has been applied
		///// </summary>
		//public override void OnApplyTemplate()
		//{
		//    base.OnApplyTemplate();

		//    // JJD 4/30/09 - TFS17157 
		//    // Added support for IsMouseOverRecord property
		//    this.WireCellArea();
		//}

        #endregion //OnApplyTemplate	
    
		#region OnContentChanged
		/// <summary>
		/// Overridden. Invoked when the <see cref="ContentControl.Content"/> has changed.
		/// </summary>
		/// <param name="oldContent">The old content</param>
		/// <param name="newContent">The new content</param>
		protected override void OnContentChanged(object oldContent, object newContent)
		{
			// AS 3/16/07
			// The base OnContentChanged tries to make the new content a logical child
			// even though it already has one. It only skips this if this instance
			// has a templated child. With cell virtualization, the cellvaluepresenter 
			// is no longer created via a template and therefore does not have a 
			// templated parent.
			//
			if (newContent != null &&
				newContent is DependencyObject &&
				LogicalTreeHelper.GetParent((DependencyObject)newContent) != null)
			{
				this.RemoveLogicalChild(oldContent);
				return;
			}

			base.OnContentChanged(oldContent, newContent);
		}
		#endregion //OnContentChanged

		#region OnCreateAutomationPeer
		/// <summary>
		/// Returns an automation peer that exposes the <see cref="CellValuePresenter"/> to UI Automation.
		/// </summary>
		/// <returns>A <see cref="Infragistics.Windows.Automation.Peers.DataPresenter.CellValuePresenterAutomationPeer"/></returns>
		protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
		{
			return new Infragistics.Windows.Automation.Peers.DataPresenter.CellValuePresenterAutomationPeer(this);
		}
		#endregion //OnCreateAutomationPeer

		#region OnEditModeStarting

		/// <summary>
		/// Called when the embedded editor is about to enter edit mode
		/// </summary>
		/// <param name="e">The event arguments</param>
		/// <remarks>Setting the <see cref="EditModeStartingEventArgs.Cancel"/> property to true will prevent the editor from entering edit mode.</remarks>
		/// <seealso cref="ValueEditor.EditModeStarting"/>
		/// <seealso cref="OnEditModeStarted"/>
		/// <seealso cref="OnEditModeEnding"/>
		/// <seealso cref="OnEditModeEnded"/>
		protected override void OnEditModeStarting(EditModeStartingEventArgs e)
		{
			DataPresenterBase dp = this.DataPresenter;
			ValueEditor editor = this.Editor;

			Debug.Assert(dp != null);
			Debug.Assert(editor != null);

			// Make sure editing is allowed on this cell. 
			// If not then cancel the operation 
			if (dp == null ||
				editor == null ||
				!this.IsEditingAllowed ||
				!this.IsEnabled)
			{
				e.Cancel = true;
				return;
			}

			Cell cell = this.Cell;

			Debug.Assert(cell != null);

			// If there is no cell then cancel out 
			if (cell == null)
			{
				e.Cancel = true;
				return;
			}

			// clear the existing selection
			dp.SelectedItems.ClearAllSelected();

			// if the clear was cancelled then cancel this operation
			if ( dp.SelectedItems.HasSelection )
			{
				e.Cancel = true;
				return;
			}

			// If the cell isn't active then activate it 
			if (!cell.IsActive)
			{
				cell.IsActive = true;

				// Make sure the cell is active. 
				// If not then cancel the operation 
				if (!cell.IsActive)
				{
					e.Cancel = true;
					return;
				}
			}

			Infragistics.Windows.DataPresenter.Events.EditModeStartingEventArgs args = new Infragistics.Windows.DataPresenter.Events.EditModeStartingEventArgs(cell, editor);

			// copy over the Cancel in case the editor decided to raise this event with Cancel
			// defaulting to true.
			args.Cancel = e.Cancel;

			// raise the corresponding event on the DataPresenterBase
			dp.RaiseEditModeStarting(args);

			// propagate the cancel to the passed in event args
			if (args.Cancel == true)
			{
				e.Cancel = true;
				return;
			}

			cell.AssociatedCellValuePresenterInternal = this;

			dp.BringCellIntoView(cell);

			// AS 7/27/09 NA 2009.2 Field Sizing
			//dp.CellValuePresenterInEdit = this;
			dp.EditHelper.OnEnterEditMode(this);

			// SSP 6/18/09 NAS9.2 IDataErrorInfo Support
			// Also enable the data error template when entering edit mode before the editor 
			// takes focus otherwise changing the content template after the focus is taken
			// by the editor may cause framework to shift focus to somewhere else, which
			// is what happens when the data presenter is inside a popup.
			// 
			this.UpdateDataError( );
		}

		#endregion //OnEditModeStarting

		#region OnEditModeStarted

		/// <summary>
		/// Called when the embedded editor has just entered edit mode
		/// </summary>
		/// <param name="e">The event arguments</param>
		/// <seealso cref="ValueEditor.EditModeStarted"/>
		/// <seealso cref="OnEditModeStarting"/>
		/// <seealso cref="OnEditModeEnding"/>
		/// <seealso cref="OnEditModeEnded"/>
		protected override void OnEditModeStarted(EditModeStartedEventArgs e)
		{
			DataPresenterBase dp = this.DataPresenter;
			ValueEditor editor = this.Editor;
			Cell cell = this.Cell;

			Debug.Assert(dp != null);
			Debug.Assert(editor != null);
			Debug.Assert(cell != null);

			if (dp == null || editor == null || cell == null)
				return;

			if ( editor is ISupportsSelectableText )
				( (ISupportsSelectableText)editor ).SelectAll( );

			// SSP 4/29/09 NAS9.2 IDataErrorInfo Support
			// 
			_editorInvalidValueInfoOverride = editor.InvalidValueErrorInfo;
			this.UpdateDataError( );

			Infragistics.Windows.DataPresenter.Events.EditModeStartedEventArgs args = new Infragistics.Windows.DataPresenter.Events.EditModeStartedEventArgs(cell, this.Editor);

			// raise the corresponded event on the DataPresenterBase
			dp.RaiseEditModeStarted(args);


            // JJD 4/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
            this.UpdateVisualStates();

        }

		#endregion //OnEditModeStarted

		#region OnEditModeEnding

		/// <summary>
		/// Called when the embedded editor is about to exit edit mode
		/// </summary>
		/// <param name="e">The event arguments</param>
		/// <remarks>
		/// <para>
		/// Setting the <see cref="EditModeEndingEventArgs.Cancel"/> property to true will prevent the editor from exiting edit mode.
		/// </para>
		/// <para></para>
		/// <para>However, if the <see cref="EditModeEndingEventArgs.Force"/> read-only property is true the action is not cancellable and setting Cancel to true will raise an exception.</para>
		/// </remarks>
		/// <seealso cref="ValueEditor.EditModeEnding"/>
		/// <seealso cref="OnEditModeEnded"/>
		/// <seealso cref="OnEditModeStarting"/>
		/// <seealso cref="OnEditModeStarted"/>
		protected override void OnEditModeEnding(EditModeEndingEventArgs e)
		{
			DataPresenterBase dp = this.DataPresenter;
			ValueEditor editor = this.Editor;
			Cell cell = this.Cell;

			Debug.Assert(dp != null);
			Debug.Assert(editor != null);
			Debug.Assert(cell != null);

			if (dp == null || editor == null || cell == null)
				return;

			Infragistics.Windows.DataPresenter.Events.EditModeEndingEventArgs args 
				= new Infragistics.Windows.DataPresenter.Events.EditModeEndingEventArgs( cell, editor, e.AcceptChanges, e.Force );

			// copy over the Cancel in case the editor decided to raise this event with Cancel
			// defaulting to true.
			args.Cancel = e.Cancel;

			// raise the corresponding event on the DataPresenterBase
			dp.RaiseEditModeEnding(args);

			// copy over the modifiable fields on the args
			e.AcceptChanges = args.AcceptChanges;
			e.Cancel = args.Cancel;

			// if cancelled return
			if (args.Cancel == true)
				return;

			cell.AssociatedCellValuePresenterInternal = this;
		}

		#endregion //OnEditModeEnding

		#region OnEditModeEnded

		/// <summary>
		/// Called when the embedded editor has just exited edit mode
		/// </summary>
		/// <param name="e">The event arguments</param>
		/// <seealso cref="ValueEditor.EditModeEnded"/>
		/// <seealso cref="OnEditModeEnding"/>
		/// <seealso cref="OnEditModeStarting"/>
		/// <seealso cref="OnEditModeStarted"/>
		protected override void OnEditModeEnded(EditModeEndedEventArgs e)
		{
			DataPresenterBase dp = this.DataPresenter;
			ValueEditor editor = this.Editor;
			Cell cell = this.Cell;

			Debug.Assert(dp != null);
			Debug.Assert(editor != null);
			Debug.Assert(cell != null);

			// AS 7/27/09 NA 2009.2 Field Sizing
			//if ( null != dp && dp.CellValuePresenterInEdit == this )
			//	dp.CellValuePresenterInEdit = null;
			if (null != dp)
				dp.EditHelper.OnEndEditMode(this, e.ChangesAccepted);

			if (dp == null || editor == null || cell == null)
				return;
 
            // JJD 11/03/08 - TFS9557
            // Re-sync the editor's value in case the data object changed its value on the set
            editor.Value = this.Value;

			Infragistics.Windows.DataPresenter.Events.EditModeEndedEventArgs args 
				= new Infragistics.Windows.DataPresenter.Events.EditModeEndedEventArgs(cell, editor, e.ChangesAccepted);

			// raise the corresponded event on the DataPresenterBase
			dp.RaiseEditModeEnded(args);

			// JJD 7/05/11 - TFS80466 
			// If the field has a style selector specified for the CVP or editor styles
			// we should invalidate them because the value has changed
			if (e.ChangesAccepted)
				this.ReinitalizeIfStyleSelectorsSpecified();

			// SSP 4/29/09 NAS9.2 IDataErrorInfo Support
			// 
			_editorInvalidValueInfoOverride = null;
			this.UpdateDataError( );


            // JJD 4/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
            this.UpdateVisualStates();

        }

		#endregion //OnEditModeEnded

		#region OnEditModeValidationError

		/// <summary>
		/// Overridden. Called when the embedded editor has input validation error.
		/// </summary>
		/// <param name="e">The event arguments</param>
		/// <seealso cref="DataPresenterBase.EditModeValidationError"/>
		protected override void OnEditModeValidationError( EditModeValidationErrorEventArgs e )
		{
			DataPresenterBase dp = this.DataPresenter;
			Cell cell = this.Cell;

			if ( null == dp || null == cell )
				return;

			Infragistics.Windows.DataPresenter.Events.EditModeValidationErrorEventArgs args 
				= new Infragistics.Windows.DataPresenter.Events.EditModeValidationErrorEventArgs(
						cell, e.Editor, e.ForceExitEditMode, e.Exception, e.ErrorMessage );

			dp.RaiseEditModeValidationError( args );

			e.ErrorMessage = args.ErrorMessage;

			// SSP 5/12/11 TFS42838
			// Don't override the editor's settings if the field doesn't have any settings.
			// 
			//e.InvalidValueBehavior = args.InvalidValueBehavior;
			if ( InvalidValueBehavior.Default != args.InvalidValueBehavior )
				e.InvalidValueBehavior = args.InvalidValueBehavior;

			// SSP 4/29/09 NAS9.2 IDataErrorInfo Support
			// While in edit mode, we do not change the visibility of the data error icon as the 
			// user types. However when the user attempts the leave the editor with an invalid
			// value, we need to show the error icon at that point.
			// 
			_editorInvalidValueInfoOverride = e.Editor.InvalidValueErrorInfo;
			this.UpdateDataError( );
		}

		#endregion // OnEditModeValidationError

		#region OnEditorCreated

		/// <summary>
		/// Called after the editor has been created but before its Content is set, its Style has been applied and before it has been sited.
		/// </summary>
		/// <param name="editor">The ValueEditor that was just created</param>
		protected override void OnEditorCreated(ValueEditor editor)
		{
			base.OnEditorCreated(editor);

			Field field = this.Field;

			Debug.Assert(field != null);

			if (field == null)
				return;
            
            // JJD 11/05/08 - TFS6094/BR33963
            // When the lablelocation is 'inCells' check to see if they explicitly
            // set the CellContentAlignment to '...Left', '...Right' or '...Center'.
            // If so, then set the editor's HorizonalContentAlignment to its
            // corresponding value.
            FieldLayout fl = field.Owner;
            if (fl != null && fl.LabelLocationResolved == LabelLocation.InCells)
            {
                CellContentAlignment contentAlignment = field.CellContentAlignmentResolved;

                HorizontalAlignment? hAlignment = null;

                switch (contentAlignment)
                {
                    case CellContentAlignment.LabelAboveValueAlignLeft:
                    case CellContentAlignment.LabelBelowValueAlignLeft:
                        hAlignment = HorizontalAlignment.Left;
                        break;
                    case CellContentAlignment.LabelAboveValueAlignCenter:
                    case CellContentAlignment.LabelBelowValueAlignCenter:
                        hAlignment = HorizontalAlignment.Center;
                        break;
                    case CellContentAlignment.LabelAboveValueAlignRight:
                    case CellContentAlignment.LabelBelowValueAlignRight:
                        hAlignment = HorizontalAlignment.Right;
                        break;
                }

                if (hAlignment.HasValue)
                    editor.HorizontalContentAlignment = hAlignment.Value;
            }

			
#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)

		}

		#endregion //OnEditorCreated	

		#region OnFieldPropertyChanged

		/// <summary>
		/// Called when a property on the associated <see cref="Field"/> object changes
		/// </summary>
		/// <param name="e"></param>
		protected override void OnFieldPropertyChanged(PropertyChangedEventArgs e)
		{
            // AS 12/5/08 NA 2009 Vol 1 - Fixed Fields
            // The base class may be looking at this ... and now it is. 
            //
            base.OnFieldPropertyChanged(e);

			if (this is ExpandedCellPresenter)
				return;

			// JJD 3/9/11 - TFS67970 - Optimization
			// Replaced bindings to field with code in OnFieldPropertyChanged
			switch (e.PropertyName)
			{
				// JJD 4/29/11 - TFS74075
				// Respond to changes to the CellValuePresenterStyleResolved
				case "CellValuePresenterStyleResolved":
					this.InvalidateStyleSelector();
					break;

				case "CellContentAlignmentResolved":
					if (this.IsWithinRecord )
						this.SetValue(GridUtilities.CellContentAlignmentProperty, this.Field.CellContentAlignmentResolved);
					break;

				// JJD 4/29/11 - TFS74075
				// Re-initialize the editor if its type or style have changed
				case "EditorStyleResolved":
				case "EditorTypeResolved":
					this.InitializeEditor(this.Value);
					break;

				case "IsSelected":
					if (this.IsWithinRecord && this.Field.IsSelected)
						this.SetValue(IsFieldSelectedProperty, KnownBoxes.TrueBox);
					else
						this.ClearValue(IsFieldSelectedProperty);
					break;
				case "SortStatus":
					this.SetValue(SortStatusInternalProperty, this.Field.SortStatus);
					break;
				case "VisibilityResolved":
					if (this.IsWithinRecord )
						this.SetValue(GridUtilities.FieldVisibilityProperty, this.Field.VisibilityResolved);
					break;
			}

            
#region Infragistics Source Cleanup (Region)





















#endregion // Infragistics Source Cleanup (Region)

		}

		#endregion //OnFieldPropertyChanged

		#region OnMouseLeftButtonDown

		/// <summary>
		/// Called when the left mouse buton is pressed
		/// </summary>
		/// <param name="e"></param>
		protected override void OnMouseLeftButtonDown( MouseButtonEventArgs e )
		{
			base.OnMouseLeftButtonDown( e );

			if ( e.Handled == true )
				return;

			DataPresenterBase dp = this.DataPresenter;

			// If any of the modifier keys is pressed down then don't go into edit mode.
			// We need to allow selection logic to work.
			if ( this.IsModifierKeyDown )
				return;

			// if we are already in edit mode then just eat the message
			if ( this.IsInEditMode )
			{
				e.Handled = true;
				return;
			}
		}

		#endregion //OnMouseLeftButtonDown

		#region OnPreviewMouseDoubleClick

		
		
		
		
		
		/// <summary>
		/// Overridden.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnPreviewMouseDoubleClick( MouseButtonEventArgs e )
		{
			base.OnPreviewMouseDoubleClick( e );

			if ( e.Handled )
				return;

			if ( this.IsModifierKeyDown )
				return;

			ValueEditor editor = this.Editor;
			Field field = this.Field;
			DataRecord record = this.Record;
			if ( null == editor || null == field || null == record || editor.IsInEditMode )
				return;

			// If cell click action is to SelectCell or SelectRow and the user double clicks on the cell
			// then go into edit mode.
			// 			
			if ( this.IsEditingAllowed
				// SSP 9/9/09 TFS19158
				// Added GetCellClickActionResolved virtual method on the DataRecord.
				// 
				//&& CellClickAction.EnterEditModeIfAllowed != field.CellClickActionResolved 
				&& CellClickAction.EnterEditModeIfAllowed != record.GetCellClickActionResolved( field )
				)
				this.ActivateAndEnterEditMode( );
		}

		#endregion // OnPreviewMouseDoubleClick

		#region OnPropertyChanged

		/// <summary>
		/// Called when a property has been changed
		/// </summary>
		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);

			if (e.Property == CellValuePresenter.SortStatusInternalProperty)
			{
				this.SetValue(SortStatusPropertyKey, this.SortStatusInternal);
			}
			else if (e.Property == CellValuePresenter.FieldProperty)
			{
				// JJD 5/25/07 
				// Refactored logic to prevent unnecessary overhead on field change
				//this.InitializeField();
				this.InitializeField(e.NewValue as Field, e.OldValue as Field);
			}
			else if (e.Property == DataContextProperty)
			{
				// JJD 5/25/07 
				// Refactored logic to prevent unnecessary overhead on field change

				//// JJD 3/08/07
				//// added support for reusing these elements for different records
				//if ( e.OldValue != null )
				//    this.UnhookAndClearRecord();

				//if (e.NewValue != null)
				//{
				//    if (this.Field != null)
				//        this.InitializeField();
				//}
				this.InitializeRecord(e.NewValue as DataRecord, e.OldValue as DataRecord);
				
				// JJD 4/13/07
				// This is no longer necessary becuase we are binding the UnboundCell's Value instead
				//this.InitializeUnboundFieldBinding();
			}
			else if ( e.Property == FocusWithinManager.IsFocusWithinProperty )
			{
				this.OnIsFocusWithinChanged( (bool)e.NewValue );
			}
			else if (e.Property == FrameworkElement.IsVisibleProperty)
			{
				// JJD 04/12/12 - TFS108549 - Optimization
				// If we are being made visible and the isValueDirty flag
				// then cear the flag and call SetValueInternal to synchrpnize
				// with the new value
				if (_isValueDirty && (bool)e.NewValue)
				{
					_isValueDirty = false;

					if (_dataRecord != null)
					{
						Field field = this.Field;

						if (field != null)
							this.SetValueInternal(_dataRecord.GetCellValue(field, CellValueType.EditAsType), false);
					}
				}
			}
		}

		#endregion //OnPropertyChanged
    
		#region OnRecordPropertyChanged

		/// <summary>
		/// Called when a property on the associated <see cref="Record"/> object changes
		/// </summary>
		/// <param name="e"></param>
		protected override void OnRecordPropertyChanged(PropertyChangedEventArgs e)
		{
			if (this._dataRecord == null)
				return;

			switch (e.PropertyName)
			{
				case "ActiveCell":
					this.SetValue(IsActiveProperty, KnownBoxes.FromValue(this.IsActive));
					break;

				case "CellSelection":
					this.SetValue(IsSelectedProperty, KnownBoxes.FromValue(this.IsSelected));
					break;

				case "IsSelected":
                    // JJD 4/30/09 - TFS17157 
                    // set the IsRecordSelectedPropertyKey 
                    if (this._dataRecord.IsSelected)
                        this.SetValue(IsRecordSelectedPropertyKey, KnownBoxes.TrueBox);
                    else
                        this.ClearValue(IsRecordSelectedPropertyKey);
                    break;

                





				// AS 10/27/09 NA 2010.1 - CardView
				case "ShouldCollapseEmptyCells":
					this.CoerceValue(VisibilityProperty);
					break;

			}
		}

		#endregion //OnRecordPropertyChanged

		// JJD 7/05/11 - TFS80466 - added
		#region OnStyleSelectorInvalidated

		internal override void OnStyleSelectorInvalidated()
		{
			this._invalidateCVPStylePending = false;
		}

		#endregion //OnStyleSelectorInvalidated	
    
		#region OnValueChanged

		/// <summary>
		/// Called when the value has changed in the editor
		/// </summary>
		internal protected override void OnValueChanged()
		{
			base.OnValueChanged();

			// JJD 7/05/11 - TFS80466 - optimization
			// Get editor property once
			//if (this._isSynchronizingEditorValue == false &&
			//    this.Editor != null &&
			//    this.Editor.IsInEditMode &&
			//    this.Cell.IsActive)
			if (this._isSynchronizingEditorValue == false)
			{
				ValueEditor editor = this.Editor;

				if (editor != null &&
					editor.IsInEditMode &&
					this.Cell.IsActive)
				{
					// SSP 5/23/07
					// Added cellValuePresenter parameter to the method.
					// 
					//this.Cell.OnEditValueChanged();
					this.Cell.OnEditValueChanged(this);

					DataPresenterBase dp = this.DataPresenter;

					// AS 7/27/09 NA 2009.2 Field Sizing
					if (null != dp)
						dp.EditHelper.OnEditValueChanged(this);
				}
			}
		}

		#endregion //OnValueChanged	

		#region OnValueValidated

		// SSP 4/17/09 NAS9.2 IDataErrorInfo Support
		// Added ValidationErrorInfo class.
		// 
		/// <summary>
		/// Overridden. Called whenever the value editor validates the value.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>Note</b> that this method is called whenever the value editor validates 
		/// the value to be valid or invalid.
		/// </para>
		/// </remarks>
		/// <seealso cref="ValueEditor.IsValueValid"/>
		/// <seealso cref="ValueEditor.InvalidValueErrorInfo"/>
		[InfragisticsFeature( FeatureName = FeatureInfo.FeatureName_IDataErrorInfo, Version = FeatureInfo.Version_9_2 )]
		internal protected override void OnValueValidated( )
		{
			base.OnValueValidated( );
			this.UpdateDataError( );
		}

		#endregion // OnValueValidated

        #region OnVisualParentChanged

		// JJD 3/11/11 - TFS67970 - Optimization 
		// Wired DataRecordCellArea for mouse over logic from the VirtualizingDataRecordCellPanel instead
		///// <summary>
		///// Called when the visual parent has changed.
		///// </summary>
		///// <param name="oldParent"></param>
		//protected override void OnVisualParentChanged(DependencyObject oldParent)
		//{
		//    base.OnVisualParentChanged(oldParent);

		//    // JJD 4/30/09 - TFS17157 
		//    // Added support for IsMouseOverRecord property
		//    this.WireCellArea();
		//}

        #endregion //OnVisualParentChanged	
    
		#region ProcessPreviewMouseLeftButtonDown

		/// <summary>
		/// Overridden.
		/// </summary>
		/// <param name="e"></param>
		protected override void ProcessPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			base.ProcessPreviewMouseLeftButtonDown( e );

			if ( e.Handled == true )
				return;

			DataPresenterBase dp = this.DataPresenter;

			// If any of the modifier keys is pressed down then don't go into edit mode.
			// We need to allow selection logic to work.
			if ( this.IsModifierKeyDown )
				return;

			// if we are already in edit mode then just return
			if ( this.IsInEditMode )
			{
				return;
			}

			// If the CellClickAction is not EnterEditMode then return and let the selection strategy
			// handle selection/activation etc.
			// SSP 9/9/09 TFS19158
			// Added GetCellClickActionResolved virtual method on the DataRecord.
			// 
			//if ( this.Field.CellClickActionResolved != CellClickAction.EnterEditModeIfAllowed )
			DataRecord record = this.Record;
			Field field = this.Field;
			if ( null == record || null == field 
				|| record.GetCellClickActionResolved( field ) != CellClickAction.EnterEditModeIfAllowed )
				return;

            // JJD 5/10/09 - TFS16298
            // Get the left/top point relative to the dp before we try to activate the call
			// MD 8/24/11 - TFS80785
			// If the data presenter itself, moves, the TranslatePoint will still return the same value later because
			// the cell hasn't moved relative to the top-left of the data presenter. So instead, we should check to see
			// if the cell has moved out from under the mouse.
			//Point leftTopBefore = this.TranslatePoint(new Point(), dp);
			Point mouseRelativePosition = e.GetPosition(this);

			this.ActivateAndEnterEditMode( );

			// JJD 8/24/11 - TFS8457
			// If the cell isn't active at this point it means one of the events was cancelled
			// so we need to eat the mouse event so we don't try to activate the cell again
			if (!this.IsActive)
			{
				e.Handled = true;
				return;
			}

            // JJD 5/10/09 - TFS16298
            // After the call to ActivateAndEnterEditMode, if we have been scrolled
            // in either dimension then eat the mouse message.
            // Otherwise, the mouse might be over a different cell which would cause 
            // the activation of the wrong cell/record.
			// MD 8/24/11 - TFS80785
			// See comment above.
			//if (leftTopBefore != this.TranslatePoint(new Point(), dp))
			if (mouseRelativePosition != e.GetPosition(this))
                e.Handled = true;
		}

		#endregion // ProcessPreviewMouseLeftButtonDown
    
		#region ToString

		/// <summary>
		/// Returns a string representation of the object
		/// </summary>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append("CellValuePresenter: ");

			if (this.Field != null)
			{
				sb.Append(this.Field.ToString());
				sb.Append(", ");
			}
			if (this.Record != null)
				sb.Append(this.Record.ToString());

			return sb.ToString();
		}

		#endregion //ToString

        // JJD 4/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
        #region SetVisualState


        /// <summary>
        /// Called to set the VisualStates of the editor
        /// </summary>
        /// <param name="useTransitions">Determines whether transitions should be used.</param>
        protected override void SetVisualState(bool useTransitions)
        {
            base.SetVisualState(useTransitions);

            // set active state
            if ( this.IsActive )
                VisualStateManager.GoToState(this, VisualStateUtilities.StateActive, useTransitions);
            else                    
                VisualStateManager.GoToState(this, VisualStateUtilities.StateInactive, useTransitions);

            // set drag state
            if (this.IsDragIndicator)
                VisualStateManager.GoToState(this, VisualStateUtilities.StateDragging, useTransitions);
            else
                VisualStateManager.GoToState(this, VisualStateUtilities.StateNotDragging, useTransitions);

            // set edit state
            if ( this.IsEditingAllowed && this.IsEnabled)
                VisualStateManager.GoToState(this, VisualStateUtilities.StateEditable, useTransitions);
            else                    
                VisualStateManager.GoToState(this, VisualStateUtilities.StateUneditable, useTransitions);

            // set focus state
            if ( FocusWithinManager.CheckFocusWithinHelper( this ) )
                VisualStateManager.GoToState(this, VisualStateUtilities.StateFocused, useTransitions);
            else                    
                VisualStateManager.GoToState(this, VisualStateUtilities.StateUnfocused, useTransitions);

            // set interaction state
            if ( this.IsInEditMode )
                VisualStateManager.GoToState(this, VisualStateUtilities.StateEditing, useTransitions);
            else                    
                VisualStateManager.GoToState(this, VisualStateUtilities.StateDisplay, useTransitions);

            // set selected state
            if (this.IsSelected)
                VisualStateManager.GoToState(this, VisualStateUtilities.StateSelected, useTransitions);
            else
            {
                if (this.IsFieldSelected)
                    VisualStateUtilities.GoToState(this, useTransitions, VisualStateUtilities.StateFieldSelected, VisualStateUtilities.StateSelected);
                else
                    VisualStateManager.GoToState(this, VisualStateUtilities.StateUnselected, useTransitions);
            }
        }


        #endregion //SetVisualState	

		// JJD 6/29/11 - TFS79601 - added
		#region SupportsAsyncOperations

		/// <summary>
		/// Determines if asynchronous operations are supported (read-only)
		/// </summary>
		/// <value>True if asynchronous operations are supported, otherwise false.</value>
		/// <remarks>
		/// <para class="body">This property returns false during certain operations that are synchronous in nature, e.g. during a report or export operation.</para>
		/// </remarks>
		internal protected override bool SupportsAsyncOperations 
		{ 
			get 
			{
				DataPresenterBase dp = this.DataPresenter;

				if (dp != null)
					return !dp.IsSynchronousControl;

				return true; 
			} 
		}

		#endregion //SupportsAsyncOperations	
    
		#endregion //Base class overrides

		#region Properties

		#region Public Properties

		#region DataError

		// SSP 4/13/09 NAS9.2 IDataErrorInfo Support
		// 

		/// <summary>
		/// Identifies the read-only <see cref="DataError"/> dependency property.
		/// </summary>
		[InfragisticsFeature( FeatureName = FeatureInfo.FeatureName_IDataErrorInfo, Version = FeatureInfo.Version_9_2 )]
		public static readonly DependencyProperty DataErrorProperty 
			= DataRecordPresenter.DataErrorProperty.AddOwner( typeof( CellValuePresenter ) );

		/// <summary>
		/// Returns the associated cell's data error.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// If the underlying ValueEditor that's being used to display the cell's value
		/// has a value validation error (its <see cref="ValueEditor.IsValueValid"/> set to false),
		/// then this property will return the error message explaining why the value is invalid.
		/// If the ValueEditor's IsValueValid is true, then the associated cell's <b>DataError</b> 
		/// property value is returned.
		/// </para>
		/// </remarks>
		/// <seealso cref="ValueEditor.InvalidValueErrorInfo"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.Cell.DataError"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.Cell.HasDataError"/>
		/// <seealso cref="RecordSelector.DataError"/>
		/// <seealso cref="RecordSelector.HasDataError"/>
		/// <seealso cref="DataRecordPresenter.HasDataError"/>
		/// <seealso cref="DataRecordPresenter.DataError"/>
		//[Description( "Data error associated with the cell." )]
		//[Category( "Data" )]
		[Bindable( true )]
		[Browsable( false )]
		[ReadOnly( true )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		[InfragisticsFeature( FeatureName = FeatureInfo.FeatureName_IDataErrorInfo, Version = FeatureInfo.Version_9_2 )]
		public object DataError
		{
			get
			{
				return (object)this.GetValue( DataErrorProperty );
			}
		}

		internal void UpdateDataError( )
		{
			ValueEditor editor = this.Editor;
			Field field = this.Field;
			DataRecord dataRecord = this.Record;

			bool hasDataError = false;
			object dataError = null;
			bool isDataErrorTemplateActive = false;

			// JJD 07/30/12 - TFS117982
			// Make sure the field is still valid by checking to see if its Index is not -1.
			// This can happen on a asynchronous call after clearing the fields collection.
			//bool supportDataErrorInfo = null != field && field.SupportDataErrorInfoResolved;
			bool supportDataErrorInfo = null != field && field.SupportDataErrorInfoResolved && field.Index >= 0;
			if ( supportDataErrorInfo )
			{
				if ( null != editor && ( null != _editorInvalidValueInfoOverride || !editor.IsValueValid && !editor.IsInEditMode ) )
				{
					hasDataError = true;
					ValidationErrorInfo invalidValueInfo = null != _editorInvalidValueInfoOverride
						? _editorInvalidValueInfoOverride : editor.InvalidValueErrorInfo;

					Debug.Assert( null != invalidValueInfo );
					if ( null != invalidValueInfo )
						dataError = invalidValueInfo.ErrorMessage;
				}
				else
				{
					if ( null != dataRecord && null != field )
					{
						hasDataError = dataRecord.GetCellHasDataError( field );
						if ( hasDataError )
							dataError = dataRecord.GetCellDataError( field );
					}
				}

				DataPresenterBase dp = field.DataPresenter;
				isDataErrorTemplateActive = hasDataError || null != editor && editor.IsInEditMode
					// Also enable the template when entering edit mode before the editor takes 
					// focus otherwise changing the content template after the focus is taken
					// by the editor may cause framework to shift focus to somewhere else, which
					// is what happens when the data presenter is inside a popup.
					// 
					// AS 7/27/09 NA 2009.2 Field Sizing
					//|| null != dp && this == dp.CellValuePresenterInEdit;
					|| null != dp && dp.EditHelper.IsEditCell(this);
			}

			this.SetValue( DataRecordPresenter.HasDataErrorPropertyKey, KnownBoxes.FromValue( hasDataError ) );
			this.SetValue( DataRecordPresenter.DataErrorPropertyKey, dataError );

			_pendingIsDataErrorTemplateActive = isDataErrorTemplateActive;
			
			if ( this.IsMeasureValid )
				this.SyncPendingIsDataErrorTemplateActive( );

			RecordSelector.UpdateDataErrorDisplayModeProperties( dataRecord, this );
		}

		private void SyncPendingIsDataErrorTemplateActive( )
		{
			bool currentValue = (bool)this.GetValue( IsDataErrorTemplateActiveProperty );
			if ( currentValue != _pendingIsDataErrorTemplateActive )
			{
				// SSP 6/17/09 TFS17519
				// Apparently there's an issue with the WPF framework where IsMouseOver property
				// on the ancestor elements is not properly maintained when a descendant element
				// with the mouse over it gets pulled of the element tree. In this case, changing
				// the content template of the content presenter that contains the editor results
				// in the editor being temporarily pulled of the element tree. This causes the
				// IsMouseOver of the ancestor elements to remain true even if the mouse is not
				// over them anymore. The solution is to capture the mouse on an ancestor element
				// before changing the template which will cause the framework to set IsMouseOver
				// to false on the descendant elements and it won't matter if a descendant 
				// element gets pulled out of the element tree. After the template is switched,
				// we release the capture below which will cause the framework to set IsMouseOver 
				// property on all the correct elements.
				// 
				bool mouseCaptured = false;
				if ( this.IsMouseOver && null == Mouse.Captured )
					mouseCaptured = this.CaptureMouse( );

				this.SetValue( IsDataErrorTemplateActivePropertyKey, KnownBoxes.FromValue( _pendingIsDataErrorTemplateActive ) );
				this.UpdateLayout( );

				// SSP 6/17/09 TFS17519
				// Related to change above.
				// 
				if ( mouseCaptured )
					this.ReleaseMouseCapture( );
			}
		}

        /// <summary>
        /// Invoked to measure the element and its children.
        /// </summary>
        /// <param name="availableSize">The size that reflects the available size that this element can give to its children.</param>
        /// <returns>The <see cref="System.Windows.Size"/> that represents the desired size of the element.</returns>
        protected override Size MeasureOverride(Size availableSize)
		{
			Size ret = base.MeasureOverride( availableSize );

			this.SyncPendingIsDataErrorTemplateActive( );

			return ret;
		}

		#endregion // DataError

		#region HasDataError

		// SSP 4/13/09 NAS9.2 IDataErrorInfo Support
		// 

		/// <summary>
		/// Identifies the read-only <see cref="HasDataError"/> dependency property.
		/// </summary>
		[InfragisticsFeature( FeatureName = FeatureInfo.FeatureName_IDataErrorInfo, Version = FeatureInfo.Version_9_2 )]
		public static readonly DependencyProperty HasDataErrorProperty = 
			DataRecordPresenter.HasDataErrorProperty.AddOwner( typeof( CellValuePresenter ) );

		/// <summary>
		/// Indicates if the associated cell has data error.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>HasDataError</b> property returns true if the associated Cells's
		/// <see cref="Infragistics.Windows.DataPresenter.Cell.HasDataError"/> property returns 
		/// true or if the underlying ValueEditor's <see cref="ValueEditor.IsValueValid"/> 
		/// property returns true.
		/// </para>
		/// </remarks>
		/// <seealso cref="ValueEditor.IsValueValid"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.Cell.DataError"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.Cell.HasDataError"/>
		/// <seealso cref="RecordSelector.DataError"/>
		/// <seealso cref="RecordSelector.HasDataError"/>
		/// <seealso cref="DataRecordPresenter.HasDataError"/>
		/// <seealso cref="DataRecordPresenter.DataError"/>
		//[Description( "Indicates if the cell has data error." )]
		//[Category( "Data" )]
		[Bindable( true )]
		[Browsable( false )]
		[ReadOnly( true )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		[InfragisticsFeature( FeatureName = FeatureInfo.FeatureName_IDataErrorInfo, Version = FeatureInfo.Version_9_2 )]
		public bool HasDataError
		{
			get
			{
				return (bool)this.GetValue( HasDataErrorProperty );
			}
		}

		#endregion // HasDataError

		#region IsActive

		/// <summary>
		/// Identifies the 'IsActive' dependency property
		/// </summary>
		public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register("IsActive",
			typeof(bool), typeof(CellValuePresenter), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnIsActiveChanged), new CoerceValueCallback(CoerceIsActive)));

		private static void OnIsActiveChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			CellValuePresenter cvp = target as CellValuePresenter;

			Debug.Assert(cvp != null);

			if (cvp != null)
			{
				// JJD 5/30/07 
				// Attempt a coerce if the property was cleared
				if (cvp.ReadLocalValue(IsActiveProperty) == DependencyProperty.UnsetValue)
				{
					bool newValue = (bool)e.NewValue;

					CoerceIsActive(cvp, KnownBoxes.FromValue(newValue));

					// JJD 5/30/07 
					// If we are still out of sync then set the dependencyproperty value back
					if (cvp.IsActive != newValue)
						cvp.SetValue(IsActiveProperty, KnownBoxes.FromValue(cvp.IsActive));
				}

				if (cvp.IsActive)
				{
					//take the focus if necessary.
					cvp.FocusIfAppropriate();
				}

                cvp.OnIsActiveChanged();


                // JJD 4/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
                cvp.UpdateVisualStates();

			}
		}

        /// <summary>
        /// Called when the IsActive property has changed
        /// </summary>
        protected virtual void OnIsActiveChanged()
        {
        }

		private static object CoerceIsActive(DependencyObject target, object value)
		{
			CellValuePresenter cvp = target as CellValuePresenter;

			Debug.Assert(cvp != null);
			Debug.Assert(value is bool);

			if (cvp != null)
			{
				if (cvp.DataPresenter != null)
				{
					if ((bool)value == false)
					{
						// get the curretn active cell
						Cell currentActiveCell = cvp.DataPresenter.ActiveCell;

						if (currentActiveCell != null)
						{
							// if the field and record are the same then clear the active cell
							if (cvp.Field == currentActiveCell.Field &&
								cvp.Record == currentActiveCell.Record)
							{
								cvp.DataPresenter.ActiveCell = null;
							}
						}
					}
					else
					{
						DataRecord dr = cvp.Record;
						Field fld = cvp.Field;

						Debug.Assert(dr != null);
						Debug.Assert(fld != null);

						// set the active cell on the datapresenter
						if (dr != null && fld != null)
							cvp.DataPresenter.ActiveCell = dr.Cells[fld];
					}

					return cvp.IsActive;
				}
			}

			return KnownBoxes.FalseBox;
		}

		/// <summary>
		/// Gets/sets if this <see cref="Infragistics.Windows.DataPresenter.Field"/> is active.
		/// </summary>
		/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.ActiveCell"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.Cell.IsActive"/>
		public bool IsActive
		{
			get
			{
				DataRecord dr = this.Record;

				if (dr == null ||
					!dr.IsActive)
					return false;

				Field fld = this.Field;

				if (fld == null)
					return false;

				Cell activeCell = this.DataPresenter.ActiveCell;

				if (activeCell != null)
					return activeCell.Field == fld;

				return false;
			}
			set
			{
				this.SetValue(CellValuePresenter.IsActiveProperty, KnownBoxes.FromValue(value));
			}
		}

		#endregion //IsActive

		#region IsDataErrorDisplayModeHighlight

		// SSP 4/23/09 NAS9.2 IDataErrorInfo Support
		// 

		/// <summary>
		/// Identifies the read-only <see cref="IsDataErrorDisplayModeHighlight"/> dependency property.
		/// </summary>
		[InfragisticsFeature( FeatureName = FeatureInfo.FeatureName_IDataErrorInfo, Version = FeatureInfo.Version_9_2 )]
		public static readonly DependencyProperty IsDataErrorDisplayModeHighlightProperty = 
			RecordSelector.IsDataErrorDisplayModeHighlightProperty.AddOwner( typeof( CellValuePresenter ) );

		/// <summary>
		/// Indicates if the cell is to be highlighted if it has data error.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// This property reflects the setting of <see cref="FieldLayoutSettings.DataErrorDisplayMode"/> property.
		/// If it's set to <i>Highlight</i> or <i>ErrorIconAndHighlight</i>, this property will return true.
		/// </para>
		/// </remarks>
		/// <seealso cref="FieldLayoutSettings.DataErrorDisplayMode"/>
		/// <seealso cref="FieldLayoutSettings.SupportDataErrorInfo"/>
		/// <seealso cref="DataPresenterBase.DataErrorContentTemplateKey"/>
		/// <seealso cref="DataPresenterBase.DataErrorIconStyleKey"/>
		//[Description( "Indicates if the cell is to be highlighted if it has data error." )]
		//[Category( "Appearance" )]
		[Bindable( true )]
		[Browsable( false )]
		[ReadOnly( true )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		[InfragisticsFeature( FeatureName = FeatureInfo.FeatureName_IDataErrorInfo, Version = FeatureInfo.Version_9_2 )]
		public bool IsDataErrorDisplayModeHighlight
		{
			get
			{
				return (bool)this.GetValue( IsDataErrorDisplayModeHighlightProperty );
			}
		}

		#endregion // IsDataErrorDisplayModeHighlight

		#region IsDataErrorDisplayModeIcon

		// SSP 4/23/09 NAS9.2 IDataErrorInfo Support
		// 

		/// <summary>
		/// Identifies the read-only <see cref="IsDataErrorDisplayModeIcon"/> dependency property.
		/// </summary>
		[InfragisticsFeature( FeatureName = FeatureInfo.FeatureName_IDataErrorInfo, Version = FeatureInfo.Version_9_2 )]
		public static readonly DependencyProperty IsDataErrorDisplayModeIconProperty
				= RecordSelector.IsDataErrorDisplayModeIconProperty.AddOwner( typeof( CellValuePresenter ) );

		/// <summary>
		/// Indicates if the cell is to display an error icon if it has data error.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// This property reflects the setting of <see cref="FieldLayoutSettings.DataErrorDisplayMode"/> property.
		/// If it's set to <i>ErrorIcon</i> or <i>ErrorIconAndHighlight</i>, this property will return true.
		/// </para>
		/// </remarks>
		/// <seealso cref="FieldLayoutSettings.DataErrorDisplayMode"/>
		/// <seealso cref="FieldLayoutSettings.SupportDataErrorInfo"/>
		/// <seealso cref="DataPresenterBase.DataErrorContentTemplateKey"/>
		/// <seealso cref="DataPresenterBase.DataErrorIconStyleKey"/>
		//[Description( "Indicates if the cell is to display an error icon if it has data error." )]
		//[Category( "Appearance" )]
		[Bindable( true )]
		[Browsable( false )]
		[ReadOnly( true )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		[InfragisticsFeature( FeatureName = FeatureInfo.FeatureName_IDataErrorInfo, Version = FeatureInfo.Version_9_2 )]
		public bool IsDataErrorDisplayModeIcon
		{
			get
			{
				return (bool)this.GetValue( IsDataErrorDisplayModeIconProperty );
			}
		}

		#endregion // IsDataErrorDisplayModeIcon

		#region IsDataErrorTemplateActive

		// SSP 5/27/09 NAS9.2 IDataErrorInfo Support
		// 

		/// <summary>
		/// Identifies the property key for read-only <see cref="IsDataErrorTemplateActive"/> dependency property.
		/// </summary>
		private static readonly DependencyPropertyKey IsDataErrorTemplateActivePropertyKey = DependencyProperty.RegisterReadOnly(
			"IsDataErrorTemplateActive",
			typeof( bool ),
			typeof( CellValuePresenter ),
			new FrameworkPropertyMetadata( KnownBoxes.FalseBox, FrameworkPropertyMetadataOptions.None )
		);

		/// <summary>
		/// Identifies the read-only <see cref="IsDataErrorTemplateActive"/> dependency property.
		/// </summary>
		[InfragisticsFeature( FeatureName = FeatureInfo.FeatureName_IDataErrorInfo, Version = FeatureInfo.Version_9_2 )]
		public static readonly DependencyProperty IsDataErrorTemplateActiveProperty = IsDataErrorTemplateActivePropertyKey.DependencyProperty;

		/// <summary>
		/// Indicates if the data error template is being used to display the editor.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>IsDataErrorTemplateActive</b> property is used by the CellValuePresenter template
		/// to determine if the data error template (see <see cref="DataPresenterBase.DataErrorContentTemplateKey"/>)
		/// should be used as the content template for the content presenter that's used to display the
		/// editor inside the cell. The data error template is used to display the data error information
		/// when data error info support is enabled.
		/// Data error info support is enabled by the <see cref="FieldLayoutSettings.SupportDataErrorInfo"/>
		/// and <see cref="FieldLayoutSettings.DataErrorDisplayMode"/> properties.
		/// </para>
		/// </remarks>
		/// <seealso cref="FieldLayoutSettings.SupportDataErrorInfo"/>
		/// <seealso cref="FieldLayoutSettings.DataErrorDisplayMode"/>
		/// <seealso cref="DataError"/>
		/// <seealso cref="HasDataError"/>
		//[Description( "Indicates if the data error template is being used." )]
		//[Category( "Data" )]
		[Bindable( true )]
		[Browsable( false )]
		[ReadOnly( true )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		[InfragisticsFeature( FeatureName = FeatureInfo.FeatureName_IDataErrorInfo, Version = FeatureInfo.Version_9_2 )]
		public bool IsDataErrorTemplateActive
		{
			get
			{
				return (bool)this.GetValue( IsDataErrorTemplateActiveProperty );
			}
		}

		#endregion // IsDataErrorTemplateActive

		#region IsFieldSelected

		/// <summary>
		/// Identifies the <see cref="IsFieldSelected"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsFieldSelectedProperty = DependencyProperty.Register("IsFieldSelected",
			typeof(bool), typeof(CellValuePresenter), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Returns true if the field is selected (read-only)
		/// </summary>
		/// <seealso cref="IsFieldSelectedProperty"/>
		//[Description("Returns true if the field is selected (read-only)")]
		//[Category("Behavior")]
		public bool IsFieldSelected
		{
			get
			{
				return (bool)this.GetValue(CellValuePresenter.IsFieldSelectedProperty);
			}
			set
			{
				throw new NotSupportedException( DataPresenterBase.GetString( "LE_NotSupportedException_3" ) );
				//this.SetValue(CellValuePresenter.IsFieldSelectedProperty, value);
			}
		}

		#endregion //IsFieldSelected

        // JJD 4/30/09 - TFS17157 - added
        #region IsMouseOverRecord

        private static readonly DependencyPropertyKey IsMouseOverRecordPropertyKey =
            DependencyProperty.RegisterReadOnly("IsMouseOverRecord",
            typeof(bool), typeof(CellValuePresenter), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

        /// <summary>
        /// Identifies the <see cref="IsMouseOverRecord"/> dependency property
        /// </summary>
        public static readonly DependencyProperty IsMouseOverRecordProperty =
            IsMouseOverRecordPropertyKey.DependencyProperty;

        /// <summary>
        /// Returns whether the mouse is over the ancestor DataRecordCellArea (read-only)
        /// </summary>
        /// <seealso cref="IsMouseOverRecordProperty"/>
        //[Description("Returns whether the mouse is over the ancestor DataRecordCellArea (read-only)")]
        //[Category("Behavior")]
        [ReadOnly(true)]
        [Bindable(true)]
        public bool IsMouseOverRecord
        {
            get
            {
                return (bool)this.GetValue(CellValuePresenter.IsMouseOverRecordProperty);
            }
        }

        #endregion //IsMouseOverRecord

        // JJD 4/30/09 - TFS17157 - added
        #region IsRecordSelected

        private static readonly DependencyPropertyKey IsRecordSelectedPropertyKey =
            DependencyProperty.RegisterReadOnly("IsRecordSelected",
            typeof(bool), typeof(CellValuePresenter), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

        /// <summary>
        /// Identifies the <see cref="IsRecordSelected"/> dependency property
        /// </summary>
        public static readonly DependencyProperty IsRecordSelectedProperty =
            IsRecordSelectedPropertyKey.DependencyProperty;

        /// <summary>
        /// Returns true if the associated record is selected (read-only)
        /// </summary>
        /// <seealso cref="IsRecordSelectedProperty"/>
        //[Description("Returns true if the associated record is selected (read-only)")]
        //[Category("Behavior")]
        [ReadOnly(true)]
        [Bindable(true)]
        public bool IsRecordSelected
        {
            get
            {
                return (bool)this.GetValue(CellValuePresenter.IsRecordSelectedProperty);
            }
        }

        #endregion //IsRecordSelected

		#region IsSelected

		/// <summary>
		/// Identifies the 'IsSelected' dependency property
		/// </summary>
		public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register("IsSelected",
			typeof(bool), typeof(CellValuePresenter), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnIsSelectedChanged), new CoerceValueCallback(CoerceIsSelected)));

		private static void OnIsSelectedChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			CellValuePresenter cvp = target as CellValuePresenter;

			Debug.Assert(cvp != null);

			if (cvp != null)
			{
				// JJD 5/30/07 
				// Attempt a coerce if the property was cleared
				if (cvp.ReadLocalValue(IsSelectedProperty) == DependencyProperty.UnsetValue)
				{
					bool newValue = (bool)e.NewValue;

					CoerceIsSelected(cvp, KnownBoxes.FromValue(newValue));

					// JJD 5/30/07 
					// If we are still out of sync then set the dependencyproperty value back
					if (cvp.IsSelected != newValue)
						cvp.SetValue(IsSelectedProperty, KnownBoxes.FromValue(cvp.IsSelected));
				}


                // JJD 4/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
                cvp.UpdateVisualStates();

            }
		}

		private static object CoerceIsSelected(DependencyObject target, object value)
		{
			CellValuePresenter fcell = target as CellValuePresenter;

			Debug.Assert(fcell != null);
			Debug.Assert(value is bool);

			if (fcell != null)
			{
				if (fcell.DataPresenter != null)
				{
					DataRecord dr = fcell.Record;
					Field fld = fcell.Field;

					Debug.Assert(dr != null);
					Debug.Assert(fld != null);

					if ((bool)value == false)
					{
						// de-select the cell
						if (dr.IsCellAllocated(fld))
							dr.Cells[fld].IsSelected = false;
					}
					else
					{

						// select the cell
						if (dr != null && fld != null)
							dr.Cells[fld].IsSelected = true;
					}

					return fcell.IsSelected;
				}
			}

			return KnownBoxes.FalseBox;
		}

		/// <summary>
		/// Gets/sets if this <see cref="Infragistics.Windows.DataPresenter.Field"/> is selected.
		/// </summary>
		/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.SelectedItems"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.SelectedItemHolder"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.Cell.IsSelected"/>
		public bool IsSelected
		{
			get
			{
				DataRecord dr = this.Record;

				if (dr == null)
					return false;

				Field fld = this.Field;

				if (fld == null || !dr.IsCellAllocated(fld))
					return false;

				return dr.Cells[fld].IsSelected;
			}
			set
			{
				this.SetValue(CellValuePresenter.IsSelectedProperty, value);
			}
		}

		#endregion //IsSelected

		#region SortStatus

		private static readonly DependencyPropertyKey SortStatusPropertyKey =
			DependencyProperty.RegisterReadOnly("SortStatus",
			typeof(SortStatus), typeof(CellValuePresenter), new FrameworkPropertyMetadata(SortStatus.NotSorted));

		/// <summary>
		/// Identifies the 'SortStatus' dependency property
		/// </summary>
		public static readonly DependencyProperty SortStatusProperty =
			SortStatusPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the sort status of the Field (read-only)
		/// </summary>
		//[Description("Returns the sort status of the Field (read-only)")]
		//[Category("Behavior")]
		public SortStatus SortStatus
		{
			get
			{
				return (SortStatus)this.GetValue(CellValuePresenter.SortStatusProperty);
			}
		}

		#endregion //SortStatus

		#region Value

		/// <summary>
		/// Identifies the 'Value' dependency property
		/// </summary>
		public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value",
			typeof(object), typeof(CellValuePresenter), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(ValueChangedCallback), new CoerceValueCallback(CoerceValue)));

		private static void ValueChangedCallback(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			CellValuePresenter cvp = target as CellValuePresenter;

			if (cvp != null)
			{
				object newValue = e.NewValue;

				// JJD 5/30/07 
				// Attempt a coerce if the property was cleared
				if (cvp.ReadLocalValue(ValueProperty) == DependencyProperty.UnsetValue)
				{
					CoerceValue(cvp, newValue);

					// JJD 5/30/07 
					// Check if we are out of sync with the dependencyproperty value
					// If we are still out of sync then set the dependencyproperty value back
					if (cvp.Value != newValue)
						cvp.SetValue(ValueProperty, newValue);
				}

				if (cvp._initializingValue == false && cvp.Editor != null)
				{
					// set a flag so that we can ignore the change notification from the editor
					cvp._isSynchronizingEditorValue = true;

					try
					{
						// synchronize the editor's value
						cvp.Editor.Value = e.NewValue;
					}
					finally
					{
						// reset the sync flag
						cvp._isSynchronizingEditorValue = false;
					}

				}

				// AS 10/13/09
				// The CLR get/set of the CellPresenter's Value property interact with the record directly 
				// even though we have a backing DP and we only initialize its Value DP when the record or 
				// field changes so once the value changes after the element has been initialized it can 
				// get out of sync. We need to update it whenever the cell's value has been changed.
				//
				CellPresenterLayoutElementBase presenterElement = VisualTreeHelper.GetParent(cvp) as CellPresenterLayoutElementBase;
				CellPresenter cp = null != presenterElement ? presenterElement.CellPresenter as CellPresenter : null;

				if (null != cp)
				{
					cp.SetValue(CellPresenter.ValueProperty, e.NewValue);
				}

				// JJD 7/05/11 - TFS80466 
				// If the field has a style selector specified for the CVP or editor styles
				// we should invalidate them because the value has changed
				cvp.ReinitalizeIfStyleSelectorsSpecified();

				// AS 10/13/09 NA 2010.1 - CardView
				// A cell can be collapsed when its value has been changed.
				//
				cvp.CoerceValue(VisibilityProperty);
				return;
			}

			Debug.Fail("Invalid target, must be CellValuePresenter not " + (target != null ? target.GetType().ToString() : "<Null>"));
			return;
		}

		private static object CoerceValue(DependencyObject target, object value)
		{
			// JJD 3/11/11 - TFS67970 - Optimization 
			// seta flag so we know that CoerceValue was called
			_WasCoerceValueCalled = true;

			CellValuePresenter cvp = target as CellValuePresenter;

			if (cvp != null)
				return cvp.OnCoerceValue(value);

			Debug.Fail("Invalid target, must be CellValuePresenter not " + (target != null ? target.GetType().ToString() : "<Null>"));
			return null;
		}

		/// <summary>
		/// Called to coerce the value during a set
		/// </summary>
		/// <param name="value">The value to coerce</param>
		/// <returns>The coerced value.</returns>
		protected virtual object OnCoerceValue(object value)
		{
			// SSP 1/21/09 TFS12327
			// See notes on CVPValueWrapper for more info.
			// 
			// ----------------------------------------------------
			bool updateRecord = true;
			CVPValueWrapper wrapper = value as CVPValueWrapper;
			if ( null != wrapper )
			{
				value = wrapper._value;
				updateRecord = wrapper._updateRecord;
			}
			// ----------------------------------------------------

			// during initialization we don't want to set the value back on the cell
			if (this._initializingValue == true)
				return value;

			Field field = this.Field;
			if (field == null)
				return null;

			// SSP 1/21/09 TFS12327
			// Only update the record if updateRecord is true.
			// 
			//field.SetCellValue( this.Record, value, true );
			if ( updateRecord )
                // AS 4/15/09 Field.(Get|Set)CellValue
				//field.SetCellValue( this.Record, value, true );
				this.Record.SetCellValue( field, value, true );

			return this.Value;
		}

		/// <summary>
		/// Gets/sets the value of the cell
		/// </summary>
		public object Value
		{
			get
			{
				Field field = this.Field;
				if (field == null)
					return null;

				DataRecord rcd = this.Record;

				if (rcd == null)
					return null;

                // JJD 5/29/09 - TFS18063 
                // Use the new overload to GetCellValue which will return the value 
                // converted into EditAsType
                //return this.Field.GetCellValue(rcd, true);
                return rcd.GetCellValue( field, CellValueType.EditAsType );
			}
			set
			{
				this.SetValue(ValueProperty, value);
			}
		}

		#endregion //Value

		// JM 6/12/09 NA 2009.2 DataValueChangedEvent
		#region ValueHistory

		internal static readonly DependencyPropertyKey ValueHistoryPropertyKey =
			DependencyProperty.RegisterReadOnly("ValueHistory",
			typeof(IList<DataValueInfo>), typeof(CellValuePresenter), new FrameworkPropertyMetadata((IList)null));

		/// <summary>
		/// Identifies the <see cref="ValueHistory"/> dependency property
		/// </summary>
		[InfragisticsFeature(Version = FeatureInfo.Version_9_2, FeatureName = FeatureInfo.FeatureName_DataValueChangedEvent)]
		public static readonly DependencyProperty ValueHistoryProperty =
			ValueHistoryPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns an IList of <see cref="DataValueInfo"/> instances contain the value change history for the data values associated with this CellValuePresenter.
		/// </summary>
		/// <seealso cref="ValueHistoryProperty"/>
		//[Description("Returns an IList of DataValueInfo instances contain the value change history for the data values associated with this CellValuePresenter.")]
		//[Category("Behavior")]
		[Bindable(true)]
		[ReadOnly(true)]
		[InfragisticsFeature(Version = FeatureInfo.Version_9_2, FeatureName = FeatureInfo.FeatureName_DataValueChangedEvent)]
		public IList<DataValueInfo> ValueHistory
		{
			get
			{
				return (IList<DataValueInfo>)this.GetValue(CellValuePresenter.ValueHistoryProperty);
			}
		}

		#endregion //ValueHistory

		#region Styling Properties

		#region BackgroundHover


		/// <summary>
		/// Identifies the <see cref="BackgroundHover"/> dependency property
		/// </summary>	
		public static readonly DependencyProperty BackgroundHoverProperty = DependencyProperty.Register("BackgroundHover",
			typeof(Brush), typeof(CellValuePresenter), new FrameworkPropertyMetadata((object)null));

		/// <summary>
		/// The brush applied to the background area when IsMouseOver = true.
		/// </summary>
		/// <seealso cref="BackgroundHoverProperty"/>
		//[Description("The brush applied to the background area when IsMouseOver = true.")]
		//[Category("Brushes")]
		public Brush BackgroundHover
		{
			get
			{
				return (Brush)this.GetValue(CellValuePresenter.BackgroundHoverProperty);
			}
			set
			{
				this.SetValue(CellValuePresenter.BackgroundHoverProperty, value);
			}
		}

		#endregion BackgroundHover		
				
		#region BorderHoverBrush


		/// <summary>
		/// Identifies the <see cref="BorderHoverBrush"/> dependency property
		/// </summary>	
		public static readonly DependencyProperty BorderHoverBrushProperty = DependencyProperty.Register("BorderHoverBrush",
			typeof(Brush), typeof(CellValuePresenter), new FrameworkPropertyMetadata((object)null));

		/// <summary>
		/// The border brush applied to the background area when IsMouseOver = true.
		/// </summary>
		/// <seealso cref="BorderHoverBrushProperty"/>
		//[Description("The border brush applied to the background area when IsMouseOver = true.")]
		//[Category("Brushes")]
		public Brush BorderHoverBrush
		{
			get
			{
				return (Brush)this.GetValue(CellValuePresenter.BorderHoverBrushProperty);
			}
			set
			{
				this.SetValue(CellValuePresenter.BorderHoverBrushProperty, value);
			}
		}

		#endregion BorderHoverBrush		
				
		#region BackgroundActive

		/// <summary>
		/// Identifies the <see cref="BackgroundActive"/> dependency property
		/// </summary>	
		public static readonly DependencyProperty BackgroundActiveProperty = DependencyProperty.Register("BackgroundActive",
			typeof(Brush), typeof(CellValuePresenter), new FrameworkPropertyMetadata((object)null));

		/// <summary>
		/// The brush applied to the background area when IsActive = true.
		/// </summary>
		/// <seealso cref="BackgroundActiveProperty"/>
		//[Description("The brush applied to the background area when IsActive = true.")]
		//[Category("Brushes")]
		public Brush BackgroundActive
		{
			get
			{
				return (Brush)this.GetValue(CellValuePresenter.BackgroundActiveProperty);
			}
			set
			{
				this.SetValue(CellValuePresenter.BackgroundActiveProperty, value);
			}
		}

		#endregion BackgroundActive		
				
		#region BorderActiveBrush

		/// <summary>
		/// Identifies the <see cref="BorderActiveBrush"/> dependency property
		/// </summary>	
		public static readonly DependencyProperty BorderActiveBrushProperty = DependencyProperty.Register("BorderActiveBrush",
			typeof(Brush), typeof(CellValuePresenter), new FrameworkPropertyMetadata((object)null));

		/// <summary>
		/// The border brush applied to the background area when IsActive = true.
		/// </summary>
		/// <seealso cref="BorderActiveBrushProperty"/>
		//[Description("The border brush applied to the background area when IsActive = true.")]
		//[Category("Brushes")]
		public Brush BorderActiveBrush
		{
			get
			{
				return (Brush)this.GetValue(CellValuePresenter.BorderActiveBrushProperty);
			}
			set
			{
				this.SetValue(CellValuePresenter.BorderActiveBrushProperty, value);
			}
		}

		#endregion BorderActiveBrush		
				
		#region BackgroundSelected

		/// <summary>
		/// Identifies the <see cref="BackgroundSelected"/> dependency property
		/// </summary>
		public static readonly DependencyProperty BackgroundSelectedProperty = DependencyProperty.Register("BackgroundSelected",
			typeof(Brush), typeof(CellValuePresenter), new FrameworkPropertyMetadata((object)null));

		/// <summary>
		/// The brush applied to the background area when IsFieldSelected = true.
		/// </summary>
		/// <seealso cref="BackgroundSelectedProperty"/>
		//[Description("The brush applied to the background area when IsSelected = true.")]
		//[Category("Brushes")]
		public Brush BackgroundSelected
		{
			get
			{
				return (Brush)this.GetValue(CellValuePresenter.BackgroundSelectedProperty);
			}
			set
			{
				this.SetValue(CellValuePresenter.BackgroundSelectedProperty, value);
			}
		}

		#endregion BackgroundSelected	
				
		#region BorderSelectedBrush

		/// <summary>
		/// Identifies the <see cref="BorderSelectedBrush"/> dependency property
		/// </summary>	
		public static readonly DependencyProperty BorderSelectedBrushProperty = DependencyProperty.Register("BorderSelectedBrush",
			typeof(Brush), typeof(CellValuePresenter), new FrameworkPropertyMetadata((object)null));

		/// <summary>
		/// The border brush applied to the background area when IsFieldSelected = true.
		/// </summary>
		/// <seealso cref="BorderSelectedBrushProperty"/>
		//[Description("The border brush applied to the background area when IsSelected = true.")]
		//[Category("Brushes")]
		public Brush BorderSelectedBrush
		{
			get
			{
				return (Brush)this.GetValue(CellValuePresenter.BorderSelectedBrushProperty);
			}
			set
			{
				this.SetValue(CellValuePresenter.BorderSelectedBrushProperty, value);
			}
		}

		#endregion BorderSelectedBrush

		#region BackgroundFieldSelected

		/// <summary>
		/// Identifies the <see cref="BackgroundFieldSelected"/> dependency property
		/// </summary>	
		public static readonly DependencyProperty BackgroundFieldSelectedProperty = DependencyProperty.Register("BackgroundFieldSelected",
			typeof(Brush), typeof(CellValuePresenter), new FrameworkPropertyMetadata((object)null));

		/// <summary>
		/// The brush applied to the background area when IsFieldSelected = true.
		/// </summary>
		/// <seealso cref="BackgroundFieldSelectedProperty"/>
		//[Description("The brush applied to the background area when IsFieldSelected = true.")]
		//[Category("Brushes")]
		public Brush BackgroundFieldSelected
		{
			get
			{
				return (Brush)this.GetValue(CellValuePresenter.BackgroundFieldSelectedProperty);
			}
			set
			{
				this.SetValue(CellValuePresenter.BackgroundFieldSelectedProperty, value);
			}
		}

		#endregion BackgroundFieldSelected	
		
		#region BorderFieldSelectedBrush

		/// <summary>
		/// Identifies the <see cref="BorderFieldSelectedBrush"/> dependency property
		/// </summary>	
		public static readonly DependencyProperty BorderFieldSelectedBrushProperty = DependencyProperty.Register("BorderFieldSelectedBrush",
			typeof(Brush), typeof(CellValuePresenter), new FrameworkPropertyMetadata((object)null));

		/// <summary>
		/// The border brush applied to the background area when IsFieldSelected = true.
		/// </summary>
		/// <seealso cref="BorderFieldSelectedBrushProperty"/>
		//[Description("The border brush applied to the background area when IsFieldSelected = true.")]
		//[Category("Brushes")]
		public Brush BorderFieldSelectedBrush
		{
			get
			{
				return (Brush)this.GetValue(CellValuePresenter.BorderFieldSelectedBrushProperty);
			}
			set
			{
				this.SetValue(CellValuePresenter.BorderFieldSelectedBrushProperty, value);
			}
		}

		#endregion BorderFieldSelectedBrush
				
		#region BackgroundPrimary

		/// <summary>
		/// Identifies the <see cref="BackgroundPrimary"/> dependency property
		/// </summary>	
		public static readonly DependencyProperty BackgroundPrimaryProperty = DependencyProperty.Register("BackgroundPrimary",
			typeof(Brush), typeof(CellValuePresenter), new FrameworkPropertyMetadata((object)null));

		/// <summary>
		/// The brush applied to the background area when HighlightAsPrimary = true.
		/// </summary>
		/// <seealso cref="BackgroundPrimaryProperty"/>
		//[Description("The brush applied to the background area when HighlightAsPrimary = true.")]
		//[Category("Brushes")]
		public Brush BackgroundPrimary
		{
			get
			{
				return (Brush)this.GetValue(CellValuePresenter.BackgroundPrimaryProperty);
			}
			set
			{
				this.SetValue(CellValuePresenter.BackgroundPrimaryProperty, value);
			}
		}

		#endregion BackgroundPrimary		
				
		#region BorderPrimaryBrush

		/// <summary>
		/// Identifies the <see cref="BorderPrimaryBrush"/> dependency property
		/// </summary>	
		public static readonly DependencyProperty BorderPrimaryBrushProperty = DependencyProperty.Register("BorderPrimaryBrush",
			typeof(Brush), typeof(CellValuePresenter), new FrameworkPropertyMetadata((object)null));

		/// <summary>
		/// The border brush applied to the background area when HighlightAsPrimary = true.
		/// </summary>
		/// <seealso cref="BorderPrimaryBrushProperty"/>
		//[Description("The border brush applied to the background area when HighlightAsPrimary = true.")]
		//[Category("Brushes")]
		public Brush BorderPrimaryBrush
		{
			get
			{
				return (Brush)this.GetValue(CellValuePresenter.BorderPrimaryBrushProperty);
			}
			set
			{
				this.SetValue(CellValuePresenter.BorderPrimaryBrushProperty, value);
			}
		}

		#endregion BorderPrimaryBrush		
				
		#region ForegroundStyle

		/// <summary>
		/// Identifies the <see cref="ForegroundStyle"/> dependency property
		/// </summary>	
		public static readonly DependencyProperty ForegroundStyleProperty = DependencyProperty.Register("ForegroundStyle",
			typeof(Style), typeof(CellValuePresenter), new FrameworkPropertyMetadata((object)null));

		/// <summary>
		/// Style applied to internal ContentPresenter used by default templates.
		/// </summary>	
		/// <seealso cref="ForegroundStyleProperty"/>	
		//[Description("Style applied to internal ContentPresenter used by default templates.")]
		//[Category("Appearance")]
		public Style ForegroundStyle
		{
			get
			{
				return (Style)this.GetValue(CellValuePresenter.ForegroundStyleProperty);
			}
			set
			{
				this.SetValue(CellValuePresenter.ForegroundStyleProperty, value);
			}
		}

		#endregion ForegroundStyle		
				
		#region ForegroundActiveStyle

		/// <summary>
		/// Identifies the <see cref="ForegroundActiveStyle"/> dependency property
		/// </summary>	
		public static readonly DependencyProperty ForegroundActiveStyleProperty = DependencyProperty.Register("ForegroundActiveStyle",
			typeof(Style), typeof(CellValuePresenter), new FrameworkPropertyMetadata((object)null));

		/// <summary>
		/// Style applied to default ContentPresenter when IsActive = true.
		/// </summary>
		/// <seealso cref="ForegroundActiveStyleProperty"/>
		//[Description("Style applied to default ContentPresenter when IsActive = true.")]
		//[Category("Appearance")]
		public Style ForegroundActiveStyle
		{
			get
			{
				return (Style)this.GetValue(CellValuePresenter.ForegroundActiveStyleProperty);
			}
			set
			{
				this.SetValue(CellValuePresenter.ForegroundActiveStyleProperty, value);
			}
		}

		#endregion ForegroundActiveStyle		
				
		#region ForegroundAlternateStyle

		/// <summary>
		/// Identifies the <see cref="ForegroundAlternateStyle"/> dependency property
		/// </summary>	
		public static readonly DependencyProperty ForegroundAlternateStyleProperty = DependencyProperty.Register("ForegroundAlternateStyle",
			typeof(Style), typeof(CellValuePresenter), new FrameworkPropertyMetadata((object)null));

		/// <summary>
		/// Style applied to default ContentPresenter when IsAlternate = true.
		/// </summary>
		/// <seealso cref="ForegroundAlternateStyleProperty"/>
		//[Description("Style applied to default ContentPresenter when IsAlternate = true.")]
		//[Category("Appearance")]
		public Style ForegroundAlternateStyle
		{
			get
			{
				return (Style)this.GetValue(CellValuePresenter.ForegroundAlternateStyleProperty);
			}
			set
			{
				this.SetValue(CellValuePresenter.ForegroundAlternateStyleProperty, value);
			}
		}

		#endregion ForegroundAlternateStyle		
				
		#region ForegroundSelectedStyle

		/// <summary>
		/// Identifies the <see cref="ForegroundSelectedStyle"/> dependency property
		/// </summary>	
		public static readonly DependencyProperty ForegroundSelectedStyleProperty = DependencyProperty.Register("ForegroundSelectedStyle",
			typeof(Style), typeof(CellValuePresenter), new FrameworkPropertyMetadata((object)null));

		/// <summary>
		/// Style applied to default ContentPresenter when IsFieldSelected = true.
		/// </summary>
		/// <seealso cref="ForegroundSelectedStyleProperty"/>
		//[Description("Style applied to default ContentPresenter when IsFieldSelected = true.")]
		//[Category("Appearance")]
		public Style ForegroundSelectedStyle
		{
			get
			{
				return (Style)this.GetValue(CellValuePresenter.ForegroundSelectedStyleProperty);
			}
			set
			{
				this.SetValue(CellValuePresenter.ForegroundSelectedStyleProperty, value);
			}
		}

		#endregion ForegroundSelectedStyle		
				
		#region ForegroundFieldSelectedStyle

		/// <summary>
		/// Identifies the <see cref="ForegroundFieldSelectedStyle"/> dependency property
		/// </summary>	
		public static readonly DependencyProperty ForegroundFieldSelectedStyleProperty = DependencyProperty.Register("ForegroundFieldSelectedStyle",
			typeof(Style), typeof(CellValuePresenter), new FrameworkPropertyMetadata((object)null));

		/// <summary>
		/// Style applied to default ContentPresenter when IsFieldSelected = true.
		/// </summary>
		/// <seealso cref="ForegroundFieldSelectedStyleProperty"/>
		//[Description("Style applied to default ContentPresenter when IsFieldSelected = true.")]
		//[Category("Appearance")]
		public Style ForegroundFieldSelectedStyle
		{
			get
			{
				return (Style)this.GetValue(CellValuePresenter.ForegroundFieldSelectedStyleProperty);
			}
			set
			{
				this.SetValue(CellValuePresenter.ForegroundFieldSelectedStyleProperty, value);
			}
		}

		#endregion ForegroundFieldSelectedStyle		

				
		#region ForegroundHoverStyle

		/// <summary>
		/// Identifies the <see cref="ForegroundHoverStyle"/> dependency property
		/// </summary>	
		public static readonly DependencyProperty ForegroundHoverStyleProperty = DependencyProperty.Register("ForegroundHoverStyle",
			typeof(Style), typeof(CellValuePresenter), new FrameworkPropertyMetadata((object)null));

		/// <summary>
		/// Style applied to default ContentPresenter when IsMouseOver = true.
		/// </summary>
		/// <seealso cref="ForegroundHoverStyleProperty"/>
		//[Description("Style applied to default ContentPresenter when IsMouseOver = true.")]
		//[Category("Appearance")]
		public Style ForegroundHoverStyle
		{
			get
			{
				return (Style)this.GetValue(CellValuePresenter.ForegroundHoverStyleProperty);
			}
			set
			{
				this.SetValue(CellValuePresenter.ForegroundHoverStyleProperty, value);
			}
		}

		#endregion ForegroundHoverStyle		
				
		#region ForegroundPrimaryStyle

		/// <summary>
		/// Identifies the <see cref="ForegroundPrimaryStyle"/> dependency property
		/// </summary>	
		public static readonly DependencyProperty ForegroundPrimaryStyleProperty = DependencyProperty.Register("ForegroundPrimaryStyle",
			typeof(Style), typeof(CellValuePresenter), new FrameworkPropertyMetadata((object)null));

		/// <summary>
		/// Style applied to default ContentPresenter when HighlightAsPrimary = true.
		/// </summary>
		/// <seealso cref="ForegroundPrimaryStyleProperty"/>
		//[Description("Style applied to default ContentPresenter when HighlightAsPrimary = true.")]
		//[Category("Appearance")]
		public Style ForegroundPrimaryStyle
		{
			get
			{
				return (Style)this.GetValue(CellValuePresenter.ForegroundPrimaryStyleProperty);
			}
			set
			{
				this.SetValue(CellValuePresenter.ForegroundPrimaryStyleProperty, value);
			}
		}

		#endregion ForegroundPrimaryStyle		

		#region CornerRadius

		/// <summary>
		/// Identifies the <see cref="CornerRadius"/> dependency property
		/// </summary>	
		public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register("CornerRadius",
			typeof(CornerRadius), typeof(CellValuePresenter), new FrameworkPropertyMetadata(new CornerRadius(0)));

		/// <summary>
		/// Sets the CornerRadius of default borders.
		/// </summary>
		/// <seealso cref="CornerRadiusProperty"/>
		//[Description("CornerRadius used by borders in default templates.")]
		//[Category("Appearance")]
		public CornerRadius CornerRadius
		{
			get
			{
				return (CornerRadius)this.GetValue(CellValuePresenter.CornerRadiusProperty);
			}
			set
			{
				this.SetValue(CellValuePresenter.CornerRadiusProperty, value);
			}
		}

		#endregion CornerRadius	
							
		#endregion // Styling Properties

		#endregion //Public Properties

		#region Internal Properties

		#region Cell

		internal Cell Cell
		{
			get
			{
				if (this._cell != null)
					return this._cell;

				Field fld = this.Field;
				DataRecord rcd = this.Record;

				if (fld == null || rcd == null)
					return null;

				// AS 9/21/09 TFS22398
				if (fld.Index < 0)
					return null;

				this._cell = rcd.Cells[fld];

				return this._cell;
			}
		}

		#endregion //Cell

		#region SortStatusInternal

		private static readonly DependencyProperty SortStatusInternalProperty = DependencyProperty.Register("SortStatusInternal",
			typeof(SortStatus), typeof(CellValuePresenter), new FrameworkPropertyMetadata(SortStatus.NotSorted));

		private SortStatus SortStatusInternal
		{
			get
			{
				return (SortStatus)this.GetValue(CellValuePresenter.SortStatusInternalProperty);
			}
			set
			{
				this.SetValue(CellValuePresenter.SortStatusInternalProperty, value);
			}
		}

		#endregion //SortStatusInternal

		#region IsModifierKeyDown

		/// <summary>
		/// Indicates if any of the modifier keys are currently down.
		/// </summary>
		internal bool IsModifierKeyDown
		{
			get
			{
				return 0 != ( ( ModifierKeys.Alt | ModifierKeys.Control | ModifierKeys.Shift ) & Keyboard.Modifiers );
			}
		}

		#endregion // IsModifierKeyDown

		// JJD 3/11/11 - TFS67970 - Optimization
		#region WeakRef

		// JJD 3/11/11 - TFS67970 - Optimization
		// Cache a weak reference to this CellValuePresenter to be used by its associated Cell
		// This prevents heap fragmentation when this object is recycled
		internal WeakReference WeakRef
		{
			get
			{
				if (_weakRef == null)
					_weakRef = new WeakReference(this);

				return _weakRef;
			}
		}

		#endregion //WeakRef	
    
		#endregion //Internal Properties

		#endregion //Properties

		#region Methods

			#region Public Methods

				// JJD 5/23/07 - Added FromCell method
				#region FromCell

		/// <summary>
		/// Returns the <see cref="CellValuePresenter"/> that represents a specific cell.
		/// </summary>
		/// <param name="cell">The specific cell.</param>
		/// <returns>The <see cref="CellValuePresenter"/> that represents the cell or null if not found in the visual tree.</returns>
		/// <exception cref="ArgumentNullException">If cell is null.</exception>
		/// <seealso cref="DataRecord"/>
		/// <seealso cref="FromRecordAndField(DataRecord,Field)"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.Field"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.Cell"/>
		/// <seealso cref="RecordPresenter.FromRecord(Record)"/>
		/// <seealso cref="DataPresenterBase.RecordsInViewChanged"/>
		/// <seealso cref="DataPresenterBase.GetRecordsInView(bool)"/>
		/// <seealso cref="DataPresenterBase.BringRecordIntoView(Record)"/>
		/// <seealso cref="DataPresenterBase.BringCellIntoView(Cell)"/>
		public static CellValuePresenter FromCell(Cell cell)
		{
			if (cell == null)
				throw new ArgumentNullException("cell");

			DataRecord record = cell.Record;
			Field field = cell.Field;

			if (record == null ||
				 field == null)
				return null;

			return FromRecordAndField(record, field);
		}

				#endregion //FromCell

				#region FromRecordAndField

		/// <summary>
		/// Returns the <see cref="CellValuePresenter"/> that represents a specific cell.
		/// </summary>
		/// <param name="record">The cell's associated DataRecord.</param>
		/// <param name="field">The cell's associated Field.</param>
		/// <returns>The <see cref="CellValuePresenter"/> that represents the cell or null if not found in the visual tree.</returns>
		/// <exception cref="ArgumentNullException">If record or field is null.</exception>
		/// <seealso cref="FromCell(Cell)"/>
		/// <seealso cref="DataRecord"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.Field"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.Cell"/>
		/// <seealso cref="RecordPresenter.FromRecord(Record)"/>
		/// <seealso cref="DataPresenterBase.RecordsInViewChanged"/>
		/// <seealso cref="DataPresenterBase.GetRecordsInView(bool)"/>
		/// <seealso cref="DataPresenterBase.BringRecordIntoView(Record)"/>
		/// <seealso cref="DataPresenterBase.BringCellIntoView(Cell)"/>
		public static CellValuePresenter FromRecordAndField(DataRecord record, Field field)
		{

		// MD 8/20/10
		// We don't need the CVPCache anymore. We can get similar gains by caching the cell panel on the data record presenter.
		//    return FromRecordAndField( record, field, false );
		//}
		//
		//internal static CellValuePresenter FromRecordAndField( DataRecord record, Field field, bool isCellCVPBeingInitialized )
		//{
			if (record == null)
				throw new ArgumentNullException("record");

			if (field == null)
				throw new ArgumentNullException("field");

			DataRecordPresenter drp = record.AssociatedRecordPresenter as DataRecordPresenter;

			// if null that means the record element wasn't hydrated and
			// therefore is not in view so we know that we can't ave a cell value presetner
			if (drp == null)
				return null;

			// get the cell if allocated
            
            
			
			
            Cell cell = record.GetCellIfAllocated( field );

			// MD 8/20/10
			// We don't need the CVPCache anymore. We can get similar gains by caching the cell panel on the data record presenter.
			//// If the cell has been allocated and its AssociatedCellValuePresenterInternal 
			//// initialized then return it. If cell is null or if the cell's 
			//// AssociatedCellValuePresenter property's value is being initialized then we 
			//// need to get the cvp from cvp cache or if we don't have cvp cache then by 
			//// doing an element search.
			//// 
			//if ( null != cell && ! isCellCVPBeingInitialized )
			//    return cell.AssociatedCellValuePresenterInternal;
			//
			//// SSP 6/7/10 - Optimizations - TFS34031
			//// We only keep entries in cvpCache if cell has not been allocated. However if cell's just been
			//// allocated (which is indicated by isCellAssociatedCellValuePresenterInternalValid flag) then
			//// get the cached cvp.
			//// 
			//// --------------------------------------------------------------------------------------------
			//DataPresenterBase dp = field.DataPresenter;
			//CellValuePresenter cellValuePresenter = null;
			//CVPCache cvpCache = null != dp ? dp._cvpCache : null;
			//if ( null != cvpCache && ( null == cell || isCellCVPBeingInitialized ) && cvpCache.CacheRequested( ) )
			//{
			//    cellValuePresenter = cvpCache.GetCachedCVP( record, field );
			//
			//    // Since the cell is allocated now, it will maintain the cvp cache from now on so remove 
			//    // the entry from cvpCache.
			//    // 
			//    if ( null != cell && isCellCVPBeingInitialized )
			//        cvpCache.Remove( record, field );
			//
			//    return cellValuePresenter;
			//}
			//// --------------------------------------------------------------------------------------------

			CellValuePresenter cellValuePresenter = null;

			// If the cell has been allocated and its AssociatedCellValuePresenterInternal 
			// initialized then return it. If cell is null or if the cell's 
			// AssociatedCellValuePresenter property's value is being initialized then we 
			// need to get the cvp from cvp cache or if we don't have cvp cache then by 
			// doing an element search.
			// 
			if (null != cell)
			{
				cellValuePresenter = cell.AssociatedCellValuePresenterInternal;

				if (cellValuePresenter != null)
					return cellValuePresenter;
			}

			// MD 8/19/10
			// Try to use the cached associated virtualizing cell panel.
			VirtualizingDataRecordCellPanel cellPanel = drp.AssociatedVirtualizingDataRecordCellPanel;
			if (cellPanel == null)
			{
				FrameworkElement contentSite = drp.GetRecordContentSite();

				if (null == contentSite)
					return null;

				// MD 8/19/10
				// This variable is already defined.
				//VirtualizingDataRecordCellPanel cellPanel = Utilities.GetDescendantFromType(contentSite, typeof(VirtualizingDataRecordCellPanel), true) as VirtualizingDataRecordCellPanel;
				cellPanel = Utilities.GetDescendantFromType(contentSite, typeof(VirtualizingDataRecordCellPanel), true) as VirtualizingDataRecordCellPanel;

				if (null == cellPanel)
					return null;
			}

            cellValuePresenter = cellPanel.GetCellValuePresenter( field );

			// SSP 8/28/09 - Enhanced grid-view - TFS21591
			// Is the field is expandable then its cvp will not be in the virtualizing cell panel.
			// We need to actually find the expandable field record presenter and get the cvp from
			// it.
			// 
			//if ( null == cellValuePresenter && null != cell && field.IsExpandable == true )
			//	cellValuePresenter = GetCVPForExpandableFieldHelper( field, record );

			// MD 8/20/10
			// We don't need the CVPCache anymore. We can get similar gains by caching the cell panel on the data record presenter.
			//// set the associated cvp on the cell to improve performance the next time
			//// we are looking for the presenter
			//if ( cellValuePresenter != null )
			//{
			//    if ( cell != null )
			//    {
			//        // If isCellBeingInitialized is true then the Cell's AssociatedCellValuePresenterInternal
			//        // is currently being initialized and we don't need to set it here.
			//        // 
			//        if ( ! isCellCVPBeingInitialized )
			//            cell.AssociatedCellValuePresenterInternal = cellValuePresenter;
			//    }
			//    // If the cell hasn't been allocated then cache cvp's in the cvpCache.
			//    // 
			//    else if ( null != cvpCache )
			//    {
			//        cvpCache.Add( record, field, cellValuePresenter );
			//    }
			//}
			if (cellValuePresenter != null && cell != null)
				cell.AssociatedCellValuePresenterInternal = cellValuePresenter;

			return cellValuePresenter;

            
#region Infragistics Source Cleanup (Region)





















































#endregion // Infragistics Source Cleanup (Region)

		}

				#endregion //FromRecordAndField

			#endregion //Public Methods

			#region Internal Methods

			#region ActivateAndEnterEditMode

		
		
		/// <summary>
		/// Activates and enters the cell into edit mode.
		/// </summary>
		/// <returns></returns>
		internal bool ActivateAndEnterEditMode( )
		{
			DataPresenterBase dp = this.DataPresenter;

			// If the cell is not already active, activate it first.
			if ( !this.IsActive )
				( (ISelectionHost)dp ).ActivateItem( this.Cell, false );

			if ( this.IsActive && !this.IsInEditMode )
			{
				// enter edit mode if editing is allowed
				if ( this.IsEditingAllowed )
				{
					this.DataPresenter.UpdateLayout( );
					this.StartEditMode( );
					return true;
				}
			}

			return false;
		}

			#endregion // ActivateAndEnterEditMode

			#region CommitEditValueHelper

		internal bool CommitEditValueHelper( object editedValue )
		{
			Cell cell = this.Cell;
			Field field = null != cell ? cell.Field : null;
			DataPresenterBase dp = this.DataPresenter;

			Debug.Assert( null != field && null != dp );
			if ( null == field || null == dp )
				return false;

			
#region Infragistics Source Cleanup (Region)






































#endregion // Infragistics Source Cleanup (Region)

			//// ----------------------------------------------------------------------
			this.Record.CoerceNullEditValue(field, ref editedValue);

            // AS 4/15/09 Field.(Get|Set)CellValue
			//bool success = field.SetCellValue( cell.Record, editedValue, true );
			// AS 5/22/09 NA 2009.2 Undo/Redo
			// Added another parameter to provide the original value to store in the undo operation. This 
			// is necessary because while in edit mode the value of the cell could have been changed. This 
			// happens for example in a filter cell - the value is changed as you type.
			//
            //bool success = cell.Record.SetCellValue(field, editedValue, true);
			// AS 5/28/09 NA 2009.2 Undo/Redo
			// For the filter cell we will let it store the original value which includes the operator.
			//
			bool addToUndo = cell.Record.IsDataRecord;
            bool success = cell.Record.SetCellValue(field, editedValue, true, addToUndo, false, null != this.Editor ? this.Editor.OriginalValue : DataRecord.UndoValueMissing, true);

			// AS 5/28/09 NA 2009.2 Undo/Redo
			// Added a virtual that the filter cvp could use to add its undo action for the commit.
			//
			this.OnCommitEditValue(editedValue);

			if ( success && cell.IsDataChanged )
			{
				// if the update mode is on cell change then we need to
				// commit any changes at this pointunless the record is 
				// an add record
				if ( cell.Record != null &&
					cell.Record.IsAddRecord == false )
				{
					switch ( dp.UpdateMode )
					{
						case UpdateMode.OnCellChange:
						case UpdateMode.OnCellChangeOrLostFocus:
							if (cell.Record.IsDataChangedSinceLastCommitAttempt)
							{
								// JJD 5/23/07 - BR23169
								// Call the Update method instead so the RecordUpdating and RecordUpdated events get raised
								//cell.Record.CommitChanges();
								cell.Record.Update();
							}
							break;
					}
				}
			}

			return success;
		}

			#endregion // CommitEditValueHelper

			#region FocusIfAppropriate

		internal void FocusIfAppropriate()
		{
			DataPresenterBase dp = this.DataPresenter;

			if (dp == null)
				return;

			if (false == (bool)this.GetValue(FocusWithinManager.IsFocusWithinProperty) )
			{
				// JJD 3/12/07
				// if the datapresenter has keyboard focus within we want to take keyboard focus
				if (dp.IsKeyboardFocusWithin)
				{
					// AS 7/19/07 BR25044
					// If we're in edit mode, then put focus into 
					// the editor instead of the cell.
					//
					if (this.Editor != null && this.Editor.IsInEditMode)
						this.Editor.Focus();
					else
						this.Focus();
				}
				else
					// JJD 3/14/07
					// Otherwise if the datapresenter has logial focus within we want to take logical focus
					if (true == (bool)dp.GetValue(FocusWithinManager.IsFocusWithinProperty))
					{
						DependencyObject scope = FocusManager.GetFocusScope(this);

						if (scope != null)
						{
							// AS 5/11/12 TFS104724
							//FocusManager.SetFocusedElement(scope, this);
							Utilities.SetFocusedElement(scope, this);
						}
					}
			}
		}

			#endregion //FocusIfAppropriate	
			
			// JJD 04/12/12 - TFS108549 - Optimization
			#region MarkValueDirty

		internal void MarkValueDirty()
		{
			_isValueDirty = true;
		}

			#endregion //MarkValueDirty	
    

            // JJD 12/24/08 - added
            #region OnCellSettingsInitialized

        /// <summary>
        /// Celled when all the cell settings have been initialized
        /// </summary>
        protected virtual void OnCellSettingsInitialized() { }

            #endregion //OnCellSettingsInitialized	
    
			// AS 5/28/09 NA 2009.2 Undo/Redo
			#region OnCommitEditValue
		internal virtual void OnCommitEditValue(object editedValue)
		{
		} 
			#endregion //OnCommitEditValue

			#region OnIsFocusWithinChanged

		/// <summary>
		/// Called when the element either recieves or loses the logical focus.
		/// </summary>
		/// <param name="gotFocus"></param>
		internal void OnIsFocusWithinChanged( bool gotFocus )
		{
			if ( gotFocus && !(this is ExpandedCellPresenter))
			{
				Field field = this.Field;

				// SSP 9/9/09 TFS19158
				// 
				DataRecord record = this.Record;

				if ( field == null ||
					 field.Owner == null
					// SSP 9/9/09 TFS19158
					// 
					|| null == record
					)
					return;

                // AS 1/22/09 NA 2009 Vol 1 - Fixed Fields
                // The cell could end shifting focus within itself but we want the 
                // entire cell to be brought into view in that case.
                //
                this.BringIntoView();

                // try to activate the cell
				if ( !this.IsActive )
				{
					// SSP 9/9/09 TFS19158
					// Added GetCellClickActionResolved virtual method on the DataRecord.
					// 
					//switch ( this.Field.CellClickActionResolved )
					switch ( record.GetCellClickActionResolved( field ) )
					{
						case CellClickAction.SelectCell:
						case CellClickAction.EnterEditModeIfAllowed:
							this.IsActive = true;
							break;
					}
				}
			}


            // JJD 4/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
            this.UpdateVisualStates();

        }

				#endregion // OnIsFocusWithinChanged

			#region SetValueInternal

		// SSP 1/21/09 TFS12327
		// See comments on CVPValueWrapper class for more info.
		// 
		/// <summary>
		/// Sets the Value property to the specified value.
		/// </summary>
		/// <param name="newValue">New value.</param>
		/// <param name="updateRecord">Indicates whether to update the
		/// data record's value with the new value.</param>
		internal void SetValueInternal( object newValue, bool updateRecord )
		{
			// JJD 04/12/12 - TFS108549 - Optimization
			// clear the _isValueDirty flag
			_isValueDirty = false;

			// JJD 3/11/11 - TFS67970 - Optimization
			// Cache and re-use the value wrapper
			//object v = updateRecord ? newValue 
			//    : new CVPValueWrapper( newValue, updateRecord );
			object v;
			if (updateRecord)
				v = newValue;
			else
			{
				if (_valueWrapper == null)
					_valueWrapper = new CVPValueWrapper(newValue, updateRecord);
				else
				{
					_valueWrapper._value = newValue;
					_valueWrapper._updateRecord = updateRecord;
					
					// Since we are re-using the wrapper reset the coerced flag
					// so we whther the coerce logic was exectued after the property set below
					_WasCoerceValueCalled = false;
				}
				

				v = _valueWrapper;
			}

			this.SetValue( ValueProperty, v );

			// JJD 3/11/11 - TFS67970 - Optimization
			// If the coerce was'nt called (since we re-used the wrapper above then
			// force a coerce of the value
			if (_WasCoerceValueCalled == false)
				this.CoerceValue(ValueProperty);
		}

			#endregion // SetValueInternal

			// JM 11-12-08 TFS10196
			#region SyncValueWithCellValue

		// JJD 04/12/12 - TFS108549 - Optimization - added isRecordPresenterDeactivated parameter
		//internal static void SyncValueWithCellValue(DataRecord dataRecord, Field field)
		internal static void SyncValueWithCellValue(DataRecord dataRecord, Field field, bool isRecordPresenterDeactivated)
		{
			// get the cellvaluepresenter for the field
			CellValuePresenter cvp = CellValuePresenter.FromRecordAndField(dataRecord, field);

			// synchronize the cell value presenter's value property
			// JJD 5/27/08 - BR32640
			// Bypass updating the cell value if the cell is in edit mode and
			// the user has changed the value
			// if (cvp != null)
            if (cvp != null && (!cvp.IsInEditMode || !cvp.Editor.HasValueChanged))
            {
				// JJD 04/12/12 - TFS108549 - Optimization
				// If the record presenter is deactivated (i.e. unused) then we want to just 
				// mark the cvp's value dirty. When the cvp becomes visible again we will
				// re-sync the value
				if (isRecordPresenterDeactivated)
					cvp.MarkValueDirty();
				else
				{
					// SSP 1/21/09 TFS12327
					// Ensure that CVP doesn't try to update field value since it's being
					// set to the field value. See notes on CellValuePresenter.CVPValueWrapper 
					// class for more info.
					// 
					//cvp.Value = field.GetCellValue(dataRecord, true);
					// JJD 5/29/09 - TFS18063 
					// Use the new overload to GetCellValue which will return the value 
					// converted into EditAsType
					//cvp.SetValueInternal(field.GetCellValue(dataRecord, true), false);
					cvp.SetValueInternal(dataRecord.GetCellValue(field, CellValueType.EditAsType), false);
				}
            }
		}

			#endregion //SyncValueWithCellValue

			#endregion //Internal Methods

			#region Private Methods

				#region GetCVPForExpandableFieldHelper

		// SSP 8/28/09 - Enhanced grid-view - TFS21591
		// Is the field is expandable then its cvp will not be in the virtualizing cell panel.
		// We need to actually find the expandable field record presenter and get the cvp from
		// it.
		// 
		private static CellValuePresenter GetCVPForExpandableFieldHelper( Field expandableField, DataRecord record )
		{
			ExpandableFieldRecordCollection expandableRecordColl = record.ChildRecordsIfAllocated;

			ExpandableFieldRecord expandableRecord = null != expandableRecordColl
				? expandableRecordColl.GetItemIfExists( expandableField ) : null;

			ExpandableFieldRecordPresenter expandableRP = null != expandableRecord
				? expandableRecord.AssociatedRecordPresenter as ExpandableFieldRecordPresenter : null;

			if ( null != expandableRP )
			{
				Utilities.DependencyObjectSearchCallback<CellValuePresenter> callback = new Utilities.DependencyObjectSearchCallback<CellValuePresenter>( delegate( CellValuePresenter cvp )
				{
					// return true if the field and the record matches
					return cvp.Field == expandableField && cvp.Record == record;
				} );

				return Utilities.GetDescendantFromType<CellValuePresenter>( expandableRP, true, callback );
			}

			return null;
		}

				#endregion // GetCVPForExpandableFieldHelper

				// JJD 5/25/07 - Optimization - refactored
				#region InitializeCellLevelSettings

		private void InitializeCellLevelSettings(DataRecord newRecord, DataRecord oldRecord, Field newField, Field oldField, bool isWithinRecord)
		{
			// AS 8/4/09 NA 2009.2
			// Moved to a helper method.
			//
			//if (this._cell != null)
			//{
			//    // JJD 1/24/08 - BR29985
			//    // clear out the old cell's AssociatedCellValuePresenterInternal
			//    if ( this._cell.AssociatedCellValuePresenterInternal == this)
			//        this._cell.AssociatedCellValuePresenterInternal = null;
			//
			//    // JJD 5/31/07
			//    // Always clear the cached cell reference
			//    this._cell = null;
			//}
			this.ReleaseCell();

			//this.InvalidateProperty(CellValuePresenter.ContentProperty);
			if (newRecord == null || newField == null)
				return;

			// AS 1/27/10 NA 2010.1 - CardView
			// The value may not change but the associated record's should collapse cells may be different.
			// Technically the Field's visibility/contentalignment/etc could be different if recycled with 
			// a different cell.
			//
			if (newRecord.FieldLayout.IsEmptyCellCollapsingSupportedByView)
				this.CoerceValue(VisibilityProperty);

			Debug.Assert(newField.Index >= 0);

            bool isFilterCell = newRecord.RecordType == RecordType.FilterRecord;

			DataPresenterBase dp = newRecord.DataPresenter;

			// JJD 1/25/07 - BR19365
			// Get the cell if allocated
			//Cell cell = newRecord.Cells.GetCellIfAllocated(newField);
            Cell cell;

            // JJD 1/24/08 - BR29985
            // Always get the cell if it is unbound since it will always get
            // crfeated anyway when we go the get the value
            //if (newField.IsUnbound)
            if (newField.IsUnbound || isFilterCell)
                cell = newRecord.Cells[newField];
            else
			    cell = newRecord.GetCellIfAllocated(newField);

            // JJD 1/24/08 - BR29985
            // update the member variable
            this._cell = cell;
            
			// initialize the slected status if a cell has been allocated
			// AS 3/22/07 BR21259
			//if (cell != null)
			if (cell != null && isWithinRecord)
			{
				cell.AssociatedCellValuePresenterInternal = this;
				
				// JJD 3/9/11 - TFS67970 - Optimization
				//this.SetValue(IsSelectedProperty, KnownBoxes.FromValue(cell.IsSelected));
				if (cell.IsSelected )
					this.SetValue(IsSelectedProperty, KnownBoxes.TrueBox);
				else
					this.ClearValue(IsSelectedProperty);
			}
			else
			{
				// AS 3/22/07 BR21259
				this.ClearValue(IsSelectedProperty);
			}

			// MD 8/20/10
			// We don't need the CVPCache anymore. We can get similar gains by caching the cell panel on the data record presenter.
			////~ SSP 6/7/10 - Optimizations - TFS34031
			////~ 
			////~ ------------------------------------------------------------------------------------------
			//CVPCache cvpCache = dp._cvpCache;
			//if ( null != cvpCache && cvpCache.IsCaching )
			//{
			//    cvpCache.Remove( oldRecord, oldField );
			//    cvpCache.Add( newRecord, newField, this );
			//}
			////~ ------------------------------------------------------------------------------------------

			FieldLayout fl = newField.Owner;

			Debug.Assert(fl != null);

            // AS 12/17/08 NA 2009 Vol 1 - Fixed Fields 
			//if (!(this is ExpandedCellPresenter))
			//	this.InitializeWidthAndHeight(newRecord, newField, isWithinRecord);

            // JJD 11/12/09 - TFS24752
            // Get the cell value once and cache it so we don't call the get accessor
            // on the data item multiple times
            object cellValue = this.Value;

			if (fl != null)
			{
				DataPresenterBase layoutOwner = fl.DataPresenter;

                // JJD 2/17/09 - TFS14047
                // Moved logic into InitalizeEditor method
                #region Old code

                //if (layoutOwner != null)
                //{
                //    // AS 5/24/07 Recycle elements
                //    // This body used to be within the "if (record != this._dataRecord)"
                //    // block above but since we can recycle cells for different fields
                //    // but for the same record, we should be getting into this routine
                //    // to reinitialize the editor type, edit as type, etc.
                //    //
                //    Type editAsType = null;
                //    Type editorType = null;
                //    Style editorStyle = null;

                //    // JJD 12/22/08
                //    // For filter cells use the new properties exposed off the field to
                //    // get the editor type and style
                //    if ( isFilterCell )
                //    {
                //        editorType = newField.FilterEditorTypeResolved;
                //        editAsType = newField.FilterEditAsTypeResolved;
                //    }
                //    else 
                //    {
                //        if (cell != null)
                //        {
                //            // set the editor type
                //            editorType = cell.EditorTypeResolved;
                //            // set the ValueType to EditAsType
                //            editAsType = cell.EditAsTypeResolved;
                //            // set the editor style
                //            editorStyle = cell.EditorStyle;
                //        }
                //        else
                //        {
                //            // set the editor type
                //            editorType = newField.EditorTypeResolved;
                //            // set the ValueType to EditAsType
                //            editAsType = newField.EditAsTypeResolved;
                //        }

                //        // it is wasn't set at a cell level then call the select style logic 
                //        if (editorStyle == null)
                //            editorStyle = layoutOwner.InternalEditorStyleSelector.SelectStyle(this.Value, this);
                //    }


                //    // JJD 5/25/07 - Optimization
                //    // Added InitializeEditorSettings method
                //    // So we can do an atomic update of all 3 values so we don't trigger
                //    // multiple invalidations of the editor
                //    //// set the editor type
                //    //this.EditorType = editorType;
                //    //// set the ValueType to EditAsType
                //    //this.ValueType = editAsType;
                //    //// set the editor style
                //    //this.EditorStyle = editorStyle;
                //    this.InitializeEditorSettings(editorType, editAsType, editorStyle);
                //}
                #endregion //Old code	
                // JJD 11/12/09 - TFS24752
                // Pass the cached cell value so we don't call the get accessor
                // on the data item multiple times
                //this.InitializeEditor();
                this.InitializeEditor(cellValue);

				// initialize the active property
				// AS 3/22/07 BR21259
				// Do not reflect the active state within a cell unless it is in a record.
				//
				//if (this.IsActive)
				if (isWithinRecord && this.IsActive)
					this.SetValue(IsActiveProperty, KnownBoxes.TrueBox);
				else
					this.ClearValue(IsActiveProperty);

                
#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

			}

			// set a flag so we don't try setting the cell value during the Value property coerce
			this._initializingValue = true;
			try
			{
				// SSP 1/21/09 TFS12327
				// Ensure that CVP doesn't try to update field value since it's being
				// set to the field value. See notes on CellValuePresenter.CVPValueWrapper 
				// class for more info.
				// 
				//this.SetValue(ValueProperty, this.Value);

                // JJD 11/12/09 - TFS24752
                // Use the cached cell value so we don't call the get accessor
                // on the data item multiple times
                //this.SetValueInternal(this.Value, false);
                //
                //if (this.Editor != null)
                //    this.Editor.Value = this.Value;
                this.SetValueInternal(cellValue, false);

				// JJD 3/9/11 - TFS67970 - Optimization
				// Get the Editor property once
				ValueEditor editor = this.Editor;
				if (editor != null)
					editor.Value = cellValue;
			}
			finally
			{
				// reset the flag
				this._initializingValue = false;
			}

			// SSP 4/17/09 NAS9.2 IDataErrorInfo Support
			// 
			this.UpdateDataError( );

			this.InvalidateStyleSelector();

            this.OnCellSettingsInitialized();

			// JM 6/12/09 NA 2009.2 DataValueChangedEvent
			if (fl is FieldLayout			&& 
				fl.DataPresenter != null	&&
				this._dataRecord != null	&&
				this._dataRecord is TemplateDataRecord == false &&
				newField.DataValueChangedNotificationsActiveResolved)
			{
				bool isNew = (newRecord != oldRecord && oldRecord != null) ? false : true;
				

				// Make sure this CVP's ValueHistory proeprty is set to its associated cell's value.
				// JJD 07/16/02 - TFS117265
				// Instead of checking if ValueHistory is null check to see if the history
				// has changed (which it can when presenters are recycled)
				//if (this.ValueHistory == null)
				DataValueInfoHistory history = this.Cell.GetDataValueHistoryCache(this);
				if (this.ValueHistory != history)
				{
					// JJD 3/11/11 - TFS67970 - Optimization 
					// Call the new GetDataValueHistoryCache which takes the cvp as a param 
					// which eliminates the overhead of getting it from within the DataValueInfoHistoryCache getter
					//this.SetValue(CellValuePresenter.ValueHistoryPropertyKey, this.Cell.DataValueInfoHistoryCache);
					// JJD 07/16/02 - TFS117265
					// Use the DataValueInfoHistory instance returned above
					//this.SetValue(CellValuePresenter.ValueHistoryPropertyKey, this.Cell.GetDataValueHistoryCache(this));
					this.SetValue(CellValuePresenter.ValueHistoryPropertyKey, history);
				}

				// If the DataValueChangedScope for the associated Field is OnlyRecordsInView, initialize the history with the current Value.
				// Note that for DataValueChangedScope = AllAllocatedRecords this initialization is done when we fire the InitalizeRecord event
				// in Record.FireInitializeRecord.
				if (newField.DataValueChangedScopeResolved == DataValueChangedScope.OnlyRecordsInView)
					this.Cell.AddDataValueChangedHistoryEntryForCurrentValue();

				Infragistics.Windows.DataPresenter.Events.InitializeCellValuePresenterEventArgs args
					= new Infragistics.Windows.DataPresenter.Events.InitializeCellValuePresenterEventArgs(this, isNew);

				fl.DataPresenter.RaiseInitializeCellValuePresenter(args);
			}
		}

				#endregion //InitializeCellLevelSettings	

                // JJD 2/17/09 - TFS14047 - added
                #region InitializeEditor

        // JJD 11/12/09 - TFS24752
        // added cellValue param
        //internal void InitializeEditor()
        internal void InitializeEditor(object cellValue)
        {
			// JJD 7/05/11 - TFS80466
			_invalidateEditorStylePending = false;

            Field field = this.Field;

            if (field == null)
                return;

            FieldLayout fl = field.Owner;

            Debug.Assert(fl != null);

            if (fl == null)
                return;

            // AS 12/17/08 NA 2009 Vol 1 - Fixed Fields 
            //if (!(this is ExpandedCellPresenter))
            //	this.InitializeWidthAndHeight(newRecord, newField, isWithinRecord);

            DataPresenterBase dp = fl.DataPresenter;

            Debug.Assert(dp != null);

            if (dp == null)
                return;

            bool isFilterCell = this._cell is FilterCell;

            // AS 5/24/07 Recycle elements
            // This body used to be within the "if (record != this._dataRecord)"
            // block above but since we can recycle cells for different fields
            // but for the same record, we should be getting into this routine
            // to reinitialize the editor type, edit as type, etc.
            //
            Type editAsType = null;
            Type editorType = null;
            Style editorStyle = null;

			
#region Infragistics Source Cleanup (Region)































#endregion // Infragistics Source Cleanup (Region)

			if (null == _dataRecord)
				return;

			editorType = _dataRecord.GetEditorTypeResolved(field);
			editAsType = _dataRecord.GetEditAsTypeResolved(field);

			if (!isFilterCell)
			{
				// JM 05-05-11 TFS34566
				if (null == this._cell)
					this._cell = this._dataRecord.GetCellIfAllocated(field);

				if (this._cell != null)
					editorStyle = this._cell.EditorStyle;

				// it is wasn't set at a cell level then call the select style logic 
                if (editorStyle == null)
                {
                    // JJD 11/12/09 - TFS24752
                    // Use the passed in cell value so we don't call the get accessor
                    // on the data item multiple times
                    //editorStyle = dp.InternalEditorStyleSelector.SelectStyle(this.Value, this);
                    editorStyle = dp.InternalEditorStyleSelector.SelectStyle(cellValue, this);
                }
			}


            // JJD 5/25/07 - Optimization
            // Added InitializeEditorSettings method
            // So we can do an atomic update of all 3 values so we don't trigger
            // multiple invalidations of the editor
            //// set the editor type
            //this.EditorType = editorType;
            //// set the ValueType to EditAsType
            //this.ValueType = editAsType;
            //// set the editor style
            //this.EditorStyle = editorStyle;
            this.InitializeEditorSettings(editorType, editAsType, editorStyle);
        }

                #endregion //InitializeEditor	
    
				// JJD 5/25/07 - Optimization - refactored
				#region InitializeField

		private void InitializeField(Field newField, Field oldField)
		{
			bool isWithinRecord = false;

			if (newField == null)
			{
				// JJD 3/9/11 - TFS67970 - Optimization
				// Replace binding to field with code in OnFieldPropertyChanged
				//if (oldField != null)
				//{
				//    BindingOperations.ClearBinding(this, IsFieldSelectedProperty);
				//    BindingOperations.ClearBinding(this, SortStatusInternalProperty);
				//    this.ClearValue(IsSelectedProperty);
				//}
			}
			else
			{
				// AS 3/22/07 BR21259
				// There are certain settings that we do not want to reflect in the element
				// unless the element is within a record.
				//
				isWithinRecord = this.IsWithinRecord;

				// AS 5/23/07 BR23120
				// The CellContentAlignment is really the relationship between the label 
				// and the cell value presenter when using cell presenters and should not
				// affect how the value of the cell is aligned within the cell value presenter.
				// We may in the future add properties to FieldSettings to control these - in 
				// which case we'll have to make sure that we resolve to default and then clear
				// the local value if that is the case so we don't override any setting they
				// may have done in the style.
				//
				//CellContentAlignment cellContentAlignment = field.CellContentAlignmentResolved;
				//
				//this.SetValue(HorizontalContentAlignmentProperty, KnownBoxes.FromValue(FieldLayoutTemplateGenerator.GetHorizontalAlignmentCell(cellContentAlignment)));
				//this.SetValue(VerticalContentAlignmentProperty, KnownBoxes.FromValue(FieldLayoutTemplateGenerator.GetVerticalAlignmentCell(cellContentAlignment)));
				
				// JJD 3/9/11 - TFS67970 - Optimization
				// Replace binding to field with code in OnFieldPropertyChanged
				//this.SetBinding(SortStatusInternalProperty, Utilities.CreateBindingObject(Field.SortStatusProperty, BindingMode.OneWay, newField));
				this.SetValue(SortStatusInternalProperty, newField.SortStatus);

				// AS 5/23/07
				// There is no need to do this as we were already conditionally binding to this
				// below as long as this cell value presenter is within a record.
				//
				//// JJD 5/1/07
				//// Bind IsFieldSelectedProperty to field's IsSelected property
				//this.SetBinding(IsFieldSelectedProperty, Utilities.CreateBindingObject(Field.IsSelectedProperty, BindingMode.OneWay, field));

				// AS 3/22/07 BR21259
				// JJD 3/9/11 - TFS67970 - Optimization
				// Replace binding to field with code in OnFieldPropertyChanged
				//if (isWithinRecord)
				//    this.SetBinding(IsFieldSelectedProperty, Utilities.CreateBindingObject(Field.IsSelectedProperty, BindingMode.OneWay, newField));
				if ( isWithinRecord && newField.IsSelected)
					this.SetValue(IsFieldSelectedProperty, KnownBoxes.TrueBox);
				else
				    this.ClearValue(IsFieldSelectedProperty);

				FieldLayout fl = newField.Owner;

				Debug.Assert(fl != null);

				if (!(this is ExpandedCellPresenter))
				{
					if (!newField.IsExpandableResolved)
					{
						// JJD 3/9/11 - TFS67970 - Optimization
						// Cache the content binding
						//this.SetBinding(CellValuePresenter.ContentProperty, Utilities.CreateBindingObject(CellValuePresenter.ValueProperty, BindingMode.TwoWay, this));
						if (_contentBinding == null)
						{
							_contentBinding = Utilities.CreateBindingObject(CellValuePresenter.ValueProperty, BindingMode.TwoWay, this);
							this.SetBinding(CellValuePresenter.ContentProperty, _contentBinding);
						}

						// JJD 4/13/07
						// This is no longer necessary becuase we are binding the UnboundCell's Value instead
						//this.InitializeUnboundFieldBinding();
					}
					else
					{
						// JJD 3/9/11 - TFS67970 - Optimization
						// clear the cached binding
						if (_contentBinding != null)
						{
							_contentBinding = null;
							BindingOperations.ClearBinding(this, CellValuePresenter.ContentProperty);
						}
					}
				}

				// AS 3/22/07 BR21259
				// Don't tie the visibility of the cell to that of the newField unless it
				// is used within a record. They could make the scroll tip newField a
				// newField that is hidden to provide additional information.
				//
				if (isWithinRecord)
				{
					// AS 8/24/09 TFS19532
					// See GridUtilities.CoerceFieldElementVisibility for details.
					//
					//// JJD 2/21/07 - BR20007
					//// if the newField's VisibilityResolved is not Visible then syncronize this element's
					//// visibility property with it.
					//Visibility visibility = newField.VisibilityResolved;
					//
					//if (visibility != Visibility.Visible)
					//	this.Visibility = visibility;
					// JJD 3/9/11 - TFS67970 - Optimization
					// Replace binding to field with code in OnFieldPropertyChanged
					//this.SetBinding(GridUtilities.FieldVisibilityProperty, Utilities.CreateBindingObject("VisibilityResolved", BindingMode.OneWay, newField));
					this.SetValue(GridUtilities.FieldVisibilityProperty, newField.VisibilityResolved);

					// JJD 3/9/11 - TFS67970 - Optimization
					// Replace binding to field with code in OnFieldPropertyChanged
					// AS 8/26/09 CellContentAlignment
					//this.SetBinding(GridUtilities.CellContentAlignmentProperty, Utilities.CreateBindingObject("CellContentAlignmentResolved", BindingMode.OneWay, newField));
					this.SetValue(GridUtilities.CellContentAlignmentProperty, newField.CellContentAlignmentResolved);
				}
				else
				{
					// JJD 3/9/11 - TFS67970 - Optimization
					// Replace binding to field with code in OnFieldPropertyChanged
					// AS 8/24/09 TFS19532
					//BindingOperations.ClearBinding(this, GridUtilities.FieldVisibilityProperty);

					// AS 8/26/09 CellContentAlignment
					//BindingOperations.ClearBinding(this, GridUtilities.CellContentAlignmentProperty);
					this.ClearValue(GridUtilities.FieldVisibilityProperty);
					this.ClearValue(GridUtilities.CellContentAlignmentProperty);
				}

				if (fl != null)
				{
					// JJD 5/1/07
					// Initialize the HasSeparateHeader property
					this.SetValue(HasSeparateHeaderProperty, fl.HasSeparateHeader);
				}
			}

			this.InitializeCellLevelSettings(this._dataRecord, this._dataRecord, newField, oldField, isWithinRecord);
		}
				
				#endregion //InitializeField	

				// JJD 5/25/07 - Optimization - refactored
				#region InitializeRecord

		private void InitializeRecord(DataRecord newRecord, DataRecord oldRecord)
		{
			// MD 6/22/11 - TFS62384
			// We need to keep track of when we are initializing the record.
			bool oldIsInitializingRecord = _isInitializingRecord;
			_isInitializingRecord = true;

			// JJD 3/08/07
			// added support for reusing these elements for different records
			if (oldRecord != null)
				this.UnhookAndClearRecord();
			
			this._dataRecord = newRecord;

			if (this._dataRecord == null)
				return;

			// AS 3/22/07 BR21259
			// There are certain settings that we do not want to reflect in the element
			// unless the element is within a record.
			//
			bool isWithinRecord = this.IsWithinRecord;

			// use the weak event manager to hook the event so we don't get rooted
			// AS 3/22/07 BR21259
			// We only listen to the newRecord for things like isactive, isselected, size change, etc.
			// which should not be reflected unless it is used within a newRecord.
			//
            if (isWithinRecord)
            {
                PropertyChangedEventManager.AddListener(this._dataRecord, this, string.Empty);

                // JJD 4/30/09 - TFS17157 
                // set the IsRecordSelectedPropertyKey 
                if (this._dataRecord.IsSelected)
                    this.SetValue(IsRecordSelectedPropertyKey, KnownBoxes.TrueBox);
                else
                    this.ClearValue(IsRecordSelectedPropertyKey);
            }

			Field fld = this.Field;

			// JJD 1/4/11 - TFS62509
			// Make sure the field is still in the collection before initializing it
			//if ( fld != null )
			if ( fld != null && fld.Index >= 0)
				this.InitializeCellLevelSettings(newRecord, oldRecord, fld, fld, isWithinRecord);

			// MD 6/22/11 - TFS62384
			// Reset the value.
			this._isInitializingRecord = oldIsInitializingRecord;
		}

				#endregion //InitializeRecord	

				// JJD 5/25/07 - Optimization - refactored
				#region InitializeWidthAndHeight

        
#region Infragistics Source Cleanup (Region)














































































#endregion // Infragistics Source Cleanup (Region)

				#endregion //InitializeWidthAndHeight	
    
				// JJD 5/25/07 - Optimization
				#region Obsolete code - refactored into InitializeRecord, InitializeField and InitializeCellLevelSettings

		//private void InitializeField()
		//{
		//    /* AS 5/24/07 Recycle elements
		//     * This was moved to the bottom of this routine because when recycling
		//     * cells, the Value for the cell may be of a different datatype.
		//     * 
		//    if (this.Record != null)
		//    {
		//        // set a flag so we don't try setting the cell value during the Value property coerce
		//        this._initializingValue = true;
		//        try
		//        {
		//            this.SetValue(ValueProperty, this.Value);

		//            if (this.Editor != null)
		//                this.Editor.Value = this.Value;
		//        }
		//        finally
		//        {
		//            // reset the flag
		//            this._initializingValue = false;
		//        }
		//    }
		//    */

		//    this.InvalidateProperty(CellValuePresenter.ContentProperty);

		//    Field field = this.Field;

		//    if (field == null)
		//        this.UnhookAndClearRecord();
		//    else
		//    {
		//        // AS 3/22/07 BR21259
		//        // There are certain settings that we do not want to reflect in the element
		//        // unless the element is within a record.
		//        //
		//        bool isWithinRecord = this.IsWithinRecord;

		//        // AS 5/23/07 BR23120
		//        // The CellContentAlignment is really the relationship between the label 
		//        // and the cell value presenter when using cell presenters and should not
		//        // affect how the value of the cell is aligned within the cell value presenter.
		//        // We may in the future add properties to FieldSettings to control these - in 
		//        // which case we'll have to make sure that we resolve to default and then clear
		//        // the local value if that is the case so we don't override any setting they
		//        // may have done in the style.
		//        //
		//        //CellContentAlignment cellContentAlignment = field.CellContentAlignmentResolved;
		//        //
		//        //this.SetValue(HorizontalContentAlignmentProperty, KnownBoxes.FromValue(FieldLayoutTemplateGenerator.GetHorizontalAlignmentCell(cellContentAlignment)));
		//        //this.SetValue(VerticalContentAlignmentProperty, KnownBoxes.FromValue(FieldLayoutTemplateGenerator.GetVerticalAlignmentCell(cellContentAlignment)));
		//        this.SetBinding(SortStatusInternalProperty, Utilities.CreateBindingObject(Field.SortStatusProperty, BindingMode.OneWay, field));

		//        // AS 5/23/07
		//        // There is no need to do this as we were already conditionally binding to this
		//        // below as long as this cell value presenter is within a record.
		//        //
		//        //// JJD 5/1/07
		//        //// Bind IsFieldSelectedProperty to field's IsSelected property
		//        //this.SetBinding(IsFieldSelectedProperty, Utilities.CreateBindingObject(Field.IsSelectedProperty, BindingMode.OneWay, field));

		//        // AS 3/22/07 BR21259
		//        if (isWithinRecord)
		//            this.SetBinding(IsFieldSelectedProperty, Utilities.CreateBindingObject(Field.IsSelectedProperty, BindingMode.OneWay, field));
		//        else
		//            BindingOperations.ClearBinding(this, IsFieldSelectedProperty);

		//        DataRecord record = this.Record;
 
		//        Cell cell = null;

		//        // JJD 1/25/07 - BR19365
		//        // Get the cell if allocated
		//        if (record != null && record.Cells != null)
		//            cell = record.Cells.GetCellIfAllocated(field);

		//        DataPresenterBase dp = field.Owner.DataPresenter;

		//        // initialize the slected status if a cell has been allocated
		//        // AS 3/22/07 BR21259
		//        //if (cell != null)
		//        if (cell != null && isWithinRecord)
		//        {
		//            cell.AssociatedCellValuePresenterInternal = this;
		//            this.SetValue(IsSelectedProperty, KnownBoxes.FromValue(cell.IsSelected));
		//        }
		//        else
		//        {
		//            // AS 3/22/07 BR21259
		//            this.ClearValue(IsSelectedProperty);
		//        }

		//        FieldLayout fl = field.Owner;

		//        Debug.Assert(fl != null);

		//        if (!(this is ExpandedCellPresenter))
		//        {
		//            this.MinWidth = field.CellMinWidthResolved;
		//            this.MinHeight = field.CellMinHeightResolved;
		//            this.MaxWidth = field.CellMaxWidthResolved;
		//            this.MaxHeight = field.CellMaxHeightResolved;

		//            // AS 3/22/07 BR21259
		//            // The record sizing information should not be reflected unless
		//            // the cell is within a record.
		//            //
		//            if (isWithinRecord)
		//            {
		//                if (!this._individuallySized && fl != null)
		//                {
		//                    if (fl.DataRecordSizingModeResolved == DataRecordSizingMode.IndividuallySizable)
		//                    {
		//                        bool isWidth = fl.IsHorizontal;

		//                        if (isWidth)
		//                            this.Width = field.CellWidthResolved;
		//                        else
		//                            this.Height = field.CellHeightResolved;
		//                    }
		//                }
		//            }

		//            // for add records makes sure we have a reasonable minimum size
		//            ViewBase currentView = dp.CurrentViewInternal;
		//            if (currentView.HasLogicalOrientation &&
		//                record != null && record.IsAddRecord)
		//            {
		//                double minsize;
		//                DependencyProperty property;
		//                if (currentView.LogicalOrientation == Orientation.Vertical)
		//                {
		//                    property = MinHeightProperty;
		//                    minsize = 10;
		//                }
		//                else
		//                {
		//                    property = MinWidthProperty;
		//                    minsize = 60;
		//                }

		//                double value = (double)this.GetValue(property);

		//                // if a minimum value wasn't set then do so now
		//                if (double.IsNaN(value) || value < minsize)
		//                    this.SetValue(property, minsize);
		//            }

		//            if (!field.IsExpandableResolved)
		//            {
		//                this.SetBinding(CellValuePresenter.ContentProperty, Utilities.CreateBindingObject(CellValuePresenter.ValueProperty, BindingMode.TwoWay, this));
						
		//                // JJD 4/13/07
		//                // This is no longer necessary becuase we are binding the UnboundCell's Value instead
		//                //this.InitializeUnboundFieldBinding();
		//            }
		//        }

		//        // AS 3/22/07 BR21259
		//        // Don't tie the visibility of the cell to that of the field unless it
		//        // is used within a record. They could make the scroll tip field a
		//        // field that is hidden to provide additional information.
		//        //
		//        if (isWithinRecord)
		//        {
		//            // JJD 2/21/07 - BR20007
		//            // if the field's VisibilityResolved is not Visible then syncronize this element's
		//            // visibility property with it.
		//            Visibility visibility = field.VisibilityResolved;

		//            if (visibility != Visibility.Visible)
		//                this.Visibility = visibility;
		//        }

		//        if (fl != null)
		//        {
		//            // JJD 5/1/07
		//            // Initialize the HasSeparateHeader property
		//            this.SetValue(HasSeparateHeaderProperty, fl.HasSeparateHeader);

		//            #region Initialize DataRecord

		//            DataPresenterBase layoutOwner = fl.DataPresenter;

		//            if (layoutOwner != null)
		//            {
		//                if (record != this._dataRecord)
		//                {
		//                    this.UnhookAndClearRecord();

		//                    this._dataRecord = record;

		//                    // hook datarecord's property changed event
		//                    if (this._dataRecord != null)
		//                    {
		//                        // use the weak event manager to hook the event so we don't get rooted
		//                        // AS 3/22/07 BR21259
		//                        // We only listen to the record for things like isactive, isselected, size change, etc.
		//                        // which should not be reflected unless it is used within a record.
		//                        //
		//                        if (isWithinRecord)
		//                            PropertyChangedEventManager.AddListener(this._dataRecord, this, string.Empty);
		//                    }
		//                }

		//                // AS 5/24/07 Recycle elements
		//                // This body used to be within the "if (record != this._dataRecord)"
		//                // block above but since we can recycle cells for different fields
		//                // but for the same record, we should be getting into this routine
		//                // to reinitialize the editor type, edit as type, etc.
		//                //
		//                {
		//                    if (this._dataRecord != null)
		//                    {
		//                        Type editAsType = null;
		//                        Type editorType = null;
		//                        Style editorStyle = null;

		//                        if (cell != null)
		//                        {
		//                            // set the editor type
		//                            editorType = cell.EditorTypeResolved;
		//                            // set the ValueType to EditAsType
		//                            editAsType = cell.EditAsTypeResolved;
		//                            // set the editor style
		//                            editorStyle = cell.EditorStyle;
		//                        }
		//                        else
		//                        {
		//                            // set the editor type
		//                            editorType = field.EditorTypeResolved;
		//                            // set the ValueType to EditAsType
		//                            editAsType = field.EditAsTypeResolved;
		//                        }

		//                        // it is wasn't set at a cell level then call the select style logic 
		//                        if (editorStyle == null)
		//                            editorStyle = layoutOwner.InternalEditorStyleSelector.SelectStyle(this.Value, this);

		//                        // set the editor type
		//                        this.EditorType = editorType;

		//                        // set the ValueType to EditAsType
		//                        this.ValueType = editAsType;

		//                        // set the editor style
		//                        this.EditorStyle = editorStyle;
		//                    }

		//                    // initialize the active property
		//                    // AS 3/22/07 BR21259
		//                    // Do not reflect the active state within a cell unless it is in a record.
		//                    //
		//                    //if (this.IsActive)
		//                    if (this.IsActive && isWithinRecord)
		//                    {
		//                        this.SetValue(IsActiveProperty, KnownBoxes.TrueBox);
		//                        this.Cell.AssociatedCellValuePresenterInternal = this;
		//                    }
		//                    else
		//                        this.ClearValue(IsActiveProperty);
		//                }
		//            }

		//            #endregion //Initialize DataRecord

		//            // AS 3/22/07 BR21259
		//            // Do not link the size of the cell to the size of the record 
		//            // unless the cell is within a record.
		//            //
		//            //if (!(this is ExpandedCellPresenter))
		//            if (!(this is ExpandedCellPresenter) && isWithinRecord)
		//                this.RefreshIndividualRecordSizeBindings();
		//        }
		//    }

		//    // AS 5/24/07 Recycle elements
		//    // This routine was moved down from the very top of the method
		//    // because it was causing first chance conversions when the datatype
		//    // of the field it was associated with was different than the 
		//    // datatype of the field its now associated with.
		//    //
		//    if (this.Record != null)
		//    {
		//        // set a flag so we don't try setting the cell value during the Value property coerce
		//        this._initializingValue = true;
		//        try
		//        {
		//            this.SetValue(ValueProperty, this.Value);

		//            if (this.Editor != null)
		//                this.Editor.Value = this.Value;
		//        }
		//        finally
		//        {
		//            // reset the flag
		//            this._initializingValue = false;
		//        }
		//    }

		//    this.InvalidateStyleSelector();
		//}

				#endregion //Obsolete code - refactored into InitializeRecord, InitializeField and InitializeCellLevelSettings	

				// JJD 4/13/07
				// This is no longer necessary becuase we are binding the UnboundCell's Value instead
				#region Obsolete code

		//#region InitializeUnboundFieldBinding

		//private void InitializeUnboundFieldBinding()
		//{
		//UnboundField unboundField = this.Field as UnboundField;

		//if (unboundField != null)
		//{
		//    DataRecord rcd = this.Record;

		//    if (rcd == null)
		//        return;

		//    PropertyPath bindingPath = unboundField.BindingPath;

		//    if (bindingPath != null)
		//    {
		//        Binding binding = new Binding();

		//        binding.Path = bindingPath;
		//        binding.Mode = unboundField.BindingMode;
		//        binding.Source = rcd.DataItem;
		//        try
		//        {
		//            this.SetBinding(ValueProperty, binding);
		//        }
		//        catch (Exception e)
		//        {
		//            throw new InvalidOperationException( SR.GetString( "LE_InvalidOperationException_6", unboundField.Name ), e );
		//        }
		//    }
		//}
		//}

		//#endregion //InitializeUnboundFieldBinding	

				#endregion //Obsolete code	
 
                // JJD 4/30/09 - TFS17157 - added
                #region OnRecordMouseOverChanged

		// JJD 3/11/11 - TFS67970 - Optimization 
		// This method is now being called from the VirtualizingDataRecordCellPanel
		//private void OnRecordMouseOverChanged()
        internal void OnRecordMouseOverChanged(bool isMouseOverCellArea)
        {
			//if (this._recordCellArea == null)
			//    return;

            //if (this._recordCellArea.IsMouseOver)
            if (isMouseOverCellArea)
                this.SetValue(IsMouseOverRecordPropertyKey, KnownBoxes.TrueBox);
            else
                this.ClearValue(IsMouseOverRecordPropertyKey);
        }

                #endregion //OnRecordMouseOverChanged	
        
				// MD 6/22/11 - TFS62384
				#region OnRequestBringIntoView

		private static void OnRequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
		{
			CellValuePresenter cvp = (CellValuePresenter)sender;

			// If the cell value presenting is currently in InitializeRecord, don't let it scroll into view, because it may be 
			// in InitializeRecord as a result of the user scrolling, in which case we don't want to step on their scroll position.
			if (cvp._isInitializingRecord)
				e.Handled = true;
		} 

				#endregion  // OnRequestBringIntoView

				// JJD 7/05/11 - TFS80466 - added
				#region ProcessReinitalizeStyleSelector

		private void ProcessReinitalizeStyleSelector()
		{
			// if the dirty flags have been cleared then ignore the notification
			if (_invalidateCVPStylePending == false && _invalidateEditorStylePending == false)
				return;

			// JJD 7/05/11 - TFS80466 
			// If the field has a style selector specified for the CVP or editor styles
			// we should invalidate them because the value has changed
			Field fld = this.Field;

			if (fld != null)
			{
				if (_invalidateCVPStylePending && fld.HasCellValuePresenterStyleSelector)
				{
					if (!this.IsInEditMode)
						this.InvalidateStyleSelector();
				}

				if (_invalidateEditorStylePending && fld.HasEditorStyleSelector)
				{
					if (!this.IsInEditMode)
						this.InitializeEditor(this.Value);
				}
			}

			_invalidateCVPStylePending = false;
			_invalidateEditorStylePending = false;
		}

				#endregion //ProcessReinitalizeStyleSelector	
    
				// AS 8/4/09 NA 2009.2
				#region ReleaseCell
		private void ReleaseCell()
		{
			if (this._cell != null)
			{
				// JJD 1/24/08 - BR29985
				// clear out the old cell's AssociatedCellValuePresenterInternal
				// JJD 3/9/11 - TFS67970 - Optimization
				// Call ClearAssociatedIfMatch which is more efficient
				//if (this._cell.AssociatedCellValuePresenterInternal == this)
				//    this._cell.AssociatedCellValuePresenterInternal = null;
				this._cell.ClearAssociatedIfMatch(this);

				// JJD 5/31/07
				// Always clear the cached cell reference
				this._cell = null;
			}
		} 
				#endregion //ReleaseCell

				#region RefreshIndividualRecordSizeBindings

        
#region Infragistics Source Cleanup (Region)































































#endregion // Infragistics Source Cleanup (Region)

				#endregion //RefreshIndividualRecordSizeBindings	

				// JJD 7/05/11 - TFS80466 - added
				#region ReinitalizeIfStyleSelectorsSpecified

		private void ReinitalizeIfStyleSelectorsSpecified()
		{
			// JJD 7/05/11 - TFS80466 
			// bypass if already pending
			if (this._invalidateEditorStylePending ||
				this._invalidateCVPStylePending ||
				this._dataRecord == null ||
				this._dataRecord.IsTemplateDataRecord ||
				this.Field == null)
				return;

			// JJD 7/05/11 - TFS80466 
			// If the field has a style selector specified for the CVP or editor styles
			// we should invalidate them because the value has changed
			Field fld = this.Field;

			if (fld != null)
			{
				if (fld.HasCellValuePresenterStyleSelector || fld.HasEditorStyleSelector)
				{
					if (!this.IsInEditMode)
					{
						this._invalidateCVPStylePending = true;
						this._invalidateEditorStylePending = true;

						this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new GridUtilities.MethodDelegate(this.ProcessReinitalizeStyleSelector));
					}
				}
			}
		}

				#endregion //ReinitalizeIfStyleSelectorsSpecified	
    
				#region UnhookAndClearRecord

		private void UnhookAndClearRecord()
		{
			// unhook from old datarecord
			if (this._dataRecord != null)
			{
				// AS 8/4/09 NA 2009.2
				// InitializeRecord bails out if there is no record so if the element was associated 
				// with a cell but is then released (via the DataContext going to null), the Record 
				// property will be null but the Cell property will still be set.
				//
				this.ReleaseCell();

				// unhook the event listener for the old record
				PropertyChangedEventManager.RemoveListener(this._dataRecord, this, string.Empty);
				this._dataRecord = null;
			}
		}

				#endregion //UnhookAndClearRecord

                #region WireCellArea

		// JJD 3/11/11 - TFS67970 - Optimization 
		// Wired DataRecordCellArea for mouse over logic from the VirtualizingDataRecordCellPanel instead
		//private void WireCellArea()
		//{
		//    if (RecordScrollTip.GetIsInRecordScrollTip(this))
		//    {
		//        Debug.Assert(this._recordCellArea == null);
		//        return;
		//    }

		//    DataRecordCellArea cellArea = Utilities.GetAncestorFromType(this, typeof(DataRecordCellArea), true, this.DataPresenter) as DataRecordCellArea;

		//    if (cellArea == this._recordCellArea)
		//        return;

		//    this._recordCellArea = cellArea;

		//    if (this._recordCellArea != null)
		//    {
		//        this._recordMouseOverTracker = new PropertyValueTracker(this._recordCellArea, "IsMouseOver", this.OnRecordMouseOverChanged);
		//        this.OnRecordMouseOverChanged();
		//    }
		//    else
		//    {
		//        this._recordMouseOverTracker = null;
		//        this.ClearValue(IsMouseOverRecordPropertyKey);
		//    }
		//}

                #endregion //WireCellArea	
    
			#endregion //Private Methods

		#endregion //Methods

		#region ISelectableElement Members

		ISelectableItem ISelectableElement.SelectableItem
		{
			get
			{
				DataRecord dr = this.Record as DataRecord;

				if (dr == null)
					return null;

				Infragistics.Windows.DataPresenter.Field fld = this.Field;

				if (fld == null)
					return null;

				// if the cell click action is set to select the record
				// then return the data record 
				// SSP 9/9/09 TFS19158
				// Added GetCellClickActionResolved virtual method on the DataRecord.
				// 
				//switch (fld.CellClickActionResolved)
				switch ( dr.GetCellClickActionResolved( fld ) )
				{
					case CellClickAction.SelectRecord:
						return dr;
				}

				Cell cell = dr.Cells[fld];

				Debug.Assert(cell != null);

				// AS 3/22/07 BR21259
				// This should never happen but just in case...
				//
				Debug.Assert(this.IsWithinRecord || this is ExpandedCellPresenter, "We should not be associating the cell with the cell value presenter unless it is within a record!");

				// have the cell cache this element 
				if (cell != null)
					cell.AssociatedCellValuePresenterInternal = this;

				return cell as ISelectableItem;
			}
		}

		#endregion

        // JJD 2/7/08 - BR30444 - added selector class
        #region CVPContentTemplateSelector private class

        internal class CVPContentTemplateSelector : DataTemplateSelector
        {
            #region Static Properties

            #region Instance

            static internal CVPContentTemplateSelector Instance = new CVPContentTemplateSelector();

            #endregion //Instance

            #endregion //Static Properties

            #region SelectTemplate

            public override DataTemplate SelectTemplate(object item, DependencyObject container)
            {
                ContentPresenter cp = container as ContentPresenter;

                Debug.Assert(cp != null, "This only applies to ContentPresenter");
                if (cp == null)
                    return null;

                // If the valkue is an ImageSource then provide a default
                ImageSource imageSource = item as ImageSource;
                if (imageSource != null)
                    return LabelPresenter.LPContentTemplateSelector.SelectTemplateForImageSource(imageSource, cp);

                return null;
            }

            #endregion //SelectTemplate
        }

        #endregion //CVPContentTemplateSelector private class
    }

	#endregion //CellValuePresenter

	#region ExpandedCellPresenter

	/// <summary>
	/// An element used to display the value of an ExpandableFieldRecord (that does not contain a list of items) when it is expanded.
	/// </summary>
	/// <remarks>
	/// <p class="body">Refer to the <a href="xamData_Terms_Fields.html">Fields</a> topic in the Developer's Guide for an explanation of fields.</p>
	/// <p class="body">Refer to the <a href="xamData_TheoryOfOperation.html">Theory of Operation</a> topic in the Developer's Guide for an explanation of how this object is used.</p>
	/// </remarks>
	/// <seealso cref="Cell"/>
	/// <seealso cref="CellValuePresenter"/>
	/// <seealso cref="Field"/>
	/// <seealso cref="ExpandableFieldRecord"/>
	public class ExpandedCellPresenter : CellValuePresenter
	{
		#region Private Members

		private DataPresenterBase _presenter;
		private ExpandableFieldRecord _record;

		#endregion //Private Members

		#region Constructors

		static ExpandedCellPresenter()
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(ExpandedCellPresenter), new FrameworkPropertyMetadata(typeof(ExpandedCellPresenter)));

			// JJD 1/25/07 - BR18936
			// Default to not stretching
			FrameworkElement.HorizontalAlignmentProperty.OverrideMetadata(typeof(ExpandedCellPresenter), new FrameworkPropertyMetadata(HorizontalAlignment.Left));
			FrameworkElement.VerticalAlignmentProperty.OverrideMetadata(typeof(ExpandedCellPresenter), new FrameworkPropertyMetadata(VerticalAlignment.Top));
		}

		internal ExpandedCellPresenter(DataPresenterBase dataPresenter, ExpandableFieldRecord record)
		{
			this._presenter = dataPresenter;
			this._record = record;

			// use the weak event manager to hook the event so we don't get rooted
			PropertyChangedEventManager.AddListener(this._record, this, string.Empty);

			this.SetBinding(RenderVersionProperty, Utilities.CreateBindingObject(DataPresenterBase.RenderVersionProperty, BindingMode.OneWay, dataPresenter));
		}

		#endregion //Constructors

		#region Base class overrides

			#region OnCoerceValue

		/// <summary>
		/// Called to coerce the value during a set
		/// </summary>
		/// <param name="value">The value to coerce</param>
		/// <returns>The coerced value.</returns>
		protected override object OnCoerceValue(object value)
		{
			// SSP 6/14/12 TFS114678
			// 
			CellValuePresenter.CVPValueWrapper wrapper = value as CellValuePresenter.CVPValueWrapper;
			if ( null != wrapper )
				value = wrapper._value;

			this.Content = value;
			return value;
		}

			#endregion //OnCoerceValue	

			#region OnFieldPropertyChanged

		/// <summary>
		/// Called when a property on the associated <see cref="Field"/> object changes
		/// </summary>
		/// <param name="e"></param>
		protected override void OnFieldPropertyChanged(PropertyChangedEventArgs e)
		{
			base.OnFieldPropertyChanged(e);
			
			switch (e.PropertyName)
			{
				// JJD 4/29/11 - TFS74075
				// Respond to changes to the ExpandedCellStyleResolved
				case "ExpandedCellStyleResolved":
					this.InvalidateStyleSelector();
					break;
			}
		}

			#endregion //OnFieldPropertyChanged
    
			#region OnPropertyChanged

		/// <summary>
		/// Called when a property has been changed
		/// </summary>
		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);
		}

		#	endregion //OnPropertyChanged

			#region OnRecordPropertyChanged

		/// <summary>
		/// Called when a property on the associated <see cref="Record"/> object changes
		/// </summary>
		/// <param name="e"></param>
		protected override void OnRecordPropertyChanged(PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "IsExpanded")
			{
				if (!this._record.IsExpanded)
				{
					this.ClearValue(ContentProperty);

					// unhook the event listener for the record
					PropertyChangedEventManager.RemoveListener(this._record, this, string.Empty);
				}
			}
		}

			#endregion //OnRecordPropertyChanged

		#endregion //Base class overrides

		#region Properties

		#region Public Properties

		#region Orientation

		/// <summary>
		/// Identifies the 'Orientation' dependency property
		/// </summary>
		public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register("Orientation",
			typeof(Orientation), typeof(ExpandedCellPresenter), new FrameworkPropertyMetadata(Orientation.Horizontal));

		/// <summary>
		/// Sets the orientation for the stack panel of expanded cells
		/// </summary>
		//[Description("Sets the orientation for the stack panel of expanded cells")]
		//[Category("Behavior")]
		public Orientation Orientation
		{
			get
			{
				return (Orientation)this.GetValue(ExpandedCellPresenter.OrientationProperty);
			}
			set
			{
				this.SetValue(ExpandedCellPresenter.OrientationProperty, KnownBoxes.FromValue(value));
			}
		}

		#endregion //Orientation

		#endregion //Public Properties

		#region Internal Properties

		#region RenderVersion

		internal static readonly DependencyProperty RenderVersionProperty = DependencyProperty.Register("RenderVersion",
			typeof(int), typeof(ExpandedCellPresenter), new FrameworkPropertyMetadata(0));

		internal int RenderVersion
		{
			get
			{
				return (int)this.GetValue(ExpandedCellPresenter.RenderVersionProperty);
			}
			set
			{
				this.SetValue(ExpandedCellPresenter.RenderVersionProperty, value);
			}
		}

		#endregion //RenderVersion

		#endregion //Internal Properties

		#endregion //Properties

		#region Methods

		#region Private Methods

		#endregion //Private Methods

		#endregion //Methods

	}

	#endregion //ExpandedCellPresenter
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