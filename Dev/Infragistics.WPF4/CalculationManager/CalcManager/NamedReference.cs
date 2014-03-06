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
	/// Used to refer to a predefined or a calculated value in formulas participating in a <see cref="XamCalculationManager"/>.
	/// </summary>
	/// <remarks>
	/// <p class="body">A NamedReference is a reference in the calculation network with no associated control.</p>
	/// <p class="body">By creating a NamedReference with a <see cref="Formula"/>, you can create constants to be used in other calculations, or store the results of a calculation which may not need to be displayed on-screen, but can be used in code or in other calculations.</p>
	/// <p class="body">To get the calculated value of a NamedReference, use the <see cref="NamedReference.Value"/> property or the <see cref="NamedReference.Result"/> property which returns the same value in the form
	/// of a <see cref="CalculationValue"/> object which fully describes the calculation result, including error information if any.</p>
	/// </remarks>
	public class NamedReference : PropertyChangeNotifierExtended
	{
		#region Member Vars

		private string _referenceId;
		private string _formula;
		private string _category;
		private object _value;
		private CalculationValue _result;
		private NamedReferenceCollection _parentCollection;
		internal readonly ReferenceManager _referenceManager;

		#endregion // Member Vars

		#region Constructor

		/// <summary>
		/// Constructor. Initializes a new instance of <see cref="NamedReference"/>.
		/// </summary>
		public NamedReference( )
		{
			_referenceManager = new ReferenceManager( this.CreateNewReference, this.InitializeFormulaResult, ii => this.Formula = ii );
		} 

		#endregion // Constructor

		#region Properties

		#region Public Properties

		#region Category

		/// <summary>
		/// Specifies the category of the NamedReference which is used by the formula editor UI to categorize the named references.
		/// </summary>
		public string Category
		{
			get
			{
				return _category;
			}
			set
			{
				if ( _category != value )
				{
					_category = value;
					this.RaisePropertyChangedEvent( "Category" );
				}
			}
		}

		#endregion // Category

		#region Formula

		/// <summary>
		/// Specifies a formula for the named reference.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>Formula</b> property is used to specify a formula for the named reference. The result of the 
		/// formula calculation will be returned by the <see cref="Value"/> property.
		/// </para>
		/// <para class="body">
		/// <b>Note</b> that if you don't specify a formula, the <see cref="Value"/> property's value will 
		/// used in formulas of other objects that refer to this named reference.
		/// </para>
		/// </remarks>
		public string Formula
		{
			get
			{
				return _formula;
			}
			set
			{
				if ( _formula != value )
				{
					_formula = value;
					
					_referenceManager.OnFormulaChanged( _formula );

					this.RaisePropertyChangedEvent( "Formula" );
				}
			}
		}

		#endregion // Formula

		#region ReferenceId

		/// <summary>
		/// Specifies the id of the reference.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// You can refer to this reference in a formula using the relative form "[ReferenceId]" from formulas
		/// of root level objects or using the absolute name form "[//ReferenceId]" from any formula.
		/// </para>
		/// <para class="body">
		/// <b>Note:</b> The id's of all named references must be unique and non-null and non-empty strings.
		/// Any characters deemed special according to the reference naming specification will be allowed
		/// however they need to be escaped using '\' escape character when referenced within a formula. 
		/// Special characters are '/', '\', '(', ')', ':', '"', ',' and '='.
		/// </para>
		/// </remarks>
		public string ReferenceId
		{
			get
			{
				return _referenceId;
			}
			set
			{
				if ( _referenceId != value )
				{
					if ( null != _parentCollection )
						_parentCollection.OnChangingReferenceId( this, value );

					_referenceId = value;

					_referenceManager.OnNameChanged( _referenceId );

					this.RaisePropertyChangedEvent( "ReferenceId" );
				}
			}
		}

		#endregion // ReferenceId

		#region Result

		/// <summary>
		/// Returns the result of <see cref="Formula"/> calculation as a <see cref="CalculationValue"/> object.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// If <see cref="Formula"/> property is set, <b>Result</b> property returns the formula calculation result as
		/// a <see cref="CalculationValue"/> object. The underlying value of the result object 
		/// (<i>CalculationValue</i>.<see cref="CalculationValue.Value"/>) can also be accessed using <see cref="Value"/>
		/// property.
		/// </para>
		/// <para class="body">
		/// If <see cref="Formula"/> property is not set, this property returns null.
		/// </para>
		/// </remarks>
		public CalculationValue Result
		{
			get
			{
				return _result;
			}
			private set
			{
				if ( _result != value )
				{
					_result = value;
					this.RaisePropertyChangedEvent( "Result" );
				}
			}
		}

		#endregion // Result

		#region Value

		/// <summary>
		/// Specifies the value of the named reference. If <see cref="Formula"/> is set, this will return the result of the formula calculation.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>Value</b> property specifies the value of the named reference. When this reference is referred in formulas of other objects,
		/// this value will be used in those formula calculations. <b>Note</b> that this property cannot be specified if <see cref="Formula"/>
		/// property is specified. If <i>Formula</i> property is set, this property will return the result of the formula calculation.
		/// Furthermore attempting to set this property when <i>Formula</i> property is set will result in an exception.
		/// </para>
		/// <para class="body">
		/// <b>Note:</b> When <i>Formula</i> is set, it may get calculated asynchronously by the XamCalculationManager and therefore
		/// the result of the formula calculation may not be available right away.
		/// </para>
		/// </remarks>
		/// <seealso cref="Result"/>





		public object Value
		{
			get
			{
				return _value;
			}
			set
			{
				if ( _value != value )
				{
					if ( this.HasFormula )
						throw new InvalidOperationException( SRUtil.GetString( "LE_InvalidOperationException_1" ) );

					_value = value;

					_referenceManager.OnExternalValueChanged( _value );

					this.RaisePropertyChangedEvent( "Value" );
				}
			}
		}

		#endregion // Value

		#endregion // Public Properties

		#region Internal Properties

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

		#region HasFormula

		internal bool HasFormula
		{
			get
			{
				return ! string.IsNullOrEmpty( _formula );
			}
		}

		#endregion // HasFormula 

		#region ParentCollection

		internal NamedReferenceCollection ParentCollection
		{
			get
			{
				return _parentCollection;
			}
		} 

		#endregion // ParentCollection

		#endregion // Internal Properties

		#endregion // Properties

		#region Methods

		#region Private Methods

		#region CreateNewReference

		private FormulaRefBase CreateNewReference( )
		{
			return new NamedReferenceProxy( this );
		} 

		#endregion // CreateNewReference

		#endregion // Private Methods

		#region Internal Methods

		#region InitializeParentCollection

		internal void InitializeParentCollection( NamedReferenceCollection parentCollection )
		{
			if ( _parentCollection != parentCollection )
			{
				if ( null != _parentCollection )
					throw new InvalidOperationException( SRUtil.GetString( "LE_Exception_1" ) );

				_parentCollection = parentCollection;

				_referenceManager.InitCalcManager( parentCollection.CalcManager );
			}
		}

		#endregion // InitializeParentCollection

		#region InitializeFormulaResult

		/// <summary>
		/// Initializes the result of the formula calculation.
		/// </summary>
		/// <param name="val">Calculated value.</param>
		internal void InitializeFormulaResult( CalculationValue val )
		{
			this.Result = val;
		} 

		#endregion // InitializeFormulaResult

		#endregion // Internal Methods 

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