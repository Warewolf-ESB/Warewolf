using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Xml;




using System.Drawing;
using Infragistics.Documents.Excel.Serialization.Excel2007.XLSX;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements

{






	internal class CalculationPropertiesElement : XLSXElementBase
    {
        #region XML Schema fragment <docs>
        //<complexType name="CT_CalcPr">
        //<attribute name="calcId" type="xsd:unsignedInt"/>
        //<attribute name="calcMode" type="ST_CalcMode" use="optional" default="auto"/>
        //<attribute name="fullCalcOnLoad" type="xsd:boolean" use="optional" default="false"/>
        //<attribute name="refMode" type="ST_RefMode" use="optional" default="A1"/>
        //<attribute name="iterate" type="xsd:boolean" use="optional" default="false"/>
        //<attribute name="iterateCount" type="xsd:unsignedInt" use="optional" default="100"/>
        //<attribute name="iterateDelta" type="xsd:double" use="optional" default="0.001"/>
        //<attribute name="fullPrecision" type="xsd:boolean" use="optional" default="true"/>
        //<attribute name="calcCompleted" type="xsd:boolean" use="optional" default="true"/>
        //<attribute name="calcOnSave" type="xsd:boolean" use="optional" default="true"/>
        //<attribute name="concurrentCalc" type="xsd:boolean" use="optional" default="true"/>
        //<attribute name="concurrentManualCount" type="xsd:unsignedInt" use="optional"/>
        //<attribute name="forceFullCalc" type="xsd:boolean" use="optional"/>
        //</complexType>
        #endregion XML Schema fragment <docs>

        #region Constants

        /// <summary>
        /// "calcPr"
        /// </summary>
        public const string LocalName = "calcPr";

        /// <summary>
        /// "http://schemas.openxmlformats.org/spreadsheetml/2006/main/calcPr"
        /// </summary>
        public const string QualifiedName =
			XLSXElementBase.DefaultXmlNamespace +
			XmlElementBase.NamespaceSeparator +
			CalculationPropertiesElement.LocalName;

        private const string CalcIdAttributeName = "calcId";
        private const string CalcModeAttributeName = "calcMode";
        private const string FullCalcOnLoadAttributeName = "fullCalcOnLoad";
        private const string RefModeAttributeName = "refMode";
        private const string IterateAttributeName = "iterate";
        private const string IterateCountAttributeName = "iterateCount";
        private const string IterateDeltaAttributeName = "iterateDelta";
        private const string FullPrecisionAttributeName = "fullPrecision";
        private const string CalcCompletedAttributeName = "calcCompleted";
        private const string CalcOnSaveAttributeName = "calcOnSave";
        private const string ConcurrentCalcAttributeName = "concurrentCalc";
        private const string ConcurrentManualCountcAttributeName = "concurrentManualCount";
        private const string ForceFullCalccAttributeName = "forceFullCalc";

        private const uint DefaultCalcId = 125725;

        #endregion Constants

        #region Base class overrides

            #region Type
        /// <summary>
        /// Returns the <see cref="XLSXElementType">XLSXElementType</see> constant which identifies this element.
        /// </summary>
        public override XLSXElementType Type
        {
            get { return XLSXElementType.calcPr; }
        }
            #endregion Type

            #region Load


#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		protected override void Load( Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode )
        {
            Workbook workbook = manager.Workbook;
            object attributeValue = null;

			foreach ( ExcelXmlAttribute attribute in element.Attributes )
			{
				string attributeName = XmlElementBase.GetQualifiedAttributeName( attribute );

                switch ( attributeName )
                {
                    case CalculationPropertiesElement.CalcIdAttributeName:
                    {
                        attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.UInt, 0 );

                        //  RoundTrip - Page 1882
                        //uint calcId = (uint)attributeValue;
                    }
                    break;

                    case CalculationPropertiesElement.CalcModeAttributeName:
                    {
                        attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.ST_CalcMode, CalculationMode.Automatic );
                        workbook.CalculationMode = (CalculationMode)attributeValue;
                    }
                    break;

                    case CalculationPropertiesElement.FullCalcOnLoadAttributeName:
                    {
                        attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.Boolean, false );
                        
                        //  RoundTrip - Page 1883
                        //bool fullCalcOnLoad = (bool)attributeValue;
                    }
                    break;

                    case CalculationPropertiesElement.RefModeAttributeName:
                    {
                        attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.ST_RefMode, CellReferenceMode.A1 );
                        workbook.CellReferenceMode = (CellReferenceMode)attributeValue;
                    }
                    break;

                    case CalculationPropertiesElement.IterateAttributeName:
                    {
                        attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.Boolean, false );
                        workbook.IterativeCalculationsEnabled = (bool)attributeValue;
                    }
                    break;

                    case CalculationPropertiesElement.IterateCountAttributeName:
                    {
                        attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.UInt, 100 );
                        workbook.MaxRecursionIterations = Utilities.ToInteger(attributeValue);
                    }
                    break;

                    case CalculationPropertiesElement.IterateDeltaAttributeName:
                    {
                        attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.Double, .001 );
                        workbook.MaxChangeInIteration = (double)attributeValue;
                    }
                    break;

                    case CalculationPropertiesElement.FullPrecisionAttributeName:
                    {
                        attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.Boolean, true );
                        bool fullPrecision = (bool)attributeValue;
                        workbook.Precision = fullPrecision ? Precision.UseRealCellValues : Precision.UseDisplayValues;
                    }
                    break;

                    case CalculationPropertiesElement.CalcCompletedAttributeName:
                    {
                        attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.Boolean, true );
                        
                        //  RoundTrip - Page 1882
                        //bool calcCompleted = (bool)attributeValue;
                    }
                    break;

                    case CalculationPropertiesElement.CalcOnSaveAttributeName:
                    {
                        attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.Boolean, true );
                        workbook.RecalculateBeforeSave = (bool)attributeValue;
                    }
                    break;

                    case CalculationPropertiesElement.ConcurrentCalcAttributeName:
                    {
                        attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.Boolean, true );

                        //  RoundTrip - Page 1883
                        //bool concurrentCalc = (bool)attributeValue;
                    }
                    break;

                    case CalculationPropertiesElement.ConcurrentManualCountcAttributeName:
                    {
                        attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.UInt, 0 );

                        //  RoundTrip - Page 1883
                        //int concurrentManualCount = Utilities.ToInteger(attributeValue);
                    }
                    break;

                    case CalculationPropertiesElement.ForceFullCalccAttributeName:
                    {
                        attributeValue = XmlElementBase.GetAttributeValue( attribute, DataType.Boolean, false );

                        //  RoundTrip - Page 1883
                        //bool forceFullCalc = (bool)attributeValue;
                    }
                    break;

                }
            }

        }
            #endregion Load

            #region Save


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		protected override void Save( Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value )
        {
            //  Get a reference to the Workbook
            Workbook workbook = manager.Workbook;

            string attributeValue = string.Empty;
            string attributeName = string.Empty;
            
            #region (calcId)
            //<attribute name="calcId" type="xsd:unsignedInt"/>
            
            //  RoundTrip - Page 1883
            uint calcId = DefaultCalcId; 
           
            attributeName = CalculationPropertiesElement.CalcIdAttributeName;
            
            
            attributeValue = calcId.ToString();
            
            XmlElementBase.AddAttribute( element, attributeName, attributeValue );
            
            #endregion (calcId)

            #region CalculationMode
            //<attribute name="calcMode" type="ST_CalcMode" use="optional" default="auto"/>

            if ( workbook.CalculationMode != CalculationMode.Automatic )
            {
                attributeName = CalculationPropertiesElement.CalcModeAttributeName;

                attributeValue = XmlElementBase.GetXmlString( workbook.CalculationMode, DataType.ST_CalcMode );

				XmlElementBase.AddAttribute( element, attributeName, attributeValue );
            }
            
            #endregion CalculationMode

            #region (fullCalcOnLoad)
            //<attribute name="fullCalcOnLoad" type="xsd:boolean" use="optional" default="false"/>

            //  RoundTrip - Page 1883
            //attributeName = CalculationPropertiesElement.FullCalcOnLoadAttributeName;

            //attributeValue = false;

            //XmlElementBase.AddAttributeLocal( document, element, attributeName, attributeValue );
            
            #endregion (fullCalcOnLoad)

            #region CellReferenceMode (refMode)
            //<attribute name="refMode" type="ST_RefMode" use="optional" default="A1"/>

            if ( workbook.CellReferenceMode != CellReferenceMode.A1 )
            {
                attributeName = CalculationPropertiesElement.RefModeAttributeName;
                
                attributeValue = XmlElementBase.GetXmlString( workbook.CellReferenceMode, DataType.ST_RefMode );

				XmlElementBase.AddAttribute( element, attributeName, attributeValue );
            }
            #endregion CellReferenceMode (refMode)

            #region IterativeCalculationsEnabled (iterate)
            //<attribute name="iterate" type="xsd:boolean" use="optional" default="false"/>

            if ( workbook.IterativeCalculationsEnabled )
            {
                attributeName = CalculationPropertiesElement.IterateAttributeName;
                
                attributeValue = XmlElementBase.GetXmlString( workbook.IterativeCalculationsEnabled, DataType.Boolean );

				XmlElementBase.AddAttribute( element, attributeName, attributeValue );
            }
            #endregion IterativeCalculationsEnabled (iterate)

            #region MaxRecursionIterations (iterateCount)
            //<attribute name="iterateCount" type="xsd:unsignedInt" use="optional" default="100"/>

            if ( workbook.MaxRecursionIterations != 100 )
            {
                attributeName = CalculationPropertiesElement.IterateCountAttributeName;
                
                attributeValue = XmlElementBase.GetXmlString( workbook.MaxRecursionIterations, DataType.Int32 );

				XmlElementBase.AddAttribute( element, attributeName, attributeValue );
            }
            #endregion MaxRecursionIterations (iterateCount)

            #region MaxChangeInIteration (iterateDelta)
            //<attribute name="iterateDelta" type="xsd:double" use="optional" default="0.001"/>

            if ( workbook.MaxChangeInIteration != .001 )
            {
                attributeName = CalculationPropertiesElement.IterateDeltaAttributeName;
                
                attributeValue = XmlElementBase.GetXmlString( workbook.MaxChangeInIteration, DataType.Double );

				XmlElementBase.AddAttribute( element, attributeName, attributeValue );
            }
            #endregion MaxRecursionIterations (iterateDelta)

            #region Precision (fullPrecision)
            //<attribute name="fullPrecision" type="xsd:boolean" use="optional" default="true"/>

            if ( workbook.Precision != Precision.UseRealCellValues )
            {
                attributeName = CalculationPropertiesElement.FullPrecisionAttributeName;
                                
                bool fullPrecision = workbook.Precision == Precision.UseRealCellValues ? true : false;
                attributeValue = XmlElementBase.GetXmlString( fullPrecision, DataType.Boolean );
				XmlElementBase.AddAttribute( element, attributeName, attributeValue );
            }
            #endregion MaxRecursionIterations (iterateDelta)

            #region RecalculateBeforeSave (calcOnSave)
            //<attribute name="calcOnSave" type="xsd:boolean" use="optional" default="true"/>

            if ( workbook.RecalculateBeforeSave == false )
            {
                attributeName = CalculationPropertiesElement.CalcOnSaveAttributeName;
                
                attributeValue = XmlElementBase.GetXmlString( workbook.RecalculateBeforeSave, DataType.Boolean );

				XmlElementBase.AddAttribute( element, attributeName, attributeValue );
            }
            #endregion RecalculateBeforeSave (calcOnSave)

            #region (calcCompleted)
            //<attribute name="calcCompleted" type="xsd:boolean" use="optional" default="true"/>
            
            //  RoundTrip - Page 1884
            //bool calcCompleted = false;

            //attributeName = CalculationPropertiesElement.CalcCompletedAttributeName;
            
            //attributeValue = XmlElementBase.GetXmlString( calcCompleted, DataType.Boolean );
            
            //XmlElementBase.AddAttributeLocal( document, element, attributeName, attributeValue );

            #endregion (calcCompleted)

            #region (concurrentCalc)
            //<attribute name="concurrentCalc" type="xsd:boolean" use="optional" default="true"/>
            
            //  RoundTrip - Page 1883
            //bool concurrentCalc = false;

            //attributeName = CalculationPropertiesElement.ConcurrentCalcAttributeName;
            
            //attributeValue = XmlElementBase.GetXmlString( concurrentCalc, DataType.Boolean );
            
            //XmlElementBase.AddAttributeLocal( document, element, attributeName, attributeValue );

            #endregion (concurrentCalc)

            #region (concurrentManualCount)
            //<attribute name="concurrentManualCount" type="xsd:unsignedInt" use="optional"/>
            
            //  RoundTrip - Page 1883
            //int concurrentManualCount = 0;

            //attributeName = CalculationPropertiesElement.ConcurrentManualCountcAttributeName;
            
            //attributeValue = XmlElementBase.GetXmlString( concurrentManualCount, DataType.Integer );
            
            //XmlElementBase.AddAttributeLocal( document, element, attributeName, attributeValue );

            #endregion (concurrentManualCount)

            #region (forceFullCalc)
            //<attribute name="forceFullCalc" type="xsd:boolean" use="optional"/>
            
            //  RoundTrip - Page 1883
            //bool forceFullCalc = false;

            //attributeName = CalculationPropertiesElement.ForceFullCalccAttributeName;
            
            //attributeValue = XmlElementBase.GetXmlString( forceFullCalc, DataType.Boolean );
            
            //XmlElementBase.AddAttributeLocal( document, element, attributeName, attributeValue );

            #endregion (forceFullCalc)

        }
            #endregion Save

        #endregion Base class overrides
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