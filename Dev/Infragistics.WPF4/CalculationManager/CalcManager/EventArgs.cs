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

	#region DataAccessErrorEventArgs Class

	/// <summary>
	/// Event args for <see cref="ItemCalculatorBase"/>.<see cref="ItemCalculatorBase.DataAccessError"/> event.
	/// </summary>
	public class DataAccessErrorEventArgs : System.EventArgs
	{
		#region Member Variables

		private object _item;
		private object _value;
		private string _property;
		private string _errorMessage;
		private bool _thrownDuringSet;
		private Exception _exception;

		#endregion //Member Variables

		#region Constructor
		internal DataAccessErrorEventArgs(object item, string property, string errorMessage, bool thrownDuringSet, object value, Exception exception)
		{
			this._item = item;
			this._property = property;
			this._exception = exception;
			this._errorMessage = errorMessage;
			this._thrownDuringSet = thrownDuringSet;
			this._value = value;
		}
		#endregion //Constructor

		#region Properties

		/// <summary>
		/// Returns the exception that was thrown (read-only).
		/// </summary>
		public Exception Exception
		{
			get { return this._exception; }
		}

		/// <summary>
		/// Returns or sets the error message.
		/// </summary>
		/// <remarks>
		/// <para class="note"><b>Note:</b> if <see cref="ThrownDuringSet"/> is false this string will be returned as part of one or more formula errors.</para>
		/// </remarks>
		public string ErrorMessage
		{
			get { return this._errorMessage; }
			set { this._errorMessage = value; }
		}

		/// <summary>
		/// Gets the item that the get or set was attempted for (read-only).
		/// </summary>
		public object Item
		{
			get { return this._item; }
		}

		/// <summary>
		/// Returns the name of the property (read-only).
		/// </summary>
		public string Property
		{
			get { return this._property; }
		}

		/// <summary>
		/// Returns whether the exception was thrown during a 'set' operation (read-only).
		/// </summary>
		/// <value>True if the exception was thrown during a 'set' operation, otherwise false which denotes that the exception was thrown during a 'get' operation.</value>
		public bool ThrownDuringSet
		{
			get { return this._thrownDuringSet; }
		}

		/// <summary>
		/// Returns the value that was attempting to be set on the property (read-only).
		/// </summary>
		/// <remarks>
		/// <para class="note"><b>Note:</b> if <see cref="ThrownDuringSet"/> is false then this property will return null.</para>
		/// </remarks>
		public object Value
		{
			get { return this._value; }
		}

		#endregion //Properties
	}
	#endregion //DataAccessErrorEventArgs Class

	#region FormulaErrorEventArgsBase Class

	/// <summary>
	/// Base class for event args for formula error events of XamCalculationManager.
	/// </summary>
	public abstract class FormulaErrorEventArgsBase : System.EventArgs
	{
		#region Member Variables

		private object context = null;
		private string errorDisplayText = null;

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="FormulaErrorEventArgsBase"/>.
		/// </summary>
		/// <param name="context">Object that provides some context for the reference for which the formula error has occurred.</param>
		/// <param name="errorDisplayText">Error message to display</param>
		protected FormulaErrorEventArgsBase( object context, string errorDisplayText )
		{
			this.context = context;
			this.errorDisplayText = errorDisplayText;
		}
		#endregion //Constructor

		#region Properties
		/// <summary>
		/// Gets the context for the formula error.
		/// </summary>
		/// <remarks><p class="body">The context is the target object of the formula. It could be a Cell, a Control, a NamedReference, etc.</p></remarks>
		public object Context
		{
			get { return this.context; }
		}

		/// <summary>
		/// Returns or sets the error message to display.
		/// </summary>
		/// <remarks><p class="body">The error text to be displayed. The display of the error text will vary depending on the Context. A Control on a form may display the ErrorDisplayText as a tooltip on an error icon. A NamedReference may not display the ErrorDisplayText since it doesn't have a visual representation. 
		/// A Grid cell will display the ErrorDisplayText in the cell, if possible or as a tooltip on an error icon.</p></remarks>
		public string ErrorDisplayText
		{
			get { return this.errorDisplayText; }
			set { this.errorDisplayText = value; }
		}
		#endregion //Properties
	}
	#endregion //FormulaErrorEventArgsBase Class

	#region FormulaCalculationErrorEventArgs Class

	/// <summary>
	/// Event args for <see cref="XamCalculationManager.FormulaCalculationError"/> event of XamCalculationManager.
	/// </summary>
	public class FormulaCalculationErrorEventArgs : FormulaErrorEventArgsBase
	{
		#region Member Variables

		private object errorValue = null;
		private CalculationErrorValue errorInfo = null;

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="FormulaCalculationErrorEventArgs"/>.
		/// </summary>
		/// <param name="context">Object for which the calculation error is occuring.</param>
		/// <param name="errorDisplayText">Error message to display</param>
		/// <param name="errorValue">Error value that should be used to update the object whose calculation resulted in an error</param>
		/// <param name="errorInfo"><see cref="CalculationErrorValue"/> providing information about the error</param>
		public FormulaCalculationErrorEventArgs( object context, string errorDisplayText, object errorValue, CalculationErrorValue errorInfo )
			: base( context, errorDisplayText )
		{
			this.errorValue = errorValue;
			this.errorInfo = errorInfo;
		}
		#endregion //Constructor

		#region Properties

		/// <summary>
		/// Gets the value that's in error.
		/// </summary>
		/// <remarks>
		/// <p class="body">If a Calculation or Reference Error occurs during a calculation, ErrorValue can be used to specify a default value to be used for display or saving. When an error occurs in a calculation, this value will be assigned to the appropriate property of the Context object. For example, in the case of a TextBox's Text property, the property itself will be set to the error value so that it both displays in the TextBox and is saved if the Text property is bound. In the case of an data grid cell, this value will not be displayed, but will be saved to the underlying data source. Objects that do not display or bind, such as a NamedReference, will ignore the ErrorValue.</p>
		/// </remarks>
		public object ErrorValue
		{
			get { return this.errorValue; }
			set { this.errorValue = value; }
		}

		/// <summary>
		/// Gets the <see cref="CalculationErrorValue"/>.
		/// </summary>
		/// <remarks><p class="body">Provides detailed information about the error, such as the exact type of error.</p></remarks>
		public CalculationErrorValue ErrorInfo
		{
			get { return this.errorInfo; }
		}

		#endregion //Properties
	}

	#endregion //FormulaCalculationErrorEventArgs Class 

	#region FormulaReferenceErrorEventArgs Class

	/// <summary>
	/// Event args for <see cref="XamCalculationManager.FormulaCalculationError"/> or <see cref="XamCalculationManager.FormulaReferenceError"/> events of a <see cref="XamCalculationManager"/>.
	/// </summary>
	public class FormulaReferenceErrorEventArgs : FormulaCalculationErrorEventArgs
	{
		#region Constructor

		/// <summary>
		/// Initializes a new <see cref="FormulaCalculationErrorEventArgs"/>
		/// </summary>
		/// <param name="context">Object for which the calculation error is occurring</param>
		/// <param name="errorDisplayText">Error message to display</param>
		/// <param name="errorInfo"><see cref="CalculationErrorValue"/> providing information about the error</param>
		/// <param name="errorValue">Error value that should be used to update the object whose calculation resulted in an error</param>
		public FormulaReferenceErrorEventArgs( object context, string errorDisplayText, object errorValue, CalculationErrorValue errorInfo )
			: base( context, errorDisplayText, errorValue, errorInfo )
		{
		}

		#endregion // Constructor
	}

	#endregion // FormulaReferenceErrorEventArgs Class

	#region FormulaSyntaxErrorEventArgs Class

	/// <summary>
	/// Event args for <see cref="XamCalculationManager.FormulaSyntaxError"/> event of an <see cref="XamCalculationManager"/>.
	/// </summary>
	public class FormulaSyntaxErrorEventArgs : FormulaErrorEventArgsBase
	{
		/// <summary>
		/// Creates a new <see cref="FormulaSyntaxErrorEventArgs"/> and sets properties to the specified values.
		/// </summary>
		/// <param name="context">Sets the <see cref="FormulaErrorEventArgsBase.Context"/> property.</param>
		/// <param name="errorDisplayText">Sets the <see cref="FormulaErrorEventArgsBase.ErrorDisplayText"/> property.</param>
		public FormulaSyntaxErrorEventArgs( object context, string errorDisplayText )
			: base( context, errorDisplayText )
		{
		}
	}

	#endregion FormulaSyntaxErrorEventArgs Class

	#region FormulaCircularityErrorEventArgs Class

	/// <summary>
	/// Event args for <see cref="XamCalculationManager.FormulaCircularityError"/> event of an <see cref="XamCalculationManager"/>.
	/// </summary>
	public class FormulaCircularityErrorEventArgs : FormulaErrorEventArgsBase
	{
		bool displayErrorMessage = true;

		/// <summary>
		/// Creates a new <see cref="FormulaCircularityErrorEventArgs"/> and sets properties to the specified values.
		/// </summary>
		/// <param name="context">Sets the <see cref="FormulaErrorEventArgsBase.Context"/> property.</param>
		/// <param name="errorDisplayText">Sets the <see cref="FormulaErrorEventArgsBase.ErrorDisplayText"/> property.</param>
		/// <param name="displayErrorMessage">Sets the <see cref="DisplayErrorMessage"/> property.</param>
		public FormulaCircularityErrorEventArgs( object context, string errorDisplayText, bool displayErrorMessage )
			: base( context, errorDisplayText )
		{
			this.displayErrorMessage = displayErrorMessage;
		}

		/// <summary>
		/// Gets / sets whether to display an error message to the user.
		/// </summary>
		/// <remarks><p class="body">Gets / sets whether to display an error message to the user.</p></remarks>
		public bool DisplayErrorMessage
		{
			get { return this.displayErrorMessage; }
			set { this.displayErrorMessage = value; }
		}
	}

	#endregion FormulaCircularityErrorEventArgs Class

	#region ValueDirtiedEventArgs Class

	/// <summary>
	/// Base class for event args that have an associated row.
	/// </summary>
	public class ValueDirtiedEventArgs : System.EventArgs
	{
		private object context = null;
		private ValueDirtiedAction action = ValueDirtiedAction.ValueChanged;

		/// <summary>
		/// Initializes a new <see cref="ValueDirtiedEventArgs"/> object.
		/// </summary>
		/// <param name="context">Sets the <see cref="Context"/> property.</param>
		/// <param name="action">Sets the <see cref="Action"/> property.</param>
		public ValueDirtiedEventArgs( object context, ValueDirtiedAction action )
		{
			this.context = context;
			this.action = action;
		}

		/// <summary>
		/// Gets the Context.
		/// </summary>
		/// <remarks><p class="body">The <b>Context</b> indicates the object which was changed. This can be any object that particpates in the calculation network such a Cell, NamedReference, Control, etc.</p></remarks>
		public object Context
		{
			get { return this.context; }
		}

		/// <summary>
		/// Gets a the action which occurred.
		/// </summary>
		/// <remarks>
		/// <p class="body">The action indicates what type of change occurred. See the <see cref="ValueDirtiedAction"/> enumeration for the list of possible actions.</p>
		/// </remarks>
		public ValueDirtiedAction Action
		{
			get { return this.action; }
		}
	}

	#endregion ValueDirtiedEventArgs Class
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