using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.ComponentModel;
using System.Windows.Media;
using Infragistics.Documents.Word;
using WordFont = Infragistics.Documents.Word.Font;

namespace Infragistics.Windows.DataPresenter.WordWriter
{
	/// <summary>
	/// Base class for an object that exposes format settings for table cells when exporting to Word
	/// </summary>
	[TypeConverter(typeof(ExpandableObjectConverter))]
	public class WordTableCellSettingsBase : DependencyObject
	{
		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="WordTableCellSettingsBase"/>
		/// </summary>
		protected WordTableCellSettingsBase()
		{
		}
		#endregion //Constructor

		#region Properties

		#region Public Properties

		#region BackColor

		/// <summary>
		/// Identifies the <see cref="BackColor"/> dependency property
		/// </summary>
		public static readonly DependencyProperty BackColorProperty = DependencyProperty.Register("BackColor",
			typeof(Color), typeof(WordTableCellSettingsBase), new FrameworkPropertyMetadata(WordExporter.EmptyColor));

		/// <summary>
		/// Returns or sets the background color for the object
		/// </summary>
		/// <seealso cref="BackColorProperty"/>
		/// <seealso cref="Infragistics.Documents.Word.TableCellPropertiesBase.BackColor"/>
		[Bindable(true)]
		public Color BackColor
		{
			get
			{
				return (Color)this.GetValue(WordTableCellSettingsBase.BackColorProperty);
			}
			set
			{
				this.SetValue(WordTableCellSettingsBase.BackColorProperty, value);
			}
		}

		#endregion //BackColor

		#region Margins

		/// <summary>
		/// Identifies the <see cref="Margins"/> dependency property
		/// </summary>
		public static readonly DependencyProperty MarginsProperty = DependencyProperty.Register("Margins",
			typeof(DeviceUnitThickness?), typeof(WordTableCellSettingsBase), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Returns or sets the amount of padding between the cell content and its borders.
		/// </summary>
		/// <seealso cref="MarginsProperty"/>
		/// <seealso cref="Infragistics.Documents.Word.TableCellPropertiesBase.Margins"/>
		[Bindable(true)]
		public DeviceUnitThickness? Margins
		{
			get
			{
				return (DeviceUnitThickness?)this.GetValue(WordTableCellSettingsBase.MarginsProperty);
			}
			set
			{
				this.SetValue(WordTableCellSettingsBase.MarginsProperty, value);
			}
		}

		#endregion //Margins

		#region VerticalAlignment

		/// <summary>
		/// Identifies the <see cref="VerticalAlignment"/> dependency property
		/// </summary>
		public static readonly DependencyProperty VerticalAlignmentProperty = DependencyProperty.Register("VerticalAlignment",
			typeof(TableCellVerticalAlignment), typeof(WordTableCellSettingsBase), new FrameworkPropertyMetadata(TableCellVerticalAlignment.Default));

		/// <summary>
		/// Returns or sets a value indicating how the cell content is arranged vertically.
		/// </summary>
		/// <seealso cref="VerticalAlignmentProperty"/>
		/// <seealso cref="Infragistics.Documents.Word.TableCellPropertiesBase.VerticalAlignment"/>
		[Bindable(true)]
		public TableCellVerticalAlignment VerticalAlignment
		{
			get
			{
				return (TableCellVerticalAlignment)this.GetValue(WordTableCellSettingsBase.VerticalAlignmentProperty);
			}
			set
			{
				this.SetValue(WordTableCellSettingsBase.VerticalAlignmentProperty, value);
			}
		}

		#endregion //VerticalAlignment

		#endregion //Public Properties

		#endregion //Properties

		#region Methods

		#region Initialize
		internal virtual void Initialize(WordDocumentWriter writer, TableCellPropertiesBase cellProperties)
		{
			UnitOfMeasurement unit = writer.Unit;

			cellProperties.BackColor = WordExporter.ResolveProperty(this.BackColor, cellProperties.BackColor);
			cellProperties.Margins = WordExporter.ConvertUnits(this.Margins, unit) ?? cellProperties.Margins;
			cellProperties.VerticalAlignment = WordExporter.ResolveProperty(this.VerticalAlignment, cellProperties.VerticalAlignment, TableCellVerticalAlignment.Default);
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
	public class WordDefaultTableCellSettings : WordTableCellSettingsBase
	{
		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="WordDefaultTableSettings"/>
		/// </summary>
		public WordDefaultTableCellSettings()
		{
		}
		#endregion //Constructor
	}

	/// <summary>
	/// Exposes format settings for table cells when exporting to Word
	/// </summary>
	public class WordTableCellSettings : WordTableCellSettingsBase
	{
		#region Member Variables

		private Color? _foreColor;
		private WordFontSettings _fontSettings;
		private WordBorderSettings _borderSettings;

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="WordTableCellSettings"/>
		/// </summary>
		public WordTableCellSettings()
		{
		}
		#endregion //Constructor

		#region Properties

		#region Public Properties

		#region BorderSettings

		/// <summary>
		/// Identifies the <see cref="BorderSettings"/> dependency property
		/// </summary>
		public static readonly DependencyProperty BorderSettingsProperty = DependencyProperty.Register("BorderSettings",
			typeof(WordBorderSettings), typeof(WordTableCellSettings), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnBorderSettingsChanged)));

		private static void OnBorderSettingsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var settings = d as WordTableCellSettings;
			settings._borderSettings = e.NewValue as WordBorderSettings;
		}

		/// <summary>
		/// Returns or sets settings for the border of the cell.
		/// </summary>
		/// <remarks>
		/// <p class="note"><b>Note:</b> Since the DataPresenter is comprised of many table cells for the export it is recommended that 
		/// border settings only be applied at the table level or in such a way that this affects a relatively few number of cells.</p>
		/// </remarks>
		/// <seealso cref="BorderSettingsProperty"/>
		/// <seealso cref="WordTableSettingsBase.BorderSettings"/>
		/// <seealso cref="Infragistics.Documents.Word.TableCellProperties.BorderProperties"/>
		[Bindable(true)]
		public WordBorderSettings BorderSettings
		{
			get
			{
				return _borderSettings;
			}
			set
			{
				this.SetValue(WordTableCellSettings.BorderSettingsProperty, value);
			}
		}

		#endregion //BorderSettings

		#region FontSettings

		/// <summary>
		/// Identifies the <see cref="FontSettings"/> dependency property
		/// </summary>
		public static readonly DependencyProperty FontSettingsProperty = DependencyProperty.Register("FontSettings",
			typeof(WordFontSettings), typeof(WordTableCellSettings), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnFontSettingsChanged)));

		private static void OnFontSettingsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var settings = d as WordTableCellSettings;
			settings._fontSettings = e.NewValue as WordFontSettings;
		}

		/// <summary>
		/// Returns or sets settings for the Font of the cell contents.
		/// </summary>
		/// <seealso cref="FontSettingsProperty"/>
		/// <seealso cref="Infragistics.Documents.Word.Font"/>
		[Bindable(true)]
		public WordFontSettings FontSettings
		{
			get
			{
				return _fontSettings;
			}
			set
			{
				this.SetValue(WordTableCellSettings.FontSettingsProperty, value);
			}
		}

		#endregion //FontSettings

		#region HorizontalAlignment

		/// <summary>
		/// Identifies the <see cref="HorizontalAlignment"/> dependency property
		/// </summary>
		public static readonly DependencyProperty HorizontalAlignmentProperty = DependencyProperty.Register("HorizontalAlignment",
			typeof(ParagraphAlignment?), typeof(WordTableCellSettings), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Returns or sets a value indicating how the cell content is arranged vertically.
		/// </summary>
		/// <remarks>
		/// <p class="note"><b>Note:</b> This property affects the alignment of the parapgraph and is provided as an easy way 
		/// of initializing the horizontal alignment of the cell's content without setting the <see cref="ParagraphSettings"/> 
		/// property. If the value of the <see cref="Infragistics.Windows.DataPresenter.WordWriter.WordParagraphSettings.Alignment"/> 
		/// is set on any of the <see cref="ParagraphSettings"/> when the properties are being resolved then that value will be 
		/// taken first even if the HorizontalAlignment was set on the WordCellSettings of a Field's Settings and the Alignment 
		/// was set on the ParagraphSettings of a WordCellSettings set on the FieldLayout's FieldSettings for example.</p>
		/// </remarks>
		/// <seealso cref="HorizontalAlignmentProperty"/>
		/// <seealso cref="Infragistics.Documents.Word.ParagraphPropertiesBase.Alignment"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.WordWriter.WordParagraphSettings.Alignment"/>
		[Bindable(true)]
		public ParagraphAlignment? HorizontalAlignment
		{
			get
			{
				return (ParagraphAlignment?)this.GetValue(WordTableCellSettings.HorizontalAlignmentProperty);
			}
			set
			{
				this.SetValue(WordTableCellSettings.HorizontalAlignmentProperty, value);
			}
		}

		#endregion //HorizontalAlignment

		#region ParagraphSettings

		/// <summary>
		/// Identifies the <see cref="ParagraphSettings"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ParagraphSettingsProperty = DependencyProperty.Register("ParagraphSettings",
			typeof(WordParagraphSettings), typeof(WordTableCellSettings), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Returns or sets the paragraph settings for the cell contents.
		/// </summary>
		/// <seealso cref="ParagraphSettingsProperty"/>
		[Bindable(true)]
		public WordParagraphSettings ParagraphSettings
		{
			get
			{
				return (WordParagraphSettings)this.GetValue(WordTableCellSettings.ParagraphSettingsProperty);
			}
			set
			{
				this.SetValue(WordTableCellSettings.ParagraphSettingsProperty, value);
			}
		}

		#endregion //ParagraphSettings

		#region TextDirection

		/// <summary>
		/// Identifies the <see cref="TextDirection"/> dependency property
		/// </summary>
		public static readonly DependencyProperty TextDirectionProperty = DependencyProperty.Register("TextDirection",
			typeof(TableCellTextDirection), typeof(WordTableCellSettings), new FrameworkPropertyMetadata(TableCellTextDirection.Normal));

		/// <summary>
		/// Returns or sets the direction of the text flow for the text within the table cell.
		/// </summary>
		/// <seealso cref="TextDirectionProperty"/>
		/// <seealso cref="Infragistics.Documents.Word.TableCellProperties.TextDirection"/>
		[Bindable(true)]
		public TableCellTextDirection TextDirection
		{
			get
			{
				return (TableCellTextDirection)this.GetValue(WordTableCellSettings.TextDirectionProperty);
			}
			set
			{
				this.SetValue(WordTableCellSettings.TextDirectionProperty, value);
			}
		}

		#endregion //TextDirection

		#endregion //Public Properties

		#region Internal Properties

		#region ForeColor
		/// <summary>
		/// Returns/sets the default forecolor.
		/// </summary>
		internal Color? ForeColor
		{
			get { return _foreColor; }
			set { _foreColor = value; }
		}
		#endregion //ForeColor

		#endregion //Internal Properties

		#endregion //Properties

		#region Methods

		#region HasFontSettings
		internal bool HasFontSettings()
		{
			if (this.ForeColor == null)
			{
				if (this.FontSettings == null)
					return false;
			}

			return true;
		}
		#endregion //HasFontSettings

		#region HasParagraphSettings
		internal bool HasParagraphSettings()
		{
			if (this.HorizontalAlignment == null)
			{
				if (this.ParagraphSettings == null)
					return false;
			}

			return true;
		}
		#endregion //HasParagraphSettings

		#region Initialize
		internal override void Initialize(WordDocumentWriter writer, TableCellPropertiesBase cellPropertiesBase)
		{
			base.Initialize(writer, cellPropertiesBase);

			var cellProperties = cellPropertiesBase as TableCellProperties;

			if (null != cellProperties)
			{
				UnitOfMeasurement unit = writer.Unit;
				var borderSettings = this.BorderSettings;

				if (null != borderSettings && borderSettings.HasValues)
					borderSettings.Initialize(writer, cellProperties.BorderProperties);

				cellProperties.TextDirection = WordExporter.ResolveProperty(this.TextDirection, cellProperties.TextDirection, TableCellTextDirection.Normal);
			}
		}

		internal void Initialize(WordDocumentWriter writer, ParagraphProperties paragraphProperties)
		{
			if (paragraphProperties != null)
			{
				var paragraphSettings = this.ParagraphSettings;

				if (null != paragraphSettings)
					paragraphSettings.Initialize(writer, paragraphProperties);

				if (paragraphProperties.Alignment == null)
					paragraphProperties.Alignment = this.HorizontalAlignment;
			}
		}

		internal void Initialize(WordDocumentWriter writer, WordFont font)
		{
			if (font != null)
			{
				if (null != _fontSettings)
					_fontSettings.Initialize(writer, font);

				if (_foreColor != null && font.ForeColor == WordExporter.EmptyColor)
					font.ForeColor = _foreColor.Value;
			}
		}

		internal void Initialize(WordDocumentWriter writer, TableBorderProperties borderProperties)
		{
			var borderSettings = this.BorderSettings;

			if (null != borderSettings && borderSettings.HasValues)
				borderSettings.Initialize(writer, borderProperties);
		} 

		#endregion //Initialize 

		#region ShouldSerializerBorderSettings
		internal bool ShouldSerializerBorderSettings()
		{
			var borderSettings = this.BorderSettings;
			return borderSettings != null && borderSettings.HasValues;
		} 
		#endregion //ShouldSerializerBorderSettings

		#endregion //Methods	
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