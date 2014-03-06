using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Documents;
using System.Diagnostics;
using System.Collections;
using Infragistics.Windows.Helpers;
using System.Windows.Data;

namespace Infragistics.Windows.Ribbon
{
	/// <summary>
	/// Arranges and displays up to 2 lines of centered text plus a glyph - used for large <see cref="XamRibbon"/> tools.
	/// </summary>
	[TemplatePart(Name = "PART_Line1", Type = typeof(TextBlock))]
	[TemplatePart(Name = "PART_Line2", Type = typeof(TextBlock))]
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class LargeToolCaptionPresenter : Control
	{
		#region Member Variables

		private TextBlock									_textBlockPrivate = new TextBlock();
		private TextBlock									_line1;
		private TextBlock									_line2;

		private List<WordInfo>								_wordInfos;
		private bool										_cachedWordInfoDirty = true;
		private bool										_cachedGlyphInfoDirty = true;
		private InlineUIContainer							_glyphInlineUIContainer = null;
		private Size										_glyphSize = Size.Empty;

		#endregion //Member Variables

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="LargeToolCaptionPresenter"/> class.
		/// </summary>
		public LargeToolCaptionPresenter()
		{
		}

		static LargeToolCaptionPresenter()
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(LargeToolCaptionPresenter), new FrameworkPropertyMetadata(typeof(LargeToolCaptionPresenter)));
			FrameworkElement.FocusableProperty.OverrideMetadata(typeof(LargeToolCaptionPresenter), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));
		}

		#endregion //Constructor

		#region Base Class Overrides

			#region MeasureOverride

		/// <summary>
		/// Measures the LargeToolCaptionPresenter by determining the narrrowest width that can contain the Text and Glyph on 2 lines
		/// and sets the content of each line.
		/// </summary>
		/// <param name="availableSize">The size available to the LargeToolCaptionPresenter.</param>
		/// <returns>The size desired by the LargeToolCaptionPresenter.</returns>
		protected override Size MeasureOverride(Size availableSize)
		{
			// Don't bother proceeding if we don't have the required TextBlock elements in our template.
			Debug.Assert(this._line1 != null, "The Line1 element was not found in the LargeToolCaptionPresenter template!");
			Debug.Assert(this._line2 != null, "The Line2 element was not found in the LargeToolCaptionPresenter template!");
			if (this._line1 == null | this._line2 == null)
				return new Size(1, 1);
				

			// If our cache of WordInfos is dirty, rebuld the cache.
			if (this._cachedWordInfoDirty)
			{
				// Split the string into words based on embedded spaces.
				string		fullText	= this.Text is string ? (string)this.Text : string.Empty;
				string[]	tempWords	= fullText.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
				
				this._wordInfos			= new List<WordInfo>(tempWords.Length);
				for (int i = 0; i < tempWords.Length; i++)
				{
					string currentWord = tempWords[i];

					this._wordInfos.Add(new WordInfo(currentWord, this.MeasureText(currentWord)));
				}

				this._cachedWordInfoDirty = false;
			}


			// If our cache of glyph information is dirty, rebuild the cache.
			if (this._cachedGlyphInfoDirty)
			{
				Size glyphSize = Size.Empty;

				if (this._glyphInlineUIContainer != null)
				{
					this._glyphInlineUIContainer.Child = null;
					this._glyphInlineUIContainer = null;
				}

				if (this.Glyph != null)
				{
					this.Glyph.Measure(availableSize);
					this._glyphSize = this.Glyph.DesiredSize;

					if (this._glyphInlineUIContainer == null)
					{
						this._glyphInlineUIContainer = new InlineUIContainer(this.Glyph);

						// AS 7/8/08 BR33724
						// The container's enabled state is only being initialized once and
						// when it changes thereafter, its enabled state isn't changing so
						// the glyph it contains doesn't update either. We'll keep the 
						// container and element's enabled states in sync.
						//
						this._glyphInlineUIContainer.SetBinding(InlineUIContainer.IsEnabledProperty, Utilities.CreateBindingObject(UIElement.IsEnabledProperty, BindingMode.OneWay, this));
					}
				}

				this._cachedGlyphInfoDirty = false;
			}


			// Calculate the width of a space character.
			double spaceWidth = this.MeasureText(" ").Width;


			// Reset the contents of the textblicks in our template.
			this._line1.Inlines.Clear();
			this._line2.Inlines.Clear();
			this._line1.Text = this._line2.Text = "";

			// Determine how many lines we need and set the text and glyph for each line.
			//
			// If we have no words, the just place the glyph on line 1.
			if (this._wordInfos.Count == 0)
			{
				if (this._glyphSize != Size.Empty)
				{
					// AS 10/2/07
					// See 1 word section for info.
					//
					this._line1.Inlines.Add(new Run("\uFEFF"));
					this._line1.Inlines.Add(this._glyphInlineUIContainer);
				}
			}
			else
			// If we have 1 word, place it on line 1 and the glyph on line 2.
			if (this._wordInfos.Count == 1)
			{
				WordInfo	wordInfo = this._wordInfos[0];
				Size		wordSize = wordInfo.Size;

				this._line1.Text	= wordInfo.Word;
				if (this._glyphSize != Size.Empty)
				{
					// AS 10/2/07
					// Added a zero width non-breaking space in front of the glyph so the line
					// has some text that it can use to calculate the baseline. Without this, the 
					// second line with just the glyph will not be lined up the same as one with 
					// two lines of text.
					//
					this._line2.Inlines.Add(new Run("\uFEFF"));
					this._line2.Inlines.Add(this._glyphInlineUIContainer);
				}
			}
			else
			// Since we have more than 1 word, place half on the first line and half on the second line with the glyph.
			{
				int line1WordCount = this._wordInfos.Count / 2;
				if ((line1WordCount * 2) != this._wordInfos.Count)
					line1WordCount++;

				// Construct Line 1 with the first half of the words.
				StringBuilder sb = new StringBuilder(100);
				for (int i = 0; i < line1WordCount; i++)
				{
					WordInfo	wordInfo = this._wordInfos[i];
					Size		wordSize = wordInfo.Size;

					sb.Append(wordInfo.Word);

					if (i < (line1WordCount - 1))
						sb.Append(" ");
				}

				this._line1.Text =  sb.ToString();


				// Construct Line 2 with the second half of the words plus the glyph.
				sb						= new StringBuilder(100);
				double wordInfosCount	= this._wordInfos.Count;
				for (int i = line1WordCount; i < wordInfosCount; i++)
				{
					WordInfo	wordInfo = this._wordInfos[i];
					Size		wordSize = wordInfo.Size;

					sb.Append(wordInfo.Word);

					sb.Append(" ");
				}

				this._line2.Text = sb.ToString();
				if (this._glyphSize != Size.Empty)
					this._line2.Inlines.Add(this._glyphInlineUIContainer);
			}


			return base.MeasureOverride(availableSize);
		}

			#endregion //MeasureOverride	

			#region OnApplyTemplate

		/// <summary>
		/// Called when the template is applied
		/// </summary>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();


			// Establish references to the parts we care about.
			this._line1 = this.GetTemplateChild("PART_Line1") as TextBlock;
			this._line2 = this.GetTemplateChild("PART_Line2") as TextBlock;
		}

			#endregion //OnApplyTemplate	
    
		#endregion //Base Class Overrides

		#region Properties

			#region Public Properties

				#region Text

		/// <summary>
		/// Identifies the <see cref="Text"/> dependency property
		/// </summary>
		public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text",
			typeof(object), typeof(LargeToolCaptionPresenter), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure, new PropertyChangedCallback(OnTextChanged)));

		private static void OnTextChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			LargeToolCaptionPresenter ltcp = target as LargeToolCaptionPresenter;
			if (ltcp != null)
				ltcp._cachedWordInfoDirty = true;
		}

		/// <summary>
		/// Returns/sets the text to be displayed.  Note that while this property is defined as type object to support binding to generic content, the control will ignore and values that are type string.
		/// </summary>
		/// <seealso cref="TextProperty"/>
		//[Description("Returns/sets the text to be displayed.  Note that while this property is defined as type object to support binding to generic content, the control will ignore and values that are type string.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public object Text
		{
			get
			{
				return (object)this.GetValue(LargeToolCaptionPresenter.TextProperty);
			}
			set
			{
				this.SetValue(LargeToolCaptionPresenter.TextProperty, value);
			}
		}

				#endregion //Text

				#region Glyph

		/// <summary>
		/// Identifies the <see cref="Glyph"/> dependency property
		/// </summary>
		public static readonly DependencyProperty GlyphProperty = DependencyProperty.Register("Glyph",
			typeof(UIElement), typeof(LargeToolCaptionPresenter), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure, new PropertyChangedCallback(OnGlyphChanged)));

		private static void OnGlyphChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			LargeToolCaptionPresenter ltcp = target as LargeToolCaptionPresenter;
			if (ltcp != null)
				ltcp._cachedGlyphInfoDirty = true;
		}

		/// <summary>
		/// Returns/sets the UIElement that is displayed centered on the second line of the caption.  The glyph will be centered with the seond line of text (if any).
		/// </summary>
		/// <seealso cref="GlyphProperty"/>
		//[Description("Returns/sets the UIElement that is displayed centered on the second line of the caption.  The glyph will be centered with the seond line of text (if any).")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public UIElement Glyph
		{
			get
			{
				return (UIElement)this.GetValue(LargeToolCaptionPresenter.GlyphProperty);
			}
			set
			{
				this.SetValue(LargeToolCaptionPresenter.GlyphProperty, value);
			}
		}

				#endregion //Glyph

			#endregion Public Properties

		#endregion //Properties

		#region Methods

			#region Private Methods

				#region MeasureText

		private Size MeasureText(Inline [] inlines)
		{
			this._textBlockPrivate.Inlines.Clear();
			this._textBlockPrivate.Inlines.AddRange(inlines);

			this._textBlockPrivate.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

			return this._textBlockPrivate.DesiredSize;
		}

		private Size MeasureText(string text)
		{
			this._textBlockPrivate.Text = text;

			this._textBlockPrivate.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

			return this._textBlockPrivate.DesiredSize;
		}

				#endregion //MeasureText	

				#region TrimTextToLength

		private string TrimTextToLength(string text, double trimToLength)
		{
			double	maxLength	= Math.Max(trimToLength, 0);
			string	currentText = text;

			double textLength = this.MeasureText(currentText).Width;
			while (textLength != 0 && textLength > trimToLength)
			{
				currentText = currentText.Substring(0, currentText.Length - 1);
				textLength	= this.MeasureText(currentText).Width;
			}

			return currentText;
		}

				#endregion //MeasureText	
    
			#endregion //Private Methods

		#endregion //Methods

		#region WordInfo struct (private)

		private struct WordInfo
		{
			internal string			Word;
			internal Size			Size;

			internal WordInfo(string word, Size size)
			{
				this.Word	= word;
				this.Size	= size;
			}
		}

		#endregion //WordInfo struct (private)
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