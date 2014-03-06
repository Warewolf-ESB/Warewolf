using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements
{
    internal class RichTextRunElement : XLSXElementBase
    {
        #region XML Schema Fragment

        //<complexType name="CT_RElt"> 
        //  <sequence> 
        //  <element name="rPr" type="CT_RPrElt" minOccurs="0" maxOccurs="1"/> 
        //  <element name="t" type="ST_Xstring" minOccurs="1" maxOccurs="1"/> 
        //  </sequence> 
        //</complexType> 

        #endregion //XML Schema Fragment

        #region Constants






        public const string LocalName = "r";






        public const string QualifiedName =
            XLSXElementBase.DefaultXmlNamespace +
            XmlElementBase.NamespaceSeparator +
            RichTextRunElement.LocalName;

        #endregion //Constants

        #region Base class overrides

        #region Type
        /// <summary>
		/// Returns the <see cref="XLSXElementType">XLSXElementType</see> constant which identifies this element.
        /// </summary>
        public override XLSXElementType Type
        {
            get { return XLSXElementType.r; }
        }
        #endregion Type
        
        #region Load

        protected override void Load(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode)
        {
			// MD 1/29/12 - 12.1 - Cell Format Updates
			// Move this to the RichTextRunPropertiesElement so we only create runs when we need to.
			#region Moved

			//// MD 11/3/10 - TFS49093
			//// The formatted string data is now stored on the FormattedStringElement.
			//// Get the FormattedString off of the context stack so that we can set the value
			////WorkbookSerializationManager.FormattedStringHolder holder =
			////    (WorkbookSerializationManager.FormattedStringHolder)manager.ContextStack[ typeof( WorkbookSerializationManager.FormattedStringHolder ) ];
			////if ( holder == null )
			////{
			////    Utilities.DebugFail( "Could not get the formatted string holder from the context stack" );
			////    return;
			////}
			////
			////int startPosition = 0;
			////string formattedValue = holder.Value.UnformattedString;
			////if ( formattedValue != null )
			////    startPosition = holder.Value.UnformattedString.Length;
			////
			////FormattedStringRun run = new FormattedStringRun( holder.Value, startPosition, manager.Workbook );
			////holder.Value.FormattingRuns.Add( run );
			//FormattedStringElement formattedStringElement = manager.ContextStack[typeof(FormattedStringElement)] as FormattedStringElement;
			//if (formattedStringElement == null)
			//{
			//    Utilities.DebugFail("Could not get the formatted string element from the context stack");
			//    return;
			//}

			//int startPosition = 0;
			//string formattedValue = formattedStringElement.UnformattedString;
			//if (formattedValue != null)
			//    startPosition = formattedStringElement.UnformattedString.Length;

			//// MD 11/9/11 - TFS85193
			////FormattingRun run = new FormattingRun(formattedStringElement, startPosition, manager.Workbook);
			//FormattedStringRun run = new FormattedStringRun(formattedStringElement, startPosition);

			//formattedStringElement.FormattingRuns.Add(run);
			//// ------------------- End of fix for TFS49093 ------------------------------

			//// MD 11/9/11 - TFS85193
			//// This does nothing so it can be removed.
			////WorkbookFontData fontData = new WorkbookFontData( manager.Workbook );
			////run.Font.SetFontFormatting( fontData );

			//// MD 2/15/11 - TFS66333
			//// The element might have been switched out when setting the formatting above.
			////manager.ContextStack.Push( fontData );
			//// MD 11/9/11 - TFS85193
			//// We need to pass in the proxy so we don't modify the element if any other things are referencing it.
			////manager.ContextStack.Push(run.Font.Element);
			//manager.ContextStack.Push(run.GetFontInternal(manager.Workbook));

			//// Push an empty ColorInfo onto the context stack so it can be populated later
			//ColorInfo info = new ColorInfo();
			//manager.ContextStack.Push(info);

			#endregion // Moved
        }

        #endregion Load

		// MD 1/29/12 - 12.1 - Cell Format Updates
		// Move this to the RichTextRunPropertiesElement so we only create runs when we need to.
		#region Moved

		//#region OnAfterLoadChildElements

		//protected override void OnAfterLoadChildElements( Excel2007WorkbookSerializationManager manager, ElementDataCache elementCache )
		//{
		//    ColorInfo colorInfo = (ColorInfo)manager.ContextStack[typeof(ColorInfo)];
		//    if (colorInfo == null)
		//    {
		//        Utilities.DebugFail("For some reason, the ColorInfo was removed from the context stack");
		//        return;
		//    }

		//    // MD 11/9/11 - TFS85193
		//    // We now modify the proxy instead of the element
		//    //WorkbookFontData font = (WorkbookFontData)manager.ContextStack[ typeof( WorkbookFontData ) ];
		//    WorkbookFontProxy font = (WorkbookFontProxy)manager.ContextStack[typeof(WorkbookFontProxy)];

		//    if (font == null)
		//    {
		//        Utilities.DebugFail("Could not get the font data from the context stack");
		//        return;
		//    }

		//    if (colorInfo.IsDefault == false)
		//    {
		//        // MD 1/17/12 - 12.1 - Cell Format Updates
		//        //font.Color = colorInfo.ResolveColor(manager);
		//        font.ColorInfo = colorInfo.ResolveColorInfo(manager);
		//    }

		//    // MD 9/15/08 - Excel 2007 Format
		//    // All properties should be resolved when loading
		//    // MD 11/9/11 - TFS85193
		//    // We now modify the proxy instead of the element
		//    //font.SetFontFormatting( font.ResolvedFontData() );
		//    // MD 1/17/12 - 12.1 - Cell Format Updates
		//    IWorkbookFontDefaultsResolver fontDefaultsResolver = (IWorkbookFontDefaultsResolver)manager.ContextStack[typeof(IWorkbookFontDefaultsResolver)];
		//    font.SetFontFormatting(font.Element.ResolvedFontData(fontDefaultsResolver));
		//}
		//#endregion //OnAfterLoadChildElements

		#endregion // Moved

        #region Save

        protected override void Save(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value)
        {
			// MD 11/3/10 - TFS49093
			// The formatted string data is now stored on the FormattedStringElement.
			//FormattedString formattedString = (FormattedString)manager.ContextStack[ typeof( FormattedString ) ];
			FormattedStringElement formattedString = (FormattedStringElement)manager.ContextStack[typeof(FormattedStringElement)];
            if (formattedString == null)
            {
                Utilities.DebugFail("Could not get the formatted string from the context stack");
                return;
            }

			// MD 11/9/11 - TFS85193
			//ListContext<FormattedStringRun> formattingRuns = (ListContext<FormattedStringRun>)manager.ContextStack[ typeof( ListContext<FormattedStringRun> ) ];
			ListContext<FormattingRunBase> formattingRuns = (ListContext<FormattingRunBase>)manager.ContextStack[typeof(ListContext<FormattingRunBase>)];

            if (formattingRuns == null)
            {
                Utilities.DebugFail("Could not get the list of formatting runs from the context stack");
                return;
            }

			// MD 9/26/08
			// This is duplicate code. Use the UnformattedString property of the formatting run instead.
            //int runLength = -1;
            //FormattedStringRun currentRun = formattingRuns.ConsumeCurrentItem() as FormattedStringRun;
            //if (formattingRuns.CurrentItem != null)
            //    runLength = ((FormattedStringRun)formattingRuns.CurrentItem).FirstFormattedChar - currentRun.FirstFormattedChar;
			//
			//string text = runLength == -1 ?
			//    formattedString.UnformattedString.Substring(currentRun.FirstFormattedChar) :
			//    formattedString.UnformattedString.Substring(currentRun.FirstFormattedChar, runLength);
			// MD 11/9/11 - TFS85193
			//FormattingRun currentRun = formattingRuns.ConsumeCurrentItem() as FormattingRun;
			FormattedStringRun currentRun = formattingRuns.ConsumeCurrentItem() as FormattedStringRun;
			string text = currentRun.UnformattedString;
			Debug.Assert( String.IsNullOrEmpty( text ) == false, "The unformatted string of a run should not be empty." );

            // We don't need to serialize a properties element if the font has the default data
			// MD 11/9/11 - TFS85193
			// MD 1/29/12 - 12.1 - Cell Format Updates
			// Always write runs which aren't the first run. Also, the font could have all default properties, but it could have resolved to a 
			// non-default font, so base this on the save index not being 0 (the index of the default font).
			//if (currentRun.Font.HasDefaultValue == false)
			WorkbookFontProxy fontProxy = currentRun.GetFontInternal(manager.Workbook);
			if (currentRun.FirstFormattedCharAbsolute != 0 || fontProxy.SaveIndex != 0)
            {
                // Push the font onto the context stack so that the various child elements can serialize
				// MD 11/9/11 - TFS85193
				//int indexInCollection = currentRun.Font.Element.IndexInFontCollection;
				// MD 1/18/12 - 12.1 - Cell Format Updates
				//int indexInCollection = fontProxy.Element.GetIndexInFontCollection(FontResolverType.Normal);
				int indexInCollection = fontProxy.SaveIndex;

                if (manager.Fonts.Count > indexInCollection)
                {
					// MD 1/31/12 - TFS100573
                    //manager.ContextStack.Push(manager.Fonts[indexInCollection]);
					WorkbookFontData font = manager.Fonts[indexInCollection];
					manager.ContextStack.Push(font);

					// MD 1/17/12 - 12.1 - Cell Format Updates
					//// Push a ColorInfo object onto the stack so that it can be populated and we can read it in later
					//ColorInfo info = new ColorInfo();
					////
					//// Roundtrip - Populate the remaining attributes that we need to store in the Load
					//// MD 11/9/11 - TFS85193
					////info.RGB = currentRun.Font.Color;
					//info.RGB = fontProxy.Element.Color;
					// MD 1/31/12 - TFS100573
					//ColorInfo info = ColorInfo.CreateColorInfo(fontProxy.Element.ColorInfo);
					ColorInfo info = ColorInfo.CreateColorInfo(manager, font.ColorInfo, ColorableItem.CellFont);

                    manager.ContextStack.Push(info);

                    // Add the 'rPR' element
                    XmlElementBase.AddElement(element, RichTextRunPropertiesElement.QualifiedName);
                }
                else
                    Utilities.DebugFail("We were given an index into the fonts collection that was invalid");
            }

            // Add the text to the context stack so that the text element can process it
            manager.ContextStack.Push(text);

            // Add the 't' element, since the 'si' element will not contain it with a run
            XmlElementBase.AddElement(element, TextElement.QualifiedName);
        }
        #endregion Save

        #endregion Base class overrides

		// MD 11/4/10 - TFS49093
		#region SaveDirectHelper

		public static void SaveDirectHelper(
			Excel2007WorkbookSerializationManager manager,
			XmlWriter writer,
			FormattedStringRun run)
		{
			writer.WriteStartElement(RichTextRunElement.LocalName);

			// MD 11/9/11 - TFS85193
			//if (run.Font.HasDefaultValue == false)
			//{
			//    WorkbookFontData fontData = manager.Fonts[run.Font.Element.IndexInFontCollection];
			//    RichTextRunPropertiesElement.SaveDirectHelper(writer, fontData);
			//}
			WorkbookFontProxy fontProxy = run.GetFontInternal(manager.Workbook);

			// MD 1/29/12 - 12.1 - Cell Format Updates
			// Always write runs which aren't the first run. Also, the font could have all default properties, but it could have resolved to a 
			// non-default font, so base this on the save index not being 0 (the index of the default font).
			//if (fontProxy.HasDefaultValue == false)
			if (run.FirstFormattedCharAbsolute != 0 || fontProxy.SaveIndex != 0)
			{
				// MD 1/18/12 - 12.1 - Cell Format Updates
				//WorkbookFontData fontData = manager.Fonts[fontProxy.Element.GetIndexInFontCollection(FontResolverType.Normal)];
				WorkbookFontData fontData = manager.Fonts[fontProxy.SaveIndex];
				RichTextRunPropertiesElement.SaveDirectHelper(writer, fontData);
			}

			TextElement.SaveDirectHelper(writer, run.UnformattedString);
			writer.WriteEndElement();
		}

		#endregion // SaveDirectHelper
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