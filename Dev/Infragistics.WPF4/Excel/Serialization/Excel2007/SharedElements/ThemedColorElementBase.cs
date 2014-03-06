using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Xml;




using System.Drawing;


namespace Infragistics.Documents.Excel.Serialization.Excel2007.SharedElements
{
    internal abstract class ThemedColorElementBase : XmlElementBase
    {
        #region Base Class Overrides

        #region Load

        protected override void Load(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode)
        {
            manager.ContextStack.Push(new ChildDataItem());
        }

        #endregion Load

        #region Save

        protected override void Save(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value)
        {
            Utilities.DebugFail("Save() method for themedColorElementBase element not yet implemented.");
        }

        #endregion Save

        #region OnAfterLoadChildElements

		protected override void OnAfterLoadChildElements( Excel2007WorkbookSerializationManager manager, ElementDataCache elementCache )
        {
			// MD 1/16/12 - 12.1 - Cell Format Updates
			// This will no longer be on the context stack.
			//ListContext<Color> listContext = (ListContext<Color>)manager.ContextStack[typeof(ListContext<Color>)] as ListContext<Color>;
			//
			//if (listContext == null)
			//{
			//    Utilities.DebugFail("ListContext object not found on the ContextStack.");
			//    return;
			//}

            ChildDataItem dataItem = (ChildDataItem)manager.ContextStack[typeof(ChildDataItem)];
            if (dataItem == null ||
                dataItem.Data == null)
            {
                Utilities.DebugFail("Color object from child element not found on the ContextStack.");
                return;
            }

            Color color = (Color)dataItem.Data;

			// MD 1/16/12 - 12.1 - Cell Format Updates
			// Populate the ThemeColors collection directly.
            //listContext.AddItem(color);
			manager.Workbook.ThemeColors[(int)this.ThemeColorType] = color;

            #region ColorSchemeInfo is no longer used. Delete this section once the themes are figured out.

            //ColorSchemeInfo schemeInfo = (ColorSchemeInfo)manager.ContextStack[typeof(ColorSchemeInfo)] as ColorSchemeInfo;

            //if (schemeInfo == null)
            //{
            //    Utilities.DebugFail("ColorSchemeInfo object not found on the ContextStack.");
            //    return;
            //}

            //ChildDataItem dataItem = (ChildDataItem)manager.ContextStack[typeof(ChildDataItem)];
            //if (dataItem == null ||
            //    dataItem.Data == null)
            //{
            //    Utilities.DebugFail("Color object from child element not found on the ContextStack.");
            //    return;
            //}

            //Color color = (Color)dataItem.Data;

            //// assign the color to the approriate property based on the current "type"

            //if (this.PropertyOnColorSchemeInfo != string.Empty)
            //{
            //    System.Reflection.PropertyInfo pInfo = schemeInfo.GetType().GetProperty(this.PropertyOnColorSchemeInfo);
            //    if (pInfo != null)
            //        pInfo.SetValue(schemeInfo, color, null);
            //}

            //#region Another way to set the properties without reflection

            ////switch (this.ElementName)
            ////{
            ////    case Accent1Element.QualifiedName:
            ////        schemeInfo.Accent1 = color;
            ////        break;
            ////    case Accent2Element.QualifiedName:
            ////        schemeInfo.Accent2 = color;
            ////        break;
            ////    case Accent3Element.QualifiedName:
            ////        schemeInfo.Accent3 = color;
            ////        break;
            ////    case Accent4Element.QualifiedName:
            ////        schemeInfo.Accent4 = color;
            ////        break;
            ////    case Accent5Element.QualifiedName:
            ////        schemeInfo.Accent5 = color;
            ////        break;
            ////    case Accent6Element.QualifiedName:
            ////        schemeInfo.Accent6 = color;
            ////        break;
            ////    case Dk1Element.QualifiedName:
            ////        schemeInfo.Dark1 = color;
            ////        break;
            ////    case Dk2Element.QualifiedName:
            ////        schemeInfo.Dark2 = color;
            ////        break;
            ////    case FolHlinkElement.QualifiedName:
            ////        schemeInfo.FollowedHyperlink = color;
            ////        break;
            ////    case HlinkElement.QualifiedName:
            ////        schemeInfo.Hyperlink = color;
            ////        break;
            ////    case Lt1Element.QualifiedName:
            ////        schemeInfo.Light1 = color;
            ////        break;
            ////    case Lt2Element.QualifiedName:
            ////        schemeInfo.Light2 = color;
            ////        break;
            ////}

            //#endregion

            #endregion

        }

        #endregion OnAfterLoadChildElements

        #endregion Base Class Overrides

		// MD 1/16/12 - 12.1 - Cell Format Updates
		protected abstract WorkbookThemeColorType ThemeColorType { get; }
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