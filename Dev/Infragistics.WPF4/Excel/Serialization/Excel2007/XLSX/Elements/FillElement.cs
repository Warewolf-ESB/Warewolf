using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements
{
   internal class FillElement : XLSXElementBase
    {
        #region Constants

        public const string LocalName = "fill";

        public const string QualifiedName =
            XLSXElementBase.DefaultXmlNamespace +
            XmlElementBase.NamespaceSeparator +
            FillElement.LocalName;

        #endregion Constants

        #region Private Members

        private string parentElementName = string.Empty;

        #endregion Private Members


        #region Base Class Overrides

        #region Type

        /// <summary>
        /// Returns the <see cref="XLSXElementType">XLSXElementType</see> constant which identifies this element.
        /// </summary>
        public override XLSXElementType Type
        {
            get { return XLSXElementType.fill; }
        }
        #endregion Type

        #region Load

        protected override void Load(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode)
        {
            if (element.ParentNode != null)
                this.parentElementName = element.ParentNode.LocalName;

            FillInfo fillInfo = new FillInfo();
            manager.ContextStack.Push(fillInfo);

        }

        #endregion Load

        #region Save

        protected override void Save(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value)
        {
            FillInfo fillInfo = null;
            
            ListContext<FillInfo> listContext = manager.ContextStack[typeof(ListContext<FillInfo>)] as ListContext<FillInfo>;
            if (listContext != null)
                fillInfo = listContext.ConsumeCurrentItem() as FillInfo;

            if (fillInfo == null)
                fillInfo = (FillInfo)manager.ContextStack[typeof(FillInfo)];

            if (fillInfo == null)
            {
                Utilities.DebugFail("FillInfo object not found on the context stack");
                return;
            }

            if (fillInfo.PatternFill != null)
            {
                manager.ContextStack.Push(fillInfo.PatternFill);
                XLSXElementBase.AddElement(element, PatternFillElement.QualifiedName);
            }

            if (fillInfo.GradientFill != null)
            {
                manager.ContextStack.Push(fillInfo.GradientFill);
                XLSXElementBase.AddElement(element, GradientFillElement.QualifiedName);
            }
        }

        #endregion Save

        #region OnAfterLoadChildElements

		protected override void OnAfterLoadChildElements( Excel2007WorkbookSerializationManager manager, ElementDataCache elementCache )
        {
            FillInfo fillInfo = (FillInfo)manager.ContextStack[typeof(FillInfo)];

            if (fillInfo == null)
            {
                Utilities.DebugFail("The FillInfo was removed from the context stack");
                return;
            }
            if (fillInfo.GradientFill == null &&
                fillInfo.PatternFill == null)
            {
                Utilities.DebugFail("The FillInfo was not populated with either a Pattern or Gradient fill");
                return;
            }

            switch (this.parentElementName)
            {
                case FillsElement.LocalName:
                    manager.Fills.Add(fillInfo);
                    break;
                case DxfElement.LocalName:

                    ChildDataItem childDataItem = (ChildDataItem)manager.ContextStack[typeof(ChildDataItem)];
                    if (childDataItem != null)
                    {
                        DxfInfo dxfInfo = childDataItem.Data as DxfInfo;
                        if (dxfInfo != null)
                            dxfInfo.Fill = fillInfo;
                    }
                    break;
                default:
                    
                    Utilities.DebugFail("Fill element for unknown parent element being loaded.");
                    break;
            }
        }

        #endregion OnAfterLoadChildElements

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