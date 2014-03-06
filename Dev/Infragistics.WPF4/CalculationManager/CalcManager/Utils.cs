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



using Infragistics.Windows.Licensing;


using Infragistics.Calculations;
using Infragistics.Calculations.Engine;

namespace Infragistics.Calculations
{
	internal class Utils
	{
		#region AddFormulaHelper

		/// <summary>
		/// Adds the specified formula to the specified calc manager. If the formula has syntax error
		/// then it does not add it.
		/// </summary>
		/// <param name="calcManager"></param>
		/// <param name="formula"></param>
		/// <returns></returns>
		internal static bool AddFormulaHelper( ICalculationManager calcManager, ICalculationFormula formula )
		{
			// The calc-engine does not handle call to RemoveFormula properly if the formula has syntax errors, even if the
			// formula was never added to it, which is what we have been doing, not adding any formulas with syntax errors.
			//
			if ( !formula.HasSyntaxError )
			{
				calcManager.AddFormula( formula );
				return true;
			}

			return false;
		}

		#endregion // AddFormulaHelper

		#region AddListenerHelper

		// This is used by the unit tests because they don't have access to the ISupportPropertyChangeNotifications,
		// which is defined in windows.
		// 
		internal static void AddAttachedPropertyListenerHelper( Action<object, object, string, object> handler, object owner )
		{
			var listener = new PropertyChangeListener<object>( owner, handler );

			XamCalculationManager.AttachedPropertyListeners.Add( listener, false );
		} 

		#endregion // AddListenerHelper

		#region AreSameTypeAndEqual

		/// <summary>
		/// Returns true if x and y are the same type anre are equal.
		/// </summary>
		public static bool AreSameTypeAndEqual( object x, object y )
		{
			return x == y
				|| null != x && null != y && x.GetType( ) == y.GetType( ) && object.Equals( x, y );
		}

		#endregion // AreSameTypeAndEqual

		#region ConvertMillisecondsToTicks

		/// <summary>
		/// Converts milliseconds to ticks.
		/// </summary>
		/// <param name="millis">Must be non-negative.</param>
		/// <returns></returns>
		internal static long ConvertMillisecondsToTicks( long millis )
		{
			CoreUtilities.ValidateIsNotNegative( millis, "millis" );

			// Conditionals below exclude case where 'millis' equals -1, which
			// ordinarily would get assigned to 'ticks' directly w/o conversion.
			long ticks = -1;

			try
			{
				// Convert millis into ticks (c.f., DateTime.Ticks).
				ticks = millis * TimeSpan.TicksPerMillisecond;
			}
			// Verify 'millis' is not above the acceptable range (i.e., its
			// conversion into 100 nanosecond ticks exceeds Int64.MaxValue).
			catch ( System.OverflowException )
			{
				throw new ArgumentOutOfRangeException( SRUtil.GetString( "LE_Exception_6" ), SRUtil.GetString( "LE_Exception_33" ) );
			}

			return ticks;
		}

		#endregion // ConvertMillisecondsToTicks

		#region ConvertValueHelper

		internal static CalculationValue ConvertValueHelper(object val, CultureInfo culture, IValueConverter converter, object converterParameter, Type valueType, Type treatAsType)
		{
			bool isConvertedValue = false;

			if (culture == null)
				culture = CultureInfo.CurrentCulture;

			// If a converter was specified on the control calc settings then use that.
			// 
			if (null != converter)
			{
				object convertedValue = converter.Convert(val, valueType, converterParameter, culture); 
				
				isConvertedValue = true;

				if (convertedValue == Binding.DoNothing)
				{
					convertedValue = val;
					isConvertedValue = false;
				}

				val = convertedValue;
			}
			// Otherwise if TreatAsType was specified then convert the control's value
			// tot that type using control's language.
			// 
			else if (null != treatAsType)
			{
				val = CoreUtilities.ConvertDataValue(val, treatAsType, culture, null);
				isConvertedValue = true;
			}

			// If either ValueConverter or TreatAsType were explicitly specified then use the above converted 
			// value which we converted using the control's language above. If neither of these settings were 
			// specified then we don't know what context the value is going to be used in formula (whether for 
			// string functions or double functions etc...) and therefore we need to store the control culture 
			// along with the value inside a calc value so when a calc function tries to convert the calc 
			// value to a specific type we can use the control culture for conversion. We'll only do this for 
			// string values as they are the ones that will need to be parsed into let's say double using the 
			// control's culture.
			// 
			if (!isConvertedValue && null != culture && val is string)
				val = new FormattedValue(val, culture);

			return Utils.ToCalcValue(val);
		}

		#endregion //ConvertValueHelper	
    
		#region GetParticipantRootNameHelper

		internal static string GetParticipantRootNameHelper( ICalculationParticipant participant )
		{
			CoreUtilities.ValidateNotNull( participant, "participant" );

			ICalculationReference rootReference = participant.RootReference;

			CoreUtilities.ValidateNotNull( rootReference, SRUtil.GetString( "LE_Exception_15" ) );

			RefParser refParser = new RefParser( rootReference.AbsoluteName );

			if ( !refParser.IsRoot )
				throw new ArgumentException( SRUtil.GetString( "LE_Exception_16" ), SRUtil.GetString( "LE_Exception_11" ) );

			return refParser.RootName;
		}

		#endregion // GetParticipantRootNameHelper

		#region GetRootName

		internal static string GetRootName( string reference )
		{
			RefParser parser = RefParser.Parse( reference );
			return parser.RootName;
		}

		#endregion // GetRootName

		#region IsRootReference

		internal static bool IsRootReference(string reference)
		{
			return reference != null && reference.StartsWith(RefParser.RefFullyQualifiedString);
		}

		#endregion // IsRootReference

		#region LogDebuggerError/Warning

		internal static void LogDebuggerError(string errorMsg)
		{
			LogDebuggerError("Global", errorMsg);
		}
		internal static void LogDebuggerError(string category, string errorMsg)
		{
			LogDebuggerErrorHelper(category, errorMsg, 50);
		}

		internal static void LogDebuggerWarning(string errorMsg)
		{
			LogDebuggerWarning("Global", errorMsg);
		}
		internal static void LogDebuggerWarning(string category, string errorMsg)
		{
			LogDebuggerErrorHelper(category, errorMsg, 40);
		}

		private static void LogDebuggerErrorHelper(string category, string errorMsg, int level)
		{
			if (Debugger.IsAttached && Debugger.IsLogging())
				Debugger.Log(level, category, errorMsg);
		}

		#endregion //LogDebuggerError	

		#region NormalizeElementName

		/// <summary>
		/// Returns the lowercase version of the element name.
		/// </summary>
		/// <param name="name">Element name.</param>
		/// <returns></returns>
		internal static string NormalizeElementName( string name )
		{
			return null != name ? name.ToLowerInvariant( ) : null;
		} 

		#endregion // NormalizeElementName

		#region RemoveFormulaHelper

		/// <summary>
		/// Removes the specified formula from the specified calc manager. If the formula has 
		/// syntax error then it takes no action as a formula with syntax error should never have
		/// been added to the calc network in the first place.
		/// </summary>
		/// <param name="calcManager"></param>
		/// <param name="formula"></param>
		internal static void RemoveFormulaHelper( ICalculationManager calcManager, ICalculationFormula formula )
		{
			// The calc-engine does not handle call to RemoveFormula properly if the formula has syntax errors, even if the
			// formula was never added to it, which is what we have been doing, not adding any formulas with syntax errors.
			//
			if ( !formula.HasSyntaxError )
				calcManager.RemoveFormula( formula );
		}

		#endregion // RemoveFormulaHelper

		#region ToCalcValue

		/// <summary>
		/// If 'val' is a calculation value then returns that instance otherwise wraps it inside a new CalculationValue object.
		/// </summary>
		/// <param name="val"></param>
		/// <returns></returns>
		internal static CalculationValue ToCalcValue( object val )
		{
			CalculationValue r = val as CalculationValue;
			if ( null == r )
				r = new CalculationValue( val );

			return r;
		}

		internal static CalculationValue ToCalcValue( CalculationErrorCode calcErrorCode )
		{
			return new CalculationValue( new CalculationErrorValue( calcErrorCode ) );
		}

		#endregion // ToCalcValue
	}


	#region FormattedValue Class

	/// <summary>
	/// Represents a value that will be converted using the culture specified in the constructor.
	/// </summary>
	internal class FormattedValue : IConvertible
	{
		#region Member Vars

		private IFormatProvider _formatProvider;
		private CalculationValue _calcValue;

		#endregion // Member Vars

		#region Constructor

		/// <summary>
		/// Constructor. Initializes a new instance of <see cref="FormattedValue"/>.
		/// </summary>
		/// <param name="val">Specifies the underlying value.</param>
		/// <param name="formatProvider">The format provider that was used to format the string value. When
		/// converting to other types, this is the format provider that will be used for parsing.</param>
		public FormattedValue( object val, IFormatProvider formatProvider )
		{
			_formatProvider = formatProvider;
			_calcValue = Utils.ToCalcValue( val );
		}

		#endregion // Constructor

		#region Base Overrides

		#region Equals

		public override bool Equals( object val )
		{
			var fv = val as FormattedValue;
			if ( null != fv )
				val = fv._calcValue;

			return _calcValue.Equals( val );
		}

		#endregion // Equals

		#region GetHashCode

		public override int GetHashCode( )
		{
			return _calcValue.GetHashCode( );
		}

		#endregion // GetHashCode

		#region ToString

		public override string ToString( )
		{
			return _calcValue.ToString( _formatProvider );
		}

		#endregion // ToString

		#endregion // Base Overrides

		#region IConvertible Implementation

		TypeCode IConvertible.GetTypeCode( )
		{
			return _calcValue.GetTypeCode( );
		}

		public bool ToBoolean( IFormatProvider provider )
		{
			return _calcValue.ToBoolean( _formatProvider );
		}

		public byte ToByte( IFormatProvider provider )
		{
			return _calcValue.ToByte( _formatProvider );
		}

		public char ToChar( IFormatProvider provider )
		{
			return _calcValue.ToChar( _formatProvider );
		}

		public DateTime ToDateTime( IFormatProvider provider )
		{
			return _calcValue.ToDateTime( _formatProvider );
		}

		public decimal ToDecimal( IFormatProvider provider )
		{
			return _calcValue.ToDecimal( _formatProvider );
		}

		public double ToDouble( IFormatProvider provider )
		{
			return _calcValue.ToDouble( _formatProvider );
		}

		public short ToInt16( IFormatProvider provider )
		{
			return _calcValue.ToInt16( _formatProvider );
		}

		public int ToInt32( IFormatProvider provider )
		{
			return _calcValue.ToInt32( _formatProvider );
		}

		public long ToInt64( IFormatProvider provider )
		{
			return _calcValue.ToInt64( _formatProvider );
		}

		public sbyte ToSByte( IFormatProvider provider )
		{
			return ( (IConvertible)_calcValue ).ToSByte( _formatProvider );
		}

		public float ToSingle( IFormatProvider provider )
		{
			return ( (IConvertible)_calcValue ).ToSingle( _formatProvider );
		}

		public string ToString( IFormatProvider provider )
		{
			return _calcValue.ToString( _formatProvider );
		}

		public object ToType( Type conversionType, IFormatProvider provider )
		{
			return ( (IConvertible)_calcValue ).ToType( conversionType, _formatProvider );
		}

		public ushort ToUInt16( IFormatProvider provider )
		{
			return ( (IConvertible)_calcValue ).ToUInt16( _formatProvider );
		}

		public uint ToUInt32( IFormatProvider provider )
		{
			return ( (IConvertible)_calcValue ).ToUInt32( _formatProvider );
		}

		public ulong ToUInt64( IFormatProvider provider )
		{
			return ( (IConvertible)_calcValue ).ToUInt64( _formatProvider );
		}

		#endregion // IConvertible Implementation
	} 

	#endregion // FormattedValue Class
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