using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Diagnostics;
using System.ComponentModel;
using System.Globalization;
using Infragistics.Shared;
using Infragistics.Windows;
using Infragistics.Windows.Resizing;
using Infragistics.Windows.Controls;
using Infragistics.Windows.Licensing;
using Infragistics.Windows.Commands;
using Infragistics.Windows.Editors.Events;
using Infragistics.Windows.Controls.Events;
using Infragistics.Windows.Helpers;
using Infragistics.Windows.Automation.Peers.Editors;
using System.Windows.Automation.Peers;
using Infragistics.Windows.Internal;


namespace Infragistics.Windows.Editors
{
	#region XamCheckEditor Class

	/// <summary>
	/// An editor control that can be used to display or edit boolean values.
	/// </summary>
	/// <remarks>
	/// <p class="body">The <b>XamCheckEditor</b> inherits from <see cref="ValueEditor"/> and is designed to be embedded within 
	/// value editor hosts, like the <b>XamDataGrid</b>, although it can be used as a stand alone control as well. It displays a 
	/// checkbox which the user can click to change the value.</p>
	/// <p class="body">The control can represent three states - checked, unchecked and 
	/// indeterminate. By default, the control only toggles between checked and unchecked. To support indeterminate, the  
	/// <see cref="XamCheckEditor.IsThreeState"/> property should be set to true.</p>
	/// </remarks>

    // JJD 4/15/10 - NA2010 Vol 2 - Added support for VisualStateManager
    [TemplateVisualState(Name = VisualStateUtilities.StateUnchecked,       GroupName = VisualStateUtilities.GroupCheck)]
    [TemplateVisualState(Name = VisualStateUtilities.StateChecked,         GroupName = VisualStateUtilities.GroupCheck)]
    [TemplateVisualState(Name = VisualStateUtilities.StateIndeterminate,   GroupName = VisualStateUtilities.GroupCheck)]

    
	
	//[Description( "Check Editor that uses a checkbox to display and edit values." )]
	public class XamCheckEditor : ValueEditor
	{
		#region static constants

		#endregion //static constants

		#region Variables

		private UltraLicense _license;		

		#endregion //Variables

		#region Constructors

		static XamCheckEditor( )
		{
			// Default value type for this editor should be double.
			ValueTypeProperty.OverrideMetadata( typeof( XamCheckEditor ), new FrameworkPropertyMetadata( typeof( bool ) ) );

			// Default the alignment to Center for the checkbox.
			HorizontalContentAlignmentProperty.OverrideMetadata( typeof( XamCheckEditor ), new FrameworkPropertyMetadata( HorizontalAlignment.Center ) );
			VerticalContentAlignmentProperty.OverrideMetadata( typeof( XamCheckEditor ), new FrameworkPropertyMetadata( VerticalAlignment.Center ) );

			//This OverrideMetadata call tells the system that this element wants to provide a style that is different than its base class.
			//This style is defined in themes\generic.xaml
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata( typeof( XamCheckEditor ), new FrameworkPropertyMetadata( typeof( XamCheckEditor ) ) );

			// SSP 2/11/09 TFS13380
			// This is to make tab/arrow navigation work properly.
			// 
			KeyboardNavigation.TabNavigationProperty.OverrideMetadata( typeof( XamCheckEditor ), new FrameworkPropertyMetadata( KeyboardNavigationMode.Local ) );
			KeyboardNavigation.ControlTabNavigationProperty.OverrideMetadata( typeof( XamCheckEditor ), new FrameworkPropertyMetadata( KeyboardNavigationMode.Local ) );
			KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata( typeof( XamCheckEditor ), new FrameworkPropertyMetadata( KeyboardNavigationMode.Local ) );
			KeyboardNavigation.IsTabStopProperty.OverrideMetadata( typeof( XamCheckEditor ), new FrameworkPropertyMetadata( false ) );
		}

		/// <summary>
		/// Initializes a new <see cref="XamCheckEditor"/> instance
		/// </summary>
		public XamCheckEditor( )
		{
			// verify and cache the license
			//
			// Wrapped in a try/catch for a FileNotFoundException.
			// When the assembly is loaded dynamically, VS seems 
			// to be trying to reload a copy of Shared even though 
			// one is in memory. This generates a FileNotFoundException
			// when the dll is not in the gac and not in the AppBase
			// for the AppDomain.
			//
			// AS 11/7/07 BR21903
			// Always do the license checks.
			//
			//if ( DesignerProperties.GetIsInDesignMode( this ) )
			{
				try
				{
					// We need to pass our type into the method since we do not want to pass in 
					// the derived type.
					this._license = LicenseManager.Validate( typeof( XamCheckEditor ), this ) as UltraLicense;
				}
				catch ( System.IO.FileNotFoundException ) { }
			}
		}

		#endregion // Constructors

		#region Base Overrides

		#region InitializeValueProperties

		
		
		
		
#region Infragistics Source Cleanup (Region)













#endregion // Infragistics Source Cleanup (Region)


		/// <summary>
		/// Overridden. Initializes the value properties. This synchronizes all the value properties if one of
		/// them is set in xaml since we delay syncrhonization until after initialization in case
		/// other settings in xaml effect how they are synchronized.
		/// </summary>
		internal override void InitializeValueProperties( )
		{
			// We are deferring synchronization of Value and Text properties until the control
			// is initialized. This is because during initialization not all properties (like
			// ValueType or Mask) may have been set when Value or Text gets set.
			// 
			// SSP 1/7/08 BR29457
			// 
			//if ( DependencyProperty.UnsetValue != this.ReadLocalValue( IsCheckedProperty ) )
			if ( Utils.IsValuePropertySet( IsCheckedProperty, this ) )
				this.Value = this.IsChecked;

			base.InitializeValueProperties( );

			
			
			this.SetIsCheckedHelper( this.Value );
		}

		#endregion // InitializeValueProperties

		#region CanEditType

		/// <summary>
		/// Determines if the editor natively supports editing values of specified type.
		/// </summary>
		/// <param name="type">The data type to check.</param>
		/// <returns>Returns True if the editor natively supports editing values of specified type, False otherwise.</returns>
		/// <remarks>
		/// <p class="body">
		/// XamCheckEditor's implementation returns True for only the string type since that's
		/// the only data type it natively renders and edits.
		/// </p>
		/// <p class="body">
		/// See ValueEditor's <see cref="ValueEditor.CanEditType"/> for more information.
		/// </p>
		/// </remarks>
		public override bool CanEditType( Type type )
		{
			return typeof( bool ) == type || typeof( bool? ) == type;
		}

		#endregion //CanEditType

		#region CanRenderType

		/// <summary>
		/// Determines if the editor natively supports displaying of values of specified type.
		/// </summary>
		/// <param name="type">The data type to check.</param>
		/// <returns>Returns True if the editor natively supports displaying values of specified type, False otherwise.</returns>
		/// <remarks>
		/// <p class="body">
		/// XamCheckEditor's implementation returns True for only the string type since that's
		/// the only data type it natively renders and edits.
		/// </p>
		/// <p class="body">
		/// See ValueEditor's <see cref="ValueEditor.CanRenderType"/> for more information.
		/// </p>
		/// </remarks>
		public override bool CanRenderType( Type type )
		{
			return this.CanEditType( type );
		}

		#endregion //CanRenderType

		#region IsExtentBasedOnValue

		/// <summary>
		/// Indicates whether the desired width or the height of the editor is based on the value.
		/// </summary>
		/// <param name="orientation">Orientation of the extent being evaluated. Horizontal indicates the width and vertical indicates the height.</param>
		/// <returns>True if extent is based on the value.</returns>
		/// <remarks>
		/// <para class="body">
		/// XamCheckEditor's implementation returns False since the value of the editor does not have any
		/// effect on the desired size of the control.
		/// </para>
		/// <para class="body">
		/// See ValueEditor's <see cref="ValueEditor.IsExtentBasedOnValue"/> for more information.
		/// </para>
		/// </remarks>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public override bool IsExtentBasedOnValue( Orientation orientation )
		{
			return false;
		}

		#endregion // IsExtentBasedOnValue

		#region OnCreateAutomationPeer
		/// <summary>
		/// Returns an automation peer that exposes the <see cref="XamCheckEditor"/> to UI Automation.
		/// </summary>
		/// <returns>A <see cref="Infragistics.Windows.Automation.Peers.Editors.XamCheckEditorAutomationPeer"/></returns>
		protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
		{
			return new Infragistics.Windows.Automation.Peers.Editors.XamCheckEditorAutomationPeer(this);
		}
		#endregion //OnCreateAutomationPeer

		#region OnPreviewMouseLeftButtonDown

		// SSP 5/9/08 BR32695
		// 
		/// <summary>
		/// Preview method called when the left mouse button is pressed.
		/// </summary>
		/// <param name="e">The event args containing more information on the mouse event.</param>
		protected override void OnPreviewMouseLeftButtonDown( MouseButtonEventArgs e )
		{
			base.OnPreviewMouseLeftButtonDown( e );

			if ( ! e.Handled )
				this.StartEditModeOnMouseDownHelper( );
		}

		#endregion // OnPreviewMouseLeftButtonDown

        #region SetVisualState


        // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
        /// <summary>
        /// Called to set the VisualStates of the editor
        /// </summary>
        /// <param name="useTransitions">Determines whether transitions should be used.</param>
        protected override void SetVisualState(bool useTransitions)
        {
            base.SetVisualState(useTransitions);

            bool? isChecked = this.IsChecked;

            // Set Check states
            if (isChecked == null)
                VisualStateManager.GoToState(this, VisualStateUtilities.StateIndeterminate, useTransitions);
            else if (isChecked.Value == true)
                VisualStateManager.GoToState(this, VisualStateUtilities.StateChecked, useTransitions);
            else
                VisualStateManager.GoToState(this, VisualStateUtilities.StateUnchecked, useTransitions);
        }


        #endregion //SetVisualState	
    
		#region SyncValuePropertiesOverride

		
		
		

		
#region Infragistics Source Cleanup (Region)












#endregion // Infragistics Source Cleanup (Region)


		/// <summary>
		/// Overridden. Called to synchronize value and text properties. Derived classes can override this
		/// method if they have their own value properties (like the XamCheckEditor which has
		/// IsChecked property) because the Value, Text and all other editor specific value
		/// related properties need to be kept in sync. Default implementation synchronizes
		/// Value and Text properties.
		/// </summary>
		/// <param name="prop">Property that changed.</param>
		/// <param name="newValue">New value of the property.</param>
		/// <param name="error">Set this to the any error message if synchronization fails (for example
		/// because of type conversion or some other problem with syncrhonizing the new value).</param>
		/// <returns>Value indicating whether the new value should be considered valid. If false is
		/// returned, IsValueValid property will be set to false.</returns>
		internal override bool SyncValuePropertiesOverride( DependencyProperty prop, object newValue, out Exception error )
		{
			bool retVal = base.SyncValuePropertiesOverride( prop, newValue, out error );
			
			this.SetIsCheckedHelper( this.Value );

			return retVal;
		}

		#endregion // SyncValuePropertiesOverride

		#endregion // Base Overrides

		#region Public Properties

		#region IsChecked

		/// <summary>
		/// Identifies the <see cref="IsChecked"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register(
			"IsChecked",
			typeof( bool? ),
			typeof( XamCheckEditor ),
			new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.None,
				new PropertyChangedCallback( OnIsCheckedChanged ) )
			);

		/// <summary>
		/// Gets or sets the checked state of the check editor. Null indicates the check editor is in indeterminate
		/// state.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// When the check editor is checked the IsChecked will return True. When it's unchecked the property
		/// will return False. When it's in indeterminate state the property will return null.
		/// </para>
		/// <para class="body">
		/// <b>IsChecked</b>, <see cref="ValueEditor.Value"/> and <see cref="ValueEditor.Text"/> are all 
		/// synchronized. When the editor's value is modified, all three properties will reflect the
		/// current state of the check editor.
		/// </para>
		/// </remarks>
		/// <seealso cref="ValueEditor.Value"/>
		/// <seealso cref="ValueEditor.Text"/>
		//[Description( "Gets or sets the checked state of the check editor." )]
		//[Category( "Data" )]
		[Bindable( true )]
		[Browsable( false )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		[TypeConverter(typeof(Infragistics.Windows.Helpers.NullableConverter<bool>))] // AS 5/15/08 BR32816
		public bool? IsChecked
		{
			get
			{
				return (bool?)this.GetValue( IsCheckedProperty );
			}
			set
			{
				this.SetValue(IsCheckedProperty, KnownBoxes.FromValue(value));
			}
		}

		private static void OnIsCheckedChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			XamCheckEditor editor = (XamCheckEditor)dependencyObject;

			// If IsChecked was being set as a result of change in Value property, then don't set
			// the Value property recursively.
			// 
			if ( ! editor.SyncingValueProperties )
				editor.Value = e.NewValue;

            // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
            editor.UpdateVisualStates();


			// raise a property changed for the automation peer
			XamCheckEditorAutomationPeer peer = UIElementAutomationPeer.FromElement(editor) as XamCheckEditorAutomationPeer;

			if (null != peer)
				peer.RaiseToggleStatePropertyChangedEvent((bool?)e.OldValue, (bool?)e.NewValue);
		}

		#endregion // IsChecked

		#region IsThreeState

		/// <summary>
		/// Identifies the <see cref="IsThreeState"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsThreeStateProperty = DependencyProperty.Register(
			"IsThreeState",
			typeof( bool ),
			typeof( XamCheckEditor ),
			new FrameworkPropertyMetadata( KnownBoxes.FalseBox, FrameworkPropertyMetadataOptions.None,
				null )
			);

		/// <summary>
		/// Specifies whether the check editor toggles between three states or two states. Default value is <b>false</b>.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// If <b>IsThreeState</b> is set to true then the check editor will toggle between three states: checked, unchecked and indeterminate.
		/// When the editor is in indeterminate state, the <see cref="ValueEditor.Value"/> and <see cref="IsChecked"/> properties will return <b>null</b> value. 
		/// </para>
		/// </remarks>
		//[Description( "Specifies whether the check editor toggles between three or two states." )]
		//[Category( "Behavior" )]
		[Bindable( true )]
		public bool IsThreeState
		{
			get
			{
				return (bool)this.GetValue( IsThreeStateProperty );
			}
			set
			{
				this.SetValue( IsThreeStateProperty, value );
			}
		}

		/// <summary>
		/// Returns true if the IsThreeState property is set to a non-default value.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public bool ShouldSerializeIsThreeState( )
		{
			return Utilities.ShouldSerialize( IsThreeStateProperty, this );
		}

		/// <summary>
		/// Resets the IsThreeState property to its default state.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public void ResetIsThreeState( )
		{
			this.ClearValue( IsThreeStateProperty );
		}

		#endregion // IsThreeState

		#endregion // Public Properties

		#region Internal Methods

		#region ToggleIsChecked





		internal void ToggleIsChecked()
		{
			Debug.Assert(this.IsInEditMode, "Shouldn't the editor be in edit mode?");

			//  Note, the order is based upon the order documented for the 
			// Toggle method of the IToggleProvider and should not be changed.
			//
			bool? isChecked = this.IsChecked;
			bool isThreeState = this.IsThreeState;

			// if its indeterminate or unchecked and we're not three state then its on
			if (isChecked == null)		// indeterminate => on
				isChecked = true;
			else if (isChecked.Value)	// on => off
				isChecked = false;
			else if (isThreeState)		// must be off
				isChecked = null;
			else
				isChecked = true;

			this.IsChecked = isChecked;
		}
		#endregion //ToggleIsChecked

		#endregion //Internal Methods

		#region Private Methods/Properties

		#region SetIsCheckedHelper

		private void SetIsCheckedHelper( object value )
		{
			value = Utilities.ConvertDataValue( value, typeof( bool ), this.FormatProviderResolved, this.Format );
			if ( ! ( value is bool ) )
				this.IsChecked = null;
			else
				this.IsChecked = (bool)value;
		}

		#endregion // SetIsCheckedHelper

		#endregion // Private Methods/Properties
	}

	#endregion // XamCheckEditor Class

	#region ValueEditorCheckBox Class

	/// <summary>
	/// Checkbox class used by the <b>XamCheckEditor</b>.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// <b>ValueEditorCheckBox</b> is not meant to be used as a stand-alone control. It's used
	/// by the <b>XamCheckEditor</b>.
	/// </para>
	/// </remarks>
	//[Description( "Check box derived class meant to be used by the XamCheckEditor." )]
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class ValueEditorCheckBox : CheckBox
	{
		/// <summary>
		/// Used to prevent changes to the checkbox when the owning <see cref="ValueEditor"/> is not in edit mode.
		/// </summary>
		/// <param name="e">Event arguments</param>
		protected override void OnMouseLeftButtonDown( MouseButtonEventArgs e )
		{
			ValueEditor editor = (ValueEditor)Utilities.GetAncestorFromType( this, typeof( ValueEditor ), true );
			if ( null != editor && ! editor.IsInEditMode )
				return;			

			base.OnMouseLeftButtonDown( e );
		}
	}

	#endregion // ValueEditorCheckBox Class
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