using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.ComponentModel;
using Infragistics.Documents.Word;

namespace Infragistics.Windows.DataPresenter.WordWriter
{
	/// <summary>
	/// Exposes sections settings for the Word export.
	/// </summary>
	[TypeConverter(typeof(ExpandableObjectConverter))]
	public class WordSectionSettings : DependencyObject
	{
		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="WordSectionSettings"/>
		/// </summary>
		public WordSectionSettings()
		{
		}
		#endregion //Constructor

		#region Properties

		#region FooterMargin
		
#region Infragistics Source Cleanup (Region)





















#endregion // Infragistics Source Cleanup (Region)

		#endregion //FooterMargin

		#region HeaderMargin
		
#region Infragistics Source Cleanup (Region)





















#endregion // Infragistics Source Cleanup (Region)

		#endregion //HeaderMargin

		#region PageMargins

		/// <summary>
		/// Identifies the <see cref="PageMargins"/> dependency property
		/// </summary>
		public static readonly DependencyProperty PageMarginsProperty = DependencyProperty.Register("PageMargins",
			typeof(DeviceUnitThickness?), typeof(WordSectionSettings), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Returns or sets the margins for all pages in the section to which the settings are applied.
		/// </summary>
		/// <seealso cref="PageMarginsProperty"/>
		/// <seealso cref="Infragistics.Documents.Word.SectionProperties.PageMargins"/>
		[Bindable(true)]
		public DeviceUnitThickness? PageMargins
		{
			get
			{
				return (DeviceUnitThickness?)this.GetValue(WordSectionSettings.PageMarginsProperty);
			}
			set
			{
				this.SetValue(WordSectionSettings.PageMarginsProperty, value);
			}
		}

		#endregion //PageMargins

		#region PageOrientation

		/// <summary>
		/// Identifies the <see cref="PageOrientation"/> dependency property
		/// </summary>
		public static readonly DependencyProperty PageOrientationProperty = DependencyProperty.Register("PageOrientation",
			typeof(PageOrientation), typeof(WordSectionSettings), new FrameworkPropertyMetadata(Infragistics.Documents.Word.PageOrientation.Default));

		/// <summary>
		/// Returns or sets the orientation for all the pages in the section to which the settings are applied.
		/// </summary>
		/// <seealso cref="PageOrientationProperty"/>
		/// <seealso cref="Infragistics.Documents.Word.SectionProperties.PageOrientation"/>
		[Bindable(true)]
		public PageOrientation PageOrientation
		{
			get
			{
				return (PageOrientation)this.GetValue(WordSectionSettings.PageOrientationProperty);
			}
			set
			{
				this.SetValue(WordSectionSettings.PageOrientationProperty, value);
			}
		}

		#endregion //PageOrientation

		#region PageSize

		/// <summary>
		/// Identifies the <see cref="PageSize"/> dependency property
		/// </summary>
		public static readonly DependencyProperty PageSizeProperty = DependencyProperty.Register("PageSize",
			typeof(DeviceUnitSize?), typeof(WordSectionSettings), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Returns or sets the size of the pages in the section to which the settings are applied.
		/// </summary>
		/// <seealso cref="PageSizeProperty"/>
		/// <seealso cref="Infragistics.Documents.Word.SectionProperties.PageSize"/>
		[Bindable(true)]
		public DeviceUnitSize? PageSize
		{
			get
			{
				return (DeviceUnitSize?)this.GetValue(WordSectionSettings.PageSizeProperty);
			}
			set
			{
				this.SetValue(WordSectionSettings.PageSizeProperty, value);
			}
		}

		#endregion //PageSize

		#region PaperCode

		/// <summary>
		/// Identifies the <see cref="PaperCode"/> dependency property
		/// </summary>
		public static readonly DependencyProperty PaperCodeProperty = DependencyProperty.Register("PaperCode",
			typeof(int?), typeof(WordSectionSettings), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Returns or sets a value which represents a printer specific paper code for the paper type for all pages for the section to which the settings are applied.
		/// </summary>
		/// <seealso cref="PaperCodeProperty"/>
		/// <seealso cref="Infragistics.Documents.Word.SectionProperties.PaperCode"/>
		[Bindable(true)]
		public int? PaperCode
		{
			get
			{
				return (int?)this.GetValue(WordSectionSettings.PaperCodeProperty);
			}
			set
			{
				this.SetValue(WordSectionSettings.PaperCodeProperty, value);
			}
		}

		#endregion //PaperCode

		#endregion //Properties

		#region Methods

		#region Initialize
		internal void Initialize(WordDocumentWriter writer, SectionProperties sectionProperties)
		{
			UnitOfMeasurement unit = writer.Unit;
			//sectionProperties.HeaderMargin = WordExporter.ConvertUnits(this.HeaderMargin, null, null, unit);
			//sectionProperties.FooterMargin = WordExporter.ConvertUnits(this.FooterMargin, null, null, unit);
			sectionProperties.PageMargins = WordExporter.ConvertUnits(this.PageMargins, unit) ?? sectionProperties.PageMargins;
			sectionProperties.PageSize = WordExporter.ConvertUnits(this.PageSize, unit) ?? sectionProperties.PageSize;
			sectionProperties.PaperCode = this.PaperCode ?? sectionProperties.PaperCode;
			sectionProperties.PageOrientation = WordExporter.ResolveProperty(this.PageOrientation, sectionProperties.PageOrientation, PageOrientation.Default);
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