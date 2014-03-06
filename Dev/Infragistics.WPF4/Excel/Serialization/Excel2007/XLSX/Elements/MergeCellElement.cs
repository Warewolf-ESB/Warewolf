using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;
using System.Globalization;




namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements

{
    internal class MergeCellElement : XLSXElementBase
    {
        #region Constants






        public const string LocalName = "mergeCell";






        public const string QualifiedName =
            XLSXElementBase.DefaultXmlNamespace +
            XmlElementBase.NamespaceSeparator +
            MergeCellElement.LocalName;

        private const string RefAttributeName = "ref";

        #endregion //Constants

        #region Base Class Overrides

        #region Type

        public override XLSXElementType Type
        {
            get { return XLSXElementType.mergeCell; }
        }
        #endregion //Type

        #region Load



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        protected override void Load(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode)
        {
            ChildDataItem item = manager.ContextStack[typeof(ChildDataItem)] as ChildDataItem;
            if (item == null)
            {
                Utilities.DebugFail("Could not get the ChildDataItem from the ContextStack");
                return;
            }

            Worksheet worksheet = item.Data as Worksheet;
            if (worksheet == null)
            {
                Utilities.DebugFail("Could not get the worksheet from the ContextStack");
                return;
            }
            
            foreach (ExcelXmlAttribute attribute in element.Attributes)
            {
                string attributeName = XmlElementBase.GetQualifiedAttributeName(attribute);
                switch (attributeName)
                {
                    case MergeCellElement.RefAttributeName:
                        string address = (string)XmlElementBase.GetValue(attribute.Value, DataType.ST_Ref, String.Empty);

						// MD 4/6/12 - TFS101506
                        //WorksheetRegion region = Utilities.ParseA1RegionAddress(address, worksheet);
						WorksheetRegion region = Utilities.ParseA1RegionAddress(address, worksheet, CultureInfo.InvariantCulture);

                        if (region != null)
                            worksheet.MergedCellsRegions.Add(region.FirstRow, region.FirstColumn, region.LastRow, region.LastColumn);
                        else
                            Utilities.DebugFail("Unable to parse region from address: " + address);

                        break;

                    default:
                        Utilities.DebugFail("Unknown attibute");
                        break;
                }
            }
        }
        #endregion //Load

        #region Save



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

        protected override void Save(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value)
        {
            Worksheet worksheet = manager.ContextStack[typeof(Worksheet)] as Worksheet;
            if (worksheet == null)
            {
                Utilities.DebugFail("Could not get worksheet from context stack");
                return;
            }

            ListContext<WorksheetMergedCellsRegion> list = manager.ContextStack[typeof(ListContext<WorksheetMergedCellsRegion>)] as ListContext<WorksheetMergedCellsRegion>;
            if (list == null)
            {
                Utilities.DebugFail("Unable to get merged cell region from context stack");
                return;
            }

            WorksheetMergedCellsRegion region = list.ConsumeCurrentItem() as WorksheetMergedCellsRegion;
            string refRegion = XmlElementBase.GetXmlString(region.ToString(CellReferenceMode.A1, false, true, true), DataType.ST_Ref);
            XmlElementBase.AddAttribute(element, MergeCellElement.RefAttributeName, refRegion);
        }
        #endregion //Save

        #endregion //Base Class Overrides
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