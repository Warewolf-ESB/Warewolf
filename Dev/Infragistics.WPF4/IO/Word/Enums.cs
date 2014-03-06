using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;


using System.Windows;
using System.Windows.Media;





namespace Infragistics.Documents.Word
{
    #region WordProcessingMLAttributeType enumeration

    /// <summary>
    /// Enums whose constant names correspond to the attributes in the
    /// http://schemas.openxmlformats.org/wordprocessingml/2006/main
    /// namespace. Only contains the ones we currently use.
    /// </summary>
    internal enum WordProcessingMLAttributeType
    {
        /// <summary>w:val</summary>
        val,

        /// <summary>w:left</summary>
        left,

        /// <summary>w:right</summary>
        right,

        /// <summary>w:history</summary>
        history,

        /// <summary>w:tooltip</summary>
        tooltip,

        /// <summary>w:themeColor</summary>
        themeColor,

        /// <summary>w:asciiTheme</summary>
        asciiTheme,

        /// <summary>w:eastAsiaTheme</summary>
        eastAsiaTheme,

        /// <summary>w:hAnsiTheme</summary>
        hAnsiTheme,

        /// <summary>w:cstheme</summary>
        cstheme,

        /// <summary>w:eastAsia</summary>
        eastAsia,

        /// <summary>w:bidi</summary>
        bidi,

        /// <summary>w:after</summary>
        after,

        /// <summary>w:before</summary>
        before,

        /// <summary>w:line</summary>
        line,

        /// <summary>w:lineRule</summary>
        lineRule,

        /// <summary>w:hRule</summary>
        hRule,

        /// <summary>w:w</summary>
        w,

        /// <summary>w:type</summary>
        type,

        /// <summary>w:fill</summary>
        fill,

        /// <summary>w:h</summary>
        h,

        /// <summary>w:orient</summary>
        orient,

        /// <summary>w:code</summary>
        code,

        /// <summary>w:top</summary>
        top,

        /// <summary>w:bottom</summary>
        bottom,

        /// <summary>w:header</summary>
        header,

        /// <summary>w:footer</summary>
        footer,

        /// <summary>w:gutter</summary>
        gutter,

        /// <summary>w:space</summary>
        space,

        /// <summary>w:linePitch</summary>
        linePitch,

        /// <summary>w:instr</summary>
        instr,

        /// <summary>w:fldCharType</summary>
        fldCharType,

        /// <summary>w:start</summary>
        start,
    }
    #endregion WordProcessingMLAttributeType

    #region WordProcessingMLElementType enumeration

    /// <summary>
    /// Enums whose constant names correspond to the elements in the
    /// http://schemas.openxmlformats.org/wordprocessingml/2006/main
    /// namespace. Only contains the ones we currently use.
    /// </summary>
    internal enum WordProcessingMLElementType
    {
        /// <summary>document</summary>
        document,
        
        /// <summary>document</summary>
        body,
        
        /// <summary>p (paragraph)</summary>
        p,
        
        /// <summary>pPr (paragraph properties)</summary>
        pPr,
        
        /// <summary>jc (alignment)</summary>
        jc,
        
        /// <summary>ind (indentation)</summary>
        ind,

        /// <summary>tblInd (table indentation)</summary>
        tblInd,

        /// <summary>pageBreakBefore</summary>
        pageBreakBefore,
        
        /// <summary>r (text run)</summary>
        r,

        /// <summary>rPr (run properties)</summary>
        rPr,

        /// <summary>cantSplit (Table Row Cannot Break Across Pages)</summary>
        cantSplit,

        /// <summary>rStyle (run style)</summary>
        rStyle,

        /// <summary>noProof (no proof)</summary>
        noProof,

        /// <summary>t (text)</summary>
        t,                

        /// <summary>val</summary>
        val,                

        /// <summary>br (break)</summary>
        br,                

        /// <summary>rr (carriage return)</summary>
        cr,                

        /// <summary>kern (character kerning)</summary>
        kern,                

        /// <summary>w (expanded/compressed text)</summary>
        w,                

        /// <summary>spacing</summary>
        spacing,                

        /// <summary>position</summary>
        position,                

        /// <summary>hyperlink</summary>
        hyperlink,                

        /// <summary>lang</summary>
        lang,                

        /// <summary>drawing</summary>
        drawing,                

        /// <summary>tbl</summary>
        tbl,                

        /// <summary>tblPr</summary>
        tblPr,                

        /// <summary>tblGrid</summary>
        tblGrid,                

        /// <summary>tblHeader</summary>
        tblHeader,                

        /// <summary>tblCellSpacing</summary>
        tblCellSpacing,                

        /// <summary>tblCellMar</summary>
        tblCellMar,                

        /// <summary>tcMar</summary>
        tcMar,                

        /// <summary>tblLayout</summary>
        tblLayout,                

        /// <summary>tblW</summary>
        tblW,                

        /// <summary>gridCol</summary>
        gridCol,                

        /// <summary>gridSpan</summary>
        gridSpan,                

        /// <summary>tr</summary>
        tr,                

        /// <summary>tc</summary>
        tc,                

        /// <summary>trPr</summary>
        trPr,                

        /// <summary>tcPr</summary>
        tcPr,                

        /// <summary>trHeight</summary>
        trHeight,                

        /// <summary>tcW</summary>
        tcW,                

        /// <summary>vMerge</summary>
        vMerge,

        /// <summary>w:left</summary>
        left,

        /// <summary>w:right</summary>
        right,

        /// <summary>w:top</summary>
        top,

        /// <summary>w:bottom</summary>
        bottom,

        /// <summary>w:shd</summary>
        shd,

        /// <summary>w:tblBorders</summary>
        tblBorders,

        /// <summary>w:tcBorders</summary>
        tcBorders,

        /// <summary>w:insideH</summary>
        insideH,

        /// <summary>w:insideV</summary>
        insideV,

        /// <summary>w:tblStyle</summary>
        tblStyle,

        /// <summary>w:textDirection</summary>
        textDirection,

        /// <summary>w:vAlign</summary>
        vAlign,

        /// <summary>w:sectPr</summary>
        sectPr,

        /// <summary>w:pgSz</summary>
        pgSz,

        /// <summary>w:pgMar</summary>
        pgMar,

        /// <summary>w:cols</summary>
        cols,

        /// <summary>w:docGrid</summary>
        docGrid,

        /// <summary>w:hdr</summary>
        hdr,

        /// <summary>w:ftr</summary>
        ftr,

        /// <summary>w:headerReference</summary>
        headerReference,

        /// <summary>w:footerReference</summary>
        footerReference,

        /// <summary>w:titlePg</summary>
        titlePg,

        /// <summary>w:fldSimple</summary>
        fldSimple,

        /// <summary>w:fldChar</summary>
        fldChar,

        /// <summary>w:instrText</summary>
        instrText,

        /// <summary>w:pgNumType</summary>
        pgNumType,

        /// <summary>w:pict</summary>
        pict,

        /// <summary>w:gridBefore</summary>
        gridBefore,

        /// <summary>w:gridAfter</summary>
        gridAfter,
    }
    
    #endregion WordProcessingMLElementType enumeration

    #region XmlElementType enumeration
    internal enum XmlElementType
    {
        /// <summary>space</summary>
        space,                
    }

    #endregion XmlElementType

    #region XmlNamespaceElementType enumeration
    internal enum XmlNamespaceElementType
    {
        /// <summary>xmlns</summary>
        xmlns,                
    }

    #endregion XmlNamespaceElementType

    #region RelationshipsElementType enumeration
    internal enum RelationshipsElementType
    {
        /// <summary>id</summary>
        id,
    }
    #endregion RelationshipsElementType enumeration

    #region RelationshipsAttributeType enumeration
    internal enum RelationshipsAttributeType
    {
        /// <summary>embed</summary>
        embed,
    }
    #endregion RelationshipsAttributeType enumeration

    #region WordStylesElementType enumeration
    internal enum WordStylesElementType
    {
        /// <summary>styles</summary>
        styles,
        
        /// <summary>docDefaults</summary>
        docDefaults,
        
        /// <summary>rPrDefault</summary>
        rPrDefault,        

        /// <summary>pPrDefault</summary>
        pPrDefault,        

        /// <summary>style</summary>
        style,        

        /// <summary>name</summary>
        name,        
    }
    #endregion WordStylesElementType enumeration

    #region WordStylesAttributeType enumeration
    internal enum WordStylesAttributeType
    {
        /// <summary>type</summary>
        type,
        
        /// <summary>styleId</summary>
        styleId,
        
        /// <summary>val</summary>
        val,        
        
        /// <summary>default</summary>
        _default,        
    }
    #endregion WordStylesAttributeType enumeration

    #region WordFontElementType enumeration
    internal enum WordFontElementType
    {
        /// <summary>rFonts (typeface names)</summary>
        rFonts,

        /// <summary>sz (size)</summary>
        sz,

        /// <summary>sz (size for complex script)</summary>
        szCs,

        /// <summary>color</summary>
        color,

        /// <summary>b (bold)</summary>
        b,

        /// <summary>bCs (bold for complex script)</summary>
        bCs,

        /// <summary>i (italic)</summary>
        i,

        /// <summary>i (italic for complex script)</summary>
        iCs,

        /// <summary>u (underline)</summary>
        u,

        /// <summary>ascii</summary>
        ascii,

        /// <summary>cs (complex script)</summary>
        cs,

        /// <summary>eastAsia</summary>
        eastAsia,

        /// <summary>hAnsi</summary>
        hAnsi,

        /// <summary>caps</summary>
        caps,

        /// <summary>smallCaps</summary>
        smallCaps,

        /// <summary>strike</summary>
        strike,

        /// <summary>dstrike</summary>
        dstrike,

        /// <summary>vanish</summary>
        vanish,

        /// <summary>emboss</summary>
        emboss,

        /// <summary>imprint</summary>
        imprint,

        /// <summary>outline</summary>
        outline,

        /// <summary>shadow</summary>
        shadow,

        /// <summary>rtl</summary>
        rtl,

        /// <summary>vertAlign</summary>
        vertAlign,
    }
    #endregion WordFontElementType enumeration

    #region WordProcessingDrawingElementType
    internal enum WordProcessingDrawingElementType
    {
        /// <summary>inline</summary>
        inline,

        /// <summary>anchor</summary>
        anchor,

        /// <summary>simplePos</summary>
        simplePos,

        /// <summary>extent</summary>
        extent,

        /// <summary>docPr</summary>
        docPr,

        /// <summary>positionH</summary>
        positionH,

        /// <summary>positionV</summary>
        positionV,

        /// <summary>align</summary>
        align,

        /// <summary>posOffset</summary>
        posOffset,

        /// <summary>wrapTopAndBottom</summary>
        wrapTopAndBottom,

        /// <summary>wrapNone</summary>
        wrapNone,

        /// <summary>wrapSquare</summary>
        wrapSquare,

        /// <summary>cNvGraphicFramePr</summary>
        cNvGraphicFramePr,
    }
    #endregion WordProcessingDrawingElementType

    #region WordProcessingDrawingAttributeType
    internal enum WordProcessingDrawingAttributeType
    {
        /// <summary>distT</summary>
        distT,

        /// <summary>distB</summary>
        distB,

        /// <summary>distL</summary>
        distL,

        /// <summary>distR</summary>
        distR,

        /// <summary>cx</summary>
        cx,

        /// <summary>cy</summary>
        cy,

        /// <summary>id</summary>
        id,

        /// <summary>name</summary>
        name,

        /// <summary>title</summary>
        title,

        /// <summary>descr</summary>
        descr,

        /// <summary>uri</summary>
        uri,

        /// <summary>x</summary>
        x,

        /// <summary>y</summary>
        y,

        /// <summary>prst</summary>
        prst,

        /// <summary>simplePos</summary>
        simplePos,

        /// <summary>relativeHeight</summary>
        relativeHeight,

        /// <summary>behindDoc</summary>
        behindDoc,

        /// <summary>locked</summary>
        locked,

        /// <summary>layoutInCell</summary>
        layoutInCell,

        /// <summary>allowOverlap</summary>
        allowOverlap,

        /// <summary>relativeFrom</summary>
        relativeFrom,

        /// <summary>wrapText</summary>
        wrapText,

        /// <summary>cmpd</summary>
        cmpd,

        /// <summary>w</summary>
        w,

        /// <summary>val</summary>
        val,

        /// <summary>noChangeAspect</summary>
        noChangeAspect,

        /// <summary>lim</summary>
        lim,
    }
    #endregion WordProcessingDrawingAttributeType

    #region DrawingMLAttributeType
    internal enum DrawingMLAttributeType
    {
        tooltip,
    }
    #endregion DrawingMLAttributeType

    #region DrawingMLElementType
    internal enum DrawingMLElementType
    {
        /// <summary>graphic</summary>
        graphic,

        /// <summary>graphicData</summary>
        graphicData,

        /// <summary>graphicFrameLocks</summary>
        graphicFrameLocks,

        /// <summary>blip</summary>
        blip,

        /// <summary>stretch</summary>
        stretch,

        /// <summary>fillRect</summary>
        fillRect,

        /// <summary>xfrm</summary>
        xfrm,

        /// <summary>ext</summary>
        ext,

        /// <summary>off</summary>
        off,

        /// <summary>off</summary>
        prstGeom,

        /// <summary>ln</summary>
        ln,

        /// <summary>solidFill</summary>
        solidFill,

        /// <summary>srgbClr</summary>
        srgbClr,

        /// <summary>a:miter</summary>
        miter,

        /// <summary>a:bevel</summary>
        bevel,

        /// <summary>a:round</summary>
        round,

        /// <summary>a:hlinkClick</summary>
        hlinkClick,
    }
    #endregion DrawingMLElementType

    #region DrawingMLPicElementType
    internal enum DrawingMLPicElementType
    {
        /// <summary>pic</summary>
        pic,

        /// <summary>nvPicPr</summary>
        nvPicPr,

        /// <summary>cNvPr</summary>
        cNvPr,

        /// <summary>cNvPicPr</summary>
        cNvPicPr,

        /// <summary>blipFill</summary>
        blipFill,

        /// <summary>spPr</summary>
        spPr,
    }
    #endregion DrawingMLPicElementType

    #region RunStyle enumeration
    internal enum RunStyle
    {
        None,
        Hyperlink,
    }
    #endregion RunStyle enumeration

    #region HeaderOrFooter
    internal enum HeaderOrFooter
    {
        Header,
        Footer
    }
    #endregion HeaderOrFooter

    #region LineSpacingRule enumeration





    internal enum LineSpacingRule
    {
        /// <summary>
        /// Specifies that the line spacing is automatically determined
        /// by the size of its contents, with no predetermined minimum or
        /// maximum size.
        /// </summary>
        Auto,

        /// <summary>
        /// Specifies that the height of the line is at least
        /// the value specified, but may be expanded to fit its
        /// content as needed.
        /// </summary>
        AtLeast,

        /// <summary>
        /// Specifies that the height of the line shall be exactly the
        /// value specified, regardless of the size of the contents.
        /// The contents are clipped as necessary under this setting.
        /// </summary>
        Exact,
    }
    #endregion LineSpacingRule enumeration

    #region VmlElementType
    internal enum VmlElementType
    {
        shape,
        rect,
        oval,
        path,
        stroke,
    }
    #endregion VmlElementType

    #region VmlAttributeType
    internal enum VmlAttributeType
    {
        id,
        coordsize,
        ext,
        path,
        filled,
        arrowok,
        fillok,
        shapetype,
        type,
        style,
        strokecolor,
        fillcolor,
        strokeweight,
        href,
        title,
        alt,
        dashstyle,
    }
    #endregion VmlAttributeType

    #region OfficeAttributeType
    internal enum OfficeAttributeType
    {
        spt,
        oned,
        connecttype,
        connectortype,
    }
    #endregion OfficeAttributeType

    #region OfficeElementType
    internal enum OfficeElementType
    {
        _lock,
    }
    #endregion OfficeElementType

    #region OfficeWordElementType
    internal enum OfficeWordElementType
    {
        wrap,
        anchorlock,
    }
    #endregion OfficeWordElementType

    #region OfficeWordAttributeType
    internal enum OfficeWordAttributeType
    {
        type,
        side,
    }
    #endregion OfficeWordAttributeType
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