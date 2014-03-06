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
	/// This is a support interface used for interaction between calc participants like XamDataGrid and calc manager.
	/// </summary>
	public interface ICalculationManager
	{
		/// <summary>
		/// Pefroms the specified action.
		/// </summary>
		/// <param name="action">Action to perform.</param>
		/// <param name="data">Optional data that some actions may require.</param>
		/// <returns>Return value if any of the action performed.</returns>
		object PerformAction( CalculationEngineAction action, object data );

		/// <summary>
		/// Compiles the specified formula. Returns an instance of <see cref="ICalculationFormula"/> which represents a formula. Throws NullReferenceException if baseReference or formula is null. NOTE: This method does not add the formula to the calculation network.
		/// </summary>
		/// <param name="baseReference">Base reference to use for the reference parsing. Returned formula instance will make use this reference as its base reference during parsing as well as during evaluation time to resolve relative references.</param>
		/// <param name="formula">The formula string.</param>
		/// <param name="suppressSyntaxErrorEvent">Pass in true to prevent the Syntax error event from firing.</param>
		/// <remarks>
		/// <p class="body">Returns an instance of <see cref="ICalculationFormula"/>. ICalculationFormula has information on any syntax or reference errors.</p>
		/// </remarks>
		ICalculationFormula CompileFormula( ICalculationReference baseReference, string formula, bool suppressSyntaxErrorEvent );

		/// <summary>
		/// Used to notify the CalcManager that a calculation or reference error has occurred. Underlying implementation may use this to raise an event, for example
		/// the <see cref="XamCalculationManager.FormulaReferenceError"/> or <see cref="XamCalculationManager.FormulaSyntaxError"/> events of the <see cref="XamCalculationManager"/>.
		/// </summary>
		/// <param name="reference">The <see cref="ICalculationReference"/> associated with the formula.</param>
		/// <param name="errorInfo">The <see cref="CalculationErrorValue"/> describing the error.</param>
		/// <param name="defaultReplacementValue">The value that will be used set on the target object. For example, if the formula is on a control, this is the value that will be set on it.</param>
		/// <returns>The <see cref="FormulaCalculationErrorEventArgs"/> created from the specified parameters.</returns>
		FormulaCalculationErrorEventArgs RaiseFormulaError( ICalculationReference reference, CalculationErrorValue errorInfo, object defaultReplacementValue );


		/// <summary>
		/// Checks to see if a reference is dirty, and optionally waits for it to be recalculated
		/// </summary>
		/// <param name="reference">Reference whose dirty state is requested</param>
		/// <param name="wait">Denotes whether caller wants to wait until the reference is recalculated</param>
		/// <returns>True if the reference is dirty</returns>
		bool EnsureCalculated( ICalculationReference reference, bool wait );


		/// <summary>
		/// Adds the specified formula to the calculation network. This method takes no action if the formula has already been added to the calculcation network. Also it throws an exception if the formula contains a syntax error. However if it contains a reference error, it still adds the formula to the calculation network. This method throws an exception if the formula was compiled using a different ICalculationManager.
		/// </summary>
		/// <param name="formula">The <see cref="ICalculationFormula"/> to add to the calculation network.</param>
		void AddFormula( ICalculationFormula formula );

		/// <summary>
		/// Removes the specified formula from the canclulation network. Takes no action if the formula doesn't exist in the calculation network.
		/// </summary>
		/// <param name="formula">The <see cref="ICalculationFormula"/> to remove from the calculation network.</param>
		void RemoveFormula( ICalculationFormula formula );

		/// <summary>
		/// Notifies the calc manager of the change in reference' value.
		/// </summary>
		/// <param name="reference"></param>
		void NotifyValueChanged( ICalculationReference reference );

		/// <summary>
		/// Removes the reference and the associated formula from the calc network.
		/// </summary>
		/// <param name="reference"></param>
		/// <remarks>
		/// <p class="body"><b>RemoveReference</b> removes the specified reference from the calculation network. If the reference has a formula, the formula is also removed.</p>
		/// </remarks>
		void RemoveReference( ICalculationReference reference );

		/// <summary>
		/// Deletes the reference and the associated formula from the calc network.
		/// </summary>
		/// <param name="reference"></param>
		/// <remarks>
		/// <p class="body"><b>RemoveRowReference</b> removes the specified reference from the calculation network. This method should be used when removing the reference for an item in a collection, such as a Record or a Row object. For other (non-collection) objects, use RemoveReference. If the reference has a formula, the formula is also removed.</p>
		/// </remarks>
		void RemoveRowReference( ICalculationReference reference );

		/// <summary>
		/// Adds the reference and the associated formula to the calc network.
		/// </summary>
		/// <param name="reference"></param>
		/// <remarks>
		/// <p class="body"><b>AddReference</b> adds the specified reference to the calculation network. </p>
		/// </remarks>
		void AddReference( ICalculationReference reference );

		/// <summary>
		/// Inserts the reference and the associated formula to the calc network.
		/// </summary>
		/// <param name="reference"></param>
		/// <remarks>
		/// <p class="body"><b>AddRowReference</b> adds the specified reference to the calculation network. This method should be used when adding a reference for an item in a collection, such as a Record or Row object. For other (non-collection) objects, use AddReference.</p>
		/// </remarks>
		void AddRowReference( ICalculationReference reference );

		/// <summary>
		/// Notifies the Calc Manager that a Rows collection was sorted.
		/// </summary>
		/// <param name="reference"></param>
		void RowsCollectionReferenceSorted( ICalculationReference reference );

		/// <summary>
		/// Notifies the Calc Manager that a Rows collection was resynched.
		/// </summary>
		void RowsCollectionReferenceResynched( ICalculationReference reference );

		/// <summary>
		/// Notifies the Calc Manager that the visibility of part of a Rows Collection has changed.
		/// </summary>
		void RowsCollectionVisibilityChanged( ICalculationReference reference );

		/// <summary>
		/// Returns a reference with the specified name. If no such reference exists, returns null.
		/// </summary>
		/// <param name="name">The absolute or relative name of the reference to locate. If relative name is specified, it will be assumed to be a root level reference.</param>
		ICalculationReference GetReference( string name );

		/// <summary>
		/// Adds an <see cref="ICalculationParticipant"/> to the CalcManager. A Particpant must be added to the CalcManager before it can be used in any calculations.
		/// </summary>
		/// <param name="participant">An <see cref="ICalculationParticipant"/> to be added to the CalcManager.</param>
		void AddParticipant( ICalculationParticipant participant );

		/// <summary>
		/// Removes an <see cref="ICalculationParticipant"/> from the CalcManager.
		/// </summary>
		/// <param name="participant">An <see cref="ICalculationParticipant"/> to be removed from the CalcManager.</param>
		void RemoveParticipant( ICalculationParticipant participant );

		/// <summary>
		/// Processes all events in the event queue.
		/// </summary>
		void ProcessEventQueue( );
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