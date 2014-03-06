using System;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Controls;
using System.Text.RegularExpressions;
using System.Linq;
using System.Windows.Data;
using System.Globalization;
using System.Collections.ObjectModel;
using System.Windows.Controls.Primitives;

namespace Infragistics.Controls.Editors
{
	#region Utils Class

	/// <summary>
	/// A class with helper routines.
	/// </summary>
	internal static class CalendarUtilities
	{
        #region Constants

        internal static readonly object ZeroInt = 0;
        internal static readonly object ZeroDouble = 0d;
        internal const int MouseWheelScrollDelta = 120;

        #endregion //Constants

		#region Member Variables

		// AS 10/15/09 TFS23860
		private static readonly Type UIElement3DType;

		#endregion //Member Variables

		#region Constructor
		static CalendarUtilities()
		{
			// AS 10/15/09 TFS23860
			UIElement3DType = Type.GetType("System.Windows.UIElement3D, " + typeof(UIElement).Assembly.FullName, false);
		} 
		#endregion //Constructor

        #region Delegates

        // AS 2/9/09 TFS11631
        internal delegate void MethodInvoker();

        #endregion //Delegates

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

		#region GetCalendar

		internal static CalendarBase GetCalendar(UIElement element)
		{
			bool dummy;

			return GetCalendar(element, out dummy);
		}

		internal static CalendarBase GetCalendar(UIElement element, out bool initialized)
		{
			CalendarBase cal = CalendarBase.GetCalendar(element);

			initialized = false;

			// see if the Calendar property has been initialized yet
			if (cal == null)
			{
				// since it wasn't look for one in the visual ancestors 
				cal = PresentationUtilities.GetVisualAncestor<CalendarBase>(element, null);

				// if found initialize it on the us and set a flag so we know to initialize our children below
				if (cal != null)
				{
					CalendarBase.SetCalendar(element, cal);
					initialized = true;
				}
			}

			return cal;
		}

		#endregion //GetCalendar	

		#region GetEnumValues
		internal static List<T> GetEnumValues<T>()
			where T : struct
		{
			Debug.Assert(typeof(T).IsEnum, "Only handles enums");

			return typeof(T).GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)
				.Where(f => f.IsLiteral)
				.Select(f => (T)f.GetValue(null))
				.ToList();
		}
		#endregion // GetEnumValues
    
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
			else if ( CalendarUtilities.IsNumericType( targetType ) )
				errorMessage = GetString( isTextEmpty ? "LMSG_EnteredNumberEmpty" : "LMSG_EnteredNumberInvalid", text );
			else
				errorMessage = GetString( isTextEmpty ? "LMSG_EnteredValueEmpty" : "LMSG_EnteredValueInvalid", text );

			return new Exception( errorMessage );
		}

		#endregion // GetTextToValueConversionError

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

		#region ValidateEnum

		private static bool IsEnumFlags( Type enumType )
		{
			object[] flagsAttributes = enumType.GetCustomAttributes( typeof( FlagsAttribute ), true );
			return null != flagsAttributes && flagsAttributes.Length > 0;
		}

		
#region Infragistics Source Cleanup (Region)










#endregion // Infragistics Source Cleanup (Region)




#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal static void ValidateEnum( string argumentName, Type enumType, object enumVal )
		{
			if ( !Enum.IsDefined( enumType, enumVal ) && !IsEnumFlags( enumType ) )
				throw new ArgumentException( argumentName );
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

		#region DictionaryKey

		private static readonly object NULL_KEY = new object( );
		internal static object DictionaryKey( object o )
		{
			return null == o ? NULL_KEY : o;
		}

		#endregion // DictionaryKey

		// AS 10/15/09 TFS23860
		#region IsUIElementOrUIElement3D
		internal static bool IsUIElementOrUIElement3D(DependencyObject d)
		{
			if (d is UIElement)
				return true;

			if (null != d && null != UIElement3DType && UIElement3DType.IsAssignableFrom(d.GetType()))
				return true;

			return false;
		}
		#endregion //IsUIElementOrUIElement3D

		#region GetString
		internal static string GetString(string name)
		{
			return GetString(name, null);
		}

		internal static string GetString(string name, params object[] args)
		{
#pragma warning disable 436
			return SR.GetString(name, args);
#pragma warning restore 436
		}
		#endregion // GetString

		#region SetBoolProperty

		internal static void SetBoolProperty(DependencyObject item, DependencyProperty dp, bool newValue, bool oldValue, bool defaultValue)
		{
			if (oldValue == newValue)
				return;

			if (newValue == defaultValue)
				item.ClearValue(dp);
			else
				item.SetValue(dp, KnownBoxes.FromValue(newValue));
		}

		internal static void SetBoolProperty(DependencyObject item, DependencyPropertyKey dpk, bool newValue, bool oldValue, bool defaultValue)
		{
			if (oldValue == newValue)
				return;

			if (newValue == defaultValue)
				item.ClearValue(dpk);
			else
				item.SetValue(dpk, KnownBoxes.FromValue(newValue));
		}

		#endregion //SetBoolProperty	
    
		#region ValidateNonHiddenVisibility

		internal static bool ValidateNonHiddenVisibility(DependencyPropertyChangedEventArgs e)
		{
			Visibility newVis = (Visibility)e.NewValue;

			switch (newVis)
			{
				case Visibility.Visible:
				case Visibility.Collapsed:
					return newVis == Visibility.Visible;
				default:
					throw new NotSupportedException(GetString("LE_HiddenNotSupported", DependencyPropertyUtilities.GetName(e.Property), newVis));
			}
		}

		#endregion //ValidateNonHiddenVisibility
	}

	#endregion // Utils Class

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