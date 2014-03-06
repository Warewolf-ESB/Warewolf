using System;
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
using Infragistics.Windows.Selection;
using Infragistics.Windows.Controls;
using Infragistics.Windows.DataPresenter.Internal;
using Infragistics.Windows.Internal;

namespace Infragistics.Windows.DataPresenter
{

	/// <summary>
	/// An element used to display the Label of a Field.
	/// </summary>
	/// <remarks>
	/// <p class="body">Refer to the <a href="xamData_Terms_Fields.html">Fields</a> topic in the Developer's Guide for an explanation of fields.</p>
	/// <p class="body">Refer to the <a href="xamData_Terms_Fields_Field_Label.html">Field Label</a> topic in the Developer's Guide for an explanation of the Field Labels.</p>
	/// <p class="body">Refer to the <a href="xamData_Terms_Fields_Field_Settings.html">Field Settings</a> topic in the Developer's Guide for an explanation of the FieldSettings object.</p>
	/// <p class="body">Refer to the <a href="xamData_TheoryOfOperation.html">Theory of Operation</a> topic in the Developer's Guide for an explanation of how this object is used.</p>
	/// </remarks>
	/// <seealso cref="DataPresenterBase"/>
	/// <seealso cref="Field"/>
	/// <seealso cref="Field.Label"/>
	/// <seealso cref="CellValuePresenter"/>
	/// <seealso cref="CellPresenter"/>
	//[Description("A control used to display the 'Label' of an associated 'Field' in the 'DataPresenterBase' derived controls such as 'XamDataGrid'.")]

    // JJD 4/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
    [TemplateVisualState(Name = VisualStateUtilities.StatePressed,         GroupName = VisualStateUtilities.GroupCommon)]
    
    [TemplateVisualState(Name = VisualStateUtilities.StateDragging,        GroupName = VisualStateUtilities.GroupDrag)]
    [TemplateVisualState(Name = VisualStateUtilities.StateDragSource,      GroupName = VisualStateUtilities.GroupDrag)]
    [TemplateVisualState(Name = VisualStateUtilities.StateNotDragging,     GroupName = VisualStateUtilities.GroupDrag)]
    
    [TemplateVisualState(Name = VisualStateUtilities.StateCellArea,        GroupName = VisualStateUtilities.GroupLocation)]
    [TemplateVisualState(Name = VisualStateUtilities.StateHeaderArea,      GroupName = VisualStateUtilities.GroupLocation)]
    [TemplateVisualState(Name = VisualStateUtilities.StateFieldChooser,    GroupName = VisualStateUtilities.GroupLocation)]
    [TemplateVisualState(Name = VisualStateUtilities.StateGroupByArea,     GroupName = VisualStateUtilities.GroupLocation)]
    
    [TemplateVisualState(Name = VisualStateUtilities.StateSelected,        GroupName = VisualStateUtilities.GroupSelection)]
    [TemplateVisualState(Name = VisualStateUtilities.StateUnselected,      GroupName = VisualStateUtilities.GroupSelection)]
    
    [TemplateVisualState(Name = VisualStateUtilities.StateSortAscending,   GroupName = VisualStateUtilities.GroupSort)]
    [TemplateVisualState(Name = VisualStateUtilities.StateSortDescending,  GroupName = VisualStateUtilities.GroupSort)]
    [TemplateVisualState(Name = VisualStateUtilities.StateUnsorted,        GroupName = VisualStateUtilities.GroupSort)]

	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class LabelPresenter : DataItemPresenter, ISelectableElement
	{
		#region Private Members

		private bool _mouseClickedOnLabel;

		
		
		private Point _lastLeftButtonDownPoint;

		#endregion //Private Members

		#region Constructors

		static LabelPresenter()
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(LabelPresenter), new FrameworkPropertyMetadata(typeof(LabelPresenter)));
			FrameworkElement.FocusableProperty.OverrideMetadata(typeof(LabelPresenter), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));
            
            // JJD 2/7/08 - BR30444
            // Change the default ContentTemplateSelector to our instance so we 
            // can provide appropriate defalu temples based on data type and
            // hook up bindings to properties off the Field.
            LabelPresenter.ContentTemplateSelectorProperty.OverrideMetadata(typeof(LabelPresenter), new FrameworkPropertyMetadata(LPContentTemplateSelector.Instance));
            
            // JJD 2/7/08 - BR30444
            // Added Coerce callbacks for the ContentAlignment properties
            LabelPresenter.HorizontalContentAlignmentProperty.OverrideMetadata(typeof(LabelPresenter), new FrameworkPropertyMetadata(HorizontalAlignment.Stretch, null, new CoerceValueCallback(CoerceHorizontalContentAlignment)));
            LabelPresenter.VerticalContentAlignmentProperty.OverrideMetadata(typeof(LabelPresenter), new FrameworkPropertyMetadata(VerticalAlignment.Center, null, new CoerceValueCallback(CoerceVerticalContentAlignment)));

			// AS 8/24/09 TFS19532
			UIElement.VisibilityProperty.OverrideMetadata(typeof(LabelPresenter), new FrameworkPropertyMetadata(null, new CoerceValueCallback(GridUtilities.CoerceFieldElementVisibility)));
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LabelPresenter"/> class
		/// </summary>
		public LabelPresenter()
		{
		}

		#endregion //Constructors

		#region Base class overrides

		#region IsEditingAllowed

		/// <summary>
		/// Returns true is editing is allowed
		/// </summary>
		public override bool IsEditingAllowed
		{
			
			get { return false; }
		}

		#endregion //IsEditingAllowed
		
		#region MeasureOverride
        
#region Infragistics Source Cleanup (Region)

























































































#endregion // Infragistics Source Cleanup (Region)

		#endregion //MeasureOverride

		#region OnCreateAutomationPeer
		/// <summary>
		/// Returns an automation peer that exposes the <see cref="LabelPresenter"/> to UI Automation.
		/// </summary>
		/// <returns>A <see cref="Infragistics.Windows.Automation.Peers.DataPresenter.LabelPresenterAutomationPeer"/></returns>
		protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
		{
			return new Infragistics.Windows.Automation.Peers.DataPresenter.LabelPresenterAutomationPeer(this);
		}
		#endregion //OnCreateAutomationPeer

		#region OnEditModeStarting

		/// <summary>
		/// Called when the embedded editor is about to enter edit mode
		/// </summary>
		/// <param name="e">The event arguments</param>
		protected override void OnEditModeStarting(Infragistics.Windows.Editors.Events.EditModeStartingEventArgs e)
		{
			
			e.Cancel = true;
		}

		#endregion //OnEditModeStarting

		#region OnEditModeStarted

		/// <summary>
		/// Called when the embedded editor has just entered edit mode
		/// </summary>
		/// <param name="e">The event arguments</param>
		protected override void OnEditModeStarted(Infragistics.Windows.Editors.Events.EditModeStartedEventArgs e)
		{
		}

		#endregion //OnEditModeStarted

		#region OnEditModeEnding

		/// <summary>
		/// Called when the embedded editor is about to exit edit mode
		/// </summary>
		/// <param name="e">The event arguments</param>
		protected override void OnEditModeEnding(Infragistics.Windows.Editors.Events.EditModeEndingEventArgs e)
		{
		}

		#endregion //OnEditModeEnding

		#region OnEditModeEnded

		/// <summary>
		/// Called when the embedded editor has just exited edit mode
		/// </summary>
		protected override void OnEditModeEnded(Infragistics.Windows.Editors.Events.EditModeEndedEventArgs e)
		{
		}

		#endregion //OnEditModeEnded

		#region CommitEditValue

		/// <summary>
		/// Overridden. Called after OnEditModeEnding however before OnEditModeEnded. This is 
		/// called after input validation succeeds to let the host know to commit the value.
		/// This implementation does nothing as label presenters do not enter edit mode.
		/// </summary>
		/// <param name="editedValue">The edited value that is to be committed.</param>
		/// <param name="stayInEditMode">Whether the editor should cancel the operation of exitting edit mode.</param>
		/// <returns>Returns true if commit succeeded, false otherwise.</returns>
		internal protected override bool CommitEditValue( object editedValue, out bool stayInEditMode )
		{
			stayInEditMode = false;
			return false;
		}

		#endregion // CommitEditValue

		#region OnEditModeValidationError

		/// <summary>
		/// Overridden. Called when the embedded editor has input validation error.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnEditModeValidationError( Infragistics.Windows.Editors.Events.EditModeValidationErrorEventArgs e )
		{
		}

		#endregion // OnEditModeValidationError

		#region OnFieldPropertyChanged

		/// <summary>
		/// Called when a property on the associated <see cref="Field"/> object changes
		/// </summary>
		/// <param name="e"></param>
		protected override void OnFieldPropertyChanged(PropertyChangedEventArgs e)
		{
            // AS 12/5/08 NA 2009 Vol 1 - Fixed Fields
            base.OnFieldPropertyChanged(e);

            switch (e.PropertyName)
			{
				// JJD 4/29/11 - TFS74075
				// Respond to changes to the LabelPresenterStyleResolved
				case "LabelPresenterStyleResolved":
					this.InvalidateStyleSelector();
					break;
				//case "LabelWidthResolved":
				//    if (!this.Field.Owner.IsHorizontal)
				//        this.Width = this.Field.LabelWidthResolved;
				//    break;
				//case "LabelHeightResolved":
				//    if (this.Field.Owner.IsHorizontal)
				//        this.Height = this.Field.LabelHeightResolved;
				//    break;
				case "CellContentAlignmentResolved":
				case "VisibilityResolved":
					{
						bool isInFieldChooser = this.IsInFieldChooser || this.DataContext is FieldChooserEntry;

						bool isInGroupByArea = this.IsInGroupByArea;
						if (!isInFieldChooser && !isInGroupByArea)
						{
							if (e.PropertyName == "CellContentAlignmentResolved")
								this.SetValue(GridUtilities.CellContentAlignmentProperty, this.Field.CellContentAlignmentResolved);
							else
								this.SetValue(GridUtilities.FieldVisibilityProperty, this.Field.VisibilityResolved);
						}
						break;
					}
				case "SortStatus":
					// JJD 3/9/11 - TFS67970 - Optimization
					// Replace binding to field with code in OnFieldPropertyChanged
					this.SortStatusInternal = this.Field.SortStatus;
                    break;
                case "Label":
                    // JJD 2/7/08 - BR30444
                    // Update the content of the label
                    this.Content = this.Field.Label;
                    break;
                
#region Infragistics Source Cleanup (Region)










#endregion // Infragistics Source Cleanup (Region)


                // AS 12/5/08 NA 2009 Vol 1 - Fixed Fields
                case "FixedButtonVisibility":
                    this.SetValue(FixedButtonVisibilityPropertyKey, this.Field == null || this.Field.FixedButtonVisibility == Visibility.Collapsed
                        ? DependencyProperty.UnsetValue
                        : KnownBoxes.VisibilityVisibleBox);
                    break;

                case "AllowRecordFilteringResolved":

                    // JJD 1/6/09 - NA 2009 vol 1 
                    // Initialize FilterButtonVisibility property
                    this.SetValue(FilterButtonVisibilityPropertyKey, this.Field == null || this.Field.AllowRecordFilteringResolved == false || this.Field.Owner.FilterUITypeResolved != FilterUIType.LabelIcons
                        ? DependencyProperty.UnsetValue
                        : KnownBoxes.VisibilityVisibleBox);
                    break;
			}
		}

		#endregion //OnFieldPropertyChanged

		#region OnLostMouseCapture

		/// <summary>
		/// Called when mouse capture is lost
		/// </summary>
		protected override void OnLostMouseCapture(MouseEventArgs e)
		{
			this._mouseClickedOnLabel = false;

			// reset the IsPressed property and capture the mouse
			this.SetValue(IsPressedPropertyKey, KnownBoxes.FalseBox);

			base.OnLostMouseCapture(e);
		}

		#endregion //OnLostMouseCapture

		#region OnMouseLeftButtonDown

		/// <summary>
		/// Called when the left mouse button is pressed
		/// </summary>
		protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			base.OnMouseLeftButtonDown(e);

			if (!e.Handled && e.MouseDevice.Captured != this &&
				e.ButtonState == MouseButtonState.Pressed)
			{
				// set the IsPressed property and capture the mouse
				this.SetValue(IsPressedPropertyKey, KnownBoxes.TrueBox);

				// set a flag so we know that they clicked on us
				this._mouseClickedOnLabel = true;

				
				
				_lastLeftButtonDownPoint = e.GetPosition( this );
				
				// capture the mouse so we know we will get the up
				this.CaptureMouse();

				Field fld = this.Field;

				// Only do sortiing if we are inside a DataPresenterBase
				if (fld != null &&
					fld.Owner.DataPresenter != null)
				{
					// SSP 6/19/09 - NAS9.2 Field Chooser
					// Added the if block and enclosed the existing code into the else block.
					// This is to make the keyboard navigation work in the list box because the
					// list box requires the item container to contain focus in order for
					// the keyboard navigation to work. However since the label presenter 
					// is not focusable, we have to make sure the item container is focused.
					// 
					if ( this.IsInFieldChooser )
					{
						if ( ! this.IsKeyboardFocusWithin )
							GridUtilities.FocusTrivialAncestor( this );

						// Select the field in the field chooser.
						// 
						FieldChooser fieldChooser = (FieldChooser)Utilities.GetAncestorFromType( this, typeof( FieldChooser ), true );
						if ( null != fieldChooser )
							fieldChooser.SelectFieldHelper( fld );

						e.Handled = true;
					}
					else
					{
						LabelClickAction clickAction = fld.LabelClickActionResolved;

						switch ( clickAction )
						{
							case LabelClickAction.SortByOneFieldOnly:
							case LabelClickAction.SortByMultipleFields:
							// JJD 12/03/08 
							// Added support for cycling thru ascending/descending/unsorted
							case LabelClickAction.SortByOneFieldOnlyTriState:
							case LabelClickAction.SortByMultipleFieldsTriState:
								e.Handled = true;
								break;
								// SSP 8/14/09 TFS17812
								// If field moving is enabled then we need to set the e.Handled to
								// true here otherwise the selection strategy will end up selecting
								// the record because the LabelPresenter returns the record from
								// ISelectableElement.SelectableItem implementation, which causes
								// the field drag and drop to not work.
								// 
							case LabelClickAction.Nothing:
								if ( AllowFieldMoving.No != fld.Owner.AllowFieldMovingResolved )
									e.Handled = true;
								break;
						}
					}
				}
			}
		}

		#endregion //OnMouseLeftButtonDown

		#region OnMouseLeftButtonUp

		/// <summary>
		/// Called when the left mouse button is released.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
		{
			base.OnMouseLeftButtonUp(e);

			
			
			
			this.ProcessMouseLeftButtonUpHelper( e, false );
		}
		
		#endregion //OnMouseLeftButtonUp

		#region OnMouseMove

		/// <summary>
		/// Called when the mouse mouses over the elemnt or when it has mouse capture.
		/// </summary>
		/// <param name="e">The mouse argument</param>
		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);

			if (this._mouseClickedOnLabel == true)
			{
				
				
				
				//Point pt = Mouse.PrimaryDevice.GetPosition(this);
				Point pt = e.GetPosition( this );


				if ( GridUtilities.Distance( pt, _lastLeftButtonDownPoint ) >= 2 )
				{
					
					
					
					if ( this.StartDragHelper( e, true ) )
						return;
				}

				

				bool isMouseOver = (pt.X >= 0 &&
									 pt.Y >= 0 &&
									 pt.X <= this.ActualWidth &&
									 pt.Y <= this.ActualHeight);

				if ( isMouseOver != this.IsPressed )
					this.SetValue(IsPressedPropertyKey, KnownBoxes.FromValue(isMouseOver));
			}
		}

		#endregion //OnMouseMove	
    
		#region OnPropertyChanged

		/// <summary>
		/// Called when a property is changed
		/// </summary>
		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);

			if (e.Property == LabelPresenter.FieldProperty)
			{
				Field field = this.Field;

				if (field != null)
				{
					CellContentAlignment cellContentAlignment = field.CellContentAlignmentResolved;

                    // JJD 2/7/08 - BR30444
                    // Moved logic into Coerce callbacks instead
                    //this.SetValue(HorizontalContentAlignmentProperty, KnownBoxes.FromValue(FieldLayoutTemplateGenerator.GetHorizontalAlignmentLabel(cellContentAlignment)));
                    //this.SetValue(VerticalContentAlignmentProperty, KnownBoxes.FromValue(FieldLayoutTemplateGenerator.GetVerticalAlignmentLabel(cellContentAlignment)));
                    this.CoerceValue(HorizontalContentAlignmentProperty);
                    this.CoerceValue(VerticalContentAlignmentProperty);

                    // JJD 5/27/09 - TFS17862 
                    // Coerce the filter and fixed button visibility properties
                    this.CoerceValue(FilterButtonVisibilityProperty);
                    this.CoerceValue(FixedButtonVisibilityProperty);

					//if (field.Owner != null && field.Owner.IsHorizontal)
					//    this.Height = field.LabelHeightResolved;
					//else
					//	this.Width = field.LabelWidthResolved;

					this.MinWidth = field.LabelMinWidthResolved;
					this.MinHeight = field.LabelMinHeightResolved;
					this.MaxWidth = field.LabelMaxWidthResolved;
					this.MaxHeight = field.LabelMaxHeightResolved;

					// SSP 6/30/09 - NAS9.2 Field Chooser
					// Enclosed the existing code that sets the Visibility inside the if block.
					// We shouldn't set the Visibility on the field when in field chooser and
					// in group-by area.
					// 
					// SSP 11/18/09 TFS24444
					// Even though IsInFieldChooser is set before Field binding in xaml, Field gets 
					// set first and IsInFieldChooser happens to be false when we get here.
					// 
					//bool isInFieldChooser = this.IsInFieldChooser;
					bool isInFieldChooser = this.IsInFieldChooser || this.DataContext is FieldChooserEntry;

					bool isInGroupByArea = this.IsInGroupByArea;
					if ( ! isInFieldChooser && ! isInGroupByArea )
					{
						// AS 8/24/09 TFS19532
						// See GridUtilities.CoerceFieldElementVisibility for details.
						//
						//Visibility visibility = field.VisibilityResolved;
						//
						//if (visibility != Visibility.Visible)
						//    this.Visibility = visibility;
						// JJD 3/9/11 - TFS67970 - Optimization
						// Replace binding to field with code in OnFieldPropertyChanged
						//this.SetBinding(GridUtilities.FieldVisibilityProperty, Utilities.CreateBindingObject("VisibilityResolved", BindingMode.OneWay, field));
						this.SetValue(GridUtilities.FieldVisibilityProperty, field.VisibilityResolved);

						// SSP 10/21/09 TFS22721
						// Moved this from below. We shouldn't set this property on the label that's part of
						// field chooser.
						// 
						// AS 8/26/09 CellContentAlignment
						// JJD 3/9/11 - TFS67970 - Optimization
						// Replace binding to field with code in OnFieldPropertyChanged
						//this.SetBinding(GridUtilities.CellContentAlignmentProperty, Utilities.CreateBindingObject("CellContentAlignmentResolved", BindingMode.OneWay, field));
						this.SetValue(GridUtilities.CellContentAlignmentProperty, cellContentAlignment);
					}

					// SSP 10/21/09 TFS22721
					// If the label is in field chooser then don't set the CellContentAlignmentProperty which causes
					// the label presenter's visibility to be coerced to collapsed if the property value is ValueOnly. 
					// Moved this above in the if block.
					// 
					




					// JJD 3/9/11 - TFS67970 - Optimization
					// Replace binding to field with code in OnFieldPropertyChanged
					//this.SetBinding(SortStatusInternalProperty, Utilities.CreateBindingObject(Field.SortStatusProperty, BindingMode.OneWay, field));
					//this.SetBinding(ContentProperty, Utilities.CreateBindingObject(Field.LabelProperty, BindingMode.OneWay, field));
					this.SetValue(SortStatusInternalProperty, field.SortStatus);
					this.SetValue(ContentProperty, field.Label);
                    
                    // JJD 1/6/09 - NA 2009 vol 1 
                    // Initialize FixedButtonVisibility property
                    this.SetValue(FixedButtonVisibilityPropertyKey, field.FixedButtonVisibility == Visibility.Collapsed
                        ? DependencyProperty.UnsetValue
                        : KnownBoxes.VisibilityVisibleBox);
                    
                    // JJD 1/6/09 - NA 2009 vol 1 
                    // Initialize FilterButtonVisibility property
                    this.SetValue(FilterButtonVisibilityPropertyKey, field.AllowRecordFilteringResolved == false || field.Owner.FilterUITypeResolved != FilterUIType.LabelIcons
                        ? DependencyProperty.UnsetValue
                        : KnownBoxes.VisibilityVisibleBox);

                    // JJD 10/06/08 - TFS8358
                    // If the label is not being shown then set its width or height exlicitly to zero
                    if (cellContentAlignment == CellContentAlignment.ValueOnly
						// SSP 10/21/09 TFS22721
						// CellContentAlignment shouldn't affect labels in the field chooser and group-by area.
						// 
						&& ! isInFieldChooser && ! isInGroupByArea 
						)
                    {
                        FieldLayout fl = field.Owner;

                        if (fl != null && fl.IsHorizontal)
                        {
                            this.MinWidth = 0;
                            this.Width = 0;
                        }
                        else
                        {
                            this.MinHeight = 0;
                            this.Height = 0;
                        }
                    }
                }

				this.InvalidateStyleSelector();
            }
			else if (e.Property == LabelPresenter.SortStatusInternalProperty)
			{
				this.SetValue(SortStatusPropertyKey, this.SortStatusInternal);
			}
		}

		#endregion //OnPropertyChanged

		#region OnQueryCursor

		/// <summary>
		/// Called when the mouse is over the element
		/// </summary>
		protected override void OnQueryCursor(QueryCursorEventArgs e)
		{
			if (this._mouseClickedOnLabel)
				e.Handled = true;

			base.OnQueryCursor(e);
		}

		#endregion OnQueryCursor

		#region OnVisualParentChanged

		/// <summary>
		/// Called when the visual parent has changed
		/// </summary>
		/// <param name="oldParent"></param>
		protected override void OnVisualParentChanged( DependencyObject oldParent )
		{
			base.OnVisualParentChanged( oldParent );

			// JJD 4/17/09 - NA 2009 vol 2 - Cross band grouping
			// Hide filter buttons on labels in the group-by area for child field-layout 
			// fields since they don't have the context of record manager in group-by area.
			// 
			this.CoerceValue( FilterButtonVisibilityProperty );
		}

		#endregion //OnVisualParentChanged	

		#region ToString

		/// <summary>
		/// Returns a string representation of the object
		/// </summary>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append("LabelPresenter: ");

			if (this.Field != null)
			{
				sb.Append(this.Field.ToString());
				sb.Append(", ");
			}

			if (this.Content != null)
				sb.Append(this.Content.ToString());

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
            if (this.IsEnabled == false)
                VisualStateManager.GoToState(this, VisualStateUtilities.StateDisabled, useTransitions);
            else
            {
                if (this.IsMouseOver)
                {
                    if (this.IsPressed)
                        VisualStateUtilities.GoToState(this, useTransitions, VisualStateUtilities.StatePressed, VisualStateUtilities.StateMouseOver, VisualStateUtilities.StateNormal);
                    else
                        VisualStateUtilities.GoToState(this, useTransitions, VisualStateUtilities.StateMouseOver, VisualStateUtilities.StateNormal);
                }
                else
                {
                    if (this.IsPressed)
                        VisualStateUtilities.GoToState(this, useTransitions, VisualStateUtilities.StatePressed, VisualStateUtilities.StateNormal);
                    else
                        VisualStateManager.GoToState(this, VisualStateUtilities.StateNormal, useTransitions);
                }
            }

            // set drag state
            if (this.IsDragIndicator)
                VisualStateManager.GoToState(this, VisualStateUtilities.StateDragging, useTransitions);
            else
            if (this.IsDragSource)
                VisualStateUtilities.GoToState(this, useTransitions, VisualStateUtilities.StateDragSource, VisualStateUtilities.StateNotDragging);
            else
                VisualStateManager.GoToState(this, VisualStateUtilities.StateNotDragging, useTransitions);

            Field fld = this.Field;

            // set location state
            if ( this.IsInFieldChooser )
                VisualStateManager.GoToState(this, VisualStateUtilities.StateFieldChooser, useTransitions);
            else
            if (this.IsInGroupByArea)
                VisualStateManager.GoToState(this, VisualStateUtilities.StateGroupByArea, useTransitions);
            else
            {
                FieldLayout fl = fld != null ? fld.Owner : null;

                if (fl != null && fl.HasSeparateHeader == false)
                    VisualStateManager.GoToState(this, VisualStateUtilities.StateCellArea, useTransitions);
                else
                    VisualStateManager.GoToState(this, VisualStateUtilities.StateHeaderArea, useTransitions);
            }

            // set selected state
            if (fld != null && fld.IsSelected)
                VisualStateManager.GoToState(this, VisualStateUtilities.StateSelected, useTransitions);
            else
                VisualStateManager.GoToState(this, VisualStateUtilities.StateUnselected, useTransitions);

            // set sort state
            string state = null;

            switch(this.SortStatus)
            {
                case SortStatus.Ascending:
                    state = VisualStateUtilities.StateSortAscending;
                    break;
                case SortStatus.Descending:
                    state = VisualStateUtilities.StateSortDescending;
                    break;
                case SortStatus.NotSorted:
                    state = VisualStateUtilities.StateUnsorted;
                    break;
            }

            if (state != null)
                VisualStateManager.GoToState(this, state, useTransitions);
        }



        #endregion //SetVisualState	

		#endregion //Base class overrides

		#region Properties

		#region Public Properties

        // JJD 1/6/09 NA 2009 Vol 1 - Filtering
        #region FilterButtonVisibility

        private static readonly DependencyPropertyKey FilterButtonVisibilityPropertyKey =
            DependencyProperty.RegisterReadOnly("FilterButtonVisibility",
            typeof(Visibility), typeof(LabelPresenter), new FrameworkPropertyMetadata(KnownBoxes.VisibilityCollapsedBox, null, new CoerceValueCallback(CoerceFilterButtonVisibility)));

        /// <summary>
        /// Identifies the <see cref="FilterButtonVisibility"/> dependency property
        /// </summary>
        public static readonly DependencyProperty FilterButtonVisibilityProperty =
            FilterButtonVisibilityPropertyKey.DependencyProperty;

        // JJD 5/27/09 - TFS17862 - added
        // If we are in the GroupByArea we need to coerce the value to Collapsed
        private static object CoerceFilterButtonVisibility(DependencyObject target, object value)
        {
            if (Object.Equals(Visibility.Visible, value))
            {
                LabelPresenter lp = target as LabelPresenter;

                if (lp != null && lp.IsInGroupByArea)
                {
                    Field field = lp.Field;

                    if (field != null)
                    {
                        FieldLayout fl = field.Owner;

                        if (fl != null && fl.DataPresenter != null)
                        {
                            RecordManager rm = fl.DataPresenter.RecordManager;

                            // if the fieldlayout is not contained in the root
                            // record manager then return collapsed
                            if (rm == null || rm.ContainsFieldLayout(fl) == false)
                                return KnownBoxes.VisibilityCollapsedBox;
                        }
                    }
                }
            }

            return value;
        }

        /// <summary>
        /// Indicates the preferred visibility of the FixedFieldButton within the element based on the resolved FixedFieldUIType and the AllowFixing of the Field.
        /// </summary>
        /// <seealso cref="FilterButtonVisibilityProperty"/>
        //[Description("Indicates the visibility of the FilterButton within the element based on the resolved FixedFieldUIType and the AllowFixing of the Field.")]
        //[Category("Behavior")]
        [Bindable(true)]
        [ReadOnly(true)]
        public Visibility FilterButtonVisibility
        {
            get
            {
                return (Visibility)this.GetValue(LabelPresenter.FilterButtonVisibilityProperty);
            }
        }

        #endregion //FilterButtonVisibility

        // AS 12/5/08 NA 2009 Vol 1 - Fixed Fields
        #region FixedButtonVisibility

        private static readonly DependencyPropertyKey FixedButtonVisibilityPropertyKey =
            DependencyProperty.RegisterReadOnly("FixedButtonVisibility",
            typeof(Visibility), typeof(LabelPresenter), new FrameworkPropertyMetadata(KnownBoxes.VisibilityCollapsedBox));

        /// <summary>
        /// Identifies the <see cref="FixedButtonVisibility"/> dependency property
        /// </summary>
        public static readonly DependencyProperty FixedButtonVisibilityProperty =
            FixedButtonVisibilityPropertyKey.DependencyProperty;

        /// <summary>
        /// Indicates the preferred visibility of the FixedFieldButton within the element based on the resolved FixedFieldUIType and the AllowFixing of the Field.
        /// </summary>
        /// <seealso cref="FixedButtonVisibilityProperty"/>
        //[Description("Indicates the preferred visibility of the FixedFieldButton within the element based on the resolved FixedFieldUIType and the AllowFixing of the Field.")]
        //[Category("Behavior")]
        [Bindable(true)]
        [ReadOnly(true)]
        public Visibility FixedButtonVisibility
        {
            get
            {
                return (Visibility)this.GetValue(LabelPresenter.FixedButtonVisibilityProperty);
            }
        }

        #endregion //FixedButtonVisibility
				
		#region InnerBorderBrush

		/// <summary>
		/// Identifies the <see cref="InnerBorderBrush"/> dependency property
		/// </summary>	
		public static readonly DependencyProperty InnerBorderBrushProperty = DependencyProperty.Register("InnerBorderBrush",
			typeof(Brush), typeof(LabelPresenter), new FrameworkPropertyMetadata((object)null));

		/// <summary>
		/// Brush applied to innermost of two default borders.
		/// </summary>
		/// <seealso cref="InnerBorderBrushProperty"/>
		//[Description("Brush applied to innermost of two default borders")]
		//[Category("Brushes")]
		public Brush InnerBorderBrush
		{
			get
			{
				return (Brush)this.GetValue(LabelPresenter.InnerBorderBrushProperty);
			}
			set
			{
				this.SetValue(LabelPresenter.InnerBorderBrushProperty, value);
			}
		}

		#endregion InnerBorderBrush		
		
		#region IsDragSource

		
		

		/// <summary>
		/// Identifies the <see cref="IsDragSource"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsDragSourceProperty = DependencyProperty.Register(
				"IsDragSource",
				typeof( bool ),
				typeof( LabelPresenter ),
				new FrameworkPropertyMetadata( KnownBoxes.FalseBox 

            // JJD 4/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
            , new PropertyChangedCallback(OnVisualStatePropertyChanged)

));


		/// <summary>
		/// Indicates if this label presenter in the data grid is being dragged.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// When a label presenter inside data presenter is clicked and dragged, it's IsDragSource will
		/// be set to <b>true</b>. Note that this is not the label presenter that's embedded inside
		/// drag indicator. To check if a label presenter is embedded inside the drag indicator window,
		/// use its <seealso cref="DataItemPresenter.IsDragIndicator"/> property.
		/// </para>
		/// </remarks>
		/// <seealso cref="DataItemPresenter.IsDragIndicator"/>
		//[Description( "Indicates if this label presenter in the data grid is being dragged." )]
		//[Category( "Data" )]
		[Bindable( true )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public bool IsDragSource
		{
			get
			{
				return (bool)this.GetValue( IsDragSourceProperty );
			}
			set
			{
				this.SetValue( IsDragSourceProperty, value );
			}
		}

		#endregion // IsDragSource

		#region IsInFieldChooser

		// SSP 6/8/09 - NAS9.2 Field Chooser
		// 

		/// <summary>
		/// Identifies the <see cref="IsInFieldChooser"/> dependency property.
		/// </summary>
		[InfragisticsFeature( FeatureName = FeatureInfo.FeatureName_FieldChooser, Version = FeatureInfo.Version_9_2 )]
		public static readonly DependencyProperty IsInFieldChooserProperty = DependencyProperty.Register(
			"IsInFieldChooser",
			typeof( bool ),
			typeof( LabelPresenter ),
			new FrameworkPropertyMetadata( KnownBoxes.FalseBox 

            // JJD 4/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
            , new PropertyChangedCallback(OnVisualStatePropertyChanged)

));


		/// <summary>
		/// Indicates if this LabelPresenter is in a <see cref="FieldChooser"/>.
		/// </summary>
		/// <seealso cref="FieldChooser"/>
		//[Description( "Indicates if the LabelPresenter is in a FieldChooser." )]
		//[Category( "Data" )]
		[Bindable( true )]
		[Browsable( false )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		[InfragisticsFeature( FeatureName = FeatureInfo.FeatureName_FieldChooser, Version = FeatureInfo.Version_9_2 )]
		public bool IsInFieldChooser
		{
			get
			{
				return (bool)this.GetValue( IsInFieldChooserProperty );
			}
			set
			{
				this.SetValue( IsInFieldChooserProperty, value );
			}
		}

		#endregion // IsInFieldChooser

        // JJD 4/07/09 - NA 2009 vol 2 
        #region IsInGroupByArea

        /// <summary>
        /// Identifies the <see cref="IsInGroupByArea"/> dependency property
        /// </summary>
        [InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_CrossBandGrouping, Version = FeatureInfo.Version_9_2)]
        public static readonly DependencyProperty IsInGroupByAreaProperty =
			DependencyProperty.Register( "IsInGroupByArea",
			typeof( bool ), typeof( LabelPresenter ),
			new FrameworkPropertyMetadata( KnownBoxes.FalseBox, FrameworkPropertyMetadataOptions.None,
				new PropertyChangedCallback( OnIsInGroupByAreaChanged ) ) );

		private static void OnIsInGroupByAreaChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			LabelPresenter lp = (LabelPresenter)dependencyObject;
			lp.CoerceValue( FilterButtonVisibilityProperty );

            // JJD 4/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
            lp.UpdateVisualStates();


		}

        /// <summary>
		/// Indicates if this LabelPresenter is in the GroupByArea.
        /// </summary>
        /// <seealso cref="IsInGroupByAreaProperty"/>
        //[Description("Indicates if this LabelPresenter is in the GroupByArea.")]
        //[Category("Data")]
		[Bindable( true )]
		[Browsable( false )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
        [InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_CrossBandGrouping, Version = FeatureInfo.Version_9_2)]
        public bool IsInGroupByArea
        {
            get
            {
                return (bool)this.GetValue(LabelPresenter.IsInGroupByAreaProperty);
            }
			set
			{
				this.SetValue( IsInGroupByAreaProperty, KnownBoxes.FromValue( value ) );
			}
        }

        #endregion //IsInGroupByArea

		#region IsPressed

		private static readonly DependencyPropertyKey IsPressedPropertyKey =
			DependencyProperty.RegisterReadOnly("IsPressed",
			typeof(bool), typeof(LabelPresenter), new FrameworkPropertyMetadata(KnownBoxes.FalseBox

            // JJD 4/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
            , new PropertyChangedCallback(OnVisualStatePropertyChanged)

));

		/// <summary>
		/// Identifies the <see cref="IsPressed"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsPressedProperty =
			IsPressedPropertyKey.DependencyProperty;

		/// <summary>
		/// Gets a value that indicates whether a label is in a pressed state."/>
		/// </summary>
		/// <seealso cref="IsPressedProperty"/>
		//[Description("Gets a value that indicates whether a label is in a pressed state.")]
		//[Category("Behavior")]
		[Bindable(true)]
		[ReadOnly(true)]
		public bool IsPressed
		{
			get
			{
				return (bool)this.GetValue(LabelPresenter.IsPressedProperty);
			}
		}

		#endregion //IsPressed

		#region IsSelectedInFieldChooser

		// SSP 6/9/09 - NAS9.2 Field Chooser
		// 

		/// <summary>
		/// Identifies the <see cref="IsSelectedInFieldChooser"/> dependency property.
		/// </summary>
		[InfragisticsFeature( FeatureName = FeatureInfo.FeatureName_FieldChooser, Version = FeatureInfo.Version_9_2 )]
		public static readonly DependencyProperty IsSelectedInFieldChooserProperty = DependencyProperty.Register(
			"IsSelectedInFieldChooser",
			typeof( bool ),
			typeof( LabelPresenter ),
			new FrameworkPropertyMetadata( KnownBoxes.FalseBox, FrameworkPropertyMetadataOptions.None )
		);

		/// <summary>
		/// Indicates if a the LabelPresenter is selected in the field chooser (read-only).
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// When this LabelPresenter is inside a <see cref="FieldChooser"/>, <b>IsSelectedInFieldChooser</b>
		/// property returns a value indicating whether the LabelPresenter is currently selected in the FieldChooser.
		/// </para>
		/// </remarks>
		/// <seealso cref="IsInFieldChooser"/>
		/// <seealso cref="FieldChooser.IsDraggingItem"/>
		/// <seealso cref="FieldChooser.IsDraggingItemFromDataPresenter"/>
		/// <seealso cref="FieldChooser.IsDragItemOver"/>
		//[Description( "Indicates if the LabelPresenter is currently selected in the FieldChooser." )]
		//[Category( "Data" )]
		[Bindable( true )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		[InfragisticsFeature( FeatureName = FeatureInfo.FeatureName_FieldChooser, Version = FeatureInfo.Version_9_2 )]
		public bool IsSelectedInFieldChooser
		{
			get
			{
				return (bool)this.GetValue( IsSelectedInFieldChooserProperty );
			}
			set
			{
				this.SetValue( IsSelectedInFieldChooserProperty, value );
			}
		}

		#endregion // IsSelectedInFieldChooser

		#region LabelHighlight

		/// <summary>
		/// Identifies the <see cref="LabelHighlight"/> dependency property
		/// </summary>	
		public static readonly DependencyProperty LabelHighlightProperty = DependencyProperty.Register("LabelHighlight",
			typeof(Brush), typeof(LabelPresenter), new FrameworkPropertyMetadata((object)null));
		
		/// <summary>
		/// The brush used by default templates when IsMouseOver == True.
		/// </summary>
		/// <seealso cref="LabelHighlightProperty"/>
		//[Description("The brush used by default templates when IsMouseOver == True.")]
		//[Category("Brushes")]
		public Brush LabelHighlight
		{
			get
			{
				return (Brush)this.GetValue(LabelPresenter.LabelHighlightProperty);
			}
			set
			{
				this.SetValue(LabelPresenter.LabelHighlightProperty, value);
			}
		}

		#endregion //LabelHighlight
				
		#region OuterBorderBrush

		/// <summary>
		/// Identifies the <see cref="OuterBorderBrush"/> dependency property
		/// </summary>	
		public static readonly DependencyProperty OuterBorderBrushProperty = DependencyProperty.Register("OuterBorderBrush",
			typeof(Brush), typeof(LabelPresenter), new FrameworkPropertyMetadata((object)null));

		/// <summary>
		/// Brush applied to outermost of two default borders.
		/// </summary>
		/// <seealso cref="OuterBorderBrushProperty"/>
		//[Description("Brush applied to outermost of two default borders")]
		//[Category("Brushes")]
		public Brush OuterBorderBrush
		{
			get
			{
				return (Brush)this.GetValue(LabelPresenter.OuterBorderBrushProperty);
			}
			set
			{
				this.SetValue(LabelPresenter.OuterBorderBrushProperty, value);
			}
		}

		#endregion OuterBorderBrush		

		#region SortStatus

		private static readonly DependencyPropertyKey SortStatusPropertyKey =
			DependencyProperty.RegisterReadOnly("SortStatus",
			typeof(SortStatus), typeof(LabelPresenter), new FrameworkPropertyMetadata(SortStatus.NotSorted

            // JJD 4/19/10 - NA2010 Vol 2 - Added support for VisualStateManager
            , new PropertyChangedCallback(OnVisualStatePropertyChanged)

));

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
				return (SortStatus)this.GetValue(LabelPresenter.SortStatusProperty);
			}
		}

		#endregion //SortStatus

		#endregion //Public Properties

		#region Private Properties

		#region SortStatusInternal

		private static readonly DependencyProperty SortStatusInternalProperty = DependencyProperty.Register("SortStatusInternal",
			typeof(SortStatus), typeof(LabelPresenter), new FrameworkPropertyMetadata(SortStatus.NotSorted));

		private SortStatus SortStatusInternal
		{
			get
			{
				return (SortStatus)this.GetValue(LabelPresenter.SortStatusInternalProperty);
			}
			set
			{
				this.SetValue(LabelPresenter.SortStatusInternalProperty, value);
			}
		}

		#endregion //SortStatusInternal

		#endregion //Private Properties

		#endregion //Properties

        #region Methods

            #region Private Methods

                // JJD 2/7/08 - BR30444 - added
                #region CoerceHorizontalContentAlignment

        private static object CoerceHorizontalContentAlignment(DependencyObject target, object value)
        {
            LabelPresenter lp = target as LabelPresenter;

            if (lp != null)
            {
                object alignment = lp.GetDefaultAlignment(false);

                if (alignment != null)
                    return alignment;
            }

            return value;

        }

                #endregion //CoerceHorizontalContentAlignment	
    
                // JJD 2/7/08 - BR30444 - added
                #region CoerceVerticalContentAlignment

        private static object CoerceVerticalContentAlignment(DependencyObject target, object value)
        {
            LabelPresenter lp = target as LabelPresenter;

            if (lp != null)
            {
                object alignment = lp.GetDefaultAlignment(true);

                if (alignment != null)
                    return alignment;
            }

            return value;
        }

                #endregion //CoerceVerticalContentAlignment	
    
                // JJD 2/7/08 - BR30444 - added
                #region GetDefaultAlignment

        private object GetDefaultAlignment(bool verticalAlignment)
        {
            Field field = this.Field;

            if (field == null)
                return null;

            FieldLayout fl = field.Owner;
            if (fl == null)
                return null;

            ValueSource vs;

            if (verticalAlignment)
                vs = DependencyPropertyHelper.GetValueSource(this, LabelPresenter.VerticalContentAlignmentProperty);
            else
                vs = DependencyPropertyHelper.GetValueSource(this, LabelPresenter.HorizontalContentAlignmentProperty);

            // if the property was explicitly set the don't provide a default
            if (vs.BaseValueSource > BaseValueSource.Default)
                return null;

            // If we are using cell presenters then always map the alignment from the cellContentAlignment
            if (fl.UseCellPresenters)
            {
                CellContentAlignment cellContentAlignment = field.CellContentAlignmentResolved;
                if (verticalAlignment)
                {
                    if (verticalAlignment == true)
                        return FieldLayoutTemplateGenerator.GetVerticalAlignmentLabel(cellContentAlignment);
                }
                else
                {
                    if (verticalAlignment == false)
                        return FieldLayoutTemplateGenerator.GetHorizontalAlignmentLabel(cellContentAlignment);
                }
            }

            if (verticalAlignment)
                return VerticalAlignment.Center;
            else
                return HorizontalAlignment.Stretch;
        }

                #endregion //GetDefaultAlignment	
        
				#region ProcessMouseUpHelper

		
		
		
		private void ProcessMouseLeftButtonUpHelper( MouseButtonEventArgs eventArgs, bool dragStarted )
		{
			// reset the IsPressed property and capture the mouse
			this.SetValue( IsPressedPropertyKey, KnownBoxes.FalseBox );

			if ( this._mouseClickedOnLabel )
			{
				this._mouseClickedOnLabel = false;

				this.ReleaseMouseCapture( );

				if ( null != eventArgs && !eventArgs.Handled && !dragStarted )
				{
					IInputElement mouseOverElement = eventArgs.MouseDevice.DirectlyOver;

					if ( mouseOverElement is Visual &&
						( (Visual)mouseOverElement ).IsDescendantOf( this ) )
					{
						Field fld = this.Field;

						if ( fld != null )
						{
							// SSP 6/19/09 - NAS9.2 Field Chooser
							// Added the if block and enclosed the existing code into the else block.
							// 
							if ( this.IsInFieldChooser )
							{
								eventArgs.Handled = true;
							}
							else
							{
								if ( fld.PerformLabelClickAction( ) )
									eventArgs.Handled = true;
							}
						}
					}
				}
			}
		}

				#endregion // ProcessMouseUpHelper
    
            #endregion //Private Methods	

			#region Internal Methods

				#region StartDragHelper



		
		
		
		internal bool StartDragHelper( MouseEventArgs e, bool calledFromLabelPresenterMouseMove )
		{
			Field field = this.Field;
			if ( null == field )
				return false;

			// JM 04-09 CrossBand Grouping feature.
			// We also need to allow the drag to start if the GroupByAreaMode is one of the multiple FieldLayout modes
			// and the Field can be grouped (based on Field.AllowGroupByResolved).
			//if ( AllowFieldMoving.No != field.Owner.AllowFieldMovingResolved )
			DataPresenterBase dp = field.DataPresenter;
			bool multipleFieldLayoutGroupingEnabled =
										dp != null && dp.GroupByAreaMode != GroupByAreaMode.DefaultFieldLayoutOnly;
			if ( AllowFieldMoving.No != field.Owner.AllowFieldMovingResolved ||
				( multipleFieldLayoutGroupingEnabled && field.AllowGroupByResolved ) )
			{
				// JM 04-09 CrossBand Grouping feature.  Move this line above.
				//DataPresenterBase dp = field.DataPresenter;
				if ( null != dp
					
					// If a cell is in edit mode then exit edit mode and commit changes.
					// 
					&& dp.EndEditMode( true, false ) )
				{
					// Clear any flags like IsPressed since we are going to drag
					// mode where further mouse messages will be processed by the
					// drag manager logic.
					// 
					if ( calledFromLabelPresenterMouseMove )
						this.ProcessMouseLeftButtonUpHelper( null, true );

					// SSP 6/24/09 - NAS9.2 Field Chooser
					// 
					// --------------------------------------------------------------------------------
					FieldChooser fieldChooser = this.IsInFieldChooser
						? (FieldChooser)Utilities.GetAncestorFromType( this, typeof( FieldChooser ), true )
						: null;

					if ( null != fieldChooser ? fieldChooser.CaptureMouse( ) : dp.CaptureMouse( ) )
					{
						FieldDragManager fieldDragManager = new FieldDragManager( field, this, fieldChooser );
						fieldDragManager.StartDrag( e );
						return true;
					}

					
#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

					// --------------------------------------------------------------------------------
				}
			}

			return false;
		}



				#endregion // StartDragHelper

			#endregion // Internal Methods

		#endregion //Methods

		#region ISelectableElement Members

		ISelectableItem ISelectableElement.SelectableItem
		{
			get
			{
				Field fld = this.Field;

				// have the field cache this element 
				// AS 4/12/11 TFS62951
				// See the comments on the FieldLayout._lastSelectableItemLabel member variable.
				//
				//if (fld != null)
				//    fld.LastFieldLabelAccessed = this;
				if (fld != null && fld.Owner != null)
					fld.Owner.LastSelectableItemLabel = this;

				// JJD 01/19/07 - BR17350
				// If the label click action is nothing then return null
				if (fld.LabelClickActionResolved == LabelClickAction.Nothing)
				{
					// [JJM BR20183 2-23-07]
					// If we do not have separate header, return the DataContext (i.e., the Record)
					// as an ISelectableItem.
					if (fld.Owner.HasSeparateHeader)
						return null;

					return this.DataContext as ISelectableItem;
				}

				return fld as ISelectableItem;
			}
		}

		#endregion

        // JJD 2/7/08 - BR30444 - added selector class
        #region LPContentTemplateSelector private class

        internal class LPContentTemplateSelector : DataTemplateSelector
        {
            #region Members

            static internal readonly DataTemplate DataTemplateForImageSource;
            static internal readonly DataTemplate DataTemplateForString;

            #endregion //Members	
    
            #region Constructors

            static LPContentTemplateSelector()
            {
                #region Initialize DataTemplateForString

                DataTemplateForString = new DataTemplate(typeof(string));

                FrameworkElementFactory fefTextBlock = new FrameworkElementFactory(typeof(TextBlock));

                #region Bind Text property

                Binding binding = new Binding();
                binding.Mode = BindingMode.OneWay;

                fefTextBlock.SetBinding(TextBlock.TextProperty, binding);

                #endregion //Bind Text property

                #region Bind TextAlignment property

                binding = new Binding();
                RelativeSource rs = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(LabelPresenter), 1);

                binding.RelativeSource = rs;
                binding.Path = new PropertyPath("Field.LabelTextAlignmentResolved");
                binding.Mode = BindingMode.OneWay;

                fefTextBlock.SetBinding(TextBlock.TextAlignmentProperty, binding);

                #endregion //Bind TextAlignment property

                #region Bind TextTrimming property

                binding = new Binding();
                rs = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(LabelPresenter), 1);

                binding.RelativeSource = rs;
                binding.Path = new PropertyPath("Field.LabelTextTrimmingResolved");
                binding.Mode = BindingMode.OneWay;

                fefTextBlock.SetBinding(TextBlock.TextTrimmingProperty, binding);

                #endregion //Bind TextTrimming property

                #region Bind TextWrapping property

                binding = new Binding();
                rs = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(LabelPresenter), 1);

                binding.RelativeSource = rs;
                binding.Path = new PropertyPath("Field.LabelTextWrappingResolved");
                binding.Mode = BindingMode.OneWay;

                fefTextBlock.SetBinding(TextBlock.TextWrappingProperty, binding);

                #endregion //Bind TextWrapping property

                DataTemplateForString.VisualTree = fefTextBlock;

                DataTemplateForString.Seal();

                #endregion //Initialize DataTemplateForString

                #region Initialize DataTemplateForImageSource

                DataTemplateForImageSource = new DataTemplate(typeof(ImageSource));

                FrameworkElementFactory fefImage = new FrameworkElementFactory(typeof(Image));

                #region Bind Source property

                binding = new Binding();
                binding.Mode = BindingMode.OneWay;

                fefImage.SetBinding(Image.SourceProperty, binding);

                #endregion //Bind Source property

                DataTemplateForImageSource.VisualTree = fefImage;

                DataTemplateForImageSource.Seal();

                #endregion //Initialize DataTemplateForImageSource
            }

            #endregion //Constructors	
    
            #region Static Properties

                #region Instance

            static internal LPContentTemplateSelector Instance = new LPContentTemplateSelector();

                #endregion //Instance

            #endregion //Static Properties

            #region Static Methods

                #region SelectTemplateForImageSource

            internal static DataTemplate SelectTemplateForImageSource(ImageSource imageSource, ContentPresenter presenter)
            {
                Debug.Assert(presenter != null, "This only applies to ContentPresenter");
                Debug.Assert(imageSource != null, "This only applies to ImageSource");
                if (presenter == null || imageSource == null)
                    return null;

                object resource = presenter.TryFindResource(imageSource.GetType());

                if (resource is DataTemplate)
                    return null;

                // If the ImageSource is not frozen then we need to Freeze it
                // here. Otherwise, a null ref exception is thrown during ApplyTemplate
                if (!imageSource.IsFrozen)
                    imageSource.Freeze();

                return DataTemplateForImageSource;
            }

                #endregion //SelectTemplateForImageSource

            #endregion //Description	
        
            #region SelectTemplate

            public override DataTemplate SelectTemplate(object item, DependencyObject container)
            {
                ContentPresenter cp = container as ContentPresenter;

                Debug.Assert(cp != null, "This only applies to ContentPresenter");
                if (cp == null)
                    return null;

                 // If the label is a string then provide a default 
                if (item is string)
                    return DataTemplateForString;

                // If the label is an ImageSource then provide a default
                ImageSource imageSource = item as ImageSource;
                if (imageSource != null)
                    return SelectTemplateForImageSource(imageSource, cp);

                return null;
            }

            #endregion //SelectTemplate
        }

        #endregion //LPContentTemplateSelector private class
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