using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Xml;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements
{
    internal class PageSetupElement : XLSXElementBase
    {
        #region XML Schema Fragment

        //<complexType name="CT_PageSetup"> 
        //  <attribute name="paperSize" type="xsd:unsignedInt" use="optional" default="1"/> 
        //  <attribute name="scale" type="xsd:unsignedInt" use="optional" default="100"/> 
        //  <attribute name="firstPageNumber" type="xsd:unsignedInt" use="optional" default="1"/> 
        //  <attribute name="fitToWidth" type="xsd:unsignedInt" use="optional" default="1"/> 
        //  <attribute name="fitToHeight" type="xsd:unsignedInt" use="optional" default="1"/> 
        //  <attribute name="pageOrder" type="ST_PageOrder" use="optional" default="downThenOver"/> 
        //  <attribute name="orientation" type="ST_Orientation" use="optional" default="default"/> 
        //  <attribute name="usePrinterDefaults" type="xsd:boolean" use="optional" default="true"/> 
        //  <attribute name="blackAndWhite" type="xsd:boolean" use="optional" default="false"/> 
        //  <attribute name="draft" type="xsd:boolean" use="optional" default="false"/> 
        //  <attribute name="cellComments" type="ST_CellComments" use="optional" default="none"/> 
        //  <attribute name="useFirstPageNumber" type="xsd:boolean" use="optional" default="false"/> 
        //  <attribute name="errors" type="ST_PrintError" use="optional" default="displayed"/> 
        //  <attribute name="horizontalDpi" type="xsd:unsignedInt" use="optional" default="600"/> 
        //  <attribute name="verticalDpi" type="xsd:unsignedInt" use="optional" default="600"/> 
        //  <attribute name="copies" type="xsd:unsignedInt" use="optional" default="1"/> 
        //  <attribute ref="r:id" use="optional"/> 
        //</complexType> 

        #endregion //Xml Schema Fragment

        #region Constants






        public const string LocalName = "pageSetup";






        public const string QualifiedName =
            XLSXElementBase.DefaultXmlNamespace +
            XmlElementBase.NamespaceSeparator +
            PageSetupElement.LocalName;

        private const string PaperSizeAttributeName = "paperSize";
        private const string ScaleAttributeName = "scale";
        private const string FirstPageNumberAttributeName = "firstPageNumber";
        private const string FitToWidthAttributeName = "fitToWidth";
        private const string FitToHeightAttributeName = "fitToHeight";
        private const string PageOrderAttributeName = "pageOrder";
        private const string OrientationAttributeName = "orientation";
        private const string UsePrinterDefaultsAttributeName = "usePrinterDefaults";
        private const string BlackAndWhiteAttributeName = "blackAndWhite";
        private const string DraftAttributeName = "draft";
        private const string CellCommentsAttributeName = "cellComments";
        private const string UseFirstPageNumberAttributeName = "useFirstPageNumber";
        private const string ErrorsAttributeName = "errors";
        private const string HorizontalDpiAttributeName = "horizontalDpi";
        private const string VerticalDpiAttributeName = "verticalDpi";
        private const string CopiesAttributeName = "copies";

        #endregion //Constants

        #region Base Class Overrides

        #region Type






        public override XLSXElementType Type
        {
            get { return XLSXElementType.pageSetup; }
        }
        #endregion //Type

        #region Load

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

            //  BF 8/8/08
            PrintOptions options = manager.ContextStack[typeof(PrintOptions)] as PrintOptions;
            if ( options == null )
            {
                Debug.Assert( false, "Couldn't get expected context." );
                return;
            }
            
            foreach (ExcelXmlAttribute attribute in element.Attributes)
            {
                string attributeName = XmlElementBase.GetQualifiedAttributeName(attribute);
                
                //  BF 8/8/08 (see above)
                //PrintOptions options = worksheet.PrintOptions;                

                switch (attributeName)
                {
                    case PageSetupElement.PaperSizeAttributeName:
                        PaperSize paperSize = (PaperSize)((uint)XmlElementBase.GetValue(attribute.Value, DataType.UInt, 1));

						if (Enum.IsDefined(typeof(PaperSize), paperSize) == false)
						{
							Utilities.DebugFail("The loaded PaperSize value is not defined.");
							break;
						}

                        options.PaperSize = paperSize;
                        break;

                    case PageSetupElement.ScaleAttributeName:
                        int scale = (int)XmlElementBase.GetValue(attribute.Value, DataType.Integer, 100);
						
						// MD 3/15/11 - TFS64430
						// We shouldn't throw errors when loading an invalid scale. 
						// Call SetScalingFactor and pass False so we don't throw an error.
						//options.ScalingFactor = scale;
						options.SetScalingFactor(scale, false);
                        break;                    

                    case PageSetupElement.FirstPageNumberAttributeName:
                        int firstPageNumber = (int)XmlElementBase.GetValue(attribute.Value, DataType.Integer, 1);
                        options.StartPageNumber = firstPageNumber;
                        break;

                    case PageSetupElement.FitToWidthAttributeName:
                        int fitToWidth = (int)XmlElementBase.GetValue(attribute.Value, DataType.Integer, 1);
                        options.MaxPagesHorizontally = fitToWidth;
                        break;

                    case PageSetupElement.FitToHeightAttributeName:
                        int fitToHeight = (int)XmlElementBase.GetValue(attribute.Value, DataType.Integer, 1);
                        options.MaxPagesVertically = fitToHeight;
                        break;

                    case PageSetupElement.PageOrderAttributeName:
                        PageOrder pageOrder = (PageOrder)XmlElementBase.GetValue(attribute.Value, DataType.ST_PageOrder, PageOrder.DownThenOver);
                        options.PageOrder = pageOrder;
                        break;

                    case PageSetupElement.OrientationAttributeName:
                        Orientation orientation = (Orientation)XmlElementBase.GetValue(attribute.Value, DataType.ST_Orientation, Orientation.Default);
                        options.Orientation = orientation;
                        break;

                    case PageSetupElement.UsePrinterDefaultsAttributeName:
                        // Roundtrip - Page 1990
                        //bool usePrinterDefaults = (bool)XmlElementBase.GetValue(attribute.Value, DataType.Boolean, true);
                        break;

                    case PageSetupElement.BlackAndWhiteAttributeName:
                        bool blackAndWhite = (bool)XmlElementBase.GetValue(attribute.Value, DataType.Boolean, false);
                        options.PrintInBlackAndWhite = blackAndWhite;
                        break;

                    case PageSetupElement.DraftAttributeName:
                        bool draft = (bool)XmlElementBase.GetValue(attribute.Value, DataType.Boolean, false);
                        options.DraftQuality = draft;
                        break;

                    case PageSetupElement.CellCommentsAttributeName:
                        PrintNotes printNotes = (PrintNotes)XmlElementBase.GetValue(attribute.Value, DataType.ST_CellComments, PrintNotes.DontPrint);
                        options.PrintNotes = printNotes;
                        break;

                    case PageSetupElement.UseFirstPageNumberAttributeName:
                        bool useFirstPageNumber = (bool)XmlElementBase.GetValue(attribute.Value, DataType.Boolean, false);
                        if (useFirstPageNumber)
                            options.PageNumbering = PageNumbering.UseStartPageNumber;
                        else
                            options.PageNumbering = PageNumbering.Automatic;

                        break;

                    case PageSetupElement.ErrorsAttributeName:
                        PrintErrors printErrors = (PrintErrors)XmlElementBase.GetValue(attribute.Value, DataType.ST_PrintError, PrintErrors.PrintAsDisplayed);
                        options.PrintErrors = printErrors;
                        break;

                    case PageSetupElement.HorizontalDpiAttributeName:
						// MD 2/4/11
						// Found while fixing TFS65015
						// The horizontalDpi value is defined as a uint, not an int.
						//int horizontalDpi = (int)XmlElementBase.GetValue(attribute.Value, DataType.Integer, 600);
						int horizontalDpi = (int)(uint)XmlElementBase.GetValue(attribute.Value, DataType.UInt32, 600);
						horizontalDpi = Math.Max(0, Math.Min(horizontalDpi, 65535));

                        options.Resolution = horizontalDpi;
                        break;

                    case PageSetupElement.VerticalDpiAttributeName:
						// MD 2/4/11
						// Found while fixing TFS65015
						// The verticalDpi value is defined as a uint, not an int.
						//int verticalDpi = (int)XmlElementBase.GetValue(attribute.Value, DataType.Integer, 600);
						int verticalDpi = (int)(uint)XmlElementBase.GetValue(attribute.Value, DataType.UInt32, 600);
						verticalDpi = Math.Max(0, Math.Min(verticalDpi, 65535));

                        options.VerticalResolution = verticalDpi;
                        break;

                    case PageSetupElement.CopiesAttributeName:
                        int numCopies = (int)XmlElementBase.GetValue(attribute.Value, DataType.Integer, 1);
                        options.NumberOfCopies = numCopies;
                        break;

					case XmlElementBase.RelationshipIdAttributeName:
                        // Roundtrip - 1988
                        // Page Store this value and also possibly store the file that this is 
                        // pointing to on the worksheet itself
                        // string relationshipId = (string)XmlElementBase.GetValue(attribute.Value, DataType.ST_RelationshipID, 0);
                        break;

                    default:
                        Utilities.DebugFail("Encountered an unknown attribute");
                        break;
                }
            }
        }
        #endregion //Load

        #region Save

        protected override void Save(Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value)
        {
            Worksheet worksheet = manager.ContextStack[typeof(Worksheet)] as Worksheet;
            if (worksheet == null)
            {
                Utilities.DebugFail("Could not get worksheet from the context stack");
                return;
            }
            
            //  BF 8/8/08
            //PrintOptions options = worksheet.PrintOptions;
            PrintOptions options = manager.ContextStack[typeof(PrintOptions)] as PrintOptions;
            if ( options == null )
            {
                Debug.Assert( false, "Couldn't get expected context." );
                return;
            }
                        
            string attributeValue = null;

            //  Get the property values into stack vars
            PaperSize paperSize = options.PaperSize;
            int scalingFactor = options.ScalingFactor;
            int startPageNumber = options.StartPageNumber;
            int maxPagesHorizontally = options.MaxPagesHorizontally;
            int maxPagesVertically = options.MaxPagesVertically;
            PageOrder pageOrder = options.PageOrder;
            Orientation orientation = options.Orientation;
            bool printInBlackAndWhite = options.PrintInBlackAndWhite;
            bool draftQuality = options.DraftQuality;
            PrintNotes printNotes = options.PrintNotes;
            PageNumbering pageNumbering = options.PageNumbering;
            PrintErrors printErrors = options.PrintErrors;
            int resolution = options.Resolution;
            int verticalResolution = options.VerticalResolution;
            int numberOfCopies = options.NumberOfCopies;

            // Add the 'paperSize' attribute
            if ( paperSize != PrintOptions.defaultPaperSize )
            {
                attributeValue = XmlElementBase.GetXmlString(paperSize, DataType.UInt, PaperSize.Letter, false);
                if (attributeValue != null)
                    XmlElementBase.AddAttribute(element, PageSetupElement.PaperSizeAttributeName, attributeValue);
            }

            // Add the 'scale' attribute
            if ( scalingFactor != PrintOptions.defaultScalingFactor )
            {
                attributeValue = XmlElementBase.GetXmlString(scalingFactor, DataType.UInt, 100, false);
                if (attributeValue != null)
                    XmlElementBase.AddAttribute(element, PageSetupElement.ScaleAttributeName, attributeValue);
            }

            // Add the 'firstPageNumber' attribute
            if ( startPageNumber != PrintOptions.defaultStartPageNumber )
            {
                attributeValue = XmlElementBase.GetXmlString(startPageNumber, DataType.UInt, 1, false);
                if (attributeValue != null)
                    XmlElementBase.AddAttribute(element, PageSetupElement.FirstPageNumberAttributeName, attributeValue);
            }

            // Add the 'fitToWidth' attribute
            if ( maxPagesHorizontally != PrintOptions.defaultMaxPagesHorizontally )
            {
                attributeValue = XmlElementBase.GetXmlString(maxPagesHorizontally, DataType.UInt, 1, false);
                if (attributeValue != null)
                    XmlElementBase.AddAttribute(element, PageSetupElement.FitToWidthAttributeName, attributeValue);
            }

            // Add the 'fitToHeight' attribute
            if ( maxPagesVertically != PrintOptions.defaultMaxPagesVertically )
            {
                attributeValue = XmlElementBase.GetXmlString(maxPagesVertically, DataType.UInt, 1, false);
                if (attributeValue != null)
                    XmlElementBase.AddAttribute(element, PageSetupElement.FitToHeightAttributeName, attributeValue);
            }

            // Add the 'pageOrder' attribute
            if ( pageOrder != PrintOptions.defaultPageOrder )
            {
                attributeValue = XmlElementBase.GetXmlString(pageOrder, DataType.ST_PageOrder, PageOrder.DownThenOver, false);
                if (attributeValue != null)
                    XmlElementBase.AddAttribute(element, PageSetupElement.PageOrderAttributeName, attributeValue);
            }

            // Add the 'orientation' attribute
            if ( orientation != Orientation.Default )
            {
                attributeValue = XmlElementBase.GetXmlString(orientation, DataType.ST_Orientation, Orientation.Default, false);
                if (attributeValue != null)
                    XmlElementBase.AddAttribute(element, PageSetupElement.OrientationAttributeName, attributeValue);
            }

            // Roundtrip - Page 1990
            // Add the 'usePrinterDefaults' attribute

            // Add the 'blackAndWhite' attribute
            if ( printInBlackAndWhite != PrintOptions.defaultPrintInBlackAndWhite )
            {
                attributeValue = XmlElementBase.GetXmlString(printInBlackAndWhite, DataType.Boolean, false, false);
                if (attributeValue != null)
                    XmlElementBase.AddAttribute(element, PageSetupElement.BlackAndWhiteAttributeName, attributeValue);
            }

            // Add the 'draft' attribute
            if ( draftQuality != PrintOptions.defaultDraftQuality )
            {
                attributeValue = XmlElementBase.GetXmlString(draftQuality, DataType.Boolean, false, false);
                if (attributeValue != null)
                    XmlElementBase.AddAttribute(element, PageSetupElement.DraftAttributeName, attributeValue);
            }

            // Add the 'cellComments' attribute
            if ( printNotes != PrintOptions.defaultPrintNotes )
            {
                attributeValue = XmlElementBase.GetXmlString(printNotes, DataType.ST_CellComments, PrintNotes.DontPrint, false);
                if (attributeValue != null)
                    XmlElementBase.AddAttribute(element, PageSetupElement.CellCommentsAttributeName, attributeValue);
            }

            // Add the 'useFirstPageNumber' attribute
            if ( pageNumbering != PrintOptions.defaultPageNumbering )
            {
                bool useFirstPageNumbering = pageNumbering == PageNumbering.UseStartPageNumber ? true : false;
                attributeValue = XmlElementBase.GetXmlString(useFirstPageNumbering, DataType.Boolean, false, false);
                if (attributeValue != null)
                    XmlElementBase.AddAttribute(element, PageSetupElement.UseFirstPageNumberAttributeName, attributeValue);
            }

            // Add the 'printErrors' attribute
            if ( printErrors != PrintOptions.defaultPrintErrors )
            {
                attributeValue = XmlElementBase.GetXmlString(printErrors, DataType.ST_PrintError, PrintErrors.PrintAsDisplayed, false);
                if (attributeValue != null)
                    XmlElementBase.AddAttribute(element, PageSetupElement.ErrorsAttributeName, attributeValue);
            }

            // Add the 'horizontalDpi' attribute
            if ( resolution != PrintOptions.defaultResolution )
            {
                attributeValue = XmlElementBase.GetXmlString(resolution, DataType.Integer, 600, false);
                if (attributeValue != null)
                    XmlElementBase.AddAttribute(element, PageSetupElement.HorizontalDpiAttributeName, attributeValue);
            }

            // Add the 'verticalDpi' attribute
            if ( verticalResolution != PrintOptions.defaultVerticalResolution )
            {
                attributeValue = XmlElementBase.GetXmlString(verticalResolution, DataType.Integer, 600, false);
                if (attributeValue != null)
                    XmlElementBase.AddAttribute(element, PageSetupElement.VerticalDpiAttributeName, attributeValue);
            }

            // Add the 'copies' attribute 
            if ( numberOfCopies != PrintOptions.defaultNumberOfCopies )
            {
                attributeValue = XmlElementBase.GetXmlString(numberOfCopies, DataType.Integer, 1, false);
                if (attributeValue != null)
                    XmlElementBase.AddAttribute(element, PageSetupElement.CopiesAttributeName, attributeValue);
            }
            // Roundtrip - Add the 'r:id' attribute
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