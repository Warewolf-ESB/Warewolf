using System;
using System.Collections.Generic;
using System.Text;
using Infragistics.Documents.Excel.Serialization.BIFF8;




using System.Drawing;
using Infragistics.Shared;


namespace Infragistics.Documents.Excel
{






	internal class WorkbookFontOwnerAccessor :
		IWorkbookFont
	{
		#region Member Variables

		private WorksheetCellFormatProxy owningProxy;

		#endregion Member Variables

		#region Constructor

		public WorkbookFontOwnerAccessor( WorksheetCellFormatProxy owningProxy )
		{
			this.owningProxy = owningProxy;
		}

		#endregion Constructor

		#region Interfaces

		// MD 1/17/12 - 12.1 - Cell Format Updates
		#region IWorkbookFont Members

		Color IWorkbookFont.Color
		{
			get
			{
				WorkbookColorInfo colorInfo = this.ColorInfo;
				if (colorInfo == null)
					return Utilities.ColorEmpty;

				return colorInfo.GetResolvedColor(this.owningProxy.Element.Workbook);
			}
			set
			{
				value = Utilities.RemoveAlphaChannel(value);
				this.ColorInfo = Utilities.ToColorInfo(value);
			}
		}

		#endregion // IWorkbookFont Members

		#endregion // Interfaces

		#region Methods

		#region SetFontFormatting

		public void SetFontFormatting( IWorkbookFont source )
		{
			if ( source == null )
				throw new ArgumentNullException( "source", SR.GetString( "LE_ArgumentNullException_SourceFont" ) );

			// MD 12/21/11 - 12.1 - Table Support
			// The proxies no longer store a reference to the collection. It is stored on the element, so it needs to be returned from the BeforeSet.
			//this.owningProxy.BeforeSet();
			GenericCachedCollection<WorksheetCellFormatData> collection = this.owningProxy.BeforeSet();

			try
			{
				this.owningProxy.Element.FontInternal.SetFontFormatting( source );
			}
			finally
			{
				// MD 12/21/11 - 12.1 - Table Support
				// The proxies no longer store a reference to the collection. It is stored on the element, so it needs to be passed into the AfterSet.
				//this.owningProxy.AfterSet();
				this.owningProxy.AfterSet(collection);
			}
		}

		#endregion SetFontFormatting

		// MD 12/21/11 - 12.1 - Table Support
		#region SetFontProperty

		private void SetFontProperty<TValue>(TValue value, CellFormatValue valueType, bool callBeforeAndAfterSet, CellFormatValueChangedOptions options,
			Utilities.PropertyGetter<IWorkbookFont, TValue> propertyGetter,
			Utilities.PropertySetter<IWorkbookFont, TValue> propertySetter)
		{
			TValue existingValue = propertyGetter(this);
			if (EqualityComparer<TValue>.Default.Equals(value, existingValue))
				return;

			this.owningProxy.OnPropertyChanging(valueType);

			GenericCachedCollection<WorksheetCellFormatData> collection = null;
			if (callBeforeAndAfterSet)
				collection = this.owningProxy.BeforeSet();

			propertySetter(this.owningProxy.Element.FontInternal, value);

			if (callBeforeAndAfterSet)
				this.owningProxy.AfterSet(collection);

			this.owningProxy.OnPropertyChanged(valueType, options);
		}

		#endregion // SetFontProperty

		#endregion Methods

		#region Properties

		#region Bold

		public ExcelDefaultableBoolean Bold
		{
			get { return this.owningProxy.Element.FontInternal.Bold; }
			set
			{
				// MD 10/21/10 - TFS34398
				// Moved all code to a new overload so we can prevent calling BeforeSet and AfterSet in certain cases.
				//if ( this.Bold != value )
				//{
				//    this.owningProxy.BeforeSet();
				//    this.owningProxy.Element.FontInternal.Bold = value;
				//    this.owningProxy.AfterSet();
				//
				//    // MD 10/13/10 - TFS43003
				//    this.owningProxy.OnPropertyChanged(CellFormatValue.FontBold);
				//}
				this.SetBold(value, true, CellFormatValueChangedOptions.DefaultBehavior);
			}
		}

		public void SetBold(ExcelDefaultableBoolean value, bool callBeforeAndAfterSet, CellFormatValueChangedOptions options)
		{
			// MD 12/21/11 - 12.1 - Table Support
			// Refactored duplicate code into a common SetFontProperty method.
			#region Refactored

			//if (this.Bold != value)
			//{
			//    if (callBeforeAndAfterSet)
			//        this.owningProxy.BeforeSet();

			//    this.owningProxy.Element.FontInternal.Bold = value;

			//    if (callBeforeAndAfterSet)
			//        this.owningProxy.AfterSet();

			//    // MD 10/13/10 - TFS43003
			//    this.owningProxy.OnPropertyChanged(CellFormatValue.FontBold, options);		
			//}

			#endregion // Refactored
			this.SetFontProperty(value, CellFormatValue.FontBold, callBeforeAndAfterSet, options, Utilities.FontBoldGetter, Utilities.FontBoldSetter);
		}

		#endregion Bold

		// MD 1/17/12 - 12.1 - Cell Format Updates
		// Removed. Color has been replaced by ColorInfo.
		#region Removed

		//#region Color

		//public Color Color
		//{
		//    get { return this.owningProxy.Element.FontInternal.Color; }
		//    set
		//    {
		//        // MD 10/21/10 - TFS34398
		//        // Moved all code to a new overload so we can prevent calling BeforeSet and AfterSet in certain cases.
		//        //if ( this.Color != value )
		//        //{
		//        //    this.owningProxy.BeforeSet();
		//        //    this.owningProxy.Element.FontInternal.Color = value;
		//        //    this.owningProxy.AfterSet();
		//        //
		//        //    // MD 10/13/10 - TFS43003
		//        //    this.owningProxy.OnPropertyChanged(CellFormatValue.FontColor);
		//        //}
		//        this.SetColor(value, true, CellFormatValueChangedOptions.DefaultBehavior);
		//    }
		//}

		//public void SetColor(Color value, bool callBeforeAndAfterSet, CellFormatValueChangedOptions options)
		//{
		//    // MD 12/21/11 - 12.1 - Table Support
		//    // Refactored duplicate code into a common SetFontProperty method.
		//    #region Refactored

		//    //if (this.Color != value)
		//    //{
		//    //    // MD 11/29/11 - TFS96205
		//    //    this.owningProxy.Element.ClearRoundTripProp(ExtPropType.CellTextColor);

		//    //    if (callBeforeAndAfterSet)
		//    //        this.owningProxy.BeforeSet();

		//    //    this.owningProxy.Element.FontInternal.Color = value;

		//    //    if (callBeforeAndAfterSet)
		//    //        this.owningProxy.AfterSet();

		//    //    // MD 10/13/10 - TFS43003
		//    //    this.owningProxy.OnPropertyChanged(CellFormatValue.FontColor, options);
		//    //}

		//    #endregion // Refactored
		//    if (this.SetFontProperty(value, CellFormatValue.FontColor, callBeforeAndAfterSet, options, Utilities.FontColorGetter, Utilities.FontColorSetter))
		//        this.owningProxy.Element.ClearRoundTripProp(ExtPropType.CellTextColor);
		//}

		//#endregion Color

		#endregion // Removed
		#region Color

		public WorkbookColorInfo ColorInfo
		{
			get { return this.owningProxy.Element.FontInternal.ColorInfo; }
			set
			{
				this.SetColorInfo(value, true, CellFormatValueChangedOptions.DefaultBehavior);
			}
		}

		public void SetColorInfo(WorkbookColorInfo value, bool callBeforeAndAfterSet, CellFormatValueChangedOptions options)
		{
			this.SetFontProperty(value, CellFormatValue.FontColorInfo, callBeforeAndAfterSet, options, Utilities.FontColorInfoGetter, Utilities.FontColorInfoSetter);
		}

		#endregion Color

		#region Height

		public int Height
		{
			get { return this.owningProxy.Element.FontInternal.Height; }
			set
			{
				// MD 10/21/10 - TFS34398
				// Moved all code to a new overload so we can prevent calling BeforeSet and AfterSet in certain cases.
				//if ( this.Height != value )
				//{
				//    this.owningProxy.BeforeSet();
				//    this.owningProxy.Element.FontInternal.Height = value;
				//    this.owningProxy.AfterSet();
				//
				//    // MD 10/13/10 - TFS43003
				//    this.owningProxy.OnPropertyChanged(CellFormatValue.FontHeight);
				//}
				this.SetHeight(value, true, CellFormatValueChangedOptions.DefaultBehavior);
			}
		}

		public void SetHeight(int value, bool callBeforeAndAfterSet, CellFormatValueChangedOptions options)
		{
			// MD 12/21/11 - 12.1 - Table Support
			// Refactored duplicate code into a common SetFontProperty method.
			#region Refactored

			//if (this.Height != value)
			//{
			//    if (callBeforeAndAfterSet)
			//        this.owningProxy.BeforeSet();

			//    this.owningProxy.Element.FontInternal.Height = value;

			//    if (callBeforeAndAfterSet)
			//        this.owningProxy.AfterSet();

			//    // MD 10/13/10 - TFS43003
			//    this.owningProxy.OnPropertyChanged(CellFormatValue.FontHeight, options);
			//}

			#endregion // Refactored
			this.SetFontProperty(value, CellFormatValue.FontHeight, callBeforeAndAfterSet, options, Utilities.FontHeightGetter, Utilities.FontHeightSetter);
		}

		#endregion Height

		#region Italic

		public ExcelDefaultableBoolean Italic
		{
			get { return this.owningProxy.Element.FontInternal.Italic; }
			set
			{
				// MD 10/21/10 - TFS34398
				// Moved all code to a new overload so we can prevent calling BeforeSet and AfterSet in certain cases.
				//if ( this.Italic != value )
				//{
				//    this.owningProxy.BeforeSet();
				//    this.owningProxy.Element.FontInternal.Italic = value;
				//    this.owningProxy.AfterSet();
				//
				//    // MD 10/13/10 - TFS43003
				//    this.owningProxy.OnPropertyChanged(CellFormatValue.FontItalic);
				//}
				this.SetItalic(value, true, CellFormatValueChangedOptions.DefaultBehavior);
			}
		}

		public void SetItalic(ExcelDefaultableBoolean value, bool callBeforeAndAfterSet, CellFormatValueChangedOptions options)
		{
			// MD 12/21/11 - 12.1 - Table Support
			// Refactored duplicate code into a common SetFontProperty method.
			#region Refactored

			//if (this.Italic != value)
			//{
			//    if (callBeforeAndAfterSet)
			//        this.owningProxy.BeforeSet();

			//    this.owningProxy.Element.FontInternal.Italic = value;

			//    if (callBeforeAndAfterSet)
			//        this.owningProxy.AfterSet();

			//    // MD 10/13/10 - TFS43003
			//    this.owningProxy.OnPropertyChanged(CellFormatValue.FontItalic, options);
			//}

			#endregion // Refactored
			this.SetFontProperty(value, CellFormatValue.FontItalic, callBeforeAndAfterSet, options, Utilities.FontItalicGetter, Utilities.FontItalicSetter);
		}

		#endregion Italic

		#region Name

		public string Name
		{
			get { return this.owningProxy.Element.FontInternal.Name; }
			set
			{
				// MD 10/21/10 - TFS34398
				// Moved all code to a new overload so we can prevent calling BeforeSet and AfterSet in certain cases.
				//if ( this.Name != value )
				//{
				//    this.owningProxy.BeforeSet();
				//    this.owningProxy.Element.FontInternal.Name = value;
				//    this.owningProxy.AfterSet();
				//
				//    // MD 10/13/10 - TFS43003
				//    this.owningProxy.OnPropertyChanged(CellFormatValue.FontName);
				//}
				this.SetName(value, true, CellFormatValueChangedOptions.DefaultBehavior);
			}
		}

		public void SetName(string value, bool callBeforeAndAfterSet, CellFormatValueChangedOptions options)
		{
			// MD 12/21/11 - 12.1 - Table Support
			// Refactored duplicate code into a common SetFontProperty method.
			#region Refactored

			//if (this.Name != value)
			//{
			//    // MD 11/29/11 - TFS96205
			//    this.owningProxy.Element.ClearRoundTripProp(ExtPropType.FontScheme);

			//    if (callBeforeAndAfterSet)
			//        this.owningProxy.BeforeSet();

			//    this.owningProxy.Element.FontInternal.Name = value;

			//    if (callBeforeAndAfterSet)
			//        this.owningProxy.AfterSet();

			//    // MD 10/13/10 - TFS43003
			//    this.owningProxy.OnPropertyChanged(CellFormatValue.FontName, options);
			//}

			#endregion // Refactored
			this.SetFontProperty(value, CellFormatValue.FontName, callBeforeAndAfterSet, options, Utilities.FontNameGetter, Utilities.FontNameSetter);
		}

		#endregion Name

		#region Strikeout

		public ExcelDefaultableBoolean Strikeout
		{
			get { return this.owningProxy.Element.FontInternal.Strikeout; }
			set
			{
				// MD 10/21/10 - TFS34398
				// Moved all code to a new overload so we can prevent calling BeforeSet and AfterSet in certain cases.
				//if ( this.Strikeout != value )
				//{
				//    this.owningProxy.BeforeSet();
				//    this.owningProxy.Element.FontInternal.Strikeout = value;
				//    this.owningProxy.AfterSet();
				//
				//    // MD 10/13/10 - TFS43003
				//    this.owningProxy.OnPropertyChanged(CellFormatValue.FontStrikeout);
				//}
				this.SetStrikeout(value, true, CellFormatValueChangedOptions.DefaultBehavior);
			}
		}

		public void SetStrikeout(ExcelDefaultableBoolean value, bool callBeforeAndAfterSet, CellFormatValueChangedOptions options)
		{
			// MD 12/21/11 - 12.1 - Table Support
			// Refactored duplicate code into a common SetFontProperty method.
			#region Refactored

			//if (this.Strikeout != value)
			//{
			//    if (callBeforeAndAfterSet)
			//        this.owningProxy.BeforeSet();

			//    this.owningProxy.Element.FontInternal.Strikeout = value;

			//    if (callBeforeAndAfterSet)
			//        this.owningProxy.AfterSet();

			//    // MD 10/13/10 - TFS43003
			//    this.owningProxy.OnPropertyChanged(CellFormatValue.FontStrikeout, options);
			//}

			#endregion // Refactored
			this.SetFontProperty(value, CellFormatValue.FontStrikeout, callBeforeAndAfterSet, options, Utilities.FontStrikeoutGetter, Utilities.FontStrikeoutSetter);
		}

		#endregion Strikeout

		#region SuperscriptSubscriptStyle

		public FontSuperscriptSubscriptStyle SuperscriptSubscriptStyle
		{
			get { return this.owningProxy.Element.FontInternal.SuperscriptSubscriptStyle; }
			set
			{
				// MD 10/21/10 - TFS34398
				// Moved all code to a new overload so we can prevent calling BeforeSet and AfterSet in certain cases.
				//if ( this.SuperscriptSubscriptStyle != value )
				//{
				//    this.owningProxy.BeforeSet();
				//    this.owningProxy.Element.FontInternal.SuperscriptSubscriptStyle = value;
				//    this.owningProxy.AfterSet();
				//
				//    // MD 10/13/10 - TFS43003
				//    this.owningProxy.OnPropertyChanged(CellFormatValue.FontSuperscriptSubscriptStyle);
				//}
				this.SetSuperscriptSubscriptStyle(value, true, CellFormatValueChangedOptions.DefaultBehavior);
			}
		}

		public void SetSuperscriptSubscriptStyle(FontSuperscriptSubscriptStyle value, bool callBeforeAndAfterSet, CellFormatValueChangedOptions options)
		{
			// MD 12/21/11 - 12.1 - Table Support
			#region Refactored

			// Refactored duplicate code into a common SetFontProperty method.
			//if (this.SuperscriptSubscriptStyle != value)
			//{
			//    if (callBeforeAndAfterSet)
			//        this.owningProxy.BeforeSet();

			//    this.owningProxy.Element.FontInternal.SuperscriptSubscriptStyle = value;

			//    if (callBeforeAndAfterSet)
			//        this.owningProxy.AfterSet();

			//    // MD 10/13/10 - TFS43003
			//    this.owningProxy.OnPropertyChanged(CellFormatValue.FontSuperscriptSubscriptStyle, options);
			//}

			#endregion // Refactored
			this.SetFontProperty(value, CellFormatValue.FontSuperscriptSubscriptStyle, callBeforeAndAfterSet, options, Utilities.FontSuperscriptSubscriptStyleGetter, Utilities.FontSuperscriptSubscriptStyleSetter);
		}

		#endregion SuperscriptSubscriptStyle

		#region UnderlineStyle

		public FontUnderlineStyle UnderlineStyle
		{
			get { return this.owningProxy.Element.FontInternal.UnderlineStyle; }
			set
			{
				// MD 10/21/10 - TFS34398
				// Moved all code to a new overload so we can prevent calling BeforeSet and AfterSet in certain cases.
				//if ( this.UnderlineStyle != value )
				//{
				//    this.owningProxy.BeforeSet();
				//    this.owningProxy.Element.FontInternal.UnderlineStyle = value;
				//    this.owningProxy.AfterSet();
				//
				//    // MD 10/13/10 - TFS43003
				//    this.owningProxy.OnPropertyChanged(CellFormatValue.FontUnderlineStyle);
				//}
				this.SetUnderlineStyle(value, true, CellFormatValueChangedOptions.DefaultBehavior);
			}
		}

		public void SetUnderlineStyle(FontUnderlineStyle value, bool callBeforeAndAfterSet, CellFormatValueChangedOptions options)
		{
			// MD 12/21/11 - 12.1 - Table Support
			// Refactored duplicate code into a common SetFontProperty method.
			#region Refactored

			//if (this.UnderlineStyle != value)
			//{
			//    if (callBeforeAndAfterSet)
			//        this.owningProxy.BeforeSet();

			//    this.owningProxy.Element.FontInternal.UnderlineStyle = value;

			//    if (callBeforeAndAfterSet)
			//        this.owningProxy.AfterSet();

			//    // MD 10/13/10 - TFS43003
			//    this.owningProxy.OnPropertyChanged(CellFormatValue.FontUnderlineStyle, options);
			//}

			#endregion // Refactored
			this.SetFontProperty(value, CellFormatValue.FontUnderlineStyle, callBeforeAndAfterSet, options, Utilities.FontUnderlineStyleGetter, Utilities.FontUnderlineStyleSetter);
		}

		#endregion UnderlineStyle

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