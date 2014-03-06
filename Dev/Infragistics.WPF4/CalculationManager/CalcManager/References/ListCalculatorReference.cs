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
	internal class ListCalculatorReference : ItemCalculatorReferenceBase
	{
		private ListCalculator _listCalculator;

		internal ListCalculatorReference(ListCalculator calculator)
			: base(calculator)
		{
			_listCalculator = calculator;
		}

		#region Base class overrides

		#region Methods

		#region CreateReference

		public override ICalculationReference CreateReference(string inReference)
		{
			RefParser refParser = RefParser.Parse(inReference);

			if (RefParser.AreStringsEqual(this.ElementName, refParser.RootName, true)
				&& refParser.TupleCount == 2)
			{
				string name = refParser.LastTuple.Name;
				if (refParser.HasScopeAll)
					return this.FindAll(name);
				if (refParser.HasAbsoluteIndex)
					return this.FindItem(name, refParser.LastTuple.ScopeIndex, false);
				if (refParser.HasRelativeIndex)
					return this.FindItem(name, refParser.LastTuple.ScopeIndex, true);

				ICalculationReference childref = _listCalculator.GetReference(name, true, true, true, true);

				if (childref != null)
					return childref;

				return new CalculationReferenceError(inReference, "Could not find Reference");
			}

			return base.CreateReference(inReference);
		}

		#endregion //CreateReference	
    
		#region FindItem

		public override ICalculationReference FindItem(string name)
		{
			return _listCalculator.GetReference(name);
		}
		public override ICalculationReference FindItem(string name, int index, bool isRelative)
		{
			ICalculationReference cellRef = _listCalculator.GetCellAtIndexReference(name, index, isRelative);

			if (cellRef != null)
				return cellRef;

			return base.FindItem(name, index, isRelative);
		}

		#endregion //FindItem	
    		
		#region FindAll

		public override ICalculationReference FindAll(string name)
		{
			ICalculationReference allref = _listCalculator.GetAllCellsInColumnReference(name);

			if (allref != null)
				return allref;

			return base.FindAll(name);
		}

		#endregion //FindAll	
    
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