using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infragistics.Calculations.Engine;
using System.Reflection;
using System.Windows.Data;
using System.Globalization;
using System.Windows;
using System.ComponentModel;
using Infragistics.Windows.Internal;

namespace Infragistics.Calculations
{
	internal class ItemPropertyReference : FormulaRefBase
	{
		#region Private Members

		private ItemCalculatorReferenceBase _root;
		private object _item;
		private IItemPropertyValueAccessor _valueAccessor;
		private string _referenceId;
		private string _elementName;
		private bool _inValueSet;
		private CalculationValue _value;
		private PropertyValueTracker _tracker;

		#endregion //Private Members	
    
		#region Constructor

		internal ItemPropertyReference(ItemCalculatorReferenceBase root, string referenceId, object item, IItemPropertyValueAccessor valueAccessor, bool trackValueChange)
			: base(root.Calculator.CalculationManager)
		{
			_root = root;
			_item = item;
			_valueAccessor = valueAccessor;
			_referenceId = referenceId;

			if (trackValueChange &&
				_valueAccessor != null &&
				item is DependencyObject &&
				!(item is INotifyPropertyChanged))
			{
				_tracker = new PropertyValueTracker(_item, _valueAccessor.Name, new PropertyValueTracker.PropertyValueChangedHandler(this.OnPropertyChanged));
			}
		}

		#endregion //Constructor
 
		#region Properties

		#region Item

		internal object Item { get { return _item; } }

		#endregion //Item	

		#region ValueAccessor

		internal IItemPropertyValueAccessor ValueAccessor { get { return _valueAccessor; } }

		#endregion //ValueAccessor

		#region ReferenceId

		internal string ReferenceId { get { return _referenceId; } }

		#endregion //ReferenceId

		#endregion //Properties

		#region Methods

		#region Internal Methods

		#region OnPropertyChanged

		internal void OnPropertyChanged()
		{
			// check the flag before dirtying the value
			if (false == _inValueSet)
			{
				_value = null;

				this._calcManager.InternalNotifyValueChanged(this);
			}
		}

		#endregion //OnPropertyChanged	
    
		#region SetResultOnCalculator

		internal void SetResultOnCalculator(CalculationValue value)
		{
			if (this.Formula != null)
			{
				_root.Calculator.SetResult(value, _referenceId, this);
			}
		}

		#endregion //SetResultOnCalculator	

		#endregion //Internal Methods
    
		#endregion //Methods

		#region Base class overrides

		#region Properties

		#region AbsoluteName

		public override string AbsoluteName
		{
			get
			{
				return _root.AbsoluteName + RefParser.RefSeperatorString + this.ElementName;
			}
		}

		#endregion AbsoluteName

		#region BaseParent

		public override Engine.RefBase BaseParent
		{
			get
			{
				return _root;
			}
		}

		#endregion //BaseParent

		#region ElementName

		public override string ElementName
		{
			get
			{
				if (_elementName == null)
					_elementName = RefParser.EscapeString(_referenceId, false);

				return _elementName;
			}
		}

		#endregion //ElementName	

		#region IsDataReference

		public override bool IsDataReference
		{
			get
			{
				return true;
			}
		}

		#endregion //IsDataReference	
 
		#region Value

		public override CalculationValue Value
		{
			get
			{
				// If there is a syntax error then return that
				if (this.FormulaSyntaxErrorValue != null)
					return this.FormulaSyntaxErrorValue;

				if (_valueAccessor == null)
				{
					if (_value != null)
						return _value;

					return base.Value;
				}

				if (_value == null)
				{
					try
					{
						_value = this.GetCalculationValueCore();
					}
					catch (Exception e)
					{
						string errorMsg = string.Format(SRUtil.GetString("PropertyGetFailed"), _valueAccessor.Name, _item);

						DataAccessErrorEventArgs args = new DataAccessErrorEventArgs(_item, _valueAccessor.Name, errorMsg, false, null, e);

						// Raise the DataAccessError event on the calculator
						_root.Calculator.RaiseDataAccessErrorEvent(args);

						_value = new CalculationValue( new CalculationErrorValue(CalculationErrorCode.Value, args.ErrorMessage));
					}
				}

				return _value;
			}
			set
			{

				if ( _value != value )
				{
					FormulaCalculationErrorEventArgs errorArgs = null;

					if ( value != null )
					{
						CalculationErrorValue errorValue = value.Value as CalculationErrorValue;
						if ( errorValue != null )
							errorArgs = _calcManager.RaiseFormulaError( this, errorValue, null );
					}

					_value = value;

					// update the calculator with the result
					this.SetResultOnCalculator(_value);

					if (errorArgs == null && _valueAccessor != null)
					{
						object valueToSet = value.Value;

						try
						{
							IValueConverter converter = _root.Calculator.ValueConverter;
							object convertedValue;

							if (converter != null)
							{
								convertedValue = converter.ConvertBack(valueToSet, _valueAccessor.PropertyType, _valueAccessor.Name, _root.Calculator.CultureResolved);


								if (convertedValue == Binding.DoNothing)
									convertedValue = valueToSet;


								valueToSet = convertedValue;
							}

							if (valueToSet != null &&
								!_valueAccessor.PropertyType.IsAssignableFrom(valueToSet.GetType()))
							{
								// SSP 10/12/11 TFS88251 TFS91512
								// Since type converters do not exist in SL, in order to get consistent behavior, pass false for the
								// useTypeConverters parameter.
								// 
								//convertedValue = CoreUtilities.ConvertDataValue(valueToSet, _valueAccessor.PropertyType, _root.Calculator.CultureResolved, null);
								convertedValue = CoreUtilities.ConvertDataValue( valueToSet, _valueAccessor.PropertyType, _root.Calculator.CultureResolved, null, false );

								if (convertedValue != null)
									valueToSet = convertedValue;
							}


							// set a flag so we know to ignore property change notifications during the call
							// to SetValue below
							_inValueSet = true;

							_valueAccessor.SetValue(_item, valueToSet);
							
							// reset the flag
							_inValueSet = false;
						}
						catch (Exception e)
						{
							// reset the flag before raising the event
							_inValueSet = false;

							string errorMsg = string.Format(SRUtil.GetString("PropertySetFailed"), _valueAccessor.Name, _item, valueToSet);

							DataAccessErrorEventArgs args = new DataAccessErrorEventArgs(_item, _valueAccessor.Name, errorMsg, true, valueToSet, e);

							// Raise the DataAccessError event on the calculator
							_root.Calculator.RaiseDataAccessErrorEvent(args);
						}
					}
				}
			}
		}

		#endregion //Value	
    
		#endregion // Properties

		#region Methods

		#region Protected Methods

		#region GetCalculationValueCore

		protected virtual CalculationValue GetCalculationValueCore()
		{
			object valueToRtn = _valueAccessor.GetValue(this.Item);

			IValueConverter converter = _root.Calculator.ValueConverter;

			return Utils.ConvertValueHelper(valueToRtn, null, converter, _valueAccessor.Name, _valueAccessor.PropertyType, null);
		}

		#endregion //GetCalculationValueCore

		#endregion //Protected Methods	
            
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