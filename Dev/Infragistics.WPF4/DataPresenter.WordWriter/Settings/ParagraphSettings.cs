using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.ComponentModel;
using Infragistics.Documents.Word;

namespace Infragistics.Windows.DataPresenter.WordWriter
{
	/// <summary>
	/// Exposes format settings for table cells when exporting to Word
	/// </summary>
	[TypeConverter(typeof(ExpandableObjectConverter))]
	public class WordParagraphSettings : DependencyObject
	{
		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="WordParagraphSettings"/>
		/// </summary>
		public WordParagraphSettings()
		{
		}
		#endregion //Constructor

		#region Properties

		#region Alignment

		/// <summary>
		/// Identifies the <see cref="Alignment"/> dependency property
		/// </summary>
		public static readonly DependencyProperty AlignmentProperty = DependencyProperty.Register("Alignment",
			typeof(ParagraphAlignment?), typeof(WordParagraphSettings), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Returns or sets the horizontal alignment for the paragraph.
		/// </summary>
		/// <seealso cref="AlignmentProperty"/>
		/// <seealso cref="ParagraphPropertiesBase.Alignment"/>
		[Bindable(true)]
		public ParagraphAlignment? Alignment
		{
			get
			{
				return (ParagraphAlignment?)this.GetValue(WordParagraphSettings.AlignmentProperty);
			}
			set
			{
				this.SetValue(WordParagraphSettings.AlignmentProperty, value);
			}
		}

		#endregion //Alignment

		#region LeftIndent

		/// <summary>
		/// Identifies the <see cref="LeftIndent"/> dependency property
		/// </summary>
		public static readonly DependencyProperty LeftIndentProperty = DependencyProperty.Register("LeftIndent",
			typeof(DeviceUnitLength?), typeof(WordParagraphSettings), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Returns or sets the amount of indentation from the left margin for the paragraph.
		/// </summary>
		/// <seealso cref="RightIndent"/>
		/// <seealso cref="LeftIndentProperty"/>
		/// <seealso cref="Infragistics.Documents.Word.ParagraphPropertiesBase.LeftIndent"/>
		[Bindable(true)]
		public DeviceUnitLength? LeftIndent
		{
			get
			{
				return (DeviceUnitLength?)this.GetValue(WordParagraphSettings.LeftIndentProperty);
			}
			set
			{
				this.SetValue(WordParagraphSettings.LeftIndentProperty, value);
			}
		}

		#endregion //LeftIndent

		#region LineSpacing

		/// <summary>
		/// Identifies the <see cref="LineSpacing"/> dependency property
		/// </summary>
		public static readonly DependencyProperty LineSpacingProperty = DependencyProperty.Register("LineSpacing",
			typeof(DeviceUnitLength?), typeof(WordParagraphSettings), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Returns or sets the amount of vertical spacing that appears between lines of text within the paragraph based on the <see cref="LineSpacingRule"/>.
		/// </summary>
		/// <seealso cref="LineSpacingRule"/>
		/// <seealso cref="LineSpacingProperty"/>
		/// <seealso cref="Infragistics.Documents.Word.ParagraphProperties.LineSpacingAuto"/>
		/// <seealso cref="Infragistics.Documents.Word.ParagraphProperties.LineSpacingExact"/>
		/// <seealso cref="Infragistics.Documents.Word.ParagraphProperties.LineSpacingMinimum"/>
		[Bindable(true)]
		public DeviceUnitLength? LineSpacing
		{
			get
			{
				return (DeviceUnitLength?)this.GetValue(WordParagraphSettings.LineSpacingProperty);
			}
			set
			{
				this.SetValue(WordParagraphSettings.LineSpacingProperty, value);
			}
		}

		#endregion //LineSpacing

		#region LineSpacingRule

		/// <summary>
		/// Identifies the <see cref="LineSpacingRule"/> dependency property
		/// </summary>
		public static readonly DependencyProperty LineSpacingRuleProperty = DependencyProperty.Register("LineSpacingRule",
			typeof(WordParagraphLineSpacingRule?), typeof(WordParagraphSettings), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Returns or sets the amount of vertical spacing that appears between lines of text within the paragraph based on the <see cref="LineSpacingRule"/>.
		/// </summary>
		/// <seealso cref="LineSpacing"/>
		/// <seealso cref="LineSpacingRuleProperty"/>
		/// <seealso cref="Infragistics.Documents.Word.ParagraphProperties.LineSpacingAuto"/>
		/// <seealso cref="Infragistics.Documents.Word.ParagraphProperties.LineSpacingExact"/>
		/// <seealso cref="Infragistics.Documents.Word.ParagraphProperties.LineSpacingMinimum"/>
		[Bindable(true)]
		public WordParagraphLineSpacingRule? LineSpacingRule
		{
			get
			{
				return (WordParagraphLineSpacingRule?)this.GetValue(WordParagraphSettings.LineSpacingRuleProperty);
			}
			set
			{
				this.SetValue(WordParagraphSettings.LineSpacingRuleProperty, value);
			}
		}

		#endregion //LineSpacingRule

		#region RightToLeft

		/// <summary>
		/// Identifies the <see cref="RightToLeft"/> dependency property
		/// </summary>
		public static readonly DependencyProperty RightToLeftProperty = DependencyProperty.Register("RightToLeft",
			typeof(bool?), typeof(WordParagraphSettings), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Returns or sets whether the paragraph is presented in a right to left direction.
		/// </summary>
		/// <seealso cref="RightToLeftProperty"/>
		/// <seealso cref="ParagraphProperties.RightToLeft"/>
		[Bindable(true)]
		public bool? RightToLeft
		{
			get
			{
				return (bool?)this.GetValue(WordParagraphSettings.RightToLeftProperty);
			}
			set
			{
				this.SetValue(WordParagraphSettings.RightToLeftProperty, value);
			}
		}

		#endregion //RightToLeft

		#region RightIndent

		/// <summary>
		/// Identifies the <see cref="RightIndent"/> dependency property
		/// </summary>
		public static readonly DependencyProperty RightIndentProperty = DependencyProperty.Register("RightIndent",
			typeof(DeviceUnitLength?), typeof(WordParagraphSettings), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Returns or sets the amount of indentation from the right margin for the paragraph.
		/// </summary>
		/// <seealso cref="LeftIndent"/>
		/// <seealso cref="RightIndentProperty"/>
		/// <seealso cref="Infragistics.Documents.Word.ParagraphProperties.RightIndent"/>
		[Bindable(true)]
		public DeviceUnitLength? RightIndent
		{
			get
			{
				return (DeviceUnitLength?)this.GetValue(WordParagraphSettings.RightIndentProperty);
			}
			set
			{
				this.SetValue(WordParagraphSettings.RightIndentProperty, value);
			}
		}

		#endregion //RightIndent

		#region SpacingAfter

		/// <summary>
		/// Identifies the <see cref="SpacingAfter"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SpacingAfterProperty = DependencyProperty.Register("SpacingAfter",
			typeof(DeviceUnitLength?), typeof(WordParagraphSettings), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Returns or sets the amount of spacing below the paragraph.
		/// </summary>
		/// <seealso cref="SpacingBefore"/>
		/// <seealso cref="SpacingAfterProperty"/>
		/// <seealso cref="Infragistics.Documents.Word.ParagraphProperties.SpacingAfter"/>
		[Bindable(true)]
		public DeviceUnitLength? SpacingAfter
		{
			get
			{
				return (DeviceUnitLength?)this.GetValue(WordParagraphSettings.SpacingAfterProperty);
			}
			set
			{
				this.SetValue(WordParagraphSettings.SpacingAfterProperty, value);
			}
		}

		#endregion //SpacingAfter

		#region SpacingBefore

		/// <summary>
		/// Identifies the <see cref="SpacingBefore"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SpacingBeforeProperty = DependencyProperty.Register("SpacingBefore",
			typeof(DeviceUnitLength?), typeof(WordParagraphSettings), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Returns or sets the amount of spacing above the paragraph.
		/// </summary>
		/// <seealso cref="SpacingBeforeProperty"/>
		/// <seealso cref="SpacingAfter"/>
		/// <seealso cref="Infragistics.Documents.Word.ParagraphProperties.SpacingBefore"/>
		[Bindable(true)]
		public DeviceUnitLength? SpacingBefore
		{
			get
			{
				return (DeviceUnitLength?)this.GetValue(WordParagraphSettings.SpacingBeforeProperty);
			}
			set
			{
				this.SetValue(WordParagraphSettings.SpacingBeforeProperty, value);
			}
		}

		#endregion //SpacingBefore

		#endregion //Properties

		#region Methods

		#region Initialize
		internal void Initialize(WordDocumentWriter writer, ParagraphProperties paragraphProperties)
		{
			UnitOfMeasurement unit = writer.Unit;

			paragraphProperties.Alignment = this.Alignment ?? paragraphProperties.Alignment;
			paragraphProperties.LeftIndent = WordExporter.ConvertUnits(this.LeftIndent, null, null, unit) ?? paragraphProperties.LeftIndent;
			paragraphProperties.RightIndent = WordExporter.ConvertUnits(this.RightIndent, null, null, unit) ?? paragraphProperties.RightIndent;
			paragraphProperties.RightToLeft = this.RightToLeft ?? paragraphProperties.RightToLeft;
			paragraphProperties.SpacingAfter = WordExporter.ConvertUnits(this.SpacingAfter, null, null, unit) ?? paragraphProperties.SpacingAfter;
			paragraphProperties.SpacingBefore = WordExporter.ConvertUnits(this.SpacingBefore, null, null, unit) ?? paragraphProperties.SpacingBefore;

			DeviceUnitLength? lineSpacing = this.LineSpacing;

			if (null != lineSpacing)
			{
				// default to auto as word does if unspecified
				float? value = WordExporter.ConvertUnits(lineSpacing, null, null, unit);

				switch (this.LineSpacingRule ?? WordParagraphLineSpacingRule.Auto)
				{
					case WordParagraphLineSpacingRule.Auto:
						paragraphProperties.LineSpacingAuto = value;
						break;
					case WordParagraphLineSpacingRule.Exact:
						paragraphProperties.LineSpacingExact = value;
						break;
					case WordParagraphLineSpacingRule.AtLeast:
						paragraphProperties.LineSpacingMinimum = value;
						break;
				}
			}
		}
		#endregion //Initialize

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