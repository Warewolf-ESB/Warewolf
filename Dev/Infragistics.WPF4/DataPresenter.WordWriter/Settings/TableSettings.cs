using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.ComponentModel;
using System.Windows.Media;
using Infragistics.Documents.Word;

namespace Infragistics.Windows.DataPresenter.WordWriter
{
	/// <summary>
	/// Base class that exposes settings for the Word table related settings used by the <see cref="DataPresenterWordWriter"/>
	/// </summary>
	[TypeConverter(typeof(ExpandableObjectConverter))]
	public class WordTableSettingsBase : DependencyObject
	{
		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="WordDefaultTableSettings"/>
		/// </summary>
		protected WordTableSettingsBase()
		{
		}
		#endregion //Constructor

		#region Properties

		#region Alignment

		/// <summary>
		/// Identifies the <see cref="Alignment"/> dependency property
		/// </summary>
		public static readonly DependencyProperty AlignmentProperty = DependencyProperty.Register("Alignment",
			typeof(ParagraphAlignment?), typeof(WordTableSettingsBase), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Returns or sets the horizontal alignment for the table.
		/// </summary>
		/// <seealso cref="AlignmentProperty"/>
		/// <seealso cref="Infragistics.Documents.Word.ParagraphPropertiesBase.Alignment"/>
		[Bindable(true)]
		public ParagraphAlignment? Alignment
		{
			get
			{
				return (ParagraphAlignment?)this.GetValue(WordTableSettingsBase.AlignmentProperty);
			}
			set
			{
				this.SetValue(WordTableSettingsBase.AlignmentProperty, value);
			}
		}

		#endregion //Alignment

		#region BorderSettings

		/// <summary>
		/// Identifies the <see cref="BorderSettings"/> dependency property
		/// </summary>
		public static readonly DependencyProperty BorderSettingsProperty = DependencyProperty.Register("BorderSettings",
			typeof(WordBorderSettings), typeof(WordTableSettingsBase), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Returns or sets settings for the border of the cell.
		/// </summary>
		/// <remarks>
		/// <p class="note"><b>Note:</b> Since the DataPresenter is comprised of many table cells for the export it is recommended that 
		/// border settings only be applied at the table level or in such a way that this affects a relatively few number of cells.</p>
		/// </remarks>
		/// <seealso cref="BorderSettingsProperty"/>
		/// <seealso cref="Infragistics.Documents.Word.TablePropertiesBase.BorderProperties"/>
		[Bindable(true)]
		public WordBorderSettings BorderSettings
		{
			get
			{
				return (WordBorderSettings)this.GetValue(WordTableSettingsBase.BorderSettingsProperty);
			}
			set
			{
				this.SetValue(WordTableSettingsBase.BorderSettingsProperty, value);
			}
		}

		#endregion //BorderSettings

		#region LeftIndent

		/// <summary>
		/// Identifies the <see cref="LeftIndent"/> dependency property
		/// </summary>
		public static readonly DependencyProperty LeftIndentProperty = DependencyProperty.Register("LeftIndent",
			typeof(DeviceUnitLength?), typeof(WordTableSettingsBase), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Returns or sets the amount of indentation from the left margin for the table.
		/// </summary>
		/// <remarks>
		/// <p class="note"><b>Note:</b> Word does not exclude this value from the size available for the table so that 
		/// table may be pushed outside the viewable area if using a Fixed layout.</p>
		/// </remarks>
		/// <seealso cref="LeftIndentProperty"/>
		/// <seealso cref="Infragistics.Documents.Word.ParagraphPropertiesBase.LeftIndent"/>
		[Bindable(true)]
		public DeviceUnitLength? LeftIndent
		{
			get
			{
				return (DeviceUnitLength?)this.GetValue(WordTableSettingsBase.LeftIndentProperty);
			}
			set
			{
				this.SetValue(WordTableSettingsBase.LeftIndentProperty, value);
			}
		}

		#endregion //LeftIndent

		#region NestedTableMargins

		/// <summary>
		/// Identifies the <see cref="NestedTableMargins"/> dependency property
		/// </summary>
		public static readonly DependencyProperty NestedTableMarginsProperty = DependencyProperty.Register("NestedTableMargins",
			typeof(DeviceUnitThickness?), typeof(WordTableSettingsBase), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Returns or sets the amount of space to put around nested child tables.
		/// </summary>
		/// <seealso cref="NestedTableMarginsProperty"/>
		[Bindable(true)]
		public DeviceUnitThickness? NestedTableMargins
		{
			get
			{
				return (DeviceUnitThickness?)this.GetValue(WordTableSettingsBase.NestedTableMarginsProperty);
			}
			set
			{
				this.SetValue(WordTableSettingsBase.NestedTableMarginsProperty, value);
			}
		}

		#endregion //NestedTableMargins

		#endregion //Properties

		#region Methods

		#region GetLeftIndent
		internal static float? GetLeftIndent(float? twipsValue, UnitOfMeasurement unit)
		{
			if (twipsValue == null)
				return null;

			float value = Math.Max(Math.Min(twipsValue.Value, TableProperties.IndentMaxValue), -TableProperties.IndentMaxValue);

			return WordDocumentWriter.ConvertUnits(value, UnitOfMeasurement.Twip, unit);
		} 
		#endregion //GetLeftIndent

		#region Initialize
		internal virtual void Initialize(WordDocumentWriter writer, TablePropertiesBase tableProperties, TableLayout defaultTableLayout)
		{
			UnitOfMeasurement unit = writer.Unit;
			var borderProps = this.BorderSettings;
			
			if (null != borderProps && borderProps.HasValues)
				borderProps.Initialize(writer, tableProperties.BorderProperties);

			tableProperties.Alignment = this.Alignment ?? tableProperties.Alignment;
            tableProperties.LeftIndent = WordExporter.ConvertUnits(this.LeftIndent, -TableProperties.IndentMaxValue, TableProperties.IndentMaxValue, unit) ?? tableProperties.LeftIndent;
		}
		#endregion //Initialize 

		#endregion //Methods
	}

	/// <summary>
	/// Exposes settings for the <see cref="WordDocumentWriter.DefaultTableProperties"/>
	/// </summary>
	/// <seealso cref="DefaultTableProperties"/>
	/// <seealso cref="DataPresenterWordWriter.DefaultTableSettings"/>
	[TypeConverter(typeof(ExpandableObjectConverter))]
	public class WordDefaultTableSettings : WordTableSettingsBase
	{
		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="WordDefaultTableSettings"/>
		/// </summary>
		public WordDefaultTableSettings()
		{
		}
		#endregion //Constructor

		#region Base class overrides

		#region Initialize
		internal override void Initialize(WordDocumentWriter writer, TablePropertiesBase tablePropertiesBase, TableLayout defaultTableLayout)
		{
			base.Initialize(writer, tablePropertiesBase, defaultTableLayout);

			var tableProperties = tablePropertiesBase as DefaultTableProperties;

			if (null != tableProperties)
			{
				var unit = writer.Unit;

				var cellSettings = this.CellSettings;

				if (null != cellSettings)
					cellSettings.Initialize(writer, tableProperties.CellProperties);
			}
		}
		#endregion //Initialize

		#endregion //Base class overrides

		#region Properties

		#region CellSettings

		/// <summary>
		/// Identifies the <see cref="CellSettings"/> dependency property
		/// </summary>
		public static readonly DependencyProperty CellSettingsProperty = DependencyProperty.Register("CellSettings",
			typeof(WordDefaultTableCellSettings), typeof(WordDefaultTableSettings), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Returns or sets default settings for the cells in the table.
		/// </summary>
		/// <seealso cref="CellSettingsProperty"/>
		/// <seealso cref="Infragistics.Documents.Word.DefaultTableProperties.CellProperties"/>
		[Bindable(true)]
		public WordDefaultTableCellSettings CellSettings
		{
			get
			{
				return (WordDefaultTableCellSettings)this.GetValue(WordDefaultTableSettings.CellSettingsProperty);
			}
			set
			{
				this.SetValue(WordDefaultTableSettings.CellSettingsProperty, value);
			}
		}

		#endregion //CellSettings

		#endregion //Properties
	}

	/// <summary>
	/// Exposes settings for the tables created during an export of a DataPresenterBase to Word.
	/// </summary>
	/// <seealso cref="DefaultTableProperties"/>
	public class WordTableSettings : WordTableSettingsBase
	{
		#region Member Variables

		private const float MaxPreferredWidthInTwips = 22f * 1440;

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="WordTableSettings"/>
		/// </summary>
		public WordTableSettings()
		{
		} 
		#endregion //Constructor

		#region Base class overrides

		#region Initialize
		internal override void Initialize(WordDocumentWriter writer, TablePropertiesBase tablePropertiesBase, TableLayout defaultTableLayout)
		{
			base.Initialize(writer, tablePropertiesBase, defaultTableLayout);

			var tableProperties = tablePropertiesBase as TableProperties;

			if (null != tableProperties)
			{
				var unit = writer.Unit;
				tableProperties.Layout = this.Layout ?? defaultTableLayout;
				tableProperties.PreferredWidth = WordExporter.ConvertUnits(this.PreferredWidth, 0f, MaxPreferredWidthInTwips, unit) ?? tableProperties.PreferredWidth;
				tableProperties.PreferredWidthAsPercentage = WordExporter.ConvertUnits(this.PreferredWidthAsPercentage, 0.2f, 100f, UnitOfMeasurement.Twip, UnitOfMeasurement.Twip) ?? tableProperties.PreferredWidthAsPercentage ?? 100f;
				tableProperties.CellMargins = WordExporter.ConvertUnits(this.CellMargins, unit) ?? tableProperties.CellMargins;
			}
		}
		#endregion //Initialize

		#endregion //Base class overrides

		#region Properties

		#region CellMargins

		/// <summary>
		/// Identifies the <see cref="CellMargins"/> dependency property
		/// </summary>
		public static readonly DependencyProperty CellMarginsProperty = DependencyProperty.Register("CellMargins",
			typeof(DeviceUnitThickness?), typeof(WordTableSettings), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Returns or sets the amount of padding between the cell content and its borders.
		/// </summary>
		/// <seealso cref="CellMarginsProperty"/>
		/// <seealso cref="Infragistics.Documents.Word.TableProperties.CellMargins"/>
		[Bindable(true)]
		public DeviceUnitThickness? CellMargins
		{
			get
			{
				return (DeviceUnitThickness?)this.GetValue(WordTableSettings.CellMarginsProperty);
			}
			set
			{
				this.SetValue(WordTableSettings.CellMarginsProperty, value);
			}
		}

		#endregion //CellMargins

		#region Layout

		/// <summary>
		/// Identifies the <see cref="Layout"/> dependency property
		/// </summary>
		public static readonly DependencyProperty LayoutProperty = DependencyProperty.Register("Layout",
			typeof(TableLayout?), typeof(WordTableSettings), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Returns or sets the layout algorithm used to determine the size and cells of the table.
		/// </summary>
		/// <seealso cref="LayoutProperty"/>
		/// <seealso cref="Infragistics.Documents.Word.TableProperties.Layout"/>
		[Bindable(true)]
		public TableLayout? Layout
		{
			get
			{
				return (TableLayout?)this.GetValue(WordTableSettings.LayoutProperty);
			}
			set
			{
				this.SetValue(WordTableSettings.LayoutProperty, value);
			}
		}

		#endregion //Layout

		#region PreferredWidth

		/// <summary>
		/// Identifies the <see cref="PreferredWidth"/> dependency property
		/// </summary>
		public static readonly DependencyProperty PreferredWidthProperty = DependencyProperty.Register("PreferredWidth",
			typeof(DeviceUnitLength?), typeof(WordTableSettings), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Returns or sets the preferred width of the table in twips.
		/// </summary>
		/// <seealso cref="PreferredWidthProperty"/>
		/// <seealso cref="Infragistics.Documents.Word.TableProperties.PreferredWidth"/>
		[Bindable(true)]
		public DeviceUnitLength? PreferredWidth
		{
			get
			{
				return (DeviceUnitLength?)this.GetValue(WordTableSettings.PreferredWidthProperty);
			}
			set
			{
				this.SetValue(WordTableSettings.PreferredWidthProperty, value);
			}
		}

		#endregion //PreferredWidth

		#region PreferredWidthAsPercentage

		/// <summary>
		/// Identifies the <see cref="PreferredWidthAsPercentage"/> dependency property
		/// </summary>
		public static readonly DependencyProperty PreferredWidthAsPercentageProperty = DependencyProperty.Register("PreferredWidthAsPercentage",
			typeof(float?), typeof(WordTableSettings), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Returns or sets the preferred width of the table expressed as a percent of the containing element.
		/// </summary>
		/// <seealso cref="PreferredWidthAsPercentageProperty"/>
		/// <seealso cref="Infragistics.Documents.Word.TableProperties.PreferredWidthAsPercentage"/>
		[Bindable(true)]
		public float? PreferredWidthAsPercentage
		{
			get
			{
				return (float?)this.GetValue(WordTableSettings.PreferredWidthAsPercentageProperty);
			}
			set
			{
				this.SetValue(WordTableSettings.PreferredWidthAsPercentageProperty, value);
			}
		}

		#endregion //PreferredWidthAsPercentage

		#endregion //Properties
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