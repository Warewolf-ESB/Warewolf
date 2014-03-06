using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infragistics.Collections;
using System.Collections.Specialized;
using Infragistics.Calculations.Engine;
using System.Diagnostics;
using System.Windows.Threading;
using System.Threading;

namespace Infragistics.Calculations
{
	/// <summary>
	/// A read-only dictionary that returns the results of <see cref="ItemCalculationBase.Formula"/> calculations defined via an <see cref="ItemCalculator"/>'s <see cref="ItemCalculator.Calculations"/> collection or
	/// a <see cref="ListCalculator"/>'s <see cref="ListCalculator.ListCalculations"/> collection.
	/// </summary>
	/// <seealso cref="ItemCalculator"/>
	/// <seealso cref="ItemCalculator.Calculations"/>
	/// <seealso cref="ItemCalculator.Results"/>
	/// <seealso cref="ListCalculator"/>
	/// <seealso cref="ListCalculator.ListCalculations"/>
	/// <seealso cref="ListCalculator.ListResults"/>
	public sealed class CalculationResultsDictionary : PropertyChangeNotifier, IDictionary<string, CalculationResult>
	{
		#region Private Members

		private ItemCalculatorBase _calculator;
		private Dictionary<string, CalculationResult> _dictionary = new Dictionary<string, CalculationResult>(StringComparer.InvariantCultureIgnoreCase);
		private bool _propertyChangePending;

		#endregion //Private Members	

		#region CalculationResultsDictionary

		internal CalculationResultsDictionary(ItemCalculatorBase calculator)
		{
			_calculator = calculator;
		}

		#endregion //CalculationResultsDictionary	
    
		#region Properties

		#region Public Properties

		#region Indexer

		/// <summary>
		/// Indexer [string]
		/// </summary>
		/// <param name="key">The resolved RefereceId of an associated <see cref="ItemCalculation"/> or <see cref="ListCalculation"/> that had a Formula specified.</param>
		/// <returns>The result of the formula calculation</returns>
		/// <remarks>
		/// <para class="note"><b>Note:</b> if a <see cref="CalculationResult"/> is associated with an <see cref="ItemCalculation"/> whose <see cref="ItemCalculationBase.ReferenceId"/> is not specified then 
		/// the key of the result will be its <see cref="ItemCalculation.TargetProperty"/></para>
		/// </remarks>
		/// <seealso cref="ItemCalculation"/>
		/// <seealso cref="ListCalculation"/>
		/// <seealso cref="ItemCalculator.Results"/>
		/// <seealso cref="ListCalculator.ListResults"/>
		/// <seealso cref="ItemCalculationBase.Formula"/>
		public CalculationResult this[string key]
		{
			get
			{
				CalculationResult result;
				if (_dictionary.TryGetValue(key, out result))
					return result;

				if (Debugger.IsAttached && Debugger.IsLogging())
					Utils.LogDebuggerWarning(SRUtil.GetString("ResultKeyNotFound", key) + Environment.NewLine);

				return null;
			}
			set
			{
				throw new NotSupportedException(SRUtil.GetString("LE_NotSupported_1"));
			}
		}

		#endregion //Indexer	
    
		#endregion //Public Properties	
    
		#endregion //Properties	
        
		#region Methods

		#region Internal Methods

		#region InternalAdd

		internal void InternalAdd(string key, CalculationResult value)
		{
			_dictionary.Add(key, value);

			this.RaiseChangedEventsAsync();

		}

		#endregion //InternalAdd	
    
		#region InternalClear

		internal void InternalClear()
		{
			_dictionary.Clear();

			this.RaiseChangedEventsAsync();

		}

		#endregion //InternalClear	
    
		#region InternalRemove

		internal bool InternalRemove(string key)
		{
			bool rtn = _dictionary.Remove(key);

			this.RaiseChangedEventsAsync();

			return rtn;
		}

		#endregion //InternalRemove	
    
		#region InternalSet

		internal void InternalSet(string key, CalculationValue value)
		{
			CalculationResult result = this.VerifyCalculationResultExists(key);
			
			result.CalculationValue = value;

			// for ItemCalculator's we need to clear FilterOutErrors flag
 			// now that the value has been set
			if (_calculator is ItemCalculator)
			{
				InternalCalculationResult icr = result as InternalCalculationResult;

				if (icr != null)
					icr.FilterOutErrors = false;
			}
		}

		#endregion //InternalSet	
 
		#region OnIsItemSetChanged

		internal void OnIsItemSetChanged(bool isItemSet)
		{
			// for ItemCalculator's we need to manage the FilterOutErrors flag
			// and the value of the results when the item is set
			if (!(_calculator is ItemCalculator))
				return;

			// Loop over the results and reset their FilterOutErrors flags accordingly
			foreach (CalculationResult result in _dictionary.Values)
			{
				InternalCalculationResult icr = result as InternalCalculationResult;

				if (icr != null)
				{
					// If the item is not longer set then change the calc value accordingly
					// Note: if the item is being set we can clear the calc value because
					// the calc engine will do the set
					if (isItemSet)
						icr.CalculationValue = null;
					else
					{
						CalculationValue value = icr.CalculationValue;

						// If the value is a syntax error (i.e.its error code is 'NA' we
						// should leave it alone. Otherwise we should set the value to the
						// 'Item Not Set' error
						if (value == null || (value.IsError && ((CalculationErrorValue)( value.Value)).Code != CalculationErrorCode.NA))
							icr.CalculationValue = InternalCalculationResult.ItemNotSetCalcValue;
					}

					// set the FilterOutErrors property appropriately
					icr.FilterOutErrors = !isItemSet;

				}
			}
		}

		#endregion //OnIsItemSetChanged	
    
		#region VerifyCalculationResultExists

		internal CalculationResult VerifyCalculationResultExists(string key)
		{
			if (_dictionary.ContainsKey(key))
				return _dictionary[key];

			InternalCalculationResult result = new InternalCalculationResult(true);

			// for ItemCalculator's we need to initialize the results with
			// an 'Item Not Set' error
			if (_calculator is ItemCalculator)
				result.CalculationValue = InternalCalculationResult.ItemNotSetCalcValue;
			
			_dictionary[key] = result;

			this.RaiseChangedEventsAsync();

			return result;
		}

		#endregion //VerifyCalculationResultExists

		#endregion //Internal Methods

		#region Private Methods

		#region RaiseChangedEvents

		private void RaiseChangedEventsAsync()
		{
			if (_propertyChangePending)
				return;

			_propertyChangePending = true;

			var syncContext = new DispatcherSynchronizationContext();
			syncContext.Post(new SendOrPostCallback(this.RaiseChangedEvents), null);
		}

		private void RaiseChangedEvents(object dummyParam)
		{
			_propertyChangePending = false;

			this.RaisePropertyChangedEvent("Item[]");
			this.RaisePropertyChangedEvent("Count");
		}

		#endregion //RaiseChangedEvents	
    
		#endregion //Private Methods	
    
		#endregion //Methods	

		#region IDictionary<string,CalculationResult> Members

		void IDictionary<string, CalculationResult>.Add(string key, CalculationResult value)
		{
			throw new NotSupportedException(SRUtil.GetString("LE_NotSupported_1"));
		}

		bool IDictionary<string, CalculationResult>.ContainsKey(string key)
		{
			return _dictionary.ContainsKey(key); ;
		}

		ICollection<string> IDictionary<string, CalculationResult>.Keys
		{
			get { return _dictionary.Keys; }
		}

		bool IDictionary<string, CalculationResult>.Remove(string key)
		{
			throw new NotSupportedException(SRUtil.GetString("LE_NotSupported_1"));
		}

		bool IDictionary<string, CalculationResult>.TryGetValue(string key, out CalculationResult value)
		{
			return _dictionary.TryGetValue(key, out value);
		}

		ICollection<CalculationResult> IDictionary<string, CalculationResult>.Values
		{
			get { return _dictionary.Values; }
		}

		#endregion

		#region ICollection<KeyValuePair<string,CalculationResult>> Members

		void ICollection<KeyValuePair<string, CalculationResult>>.Add(KeyValuePair<string, CalculationResult> item)
		{
			throw new NotSupportedException(SRUtil.GetString("LE_NotSupported_1"));
		}

		void ICollection<KeyValuePair<string, CalculationResult>>.Clear()
		{
			throw new NotSupportedException(SRUtil.GetString("LE_NotSupported_1"));
		}

		bool ICollection<KeyValuePair<string, CalculationResult>>.Contains(KeyValuePair<string, CalculationResult> item)
		{
			return _dictionary.Contains(item);
		}

		void ICollection<KeyValuePair<string, CalculationResult>>.CopyTo(KeyValuePair<string, CalculationResult>[] array, int arrayIndex)
		{
			((ICollection<KeyValuePair<string, CalculationResult>>)_dictionary).CopyTo(array, arrayIndex);
		}

		int ICollection<KeyValuePair<string, CalculationResult>>.Count
		{
			get { return _dictionary.Count; }
		}

		bool ICollection<KeyValuePair<string, CalculationResult>>.IsReadOnly
		{
			get { return true; }
		}

		bool ICollection<KeyValuePair<string, CalculationResult>>.Remove(KeyValuePair<string, CalculationResult> item)
		{
			throw new NotSupportedException(SRUtil.GetString("LE_NotSupported_1"));
		}

		#endregion

		#region IEnumerable<KeyValuePair<string,CalculationResult>> Members

		IEnumerator<KeyValuePair<string, CalculationResult>> IEnumerable<KeyValuePair<string, CalculationResult>>.GetEnumerator()
		{
			return _dictionary.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return _dictionary.GetEnumerator();
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