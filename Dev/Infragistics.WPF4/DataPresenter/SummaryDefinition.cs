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

using Infragistics.Windows.DataPresenter.Calculations;


namespace Infragistics.Windows.DataPresenter
{

	#region SummaryDefinition Class

	/// <summary>
	/// Describes a summary to calculate.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// A <b>SummaryDefinition</b> object describes summary calculation to perform on values of a perticular field.
	/// It contains information on what type of calculation to perform (whether sum, average etc...) via its
	/// <see cref="SummaryDefinition.Calculator"/> property. It also has various properties like 
	/// <see cref="SummaryDefinition.DisplayArea"/> and <see cref="SummaryDefinition.Position"/> that control 
	/// where the calculation results are displayed.
	/// </para>
	/// <para class="body">
	/// Add one or more instance of this object to FieldLayout's <see cref="Infragistics.Windows.DataPresenter.FieldLayout.SummaryDefinitions"/> collection to 
	/// summarize data. The calculation results are stored in <see cref="SummaryResult"/> objects. Summary results can be
	/// accessed via RecordCollection's <see cref="RecordCollectionBase.SummaryResults"/> property.
	/// </para>
	/// <para class="body">
	/// To enable the user interface that lets the user summarize data, set the FieldSettings'
	/// <see cref="FieldSettings.AllowSummaries"/> and <see cref="FieldSettings.SummaryUIType"/> properties.
	/// </para>
	/// <seealso cref="Infragistics.Windows.DataPresenter.FieldLayout.SummaryDefinitions"/>
	/// <seealso cref="SummaryResult"/>
	/// <seealso cref="RecordCollectionBase.SummaryResults"/>
	/// <see cref="FieldSettings.AllowSummaries"/>
	/// <see cref="FieldSettings.SummaryUIType"/>
	/// </remarks>
	public class SummaryDefinition : DependencyObject
	{
		#region Nested Data Structures

		#region SerializationInfo Class

		// SSP 9/3/09 TFS18172
		// Added SerializationInfo for saving and loading summaries.
		// 

		/// <summary>
		/// Provides necessary information to the ObjectSerializerBase for serializing and deserializing a ConditionGroup.
		/// </summary>
		internal class SerializationInfo : DependencyObjectSerializationInfo
		{
			#region Member Vars

			private IEnumerable<PropertySerializationInfo> _props;

			#endregion // Member Vars

			#region Constructor

			internal SerializationInfo( )
				: base( typeof( SummaryDefinition ),
					new DependencyProperty[]
					{
						SummaryDefinition.KeyProperty,
						SummaryDefinition.DisplayAreaProperty,
						SummaryDefinition.PositionFieldNameProperty,
						SummaryDefinition.PositionProperty,
						SummaryDefinition.SourceFieldNameProperty,
						SummaryDefinition.StringFormatProperty,
						SummaryDefinition.StringFormatProviderProperty,
						SummaryDefinition.ToolTipProperty
					}
					)
			{
			}

			#endregion // Constructor

			#region Base Overrides

			#region SerializedProperties

			public override IEnumerable<PropertySerializationInfo> SerializedProperties
			{
				get
				{
					if ( null == _props )
					{
						_props = GridUtilities.Aggregate<PropertySerializationInfo>(
							base.SerializedProperties,
							new PropertySerializationInfo[]
							{
								new PropertySerializationInfo( typeof( string ), "Calculator" )
							}
						);
					}

					return _props;
				}
			}

			#endregion // SerializedProperties

			#region Serialize

			public override Dictionary<string, object> Serialize( object obj )
			{
				SummaryDefinition summary = (SummaryDefinition)obj;

				Dictionary<string, object> values = base.Serialize( summary );

				SummaryCalculator calculator = summary.Calculator;
				string calculatorName = null != calculator ? calculator.Name : null;
				if ( null != calculator && !string.IsNullOrEmpty( calculatorName ) )
					values["Calculator"] = calculatorName;

				return values;
			}

			#endregion // Serialize

			#region Deserialize

			public override object Deserialize( Dictionary<string, object> values )
			{
				SummaryDefinition summary = (SummaryDefinition)base.Deserialize( values );

				object v;
				if ( values.TryGetValue( "Calculator", out v ) )
					summary.Calculator = SummaryCalculator.GetCalculator( (string)v );

				return summary;
			}

			#endregion // Deserialize

			#endregion // Base Overrides
		}

		#endregion // SerializationInfo Class

		#endregion // Nested Data Structures

		#region Private Vars

		private SummaryDefinitionCollection _parentCollection;
		private int _cachedCalculationVersion;

		#endregion // Private Vars

		#region Constructor

		/// <summary>
		/// Constructor. Initializes a new instance of <see cref="SummaryDefinition"/> object.
		/// </summary>
		public SummaryDefinition( )
		{
		}

		#endregion // Constructor

		#region Base Overrides

		/// <summary>
		/// Overridden. Called when a property's value changes.
		/// </summary>
		/// <param name="e">Event args that contains information on property value change.</param>
		protected override void OnPropertyChanged( DependencyPropertyChangedEventArgs e )
		{
			base.OnPropertyChanged( e );

			SummaryDefinitionCollection parentColl = this.ParentCollection;
			FieldLayout fieldLayout = null != parentColl ? parentColl.FieldLayout : null;

			bool bumpCalcVersion = false;
			bool summaryVisibilityPotentiallyChanged = false;
			bool recalcDisplayText = false;
			DependencyProperty prop = e.Property;

			if ( CalculatorProperty == prop )
			{
				bumpCalcVersion = true;
			}
			else if ( SourceFieldNameProperty == prop )
			{
				bumpCalcVersion = true;
				summaryVisibilityPotentiallyChanged = true;
			}
			else if ( DisplayAreaProperty == prop )
			{
				summaryVisibilityPotentiallyChanged = true;
			}
			else if ( PositionFieldNameProperty == prop )
			{
				summaryVisibilityPotentiallyChanged = true;
			}
			else if ( PositionProperty == prop )
			{
				summaryVisibilityPotentiallyChanged = true;
			}
			else if ( StringFormatProperty == prop )
			{
				recalcDisplayText = true;
			}
			else if ( StringFormatProviderProperty == prop )
			{
				recalcDisplayText = true;
			}

			// Bumping calc version will also cause us to recalculate the display text.
			// 
			if ( bumpCalcVersion || recalcDisplayText )
				this.BumpCalculationVersion( );

			if ( summaryVisibilityPotentiallyChanged && null != parentColl )
				parentColl.BumpSummariesVersion( );
		}

		#endregion // Base Overrides

		#region Properties

		#region Public Properties

		#region Calculator

		/// <summary>
		/// Identifies the <see cref="Calculator"/> dependency property
		/// </summary>
		public static readonly DependencyProperty CalculatorProperty = DependencyProperty.Register(
				"Calculator",
				typeof( SummaryCalculator ),
				typeof( SummaryDefinition ),
				new FrameworkPropertyMetadata( null )
			);

		/// <summary>
		/// Specifies the type of summary calculation to perform.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// Built-in summary calculators are provided via static <see cref="SummaryCalculator.Average"/>,
		/// <see cref="SummaryCalculator.Count"/>, <see cref="SummaryCalculator.Maximum"/>, 
		/// <see cref="SummaryCalculator.Minimum"/> and <see cref="SummaryCalculator.Sum"/> static
		/// properties of <see cref="SummaryCalculator"/> class.
		/// </para>
		/// <para class="body">
		/// You can also set this property to a custom summary calculator. To specify a custom summary calculation 
		/// logic by deriving a class from SummaryCalculator and overriding methods to provide custom calculation logic.
		/// </para>
		/// </remarks>
		//[Description( "Specifies the type of summary calculation to perform." )]
		//[Category( "Data" )]
		[Bindable( true )]
		public SummaryCalculator Calculator
		{
			get
			{
				return (SummaryCalculator)this.GetValue( CalculatorProperty );
			}
			set
			{
				this.SetValue( CalculatorProperty, value );
			}
		}

		/// <summary>
		/// Returns true if the Calculator property is set to a non-default value.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public bool ShouldSerializeCalculator( )
		{
			return Utilities.ShouldSerialize( CalculatorProperty, this );
		}

		/// <summary>
		/// Resets the Calculator property to its default state.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public void ResetCalculator( )
		{
			this.ClearValue( CalculatorProperty );
		}

		#endregion // Calculator

		#region CalculationSettings


		/// <summary>
		/// Identifies the <see cref="CalculationSettings"/> dependency property
		/// </summary>
		[InfragisticsFeature(FeatureName = "XamCalculationManager", Version = "11.2")]
		public static readonly DependencyProperty CalculationSettingsProperty = DependencyProperty.Register("CalculationSettings",
			typeof(SummaryCalculationSettings), typeof(SummaryDefinition), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnCalculationSettingsChanged)));

		private static void OnCalculationSettingsChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			SummaryDefinition summarydef = target as SummaryDefinition;

			summarydef._calculationSettings = e.NewValue as SummaryCalculationSettings;
		}

		private SummaryCalculationSettings _calculationSettings;

		/// <summary>
		/// Get/sets the calculation settings related with this summary definition. This information is used for exposing cell summary data to a XamCalculationManager's network.
		/// </summary>
		/// <remarks>
		/// <para class="note"><b>Note: </b> this property will be ignored unless <see cref="DataPresenterBase"/>'s  <see cref="DataPresenterBase.CalculationAdapter"/> is set.</para>
		/// <para class="body">
		/// <b>Note:</b> To format the result of the formula, specify the <see cref="StringFormat"/> property.
		/// </para>
		/// <para class="body">
		/// <b>Note:</b> If you specify <seealso cref="DataPresenterCalculationSettingsBase.Formula"/> on the 
		/// CalculationSettings then <see cref="SourceFieldName"/> and <see cref="Calculator"/> properties must not 
		/// be specified. Otherwise the Formula setting will be ignored and SourceField and Calculator settings
		/// will be honored. They are exclusive. You can either use the XamCalculationManager's formula calculation
		/// functionality for the summary or the built-in summary calculators for a summary.
		/// </para>
		/// </remarks>
		/// <seealso cref="CalculationSettingsProperty"/>
		/// <seealso cref="DataPresenterBase.CalculationAdapter"/>
		/// <seealso cref="Field.CalculationSettings"/>
		/// <seealso cref="StringFormat"/>
		//[Description("Get/sets the calculation settings related to this field.")]
		//[Category("Behavior")]
		[InfragisticsFeature(FeatureName = "XamCalculationManager", Version = "11.2")]
		public SummaryCalculationSettings CalculationSettings
		{
			get
			{
				return _calculationSettings;
			}
			set
			{
				this.SetValue(SummaryDefinition.CalculationSettingsProperty, value);
			}
		}

		#endregion //CalculationSettings

		#region DisplayArea

		/// <summary>
		/// Identifies the <see cref="DisplayArea"/> dependency property
		/// </summary>
		public static readonly DependencyProperty DisplayAreaProperty = DependencyProperty.Register(
				"DisplayArea",
				typeof( SummaryDisplayAreas? ),
				typeof( SummaryDefinition ),
				new FrameworkPropertyMetadata( null )
			);

		/// <summary>
		/// Specifies if and where the summary is displayed. 
		/// Default is resolved to <b>TopLevelOnly</b> and <b>InGroupByRecords</b>.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>DisplayArea</b> property controls if and where summary calculation results for this
		/// field are displayed. It's a <i>flags enum</i> type (see <see cref="SummaryDisplayAreas"/>) so
		/// you can specify multiple display areas where the summary should be displayed.
		/// You can also specify summary display area on a field basis by setting FieldSettings' 
		/// <see cref="FieldSettings.SummaryDisplayArea"/> property.
		/// </para>
		/// <para class="body">
		/// By default this property is resolved to <b>TopLevelOnly</b> and <b>InGroupByRecords</b>, which 
		/// means that the summaries will be displayed in each group-by record (if there are group-by records)
		/// and for the top level records. See <see cref="SummaryDisplayAreas"/> enum for more information.
		/// </para>
		/// <para class="body">
		/// To enable the UI that lets the user select summary calculations to perform on fields, set the
		/// FieldSettings' <see cref="FieldSettings.AllowSummaries"/> property. See 
		/// <see cref="FieldSettings.AllowSummaries"/> for more information.
		/// </para>
		/// </remarks>
		/// <seealso cref="FieldSettings.SummaryDisplayArea"/>
		/// <seealso cref="FieldSettings.AllowSummaries"/>
		/// <seealso cref="FieldSettings.SummaryUIType"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.FieldLayout.SummaryDefinitions"/>
		//[Description( "Specifies if and where summaries are displayed." )]
		//[Category( "Behavior" )]
		[Bindable( true )]
		[TypeConverter(typeof(Infragistics.Windows.Helpers.NullableConverter<SummaryDisplayAreas>))] // AS 5/15/08 BR32816
		public SummaryDisplayAreas? DisplayArea
		{
			get
			{
				return (SummaryDisplayAreas?)this.GetValue( DisplayAreaProperty );
			}
			set
			{
				this.SetValue( DisplayAreaProperty, value );
			}
		}

		/// <summary>
		/// Returns true if the DisplayArea property is set to a non-default value.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public bool ShouldSerializeDisplayArea( )
		{
			return Utilities.ShouldSerialize( DisplayAreaProperty, this );
		}

		/// <summary>
		/// Resets the DisplayArea property to its default state.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public void ResetDisplayArea( )
		{
			this.ClearValue( DisplayAreaProperty );
		}

		#endregion // DisplayArea

		#region StringFormat

		/// <summary>
		/// Identifies the <see cref="StringFormat"/> dependency property
		/// </summary>
		public static readonly DependencyProperty StringFormatProperty = DependencyProperty.Register(
				"StringFormat",
				typeof( string ),
				typeof( SummaryDefinition ),
				new FrameworkPropertyMetadata( null )
			);

		/// <summary>
		/// Specifies the format to use to when displaying the summary result's value.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>StringFormat</b> specifies the format to use when displaying the summary result's value.
		/// The resulting value is returned by the SummaryResult's <see cref="SummaryResult.DisplayText"/> property.
		/// </para>
		/// <para class="body">
		/// String format is used with the <i>String.Format</i> method to format the summary value. This
		/// lets you specify any arbitrary text inside the format. String.Format method is called with
		/// summary result's value, sumamry calculator name (Sum, Average etc...), summary definition's key,
		/// and the source field name as parameters. This means that you can refer to any of these values
		/// inside the format you specify using {0}, {1}, {2} and {3} respectively.
		/// </para>
		/// <para class="body">
		/// The default value of this property is "{1} = {0}".
		/// </para>
		/// </remarks>
		/// <seealso cref="Field.SummaryStringFormats"/>
		/// <seealso cref="StringFormatProvider"/>
		/// <seealso cref="SummaryResult.Value"/>
		/// <seealso cref="SummaryResult.DisplayText"/>
		//[Description( "Format to use to when displaying the summary result." )]
		//[Category( "Data" )]
		[Bindable( true )]
		public string StringFormat
		{
			get
			{
				return (string)this.GetValue( StringFormatProperty );
			}
			set
			{
				this.SetValue( StringFormatProperty, value );
			}
		}

		/// <summary>
		/// Returns true if the StringFormat property is set to a non-default value.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public bool ShouldSerializeStringFormat( )
		{
			return Utilities.ShouldSerialize( StringFormatProperty, this );
		}

		/// <summary>
		/// Resets the StringFormat property to its default state.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public void ResetStringFormat( )
		{
			this.ClearValue( StringFormatProperty );
		}

		#endregion // StringFormat

		#region StringFormatProvider

		/// <summary>
		/// Identifies the <see cref="StringFormatProvider"/> dependency property
		/// </summary>
		public static readonly DependencyProperty StringFormatProviderProperty = DependencyProperty.Register(
				"StringFormatProvider",
				typeof( IFormatProvider ),
				typeof( SummaryDefinition ),
				new FrameworkPropertyMetadata( null )
			);

		/// <summary>
		/// Specifies the format provider to use when formatting the result of the summary.
		/// </summary>
		/// <remarks>
		/// <b>StringFormatProvider</b> along with the <see cref="StringFormat"/> properties are used
		/// to format the summary result for displaying. See <see cref="StringFormat"/> for more information.
		/// </remarks>
		/// <seealso cref="StringFormat"/>
		/// <seealso cref="Field.SummaryStringFormats"/>
		//[Description( "Format provider to use when formatting the summary result." )]
		//[Category( "Data" )]
		[Bindable( true )]
		public IFormatProvider StringFormatProvider
		{
			get
			{
				return (IFormatProvider)this.GetValue( StringFormatProviderProperty );
			}
			set
			{
				this.SetValue( StringFormatProviderProperty, value );
			}
		}

		/// <summary>
		/// Returns true if the StringFormatProvider property is set to a non-default value.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public bool ShouldSerializeStringFormatProvider( )
		{
			return Utilities.ShouldSerialize( StringFormatProviderProperty, this );
		}

		/// <summary>
		/// Resets the StringFormatProvider property to its default state.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public void ResetStringFormatProvider( )
		{
			this.ClearValue( StringFormatProviderProperty );
		}

		#endregion // StringFormatProvider

		#region Key

		/// <summary>
		/// Identifies the <see cref="Key"/> dependency property
		/// </summary>
		public static readonly DependencyProperty KeyProperty = DependencyProperty.Register(
				"Key",
				typeof( string ),
				typeof( SummaryDefinition ),
				new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.None,
					null, new CoerceValueCallback( OnCoerceKey ) )
			);

		/// <summary>
		/// Gets or sets the key of this SummaryDefinition object.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>Key</b> property is used to attach an identifier to a SummaryDefinition. This key
		/// can later be used to access the <see cref="SummaryDefinition"/> from the <see cref="SummaryDefinitionCollection"/>
		/// or the associated <see cref="SummaryResult"/> from the <see cref="SummaryResultCollection"/>.
		/// </para>
		/// </remarks>
		/// <seealso cref="SummaryDefinitionCollection"/>
		/// <seealso cref="SummaryResultCollection"/>
		//[Description( "Key of the SummaryDefinition object" )]
		//[Category( "Data" )]
		[Bindable( true )]
		public string Key
		{
			get
			{
				return (string)this.GetValue( KeyProperty );
			}
			set
			{
				this.SetValue( KeyProperty, value );
			}
		}

		private static object OnCoerceKey( DependencyObject dependencyObject, object valueAsObject )
		{
			string key = (string)valueAsObject;
			return key;
		}

		/// <summary>
		/// Returns true if the Key property is set to a non-default value.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public bool ShouldSerializeKey( )
		{
			return Utilities.ShouldSerialize( KeyProperty, this );
		}

		/// <summary>
		/// Resets the Key property to its default state.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public void ResetKey( )
		{
			this.ClearValue( KeyProperty );
		}

		#endregion // Key

		#region ParentCollection

		/// <summary>
		/// Returns the collection if any that this SummaryDefinition instance belongs to.
		/// </summary>
		public SummaryDefinitionCollection ParentCollection
		{
			get
			{
				return _parentCollection;
			}
		}

		#endregion // ParentCollection

		#region Position

		/// <summary>
		/// Identifies the <see cref="Position"/> dependency property
		/// </summary>
		public static readonly DependencyProperty PositionProperty = DependencyProperty.Register(
				"Position",
				typeof( SummaryPosition ),
				typeof( SummaryDefinition ),
				new FrameworkPropertyMetadata( SummaryPosition.Default )
			);

		/// <summary>
		/// Specifies whether the summary results are displayed aligned with the field
		/// or whether they are displayed left, center or right of the summary record.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// By default summaries are displayed in the summary record aligned with the source
		/// field. You can use <b>Position</b> along with <b>PositionFieldName</b> properties
		/// to change this behavior and instead display the summary under a different field
		/// or display the summary left, center or right part of the summary record.
		/// </para>
		/// <para class="body">
		/// <b>NOTE:</b> If you want to control the visibility and display area of the 
		/// summaries then use the <see cref="DisplayArea"/> property.
		/// </para>
		/// </remarks>
		/// <seealso cref="PositionFieldName"/>
		/// <seealso cref="DisplayArea"/>
		/// <seealso cref="FieldSettings.SummaryDisplayArea"/>
		//[Description( "Position of the summary in the summary record." )]
		//[Category( "Appearance" )]
		[Bindable( true )]
		public SummaryPosition Position
		{
			get
			{
				return (SummaryPosition)this.GetValue( PositionProperty );
			}
			set
			{
				this.SetValue( PositionProperty, value );
			}
		}

		/// <summary>
		/// Returns true if the Position property is set to a non-default value.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public bool ShouldSerializePosition( )
		{
			return Utilities.ShouldSerialize( PositionProperty, this );
		}

		/// <summary>
		/// Resets the Position property to its default state.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public void ResetPosition( )
		{
			this.ClearValue( PositionProperty );
		}

		#endregion // Position

		#region PositionFieldName

		/// <summary>
		/// Identifies the <see cref="PositionFieldName"/> dependency property
		/// </summary>
		public static readonly DependencyProperty PositionFieldNameProperty = DependencyProperty.Register(
				"PositionFieldName",
				typeof( string ),
				typeof( SummaryDefinition ),
				new FrameworkPropertyMetadata( null )
			);


		/// <summary>
		/// Specifies a different field other than the source field under which to position the summary result.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// If <see cref="SummaryPosition"/> property is set to <b>UseSummaryPositionField</b>, then the summary 
		/// will be positioned under this field inside the summary record. If this property is not set, it will 
		/// be positioned under the field specified by SourceFieldName.
		/// </para>
		/// </remarks>
		//[Description( "The field under which to position the summary result." )]
		//[Category( "Appearance" )]
		[Bindable( true )]
		public string PositionFieldName
		{
			get
			{
				return (string)this.GetValue( PositionFieldNameProperty );
			}
			set
			{
				this.SetValue( PositionFieldNameProperty, value );
			}
		}

		/// <summary>
		/// Returns true if the PositionFieldName property is set to a non-default value.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public bool ShouldSerializePositionFieldName( )
		{
			return Utilities.ShouldSerialize( PositionFieldNameProperty, this );
		}

		/// <summary>
		/// Resets the PositionFieldName property to its default state.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public void ResetPositionFieldName( )
		{
			this.ClearValue( PositionFieldNameProperty );
		}

		#endregion // PositionFieldName

		#region SourceFieldName

		/// <summary>
		/// Identifies the <see cref="SourceFieldName"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SourceFieldNameProperty = DependencyProperty.Register(
				"SourceFieldName",
				typeof( string ),
				typeof( SummaryDefinition ),
				new FrameworkPropertyMetadata( null )
			);

		/// <summary>
		/// Specifies the name of the field that will be summarized. This is the field whose cell 
		/// values will be used in the summary calculation.
		/// </summary>
		/// <seealso cref="Calculator"/>
		/// <seealso cref="SummaryResult"/>
		/// <seealso cref="SummaryResult.Value"/>
		//[Description( "Field whose values are summarized" )]
		//[Category( "Data" )]
		[Bindable( true )]
		public string SourceFieldName
		{
			get
			{
				return (string)this.GetValue( SourceFieldNameProperty );
			}
			set
			{
				this.SetValue( SourceFieldNameProperty, value );
			}
		}

		/// <summary>
		/// Returns true if the SourceFieldName property is set to a non-default value.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public bool ShouldSerializeSourceFieldName( )
		{
			return Utilities.ShouldSerialize( SourceFieldNameProperty, this );
		}

		/// <summary>
		/// Resets the SourceFieldName property to its default state.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public void ResetSourceFieldName( )
		{
			this.ClearValue( SourceFieldNameProperty );
		}

		#endregion // SourceFieldName

		#region ToolTip

		/// <summary>
		/// Identifies the <see cref="ToolTip"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ToolTipProperty = DependencyProperty.Register(
				"ToolTip",
				typeof( object ),
				typeof( SummaryDefinition ),
				new FrameworkPropertyMetadata( null )
			);

		/// <summary>
		/// Specifies the tooltip to display when the mouse is hovered over the associated summary result.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>ToolTip</b> specifies the tooltip to display when the mouse is hovered over the associated summary result. 
		/// Note that you can specify a string value as a value for this property.
		/// </para>
		/// </remarks>
		/// <seealso cref="SummaryResult.ToolTip"/>
		//[Description( "Tooltip to display over the associated summary results." )]
		//[Category( "Data" )]
		[Bindable( true )]
		public object ToolTip
		{
			get
			{
				return (object)this.GetValue( ToolTipProperty );
			}
			set
			{
				this.SetValue( ToolTipProperty, value );
			}
		}

		/// <summary>
		/// Returns true if the ToolTip property is set to a non-default value.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public bool ShouldSerializeToolTip( )
		{
			return Utilities.ShouldSerialize( ToolTipProperty, this );
		}

		/// <summary>
		/// Resets the ToolTip property to its default state.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public void ResetToolTip( )
		{
			this.ClearValue( ToolTipProperty );
		}

		#endregion // ToolTip

		#endregion // Public Properties

		#region Private/Internal Properties

		#region CalculationVersion

		/// <summary>
		/// Identifies the <see cref="CalculationVersion"/> dependency property
		/// </summary>
		internal static readonly DependencyProperty CalculationVersionProperty = DependencyProperty.Register(
				"CalculationVersion",
				typeof( int ),
				typeof( SummaryDefinition ),
				new FrameworkPropertyMetadata( 0, new PropertyChangedCallback( OnCalculationVersionChanged ) )
			);

		internal int CalculationVersion
		{
			get
			{
				return _cachedCalculationVersion;
			}
			set
			{
				this.SetValue( CalculationVersionProperty, value );
			}
		}

		private static void OnCalculationVersionChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			int newVal = (int)e.NewValue;
			SummaryDefinition def = (SummaryDefinition)dependencyObject;
			def._cachedCalculationVersion = newVal;
		}

		#endregion // CalculationVersion

		#region FieldLayout

		// SSP 8/22/11 - Calc functionality
		// 
		/// <summary>
		/// Gets the associated field-layout.
		/// </summary>
		public FieldLayout FieldLayout
		{
			get
			{
				return null != _parentCollection ? _parentCollection.FieldLayout : null;
			}
		} 

		#endregion // FieldLayout

		#region HasFormula

		// SSP 9/30/11 Calc
		// 
		/// <summary>
		/// Returns true if Formula is set on the calculation settings.
		/// </summary>
		internal bool HasFormula
		{
			get
			{

				return null != _calculationSettings && !string.IsNullOrEmpty( _calculationSettings.Formula );



			}
		} 

		#endregion // HasFormula

		#endregion // Private/Internal Properties

		#endregion // Properties

		#region Methods

		#region Public Methods

		#region Refresh

		/// <summary>
		/// Recalculates the summary.
		/// </summary>
		public void Refresh( )
		{
			this.BumpCalculationVersion( );
		}

		#endregion // Refresh

		#endregion // Public Methods

		#region Private/Internal Methods

		#region BumpCalculationVersion

		internal void BumpCalculationVersion( )
		{
			this.CalculationVersion++;
		}

		#endregion // BumpCalculationVersion

		#region InternalSetParentCollection

		internal void InternalSetParentCollection( SummaryDefinitionCollection parentCollection )
		{
			_parentCollection = parentCollection;
		}

		#endregion // InternalSetParentCollection

		#endregion // Private/Internal Methods

		#endregion // Methods
	}

	#endregion // SummaryDefinition Class

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