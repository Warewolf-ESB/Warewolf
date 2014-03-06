using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using System.Text.RegularExpressions;
using Infragistics.Documents.Core;
using Infragistics.Documents.Core.Packaging;


using System.Windows;
using System.Windows.Media;





namespace Infragistics.Documents.Word
{
    #region WordDocumentExportManager class
    /// <summary>
    /// Class used to export data in a format recognized by a consumer
    /// of wordProcessingML-compliant markup.
    /// </summary>
    internal class WordDocumentExportManager : OfficeDocumentExportManager
    {
        #region Member variables
        
        internal const string HyperlinkContentTypeValue = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/hyperlink";
        private WordDocumentPartExporter documentPartExporter = null;
        private ParagraphProperties defaultParagraphProperties = null;
        private SectionProperties finalSectionProperties = null;
        private DefaultTableProperties defaultTableProperties = null;
        private WordDocumentProperties documentProperties = null;
        private Font defaultFont = null;
        private bool hasHyperlinks = false;

        #endregion Member variables

        #region Constructor
        internal WordDocumentExportManager( IPackage zipPackage ) : base( zipPackage )
        {
        }
        #endregion Constructor

        #region DocumentPartExporter
        internal WordDocumentPartExporter DocumentPartExporter
        {
            get { return this.documentPartExporter; }
        }
        #endregion DocumentPartExporter

        #region GetDefaultParagraphProperties
        internal ParagraphProperties GetDefaultParagraphProperties( IUnitOfMeasurementProvider provider )
        {
            if ( this.defaultParagraphProperties == null )
                this.defaultParagraphProperties = new ParagraphProperties( provider );

            return this.defaultParagraphProperties;
        }
        #endregion GetDefaultParagraphProperties

        #region GetFinalSectionProperties
        internal SectionProperties GetFinalSectionProperties( IUnitOfMeasurementProvider provider )
        {
            if ( this.finalSectionProperties == null )
                this.finalSectionProperties = new SectionProperties( provider );

            return this.finalSectionProperties;
        }

        internal SectionProperties GetFinalSectionProperties() { return this.finalSectionProperties; }

        #endregion GetFinalSectionProperties

        #region GetDefaultTableProperties

        internal DefaultTableProperties GetDefaultTableProperties( IUnitOfMeasurementProvider provider )
        {
            if ( this.defaultTableProperties == null )
                this.defaultTableProperties = new DefaultTableProperties( provider );

            return this.defaultTableProperties;
        }

        internal DefaultTableProperties GetDefaultTableProperties() { return this.defaultTableProperties; }

        #endregion GetDefaultTableProperties

        #region GetDocumentProperties
        internal override OfficeDocumentProperties GetDocumentProperties()
        {
            if ( this.documentProperties == null )
                this.documentProperties = new WordDocumentProperties();

            return this.documentProperties;            
        }
        #endregion GetDocumentProperties

        #region GetDefaultFont
        internal Font GetDefaultFont( IUnitOfMeasurementProvider provider )
        {
            if ( this.defaultFont == null )
                this.defaultFont = new Font( provider );

            return this.defaultFont;
        }

        internal Font GetDefaultFont()
        {
            return this.defaultFont;
        }

        #endregion DefaultFont

        #region HasHyperlinks
        internal bool HasHyperlinks
        {
            get { return this.hasHyperlinks; }
            set { this.hasHyperlinks = value; }
        }
        #endregion HasHyperlinks

        #region StartDocument
        /// <summary>
        /// Begins the serialization process for a wordProcessingML-compliant document
        /// </summary>
        public WriteState StartDocument()
        {
            if ( this.documentPartExporter != null )
                return this.documentPartExporter.WriteState;

            this.documentPartExporter = new WordDocumentPartExporter( this );

            //  Create the '/word/document.xml' part.
            this.CreatePartInPackage(
                documentPartExporter,
                WordDocumentPartExporter.DefaultPartName,
                null);

            //  BF 1/22/11  partRelationshipCounters_change
            //  Not sure I fully understand this yet...the part has to
            //  have "itself" on its own stack, this is so things like
            //  hyperlinks and images go in the right place.
            documentPartExporter.PartRelationshipCounters.Push( this.partRelationshipCounters.Peek() );

            if ( documentPartExporter.ContentStream == null )
            {
                SerializationUtilities.DebugFail( "WordDocumentPartExporter.ContentStream returned null but we need one of those." );
                return this.documentPartExporter.WriteState;
            }

            //  Call StartDocument to write the opening tags
            return this.documentPartExporter.StartDocument();
        }
        #endregion StartDocument

        #region EndDocument
        /// <summary>
        /// Begins the serialization process for a wordProcessingML-compliant document
        /// </summary>
        public void EndDocument()
        {
            if ( this.documentPartExporter == null || this.documentPartExporter.ContentStream == null )
                return;

            this.documentPartExporter.EndDocument();

            //  Add the styles part if necessary, before popping the
            //  part relationship counter off the stack, because the
            //  styles part is in the \word part.
            this.AddPendingParts( PendingPartType.Styles );

            //  Pop the counters so we get back to the root part, which
            //  is where the document properties go.
            
            if ( this.partRelationshipCounters != null )
                //  BF 2/4/11   TFS64927
                //  We need to get back to the root part here.
                //this.partRelationshipCounters.Pop();
                this.partRelationshipCounters.Clear();

            //  Add the doc props parts if necessary, before popping the
            //  part relationship counter off the stack, because the
            //  styles part is in the \word part.
            this.AddPendingParts( PendingPartType.DocumentProperties );
        }
        #endregion EndDocument

        #region AddPendingParts
        /// <summary>
        /// Since during the course of writing to the \word\document.xml
        /// it might become necessary to add a part (styles for example)
        /// in which case we need to do that now.
        /// </summary>
        internal void AddPendingParts( PendingPartType partType )
        {
            switch ( partType )
            {
                case PendingPartType.Styles:
                {
                    WordStylesPartExporter stylesPart =
                        new WordStylesPartExporter(
                            this.hasHyperlinks,
                            this.defaultFont, 
                            this.defaultParagraphProperties,
                            this.defaultTableProperties );

                    this.CreatePartInPackage(
                        stylesPart,
                        WordStylesPartExporter.DefaultPartName,
                        null);
                }
                break;

                case PendingPartType.DocumentProperties:
                {
                    if ( this.documentProperties != null )
                        this.SaveDocumentProperties( this.documentProperties );
                }
                break;
            }
        }
        #endregion AddPendingParts

        #region Dispose
        public override void Dispose()
        {
            base.Dispose();

            if ( this.documentPartExporter != null )
            {
                this.documentPartExporter.Dispose();
                this.documentPartExporter = null;
            }
        }
        #endregion Dispose

        #region PendingPartType enumeration
        internal enum PendingPartType
        {
            None,
            Styles,
            DocumentProperties,
        }
        #endregion PendingPartType enumeration
    }
    #endregion WordDocumentExportManager
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