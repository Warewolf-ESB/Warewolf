using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Data;
using System.Security;
using Infragistics.Shared;
using Infragistics.Windows;
using Infragistics.Windows.Resizing;
using Infragistics.Windows.Controls;
using Infragistics.Windows.Editors.Events;
using Infragistics.Windows.Helpers;


namespace Infragistics.Windows.Editors
{    
	#region ValuePresenter class

	/// <summary>
	/// Abstract base class used for in place editing.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// <b>ValuePresenter</b> is an abstract base class from which various value presenters
	/// are derived from. These value presenters are used by controls like <b>XamDataGrid</b>.
	/// However typically there is no need to use them directly. This is mainly as supporting
	/// infrastructure for other complex controls that embed value editors inside them.
	/// </para>
	/// </remarks>
	[TemplatePart(Name="PART_EditorSite",Type=typeof(ContentPresenter))]
	//[Description( "Abstract base class for hosting value editors in an embedded manner." )]
	public abstract class ValuePresenter : ContentControl
	{
		#region Private Members

		private bool _isEditorSited;
		private ContentPresenter _editorSite;
		private ValueEditor _editor;
		private Style _editorStyle;
		private Type _editorType;
		private Type _valueType;

		internal int _previewMouseLeftButtonDown_EnterEditMode_TimeStamp;

		#endregion //Private Members

		#region Constructors

		static ValuePresenter()
		{
			// SSP 5/29/09 - IDataErrorInfo Support - TFS20681
			// ContentPresenter that contains the editor in the CellValuePresenter changes its content template.
			// This can momentarily cause the editor to be out of visual tree and and any of the following 
			// events raised by the editor won't bubble up. Therefore we need to hook into the ValueEditor class
			// instead of the ValuePresenter.
			// --------------------------------------------------------------------------------------------------
			EventManager.RegisterClassHandler( typeof( ValueEditor ), ValueEditor.EditModeStartingEvent, new EventHandler<EditModeStartingEventArgs>( ClassHandler_EditModeStarting ) );
			EventManager.RegisterClassHandler( typeof( ValueEditor ), ValueEditor.EditModeStartedEvent, new EventHandler<EditModeStartedEventArgs>( ClassHandler_EditModeStarted ) );
			EventManager.RegisterClassHandler( typeof( ValueEditor ), ValueEditor.EditModeEndingEvent, new EventHandler<EditModeEndingEventArgs>( ClassHandler_EditModeEnding ) );
			EventManager.RegisterClassHandler( typeof( ValueEditor ), ValueEditor.EditModeEndedEvent, new EventHandler<EditModeEndedEventArgs>( ClassHandler_EditModeEnded ) );
			EventManager.RegisterClassHandler( typeof( ValueEditor ), ValueEditor.EditModeValidationErrorEvent, new EventHandler<EditModeValidationErrorEventArgs>( ClassHandler_EditModeValidationError ) );
			
#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

			// SSP 10/20/09 TFS23919
			// As a result of above change, the event setters in the styles of the editors stopped
			// working because the above event handlers marked e.Handled = true. We need to revert
			// back to marking e.Handled = true in the class handlers attached to ValuePresenter so
			// the event setters on the styles of the editors work.
			// 
			
			EventManager.RegisterClassHandler( typeof( ValuePresenter ), ValueEditor.EditModeStartingEvent, new EventHandler<EditModeStartingEventArgs>( ClassHandler_ValuePresenter_EditModeStarting ) );
			EventManager.RegisterClassHandler( typeof( ValuePresenter ), ValueEditor.EditModeStartedEvent, new EventHandler<EditModeStartedEventArgs>( ClassHandler_ValuePresenter_EditModeStarted ) );
			EventManager.RegisterClassHandler( typeof( ValuePresenter ), ValueEditor.EditModeEndingEvent, new EventHandler<EditModeEndingEventArgs>( ClassHandler_ValuePresenter_EditModeEnding ) );
			EventManager.RegisterClassHandler( typeof( ValuePresenter ), ValueEditor.EditModeEndedEvent, new EventHandler<EditModeEndedEventArgs>( ClassHandler_ValuePresenter_EditModeEnded ) );
			EventManager.RegisterClassHandler( typeof( ValuePresenter ), ValueEditor.EditModeValidationErrorEvent, new EventHandler<EditModeValidationErrorEventArgs>( ClassHandler_ValuePresenter_EditModeValidationError ) );
			
			// --------------------------------------------------------------------------------------------------

			// MD 3/21/11 - TFS63735
			HorizontalContentAlignmentProperty.OverrideMetadata(typeof(ValuePresenter), new FrameworkPropertyMetadata(OnHorizontalContentAlignmentChanged));
			HorizontalAlignmentProperty.OverrideMetadata(typeof(ValuePresenter), new FrameworkPropertyMetadata(OnHorizontalAlignmentChanged));
		}

		/// <summary>
		/// Initializes a new <see cref="ValuePresenter"/>
		/// </summary>
		protected ValuePresenter()
		{
		}

		#endregion //Constructors

		#region Base class overrides

			#region OnApplyTemplate

		/// <summary>
		/// Called when the template is applied
		/// </summary>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			// SSP 10/25/10 TFS42749
			// Added an overload that takes in templateApplied parameter.
			// 
			//this.ValidateEditorIsSited();
			this.ValidateEditorIsSited( true );
		}

			#endregion //OnApplyTemplate	

			#region OnPreviewMouseLeftButtonDown

		/// <summary>
		/// Overridden.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnPreviewMouseLeftButtonDown( MouseButtonEventArgs e )
		{
			base.OnPreviewMouseLeftButtonDown( e );

			ValueEditor editor = this.Editor;
			if ( null == editor )
				return;

			bool isInEditModeBefore = editor.IsInEditMode;

			this.ProcessPreviewMouseLeftButtonDown( e );

			bool isInEditModeAfter = editor.IsInEditMode;

			if ( isInEditModeAfter && ! isInEditModeBefore )
			{
				// We need to know not to enter drag selection mode when the user clicks the
				// editor the first time to enter edit mode.
				// 
				_previewMouseLeftButtonDown_EnterEditMode_TimeStamp = e.Timestamp;

				// SSP 10/9/08 BR33762
				// Allow for setting selection start/length in the EditModeStarted event.
				// Moved this into ValueEditor.StartEditMode.
				// 
				
#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

			}
		}

			#endregion // OnPreviewMouseLeftButtonDown
        
		#endregion //Base class overrides

		#region Properties

			#region Public Properties

				#region Editor

		/// <summary>
		/// Returns the associated <see cref="ValueEditor"/> (read-only). 
		/// </summary>
		[Browsable(false)]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden)]
		[ReadOnly(true)]
		[Bindable(false)]
		public ValueEditor Editor { get { return this._editor; } }

				#endregion //Editor
    
				#region IsEditingAllowed

		/// <summary>
		/// Returns true if editing is allowed
		/// </summary>
		public abstract bool IsEditingAllowed{ get; }

				#endregion //IsEditingAllowed

				#region IsInEditMode

		/// <summary>
		/// Identifies the <see cref="IsInEditMode"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsInEditModeProperty = DependencyProperty.Register("IsInEditMode",
			typeof(bool), typeof(ValuePresenter), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, null, new CoerceValueCallback(CoerceIsInEditMode)));

		private static object CoerceIsInEditMode(DependencyObject target, object value)
		{
			ValuePresenter host = target as ValuePresenter;

			Debug.Assert(host != null);
			Debug.Assert(value is bool);

			if (host != null && value is bool)
			{
				bool newValue = (bool)value;

				if (host._editor == null)
					return false;

				if (newValue != host._editor.IsInEditMode)
					host._editor.IsInEditMode = false;

				return host._editor.IsInEditMode;
			}

			return value;
		}

		/// <summary>
		/// Gets/sets whether the editor is in edit mode
		/// </summary>
		/// <seealso cref="IsInEditModeProperty"/>
		/// <seealso cref="StartEditMode"/>
		/// <seealso cref="EndEditMode"/>
		//[Description("Gets/sets whether the editor is in edit mode")]
		//[Category("Behavior")]
		[Bindable(true)]
		public bool IsInEditMode
		{
			get
			{
				return (bool)this.GetValue(ValuePresenter.IsInEditModeProperty);
			}
			set
			{
				this.SetValue(ValuePresenter.IsInEditModeProperty, KnownBoxes.FromValue(value));
			}
		}

				#endregion //IsInEditMode
    
			#endregion //Public Properties

			#region Protected Properties

				#region AlwaysValidate

		// SSP 2/6/09 TFS10586
		// Added AlwaysValidate property. Data presenter overrides this in CellValuePresenter and returns
		// true if the cell is in add-record.
		// 
		/// <summary>
		/// Indicates whether the editor should always validate its value even if the user doesn't modify it.
		/// </summary>
		internal protected virtual bool AlwaysValidate
		{
			get
			{
				return false;
			}
		}

				#endregion // AlwaysValidate

				// AS 5/25/07 DefaultInvalidValueBehavior
				// Instead of the grid's cellvaluepresenter conditionally setting the 
				// InvalidValueBehavior of the editor, we'll let the editor query the
				// default InvalidValueBehavior if one has not been set on the editor
				// itself.
				//
				#region DefaultInvalidValueBehavior
		/// <summary>
		/// Returns the <see cref="InvalidValueBehavior"/> that should be used by the <see cref="ValueEditor"/> if the <see cref="ValueEditor.InvalidValueBehavior"/> has not been set.
		/// </summary>
		internal protected virtual InvalidValueBehavior DefaultInvalidValueBehavior
		{
			get { return InvalidValueBehavior.Default; }
		} 
				#endregion //DefaultInvalidValueBehavior

				#region EditorSite

		/// <summary>
		/// Returns the ContentPresenter in the visual tree that was named 'PART_EditorSite'. 
		/// </summary>
		protected ContentPresenter EditorSite { get { return this._editorSite; } }

				#endregion //EditorSite

				#region EditorStyle

		/// <summary>
		/// Gets/sets the style to use for the editor
		/// </summary>
		/// <remarks>
		/// <para>
		/// Note: that the TargetType property of the style must be a type that is derived from <see cref="ValueEditor"/>
		/// </para>
		/// </remarks>
		protected Style EditorStyle
		{
			get { return this._editorStyle; }
			set
			{
				// JJD 5/25/07 - Optimization 
				// Moved logic to helper routine
				this.SetEditorStyle(value, true);
			}
		}

				#endregion //EditorStyle	

				#region EditorType

		/// <summary>
		/// Gets/sets the type of the editor
		/// </summary>
		/// <remarks>
		/// <para>
		/// Note: that the property must be a type that is derived from <see cref="ValueEditor"/>
		/// </para>
		/// </remarks>
		protected Type EditorType
		{
			get { return this._editorType; }
			set
			{
				// JJD 5/25/07 - Optimization 
				// Moved logic to helper routine
				this.SetEditorType(value, true);
			}
		}

				#endregion //EditorType	

				#region EditorTypeResolved

		
		
		
		/// <summary>
		/// Returns the resolved editor type based on <see cref="EditorType"/>
		/// and the target type of the <see cref="EditorStyle"/>.
		/// </summary>
		protected Type EditorTypeResolved
		{
			get
			{
				Type editorType = _editorType;

				if ( null != _editorStyle )
				{
					Type styleTargetType = _editorStyle.TargetType;
					if ( null == editorType
						|| null != styleTargetType && !styleTargetType.IsAssignableFrom( editorType ) )
						editorType = styleTargetType;
				}

				return editorType;
			}
		}

				#endregion // EditorTypeResolved

				// JJD 6/29/11 - TFS79601 - added
				#region SupportsAsyncOperations

		/// <summary>
		/// Determines if asynchronous operations are supported (read-only)
		/// </summary>
		/// <value>True if asynchronous operations are supported, otherwise false. The default is true.</value>
		/// <remarks>
		/// <para class="body">The default implementation always returns true. This property is intended to be overridden and to return false during certain operations that are synchronous in nature, e.g. during a report or export operation.</para>
		/// </remarks>
		internal protected virtual bool SupportsAsyncOperations { get { return true; } }

				#endregion //SupportsAsyncOperations	
     
				#region ValueType

		/// <summary>
		/// Specifies the type of value this ValuePresenter will be presenting.
		/// </summary>
		public Type ValueType
		{
			get
			{
				return _valueType;
			}
			set
			{
				// JJD 5/25/07 - Optimization 
				// Moved logic to helper routine
				this.SetValueType(value, true);
			}
		}
				#endregion // ValueType

		#endregion //Protected Properties

		#endregion //Properties

		#region Methods

			#region Public Methods

				#region EndEditMode

		/// <summary>
		/// Takes the editor out of edit mode
		/// </summary>
		/// <param name="acceptChanges">If true will accept any cchanges that were made while in edit mode.</param>
		/// <param name="force">If true means the EditModeEnding event can't be cancelled.</param>
		/// <seealso cref="StartEditMode"/>
		/// <seealso cref="ValueEditor.EditModeEnding"/>
		/// <seealso cref="ValueEditor.EditModeEnded"/>
		/// <seealso cref="ValueEditor.EditModeStarting"/>
		/// <seealso cref="ValueEditor.EditModeStarted"/>
		public void EndEditMode(bool acceptChanges, bool force)
		{
			if ( this._editor == null)
				return;

			this._editor.EndEditMode(acceptChanges, force);
		}

				#endregion //EndEditMode

				#region StartEditMode

		/// <summary>
		/// Puts the editor into edit mode
		/// </summary>
		/// <returns>True if successful.</returns>
		/// <seealso cref="EndEditMode"/>
		/// <seealso cref="ValueEditor.EditModeEnding"/>
		/// <seealso cref="ValueEditor.EditModeEnded"/>
		/// <seealso cref="ValueEditor.EditModeStarting"/>
		/// <seealso cref="ValueEditor.EditModeStarted"/>
		public bool StartEditMode()
		{
			if (this._editor == null)
				return false;

			// SSP 5/29/09 - IDataErrorInfo Support
			// ContentPresenter that contains the editor in the CellValuePresenter changes its content template.
			// This can momentarily cause the editor to be out of visual tree which could cause problems with
			// giving focus to the editor. Therefore we have to make sure any pending content template application 
			// is done before we start the process of entering edit mode.
			// 
			this.UpdateLayout( );

            // AS 10/3/08 TFS8634
            // Moved check to the editor for consistency.
            //
			//if ( !this.IsEditingAllowed )
			//	return false;

			return this._editor.StartEditMode();
		}

				#endregion //StartEditMode

			#endregion //Public Methods

			#region Internal Methods

			#endregion //Internal Methods	
        
			#region Protected Methods

				// JJD 5/25/07 - Optimization
				// Added InitializeEditorSettings method
				#region InitializeEditorSettings

		/// <summary>
		/// Sets multiple editor settings in one atomic operation.
		/// </summary>
		/// <param name="editorType">The type of the editor to use.</param>
		/// <param name="valueType">The type to use when editng the value, i.e. 'edit as type'.</param>
		/// <param name="editorStyle">The style of the editor.</param>
		protected void InitializeEditorSettings(Type editorType, Type valueType, Style editorStyle)
		{
			// Pass in false to SetValueType so we don't trigger an invalidation
			this.SetValueType(valueType, false);

			// Pass in false to SetEditorType so we don't trigger an invalidation
			this.SetEditorType(editorType, false);

			// Pass in false to SetEditorStyle so we don't trigger an invalidation
			this.SetEditorStyle(editorStyle, false);

			if ( this._editorSite != null )
				this.ValidateEditorIsSited();
		}

				#endregion //InitializeEditorSettings	

                // JJD 1/9/09 - NA 2009 vol 1 - Record filtering - added
                #region IsCurrentValueValid

        /// <summary>
        /// Performs any additional validation required.
        /// </summary>
        /// <param name="error">out param containing the error if method returns false.</param>
        /// <remarks>
        /// <para class="note"><b>Note:</b> the default implementaion always returns true. This method is intended 
        /// for use by derived classes that need to perform some additional validatio logic.</para>
        /// </remarks>
        /// <returns>True if the current value is valid, otherwise false.</returns>
        internal protected virtual bool IsCurrentValueValid(out Exception error)
        {
            error = null;
            return true;
        }

                #endregion //IsCurrentValueValid	
    
				#region OnEditorCreated

		/// <summary>
		/// Called after the editor has been created but before its Content is set, its Style has been applied and before it has been sited.
		/// </summary>
		/// <param name="editor">The ValueEditor that was just created</param>
		protected virtual void OnEditorCreated(ValueEditor editor) 
		{
		}

				#endregion //OnEditorCreated	

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
		protected abstract void OnEditModeStarting(EditModeStartingEventArgs e);

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
		protected abstract void OnEditModeStarted(EditModeStartedEventArgs e);

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
		protected abstract void OnEditModeEnding(EditModeEndingEventArgs e);

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
		protected abstract void OnEditModeEnded(EditModeEndedEventArgs e);

				#endregion //OnEditModeEnded	

				#region OnEditModeValidationError

		/// <summary>
		/// Called when the embedded editor has input validation error
		/// </summary>
		/// <param name="e">The event arguments</param>
		/// <seealso cref="ValueEditor.EditModeValidationError"/>
		protected abstract void OnEditModeValidationError( EditModeValidationErrorEventArgs e );

				#endregion // OnEditModeValidationError

				// MD 7/16/10 - TFS26592
				#region OnEditorValueChanged

		/// <summary>
		/// Called when the editor's value has changed but it's <seealso cref="ValueEditor.ValueChanged"/> event has been suppressed.
		/// </summary>
		/// <param name="args">The event arguments</param>
		/// <seealso cref="ValueEditor.ValueChanged"/>
		protected internal virtual void OnEditorValueChanged(RoutedEventArgs args) { } 

				#endregion // OnEditorValueChanged

				#region CommitEditValue

		/// <summary>
		/// Called after OnEditModeEnding however before OnEditModeEnded. This is called after
		/// input validation succeeds to let the host know to commit the value.
		/// </summary>
		/// <param name="editedValue">The edited value that is to be committed.</param>
		/// <param name="stayInEditMode">Whether the editor should cancel the operation of exitting edit mode.</param>
		/// <returns>Returns true if commit succeeded, false otherwise.</returns>
		internal protected abstract bool CommitEditValue( object editedValue, out bool stayInEditMode );

				#endregion // CommitEditValue

				#region OnValueChanged

		/// <summary>
		/// Called when the value has been changed.
		/// </summary>
		internal protected virtual void OnValueChanged()
		{
		}

				#endregion //OnValueChanged	

				#region OnValueEditorKeyDown

		/// <summary>
		/// Called by the ValueEditor before it process its OnKeyDown. Default implementation
		/// returns false. You can override and return true from this method if you want to
		/// prevent the ValueEditor from processing the key.
		/// </summary>
		/// <param name="e"></param>
		/// <returns>Return true to prevent the value editor from processing the key.</returns>
		internal protected virtual bool OnValueEditorKeyDown( KeyEventArgs e )
		{
			return false;
		}

				#endregion // OnValueEditorKeyDown

				#region OnValueValidated

		// SSP 4/17/09 NAS9.2 IDataErrorInfo Support
		// Added ValidationErrorInfo class.
		// 
		/// <summary>
		/// Called whenever the value editor validates the value.
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
		internal protected virtual void OnValueValidated( )
		{
		}

				#endregion // OnValueValidated
    
				#region ProcessPreviewMouseLeftButtonDown

		/// <summary>
		/// This class overrides OnPreviewMouseLeftButtonDown. This method is called by
		/// OnPreviewMouseLeftButtonDown implementation for derived editors to go into
		/// edit mode and do any processing required from this event.
		/// </summary>
		/// <param name="e"></param>
		protected virtual void ProcessPreviewMouseLeftButtonDown( MouseButtonEventArgs e )
		{
		}

				#endregion // ProcessPreviewMouseLeftButtonDown

				// MD 7/16/10 - TFS26592
				#region ShouldSuppressEvent

		/// <summary>
		/// Determines whether the event associated with the specified event arguments should be suppressed.
		/// </summary>
		/// <param name="args">The event arguments which will be fired with the event if it is not suppressed.</param>
		/// <returns>True if the event should be suppressed; False otherwise.</returns>
		protected internal virtual bool ShouldSuppressEvent(RoutedEventArgs args)
		{
			return false;
		} 

				#endregion // ShouldSuppressEvent

			#endregion //Protected Methods

			#region Private Methods

				#region ClassHandler_EditModeStarting

		// MD 7/16/10 - TFS26592
		// Made internal so this could be called by the ValueEditor in the case that the event is suppressed.
		//private static void ClassHandler_EditModeStarting(object sender, EditModeStartingEventArgs e)
		internal static void ClassHandler_EditModeStarting(object sender, EditModeStartingEventArgs e)
		{
			ValueEditor editor = e.OriginalSource as ValueEditor;

			Debug.Assert(editor != null);

			if (editor != null)
			{
				ValuePresenter host = editor.Host as ValuePresenter;

				if (host != null)
				{
					host.OnEditModeStarting(e);

					
					
					
					
				}
			}
		}

				#endregion //ClassHandler_EditModeStarting	

				#region ClassHandler_EditModeStarted

		// MD 7/16/10 - TFS26592
		// Made internal so this could be called by the ValueEditor in the case that the event is suppressed.
		//private static void ClassHandler_EditModeStarted(object sender, EditModeStartedEventArgs e)
		internal static void ClassHandler_EditModeStarted(object sender, EditModeStartedEventArgs e)
		{
			ValueEditor editor = e.OriginalSource as ValueEditor;

			Debug.Assert(editor != null);

			if (editor != null)
			{
				ValuePresenter host = editor.Host as ValuePresenter;

				if (host != null)
				{
					host.IsInEditMode = true; 
					host.OnEditModeStarted(e);

					
					
					
					
				}
			}
		}

				#endregion //ClassHandler_EditModeStarted	

				#region ClassHandler_EditModeEnding

		// MD 7/16/10 - TFS26592
		// Made internal so this could be called by the ValueEditor in the case that the event is suppressed.
		//private static void ClassHandler_EditModeEnding(object sender, EditModeEndingEventArgs e)
		internal static void ClassHandler_EditModeEnding(object sender, EditModeEndingEventArgs e)
		{
			ValueEditor editor = e.OriginalSource as ValueEditor;

			Debug.Assert(editor != null);

			if (editor != null)
			{
				ValuePresenter host = editor.Host as ValuePresenter;

				if (host != null)
				{
					host.OnEditModeEnding(e);

					
					
					
					
				}
			}
		}

				#endregion //ClassHandler_EditModeEnding	

				#region ClassHandler_EditModeEnded

		// MD 7/16/10 - TFS26592
		// Made internal so this could be called by the ValueEditor in the case that the event is suppressed.
		//private static void ClassHandler_EditModeEnded(object sender, EditModeEndedEventArgs e)
		internal static void ClassHandler_EditModeEnded(object sender, EditModeEndedEventArgs e)
		{
			ValueEditor editor = e.OriginalSource as ValueEditor;

			Debug.Assert(editor != null);

			if (editor != null)
			{
				ValuePresenter host = editor.Host as ValuePresenter;

				if (host != null)
				{
					host.IsInEditMode = false;
					host.OnEditModeEnded(e);

					
					
					
					
				}
			}
		}

				#endregion //ClassHandler_EditModeEnded	

				#region ClassHandler_EditModeValidationError

		// MD 7/16/10 - TFS26592
		// Made internal so this could be called by the ValueEditor in the case that the event is suppressed.
		//private static void ClassHandler_EditModeValidationError( object sender, EditModeValidationErrorEventArgs e )
		internal static void ClassHandler_EditModeValidationError(object sender, EditModeValidationErrorEventArgs e)
		{
			ValueEditor editor = e.OriginalSource as ValueEditor;

			Debug.Assert( editor != null );

			if ( editor != null )
			{
				ValuePresenter host = editor.Host as ValuePresenter;

				if ( host != null )
				{
					host.OnEditModeValidationError( e );

					
					
					
					
				}
			}
		}

				#endregion // ClassHandler_EditModeValidationError

				#region ClassHandler_ValuePresenter_MarkHandledHelper

		// SSP 10/20/09 TFS23919
		// 
		private static void ClassHandler_ValuePresenter_MarkHandledHelper( RoutedEventArgs e )
		{
			ValueEditor editor = e.OriginalSource as ValueEditor;
			ValuePresenter host = null != editor ? editor.Host as ValuePresenter : null;
			if ( null != host )
				e.Handled = true;
		}

				#endregion // ClassHandler_ValuePresenter_MarkHandledHelper

				#region ClassHandler_ValuePresenter_EditModeStarting

		// SSP 10/20/09 TFS23919
		// 
		private static void ClassHandler_ValuePresenter_EditModeStarting( object sender, EditModeStartingEventArgs e )
		{
			ClassHandler_ValuePresenter_MarkHandledHelper( e );
		}

				#endregion // ClassHandler_ValuePresenter_EditModeStarting

				#region ClassHandler_ValuePresenter_EditModeStarted

		// SSP 10/20/09 TFS23919
		// 
		private static void ClassHandler_ValuePresenter_EditModeStarted( object sender, EditModeStartedEventArgs e )
		{
			ClassHandler_ValuePresenter_MarkHandledHelper( e );
		}

				#endregion // ClassHandler_ValuePresenter_EditModeStarted

				#region ClassHandler_ValuePresenter_EditModeEnding

		// SSP 10/20/09 TFS23919
		// 
		private static void ClassHandler_ValuePresenter_EditModeEnding( object sender, EditModeEndingEventArgs e )
		{
			ClassHandler_ValuePresenter_MarkHandledHelper( e );
		}

				#endregion // ClassHandler_ValuePresenter_EditModeEnding

				#region ClassHandler_ValuePresenter_EditModeEnded

		// SSP 10/20/09 TFS23919
		// 
		private static void ClassHandler_ValuePresenter_EditModeEnded( object sender, EditModeEndedEventArgs e )
		{
			ClassHandler_ValuePresenter_MarkHandledHelper( e );
		}

				#endregion // ClassHandler_ValuePresenter_EditModeEnded

				#region ClassHandler_ValuePresenter_EditModeValidationError

		// SSP 10/20/09 TFS23919
		// 
		private static void ClassHandler_ValuePresenter_EditModeValidationError( object sender, EditModeValidationErrorEventArgs e )
		{
			ClassHandler_ValuePresenter_MarkHandledHelper( e );
		}

				#endregion // ClassHandler_ValuePresenter_EditModeValidationError

				#region DirtyEditor

		private void DirtyEditor( bool validateEditorIsSited )
		{
			_editor = null;
			_isEditorSited = false;

			if ( validateEditorIsSited )
				this.ValidateEditorIsSited( );
		}

				#endregion // DirtyEditor

				// MD 3/21/11 - TFS63735
				#region OnHorizontalContentAlignmentChanged

		private static void OnHorizontalContentAlignmentChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			ValuePresenter instance = (ValuePresenter)target;
			instance.VerfiyOptimizedWidthMeasurement();
		}

				#endregion // OnHorizontalContentAlignmentChanged

				// MD 3/21/11 - TFS63735
				#region OnHorizontalAlignmentChanged

		private static void OnHorizontalAlignmentChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			ValuePresenter instance = (ValuePresenter)target;
			instance.VerfiyOptimizedWidthMeasurement();
		} 

				#endregion // OnHorizontalAlignmentChanged

				#region SetEditorType

		// JJD 5/25/07 - Optimization 
		// Moved logic to helper routine
		private void SetEditorType(Type value, bool validateEditorIsSited)
		{
			if (this._editorType != value)
			{
				if (value != null)
				{
					if (!typeof(ValueEditor).IsAssignableFrom(value))
						throw new ArgumentException(ValueEditor.GetString("LE_ArgumentException_9"), "value");
				}

				this._editorType = value;

				if (this._editor != null)
				{
					if (this._editorType == null ||
						// SSP 3/7/08 BR30878 BR30658 
						// 
						//this._editorType != this._editor.GetType()
						this.EditorTypeResolved != this._editor.GetType( )
						)
					{
						this.DirtyEditor(validateEditorIsSited);
					}
				}
			}
		}

				#endregion //SetEditorType	

				#region SetEditorStyle

		// JJD 5/25/07 - Optimization 
		// Moved logic to helper routine
		private void SetEditorStyle(Style value, bool validateEditorIsSited)
		{
			if (this._editorStyle != value)
			{
				if (value != null)
				{
                    // JJD 4/23/09 - TFS17037
                    // Call Utilities.VerifyTargetTypeOfStyle method instead
                    //if (value.TargetType == null ||
                    //    !typeof(ValueEditor).IsAssignableFrom(value.TargetType))
                    //    throw new ArgumentException(SR.GetString("LE_ArgumentException_8"), "value");
                    Utilities.ValidateTargetTypeOfStyle(value, typeof(ValueEditor), "EditorStyle");

					// SSP 3/7/08 BR30878 
					// Added EditorTypeResolved. We should resolved the editor type rather than
					// override EditorType property because this logic relies on the order in which
					// EditorType and EditorStyle are set.
					// 
					
#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

				}

				this._editorStyle = value;

				if (this._editor != null)
				{
					if (this._editorStyle != this._editor.Style
						// SSP 3/7/08 BR30878 BR30658 
						// 
						//|| this._editorType != this._editor.GetType()
						|| this.EditorTypeResolved != this._editor.GetType( )
						)
					{
						this._editor = null;
						this._isEditorSited = false;
					}
				}

				if (validateEditorIsSited)
					this.ValidateEditorIsSited();
			}
		}

				#endregion //SetEditorStyle	

				#region SetValueType

		// JJD 5/25/07 - Optimization 
		// Moved logic to helper routine
		private void SetValueType(Type value, bool validateEditorIsSited)
		{
			if (_valueType != value)
			{
				_valueType = value;

				if (_isEditorSited)
					this.DirtyEditor(validateEditorIsSited);
			}
		}

				#endregion //SetValueType	
            	
				#region ValidateEditorIsSited

		// SSP 10/25/10 TFS42749
		// 
		private void ValidateEditorIsSited( )
		{
			this.ValidateEditorIsSited( false );
		}

		// SSP 10/25/10 TFS42749
		// Added an overload that takes in templateApplied parameter.
		// 
		//private void ValidateEditorIsSited()
		private void ValidateEditorIsSited( bool templateApplied )
		{
			// SSP 10/25/10 TFS42749
			// 
			//if (this._editorSite == null)
			bool initializeEditor = false;
			if ( this._editorSite == null || templateApplied )
			{
				DependencyObject elementLabelledEditorSite = base.GetTemplateChild("PART_EditorSite");

				// SSP 10/25/10 TFS42749
				// Related to above change.
				// 
				// ----------------------------------------------------------------------------
				//this._editorSite = elementLabelledEditorSite as ContentPresenter;
				ContentPresenter newEditorSite = elementLabelledEditorSite as ContentPresenter;
				if ( newEditorSite != _editorSite )
				{
					_editorSite = newEditorSite;
					_isEditorSited = false;
					initializeEditor = true;
				}
				// ----------------------------------------------------------------------------

				if (this._editorSite == null &&
					elementLabelledEditorSite != null)
					throw new NotSupportedException(ValueEditor.GetString("LE_NotSupportedException_4"));
			}

			// SSP 3/7/08 BR30878 BR30658 
			// Use the new EditorTypeResolved property instead.
			// 
			//Type editorType = this.EditorType;
			Type editorType = this.EditorTypeResolved;

			if (this._isEditorSited == true ||
				editorType == null ||
				this._editorSite == null)
				return;

			this._isEditorSited = true;

			try
			{
				if (this._editor == null)
				{
					// SSP 10/25/10 TFS42749
					// 
					initializeEditor = true;

					// create the editor

					// JJD 4/25/07
					// Optimization - for the known editor types don't use reflection to
					// create the editor
					//this._editor = Activator.CreateInstance(this._editorType) as ValueEditor;
					if ( editorType == typeof(XamTextEditor))
						_editor = new XamTextEditor();

					else
					if ( editorType == typeof(XamMaskedEditor))
						this._editor = new XamMaskedEditor();

					else
					if ( editorType == typeof(XamCheckEditor))
						this._editor = new XamCheckEditor();

					else
					if ( editorType == typeof(XamNumericEditor))
						this._editor = new XamNumericEditor();
					else
					if ( editorType == typeof(XamDateTimeEditor))
						this._editor = new XamDateTimeEditor();
					else
					if ( editorType == typeof(XamCurrencyEditor))
						this._editor = new XamCurrencyEditor();

					else
						this._editor = Activator.CreateInstance(editorType) as ValueEditor;
				}

				// SSP 10/25/10 TFS42749
				// Enclosed the existing code into the if block. The existing code was moved out
				// of the above if block.
				// 
				if ( initializeEditor )
				{
					// give th editor a refernce to this object.
					this._editor.InitializeHostInfo(this, this);

					// set the value type on the editor
					this._editor.ValueType = this.ValueType;

					// call the virtual OnEditorCreated method to allow
					// derived classes to do additional setup
					this.OnEditorCreated(this._editor);

					// set the specified style
					if (this._editorStyle != null)
						this._editor.Style = this._editorStyle;

					// set the editor's value to our content
					this._editor.Value = this.Content;

					// set the editor site's content to the editor we just created
					this._editorSite.Content = this._editor;

					// MD 3/21/11 - TFS63735
					this.VerfiyOptimizedWidthMeasurement();
				}
			}
			catch (SecurityException)
			{
			}

		}

				#endregion //ValidateEditorIsSited	
    
				#region ValidateHostContext

		private void ValidateHostContext(object hostContext)
		{
			if (hostContext == null)
				throw new ArgumentNullException("hostContext");

			if (hostContext != this)
				throw new ArgumentException( ValueEditor.GetString( "LE_ArgumentException_10" ), "hostContext" );

		}

				#endregion //ValidateHostContext

				// MD 3/21/11 - TFS63735
				#region VerfiyOptimizedWidthMeasurement

		private void VerfiyOptimizedWidthMeasurement()
		{
			// If either the HorizontalContentAlignment or HorizontalAlignment is center or right, we should disable the
			// width optimization on the SimpleTextBlock so it measures the text and the alignment looks correct to the user.
			bool optimizeWidthMeasurement = true;

			switch (this.HorizontalContentAlignment)
			{
				case HorizontalAlignment.Center:
				case HorizontalAlignment.Right:
					optimizeWidthMeasurement = false;
					break;
			}

			if (optimizeWidthMeasurement)
			{
				switch (this.HorizontalAlignment)
				{
					case HorizontalAlignment.Center:
					case HorizontalAlignment.Right:
						optimizeWidthMeasurement = false;
						break;
				}
			}

			this.SetValue(SimpleTextBlock.OptimizeWidthMeasurementProperty, KnownBoxes.FromValue(optimizeWidthMeasurement));
		}

				#endregion // VerfiyOptimizedWidthMeasurement

			#endregion //Private Methods	
        
		#endregion //Methods
	}

	#endregion //ValuePresenter class	
    
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