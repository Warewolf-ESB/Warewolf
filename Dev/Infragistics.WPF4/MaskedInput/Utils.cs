using System;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Controls;
using Infragistics.Collections;
using Infragistics.Controls;
using Infragistics.Controls.Primitives;
using System.Globalization;
using System.Collections.ObjectModel;
using System.Windows.Controls.Primitives;
using System.Text.RegularExpressions;
using System.Windows.Markup;

namespace Infragistics.Controls.Editors.Primitives
{

	#region Utils Class

	/// <summary>
	/// A class with helper routines.
	/// </summary>
	internal static class Utils
	{
		#region Nested Data Structures

		#region Range Class

		// SSP 3/23/09 IME
		// Added Range class.
		// 
		internal class Range
		{
			private int _start;
			private int _length;

			internal Range( int start, int length )
			{
				_start = start;
				_length = length;
			}

			internal int Start
			{
				get
				{
					return _start;
				}
				set
				{
					_start = value;
				}
			}

			internal int Length
			{
				get
				{
					return _length;
				}
				set
				{
					_length = value;
				}
			}
		}

		#endregion // Range Class

		#endregion // Nested Data Structures

		#region AreEqual

		internal static bool AreEqual( object x, object y )
		{
			return x == y 
				|| null != x && null != y && x.GetType( ) == y.GetType( ) && object.Equals( x, y );
		}

		#endregion // AreEqual

		#region CompareStrings

		internal static int CompareStrings( string x, string y, bool ignoreCase, CultureInfo culture = null )
		{
			return string.Compare( x, y, culture ?? CultureInfo.CurrentCulture, ignoreCase ? CompareOptions.IgnoreCase : CompareOptions.None );
		} 

		#endregion // CompareStrings

		#region Exists

		/// <summary>
		/// Returns true if the item exists in the specified enumerable.
		/// </summary>
		/// <param name="e"></param>
		/// <param name="item"></param>
		/// <returns></returns>
		public static bool Exists( IEnumerable e, object item )
		{
			foreach ( object i in e )
			{
				if ( object.Equals( item, i ) )
					return true;
			}

			return false;
		}

		#endregion // Exists

		#region GetAncestorFromName

		internal static FrameworkElement GetAncestorFromName( DependencyObject elem, string name, FrameworkElement ceiling = null )
		{
			return PresentationUtilities.GetVisualAncestor<FrameworkElement>( elem, ii => null != ii && ii.Name == name, ceiling );
		}

		#endregion // GetAncestorFromName

		#region GetClipboardText

		internal static string GetClipboardText( )
		{

			return Infragistics.Windows.Helpers.ClipboardHelper.GetText( );
			


#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

		} 

		#endregion // GetClipboardText

		#region GetCultureInfo

		internal static CultureInfo GetCultureInfo( FrameworkElement elem )
		{
			return CultureInfo.CurrentCulture;
		}

		#endregion // GetCultureInfo

		#region GetDescendantFromName

		internal static FrameworkElement GetDescendantFromName( DependencyObject elem, string name )
		{
			return PresentationUtilities.GetVisualDescendant<FrameworkElement>( elem, ii => null != ii && ii.Name == name );
		}

		#endregion // GetDescendantFromName

		#region GetString
		internal static string GetString( string name )
		{
			return GetString( name, null );
		}

		internal static string GetString( string name, params object[] args )
		{
#pragma warning disable 436
			return SR.GetString( name, args );
#pragma warning restore 436
		}
		#endregion // GetString

		#region GetTextToValueConversionError

		// SSP 4/21/09 NAS9.2 IDataErrorInfo Support
		// 
		/// <summary>
		/// Gets an error message to display when user input fails to be converted to the specified
		/// target type.
		/// </summary>
		/// <param name="targetType"></param>
		/// <param name="text"></param>
		/// <returns></returns>
		internal static Exception GetTextToValueConversionError( Type targetType, string text )
		{
			string errorMessage;

			bool isTextEmpty = string.IsNullOrEmpty( text ) || 0 == text.Trim( ).Length;

			if ( typeof( DateTime ) == targetType )
				errorMessage = GetString( isTextEmpty ? "LMSG_EnteredDateEmpty" : "LMSG_EnteredDateInvalid", text );
			else if ( Utils.IsNumericType( targetType ) )
				errorMessage = GetString( isTextEmpty ? "LMSG_EnteredNumberEmpty" : "LMSG_EnteredNumberInvalid", text );
			else
				errorMessage = GetString( isTextEmpty ? "LMSG_EnteredValueEmpty" : "LMSG_EnteredValueInvalid", text );

			return new Exception( errorMessage );
		}

		#endregion // GetTextToValueConversionError

		#region HitTest

		// SSP 10/14/09 - NAS10.1 Spin Buttons
		// 
		//internal static DependencyObject HitTest( UIElement elem, Point location, Type elementType, bool allowSubclassOfType )
		//{
		//    HitTestResult hr = VisualTreeHelper.HitTest( elem, location );
		//    if ( null != hr && null != hr.VisualHit )
		//        return Utilities.GetAncestorFromType( hr.VisualHit, elementType, allowSubclassOfType, elem );

		//    return null;
		//}

		#endregion // HitTest

		#region IsFocusable

		internal static bool IsFocusable( ValueInput editor )
		{
			return XamlHelper.GetFocusable( editor );
		} 

		#endregion // IsFocusable

		#region IsInfinityOrNaN

		// SSP 3/16/09 TFS6232
		// 
		/// <summary>
		/// Returns true if the specified val is double or float and infinity or NaN.
		/// </summary>
		/// <param name="val"></param>
		/// <returns></returns>
		internal static bool IsInfinityOrNaN( object val )
		{
			if ( val is double )
			{
				double d = (double)val;
				return double.IsNaN( d ) || double.IsInfinity( d );
			}
			else if ( val is float )
			{
				float f = (float)val;
				return float.IsNaN( f ) || float.IsInfinity( f );
			}
			else
				return false;
		}

		#endregion // IsInfinityOrNaN

		#region IsMouseOverButton

		// SSP 10/2/09 - NAS10.1 Spin Buttons
		// 
		internal static bool IsMouseOverButton( ValueInput editor, MouseEventArgs e )
		{
			DependencyObject elem = e.OriginalSource as DependencyObject;
			if ( null != elem )
			{
				ButtonBase button = PresentationUtilities.GetVisualAncestor<ButtonBase>( elem, null, editor );
				if ( null != button )
					return true;

				// SSP 8/16/10 TFS28574
				// If the command is not executable then the buttons get disabled and their hit test visibility
				// becomes transparent. In which case we'll get the panel containing the spin buttons which is 
				// named PART_SpinButtons.
				// 
				if ( null != GetAncestorFromName( elem, "PART_SpinButtons" ) )
					return true;
			}

			return false;
		}

		#endregion // IsMouseOverButton

		#region IsMouseOverElement

		/// <summary>
		/// Returns a value indicating whether the mouse position associated with the specified 
		/// mouse event args is inside the specified element.
		/// </summary>
		/// <param name="element"></param>
		/// <param name="e"></param>
		/// <returns></returns>
		public static bool IsMouseOverElement( DependencyObject element, MouseEventArgs e )
		{
			FrameworkElement fe = element as FrameworkElement;

			if ( null != fe )
			{
				Point mousePos = e.GetPosition( fe );

				Rect rect = new Rect( 0, 0, fe.ActualWidth, fe.ActualHeight );
				if ( rect.Contains( mousePos ) )
					return true;
			}

			return false;
		}

		#endregion // IsMouseOverElement

		#region IsNumericType

		/// <summary>
		/// Determines if a given System.Type is a numeric type.
		/// </summary>
		/// <param name="type">The System.Type to test.</param>
		/// <returns>True if the type is a numeric type.</returns>
		public static bool IsNumericType( System.Type type )
		{
			if ( type.IsPrimitive || type == typeof( decimal ) )
			{
				if ( type != typeof( bool ) && type != typeof( char ) )
					return true;
				else
					return false;
			}
			return false;
		}

		#endregion // IsNumericType

		#region IsValueEmpty

		// SSP 1/7/08 BR29457
		// 
		/// <summary>
		/// Returns true if the specified val is null, DBNull, empty string, or DependencyProperty.UnsetValue.
		/// </summary>
		/// <param name="val">Value to test</param>
		public static bool IsValueEmpty( object val )
		{
			return null == val
				|| DBNull.Value == val
				|| string.Empty == val as string
				|| DependencyProperty.UnsetValue == val;
		}

		#endregion // IsValueEmpty

		#region IsValuePropertySet

		// SSP 1/7/08 BR29457
		// 
		internal static bool IsValuePropertySet( DependencyProperty prop, DependencyObject obj )
		{
			return DependencyPropertyUtilities.ShouldSerialize( obj, prop );
		}

		#endregion // IsValuePropertySet

		#region SetClipboardText

		internal static bool SetClipboardText( string text )
		{

			return Infragistics.Windows.Helpers.ClipboardHelper.SetText( text );


#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

		} 

		#endregion // SetClipboardText

		#region ShowMessageBox

		internal static void ShowMessageBox( DependencyObject owner, string message, string title, MessageBoxButton buttons

				, MessageBoxImage messageBoxImage 

			)
		{

			MessageBox.Show( message, title, buttons

				, messageBoxImage

			);
		}

		#endregion // ShowMessageBox

		#region ToTextAlignment

		internal static TextAlignment ToTextAlignment( HorizontalAlignment horizAlign )
		{
			switch ( horizAlign )
			{
				case HorizontalAlignment.Center:
					return TextAlignment.Center;
				case HorizontalAlignment.Right:
					return TextAlignment.Right;
				case HorizontalAlignment.Stretch:
					return TextAlignment.Justify;
				case HorizontalAlignment.Left:
				default:
					return TextAlignment.Left;
			}
		}

		#endregion // ToTextAlignment

		#region ValidateEnum

		private static bool IsEnumFlags( Type enumType )
		{
			object[] flagsAttributes = enumType.GetCustomAttributes( typeof( FlagsAttribute ), true );
			return null != flagsAttributes && flagsAttributes.Length > 0;
		}



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal static void ValidateEnum( string argumentName, Type enumType, object enumVal )
		{
			if ( !Enum.IsDefined( enumType, enumVal ) && !IsEnumFlags( enumType ) )

				throw new InvalidEnumArgumentException( argumentName, (int)enumVal, enumType );



		}

		#endregion // ValidateEnum

		#region ValidateNull



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal static void ValidateNull( string paramName, object val )
		{
			if ( null == val )
				throw new ArgumentNullException( paramName );
		}







		internal static void ValidateNull( object val )
		{
			if ( null == val )
				throw new ArgumentNullException( );
		}

		#endregion // ValidateNull
	}

	#endregion // Utils Class

	#region IRaiseEventDefinition Interface

	internal interface IRaiseEventDefinition
	{
		void Raise( );
	}

	#endregion // IRaiseEventDefinition Interface

	#region RaiseEventDefinition Class

	internal class RaiseEventDefinition<TEventArgs> : IRaiseEventDefinition
		where TEventArgs : EventArgs
	{
		private object _sender;
		private EventHandler<TEventArgs> _handlerList;
		private TEventArgs _args;

		public RaiseEventDefinition( object sender, EventHandler<TEventArgs> handlerList, TEventArgs args )
		{
			Utils.ValidateNull( sender );
			Utils.ValidateNull( args );

			_sender = sender;
			_handlerList = handlerList;
			_args = args;
		}

		public void Raise( )
		{
			if ( null != _handlerList )
				_handlerList( _sender, _args );
		}
	}

	#endregion // RaiseEventDefinition Class

	#region ValidatedObservableCollection<T> Class

	// SSP 4/21/09 NAS9.2 IDataErrorInfo Support
	// 
	/// <summary>
	/// An observable collection that allows you to specify a delegate for validating items being
	/// added to the collection.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	internal class ValidatedObservableCollection<T> : ObservableCollection<T>
	{
		public delegate void ValidateCallback( T item );
		private ValidateCallback _validationCallback;
		private bool _allowNull;		

		public ValidatedObservableCollection( bool allowNull, ValidateCallback validationCallback )
		{
			_allowNull = allowNull;
			_validationCallback = validationCallback;
		}

		private void ValidateItem( T item )
		{
			if ( !_allowNull && null == item )
				throw new ArgumentNullException( "item" );

			if ( null != _validationCallback )
				_validationCallback( item );
		}

		protected override void InsertItem( int index, T item )
		{
			this.ValidateItem( item );

			base.InsertItem( index, item );
		}

		protected override void SetItem( int index, T item )
		{
			this.ValidateItem( item );

			base.SetItem( index, item );
		}
	}

	#endregion // ValidatedObservableCollection<T> Class

	#region HorizontalToTextAlignmentConverter Class

	// SSP 3/23/09 IME
	// Added HorizontalToTextAlignmentConverter.
	// 
	/// <summary>
	/// A converter that converts HorizontalAlignment to TextAlignment.
	/// </summary>
	public class HorizontalToTextAlignmentConverter : IValueConverter
	{
		#region IValueConverter Members

		object IValueConverter.Convert( object value, Type targetType, object parameter, CultureInfo culture )
		{
			if ( value is HorizontalAlignment )
			{
				switch ( (HorizontalAlignment)value )
				{
					case HorizontalAlignment.Center:
						return TextAlignment.Center;
					case HorizontalAlignment.Right:
						return TextAlignment.Right;
				}
			}

			return TextAlignment.Left;
		}

		object IValueConverter.ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
		{
			if ( value is TextAlignment )
			{
				switch ( (TextAlignment)value )
				{
					case TextAlignment.Center:
						return HorizontalAlignment.Center;
					case TextAlignment.Right:
						return HorizontalAlignment.Right;
				}
			}

			return HorizontalAlignment.Left;
		}

		#endregion
	}

	#endregion // HorizontalToTextAlignmentConverter Class

	#region MaskCharConverter

	// SSP 9/16/11 TFS84979
	// 
	/// <summary>
	/// Used as TypeConverter attribute on PadChar and PromptChar properties of the XamMaskedInput.
	/// </summary>
	public class MaskCharConverter : TypeConverter
	{
		/// <summary>
		/// Returns true if the source type is string. False otherwise.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="sourceType"></param>
		/// <returns></returns>
		public override bool CanConvertFrom( ITypeDescriptorContext context, Type sourceType )
		{
			return typeof( string ) == sourceType;
		}

		/// <summary>
		/// Returns true if destination type is char. False otherwise.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="destinationType"></param>
		/// <returns></returns>
		public override bool CanConvertTo( ITypeDescriptorContext context, Type destinationType )
		{
			return typeof( char ) == destinationType;
		}

		/// <summary>
		/// Returns the first character of the value which must be a string. If length is greater than 1,
		/// raises an exception. If length is 0, returns character of value 0.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="culture"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public override object ConvertFrom( ITypeDescriptorContext context, CultureInfo culture, object value )
		{
			string s = (string)value;
			if ( !string.IsNullOrEmpty( s ) )
				return Convert.ToChar( s );

			return (char)0;
		}


		/// <summary>
		/// Converts value which must be a character to string. If character of value 0 is specified, returns empty string.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="culture"></param>
		/// <param name="value"></param>
		/// <param name="destinationType"></param>
		/// <returns></returns>
		public override object ConvertTo( ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType )
		{
			char c = (char)value;
			return 0 == c ? string.Empty : Convert.ToString( c );
		}
	} 

	#endregion // MaskCharConverter

	#region SpinInfo Class
	
	// SSP 10/8/09 - NAS10.1 Spin Buttons
	// 
	internal class SpinInfo
	{
		#region IncrementType Enum

		private enum IncrementType
		{
			Day,
			Month,
			Year,
			Log10,
			TimeSpan,
			NumbericAmount,
            // SSP 5/13/10 TFS32082
            // 
            Hour,
            Minute,
            Second,
            // Month or minute based on whether the MaskedInput's mask has 
            // date sections or just the time sections.
            // 
            MonthOrMinute
		}

		#endregion // IncrementType Enum

		#region Member Vars

		private TimeSpan _incrementAmountTimeSpan;
		private decimal _incrementAmountNumeric;
		private IncrementType _incrementType;
		private ValueInput _valueInput;
		private object _origSpinIncrement;

		private int _acceleration_SpinCount;
		private DateTime? _lastSpinTime;

		private IFormatProvider _cachedCalendar_lastFormatProvider;
        private System.Globalization.Calendar _cachedCalendar;

		#endregion // Member Vars

		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		private SpinInfo( )
		{
		}

		#endregion // Constructor

		#region Methods

		#region CanSpin

		public bool CanSpin( bool up )
		{
			object newValue;
			return this.SpinHelper( up, out newValue );
		}

		#endregion // CanSpin

		#region CheckElapsedTimeHelper

		internal void CheckElapsedTimeHelper( )
		{
			DateTime currentTime = DateTime.Now;
			if ( _lastSpinTime.HasValue )
			{
				DateTime lastTime = _lastSpinTime.Value;
				TimeSpan span = currentTime - lastTime;
				if ( span.TotalMilliseconds > 500 )
					this.ResetAcceleration( );
			}

			_lastSpinTime = currentTime;
		}

		#endregion // CheckElapsedTimeHelper

		#region ConvertDataValue

		private object ConvertDataValue( object val, Type type )
		{
			return CoreUtilities.ConvertDataValue( val, type, _valueInput.FormatProvider, null );
		}

		#endregion // ConvertDataValue

		#region ConvertToDecimal

		private object ConvertToDecimal( object val )
		{
			if ( null != val && val.GetType( ) != typeof( DateTime ) )
				return this.ConvertDataValue( val, typeof( decimal ) );

			return null;
		}

		#endregion // ConvertToDecimal

        #region ConvertToEditorValueType

        // SSP 5/14/10 TFS32082
        // 
        private object ConvertToEditorValueType( object value )
        {
            Type editorValueType = _valueInput.ValueTypeResolved;
			XamMaskedInput maskedInput = _valueInput as XamMaskedInput;
			if ( null != maskedInput )
			{
				// If the value is DateTime but the editor's ValueType is string then DateTime.ToString( )
				// may not result in the text that matches the mask (for example mask can contain just the time
				// sections). Therefore we have to apply the date to mask and get the text from the sections.
				// 
				if ( value is DateTime && typeof( string ) == maskedInput.ValueTypeResolved )
					return MaskedInputDefaultConverter.ValueToTextConverter.ConvertToTextBasedOnMask( maskedInput, value, true );
			}

            return this.ConvertDataValue( value, editorValueType );
        }

        #endregion // ConvertToEditorValueType

		#region GetCalendar

        private System.Globalization.Calendar GetCalendar()
		{
			IFormatProvider formatProvider = _valueInput.FormatProviderResolved;
			if ( null == _cachedCalendar || formatProvider != _cachedCalendar_lastFormatProvider )
			{
				_cachedCalendar_lastFormatProvider = formatProvider;
				_cachedCalendar = XamMaskedInput.GetCultureCalendar( formatProvider );
			}

			return _cachedCalendar;
		}

		#endregion // GetCalendar

		#region GetCurrentValueAsDate

		private bool GetCurrentValueAsDate( out DateTime val )
		{
			object convertedVal = this.ConvertDataValue( _valueInput.Value, typeof( DateTime ) );
			if ( convertedVal is DateTime )
			{
				val = (DateTime)convertedVal;
				return true;
			}

			val = DateTime.MinValue;
			return false;
		}

		#endregion // GetCurrentValueAsDate

		#region GetDefaultMinMaxDate

		internal static DateTime GetDefaultMinMaxDate( bool min )
		{
			return CalendarHelper.GetMinMax( CultureInfo.CurrentCulture.Calendar, min );
		}

		#endregion // GetDefaultMinMaxDate

		#region GetResolvedMinMax

		private void GetResolvedMinMax( out object min, out object max, ref Type valueType )
		{
			min = max = null;

			ValueConstraint vc = _valueInput.ValueConstraint;
			if ( null != vc )
			{
				min = vc.MinInclusive;
				max = vc.MaxExclusive;
			}

			XamMaskedInput maskedInput = _valueInput as XamMaskedInput;
			if ( null != maskedInput )
			{
				if ( null == valueType || typeof( object ) == valueType || typeof( string ) == valueType )
				{
					Type deduceFromMaskType = XamMaskedInput.DeduceEditAsType( maskedInput.Sections );
					if ( null != deduceFromMaskType )
						valueType = deduceFromMaskType;
				}
			}

			Type valueTypeUnderlying = CoreUtilities.GetUnderlyingType( valueType );

			if ( Utils.IsNumericType( valueTypeUnderlying ) )
			{
				if ( null != maskedInput )
				{
					NumberSection numberSection = null != maskedInput.Sections
						? (NumberSection)XamMaskedInput.GetSection( maskedInput.Sections, typeof( NumberSection ) )
						: null;

					// JM 06-29-10 TFS33414 Check for null.
					if (numberSection != null)
					{
						// SSP 4/6/12 TFS95799
						// 
						//min = numberSection.MinValue;
						//max = numberSection.MaxValue;
						numberSection.VerifyMinMaxCache( true );
						min = numberSection.lastConvertedMinValWithFractionPart;
						max = numberSection.lastConvertedMaxValWithFractionPart;
					}
				}
			}
			else if ( typeof( DateTime ) == valueTypeUnderlying )
			{
				if ( null == min )
					min = GetDefaultMinMaxDate( true );

				if ( null == max )
					max = GetDefaultMinMaxDate( false );
			}
		}

		#endregion // GetResolvedMinMax

		#region GetResolvedMinMaxAsDecimal

		private void GetResolvedMinMaxAsDecimal( out object min, out object max )
		{
			Type valueType = _valueInput.ValueTypeResolved;
			this.GetResolvedMinMax( out min, out max, ref valueType );

			min = this.ConvertToDecimal( min );
			max = this.ConvertToDecimal( max );
		}

		#endregion // GetResolvedMinMaxAsDecimal

		#region GetStartValue

		private object GetStartValue( bool up, bool wrapping )
		{
			Type valueType = CoreUtilities.GetUnderlyingType( _valueInput.ValueTypeResolved );

			object min, max;
			this.GetResolvedMinMax( out min, out max, ref valueType );

			if ( Utils.IsNumericType( valueType ) )
			{
				object dMinObj = null != min ? this.ConvertDataValue( min, typeof( decimal ) ) : null;
				object dMaxObj = null != max ? this.ConvertDataValue( max, typeof( decimal ) ) : null;

				if ( ! wrapping )
				{
					if ( ( null == dMinObj || dMinObj is decimal && (decimal)dMinObj < 0m )
						&& ( null == dMaxObj || dMaxObj is decimal && 0m < (decimal)dMaxObj ) )
					{
						// SSP 8/6/12 TFS118267
						// I noticed this while debugging TFS118267. We should return a value is the 
						// same type as the editor's ValueType.
						// 
						//return 0m;
						return this.ConvertToEditorValueType( 0m );						
					}
				}
			}
			else if ( typeof( DateTime ) == valueType )
			{
				object dateMinObj = this.ConvertDataValue( min, typeof( DateTime ) );
				object dateMaxObj = this.ConvertDataValue( max, typeof( DateTime ) );
				
				if ( ! wrapping )
				{
					DateTime startDate = DateTime.Now;

					if ( ( Utils.IsValueEmpty( dateMinObj ) || (DateTime)dateMinObj <= startDate )
						&& ( Utils.IsValueEmpty( dateMaxObj ) || startDate <= (DateTime)dateMaxObj ) )
						return startDate;
				}
			}

			// SSP 5/14/12 TFS111240
			// 
			// --------------------------------------------------------------------
			//return up ? min : max;
			object val = up ? min : max;
			if ( null != val && ! valueType.IsInstanceOfType( val ) )
			{
				object convertedVal = this.ConvertDataValue( val, valueType );
				if ( null != convertedVal )
					val = convertedVal;
			}

			return val;
			// --------------------------------------------------------------------
		}

		#endregion // GetStartValue

		#region IsIncrementTypeForDateTime

		// SSP 2/16/10 TFS27573
		// 
		private bool IsIncrementTypeForDateTime( )
		{
			switch ( _incrementType )
			{
				case IncrementType.Day:
				case IncrementType.Month:
				case IncrementType.Year:
				case IncrementType.TimeSpan:
                // SSP 5/13/10 TFS32082
                // Added the following four new members.
                // 
                case IncrementType.MonthOrMinute:
                case IncrementType.Hour:
                case IncrementType.Minute:
                case IncrementType.Second:
					return true;
			}

			return false;
		}

		#endregion // IsIncrementTypeForDateTime

		#region IsSameSpinIncrement

		internal bool IsSameSpinIncrement( object amount )
		{
			return Utils.AreEqual( amount, _origSpinIncrement );
		}

		#endregion // IsSameSpinIncrement
		
		#region IsValueValid

		private bool IsValueValid( object value )
		{
			Exception error;
			if ( !_valueInput.ValidateValue( value, out error ) )
				return false;

			// SSP 2/16/10 TFS27573
			// Enclosed the existing code in the if block. If we are working with date-time values
			// then don't bother validating against min or max decimal values.
			// 
			if ( ! this.IsIncrementTypeForDateTime( ) )
			{
				object val = this.ConvertToDecimal( value );
				if ( null != val )
				{
					object min, max;
					this.GetResolvedMinMaxAsDecimal( out min, out max );
					if ( null != min && (decimal)val < (decimal)min )
						return false;

					if ( null != max && (decimal)val > (decimal)max )
						return false;
				}
			}

			return true;
		}

		#endregion // IsValueValid

		#region Parse

		internal static SpinInfo Parse( ValueInput valueInput, object spinIncrement )
		{
			Utils.ValidateNull( valueInput );

			SpinInfo info = new SpinInfo( );
			info._valueInput = valueInput;

			if ( info.Parse( spinIncrement ) )
				return info;

			return null;
		}

		private bool Parse( object spinIncrement )
		{
			_origSpinIncrement = spinIncrement;

			if ( spinIncrement is string )
			{
				string str = ( (string)spinIncrement ).Trim( );

				Match match = Regex.Match( str, 
                    // SSP 5/13/10 TFS32082
                    // Added logic to support 'h' and 's' for hours and seconds as well as 
                    // treat 'm' as minute if it's a time mask.
                    // 
                    //@"^([\-\+]?)(\d+)\s*([Dd]|[Mm]|[Yy])$" 
                    @"^([\-\+]?)(\d+)\s*([Dd]|[Mm]|[Yy]|[Hh]|[Ss])$" 
                );
				if ( null != match && match.Success )
				{
					bool isNegative = "-" == match.Groups[1].Value;
					if ( !decimal.TryParse( match.Groups[2].Value, out _incrementAmountNumeric ) )
						return false;

					string symbol = match.Groups[3].Value;

                    // SSP 5/13/10 TFS32082
                    // Added logic to support 'h' and 's' for hours and seconds as well as 
                    // treat 'm' as minute if it's a time mask.
                    // 
                    // ------------------------------------------------------------------------
                    symbol = symbol.ToLower( );
                    IncrementType incrementType;
                    switch ( symbol )
                    {
                        case "y":
                            incrementType = IncrementType.Year;
                            break;
                        case "m":
                            incrementType = IncrementType.MonthOrMinute;
                            break;
                        case "d":
                            incrementType = IncrementType.Day;
                            break;
                        case "h":
                            incrementType = IncrementType.Hour;
                            break;
                        case "s":
                            incrementType = IncrementType.Second;
                            break;
                        default:
                            Debug.Assert( false );
                            return false;
                    }

                    _incrementType = incrementType;					
                    // ------------------------------------------------------------------------

					return true;
				}

				match = Regex.Match( str, @"^log$" );
				if ( null != match && match.Success )
				{
					_incrementType = IncrementType.Log10;
					return true;
				}
			}
			else if ( spinIncrement is TimeSpan )
			{
				_incrementType = IncrementType.TimeSpan;
				_incrementAmountTimeSpan = (TimeSpan)spinIncrement;
				return true;
			}

			object decimalValue = this.ConvertDataValue( spinIncrement, typeof( decimal ) );
			if ( decimalValue is decimal )
			{
				_incrementType = IncrementType.NumbericAmount;
				_incrementAmountNumeric = (decimal)decimalValue;
				return true;
			}

			return false;
		}

		#endregion // Parse

		#region ResetAcceleration

		internal void ResetAcceleration( )
		{
			_acceleration_SpinCount = 0;
		}

		#endregion // ResetAcceleration

		#region Spin

		public bool Spin( bool up )
		{
			this.CheckElapsedTimeHelper( );

			object newValue;
			if ( this.SpinHelper( up, out newValue ) )
			{
				_valueInput.Value = newValue;
				_acceleration_SpinCount++;
				return true;
			}

			return false;
		}

		#endregion // Spin

		#region SpinHelper

		private bool SpinHelper( bool up, out object newValue )
		{
			object currentValue = _valueInput.Value;
			newValue = null;

			if ( Utils.IsValueEmpty( currentValue ) )
			{
				newValue = this.GetStartValue( up, false );
				return true;
			}

            // SSP 5/14/10 TFS32082
            // Refactored code. Added the if block and enclosed the existing code into the else block.
            // Also code in the if block is refactored version of the original code that's left commented
            // in the else block.
            // 
            if ( this.IsIncrementTypeForDateTime( ) )
            {
                DateTime val;
                if ( this.GetCurrentValueAsDate( out val ) )
                {
                    decimal amountNumeric = up ? _incrementAmountNumeric : -_incrementAmountNumeric;

                    try
                    {
                        System.Globalization.Calendar calendar = this.GetCalendar( );
                        DateTime origVal = val;
                        switch ( _incrementType )
                        {
                            case IncrementType.Day:
                                val = CalendarHelper.AddDays( val, (int)amountNumeric, calendar );
                                break;
                            case IncrementType.Month:
								val = CalendarHelper.AddMonths( val, (int)amountNumeric, calendar );
                                break;
                            case IncrementType.Year:
								val = CalendarHelper.AddYears( val, (int)amountNumeric, calendar );
                                break;
                            // SSP 5/14/10 TFS32082
                            // Added Hour, Minute, Second, MonthOrMinute members to the IncrementType.
                            // 
                            case IncrementType.Hour:
                                val = val.AddHours( (double)amountNumeric );
                                break;
                            case IncrementType.Minute:
                                val = val.AddMinutes( (double)amountNumeric );
                                break;
                            case IncrementType.Second:
                                val = val.AddSeconds( (double)amountNumeric );
                                break;
                            case IncrementType.MonthOrMinute:
                                {
                                    bool incrementMinute = false;

                                    // If the MaskedInput has no date sections but just the time sections
                                    // then increment by minutes. If the MaskedInput has both the date and
                                    // time sections then still increment by month.
                                    // 
                                    XamMaskedInput maskedInput = _valueInput as XamMaskedInput;
                                    if ( null != maskedInput )
                                    {
                                        bool hasDateSections, hasTimeSections;
                                        XamMaskedInput.DeduceEditAsType( maskedInput.Sections, out hasDateSections, out hasTimeSections );

                                        if ( !hasDateSections && hasTimeSections )
                                            incrementMinute = true;
                                    }

                                    if ( incrementMinute )
                                        val = val.AddMinutes( (double)amountNumeric );
                                    else
										val = CalendarHelper.AddMonths( val, (int)amountNumeric, calendar );
                                }
                                break;
                            case IncrementType.TimeSpan:
                                val = up ? val.Add( _incrementAmountTimeSpan ) : val.Subtract( _incrementAmountTimeSpan );
                                break;
                        }

                        // Calendar add methods above will return the same value when reaching
                        // the limit.
                        // 
                        if ( origVal != val )
                            newValue = val;
                    }
                    catch
                    {
                    }
                }
            }
            else
            {
                switch ( _incrementType )
                {
                    case IncrementType.NumbericAmount:
                        {
                            object convertedVal = this.ConvertDataValue( currentValue, typeof( decimal ) );
                            if ( convertedVal is decimal )
                            {
                                try
                                {
                                    newValue = (decimal)convertedVal + ( up ? _incrementAmountNumeric : -_incrementAmountNumeric );
                                }
                                catch
                                {
                                }
                            }
                        }
                        break;
                    case IncrementType.Log10:
                        {
                            object convertedVal = this.ConvertDataValue( currentValue, typeof( decimal ) );
                            if ( convertedVal is decimal )
                            {
                                try
                                {
                                    int exp = Math.Min( 10, _acceleration_SpinCount / 40 );
                                    object lastValidNewValue;
                                    decimal dNewValue;
                                    do
                                    {
                                        decimal amount = (decimal)Math.Pow( 10, exp );

                                        dNewValue = (decimal)convertedVal + ( up ? amount : -amount );
                                        dNewValue = decimal.Floor( dNewValue / amount ) * amount;

                                        lastValidNewValue = newValue;
                                        newValue = this.ConvertDataValue( dNewValue, _valueInput.ValueTypeResolved );
                                        exp--;
                                    }
                                    while ( exp >= 0 && null != newValue
                                            && ( !this.IsValueValid( newValue ) || exp > 0 && 0 == dNewValue ) );

                                    if ( null == newValue )
                                        newValue = lastValidNewValue;
                                }
                                catch
                                {
                                }
                            }
                        }
                        break;
                    default:
                        Debug.Assert( false, "Unknown IncrementType value." );
                        break;
                }
            }

			if ( null != newValue )
			{
                // SSP 5/14/10 TFS32082
                // Use the new ConvertToEditorValueType method, which takes into account a setup
                // with MaskedInput where it's ValueType is string (which is the default) and
                // mask is set to a time mask, or any mask that doesn't correspond to what 
                // DateTime.ToString would return, like for example the sections are out of order
                // or missing etc..., in which case the text that we get has to be based on the
                // mask so IsValueValid call below as well as when the value is set on the editor
                // end up matching to the mask correctly.
                // 
                //newValue = this.ConvertDataValue( newValue, _valueInput.ValueType );
                newValue = this.ConvertToEditorValueType( newValue );

				if ( null != newValue && this.IsValueValid( newValue ) )
					return true;
			}

			if ( _valueInput is XamMaskedInput && ( (XamMaskedInput)_valueInput ).SpinWrap )
			{
				object tmp = this.GetStartValue( up, true );
				if ( this.IsValueValid( tmp ) )
				{
					newValue = tmp;
					return true;
				}
			}

			return false;
		}

		#endregion // SpinHelper

		#endregion // Methods
	}
	
	#endregion // SpinInfo Class
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