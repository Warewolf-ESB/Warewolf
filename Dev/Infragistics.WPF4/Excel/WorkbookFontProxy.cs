using System;
using System.Collections.Generic;
using System.Text;




using System.Drawing;
using Infragistics.Shared;


namespace Infragistics.Documents.Excel
{
	internal sealed class WorkbookFontProxy : GenericCacheElementProxy<WorkbookFontData>,
		IWorkbookFont,
		ISolidColorItem		// MD 8/23/11 - TFS84306
	{
		#region Member Variables

		// MD 12/31/11 - 12.1 - Cell Format Updates
		private WorksheetCellFormatData owningCellFormat;
		private short saveIndex = -1;

		// MD 7/26/10 - TFS34398
		// The derived proxies must now manage the workbook themselves.
		// MD 3/13/12 - 12.1 - Table Support
		// This is no longer needed.
		//private Workbook workbook;

		#endregion // Member Variables

		#region Constructor

		// MD 12/31/11 - 12.1 - Cell Format Updates
		public WorkbookFontProxy(WorkbookFontData element, Workbook workbook, WorksheetCellFormatData owningCellFormat)
			: this(element, workbook) 
		{
			this.owningCellFormat = owningCellFormat;
		}

		// MD 1/9/12 - 12.1 - Cell Format Updates
		// Removed the parent collection because we can get it from the passed in workbook.
		//public WorkbookFontProxy( WorkbookFontData element, GenericCachedCollection<WorkbookFontData> parentCollection, Workbook workbook )
		//    // MD 7/26/10 - TFS34398
		//    // The derived proxies must now manage the workbook themselves.
		//    //: base( element, parentCollection, workbook ) { }
		//    : base(element, parentCollection) 
		public WorkbookFontProxy(WorkbookFontData element, Workbook workbook)
			: base(element, workbook == null ? null : workbook.Fonts) 
		{
			// MD 3/13/12 - 12.1 - Table Support
			// This is no longer needed.
			//this.workbook = workbook;
		}

		// MD 12/31/11 - 12.1 - Cell Format Updates
		// This is not needed.
		//public WorkbookFontProxy( WorkbookFontProxy source, GenericCachedCollection<WorkbookFontData> parentCollection, Workbook workbook )
		//    : this( source.Element, parentCollection, workbook ) { }

		#endregion Constructor

		#region Interfaces

		// MD 1/17/12 - 12.1 - Cell Format Updates
		#region IWorkbookFont Members
      
		Color IWorkbookFont.Color
		{
			get
			{
				return this.GetColor(this.ColorInfo);
			}
			set
			{
				value = Utilities.RemoveAlphaChannel(value);
				this.ColorInfo = Utilities.ToColorInfo(value);
			}
		}
    
		#endregion // IWorkbookFont Members

		// MD 1/17/12 - 12.1 - Cell Format Updates
		#region ISolidColorItem Members

		Color ISolidColorItem.Color
		{
			get
			{
				return this.GetColor(this.ColorInfo);
			}
			set
			{
				this.ColorInfo = Utilities.ToColorInfo(value);
			}
		}

		#endregion // ISolidColorItem Members

		#endregion // Interfaces

		#region Base Class Overrides

		// MD 12/31/11 - 12.1 - Cell Format Updates
		#region AfterSet

		public override void AfterSet(GenericCachedCollection<WorkbookFontData> collection)
		{
			base.AfterSet(collection);

			if (this.owningCellFormat != null)
				this.owningCellFormat.OnFontChanged();
		} 

		#endregion // AfterSet

		// MD 12/22/11 - 12.1 - Table Support
		// This is no longer needed.
		#region Removed

		//// MD 7/26/10 - TFS34398
		//// The derived proxies must now manage the workbook themselves.
		//#region Workbook

		//public override Workbook Workbook
		//{
		//    get { return this.workbook; }
		//}

		//#endregion // Workbook 

		#endregion // Removed

		#endregion // Base Class Overrides

		#region Methods

		#region SetFontFormatting

		public void SetFontFormatting( IWorkbookFont source )
		{
			if ( source == null )
				throw new ArgumentNullException( "source", SR.GetString( "LE_ArgumentNullException_SourceFont" ) );

			WorkbookFontData data = source as WorkbookFontData;

			if ( data != null )
			{
				// MD 12/22/11 - 12.1 - Table Support
				// We don't need this anymore because SetToElement clones the new element if it is from a different cache collection.
				//if ( data.Workbook != this.Workbook )
				//    throw new ArgumentException( SR.GetString( "LE_ArgumentException_FontFromOtherWorkbook" ), "source" );

				this.SetToElement( data );
				return;
			}

			WorkbookFontProxy proxy = source as WorkbookFontProxy;

			if ( proxy != null )
			{
				// MD 1/18/12 - 12.1 - Cell Format Updates
				this.saveIndex = proxy.saveIndex;

				// MD 12/22/11 - 12.1 - Table Support
				// We don't need this anymore because SetToElement clones the new element if it is from a different cache collection.
				//if ( proxy.Workbook != this.Workbook )
				//    throw new ArgumentException( SR.GetString( "LE_ArgumentException_FontFromOtherWorkbook" ), "source" );

				this.SetToElement( proxy.Element );
				return;
			}

			// MD 12/21/11 - 12.1 - Table Support
			//// MD 6/19/07 - BR24109
			//// Since we are changing the element, call the appropriate method before setting any properties
			//this.BeforeSet();
			//
			//this.Element.SetFontFormatting( source );
			//
			//// MD 6/19/07 - BR24109
			//// Since we changed the element, call the appropriate method afterwards
			//this.AfterSet();
			GenericCachedCollection<WorkbookFontData> collection = this.BeforeSet();
			this.Element.SetFontFormatting(source);
			this.AfterSet(collection);
		}

		#endregion SetFontFormatting

		// MD 1/16/12 - 12.1 - Cell Format Updates
		#region GetColor

		private Color GetColor(WorkbookColorInfo colorInfo)
		{
			if (colorInfo == null)
				return Utilities.ColorEmpty;

			Workbook workbook = this.Element.Workbook;
			if (workbook == null)
			{
				Utilities.DebugFail("This is unexpected.");
				return Utilities.ColorEmpty;
			}

			return colorInfo.GetResolvedColor(workbook);
		}

		#endregion // GetColor

		// MD 12/21/11 - 12.1 - Table Support
		#region SetFontProperty

		private void SetFontProperty<TValue>(TValue value,
			Utilities.PropertyGetter<IWorkbookFont, TValue> propertyGetter,
			Utilities.PropertySetter<IWorkbookFont, TValue> propertySetter)
		{
			TValue existingValue = propertyGetter(this);
			if (EqualityComparer<TValue>.Default.Equals(value, existingValue))
				return;

			GenericCachedCollection<WorkbookFontData> collection = this.BeforeSet();
			propertySetter(this.element, value);
			this.AfterSet(collection);
		}

		#endregion // SetFontProperty

		#endregion Methods

		#region Properties

		#region Public Properties

		#region Bold

		public ExcelDefaultableBoolean Bold
		{
			get { return this.Element.Bold; }
			set
			{
				// MD 12/21/11 - 12.1 - Table Support
				// Refactored duplicate code into a common SetFontProperty method.
				#region Refactored

				//if ( this.Bold != value )
				//{
				//    this.BeforeSet();
				//    this.element.Bold = value;
				//    this.AfterSet();
				//}

				#endregion // Refactored
				this.SetFontProperty(value, Utilities.FontBoldGetter, Utilities.FontBoldSetter);
			}
		}

		#endregion Bold

		// MD 1/17/12 - 12.1 - Cell Format Updates
		// Removed. Color has been replaced by ColorInfo.
		#region Removed

		//#region Color

		//public Color Color
		//{
		//    get { return this.Element.Color; }
		//    set
		//    {
		//        // MD 12/21/11 - 12.1 - Table Support
		//        // Refactored duplicate code into a common SetFontProperty method.
		//        #region Refactored

		//        //if ( this.Color != value )
		//        //{
		//        //    this.BeforeSet();
		//        //    this.element.Color = value;
		//        //    this.AfterSet();
		//        //}

		//        #endregion // Refactored
		//        this.SetFontProperty(value, Utilities.FontColorGetter, Utilities.FontColorSetter);
		//    }
		//}

		//#endregion Color

		#endregion // Removed
		#region ColorInfo

		public WorkbookColorInfo ColorInfo
		{
			get { return this.Element.ColorInfo; }
			set
			{
				this.SetFontProperty(value, Utilities.FontColorInfoGetter, Utilities.FontColorInfoSetter);
			}
		}

		#endregion ColorInfo

		#region Height

		public int Height
		{
			get { return this.Element.Height; }
			set
			{
				// MD 12/21/11 - 12.1 - Table Support
				// Refactored duplicate code into a common SetFontProperty method.
				#region Refactored

				//if ( this.Height != value )
				//{
				//    this.BeforeSet();
				//    this.element.Height = value;
				//    this.AfterSet();
				//}

				#endregion // Refactored
				this.SetFontProperty(value, Utilities.FontHeightGetter, Utilities.FontHeightSetter);
			}
		}

		#endregion Height

		#region Italic

		public ExcelDefaultableBoolean Italic
		{
			get { return this.Element.Italic; }
			set
			{
				// MD 12/21/11 - 12.1 - Table Support
				// Refactored duplicate code into a common SetFontProperty method.
				#region Refactored

				//if ( this.Italic != value )
				//{
				//    this.BeforeSet();
				//    this.element.Italic = value;
				//    this.AfterSet();
				//}

				#endregion // Refactored
				this.SetFontProperty(value, Utilities.FontItalicGetter, Utilities.FontItalicSetter);
			}
		}

		#endregion Italic

		#region Name

		public string Name
		{
			get { return this.Element.Name; }
			set
			{
				// MD 12/21/11 - 12.1 - Table Support
				// Refactored duplicate code into a common SetFontProperty method.
				#region Refactored

				//if ( this.Name != value )
				//{
				//    this.BeforeSet();
				//    this.element.Name = value;
				//    this.AfterSet();
				//}

				#endregion // Refactored
				this.SetFontProperty(value, Utilities.FontNameGetter, Utilities.FontNameSetter);
			}
		}

		#endregion Name

		#region Strikeout

		public ExcelDefaultableBoolean Strikeout
		{
			get { return this.Element.Strikeout; }
			set
			{
				// MD 12/21/11 - 12.1 - Table Support
				// Refactored duplicate code into a common SetFontProperty method.
				#region Refactored

				//if ( this.Strikeout != value )
				//{
				//    this.BeforeSet();
				//    this.element.Strikeout = value;
				//    this.AfterSet();
				//}

				#endregion // Refactored
				this.SetFontProperty(value, Utilities.FontStrikeoutGetter, Utilities.FontStrikeoutSetter);
			}
		}

		#endregion Strikeout

		#region SuperscriptSubscriptStyle

		public FontSuperscriptSubscriptStyle SuperscriptSubscriptStyle
		{
			get { return this.Element.SuperscriptSubscriptStyle; }
			set
			{
				// MD 12/21/11 - 12.1 - Table Support
				// Refactored duplicate code into a common SetFontProperty method.
				#region Refactored

				//if ( this.SuperscriptSubscriptStyle != value )
				//{
				//    this.BeforeSet();
				//    this.element.SuperscriptSubscriptStyle = value;
				//    this.AfterSet();
				//}

				#endregion // Refactored
				this.SetFontProperty(value, Utilities.FontSuperscriptSubscriptStyleGetter, Utilities.FontSuperscriptSubscriptStyleSetter);
			}
		}

		#endregion SuperscriptSubscriptStyle

		#region UnderlineStyle

		public FontUnderlineStyle UnderlineStyle
		{
			get { return this.Element.UnderlineStyle; }
			set
			{
				// MD 12/21/11 - 12.1 - Table Support
				// Refactored duplicate code into a common SetFontProperty method.
				#region Refactored

				//if ( this.UnderlineStyle != value )
				//{
				//    this.BeforeSet();
				//    this.element.UnderlineStyle = value;
				//    this.AfterSet();
				//}

				#endregion // Refactored
				this.SetFontProperty(value, Utilities.FontUnderlineStyleGetter, Utilities.FontUnderlineStyleSetter);
			}
		}

		#endregion UnderlineStyle

		#endregion // Public Properties

		#region Internal Properties

		// MD 1/18/12 - 12.1 - Cell Format Updates
		#region SaveIndex

		internal short SaveIndex
		{
			get { return this.saveIndex; }
			set { this.saveIndex = value; }
		}

		#endregion // SaveIndex

		#endregion // Internal Properties

		#endregion Properties
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