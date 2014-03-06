using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Xml;
using Infragistics.Documents.Excel.Serialization.Excel2007.SharedContentTypes;




using System.Drawing;


namespace Infragistics.Documents.Excel.Serialization.Excel2007.SharedElements
{



    internal class SrgbClrElement : XmlElementBase
    {
        #region Constants

        public const string LocalName = "srgbClr";

        public const string QualifiedName =
            ThemePart.DefaultNamespace +
            XmlElementBase.NamespaceSeparator +
            SrgbClrElement.LocalName;

        public const string ValAttributeName = "val";

        #endregion Constants

        #region Base Class Overrides

        #region ElementName

        public override string ElementName
        {
            get { return SrgbClrElement.QualifiedName; }
        }

        #endregion ElementName

        #region Load

        protected override void Load(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode)
        {
			// MD 8/23/11 - TFS84306
			// Refactored to support this element being under multiple parent elements.
			#region Old Code

			////  BF 9/9/08
			//#region Drawings part specific
			//WorkbookFontProxy font = manager.ContextStack[typeof(WorkbookFontProxy)] as WorkbookFontProxy;
			//if (font != null)
			//{
			//    string clrStringValue = element.Attributes[SrgbClrElement.ValAttributeName].Value;
			//    font.Color = Utilities.ColorFromArgb(Utilities.FromHexBinary3(clrStringValue));
			//    return;
			//}
			//#endregion Drawings part specific

			//ChildDataItem dataItem = (ChildDataItem)manager.ContextStack[typeof(ChildDataItem)] as ChildDataItem;

			//if (dataItem == null)
			//{
			//    Utilities.DebugFail("ChildDataItem object not found on the ContextStack.");
			//    return;
			//}

			//string val = (string)XmlElementBase.GetAttributeValue(element.Attributes[SrgbClrElement.ValAttributeName], DataType.String, String.Empty);
			//if (val == string.Empty)
			//{
			//    Utilities.DebugFail("val Attribute not properly set.");
			//    return;
			//}

			//Color color = Utilities.ColorEmpty;
			//try
			//{
			//    color = Utilities.ColorFromHtml("#" + val);
			//}
			//catch (Exception ex)
			//{
			//    Utilities.DebugFail("Unable to convert val attribute to a valid color. " + ex.Message);
			//    return;
			//}

			//dataItem.Data = color;

			#endregion  // Old Code
            ChildDataItem dataItem = (ChildDataItem)manager.ContextStack[typeof(ChildDataItem)];

            if (dataItem == null)
            {
                Utilities.DebugFail("ChildDataItem object not found on the ContextStack.");
                return;
            }

			int val = (int)XmlElementBase.GetAttributeValue(element.Attributes[SrgbClrElement.ValAttributeName], DataType.ST_HexBinary3, 0);
			val += unchecked((int)0xFF000000);
			dataItem.Data = Utilities.ColorFromArgb(val);
        }

        #endregion Load

        #region Save

        protected override void Save(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value)
        {
			Utilities.DebugFail("Save() method for SrgbClr element not yet implemented.");

			// MD 8/23/11 - TFS84306
			// Started implementing this method, but it is not actually used yet, so I left it commented out.
			//ChildDataItem dataItem = (ChildDataItem)manager.ContextStack[typeof(ChildDataItem)];
			//if (dataItem == null)
			//{
			//    Utilities.DebugFail("ChildDataItem object not found on the ContextStack.");
			//    return;
			//}

			//string attributeValue = XmlElementBase.GetXmlString(dataItem.Data, DataType.ST_HexBinary3);
			//XmlElementBase.AddAttribute(element, SrgbClrElement.ValAttributeName, attributeValue);
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