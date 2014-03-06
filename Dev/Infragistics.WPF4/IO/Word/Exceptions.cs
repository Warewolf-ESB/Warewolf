using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.ComponentModel;
using Infragistics.Documents.Core;


using System.Windows;
using System.Windows.Media;







using SR = Infragistics.Shared.SR;


namespace Infragistics.Documents.Word
{
    #region WordDocumentWriterException class
    /// <summary>
    /// Thrown when an error is encountered by the
    /// <see cref="Infragistics.Documents.Word.WordDocumentWriter">WordDocumentWriter</see>.
    /// </summary>

    [InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_IGWord)]

    public class WordDocumentWriterException : Exception
    {
        private WordDocumentWriterExceptionReason reason = WordDocumentWriterExceptionReason.None;

        internal WordDocumentWriterException( WordDocumentWriterExceptionReason reason ) : this( reason, null ){}

        internal WordDocumentWriterException( WordDocumentWriterExceptionReason reason, Exception innerException ) : base(null, innerException)
        {
            this.reason = reason;
        }

        /// <summary>
        /// Returns the message for the exception.
        /// </summary>
        public override string Message
        {
            get { return WordDocumentWriterException.GetExceptionMessage(this.reason); }
        }

        /// <summary>
        /// Returns an enumerated value which identifies the reason for the exception.
        /// </summary>
        public WordDocumentWriterExceptionReason Reason{ get { return this.reason; } }

        static private string GetExceptionMessage( WordDocumentWriterExceptionReason reason )
        {
            switch ( reason )
            {
                case WordDocumentWriterExceptionReason.DocumentNotOpen:
                    return SR.GetString("Exception_DocumentNotOpen");

                case WordDocumentWriterExceptionReason.ParagraphNotOpen:
                    return SR.GetString("Exception_ParagraphNotOpen");

                case WordDocumentWriterExceptionReason.ParagraphNotClosed:
                    return SR.GetString("Exception_ParagraphNotClosed");

                case WordDocumentWriterExceptionReason.ParagraphNotClosedSectionStarted:
                    return SR.GetString("Exception_ParagraphNotClosedSectionStarted");

                case WordDocumentWriterExceptionReason.ParagraphNotClosedTableStarted:
                    return SR.GetString("Exception_ParagraphNotClosedTableStarted");

                case WordDocumentWriterExceptionReason.ParagraphNotClosedTableRowStarted:
                    return SR.GetString("Exception_ParagraphNotClosedTableRowStarted");

                case WordDocumentWriterExceptionReason.ParagraphNotClosedTableCellStarted:
                    return SR.GetString("Exception_ParagraphNotClosedTableCellStarted");

                case WordDocumentWriterExceptionReason.NestedParagraph:
                    return SR.GetString("Exception_NestedParagraph");

                case WordDocumentWriterExceptionReason.NestedTable:
                    return SR.GetString("Exception_NestedTable");

                case WordDocumentWriterExceptionReason.TableNotOpen:
                    return SR.GetString("Exception_TableNotOpen");

                case WordDocumentWriterExceptionReason.TableNotClosed:
                    return SR.GetString("Exception_TableNotClosed");

                case WordDocumentWriterExceptionReason.RowNotOpen:
                    return SR.GetString("Exception_RowNotOpen");

                case WordDocumentWriterExceptionReason.RowNotClosed:
                    return SR.GetString("Exception_RowNotClosed");

                case WordDocumentWriterExceptionReason.NestedRow:
                    return SR.GetString("Exception_NestedRow");

                case WordDocumentWriterExceptionReason.NestedCell:
                    return SR.GetString("Exception_NestedCell");

                case WordDocumentWriterExceptionReason.CellNotOpen:
                    return SR.GetString("Exception_CellNotOpen");

                case WordDocumentWriterExceptionReason.WriterError:
                    return SR.GetString("Exception_WriterError");

                case WordDocumentWriterExceptionReason.HeaderWriterError:
                    return SR.GetString("Exception_HeaderWriterError");

                case WordDocumentWriterExceptionReason.FooterWriterError:
                    return SR.GetString("Exception_FooterWriterError");

                case WordDocumentWriterExceptionReason.DocumentClosureError:
                    return SR.GetString("Exception_DocumentClosureError");

                case WordDocumentWriterExceptionReason.HeaderFooterWriterClosureError:
                    return SR.GetString("Exception_HeaderFooterWriterClosureError");

                case WordDocumentWriterExceptionReason.HeaderFooterWriterNotClosed:
                    return SR.GetString("Exception_HeaderFooterWriterNotClosed");

                case WordDocumentWriterExceptionReason.HeaderFooterWriterNotOpened:
                    return SR.GetString("Exception_HeaderFooterWriterNotOpened");

                case WordDocumentWriterExceptionReason.RowClosedNoCell:
                    return SR.GetString("Exception_RowClosedNoCell");

                case WordDocumentWriterExceptionReason.CellNotClosedTableClosed:
                    return SR.GetString("Exception_CellNotClosedTableClosed");

                case WordDocumentWriterExceptionReason.RowNotClosedTableClosed:
                    return SR.GetString("Exception_RowNotClosedTableClosed");

                case WordDocumentWriterExceptionReason.TableOpenSectionStarted:
                    return SR.GetString("Exception_TableOpenSectionStarted");

                case WordDocumentWriterExceptionReason.RowOpenSectionStarted:
                    return SR.GetString("Exception_RowOpenSectionStarted");

                case WordDocumentWriterExceptionReason.CellOpenSectionStarted:
                    return SR.GetString("Exception_CellOpenSectionStarted");




            }

            return string.Empty;
        }
    }
    #endregion WordDocumentWriterException class

    #region WordDocumentWriterXmlWriterException class
    /// <summary>
    /// Thrown when an exception is thrown by the XMLWriter during the course
    /// of writing content to the main document part.
    /// </summary>

    [InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_IGWord)]

    public class WordDocumentWriterXmlWriterException : WordDocumentWriterException
    {
        private string element = null;
        private string attribute = null;

        internal WordDocumentWriterXmlWriterException( Exception innerException, string element, string attribute ) :
            base(WordDocumentWriterExceptionReason.WriterError, innerException )
        {
            this.element = element;
            this.attribute = attribute;
        }

        /// <summary>
        /// Returns the fully qualified name of the XML element that was
        /// being written at the time the exception was thrown, or null if
        /// there is no associated element.
        /// </summary>
        public string Element { get { return this.element; } }

        /// <summary>
        /// Returns the fully qualified name of the XML attribute that was
        /// being written at the time the exception was thrown, or null if
        /// there is no associated attribute.
        /// </summary>
        public string Attribute { get { return this.attribute; } }
    }
    #endregion WordDocumentWriterXmlWriterException class

    #region WordDocumentWriterExceptionReason
    /// <summary>
    /// Constants which identify the reason an exception was thrown by the
    /// <see cref="Infragistics.Documents.Word.WordDocumentWriter">WordDocumentWriter</see>
    /// class.
    /// </summary>

    [InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_IGWord)]

    public enum WordDocumentWriterExceptionReason
    {
        /// <summary>
        /// Used for variable initialization.
        /// </summary>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        None = 0,

        /// <summary>
        /// A write method was called before the document was opened.
        /// </summary>
        DocumentNotOpen,

        /// <summary>
        /// An attempt was made to add content such as a text run, hyperlink,
        /// or picture when there is currently no open paragraph.
        /// </summary>
        ParagraphNotOpen,

        /// <summary>
        /// An attempt was made to add content such as a row or cell
        /// when there is currently no open table.
        /// </summary>
        TableNotOpen,

        /// <summary>
        /// A previously opened paragraph was not closed.
        /// </summary>
        ParagraphNotClosed,

        /// <summary>
        /// A new table was started before all paragraphs were closed.
        /// </summary>
        ParagraphNotClosedTableStarted,

        /// <summary>
        /// A new table row was started before all paragraphs were closed.
        /// </summary>
        ParagraphNotClosedTableRowStarted,

        /// <summary>
        /// A new table cell was started before all paragraphs were closed.
        /// </summary>
        ParagraphNotClosedTableCellStarted,

        /// <summary>
        /// An attempt was made to start a paragraph when one is already open.
        /// </summary>
        NestedParagraph,

        /// <summary>
        /// A previously opened table was not closed.
        /// </summary>
        TableNotClosed,

        /// <summary>
        /// An attempt was made to start a table when one is already open.
        /// </summary>
        NestedTable,

        /// <summary>
        /// An attempt was made to add content to a table row or close a table row
        /// when there is currently no open row.
        /// </summary>
        RowNotOpen,

        /// <summary>
        /// An attempt was made to add a table row when one is already open in the same table.
        /// </summary>
        NestedRow,

        /// <summary>
        /// A table row was opened, but was not properly closed.
        /// </summary>
        RowNotClosed,

        /// <summary>
        /// An attempt was made to close a table row that has no table cells.
        /// </summary>
        RowClosedNoCell,

        /// <summary>
        /// A table cell was opened, but was not properly closed.
        /// </summary>
        CellNotClosed,

        /// <summary>
        /// A table was closed, but closure of a previously opened table cell is pending.
        /// </summary>
        CellNotClosedTableClosed,

        /// <summary>
        /// A table was closed, but closure of a previously opened table row is pending.
        /// </summary>
        RowNotClosedTableClosed,

        /// <summary>
        /// An attempt was made to add a table cell when one is already open in the same table.
        /// </summary>
        NestedCell,

        /// <summary>
        /// An attempt was made to close a table cell when there is currently no open cell.
        /// </summary>
        CellNotOpen,

        /// <summary>
        /// The XML writer used to serialize the content is in an error state.
        /// </summary>
        WriterError,

        /// <summary>
        /// An exception was thrown during the course of writing content
        /// to a header section part. The inner exception contains more
        /// specific information about the error.
        /// </summary>
        HeaderWriterError,

        /// <summary>
        /// An exception was thrown during the course of writing content
        /// to a footer section part. The inner exception contains more
        /// specific information about the error.
        /// </summary>
        FooterWriterError,

        /// <summary>
        /// An exception was thrown during the course of closing a document.
        /// The inner exception contains more specific information about the error.
        /// </summary>
        DocumentClosureError,

        /// <summary>
        /// An exception was thrown during the course of closing a header or footer writer.
        /// The inner exception contains more specific information about the error.
        /// </summary>
        HeaderFooterWriterClosureError,

        /// <summary>
        /// The document writer was closed, but closure of a header/footer writer is still pending.
        /// </summary>
        HeaderFooterWriterNotClosed,

        /// <summary>
        /// The document writer was closed, but a header/footer writer that was
        /// previously created was never opened.
        /// </summary>
        HeaderFooterWriterNotOpened,

        /// <summary>
        /// A new section was started before all paragraphs were closed.
        /// </summary>
        ParagraphNotClosedSectionStarted,

        /// <summary>
        /// An attempt was made to define a section when a table was open.
        /// </summary>
        TableOpenSectionStarted,

        /// <summary>
        /// An attempt was made to define a section when a table row was open.
        /// </summary>
        RowOpenSectionStarted,

        /// <summary>
        /// An attempt was made to define a section when a table cell was open.
        /// </summary>
        CellOpenSectionStarted,



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)


    }
    #endregion WordDocumentWriterExceptionReason
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