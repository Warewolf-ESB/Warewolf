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

	#region CalculationFrequency Enum

	/// <summary>
	/// Determines how frequently the calculations are done by the the XamCalculationManager.
	/// </summary>
	/// <remarks>
	/// <p class="body">This enumeration is used by the <see cref="XamCalculationManager"/> component to determine when calculations are performed.</p>
	/// </remarks>
	/// <seealso cref="XamCalculationManager.CalculationFrequency"/>
	/// <seealso cref="XamCalculationManager.AsynchronousCalculationDuration"/>
	public enum CalculationFrequency
	{
		/// <summary>
		/// Calculations will be performed on a timer. This frees the UI Thread, but means that calculations may not always be current.
		/// </summary>
		Asynchronous,

		/// <summary>
		/// Calculations are performed every time a change notifiation is received. That is, every time a value involved in a calculation is changed, all dirtied calculations are recalculated and the UI thread is locked until all calculations are complete.
		/// </summary>
		Synchronous,

		/// <summary>
		/// XamCalculationManager never performs any calculations until the <see cref="XamCalculationManager.PerformCalculations"/> method is explicitly invoked. 
		/// </summary>
		Manual,
	}

	#endregion CalculationFrequency Enum

	#region ReferenceNodeType Enum

	/// <summary>
	/// Enumeration used to indicate the type of object that is represented by the <see cref="CalculationReferenceNode"/>
	/// </summary>
	public enum ReferenceNodeType
	{
		/// <summary>
		/// A control or sub-object of a control Node
		/// </summary>
		Control,

		/// <summary>
		/// The NamedReferencesGroup Node
		/// </summary>
		NamedReferencesGroup,

		/// <summary>
		/// A NamedReference Node
		/// </summary>
		NamedReference,

		/// <summary>
		/// The ControlsGroup Node
		/// </summary>
		ControlsGroup,

		/// <summary>
		/// The NamedReferences "All" Group Node
		/// </summary>
		NamedReferencesAllGroup,

		/// <summary>
		/// The NamedReferences "Unassigned" Group Node
		/// </summary>
		NamedReferencesUnassignedGroup,

		/// <summary>
		/// A NamedReferences "Other" Group Node
		/// </summary>
		NamedReferencesOtherGroup,

		/// <summary>
		/// The root reference for an ItemCalculator or ListCalculator
		/// </summary>
		Calculator,

		/// <summary>
		/// The reference for an ItemCalculation or ListCalculation inside an ItemCalculator or ListCalculator
		/// </summary>
		CalculatorCalculation,

		/// <summary>
		/// The reference for the property of an item inside an ItemCalculator or ListCalculator
		/// </summary>
		CalculatorItemProperty,

		/// <summary>
		/// The ListCalculations Node Group
		/// </summary>
		ListCalculationsGroup,
	}

	#endregion ReferenceNodeType Enum

	#region ValueDirtiedAction Enum

	/// <summary>
	/// Indicates which type of action took place to dirty the Calculation Network
	/// </summary>
	public enum ValueDirtiedAction
	{
		/// <summary>
		/// A reference was added to the Calc Network
		/// </summary>
		ReferenceAdded,

		/// <summary>
		/// A reference was removed the Calc Network. Note that in this case the formula associated with the reference is also removed.
		/// </summary>
		ReferenceRemoved,


		/// <summary>
		/// A row reference was added to the Calc Network. A row reference is different from a regular reference because it refers to an item in a collection such as a data grid Record or Row.
		/// </summary>





		RowReferenceAdded,


		/// <summary>
		/// A row reference was removed the Calc Network. Note that in this case the formula associated with the reference is also removed. A row reference is differnet from a regular reference because it refer to an item in a collection such as a data grid Record or Row.
		/// </summary>





		RowReferenceRemoved,

		/// <summary>
		/// A rows collection reference was resynched.
		/// </summary>
		RowsCollectionReferenceResynched,

		/// <summary>
		/// A rows collection reference was sorted.
		/// </summary>
		RowsCollectionReferenceSorted,

		/// <summary>
		/// The visibility of a row or rows in a rows collection has changed.
		/// </summary>
		RowsCollectionVisibilityChanged,

		/// <summary>
		/// A Formula was added to the Calc Network
		/// </summary>
		FormulaAdded,

		/// <summary>
		/// A Formula was removed from the Calc Network
		/// </summary>
		FormulaRemoved,

		/// <summary>
		/// A Value in the Calc Network has changed
		/// </summary>
		ValueChanged,
	}

	#endregion ValueDirtiedAction Enum

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