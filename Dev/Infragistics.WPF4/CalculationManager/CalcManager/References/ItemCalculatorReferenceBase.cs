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
using Infragistics.Calculations;
using Infragistics.Calculations.Engine;

namespace Infragistics.Calculations
{
	internal class ItemCalculatorReferenceBase : FormulaRefBase
	{
		#region Private Members

		private ItemCalculatorBase _calculator;
		private string _elementName;

		#endregion //Private Members	
    
		#region Constructor

		internal ItemCalculatorReferenceBase(ItemCalculatorBase calculator)
			: base(calculator.CalculationManager)
		{
			_calculator = calculator;
		}

		#endregion //Constructor	
 
		#region Properties

		#region Calculator

		internal ItemCalculatorBase Calculator { get { return _calculator; } }

		#endregion //Calculator	
    
		#endregion //Properties	
    
		#region Base class overrides

		#region Properties

		#region ElementName

		public override string ElementName
		{
			get
			{
				if (_elementName == null)
					_elementName = RefParser.EscapeString(this._calculator.ReferenceIdResolved, false);

				return _elementName;
			}
		}

		#endregion //ElementName	
    
		#endregion // Properties

		#region Methods

		#region ContainsReference

		public override bool ContainsReference(ICalculationReference inReference)
		{
			// for the calculator root IsSubsetReference is the same as Contains since
			// there are no logical references above the calculator
			return this.IsSubsetReference(inReference);
		}

		#endregion //ContainsReference	
    
		#region FindRoot
    
    	protected override RefBase FindRoot()
		{
 			 return this;
		}

		#endregion //FindRoot	
    
		#region GetChildReferences

		public override ICalculationReference[] GetChildReferences(ChildReferenceType referenceType)
		{
			return _calculator.GetChildReferences(referenceType);
		}

		#endregion //GetChildReferences	
    
		#region IsSubsetReference

		public override bool IsSubsetReference(ICalculationReference inReference)
		{
			ICalculationReference testRef = inReference;

			// walk up the parent chain looking for this calculator ref
			while (testRef != null )
			{
				if ( testRef == this )
					return true;

				testRef = testRef.Parent;
			}

			return false;
		}

		#endregion //IsSubsetReference	

		#endregion // Methods

		#endregion // Base class overrides
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