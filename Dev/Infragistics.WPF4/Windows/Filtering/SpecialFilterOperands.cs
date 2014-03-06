using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.Globalization;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Infragistics.Shared;
using Infragistics.Windows.Internal;

namespace Infragistics.Windows.Controls
{

	#region SpecialFilterOperands Class

	/// <summary>
	/// Maintains a registry of special filter operands and also exposes built-in special filter operands as static properties.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// <b>SpecialFilterOperands</b> maintains a registry of special operands. It exposes built-in special operands via static 
	/// properties, like <see cref="SpecialFilterOperands.Blanks"/>, <see cref="SpecialFilterOperands.Quarter1"/>,
	/// <seealso cref="SpecialFilterOperands.AboveAverage"/> etc...
	/// </para>
	/// <para class="body">
	/// You can use <b>SpecialFilterOperands</b> class to <see cref="SpecialFilterOperands.Register"/> and 
	/// <see cref="SpecialFilterOperands.Unregister"/> custom as well as built-in special filter operands.
	/// You can replace the logic for a built-in operand by un-registering it and then registering your own
	/// custom special operand instance with the same name. Controls and features aware of special filter operands 
	/// (like the record filtering functionality of the data presenter) will automatically integrate the registered
	/// special operands with their UI.
	/// </para>
	/// </remarks>
	/// <seealso cref="SpecialFilterOperandBase"/>
	/// <seealso cref="SpecialFilterOperands.Register"/>
	/// <seealso cref="SpecialFilterOperands.Unregister"/>
	/// <seealso cref="SpecialFilterOperands.GetRegisteredOperand( string )"/>
	public class SpecialFilterOperands
	{
		#region Nested Data Structures

		#region OperandWrapper Class

		internal class OperandWrapper : SpecialFilterOperandBase
		{
			#region Nested Data Structures

			#region OperandWrapperSerializationInfo Class

			/// <summary>
			/// Provides necessary information to the ObjectSerializerBase for serializing and deserializing a ConditionGroup.
			/// </summary>
			internal class OperandWrapperSerializationInfo : ObjectSerializationInfo
			{
				private PropertySerializationInfo[] _props;
				private const string NameProp = "Name";

				public override IEnumerable<PropertySerializationInfo> SerializedProperties
				{
					get
					{
						if ( null == _props )
						{
							_props = new PropertySerializationInfo[]
							{
								new PropertySerializationInfo( typeof( string ), NameProp )
							};
						}

						return _props;
					}
				}

				public override Dictionary<string, object> Serialize( object obj )
				{
					OperandWrapper x = (OperandWrapper)obj;

					Dictionary<string, object> values = new Dictionary<string, object>( );

					values[NameProp] = x.Name;

					return values;
				}

				public override object Deserialize( Dictionary<string, object> values )
				{
					object v;
					if ( values.TryGetValue( NameProp, out v ) )
						return new OperandWrapper( (string)v );

					return null;
				}
			}

			#endregion // OperandWrapperSerializationInfo Class

			#endregion // Nested Data Structures

			#region Member Vars

			private string _operandName;
			private int _verifiedRegisteredOperandsVersion;
			private SpecialFilterOperandBase _cachedOperand;
			
			// Holds wrappers.
			private static Dictionary<string, OperandWrapper> g_wrappers = new Dictionary<string, OperandWrapper>( );

			#endregion // Member Vars

			#region Constructor

			static OperandWrapper( )
			{
			}

			/// <summary>
			/// Constructor.
			/// </summary>
			/// <param name="operandName">Name of the operand that will be wrapped by this wrapper.</param>
			private OperandWrapper( string operandName )
			{
				_operandName = operandName;
			}

			#endregion // Constructor

            #region Base class overrides

			#region EqualsOverride

			// SSP 1/5/10 TFS25670
			// We need to override Equals on the operands so the combo editor used for drop-down
			// finds a matching entry even when the instance being searched is a different instance
			// from the one in the drop-down list. This can happen after deserialization for example.
			// 
			/// <summary>
			/// Overridden.
			/// </summary>
			/// <param name="obj"></param>
			/// <returns></returns>
			protected override bool EqualsOverride( SpecialFilterOperandBase obj )
			{
				SpecialFilterOperandBase xx = this.Source;
				SpecialFilterOperandBase yy;

				OperandWrapper wrapper = obj as OperandWrapper;
				if ( null != wrapper )
				{
					yy = wrapper.Source;

					if ( null == xx && null == yy )
						return this.Name == wrapper.Name;
				}
				else
				{
					yy = obj as SpecialFilterOperandBase;
				}

				if ( null != xx && null != yy )
					return xx.Equals( yy );

				return false;
			}

			#endregion // EqualsOverride

			#region GetHashCode

			// SSP 1/5/10 TFS25670
			// We need to override Equals on the operands so the combo editor used for drop-down
			// finds a matching entry even when the instance being searched is a different instance
			// from the one in the drop-down list. This can happen after deserialization for example.
			// 
			/// <summary>
			/// Overridden. Returns the hash code.
			/// </summary>
			/// <returns>Hash code of the object.</returns>
			/// <remarks>
			/// <para class="body">
			/// <b>Note:</b> If you override <see cref="EqualsOverride"/> then also override this method
			/// and return an appropriate hash code.
			/// </para>
			/// </remarks>
			public override int GetHashCode( )
			{
				SpecialFilterOperandBase source = this.Source;
				return null != source ? source.GetHashCode( ) : 0;
			}

			#endregion // GetHashCode

                #region ToString

            /// <summary>
            /// Returns a string representation of the operand.
            /// </summary>
            public override string ToString()
            {
				SpecialFilterOperandBase source = this.Source;

				return null != source ? source.ToString( ) : this.Name;
            }

                #endregion //ToString

			#endregion //Base class overrides

			#region Properties

			#region Private Properties

			#region Source

			private SpecialFilterOperandBase Source
			{
				get
				{
					if ( _verifiedRegisteredOperandsVersion != SpecialFilterOperands.g_registeredOperandsVersion )
					{
						_verifiedRegisteredOperandsVersion = SpecialFilterOperands.g_registeredOperandsVersion;

						SpecialFilterOperandBase operand = SpecialFilterOperands.GetRegisteredOperand( _operandName );
						if ( null == operand )
							operand = SpecialFilterOperands.GetBuiltInOperand( _operandName );

						Debug.Assert( null != operand );
						if ( null != operand )
							_cachedOperand = operand;
					}

					return _cachedOperand;
				}
			}

			#endregion // Source

			#endregion // Private Properties

			#region Public Properties

			#region Description

			public override object Description 
			{
				get
				{
					SpecialFilterOperandBase source = this.Source;
					return null != source ? source.Description : string.Empty;
				}
			}

			#endregion // Description

			#region DisplayContent

			public override object DisplayContent 
			{
				get
				{
					SpecialFilterOperandBase source = this.Source;
					return null != source ? source.DisplayContent : null;
				}
			}

			#endregion // DisplayContent

			#region Name

			public override string Name 
			{
				get
				{
					SpecialFilterOperandBase source = this.Source;
					return null != source ? source.Name : _operandName;
				}
			}

			#endregion // Name

			#region UsesAllValues

			public override bool UsesAllValues
			{
				get
				{
					SpecialFilterOperandBase source = this.Source;
					return null != source && source.UsesAllValues;
				}
			}

			#endregion // UsesAllValues

			#endregion // Public Properties

			#endregion // Properties

			#region Methods

			#region Private/Internal Methods

			internal static OperandWrapper GetWrapper( string operandName )
			{
				lock ( g_wrappers )
				{
					OperandWrapper ret;
					if ( g_wrappers.TryGetValue( operandName, out ret ) )
						return ret;

					ret = new OperandWrapper( operandName );
					Debug.Assert( null != ret.Source, string.Format( "{0} operand doesn't exist.", operandName ) );
					if ( null != ret.Source )
					{
						g_wrappers[operandName] = ret;
						return ret;
					}
				}

				return null;
			}

			#endregion // Private/Internal Methods

			#region Public Methods

			#region IsMatch

			public override bool IsMatch( ComparisonOperator comparisonOperator, object value, ConditionEvaluationContext context )
			{
				SpecialFilterOperandBase source = this.Source;
				return null != source && source.IsMatch( comparisonOperator, value, context );
			}

			#endregion // IsMatch

			#region SupportsOperator

			public override bool SupportsOperator( ComparisonOperator comparisonOperator )
			{
				SpecialFilterOperandBase source = this.Source;
				return null != source && source.SupportsOperator( comparisonOperator );
			}

			#endregion // SupportsOperator

			#region SupportsDataType

			public override bool SupportsDataType( Type type )
			{
				SpecialFilterOperandBase source = this.Source;
				return null != source && source.SupportsDataType( type );
			}

			#endregion // SupportsDataType

			#endregion // Public Methods

			#endregion // Methods
		}

		#endregion // OperandWrapper Class

		#endregion // Nested Data Structures

		#region Member Vars

		private static int g_registeredOperandsVersion = 1;

		// Holds the built-in operands.
		private static Dictionary<string, SpecialFilterOperandBase> g_builtInOperands
			= new Dictionary<string, SpecialFilterOperandBase>( );

		// Holds all the registered operands, including built-in ones (unless they were un-registered).
		private static Dictionary<string, SpecialFilterOperandBase> g_operands
			= new Dictionary<string, SpecialFilterOperandBase>( );

		#endregion // Member Vars

		#region Constructor [Static]

		static SpecialFilterOperands( )
		{
			foreach ( SpecialFilterOperandBase ii in SpecialFilterOperandFactory.GetBuiltInOperands( ) )
			{
				string name = ii.Name;
				g_builtInOperands.Add( name, ii );
				g_operands.Add( name, ii );
			}
		}

		#endregion // Constructor [Static]

		#region Constructor

		/// <summary>
		/// Constructor. Initializes a new instance of <see cref="SpecialFilterOperands"/>.
		/// </summary>
		private SpecialFilterOperands( )
		{
		}

		#endregion // Constructor

		#region Methods

		#region Private/Internal Methods

		#region GetSerializationValue

		/// <summary>
		/// If the specified operand is not one of our operands that we know about serializing, then
		/// we will create a wrapper that will serialize out the name of the operand. This will 
		/// anyone who registers custom operands don't have to also write logic to serialize them
		/// out.
		/// </summary>
		/// <param name="source">Source operand for which to create wrapper.</param>
		/// <returns>If source is one of our operands, then returns null. Otherwise creates a wrapper object.</returns>
		internal static object GetSerializationValue( SpecialFilterOperandBase source )
		{
			if ( source is SpecialFilterOperandFactory.IGOperand 
				|| source is OperandWrapper )
				return source;

			// If a custom operand has been registered and that needs to be serialized out,
			// then serialize out the name, which will allow us to find it when deserializing
			// since likely that will be also be a registered operand next time app loads.
			// 
			SpecialFilterOperandBase registeredOperand = GetRegisteredOperand( source.Name );
			if ( registeredOperand == source )
				return OperandWrapper.GetWrapper( source.Name );

			return source;
		}

		#endregion // GetSerializationValue

		#region RegisterSerializationInfos

		/// <summary>
		/// Registers serialization information for special operands defined in this factory.
		/// </summary>
		/// <param name="serializer"></param>
		internal static void RegisterSerializationInfos( ObjectSerializerBase serializer )
		{
			serializer.RegisterInfo( typeof( OperandWrapper ), new OperandWrapper.OperandWrapperSerializationInfo( ) );
		}

		#endregion // RegisterSerializationInfos

		#endregion // Private/Internal Methods

		#region Public Methods

		// AS - NA 11.2 Excel Style Filtering
		#region CreateDateRangeOperand
		/// <summary>
		/// Creates an operand used to compares dates to a range of dates based on a given relative date and the granularity to which the dates should be compared (e.g. compare the year).
		/// </summary>
		/// <param name="relativeDate">A date that will be used to calculate the range for the comparison.</param>
		/// <param name="scope">Defines how much of the date should be compared.</param>
		/// <returns>Returns an operand that can be used to compare for a specific range of dates.</returns>
		[InfragisticsFeature(Version = FeatureInfo.Version_11_2, FeatureName = FeatureInfo.FeatureName_ExcelFiltering)]
		public static SpecialFilterOperandBase CreateDateRangeOperand(DateTime relativeDate, DateRangeScope scope)
		{
			return new SpecialFilterOperandFactory.DateRangeOperand(relativeDate, scope);
		} 
		#endregion //CreateDateRangeOperand

		#region GetBuiltInOperand

		/// <summary>
		/// Gets the built-in special filter operand of specified name.
		/// </summary>
		/// <param name="name">Name of the special filter operand.</param>
		/// <returns>Returns built-in special filter operand of specified name.</returns>
		public static SpecialFilterOperandBase GetBuiltInOperand( string name )
		{
			SpecialFilterOperandBase ret;
			if ( g_builtInOperands.TryGetValue( name, out ret ) )
				return ret;

			return null;
		}

		#endregion // GetBuiltInOperand

		#region GetRegisteredOperand

		/// <summary>
		/// Returns the registered operand with the specified name. If no operand with the matching name is found, returns null.
		/// </summary>
		/// <param name="name">Finds the operand with this name.</param>
		/// <returns>Returns the found operand, or null if no operand with the specified name is found.</returns>
		public static SpecialFilterOperandBase GetRegisteredOperand( string name )
		{
			SpecialFilterOperandBase ret;

			lock ( g_operands )
			{
				g_operands.TryGetValue( name, out ret );
			}

			return ret;
		}

		#endregion // GetRegisteredOperand

		#region GetRegisteredOperands

		/// <summary>
		/// Returns all registered operands.
		/// </summary>
		/// <returns>
		/// Returns all the registered special operands, including the built-in special operands that
		/// are pre-registered during initialzation.
		/// </returns>
		public static SpecialFilterOperandBase[] GetRegisteredOperands( )
		{
			return GetRegisteredOperands( null );
		}

		/// <summary>
		/// Returns all registered operands that support the specified data type.
		/// </summary>
		/// <returns>
		/// Returns all the registered special operands, including the built-in special operands that
		/// are pre-registered during initialzation.
		/// </returns>
		public static SpecialFilterOperandBase[] GetRegisteredOperands( Type dataType )
		{
			List<SpecialFilterOperandBase> list = new List<SpecialFilterOperandBase>( );

			lock ( g_operands )
			{
				foreach ( SpecialFilterOperandBase ii in g_operands.Values )
				{
					if ( null == dataType || ii.SupportsDataType( dataType ) )
						list.Add( ii );
				}
			}

			return list.ToArray( );
		}

		#endregion // GetRegisteredOperands

		#region Register

		/// <summary>
		/// Registers a user defined special operand. This method can be used to re-register built-in special 
		/// operand as well, in case it was un-registered with Unregister method.
		/// </summary>
		/// <param name="operand">Operand to register.</param>
		/// <seealso cref="Unregister"/>
		/// <seealso cref="GetRegisteredOperand(string)"/>
		/// <seealso cref="GetRegisteredOperands()"/>
		public static void Register( SpecialFilterOperandBase operand )
		{
			lock ( g_operands )
			{
				g_operands[operand.Name] = operand;
				g_registeredOperandsVersion++;
			}
		}

		#endregion // Register

		#region Unregister

		/// <summary>
		/// Un-registers a previously registered operand. Built-in operands can also be un-registered.
		/// </summary>
		/// <param name="operand">Operand to unregister.</param>
		/// <seealso cref="Register"/>
		/// <seealso cref="GetRegisteredOperand(string)"/>
		/// <seealso cref="GetRegisteredOperands()"/>
		public static void Unregister( SpecialFilterOperandBase operand )
		{
			lock ( g_operands )
			{
				g_operands.Remove( operand.Name );
				g_registeredOperandsVersion++;
			}
		}

		#endregion // Unregister

		#endregion // Public Methods

		#endregion // Methods

		#region Properties

		#region Public Properties

		#region Built-in Operands

		#region Blanks

		/// <summary>
		/// An operand used to determine if a specified value is blank ('null', 'dbnull' or empty string).
		/// </summary>
		public static SpecialFilterOperandBase Blanks
		{
			get
			{
				return OperandWrapper.GetWrapper( SpecialFilterOperandFactory.BlanksOperand.NAME_Blanks );
			}
		}

		#endregion // Blanks

		#region NonBlanks

		/// <summary>
		/// An operand used to determine if a specified value is not blank (anything that's not 'null', 'dbnull' or empty string).
		/// </summary>
		public static SpecialFilterOperandBase NonBlanks
		{
			get
			{
				return OperandWrapper.GetWrapper( SpecialFilterOperandFactory.BlanksOperand.NAME_NonBlanks );
			}
		}

		#endregion // NonBlanks

		#region AboveAverage

		/// <summary>
		/// An operand used to determine if a specified value is above the average of the set 
		/// of values of which the value being tested is a member value.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>AboveAverage</b> operand is used to determine if a specified value is above 
		/// the average of the set of values of which the value being tested is a member value.
		/// In the XamDataPresenter, the set of values is the values associated with the 
		/// record collection that the record value being tested belongs to.
		/// </para>
		/// </remarks>
		public static SpecialFilterOperandBase AboveAverage
		{
			get
			{
				return OperandWrapper.GetWrapper( "AboveAverage" );
			}
		}

		#endregion // AboveAverage

		#region BelowAverage

		/// <summary>
		/// An operand used to determine if a specified value is below the average of the set 
		/// of values of which the value being tested is a member value.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>BelowAverage</b> operand is used to determine if a specified value is above 
		/// the average of the set of values of which the value being tested is a member value.
		/// In the XamDataPresenter, the set of values is the values associated with the 
		/// record collection that the record value being tested belongs to.
		/// </para>
		/// </remarks>
		public static SpecialFilterOperandBase BelowAverage
		{
			get
			{
				return OperandWrapper.GetWrapper( "BelowAverage" );
			}
		}

		#endregion // BelowAverage

		#region Top10

		/// <summary>
		/// An operand used to determine if a specified value is one of the top 10 values
		/// in the set of values that the value being tested is a member of.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>Top10</b> operand used to determine if a specified value is one of 
		/// the top 10 values in the set of values that the value being tested is a member 
		/// of. In the XamDataPresenter, the set of values is the values associated with 
		/// the record collection that the record value being tested belongs to.
		/// </para>
		/// </remarks>
		public static SpecialFilterOperandBase Top10
		{
			get
			{
				return OperandWrapper.GetWrapper( "Top10" );
			}
		}

		#endregion // Top10

		#region Top10Percentage

		/// <summary>
		/// An operand used to determine if a specified value is one of the top 10 percent of
		/// the values in the set of values that the value being tested is a member of.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>Top10Percentage</b> An operand used to determine if a specified value is one 
		/// of the top 10 percent of the values in the set of values that the value being 
		/// tested is a member of. In the XamDataPresenter, the set of values is the values 
		/// associated with the record collection that the record value being tested 
		/// belongs to.
		/// </para>
		/// </remarks>
		public static SpecialFilterOperandBase Top10Percentage
		{
			get
			{
				return OperandWrapper.GetWrapper( "Top10Percentage" );
			}
		}

		#endregion // Top10Percentage

		#region Bottom10

		
		
		
		/// <summary>
		/// An operand used to determine if a specified value is one of the bottom 10 values
		/// in the set of values that the value being tested is a member of.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>Bottom10</b> operand used to determine if a specified value is one of 
		/// the bottom 10 values in the set of values that the value being tested is a member 
		/// of. In the XamDataPresenter, the set of values is the values associated with 
		/// the record collection that the record value being tested belongs to.
		/// </para>
		/// </remarks>
		public static SpecialFilterOperandBase Bottom10
		{
			get
			{
				return OperandWrapper.GetWrapper( "Bottom10" );
			}
		}

		#endregion // Bottom10

		#region Bottom10Percentage

		
		
		
		/// <summary>
		/// An operand used to determine if a specified value is one of the bottom 10 percent of
		/// the values in the set of values that the value being tested is a member of.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>Bottom10Percentage</b> An operand used to determine if a specified value is one 
		/// of the bottom 10 percent of the values in the set of values that the value being 
		/// tested is a member of. In the XamDataPresenter, the set of values is the values 
		/// associated with the record collection that the record value being tested 
		/// belongs to.
		/// </para>
		/// </remarks>
		public static SpecialFilterOperandBase Bottom10Percentage
		{
			get
			{
				return OperandWrapper.GetWrapper( "Bottom10Percentage" );
			}
		}

		#endregion // Bottom10Percentage

		#region Tomorrow

		/// <summary>
		/// An operand used to determine if a specified value is equal to tomorrow's date.
		/// </summary>
		public static SpecialFilterOperandBase Tomorrow
		{
			get
			{
				return OperandWrapper.GetWrapper( "Tomorrow" );
			}
		}
		#endregion //Tomorrow

		#region Today

		/// <summary>
		/// An operand used to determine if a specified value is equal to today's date.
		/// </summary>
		public static SpecialFilterOperandBase Today
		{
			get
			{
				return OperandWrapper.GetWrapper( "Today" );
			}
		}
		#endregion //Today

		#region Yesterday

		/// <summary>
		/// An operand used to determine if a specified value is equal to yesterday's date.
		/// </summary>
		public static SpecialFilterOperandBase Yesterday
		{
			get
			{
				return OperandWrapper.GetWrapper( "Yesterday" );
			}
		}
		#endregion //Yesterday

		#region NextWeek

		/// <summary>
		/// An operand used to determine if a specified value is between the first and last days of next week.
		/// </summary>
		public static SpecialFilterOperandBase NextWeek
		{
			get
			{
				return OperandWrapper.GetWrapper( "NextWeek" );
			}
		}
		#endregion //NextWeek

		#region ThisWeek

		/// <summary>
		/// An operand used to determine if a specified value is between the first and last days of this week.
		/// </summary>
		public static SpecialFilterOperandBase ThisWeek
		{
			get
			{
				return OperandWrapper.GetWrapper( "ThisWeek" );
			}
		}
		#endregion //ThisWeek

		#region LastWeek

		/// <summary>
		/// An operand used to determine if a specified value is between the first and last days of last week.
		/// </summary>
		public static SpecialFilterOperandBase LastWeek
		{
			get
			{
				return OperandWrapper.GetWrapper( "LastWeek" );
			}
		}
		#endregion //LastWeek

		#region NextMonth

		/// <summary>
		/// An operand used to determine if a specified value is between the first and last days of next month.
		/// </summary>
		public static SpecialFilterOperandBase NextMonth
		{
			get
			{
				return OperandWrapper.GetWrapper( "NextMonth" );
			}
		}
		#endregion //NextMonth

		#region ThisMonth

		/// <summary>
		/// An operand used to determine if a specified value is between the first and last days of this month.
		/// </summary>
		public static SpecialFilterOperandBase ThisMonth
		{
			get
			{
				return OperandWrapper.GetWrapper( "ThisMonth" );
			}
		}
		#endregion //ThisMonth

		#region LastMonth

		/// <summary>
		/// An operand used to determine if a specified value is between the first and last days of last month.
		/// </summary>
		public static SpecialFilterOperandBase LastMonth
		{
			get
			{
				return OperandWrapper.GetWrapper( "LastMonth" );
			}
		}
		#endregion //LastMonth

		#region NextQuarter

		/// <summary>
		/// An operand used to determine if a specified value is between the first and last days of next quarter.
		/// </summary>
		public static SpecialFilterOperandBase NextQuarter
		{
			get
			{
				return OperandWrapper.GetWrapper( "NextQuarter" );
			}
		}
		#endregion //NextQuarter

		#region ThisQuarter

		/// <summary>
		/// An operand used to determine if a specified value is between the first and last days of the current quarter.
		/// </summary>
		public static SpecialFilterOperandBase ThisQuarter
		{
			get
			{
				return OperandWrapper.GetWrapper( "ThisQuarter" );
			}
		}
		#endregion //ThisQuarter

		#region LastQuarter

		/// <summary>
		/// An operand used to determine if a specified value is between the first and last days of the previous quarter.
		/// </summary>
		public static SpecialFilterOperandBase LastQuarter
		{
			get
			{
				return OperandWrapper.GetWrapper( "LastQuarter" );
			}
		}
		#endregion //LastQuarter

		#region NextYear

		/// <summary>
		/// An operand used to determine if a specified value is between the first and last days of next year.
		/// </summary>
		public static SpecialFilterOperandBase NextYear
		{
			get
			{
				return OperandWrapper.GetWrapper( "NextYear" );
			}
		}
		#endregion //NextYear

		#region ThisYear

		/// <summary>
		/// An operand used to determine if a specified value is between the first and last days of the current year.
		/// </summary>
		public static SpecialFilterOperandBase ThisYear
		{
			get
			{
				return OperandWrapper.GetWrapper( "ThisYear" );
			}
		}
		#endregion //ThisYear

		#region LastYear

		/// <summary>
		/// An operand used to determine if a specified value is between the first and last days of the previous year.
		/// </summary>
		public static SpecialFilterOperandBase LastYear
		{
			get
			{
				return OperandWrapper.GetWrapper( "LastYear" );
			}
		}
		#endregion //LastYear

		#region YearToDate

		/// <summary>
		/// An operand used to determine if a specified value is between the first day of the year and the current date.
		/// </summary>
		public static SpecialFilterOperandBase YearToDate
		{
			get
			{
				return OperandWrapper.GetWrapper( "YearToDate" );
			}
		}
		#endregion //YearToDate

		#region Quarter1

		/// <summary>
		/// An operand used to determine if a specified value falls within the first 3 months of the calendar year.
		/// </summary>
		public static SpecialFilterOperandBase Quarter1
		{
			get
			{
				return OperandWrapper.GetWrapper( "Quarter1" );
			}
		}
		#endregion //Quarter1

		#region Quarter2

		/// <summary>
		/// An operand used to determine if a specified value falls within the second 3 months of the calendar year.
		/// </summary>
		public static SpecialFilterOperandBase Quarter2
		{
			get
			{
				return OperandWrapper.GetWrapper( "Quarter2" );
			}
		}
		#endregion //Quarter1

		#region Quarter3

		/// <summary>
		/// An operand used to determine if a specified value falls within the third 3 months of the calendar year.
		/// </summary>
		public static SpecialFilterOperandBase Quarter3
		{
			get
			{
				return OperandWrapper.GetWrapper( "Quarter3" );
			}
		}
		#endregion //Quarter3

		#region Quarter4

		/// <summary>
		/// An operand used to determine if a specified value falls within the last 3 months of the calendar year.
		/// </summary>
		public static SpecialFilterOperandBase Quarter4
		{
			get
			{
				return OperandWrapper.GetWrapper( "Quarter4" );
			}
		}
		#endregion //Quarter4

		#region January

		/// <summary>
		/// An operand representing the first month of the year.
		/// </summary>
		public static SpecialFilterOperandBase January
		{
			get
			{
				return OperandWrapper.GetWrapper( "January" );
			}
		}
		#endregion //January

		#region February

		/// <summary>
		/// An operand representing the second month of the year.
		/// </summary>
		public static SpecialFilterOperandBase February
		{
			get
			{
				return OperandWrapper.GetWrapper( "February" );
			}
		}
		#endregion //February

		#region March

		/// <summary>
		/// An operand representing the third month of the year.
		/// </summary>
		public static SpecialFilterOperandBase March
		{
			get
			{
				return OperandWrapper.GetWrapper( "March" );
			}
		}
		#endregion //March

		#region April

		/// <summary>
		/// An operand representing the fourth month of the year.
		/// </summary>
		public static SpecialFilterOperandBase April
		{
			get
			{
				return OperandWrapper.GetWrapper( "April" );
			}
		}
		#endregion //April

		#region May

		/// <summary>
		/// An operand representing the fifth month of the year.
		/// </summary>
		public static SpecialFilterOperandBase May
		{
			get
			{
				return OperandWrapper.GetWrapper( "May" );
			}
		}
		#endregion //May

		#region June

		/// <summary>
		/// An operand representing the sixth month of the year.
		/// </summary>
		public static SpecialFilterOperandBase June
		{
			get
			{
				return OperandWrapper.GetWrapper( "June" );
			}
		}
		#endregion //June

		#region July

		/// <summary>
		/// An operand representing the seventh month of the year.
		/// </summary>
		public static SpecialFilterOperandBase July
		{
			get
			{
				return OperandWrapper.GetWrapper( "July" );
			}
		}
		#endregion //July

		#region August

		/// <summary>
		/// An operand representing the eighth month of the year.
		/// </summary>
		public static SpecialFilterOperandBase August
		{
			get
			{
				return OperandWrapper.GetWrapper( "August" );
			}
		}
		#endregion //August

		#region September

		/// <summary>
		/// An operand representing the ninth month of the year.
		/// </summary>
		public static SpecialFilterOperandBase September
		{
			get
			{
				return OperandWrapper.GetWrapper( "September" );
			}
		}
		#endregion //September

		#region October

		/// <summary>
		/// An operand representing the tenth month of the year.
		/// </summary>
		public static SpecialFilterOperandBase October
		{
			get
			{
				return OperandWrapper.GetWrapper( "October" );
			}
		}
		#endregion //October

		#region November

		/// <summary>
		/// An operand representing the eleventh month of the year.
		/// </summary>
		public static SpecialFilterOperandBase November
		{
			get
			{
				return OperandWrapper.GetWrapper( "November" );
			}
		}
		#endregion //November

		#region December

		/// <summary>
		/// An operand representing the last month of the year.
		/// </summary>
		public static SpecialFilterOperandBase December
		{
			get
			{
				return OperandWrapper.GetWrapper( "December" );
			}
		}
		#endregion //December

		#endregion // Built-in Operands

		#region Version

		// SSP 3/31/09 TFS15861
		// Exposed the existing version number so the XamDataGrid can re-populate the filter drop-down
		// whenever the an operand is registered/un-registered.
		// 
		/// <summary>
		/// A numer that's incremented every time an operand is registered or un-registered.
		/// </summary>
		[ EditorBrowsable( EditorBrowsableState.Never ), Browsable( false ) ]
		public static int Version
		{
			get
			{
				return g_registeredOperandsVersion;
			}
		}

		#endregion // Version

		#endregion // Public Properties

		#endregion // Properties
	}

	#endregion // SpecialFilterOperands Class

	// AS - NA 11.2 Excel Style Filtering
	#region SpecialFilterOperandTypeConverter
	/// <summary>
	/// Custom converter for string properties that represent the <see cref="SpecialFilterOperandBase.Name"/> of registered <see cref="SpecialFilterOperandBase"/>
	/// </summary>
	/// <seealso cref="SpecialFilterOperands.Register(SpecialFilterOperandBase)"/>
	public class SpecialFilterOperandTypeConverter : TypeConverter
	{
		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="SpecialFilterOperandTypeConverter"/>
		/// </summary>
		public SpecialFilterOperandTypeConverter()
		{
		}
		#endregion //Constructor

		#region Base class overrides

		#region CanConvertFrom
		/// <summary>
		/// Determines whether an object of the specified source type can be converted to a string
		/// </summary>
		/// <param name="context">Provides additional information about the operation</param>
		/// <param name="sourceType">The type from which the conversion could occur</param>
		/// <returns>True for string; otherwise false.</returns>
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			return sourceType == typeof(string);
		}
		#endregion //CanConvertFrom

		#region ConvertFrom
		/// <summary>
		/// Converts the specified value to a string
		/// </summary>
		/// <param name="context">Provides additional information about the operation</param>
		/// <param name="culture">Culture information</param>
		/// <param name="value">The value being converted</param>
		/// <returns>A string</returns>
		public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
		{
			if (value is string == false)
				throw this.GetConvertFromException(value);

			return value;
		}
		#endregion //ConvertFrom

		#region GetStandardValues
		/// <summary> 
		/// Returns a list of the names of the registered <see cref="SpecialFilterOperandBase"/>
		/// </summary>
		/// <param name="context">Provides additional information about the operation</param>
		/// <returns>A list of strings representing the names of the registered special operands</returns>
		/// <seealso cref="SpecialFilterOperands.Register(SpecialFilterOperandBase)"/>
		public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
		{
			var operands = SpecialFilterOperands.GetRegisteredOperands();
			string[] operandNames = new string[operands.Length];
			for (int i = 0; i < operands.Length; i++)
				operandNames[i] = operands[i].Name;
			return new StandardValuesCollection(operandNames);
		}
		#endregion //GetStandardValues

		#region GetStandardValuesSupported
		/// <summary>
		/// Indicates that there is a list of standard values
		/// </summary>
		/// <param name="context">Provides additional information about the operation</param>
		/// <returns>Returns true</returns>
		public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
		{
			return true;
		}
		#endregion //GetStandardValuesSupported

		#endregion //Base class overrides
	}
	#endregion //SpecialFilterOperandTypeConverter
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