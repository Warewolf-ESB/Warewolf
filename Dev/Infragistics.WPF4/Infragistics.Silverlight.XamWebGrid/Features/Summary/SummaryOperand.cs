using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Infragistics.Controls.Grids
{
	#region MaximumSummaryOperand
	/// <summary>
	/// A <see cref="SummaryOperandBase"/> implementation which executes a LINQ Maximum summary.
	/// </summary>
	public class MaximumSummaryOperand : SummaryOperandBase
	{
		#region Members

		MaximumSummaryCalculator _maxSumCalc;

		#endregion // Members

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="MaximumSummaryOperand"/> class.
		/// </summary>
		public MaximumSummaryOperand()
		{
			this._maxSumCalc = new MaximumSummaryCalculator() { };
		}

		#endregion // Constructor

		#region SummaryCalculator
		/// <summary>
		/// Gets the <see cref="SummaryCalculator"/> which will be used to calculate the summary.
		/// </summary>
		public override SummaryCalculatorBase SummaryCalculator
		{
			get
			{
				return this._maxSumCalc;
			}
		}
		#endregion // SummaryCalculator

		#region DefaultSelectionDisplayLabel
		/// <summary>
		/// Get's the default text that will be displayed in the drop down for this <see cref="SummaryOperandBase"/>
		/// </summary>
		protected override string DefaultSelectionDisplayLabel 
		{
			get
			{
				return SRGrid.GetString("MaxSummarySelectionLabel");
			}
		}
		#endregion // DefaultSelectionDisplayLabel

		#region DefaultRowDisplayLabel
		/// <summary>
		/// Get's the default text that will be displayed in the <see cref="Infragistics.Controls.Grids.Primitives.SummaryRow"/> when this <see cref="SummaryOperandBase"/> is selected.
		/// </summary>
		protected override string DefaultRowDisplayLabel 
		{
			get
			{
				return SRGrid.GetString("MaxSummaryRowLabel");
			}
		}

		#endregion // DefaultRowDisplayLabel

		#region LinqSummaryOperator
		/// <summary>
		/// Gets the operator that will be associated with this operand.
		/// </summary>						
		public override LinqSummaryOperator? LinqSummaryOperator
		{
			get
			{
				return Infragistics.LinqSummaryOperator.Maximum;
			}
		}
		#endregion // LinqSummaryOperator

	}
	#endregion // MaximumSummaryOperand

	#region MinimumSummaryOperand
	/// <summary>
	/// A <see cref="SummaryOperandBase"/> implementation which executes a LINQ Minimum summary.
	/// </summary>
	public class MinimumSummaryOperand : SummaryOperandBase
	{
		#region Members

		MinimumSummaryCalculator _minSumCalc;

		#endregion // Members

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="MinimumSummaryOperand"/> class.
		/// </summary>
		public MinimumSummaryOperand()
		{
			this._minSumCalc = new MinimumSummaryCalculator();
		}

		#endregion // Constructor

		#region SummaryCalculator
		/// <summary>
		/// Gets the <see cref="SummaryCalculator"/> which will be used to calculate the summary.
		/// </summary>
		public override SummaryCalculatorBase SummaryCalculator
		{
			get
			{
				return this._minSumCalc;
			}
		}
		#endregion // SummaryCalculator

		#region DefaultSelectionDisplayLabel
		/// <summary>
		/// Get's the default text that will be displayed in the drop down for this <see cref="SummaryOperandBase"/>
		/// </summary>
		protected override string DefaultSelectionDisplayLabel
		{
			get
			{
				return SRGrid.GetString("MinSummarySelectionLabel");
			}
		}
		#endregion // DefaultSelectionDisplayLabel

		#region DefaultRowDisplayLabel
		/// <summary>
		/// Get's the default text that will be displayed in the <see cref="Infragistics.Controls.Grids.Primitives.SummaryRow"/> when this <see cref="SummaryOperandBase"/> is selected.
		/// </summary>
		protected override string DefaultRowDisplayLabel
		{
			get
			{
				return SRGrid.GetString("MinSummaryRowLabel");
			}
		}

		#endregion // DefaultRowDisplayLabel

		#region LinqSummaryOperator
		/// <summary>
		/// Gets the operator that will be associated with this operand.
		/// </summary>						
		public override LinqSummaryOperator? LinqSummaryOperator
		{
			get
			{
				return Infragistics.LinqSummaryOperator.Minimum;
			}
		}
		#endregion // LinqSummaryOperator
	}
	#endregion // MinimumSummaryOperand

	#region CountSummaryOperand
	/// <summary>
	/// A <see cref="SummaryOperandBase"/> implementation which executes a LINQ Count summary.
	/// </summary>
	public class CountSummaryOperand : SummaryOperandBase
	{
		#region Members

		CountSummaryCalculator _countSumCalc;

		#endregion // Members

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="CountSummaryOperand"/> class.
		/// </summary>
		public CountSummaryOperand()
		{
			this._countSumCalc = new CountSummaryCalculator();
		}

		#endregion // Constructor

		#region SummaryCalculator
		/// <summary>
		/// Gets the <see cref="SummaryCalculator"/> which will be used to calculate the summary.
		/// </summary>
		public override SummaryCalculatorBase SummaryCalculator
		{
			get
			{
				return this._countSumCalc;
			}
		}
		#endregion // SummaryCalculator

		#region DefaultSelectionDisplayLabel
		/// <summary>
		/// Get's the default text that will be displayed in the drop down for this <see cref="SummaryOperandBase"/>
		/// </summary>
		protected override string DefaultSelectionDisplayLabel
		{
			get
			{
				return SRGrid.GetString("CountSummarySelectionLabel");
			}
		}
		#endregion // DefaultSelectionDisplayLabel

		#region DefaultRowDisplayLabel
		/// <summary>
		/// Get's the default text that will be displayed in the <see cref="Infragistics.Controls.Grids.Primitives.SummaryRow"/> when this <see cref="SummaryOperandBase"/> is selected.
		/// </summary>
		protected override string DefaultRowDisplayLabel
		{
			get
			{
				return SRGrid.GetString("CountSummaryRowLabel");
			}
		}

		#endregion // DefaultRowDisplayLabel

		#region LinqSummaryOperator

		/// <summary>
		/// Gets the operator that will be associated with this operand.
		/// </summary>						
		public override LinqSummaryOperator? LinqSummaryOperator
		{
			get
			{
				return Infragistics.LinqSummaryOperator.Count;
			}
		}

		#endregion // LinqSummaryOperator
	}
	#endregion // CountSummaryOperand

	#region SumSummaryOperand
	/// <summary>
	/// A <see cref="SummaryOperandBase"/> implementation which executes a LINQ Sum summary.
	/// </summary>
	public class SumSummaryOperand : SummaryOperandBase
	{
		#region Members

		SumSummaryCalculator _SumSumCalc;

		#endregion // Members

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="SumSummaryOperand"/> class.
		/// </summary>
		public SumSummaryOperand()
		{
			this._SumSumCalc = new SumSummaryCalculator();
		}

		#endregion // Constructor

		#region SummaryCalculator
		/// <summary>
		/// Gets the <see cref="SummaryCalculator"/> which will be used to calculate the summary.
		/// </summary>
		public override SummaryCalculatorBase SummaryCalculator
		{
			get
			{
				return this._SumSumCalc;
			}
		}
		#endregion // SummaryCalculator

		#region DefaultSelectionDisplayLabel
		/// <summary>
		/// Get's the default text that will be displayed in the drop down for this <see cref="SummaryOperandBase"/>
		/// </summary>
		protected override string DefaultSelectionDisplayLabel
		{
			get
			{
				return SRGrid.GetString("SumSummarySelectionLabel");
			}
		}
		#endregion // DefaultSelectionDisplayLabel

		#region DefaultRowDisplayLabel
		/// <summary>
		/// Get's the default text that will be displayed in the <see cref="Infragistics.Controls.Grids.Primitives.SummaryRow"/> when this <see cref="SummaryOperandBase"/> is selected.
		/// </summary>
		protected override string DefaultRowDisplayLabel
		{
			get
			{
				return SRGrid.GetString("SumSummaryRowLabel");
			}
		}

		#endregion // DefaultRowDisplayLabel

		#region LinqSummaryOperator

		/// <summary>
		/// Gets the operator that will be associated with this operand.
		/// </summary>						
		public override LinqSummaryOperator? LinqSummaryOperator
		{
			get
			{
				return Infragistics.LinqSummaryOperator.Sum;
			}
		}

		#endregion // LinqSummaryOperator
	}
	#endregion // SumSummaryOperand

	#region AverageSummaryOperand
	/// <summary>
	/// A <see cref="SummaryOperandBase"/> implementation which executes a LINQ Average summary.
	/// </summary>
	public class AverageSummaryOperand : SummaryOperandBase
	{
		#region Members

		AverageSummaryCalculator _AverageSumCalc;

		#endregion // Members

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="AverageSummaryOperand"/> class.
		/// </summary>
		public AverageSummaryOperand()
		{
			this._AverageSumCalc = new AverageSummaryCalculator();
		}

		#endregion // Constructor

		#region SummaryCalculator
		/// <summary>
		/// Gets the <see cref="SummaryCalculator"/> which will be used to calculate the summary.
		/// </summary>
		public override SummaryCalculatorBase SummaryCalculator
		{
			get
			{
				return this._AverageSumCalc;
			}
		}
		#endregion // SummaryCalculator

		#region DefaultSelectionDisplayLabel
		/// <summary>
		/// Get's the default text that will be displayed in the drop down for this <see cref="SummaryOperandBase"/>
		/// </summary>
		protected override string DefaultSelectionDisplayLabel
		{
			get
			{
				return SRGrid.GetString("AvgSummarySelectionLabel");
			}
		}
		#endregion // DefaultSelectionDisplayLabel

		#region DefaultRowDisplayLabel
		/// <summary>
		/// Get's the default text that will be displayed in the <see cref="Infragistics.Controls.Grids.Primitives.SummaryRow"/> when this <see cref="SummaryOperandBase"/> is selected.
		/// </summary>
		protected override string DefaultRowDisplayLabel
		{
			get
			{
				return SRGrid.GetString("AvgSummaryRowLabel");
			}
		}

		#endregion // DefaultRowDisplayLabel

		#region LinqSummaryOperator

		/// <summary>
		/// Gets the operator that will be associated with this operand.
		/// </summary>						
		public override LinqSummaryOperator? LinqSummaryOperator
		{
			get
			{
				return Infragistics.LinqSummaryOperator.Average;
			}
		}

		#endregion // LinqSummaryOperator
	}
	#endregion // AverageSummaryOperand
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