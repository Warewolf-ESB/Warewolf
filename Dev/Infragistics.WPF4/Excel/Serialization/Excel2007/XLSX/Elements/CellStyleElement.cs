using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements
{
    internal class CellStyleElement : XLSXElementBase
    {
        #region Constants

        public const string LocalName = "cellStyle";

        public const string QualifiedName =
            XLSXElementBase.DefaultXmlNamespace +
            XmlElementBase.NamespaceSeparator +
            CellStyleElement.LocalName;

        public const string BuiltinIdAttributeName = "builtinId";
        public const string CustomBuiltInIdAttributeName = "customBuiltin";
        public const string HiddenAttributeName = "hidden";
        public const string OutlineStyleAttributeName = "iLevel";
        public const string NameAttributeName = "name";
        public const string XfIdAttributeName = "xfId";

        #endregion Constants

        #region Base Class Overrides

        #region Type

        /// <summary>
        /// Returns the <see cref="XLSXElementType">XLSXElementType</see> constant which identifies this element.
        /// </summary>
        public override XLSXElementType Type
        {
            get { return XLSXElementType.cellStyle; }
        }
        #endregion Type

        #region Load



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        protected override void Load(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode)
        {
            ListContext<StyleInfo> listContext = (ListContext<StyleInfo>)manager.ContextStack[typeof(ListContext<StyleInfo>)];
            if (listContext == null)
            {
                Utilities.DebugFail("List for StyleInfo was not found on the ContextStack.");
                return;
            }

            StyleInfo styleInfo = new StyleInfo();

            foreach (ExcelXmlAttribute attribute in element.Attributes)
            {
                string attributeName = XLSXElementBase.GetQualifiedAttributeName(attribute);

                switch (attributeName)
                {
                    case CellStyleElement.BuiltinIdAttributeName:
                        styleInfo.BuiltinId = (int)XLSXElementBase.GetAttributeValue(attribute, DataType.Int32, 0);
                        break;
                    case CellStyleElement.CustomBuiltInIdAttributeName:
                        styleInfo.CustomBuiltin = (bool)XLSXElementBase.GetAttributeValue(attribute, DataType.Boolean, false);
                        break;
                    case CellStyleElement.HiddenAttributeName:
                        styleInfo.Hidden  = (bool)XLSXElementBase.GetAttributeValue(attribute, DataType.Boolean, false);
                        break;
                    case CellStyleElement.OutlineStyleAttributeName:
                        styleInfo.OutlineStyle = (int)XLSXElementBase.GetAttributeValue(attribute, DataType.Int32, 0);
                        break;
                    case CellStyleElement.NameAttributeName:
                        styleInfo.Name = (string)XLSXElementBase.GetAttributeValue(attribute, DataType.String, string.Empty);
                        break;
                    case CellStyleElement.XfIdAttributeName:
                        styleInfo.CellStyleXfId = (int)XLSXElementBase.GetAttributeValue(attribute, DataType.Int32, 0);
                        break;
                    default:
                        Utilities.DebugFail("Unknown attribute type in the cellStyle element: " + attributeName);
                        break;
                }
            }

            listContext.AddItem(styleInfo);

			// MD 1/17/11 - TFS62014
			// We won't have the resolved format data objects, which are needed for this step, until after the entire styles.xml file is processed,
			// so this must be deferred. I have moved this code into the StyleSheetElement.OnAfterLoadChildElements method, which all called after 
			// the entire file is loaded.
			//if (styleInfo.CellStyleXfId > -1 &&
			//    styleInfo.CellStyleXfId < manager.CellStyleXfs.Count)
			//{
			//    if (styleInfo.BuiltinId >= 0)
			//    {
			//        WorkbookBuiltInStyle workbookStyle = new WorkbookBuiltInStyle(manager.Workbook, manager.CellStyleXfs[styleInfo.CellStyleXfId].FormatDataObject, (BuiltInStyleType)styleInfo.BuiltinId, Convert.ToByte(styleInfo.OutlineStyle));
			//        manager.Workbook.Styles.Add(workbookStyle);
			//    }
			//    else
			//        manager.Workbook.Styles.AddUserDefinedStyle(manager.CellStyleXfs[styleInfo.CellStyleXfId].FormatDataObject, styleInfo.Name);
			//}
        }


        #endregion Load

        #region Save



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

        protected override void Save(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value)
        {
            StyleInfo styleInfo = null;

            ListContext<StyleInfo> listContext = manager.ContextStack[typeof(ListContext<StyleInfo>)] as ListContext<StyleInfo>;
            if (listContext != null)
                styleInfo = listContext.ConsumeCurrentItem() as StyleInfo;

            if (styleInfo == null)
                styleInfo = (StyleInfo)manager.ContextStack[typeof(StyleInfo)];

            if (styleInfo == null)
            {
                Utilities.DebugFail("StyleInfo object not found on the ContextStack.");
                return;
            }

            if (styleInfo.BuiltinId > -1)
                XLSXElementBase.AddAttribute(element, CellStyleElement.BuiltinIdAttributeName, XLSXElementBase.GetXmlString(styleInfo.BuiltinId, DataType.Int32));
            if (styleInfo.CustomBuiltin)
                XLSXElementBase.AddAttribute(element, CellStyleElement.CustomBuiltInIdAttributeName, XLSXElementBase.GetXmlString(styleInfo.CustomBuiltin, DataType.Boolean));
            if (styleInfo.Hidden)
                XLSXElementBase.AddAttribute(element, CellStyleElement.HiddenAttributeName, XLSXElementBase.GetXmlString(styleInfo.Hidden, DataType.Boolean));
            // 8/27/08 CDS - Changed the default from -1 to 0 due to the conversion to a byte. Only write this attribute out if its greater than 0.
            //if (styleInfo.OutlineStyle > -1)
			// MD 1/1/12 - 12.1 - Cell Format Updates
            //if (styleInfo.OutlineStyle > 0)
			if (styleInfo.OutlineStyle > 0 &&
				(styleInfo.BuiltinId == (int)BuiltInStyleType.ColLevelX || styleInfo.BuiltinId == (int)BuiltInStyleType.RowLevelX))
			{
				XLSXElementBase.AddAttribute(element, CellStyleElement.OutlineStyleAttributeName, XLSXElementBase.GetXmlString(styleInfo.OutlineStyle, DataType.Int32));
			}
            if (styleInfo.Name.Length > 0)
                XLSXElementBase.AddAttribute(element, CellStyleElement.NameAttributeName, XLSXElementBase.GetXmlString(styleInfo.Name, DataType.String));

            XLSXElementBase.AddAttribute(element, CellStyleElement.XfIdAttributeName, XLSXElementBase.GetXmlString(styleInfo.CellStyleXfId, DataType.Int32));
        }

        #endregion Save

        #endregion Base Class Overrides
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