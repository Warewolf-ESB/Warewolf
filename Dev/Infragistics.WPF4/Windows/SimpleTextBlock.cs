using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Automation.Peers;
using System.Globalization;
using System.ComponentModel;
using System.Windows.Documents;
using Infragistics.Windows.Automation.Peers;
using System.Diagnostics;

namespace Infragistics.Windows.Controls
{
	// MD 8/12/10 - TFS26592
	/// <summary>
	/// Class which is optimized to draw single line text in a container which has a fixed size, 
	/// such as a cell in a data grid.
	/// </summary>
	[System.Windows.Markup.ContentProperty("Text")]
	[DesignTimeVisible(false)]		// JM 10/1/10
	public class SimpleTextBlock : FrameworkElement
	{
		#region Member Variables

		private TextAlignment _cachedTextAlignment = TextAlignment.Left;
		private TextTrimming _cachedTextTrimming = TextTrimming.None;
		private TextWrapping _cachedTextWrapping = TextWrapping.NoWrap;

		private Size _lastMeasureConstraint;
		private FormattedText[] _formattedTexts;
		private Typeface _typeface;

		#endregion // Member Variables

		#region Constructor

		static SimpleTextBlock()
		{
			FrameworkElement.FlowDirectionProperty.OverrideMetadata(typeof(SimpleTextBlock), new FrameworkPropertyMetadata(SimpleTextBlock.OnPropertyChanged));
			FrameworkElement.HorizontalAlignmentProperty.OverrideMetadata(typeof(SimpleTextBlock), new FrameworkPropertyMetadata(SimpleTextBlock.OnHorizontalAlignmentPropertyChanged));
			FrameworkElement.LanguageProperty.OverrideMetadata(typeof(SimpleTextBlock), new FrameworkPropertyMetadata(SimpleTextBlock.OnLanguagePropertyChanged));

			Inline.TextDecorationsProperty.OverrideMetadata(typeof(SimpleTextBlock), new FrameworkPropertyMetadata(SimpleTextBlock.OnTextDecorationsPropertyChanged));

			NumberSubstitution.CultureOverrideProperty.OverrideMetadata(typeof(SimpleTextBlock), new FrameworkPropertyMetadata(SimpleTextBlock.OnNumberSubstitutionPropertyChanged));
			NumberSubstitution.CultureSourceProperty.OverrideMetadata(typeof(SimpleTextBlock), new FrameworkPropertyMetadata(SimpleTextBlock.OnNumberSubstitutionPropertyChanged));
			NumberSubstitution.SubstitutionProperty.OverrideMetadata(typeof(SimpleTextBlock), new FrameworkPropertyMetadata(SimpleTextBlock.OnNumberSubstitutionPropertyChanged));

			// MD 10/27/10 - TFS38066
			SimpleTextBlock.OptimizeWidthMeasurementProperty.OverrideMetadata(typeof(SimpleTextBlock), new FrameworkPropertyMetadata(SimpleTextBlock.OnOptimizeWidthMeasurementPropertyChanged));


			TextOptions.TextFormattingModeProperty.OverrideMetadata(typeof(SimpleTextBlock), new FrameworkPropertyMetadata(SimpleTextBlock.OnPropertyChanged)); 

		}

		/// <summary>
		/// Creates a new instance of the <see cref="SimpleTextBlock"/> class.
		/// </summary>
		public SimpleTextBlock() { }

		#endregion // Constructor

		#region Base Class Overrides

		#region MeasureOverride

		/// <summary>
		/// Invoked to measure the element and its children.
		/// </summary>
		/// <param name="constraint">The size that reflects the available size that this element can give to its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> that represents the desired size of the element.</returns>
		protected override Size MeasureOverride(Size constraint)
		{
			_lastMeasureConstraint = constraint;

			this.SetConstraintsForMeasure(constraint);

			FormattedText[] formattedTexts = this.FormattedTexts;

			if (formattedTexts.Length == 0)
			{
				FormattedText defaultText = this.CreatedFormattedText("Wj", Utilities.GetLanguageCultureInfo(this));

				// MD 10/27/10 - TFS38066
				// If we don't want to optimize the width measurement, return the correct width, which should be 0 here.
				//double width = Double.IsPositiveInfinity(constraint.Width) ? 0 : constraint.Width;
				double width = (Double.IsPositiveInfinity(constraint.Width) || this.OptimizeWidthMeasurement)
					? 0
					: constraint.Width;

				return new Size(width, Math.Min(constraint.Height, defaultText.Height));
			}

			bool isWrapping = this.TextWrapping != TextWrapping.NoWrap;

			double? defaultTextHeight = null;

			double maxWidth = 0;
			double totalHeight = 0;
			for (int i = 0; i < formattedTexts.Length; i++)
			{
				FormattedText formattedText = formattedTexts[i];

				// For some reason, when the TextAlignment is Center or Right, the measurements don't reflect the actual size 
				// width of the text. For example, when it is Center, the Width is the distance from the middle of the text to
				// the right edge. So we will always measure with a Left TextAlignment and then we will reset it when we are 
				// done measuring.
				TextAlignment oldAlignment = formattedText.TextAlignment;

				try
				{
					if (oldAlignment != TextAlignment.Left)
						formattedText.TextAlignment = TextAlignment.Left;

					// Get the Extent first so the cached metrics in the FormattedText will be reused for drawing.
					// If we ask for the Width or Height first, the cached metrics will be discarded and recalculated
					// when drawing.
					double extent = formattedText.Extent;

					// If we are not wrapping and the constraint width is valid, return the constraint width so the element
					// extends the full width available and then we will handle alignment in the OnRender.
					// MD 10/27/10 - TFS38066
					// If we don't want to optimize the width measurement, return the correct width here.
					//double width = isWrapping == false && Double.IsInfinity(constraint.Width) == false
					double width = isWrapping == false && Double.IsInfinity(constraint.Width) == false && this.OptimizeWidthMeasurement
						? constraint.Width
						: Math.Min(constraint.Width, formattedText.WidthIncludingTrailingWhitespace);

					double height = formattedText.Height;
					if (height == 0 && formattedText.Text.Length == 0)
					{
						if (defaultTextHeight.HasValue == false)
						{
							FormattedText defaultText = this.CreatedFormattedText("Wj", Utilities.GetLanguageCultureInfo(this));
							defaultTextHeight = defaultText.Height;
						}

						height = defaultTextHeight.Value;
					}

					maxWidth = Math.Max(maxWidth, width);
					totalHeight += height;
				}
				finally
				{
					// Set the TextAlignment back to what it was.
					if (oldAlignment != TextAlignment.Left)
						formattedText.TextAlignment = oldAlignment;
				}
			}

			// MD 5/11/11 - TFS75183
			// If UseLayoutRounding is enabled, the last character may get truncated, so make sure we round up for the 
			// width and height.

			if (this.UseLayoutRounding)
			{
				maxWidth = Math.Ceiling(maxWidth);
				totalHeight = Math.Ceiling(totalHeight);
			}


			return new Size(maxWidth, totalHeight);
		}

		#endregion // MeasureOverride

		#region OnCreateAutomationPeer

		/// <summary>
		/// Returns an automation peer that exposes the <see cref="SimpleTextBlock"/> to UI Automation.
		/// </summary>
		/// <returns>A <see cref="Infragistics.Windows.Automation.Peers.SimpleTextBlockAutomationPeer"/>.</returns>
		protected override AutomationPeer OnCreateAutomationPeer()
		{
			return new SimpleTextBlockAutomationPeer(this);
		}

		#endregion // OnCreateAutomationPeer

		#region OnRender

		/// <summary>
		/// Renders the contents of the <see cref="SimpleTextBlock"/> to the drawing context.
		/// </summary>
		/// <param name="drawingContext">The drawing context to which the text should be drawn.</param>
		protected override void OnRender(DrawingContext drawingContext)
		{
			this.SetConstraintsForRender();

			FormattedText[] formattedTexts = this.FormattedTexts;
			Size renderSize = this.RenderSize;
			bool isRightToLeft = this.FlowDirection == FlowDirection.RightToLeft;

			// Determine the width of the widest line of text.
			double maxTextWidth = 0;

			// MD 8/3/11 - TFS80786
			// Also determine the minimum OverhangTrailing value.
			double minOverhangTrailing = 0;

			for (int i = 0; i < formattedTexts.Length; i++)
			{
				// MD 8/3/11 - TFS80786
				// Store the current FormattedText because we need to get multiple values from it now.
				//maxTextWidth = Math.Max(maxTextWidth, formattedTexts[i].WidthIncludingTrailingWhitespace);
				FormattedText formattedText = formattedTexts[i];
				maxTextWidth = Math.Max(maxTextWidth, formattedText.WidthIncludingTrailingWhitespace);
				minOverhangTrailing = Math.Min(minOverhangTrailing, formattedText.OverhangTrailing);
			}

			// Determine the width of the area the element would have spanned if we weren't spanning the for 
			// available width. Also, determine the horizontal position of where that render area start.
			double renderAreaPosition;
			double renderAreaWidth;
			switch (this.HorizontalAlignment)
			{
				case HorizontalAlignment.Left:
					renderAreaWidth = maxTextWidth;
					renderAreaPosition = 0;
					break;

				case HorizontalAlignment.Center:
					renderAreaWidth = maxTextWidth;
					renderAreaPosition = (renderSize.Width - maxTextWidth) / 2;
					break;

				case HorizontalAlignment.Right:
					renderAreaWidth = maxTextWidth;
					renderAreaPosition = (renderSize.Width - maxTextWidth);
					break;

				default:
				case HorizontalAlignment.Stretch:
					renderAreaWidth = renderSize.Width;
					renderAreaPosition = 0;

					// If the text is wider than the available area, the text will always align with the left edge of the element, 
					// so adjust the rendering origin accordingly.
					if (maxTextWidth > renderAreaWidth)
					{
						switch (this.TextAlignment)
						{
							case TextAlignment.Center:
								renderAreaPosition += (maxTextWidth - renderAreaWidth) / 2;
								break;

							case TextAlignment.Right:
								renderAreaPosition += (maxTextWidth - renderAreaWidth);
								break;
						}
					}

					break;
			}

			// If we are flowing text RightToLeft, we need to re-mirror the text because the text is mirrored 
			// by default.
			if (isRightToLeft)
				drawingContext.PushTransform(new MatrixTransform(-1, 0, 0, 1, renderSize.Width, 0));

			// MD 8/3/11 - TFS80786
			// If the OverhangTrailing of one or more FormattedText instances is negative, the characters will be drawn slightly outside 
			// the measured text width, so increase the clip width slightly so we don't cut off those characters.
			//drawingContext.PushClip(new RectangleGeometry(new Rect(renderSize)));
			Size clipSize = renderSize;
			if (minOverhangTrailing < 0)
				clipSize.Width -= minOverhangTrailing;
			drawingContext.PushClip(new RectangleGeometry(new Rect(clipSize)));

			double? defaultTextHeight = null;

			Point origin = new Point();
			for (int i = 0; i < formattedTexts.Length; i++)
			{
				FormattedText formattedText = formattedTexts[i];

				// When no wrapping is used, we need to handle alignment manually because the element will span the full 
				// width available.
				if (this.TextWrapping == TextWrapping.NoWrap)
				{
					switch (this.TextAlignment)
					{
						case TextAlignment.Left:
						case TextAlignment.Justify:
							origin.X = renderAreaPosition;
							break;

						case TextAlignment.Center:
							origin.X = renderAreaPosition + (renderAreaWidth - formattedText.WidthIncludingTrailingWhitespace) / 2;
							break;

						case TextAlignment.Right:
							origin.X = renderAreaPosition + (renderAreaWidth - formattedText.WidthIncludingTrailingWhitespace);
							break;
					}

					// When flowing RightToLeft and no trimming is used, swap the origin to be from the other side of the element.
					// For some reason, the text drawing works normal again when using trimming.
					if (isRightToLeft && this.TextTrimming == TextTrimming.None)
						origin.X = renderSize.Width - origin.X;
				}

				// Draw this line of text.
				drawingContext.DrawText(formattedText, origin);

				double height = formattedText.Height;

				// If this is an empty line, the height should be that of the default line.
				if (height == 0 && formattedText.Text.Length == 0)
				{
					if (defaultTextHeight.HasValue == false)
					{
						FormattedText defaultText = this.CreatedFormattedText("Wj", Utilities.GetLanguageCultureInfo(this));
						defaultTextHeight = defaultText.Height;
					}

					height = defaultTextHeight.Value;
				}

				// Bump the origin Y by this line's height so the next line draws under it.
				origin.Y += height;
			}

			drawingContext.Pop();

			// If we pushed on a transform, pop it off.
			if (isRightToLeft)
				drawingContext.Pop();
		}

		#endregion // OnRender

		#endregion // Base Class Overrides

		#region Properties

		#region Private Properties

		#region FormattedText

		private FormattedText[] FormattedTexts
		{
			get
			{
				if (_formattedTexts == null)
				{
					string text = this.Text ?? string.Empty;

					CultureInfo cultureInfo = Utilities.GetLanguageCultureInfo(this);

					// We have to draw each line individually when wrapping is turned off and we are trimming, 
					// because each line needs to trim itself.
					if (this.TextWrapping == TextWrapping.NoWrap &&
						(this.TextTrimming != TextTrimming.None || this.TextAlignment != TextAlignment.Left))
					{
						List<TextRangeDefinition> lines = SimpleTextBlock.GetTextLines(text);

						_formattedTexts = new FormattedText[lines.Count];

						for (int i = 0; i < lines.Count; i++)
						{
							TextRangeDefinition line = lines[i];
							FormattedText formattedText = this.CreatedFormattedText(text.Substring(line.StartIndex, line.Length), cultureInfo);
							formattedText.MaxLineCount = 1;
							_formattedTexts[i] = formattedText;
						}
					}
					else
					{
						// When the text for the FormattedText object has a trailing newline, the measurements are off by one line, 
						// so add an extra new line ot the end of it.
						if (text.EndsWith("\n") || text.EndsWith("\r"))
							text += "\n";

						_formattedTexts = new FormattedText[] { this.CreatedFormattedText(text, cultureInfo) };
					}
				}

				return _formattedTexts;
			}
		}

		#endregion // FormattedText

		#region Typeface

		private Typeface Typeface
		{
			get
			{
				if (_typeface == null)
					_typeface = new Typeface(this.FontFamily, this.FontStyle, this.FontWeight, this.FontStretch);

				return _typeface;
			}
		}

		#endregion // Typeface

		#endregion // Private Properties

		#region Public Properties

		#region FontFamily

		/// <summary>
		/// Identifies the <see cref="FontFamily"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty FontFamilyProperty = TextElement.FontFamilyProperty.AddOwner(
			typeof(SimpleTextBlock),
			new FrameworkPropertyMetadata(SimpleTextBlock.OnTypefacePropertyChanged));

		/// <summary>
		/// Gets or sets the font family used to draw the text.
		/// </summary>
		[Bindable(true)]
		public FontFamily FontFamily
		{
			get { return (FontFamily)this.GetValue(SimpleTextBlock.FontFamilyProperty); }
			set { this.SetValue(SimpleTextBlock.FontFamilyProperty, value); }
		}

		#endregion // FontFamily

		#region FontSize

		/// <summary>
		/// Identifies the <see cref="FontSize"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty FontSizeProperty = TextElement.FontSizeProperty.AddOwner(
			typeof(SimpleTextBlock),
			new FrameworkPropertyMetadata(SimpleTextBlock.OnFontSizePropertyChanged));

		/// <summary>
		/// Gets or sets the font size used to draw the text.
		/// </summary>
		[TypeConverter(typeof(FontSizeConverter))]
		[Bindable(true)]
		public double FontSize
		{
			get { return (double)this.GetValue(SimpleTextBlock.FontSizeProperty); }
			set { this.SetValue(SimpleTextBlock.FontSizeProperty, value); }
		}

		#endregion // FontSize

		#region FontStretch

		/// <summary>
		/// Identifies the <see cref="FontStretch"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty FontStretchProperty = TextElement.FontStretchProperty.AddOwner(
			typeof(SimpleTextBlock),
			new FrameworkPropertyMetadata(SimpleTextBlock.OnTypefacePropertyChanged));

		/// <summary>
		/// Gets or sets the degree to which the font is condensed or expanded when drawn.
		/// </summary>
		[Bindable(true)]
		public FontStretch FontStretch
		{
			get { return (FontStretch)this.GetValue(SimpleTextBlock.FontStretchProperty); }
			set { this.SetValue(SimpleTextBlock.FontStretchProperty, value); }
		}

		#endregion // FontStretch

		#region FontStyle

		/// <summary>
		/// Identifies the <see cref="FontStyle"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty FontStyleProperty = TextElement.FontStyleProperty.AddOwner(
			typeof(SimpleTextBlock),
			new FrameworkPropertyMetadata(SimpleTextBlock.OnTypefacePropertyChanged));

		/// <summary>
		/// Gets or sets the font style of the text.
		/// </summary>
		[Bindable(true)]
		public FontStyle FontStyle
		{
			get { return (FontStyle)this.GetValue(SimpleTextBlock.FontStyleProperty); }
			set { this.SetValue(SimpleTextBlock.FontStyleProperty, value); }
		}

		#endregion // FontStyle

		#region FontWeight

		/// <summary>
		/// Identifies the <see cref="FontWeight"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty FontWeightProperty = TextElement.FontWeightProperty.AddOwner(
			typeof(SimpleTextBlock),
			new FrameworkPropertyMetadata(SimpleTextBlock.OnTypefacePropertyChanged));

		/// <summary>
		/// Gets or sets the weight, or thickness, of the text.
		/// </summary>
		[Bindable(true)]
		public FontWeight FontWeight
		{
			get { return (FontWeight)this.GetValue(SimpleTextBlock.FontWeightProperty); }
			set { this.SetValue(SimpleTextBlock.FontWeightProperty, value); }
		}

		#endregion // FontWeight

		#region Foreground

		/// <summary>
		/// Identifies the <see cref="Foreground"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty ForegroundProperty = TextElement.ForegroundProperty.AddOwner(
			typeof(SimpleTextBlock),
			new FrameworkPropertyMetadata(SimpleTextBlock.OnForegroundPropertyChanged));

		/// <summary>
		/// Gets or sets the brush used to draw the text.
		/// </summary>
		[Bindable(true)]
		public Brush Foreground
		{
			get { return (Brush)this.GetValue(SimpleTextBlock.ForegroundProperty); }
			set { this.SetValue(SimpleTextBlock.ForegroundProperty, value); }
		}

		#endregion // Foreground

		// MD 10/27/10 - TFS38066
		#region OptimizeWidthMeasurement

		/// <summary>
		/// Identifies the <see cref="OptimizeWidthMeasurement"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty OptimizeWidthMeasurementProperty = DependencyProperty.RegisterAttached(
			"OptimizeWidthMeasurement",
			typeof(bool),
			typeof(SimpleTextBlock),
			new FrameworkPropertyMetadata(
				true, 
				FrameworkPropertyMetadataOptions.Inherits));

		/// <summary>
		/// Gets or sets the value which indicates whether to optimize the width measurement.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// To increase performance, the SimpleTextBlock doesn't measure its content's width. It instead uses the available width.
		/// However, this may cause issues with alignment. If it does, this should be set to False.
		/// </p>
		/// </remarks>
		[Bindable(true)]
		public bool OptimizeWidthMeasurement
		{
			get { return (bool)this.GetValue(SimpleTextBlock.OptimizeWidthMeasurementProperty); }
			set { this.SetValue(SimpleTextBlock.OptimizeWidthMeasurementProperty, value); }
		}

		/// <summary>
		/// Gets the <see cref="OptimizeWidthMeasurement"/> property.  This dependency property indicates whether to optimize the width 
		/// measurement on SimpleTextBlocks within the specified DependencyObject.
		/// </summary>
		/// <seealso cref="OptimizeWidthMeasurement"/>
		public static bool GetOptimizeWidthMeasurement(DependencyObject d)
		{
			return (bool)d.GetValue(SimpleTextBlock.OptimizeWidthMeasurementProperty);
		}

		/// <summary>
		/// Sets the <see cref="OptimizeWidthMeasurement"/> property.  This dependency property indicates whether to optimize the width 
		/// measurement on SimpleTextBlocks within the specified DependencyObject.
		/// </summary>
		/// <seealso cref="OptimizeWidthMeasurement"/>
		public static void SetOptimizeWidthMeasurement(DependencyObject d, bool value)
		{
			d.SetValue(SimpleTextBlock.OptimizeWidthMeasurementProperty, value);
		}

		#endregion // OptimizeWidthMeasurement

		#region Text

		/// <summary>
		/// Identifies the <see cref="Text"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
			"Text",
			typeof(string),
			typeof(SimpleTextBlock),
			new FrameworkPropertyMetadata(
				null,
				FrameworkPropertyMetadataOptions.AffectsRender,
				SimpleTextBlock.OnPropertyChanged));

		/// <summary>
		/// Gets or sets the text drawn by the element.
		/// </summary>
		[Bindable(true)]
		public string Text
		{
			get { return (string)this.GetValue(SimpleTextBlock.TextProperty); }
			set { this.SetValue(SimpleTextBlock.TextProperty, value); }
		}

		#endregion // Text

		#region TextAlignment

		/// <summary>
		/// Identifies the <see cref="TextAlignment"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty TextAlignmentProperty = Block.TextAlignmentProperty.AddOwner(
			typeof(SimpleTextBlock),
			new FrameworkPropertyMetadata(
				TextAlignment.Left,
				FrameworkPropertyMetadataOptions.AffectsRender,
				SimpleTextBlock.OnTextAlignmentPropertyChanged));

		/// <summary>
		/// Gets or sets the horizontal alignment of text.
		/// </summary>
		[Bindable(true)]
		public TextAlignment TextAlignment
		{
			get { return _cachedTextAlignment; }
			set { this.SetValue(SimpleTextBlock.TextAlignmentProperty, value); }
		}

		#endregion // TextAlignment

		#region TextDecorations

		/// <summary>
		/// Identifies the <see cref="TextDecorations"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty TextDecorationsProperty = Inline.TextDecorationsProperty.AddOwner(typeof(SimpleTextBlock));

		/// <summary>
		/// Gets or sets the collection of effects to apply to the text.
		/// </summary>
		[Bindable(true)]
		public TextDecorationCollection TextDecorations
		{
			get { return (TextDecorationCollection)this.GetValue(SimpleTextBlock.TextDecorationsProperty); }
			set { this.SetValue(SimpleTextBlock.TextDecorationsProperty, value); }
		}

		#endregion // TextDecorations

		#region TextTrimming

		/// <summary>
		/// Identifies the <see cref="TextTrimming"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty TextTrimmingProperty = DependencyProperty.Register(
			"TextTrimming",
			typeof(TextTrimming),
			typeof(SimpleTextBlock),
			new FrameworkPropertyMetadata(
				TextTrimming.None,
				FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure,
				SimpleTextBlock.OnTextTrimmingPropertyChanged));

		/// <summary>
		/// Gets or sets the text trimming behavior to use when text overflows the render size of the element.
		/// </summary>
		[Bindable(true)]
		public TextTrimming TextTrimming
		{
			get { return _cachedTextTrimming; }
			set { this.SetValue(SimpleTextBlock.TextTrimmingProperty, value); }
		}

		#endregion // TextWrapping

		#region TextWrapping

		/// <summary>
		/// Identifies the <see cref="TextWrapping"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty TextWrappingProperty = DependencyProperty.Register(
			"TextWrapping",
			typeof(TextWrapping),
			typeof(SimpleTextBlock),
			new FrameworkPropertyMetadata(
				TextWrapping.NoWrap,
				FrameworkPropertyMetadataOptions.AffectsRender,
				SimpleTextBlock.OnTextWrappingPropertyChanged));

		/// <summary>
		/// Gets or sets the text wrapping behavior to use when text overflows the render width of the element.
		/// </summary>
		[Bindable(true)]
		public TextWrapping TextWrapping
		{
			get { return _cachedTextWrapping; }
			set { this.SetValue(SimpleTextBlock.TextWrappingProperty, value); }
		}

		#endregion // TextWrapping

		#endregion // Public Properties

		#endregion // Properties

		#region Methods

		#region CreatedFormattedText

		private FormattedText CreatedFormattedText(string text, CultureInfo cultureInfo)
		{
			FormattedText formattedText = new FormattedText(
				text,
				cultureInfo,
				this.FlowDirection,
				this.Typeface,
				this.FontSize,
				this.Foreground,
				this.GetNumberSubstitution()

				, TextOptions.GetTextFormattingMode(this) 

);

			formattedText.Trimming = this.TextTrimming;
			formattedText.TextAlignment = SimpleTextBlock.GetResolvedTextAlignment(this.TextWrapping, this.TextAlignment);
			formattedText.SetTextDecorations(this.TextDecorations);

			return formattedText;
		}

		#endregion // CreatedFormattedText

		#region GetNumberSubstitution

		private NumberSubstitution GetNumberSubstitution()
		{
			return new NumberSubstitution(
			   NumberSubstitution.GetCultureSource(this),
			   NumberSubstitution.GetCultureOverride(this),
			   NumberSubstitution.GetSubstitution(this));
		}

		#endregion // GetNumberSubstitution

		#region GetTextLines

		private static List<TextRangeDefinition> GetTextLines(string text)
		{
			List<TextRangeDefinition> lines = new List<TextRangeDefinition>();

			char[] newLineChars = new char[] { '\r', '\n' };

			int lineStartIndex = 0;
			while (true)
			{
				int newLineCharIndex = text.IndexOfAny(newLineChars, lineStartIndex);

				if (newLineCharIndex < 0)
					break;

				// The TextBlock considers "...\r...", "...\n...", and "...\r\n..." to cause line breaks.
				// So if we encounter '\r' or '\n', create a line break and if the '\r' has a '\n' directly
				// after it, skip passed that second character.
				TextRangeDefinition lineRange = new TextRangeDefinition();
				lineRange.StartIndex = lineStartIndex;
				lineRange.Length = newLineCharIndex - lineStartIndex;
				lines.Add(lineRange);

				// Determine the start index of the next line.
				lineStartIndex = newLineCharIndex + 1;

				// Skip passed the '\n' if it comes right after a '\r'
				if (lineStartIndex < text.Length &&
					text[newLineCharIndex] == '\r' && text[lineStartIndex] == '\n')
				{
					lineStartIndex++;
				}
			}

			if (lineStartIndex <= text.Length)
			{
				TextRangeDefinition lineRange = new TextRangeDefinition();
				lineRange.StartIndex = lineStartIndex;
				lineRange.Length = text.Length - lineStartIndex;
				lines.Add(lineRange);
			}

			return lines;
		}

		#endregion // GetTextLines

		#region GetResolvedTextAlignment

		private static TextAlignment GetResolvedTextAlignment(TextWrapping textWrapping, TextAlignment textAlignment)
		{
			// Because we will be handling alignement when not wrapping, we should resolve Right and Center 
			// alignments to Left.
			if (textWrapping == TextWrapping.NoWrap)
			{
				return textAlignment == TextAlignment.Justify
					? TextAlignment.Justify
					: TextAlignment.Left;
			}

			return textAlignment;
		}

		#endregion // GetResolvedTextAlignment

		#region OnFontSizePropertyChanged

		private static void OnFontSizePropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
		{
			SimpleTextBlock textBlock = (SimpleTextBlock)dependencyObject;

			if (textBlock._formattedTexts == null)
				return;

			for (int i = 0; i < textBlock._formattedTexts.Length; i++)
				textBlock._formattedTexts[i].SetFontSize((double)e.NewValue);
		}

		#endregion // OnFontSizePropertyChanged

		#region OnForegroundPropertyChanged

		private static void OnForegroundPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
		{
			SimpleTextBlock textBlock = (SimpleTextBlock)dependencyObject;

			if (textBlock._formattedTexts == null)
				return;

			for (int i = 0; i < textBlock._formattedTexts.Length; i++)
				textBlock._formattedTexts[i].SetForegroundBrush((Brush)e.NewValue);
		}

		#endregion // OnForegroundPropertyChanged

		#region OnHorizontalAlignmentPropertyChanged

		private static void OnHorizontalAlignmentPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
		{
			SimpleTextBlock textBlock = (SimpleTextBlock)dependencyObject;

			// If no wrapping is being used, we are handling text and horizontal alignment in the OnRender override, 
			// so dirty the visual.
			if (textBlock.TextWrapping == TextWrapping.NoWrap)
				textBlock.InvalidateVisual();
		}

		#endregion // OnHorizontalAlignmentPropertyChanged

		#region OnLanguagePropertyChanged

		private static void OnLanguagePropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
		{
			SimpleTextBlock textBlock = (SimpleTextBlock)dependencyObject;

			if (textBlock._formattedTexts == null)
				return;

			for (int i = 0; i < textBlock._formattedTexts.Length; i++)
				textBlock._formattedTexts[i].SetCulture(Utilities.GetLanguageCultureInfo(textBlock));
		}

		#endregion // OnLanguagePropertyChanged

		#region OnNumberSubstitutionPropertyChanged

		private static void OnNumberSubstitutionPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
		{
			SimpleTextBlock textBlock = (SimpleTextBlock)dependencyObject;

			if (textBlock._formattedTexts == null)
				return;

			for (int i = 0; i < textBlock._formattedTexts.Length; i++)
				textBlock._formattedTexts[i].SetNumberSubstitution(textBlock.GetNumberSubstitution());
		}

		#endregion // OnNumberSubstitutionPropertyChanged

		// MD 10/27/10 - TFS38066
		#region OnOptimizeWidthMeasurementPropertyChanged

		private static void OnOptimizeWidthMeasurementPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
		{
			SimpleTextBlock block = dependencyObject as SimpleTextBlock;

			if (block != null)
				block.InvalidateMeasure();
		} 

		#endregion // OnOptimizeWidthMeasurementPropertyChanged

		#region OnPropertyChanged

		private static void OnPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
		{
			SimpleTextBlock textBlock = (SimpleTextBlock)dependencyObject;

			bool isMeasureInvalidated;
			textBlock.OnTextPropertyDirtied(out isMeasureInvalidated);

			if (isMeasureInvalidated)
				return;

			switch (e.Property.Name)
			{
				case "Text":

				case "TextFormattingMode": 

					// If the constraints passed in are infinite, the text block is being used for measuring, so dirty the 
					// measure when the text changes.
					if (Double.IsInfinity(textBlock._lastMeasureConstraint.Width) ||
						Double.IsInfinity(textBlock._lastMeasureConstraint.Height)
						// SSP 11/9/10 TFS36313
						// We need to re-measure when the text changes. Note that TFS36313 isn't related to
						// this however it's something I noticed while fixing TFS36313.
						// 
						|| ! textBlock.OptimizeWidthMeasurement 
						)
					{
						textBlock.InvalidateMeasure();
					}
					break;

				case "TextWrapping":
					textBlock.InvalidateMeasure();
					break;
			}
		}

		#endregion // OnPropertyChanged

		#region OnTextAlignmentPropertyChanged

		private static void OnTextAlignmentPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
		{
			SimpleTextBlock textBlock = (SimpleTextBlock)dependencyObject;
			textBlock._cachedTextAlignment = (TextAlignment)e.NewValue;

			if (textBlock._formattedTexts == null)
				return;

			if (textBlock.TextWrapping == TextWrapping.NoWrap)
			{
				textBlock._formattedTexts = null;
			}
			else
			{
				for (int i = 0; i < textBlock._formattedTexts.Length; i++)
					textBlock._formattedTexts[i].TextAlignment = SimpleTextBlock.GetResolvedTextAlignment(textBlock.TextWrapping, textBlock.TextAlignment);
			}
		}

		#endregion // OnTextAlignmentPropertyChanged

		#region OnTextDecorationsPropertyChanged

		private static void OnTextDecorationsPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
		{
			SimpleTextBlock textBlock = (SimpleTextBlock)dependencyObject;

			if (textBlock._formattedTexts == null)
				return;

			for (int i = 0; i < textBlock._formattedTexts.Length; i++)
				textBlock._formattedTexts[i].SetTextDecorations((TextDecorationCollection)e.NewValue);

			// Even though the TextDecorations property has AffectsRender as one of its options, we don't 
			// get back into OnRender when this changes for some reason, so force an invalidation.
			textBlock.InvalidateVisual();
		}

		#endregion // OnTextDecorationsPropertyChanged

		#region OnTextTrimmingPropertyChanged

		private static void OnTextTrimmingPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
		{
			SimpleTextBlock textBlock = (SimpleTextBlock)dependencyObject;
			textBlock._cachedTextTrimming = (TextTrimming)e.NewValue;

			if (textBlock._formattedTexts == null)
				return;

			if (textBlock.TextWrapping == TextWrapping.NoWrap)
			{
				textBlock._formattedTexts = null;
			}
			else
			{
				for (int i = 0; i < textBlock._formattedTexts.Length; i++)
					textBlock._formattedTexts[i].Trimming = textBlock.TextTrimming;
			}
		}

		#endregion // OnTextTrimmingPropertyChanged

		#region OnTextPropertyDirtied

		private void OnTextPropertyDirtied(out bool isMeasureInvalidated)
		{
			isMeasureInvalidated = false;

			this._formattedTexts = null;

			// If we are using wrapping, we always have to re-measure
			if (this.TextWrapping != TextWrapping.NoWrap)
			{
				this.InvalidateMeasure();
				isMeasureInvalidated = true;
			}
		}

		#endregion // OnTextPropertyDirtied

		#region OnTextWrappingPropertyChanged

		private static void OnTextWrappingPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
		{
			((SimpleTextBlock)dependencyObject)._cachedTextWrapping = (TextWrapping)e.NewValue;

			SimpleTextBlock.OnPropertyChanged(dependencyObject, e);
		}

		#endregion // OnTextWrappingPropertyChanged

		#region OnTypefacePropertyChanged

		private static void OnTypefacePropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
		{
			SimpleTextBlock textBlock = (SimpleTextBlock)dependencyObject;

			textBlock._typeface = null;

			bool isMeasureInvalidated;
			textBlock.OnTextPropertyDirtied(out isMeasureInvalidated);
		}

		#endregion // OnTypefacePropertyChanged

		#region SetConstraintsForMeasure

		private void SetConstraintsForMeasure(Size measureConstraint)
		{
			FormattedText[] formattedTexts = this.FormattedTexts;
			Size renderSize = this.RenderSize;
			bool isWrapping = this.TextWrapping != TextWrapping.NoWrap;

			// When wrapping is used, we need to set the MaxTextWidth so the height is calculated correctly.
			if (isWrapping)
			{
				Debug.Assert(formattedTexts.Length == 1, "There should only be one FormattedText when wrapping.");

				double newMaxTextWidth = Double.IsInfinity(measureConstraint.Width)
					? 0
					: measureConstraint.Width;

				// Set the MaxTextWidth constraint so the text is wrapped properly
				if (formattedTexts[0].MaxTextWidth != newMaxTextWidth)
					formattedTexts[0].MaxTextWidth = newMaxTextWidth;
			}

			for (int i = 0; i < formattedTexts.Length; i++)
			{
				FormattedText formattedText = formattedTexts[i];

				// Reset the MaxTextHeight on each line of FormattedText.
				if (formattedText.MaxTextHeight != Double.MaxValue)
					formattedText.MaxTextHeight = Double.MaxValue;
			}
		}

		#endregion // SetConstraintsForMeasure

		#region SetConstraintsForRender

		private void SetConstraintsForRender()
		{
			FormattedText[] formattedTexts = this.FormattedTexts;
			Size renderSize = this.RenderSize;
			bool isWrapping = this.TextWrapping != TextWrapping.NoWrap;

			// When wrapping is used, we need to set the MaxTextWidth so the text will actually wrap.
			if (isWrapping)
			{
				Debug.Assert(formattedTexts.Length == 1, "There should only be one FormattedText when wrapping.");

				// If we are using wrapping, set the width contraint so the text is wrapped properly.
				if (formattedTexts[0].MaxTextWidth != renderSize.Width)
					formattedTexts[0].MaxTextWidth = renderSize.Width;
			}

			if (this.TextTrimming == TextTrimming.None)
			{
				// If we are using no trimming or wrapping, make sure the constraint is reset so the text draws 
				// right to the edge of the available width.
				if (isWrapping == false)
				{
					for (int i = 0; i < formattedTexts.Length; i++)
					{
						FormattedText formattedText = formattedTexts[i];

						if (formattedText.MaxTextWidth != 0)
							formattedText.MaxTextWidth = 0;
					}
				}
			}
			else
			{
				// When trimming is used, we need to set the MaxTextWidth so the text area will be constrained. 
				for (int i = 0; i < formattedTexts.Length; i++)
				{
					FormattedText formattedText = formattedTexts[i];

					if (formattedText.MaxTextWidth != renderSize.Width)
						formattedText.MaxTextWidth = renderSize.Width;
				}

				// We also need to set the MaxTextHeight when wrapping is used so the text will be trimmed instead 
				// of wrapped when it reaches the bottom.
				if (isWrapping)
				{
					Debug.Assert(formattedTexts.Length == 1, "There should only be one FormattedText when wrapping.");

					if (formattedTexts[0].MaxTextHeight != renderSize.Height)
						formattedTexts[0].MaxTextHeight = renderSize.Height;
				}
			}
		}

		#endregion // SetConstraintsForRender

		#endregion // Methods


		#region TextRangeDefinition struct

		private struct TextRangeDefinition
		{
			public int StartIndex;
			public int Length;
		}

		#endregion // TextRangeDefinition struct
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