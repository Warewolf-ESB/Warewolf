using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace Infragistics.Documents.Core
{
    #region DataType enumeration
    /// <summary>
    /// Constants which identify the supported data types.
    /// Note: Only "native" types are supported here, the
    /// stuff that is specific to Excel has been stripped
    /// out.
    /// </summary>
    internal enum DataType
    {
		/// <summary>System.String</summary>
		String,

		/// <summary>System.Int16</summary>
		Short,

		/// <summary>System.Int32</summary>
		Integer,

		/// <summary>System.Int64</summary>
		Long,

		/// <summary>System.Int16</summary>
		Int16 = Short,

		/// <summary>System.Int32</summary>
		Int32 = Integer,

		/// <summary>System.Int64</summary>
		Int64 = Long,

		/// <summary>System.Boolean</summary>
		Boolean,

		/// <summary>System.DateTime</summary>
		DateTime,

		/// <summary>System.Single</summary>
		Single,

		/// <summary>System.Single</summary>
		Float = Single,

		/// <summary>System.Double</summary>
		Double,

		/// <summary>System.UInt16</summary>
		UShort,

		/// <summary>System.UInt16</summary>
		UInt16 = UShort,

		/// <summary>System.UInt32</summary>
		UInt,

		/// <summary>System.UInt32</summary>
		UInt32 = UInt,

		/// <summary>System.UInt64</summary>
		ULong,

		/// <summary>System.UInt64</summary>
		UInt64 = ULong,

		/// <summary>System.Byte</summary>
		Byte,

		/// <summary>System.SByte</summary>
		SByte,

		/// <summary>System.Decimal</summary>
		Decimal,

		/// <summary>System.Object</summary>
		Object,

        #region Was supported in XmlElementBase

        ///// <summary>Represents the ST_RelationshipID simple XML type.</summary>
        //ST_RelationshipID = String,

        ///// <summary>Represents the ST_VectorBaseType simple XML type.</summary>
        //ST_VectorBaseType,

        ///// <summary>Represents the ST_Visibility simple XML type.</summary>
        //ST_Visibility,

        ///// <summary>Represents the ST_CellRef simple XML type.</summary>
        //ST_CellRef = String,

        ///// <summary>Represents the ST_Ref simple XML type.</summary>
        //ST_Ref = String,

        //// Note: This is actually in the format of "A1:C2 F4:G6" or such,
        //// but unless we actually need to parse out these values, we should
        //// just leave it as a string to minimize overhead
        ///// <summary>Represents the ST_Sqref simple XML type.</summary>
        //ST_Sqref = String,

        ///// <summary>Represents the ST_SheetState simple XML type, which is an xml-escaped string.</summary>
        //ST_Xstring = String,
        ///// <summary>Represents the ST_SheetState simple XML type</summary>
        //ST_SheetState = ST_Visibility,

        ///// <summary>Represents the ST_CalcMode simple XML type.</summary>
        //ST_CalcMode,

        ///// <summary>Represents the ST_RefMode simple XML type.</summary>
        //ST_RefMode,

        ///// <summary>Represents the ST_FontScheme simple XML type.</summary>
        //ST_FontScheme,

        ///// <summary>Represents the ST_UnderlineValues simple XML type.</summary>
        //ST_UnderlineValues,

        ///// <summary>Represents the ST_VerticalAlignRun simple XML type.</summary>
        //ST_VerticalAlignRun,

        ///// <summary>Represents the ST_BorderStyle simple XML type.</summary>
        //ST_BorderStyle,
    
        ///// <summary>Represents the ST_SheetViewType simple XML type.</summary>
        //ST_SheetViewType,

        ///// <summary>Represents the ST_Pane simple XML type.</summary>
        //ST_Pane,

        ///// <summary>Represents the ST_GradientType simple XML type.</summary>
        //ST_GradientType,

        ///// <summary>Represents the ST_PatternType simple XML type.</summary>
        //ST_PatternType,

        ///// <summary>Represents the ST_Objects simple XML type.</summary>
        //ST_Objects,

        ///// <summary>Represents the ST_Links simple XML type.</summary>
        //ST_Links,

        ///// <summary>Represents the ST_CellType simple XML type.</summary>
        //ST_CellType,

        ///// <summary>Represents the ST_HorizontalAlignment simple XML type.</summary>
        //ST_HorizontalAlignment,

        ///// <summary>Represents the ST_VerticalAlignment simple XML type.</summary>
        //ST_VerticalAlignment,

        ///// <summary>Represents the ST_Orientation simple XML type.</summary>
        //ST_Orientation,

        ///// <summary>Represents the ST_PageOrder simple XML type.</summary>
        //ST_PageOrder,

        ///// <summary>Represents the ST_CellComments simple XML type.</summary>
        //ST_CellComments,

        ///// <summary>Represents the ST_PrintError simple XML type.</summary>
        //ST_PrintError,

        ///// <summary>Represents the ST_PaneState simple XML type.</summary>
        //ST_PaneState,

        ///// <summary>Represents the ST_Guid simple XML type.</summary>
        //ST_Guid,

        ///// <summary>Represents the ST_Comments simple XML type.</summary>
        //ST_Comments,

        ///// <summary>Represents the ST_Comments simple XML type.</summary>
        //ST_CellFormulaType,

        ///// <summary>Represents the ST_UnsignedIntHex simple XML type.</summary>
        //ST_UnsignedIntHex,

        ///// <summary>Represents the ST_SystemColorVal simple XML type.</summary>
        //ST_SystemColorVal,

        ///// <summary>Represents the ST_EditAs simple XML type.</summary>
        //ST_EditAs,

        ///// <summary>Represents the ST_DrawingElementId simple XML type.</summary>
        //ST_DrawingElementId,

        ///// <summary>Represents the ST_Coordinate simple XML type.</summary>
        //ST_Coordinate,

        ///// <summary>Represents the ST_PositiveCoordinate simple XML type.</summary>
        //ST_PositiveCoordinate, 
        
        #endregion Was supported in XmlElementBase       
    }
    #endregion DataType enumeration

    #region CorePropertiesElementType enumeration

    /// <summary>
    /// Enums whose constant name is the same as the local name
    /// of the corresponding XML element in /docProps/core.xml.
    /// </summary>
    internal enum CorePropertiesElementType
    {
        /// <summary>cp:coreProperties</summary>
        coreProperties,
        
        /// <summary>dc:title</summary>
        title,
        
        /// <summary>dc:subject</summary>
        subject,

        /// <summary>dc:creator</summary>
        creator,

        /// <summary>cp:keywords</summary>
        keywords,

        /// <summary>dc:description</summary>
        description,

        /// <summary>cp:lastModifiedBy</summary>
        lastModifiedBy,

        /// <summary>dcterms:created</summary>
        created,

        /// <summary>dcterms:modified</summary>
        modified,

        /// <summary>cp:category</summary>
        category,

        /// <summary>cp:contentStatus</summary>
        contentStatus,

        /// <summary>xsi:type (Attribute)</summary>
        type,
    }

    #endregion CorePropertiesElementType enumeration

    #region ExtendedPropertiesElementType enumeration

    /// <summary>
    /// Enums whose constant name is the same as the local name
    /// of the corresponding XML element in /docProps/app.xml.
    /// </summary>
    internal enum ExtendedPropertiesElementType
    {
        /// <summary>Properties</summary>
        Properties,
        
        /// <summary>Template</summary>
        Template,
        
        /// <summary>Application</summary>
        Application,
        
        /// <summary>DocSecurity</summary>
        DocSecurity,
        
        /// <summary>DocSecurity</summary>
        ScaleCrop,
        
        /// <summary>HeadingPairs</summary>
        HeadingPairs,
        
        /// <summary>TitlesOfParts</summary>
        TitlesOfParts,
        
        /// <summary>Company</summary>
        Company,

        /// <summary>Manager</summary>
        Manager,

        /// <summary>LinksUpToDate</summary>
        LinksUpToDate,

        /// <summary>SharedDoc</summary>
        SharedDoc,

        /// <summary>HyperlinksChanged</summary>
        HyperlinksChanged,

        /// <summary>AppVersion</summary>
        AppVersion,
    }

    #endregion ExtendedPropertiesElementType enumeration

    #region VariantElementType enumeration

    /// <summary>
    /// Enums whose constant names correspond to the elements/attributes in the
    /// http://schemas.openxmlformats.org/officeDocument/2006/docPropsVTypes
    /// namespace. Only contains the ones we currently use.
    /// </summary>
    internal enum VariantElementType
    {
        /// <summary>variant</summary>
        variant,
        
        /// <summary>vector</summary>
        vector,
        
        /// <summary>i4</summary>
        i4,
        
        /// <summary>lpstr</summary>
        lpstr,
        
        /// <summary>size (attribute)</summary>
        size,
        
        /// <summary>baseType (attribute)</summary>
        baseType,        
    }
    
    #endregion VariantElementType enumeration
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