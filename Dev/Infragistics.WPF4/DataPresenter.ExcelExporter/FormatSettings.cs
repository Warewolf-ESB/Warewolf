using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;
using Infragistics.Documents.Excel;

namespace Infragistics.Windows.DataPresenter.ExcelExporter
{
    /// <summary>
    /// A class that provides a set of default appearances and settings used when exporting a <see cref="Infragistics.Windows.DataPresenter.Field"/>.
    /// </summary>
    [InfragisticsFeature(Version = FeatureInfo.Version_9_2, FeatureName = FeatureInfo.FeatureName_ExcelExporter)]
    public class FormatSettings : DependencyObject, ICloneable
    {
        #region Methods

        #region OnPropertyChanged

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) 
        {
            
            // exportCache.ClearCachedFormatSettings();
        }
        #endregion //OnPropertyChanged

        #endregion //Methods

        #region Public Properties

        #region BorderColor

        /// <summary>
        /// Identifies the <see cref="BorderColor"/> dependency property
        /// </summary>
        public static readonly DependencyProperty BorderColorProperty =
            DependencyProperty.Register("BorderColor",
            typeof(Color?), typeof(FormatSettings), new PropertyMetadata(OnPropertyChanged));

        /// <summary>
        /// Gets or sets the color of the borders around the cells of a field.
        /// </summary>
        public Color? BorderColor
        {
            get { return (Color?)this.GetValue(FormatSettings.BorderColorProperty); }
            set { this.SetValue(FormatSettings.BorderColorProperty, value); }
        }
        #endregion //BorderColor

        #region BorderStyle

        /// <summary>
        /// Identifies the <see cref="BorderStyle"/> dependency property
        /// </summary>
        public static readonly DependencyProperty BorderStyleProperty =
            DependencyProperty.Register("BorderStyle",
            typeof(CellBorderLineStyle), typeof(FormatSettings),
            new PropertyMetadata(CellBorderLineStyle.Default, OnPropertyChanged));

        /// <summary>
        /// Gets or sets the style of all of the borders.  Individual border settings take precedence.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// If the border style is set to a non-default value and the <see cref="BottomBorderColor"/> is default (Color.Empty), 
        /// it will be resolved to Color.Black.
        /// </p>
        /// </remarks>
        /// <value>The bottom border style.</value>
        /// <seealso cref="BottomBorderColor"/>
        /// <seealso cref="LeftBorderColor"/>
        /// <seealso cref="LeftBorderStyle"/>
        /// <seealso cref="RightBorderColor"/>
        /// <seealso cref="RightBorderStyle"/>
        /// <seealso cref="TopBorderColor"/>
        /// <seealso cref="TopBorderStyle"/>
        public CellBorderLineStyle BorderStyle
        {
            get { return (CellBorderLineStyle)this.GetValue(FormatSettings.BorderStyleProperty); }
            set { this.SetValue(FormatSettings.BorderStyleProperty, value); }
        }
        #endregion //BottomBorderStyle

        #region BottomBorderColor

        /// <summary>
        /// Identifies the <see cref="BottomBorderColor"/> dependency property
        /// </summary>
        public static readonly DependencyProperty BottomBorderColorProperty =
            DependencyProperty.Register("BottomBorderColor",
            typeof(Color?), typeof(FormatSettings), new PropertyMetadata(OnPropertyChanged));

        /// <summary>
        /// Gets or sets the bottom border color.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// If the border color is set to a non-default value and the <see cref="BottomBorderStyle"/> is set to Default, 
        /// it will be resolved to Thin.
        /// </p>
        /// </remarks>
        /// <value>The bottom border color.</value>
        /// <seealso cref="BottomBorderStyle"/>
        /// <seealso cref="LeftBorderColor"/>
        /// <seealso cref="LeftBorderStyle"/>
        /// <seealso cref="RightBorderColor"/>
        /// <seealso cref="RightBorderStyle"/>
        /// <seealso cref="TopBorderColor"/>
        /// <seealso cref="TopBorderStyle"/>
        public Color? BottomBorderColor 
        {
            get { return (Color?)this.GetValue(FormatSettings.BottomBorderColorProperty); }
            set { this.SetValue(FormatSettings.BottomBorderColorProperty, value); }
        }

        #endregion //BottomBorderColor

        #region BottomBorderStyle

        /// <summary>
        /// Identifies the <see cref="BottomBorderStyle"/> dependency property
        /// </summary>
        public static readonly DependencyProperty BottomBorderStyleProperty =
            DependencyProperty.Register("BottomBorderStyle",
            typeof(CellBorderLineStyle), typeof(FormatSettings), 
            new PropertyMetadata(CellBorderLineStyle.Default, OnPropertyChanged));

        /// <summary>
        /// Gets or sets the bottom border style.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// If the border style is set to a non-default value and the <see cref="BottomBorderColor"/> is default (Color.Empty), 
        /// it will be resolved to Color.Black.
        /// </p>
        /// </remarks>
        /// <value>The bottom border style.</value>
        /// <seealso cref="BottomBorderColor"/>
        /// <seealso cref="LeftBorderColor"/>
        /// <seealso cref="LeftBorderStyle"/>
        /// <seealso cref="RightBorderColor"/>
        /// <seealso cref="RightBorderStyle"/>
        /// <seealso cref="TopBorderColor"/>
        /// <seealso cref="TopBorderStyle"/>
        public CellBorderLineStyle BottomBorderStyle 
        {
            get { return (CellBorderLineStyle)this.GetValue(FormatSettings.BottomBorderStyleProperty); }
            set { this.SetValue(FormatSettings.BottomBorderStyleProperty, value); }
        }
        #endregion //BottomBorderStyle

        #region FillPattern

        /// <summary>
        /// Identifies the <see cref="FillPattern"/> dependency property
        /// </summary>
        public static readonly DependencyProperty FillPatternProperty =
            DependencyProperty.Register("FillPattern",
            typeof(FillPatternStyle), typeof(FormatSettings),
            new PropertyMetadata(FillPatternStyle.Default, OnPropertyChanged));

        /// <summary>
        /// Gets or sets the fill pattern in the cell.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// If fill pattern is set to Solid, the cell is filled using just the <see cref="FillPatternForegroundColor"/>. For all other patterns, 
        /// both FillPatternForegroundColor and <see cref="FillPatternBackgroundColor"/> are used.
        /// </p>
        /// </remarks>
        /// <value>The fill pattern in the cell.</value>
        /// <seealso cref="FillPatternForegroundColor"/>
        /// <seealso cref="FillPatternBackgroundColor"/>
        public FillPatternStyle FillPattern 
        {
            get { return (FillPatternStyle)this.GetValue(FormatSettings.FillPatternProperty); }
            set { this.SetValue(FormatSettings.FillPatternProperty, value); }
        }
        #endregion //FillPattern

        #region FillPatternBackgroundColor

        /// <summary>
        /// Identifies the <see cref="FillPatternBackgroundColor"/> dependency property
        /// </summary>
        public static readonly DependencyProperty FillPatternBackgroundColorProperty =
            DependencyProperty.Register("FillPatternBackgroundColor",
            typeof(Color?), typeof(FormatSettings), new PropertyMetadata(OnPropertyChanged));

        /// <summary>
        /// Gets or sets the background color of a field.
        /// </summary>
        /// <remarks>
        /// If the background color is set to a non-default value and the <see cref="FillPattern"/>
        /// is set to <i>Default</i>, the resolved fill pattern style is Gray50percent.
        /// </remarks>
        public Color? FillPatternBackgroundColor
        {
            get { return (Color?)this.GetValue(FormatSettings.FillPatternBackgroundColorProperty); }
            set { this.SetValue(FormatSettings.FillPatternBackgroundColorProperty, value); }
        }
        #endregion //FillPatternBackgroundColor

        #region FillPatternForegroundColor

        /// <summary>
        /// Identifies the <see cref="FillPatternForegroundColor"/> dependency property
        /// </summary>
        public static readonly DependencyProperty FillPatternForegroundColorProperty =
            DependencyProperty.Register("FillPatternForegroundColor",
            typeof(Color?), typeof(FormatSettings), new PropertyMetadata(OnPropertyChanged));

        /// <summary>
        /// Gets or sets the foreground color of the cells in a field.
        /// </summary>
        /// <remarks>
        /// If the foreground color is set to a non-default value and the <see cref="FillPattern"/>
        /// is set to <i>Default</i>, the resolved fill pattern style is Solid. There, a solid cell
        /// color can easily be applied by setting only the FillPatternForegroundColor.
        /// </remarks>
        public Color? FillPatternForegroundColor
        {
            get { return (Color?)this.GetValue(FormatSettings.FillPatternForegroundColorProperty); }
            set { this.SetValue(FormatSettings.FillPatternForegroundColorProperty, value); }
        }
        #endregion //FillPatternForegroundColor

        #region FontColor

        /// <summary>
        /// Identifies the <see cref="FontColor"/> dependency property
        /// </summary>
        public static readonly DependencyProperty FontColorProperty =
            DependencyProperty.Register("FontColor",
            typeof(Color?), typeof(FormatSettings), new PropertyMetadata(OnPropertyChanged));

        /// <summary>
        /// Gets or sets the color of the font.
        /// </summary>
        public Color? FontColor
        {
            get { return (Color?)this.GetValue(FormatSettings.FontColorProperty); }
            set { this.SetValue(FormatSettings.FontColorProperty, value); }
        }
        #endregion //FontColor

        #region FontFamily

        /// <summary>
        /// Identifies the <see cref="FontFamily"/> dependency property
        /// </summary>
        public static readonly DependencyProperty FontFamilyProperty =
            DependencyProperty.Register("FontFamily",
            typeof(FontFamily), typeof(FormatSettings), new PropertyMetadata(OnPropertyChanged));

        /// <summary>
        /// Gets or sets the font family of the object being exported.
        /// </summary>
        public FontFamily FontFamily
        {
            get { return (FontFamily)this.GetValue(FormatSettings.FontFamilyProperty); }
            set { this.SetValue(FormatSettings.FontFamilyProperty, value); }
        }
        #endregion //FontFamily

        #region FontSize

        /// <summary>
        /// Identifies the <see cref="FontSize"/> dependency property
        /// </summary>
        public static readonly DependencyProperty FontSizeProperty =
            DependencyProperty.Register("FontSize",
            typeof(DeviceUnitLength?), typeof(FormatSettings), new PropertyMetadata(OnPropertyChanged));

        /// <summary>
        /// Gets or sets the size of the font for the object being exported, in device-independent units.
        /// </summary>
        public DeviceUnitLength? FontSize
        {
            get { return (DeviceUnitLength?)this.GetValue(FormatSettings.FontSizeProperty); }
            set { this.SetValue(FormatSettings.FontSizeProperty, value); }
        }
        #endregion //FontSize

        #region FontStrikeout

        /// <summary>
        /// Identifies the <see cref="FontStrikeout"/> dependency property
        /// </summary>
        public static readonly DependencyProperty FontStrikeoutProperty =
            DependencyProperty.Register("FontStrikeout",
            typeof(ExcelDefaultableBoolean), typeof(FormatSettings),
            new PropertyMetadata(ExcelDefaultableBoolean.Default, OnPropertyChanged));

        /// <summary>
        /// Gets or sets the value which indicates whether the font is struck out.
        /// </summary>
        public ExcelDefaultableBoolean FontStrikeout
        {
            get { return (ExcelDefaultableBoolean)this.GetValue(FormatSettings.FontStrikeoutProperty); }
            set { this.SetValue(FormatSettings.FontStrikeoutProperty, value); }
        }
        #endregion //Locked

        #region FontStyle

        /// <summary>
        /// Identifies the <see cref="FontStyle"/> dependency property
        /// </summary>
        public static readonly DependencyProperty FontStyleProperty =
            DependencyProperty.Register("FontStyle",
            typeof(FontStyle), typeof(FormatSettings), new PropertyMetadata(OnPropertyChanged));

        /// <summary>
        /// Gets or sets the style of the font for the object being exported.
        /// </summary>
        public FontStyle FontStyle
        {
            get { return (FontStyle)this.GetValue(FormatSettings.FontStyleProperty); }
            set { this.SetValue(FormatSettings.FontStyleProperty, value); }
        }
        #endregion //FontStyle

        #region FontSuperscriptSubscriptStyle

        /// <summary>
        /// Identifies the <see cref="FontSuperscriptSubscriptStyle"/> dependency property
        /// </summary>
        public static readonly DependencyProperty FontSuperscriptSubscriptStyleProperty =
            DependencyProperty.Register("FontSuperscriptSubscriptStyle",
            typeof(FontSuperscriptSubscriptStyle), typeof(FormatSettings), 
            new PropertyMetadata(FontSuperscriptSubscriptStyle.Default, OnPropertyChanged));

        /// <summary>
        /// Gets or sets the underline style of the font.
        /// </summary>
        public FontSuperscriptSubscriptStyle FontSuperscriptSubscriptStyle
        {
            get { return (FontSuperscriptSubscriptStyle)this.GetValue(FormatSettings.FontSuperscriptSubscriptStyleProperty); }
            set { this.SetValue(FormatSettings.FontSuperscriptSubscriptStyleProperty, value); }
        }
        #endregion //FontUnderlineStyle

        #region FontUnderlineStyle

        /// <summary>
        /// Identifies the <see cref="FontUnderlineStyle"/> dependency property
        /// </summary>
        public static readonly DependencyProperty FontUnderlineStyleProperty =
            DependencyProperty.Register("FontUnderlineStyle",
            typeof(FontUnderlineStyle), typeof(FormatSettings), 
            new PropertyMetadata(FontUnderlineStyle.Default, OnPropertyChanged));

        /// <summary>
        /// Gets or sets the underline style of the font.
        /// </summary>
        public FontUnderlineStyle FontUnderlineStyle
        {
            get { return (FontUnderlineStyle)this.GetValue(FormatSettings.FontUnderlineStyleProperty); }
            set { this.SetValue(FormatSettings.FontUnderlineStyleProperty, value); }
        }
        #endregion //FontUnderlineStyle

        #region FontWeight

        /// <summary>
        /// Identifies the <see cref="FontWeight"/> dependency property
        /// </summary>
        public static readonly DependencyProperty FontWeightProperty =
            DependencyProperty.Register("FontWeight",
            typeof(FontWeight?), typeof(FormatSettings), new PropertyMetadata(OnPropertyChanged));

        /// <summary>
        /// Gets or sets the weight of the font for the object being exported.
        /// </summary>
        /// <remarks>
        /// <p class="note">
        /// <b>Note: </b>Since Excel does not have full support for all of the various options of
        /// font weights, the values will be converted to the closest match that Excel supports.
        /// </p>
        /// </remarks>
        public FontWeight? FontWeight
        {
            get { return (FontWeight?)this.GetValue(FormatSettings.FontWeightProperty); }
            set { this.SetValue(FormatSettings.FontWeightProperty, value); }
        }
        #endregion //FontWeight

        #region FormatString

        /// <summary>
        /// Identifies the <see cref="FormatString"/> dependency property
        /// </summary>
        public static readonly DependencyProperty FormatStringProperty =
            DependencyProperty.Register("FormatString",
            typeof(string), typeof(FormatSettings), new PropertyMetadata(OnPropertyChanged));

        /// <summary>
        /// Gets or sets the Excel-style number format string.
        /// </summary>
        /// <remarks>
        /// <p class="body">For more information on excel format strings, consult Microsoft Excel help.</p>
        /// </remarks>        
        public string FormatString 
        {
            get { return (string)this.GetValue(FormatSettings.FormatStringProperty); }
            set { this.SetValue(FormatSettings.FormatStringProperty, value); }
        }
        #endregion //FormatString

        #region HorizontalAlignment

        /// <summary>
        /// Identifies the <see cref="HorizontalAlignment"/> dependency property
        /// </summary>
        public static readonly DependencyProperty HorizontalAlignmentProperty =
            DependencyProperty.Register("HorizontalAlignment",
            typeof(HorizontalCellAlignment), typeof(FormatSettings),
            new PropertyMetadata(HorizontalCellAlignment.Default, OnPropertyChanged));

        /// <summary>
        /// Gets or sets the horizontal alignment of the content in a cell.
        /// </summary>
        /// <value>The horizontal alignment of the content in a cell.</value>
        /// <seealso cref="VerticalAlignment"/>
        public HorizontalCellAlignment HorizontalAlignment 
        {
            get { return (HorizontalCellAlignment)this.GetValue(FormatSettings.HorizontalAlignmentProperty); }
            set { this.SetValue(FormatSettings.HorizontalAlignmentProperty, value); }
        }
        #endregion //HorizontalAlignment

        #region Indent

        /// <summary>
        /// Identifies the <see cref="Indent"/> dependency property
        /// </summary>
        public static readonly DependencyProperty IndentProperty =
            DependencyProperty.Register("Indent",
            typeof(int?), typeof(FormatSettings),
            new PropertyMetadata(OnPropertyChanged));

        /// <summary>
        /// Gets or sets the indent in units of average character widths.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The assigned value is outside the valid indent level range of 0 and 15.
        /// </exception>
        /// <value>The indent in units of average character widths.</value>
        public int? Indent 
        {
            get { return (int?)this.GetValue(FormatSettings.IndentProperty); }
            set { this.SetValue(FormatSettings.IndentProperty, value); }
        }
        #endregion //Indent

        #region LeftBorderColor

        /// <summary>
        /// Identifies the <see cref="LeftBorderColor"/> dependency property
        /// </summary>
        public static readonly DependencyProperty LeftBorderColorProperty =
            DependencyProperty.Register("LeftBorderColor",
            typeof(Color?), typeof(FormatSettings), new PropertyMetadata(OnPropertyChanged));

        /// <summary>
        /// Gets or sets the left border color.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// If the border color is set to a non-default value and the <see cref="LeftBorderStyle"/> is set to Default, 
        /// it will be resolved to Thin.
        /// </p>
        /// </remarks>
        /// <value>The left border color.</value>
        /// <seealso cref="BottomBorderColor"/>
        /// <seealso cref="BottomBorderStyle"/>
        /// <seealso cref="LeftBorderStyle"/>
        /// <seealso cref="RightBorderColor"/>
        /// <seealso cref="RightBorderStyle"/>
        /// <seealso cref="TopBorderColor"/>
        /// <seealso cref="TopBorderStyle"/>
        public Color? LeftBorderColor 
        {
            get { return (Color?)this.GetValue(FormatSettings.LeftBorderColorProperty); }
            set { this.SetValue(FormatSettings.LeftBorderColorProperty, value); }
        }
        #endregion //LeftBorderColor

        #region LeftBorderStyle

        /// <summary>
        /// Identifies the <see cref="LeftBorderStyle"/> dependency property
        /// </summary>
        public static readonly DependencyProperty LeftBorderStyleProperty =
            DependencyProperty.Register("LeftBorderStyle",
            typeof(CellBorderLineStyle), typeof(FormatSettings),
            new PropertyMetadata(CellBorderLineStyle.Default, OnPropertyChanged));

        /// <summary>
        /// Gets or sets the left border style.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// If the border style is set to a non-default value and the <see cref="LeftBorderColor"/> is default (Color.Empty), 
        /// it will be resolved to Color.Black.
        /// </p>
        /// </remarks>
        /// <value>The left border style.</value>
        /// <seealso cref="BottomBorderColor"/>
        /// <seealso cref="BottomBorderStyle"/>
        /// <seealso cref="LeftBorderColor"/>
        /// <seealso cref="RightBorderColor"/>
        /// <seealso cref="RightBorderStyle"/>
        /// <seealso cref="TopBorderColor"/>
        /// <seealso cref="TopBorderStyle"/>
        public CellBorderLineStyle LeftBorderStyle 
        {
            get { return (CellBorderLineStyle)this.GetValue(FormatSettings.LeftBorderStyleProperty); }
            set { this.SetValue(FormatSettings.LeftBorderStyleProperty, value); }
        }
        #endregion //LeftBorderStyle

        #region Locked

        /// <summary>
        /// Identifies the <see cref="Locked"/> dependency property
        /// </summary>
        public static readonly DependencyProperty LockedProperty =
            DependencyProperty.Register("Locked",
            typeof(ExcelDefaultableBoolean), typeof(FormatSettings),
            new PropertyMetadata(ExcelDefaultableBoolean.Default, OnPropertyChanged));

        /// <summary>
        /// Gets or sets the valid which indicates whether the cell is locked in protected mode.
        /// </summary>
        /// <remarks>
        /// <p class="body">The Locked valid is used in Excel file only if the associated <see cref="Worksheet"/> or <see cref="Workbook"/> 
        /// is protected. Otherwise the value is ignored.
        /// </p>
        /// </remarks>
        /// <value>The valid which indicates whether the cell is locked in protected mode.</value>
        /// <seealso cref="Workbook.Protected"/>
        public ExcelDefaultableBoolean Locked 
        {
            get { return (ExcelDefaultableBoolean)this.GetValue(FormatSettings.LockedProperty); }
            set { this.SetValue(FormatSettings.LockedProperty, value); }
        }
        #endregion //Locked

        #region RightBorderColor

        /// <summary>
        /// Identifies the <see cref="RightBorderColor"/> dependency property
        /// </summary>
        public static readonly DependencyProperty RightBorderColorProperty =
            DependencyProperty.Register("RightBorderColor",
            typeof(Color?), typeof(FormatSettings), new PropertyMetadata(OnPropertyChanged));

        /// <summary>
        /// Gets or sets the right border color.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// If the border color is set to a non-default value and the <see cref="RightBorderStyle"/> is set to Default, 
        /// it will be resolved to Thin.
        /// </p>
        /// </remarks>
        /// <value>The right border color.</value>
        /// <seealso cref="BottomBorderColor"/>
        /// <seealso cref="BottomBorderStyle"/>
        /// <seealso cref="LeftBorderColor"/>
        /// <seealso cref="LeftBorderStyle"/>
        /// <seealso cref="RightBorderStyle"/>
        /// <seealso cref="TopBorderColor"/>
        /// <seealso cref="TopBorderStyle"/>
        public Color? RightBorderColor 
        {
            get { return (Color?)this.GetValue(FormatSettings.RightBorderColorProperty); }
            set { this.SetValue(FormatSettings.RightBorderColorProperty, value); }
        }
        #endregion //RightBorderColor

        #region RightBorderStyle

        /// <summary>
        /// Identifies the <see cref="RightBorderStyle"/> dependency property
        /// </summary>
        public static readonly DependencyProperty RightBorderStyleProperty =
            DependencyProperty.Register("RightBorderStyle",
            typeof(CellBorderLineStyle), typeof(FormatSettings),
            new PropertyMetadata(CellBorderLineStyle.Default, OnPropertyChanged));

        /// <summary>
        /// Gets or sets the right border style.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// If the border style is set to a non-default value and the <see cref="RightBorderColor"/> is default (Color.Empty), 
        /// it will be resolved to Color.Black.
        /// </p>
        /// </remarks>
        /// <value>The right border style.</value>
        /// <seealso cref="BottomBorderColor"/>
        /// <seealso cref="BottomBorderStyle"/>
        /// <seealso cref="LeftBorderColor"/>
        /// <seealso cref="LeftBorderStyle"/>
        /// <seealso cref="RightBorderColor"/>
        /// <seealso cref="TopBorderColor"/>
        /// <seealso cref="TopBorderStyle"/>
        public CellBorderLineStyle RightBorderStyle 
        {
            get { return (CellBorderLineStyle)this.GetValue(FormatSettings.RightBorderStyleProperty); }
            set { this.SetValue(FormatSettings.RightBorderStyleProperty, value); }
        }
        #endregion //RightBorderStyle

        #region Rotation

        /// <summary>
        /// Identifies the <see cref="Rotation"/> dependency property
        /// </summary>
        public static readonly DependencyProperty RotationProperty =
            DependencyProperty.Register("Rotation",
            typeof(int?), typeof(FormatSettings),
            new PropertyMetadata(OnPropertyChanged));

        /// <summary>
        /// Gets or sets the rotation of the cell content in degrees.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// Cell text rotation, in degrees; 0 – 90 is up 0 – 90 degrees, 91 – 180 is down 1 – 90 degrees, and 255 is vertical.
        /// </p>
        /// </remarks>
        public int? Rotation
        {
            get { return (int?)this.GetValue(FormatSettings.RotationProperty); }
            set { this.SetValue(FormatSettings.RotationProperty, value); }
        }
        #endregion //Rotation

        #region ShrinkToFit

        /// <summary>
        /// Identifies the <see cref="ShrinkToFit"/> dependency property
        /// </summary>
        public static readonly DependencyProperty ShrinkToFitProperty =
            DependencyProperty.Register("ShrinkToFit",
            typeof(ExcelDefaultableBoolean), typeof(FormatSettings),
            new PropertyMetadata(ExcelDefaultableBoolean.Default, OnPropertyChanged));

        /// <summary>
        /// Gets or sets the value indicating whether the cell content will shrink to fit the cell.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// If True, the size of the cell font will shrink so all data fits within the cell.
        /// </p>
        /// </remarks>
        /// <value>The value indicating whether the cell content will shrink to fit the cell.</value>
        public ExcelDefaultableBoolean ShrinkToFit 
        {
            get { return (ExcelDefaultableBoolean)this.GetValue(FormatSettings.ShrinkToFitProperty); }
            set { this.SetValue(FormatSettings.ShrinkToFitProperty, value); }
        }
        #endregion //ShrinkToFit

        #region TopBorderColor

        /// <summary>
        /// Identifies the <see cref="TopBorderColor"/> dependency property
        /// </summary>
        public static readonly DependencyProperty TopBorderColorProperty =
            DependencyProperty.Register("TopBorderColor",
            typeof(Color?), typeof(FormatSettings), new PropertyMetadata(OnPropertyChanged));

        /// <summary>
        /// Gets or sets the top border color.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// If the border color is set to a non-default value and the <see cref="TopBorderStyle"/> is set to Default, 
        /// it will be resolved to Thin.
        /// </p>
        /// </remarks>
        /// <value>The top border color.</value>
        /// <seealso cref="BottomBorderColor"/>
        /// <seealso cref="BottomBorderStyle"/>
        /// <seealso cref="LeftBorderColor"/>
        /// <seealso cref="LeftBorderStyle"/>
        /// <seealso cref="RightBorderColor"/>
        /// <seealso cref="RightBorderStyle"/>
        /// <seealso cref="TopBorderStyle"/>
        public Color? TopBorderColor 
        {
            get { return (Color?)this.GetValue(FormatSettings.TopBorderColorProperty); }
            set { this.SetValue(FormatSettings.TopBorderColorProperty, value); }
        }
        #endregion //TopBorderColor

        #region TopBorderStyle

        /// <summary>
        /// Identifies the <see cref="TopBorderStyle"/> dependency property
        /// </summary>
        public static readonly DependencyProperty TopBorderStyleProperty =
            DependencyProperty.Register("TopBorderStyle",
            typeof(CellBorderLineStyle), typeof(FormatSettings),
            new PropertyMetadata(CellBorderLineStyle.Default, OnPropertyChanged));

        /// <summary>
        /// Gets or sets the top border style.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// If the border style is set to a non-default value and the <see cref="TopBorderColor"/> is default (Color.Empty), 
        /// it will be resolved to Color.Black.
        /// </p>
        /// </remarks>
        /// <value>The top border style.</value>
        /// <seealso cref="BottomBorderColor"/>
        /// <seealso cref="BottomBorderStyle"/>
        /// <seealso cref="LeftBorderColor"/>
        /// <seealso cref="LeftBorderStyle"/>
        /// <seealso cref="RightBorderColor"/>
        /// <seealso cref="RightBorderStyle"/>
        /// <seealso cref="TopBorderColor"/>
        public CellBorderLineStyle TopBorderStyle 
        {
            get { return (CellBorderLineStyle)this.GetValue(FormatSettings.TopBorderStyleProperty); }
            set { this.SetValue(FormatSettings.TopBorderStyleProperty, value); }
        }
        #endregion //TopBorderStyle

        #region VerticalAlignment

        /// <summary>
        /// Identifies the <see cref="VerticalAlignment"/> dependency property
        /// </summary>
        public static readonly DependencyProperty VerticalAlignmentProperty =
            DependencyProperty.Register("VerticalAlignment",
            typeof(VerticalCellAlignment), typeof(FormatSettings),
            new PropertyMetadata(VerticalCellAlignment.Default, OnPropertyChanged));

        /// <summary>
        /// Gets or sets the vertical alignment of the content in a cell.
        /// </summary>
        /// <value>The vertical alignment of the content in a cell.</value>
        /// <seealso cref="HorizontalAlignment"/>
        public VerticalCellAlignment VerticalAlignment 
        {
            get { return (VerticalCellAlignment)this.GetValue(FormatSettings.VerticalAlignmentProperty); }
            set { this.SetValue(FormatSettings.VerticalAlignmentProperty, value); }
        }
        #endregion //VerticalAlignment

        #region WrapText

        /// <summary>
        /// Identifies the <see cref="WrapText"/> dependency property
        /// </summary>
        public static readonly DependencyProperty WrapTextProperty =
            DependencyProperty.Register("WrapText",
            typeof(ExcelDefaultableBoolean), typeof(FormatSettings),
            new PropertyMetadata(ExcelDefaultableBoolean.Default, OnPropertyChanged));

        /// <summary>
        /// Gets or sets the value which indicates whether text will wrap in a cell.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// If True, and the row associated with the cell has a default <see cref="WorksheetRow.Height"/>, the row's
        /// height will automatically be increased to fit wrapped content.
        /// </p>
        /// </remarks>
        /// <value>The value which indicates whether text will wrap in a cell.</value>
        public ExcelDefaultableBoolean WrapText 
        {
            get { return (ExcelDefaultableBoolean)this.GetValue(FormatSettings.WrapTextProperty); }
            set { this.SetValue(FormatSettings.WrapTextProperty, value); }
        }
        #endregion //WrapText

        #endregion //Public Properties

        #region ICloneable Members

        /// <summary>
        /// Returns a copy of the object.
        /// </summary>
        /// <returns>A copy of the current object.</returns>
        public object Clone()
        {
            FormatSettings formatSettings = new FormatSettings();            
            formatSettings.BorderColor = this.BorderColor;
            formatSettings.BorderStyle = this.BorderStyle;
            formatSettings.BottomBorderColor = this.BorderColor;
            formatSettings.BottomBorderStyle = this.BottomBorderStyle;
            formatSettings.FillPattern = this.FillPattern;
            formatSettings.FillPatternBackgroundColor = this.FillPatternBackgroundColor;
            formatSettings.FillPatternForegroundColor = this.FillPatternForegroundColor;
            formatSettings.FontColor = this.FontColor;
            formatSettings.FontFamily = this.FontFamily;
            formatSettings.FontSize = this.FontSize;
            formatSettings.FontStrikeout = this.FontStrikeout;
            formatSettings.FontStyle = this.FontStyle;
            formatSettings.FontSuperscriptSubscriptStyle = this.FontSuperscriptSubscriptStyle;
            formatSettings.FontUnderlineStyle = this.FontUnderlineStyle;
            formatSettings.FontWeight = this.FontWeight;
            formatSettings.FormatString = this.FormatString;
            formatSettings.HorizontalAlignment = this.HorizontalAlignment;
            formatSettings.Indent = this.Indent;
            formatSettings.LeftBorderColor = this.LeftBorderColor;
            formatSettings.LeftBorderStyle = this.LeftBorderStyle;
            formatSettings.Locked = this.Locked;
            formatSettings.RightBorderColor = this.RightBorderColor;
            formatSettings.RightBorderStyle = this.RightBorderStyle;
            formatSettings.Rotation = this.Rotation;
            formatSettings.ShrinkToFit = this.ShrinkToFit;
            formatSettings.TopBorderColor = this.TopBorderColor;
            formatSettings.TopBorderStyle = this.TopBorderStyle;
            formatSettings.VerticalAlignment = this.VerticalAlignment;
            formatSettings.WrapText = this.WrapText;

            return formatSettings;
        }

        #endregion
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