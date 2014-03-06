using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Windows.Media;
using Infragistics.Documents.Word;
using System.Windows;
using WordFont = Infragistics.Documents.Word.Font;

namespace Infragistics.Windows.DataPresenter.WordWriter
{
	/// <summary>
	/// Custom object used to provide font settings for the Word export.
	/// </summary>
	[TypeConverter(typeof(ExpandableObjectConverter))]
	public class WordFontSettings : DependencyObject
	{
		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="WordFontSettings"/>
		/// </summary>
		public WordFontSettings()
		{
		} 
		#endregion //Constructor

		
#region Infragistics Source Cleanup (Region)





















#endregion // Infragistics Source Cleanup (Region)


		#region Properties

		#region Bold

		/// <summary>
		/// Identifies the <see cref="Bold"/> dependency property
		/// </summary>
		public static readonly DependencyProperty BoldProperty = DependencyProperty.Register("Bold",
			typeof(bool?), typeof(WordFontSettings), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Returns or sets a value indicating whether the font is bolded
		/// </summary>
		/// <seealso cref="BoldProperty"/>
		/// <seealso cref="Infragistics.Documents.Word.Font.Bold"/>
		[Bindable(true)]
		public bool? Bold
		{
			get
			{
				return (bool?)this.GetValue(WordFontSettings.BoldProperty);
			}
			set
			{
				this.SetValue(WordFontSettings.BoldProperty, value);
			}
		}

		#endregion //Bold

		#region FontFamily

		/// <summary>
		/// Identifies the <see cref="FontFamily"/> dependency property
		/// </summary>
		public static readonly DependencyProperty FontFamilyProperty = DependencyProperty.Register("FontFamily",
			typeof(FontFamily), typeof(WordFontSettings), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Returns or sets the FontFamily of the font
		/// </summary>
		/// <seealso cref="FontFamilyProperty"/>
		/// <seealso cref="Infragistics.Documents.Word.Font.Name"/>
		[Bindable(true)]
		public FontFamily FontFamily
		{
			get
			{
				return (FontFamily)this.GetValue(WordFontSettings.FontFamilyProperty);
			}
			set
			{
				this.SetValue(WordFontSettings.FontFamilyProperty, value);
			}
		}
		#endregion //FontFamily

		#region ForeColor

		/// <summary>
		/// Identifies the <see cref="ForeColor"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ForeColorProperty = DependencyProperty.Register("ForeColor",
			typeof(Color), typeof(WordFontSettings), new FrameworkPropertyMetadata(WordExporter.EmptyColor));

		/// <summary>
		/// Returns or sets the color of the text
		/// </summary>
		/// <seealso cref="ForeColorProperty"/>
		/// <seealso cref="Infragistics.Documents.Word.Font.ForeColor"/>
		[Bindable(true)]
		public Color ForeColor
		{
			get
			{
				return (Color)this.GetValue(WordFontSettings.ForeColorProperty);
			}
			set
			{
				this.SetValue(WordFontSettings.ForeColorProperty, value);
			}
		}

		#endregion //ForeColor

		#region Italic

		/// <summary>
		/// Identifies the <see cref="Italic"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ItalicProperty = DependencyProperty.Register("Italic",
			typeof(bool?), typeof(WordFontSettings), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Returns or sets a value indicating if the font is italicized.
		/// </summary>
		/// <seealso cref="ItalicProperty"/>
		/// <seealso cref="Infragistics.Documents.Word.Font.Italic"/>
		[Bindable(true)]
		public bool? Italic
		{
			get
			{
				return (bool?)this.GetValue(WordFontSettings.ItalicProperty);
			}
			set
			{
				this.SetValue(WordFontSettings.ItalicProperty, value);
			}
		}

		#endregion //Italic

		#region Size

		/// <summary>
		/// Identifies the <see cref="Size"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SizeProperty = DependencyProperty.Register("Size",
			typeof(DeviceUnitLength?), typeof(WordFontSettings), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Returns or sets the size of the font in twips.
		/// </summary>
		/// <seealso cref="SizeProperty"/>
		/// <seealso cref="Infragistics.Documents.Word.Font.Size"/>
		[Bindable(true)]
		public DeviceUnitLength? Size
		{
			get
			{
				return (DeviceUnitLength?)this.GetValue(WordFontSettings.SizeProperty);
			}
			set
			{
				this.SetValue(WordFontSettings.SizeProperty, value);
			}
		}

		#endregion //Size

		#region Underline

		/// <summary>
		/// Identifies the <see cref="Underline"/> dependency property
		/// </summary>
		public static readonly DependencyProperty UnderlineProperty = DependencyProperty.Register("Underline",
			typeof(Underline), typeof(WordFontSettings), new FrameworkPropertyMetadata(Underline.Default));

		/// <summary>
		/// Returns or sets the type of underline for the font.
		/// </summary>
		/// <seealso cref="UnderlineProperty"/>
		/// <seealso cref="Infragistics.Documents.Word.Font.Underline"/>
		[Bindable(true)]
		public Underline Underline
		{
			get
			{
				return (Underline)this.GetValue(WordFontSettings.UnderlineProperty);
			}
			set
			{
				this.SetValue(WordFontSettings.UnderlineProperty, value);
			}
		}

		#endregion //Underline

		#region UnderlineColor

		/// <summary>
		/// Identifies the <see cref="UnderlineColor"/> dependency property
		/// </summary>
		public static readonly DependencyProperty UnderlineColorProperty = DependencyProperty.Register("UnderlineColor",
			typeof(Color), typeof(WordFontSettings), new FrameworkPropertyMetadata(WordExporter.EmptyColor));

		/// <summary>
		/// Returns or sets the color of the underline.
		/// </summary>
		/// <seealso cref="UnderlineColorProperty"/>
		/// <seealso cref="Infragistics.Documents.Word.Font.UnderlineColor"/>
		[Bindable(true)]
		public Color UnderlineColor
		{
			get
			{
				return (Color)this.GetValue(WordFontSettings.UnderlineColorProperty);
			}
			set
			{
				this.SetValue(WordFontSettings.UnderlineColorProperty, value);
			}
		}

		#endregion //UnderlineColor

		#endregion //Properties

		#region Methods

		#region Initialize
		internal void Initialize(WordDocumentWriter writer, WordFont font)
		{
			UnitOfMeasurement unit = writer.Unit;

			font.ForeColor = WordExporter.ResolveProperty(this.ForeColor, font.ForeColor);
			font.Bold = this.Bold ?? font.Bold;
			font.Italic = this.Italic ?? font.Italic;

			var ff = this.FontFamily;

			if (null != ff)
				font.Name = ff.Source;

			font.Size = WordExporter.ConvertUnits(this.Size, 20f, WordFont.SizeMaxValueTwips, unit) ?? font.Size;
			font.Underline = WordExporter.ResolveProperty(this.Underline, font.Underline, Underline.Default);
			font.UnderlineColor = WordExporter.ResolveProperty(this.UnderlineColor, font.UnderlineColor);
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