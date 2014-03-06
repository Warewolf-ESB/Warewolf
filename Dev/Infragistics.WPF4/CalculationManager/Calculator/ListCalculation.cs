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



using Infragistics.Windows.Licensing;


using Infragistics.Calculations;
using Infragistics.Calculations.Engine;

namespace Infragistics.Calculations
{

	/// <summary>
	/// Defines a calculation to be performed on a list of objects
	/// </summary>
	/// <seealso cref="ItemCalculationBase"/>
	/// <seealso cref="ListCalculator"/>
	/// <seealso cref="ListCalculator.ListCalculations"/>
	public class ListCalculation : ItemCalculationBase
	{
		#region Private Members

		private bool _refIdErrorLogged;

		#endregion //Private Members	
 
		#region Constructor

		/// <summary>
		/// Initializes a new instance of <see cref="ListCalculation"/>
		/// </summary>
		public ListCalculation()
		{
		}

		#endregion //Constructor	
    
		#region Base class overrides

		#region OnPropertyChanged

		/// <summary>
		/// Called when property has changed value
		/// </summary>
		/// <param name="propertyName">The name of the property</param>
		protected override void OnPropertyChanged(string propertyName)
		{
			switch (propertyName)
			{
				case "ReferenceId":
					_refIdErrorLogged = false;
					break;
			}

			base.OnPropertyChanged(propertyName);
		}

		#endregion //OnPropertyChanged

		#region ReferenceIdResolved

		internal override string ReferenceIdResolved
		{
			get
			{
				string refId = this.ReferenceId;
				
				if (!_refIdErrorLogged && string.IsNullOrWhiteSpace(refId))
				{
					_refIdErrorLogged = true;

					Utils.LogDebuggerWarning(SRUtil.GetString("ListCalculation_RefID_Warning", this) + Environment.NewLine);
				}

				return refId;
			}
		}

		#endregion //ReferenceIdResolved

		#region ToString

		/// <summary>
		/// Returns a string that represents this object;
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return string.Format(SRUtil.GetString("ListCalculation_Desc"), this.ReferenceId, this.Formula);
		}

		#endregion //ToString

		#endregion //Base class overrides
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