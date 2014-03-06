using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Xml;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements
{
    internal class ColumnElement: XLSXElementBase
    {
        #region XML Schema Fragment

        //<complexType name="CT_Col"> 
        //  <attribute name="min" type="xsd:unsignedInt" use="required"/> 
        //  <attribute name="max" type="xsd:unsignedInt" use="required"/> 
        //  <attribute name="width" type="xsd:double" use="optional"/> 
        //  <attribute name="style" type="xsd:unsignedInt" use="optional" default="0"/> 
        //  <attribute name="hidden" type="xsd:boolean" use="optional" default="false"/> 
        //  <attribute name="bestFit" type="xsd:boolean" use="optional" default="false"/> 
        //  <attribute name="customWidth" type="xsd:boolean" use="optional" default="false"/> 
        //  <attribute name="phonetic" type="xsd:boolean" use="optional" default="false"/> 
        //  <attribute name="outlineLevel" type="xsd:unsignedByte" use="optional" default="0"/> 
        //  <attribute name="collapsed" type="xsd:boolean" use="optional" default="false"/> 
        //</complexType> 

        #endregion //Xml Schema Fragment

        #region Constants






        public const string LocalName = "col";






        public const string QualifiedName =
            XLSXElementBase.DefaultXmlNamespace +
            XmlElementBase.NamespaceSeparator +
            ColumnElement.LocalName;

        private const string MinAttributeName = "min";
        private const string MaxAttributeName = "max";
        private const string WidthAttributeName = "width";
        private const string StyleAttributeName = "style";
        private const string HiddenAttributeName = "hidden";
        private const string BestFitAttributeName = "bestFit";
        private const string CustomWidthAttributeName = "customWidth";
        private const string PhoneticAttributeName = "phonetic";
        private const string OutlineLevelAttributeName = "outlineLevel";
        private const string CollapsedAttributeName = "collapsed";

        #endregion //Constants

        #region Base Class Overrides

        #region Type






        public override XLSXElementType Type
        {
            get { return XLSXElementType.col; }
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

            int min = 0, max = 0;

			// MD 2/10/12 - TFS97827
			// The DefaultColumnWidth is now the final value. Nothing needs to be resolved.
			//int width = worksheet.DefaultColumnWidthResolved;
			int width = worksheet.DefaultColumnWidth;

            bool hidden = false;                     
            byte outlineLevel = 0;
            int style = -1;
			bool customWidth = false;

            // Roundtrip
            //bool bestFit = false;
            //bool phonetic = false;

            foreach (ExcelXmlAttribute attribute in element.Attributes)
            {
                string attributeName = XmlElementBase.GetQualifiedAttributeName(attribute);
                switch (attributeName)
                {
                    case ColumnElement.MinAttributeName:
                        // The column index is 1-based and we are 0-based
                        min = (int)XmlElementBase.GetValue(attribute.Value, DataType.Integer, 0) - 1;
                        break;

                    case ColumnElement.MaxAttributeName:
                        // The column index is 1-based and we are 0-based
                        max = (int)XmlElementBase.GetValue(attribute.Value, DataType.Integer, 0) - 1;
                        break;

                    case ColumnElement.WidthAttributeName:
                        // We need to multiply this by 256 to accurately put this into the units that we'll use
						double zeroCharacterWidths = (double)XmlElementBase.GetValue( attribute.Value, DataType.Double, 0 );

						// MD 3/16/12 - TFS105094
						// MidpointRoundingAwayFromZero now returns a double.
                        //width = Utilities.MidpointRoundingAwayFromZero(256 * zeroCharacterWidths);
						width = (int)MathUtilities.MidpointRoundingAwayFromZero(256 * zeroCharacterWidths);
                        break;

                    case ColumnElement.StyleAttributeName:
                        style = (int)XmlElementBase.GetValue(attribute.Value, DataType.Integer, 0);
                        break;

                    case ColumnElement.HiddenAttributeName:
                        hidden = (bool)XmlElementBase.GetValue(attribute.Value, DataType.Boolean, false);
                        break;

                    case ColumnElement.BestFitAttributeName:
                        // Roundtrip - Page 1939
                        //bestFit = (bool)XmlElementBase.GetValue(attribute.Value, DataType.Boolean, false);
                        break;

                    case ColumnElement.CustomWidthAttributeName:
                        customWidth = (bool)XmlElementBase.GetValue(attribute.Value, DataType.Boolean, false); ;
                        break;

                    case ColumnElement.PhoneticAttributeName:
                        // Roundtrip - Page 1940
                        //phonetic = (bool)XmlElementBase.GetValue(attribute.Value, DataType.Boolean, false);
                        break;

                    case ColumnElement.OutlineLevelAttributeName:
                        outlineLevel = (byte)XmlElementBase.GetValue(attribute.Value, DataType.Byte, 0);
                        break;

                    case ColumnElement.CollapsedAttributeName:
                        // We can ignore this attribute since we're going to re-calculate it anyway in the Save method
                        break;

                    default:
                        Utilities.DebugFail("Encountered an unknown attribute");
                        break;
                }
            }

			bool hasNonDefaultData = false;

			if ( customWidth )
			{
				hasNonDefaultData = true;
			}
			else
			{
				// MD 10/24/11 - TFS89375
				// The width is allowed to be different without the customWidth attribute set if the defaultColWidth is not specified.
				//Debug.Assert( width == worksheet.DefaultColumnWidthResolved, "If the width is not custom, it should be the default width." );
				// MD 2/10/12 - TFS97827
				// The DefaultColumnWidth is now the final value. Nothing needs to be resolved.
				//Debug.Assert(worksheet.hasExplicitlyLoadedDefaultColumnWidth == false || width == worksheet.DefaultColumnWidthResolved, 
				Debug.Assert(worksheet.hasExplicitlyLoadedDefaultColumnWidth == false || width == worksheet.DefaultColumnWidth, 
					"If the width is not custom, it should be the default width.");

				// MD 2/10/12 - TFS97827
				// The DefaultColumnWidth is now the final value. Nothing needs to be resolved.
				//if (width != worksheet.DefaultColumnWidthResolved)
				if (width != worksheet.DefaultColumnWidth)
					hasNonDefaultData = true;
			}

			if ( hidden )
				hasNonDefaultData = true;

			if ( outlineLevel != 0 )
				hasNonDefaultData = true;

			WorksheetCellFormatData cellFormatToApply = null;
			if ( style > -1 )
			{
				if ( style < manager.CellXfs.Count )
				{
					WorksheetCellFormatData cellFormat = manager.CellXfs[ style ].FormatDataObject;

					// MD 1/1/12 - 12.1 - Cell Format Updates
					// The default element is now a cell format and not a style format, so we can check for equality.
					//if ( cellFormat.HasSameData( manager.Workbook.CellFormats.DefaultElement ) == false )
					if (cellFormat.EqualsInternal(manager.Workbook.CellFormats.DefaultElement) == false)
					{
						cellFormatToApply = cellFormat;
						hasNonDefaultData = true;
					}
				}
				else
					Utilities.DebugFail( "We have a style index that is greater than the number of CellXfs on the manager" );
			}

			if ( hasNonDefaultData == false )
				return;

			// MD 3/15/12 - TFS104581
			// We now store the column blocks on the worksheet.
			#region Old Code

			//WorksheetColumnCollection columns = worksheet.Columns;
			//
			//// Loop through the columns with the specified range and set the appropriate properties
			//for (int i = min; i <= max; i++)
			//{
			//    WorksheetColumn column = columns[ i ];
			//
			//    column.Width = width;
			//    column.Hidden = hidden;
			//    column.OutlineLevel = outlineLevel;
			//
			//    if ( cellFormatToApply != null )
			//        column.CellFormat.SetFormatting( cellFormatToApply );
			//}

			#endregion // Old Code
			worksheet.OnColumnBlockLoaded((short)min, (short)max, width, hidden, outlineLevel, cellFormatToApply);
        }
        #endregion //Load

        #region Save



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

        protected override void Save(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value)
        {
			Worksheet worksheet = (Worksheet)manager.ContextStack[ typeof( Worksheet ) ];

			if ( worksheet == null )
			{
				Utilities.DebugFail( "Could not get the worksheet from the ContextStack" );
				return;
			}

			// MD 3/15/12 - TFS104581
			//ListContext<WorkbookSerializationManager.ColumnBlockInfo> listContext =
			//    manager.ContextStack[typeof(ListContext<WorkbookSerializationManager.ColumnBlockInfo>)] as ListContext<WorkbookSerializationManager.ColumnBlockInfo>;
			ListContext<WorksheetColumnBlock> listContext =
				manager.ContextStack[typeof(ListContext<WorksheetColumnBlock>)] as ListContext<WorksheetColumnBlock>;

            if (listContext == null)
            {
                Utilities.DebugFail("Unable to get the list of column blocks off of the context stack");
                return;
            }

            string attributeValue = null;

			// MD 3/15/12 - TFS104581
            //WorkbookSerializationManager.ColumnBlockInfo block = listContext.ConsumeCurrentItem() as WorkbookSerializationManager.ColumnBlockInfo;
			WorksheetColumnBlock block = listContext.ConsumeCurrentItem();
            
            // Add the 'min' attribute
            attributeValue = XmlElementBase.GetXmlString(block.FirstColumnIndex + 1, DataType.UInt);
            XmlElementBase.AddAttribute(element, ColumnElement.MinAttributeName, attributeValue);

            // Add the 'max' attribute
            attributeValue = XmlElementBase.GetXmlString(block.LastColumnIndex + 1, DataType.UInt);
            XmlElementBase.AddAttribute(element, ColumnElement.MaxAttributeName, attributeValue);

            // Add the 'width' attribute
			double zeroCharacterWidths = 0;

			// MD 10/25/11 - TFS91555
			// We had the wrong check here to see if the block width was valid. 0 is still a valid width. Also, when the block width wasn't valid, 
			// we were writing out 0, which was basically collapsing the columns. We could have excluded the width attribute in that case because 
			// it is optional, but apparently when it is not specified, they assume it is 0 instead of the default column width on the worksheet 
			// (which seems like a bug). So we should always write out the resolved width.
			//if ( block.Width > 0 )
			//    zeroCharacterWidths = block.Width / 256.0;
			if (block.Width >= 0)
			{
				zeroCharacterWidths = block.Width / 256.0;
			}
			else
			{
				// MD 2/10/12 - TFS97827
				// The DefaultColumnWidth is now the final value. Nothing needs to be resolved.
				//zeroCharacterWidths = worksheet.DefaultColumnWidthResolved / 256.0;
				zeroCharacterWidths = worksheet.DefaultColumnWidth / 256.0;
			}

			//
			attributeValue = XmlElementBase.GetXmlString( zeroCharacterWidths, DataType.Double );
			XmlElementBase.AddAttribute(element, ColumnElement.WidthAttributeName, attributeValue);

			// MD 3/15/12 - TFS104581
			#region Old Code

			//// Add the 'style' attribute
			//// 09/18/08 CDS FormatIndex indexed into the Format collection which contains both styles and formats,
			//// Therefore, we should use the IndexInXfsCollection off of the WorksheetCellDataFormat object.
			//WorksheetColumn column = worksheet.Columns[block.FirstColumnIndex];
			//// 09/22/08 CDS - To aviod serializing a -1, make sure a non-default format is present
			//if (column.HasCellFormat &&
			//    // MD 3/2/12 - 12.1 - Table Support
			//    //!column.CellFormatInternal.HasDefaultValue)
			//    !column.CellFormatInternal.IsEmpty)
			//{
			//    // MD 1/10/12 - 12.1 - Cell Format Updates
			//    // We no longer cache format indexes because we can easily get them at save time.
			//    //attributeValue = XmlElementBase.GetXmlString(column.CellFormatInternal.Element.IndexInXfsCollection, DataType.Integer, 0, false);
			//    attributeValue = XmlElementBase.GetXmlString(manager.GetCellFormatIndex(column.CellFormatInternal.Element), DataType.UShort, (ushort)0, false);
			//
			//    if (attributeValue != null)
			//        XmlElementBase.AddAttribute(element, ColumnElement.StyleAttributeName, attributeValue);
			//}

			#endregion // Old Code
			if (block.CellFormat.IsEmpty == false)
			{
				attributeValue = XmlElementBase.GetXmlString(manager.GetCellFormatIndex(block.CellFormat), DataType.UShort, (ushort)0, false);
				if (attributeValue != null)
					XmlElementBase.AddAttribute(element, ColumnElement.StyleAttributeName, attributeValue);
			}

            // Add the 'hidden' attribute
            attributeValue = XmlElementBase.GetXmlString(block.Hidden, DataType.Boolean, false, false);
            if (attributeValue != null)
                XmlElementBase.AddAttribute(element, ColumnElement.HiddenAttributeName, attributeValue);

            // Roundtrip
            // Add the 'bestFit' attribute

            // Add the 'customWidth' attribute
			// MD 10/25/11 - TFS91555
			// If the block width isn't valid, it will differ from the default, but we shouldn't consider that a custom width, so 
			// also check that the block width is valid.
			//bool customWidth = block.Width != worksheet.DefaultColumnWidthResolved;
			// MD 2/10/12 - TFS97827
			// The DefaultColumnWidth is now the final value. Nothing needs to be resolved.
			//bool customWidth = block.Width >= 0 && block.Width != worksheet.DefaultColumnWidthResolved;
			bool customWidth = block.Width >= 0 && block.Width != worksheet.DefaultColumnWidth;

			attributeValue = XmlElementBase.GetXmlString( customWidth, DataType.Boolean, false, false );
			if ( attributeValue != null )
				XmlElementBase.AddAttribute( element, ColumnElement.CustomWidthAttributeName, attributeValue );

            // Roundtrip
            // Add the 'phonetic' attribute

            // Add the 'outlineLevel' attribute
			// MD 11/19/08 - TFS10637
			// The OutlineLevel property is returned as an int and cannot be unboxed to a byte. It has to be casted first.
            //attributeValue = XmlElementBase.GetXmlString(block.OutlineLevel, DataType.Byte, 0, false);
			// MD 1/28/09 - TFS12701
			// Now that the value is casted to a byte, the default value must be casted as well.
			//attributeValue = XmlElementBase.GetXmlString( (byte)block.OutlineLevel, DataType.Byte, 0, false );
			attributeValue = XmlElementBase.GetXmlString( (byte)block.OutlineLevel, DataType.Byte, (byte)0, false );

            if (attributeValue != null)
                XmlElementBase.AddAttribute(element, ColumnElement.OutlineLevelAttributeName, attributeValue);

            // Add the 'collapsed' attribute
            attributeValue = XmlElementBase.GetXmlString(block.HasCollapseIndicator, DataType.Boolean, false, false);
            if (attributeValue != null)
                XmlElementBase.AddAttribute(element, ColumnElement.CollapsedAttributeName, attributeValue);
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