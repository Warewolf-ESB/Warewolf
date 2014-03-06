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
using Infragistics.Calculations.CalcManager;

namespace Infragistics.Calculations
{
	/// <summary>
	/// Base class for calc reference implementations for named references and controls.
	/// </summary>
	internal abstract class FormulaRefBase : RefBase
	{
		#region Private Variables

		protected readonly XamCalculationManager _calcManager;
        protected readonly IDev2CalculationManager  _dev2CalcManager;
		private ICalculationFormula _formula = null;
		private bool _isDisposed;
		private CalculationValue _formulaSyntaxErrorValue;

		#endregion // Private Variables

		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		internal FormulaRefBase( XamCalculationManager calcManager )
			: base( )
		{
			CoreUtilities.ValidateNotNull( calcManager );

			_calcManager = calcManager;
		}

        internal FormulaRefBase(Dev2CalculationManager calcManager) 
            : base( )
        {
            CoreUtilities.ValidateNotNull(calcManager);

            _dev2CalcManager = calcManager;
        }

		#endregion // Constructor

		#region Properties

		#region Internal Properties

		#region CalculationManager

		internal XamCalculationManager CalculationManager { get { return _calcManager; } }

        internal IDev2CalculationManager Dev2CalculationManager { get { return _dev2CalcManager; } }

		#endregion //CalculationManager	
    
		#region HasFormula







		internal virtual bool HasFormula
		{
			get
			{
				return null != _formula;
			}
		}

		#endregion // HasFormula

		#region HasFormulaSyntaxError






		internal virtual bool HasFormulaSyntaxError
		{
			get
			{
				return null != _formula && _formula.HasSyntaxError;
			}
		}

		#endregion // HasFormulaSyntaxError

		#region FormulaSyntaxError






		internal virtual string FormulaSyntaxError
		{
			get
			{
				return this.HasFormulaSyntaxError ? _formula.SyntaxError : null;
			}
		}

		#endregion // FormulaSyntaxError  
		
		#region FormulaSyntaxErrorValue

		internal virtual CalculationValue FormulaSyntaxErrorValue { get { return _formulaSyntaxErrorValue; } }

		#endregion //FormulaSyntaxErrorValue	
    
		#endregion // Internal Properties

		#endregion // Properties

		#region Methods

		#region Internal Methods

		#region Dispose

		internal void Dispose(bool notifyCalcManager)
		{
			if (!_isDisposed)
			{
				_isDisposed = true;

				this.OnDisposed(notifyCalcManager);
			}
		}

		#endregion //Dispose	

		#region EnsureFormulaRegistered

		/// <summary>
		/// Makes sure that this reference is using the specified formula string as
		/// its formula. If not it will delete the old formula and compile and add
		/// the new formula.
		/// </summary>
		/// <param name="formulaString"></param>
		internal void EnsureFormulaRegistered( string formulaString )
		{
			if ( null == _formula || formulaString != _formula.FormulaString )
				this.RegisterFormulaHelper( formulaString );
		}

		#endregion // EnsureFormulaRegistered 

		#endregion // Internal Methods

		#region Protected Methods

		#region OnDisposed

		protected virtual void OnDisposed(bool notifyCalcManager)
		{
			if (notifyCalcManager)
				this.CalculationManager.InternalRemoveReference(this);
		}

		#endregion //OnDisposed

		#endregion //Protected Methods	
        
		#region Private Methods

		#region RegisterFormulaHelper

		/// <summary>
		/// Removes the old formula from the calc network and adds the new formula to the calc network.
		/// </summary>
		/// <param name="formulaString">Formula string.</param>
		private void RegisterFormulaHelper( string formulaString )
		{
			//Clear the cached syntax error value
			_formulaSyntaxErrorValue = null;

			// Remove the old formula first.
			//
			if ( null != _formula )
			{
				// SSP 2/4/05
				// Use the AddFormulaHelper and RemoveFormulaHelper utility methods 
				// instead. See comments above those method definitions for more info.
				//
				//calcManager.RemoveFormula( this.formula );
				// MD 1/28/09 - TFS13109
				// We need to clear the formula before calling RemoveFormulaHelper. If we don't, there is a weird timing issue with synchronous 
				// calculations where removing the formula tries to recalculate it and it causes a null reference exception.
				//RefUtils.RemoveFormulaHelper( calcManager, this.formula );
				ICalculationFormula tempFormula = _formula;
				_formula = null;
				Utils.RemoveFormulaHelper( _calcManager, tempFormula );
			}

			if ( !string.IsNullOrEmpty( formulaString ) )
			{
				_formula = _calcManager.CompileFormula( this, formulaString, false );

				// SSP 2/4/05
				// Use the AddFormulaHelper and RemoveFormulaHelper utility methods 
				// instead. See comments above those method definitions for more info.
				//
				//calcManager.AddFormula( formula );
				Utils.AddFormulaHelper( _calcManager, _formula );

				// if there is a syntax error then cache a error value that can be returned
				// from the value property of derived classes
				if (this.HasFormulaSyntaxError)
				{
					this._formulaSyntaxErrorValue = Utils.ToCalcValue(new CalculationErrorValue(CalculationErrorCode.NA, SRUtil.GetString("FormulaSyntaxError", formulaString, _formula.SyntaxError)));
				}
			}
		}

		#endregion // RegisterFormulaHelper  

		#endregion // Private Methods

		#endregion // Methods

		#region RefBase Overrides

		#region Properties

		#region AbsoluteName

		public override string AbsoluteName
		{
			get
			{
				return RefParser.RefFullyQualifiedString + this.ElementName;
			}
		}

		#endregion AbsoluteName
     
		#region Formula

		public override ICalculationFormula Formula
		{
			get
			{
				return _formula;
			}
		}

		#endregion // Formula 
 
		#region IsDisposedReference

		public override bool IsDisposedReference
		{
			get
			{
				return _isDisposed;
			}
		}

		#endregion //IsDisposedReference	

		#region Parent

		public override RefBase BaseParent
		{
			get
			{
				return null;
			}
		}

		#endregion Parent

		#endregion // Properties

		#region Methods

		#region BuildReference

		protected override ICalculationReference BuildReference( RefParser buildRP, bool forceDataRef )
		{
			// Since the references names are now case insensitive, do a case 
			// insensitive comparison.
			//
			//if ( buildRP[0].Name == this.ElementName )
			if ( RefParser.AreStringsEqual( buildRP[0].Name, this.ElementName, true ) )
				return this;

			return base.BuildReference( buildRP, forceDataRef );
		}

		#endregion BuildReference

		#region CreateReference

		public override ICalculationReference CreateReference( string inReference )
		{
			RefParser refParser = RefParser.Parse( inReference );

			if ( RefParser.AreStringsEqual( this.ElementName, refParser.RootName, true ) )
				return this;

			// Since NamedReferences and CalcSettings are always root-level
			// we can just pass this on to the CalcManager and let him 
			// figure it out.
			// 
			ICalculationReference reference = _calcManager.GetReference( inReference );

			if ( null == reference )
				return new CalculationReferenceError( inReference, "Could not find Reference" );

			return reference;
		}

		#endregion CreateReference

		#region FindAll

		/// <summary>
		/// Returns the named reference relative to this reference with scope "All".
		/// </summary>
		/// <param name="name">The element name.</param>
		/// <returns>The referenced element</returns>
		/// <see cref="BuildReference"/>
		public override ICalculationReference FindAll( string name )
		{
			return new CalculationReferenceError(name, SRUtil.GetString("FindReferenceError"));
		}

		#endregion FindAll

		#region FindItem

		public override ICalculationReference FindItem( string name, int index, bool isRelative )
		{
			return new CalculationReferenceError(name, SRUtil.GetString("FindReferenceError"));
		}


		public override ICalculationReference FindItem( string name, string index )
		{
			return new CalculationReferenceError(name, SRUtil.GetString("FindReferenceError"));
		}

		public override ICalculationReference FindItem( string name )
		{
			return new CalculationReferenceError(name, SRUtil.GetString("FindReferenceError"));
		}

		#endregion FindItem

		#region FindSummaryItem

		public override ICalculationReference FindSummaryItem( string name )
		{
			return new CalculationReferenceError(name, SRUtil.GetString("FindReferenceError"));
		}

		#endregion FindSummaryItem

		#region ResolveReference

		public override ICalculationReference ResolveReference( ICalculationReference reference, ResolveReferenceType referenceType )
		{
			return reference;
		}

		#endregion ResolveReference 

		#endregion // Methods

		#endregion // RefBase Overrides
	}


	internal class FormulaCalculationReference : FormulaRefBase
	{
		#region Member Vars

		private ICalculationFormula _formula;

		#endregion // Member Vars

		#region Constructor

		internal FormulaCalculationReference( XamCalculationManager calcManager, string formula )
			: base( calcManager )
		{
			_formula = calcManager.CompileFormula( this, formula, true );
		}

        internal FormulaCalculationReference( Dev2CalculationManager calcManager, string formula)
            : base(calcManager) {
            _formula = calcManager.CompileFormula(this, formula, true);
        } 

		#endregion // Constructor

		#region Base Overrides

		#region Formula

		public override ICalculationFormula Formula
		{
			get
			{
				return _formula;
			}
		} 

		#endregion // Formula

		#region ElementName

		public override string ElementName
		{
			get
			{
				return this.GetType( ).Name;
			}
		}

		#endregion ElementName

		#region Value

		public override CalculationValue Value
		{
			get
			{
				return new CalculationValue( );
			}
			set
			{
				Debug.Assert( false );
			}
		}

		#endregion Value

		#endregion // Base Overrides
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