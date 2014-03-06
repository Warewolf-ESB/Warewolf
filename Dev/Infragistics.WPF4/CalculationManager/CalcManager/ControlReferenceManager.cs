using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using System.Windows;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Collections.Specialized;
using System.Collections;
using System.Windows.Media;
using System.Windows.Markup;
using System.Windows.Controls;
using System.Windows.Automation.Peers;
using Infragistics.AutomationPeers;
using System.Windows.Threading;
using System.Threading;
using Infragistics.Collections;
using Infragistics.Controls;
using Infragistics.Controls.Primitives;
using System.Windows.Data;
using System.Linq;



using Infragistics.Windows.Licensing;


using Infragistics.Calculations;
using Infragistics.Calculations.Engine;

namespace Infragistics.Calculations
{
	/// <summary>
	/// A class used to maintain a calc reference for a control and manage retrieving and updating
	/// of control's value.
	/// </summary>
	internal class ControlReferenceManager : PropertyChangeNotifierExtended
	{
		#region Member Vars

		private WeakReference _wControl;
		private ControlCalculationSettings _settings;
		internal readonly ReferenceManager _referenceManager;
		private ValueGetter _valueGetter;
		private bool _settingControlValue;
		private string _fallbackName;

		#endregion // Member Vars

		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="control">Associated control or dependency object.</param>
		internal ControlReferenceManager( DependencyObject control )
		{
			CoreUtilities.ValidateNotNull( control );

			_wControl = new WeakReference( control );
			_referenceManager = new ReferenceManager( this.CreateNewReference, this.InitializeFormulaResult, this.SetFormulaCallback );
			this.Settings = XamCalculationManager.GetControlSettings( control );
			this.InitializeReferenceManager( );
		} 

		#endregion // Constructor

		#region Base Overrides

		#region OnSubObjectPropertyChanged

		/// <summary>
		/// This is the listener for calc settings and value getter and itself.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="property"></param>
		/// <param name="extraInfo"></param>
		internal override void OnSubObjectPropertyChanged( object sender, string property, object extraInfo )
		{
			base.OnSubObjectPropertyChanged( sender, property, extraInfo );

			if ( this == sender )
			{
				switch ( property )
				{
					case "Settings":
						this.DirtyValueGetter( );
						this.InitializeReferenceManager( );
						break;
				}
			}
			else if ( _settings == sender )
			{
				switch ( property )
				{
					case "ReferenceId":
						// Recalculate the name.
						// 
						this.VerifyName( );
						break;
					case "TreatAsType":
					case "TreatAsTypeName":
						break;
					case "TreatAsTypeResolved":
					case "ValueConverter":
					case "Property":
					case "Binding":
						this.DirtyValueGetter( );
						break;
					case "Formula":
						// When Formula is set, auto-generate a name if the reference id was not specified.
						// 
						if ( string.IsNullOrEmpty( _referenceManager._name ) )
							this.VerifyName( );

						this.SyncFormula( );
						break;
					default:
						Debug.Assert( false, "Unknown property." );
						break;
				}
			}
			else if ( _valueGetter == sender )
			{
				if ( !_settingControlValue )
					this.SyncValueWithControl( );
			}
		}

		#endregion // OnSubObjectPropertyChanged

		#endregion // Base Overrides

		#region Properties

		#region Internal Properties

		#region Control

		/// <summary>
		/// Gets the associated control. Will return null if the control gets garbage collected.
		/// </summary>
		internal DependencyObject Control
		{
			get
			{
				DependencyObject control = null;

				if ( null != _wControl )
				{
					control = (DependencyObject)CoreUtilities.GetWeakReferenceTargetSafe( _wControl );

					if ( null == control )
						_wControl = null;
				}

				return control;
			}
		}

		#endregion // Control

		#region CalcReference

		/// <summary>
		/// Returns the associated calc-reference. If it hasn't been created yet then returns null.
		/// </summary>
		internal ICalculationReference CalcReference
		{
			get
			{
				return _referenceManager._calcReference;
			}
		}

		#endregion // CalcReference

		#region Settings

		/// <summary>
		/// Gets the associated settings or null if none were specified.
		/// </summary>
		internal ControlCalculationSettings Settings
		{
			get
			{
				return _settings;
			}
			set
			{
				if ( _settings != value )
				{
					PropertyChangeListenerList.ManageListenerHelper( ref _settings, value, this, false );

					this.RaisePropertyChangedEvent( "Settings" );
				}
			}
		}

		#endregion // Settings  

		#region ShouldIncludeInReferenceTree

		internal bool ShouldIncludeInReferenceTree
		{
			get
			{
				// If a name was auto-generated then don't display in the formula editor. Since the 
				// auto-generated name is not persisted, if it's used in a formula, the formula
				// cannot be persisted as well.
				// 
				return _fallbackName != _referenceManager._name;
			}
		} 

		#endregion // ShouldIncludeInReferenceTree

		// MD 8/25/11 - TFS84804
		#region ValueGetter

		internal ValueGetter ValueGetter
		{
			get { return _valueGetter; }
		}

		#endregion  // ValueGetter

		#endregion // Internal Properties

		#endregion // Properties

		#region Methods

		#region Private Methods

		#region ConvertCalcValueToControlValue

		/// <summary>
		/// Converts the calculation value to the object that will be set on the control.
		/// </summary>
		/// <param name="calcVal">Calculation value.</param>
		/// <returns>Value that is to be set on the control.</returns>
		private object ConvertCalcValueToControlValue( CalculationValue calcVal )
		{
			CultureInfo controlCulture = this.GetControlCulture( );

			if ( null != _settings )
			{
				var converter = _settings.ValueConverter;
				if ( null != converter )
					return converter.ConvertBack( calcVal.Value, typeof( object ), this.Control, controlCulture );
			}

			object resolvedVal = calcVal.GetResolvedValue( );
			object convertedVal = resolvedVal;

			if ( null != _valueGetter )
			{
				if ( null != _valueGetter._targetPropertyTypeIfKnown )
				{
					// SSP 10/12/11 TFS88251 TFS91512
					// Since type converters do not exist in SL, in order to get consistent behavior, pass false for the
					// useTypeConverters parameter.
					// 
					//object tmp = CoreUtilities.ConvertDataValue( resolvedVal, _valueGetter._targetPropertyTypeIfKnown, controlCulture, null );
					object tmp = CoreUtilities.ConvertDataValue( resolvedVal, _valueGetter._targetPropertyTypeIfKnown, controlCulture, null, false );

					if ( null != tmp || CoreUtilities.IsValueEmpty( resolvedVal ) )
						convertedVal = tmp;
				}
			}

			return convertedVal;
		}

		#endregion // ConvertCalcValueToControlValue

		#region ConvertControlValueToCalcValue

		/// <summary>
		/// Converts the control's value to a CalculationValue instance that will be used to pass into
		/// the calc functions.
		/// </summary>
		/// <param name="val">Control's value.</param>
		/// <returns>CalculationValue instance.</returns>
		private CalculationValue ConvertControlValueToCalcValue( object val )
		{
			CultureInfo controlCulture = this.GetControlCulture( );

			IValueConverter converter = null;
			Type treatAsType = null;
			if ( null != _settings )
			{
				converter = _settings.ValueConverter;
				treatAsType = _settings.TreatAsTypeResolved;
			}

			
			return Utils.ConvertValueHelper(val, controlCulture, converter, this.Control, treatAsType ?? typeof(object), treatAsType);
		}

		#endregion // ConvertControlValueToCalcValue

		#region CreateNewReference

		private FormulaRefBase CreateNewReference( )
		{
			return new ControlReferenceProxy( this );
		}

		#endregion // CreateNewReference

		#region DirtyValueGetter

		/// <summary>
		/// Called whenever any setting changes that requires us to re-get the value from the control. For example, the binding
		/// or the TreatAsType.
		/// </summary>
		/// <param name="resyncValue">Whether to re-get the value and update the calc reference with it.</param>
		private void DirtyValueGetter( bool resyncValue = true )
		{
			_valueGetter = null;

			if ( resyncValue )
				this.SyncValueWithControl( );
		}

		#endregion // DirtyValueGetter

		#region GetControlCulture

		/// <summary>
		/// Gets the control's language culture. If the control is not a 
		/// </summary>
		/// <returns></returns>
		private CultureInfo GetControlCulture( )
		{
			// We decided to not use the Language property and instead use the CurrentCulture always. That's
			// what we seem to be doing in other silverlight controls and the fact that the Language property
			// always defaults to en-US even if the OS culture is different.
			// 
			return CultureInfo.CurrentCulture;

			
#region Infragistics Source Cleanup (Region)



















#endregion // Infragistics Source Cleanup (Region)

		}

		#endregion // GetControlCulture

		#region InitializeFormulaResult

		/// <summary>
		/// Called when the formula is calculated to update the underlying control with the calculated value.
		/// </summary>
		/// <param name="result"></param>
		private void InitializeFormulaResult( CalculationValue result )
		{
			// Update the XamCalculationManager.ControlResult for the control with the new value.
			// 
			this.UpdateControlResult( result );

			this.VerifyValueGetter( );
			if ( null != _valueGetter )
			{
				bool origVal = _settingControlValue;
				_settingControlValue = true;
				try
				{
					_valueGetter.Value = this.ConvertCalcValueToControlValue( result );
				}
				finally
				{
					_settingControlValue = origVal;
				}
			}
		}

		#endregion // InitializeFormulaResult

		#region InitReferenceManager

		private void InitializeReferenceManager( )
		{
			this.VerifyName( );
			this.SyncValueWithControl( );
			this.SyncFormula( );
		}

		#endregion // InitReferenceManager

		#region SetFormulaCallback

		private void SetFormulaCallback( string formula )
		{
			var control = this.Control;
			Debug.Assert( null != control );
			if ( null != control )
			{
				// Get the existing settings if any.
				// 
				var settings = _settings;
				Debug.Assert( settings == XamCalculationManager.GetControlSettings( control ) );

				// If we have existing settings object then simply set the formula on it.
				// 
				if ( null != settings )
				{
					settings.Formula = formula;
				}
				else
				{
					// Otherwise allocate a new settings object and set it on the control.
					// 
					settings = new ControlCalculationSettings( ) { Formula = formula };
					XamCalculationManager.SetControlSettings( control, settings );
					Debug.Assert( _settings == settings );
				}
			}
		} 

		#endregion // SetFormulaCallback

		#region SyncFormula

		/// <summary>
		/// Called when the Formula on the calc settings is changed. It updates the calc reference's formula.
		/// </summary>
		private void SyncFormula( )
		{
			string formula = null != _settings ? _settings.Formula : null;
			_referenceManager.OnFormulaChanged( formula );
		}

		#endregion // SyncFormula

		#region SyncValueWithControl

		/// <summary>
		/// Called when a control's value changes. It retrieves the value from the control and updates the calc reference
		/// with the new value so any formulas referencing this control will be recalculated.
		/// </summary>
		private void SyncValueWithControl( )
		{
			this.VerifyValueGetter( );

			if ( null != _valueGetter )
				_referenceManager.OnExternalValueChanged( this.ConvertControlValueToCalcValue( _valueGetter.Value ) );
			else
				_referenceManager.OnExternalValueChanged( Utils.ToCalcValue( CalculationErrorCode.Value ) );
		}

		#endregion // SyncValueWithControl

		#region UpdateControlResult

		/// <summary>
		/// Updates the XamCalculationManager.ControlResult attached property.
		/// </summary>
		/// <param name="result">New formula calculation result.</param>
		private void UpdateControlResult( CalculationValue result )
		{
			// Update the XamCalculationManager.ControlResult for the control with the new value.
			// 
			var calcManager = _referenceManager._calcManager;
			var control = this.Control;
			if ( null != calcManager && null != control )
			{
				var controlResult = XamCalculationManager.GetResult( control );
				if ( null == controlResult )
				{
					controlResult = new CalculationResult( ) { CalculationValue = result };
					XamCalculationManager.SetResult( control, controlResult );
				}
				else
				{
					controlResult.CalculationValue = result;
				}
			}
		}

		#endregion // UpdateControlResult

		#region VerifyName

		/// <summary>
		/// Re-verifies the Name based on ReferenceId and control's Name settings.
		/// </summary>
		private void VerifyName( )
		{
			string name = null != _settings ? _settings.ReferenceId : null;

			if ( string.IsNullOrEmpty( name ) )
			{
				FrameworkElement control = this.Control as FrameworkElement;
				name = null != control ? control.Name : null;

				// If this control is target of a formula, don't require name. In other words, generate
				// a new name.
				// 
				if ( string.IsNullOrEmpty( name ) && ! string.IsNullOrEmpty( null != _settings ? _settings.Formula : null ) )
				{
					if ( null == _fallbackName )
						_fallbackName = Guid.NewGuid( ).ToString( );

					name = _fallbackName;
				}
			}

			_referenceManager.OnNameChanged( name );
		}

		#endregion // VerifyName

		#region VerifyValueGetter

		/// <summary>
		/// Creates ValueGetter if not already created.
		/// </summary>
		private void VerifyValueGetter( )
		{
			if ( null == _valueGetter )
			{
				DependencyObject control = this.Control;

				string path = null != _settings ? _settings.Property : null;
				Binding binding = null != _settings ? _settings.Binding : null;

				if ( string.IsNullOrEmpty( path ) && null == binding )
					path = ValueGetter.GetDefaultBindingPath( control );

				ValueGetter newValueGetter = null;
				if ( null != binding )
					newValueGetter = new ValueGetter( control, binding );
				else if ( !string.IsNullOrEmpty( path ) )
					newValueGetter = new ValueGetter( control, path );

				PropertyChangeListenerList.ManageListenerHelper( ref _valueGetter, newValueGetter, this, true );
			}
		}

		#endregion // VerifyValueGetter

		#endregion // Private Methods 

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