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

using Infragistics.Calculations;
using Infragistics.Calculations.Engine;
using System;

namespace Infragistics.Calculations
{

	/// <summary>
	/// Defines a calculation to be performed on a item or list.
	/// </summary>
	/// <seealso cref="ListCalculation"/>
	/// <seealso cref="ListCalculator"/>
	/// <seealso cref="ListCalculator.ItemCalculations"/>
	/// <seealso cref="ItemCalculation"/>
	/// <seealso cref="ItemCalculator"/>
	/// <seealso cref="ItemCalculator.Calculations"/>
	public abstract class ItemCalculationBase : PropertyChangeNotifierExtended, IFormulaProvider
	{
		#region Private Members

		private string _referenceId;
		private string _formula;
		private ItemCalculatorBase _calculator;

		#endregion //Private Members

		#region Properties

		#region Public Properties

		#region Formula

		/// <summary>
		/// Gets/sets a string that specifies the formula that will be used to calculate a value for the item.
		/// </summary>
		public string Formula
		{
			get { return _formula; }
			set
			{
				if (value != _formula)
				{
					_formula = value;
					this.RaisePropertyChangedEvent("Formula");
				}
			}
		}

		#endregion //Formula

		#region ReferenceId

		/// <summary>
		/// At least ReferenceId or TargetProperty must be specified. If ReferenceId is not specified and TargetProperty is specified,
		/// the ReferenceId will be the TargetProperty.
		/// </summary>
		public string ReferenceId
		{
			get { return _referenceId; }
			set
			{
				if (value != _referenceId)
				{
					_referenceId = value;

					this.RaisePropertyChangedEvent("ReferenceId");
				}
			}
		}

		#endregion //ReferenceId

		#endregion //Public Properties

		#region Internal Properties

		#region ReferenceIdResolved

		internal virtual string ReferenceIdResolved
		{
			get
			{
				return _referenceId;
			}
		}

		#endregion //ReferenceIdResolved

		#endregion //Internal Properties

		#endregion //Properties

		#region Methods

		#region Internal Methods

		#region Initialize

		internal void Initialize(ItemCalculatorBase calculator)
		{
			if (calculator == _calculator)
				return;

			if ( _calculator != null && calculator != null )
				throw new InvalidOperationException(SRUtil.GetString("CalculationUsedMoreThanOnce", calculator.GetType()));

			_calculator = calculator;

			this.InvalidateReference();
		}

		#endregion //Initialize

		#region InvalidateReference

		internal void InvalidateReference()
		{
			this.RaisePropertyChangedEvent(string.Empty);
		}

		#endregion //InvalidateReference

		#endregion //Internal Methods

		#endregion //Methods

		#region IFormulaProvider Members

		ICalculationReference IFormulaProvider.Reference
		{
			get 
			{
				XamCalculationManager mgr = ((IFormulaProvider)this).CalculationManager as XamCalculationManager;

				if (mgr != null && ((IFormulaProvider)this).Participant != null)
					return _calculator.GetReference(this.ReferenceIdResolved);

				return null;
			}
		}

		ICalculationParticipant IFormulaProvider.Participant
		{
			get { return _calculator != null ? _calculator.Participant : null; }
		}

		ICalculationManager IFormulaProvider.CalculationManager
		{
			get { return _calculator != null ? _calculator.CalculationManager : null; }
		}

		#endregion
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