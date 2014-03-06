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
using Infragistics.Shared;
using Infragistics.Windows;
using Infragistics.Windows.Resizing;
using Infragistics.Windows.Controls;
using Infragistics.Windows.Themes;
using Infragistics.Windows.Editors.Events;
using Infragistics.Windows.Helpers;
using System.Text.RegularExpressions;
using System.Windows.Data;
using System.Globalization;
using System.Collections.ObjectModel;
using System.Windows.Controls.Primitives;

namespace Infragistics.Windows.Editors
{
	#region Utils Class

	/// <summary>
	/// A class with helper routines.
	/// </summary>
	internal static class Utils
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
		static Utils()
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
		internal static DependencyObject HitTest( UIElement elem, Point location, Type elementType, bool allowSubclassOfType )
		{
			HitTestResult hr = VisualTreeHelper.HitTest( elem, location );
			if ( null != hr && null != hr.VisualHit )
				return Utilities.GetAncestorFromType( hr.VisualHit, elementType, allowSubclassOfType, elem );

			return null;
		}

		#endregion // HitTest

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
		internal static bool IsMouseOverButton( ValueEditor editor, MouseEventArgs e )
		{
			DependencyObject elem = e.Source as DependencyObject;
			if ( null != elem )
			{
				ButtonBase button = (ButtonBase)Utilities.GetAncestorFromType( elem, typeof( ButtonBase ), true, editor );
				if ( null != button )
					return true;

				// SSP 8/16/10 TFS28574
				// If the command is not executable then the buttons get disabled and their hit test visibility
				// becomes transparent. In which case we'll get the panel containing the spin buttons which is 
				// named PART_SpinButtons.
				// 
				if ( null != Utilities.GetAncestorFromName( elem, "PART_SpinButtons" ) )
					return true;
			}

			return false;
		}

		#endregion // IsMouseOverButton

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

		#region SendLeftMouseButtonDownHelper

		// SSP 10/14/09 - NAS10.1 Spin Buttons
		// 
		internal static void SendLeftMouseButtonDownHelper( IInputElement elem, MouseEventArgs templateMouseArgs )
		{
			MouseButtonEventArgs args = new MouseButtonEventArgs(
				templateMouseArgs.MouseDevice, templateMouseArgs.Timestamp, MouseButton.Left, templateMouseArgs.StylusDevice );

			args.RoutedEvent = FrameworkElement.MouseLeftButtonDownEvent;
			elem.RaiseEvent( args );
		}

		#endregion // SendLeftMouseButtonDownHelper

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

		#region GetLineHeight

		internal static double GetLineHeight( System.Windows.Controls.Control ctrl )
		{
			FontFamily family = ctrl.FontFamily;
			double fontSize = ctrl.FontSize;
			return family.LineSpacing * fontSize;
		}

		#endregion // GetLineHeight

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

		#region IsValuePropertySet

		// SSP 1/7/08 BR29457
		// 
		internal static bool IsValuePropertySet( DependencyProperty prop, DependencyObject obj )
		{
			return Utilities.ShouldSerialize( prop, obj );
		}

		#endregion // IsValuePropertySet

		#region DictionaryKey

		private static readonly object NULL_KEY = new object( );
		internal static object DictionaryKey( object o )
		{
			return null == o ? NULL_KEY : o;
		}

		#endregion // DictionaryKey

        #region ValidateIntZeroOrMore
        internal static bool ValidateIntZeroOrMore(object value)
        {
            return value is int && (int)value >= 0;
        } 
        #endregion //ValidateIntZeroOrMore

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

	#region CollectionViewProxy Class

	// SSP 1/20/09 - NAS9.1 Record Filtering
	// See notes on XamComboEditor.ComboBox_ItemsSourceWrapper class definition for more info.
	// 
	/// <summary>
	/// Class that wraps another CollectionView. For all the methods and properties 
	/// it delegates to the source collection view. One of the uses is that you can override 
	/// OnSourceCollectionChanged method and take actions before or after any control bound to it
	/// gets the CollectionChanged notification.
	/// </summary>
	internal class CollectionViewProxy : ICollectionView, IList
	{
		#region Nested Data Structures

		#region DeferRefreshObject Class

		private class DeferRefreshObject : IDisposable
		{
			private CollectionViewProxy _cvp;

			internal DeferRefreshObject( CollectionViewProxy cvp )
			{
				_cvp = cvp;
			}

			public void Dispose( )
			{
				_cvp.ResumeRefresh( );
			}
		}

		#endregion // DeferRefreshObject Class

		#endregion // Nested Data Structures

		#region Member Vars

		private CollectionView _cv;
		private int _suspendRefreshCount;
		private IDisposable _cvDeferRefreshObject;

		#endregion // Member Vars

		#region Constructor

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="collectionView">Source collection view</param>
		public CollectionViewProxy( CollectionView collectionView )
		{
			Utils.ValidateNull( collectionView );

			_cv = collectionView;

			_cv.CurrentChanged += new EventHandler( this.OnSourceCurrentChanged );
			_cv.CurrentChanging += new CurrentChangingEventHandler( OnSourceCurrentChanging );
			( (INotifyCollectionChanged)_cv ).CollectionChanged += new NotifyCollectionChangedEventHandler( this.OnSourceCollectionChanged );
		}

		#endregion // Constructor

		#region Events

		public event NotifyCollectionChangedEventHandler CollectionChanged;
		public event EventHandler CurrentChanged;
		public event CurrentChangingEventHandler CurrentChanging;

		#endregion // Events

		#region Methods

		#region Protected Methods

		#region OnSourceCurrentChanging

		protected virtual void OnSourceCurrentChanging( object sender, CurrentChangingEventArgs e )
		{
			if ( null != this.CurrentChanging )
				this.CurrentChanging( this, e );
		}

		#endregion // OnSourceCurrentChanging

		#region OnSourceCurrentChanged

		protected virtual void OnSourceCurrentChanged( object sender, EventArgs e )
		{
			if ( null != this.CurrentChanged )
				this.CurrentChanged( this, e );
		}

		#endregion // OnSourceCurrentChanged

		#region OnSourceCollectionChanged

		protected virtual void OnSourceCollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
		{
			this.RaiseCollectionChanged( e );
		}

		#endregion // OnSourceCollectionChanged

		#endregion // Protected Methods

		#region Private Methods

		#region RaiseCollectionChanged

		private void RaiseCollectionChanged( NotifyCollectionChangedEventArgs e )
		{
			if ( null != CollectionChanged )
				this.CollectionChanged( this, e );
		}

		#endregion // RaiseCollectionChanged

		#region ResumeRefresh

		private void ResumeRefresh( )
		{
			if ( _suspendRefreshCount > 0 )
			{
				_suspendRefreshCount--;
				if ( 0 == _suspendRefreshCount )
				{
					if ( null != _cvDeferRefreshObject )
					{
						_cvDeferRefreshObject.Dispose( );
						_cvDeferRefreshObject = null;
					}

					// This shouldn't be necessary however there's a bug in the way ItemsControl implements
					// its HasItems property and probably in the ItemCollection/ListCollectionView etc...
					// In any case, we have to send Reset notification otherwise HasItems of the combo box
					// remains false and arrow navigation doesn't work when you drop down the drop-down with
					// no items selected.
					// 
					this.RaiseCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset ) );
				}
			}
		}

		#endregion // ResumeRefresh

		#region SuspendRefresh

		private void SuspendRefresh( )
		{
			if ( 0 == _suspendRefreshCount )
				_cvDeferRefreshObject = _cv.DeferRefresh( );

			_suspendRefreshCount++;
		}

		#endregion // SuspendRefresh

		#endregion // Private Methods

		#endregion // Methods

		#region ICollectionView Members

		public bool CanFilter
		{
			get { return _cv.CanFilter; }
		}

		public bool CanGroup
		{
			get { return _cv.CanGroup; }
		}

		public bool CanSort
		{
			get { return _cv.CanSort; }
		}

		public bool Contains( object item )
		{
			return _cv.Contains( item );
		}

		public System.Globalization.CultureInfo Culture
		{
			get
			{
				return _cv.Culture;
			}
			set
			{
				_cv.Culture = value;
			}
		}

		public object CurrentItem
		{
			get { return _cv.CurrentItem; }
		}

		public int CurrentPosition
		{
			get { return _cv.CurrentPosition; }
		}

		public IDisposable DeferRefresh( )
		{
			this.SuspendRefresh( );
			return new DeferRefreshObject( this );
		}

		public Predicate<object> Filter
		{
			get
			{
				return _cv.Filter;
			}
			set
			{
				_cv.Filter = value;
			}
		}

		public System.Collections.ObjectModel.ObservableCollection<GroupDescription> GroupDescriptions
		{
			get { return _cv.GroupDescriptions; }
		}

		public System.Collections.ObjectModel.ReadOnlyObservableCollection<object> Groups
		{
			get { return _cv.Groups; }
		}

		public bool IsCurrentAfterLast
		{
			get { return _cv.IsCurrentAfterLast; }
		}

		public bool IsCurrentBeforeFirst
		{
			get { return _cv.IsCurrentBeforeFirst; }
		}

		public bool IsEmpty
		{
			get { return _cv.IsEmpty; }
		}

		public bool MoveCurrentTo( object item )
		{
			return _cv.MoveCurrentTo( item );
		}

		public bool MoveCurrentToFirst( )
		{
			return _cv.MoveCurrentToFirst( );
		}

		public bool MoveCurrentToLast( )
		{
			return _cv.MoveCurrentToLast( );
		}

		public bool MoveCurrentToNext( )
		{
			return _cv.MoveCurrentToNext( );
		}

		public bool MoveCurrentToPosition( int position )
		{
			return _cv.MoveCurrentToPosition( position );
		}

		public bool MoveCurrentToPrevious( )
		{
			return _cv.MoveCurrentToPrevious( );
		}

		public void Refresh( )
		{
			_cv.Refresh( );
		}

		public SortDescriptionCollection SortDescriptions
		{
			get { return _cv.SortDescriptions; }
		}

		public IEnumerable SourceCollection
		{
			get { return _cv.SourceCollection; }
		}

		#endregion

		#region IEnumerable Members

		public IEnumerator GetEnumerator( )
		{
			return ( (IEnumerable)_cv ).GetEnumerator( );
		}

		#endregion

		#region IList Members

		public int Add( object value )
		{
			if ( _cv is IList )
				return ( (IList)_cv ).Add( value );
			else
				throw new NotSupportedException( );
		}

		public void Clear( )
		{
			if ( _cv is IList )
				( (IList)_cv ).Clear( );
			else
				throw new NotSupportedException( );
		}

		public int IndexOf( object value )
		{
			return _cv.IndexOf( value );
		}

		public void Insert( int index, object value )
		{
			if ( _cv is IList )
				( (IList)_cv ).Insert( index, value );
			else
				throw new NotSupportedException( );
		}

		public bool IsFixedSize
		{
			get
			{
				if ( _cv is IList )
					return ( (IList)_cv ).IsFixedSize;
				else
					return true;
			}
		}

		public bool IsReadOnly
		{
			get
			{
				if ( _cv is IList )
					return ( (IList)_cv ).IsReadOnly;
				else
					return true;
			}
		}

		public void Remove( object value )
		{
			if ( _cv is IList )
				( (IList)_cv ).Remove( value );
			else
				throw new NotSupportedException( );
		}

		public void RemoveAt( int index )
		{
			if ( _cv is IList )
				( (IList)_cv ).RemoveAt( index );
			else
				throw new NotSupportedException( );
		}

		public object this[int index]
		{
			get
			{
				return _cv.GetItemAt( index );
			}
			set
			{
				if ( _cv is IList )
					( (IList)_cv )[index] = value;
				else
					throw new NotSupportedException( );
			}
		}

		#endregion

		#region ICollection Members

		public void CopyTo( Array array, int index )
		{
			foreach ( object i in this )
				array.SetValue( i, index++ );
		}

		public int Count
		{
			get
			{
				return _cv.Count;
			}
		}

		public bool IsSynchronized
		{
			get
			{
				if ( _cv is ICollection )
					return ( (ICollection)_cv ).IsSynchronized;
				else
					return false;
			}
		}

		public object SyncRoot
		{
			get
			{
				if ( _cv is ICollection )
					return ( (ICollection)_cv ).SyncRoot;
				else
					return _cv;
			}
		}

		#endregion
	}

	#endregion // CollectionViewProxy Class

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
            // Month or minute based on whether the masked editor's mask has 
            // date sections or just the time sections.
            // 
            MonthOrMinute
		}

		#endregion // IncrementType Enum

		#region Member Vars

		private TimeSpan _incrementAmountTimeSpan;
		private decimal _incrementAmountNumeric;
		private IncrementType _incrementType;
		private ValueEditor _valueEditor;
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
			return Utilities.ConvertDataValue( val, type, _valueEditor.FormatProvider, null );
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
            Type editorValueType = _valueEditor.ValueType;
            XamMaskedEditor maskedEditor = _valueEditor as XamMaskedEditor;
            if ( null != maskedEditor )
            {
                // If the value is DateTime but the editor's ValueType is string then DateTime.ToString( )
                // may not result in the text that matches the mask (for example mask can contain just the time
                // sections). Therefore we have to apply the date to mask and get the text from the sections.
                // 
                if ( value is DateTime && typeof( string ) == maskedEditor.ValueType )
                    return MaskedEditorDefaultConverter.ValueToTextConverter.ConvertToTextBasedOnMask( maskedEditor, value, true );
            }

            return this.ConvertDataValue( value, editorValueType );
        }

        #endregion // ConvertToEditorValueType

		#region GetCalendar

        private System.Globalization.Calendar GetCalendar()
		{
			IFormatProvider formatProvider = _valueEditor.FormatProviderResolved;
			if ( null == _cachedCalendar || formatProvider != _cachedCalendar_lastFormatProvider )
			{
				_cachedCalendar_lastFormatProvider = formatProvider;
				_cachedCalendar = XamMaskedEditor.GetCultureCalendar( formatProvider );
			}

			return _cachedCalendar;
		}

		#endregion // GetCalendar

		#region GetCurrentValueAsDate

		private bool GetCurrentValueAsDate( out DateTime val )
		{
			object convertedVal = this.ConvertDataValue( _valueEditor.Value, typeof( DateTime ) );
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
			CalendarManager cm = CalendarManager.CurrentCulture;
            System.Globalization.Calendar calendar = cm.Calendar;
			if ( null != calendar )
				return min ? calendar.MinSupportedDateTime : calendar.MaxSupportedDateTime;

			return min ? DateTime.MinValue : DateTime.MaxValue;
		}

		#endregion // GetDefaultMinMaxDate

		#region GetResolvedMinMax

		private void GetResolvedMinMax( out object min, out object max, ref Type valueType )
		{
			min = max = null;

			ValueConstraint vc = _valueEditor.ValueConstraint;
			if ( null != vc )
			{
				min = vc.MinInclusive;
				max = vc.MaxExclusive;
			}

			XamMaskedEditor maskedEditor = _valueEditor as XamMaskedEditor;
			if ( null != maskedEditor )
			{
				if ( null == valueType || typeof( object ) == valueType || typeof( string ) == valueType )
				{
					Type deduceFromMaskType = XamMaskedEditor.DeduceEditAsType( maskedEditor.Sections );
					if ( null != deduceFromMaskType )
						valueType = deduceFromMaskType;
				}
			}

			Type valueTypeUnderlying = Utilities.GetUnderlyingType( valueType );

			if ( Utilities.IsNumericType( valueTypeUnderlying ) )
			{
				if ( null != maskedEditor )
				{
					NumberSection numberSection = null != maskedEditor.Sections
						? (NumberSection)XamMaskedEditor.GetSection( maskedEditor.Sections, typeof( NumberSection ) )
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
			Type valueType = _valueEditor.ValueType;
			this.GetResolvedMinMax( out min, out max, ref valueType );

			min = this.ConvertToDecimal( min );
			max = this.ConvertToDecimal( max );
		}

		#endregion // GetResolvedMinMaxAsDecimal

		#region GetStartValue

		private object GetStartValue( bool up, bool wrapping )
		{
			Type valueType = Utilities.GetUnderlyingType( _valueEditor.ValueType );

			object min, max;
			this.GetResolvedMinMax( out min, out max, ref valueType );

			if ( Utilities.IsNumericType( valueType ) )
			{
				object dMinObj = null != min ? this.ConvertDataValue( min, typeof( decimal ) ) : null;
				object dMaxObj = null != max ? this.ConvertDataValue( max, typeof( decimal ) ) : null;

				if ( ! wrapping )
				{
					if ( ( null == dMinObj || dMinObj is decimal && (decimal)dMinObj < 0m )
						&& ( null == dMaxObj || dMaxObj is decimal && 0m < (decimal)dMaxObj ) )
						return 0m;
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
			if ( null != val && !valueType.IsInstanceOfType( val ) )
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

		// SSP 3/24/11 TFS60190
		// Refactored. Moved existing code from IsValueValid into this new method.
		// 
		private bool ValidateAgainstMinMax( object value )
		{
			// SSP 2/16/10 TFS27573
			// Enclosed the existing code in the if block. If we are working with date-time values
			// then don't bother validating against min or max decimal values.
			// 
			if ( !this.IsIncrementTypeForDateTime( ) )
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

		private bool IsValueValid( object value )
		{
			// SSP 2/16/10 TFS27573
			// Enclosed the existing code in the if block. If we are working with date-time values
			// then don't bother validating against min or max decimal values.
			// 
			// SSP 3/24/11 TFS60190
			// Refactored. Moved existing code from here into the new ValidateAgainstMinMax method.
			// Also moved this logic here from below after the ValidateValue call. The reason for
			// doing is that XamMaskedEditor throws a handled exception if the value is negative
			// and it's mask sections do not allow negatives. As part of spinning logic, decremting
			// from 0 results in a negative vallue and checking against the valid min-max range
			// first eliminates having to call ValidateValue on the masked editor which as part
			// of its validation logic tries to assign the value to mask sections, which in turn
			// throw a handled exception because negative is not allowed. This is simply a 
			// performance improvement to avoid a handled exception.
			// 
			if ( !this.ValidateAgainstMinMax( value ) )
				return false;

			Exception error;
			if ( !_valueEditor.ValidateValue( value, out error ) )
				return false;

			return true;
		}

		#endregion // IsValueValid

		#region Parse

		internal static SpinInfo Parse( ValueEditor valueEditor, object spinIncrement )
		{
			Utils.ValidateNull( valueEditor );

			SpinInfo info = new SpinInfo( );
			info._valueEditor = valueEditor;

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

                    
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

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
				_valueEditor.Value = newValue;
				_acceleration_SpinCount++;
				return true;
			}

			return false;
		}

		#endregion // Spin

		#region SpinHelper

		private bool SpinHelper( bool up, out object newValue )
		{
			object currentValue = _valueEditor.Value;
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
                                val = CalendarManager.AddDays( val, (int)amountNumeric, calendar );
                                break;
                            case IncrementType.Month:
                                val = CalendarManager.AddMonths( val, (int)amountNumeric, calendar );
                                break;
                            case IncrementType.Year:
                                val = CalendarManager.AddYears( val, (int)amountNumeric, calendar );
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

                                    // If the masked editor has no date sections but just the time sections
                                    // then increment by minutes. If the masked editor has both the date and
                                    // time sections then still increment by month.
                                    // 
                                    XamMaskedEditor maskedEditor = _valueEditor as XamMaskedEditor;
                                    if ( null != maskedEditor )
                                    {
                                        bool hasDateSections, hasTimeSections;
                                        XamMaskedEditor.DeduceEditAsType( maskedEditor.Sections, out hasDateSections, out hasTimeSections );

                                        if ( !hasDateSections && hasTimeSections )
                                            incrementMinute = true;
                                    }

                                    if ( incrementMinute )
                                        val = val.AddMinutes( (double)amountNumeric );
                                    else
                                        val = CalendarManager.AddMonths( val, (int)amountNumeric, calendar );
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
                    
#region Infragistics Source Cleanup (Region)



















































#endregion // Infragistics Source Cleanup (Region)

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
                                        dNewValue = Math.Floor( dNewValue / amount ) * amount;

                                        lastValidNewValue = newValue;
                                        newValue = this.ConvertDataValue( dNewValue, _valueEditor.ValueType );
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
                // with masked editor where it's ValueType is string (which is the default) and
                // mask is set to a time mask, or any mask that doesn't correspond to what 
                // DateTime.ToString would return, like for example the sections are out of order
                // or missing etc..., in which case the text that we get has to be based on the
                // mask so IsValueValid call below as well as when the value is set on the editor
                // end up matching to the mask correctly.
                // 
                //newValue = this.ConvertDataValue( newValue, _valueEditor.ValueType );
                newValue = this.ConvertToEditorValueType( newValue );

				if ( null != newValue && this.IsValueValid( newValue ) )
					return true;
			}

			if ( _valueEditor is XamMaskedEditor && ( (XamMaskedEditor)_valueEditor ).SpinWrap )
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