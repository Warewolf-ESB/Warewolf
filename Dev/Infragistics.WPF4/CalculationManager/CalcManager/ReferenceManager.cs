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
	/// Class that manages a calc reference for a named reference or a control. It manages creation, re-creation
	/// and removal and sending out appropriate notifications to the calc manager. Both the NamedReference and 
	/// ControlReferenceManager utilize this class to manage their respective references.
	/// </summary>
	internal class ReferenceManager : PropertyChangeNotifierExtended, IFormulaProvider
	{
		#region Member Vars

		internal XamCalculationManager _calcManager;
		internal string _name;
		internal string _formula;
		internal string _elementName;
		internal FormulaRefBase _calcReference;
		private bool _calcReferenceDirty;
		private readonly Func<FormulaRefBase> _referenceCreator;
		internal readonly Action<CalculationValue> _initializeResult;
		internal CalculationValue _value;
		private Action<string> _setFormulaCallback;

		#endregion // Member Vars

		#region Constructor

		/// <summary>
		/// Constructor. Initializes a new instance of <see cref="ReferenceManager"/>.
		/// </summary>
		/// <param name="referenceCreator">Used to create calc reference. Calc references some times need to
		/// be discarded and new ones need to be created (for example when the name of the control changes)
		/// so this may get called more than once.</param>
		/// <param name="initializeResult">If there's a formula, this method will be called with the result
		/// of the formula calculation.</param>
		/// <param name="setFormulaCallback">Callback for setting formula via IFormulaProvider interface.</param>
		public ReferenceManager( Func<FormulaRefBase> referenceCreator, Action<CalculationValue> initializeResult, Action<string> setFormulaCallback )
		{
			_referenceCreator = referenceCreator;
			_initializeResult = initializeResult;
			_setFormulaCallback = setFormulaCallback;
		} 

		#endregion // Constructor

		#region Methods

		#region Internal Methods

		#region InitCalcManager

		/// <summary>
		/// Initializes calc manager.
		/// </summary>
		/// <param name="calcManager">Calc manager.</param>
		internal void InitCalcManager( XamCalculationManager calcManager )
		{
			if ( _calcManager != calcManager )
			{
				this.UnregisterFromCalcManager( );

				_calcManager = calcManager;

				// Raise CalculationManager property changed. IFormulaProvider also implements INotifyPropertyChanged.
				// 
				this.RaisePropertyChangedEvent( "CalculationManager" );

				this.RegisterWithCalcManager( );
			}
		}

		#endregion // InitCalcManager

		#region OnExternalValueChanged

		/// <summary>
		/// Called when the value of the named reference or the control changes. This method updates
		/// the calc reference with the new value. Note that this method doesn't need to be called
		/// when there's a formula since the formula calculation result will be the value of the reference.
		/// </summary>
		/// <param name="newValue">New value of the calc reference.</param>
		internal void OnExternalValueChanged( object newValue )
		{
			CalculationValue newCalcVal = Utils.ToCalcValue( newValue );

			if ( _value != newCalcVal )
			{
				_value = newCalcVal;

				if ( null != _calcReference && !_calcReference.HasFormula )
					_calcManager.InternalNotifyValueChanged( _calcReference );
			}
		}

		#endregion // OnExternalValueChanged

		#region OnFormulaChanged

		/// <summary>
		/// Called when the Formula is changed on the named reference or the control. It will re-compile and
		/// associate the new formula with the reference while deleting the old formula from the calc engine.
		/// </summary>
		/// <param name="formula">New formula. Can be null.</param>
		internal void OnFormulaChanged( string formula )
		{
			if ( _formula != formula )
			{
				_formula = formula;

				if ( this.IsCalcManagerInitializedHelper( ) )
				{
					if ( null != _calcReference && !_calcReferenceDirty )
					{
						// Re-register the formula. EnsureFormulaRegistered method will take the necessary steps to
						// remove the formula if the passed in formula string is null or empty. If the calc reference
						// hasn't been created yet then we don't need to take any action since when it does get 
						// created, it will register the formula properly.
						// 
						_calcReference.EnsureFormulaRegistered( _formula );
					}
				}

				// Raise Formula property changed. IFormulaProvider also implements INotifyPropertyChanged.
				// 
				this.RaisePropertyChangedEvent( "Formula" );
			}
		}

		#endregion // OnFormulaChanged

		#region OnNameChanged

		/// <summary>
		/// Called when the name of the underlying named reference or the control changes. This will
		/// cause the old reference to be removed and a new reference with the new name to be added.
		/// </summary>
		/// <param name="newName">New name of the control. This is the unescaped string.</param>
		internal void OnNameChanged( string newName )
		{
			if ( _name != newName )
			{
				_name = newName;

				_elementName = null != _name ? RefParser.EscapeString( _name, false ) : null;

				this.RegisterWithCalcManager( true );
			}
		}

		#endregion // OnNameChanged

		#region RegisterWithCalcManager

		/// <summary>
		/// Indicates whether to 
		/// </summary>
		/// <param name="reregister"></param>
		internal void RegisterWithCalcManager( bool reregister = false )
		{
			if ( this.IsCalcManagerInitializedHelper( ) )
			{
				if ( null == _calcReference || _calcReferenceDirty || reregister )
				{
					// Remove the old reference.
					// 
					this.UnregisterFromCalcManager( );

					if ( !string.IsNullOrEmpty( _elementName ) )
					{
						// Create a new reference and register it.
						// 
						_calcReference = _referenceCreator( );
						_calcManager.InternalAddReference( _calcReference );

						// EnsureFormulaRegistered checks for the string being null or empty.
						// 
						_calcReference.EnsureFormulaRegistered( _formula );

						// Raise Reference property changed. IFormulaProvider also implements INotifyPropertyChanged.
						// 
						this.RaisePropertyChangedEvent( "Reference" );
					}
				}
			}
		}

		#endregion // RegisterWithCalcManager

		#region UnregisterFromCalcManager

		/// <summary>
		/// Removes the calc reference from the calc manager along with its formula if any.
		/// </summary>
		internal void UnregisterFromCalcManager( )
		{
			if ( null != _calcReference )
			{
				var oldReference = _calcReference;
				_calcReference = null;

				// Remove the old reference.
				// 
				if ( null != oldReference && null != _calcManager )
					_calcManager.InternalRemoveReference( oldReference );

				this.RaisePropertyChangedEvent( "Reference" );
			}
		}

		#endregion // UnregisterFromCalcManager

		#endregion // Internal Methods

		#region Private Methods

		#region IsCalcManagerInitializedHelper

		/// <summary>
		/// Indicates whether the calc manager has been initialized. If it's not initialized yet then
		/// calc reference creation and calc manager notifications are bypassed. Furthermore, if 
		/// an existing calc reference exists and the calc manager hasn't been initialized then that
		/// calc reference will be dirtied and re-created when the calc manager is initialized.
		/// </summary>
		/// <returns>True if the calc manager is initialized.</returns>
		private bool IsCalcManagerInitializedHelper( )
		{
			if ( null != _calcManager )
			{
				if ( _calcManager.IsInitialized )
					return true;

				if ( null != _calcReference )
					_calcReferenceDirty = true;
			}

			return false;
		}

		#endregion // IsCalcManagerInitializedHelper

		#endregion // Private Methods 

		#endregion // Methods

		#region IFormulaProvider Implementation

		ICalculationReference IFormulaProvider.Reference
		{
			get 
			{ 
				return _calcReference; 
			}
		}

		ICalculationParticipant IFormulaProvider.Participant
		{
			get 
			{
				return null;
			}
		}

		ICalculationManager IFormulaProvider.CalculationManager
		{
			get 
			{
				return _calcManager;
			}
		}

		string IFormulaProvider.Formula
		{
			get
			{
				return _formula;
			}
			set
			{
				_setFormulaCallback( value );
			}
		} 

		#endregion // IFormulaProvider Implementation
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