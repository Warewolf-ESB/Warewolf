using System;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
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
using Infragistics.Controls.Editors.Primitives;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Infragistics.Controls.Editors
{
	#region ValueConstraintFlags

	/// <summary>
	/// Used for specifying which constraints should be applied to the input value of the 
	/// <see cref="ValueConstraint.Validate(object, Type, ValueConstraintFlags)"/> method.
	/// </summary>
	[Flags]
	public enum ValueConstraintFlags
	{
		/// <summary>
		/// Specifies that the <see cref="ValueConstraint.Enumeration"/> constraint should be applied during validation.
		/// </summary>
		Enumeration = 1,

		/// <summary>
		/// Specifies that the <see cref="ValueConstraint.FixedValue"/> constraint should be applied during validation.
		/// </summary>
		FixedValue = 2,

		/// <summary>
		/// Specifies that the implicit constraints imposed by the pseudo-datatype supplied by the 
		/// <see cref="ValueConstraint.ValidateAsType"/> property should be enforced during validation.
		/// </summary>
		ImplicitValueAsTypeConstraints = 4,

		/// <summary>
		/// Specifies that the implicit constraints imposed by the Type argument to the 
		/// <see cref="ValueConstraint.Validate(object, Type, ValueConstraintFlags)"/>
		/// method should be enforced during validation.
		/// </summary>
		ImplicitTypeParameterConstraints = 8,

		/// <summary>
		/// Specifies that the <see cref="ValueConstraint.MaxExclusive"/> constraint should be applied during validation.
		/// </summary>
		MaxExclusive = 16,

		/// <summary>
		/// Specifies that the <see cref="ValueConstraint.MaxInclusive"/> constraint should be applied during validation.
		/// </summary>
		MaxInclusive = 32,

		/// <summary>
		/// Specifies that the <see cref="ValueConstraint.MaxLength"/> constraint should be applied during validation.
		/// </summary>
		MaxLength = 64,

		/// <summary>
		/// Specifies that the <see cref="ValueConstraint.MinExclusive"/> constraint should be applied during validation.
		/// </summary>
		MinExclusive = 128,

		/// <summary>
		/// Specifies that the <see cref="ValueConstraint.MinInclusive"/> constraint should be applied during validation.
		/// </summary>
		MinInclusive = 256,

		/// <summary>
		/// Specifies that the <see cref="ValueConstraint.MinLength"/> constraint should be applied during validation.
		/// </summary>
		MinLength = 512,

		/// <summary>
		/// Specifies that the <see cref="ValueConstraint.Nullable"/> constraint should be applied during validation.
		/// </summary>
		Nullable = 1024,

		/// <summary>
		/// Specifies that the <see cref="ValueConstraint.RegexPattern"/> constraint should be applied during validation.
		/// </summary>
		RegexPattern = 2048,

		/// <summary>
		/// Specifies that all constraints, both implicit and explicit, should be applied during validation.
		/// </summary>
		All = 0x7fffffff,
	}

	#endregion // ValueConstraintFlags

	#region ValidateAsType

	/// <summary>
	/// Used to specify how the <see cref="ValueConstraint"/> class should validate a value.  Each of these
	/// values represents a data type which has implicit constraints, such as a minimal value, etc.  Those
	/// constraints are used by the <see cref="ValueConstraint.Validate(object, Type)"/> method to enforce data validity.
	/// </summary>
	public enum ValidateAsType
	{
		/// <summary>
		/// Represents an unrecognized or unspecified type.
		/// </summary>
		Unknown = 0,

		/// <summary>
		/// Represents a value which can either be true or false.  Corresponds to System.Boolean.
		/// </summary>
		Boolean,

		/// <summary>
		/// Represents a signed byte.  Corresponds to System.SByte.
		/// </summary>
		Byte,

		/// <summary>
		/// Represents a date and time.  Corresponds to System.DateTime.
		/// </summary>
		DateTime,

		/// <summary>
		/// Represents a high precision floating point value.  Corresponds to System.Decimal.
		/// </summary>
		Decimal,

		/// <summary>
		/// Represents a high magnitude floating point value.  Corresponds to System.Double.
		/// </summary>
		Double,

		/// <summary>
		/// Represents a floating point value.  Corresponds to System.Single.
		/// </summary>
		Float,

		/// <summary>
		/// Represents a signed 16 bit integral value.  Corresponds to System.Int16.
		/// </summary>
		Integer16,

		/// <summary>
		/// Represents a signed 32 bit integral value.  Corresponds to System.Int32.
		/// </summary>
		Integer32,

		/// <summary>
		/// Represents a signed 64 bit integral value.  Corresponds to System.Int64.
		/// </summary>
		Integer64,

		/// <summary>
		/// Represents a signed 64 bit integral number whose maximal value is -1, inclusive.  Corresponds to System.Int64.
		/// </summary>
		NegativeInteger64,

		/// <summary>
		/// Represents a signed 64 bit integral number whose minimal value is 0, inclusive.  Corresponds to System.Int64.
		/// </summary>
		NonNegativeInteger64,

		/// <summary>
		/// Represents a signed 64 bit integral number whose maximal value is 0, inclusive.  Corresponds to System.Int64.
		/// </summary>
		NonPositiveInteger64,

		/// <summary>
		/// Represents a signed 64 bit integral number whose minimal value is -1, inclusive.  Corresponds to System.Int64.
		/// </summary>
		PositiveInteger64,

		/// <summary>
		/// Represents textual data.  Corresponds to System.String.
		/// </summary>
		Text,

		/// <summary>
		/// Represents a length of time.  Corresponds to System.TimeSpan.
		/// </summary>
		TimeSpan,

		/// <summary>
		/// Represents an unsigned byte value.  Corresponds to System.Byte.
		/// </summary>
		UnsignedByte,

		/// <summary>
		/// Represents an unsigned 16 bit number.  Corresponds to System.UInt16.
		/// </summary>
		UnsignedInteger16,

		/// <summary>
		/// Represents an unsigned 32 bit number.  Corresponds to System.UInt32.
		/// </summary>
		UnsignedInteger32,

		/// <summary>
		/// Represents an unsigned 64 bit number.  Corresponds to System.UInt64.
		/// </summary>
		UnsignedInteger64,

		/// <summary>
		/// Represents a Uniform Resource Identifier.  Corresponds to System.Uri.
		/// </summary>
		Uri,
	}

	#endregion // ValidateAsType

	#region ValueConstraint Class

	/// <summary>
	/// Contains various constraints that can be applied to a data value.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// ValueConstraint object contains various properties that you can set to create constraints
	/// for data values. The ValueConstraint will check to see if a data value matches all the
	/// constraints.
	/// </para>
	/// <para class="body">
	/// <see cref="ValueInput"/> class exposes <see cref="ValueInput.ValueConstraint"/> property 
	/// that is of this object type. ValueInput's ValueConstraint property can be
	/// used to constraint what values the user can input in the editors. See <see cref="ValueInput.ValueConstraint"/>
	/// for more information.
	/// </para>
	/// <para class="body">
	/// You can use the ValueConstraint directly to validate a value by calling its
	/// <see cref="ValueConstraint.Validate(object, Type)"/> or one of the other overloads.
	/// </para>
	/// </remarks>
	public class ValueConstraint : DependencyObject
		// SSP 3/24/10 TFS27839
		// 
		, INotifyPropertyChanged
		, ISupportPropertyChangeNotifications
	{
		#region Data

		private Regex _regex;

		private const int NOTSET = -1;
		private const int DEFAULT_LENGTH = 0;

		private PropertyChangeListenerList _listeners;

		#endregion // Data

		#region Constructor

		/// <summary>
		/// Creates a new instance of <see cref="ValueConstraint"/> class without any constraints.
		/// </summary>
		public ValueConstraint( )
		{
		}

		#endregion // Constructor

		#region Public Interface

		#region Constraint Properties

		#region Enumeration

		/// <summary>
		/// Identifies the <see cref="Enumeration"/> dependency property
		/// </summary>
		public static readonly DependencyProperty EnumerationProperty = DependencyPropertyUtilities.Register(
			"Enumeration",
			typeof( IEnumerable ),
			typeof( ValueConstraint ),
			null, new PropertyChangedCallback( OnPropertyChanged ) 
			);

		/// <summary>
		/// Gets/sets an object implementing <see cref="IEnumerable"/> which contains a list of value options.
		/// Note, this property defaults to a null (Nothing) value.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// Enumeration constraints data value to be one of the elements that are part of the enumeration.
		/// In other words the value being validated by this ValueConstraint object will not be considered
		/// valid if the value is not part of this enumeration.
		/// </para>
		/// <para class="note">
		/// <b>Note:</b> The Enumeration is IEnumerable interface type. Therefore it can be set to any object
		/// that implements this interface, including Array, ArrayList etc...
		/// </para>
		/// </remarks>
		//[Description( "A list of items which must contain the value being validated in order for it to be considered a valid value." )]
		//[Category( "Data" )]
		[Bindable( true )]
		public IEnumerable Enumeration
		{
			get
			{
				return (IEnumerable)this.GetValue( EnumerationProperty );
			}
			set
			{
				this.SetValue( EnumerationProperty, value );
			}
		}

		/// <summary>
		/// Returns true if the Enumeration property is set to a non-default value.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public bool ShouldSerializeEnumeration( )
		{
			return DependencyPropertyUtilities.ShouldSerialize( this, EnumerationProperty );
		}

		/// <summary>
		/// Resets the Enumeration property to its default state.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public void ResetEnumeration( )
		{
			this.ClearValue( EnumerationProperty );
		}

		/// <summary>
		/// Returns true if the <see cref="Enumeration"/> list is not null.
		/// </summary>
		public bool HasEnumeration
		{
			get { return this.Enumeration != null; }
		}

		#endregion // Enumeration

		#region FixedValue

		/// <summary>
		/// Identifies the <see cref="FixedValue"/> dependency property
		/// </summary>
		public static readonly DependencyProperty FixedValueProperty = DependencyPropertyUtilities.Register(
			"FixedValue",
			typeof( object ),
			typeof( ValueConstraint ),
			null, new PropertyChangedCallback( OnPropertyChanged ) 
			);

		/// <summary>
		/// Gets/sets the value which the constrained value must be equal to.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// If FixedValue is set then the data value being validated must be equal to this property's value.
		/// </para>
		/// </remarks>
		//[Description( "The specific value to which the value being validated must be equal." )]
		//[Category( "Data" )]
		[Bindable( true )]
		public object FixedValue
		{
			get
			{
				return (object)this.GetValue( FixedValueProperty );
			}
			set
			{
				this.SetValue( FixedValueProperty, value );
			}
		}

		/// <summary>
		/// Returns true if the FixedValue property is set to a non-default value.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public bool ShouldSerializeFixedValue( )
		{
			return DependencyPropertyUtilities.ShouldSerialize( this, FixedValueProperty );
		}

		/// <summary>
		/// Resets the FixedValue property to its default state.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public void ResetFixedValue( )
		{
			this.ClearValue( FixedValueProperty );
		}

		/// <summary>
		/// Returns true if the <see cref="FixedValue"/> property is not null (Nothing).
		/// </summary>
		public bool HasFixedValue
		{
			get { return this.FixedValue != null; }
		}

		#endregion // FixedValue

		#region MaxExclusive

		/// <summary>
		/// Identifies the <see cref="MaxExclusive"/> dependency property
		/// </summary>
		public static readonly DependencyProperty MaxExclusiveProperty = DependencyPropertyUtilities.Register(
			"MaxExclusive",
			typeof( object ),
			typeof( ValueConstraint ),
			null, new PropertyChangedCallback( OnPropertyChanged )
			);

		/// <summary>
		/// Gets/sets the value that will constraint the data value to be less than it.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// The data value being validated must be less than the MaxExclusive. It can not be the same as
		/// MaxExclusive.
		/// </para>
		/// </remarks>
		//[Description("The value which is one greater than the maximum allowed for the constrained value.")]
		//[Category( "Data" )]
		[Bindable( true )]
		public object MaxExclusive
		{
			get
			{
				return (object)this.GetValue( MaxExclusiveProperty );
			}
			set
			{
				this.SetValue( MaxExclusiveProperty, value );
			}
		}

		/// <summary>
		/// Returns true if the MaxExclusive property is set to a non-default value.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public bool ShouldSerializeMaxExclusive( )
		{
			return DependencyPropertyUtilities.ShouldSerialize( this, MaxExclusiveProperty );
		}

		/// <summary>
		/// Resets the MaxExclusive property to its default state.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public void ResetMaxExclusive( )
		{
			this.ClearValue( MaxExclusiveProperty );
		}

		/// <summary>
		/// Returns true if the <see cref="MaxExclusive"/> property is not null (Nothing).
		/// </summary>
		public bool HasMaxExclusive
		{
			get { return this.MaxExclusive != null; }
		}

		#endregion // MaxExclusive

		#region MaxInclusive

		/// <summary>
		/// Identifies the <see cref="MaxInclusive"/> dependency property
		/// </summary>
		public static readonly DependencyProperty MaxInclusiveProperty = DependencyPropertyUtilities.Register(
			"MaxInclusive",
			typeof( object ),
			typeof( ValueConstraint ),
			null, new PropertyChangedCallback( OnPropertyChanged ) 
			);

		/// <summary>
		/// Gets/sets the maximum value which the constrained value is allowed to be.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// The data value being validated must be less than or equal to the MaxInclusive.
		/// </para>
		/// </remarks>
		//[Description("The maximum value which the constrained value is allowed to be.")]
		//[Category( "Data" )]
		[Bindable( true )]
		public object MaxInclusive
		{
			get
			{
				return (object)this.GetValue( MaxInclusiveProperty );
			}
			set
			{
				this.SetValue( MaxInclusiveProperty, value );
			}
		}

		/// <summary>
		/// Returns true if the MaxInclusive property is set to a non-default value.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public bool ShouldSerializeMaxInclusive( )
		{
			return DependencyPropertyUtilities.ShouldSerialize( this, MaxInclusiveProperty );
		}

		/// <summary>
		/// Resets the MaxInclusive property to its default state.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public void ResetMaxInclusive( )
		{
			this.ClearValue( MaxInclusiveProperty );
		}

		/// <summary>
		/// Returns true if the <see cref="MaxInclusive"/> property is not null (Nothing).
		/// </summary>
		public bool HasMaxInclusive
		{
			get { return this.MaxInclusive != null; }
		}

		#endregion // MaxInclusive

		#region MaxLength

		/// <summary>
		/// Identifies the <see cref="MaxLength"/> dependency property
		/// </summary>
		public static readonly DependencyProperty MaxLengthProperty = DependencyPropertyUtilities.Register(
			"MaxLength",
			typeof( int ),
			typeof( ValueConstraint ),
			DEFAULT_LENGTH, 
			new PropertyChangedCallback( OnPropertyChanged ) 
		);

		private static void OnMaxLengthChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			ValidateMaxLength( e.NewValue );
		}

		/// <summary>
		/// Gets/sets the maximum length (inclusive) allowed for the constrained value.
		/// This constraint applies to values of type 'string'.
		/// The default value is 0, which means that there is no limit.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// MaxLength constraint will ensure that the data value being validated has
		/// a length that's less than or equal the value of this property.
		/// </para>
		/// <para class="body">
		/// <b>Note:</b> If the data value being validated is not a string object then it
		/// will be converted to string before performing this validation.
		/// </para>
		/// </remarks>
		//[Description("The maximum length (inclusive) allowed for the constrained value.")]
		//[Category( "Data" )]
		[Bindable( true )]
		public int MaxLength
		{
			get
			{
				return (int)this.GetValue( MaxLengthProperty );
			}
			set
			{
				this.SetValue( MaxLengthProperty, value );
			}
		}

		private static bool ValidateMaxLength( object objVal )
		{
			int val = (int)objVal;

			// MaxLength must be 0 or greater.
			// 
			if ( val < DEFAULT_LENGTH )
				return false;

			return true;
		}

		/// <summary>
		/// Returns true if the MaxLength property is set to a non-default value.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public bool ShouldSerializeMaxLength( )
		{
			return DependencyPropertyUtilities.ShouldSerialize( this, MaxLengthProperty );
		}

		/// <summary>
		/// Resets the MaxLength property to its default state.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public void ResetMaxLength( )
		{
			this.ClearValue( MaxLengthProperty );
		}

		/// <summary>
		/// Returns true if the <see cref="MaxLength"/> property has a value greater than 0.
		/// </summary>
		public bool HasMaxLength
		{
			get { return this.MaxLength != DEFAULT_LENGTH; }
		}

		#endregion // MaxLength

		#region MinExclusive

		/// <summary>
		/// Identifies the <see cref="MinExclusive"/> dependency property
		/// </summary>
		public static readonly DependencyProperty MinExclusiveProperty = DependencyPropertyUtilities.Register(
			"MinExclusive",
			typeof( object ),
			typeof( ValueConstraint ),
			null,
			
			
			
			
			new PropertyChangedCallback( OnPropertyChanged ) 
		);

		/// <summary>
		/// Gets/sets the value that will constraint the data value to be greater than it.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// The data value being validated must be greater than the MinExclusive. It can not be the same as
		/// MinExclusive.
		/// </para>
		/// </remarks>
		//[Description("The value which is one less than the minimum allowed for the constrained value.")]
		//[Category( "Data" )]
		[Bindable( true )]
		public object MinExclusive
		{
			get
			{
				return (object)this.GetValue( MinExclusiveProperty );
			}
			set
			{
				this.SetValue( MinExclusiveProperty, value );
			}
		}

		/// <summary>
		/// Returns true if the MinExclusive property is set to a non-default value.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public bool ShouldSerializeMinExclusive( )
		{
			return DependencyPropertyUtilities.ShouldSerialize( this, MinExclusiveProperty );
		}

		/// <summary>
		/// Resets the MinExclusive property to its default state.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public void ResetMinExclusive( )
		{
			this.ClearValue( MinExclusiveProperty );
		}

		/// <summary>
		/// Returns true if the <see cref="MinExclusive"/> property is not null (Nothing).
		/// </summary>
		public bool HasMinExclusive
		{
			get { return this.MinExclusive != null; }
		}

		#endregion // MinExclusive

		#region MinInclusive

		/// <summary>
		/// Identifies the <see cref="MinInclusive"/> dependency property
		/// </summary>
		public static readonly DependencyProperty MinInclusiveProperty = DependencyPropertyUtilities.Register(
			"MinInclusive",
			typeof( object ),
			typeof( ValueConstraint ),
			null,
			
			
			
			
			new PropertyChangedCallback( OnPropertyChanged ) 
		);

		/// <summary>
		/// Gets/sets the minimum value which the constrained value is allowed to be.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// The data value being validated must be greater than or equal to the MinInclusive.
		/// </para>
		/// </remarks>
		//[Description("The minimum value which the constrained value is allowed to be.")]
		//[Category( "Data" )]
		[Bindable( true )]
		public object MinInclusive
		{
			get
			{
				return (object)this.GetValue( MinInclusiveProperty );
			}
			set
			{
				this.SetValue( MinInclusiveProperty, value );
			}
		}

		/// <summary>
		/// Returns true if the MinInclusive property is set to a non-default value.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public bool ShouldSerializeMinInclusive( )
		{
			return DependencyPropertyUtilities.ShouldSerialize( this, MinInclusiveProperty );
		}

		/// <summary>
		/// Resets the MinInclusive property to its default state.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public void ResetMinInclusive( )
		{
			this.ClearValue( MinInclusiveProperty );
		}

		/// <summary>
		/// Returns true if the <see cref="MinInclusive"/> property is not null (Nothing).
		/// </summary>
		public bool HasMinInclusive
		{
			get { return this.MinInclusive != null; }
		}

		#endregion // MinInclusive

		#region MinLength

		/// <summary>
		/// Identifies the <see cref="MinLength"/> dependency property
		/// </summary>
		public static readonly DependencyProperty MinLengthProperty = DependencyPropertyUtilities.Register(
			"MinLength",
			typeof( int ),
			typeof( ValueConstraint ),
			(int)0, 
			new PropertyChangedCallback( OnMinLengthChanged )
		);

		private static void OnMinLengthChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			ValidateMinLength( e.NewValue );
		}

		/// <summary>
		/// Gets/sets the minimum length (inclusive) allowed for the constrained value.
		/// This constraint applies to values of type 'string'.
		/// The default value is 0, which means there is no minimum length constraint.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// MinLength constraint will ensure that the data value being validated has
		/// a length that's greater than or equal the value of this property.
		/// </para>
		/// <para class="body">
		/// <b>Note:</b> If the data value being validated is not a string object then it
		/// will be converted to string before performing this validation.
		/// </para>
		/// </remarks>
		//[Description("The minimum length (inclusive) allowed for the constrained value.")]
		//[Category( "Data" )]
		[Bindable( true )]
		public int MinLength
		{
			get
			{
				return (int)this.GetValue( MinLengthProperty );
			}
			set
			{
				this.SetValue( MinLengthProperty, value );
			}
		}

		private static bool ValidateMinLength( object objVal )
		{
			int val = (int)objVal;

			// MinLength should be 0 or greater.
			// 
			if ( val < DEFAULT_LENGTH )
				return false;

			return true;
		}

		/// <summary>
		/// Returns true if the MinLength property is set to a non-default value.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public bool ShouldSerializeMinLength( )
		{
			return DependencyPropertyUtilities.ShouldSerialize( this, MinLengthProperty );
		}

		/// <summary>
		/// Resets the MinLength property to its default state.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public void ResetMinLength( )
		{
			this.ClearValue( MinLengthProperty );
		}

		/// <summary>
		/// Returns true if the <see cref="MinLength"/> property has a value greater than 0.
		/// </summary>
		public bool HasMinLength
		{
			get { return this.MinLength > DEFAULT_LENGTH; }
		}

		#endregion // MinLength

		#region Nullable

		/// <summary>
		/// Identifies the <see cref="Nullable"/> dependency property
		/// </summary>
		public static readonly DependencyProperty NullableProperty = DependencyPropertyUtilities.Register(
			"Nullable",
			typeof( bool? ),
			typeof( ValueConstraint ),
			null,
			
			
			
			
			new PropertyChangedCallback( OnPropertyChanged ) 
		);

		/// <summary>
		/// Gets/sets a flag which indicates if the constrained value is allowed to be null (Nothing).
		/// Default value of this property is null which is to allow null values.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// Nullable property if set to False constraints the value to be non-null. The value must not
		/// be one of null (Nothing in VB) or DBNull.
		/// </para>
		/// <para class="body">
		/// If the ValueConstraint object is associated with an editor's <see cref="ValueInput.ValueConstraint"/>
		/// property, <b>Nullable</b> indicates if the user is allowed to delete all the contents of the editor.
		/// If Nullable is set to False then the user is not allowed to delete all the contents. The user must
		/// enter a value in the editor.
		/// </para>
		/// </remarks>
		//[Description("Indicates if the constrained value is allowed to be null (Nothing).")]
		//[Category( "Data" )]
		[Bindable( true )]



		public bool? Nullable
		{
			get
			{
				return (bool?)this.GetValue( NullableProperty );
			}
			set
			{
				this.SetValue( NullableProperty, KnownBoxes.FromValue(value) );
			}
		}

		/// <summary>
		/// Returns true if the Nullable property is set to a non-default value.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public bool ShouldSerializeNullable( )
		{
			return DependencyPropertyUtilities.ShouldSerialize( this, NullableProperty );
		}

		/// <summary>
		/// Resets the Nullable property to its default state.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public void ResetNullable( )
		{
			this.ClearValue( NullableProperty );
		}

		/// <summary>
		/// Returns true if the <see cref="Nullable"/> property returns a non-default value.
		/// </summary>
		public bool HasNullable
		{
			get { return this.Nullable != null; }
		}

		#endregion // MinLength

		#region RegexPattern

		/// <summary>
		/// Identifies the <see cref="RegexPattern"/> dependency property
		/// </summary>
		public static readonly DependencyProperty RegexPatternProperty = DependencyPropertyUtilities.Register(
			"RegexPattern",
			typeof( string ),
			typeof( ValueConstraint ),
			null, new PropertyChangedCallback( OnRegexPatternChanged )
		);

		/// <summary>
		/// Gets/sets the regular expression to which the constrained value must adhere.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// RegexPattern validates the data value against a regular expression pattern. The value
		/// must match this patter for it be considered valid.
		/// </para>
		/// <para class="body">
		/// Here are some examples of regular expressions:<br/>
		/// <ul>
		/// <li><b>\d*</b> - the text must be zero or more digits</li>
		/// <li><b>\d+</b> - the text must be one or more digits</li>
		/// <li><b>[a-d]+</b> - the text must be one or more instances of a, b, c, d characters</li>
		/// <li><b>\d{3}-?\d{3}-?\d{4}</b> - U.S. phone number pattern where '-' are optional</li>
		/// </ul>
		/// </para>
		/// <para class="body">
		/// See .NET <see cref="System.Text.RegularExpressions.Regex"/> class for more information
		/// on the syntax of regular expressions. The Regex class is used to perform the matching.
		/// </para>
		/// <para class="body">
		/// <b>Note:</b> If the data value being validated is not a string object then it
		/// will be converted to string before performing this validation.
		/// </para>
		/// </remarks>
		//[Description("A regular expression to which the constrained value must adhere.")]
		//[Category( "Data" )]
		[Bindable( true )]
		public string RegexPattern
		{
			get
			{
				return (string)this.GetValue( RegexPatternProperty );
			}
			set
			{
				this.SetValue( RegexPatternProperty, value );
			}
		}

		private static bool ValidateRegexPattern( object objVal )
		{
			string val = (string)objVal;

			if ( null != val && val.Length > 0 )
			{
				try
				{
					new Regex( val );
				}
				catch
				{
					return false;
				}
			}

			return true;
		}

		private static void OnRegexPatternChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			ValueConstraint vc = (ValueConstraint)dependencyObject;

			// SSP 6/14/12 TFS104747
			// Simply null out the _regex instead which will cause us to re-create it in the match logic.
			// The reason for the change is that now we are manipulating the pattern when we create
			// the regex.
			// 
			//string newVal = (string)e.NewValue;
			//vc._regex = null != newVal && newVal.Length > 0 ? new Regex( newVal ) : null;
			vc._regex = null;

			ValidateRegexPattern( e.NewValue );
		}

		/// <summary>
		/// Returns true if the RegexPattern property is set to a non-default value.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public bool ShouldSerializeRegexPattern( )
		{
			return null != this.RegexPattern && this.RegexPattern.Length > 0;
		}

		/// <summary>
		/// Resets the RegexPattern property to its default state.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public void ResetRegexPattern( )
		{
			this.ClearValue( RegexPatternProperty );
		}

		/// <summary>
		/// Returns true if the <see cref="RegexPattern"/> is not null (Nothing).
		/// </summary>
		public bool HasRegexPattern
		{
			get { return this.RegexPattern != null; }
		}

		#endregion // RegexPattern

		#region ValidateAsType

		// SSP 4/16/09
		// Changed the ValidateAsType from a strictly CLR property to a dependency property.
		// 

		/// <summary>
		/// Identifies the <see cref="ValidateAsType"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty ValidateAsTypeProperty = DependencyPropertyUtilities.Register(
			"ValidateAsType",
			typeof( ValidateAsType ),
			typeof( ValueConstraint ),
			ValidateAsType.Unknown,
			
			
			
			
			new PropertyChangedCallback( OnPropertyChanged ) 
		);

		/// <summary>
		/// Gets/sets the <see cref="ValidateAsType"/> value which the <see cref="Validate(object,Type)"/> method should
		/// use to constrain the input value.  By default this value is set to <b>Unknown</b>.
		/// </summary>
		//[Description( "Specifies the type that the value is required to be." )]
		//[Category( "Data" )]
		[Bindable( true )]
		public ValidateAsType ValidateAsType
		{
			get
			{
				return (ValidateAsType)this.GetValue( ValidateAsTypeProperty );
			}
			set
			{
				this.SetValue( ValidateAsTypeProperty, value );
			}
		}

		/// <summary>
		/// Returns true if the ValidateAsType property is set to a non-default value.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public bool ShouldSerializeValidateAsType( )
		{
			return DependencyPropertyUtilities.ShouldSerialize( this, ValidateAsTypeProperty );
		}

		/// <summary>
		/// Resets the ValidateAsType property to its default state.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public void ResetValidateAsType( )
		{
			this.ClearValue( ValidateAsTypeProperty );
		}

		#endregion // ValidateAsType

		#endregion // Constraint Properties

		#region GetTypeFromValidateAsType [static]

		/// <summary>
		/// Returns the System.Type object which corresponds to the <see cref="ValidateAsType"/> argument.
		/// </summary>
		/// <param name="validateAsType">The psuedo-data type for which the corresponding CLR Type is needed.</param>
		public static Type GetTypeFromValidateAsType( ValidateAsType validateAsType )
		{
			Utils.ValidateEnum( "validateAsType", typeof( ValidateAsType ), validateAsType );

			Type type = null;

			switch ( validateAsType )
			{
				#region Boolean

				case ValidateAsType.Boolean:
					type = typeof( System.Boolean );
					break;

				#endregion // Boolean

				#region Byte

				case ValidateAsType.Byte:
					type = typeof( System.SByte );
					break;

				#endregion // Byte

				#region DateTime

				case ValidateAsType.DateTime:
					type = typeof( DateTime );
					break;

				#endregion // DateTime

				#region Decimal

				case ValidateAsType.Decimal:
					type = typeof( System.Decimal );
					break;

				#endregion // Decimal

				#region Double

				case ValidateAsType.Double:
					type = typeof( System.Double );
					break;

				#endregion // Double

				#region Float

				case ValidateAsType.Float:
					type = typeof( System.Single );
					break;

				#endregion // Float

				#region Integer16

				case ValidateAsType.Integer16:
					type = typeof( System.Int16 );
					break;

				#endregion // Integer16

				#region Integer32

				case ValidateAsType.Integer32:
					type = typeof( System.Int32 );
					break;

				#endregion // Integer32

				#region Integer64, NonNegativeInteger64, NegativeInteger64, NonPositiveInteger64, PositiveInteger64

				case ValidateAsType.Integer64:
				case ValidateAsType.NegativeInteger64:
				case ValidateAsType.NonNegativeInteger64:
				case ValidateAsType.NonPositiveInteger64:
				case ValidateAsType.PositiveInteger64:
					type = typeof( System.Int64 );
					break;

				#endregion // Integer64, NegativeInteger64, NonNegativeInteger64, NonPositiveInteger64, PositiveInteger64

				#region Text

				case ValidateAsType.Text:
					type = typeof( System.String );
					break;

				#endregion // Text

				#region TimeSpan

				case ValidateAsType.TimeSpan:
					type = typeof( TimeSpan );
					break;

				#endregion // TimeSpan

				#region Unknown

				case ValidateAsType.Unknown:
					type = typeof( System.Object );
					break;

				#endregion // Unknown, [default]

				#region UnsignedByte

				case ValidateAsType.UnsignedByte:
					type = typeof( System.Byte );
					break;

				#endregion // UnsignedByte

				#region UnsignedInteger16

				case ValidateAsType.UnsignedInteger16:
					type = typeof( System.UInt16 );
					break;

				#endregion // UnsignedInteger16

				#region UnsignedInteger32

				case ValidateAsType.UnsignedInteger32:
					type = typeof( System.UInt32 );
					break;

				#endregion // UnsignedInteger32

				#region UnsignedInteger64

				case ValidateAsType.UnsignedInteger64:
					type = typeof( System.UInt64 );
					break;

				#endregion // UnsignedInteger64

				#region Uri

				case ValidateAsType.Uri:
					type = typeof( System.Uri );
					break;

				#endregion // Uri

				#region [default]

				default:
					Debug.Assert( false,"Unrecognized ValidateAsType value: " + validateAsType.ToString( ) );
					break;

				#endregion // [default]
			}

			return type;
		}

		#endregion // GetTypeFromValidateAsType [static]

		#region GetValidateAsTypeFromType [static]

		/// <summary>
		/// Returns a <see cref="ValidateAsType"/> value which represents the <see cref="System.Type"/> argument.
		/// </summary>
		/// <param name="type">A Type object for which the corresponding ValidateAsType is needed.</param>
		public static ValidateAsType GetValidateAsTypeFromType( Type type )
		{
			if ( type == null )
				throw new ArgumentNullException( "type" );

			ValidateAsType validateAsType;

			switch ( type.FullName )
			{
				#region Boolean

				case "System.Boolean":
					validateAsType = ValidateAsType.Boolean;
					break;

				#endregion // Boolean

				#region Byte

				case "System.Byte":
					validateAsType = ValidateAsType.UnsignedByte;
					break;

				#endregion // Byte

				#region DateTime

				case "System.DateTime":
					validateAsType = ValidateAsType.DateTime;
					break;

				#endregion // DateTime

				#region Decimal

				case "System.Decimal":
					validateAsType = ValidateAsType.Decimal;
					break;

				#endregion // Decimal

				#region Double

				case "System.Double":
					validateAsType = ValidateAsType.Double;
					break;

				#endregion // Double

				#region Int16

				case "System.Int16":
					validateAsType = ValidateAsType.Integer16;
					break;

				#endregion // Int16

				#region Int32

				case "System.Int32":
					validateAsType = ValidateAsType.Integer32;
					break;

				#endregion // Int32

				#region Int64

				case "System.Int64":
					validateAsType = ValidateAsType.Integer64;
					break;

				#endregion // Int64

				#region SByte

				case "System.SByte":
					validateAsType = ValidateAsType.Byte;
					break;

				#endregion // SByte

				#region Single

				case "System.Single":
					validateAsType = ValidateAsType.Float;
					break;

				#endregion // Single

				#region String

				case "System.String":
					validateAsType = ValidateAsType.Text;
					break;

				#endregion // String

				#region TimeSpan

				case "System.TimeSpan":
					validateAsType = ValidateAsType.TimeSpan;
					break;

				#endregion // TimeSpan

				#region UInt16

				case "System.UInt16":
					validateAsType = ValidateAsType.UnsignedInteger16;
					break;

				#endregion // UInt16

				#region UInt32

				case "System.UInt32":
					validateAsType = ValidateAsType.UnsignedInteger32;
					break;

				#endregion // UInt32

				#region UInt64

				case "System.UInt64":
					validateAsType = ValidateAsType.UnsignedInteger64;
					break;

				#endregion // UInt64

				#region Uri

				case "System.Uri":
					validateAsType = ValidateAsType.Uri;
					break;

				#endregion // Uri

				#region [default]

				default:
					validateAsType = ValidateAsType.Unknown;
					break;

				#endregion // [default]
			}

			return validateAsType;
		}

		#endregion // GetValidateAsTypeFromType [static]

		#region HasAnyConstraints

		/// <summary>
		/// Returns true if any of the constraint properties are set on this object or if the ValidateAsType
		/// property is set to something other than 'Unknown'.
		/// </summary>
		public bool HasAnyConstraints
		{
			get
			{
				// Note, this property does not currently consider if the HasTotalDigits or HasFractionDigits
				// properties return true.  This is because those properties are not currently public.  If we
				// make them public, then those properties should be added to this list.
				//
				return
					this.HasEnumeration ||
					this.HasFixedValue ||
					this.HasMaxExclusive ||
					this.HasMaxInclusive ||
					this.HasMaxLength ||
					this.HasMinExclusive ||
					this.HasMinInclusive ||
					this.HasMinLength ||
					this.HasNullable ||
					this.HasRegexPattern ||
					// If the ValidateAsType property is set to a known type then there might be
					// implicit constraints imposed by that type.  In that case, we must return true.
					//
					this.ValidateAsType != ValidateAsType.Unknown;
			}
		}

		#endregion // HasAnyConstraints

		#region InitializeFrom Overloads

		#region InitializeFrom( ValueConstraint, bool )

		/// <summary>
		/// Copies the state of the argument into this object.
		/// </summary>
		/// <param name="source">The ValueConstraint to copy the state of.</param>
		/// <param name="copyEnumeration">Pass false if the 'enumeration' field should not be copied.</param>
		public void InitializeFrom( ValueConstraint source, bool copyEnumeration )
		{
			if ( copyEnumeration )
				this.Enumeration = source.Enumeration;

			this.FixedValue = source.FixedValue;
			this.FractionDigits = source.FractionDigits;
			this.MaxExclusive = source.MaxExclusive;
			this.MaxInclusive = source.MaxInclusive;
			this.MaxLength = source.MaxLength;
			this.MinExclusive = source.MinExclusive;
			this.MinInclusive = source.MinInclusive;
			this.MinLength = source.MinLength;
			this.Nullable = source.Nullable;
			this._regex = source._regex;
			this.RegexPattern = source.RegexPattern;
			this.TotalDigits = source.TotalDigits;
			this.ValidateAsType = source.ValidateAsType;
		}

		#endregion // InitializeFrom( ValueConstraint, bool )

		#region InitializeFrom( ValueConstraint )

		/// <summary>
		/// Copies the state of the argument into this object.
		/// </summary>
		/// <param name="source">The ValueConstraint to copy the state of.</param>
		public void InitializeFrom( ValueConstraint source )
		{
			this.InitializeFrom( source, true );
		}

		#endregion // InitializeFrom( ValueConstraint )

		#endregion // InitializeFrom Overloads

		#region Merge Overloads

		#region Merge( ValueConstraint, ValueConstraintFlags )

		/// <summary>
		/// Copies constraint settings from the source ValueConstraint to the target (i.e. the instance on which this method was invoked).
		/// Only constraint settings which have default values on the target will be assigned values from the source.
		/// Only constraint settings specified by the 'constraintFlags' flags argument will be copied.
		/// Note, this method does not copy the ValidateAsType property.
		/// </summary>
		/// <param name="source">A ValueConstraint object which contains constraint settings to be copied.</param>
		/// <param name="constraintFlags">A bit flag which specifies the constraint settings to be copied.</param>
		public void Merge( ValueConstraint source, ValueConstraintFlags constraintFlags )
		{
			this.MergeHelper( source, constraintFlags );
		}

		#endregion // Merge( ValueConstraint, ValueConstraintFlags )

		#region Merge( ValueConstraint )

		/// <summary>
		/// Copies constraint settings from the source ValueConstraint to the target (i.e. the instance on which this method was invoked).
		/// Only constraint settings which have default values on the target will be assigned values from the source.
		/// Note, this method does not copy the ValidateAsType property.
		/// </summary>
		/// <param name="source">A ValueConstraint object which contains constraint settings to be copied.</param>
		public void Merge( ValueConstraint source )
		{
			this.MergeHelper( source, ValueConstraintFlags.All );
		}

		#endregion // Merge( ValueConstraint )

		#endregion // Merge Overloads

		#region Reset Overloads

		#region Reset( ValueConstraintFlags )

		/// <summary>
		/// Resets all of the constraint settings in this object to their initial (not set) state
		/// which are specified in the 'constraintFlags' bit flag argument.
		/// Note, this method does not reset the ValidateAsType property.
		/// </summary>
		/// <param name="constraintFlags">A bit flag which specifies which constraint settings to reset.</param>
		public void Reset( ValueConstraintFlags constraintFlags )
		{
			if ( ( ValueConstraintFlags.Enumeration & constraintFlags ) != 0 )
				this.ResetEnumeration( );

			if ( ( ValueConstraintFlags.FixedValue & constraintFlags ) != 0 )
				this.ResetFixedValue( );

			if ( ( ValueConstraintFlags.MaxExclusive & constraintFlags ) != 0 )
				this.ResetMaxExclusive( );

			if ( ( ValueConstraintFlags.MaxInclusive & constraintFlags ) != 0 )
				this.ResetMaxInclusive( );

			if ( ( ValueConstraintFlags.MaxLength & constraintFlags ) != 0 )
				this.ResetMaxLength( );

			if ( ( ValueConstraintFlags.MinExclusive & constraintFlags ) != 0 )
				this.ResetMinExclusive( );

			if ( ( ValueConstraintFlags.MinInclusive & constraintFlags ) != 0 )
				this.ResetMinInclusive( );

			if ( ( ValueConstraintFlags.MinLength & constraintFlags ) != 0 )
				this.ResetMinLength( );

			if ( ( ValueConstraintFlags.Nullable & constraintFlags ) != 0 )
				this.ResetNullable( );

			if ( ( ValueConstraintFlags.RegexPattern & constraintFlags ) != 0 )
				this.ResetRegexPattern( );

			// If TotalDigits or FractionDigits are added to the public interface, uncomment these lines.
			//
			//			if( (ValueConstraintFlags.TotalDigits & constraintFlags) != 0 )
			//				this.ResetTotalDigits();
			//
			//			if( (ValueConstraintFlags.FractionDigits & constraintFlags) != 0 )
			//				this.ResetFractionDigits();
		}

		#endregion // Reset( ValueConstraintFlags )

		#region Reset()

		/// <summary>
		/// Resets all of the constraint settings in this object to their initial (not set) state.
		/// Note, this method does not reset the ValidateAsType property.
		/// </summary>
		public void Reset( )
		{
			this.Reset( ValueConstraintFlags.All );
		}

		#endregion // Reset()

		#endregion // Reset Overloads

		#region Validate Overloads

		#region Validate( object, Type, ValueConstraintFlags, IFormatProvider, string )

		/// <summary>
		/// Returns true if the argument is considered valid with regards to the current constraint settings.
		/// </summary>
		/// <param name="dataValue">
		/// The data value to validate.
		/// </param>
		/// <param name="targetType">
		/// A Type object representing the data type which the 'dataValue' should be validated as.
		/// </param>
		/// <param name="constraintFlags">
		/// A bit flag which indicates which constraints to apply during validation.
		/// </param>
		/// <param name="formatProvider">
		/// An IFormatProvider used when converting values to and from strings.
		/// </param>
		/// <param name="format">
		///	A format string to use when converting values to and from strings.
		/// </param>
		/// <param name="errorMessage">
		/// If the data value is invalid, this out param will reference an error message.
		/// </param>
		/// <returns>Returns true if the input value satisfies all of the applicable constraints set on this object, else false.</returns>
		public bool Validate( object dataValue, Type targetType, ValueConstraintFlags constraintFlags, IFormatProvider formatProvider, string format, ref string errorMessage )
		{
			if ( targetType == null )
				throw new ArgumentNullException( "targetType" );

			// If the data value is null or DBNull then we have no further processing to do.
			// If the data value is an empty string and the target type is not String, then the value is
			// considered to be null.
			//
			bool isNull = dataValue == null || dataValue == DBNull.Value;
			bool isEmptyString = !isNull && dataValue is System.String && ((string)dataValue).Length == 0;

			// MRS 11/11/05 - BR07578
			//if( isNull || (isEmptyString && targetType != typeof(string)) )
			if ( isNull || isEmptyString )
			{
				// Explicitly check if Nullable has been set to False, because we need to treat
				// Default and True as both meaning True.
				//
				bool isNullable = this.Nullable ?? true;
				if ( ! isNullable )
				{
					// "Value cannot be null."
					errorMessage = ValueInput.GetString("LMSG_ValueConstraint_Nullable");
				}

				return isNullable;
			}

			// Validate against the implicit constraints imposed by the ValidateAsType value which corresponds
			// to the targetType parameter.
			//
			if ( ( constraintFlags & ValueConstraintFlags.ImplicitTypeParameterConstraints ) != 0 )
			{
				ValidateAsType validateAsTypeFromTypeParam = ValueConstraint.GetValidateAsTypeFromType( targetType );

				if ( !this.ValidateImplicitConstraints( dataValue, validateAsTypeFromTypeParam, formatProvider, format, ref errorMessage ) )
					return false;
			}

			// Validate against the implicit constraints imposed by the ValidateAsType property.
			//
			if ( ( constraintFlags & ValueConstraintFlags.ImplicitValueAsTypeConstraints ) != 0 )
			{
				if ( !this.ValidateImplicitConstraints( dataValue, this.ValidateAsType, formatProvider, format, ref errorMessage ) )
					return false;
			}

			// Validate the input value against the explicit constraints using the Type argument.
			//
			if ( !this.ValidateExplicitConstraints( dataValue, targetType, constraintFlags, formatProvider, format, ref errorMessage ) )
				return false;

			Type typeFromValueAsType = ValueConstraint.GetTypeFromValidateAsType( this.ValidateAsType );

			// Validate the input value against the explicit constraints using the Type argument mapped to this.ValidateAsType.
			//
			if ( !this.ValidateExplicitConstraints( dataValue, typeFromValueAsType, constraintFlags, formatProvider, format, ref errorMessage ) )
				return false;

			return true;
		}

		#endregion // Validate( object, Type, ValueConstraintFlags, IFormatProvider, string )

		#region Validate( object, Type, ValueConstraintFlags )

		/// <summary>
		/// Returns true if the argument is considered valid with regards to the current constraint settings.
		/// </summary>
		/// <param name="dataValue">
		/// The data value to validate.
		/// </param>
		/// <param name="targetType">
		/// A Type object representing the data type which the 'dataValue' should be validated as. 
		/// </param>
		/// <param name="constraintFlags">
		/// A bit flag which indicates which constraints to apply during validation.
		/// </param>
		/// <returns>Returns true if the input value satisfies all of the applicable constraints set on this object, else false.</returns>
		public bool Validate( object dataValue, Type targetType, ValueConstraintFlags constraintFlags )
		{
			string notUsed = null;
			return this.Validate( dataValue, targetType, constraintFlags, null, null, ref notUsed );
		}

		#endregion // Validate( object, Type, ValueConstraintFlags )

		#region Validate( object, Type )

		/// <summary>
		/// Returns true if the argument is considered valid with regards to the current constraint settings.
		/// This overload uses <see cref="ValueConstraintFlags"/> <b>All</b>.
		/// </summary>
		/// <param name="dataValue">
		/// The data value to validate.
		/// </param>
		/// <param name="targetType">
		/// A Type object representing the data type which the 'dataValue' should be validated as. 
		/// </param>
		/// <returns>Returns true if the input value satisfies all of the applicable constraints set on this object, else false.</returns>
		public bool Validate( object dataValue, Type targetType )
		{
			return this.Validate( dataValue, targetType, ValueConstraintFlags.All );
		}

		#endregion // Validate( object, Type )

		#endregion // Validate Overloads

		#endregion // Public Interface

		#region Merge Logic

		#region MergeHelper



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

		private void MergeHelper( ValueConstraint source, ValueConstraintFlags constraintFlags )
		{
			if ( source == null || constraintFlags == 0 )
				return;

			if ( this.ShouldMergeConstraint( ValueConstraintFlags.Enumeration, constraintFlags, source ) )
				this.Enumeration = source.Enumeration;

			if ( this.ShouldMergeConstraint( ValueConstraintFlags.FixedValue, constraintFlags, source ) )
				this.FixedValue = source.FixedValue;

			if ( this.ShouldMergeConstraint( ValueConstraintFlags.MaxExclusive, constraintFlags, source ) )
				this.MaxExclusive = source.MaxExclusive;

			if ( this.ShouldMergeConstraint( ValueConstraintFlags.MaxInclusive, constraintFlags, source ) )
				this.MaxInclusive = source.MaxInclusive;

			if ( this.ShouldMergeConstraint( ValueConstraintFlags.MaxLength, constraintFlags, source ) )
				this.MaxLength = source.MaxLength;

			if ( this.ShouldMergeConstraint( ValueConstraintFlags.MinExclusive, constraintFlags, source ) )
				this.MinExclusive = source.MinExclusive;

			if ( this.ShouldMergeConstraint( ValueConstraintFlags.MinInclusive, constraintFlags, source ) )
				this.MinInclusive = source.MinInclusive;

			if ( this.ShouldMergeConstraint( ValueConstraintFlags.MinLength, constraintFlags, source ) )
				this.MinLength = source.MinLength;

			if ( this.ShouldMergeConstraint( ValueConstraintFlags.Nullable, constraintFlags, source ) )
				this.Nullable = source.Nullable;

			if ( this.ShouldMergeConstraint( ValueConstraintFlags.RegexPattern, constraintFlags, source ) )
				this.RegexPattern = source.RegexPattern;
		}

		#endregion // MergeHelper

		#region ShouldMergeConstraint



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

		private bool ShouldMergeConstraint( ValueConstraintFlags targetConstraint, ValueConstraintFlags constraintFlags, ValueConstraint source )
		{
			bool test = ( constraintFlags & targetConstraint ) != 0;

			if ( !test )
				return false;

			// If the bit flag indicates that a constraint should be merged,
			// make sure that the corresponding property has a meaningful value as well.
			//
			switch ( targetConstraint )
			{
				case ValueConstraintFlags.Enumeration:
					test = !this.HasEnumeration && source.HasEnumeration;
					break;

				case ValueConstraintFlags.FixedValue:
					test = !this.HasFixedValue && source.HasFixedValue;
					break;

				case ValueConstraintFlags.MaxExclusive:
					test = !this.HasMaxExclusive && source.HasMaxExclusive;
					break;

				case ValueConstraintFlags.MaxInclusive:
					test = !this.HasMaxInclusive && source.HasMaxInclusive;
					break;

				case ValueConstraintFlags.MaxLength:
					test = !this.HasMaxLength && source.HasMaxLength;
					break;

				case ValueConstraintFlags.MinExclusive:
					test = !this.HasMinExclusive && source.HasMinExclusive; ;
					break;

				case ValueConstraintFlags.MinInclusive:
					test = !this.HasMinInclusive && source.HasMinInclusive;
					break;

				case ValueConstraintFlags.MinLength:
					test = !this.HasMinLength && source.HasMinLength;
					break;

				case ValueConstraintFlags.Nullable:
					test = !this.HasNullable && source.HasNullable;
					break;

				case ValueConstraintFlags.RegexPattern:
					test = !this.HasRegexPattern && source.HasRegexPattern;
					break;

				default:
					Debug.Assert( false,"Unexpected ValueConstraintFlags value.  targetConstraint = " + targetConstraint.ToString( ) );
					test = false;
					break;
			}

			return test;
		}

		#endregion // ShouldMergeConstraint

		#endregion Merge Logic

		#region Validation Logic

		#region ComparisonResult [enum]

		private enum ComparisonResult
		{
			Invalid,
			Equal,
			LessThan,
			GreaterThan
		}

		#endregion // ComparisonResult [enum]

		#region EnsureNumericValueIsWithinBoundsOfDecimal



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		private object EnsureNumericValueIsWithinBoundsOfDecimal( object constraintValue )
		{
			if ( constraintValue is double && (double)constraintValue < (double)decimal.MinValue ||
				constraintValue is float && (float)constraintValue < (float)decimal.MinValue )
			{
				constraintValue = decimal.MinValue;
			}
			else if ( constraintValue is double && (double)constraintValue > (double)decimal.MaxValue ||
					 constraintValue is float && (float)constraintValue > (float)decimal.MaxValue )
			{
				constraintValue = decimal.MaxValue;
			}

			return constraintValue;
		}

		#endregion // EnsureNumericValueIsWithinBoundsOfDecimal

		#region PerformComparison


#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

		private ComparisonResult PerformComparison( object dataValue, object constraintValue, Type targetType, IFormatProvider formatProvider, string formatString )
		{
			ComparisonResult comparisonResult;
			try
			{
				object dataValueAsTargetType = CoreUtilities.ConvertDataValue(
					dataValue,
					targetType,
					formatProvider,
					formatString );

				object constraintValueAsTargetType = CoreUtilities.ConvertDataValue(
					constraintValue,
					targetType,
					formatProvider,
					formatString );

				// If the values to compare could not both be converted to the same data type,
				// then this comparison is invalid.
				//
				if ( dataValueAsTargetType == null || constraintValueAsTargetType == null )
					comparisonResult = ComparisonResult.Invalid;
				else
				{
					IComparable comparableDataValue = dataValueAsTargetType as IComparable;
					if ( comparableDataValue != null )
					{
						int res = comparableDataValue.CompareTo( constraintValueAsTargetType );
						if ( res == 0 )
							comparisonResult = ComparisonResult.Equal;
						else if ( res < 0 )
							comparisonResult = ComparisonResult.LessThan;
						else
							comparisonResult = ComparisonResult.GreaterThan;
					}
					else
						comparisonResult = ComparisonResult.Invalid;
				}
			}
			catch ( Exception ex )
			{
				Debug.Assert( false,"Exception thrown while converting values in ValueConstraint.PerformComparison().  Exception Message: " + ex.Message );
				comparisonResult = ComparisonResult.Invalid;
			}

			return comparisonResult;
		}

		#endregion // PerformComparison

		#region PerformTypeSensitiveComparison



#region Infragistics Source Cleanup (Region)

















#endregion // Infragistics Source Cleanup (Region)

		private ComparisonResult PerformTypeSensitiveComparison( object convertedDataValue, object constraintValue, Type targetType, IFormatProvider formatProvider, string formatString )
		{
			ComparisonResult result;

			if ( Utils.IsNumericType( convertedDataValue.GetType( ) ) || Utils.IsNumericType( constraintValue.GetType( ) ) )
			{
				// JAS 4/15/05
				// Make sure that the constraining value is not larger or smaller than a Decimal can be.
				//
				constraintValue = this.EnsureNumericValueIsWithinBoundsOfDecimal( constraintValue );

				result = this.PerformComparison( convertedDataValue, constraintValue, typeof( Decimal ), formatProvider, formatString );
			}
			else if ( convertedDataValue is DateTime || constraintValue is DateTime )
			{
				result = this.PerformComparison( convertedDataValue, constraintValue, typeof( DateTime ), formatProvider, formatString );
			}
			else if ( convertedDataValue is Boolean || constraintValue is Boolean )
			{
				result = this.PerformComparison( convertedDataValue, constraintValue, typeof( Boolean ), formatProvider, formatString );
			}
			else
			{
				result = this.PerformComparison( convertedDataValue, constraintValue, targetType, formatProvider, formatString );
			}

			return result;
		}

		#endregion // PerformTypeSensitiveComparison

		#region ShouldTestConstraint



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

		private bool ShouldTestConstraint( ValueConstraintFlags targetConstraint, ValueConstraintFlags constraintFlags )
		{
			bool test = ( constraintFlags & targetConstraint ) != 0;

			if ( !test )
				return false;

			// If the bit flag indicates that a constraint should be tested,
			// make sure that the corresponding property has a meaningful value as well.
			//
			switch ( targetConstraint )
			{
				case ValueConstraintFlags.Enumeration:
					test = this.HasEnumeration;
					break;

				case ValueConstraintFlags.FixedValue:
					test = this.HasFixedValue;
					break;

				case ValueConstraintFlags.MaxExclusive:
					test = this.HasMaxExclusive;
					break;

				case ValueConstraintFlags.MaxInclusive:
					test = this.HasMaxInclusive;
					break;

				case ValueConstraintFlags.MaxLength:
					test = this.HasMaxLength;
					break;

				case ValueConstraintFlags.MinExclusive:
					test = this.HasMinExclusive;
					break;

				case ValueConstraintFlags.MinInclusive:
					test = this.HasMinInclusive;
					break;

				case ValueConstraintFlags.MinLength:
					test = this.HasMinLength;
					break;

				case ValueConstraintFlags.Nullable:
					test = this.HasNullable;
					break;

				case ValueConstraintFlags.RegexPattern:
					test = this.HasRegexPattern;
					break;

				default:
					Debug.Assert( false, "Unexpected ValueConstraintFlags value.  targetConstraint = " + targetConstraint.ToString( ) );
					test = false;
					break;
			}

			return test;
		}

		#endregion // ShouldTestConstraint

		#region ValidateImplicitConstraints



#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

		private bool ValidateImplicitConstraints( object dataValue, ValidateAsType validateAsType, IFormatProvider formatProvider, string formatString, ref string errorMsg )
		{
			// Get a "hard" .NET type from the validateAsType so that we can attempt to convert the dataValue.
			//
			Type targetType = ValueConstraint.GetTypeFromValidateAsType( validateAsType );
			Debug.Assert( targetType != null, "'targetType' is null but it should never be." );

			// Convert the dataValue to an object of the target type.
			//
			object convertedValue = CoreUtilities.ConvertDataValue( dataValue, targetType, formatProvider, formatString );

			// If the dataValue could not be converted to the target type, then it is considered invalid.
			//
			if ( convertedValue == null )
			{
				// "Value could not be converted to {0}."
				errorMsg = ValueInput.GetString("LMSG_ValueConstraint_CannotConvert", targetType.FullName);
				return false;
			}

			// We use this later on in multiple cases.
			//
			IComparable comparable = convertedValue as IComparable;

			bool isValid = true;

			switch ( validateAsType )
			{
				#region Boolean

				case ValidateAsType.Boolean:
					// The only requirement for Boolean is that the dataValue could be converted to a bool.
					// If we get here, that means that the conversion was successful.
					break;

				#endregion // Boolean

				#region Byte

				case ValidateAsType.Byte:
					// The only requirement for Byte is that the dataValue could be converted to an SByte.
					// If we get here, that means that the conversion was successful.
					break;

				#endregion // Byte

				#region DateTime

				case ValidateAsType.DateTime:
					// The only requirement for DateTime is that the dataValue could be converted to a DateTime.
					// If we get here, that means that the conversion was successful.
					break;

				#endregion // DateTime

				#region Decimal

				case ValidateAsType.Decimal:
					// The only requirement for Decimal is that the dataValue could be converted to a decimal.
					// If we get here, that means that the conversion was successful.
					break;

				#endregion // Decimal

				#region Double

				case ValidateAsType.Double:
					// The only requirement for Double is that the dataValue could be converted to a double.
					// If we get here, that means that the conversion was successful.
					break;

				#endregion // Double

				#region Float

				case ValidateAsType.Float:
					// The only requirement for Float is that the dataValue could be converted to a float.
					// If we get here, that means that the conversion was successful.
					break;

				#endregion // Float

				#region Integer16

				case ValidateAsType.Integer16:
					// The only requirement for Integer16 is that the dataValue could be converted to an Int16.
					// If we get here, that means that the conversion was successful.
					break;

				#endregion // Integer16

				#region Integer32

				case ValidateAsType.Integer32:
					// The only requirement for Integer32 is that the dataValue could be converted to an Int32.
					// If we get here, that means that the conversion was successful.
					break;

				#endregion // Integer32

				#region Integer64

				case ValidateAsType.Integer64:
					// The only requirement for Integer64 is that the dataValue could be converted to an Int64.
					// If we get here, that means that the conversion was successful.
					break;

				#endregion // Integer64

				#region NegativeInteger64

				case ValidateAsType.NegativeInteger64:
					if ( comparable != null )
					{
						// Range: [Int64.MinValue, -1]
						//
						bool isLessThanZero = comparable.CompareTo( (Int64)0 ) < 0;
						isValid = isValid && isLessThanZero;

						if ( !isValid )
						{
							// "Value must be within the range of {0} and {1}."
							errorMsg = ValueInput.GetString(
								"LMSG_ValueConstraint_OutOfRange",
								Int64.MinValue.ToString( formatString, formatProvider ),
								"-1" );
						}
					}
					else
					{
						Debug.Assert( false,"The 'convertedValue' does not implement IComparable!  It's type is: " + convertedValue.GetType( ).Name );
					}
					break;

				#endregion // NegativeInteger64

				#region NonNegativeInteger64

				case ValidateAsType.NonNegativeInteger64:
					if ( comparable != null )
					{
						// Range: [0, Int64.MaxValue]
						//
						bool isGreaterThanNegativeOne = comparable.CompareTo( (Int64)( -1 ) ) > 0;
						isValid = isValid && isGreaterThanNegativeOne;

						if ( !isValid )
						{
							// "Value must be within the range of {0} and {1}."
							errorMsg = ValueInput.GetString(
								"LMSG_ValueConstraint_OutOfRange",
								"0",
								Int64.MaxValue.ToString( formatString, formatProvider ) );
						}
					}
					else
					{
						Debug.Assert( false,"The 'convertedValue' does not implement IComparable!  It's type is: " + convertedValue.GetType( ).Name );
					}
					break;

				#endregion // NonNegativeInteger64

				#region NonPositiveInteger64

				case ValidateAsType.NonPositiveInteger64:
					if ( comparable != null )
					{
						// Range: [Int64.MinValue, 0]
						//
						bool isLessThanOne = comparable.CompareTo( (Int64)( +1 ) ) < 0;
						isValid = isValid && isLessThanOne;

						if ( !isValid )
						{
							// "Value must be within the range of {0} and {1}."
							errorMsg = ValueInput.GetString(
								"LMSG_ValueConstraint_OutOfRange",
								Int64.MinValue.ToString( formatString, formatProvider ),
								"0" );
						}
					}
					else
					{
						Debug.Assert( false,"The 'convertedValue' does not implement IComparable!  It's type is: " + convertedValue.GetType( ).Name );
					}
					break;

				#endregion // NonPositiveInteger64

				#region PositiveInteger64

				case ValidateAsType.PositiveInteger64:
					if ( comparable != null )
					{
						// Range: [1, Int64.MaxValue]
						//
						bool isGreaterThanZero = comparable.CompareTo( (Int64)0 ) > 0;
						isValid = isValid && isGreaterThanZero;

						if ( !isValid )
						{
							// "Value must be within the range of {0} and {1}."
							errorMsg = ValueInput.GetString(
								"LMSG_ValueConstraint_OutOfRange",
								"1",
								Int64.MaxValue.ToString( formatString, formatProvider ) );
						}
					}
					else
					{
						Debug.Assert( false,"The 'convertedValue' does not implement IComparable!  It's type is: " + convertedValue.GetType( ).Name );
					}
					break;

				#endregion // PositiveInteger64

				#region Text

				case ValidateAsType.Text:
					// The only requirement for Text is that the dataValue could be converted to a string.
					// If we get here, that means that the conversion was successful.
					break;

				#endregion // Text

				#region TimeSpan

				case ValidateAsType.TimeSpan:
					// The only requirement for TimeSpan is that the dataValue could be converted to a TimeSpan.
					// If we get here, that means that the conversion was successful.
					break;

				#endregion // TimeSpan

				#region Unknown

				case ValidateAsType.Unknown:
					// Nothing to do here.
					break;

				#endregion // Unknown

				#region UnsignedByte

				case ValidateAsType.UnsignedByte:
					// The only requirement for UnsignedByte is that the dataValue could be converted to a Byte.
					// If we get here, that means that the conversion was successful.
					break;

				#endregion // UnsignedByte

				#region UnsignedInteger16

				case ValidateAsType.UnsignedInteger16:
					// The only requirement for UnsignedInteger16 is that the dataValue could be converted to a UInt16.
					// If we get here, that means that the conversion was successful.
					break;

				#endregion // UnsignedInteger16

				#region UnsignedInteger32

				case ValidateAsType.UnsignedInteger32:
					// The only requirement for UnsignedInteger32 is that the dataValue could be converted to a UInt32.
					// If we get here, that means that the conversion was successful.
					break;

				#endregion // UnsignedInteger32

				#region UnsignedInteger64

				case ValidateAsType.UnsignedInteger64:
					// The only requirement for UnsignedInteger64 is that the dataValue could be converted to a UInt64.
					// If we get here, that means that the conversion was successful.
					break;

				#endregion // UnsignedInteger64

				#region Uri

				case ValidateAsType.Uri:
					// The only requirement for Uri is that a System.Uri object could be created with the dataValue.
					// If we get here, that means that the object construction was successful.
					break;

				#endregion // Uri

				#region [default]

				default:
					Debug.Assert( false,"Unrecognized ValidateAsType value: " + validateAsType.ToString( ) );
					break;

				#endregion // [default]
			}

			return isValid;
		}

		#endregion // ValidateImplicitConstraints

		#region ValidateExplicitConstraints



#region Infragistics Source Cleanup (Region)
























#endregion // Infragistics Source Cleanup (Region)

		private bool ValidateExplicitConstraints( object dataValue, Type targetType, ValueConstraintFlags constraintFlags, IFormatProvider formatProvider, string formatString, ref string errorMsg )
		{
			// Convert the dataValue to an object of the target type.
			//
			object convertedDataValue = CoreUtilities.ConvertDataValue( dataValue, targetType, formatProvider, formatString );

			// If the dataValue could not be converted to the target type, then it is considered invalid.
			//
			if ( convertedDataValue == null )
			{
				// "Value could not be converted to {0}."
				errorMsg = ValueInput.GetString("LMSG_ValueConstraint_CannotConvert", targetType.FullName);
				return false;
			}

			// We use this later on in multiple places.
			//		
			string stringDataValue = convertedDataValue.ToString( );

			bool isValid = true;

			// For every explicit constraint (such as MinLength, RegexPattern, etc.) which is set AND the
			// 'constraintFlags' argument specifies, we need to test the data value to make sure that it 
			// satisfies the constraint.

			#region Enumeration

			if ( isValid && this.ShouldTestConstraint( ValueConstraintFlags.Enumeration, constraintFlags ) )
			{
				// If the valuelist does not contain the data value, then the value is invalid.
				//
				bool itemIsInList = Utils.Exists( this.Enumeration, convertedDataValue );
				if ( !itemIsInList )
				{
					isValid = false;

					// "Value not found in list of possible choices."
					errorMsg = ValueInput.GetString("LMSG_ValueConstraint_Enumeration");
				}
			}

			#endregion // Enumeration

			#region FixedValue

			if ( isValid && this.ShouldTestConstraint( ValueConstraintFlags.FixedValue, constraintFlags ) )
			{
				ComparisonResult result = this.PerformTypeSensitiveComparison(
					convertedDataValue,
					this.FixedValue,
					targetType,
					formatProvider,
					formatString );

				if ( result != ComparisonResult.Equal )
				{
					isValid = false;

					// "Value does not equal '{0}'."
					errorMsg = ValueInput.GetString("LMSG_ValueConstraint_FixedValue", this.FixedValue);
				}
			}

			#endregion // FixedValue

			#region MaxExclusive

			if ( isValid && this.ShouldTestConstraint( ValueConstraintFlags.MaxExclusive, constraintFlags ) )
			{
				ComparisonResult result = this.PerformTypeSensitiveComparison(
					convertedDataValue,
					this.MaxExclusive,
					targetType,
					formatProvider,
					formatString );

				if ( result != ComparisonResult.LessThan )
				{
					isValid = false;

					// "Value must be less than {0}."
					errorMsg = ValueInput.GetString("LMSG_ValueConstraint_MaxExclusive", this.MaxExclusive);
				}
			}

			#endregion // MaxExclusive

			#region MaxInclusive

			if ( isValid && this.ShouldTestConstraint( ValueConstraintFlags.MaxInclusive, constraintFlags ) )
			{
				ComparisonResult result = this.PerformTypeSensitiveComparison(
					convertedDataValue,
					this.MaxInclusive,
					targetType,
					formatProvider,
					formatString );

				if ( result != ComparisonResult.LessThan && result != ComparisonResult.Equal )
				{
					isValid = false;

					// "Value must be less than or equal to {0}."
					errorMsg = ValueInput.GetString("LMSG_ValueConstraint_MaxInclusive", this.MaxInclusive);
				}
			}

			#endregion // MaxInclusive

			#region MaxLength

			if ( isValid && this.ShouldTestConstraint( ValueConstraintFlags.MaxLength, constraintFlags ) )
			{
				// The MaxLength constraint is an inclusive value.
				//
				if ( this.MaxLength < stringDataValue.Length )
				{
					isValid = false;

					// "Value must contain no more than {0} characters."
					errorMsg = ValueInput.GetString("LMSG_ValueConstraint_MaxLength", this.MaxLength.ToString(formatString, formatProvider));
				}
			}

			#endregion // MaxLength

			#region MinExclusive

			if ( isValid && this.ShouldTestConstraint( ValueConstraintFlags.MinExclusive, constraintFlags ) )
			{
				ComparisonResult result = this.PerformTypeSensitiveComparison(
					convertedDataValue,
					this.MinExclusive,
					targetType,
					formatProvider,
					formatString );

				if ( result != ComparisonResult.GreaterThan )
				{
					isValid = false;

					// "Value must be greater than {0}."
					errorMsg = ValueInput.GetString("LMSG_ValueConstraint_MinExclusive", this.MinExclusive);
				}
			}

			#endregion // MinExclusive

			#region MinInclusive

			if ( isValid && this.ShouldTestConstraint( ValueConstraintFlags.MinInclusive, constraintFlags ) )
			{
				ComparisonResult result = this.PerformTypeSensitiveComparison(
					convertedDataValue,
					this.MinInclusive,
					targetType,
					formatProvider,
					formatString );

				if ( result != ComparisonResult.GreaterThan && result != ComparisonResult.Equal )
				{
					isValid = false;

					// "Value must be greater than or equal to {0}."
					errorMsg = ValueInput.GetString("LMSG_ValueConstraint_MinInclusive", this.MinInclusive);
				}
			}

			#endregion // MinInclusive

			#region MinLength

			if ( isValid && this.ShouldTestConstraint( ValueConstraintFlags.MinLength, constraintFlags ) )
			{
				// The MinLength constraint is an inclusive value.
				//
				if ( stringDataValue.Length < this.MinLength )
				{
					isValid = false;

					// "Value must contain at least {0} characters."
					errorMsg = ValueInput.GetString("LMSG_ValueConstraint_MinLength", this.MinLength.ToString(formatString, formatProvider));
				}
			}

			#endregion // MinLength

			#region Nullable

			if ( isValid && this.ShouldTestConstraint( ValueConstraintFlags.Nullable, constraintFlags ) )
			{
				// We handle the check for nullability in the Validate method, so there is nothing to do here.
				// If the data value is null, then we never should have hit this code.
				Debug.Assert( convertedDataValue != null, "If the converted data value is null, we should not be testing for nullability." );
			}

			#endregion // Nullable

			#region RegexPattern

			if ( isValid && this.ShouldTestConstraint( ValueConstraintFlags.RegexPattern, constraintFlags ) )
			{
				//Debug.Assert( this._regex != null, "'_regex' should not be null if there is a pattern to test, but it is." );

				// This should never be the case, but just in case the _regex is null, create a new one.
				// The _regex object should always be created in the RegexPattern property's setter.
				//
				if ( this._regex == null )
				{
					string pattern = this.RegexPattern;

					// SSP 6/14/12 TFS104747
					// Apparently further below we are ensuring, incorrectly, that all the input text matches 
					// the pattern. We should not have done so however in the interest of not breaking existing 
					// applications we'll retain that behavior. However in order to ensure the entire input 
					// text matches the regular expression, we have to prefix and postfix it with '^' and '$'
					// characters which are regular expression characters that specify that the text being 
					// matched much match from the start and to the end respectively. Also apparently it's
					// necessary to enclose the pattern in '(' and ')' characters otherwise patterns where
					// '|' operator is used does not work - like "a|ab" pattern "^a|ab$" doesn't work where as
					// "^(a|ab)$" works. I'm not sure why but doing so should not cause any issues.
					// 
					// ----------------------------------------------------------------------------------------
					if ( !pattern.StartsWith( "^" ) && !pattern.EndsWith( "$" ) )
						pattern = "^(" + pattern + ")$";
					// ----------------------------------------------------------------------------------------

					//Debug.Assert( false,"The ValueConstraint's RegexPattern has been set, but the cached Regex object does not exist.  Why?" );
					try { this._regex = new Regex( pattern ); }
					catch { return true; }
				}

				// JAS 5/19/05 BR04139 - Instead of finding matches anywhere in the string
				// we only want to find a match which is the entire string.
				//
				Match match = _regex.Match( stringDataValue );

				bool isCompleteMatch =
					match.Success &&
					match.Index == 0 &&
					match.Length == stringDataValue.Length;

				//if( ! this._regex.IsMatch( stringDataValue ) )
				if ( !isCompleteMatch )
				{
					isValid = false;

					// "Value does not match the required pattern."
					errorMsg = ValueInput.GetString("LMSG_ValueConstraint_RegexPattern");
				}
			}

			#endregion // RegexPattern

			return isValid;
		}

		#endregion // ValidateExplicitConstraints

		#endregion // Validation Logic

		#region Properties Not Exposed

		#region FractionDigits

		/// <summary>
		/// Identifies the <see cref="FractionDigits"/> dependency property
		/// </summary>
		private static readonly DependencyProperty FractionDigitsProperty = DependencyPropertyUtilities.Register(
			"FractionDigits",
			typeof( int? ),
			typeof( ValueConstraint ),
			
			
			
			
			null, new PropertyChangedCallback( OnPropertyChanged ) 
		);

		/// <summary>
		/// Currently this is not being used.
		/// </summary>
		//[Description( "" )]
		//[Category( "" )]
		[Bindable( true )]
		private int? FractionDigits
		{
			get
			{
				return (int?)this.GetValue( FractionDigitsProperty );
			}
			set
			{
				this.SetValue( FractionDigitsProperty, value );
			}
		}

		/// <summary>
		/// Returns true if the FractionDigits property is set to a non-default value.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		private bool ShouldSerializeFractionDigits( )
		{
			return DependencyPropertyUtilities.ShouldSerialize( this, FractionDigitsProperty );
		}

		/// <summary>
		/// Resets the FractionDigits property to its default state.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		private void ResetFractionDigits( )
		{
			this.ClearValue( FractionDigitsProperty );
		}

		private bool HasFractionDigits
		{
			get { return this.FractionDigits >= 0; }
		}

		#endregion // FractionDigits

		#region TotalDigits

		/// <summary>
		/// Identifies the <see cref="TotalDigits"/> dependency property
		/// </summary>
		private static readonly DependencyProperty TotalDigitsProperty = DependencyPropertyUtilities.Register(
			"TotalDigits",
			typeof( int? ),
			typeof( ValueConstraint ),
			
			
			
			
			null, new PropertyChangedCallback( OnPropertyChanged ) 
		);

		/// <summary>
		/// Currently this is not being used.
		/// </summary>
		//[Description( "" )]
		//[Category( "" )]
		[Bindable( true )]
		private int? TotalDigits
		{
			get
			{
				return (int?)this.GetValue( TotalDigitsProperty );
			}
			set
			{
				this.SetValue( TotalDigitsProperty, value );
			}
		}

		/// <summary>
		/// Returns true if the TotalDigits property is set to a non-default value.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		private bool ShouldSerializeTotalDigits( )
		{
			return DependencyPropertyUtilities.ShouldSerialize( this, TotalDigitsProperty );
		}

		/// <summary>
		/// Resets the TotalDigits property to its default state.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		private void ResetTotalDigits( )
		{
			this.ClearValue( TotalDigitsProperty );
		}

		private bool HasTotalDigits
		{
			get { return this.TotalDigits >= 0; }
		}

		#endregion // TotalDigits

		#endregion // Properties Not Exposed

		#region Methods

		#region Private Methods

		#region OnPropertyChanged

		private static void OnPropertyChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			ValueConstraint vc = (ValueConstraint)d;

			var prop = e.Property;
			var newVal = e.NewValue;

			if ( MaxLengthProperty == prop )
				ValidateMaxLength( newVal );
			else if ( MinLengthProperty == prop )
				ValidateMinLength( newVal );
			else if ( RegexPatternProperty == prop )
				ValidateRegexPattern( newVal );

			vc.RaisePropertyChanged( DependencyPropertyUtilities.GetName( prop ) );
		}

		#endregion // OnPropertyChanged

		#region RaisePropertyChanged

		private void RaisePropertyChanged( string propName )
		{
			if ( null != _propertyChangedDelegate )
				_propertyChangedDelegate( this, new PropertyChangedEventArgs( propName ) );

			if ( null != _listeners )
				_listeners.OnPropertyValueChanged( this, propName, null );
		} 

		#endregion // RaisePropertyChanged

		#endregion // Private Methods

		#endregion // Methods

		#region Events

		#region PropertyChanged

		private PropertyChangedEventHandler _propertyChangedDelegate;

		
		
		
		/// <summary>
		/// Raised whenever a property's value changes.
		/// </summary>
		// SSP 3/24/10 TFS27839
		// Implemented IPropertyChanged interface on this class and thus made PropertyChanged event public.
		// 
		//internal event PropertyChangedEventHandler PropertyChanged
		public event PropertyChangedEventHandler PropertyChanged
		{
			add
			{
				if ( null == _propertyChangedDelegate )
					_propertyChangedDelegate = value;
				else
					_propertyChangedDelegate = (PropertyChangedEventHandler)Delegate.Combine( _propertyChangedDelegate, value );
			}
			remove
			{
				if ( null != _propertyChangedDelegate )
					_propertyChangedDelegate = (PropertyChangedEventHandler)System.Delegate.Remove( _propertyChangedDelegate, value );
			}
		}

		#endregion // PropertyChanged

		#endregion // Events

		void ITypedSupportPropertyChangeNotifications<object, string>.AddListener( ITypedPropertyChangeListener<object, string> listener, bool useWeakReference )
		{
			if ( null == _listeners )
				_listeners = new PropertyChangeListenerList( );

			_listeners.Add( listener, useWeakReference );
		}

		void ITypedSupportPropertyChangeNotifications<object, string>.RemoveListener( ITypedPropertyChangeListener<object, string> listener )
		{
			if ( null != _listeners )
				_listeners.Remove( listener );
		}
	}

	#endregion // ValueConstraint Class

	#region ValidationErrorInfo Class

	// SSP 4/17/09 NAS9.2 IDataErrorInfo Support
	// Added ValidationErrorInfo class.
	
	
	
	
	/// <summary>
	/// Contains error information regarding why a value is invalid.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// When an editor value is invalid, editor's <see cref="ValueInput.IsValueValid"/> property returns
	/// false. To get the error information regarding why the value is invalid, use the editor's
	/// <see cref="ValueInput.InvalidValueErrorInfo"/> property which returns an instance of this class.
	/// </para>
	/// </remarks>
	/// <seealso cref="ValueInput.InvalidValueErrorInfo"/>
	/// <seealso cref="ValueInput.IsValueValid"/>
	public class ValidationErrorInfo : PropertyChangeNotifier
	{
		#region Member Vars

		private Exception _exception;
		private string _errorMessage;

		#endregion // Member Vars

		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="exception">Exception if any.</param>
		public ValidationErrorInfo( Exception exception )
		{
			_exception = exception;
			_errorMessage = exception.Message;
		}

		#endregion // Constructor

		#region ErrorMessage

		/// <summary>
		/// Error message indicating why the value is invalid.
		/// </summary>
		public string ErrorMessage
		{
			get
			{
				return _errorMessage;
			}
		}

		#endregion // ErrorMessage

		#region Exception

		/// <summary>
		/// Returns the exception if any that caused validation error.
		/// </summary>
		public Exception Exception
		{
			get
			{
				return _exception;
			}
		}

		#endregion // Exception
	}

	#endregion // ValidationErrorInfo Class
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