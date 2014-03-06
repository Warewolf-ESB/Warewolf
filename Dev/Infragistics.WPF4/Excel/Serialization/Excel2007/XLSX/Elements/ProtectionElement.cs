using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;




using Infragistics.Documents.Excel.Serialization.Excel2007.SharedContentTypes;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements

{
    class ProtectionElement : XLSXElementBase
    {
        #region Constants

        public const string LocalName = "protection";

        public const string QualifiedName =
            XLSXElementBase.DefaultXmlNamespace +
            XmlElementBase.NamespaceSeparator +
            ProtectionElement.LocalName;

        public const string HiddenAttributeName = "hidden";
        public const string LockedAttributeName = "locked";

        #endregion Constants

        #region Base Class Overrides

        #region Type

        /// <summary>
        /// Returns the <see cref="XLSXElementType">XLSXElementType</see> constant which identifies this element.
        /// </summary>
        public override XLSXElementType Type
        {
            get { return XLSXElementType.protection; }
        }
        #endregion Type

        #region Load



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        protected override void Load(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode)
        {
            ChildDataItem childDataItem = (ChildDataItem)manager.ContextStack[typeof(ChildDataItem)];
            if (childDataItem == null)
            {
                Utilities.DebugFail("Failed to find the ChildDataItem object on the stack.");
                return;
            }

            ProtectionInfo protectionInfo = new ProtectionInfo();

            FormatInfo formatInfo = childDataItem.Data as FormatInfo;
            if (formatInfo != null)
                formatInfo.Protection = protectionInfo;

            // handles when the parent is a dxf element
            DxfInfo dxfInfo = childDataItem.Data as DxfInfo;
            if (dxfInfo != null)
                dxfInfo.Protection = protectionInfo;

            foreach (ExcelXmlAttribute attribute in element.Attributes)
            {
                string attributeName = XLSXElementBase.GetQualifiedAttributeName(attribute);

                switch (attributeName)
                {
                    case ProtectionElement.HiddenAttributeName:
                        protectionInfo.Hidden = (bool)XLSXElementBase.GetAttributeValue(attribute, DataType.Boolean, false);
                        break;
                    case ProtectionElement.LockedAttributeName:
                        protectionInfo.Locked = (bool)XLSXElementBase.GetAttributeValue(attribute, DataType.Boolean, true);
                        break;
                    default:
                        Utilities.DebugFail("Unknown attribute type in the protection element: " + attributeName);
                        break;
                }
            }
        }

        #endregion Load

        #region Save



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

        protected override void Save(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value)
        {
            ProtectionInfo protectionInfo = (ProtectionInfo)manager.ContextStack[typeof(ProtectionInfo)];
            if (protectionInfo != null)
            {
                if (protectionInfo.Hidden != ProtectionInfo.DEFAULT_HIDDEN)
                    XLSXElementBase.AddAttribute(element, ProtectionElement.HiddenAttributeName, XLSXElementBase.GetXmlString(true, DataType.Boolean));
                if (protectionInfo.Locked != ProtectionInfo.DEFAULT_LOCKED)
                    XLSXElementBase.AddAttribute(element, ProtectionElement.LockedAttributeName, XLSXElementBase.GetXmlString(false, DataType.Boolean));
            }
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