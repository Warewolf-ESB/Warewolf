using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infragistics.Calculations.Engine;
using System.Reflection;
using Infragistics.Windows.Internal;

namespace Infragistics.Calculations
{
	internal class ListCalculationReference : FormulaRefBase
	{
		#region Private Members

		private ItemCalculatorReferenceBase _root;
		private ListCalculation _calculation;
		private string _elementName;
		private CalculationValue _value;

		#endregion //Private Members

		#region Constructor

		internal ListCalculationReference(ItemCalculatorReferenceBase root, ListCalculation calculation)
			: base(root.Calculator.CalculationManager)
		{
			CoreUtilities.ValidateNotNull(calculation, "calculation");
			_root = root;
			_calculation = calculation;
		}

		#endregion //Constructor

		#region Properties

		#region Calculation

		internal ListCalculation Calculation { get { return _calculation; } }

		#endregion //Calculation

		#endregion //Properties

		#region Base class overrides

		#region Properties

		#region AbsoluteName

		public override string AbsoluteName
		{
			get
			{
				return _root.AbsoluteName + RefParser.RefSeperatorString + this.ElementName;
			}
		}

		#endregion AbsoluteName

		#region BaseParent

		public override Engine.RefBase BaseParent
		{
			get
			{
				return _root;
			}
		}

		#endregion //BaseParent

		#region ElementName

		public override string ElementName
		{
			get
			{
				if (_elementName == null)
					_elementName = RefParser.EscapeString(_calculation.ReferenceIdResolved, false);

				return _elementName;
			}
		}

		#endregion //ElementName

		#region IsDataReference

		public override bool IsDataReference
		{
			get
			{
				return true;
			}
		}

		#endregion //IsDataReference	

		#region Value

		public override CalculationValue Value
		{
			get
			{
				// If there is a syntax error then return that
				if (this.FormulaSyntaxErrorValue != null)
					return this.FormulaSyntaxErrorValue;

				if (_value != null)
					return _value;

				return base.Value;
			}
			set
			{

				if (_value != value)
				{
					FormulaCalculationErrorEventArgs errorArgs = null;

					if (value != null)
					{
						CalculationErrorValue errorValue = value.Value as CalculationErrorValue;
						if (errorValue != null)
							errorArgs = _calcManager.RaiseFormulaError(this, errorValue, null);
					}

					_value = value;

					// update the calculator with the result
					this.SetResultOnCalculator(_value);

				}
			}
		}

		#endregion //Value	

		#endregion // Properties

		#region Methods

		#region CreateReference

		public override ICalculationReference CreateReference(string inReference)
		{
			ListCalculatorColumnReference columnRef = _root.Calculator.GetReference(inReference, true, true) as ListCalculatorColumnReference;

			if (columnRef != null)
				return columnRef;

			return base.CreateReference(inReference);
		}

		#endregion //CreateReference	

		#endregion // Methods

		#endregion // Base class overrides

		#region Methods

		#region Internal Methods

		#region SetResultOnCalculator

		internal void SetResultOnCalculator(CalculationValue value)
		{
			if (this.Formula != null)
			{
				_root.Calculator.SetResult(value, _calculation.ReferenceIdResolved, this);
			}
		}

		#endregion //SetResultOnCalculator	

		#endregion //Internal Methods

		#endregion //Methods
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