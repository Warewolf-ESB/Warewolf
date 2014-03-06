using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements
{
    internal class StopElement : XLSXElementBase
    {
        #region Constants

        public const string LocalName = "stop";

        public const string QualifiedName =
            XLSXElementBase.DefaultXmlNamespace +
            XmlElementBase.NamespaceSeparator +
            StopElement.LocalName;

        public const string PositionAttributeName = "position";

        #endregion Constants

        #region Base Class Overrides

        #region Type

        /// <summary>
        /// Returns the <see cref="XLSXElementType">XLSXElementType</see> constant which identifies this element.
        /// </summary>
        public override XLSXElementType Type
        {
            get { return XLSXElementType.stop; }
        }
        #endregion Type

        #region Load

        protected override void Load(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode)
        {

            ListContext<StopInfo> listContext = (ListContext<StopInfo>)manager.ContextStack[typeof(ListContext<StopInfo>)];
            if (listContext == null)
            {
                Utilities.DebugFail("Failed to find the ListContext on the Context Stack.");
                return;
            }
            StopInfo stopInfo = new StopInfo();

            foreach (ExcelXmlAttribute attribute in element.Attributes)
            {
                string attributeName = XLSXElementBase.GetQualifiedAttributeName(attribute);

                switch (attributeName)
                {
                    case StopElement.PositionAttributeName:
                        stopInfo.Position = (double)XLSXElementBase.GetAttributeValue(attribute, DataType.Double, -1);
                        break;
                    default:
                        Utilities.DebugFail("Unknown attribute type in the stop element: " + attributeName);
                        break;
                }
            }

            listContext.AddItem(stopInfo);

			manager.ContextStack.Push(stopInfo);
            manager.ContextStack.Push(new ColorInfo());
        }

        #endregion Load

		#region OnAfterLoadChildElements

		protected override void OnAfterLoadChildElements(Excel2007WorkbookSerializationManager manager, ElementDataCache elementCache)
		{
			StopInfo stopInfo = (StopInfo)manager.ContextStack[typeof(StopInfo)];
			if (stopInfo == null)
			{
				Utilities.DebugFail("For some reason, the StopInfo was removed from the context stack");
				return;
			}

			ColorInfo colorInfo = (ColorInfo)manager.ContextStack[typeof(ColorInfo)];

			if (colorInfo == null)
			{
				Utilities.DebugFail("For some reason, the ColorInfo was removed from the context stack");
				return;
			}

			stopInfo.ColorInfo = colorInfo;
		}

		#endregion OnAfterLoadChildElements

        #region Save

        protected override void Save(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value)
        {
            StopInfo stopInfo = null;

            ListContext<StopInfo> listContext = manager.ContextStack[typeof(ListContext<StopInfo>)] as ListContext<StopInfo>;
            if (listContext != null)
                stopInfo = listContext.ConsumeCurrentItem() as StopInfo;

            if (stopInfo == null)
                stopInfo = (StopInfo)manager.ContextStack[typeof(StopInfo)];

            if (stopInfo == null)
            {
                Utilities.DebugFail("StopInfo object not found on the context stack");
                return;
            }

            XLSXElementBase.AddAttribute(element, StopElement.PositionAttributeName, XLSXElementBase.GetXmlString(stopInfo.Position, DataType.Double));

            manager.ContextStack.Push(stopInfo.ColorInfo);
            XLSXElementBase.AddElement(element, ColorElement.QualifiedName);
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