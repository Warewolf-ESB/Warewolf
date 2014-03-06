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

	#region CalculationResult class

	/// <summary>
	/// Contains a <see cref="CalculationValue"/> object and sends notifications when its changed.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// Purpose of this object is to provide a result of a calculation in <see cref="ListCalculator"/>
	/// or <see cref="ItemCalculator"/> that will send notifications when the calculated value changes.
	/// For example, with a "[A] + [B]" calculation in an ItemCalculator, when either A or B property
	/// is changed on the underlying data item, CalculationResult object obtained via ItemCalculator's 
	/// <see cref="ItemCalculator.Results"/> dictionary will have its <see cref="CalculationResult.Value"/>
	/// property be automatically changed to reflect the new result and the appropriate PropertyChanged notification
	/// will be sent out. This is helpful when one wants to bind to the result of a calculation.
	/// </para>
	/// </remarks>
	public class CalculationResult : PropertyChangeNotifier
	{
		#region Member Vars

		private CalculationValue _value;
		private CalculationErrorValue _error;

		#endregion // Member Vars

		#region Constructor

		/// <summary>
		/// Constructor. Initializes a new instance of <see cref="CalculationResult"/>.
		/// </summary>
		public CalculationResult()
		{
		}

		#endregion // Constructor

		#region Properties

		#region CalculationValue

		/// <summary>
		/// Calculation result value. As the underlying formula is recalculated, this property will reflect the new result.
		/// </summary>
		public CalculationValue CalculationValue
		{
			get
			{
				return _value;
			}
			internal set
			{
				if (_value != value)
				{
					object oldVal = this.Value;
					_value = value;

					bool raiseValue = !Utils.AreSameTypeAndEqual(oldVal, this.Value);

					var error = null != value ? value.GetResolvedValue() as CalculationErrorValue : null;
					bool raiseIsError = null == _error ^ null == error;
					bool raiseErrorText = GetErrorMessage(_error) != GetErrorMessage(error);

					_error = error;

					this.RaisePropertyChangedEvent("CalculationValue");

					if (raiseValue)
						this.RaisePropertyChangedEvent("Value");

					if (raiseIsError)
						this.RaisePropertyChangedEvent("IsError");

					if (raiseErrorText)
						this.RaisePropertyChangedEvent("ErrorText");
				}
			}
		}

		#endregion // CalculationValue

		#region ErrorText

		/// <summary>
		/// If the result of the calculation is an error, returns the error text. Otherwise returns null.
		/// </summary>
		public string ErrorText
		{
			get
			{
				return null != _error ? _error.Message : null;
			}
		}

		#endregion // ErrorText

		#region IsError

		/// <summary>
		/// Indicates if the result of the calculation is an error.
		/// </summary>
		public bool IsError
		{
			get
			{
				return null != _error;
			}
		}

		#endregion // IsError

		#region Value

		/// <summary>
		/// This property returns the value of the <see cref="Infragistics.Calculations.Engine.CalculationValue.Value"/> property of the object returned by the <see cref="CalculationValue"/> property.
		/// </summary>
		/// <remarks>
		/// <para class="note"><b>Note:</b> in the case of an <see cref="ItemCalculator"/> whose <see cref="ItemCalculator.Item"/> has not been set, this property will return null even though 
		/// the <see cref="Infragistics.Calculations.Engine.CalculationValue.Value"/> property returns an error. This is useful for situations where the item might be null e.g.
		/// in applications that allow the user to select the item but don't want to display an error in that scenario.</para>
		/// </remarks>
		public object Value
		{
			get
			{
				// JJD 10/12/11 - TFS91573
				// Moved logic into GetValue virtual method instead of having this Value property defined as virtual.
				// This was required because binding in SL choked because of our internal derived class 
				// InternalCalculationResult. SL is very finicky about binding to properties exposed off internal classes.
				//return null != _value ? _value.Value : null;
				return this.GetValue();
			}
		}

		#endregion // Value

		#endregion // Properties

		#region Methods

		#region GetErrorMessage

		private static string GetErrorMessage(CalculationErrorValue val)
		{
			return null != val ? val.Message : null;
		}

		#endregion // GetErrorMessage

		#region GetValue

		// JJD 10/12/11 - TFS91573 - added
		internal virtual object GetValue()
		{
			return null != _value ? _value.Value : null;
		}

		#endregion //GetValue	
    
		#region ToString

		/// <summary>
		/// Returns the string representation of the underlying value.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return null != _value ? _value.ToString() : string.Empty;
		}

		#endregion // ToString

		#endregion // Methods
	}

	#endregion //CalculationResult class	
    
	#region InternalCalculationResult

	internal class InternalCalculationResult : CalculationResult
	{
		#region Private Members

		private bool _filterOutErrors;

		private static string _ItemNotSetMsg;
		private static CalculationValue _ItemNotSetCalcValue;

		#endregion //Private Members

		#region Constructor

		internal InternalCalculationResult(bool filterOutErrors)
		{
			_filterOutErrors = filterOutErrors;
		}

		#endregion //Constructor	
    
		#region Base class overrides

		#region GetValue

		internal override object GetValue()
		{
			// if we are filtering out errors then if the CalcValue
			// is an error then return null;
			if (this._filterOutErrors)
			{
				CalculationValue calcValue = this.CalculationValue;

				if (calcValue == null || calcValue.IsError)
					return null;
			}

			// otherwise return the base GetValue()
			return base.GetValue();
		}

		#endregion //GetValue

		#endregion //Base class overrides

		#region Properties

		#region FilterOutErrors

		internal bool FilterOutErrors
		{
			get
			{
				return _filterOutErrors;
			}
			set
			{
				if (value != _filterOutErrors)
				{
					_filterOutErrors = value;

					CalculationValue calcValue = this.CalculationValue;

					if (calcValue != null && calcValue.IsError)
						this.OnPropertyChanged("Value");

				}
			}
		}

		#endregion //FilterOutErrors

		#region ItemNotSetCalcValue static 
    
    	internal static CalculationValue ItemNotSetCalcValue
		{
			get 
			{
				string msg = SRUtil.GetString("SourceItemNotSpecified");

				// see if the cached string has changed and clear the
				// cached value if it has
				if ( _ItemNotSetMsg == null || _ItemNotSetMsg != msg)
				{
					_ItemNotSetMsg = msg;
					_ItemNotSetCalcValue = null;
				}

				if ( _ItemNotSetCalcValue == null )
				{
					_ItemNotSetCalcValue = new CalculationValue(new CalculationErrorValue(CalculationErrorCode.NA, _ItemNotSetMsg ));
				}

				return _ItemNotSetCalcValue;
			}
		}

   		#endregion //ItemNotSetCalcValue static 	
    
		#endregion //Properties
	}

	#endregion //InternalCalculationResult	
    
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