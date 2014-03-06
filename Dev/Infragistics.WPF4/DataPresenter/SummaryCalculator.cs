using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Shapes;
using System.Windows.Navigation;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Automation;
using System.Windows.Automation.Provider;
using System.Windows.Threading;
using System.Diagnostics;
//using System.Windows.Events;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text;
using Infragistics.Shared;
using Infragistics.Windows.Selection;
using Infragistics.Windows.Helpers;
using Infragistics.Windows.DataPresenter;
using Infragistics.Windows.Editors;
using Infragistics.Windows.Controls;
using Infragistics.Windows.Internal;
using Infragistics.Windows.DataPresenter.Events;
using Infragistics.Windows.DataPresenter.Internal;
using System.Collections.ObjectModel;

namespace Infragistics.Windows.DataPresenter
{

	#region SummaryCalculator Class

	/// <summary>
	/// Represents a summary calculator. It contains logic for calculating summary of data.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// <b>SummaryCalculator</b> is an abstract base class for various summary calculator implementations.
	/// <see cref="SumSummaryCalculator"/>, <see cref="MaximumSummaryCalculator"/>, <see cref="MinimumSummaryCalculator"/>,
	/// <see cref="AverageSummaryCalculator"/> and <see cref="CountSummaryCalculator"/> are built-in implementations
	/// of summary calculators. You can also implement a custom summary calculator by deriving from this class.
	/// You can integrate any such custom summary calculator implementations into the summary selection UI that
	/// the DataGrid provides by using the static <see cref="SummaryCalculator.Register"/> method.
	/// </para>
	/// </remarks>
	[ TypeConverter( typeof( SummaryCalculatorConverter ) ) ]
	public abstract class SummaryCalculator
	{
		#region Nested Classes/Structures

		/// <summary>
		/// Converter class that uses registered calculators collection to convert between
		/// calculator's name and other way around.
		/// </summary>
		public class SummaryCalculatorConverter : TypeConverter
		{
			/// <summary>
			/// Overridden. Returns true for string types.
			/// </summary>
			/// <param name="context"></param>
			/// <param name="sourceType"></param>
			/// <returns></returns>
			public override bool CanConvertFrom( ITypeDescriptorContext context, Type sourceType )
			{
				if ( typeof( string ) == sourceType )
					return true;

				return base.CanConvertFrom( context, sourceType );
			}

			/// <summary>
			/// Overridden. Returns true for string types.
			/// </summary>
			/// <param name="context"></param>
			/// <param name="destinationType"></param>
			/// <returns></returns>
			public override bool CanConvertTo( ITypeDescriptorContext context, Type destinationType )
			{
				if ( typeof( string ) == destinationType )
					return true;

				return base.CanConvertTo( context, destinationType );
			}

			/// <summary>
			/// Overridden. Converts to SummaryCalculator from the value using collection registered
			/// calculators registered via <see cref="SummaryCalculator.Register"/> method.
			/// </summary>
			/// <param name="context"></param>
			/// <param name="culture"></param>
			/// <param name="value"></param>
			/// <returns></returns>
			public override object ConvertFrom( ITypeDescriptorContext context, CultureInfo culture, object value )
			{
				string calculatorName = value as string;
				if ( null != calculatorName )
					return SummaryCalculator.GetCalculator( calculatorName );

				return base.ConvertFrom( context, culture, value );
			}

			/// <summary>
			/// Overridden. Converts the value to the destination type.
			/// </summary>
			/// <param name="context"></param>
			/// <param name="culture"></param>
			/// <param name="value"></param>
			/// <param name="destinationType"></param>
			/// <returns></returns>
			public override object ConvertTo( ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType )
			{
				SummaryCalculator calculator = value as SummaryCalculator;
				if ( null != calculator )
					return calculator.Name;

				return base.ConvertTo( context, culture, value, destinationType );
			}

			/// <summary>
			/// Overridden. Gets standard values.
			/// </summary>
			/// <param name="context"></param>
			/// <returns>Returns a collection of standard values.</returns>
			public override TypeConverter.StandardValuesCollection GetStandardValues( ITypeDescriptorContext context )
			{
				if ( context.Instance is SummaryDefinition )
				{
					List<SummaryCalculator> calculators = GridUtilities.ToList<SummaryCalculator>( SummaryCalculator.RegisteredCalculators, true );
					if ( null != calculators && calculators.Count > 0 )
						return new StandardValuesCollection( calculators );
				}

				return base.GetStandardValues( context );
			}
		}

		#endregion // Nested Classes/Structures

		#region Private Vars

		private static SumSummaryCalculator g_sumCalculator;
		private static AverageSummaryCalculator g_avgCalculator;
		private static MinimumSummaryCalculator g_minCalculator;
		private static MaximumSummaryCalculator g_maxCalculator;
		private static CountSummaryCalculator g_countCalculator;

		private static List<SummaryCalculator> g_registeredCalculators;

		#endregion // Private Vars

		#region Constructor

		/// <summary>
		/// Static constructor.
		/// </summary>
		static SummaryCalculator( )
		{
			g_registeredCalculators = new List<SummaryCalculator>( );

			// Register built-in calculators
			Register( Average );
			Register( Count );
			Register( Maximum );
			Register( Minimum );
			Register( Sum );
		}

		/// <summary>
		/// Constructor. Initializes a new instance of SummaryCalculator.
		/// </summary>
		public SummaryCalculator( )
		{
		}

		#endregion // Constructor

		#region Properties

		#region Public Properties

		#region Average [static]

		/// <summary>
		/// Returns a static instance of <see cref="AverageSummaryCalculator"/> for performing
		/// <i>average</i> summary calculation.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>Average</b> returns a static instance of <see cref="AverageSummaryCalculator"/> for
		/// performing <i>average</i> summary calculation.
		/// </para>
		/// <para class="body">
		/// To actually specify a summary calculation to perform, use FieldLayout's 
		/// <see cref="FieldLayout.SummaryDefinitions"/>.
		/// </para>
		/// </remarks>
		/// <seealso cref="AverageSummaryCalculator"/>
		/// <seealso cref="SummaryCalculator.Sum"/>
		/// <seealso cref="SummaryCalculator.Average"/>
		/// <seealso cref="SummaryCalculator.Minimum"/>
		/// <seealso cref="SummaryCalculator.Maximum"/>
		/// <seealso cref="SummaryCalculator.Count"/>
		/// <seealso cref="FieldLayout.SummaryDefinitions"/>
		/// <seealso cref="RecordCollectionBase.SummaryResults"/>
		public static AverageSummaryCalculator Average
		{
			get
			{
				if ( null == g_avgCalculator )
					g_avgCalculator = new AverageSummaryCalculator( );

				return g_avgCalculator;
			}
		}

		#endregion // Average [static]

		#region Count [static]

		/// <summary>
		/// Returns a static instance of <see cref="AverageSummaryCalculator"/> for performing
		/// <i>count</i> summary calculation.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>Count</b> returns a static instance of <see cref="AverageSummaryCalculator"/> for
		/// performing <i>Count</i> summary calculation.
		/// </para>
		/// <para class="body">
		/// To actually specify a summary calculation to perform, use FieldLayout's 
		/// <see cref="FieldLayout.SummaryDefinitions"/>.
		/// </para>
		/// </remarks>
		/// <seealso cref="AverageSummaryCalculator"/>
		/// <seealso cref="SummaryCalculator.Sum"/>
		/// <seealso cref="SummaryCalculator.Average"/>
		/// <seealso cref="SummaryCalculator.Minimum"/>
		/// <seealso cref="SummaryCalculator.Maximum"/>
		/// <seealso cref="SummaryCalculator.Count"/>
		/// <seealso cref="FieldLayout.SummaryDefinitions"/>
		/// <seealso cref="RecordCollectionBase.SummaryResults"/>
		public static CountSummaryCalculator Count
		{
			get
			{
				if ( null == g_countCalculator )
					g_countCalculator = new CountSummaryCalculator( );

				return g_countCalculator;
			}
		}

		#endregion // Count [static]

		#region Description

		/// <summary>
		/// Description of the summary calculator.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>Note:</b> This will be displayed in the summary calculator selection UI as a tooltip
		/// on the item that represents this summary calculator.
		/// </para>
		/// </remarks>
		public abstract string Description { get; }

		#endregion // Description

		#region DisplayName

		// SSP 8/29/08 BR35879
		// Added DisplayName property to allow localization. Name will contain non-localized value
		// so it can be used from code to identify the summary calculator without worrying about
		// what the DisplayName is localized to.
		// 
		/// <summary>
		/// Returns the display name of the summary calculator. This will be displayed in the UI.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>Note:</b> This will be displayed in the summary calculator selection UI to indentify to
		/// the user what type of summary calculation this is.
		/// </para>
		/// </remarks>
		/// <seealso cref="Name"/>
		public virtual string DisplayName 
		{
			get
			{
				return this.Name;
			}
		}

		#endregion // DisplayName

		#region Image

		/// <summary>
		/// Image for the summary calculator selection UI.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// Image is used in the summary calculator selection UI next to the name of the calculator.
		/// </para>
		/// </remarks>
		public virtual ImageSource Image
		{
			get
			{
				
				return null;
			}
		}

		#endregion // Image

		#region IsCalculationAffectedBySort

		/// <summary>
		/// Indicates whether the calculation is affected by the order in which data is aggregated.
		/// This is used to determine whether the calculation needs to be re-evaluated when the 
		/// records are sorted.
		/// </summary>
		public virtual bool IsCalculationAffectedBySort
		{
			get
			{
				return false;
			}
		}

		#endregion // IsCalculationAffectedBySort

		#region Maximum [static]

		/// <summary>
		/// Returns a static instance of <see cref="AverageSummaryCalculator"/> for performing
		/// <i>count</i> summary calculation.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>Count</b> returns a static instance of <see cref="AverageSummaryCalculator"/> for
		/// performing <i>Count</i> summary calculation.
		/// </para>
		/// <para class="body">
		/// To actually specify a summary calculation to perform, use FieldLayout's 
		/// <see cref="FieldLayout.SummaryDefinitions"/>.
		/// </para>
		/// </remarks>
		/// <seealso cref="MaximumSummaryCalculator"/>
		/// <seealso cref="SummaryCalculator.Sum"/>
		/// <seealso cref="SummaryCalculator.Average"/>
		/// <seealso cref="SummaryCalculator.Minimum"/>
		/// <seealso cref="SummaryCalculator.Maximum"/>
		/// <seealso cref="SummaryCalculator.Count"/>
		/// <seealso cref="FieldLayout.SummaryDefinitions"/>
		/// <seealso cref="RecordCollectionBase.SummaryResults"/>
		public static MaximumSummaryCalculator Maximum
		{
			get
			{
				if ( null == g_maxCalculator )
					g_maxCalculator = new MaximumSummaryCalculator( );

				return g_maxCalculator;
			}
		}

		#endregion // Maximum [static]

		#region Minimum [static]

		/// <summary>
		/// Returns a static instance of <see cref="AverageSummaryCalculator"/> for performing
		/// <i>count</i> summary calculation.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>Count</b> returns a static instance of <see cref="AverageSummaryCalculator"/> for
		/// performing <i>Count</i> summary calculation.
		/// </para>
		/// <para class="body">
		/// To actually specify a summary calculation to perform, use FieldLayout's 
		/// <see cref="FieldLayout.SummaryDefinitions"/>.
		/// </para>
		/// </remarks>
		/// <seealso cref="MinimumSummaryCalculator"/>
		/// <seealso cref="SummaryCalculator.Sum"/>
		/// <seealso cref="SummaryCalculator.Average"/>
		/// <seealso cref="SummaryCalculator.Minimum"/>
		/// <seealso cref="SummaryCalculator.Maximum"/>
		/// <seealso cref="SummaryCalculator.Count"/>
		/// <seealso cref="FieldLayout.SummaryDefinitions"/>
		/// <seealso cref="RecordCollectionBase.SummaryResults"/>
		public static MinimumSummaryCalculator Minimum
		{
			get
			{
				if ( null == g_minCalculator )
					g_minCalculator = new MinimumSummaryCalculator( );

				return g_minCalculator;
			}
		}

		#endregion // Minimum [static]

		#region Name

		/// <summary>
		/// Returns the name of the summary calculator. This identifies the summary calculator.
		/// </summary>
		/// <remarks>
		/// <b>Name</b> property uniquely identifies the summary calculator. <see cref="DisplayName"/>
		/// also returns the name of the summary calculator however <i>DisplayName</i> is used in the
		/// user interface to denote this calculator.
		/// </remarks>
		/// <seealso cref="DisplayName"/>
		public abstract string Name { get; }

		#endregion // Name

		#region RegisteredCalculators

		/// <summary>
		/// Returns registered summary calculators.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// Returns registered summary calculators. This will also include built-in summary
		/// calculators. See <see cref="Register"/> method for more information.
		/// </para>
		/// <see cref="Register"/>
		/// <see cref="UnRegister(string)"/>
		/// </remarks>
		public static IEnumerable<SummaryCalculator> RegisteredCalculators
		{
			get
			{
				return g_registeredCalculators;
			}
		}

		#endregion // RegisteredCalculators

		#region Sum [static]

		/// <summary>
		/// Returns a static instance of <see cref="SumSummaryCalculator"/> for performing
		/// <i>sum</i> summary calculation.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>Sum</b> returns a static instance of <see cref="SumSummaryCalculator"/> for
		/// performing <i>sum</i> summary calculation.
		/// </para>
		/// <para class="body">
		/// To actually specify a summary calculation to perform, use FieldLayout's 
		/// <see cref="FieldLayout.SummaryDefinitions"/>.
		/// </para>
		/// </remarks>
		/// <seealso cref="SumSummaryCalculator"/>
		/// <seealso cref="SummaryCalculator.Sum"/>
		/// <seealso cref="SummaryCalculator.Average"/>
		/// <seealso cref="SummaryCalculator.Minimum"/>
		/// <seealso cref="SummaryCalculator.Maximum"/>
		/// <seealso cref="SummaryCalculator.Count"/>
		/// <seealso cref="FieldLayout.SummaryDefinitions"/>
		/// <seealso cref="RecordCollectionBase.SummaryResults"/>
		public static SumSummaryCalculator Sum
		{
			get
			{
				if ( null == g_sumCalculator )
					g_sumCalculator = new SumSummaryCalculator( );

				return g_sumCalculator;
			}
		}

		#endregion // Sum [static]

		#endregion // Public Properties

		#endregion // Properties

		#region Methods

		#region Public Methods

		#region Aggregate

		/// <summary>
		/// Processes individual piece of data value.
		/// </summary>
		/// <param name="dataValue">Data value to process into calculation</param>
		/// <param name="summaryResult">SummaryResult object for which the summary is being calculated.</param>
		/// <param name="record">The record associated with the data value.</param>
		/// <remarks>
		/// <para class="body">
		/// This method is called for each data value that is to be aggregated into the calculation.
		/// </para>
		/// <para class="body">
		/// <b>Note that</b> <b>summaryResult</b> and <b>record</b> parameters are for extra context only 
		/// and typically it's not necessary to look at them or take them into account for calculation purposes.
		/// The <b>dataValue</b> should suffice for most purposes.
		/// </para>
		/// </remarks>
		public abstract void Aggregate( object dataValue, SummaryResult summaryResult, Record record );

		#endregion // Aggregate

		#region ApplyDefaultFormat

		/// <summary>
		/// Applies the default format to the specified result and returns a string representation
		/// of the summary result as it is to be displayed inside the summary result element.
		/// </summary>
		/// <param name="summaryResultValue">Calculated result value.</param>
		/// <param name="context">Associated summary result object.</param>
		/// <returns>Returns the formatted value.</returns>
		public virtual string ApplyDefaultFormat( object summaryResultValue, SummaryResult context )
		{
			Field field = context.SourceField;
			SummaryDefinition summaryDef = context.SummaryDefinition;

			string format = null;

			Type fieldDataType = Utilities.GetUnderlyingType( field.EditAsTypeResolved );
			Type resultType = null != summaryResultValue ? summaryResultValue.GetType( ) : null;

			if ( this is CountSummaryCalculator )
			{
				// For count there's no format.
				format = null;
			}
			else if ( Utilities.IsNumericType( fieldDataType ) )
			{
				Type[] decimalTypes = new Type[] { typeof( decimal ), typeof( double ), typeof( float ) };

				if ( GridUtilities.Contains( decimalTypes, fieldDataType )
					|| this is AverageSummaryCalculator )
					format = "N";
				else
					format = "G";
			}
			else if ( typeof( DateTime ) == resultType )
			{
				format = "d";
			}

			return string.Format(
				
				
				
				
				context.StringFormatProviderResolved,
				null != format ? ( "{1} = {0:" + format + "}" ) : "{1} = {0}",
				summaryResultValue,
				// SSP 9/2/08 BR35879
				// 
				//null != summaryDef.Calculator ?  summaryDef.Calculator.Name : string.Empty 
				null != summaryDef.Calculator ? summaryDef.Calculator.DisplayName : string.Empty 
				);
		}

		#endregion // ApplyDefaultFormat

		#region BeginCalculation

		/// <summary>
		/// Starts summary calculation.
		/// </summary>
		/// <param name="summaryResult">SummaryResult object for which the summary is being calculated.</param>
		/// <remarks>
		/// <para class="body">
		/// This method is called to start calculation of a summary for a particular set of data (associated
		/// with the <i>records</i> parameter). Typically you initialize any member variables in this method.
		/// For example, for a calculator that performs summation, you would initialize the sum member variable
		/// to 0. In <see cref="Aggregate"/>, you would add each value to the sum. In <see cref="EndCalculation"/>,
		/// you would return the result of the calculation.
		/// </para>
		/// <para class="body">
		/// <b>Note that</b> <b>summaryResult</b> parameter is for extra context only 
		/// and typically it's not necessary to use it or take it into account for calculation purposes.
		/// </para>
		/// </remarks>
		public abstract void BeginCalculation( SummaryResult summaryResult );

		#endregion // BeginCalculation

		#region CanProcessDataType

		/// <summary>
		/// Indicates whether this summary calculator can use objects of the specified data type
		/// as source values for the calculation.
		/// </summary>
		/// <param name="dataType">Data type</param>
		/// <returns>Returns true if the specified data type is supported</returns>
		/// <remarks>
		/// <para class="body">
		/// Indicates whether this calculator supports specified data type. <i>Sum</i> for 
		/// example supports only numeric types. Where as <i>Maximum</i> supports all data 
		/// types that implement IComparable.
		/// </para>
		/// </remarks>
		public abstract bool CanProcessDataType( Type dataType );

		#endregion // CanProcessDataType

		#region EndCalculation

		/// <summary>
		/// Called after all the data has been aggregated into the calculation to return the result of the calculation.
		/// </summary>
		/// <param name="summaryResult">SummaryResult object for which the summary is being calculated.</param>
		/// <returns>Result of the calculation.</returns>
		/// <remarks>
		/// <para class="body">
		/// This method is called after all the data has been aggregated via <see cref="Aggregate"/> method calls.
		/// It returns the result of the calculation.
		/// </para>
		/// <para class="body">
		/// <b>Note that</b> <b>summaryResult</b> parameter is for extra context only 
		/// and typically it's not necessary to use it or take it into account for calculation purposes.
		/// </para>
		/// </remarks>
		public abstract object EndCalculation( SummaryResult summaryResult );

		#endregion // EndCalculation

		#region Register

		/// <summary>
		/// Registers the specified summary calculator. Existing summary calculator with the 
		/// same name will be unregistered.
		/// </summary>
		/// <param name="calculator">Summary calculator to register</param>
		/// <remarks>
		/// <para class="body">
		/// <b>Register</b> method provides a mechanism by which you can integrate your custom
		/// calculator implementations into the summary calculator selection user interface
		/// of the DataGrid. The summary selection UI displays all the summary calculators
		/// registered via this method (restricted to calculators that support the field's
		/// data type).
		/// </para>
		/// <para class="body">
		/// You can also replace built-in summary calculators with your implementations by
		/// simply registering a calculator with the same name as the built-in calculator.
		/// </para>
		/// <seealso cref="UnRegister(SummaryCalculator)"/>
		/// <seealso cref="UnRegister(string)"/>
		/// </remarks>
		public static void Register( SummaryCalculator calculator )
		{
			UnRegister( calculator );

			lock ( g_registeredCalculators )
			{
				g_registeredCalculators.Add( calculator );
			}
		}

		#endregion // Register

		#region UnRegister

		/// <summary>
		/// Un-registers a summary calculator.
		/// </summary>
		/// <param name="calculatorName"></param>
		/// <remarks>
		/// <para class="body">
		/// You can un-register and thus effectively remove from the summary selection UI
		/// any previously registered summary calculators as well as the built-in ones by
		/// simply proving the name of the calculator.
		/// </para>
		/// <seealso cref="Register"/>
		/// </remarks>
		public static void UnRegister( string calculatorName )
		{
			lock ( g_registeredCalculators )
			{
				for ( int i = 0; i < g_registeredCalculators.Count; i++ )
				{
					SummaryCalculator cc = g_registeredCalculators[i];

					if ( cc.Name == calculatorName )
					{
						g_registeredCalculators.RemoveAt( i );
						break;
					}
				}
			}
		}

		/// <summary>
		/// Un-registers a summary calculator.
		/// </summary>
		/// <param name="calculator">SummaryCalculator to register</param>
		/// <remarks>
		/// <para class="body">
		/// You can un-register and thus effectively remove from the summary selection UI
		/// any previously registered summary calculators as well as the built-in ones.
		/// </para>
		/// <seealso cref="Register"/>
		/// </remarks>
		public static void UnRegister( SummaryCalculator calculator )
		{
			UnRegister( calculator.Name );
		}

		#endregion // UnRegister

		#endregion // Public Methods

		#region Private/Internal Methods

		#region ConvertDataType

		internal static object ConvertDataValue( object val, Type toType, SummaryResult summaryResult, Record record )
		{
			return Utilities.ConvertDataValue( val, toType, null, null );
		}

		#endregion // ConvertDataType

		#region ConvertToFieldEditAsType

		internal static object ConvertToFieldEditAsType( object val, SummaryResult summaryResult, Record record )
		{
			Field sourceField = null != summaryResult ? summaryResult.SourceField : null;
			
			
			
			
			Type type = null != sourceField ? sourceField.EditAsTypeResolved : null;

			Debug.Assert( null != type );
			if ( null == type )
				type = typeof( string );

			return ConvertDataValue( val, type, summaryResult, record );
		}

		#endregion // ConvertToFieldEditAsType

		#region GetCalculator

		internal static SummaryCalculator GetCalculator( string name )
		{
			lock ( g_registeredCalculators )
			{
				foreach ( SummaryCalculator calculator in g_registeredCalculators )
				{
					if ( calculator.Name == name )
						return calculator;
				}
			}

			return null;
		}

		#endregion // GetCalculator

		#region ToDecimal

		internal static bool ToDecimal( object val, out decimal number, SummaryResult summaryResult, Record record )
		{
			number = decimal.Zero;

			if ( GridUtilities.IsNullOrEmpty( val ) )
				return false;

			if ( !( val is decimal ) )
			{
				if ( !( val is double ) || !double.IsNaN( (double)val ) )
				{
					// JM 11-21-08 BR34688 TFS6240 - Bypass NaN and Infinity for floats and doubles.
					if (val is double)
					{
						if (double.IsNaN((double)val) || double.IsInfinity((double)val))
							return false;
					}
					else
					if (val is float)
					{
						if (float.IsNaN((float)val) || float.IsInfinity((float)val))
							return false;
					}
					
					val = ConvertDataValue(val, typeof(decimal), summaryResult, record);
				}
			}

			if ( val is decimal )
			{
				number = (decimal)val;
				return true;
			}

			return false;
		}

		#endregion // ToDecimal

		#endregion // Private/Internal Methods

		#endregion // Methods
	}

	#endregion // SummaryCalculator Class

	#region AverageSummaryCalculator Class

	/// <summary>
	/// Summary calculator for calculating the <i>average</i>.
	/// </summary>
	/// <remarks>
	/// <seealso cref="SummaryCalculator"/>
	/// <seealso cref="AverageSummaryCalculator"/>
	/// <seealso cref="SumSummaryCalculator"/>
	/// <seealso cref="CountSummaryCalculator"/>
	/// <seealso cref="MaximumSummaryCalculator"/>
	/// <seealso cref="MinimumSummaryCalculator"/>
	/// <seealso cref="SummaryDefinition"/>
	/// <seealso cref="FieldLayout.SummaryDefinitions"/>
	/// </remarks>
	public class AverageSummaryCalculator : SummaryCalculator
	{
		#region Constants

		internal static readonly string NAME = "Average";
		internal static readonly string SR_DISPLAY_NAME = "SummaryCalculator_Average_Name";
		internal static readonly string SR_DESCRIPTION = "SummaryCalculator_Average_Description";
		internal static readonly string RES_IMAGE = "SummaryCalculator_Average_Image";

		#endregion // Constants

		#region Private Vars

		private int _count;
		private decimal _sum;

		#endregion // Private Vars

		#region Constructor

		/// <summary>
		/// Constructor. Initializes a new instance of SumSummaryCalculator.
		/// </summary>
		public AverageSummaryCalculator( ) : base( )
		{
		}

		#endregion // Constructor

		#region Base Overrides

		#region Properties

		#region Public Properties

		#region Description

		/// <summary>
		/// Description of the summary calculator.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>Note:</b> This will be displayed in the summary calculator selection UI as a tooltip
		/// on the item that represents this summary calculator.
		/// </para>
		/// </remarks>
		public override string Description
		{
			get { return DataPresenterBase.GetString(SR_DESCRIPTION); }
		}

		#endregion // Description

		#region DisplayName

		// SSP 8/29/08 BR35879
		// Added DisplayName property to allow localization. Name will contain non-localized value
		// so it can be used from code to identify the summary calculator without worrying about
		// what the DisplayName is localized to.
		// 
		/// <summary>
		/// Returns the display name of the summary calculator. This will be displayed in the UI.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>Note:</b> This will be displayed in the summary calculator selection UI to indentify to
		/// the user what type of summary calculation this is.
		/// </para>
		/// </remarks>
		/// <seealso cref="Name"/>
		public override string DisplayName
		{
			get
			{
				return DataPresenterBase.GetString(SR_DISPLAY_NAME);
			}
		}

		#endregion // DisplayName

		#region Name

		/// <summary>
		/// Returns the name of the summary calculator. This identifies the summary calculator.
		/// </summary>
		/// <remarks>
		/// <b>Name</b> property uniquely identifies the summary calculator. <see cref="DisplayName"/>
		/// also returns the name of the summary calculator however <i>DisplayName</i> is used in the
		/// user interface to denote this calculator.
		/// </remarks>
		/// <seealso cref="DisplayName"/>
		public override string Name
		{
			get 
			{
				return NAME;
			}
		}

		#endregion // Name

		#endregion // Public Properties

		#endregion // Properties

		#region Methods

		#region Public Methods

		#region Aggregate

		/// <summary>
		/// Processes individual piece of data value.
		/// </summary>
		/// <param name="dataValue">Data value to process into calculation</param>
		/// <param name="summaryResult">SummaryResult object for which the summary is being calculated.</param>
		/// <param name="record">The record associated with the data value.</param>
		/// <remarks>
		/// <para class="body">
		/// This method is called for each data value that is to be aggregated into the calculation.
		/// </para>
		/// <para class="body">
		/// <b>Note that</b> <b>summaryResult</b> and <b>record</b> parameters are for extra context only 
		/// and typically it's not necessary to look at them or take them into account for calculation purposes.
		/// The <b>dataValue</b> should suffice for most purposes.
		/// </para>
		/// </remarks>
		public override void Aggregate( object dataValue, SummaryResult summaryResult, Record record )
		{
			decimal nn;
			if ( SummaryCalculator.ToDecimal( dataValue, out nn, summaryResult, record ) )
			{
				_sum += nn;
				_count++;
			}
		}

		#endregion // Aggregate

		#region BeginCalculation

		/// <summary>
		/// Starts summary calculation.
		/// </summary>
		/// <param name="summaryResult">SummaryResult object for which the summary is being calculated.</param>
		/// <remarks>
		/// <para class="body">
		/// This method is called to start calculation of a summary for a particular set of data (associated
		/// with the <i>records</i> parameter). Typically you initialize any member variables in this method.
		/// For example, for a calculator that performs summation, you would initialize the sum member variable
		/// to 0. In <see cref="Aggregate"/>, you would add each value to the sum. In <see cref="EndCalculation"/>,
		/// you would return the result of the calculation.
		/// </para>
		/// <para class="body">
		/// <b>Note that</b> <b>summaryResult</b> parameter is for extra context only 
		/// and typically it's not necessary to use it or take it into account for calculation purposes.
		/// </para>
		/// </remarks>
		public override void BeginCalculation( SummaryResult summaryResult )
		{
			_sum = 0;
			_count = 0;
		}

		#endregion // BeginCalculation

		#region CanProcessDataType

		/// <summary>
		/// Indicates whether this summary calculator can use objects of the specified data type
		/// as source values for the calculation.
		/// </summary>
		/// <param name="dataType">Data type</param>
		/// <returns>Returns true if the specified data type is supported</returns>
		/// <remarks>
		/// <para class="body">
		/// Indicates whether this calculator supports specified data type. <i>Sum</i> for 
		/// example supports only numeric types. Where as <i>Maximum</i> supports all data 
		/// types that implement IComparable.
		/// </para>
		/// </remarks>
		public override bool CanProcessDataType( Type dataType )
		{
			Type type = Utilities.GetUnderlyingType( dataType );
			return Utilities.IsNumericType( type );
		}

		#endregion // CanProcessDataType

		#region EndCalculation

		/// <summary>
		/// Called after all the data has been aggregated into the calculation to return the result of the calculation.
		/// </summary>
		/// <param name="summaryResult">SummaryResult object for which the summary is being calculated.</param>
		/// <returns>Result of the calculation.</returns>
		/// <remarks>
		/// <para class="body">
		/// This method is called after all the data has been aggregated via <see cref="Aggregate"/> method calls.
		/// It returns the result of the calculation.
		/// </para>
		/// <para class="body">
		/// <b>Note that</b> <b>summaryResult</b> parameter is for extra context only 
		/// and typically it's not necessary to use it or take it into account for calculation purposes.
		/// </para>
		/// </remarks>
		public override object EndCalculation( SummaryResult summaryResult )
		{
			decimal average = _count > 0 ? _sum / _count : 0m;

			return average;
		}

		#endregion // EndCalculation

		#endregion // Public Methods

		#endregion // Methods

		#endregion // Base Overrides

	}

	#endregion // AverageSummaryCalculator Class

	#region CountSummaryCalculator Class

	/// <summary>
	/// Summary calculator for counting number of non-empty data items.
	/// </summary>
	/// <remarks>
	/// <seealso cref="SummaryCalculator"/>
	/// <seealso cref="AverageSummaryCalculator"/>
	/// <seealso cref="SumSummaryCalculator"/>
	/// <seealso cref="CountSummaryCalculator"/>
	/// <seealso cref="MaximumSummaryCalculator"/>
	/// <seealso cref="MinimumSummaryCalculator"/>
	/// <seealso cref="SummaryDefinition"/>
	/// <seealso cref="FieldLayout.SummaryDefinitions"/>
	/// </remarks>
	public class CountSummaryCalculator : SummaryCalculator
	{
		#region Constants

		internal static readonly string NAME = "Count";
		internal static readonly string SR_DISPLAY_NAME = "SummaryCalculator_Count_Name";
		internal static readonly string SR_DESCRIPTION = "SummaryCalculator_Count_Description";
		internal static readonly string RES_IMAGE = "SummaryCalculator_Count_Image";

		#endregion // Constants

		#region Private Vars

		private int _count;

		#endregion // Private Vars

		#region Constructor

		/// <summary>
		/// Constructor. Initializes a new instance of CountSummaryCalculator.
		/// </summary>
		public CountSummaryCalculator( ) : base( )
		{
		}

		#endregion // Constructor

		#region Base Overrides

		#region Properties

		#region Public Properties

		#region Description

		/// <summary>
		/// Description of the summary calculator.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>Note:</b> This will be displayed in the summary calculator selection UI as a tooltip
		/// on the item that represents this summary calculator.
		/// </para>
		/// </remarks>
		public override string Description
		{
			get { return DataPresenterBase.GetString(SR_DESCRIPTION); }
		}

		#endregion // Description

		#region DisplayName

		// SSP 8/29/08 BR35879
		// Added DisplayName property to allow localization. Name will contain non-localized value
		// so it can be used from code to identify the summary calculator without worrying about
		// what the DisplayName is localized to.
		// 
		/// <summary>
		/// Returns the display name of the summary calculator. This will be displayed in the UI.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>Note:</b> This will be displayed in the summary calculator selection UI to indentify to
		/// the user what type of summary calculation this is.
		/// </para>
		/// </remarks>
		/// <seealso cref="Name"/>
		public override string DisplayName
		{
			get
			{
				return DataPresenterBase.GetString(SR_DISPLAY_NAME);
			}
		}

		#endregion // DisplayName

		#region Name

		/// <summary>
		/// Returns the name of the summary calculator. This identifies the summary calculator.
		/// </summary>
		/// <remarks>
		/// <b>Name</b> property uniquely identifies the summary calculator. <see cref="DisplayName"/>
		/// also returns the name of the summary calculator however <i>DisplayName</i> is used in the
		/// user interface to denote this calculator.
		/// </remarks>
		/// <seealso cref="DisplayName"/>
		public override string Name
		{
			get
			{
				return NAME;
			}
		}

		#endregion // Name

		#endregion // Public Properties

		#endregion // Properties

		#region Methods

		#region Public Methods

		#region Aggregate

		/// <summary>
		/// Processes individual piece of data value.
		/// </summary>
		/// <param name="dataValue">Data value to process into calculation</param>
		/// <param name="summaryResult">SummaryResult object for which the summary is being calculated.</param>
		/// <param name="record">The record associated with the data value.</param>
		/// <remarks>
		/// <para class="body">
		/// This method is called for each data value that is to be aggregated into the calculation.
		/// </para>
		/// <para class="body">
		/// <b>Note that</b> <b>summaryResult</b> and <b>record</b> parameters are for extra context only 
		/// and typically it's not necessary to look at them or take them into account for calculation purposes.
		/// The <b>dataValue</b> should suffice for most purposes.
		/// </para>
		/// </remarks>
		public override void Aggregate( object dataValue, SummaryResult summaryResult, Record record )
		{
			_count++;
		}

		#endregion // Aggregate

		#region BeginCalculation

		/// <summary>
		/// Starts summary calculation.
		/// </summary>
		/// <param name="summaryResult">SummaryResult object for which the summary is being calculated.</param>
		/// <remarks>
		/// <para class="body">
		/// This method is called to start calculation of a summary for a particular set of data (associated
		/// with the <i>records</i> parameter). Typically you initialize any member variables in this method.
		/// For example, for a calculator that performs summation, you would initialize the sum member variable
		/// to 0. In <see cref="Aggregate"/>, you would add each value to the sum. In <see cref="EndCalculation"/>,
		/// you would return the result of the calculation.
		/// </para>
		/// <para class="body">
		/// <b>Note that</b> <b>summaryResult</b> parameter is for extra context only 
		/// and typically it's not necessary to use it or take it into account for calculation purposes.
		/// </para>
		/// </remarks>
		public override void BeginCalculation( SummaryResult summaryResult )
		{
			_count = 0;
		}

		#endregion // BeginCalculation

		#region CanProcessDataType

		/// <summary>
		/// Indicates whether this summary calculator can use objects of the specified data type
		/// as source values for the calculation.
		/// </summary>
		/// <param name="dataType">Data type</param>
		/// <returns>Returns true if the specified data type is supported</returns>
		/// <remarks>
		/// <para class="body">
		/// Indicates whether this calculator supports specified data type. <i>Sum</i> for 
		/// example supports only numeric types. Where as <i>Maximum</i> supports all data 
		/// types that implement IComparable.
		/// </para>
		/// </remarks>
		public override bool CanProcessDataType( Type dataType )
		{
			// Since this is counting, data type doesn't matter.
			return true;
		}

		#endregion // CanProcessDataType

		#region EndCalculation

		/// <summary>
		/// Called after all the data has been aggregated into the calculation to return the result of the calculation.
		/// </summary>
		/// <param name="summaryResult">SummaryResult object for which the summary is being calculated.</param>
		/// <returns>Result of the calculation.</returns>
		/// <remarks>
		/// <para class="body">
		/// This method is called after all the data has been aggregated via <see cref="Aggregate"/> method calls.
		/// It returns the result of the calculation.
		/// </para>
		/// <para class="body">
		/// <b>Note that</b> <b>summaryResult</b> parameter is for extra context only 
		/// and typically it's not necessary to use it or take it into account for calculation purposes.
		/// </para>
		/// </remarks>
		public override object EndCalculation( SummaryResult summaryResult )
		{
			return _count;
		}

		#endregion // EndCalculation

		#endregion // Public Methods

		#endregion // Methods

		#endregion // Base Overrides
	}

	#endregion // CountSummaryCalculator Class

	#region MaximumSummaryCalculator Class

	/// <summary>
	/// Summary calculator for calculating the <i>maximum</i>.
	/// </summary>
	/// <remarks>
	/// <seealso cref="SummaryCalculator"/>
	/// <seealso cref="AverageSummaryCalculator"/>
	/// <seealso cref="SumSummaryCalculator"/>
	/// <seealso cref="CountSummaryCalculator"/>
	/// <seealso cref="MaximumSummaryCalculator"/>
	/// <seealso cref="MinimumSummaryCalculator"/>
	/// <seealso cref="SummaryDefinition"/>
	/// <seealso cref="FieldLayout.SummaryDefinitions"/>
	/// </remarks>
	public class MaximumSummaryCalculator : SummaryCalculator
	{
		#region Constants

		internal static readonly string NAME = "Maximum";
		internal static readonly string SR_DISPLAY_NAME = "SummaryCalculator_Maximum_Name";
		internal static readonly string SR_DESCRIPTION = "SummaryCalculator_Maximum_Description";
		internal static readonly string RES_IMAGE = "SummaryCalculator_Maximum_Image";

		#endregion // Constants

		#region Private Vars

		private bool _firstTime;
		private object _max;

		#endregion // Private Vars

		#region Constructor

		/// <summary>
		/// Constructor. Initializes a new instance of SumSummaryCalculator.
		/// </summary>
		public MaximumSummaryCalculator( ) : base( )
		{
		}

		#endregion // Constructor

		#region Base Overrides

		#region Properties

		#region Public Properties

		#region Description

		/// <summary>
		/// Description of the summary calculator.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>Note:</b> This will be displayed in the summary calculator selection UI as a tooltip
		/// on the item that represents this summary calculator.
		/// </para>
		/// </remarks>
		public override string Description
		{
			get { return DataPresenterBase.GetString(SR_DESCRIPTION); }
		}

		#endregion // Description

		#region DisplayName

		// SSP 8/29/08 BR35879
		// Added DisplayName property to allow localization. Name will contain non-localized value
		// so it can be used from code to identify the summary calculator without worrying about
		// what the DisplayName is localized to.
		// 
		/// <summary>
		/// Returns the display name of the summary calculator. This will be displayed in the UI.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>Note:</b> This will be displayed in the summary calculator selection UI to indentify to
		/// the user what type of summary calculation this is.
		/// </para>
		/// </remarks>
		/// <seealso cref="Name"/>
		public override string DisplayName
		{
			get
			{
				return DataPresenterBase.GetString(SR_DISPLAY_NAME);
			}
		}

		#endregion // DisplayName

		#region Name

		/// <summary>
		/// Returns the name of the summary calculator. This identifies the summary calculator.
		/// </summary>
		/// <remarks>
		/// <b>Name</b> property uniquely identifies the summary calculator. <see cref="DisplayName"/>
		/// also returns the name of the summary calculator however <i>DisplayName</i> is used in the
		/// user interface to denote this calculator.
		/// </remarks>
		/// <seealso cref="DisplayName"/>
		public override string Name
		{
			get
			{
				return NAME;
			}
		}

		#endregion // Name

		#endregion // Public Properties

		#endregion // Properties

		#region Methods

		#region Public Methods

		#region Aggregate

		/// <summary>
		/// Processes individual piece of data value.
		/// </summary>
		/// <param name="dataValue">Data value to process into calculation</param>
		/// <param name="summaryResult">SummaryResult object for which the summary is being calculated.</param>
		/// <param name="record">The record associated with the data value.</param>
		/// <remarks>
		/// <para class="body">
		/// This method is called for each data value that is to be aggregated into the calculation.
		/// </para>
		/// <para class="body">
		/// <b>Note that</b> <b>summaryResult</b> and <b>record</b> parameters are for extra context only 
		/// and typically it's not necessary to look at them or take them into account for calculation purposes.
		/// The <b>dataValue</b> should suffice for most purposes.
		/// </para>
		/// </remarks>
		public override void Aggregate( object dataValue, SummaryResult summaryResult, Record record )
		{
			MinimumSummaryCalculator.AggregateMinMaxHelper( dataValue, summaryResult, record, false, ref _max, ref _firstTime );
		}

		#endregion // Aggregate

		#region BeginCalculation

		/// <summary>
		/// Starts summary calculation.
		/// </summary>
		/// <param name="summaryResult">SummaryResult object for which the summary is being calculated.</param>
		/// <remarks>
		/// <para class="body">
		/// This method is called to start calculation of a summary for a particular set of data (associated
		/// with the <i>records</i> parameter). Typically you initialize any member variables in this method.
		/// For example, for a calculator that performs summation, you would initialize the sum member variable
		/// to 0. In <see cref="Aggregate"/>, you would add each value to the sum. In <see cref="EndCalculation"/>,
		/// you would return the result of the calculation.
		/// </para>
		/// <para class="body">
		/// <b>Note that</b> <b>summaryResult</b> parameter is for extra context only 
		/// and typically it's not necessary to use it or take it into account for calculation purposes.
		/// </para>
		/// </remarks>
		public override void BeginCalculation( SummaryResult summaryResult )
		{
			_max = null;
			_firstTime = true;
		}

		#endregion // BeginCalculation

		#region CanProcessDataType

		/// <summary>
		/// Indicates whether this summary calculator can use objects of the specified data type
		/// as source values for the calculation.
		/// </summary>
		/// <param name="dataType">Data type</param>
		/// <returns>Returns true if the specified data type is supported</returns>
		/// <remarks>
		/// <para class="body">
		/// Indicates whether this calculator supports specified data type. <i>Sum</i> for 
		/// example supports only numeric types. Where as <i>Maximum</i> supports all data 
		/// types that implement IComparable.
		/// </para>
		/// </remarks>
		public override bool CanProcessDataType( Type dataType )
		{
			Type type = Utilities.GetUnderlyingType( dataType );

			if ( typeof( IComparable ).IsAssignableFrom( type ) )
				return true;

			return false;
		}

		#endregion // CanProcessDataType

		#region EndCalculation

		/// <summary>
		/// Called after all the data has been aggregated into the calculation to return the result of the calculation.
		/// </summary>
		/// <param name="summaryResult">SummaryResult object for which the summary is being calculated.</param>
		/// <returns>Result of the calculation.</returns>
		/// <remarks>
		/// <para class="body">
		/// This method is called after all the data has been aggregated via <see cref="Aggregate"/> method calls.
		/// It returns the result of the calculation.
		/// </para>
		/// <para class="body">
		/// <b>Note that</b> <b>summaryResult</b> parameter is for extra context only 
		/// and typically it's not necessary to use it or take it into account for calculation purposes.
		/// </para>
		/// </remarks>
		public override object EndCalculation( SummaryResult summaryResult )
		{
			return _max;
		}

		#endregion // EndCalculation

		#endregion // Public Methods

		#endregion // Methods

		#endregion // Base Overrides
	}

	#endregion // MaximumSummaryCalculator Class

	#region MinimumSummaryCalculator Class

	/// <summary>
	/// Summary calculator for calculating the <i>total</i>.
	/// </summary>
	/// <remarks>
	/// <seealso cref="SummaryCalculator"/>
	/// <seealso cref="AverageSummaryCalculator"/>
	/// <seealso cref="SumSummaryCalculator"/>
	/// <seealso cref="CountSummaryCalculator"/>
	/// <seealso cref="MaximumSummaryCalculator"/>
	/// <seealso cref="MinimumSummaryCalculator"/>
	/// <seealso cref="SummaryDefinition"/>
	/// <seealso cref="FieldLayout.SummaryDefinitions"/>
	/// </remarks>
	public class MinimumSummaryCalculator : SummaryCalculator
	{
		#region Constants

		internal static readonly string NAME = "Minimum";
		internal static readonly string SR_DISPLAY_NAME = "SummaryCalculator_Minimum_Name";
		internal static readonly string SR_DESCRIPTION = "SummaryCalculator_Minimum_Description";
		internal static readonly string RES_IMAGE = "SummaryCalculator_Minimum_Image";

		#endregion // Constants

		#region Private Vars

		private bool _firstTime;
		private object _min;

		#endregion // Private Vars

		#region Constructor

		/// <summary>
		/// Constructor. Initializes a new instance of SumSummaryCalculator.
		/// </summary>
		public MinimumSummaryCalculator( ) : base( )
		{
		}

		#endregion // Constructor

		#region Base Overrides

		#region Properties

		#region Public Properties

		#region Description

		/// <summary>
		/// Description of the summary calculator.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>Note:</b> This will be displayed in the summary calculator selection UI as a tooltip
		/// on the item that represents this summary calculator.
		/// </para>
		/// </remarks>
		public override string Description
		{
			get { return DataPresenterBase.GetString(SR_DESCRIPTION); }
		}

		#endregion // Description

		#region DisplayName

		// SSP 8/29/08 BR35879
		// Added DisplayName property to allow localization. Name will contain non-localized value
		// so it can be used from code to identify the summary calculator without worrying about
		// what the DisplayName is localized to.
		// 
		/// <summary>
		/// Returns the display name of the summary calculator. This will be displayed in the UI.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>Note:</b> This will be displayed in the summary calculator selection UI to indentify to
		/// the user what type of summary calculation this is.
		/// </para>
		/// </remarks>
		/// <seealso cref="Name"/>
		public override string DisplayName
		{
			get
			{
				return DataPresenterBase.GetString(SR_DISPLAY_NAME);
			}
		}

		#endregion // DisplayName

		#region Name

		/// <summary>
		/// Returns the name of the summary calculator. This identifies the summary calculator.
		/// </summary>
		/// <remarks>
		/// <b>Name</b> property uniquely identifies the summary calculator. <see cref="DisplayName"/>
		/// also returns the name of the summary calculator however <i>DisplayName</i> is used in the
		/// user interface to denote this calculator.
		/// </remarks>
		/// <seealso cref="DisplayName"/>
		public override string Name
		{
			get
			{
				return NAME;
			}
		}

		#endregion // Name

		#endregion // Public Properties

		#endregion // Properties

		#region Methods

		#region Public Methods

		#region Aggregate

		/// <summary>
		/// Processes individual piece of data value.
		/// </summary>
		/// <param name="dataValue">Data value to process into calculation</param>
		/// <param name="summaryResult">SummaryResult object for which the summary is being calculated.</param>
		/// <param name="record">The record associated with the data value.</param>
		/// <remarks>
		/// <para class="body">
		/// This method is called for each data value that is to be aggregated into the calculation.
		/// </para>
		/// <para class="body">
		/// <b>Note that</b> <b>summaryResult</b> and <b>record</b> parameters are for extra context only 
		/// and typically it's not necessary to look at them or take them into account for calculation purposes.
		/// The <b>dataValue</b> should suffice for most purposes.
		/// </para>
		/// </remarks>
		public override void Aggregate( object dataValue, SummaryResult summaryResult, Record record )
		{
			MinimumSummaryCalculator.AggregateMinMaxHelper( dataValue, summaryResult, record, true, ref _min, ref _firstTime );
		}

		#endregion // Aggregate

		#region BeginCalculation

		/// <summary>
		/// Starts summary calculation.
		/// </summary>
		/// <param name="summaryResult">SummaryResult object for which the summary is being calculated.</param>
		/// <remarks>
		/// <para class="body">
		/// This method is called to start calculation of a summary for a particular set of data (associated
		/// with the <i>records</i> parameter). Typically you initialize any member variables in this method.
		/// For example, for a calculator that performs summation, you would initialize the sum member variable
		/// to 0. In <see cref="Aggregate"/>, you would add each value to the sum. In <see cref="EndCalculation"/>,
		/// you would return the result of the calculation.
		/// </para>
		/// <para class="body">
		/// <b>Note that</b> <b>summaryResult</b> parameter is for extra context only 
		/// and typically it's not necessary to use it or take it into account for calculation purposes.
		/// </para>
		/// </remarks>
		public override void BeginCalculation( SummaryResult summaryResult )
		{
			_min = null;
			_firstTime = true;
		}

		#endregion // BeginCalculation

		#region CanProcessDataType

		/// <summary>
		/// Indicates whether this summary calculator can use objects of the specified data type
		/// as source values for the calculation.
		/// </summary>
		/// <param name="dataType">Data type</param>
		/// <returns>Returns true if the specified data type is supported</returns>
		/// <remarks>
		/// <para class="body">
		/// Indicates whether this calculator supports specified data type. <i>Sum</i> for 
		/// example supports only numeric types. Where as <i>Maximum</i> supports all data 
		/// types that implement IComparable.
		/// </para>
		/// </remarks>
		public override bool CanProcessDataType( Type dataType )
		{
			Type type = Utilities.GetUnderlyingType( dataType );

			if ( typeof( IComparable ).IsAssignableFrom( type ) )
				return true;

			return false;
		}

		#endregion // CanProcessDataType

		#region EndCalculation

		/// <summary>
		/// Called after all the data has been aggregated into the calculation to return the result of the calculation.
		/// </summary>
		/// <param name="summaryResult">SummaryResult object for which the summary is being calculated.</param>
		/// <returns>Result of the calculation.</returns>
		/// <remarks>
		/// <para class="body">
		/// This method is called after all the data has been aggregated via <see cref="Aggregate"/> method calls.
		/// It returns the result of the calculation.
		/// </para>
		/// <para class="body">
		/// <b>Note that</b> <b>summaryResult</b> parameter is for extra context only 
		/// and typically it's not necessary to use it or take it into account for calculation purposes.
		/// </para>
		/// </remarks>
		public override object EndCalculation( SummaryResult summaryResult )
		{
			return _min;
		}

		#endregion // EndCalculation

		#endregion // Public Methods

		#endregion // Methods

		#endregion // Base Overrides

		#region Methods

		#region Internal Methods

		#region AggregateMinMaxHelper

		internal static void AggregateMinMaxHelper( object dataValue, SummaryResult summaryResult,
			Record record, bool calcMin, ref object currMinOrMax, ref bool firstTime )
		{
			if ( GridUtilities.IsNullOrEmpty( dataValue ) )
			{
				if ( calcMin && null == currMinOrMax )
					currMinOrMax = dataValue;

				return;
			}

			object val = ConvertToFieldEditAsType( dataValue, summaryResult, record );

			// If conversion fails, return.
			// 
			//Debug.Assert( null != val );
			if ( null == val )
				return;

			if ( firstTime )
			{
				currMinOrMax = val;
				firstTime = false;
			}
			else
			{
				int r;

				// If the type is IComparable then use that.
				if ( val is IComparable )
				{
					r = ( (IComparable)val ).CompareTo( currMinOrMax );
				}
				else
				{
					// Otherwise convert to string and compare strings.
					// 
					string currMinOrMaxStr = (string)ConvertDataValue( currMinOrMax, typeof( string ), summaryResult, record );
					string valStr = (string)ConvertDataValue( val, typeof( string ), summaryResult, record );

					r = String.Compare( valStr, currMinOrMaxStr, false, System.Globalization.CultureInfo.CurrentCulture );
				}

				if ( calcMin ? r < 0 : r > 0 )
					currMinOrMax = val;
			}
		}

		#endregion // AggregateMinMaxHelper

		#endregion // Internal Methods

		#endregion // Methods
	}

	#endregion // MinimumSummaryCalculator Class

	#region SumSummaryCalculator Class

	/// <summary>
	/// Summary calculator for calculating the <i>total</i>.
	/// </summary>
	/// <remarks>
	/// <seealso cref="SummaryCalculator"/>
	/// <seealso cref="AverageSummaryCalculator"/>
	/// <seealso cref="SumSummaryCalculator"/>
	/// <seealso cref="CountSummaryCalculator"/>
	/// <seealso cref="MaximumSummaryCalculator"/>
	/// <seealso cref="MinimumSummaryCalculator"/>
	/// <seealso cref="SummaryDefinition"/>
	/// <seealso cref="FieldLayout.SummaryDefinitions"/>
	/// </remarks>
	public class SumSummaryCalculator : SummaryCalculator
	{
		#region Constants

		internal static readonly string NAME = "Sum";
		internal static readonly string SR_DISPLAY_NAME = "SummaryCalculator_Sum_Name";
		internal static readonly string SR_DESCRIPTION = "SummaryCalculator_Sum_Description";
		internal static readonly string RES_IMAGE = "SummaryCalculator_Sum_Image";

		#endregion // Constants

		#region Private Vars

		private decimal _sum;

		#endregion // Private Vars

		#region Constructor

		/// <summary>
		/// Constructor. Initializes a new instance of SumSummaryCalculator.
		/// </summary>
		public SumSummaryCalculator( ) : base( )
		{
		}

		#endregion // Constructor

		#region Base Overrides

		#region Properties

		#region Public Properties

		#region Description

		/// <summary>
		/// Description of the summary calculator.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>Note:</b> This will be displayed in the summary calculator selection UI as a tooltip
		/// on the item that represents this summary calculator.
		/// </para>
		/// </remarks>
		public override string Description
		{
			get { return DataPresenterBase.GetString(SR_DESCRIPTION); }
		}

		#endregion // Description

		#region DisplayName

		// SSP 8/29/08 BR35879
		// Added DisplayName property to allow localization. Name will contain non-localized value
		// so it can be used from code to identify the summary calculator without worrying about
		// what the DisplayName is localized to.
		// 
		/// <summary>
		/// Returns the display name of the summary calculator. This will be displayed in the UI.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>Note:</b> This will be displayed in the summary calculator selection UI to indentify to
		/// the user what type of summary calculation this is.
		/// </para>
		/// </remarks>
		/// <seealso cref="Name"/>
		public override string DisplayName
		{
			get
			{
				return DataPresenterBase.GetString(SR_DISPLAY_NAME);
			}
		}

		#endregion // DisplayName

		#region Name

		/// <summary>
		/// Returns the name of the summary calculator. This identifies the summary calculator.
		/// </summary>
		/// <remarks>
		/// <b>Name</b> property uniquely identifies the summary calculator. <see cref="DisplayName"/>
		/// also returns the name of the summary calculator however <i>DisplayName</i> is used in the
		/// user interface to denote this calculator.
		/// </remarks>
		/// <seealso cref="DisplayName"/>
		public override string Name
		{
			get
			{
				return NAME;
			}
		}

		#endregion // Name

		#endregion // Public Properties

		#endregion // Properties

		#region Methods

		#region Public Methods

		#region Aggregate

		/// <summary>
		/// Processes individual piece of data value.
		/// </summary>
		/// <param name="dataValue">Data value to process into calculation</param>
		/// <param name="summaryResult">SummaryResult object for which the summary is being calculated.</param>
		/// <param name="record">The record associated with the data value.</param>
		/// <remarks>
		/// <para class="body">
		/// This method is called for each data value that is to be aggregated into the calculation.
		/// </para>
		/// <para class="body">
		/// <b>Note that</b> <b>summaryResult</b> and <b>record</b> parameters are for extra context only 
		/// and typically it's not necessary to look at them or take them into account for calculation purposes.
		/// The <b>dataValue</b> should suffice for most purposes.
		/// </para>
		/// </remarks>
		public override void Aggregate( object dataValue, SummaryResult summaryResult, Record record )
		{
			decimal nn;
			if ( SummaryCalculator.ToDecimal( dataValue, out nn, summaryResult, record ) )
			{
				_sum += nn;
			}
		}

		#endregion // Aggregate

		#region BeginCalculation

		/// <summary>
		/// Starts summary calculation.
		/// </summary>
		/// <param name="summaryResult">SummaryResult object for which the summary is being calculated.</param>
		/// <remarks>
		/// <para class="body">
		/// This method is called to start calculation of a summary for a particular set of data (associated
		/// with the <i>records</i> parameter). Typically you initialize any member variables in this method.
		/// For example, for a calculator that performs summation, you would initialize the sum member variable
		/// to 0. In <see cref="Aggregate"/>, you would add each value to the sum. In <see cref="EndCalculation"/>,
		/// you would return the result of the calculation.
		/// </para>
		/// <para class="body">
		/// <b>Note that</b> <b>summaryResult</b> parameter is for extra context only 
		/// and typically it's not necessary to use it or take it into account for calculation purposes.
		/// </para>
		/// </remarks>
		public override void BeginCalculation( SummaryResult summaryResult )
		{
			_sum = 0;
		}

		#endregion // BeginCalculation

		#region CanProcessDataType

		/// <summary>
		/// Indicates whether this summary calculator can use objects of the specified data type
		/// as source values for the calculation.
		/// </summary>
		/// <param name="dataType">Data type</param>
		/// <returns>Returns true if the specified data type is supported</returns>
		/// <remarks>
		/// <para class="body">
		/// Indicates whether this calculator supports specified data type. <i>Sum</i> for 
		/// example supports only numeric types. Where as <i>Maximum</i> supports all data 
		/// types that implement IComparable.
		/// </para>
		/// </remarks>
		public override bool CanProcessDataType( Type dataType )
		{
			Type type = Utilities.GetUnderlyingType( dataType );
			return Utilities.IsNumericType( type );
		}

		#endregion // CanProcessDataType

		#region EndCalculation

		/// <summary>
		/// Called after all the data has been aggregated into the calculation to return the result of the calculation.
		/// </summary>
		/// <param name="summaryResult">SummaryResult object for which the summary is being calculated.</param>
		/// <returns>Result of the calculation.</returns>
		/// <remarks>
		/// <para class="body">
		/// This method is called after all the data has been aggregated via <see cref="Aggregate"/> method calls.
		/// It returns the result of the calculation.
		/// </para>
		/// <para class="body">
		/// <b>Note that</b> <b>summaryResult</b> parameter is for extra context only 
		/// and typically it's not necessary to use it or take it into account for calculation purposes.
		/// </para>
		/// </remarks>
		public override object EndCalculation( SummaryResult summaryResult )
		{
			return _sum;
		}

		#endregion // EndCalculation

		#endregion // Public Methods

		#endregion // Methods

		#endregion // Base Overrides
	}

	#endregion // SumSummaryCalculator Class

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