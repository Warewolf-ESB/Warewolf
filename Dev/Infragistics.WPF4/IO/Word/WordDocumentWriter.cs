using System;
using System.Collections.Generic;
using System.ComponentModel;
//using System.Data;
using System.Text;
//using Infragistics.Documents.IO;
using System.IO;
using System.Xml;
using Infragistics.Documents.Core.Packaging;
using WORD = Infragistics.Documents.Word;



using System.Windows;
using System.Windows.Media;
using SizeF = System.Windows.Size;
using Image = System.Windows.Media.Imaging.BitmapSource;







using SR = Infragistics.Shared.SR;


namespace Infragistics.Documents.Word
{
    #region WordDocumentWriter class
    /// <summary>
    /// Abstract class which provides a fast, non-cached, forward-only way
    /// of generating streams or files containing word processing data.
    /// </summary>
    /// <seealso cref="Infragistics.Documents.Word.WordprocessingMLWriter">WordprocessingMLWriter class</seealso>

    [InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_IGWord)]

    public abstract class WordDocumentWriter :
        IUnitOfMeasurementProvider,
        IDisposable,
        IContentWriter
    {
        #region Member variables

        private UnitOfMeasurement unit = WordUtilities.DefaultUnitOfMeasurement;
        private List<WordHeaderFooterWriter> headerFooterWriters = null;
        private NewLineType newLineType = WordUtilities.DefaultNewLineType;
        private TextRun addNewLineTextRun = null;
        private TextRun tempTextRun = null;





        #endregion Member variables

        #region Create

        /// <summary>
        /// Creates and returns a new instance of a WordDocumentWriter-derived class.
        /// </summary>
        /// <param name="filename">The fully qualified path to the Office document file to be created.</param>
        /// <returns>A reference to the WordDocumentWriter-derived instance that was created.</returns>
        static public WordDocumentWriter Create( string filename )
        {
            FileStream fileStream = new FileStream( filename, FileMode.Create, FileAccess.ReadWrite );
            return WordDocumentWriter.Create( fileStream, null, true );
        }

        //  BF 1/18/11
        //  We want CLR2 consumers to see this so they are lured into calling it
        //  and getting a nice descriptive exception thrown right in their face.

        /// <summary>
        /// Creates and returns a new instance of a WordDocumentWriter-derived class.
        /// </summary>
        /// <param name="stream">The System.IO.Stream to which the content is persisted.</param>
        /// <returns>A reference to the WordDocumentWriter-derived instance that was created.</returns>
        static public WordDocumentWriter Create( Stream stream )
        {
            return WordDocumentWriter.Create( stream, null, false );
        }

        /// <summary>
        /// Creates and returns a new instance of a WordDocumentWriter-derived class.
        /// </summary>
        /// <param name="stream">The System.IO.Stream to which the content is persisted.</param>
        /// <param name="packageFactory">
        /// An
        /// <see cref="Infragistics.Documents.Core.Packaging.IPackageFactory">IPackageFactory</see>
        /// implementor which handles the packaging of the output file stream.
        /// </param>
        /// <returns>A reference to the WordDocumentWriter-derived instance that was created.</returns>

        //  Hide this if CLR3 since nobody in their right mind could
        //  possibly have any reason to want to use this.
        [Browsable(false)]
        [EditorBrowsableAttribute(EditorBrowsableState.Advanced)]

        static public WordDocumentWriter Create( Stream stream, IPackageFactory packageFactory )
        {
            return WordDocumentWriter.Create( stream, packageFactory, false );
        }

        static internal WordDocumentWriter Create( Stream stream, IPackageFactory packageFactory, bool weOwnStream )
        {
            //  BF 1/18/11
            //  We actually want to draw attention to this for CLR2 consumers,
            //  because a large majority of them can use the CLR3 version, and
            //  Package support is native on CLR3.




            WordprocessingMLWriter writer = new WordprocessingMLWriter( stream, packageFactory, weOwnStream );
            return writer;
        }

        #endregion Create

        #region Properties

        #region DocumentProperties
        /// <summary>
        /// Defines the properties of the document such as Title, Author, Manager, etc.
        /// </summary>
        public virtual WordDocumentProperties DocumentProperties { get { return null; } }
        #endregion DefaultParagraphProperties

        #region DefaultParagraphProperties
        /// <summary>
        /// Defines the default formatting for paragraphs which have
        /// no more specific formatting attributes defined.
        /// </summary>
        public virtual ParagraphProperties DefaultParagraphProperties { get { return null; } }
        #endregion DefaultParagraphProperties

        #region FinalSectionProperties
        /// <summary>
        /// Defines the page-related properties of the document such as size,
        /// margins, and orientation. Applies to all content in the document
        /// which follows the last explicitly defined section.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// This property defines the pagination for all content within the
        /// document which is not explicitly associated with a section defined
        /// via the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.DefineSection(SectionProperties)">DefineSection</see>
        /// method. Sections defined by that method apply to all content that was written
        /// prior to the method call; from this it follows that once the DefineSection method
        /// is called, pagination is then explicitly defined for all previously written content
        /// in the document. As such, the pagination as defined by this property applies to the
        /// content that was written after the last call to DefineSection, or to the "final"
        /// section of the document.
        /// </p>
        /// <p class="body">
        /// In the case where the 
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.DefineSection(SectionProperties)">DefineSection</see>
        /// method is not used, this property defines the pagination for the entire document.
        /// </p>
        /// <p class="body">
        /// Headers and/or footers can be defined for the final section using the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.AddSectionHeaderFooter(SectionHeaderFooterParts)">AddHeaderFooterSection</see>
        /// method.
        /// </p>
        /// </remarks>
        /// <seealso cref="Infragistics.Documents.Word.WordDocumentWriter.DefineSection(SectionProperties)">DefineSection</seealso>
        public virtual SectionProperties FinalSectionProperties { get { return null; } }
        #endregion FinalSectionProperties

        #region DefaultTableProperties
        /// <summary>
        /// Defines the default formatting for tables which have
        /// no more specific formatting attributes defined.
        /// </summary>
        public virtual DefaultTableProperties DefaultTableProperties { get { return null; } }
        #endregion DefaultTableProperties

        #region DefaultFont
        /// <summary>
        /// Returns the font that determines the formatting for all text runs
        /// which do not have more specific formatting attributes defined.
        /// </summary>
        public virtual WORD.Font DefaultFont { get { return null; } }

        #endregion DefaultFont

        #region Unit
        /// <summary>
        /// Returns or sets a value indicating the implied unit of measurement
        /// for properties which represent graphical quantities.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// The default unit of measurement for the properties which represent
        /// graphical quantities is the desktop publishing point, which is equivalent
        /// to 1/72 of an inch. This property can be changed to redefine that unit of
        /// measurement, so that graphical properties can be interacted with using units
        /// that are most comfortable to the end developer.
        /// </p>
        /// <p class="body">
        /// All property values are stored internally as twips (twentieths of a point),
        /// and coverted as appropriate in the property's get and set method implementations.
        /// The value of the Unit property can be changed at any time by the developer
        /// without affecting previously persisted property values.
        /// </p>
        /// </remarks>
        public UnitOfMeasurement Unit
        {
            get { return this.unit; }
            set { this.unit = value; }
        }
        #endregion Unit

        #region WriterState
        /// <summary>
        /// Returns a bitflag value which describes the current state of the writer.
        /// </summary>
        public abstract WordDocumentWriterState WriterState { get; }
        #endregion WriterState

        #region NewLineType
        /// <summary>
        /// Returns or sets a value which determines the method used to
        /// represent a newline in the generated output.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// The newline character(s) can be represented as either a line break
        /// or a carriage return. A document consumer may interpret a line break
        /// or carriage return differently depending on the circumstances under
        /// which it is used. For example, paragraph justification can behave
        /// differently in MS Word depending on whether a line break is contained
        /// within the paragraph. A carriage return is recognized in a standard
        /// paragraph as a line-breaking character, but is not recognized when the
        /// paragraph is contained within a table cell.
        /// </p>
        /// </remarks>
        public virtual NewLineType NewLineType
        {
            get { return this.newLineType; }
            set { this.newLineType = value; }
        }

        static internal NewLineType ResolveNewLineType( NewLineType? value, NewLineType defaultValue )
        {
            return value.HasValue ? value.Value : defaultValue;
        }

        #endregion NewLineType

        #region ImageEncoder


#region Infragistics Source Cleanup (Region)










#endregion // Infragistics Source Cleanup (Region)

        #endregion ImageEncoder
        
        #endregion Properties

        #region Methods

        #region CreateParagraphProperties
        /// <summary>
        /// Creates a new
        /// <see cref="Infragistics.Documents.Word.ParagraphProperties">ParagraphProperties</see>
        /// instance.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// This method returns an instance which contains a reference to this class,
        /// which enables the instance to observe the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.Unit">Unit</see>
        /// property setting, so that the developer can work in the unit of measure with which
        /// he is most comfortable.
        /// </p>
        /// <p class="body">
        /// The instance returned from this method can be passed to the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.StartParagraph(ParagraphProperties)">StartParagraph</see>
        /// method to specify the formatting attributes for the paragraph.
        /// </p>
        /// </remarks>
        public ParagraphProperties CreateParagraphProperties()
        {
            return ParagraphProperties.Create(this) as ParagraphProperties;
        }
        #endregion CreateParagraphProperties

        #region CreateTableProperties
        /// <summary>
        /// Creates a new
        /// <see cref="Infragistics.Documents.Word.TableProperties">TableProperties</see>
        /// instance.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// This method returns an instance which contains a reference to this class,
        /// which enables the instance to observe the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.Unit">Unit</see>
        /// property setting, so that the developer can work in the unit of measure with which
        /// he is most comfortable.
        /// </p>
        /// <p class="body">
        /// The instance returned from this method can be passed to the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.StartTable(int)">StartTable</see>
        /// method to specify the formatting attributes for the table.
        /// </p>
        /// </remarks>
        public TableProperties CreateTableProperties()
        {
            return TableProperties.Create(this) as TableProperties;
        }
        #endregion CreateTableProperties

        #region CreateTableRowProperties
        /// <summary>
        /// Creates a new
        /// <see cref="Infragistics.Documents.Word.TableRowProperties">TableRowProperties</see>
        /// instance.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// This method returns an instance which contains a reference to this class,
        /// which enables the instance to observe the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.Unit">Unit</see>
        /// property setting, so that the developer can work in the unit of measure with which
        /// he is most comfortable.
        /// </p>
        /// <p class="body">
        /// The instance returned from this method can be passed to the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.StartTableRow(TableRowProperties)">StartTableRow</see>
        /// method to specify the formatting attributes for the row.
        /// </p>
        /// </remarks>
        public TableRowProperties CreateTableRowProperties()
        {
            return TableRowProperties.Create(this) as TableRowProperties;
        }
        #endregion CreateTableRowProperties

        #region CreateTableCellProperties
        /// <summary>
        /// Creates a new
        /// <see cref="Infragistics.Documents.Word.TableCellProperties">TableCellProperties</see>
        /// instance.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// This method returns an instance which contains a reference to this class,
        /// which enables the instance to observe the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.Unit">Unit</see>
        /// property setting, so that the developer can work in the unit of measure with which
        /// he is most comfortable.
        /// </p>
        /// <p class="body">
        /// The instance returned from this method can be passed to the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.StartTableCell(TableCellProperties)">StartTableCell</see>
        /// method to specify the formatting attributes for the cell.
        /// </p>
        /// </remarks>
        public TableCellProperties CreateTableCellProperties()
        {
            return TableCellProperties.Create(this) as TableCellProperties;
        }
        #endregion CreateTableCellProperties

        #region CreateFont
        /// <summary>
        /// Creates a new
        /// <see cref="Infragistics.Documents.Word.Font">Font</see>
        /// instance which can be used to format text runs.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// This method returns a font which contains a reference to this class,
        /// which enables the font to observe the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.Unit">Unit</see>
        /// property setting, so that the developer can work in the unit of measure with which
        /// he is most comfortable.
        /// </p>
        /// <p class="body">
        /// The font returned from this method can be passed to the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.AddTextRun(string, Font)">AddTextRun</see>
        /// method to specify the font attributes for the run.
        /// </p>
        /// </remarks>
        public WORD.Font CreateFont()
        {
            return new WORD.Font( this );
        }
        #endregion CreateFont

        #region CreateAnchoredPicture

        /// <summary>
        /// Creates a new
        /// <see cref="Infragistics.Documents.Word.AnchoredPicture">AnchoredPicture</see>.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// This method returns an instance which contains a reference to this class,
        /// which enables the instance to observe the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.Unit">Unit</see>
        /// property setting, so that the developer can work in the unit of measure with which
        /// he is most comfortable.
        /// </p>
        /// <p class="body">
        /// The instance returned from this method can be passed to the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.AddAnchoredPicture(Image)">AddAnchoredPicture</see>
        /// method.
        /// </p>
        /// </remarks>
        public AnchoredPicture CreateAnchoredPicture( Image image )
        {
            if ( image == null )
                throw new ArgumentNullException();

            return new AnchoredPicture( this, image );
        }

        #endregion CreateAnchoredPicture

        #region CreateAnchoredShape

        /// <summary>
        /// Creates a new
        /// <see cref="Infragistics.Documents.Word.AnchoredShape">AnchoredShape</see>.
        /// </summary>
        /// <param name="shapeType">
        /// A
        /// <see cref="Infragistics.Documents.Word.ShapeType">ShapeType</see>
        /// constant which defines the shape.
        /// </param>
        /// <remarks>
        /// <p class="body">
        /// This method returns an instance which contains a reference to this class,
        /// which enables the instance to observe the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.Unit">Unit</see>
        /// property setting, so that the developer can work in the unit of measure with which
        /// he is most comfortable.
        /// </p>
        /// <p class="body">
        /// The instance returned from this method can be passed to the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.AddAnchoredShape(ShapeType)">AddAnchoredShape</see>
        /// method.
        /// </p>
        /// </remarks>
        public AnchoredShape CreateAnchoredShape( ShapeType shapeType )
        {
            return AnchoredShape.Create( this, shapeType );
        }

        /// <summary>
        /// Creates a new
        /// <see cref="Infragistics.Documents.Word.AnchoredShape">AnchoredShape</see>.
        /// </summary>
        /// <param name="shape">
        /// A previously created
        /// <see cref="Infragistics.Documents.Word.Shape">Shape</see>
        /// instance on which the returned AnchoredShape instance is based.
        /// </param>
        /// <remarks>
        /// <p class="body">
        /// This method returns an instance which contains a reference to this class,
        /// which enables the instance to observe the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.Unit">Unit</see>
        /// property setting, so that the developer can work in the unit of measure with which
        /// he is most comfortable.
        /// </p>
        /// <p class="body">
        /// The instance returned from this method can be passed to the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.AddAnchoredShape(AnchoredShape)">AddAnchoredShape</see>
        /// method.
        /// </p>
        /// </remarks>
        public AnchoredShape CreateAnchoredShape( Shape shape )
        {
            return AnchoredShape.Create( this, shape );
        }

        #endregion CreateAnchoredShape

        #region CreateVmlShape
        /// <summary>
        /// Creates a new
        /// <see cref="Infragistics.Documents.Word.VmlShape">VmlShape</see>-derived instance
        /// based on the specified <paramref name="shapeType"/>.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// This method returns an instance which contains a reference to this class,
        /// which enables the instance to observe the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.Unit">Unit</see>
        /// property setting, so that the developer can work in the unit of measure with which
        /// he is most comfortable.
        /// </p>
        /// <p class="body">
        /// The instance returned from this method can be passed to the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.AddInlineShape(InlineShape)">AddInlineShape</see>
        /// method.
        /// </p>
        /// </remarks>
        public VmlShape CreateVmlShape( ShapeType shapeType )
        {
            return VmlShape.Create( this, shapeType );
        }
        #endregion CreateVmlShape

        #region CreatePictureOutlineProperties

        /// <summary>
        /// Creates a new
        /// <see cref="Infragistics.Documents.Word.PictureOutlineProperties">PictureOutlineProperties</see>.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// This method returns an instance which contains a reference to this class,
        /// which enables the instance to observe the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.Unit">Unit</see>
        /// property setting, so that the developer can work in the unit of measure with which
        /// he is most comfortable.
        /// </p>
        /// <p class="body">
        /// The instance returned from this method can be passed to the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.AddInlinePicture(Image)">AddInlinePicture</see>
        /// method.
        /// </p>
        /// </remarks>
        public PictureOutlineProperties CreatePictureOutlineProperties()
        {
            return new PictureOutlineProperties( this );
        }

        #endregion CreatePictureOutlineProperties

        #region CreateSectionProperties
        /// <summary>
        /// Creates a new
        /// <see cref="Infragistics.Documents.Word.SectionProperties">SectionProperties</see>.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// This method returns an instance which contains a reference to this class,
        /// which enables the instance to observe the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.Unit">Unit</see>
        /// property setting, so that the developer can work in the unit of measure with which
        /// he is most comfortable.
        /// </p>
        /// <p class="body">
        /// The instance returned from this method can be passed to the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.DefineSection(SectionProperties)">DefineSection</see>
        /// method.
        /// </p>
        /// </remarks>
        public SectionProperties CreateSectionProperties()
        {
            return SectionProperties.Create( this );
        }
        #endregion CreateSectionProperties

        #region CreateTableBorderProperties
        /// <summary>
        /// Creates a new
        /// <see cref="Infragistics.Documents.Word.TableBorderProperties">TableBorderProperties</see>
        /// instance.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// This method returns an instance which contains a reference to this class,
        /// which enables the instance to observe the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.Unit">Unit</see>
        /// property setting, so that the developer can work in the unit of measure with which
        /// he is most comfortable.
        /// </p>
        /// </remarks>
        public TableBorderProperties CreateTableBorderProperties()
        {
            return new TableBorderProperties( this );
        }
        #endregion CreateTableProperties

        #region CreateTextHyperlink
        
        /// <summary>
        /// Creates a new
        /// <see cref="Infragistics.Documents.Word.TextHyperlink">TextHyperlink</see>
        /// instance which can be used to add a textual hyperlink to the document.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// The instance returned from this method can be passed to the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.AddHyperlink(TextHyperlink)">AddHyperlink</see>
        /// method.
        /// </p>
        /// </remarks>
        public WORD.TextHyperlink CreateTextHyperlink( string address )
        {
            return this.CreateTextHyperlink( address, null );
        }

        /// <summary>
        /// Creates a new
        /// <see cref="Infragistics.Documents.Word.TextHyperlink">TextHyperlink</see>
        /// instance which can be used to add a textual hyperlink to the document.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// The instance returned from this method can be passed to the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.AddHyperlink(TextHyperlink)">AddHyperlink</see>
        /// method.
        /// </p>
        /// </remarks>
        public WORD.TextHyperlink CreateTextHyperlink( string address, string text )
        {
            Hyperlink.VerifyAddress( ref address, false );
            return WORD.TextHyperlink.Create( this, address, text );
        }

        #endregion CreateTextHyperlink

        #region CreateInlinePicture

        /// <summary>
        /// Creates a new
        /// <see cref="Infragistics.Documents.Word.InlinePicture">InlinePicture</see>.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// This method returns an instance which contains a reference to this class,
        /// which enables the instance to observe the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.Unit">Unit</see>
        /// property setting, so that the developer can work in the unit of measure with which
        /// he is most comfortable.
        /// </p>
        /// <p class="body">
        /// The instance returned from this method can be passed to the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.AddInlinePicture(InlinePicture)">AddInlinePicture</see>
        /// method.
        /// </p>
        /// </remarks>
        public InlinePicture CreateInlinePicture( Image image )
        {
            return InlinePicture.Create( this, image );
        }

        #endregion CreateInlinePicture

        #region CreateInlineShape

        /// <summary>
        /// Creates a new
        /// <see cref="Infragistics.Documents.Word.InlineShape">InlineShape</see>.
        /// </summary>
        /// <param name="shapeType">
        /// A
        /// <see cref="Infragistics.Documents.Word.ShapeType">ShapeType</see>
        /// constant which defines the shape.
        /// </param>
        /// <remarks>
        /// <p class="body">
        /// This method returns an instance which contains a reference to this class,
        /// which enables the instance to observe the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.Unit">Unit</see>
        /// property setting, so that the developer can work in the unit of measure with which
        /// he is most comfortable.
        /// </p>
        /// <p class="body">
        /// The instance returned from this method can be passed to the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.AddInlineShape(InlineShape)">AddInlineShape</see>
        /// method.
        /// </p>
        /// </remarks>
        public InlineShape CreateInlineShape( ShapeType shapeType )
        {
            return InlineShape.Create( this, shapeType );
        }

        /// <summary>
        /// Creates a new
        /// <see cref="Infragistics.Documents.Word.InlineShape">InlineShape</see>.
        /// </summary>
        /// <param name="shape">
        /// A previously created
        /// <see cref="Infragistics.Documents.Word.Shape">Shape</see>
        /// instance on which the returned InlineShape instance is based.
        /// </param>
        /// <remarks>
        /// <p class="body">
        /// This method returns an instance which contains a reference to this class,
        /// which enables the instance to observe the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.Unit">Unit</see>
        /// property setting, so that the developer can work in the unit of measure with which
        /// he is most comfortable.
        /// </p>
        /// <p class="body">
        /// The instance returned from this method can be passed to the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.AddInlineShape(InlineShape)">AddInlineShape</see>
        /// method.
        /// </p>
        /// </remarks>
        public InlineShape CreateInlineShape( Shape shape )
        {
            return InlineShape.Create( this, shape );
        }

        #endregion CreateInlineShape

        #region CreatePageNumberField

        /// <summary>
        /// Creates a new
        /// <see cref="Infragistics.Documents.Word.PageNumberField">PageNumberField</see>.
        /// </summary>
        /// <param name="format">
        /// A
        /// <see cref="Infragistics.Documents.Word.PageNumberFieldFormat">PageNumberFieldFormat</see>
        /// constant which defines the format for the page number field.
        /// </param>
        /// <remarks>
        /// <p class="body">
        /// This method returns an instance which contains a reference to this class,
        /// which enables the instance to observe the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.Unit">Unit</see>
        /// property setting, so that the developer can work in the unit of measure with which
        /// he is most comfortable.
        /// </p>
        /// <p class="body">
        /// The instance returned from this method can be passed to the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.AddInlineShape(InlineShape)">AddInlineShape</see>
        /// method.
        /// </p>
        /// </remarks>
        public PageNumberField CreatePageNumberField( PageNumberFieldFormat format )
        {
            return new PageNumberField( this, format );
        }

        #endregion CreatePageNumberField

        #region StartDocument
        /// <summary>
        /// Starts a word processing document.
        /// </summary>
        public abstract void StartDocument();
        #endregion StartDocument

        #region EndDocument

        /// <summary>
        /// Closes a previously opened document.
        /// </summary>
        /// <seealso cref="Infragistics.Documents.Word.WordDocumentWriter.StartDocument">StartDocument</seealso>
        /// <exception cref="WordDocumentWriterException">Thrown if no document is currently open.</exception>
        public void EndDocument()
        {
            this.EndDocument( false );
        }

        /// <summary>
        /// Closes a previously opened document, optionally closing the writer.
        /// </summary>
        /// <param name="closeWriter">
        /// A boolean value which specifies whether the writer should also be closed.
        /// </param>
        /// <seealso cref="Infragistics.Documents.Word.WordDocumentWriter.StartDocument">StartDocument</seealso>
        /// <exception cref="WordDocumentWriterException">Thrown if no document is currently open.</exception>
        public abstract void EndDocument( bool closeWriter );
        #endregion EndDocument

        #region AddEmptyParagraph
        /// <summary>
        /// Adds a paragraph with no content.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// Word processing content consumers such as MS Word may retain the previous
        /// paragraph's properties when a new paragraph is added. Adding an empty
        /// paragraph after a paragraph with non-default formatting attributes
        /// prevents subsequent paragraphs from inheriting these attributes.
        /// </p>
        /// </remarks>
        public void AddEmptyParagraph()
        {
            this.StartParagraph();
            this.EndParagraph();
        }
        #endregion AddEmptyParagraph

        #region StartParagraph

        /// <summary>
        /// Starts a new paragraph in the document with the default formatting.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// This method starts a &lt;paragraph&gt; element.
        /// Callers are responsible for closing this element. The
        /// &lt;paragraph&gt; element can be closed by calling the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.EndParagraph">EndParagraph</see>
        /// method.
        /// </p>
        /// <p class="body">
        /// This overload uses the default alignment (left) with no indentation.
        /// To specify formatting attributes such as alignment, indentation, etc., use the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.StartParagraph(ParagraphProperties)">StartParagraph(ParagraphProperties)</see>
        /// method.
        /// </p>
        /// </remarks>
        /// <seealso cref="Infragistics.Documents.Word.WordDocumentWriter.EndParagraph">EndParagraph</seealso>
        /// <exception cref="WordDocumentWriterException">Thrown if no document is currently open.</exception>
        public void StartParagraph()
        {
            this.StartParagraph( null );
        }

        /// <summary>
        /// Starts a new paragraph in the document.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// This method starts a &lt;p&gt; element.
        /// Callers are responsible for closing this element. The
        /// &lt;p&gt; element can be closed by calling the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.EndParagraph">EndParagraph</see>
        /// method.
        /// </p>
        /// </remarks>
        /// <param name="properties">
        /// A
        /// <see cref="Infragistics.Documents.Word.ParagraphProperties">ParagraphProperties</see>
        /// instance which defines properties such as alignment and indentation for the paragraph.
        /// An new instance of this class can be obtained by calling the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.CreateParagraphProperties">CreateParagraphProperties</see>
        /// method.
        /// </param>
        /// <exception cref="WordDocumentWriterException">Thrown if no document is currently open.</exception>
        public abstract void StartParagraph( ParagraphProperties properties );

        #endregion StartParagraph

        #region EndParagraph

        /// <summary>
        /// Closes a previously created paragraph.
        /// </summary>
        /// <seealso cref="Infragistics.Documents.Word.WordDocumentWriter.StartParagraph()">StartParagraph</seealso>
        /// <exception cref="WordDocumentWriterException">Thrown if no document or paragraph is currently open.</exception>
        public abstract void EndParagraph();

        #endregion EndParagraph

        #region AddNewLine

        /// <summary>
        /// Adds a text run consisting of the newline character to the current paragraph.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// The actual character code(s) used in the generated content are determined by the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.NewLineType">NewLineType</see>
        /// property.
        /// </p>
        /// <p class="body">
        /// Before the AddNewLine method can be called, the developer must begin a paragraph via the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.StartParagraph()">StartParagraph</see>
        /// method, or an exception is thrown.
        /// </p>
        /// <p class="body">
        /// Some consumers do not always recognize newline characters within the content.
        /// For example, MS Word does not escape the current line when the paragraph appears
        /// within a table cell. The
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.AddEmptyParagraph">AddEmptyParagraph</see>
        /// method can be used as an alternative to this method, although this requires closing the current
        /// paragraph prior to calling the method.
        /// </p>
        /// </remarks>
        /// <exception cref="WordDocumentWriterException">Thrown if no document or paragraph is currently open.</exception>
        /// <exception cref="WordDocumentWriterXmlWriterException">Thrown if the XmlWriter used to write content to the stream throws an exception.</exception>
        public void AddNewLine()
        {
            this.AddNewLine( this.NewLineType );
        }

        /// <summary>
        /// Adds a text run consisting of the newline character to the current paragraph.
        /// </summary>
        /// <param name="newLineType">
        /// A
        /// <see cref="Infragistics.Documents.Word.NewLineType">NewLineType</see>
        /// constant which describes the method used to define the actual character codes
        /// used in the generated output.
        /// </param>
        /// <remarks>
        /// <p class="body">
        /// Before the AddNewLine method can be called, the developer must begin a paragraph via the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.StartParagraph()">StartParagraph</see>
        /// method, or an exception is thrown.
        /// </p>
        /// <p class="body">
        /// Some consumers do not always recognize newline characters within the content.
        /// For example, MS Word does not escape the current line when the paragraph appears
        /// within a table cell. The
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.AddEmptyParagraph">AddEmptyParagraph</see>
        /// method can be used as an alternative to this method, although this requires closing the current
        /// paragraph prior to calling the method.
        /// </p>
        /// </remarks>
        /// <exception cref="WordDocumentWriterException">Thrown if no document or paragraph is currently open.</exception>
        /// <exception cref="WordDocumentWriterXmlWriterException">Thrown if the XmlWriter used to write content to the stream throws an exception.</exception>
        public void AddNewLine( NewLineType newLineType )
        {
            TextRun tr = this.GetAddNewLineTextRun(newLineType);
            this.AddTextRun( tr );
        }

        internal TextRun GetAddNewLineTextRun( NewLineType newLineType )
        {
            if ( this.addNewLineTextRun == null )
            {
                this.addNewLineTextRun = new TextRun(Environment.NewLine, null);
                this.addNewLineTextRun.NewLineType = newLineType;
            }

            return this.addNewLineTextRun;
        }

        #endregion AddNewLine

        #region AddTextRun

        /// <summary>
        /// Adds a text run to the current paragraph.
        /// </summary>
        /// <param name="text">The string value to write.</param>
        /// <remarks>
        /// <p class="body">
        /// Before the AddTextRun method can be called, the developer must begin a paragraph via the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.StartParagraph()">StartParagraph</see>
        /// method, or an exception is thrown.
        /// </p>
        /// </remarks>
        /// <exception cref="WordDocumentWriterException">Thrown if no document or paragraph is currently open.</exception>
        /// <exception cref="WordDocumentWriterXmlWriterException">Thrown if the XmlWriter used to write content to the stream throws an exception.</exception>
        public void AddTextRun( string text )
        {
            this.AddTextRun( text, null );
        }

        /// <summary>
        /// Adds a text run to the current paragraph.
        /// </summary>
        /// <param name="text">The string value to write.</param>
        /// <param name="font">The font that is applied to all characters in the run.</param>
        /// <remarks>
        /// <p class="body">
        /// A new Font instance can be obtained using the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.CreateFont">CreateFont</see>
        /// method. An existing Font instance can also be used; changing properties on
        /// that instance does not affect previously added text runs because the information
        /// is written to the stream immediately.
        /// </p>
        /// <p class="body">
        /// Before the AddTextRun method can be called, the developer must begin a paragraph via the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.StartParagraph()">StartParagraph</see>
        /// method, or an exception is thrown.
        /// </p>
        /// </remarks>
        /// <exception cref="WordDocumentWriterException">Thrown if no document or paragraph is currently open.</exception>
        /// <exception cref="WordDocumentWriterXmlWriterException">Thrown if the XmlWriter used to write content to the stream throws an exception.</exception>
        public void AddTextRun( string text, WORD.Font font )
        {
            //  Reduce object instantiations (of a TextRun) by keeping
            //  a temp object so callers don't get penalized for using
            //  these overloads
            TextRun tr = this.GetTempTextRun( text, font );
            this.AddTextRun( tr );
        }

        /// <summary>
        /// Adds a text run to the current paragraph.
        /// </summary>
        /// <param name="text">The string value to write.</param>
        /// <param name="font">The font that is applied to all characters in the run.</param>
        /// <param name="checkSpellingAndGrammar">A boolean value specifying whether the contents of the run are checked for spelling and grammar.</param>
        /// <remarks>
        /// <p class="body">
        /// Before the AddTextRun method can be called, the developer must begin a paragraph via the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.StartParagraph()">StartParagraph</see>
        /// method, or an exception is thrown.
        /// </p>
        /// </remarks>
        /// <exception cref="WordDocumentWriterException">Thrown if no document or paragraph is currently open.</exception>
        /// <exception cref="WordDocumentWriterXmlWriterException">Thrown if the XmlWriter used to write content to the stream throws an exception.</exception>
        public void AddTextRun( string text, WORD.Font font, bool? checkSpellingAndGrammar )
        {
            TextRun textRun = new TextRun( text, font );
            textRun.CheckSpellingAndGrammar = checkSpellingAndGrammar;
            this.AddTextRun( textRun );
        }

        /// <summary>
        /// Adds a text run to the current paragraph.
        /// </summary>
        /// <param name="textRun">
        /// A
        /// <see cref="Infragistics.Documents.Word.TextRun">TextRun</see>
        /// instance which defines the properties of the text run.
        /// </param>
        /// <remarks>
        /// <p class="body">
        /// Before the AddTextRun method can be called, the developer must begin a paragraph via the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.StartParagraph()">StartParagraph</see>
        /// method, or an exception is thrown.
        /// </p>
        /// </remarks>
        /// <exception cref="WordDocumentWriterException">Thrown if no document or paragraph is currently open.</exception>
        /// <exception cref="WordDocumentWriterXmlWriterException">Thrown if the XmlWriter used to write content to the stream throws an exception.</exception>
        public abstract void AddTextRun( TextRun textRun );

        internal TextRun GetTempTextRun( string text, WORD.Font font )
        {
            if ( this.tempTextRun == null )
                this.tempTextRun = new TextRun( this, text, font );
            else
            {
                this.tempTextRun.Reset( text );

                //  TFS70792
                this.tempTextRun.Font = font;
            }

            return this.tempTextRun;
        }

        #endregion AddTextRun

        #region AddHyperlink

        /// <summary>
        /// Adds a textual hyperlink to the current paragraph.
        /// </summary>
        /// <param name="address">The address for the hyperlink.</param>
        /// <param name="text">The text displayed for the hyperlink.</param>
        /// <remarks>
        /// <p class="body">
        /// Before the AddHyperlink method can be called, the developer must begin a paragraph via the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.StartParagraph()">StartParagraph</see>
        /// method, or an exception is thrown.
        /// </p>
        /// </remarks>
        /// <exception cref="WordDocumentWriterException">Thrown if no document or paragraph is currently open.</exception>
        /// <exception cref="WordDocumentWriterXmlWriterException">Thrown if the XmlWriter used to write content to the stream throws an exception.</exception>
        public void AddHyperlink( string address, string text )
        {
            Font font = null;
            this.AddHyperlink( address, text, font ); 
        }

        /// <summary>
        /// Adds a textual hyperlink to the current paragraph.
        /// </summary>
        /// <param name="address">The address for the hyperlink.</param>
        /// <param name="text">The text displayed for the hyperlink.</param>
        /// <param name="font">
        /// The
        /// <see cref="Infragistics.Documents.Word.Font">Font</see>
        /// which defines the formatting attributes for the text that is displayed.
        /// </param>
        /// <remarks>
        /// <p class="body">
        /// Before the AddHyperlink method can be called, the developer must begin a paragraph via the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.StartParagraph()">StartParagraph</see>
        /// method, or an exception is thrown.
        /// </p>
        /// </remarks>
        /// <exception cref="WordDocumentWriterException">Thrown if no document or paragraph is currently open.</exception>
        /// <exception cref="WordDocumentWriterXmlWriterException">Thrown if the XmlWriter used to write content to the stream throws an exception.</exception>
        public void AddHyperlink( string address, string text, Font font )
        {
            List<TextRun> textRuns = TextRun.ToList( new TextRun(text, font) );
            this.AddHyperlink( address, textRuns, null ); 
        }

        /// <summary>
        /// Adds a textual hyperlink to the current paragraph.
        /// </summary>
        /// <param name="address">The address for the hyperlink.</param>
        /// <param name="text">The text displayed for the hyperlink.</param>
        /// <param name="toolTipText">The text that is displayed when the user hovers the cursor over this hyperlink.</param>
        /// <remarks>
        /// <p class="body">
        /// Before the AddHyperlink method can be called, the developer must begin a paragraph via the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.StartParagraph()">StartParagraph</see>
        /// method, or an exception is thrown.
        /// </p>
        /// </remarks>
        /// <exception cref="WordDocumentWriterException">Thrown if no document or paragraph is currently open.</exception>
        /// <exception cref="WordDocumentWriterXmlWriterException">Thrown if the XmlWriter used to write content to the stream throws an exception.</exception>
        public void AddHyperlink( string address, string text, string toolTipText )
        {
            this.AddHyperlink( address, text, null, toolTipText ); 
        }

        /// <summary>
        /// Adds a textual hyperlink to the current paragraph.
        /// </summary>
        /// <param name="address">The address for the hyperlink.</param>
        /// <param name="text">The text displayed for the hyperlink.</param>
        /// <param name="font">
        /// The
        /// <see cref="Infragistics.Documents.Word.Font">Font</see>
        /// which defines the formatting attributes for the text that is displayed.
        /// </param>
        /// <param name="toolTipText">The text that is displayed when the user hovers the cursor over this hyperlink.</param>
        /// <remarks>
        /// <p class="body">
        /// Before the AddHyperlink method can be called, the developer must begin a paragraph via the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.StartParagraph()">StartParagraph</see>
        /// method, or an exception is thrown.
        /// </p>
        /// </remarks>
        /// <exception cref="WordDocumentWriterException">Thrown if no document or paragraph is currently open.</exception>
        /// <exception cref="WordDocumentWriterXmlWriterException">Thrown if the XmlWriter used to write content to the stream throws an exception.</exception>
        public void AddHyperlink( string address, string text, Font font, string toolTipText )
        {
            List<TextRun> textRuns = TextRun.ToList( new TextRun(text, font ) );
            this.AddHyperlink( address, textRuns, toolTipText ); 
        }

        /// <summary>
        /// Adds a textual hyperlink to the current paragraph.
        /// </summary>
        /// <param name="address">The address for the hyperlink.</param>
        /// <param name="textRuns">
        /// A list of
        /// <see cref="Infragistics.Documents.Word.TextRun">TextRun</see>
        /// instances which define the rich content for the hyperlink.
        /// </param>
        /// <param name="toolTipText">The text that is displayed when the user hovers the cursor over this hyperlink.</param>
        /// <remarks>
        /// <p class="body">
        /// This overload makes it possible for the developer to display text with mixed
        /// font attributes in the hyperlink. Each text fragment with different font
        /// attributes must be manifested as a separate text run. Each one is added to
        /// the <paramref name="textRuns"/> list prior to calling the method.
        /// </p>
        /// <p class="body">
        /// Before the AddHyperlink method can be called, the developer must begin a paragraph via the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.StartParagraph()">StartParagraph</see>
        /// method, or an exception is thrown.
        /// </p>
        /// <p class="body">
        /// In the case where the <paramref name="address"/> begins with "www.", the prefix
        /// "http://" will be prepended to avoid creating a malformed URI.
        /// </p>
        /// </remarks>
        /// <exception cref="WordDocumentWriterException">Thrown if no document or paragraph is currently open.</exception>
        /// <exception cref="WordDocumentWriterXmlWriterException">Thrown if the XmlWriter used to write content to the stream throws an exception.</exception>
        public void AddHyperlink( string address, IList<TextRun> textRuns, string toolTipText )
        {
            TextHyperlink hyperlink = TextHyperlink.Create( this, address, null );
            hyperlink.TextRuns = textRuns;
            hyperlink.ToolTipText = toolTipText;

            this.AddHyperlink( hyperlink );
        }

        /// <summary>
        /// Adds a textual hyperlink to the current paragraph.
        /// </summary>
        /// <param name="hyperlink">
        /// A
        /// <see cref="Infragistics.Documents.Word.TextHyperlink">TextHyperlink</see>
        /// instance which defines the properties of the hyperlink.
        /// </param>
        /// <remarks>
        /// <p class="body">
        /// A
        /// <see cref="Infragistics.Documents.Word.TextHyperlink">TextHyperlink</see>
        /// instance can be created using the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.CreateTextHyperlink(string)">CreateTextHyperlink</see>
        /// method.
        /// </p>
        /// </remarks>
        public abstract void AddHyperlink( TextHyperlink hyperlink );

        #endregion AddHyperlink

        #region AddInlinePicture

        /// <summary>
        /// Adds an inline picture to the current paragraph.
        /// </summary>
        /// <param name="image">The image on which the picture is based.</param>
        /// <remarks>
        /// <p class="body">
        /// Before the AddInlinePicture method can be called, the developer must begin a paragraph via the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.StartParagraph()">StartParagraph</see>
        /// method, or an exception is thrown.
        /// </p>
        /// </remarks>
        /// <exception cref="WordDocumentWriterException">Thrown if no document or paragraph is currently open.</exception>
        /// <exception cref="WordDocumentWriterXmlWriterException">Thrown if the XmlWriter used to write content to the stream throws an exception.</exception>
        public void AddInlinePicture( Image image )
        {
            this.AddInlinePicture( image, null );
        }

        /// <summary>
        /// Adds an inline picture with the specified size to the current paragraph.
        /// </summary>
        /// <param name="image">The image on which the picture is based.</param>
        /// <param name="size">If specified, the size at which the picture is rendered. If null, the natural size of the associated image is used.</param>
        /// <remarks>
        /// <p class="body">
        /// Before the AddInlinePicture method can be called, the developer must begin a paragraph via the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.StartParagraph()">StartParagraph</see>
        /// method, or an exception is thrown.
        /// </p>
        /// </remarks>
        /// <exception cref="WordDocumentWriterException">Thrown if no document or paragraph is currently open.</exception>
        /// <exception cref="WordDocumentWriterXmlWriterException">Thrown if the XmlWriter used to write content to the stream throws an exception.</exception>
        public void AddInlinePicture( Image image, SizeF? size )
        {
            InlinePicture inlinePicture = InlinePicture.Create( this, image );
            inlinePicture.Size = size;

            this.AddInlinePicture( inlinePicture );
        }

        /// <summary>
        /// Adds the specified
        /// <see cref="Infragistics.Documents.Word.InlinePicture">InlinePicture</see>
        /// to the current paragraph.
        /// </summary>
        /// <param name="inlinePicture">
        /// The
        /// <see cref="Infragistics.Documents.Word.InlinePicture">InlinePicture</see>
        /// to add.
        /// </param>
        public abstract void AddInlinePicture( InlinePicture inlinePicture );

        #endregion AddInlinePicture

        #region AddInlineShape

        /// <summary>
        /// Adds an inline
        /// <see cref="Infragistics.Documents.Word.VmlShape">shape</see>
        /// to the current paragraph, using Vector Markup Language (VML).
        /// </summary>
        /// <param name="shapeType">
        /// A
        /// <see cref="Infragistics.Documents.Word.ShapeType">ShapeType</see>
        /// constant which defines the type of shape to add.
        /// </param>
        /// <remarks>
        /// <p class="body">
        /// Before the AddInlineShape method can be called, the developer must begin a paragraph via the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.StartParagraph()">StartParagraph</see>
        /// method, or an exception is thrown.
        /// </p>
        /// </remarks>
        /// <exception cref="WordDocumentWriterException">Thrown if no document or paragraph is currently open.</exception>
        /// <exception cref="WordDocumentWriterXmlWriterException">Thrown if the XmlWriter used to write content to the stream throws an exception.</exception>
        /// <seealso cref="Infragistics.Documents.Word.ShapeType">ShapeType enumeration</seealso>
        public void AddInlineShape( ShapeType shapeType )
        {
            this.AddInlineShape( shapeType, null );
        }

        /// <summary>
        /// Adds an inline
        /// <see cref="Infragistics.Documents.Word.VmlShape">shape</see>
        /// to the current paragraph, using Vector Markup Language (VML).
        /// </summary>
        /// <param name="shapeType">
        /// A
        /// <see cref="Infragistics.Documents.Word.ShapeType">ShapeType</see>
        /// constant which defines the type of shape to add.
        /// </param>
        /// <param name="size">If specified, the size at which the shape is rendered. If null, the shape is rendered at a size of one inch square..</param>
        /// <remarks>
        /// <p class="body">
        /// Before the AddInlineShape method can be called, the developer must begin a paragraph via the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.StartParagraph()">StartParagraph</see>
        /// method, or an exception is thrown.
        /// </p>
        /// </remarks>
        /// <exception cref="WordDocumentWriterException">Thrown if no document or paragraph is currently open.</exception>
        /// <exception cref="WordDocumentWriterXmlWriterException">Thrown if the XmlWriter used to write content to the stream throws an exception.</exception>
        /// <seealso cref="Infragistics.Documents.Word.ShapeType">ShapeType enumeration</seealso>
        public void AddInlineShape( ShapeType shapeType, Size? size )
        {
            InlineShape inlineShape = InlineShape.Create( this, shapeType );
            inlineShape.Size = size;
            this.AddInlineShape( inlineShape );
        }

        /// <summary>
        /// Adds an inline
        /// <see cref="Infragistics.Documents.Word.VmlShape">shape</see>
        /// to the current paragraph, using Vector Markup Language (VML).
        /// </summary>
        /// <param name="shape">
        /// The shape to add, such as a line, rectangle, ellipse, etc.
        /// </param>
        /// <remarks>
        /// <p class="body">
        /// Before the AddInlineShape method can be called, the developer must begin a paragraph via the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.StartParagraph()">StartParagraph</see>
        /// method, or an exception is thrown.
        /// </p>
        /// </remarks>
        /// <exception cref="WordDocumentWriterException">Thrown if no document or paragraph is currently open.</exception>
        /// <exception cref="WordDocumentWriterXmlWriterException">Thrown if the XmlWriter used to write content to the stream throws an exception.</exception>
        /// <seealso cref="Infragistics.Documents.Word.ShapeType">ShapeType enumeration</seealso>
        public void AddInlineShape( VmlShape shape )
        {
            InlineShape inlineShape = InlineShape.Create( this, shape );
            this.AddInlineShape( inlineShape );
        }

        /// <summary>
        /// Adds an
        /// <see cref="Infragistics.Documents.Word.InlineShape">InlineShape</see>
        /// to the current paragraph, using Vector Markup Language (VML).
        /// </summary>
        /// <param name="inlineShape">
        /// The instance to add.
        /// </param>
        /// <remarks>
        /// <p class="body">
        /// Before the AddInlineShape method can be called, the developer must begin a paragraph via the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.StartParagraph()">StartParagraph</see>
        /// method, or an exception is thrown.
        /// </p>
        /// </remarks>
        /// <exception cref="WordDocumentWriterException">Thrown if no document or paragraph is currently open.</exception>
        /// <exception cref="WordDocumentWriterXmlWriterException">Thrown if the XmlWriter used to write content to the stream throws an exception.</exception>
        /// <seealso cref="Infragistics.Documents.Word.ShapeType">ShapeType enumeration</seealso>
        public abstract void AddInlineShape( InlineShape inlineShape );

        #endregion AddInlineShape

        #region AddAnchoredPicture
        
        /// <summary>
        /// Adds an anchored picture to the current paragraph.
        /// </summary>
        /// <param name="image">The image on which the picture is based.</param>
        /// <remarks>
        /// <p class="body">
        /// Before the AddAnchoredPicture method can be called, the developer must begin a paragraph via the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.StartParagraph()">StartParagraph</see>
        /// method, or an exception is thrown.
        /// </p>
        /// </remarks>
        /// <exception cref="WordDocumentWriterException">Thrown if no document or paragraph is currently open.</exception>
        /// <exception cref="WordDocumentWriterXmlWriterException">Thrown if the XmlWriter used to write content to the stream throws an exception.</exception>
        public void AddAnchoredPicture( Image image )
        {
            AnchoredPicture anchoredPicture = this.CreateAnchoredPicture( image );
            this.AddAnchoredPicture( anchoredPicture );
        }

        /// <summary>
        /// Adds an anchored picture to the current paragraph.
        /// </summary>
        /// <param name="anchoredPicture">
        /// The
        /// <see cref="Infragistics.Documents.Word.AnchoredPicture">AnchoredPicture</see>
        /// instance to be added to the paragraph.
        /// </param>
        /// <remarks>
        /// <p class="body">
        /// An AnchoredPicture instance can be obtained using the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.CreateAnchoredPicture">CreateAnchoredPicture</see>
        /// method. An existing instance can also be used; changing properties on
        /// that instance does not affect previously added pictures because the information
        /// is written to the stream immediately.
        /// </p>
        /// <p class="body">
        /// Before the AddAnchoredPicture method can be called, the developer must begin a paragraph via the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.StartParagraph()">StartParagraph</see>
        /// method, or an exception is thrown.
        /// </p>
        /// </remarks>
        /// <exception cref="WordDocumentWriterException">Thrown if no document or paragraph is currently open.</exception>
        /// <exception cref="WordDocumentWriterXmlWriterException">Thrown if the XmlWriter used to write content to the stream throws an exception.</exception>
        public abstract void AddAnchoredPicture( AnchoredPicture anchoredPicture );

        #endregion AddAnchoredPicture

        #region AddAnchoredShape

        /// <summary>
        /// Adds an anchored shape with the specified size to the current paragraph.
        /// </summary>
        /// <param name="shapeType">
        /// A
        /// <see cref="Infragistics.Documents.Word.ShapeType">ShapeType</see>
        /// constant which defines the type of shape to add.
        /// </param>
        /// <remarks>
        /// <p class="body">
        /// Before the AddAnchoredShape method can be called, the developer must begin a paragraph via the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.StartParagraph()">StartParagraph</see>
        /// method, or an exception is thrown.
        /// </p>
        /// </remarks>
        /// <seealso cref="Infragistics.Documents.Word.ShapeType">ShapeType enumeration</seealso>
        /// <exception cref="WordDocumentWriterException">Thrown if no document or paragraph is currently open.</exception>
        /// <exception cref="WordDocumentWriterXmlWriterException">Thrown if the XmlWriter used to write content to the stream throws an exception.</exception>
        public void AddAnchoredShape( ShapeType shapeType )
        {
            AnchoredShape shape = AnchoredShape.Create( this, shapeType );
            this.AddAnchoredShape( shape );
        }

        /// <summary>
        /// Adds an anchored shape with the specified size to the current paragraph.
        /// </summary>
        /// <param name="anchoredShape">
        /// The
        /// <see cref="Infragistics.Documents.Word.AnchoredShape">AnchoredShape</see>
        /// instance to be added to the paragraph.
        /// </param>
        /// <remarks>
        /// <p class="body">
        /// An AnchoredShape instance can be obtained using the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.CreateAnchoredShape(ShapeType)">CreateAnchoredShape</see>
        /// method. An existing instance can also be used; changing properties on
        /// that instance does not affect previously added shapes because the information
        /// is written to the stream immediately.
        /// </p>
        /// <p class="body">
        /// Before the AddAnchoredShape method can be called, the developer must begin a paragraph via the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.StartParagraph()">StartParagraph</see>
        /// method, or an exception is thrown.
        /// </p>
        /// </remarks>
        /// <seealso cref="Infragistics.Documents.Word.ShapeType">ShapeType enumeration</seealso>
        /// <exception cref="WordDocumentWriterException">Thrown if no document or paragraph is currently open.</exception>
        /// <exception cref="WordDocumentWriterXmlWriterException">Thrown if the XmlWriter used to write content to the stream throws an exception.</exception>
        public abstract void AddAnchoredShape( AnchoredShape anchoredShape );
        #endregion AddAnchoredShape

        #region StartTable

        /// <summary>
        /// Starts a new table in the document with the specified number of columns.
        /// </summary>
        /// <param name="columnCount">
        /// The total number of columns in the table. Columns added with this overload
        /// have a width of one inch.
        /// Note that by default, actual column widths are
        /// determined automatically; to override this behavior and apply explicit
        /// widths, use the overload that takes a
        /// <see cref="Infragistics.Documents.Word.TableProperties">TableProperties</see>
        /// instance, and set the
        /// <see cref="Infragistics.Documents.Word.TableProperties.Layout">Layout</see>
        /// property of the TableProperties to 'Fixed'.
        /// </param>
        /// <remarks>
        /// <p class="body">
        /// This method starts a &lt;tbl&gt; element.
        /// Callers are responsible for closing this element. The
        /// &lt;tbl&gt; element can be closed by calling the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.EndTable()">EndTable</see>
        /// method.
        /// </p>
        /// </remarks>
        /// <seealso cref="Infragistics.Documents.Word.WordDocumentWriter.EndTable()">EndTable</seealso>
        /// <exception cref="WordDocumentWriterException">Thrown if no document is currently open.</exception>
        public void StartTable( int columnCount )
        {
            this.StartTable( columnCount, null );
        }

        /// <summary>
        /// Starts a new table in the document with the specified number of columns.
        /// </summary>
        /// <param name="columnCount">
        /// The total number of columns in the table. Columns added with this overload
        /// have a width of one inch.
        /// Note that by default, actual column widths are
        /// determined automatically; to override this behavior and apply explicit
        /// widths, use the overload that takes a
        /// <see cref="Infragistics.Documents.Word.TableProperties">TableProperties</see>
        /// instance, and set the
        /// <see cref="Infragistics.Documents.Word.TableProperties.Layout">Layout</see>
        /// property of the TableProperties to 'Fixed'.
        /// </param>
        /// <param name="properties">
        /// A reference to a
        /// <see cref="Infragistics.Documents.Word.TableProperties">TableProperties</see>
        /// instance which defines formatting attributes for the table.
        /// </param>
        /// <remarks>
        /// <p class="body">
        /// This method starts a &lt;tbl&gt; element.
        /// Callers are responsible for closing this element. The
        /// &lt;tbl&gt; element can be closed by calling the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.EndTable()">EndTable</see>
        /// method.
        /// </p>
        /// </remarks>
        /// <seealso cref="Infragistics.Documents.Word.WordDocumentWriter.EndTable()">EndTable</seealso>
        /// <exception cref="WordDocumentWriterException">Thrown if no document is currently open.</exception>
        public void StartTable( int columnCount, TableProperties properties )
        {
            WordprocessingMLWriter.VerifyColumnCount( columnCount );

            List<float> columnWidths = new List<float>( columnCount );
            
            float width =
                columnCount <= WordUtilities.MaxPageWidthInInches ?
                WordUtilities.DefaultTableColumnWidthInTwips :
                WordUtilities.MaxPageWidthInTwips / columnCount;

            width = WordUtilities.ConvertFromTwips( this.Unit, width );
            
            for ( int i = 0; i < columnCount; i ++ )
            {
                columnWidths.Add( width );
            }

            this.StartTable( columnWidths, properties );
        }

        /// <summary>
        /// Starts a new table in the document with the number of columns
        /// equal to the number of elements in the specified <paramref name="columnWidths"/>
        /// list, and with the width of each column defined by the corresponding value in the list.
        /// </summary>
        /// <param name="columnWidths">
        /// A list of values which contains the width for each column to be added.
        /// The number of columns is determined by the number of elements in the list.
        /// The unit of measure for each value in the list is determined by the value of the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.Unit">Unit</see>
        /// property.
        /// </param>
        /// <param name="properties">
        /// A reference to a
        /// <see cref="Infragistics.Documents.Word.TableProperties">TableProperties</see>
        /// instance which defines formatting attributes for the table.
        /// </param>
        /// <remarks>
        /// <p class="body">
        /// This method starts a &lt;tbl&gt; element.
        /// Callers are responsible for closing this element. The
        /// &lt;tbl&gt; element can be closed by calling the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.EndTable()">EndTable</see>
        /// method.
        /// </p>
        /// <p class="body">
        /// A table must contain at least one and no more than 63 columns; specifying
        /// a value outside this range results in an exception being thrown.
        /// </p>
        /// </remarks>
        /// <seealso cref="Infragistics.Documents.Word.WordDocumentWriter.EndTable()">EndTable</seealso>
        /// <exception cref="ArgumentNullException">Thrown if the value of the <paramref name="columnWidths"/> parameter is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the list referenced by the <paramref name="columnWidths"/> parameter is empty.</exception>
        /// <exception cref="WordDocumentWriterException">Thrown if no document is currently open.</exception>
        public abstract void StartTable( IList<float> columnWidths, TableProperties properties );

        #endregion StartTable

        #region StartTableRow / EndTableRow

        /// <summary>
        /// Starts a new row in the current table with the default height.
        /// </summary>
        public void StartTableRow()
        {
            this.StartTableRow( null );
        }

        /// <summary>
        /// Starts a new row in the current table.
        /// </summary>
        public abstract void StartTableRow( TableRowProperties properties );

        /// <summary>
        /// Closes the current table row.
        /// </summary>
        public abstract void EndTableRow();

        #endregion StartTableRow / EndTableRow

        #region StartTableCell / EndTableCell / AddTableCell

        /// <summary>
        /// Starts a new cell in the current row.
        /// </summary>
        public abstract void StartTableCell( TableCellProperties properties );

        /// <summary>
        /// Closes a previously opened cell.
        /// </summary>
        public abstract void EndTableCell();

        /// <summary>
        /// Adds a cell to the current row, with one paragraph containing
        /// a simple text run with the specified <paramref name="text"/>.
        /// </summary>
        /// <param name="text">The string to be displayed within the cell.</param>
        public void AddTableCell( string text )
        {
            this.AddTableCell( text, null );
        }

        /// <summary>
        /// Adds a cell to the current row, with one paragraph containing
        /// a simple text run with the specified <paramref name="text"/>,
        /// optionally with the specified properties.
        /// </summary>
        /// <param name="text">The string to be displayed within the cell.</param>
        /// <param name="properties">
        /// A
        /// <see cref="Infragistics.Documents.Word.TableCellProperties">TableCellProperties</see>
        /// instance which defines the formatting attributes for the cell.
        /// </param>
        public abstract void AddTableCell( string text, TableCellProperties properties );

        #endregion StartTableCell / EndTableCell / AddTableCell

        #region EndTable

        /// <summary>
        /// Closes a previously opened table.
        /// </summary>
        /// <seealso cref="Infragistics.Documents.Word.WordDocumentWriter.StartTable(int)">StartTable</seealso>
        /// <exception cref="WordDocumentWriterException">Thrown if no document or table is currently open.</exception>
        public abstract void EndTable();

        #endregion EndTable

        #region DefineSection

        /// <summary>
        /// Creates a section in the document, which defines the pagination
        /// for paragraphs and tables that were written since the last section
        /// was created.
        /// </summary>
        /// <param name="properties">
        /// A
        /// <see cref="Infragistics.Documents.Word.SectionProperties">SectionProperties</see>
        /// instance which defines properties such as page size, margins, and orientation.
        /// An new instance of this class can be obtained by calling the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.CreateSectionProperties">CreateSectionProperties</see>
        /// method.
        /// </param>
        /// <remarks>
        /// <p class="body">
        /// WordprocessingML does not natively store the concept of pages, since the
        /// number of pages in a document is driven by the nature of the content.
        /// Consider the case where the user changes the size of the font for all text
        /// in a document; the number of pages in that document is likely to change,
        /// since factors such as the number of characters in the document and
        /// the size of each character determine the total amount of space required to
        /// present the content.
        /// </p>
        /// <p class="body">
        /// This method defines a section which applies to content (i.e., paragraphs
        /// and tables) that was previously written to the document. The properties
        /// of the object passed to this method define the page size, orientation,
        /// and margins for the pages on which that content will appear, thus defining
        /// a section in the document to which the designated content belongs.
        /// </p>
        /// <p class="body">
        /// <b>Example: </b> To add three paragraphs which are to appear on pages that
        /// are 7" x 5" at landscape orientation, first call the StartDocument, AddTextRun,
        /// and EndDocument methods for each of the three paragraphs. Next, call the
        /// DefineSection method, passing in a SectionProperties instance on which the
        /// PageSize property is set to (7, 5), and the PageOrientation property is set
        /// to 'Landscape'.
        /// </p>
        /// <p class="body">
        /// The
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.FinalSectionProperties">FinalSectionProperties</see>
        /// property can be used to define the pagination for all content which is
        /// not associated with any section defined by this method.
        /// </p>
        /// </remarks>
        /// <exception cref="WordDocumentWriterException">Thrown if no document is currently open, or if a paragraph is currently open.</exception>
        /// <seealso cref="Infragistics.Documents.Word.WordDocumentWriter.CreateSectionProperties()">CreateSectionProperties</seealso>
        /// <seealso cref="Infragistics.Documents.Word.WordDocumentWriter.FinalSectionProperties">FinalSectionProperties</seealso>
        public abstract void DefineSection( SectionProperties properties );

        #endregion DefineSection

        #region FromPixels / ToPixels

        /// <summary>
        /// Converts the specified <paramref name="value"/> into the specified unit of measure.
        /// </summary>
        /// <param name="value">A value which represents a number of pixels.</param>
        /// <param name="dpi">The resolution, expressed in dots per inch.</param>
        /// <remarks>
        /// <p class="body">
        /// The unit of measure in which the return value is expressed
        /// is defined by the value of the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.Unit">Unit</see>
        /// property.
        /// </p>
        /// </remarks>
        public float FromPixels( int value, float dpi )
        {
            return WordDocumentWriter.FromPixels( value, dpi, this.Unit );
        }

        /// <summary>
        /// Converts the specified <paramref name="value"/> into a pixel
        /// value at the specified resolution.
        /// </summary>
        /// <param name="value">
        /// A value which represents a linear graphical quantity. The implied unit
        /// of measure is defined by the value of the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.Unit">Unit</see>
        /// property.
        /// </param>
        /// <param name="dpi">The resolution, expressed in dots per inch.</param>
        public int ToPixels( float value, float dpi )
        {
            return WordDocumentWriter.ToPixels( value, dpi, this.Unit );
        }

        /// <summary>
        /// Converts the specified <paramref name="value"/> into the specified unit of measure.
        /// </summary>
        /// <param name="value">A value which represents a number of pixels.</param>
        /// <param name="dpi">The resolution, expressed in dots per inch.</param>
        /// <param name="unit">
        /// A
        /// <see cref="Infragistics.Documents.Word.UnitOfMeasurement">UnitOfMeasurement</see>
        /// constant which defines the units into which the value is to be converted.
        /// </param>
        static public float FromPixels( int value, float dpi, UnitOfMeasurement unit )
        {
            return WordUtilities.FromPixels( value, dpi, unit );
        }

        /// <summary>
        /// Converts the specified <paramref name="value"/> into a pixel
        /// value at the specified resolution.
        /// </summary>
        /// <param name="value">A value which represents a linear graphical quantity.</param>
        /// <param name="dpi">The resolution, expressed in dots per inch.</param>
        /// <param name="unit">
        /// A
        /// <see cref="Infragistics.Documents.Word.UnitOfMeasurement">UnitOfMeasurement</see>
        /// constant which defines the unit of measure in which the <paramref name="value"/>
        /// is expressed.
        /// </param>
        static public int ToPixels( float value, float dpi, UnitOfMeasurement unit )
        {
            return WordUtilities.ToPixels( value, dpi, unit );
        }

        #endregion FromPixels / ToPixels

        #region ConvertUnits
        /// <summary>
        /// Converts the specified floating-point <paramref name="value"/>
        /// based on the specified units of measurement.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="from">
        /// A
        /// <see cref="Infragistics.Documents.Word.UnitOfMeasurement">UnitOfMeasurement</see>
        /// constant which defines the unit of measure of the specified <paramref name="value"/>.
        /// </param>
        /// <param name="to">
        /// A UnitOfMeasurement constant which defines the unit of measure of the returned value.
        /// </param>
        /// <returns>The converted value.</returns> 
        /// <remarks>
        /// <p class="body">
        /// Use the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.Unit">Unit</see>
        /// property to define the implied unit of measure for all properties which represent
        /// a linear graphical quantity such as font sizes, cell widths, picture positions, etc.
        /// </p>
        /// <p class="body">
        /// One inch is equal to 1,440 twips (twentieths of a desktop point).
        /// </p>
        /// <p class="body">
        /// One inch is equal to 72 desktop points.
        /// </p>
        /// </remarks>
        static public float ConvertUnits( float value, UnitOfMeasurement from, UnitOfMeasurement to )
        {
            return WordUtilities.Convert( value, from, to );
        }
        #endregion ConvertUnits

        #region Close
        /// <summary>
        /// Closes the writer and finalizes content.
        /// </summary>
        public abstract void Close();
        #endregion Close

        #region AddSectionHeaderFooter

        /// <summary>
        /// Adds headers and/or footers to the main section of the document.
        /// </summary>
        public SectionHeaderFooterWriterSet AddSectionHeaderFooter( SectionHeaderFooterParts parts )
        {
            return this.AddSectionHeaderFooter( parts, this.FinalSectionProperties );
        }

        /// <summary>
        /// Adds headers and/or footers to the specified section of the document.
        /// </summary>
        /// <param name="parts">
        /// A bitflags value which defines the header and/or footer parts to be included in the section.
        /// </param>
        /// <param name="sectionProperties">
        /// A
        /// <see cref="Infragistics.Documents.Word.SectionProperties">SectionProperties</see>
        /// instance which contains the properties for the document section in which the headers/footers
        /// will appear. When null is specified, headers/footers are created for the final section,
        /// with the section properties being defined by the instance returned from the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.FinalSectionProperties"></see>
        /// property.
        /// </param>
        /// <seealso cref="Infragistics.Documents.Word.WordDocumentWriter.FinalSectionProperties">FinalSectionProperties</seealso>
        /// <seealso cref="Infragistics.Documents.Word.WordDocumentWriter.DefineSection">DefineSection</seealso>
        /// <seealso cref="Infragistics.Documents.Word.SectionProperties">SectionProperties class</seealso>
        public abstract SectionHeaderFooterWriterSet AddSectionHeaderFooter( SectionHeaderFooterParts parts, SectionProperties sectionProperties );

        #endregion AddSectionHeaderFooter

        #region AddPageNumberField

        //  BF 4/12/11  TFS72408
        #region Obsolete
        ///// <summary>
        ///// Adds a page numbering field to the current paragraph.
        ///// </summary>
        ///// <param name="fieldType">
        ///// A
        ///// <see cref="Infragistics.Documents.Word.PageNumberFieldFormat">PageNumberFieldFormat</see>
        ///// value which defines the format for the page number.
        ///// </param>
        //public void AddPageNumberField( PageNumberFieldFormat fieldType )
        //{
        //    this.AddPageNumberField( fieldType, null );
        //}

        ///// <summary>
        ///// Adds a page numbering field to the current paragraph.
        ///// </summary>
        ///// <param name="format">
        ///// A
        ///// <see cref="Infragistics.Documents.Word.PageNumberFieldFormat">PageNumberFieldFormat</see>
        ///// value which defines the format for the page number field.
        ///// </param>
        ///// <param name="font">
        ///// A
        ///// <see cref="Infragistics.Documents.Word.Font">Font</see>
        ///// instance which defines the formatting attributes for the page number.
        ///// </param>
        ///// <remarks>
        ///// <p class="body">
        ///// This method can only be called when a paragraph is currently open.
        ///// A page number field is similar to a text run; it can be embedded
        ///// within the paragraph adjacent to any other kind of content such
        ///// as text, images, or hyperlinks.
        ///// </p>
        ///// </remarks>
        //public void AddPageNumberField( PageNumberFieldFormat format, Font font )
        //{
        //    PageNumberField pageNumberField = new PageNumberField( this, format );
        //    pageNumberField.Font = font;
        //    this.AddPageNumberField( pageNumberField );
        //}

        ///// <summary>
        ///// Adds a page numbering field to the current paragraph.
        ///// </summary>
        ///// <param name="pageNumberField">
        ///// A
        ///// <see cref="Infragistics.Documents.Word.PageNumberField">PageNumberField</see>
        ///// value which defines the properties of the page number.
        ///// </param>
        ///// <remarks>
        ///// <p class="body">
        ///// This method can only be called when a paragraph is currently open.
        ///// A page number field is similar to a text run; it can be embedded
        ///// within the paragraph adjacent to any other kind of content such
        ///// as text, images, or hyperlinks.
        ///// </p>
        ///// </remarks>
        //public abstract void AddPageNumberField( PageNumberField pageNumberField );
        #endregion Obsolete

        #endregion AddPageNumberField

        #region Flush
        /// <summary>
        /// Flushes the contents of the buffer to the underlying stream.
        /// </summary>
        public abstract void Flush();
        #endregion Flush

        #region CacheHeaderFooterWriters
        internal void CacheHeaderFooterWriters( List<WordHeaderFooterWriter> writerList )
        {
            if ( writerList == null || writerList.Count == 0 )
                return;

            if ( this.headerFooterWriters == null )
                this.headerFooterWriters = new List<WordHeaderFooterWriter>();

            this.headerFooterWriters.AddRange( writerList );
        }
        #endregion CacheHeaderFooterWriters

        #region VerifyHeaderFooterWriters
        internal void VerifyHeaderFooterWriters()
        {
            if ( this.headerFooterWriters == null )
                return;

            foreach( WordHeaderFooterWriter writer in this.headerFooterWriters )
            {
                WriterStateVerification verifier = writer != null ? writer.StateVerification : null;
                if ( verifier == null )
                    continue;

                if ( verifier.CurrentState.DocumentOpened &&
                     verifier.CurrentState.DocumentClosed == false )
                {
                    throw new WordDocumentWriterException(WordDocumentWriterExceptionReason.HeaderFooterWriterNotClosed);
                }
                else
                if ( verifier.CurrentState.DocumentOpened == false )
                {
                    throw new WordDocumentWriterException(WordDocumentWriterExceptionReason.HeaderFooterWriterNotOpened);
                }
            }
        }

        #endregion VerifyHeaderFooterWriters

        #region DisposeHeaderFooterWriters
        internal void DisposeHeaderFooterWriters()
        {
            if ( this.headerFooterWriters == null )
                return;
            
            foreach( WordHeaderFooterWriter writer in this.headerFooterWriters )
            {
                writer.Dispose();
            }

            this.headerFooterWriters = null;
        }
        #endregion DisposeHeaderFooterWriters

        #endregion Methods

        #region IDisposable

        /// <summary>
        /// Releases any resources being used by this object.
        /// </summary>
        void IDisposable.Dispose()
        {
            this.Dispose();
        }

        /// <summary>
        /// Releases any resources being used by this object.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// Derived classes must override this method and release any resources
        /// currently in use.
        /// </p>
        /// </remarks>
        public abstract void Dispose();
        
        #endregion IDisposable

        #region IUnitOfMeasurementProvider

        UnitOfMeasurement IUnitOfMeasurementProvider.Unit
        {
            get { return this.Unit; }
        }

        #endregion IUnitOfMeasurementProvider

        #region IContentWriter interface implementation

        void IContentWriter.AddEmptyParagraph()
        {
            this.AddEmptyParagraph();
        }

        void IContentWriter.StartParagraph(ParagraphProperties properties)
        {
            this.StartParagraph( properties );
        }

        void IContentWriter.EndParagraph()
        {
            this.EndParagraph();
        }

        void IContentWriter.AddNewLine(NewLineType newLineType)
        {
            this.AddNewLine( newLineType );
        }

        void IContentWriter.AddTextRun(TextRun textRun)
        {
            this.AddTextRun( textRun );
        }

        void IContentWriter.AddTextRun(string text)
        {
            this.AddTextRun( text );
        }

        void IContentWriter.AddHyperlink(TextHyperlink hyperlink)
        {
            this.AddHyperlink( hyperlink );
        }

        void IContentWriter.AddInlinePicture(InlinePicture inlinePicture)
        {
            this.AddInlinePicture( inlinePicture );
        }

        void IContentWriter.AddAnchoredPicture(AnchoredPicture anchoredPicture)
        {
           this.AddAnchoredPicture( anchoredPicture );
        }


        void IContentWriter.StartTable(IList<float> columnWidths, TableProperties properties)
        {
            this.StartTable( columnWidths, properties );
        }

        void IContentWriter.StartTableRow(TableRowProperties properties)
        {
            this.StartTableRow( properties );
        }

        void IContentWriter.EndTableRow()
        {
            this.EndTableRow();
        }

        void IContentWriter.StartTableCell(TableCellProperties properties)
        {
            this.StartTableCell( properties );
        }

        void IContentWriter.EndTableCell()
        {
            this.EndTableCell();
        }

        void IContentWriter.AddTableCell(string text, TableCellProperties properties)
        {
            this.AddTableCell( text, properties );
        }

        void IContentWriter.EndTable()
        {
            this.EndTable();
        }

        void IContentWriter.AddAnchoredShape(AnchoredShape anchoredShape)
        {
            this.AddAnchoredShape( anchoredShape );
        }

        void IContentWriter.AddInlineShape(InlineShape inlineShape)
        {
            this.AddInlineShape( inlineShape );
        }

        void IContentWriter.AddPageNumberField(PageNumberField pageNumberField)
        {
            //  BF 4/12/11  TFS72408
            //this.AddPageNumberField( pageNumberField );
        }

        void IContentWriter.Start()
        {
            this.StartDocument();
        }

        void IContentWriter.Close()
        {
            this.EndDocument( true );
        }

        UnitOfMeasurement IContentWriter.Unit { get { return this.Unit; } }

        WordDocumentWriter IContentWriter.DocumentWriter { get { return this; } }

        #endregion IContentWriter interface implementation
    }
    #endregion WordDocumentWriter class

    #region WordprocessingMLWriter class
    /// <summary>
    /// Represents a writer that provides a fast, non-cached, forward-only way
    /// of generating streams or files containing XML data that conforms
    /// to the WordprocessingML OpenXml specification.
    /// </summary>
    /// <remarks>
    /// <p class="body">
    /// The WordprocessingMLWriter class is a concrete implementation of the
    /// <see cref="Infragistics.Documents.Word.WordDocumentWriter">WordDocumentWriter</see>
    /// class which produces content that is recognizable by WordprocessingML OpenXml
    /// consumers such as Microsoft Word 2007.
    /// </p>
    /// <p class="body">
    /// The developer communicates with the WordDocumentWriter in a "forward-only"
    /// manner; as soon as a content creation method  is called, that content is
    /// irreversibly written to the target document. While this method is generally
    /// considered to be more complicated, it offers the benefit of a greatly reduced
    /// memory footprint over an object-driven approach.
    /// </p>
    /// <p class="body">
    /// A new instance of this class can be created using the
    /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.Create(string)">Create</see>
    /// method.
    /// </p>
    /// <p class="body">
    /// To begin a new WordprocessingML document, the developer calls the
    /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.StartDocument">StartDocument</see>
    /// method. This method starts the &lt;document&gt; and &lt;body&gt; elements for the document.
    /// After execution returns from the method, these elements are left open, so that content can be added.
    /// The elements are not closed until the developer calls the
    /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.EndDocument(bool)">EndDocument</see>
    /// method, or until the WordDocumentWriter instance is disposed of.
    /// </p>
    /// <p class="body">
    /// After the StartDocument method is called, blocks of content can be added via (for example) the
    /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.StartParagraph()">StartParagraph</see>
    /// method. After adding a paragraph, the developer will typically populate it with content using methods like
    /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.AddTextRun(string)">AddTextRun</see>
    /// and
    /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.AddHyperlink(string, string)">AddHyperlink</see>.
    /// These methods are executed as atomic operations, i.e., all content required to represent the content run
    /// is added to the document stream before execution returns from the method.
    /// After all content has been added to the paragraph, the developer calls the
    /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.EndParagraph">EndParagraph</see>
    /// method to close the paragraph element.
    /// </p>
    /// <p class="body">
    /// Attempting to add content to the document when the writer is not in the required
    /// state for the operation causes a
    /// <see cref="Infragistics.Documents.Word.WordDocumentWriterException">WordDocumentWriterException</see>
    /// to be thrown. For example, calling the
    /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.AddTextRun(string)">AddTextRun</see>
    /// method when the
    /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.StartParagraph()">StartParagraph</see>
    /// method had not previously been called to open a paragraph is not valid (since a text run must
    /// belong to a paragraph) and will cause an exception to be thrown. These exceptions can be handled
    /// for debugging purposes during the early phases of application development, but there is generally
    /// no way for the developer to recover gracefully from these exceptions, and they should be interpreted
    /// as in indication that there is a problem with the client code.
    /// </p>
    /// <p class="body">
    /// After all content blocks have been added, the developer calls the
    /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.EndDocument(bool)">EndDocument</see>
    /// method to close the &lt;document&gt; and &lt;body&gt; elements. 
    /// </p>
    /// <p class="body">
    /// Page size, margins, and orientation can be set for the entire document via the
    /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.FinalSectionProperties">FinalSectionProperties</see>
    /// property, and for a range of previously written paragraphs and tables via the
    /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.DefineSection(SectionProperties)">DefineSection</see>
    /// method.
    /// </p>
    /// <p class="body">
    /// The content stream is closed when the WordDocumentWriter is disposed of,
    /// and the resulting WordprocessingML file is created.
    /// </p>
    /// <p class="body">
    /// The developer can specify the unit of measure to be used when interpreting
    /// properties which represent graphical measurements using the
    /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.Unit">Unit</see>
    /// property. The objects returned from the
    /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.CreateFont">CreateFont</see>,
    /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.CreateParagraphProperties">CreateParagraphProperties</see>,
    /// and
    /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.CreateAnchoredPicture">CreateAnchoredPicture</see>
    /// methods maintain a reference to this class, and use the value of the Unit property to determine the unit of measure
    /// in which to express certain property values. For example, if the developer assigns a value of 'Inch' to the Unit property,
    /// then obtains a Font using the CreateFont method, the get method of the Size property will return the value expressed in
    /// inches, and the set method will assume the values assigned to it to be expressed in inches. This association is maintained
    /// for the lifetime of the object, so that if the developer changes the value of the Unit property, the new unit of measure is
    /// automatically used the next time the property is accessed.
    /// </p>
    /// </remarks>

    [InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_IGWord)]

    public class WordprocessingMLWriter : WordDocumentWriter
    {
        #region Member variables

        private Stream stream = null;
        private WordDocumentExportManager exportManager = null;
        private WriterStateVerification stateVerification = null;
        private bool weOwnStream = false;
        private WordPartExporterXmlExceptionHandler xmlExceptionHandler = null;

        #endregion Member variables

        #region Constructor

        internal WordprocessingMLWriter( Stream stream, IPackageFactory packageFactory, bool weOwnStream )
        {
            this.weOwnStream = weOwnStream;
            this.Open( stream, packageFactory );
        }

        #endregion Constructor

        #region Properties

        #region DocumentProperties
        /// <summary>
        /// Defines the properties of the document such as Title, Author, Manager, etc.
        /// </summary>
        public override WordDocumentProperties DocumentProperties
        {
            get { return this.exportManager != null ? this.exportManager.GetDocumentProperties() as WordDocumentProperties : null; }
        }
        #endregion DefaultParagraphProperties

        #region DefaultParagraphProperties
        /// <summary>
        /// Defines the default formatting for paragraphs which have
        /// no more specific formatting attributes defined.
        /// </summary>
        public override ParagraphProperties DefaultParagraphProperties
        {
            get { return this.exportManager != null ? this.exportManager.GetDefaultParagraphProperties(this) : null; }
        }
        #endregion DefaultParagraphProperties

        #region FinalSectionProperties
        /// <summary>
        /// Defines the page-related properties of the document such as size,
        /// margins, and orientation. Applies to all content in the document
        /// which follows the last explicitly defined section.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// This property defines the pagination for all content within the
        /// document which is not explicitly associated with a section defined
        /// via the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.DefineSection(SectionProperties)">DefineSection</see>
        /// method. Sections defined by that method apply to all content that was written
        /// prior to the method call; from this it follows that once the DefineSection method
        /// is called, pagination is then explicitly defined for all previously written content
        /// in the document. As such, the pagination as defined by this property applies to the
        /// content that was written after the last call to DefineSection, or to the "final"
        /// section of the document.
        /// </p>
        /// <p class="body">
        /// In the case where the 
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.DefineSection(SectionProperties)">DefineSection</see>
        /// method is not used, this property defines the pagination for the entire document.
        /// </p>
        /// <p class="body">
        /// Headers and/or footers can be defined for the final section using the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.AddSectionHeaderFooter(SectionHeaderFooterParts)">AddHeaderFooterSection</see>
        /// method.
        /// </p>
        /// </remarks>
        /// <seealso cref="Infragistics.Documents.Word.WordDocumentWriter.DefineSection(SectionProperties)">DefineSection</seealso>
        public override SectionProperties FinalSectionProperties
        {
            get { return this.exportManager != null ? this.exportManager.GetFinalSectionProperties(this) : null; }
        }
        #endregion FinalSectionProperties

        #region DefaultTableProperties
        /// <summary>
        /// Defines the default formatting for tables which have
        /// no more specific formatting attributes defined.
        /// </summary>
        public override DefaultTableProperties DefaultTableProperties
        {
            get { return this.exportManager != null ? this.exportManager.GetDefaultTableProperties(this) : null; }
        }
        #endregion DefaultTableProperties

        #region DefaultFont
        /// <summary>
        /// Returns the font that determines the formatting for all text runs
        /// which do not have more specific formatting attributes defined.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// The DefaultFont property can be used to assign a document-level font,
        /// the attributes of which all text runs acquire when no more specific
        /// setting exists. For example, to italicize the entire document, set
        /// the Italic property of the font returned by this property to true.
        /// </p>
        /// </remarks>
        public override WORD.Font DefaultFont
        {
            get { return this.exportManager != null ? this.exportManager.GetDefaultFont(this) : null; }
        }

        #endregion DefaultFont

        #region DocumentPartExporter





        internal WordDocumentPartExporter DocumentPartExporter
        {
            get
            {
                WordDocumentPartExporter partExporter = this.exportManager != null ? this.exportManager.DocumentPartExporter : null;
                
                if ( partExporter != null && this.xmlExceptionHandler == null )
                {
                    this.xmlExceptionHandler = new WordPartExporterXmlExceptionHandler( this.OnWordDocumentPartExporterXmlException );
                    partExporter.XmlException += this.xmlExceptionHandler;
                }

                return partExporter;
            }
        }

        void OnWordDocumentPartExporterXmlException( object sender, WordPartExporterXmlExceptionEventArgs e )
        {
            throw new WordDocumentWriterXmlWriterException(
                e.InnerException,
                e.Element,
                e.Attribute );
        }

        #endregion DocumentPartExporter

        #region StateVerification
        private WriterStateVerification StateVerification
        {
            get
            {
                if ( this.stateVerification == null )
                    this.stateVerification = new WriterStateVerification();

                return this.stateVerification;
            }
        }
        #endregion StateVerification

        #region WriterState
        /// <summary>
        /// Returns a bitflag value which describes the current state of the writer.
        /// </summary>
        public override WordDocumentWriterState WriterState
        {
            get
            {
                return this.StateVerification.State;
            }
        }
        #endregion WriterState

        #endregion Properties

        #region Methods

        #region StartDocument
        /// <summary>
        /// Starts a WordprocessingML document.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// This method starts the &lt;document&gt; and &lt;body&gt; elements
        /// for the wordprocessingML document. Callers are responsible for
        /// closing all elements written to the document stream. The
        /// &lt;document&gt; and &lt;body&gt; elements can be closed by calling
        /// the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.EndDocument(bool)">EndDocument</see>
        /// method.
        /// </p>
        /// </remarks>
        /// <seealso cref="Infragistics.Documents.Word.WordDocumentWriter.EndDocument(bool)">EndDocument</seealso>
        /// <exception cref="WordDocumentWriterException">Thrown if the XmlWriter used to create the content could not be opened.</exception>
        public override void StartDocument()
        {
            //  Tell the export manager to start a document
            WriteState state = this.exportManager.StartDocument();

            //  If that failed, throw an exception.
            if ( state == WriteState.Error || state == WriteState.Closed )
                throw new WordDocumentWriterException(WordDocumentWriterExceptionReason.WriterError);

            //  If that succeeded, set the 'DocumentOpened' flag.
            this.StateVerification.CurrentState.DocumentOpened = true;
        }
        #endregion StartDocument

        #region EndDocument
        /// <summary>
        /// Closes a previously opened document, optionally closing the writer.
        /// </summary>
        /// <param name="closeWriter">
        /// A boolean value which specifies whether the writer should also be closed.
        /// </param>
        /// <seealso cref="Infragistics.Documents.Word.WordDocumentWriter.StartDocument">StartDocument</seealso>
        /// <exception cref="WordDocumentWriterException">Thrown if no document is currently open.</exception>
        public override void EndDocument( bool closeWriter )
        {
            //  BF 4/1/11
            if ( this.StateVerification.CurrentState.DocumentClosed == false )
            {
                //  Make sure there are no paragraphs, tables, table rows, table cells,
                //  etc., pending closure
                this.StateVerification.VerifyDocumentClosure( WordDocumentWriterExceptionReason.DocumentClosureError );

                //  Tell the export manager to close the document.
                this.exportManager.EndDocument();

                //  Set the DocumentClosed flag.
                this.StateVerification.CurrentState.DocumentClosed = true;
            }

            //  Close the writer if the caller said to
            if ( closeWriter )
                this.Close();
        }
        #endregion EndDocument

        #region StartParagraph

        /// <summary>
        /// Starts a new paragraph in the document.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// This method starts a &lt;p&gt; element.
        /// Callers are responsible for closing this element. The
        /// &lt;p&gt; element can be closed by calling the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.EndParagraph">EndParagraph</see>
        /// method.
        /// </p>
        /// </remarks>
        /// <param name="properties">
        /// A
        /// <see cref="Infragistics.Documents.Word.ParagraphProperties">ParagraphProperties</see>
        /// instance which defines properties such as alignment and indentation for the paragraph.
        /// An new instance of this class can be obtained by calling the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.CreateParagraphProperties">CreateParagraphProperties</see>
        /// method.
        /// </param>
        /// <exception cref="WordDocumentWriterException">Thrown if no document is currently open.</exception>
        public override void StartParagraph( ParagraphProperties properties )
        {
            WordprocessingMLWriter.StartParagraph( properties, this.StateVerification, this.DocumentPartExporter );
        }

        static internal void StartParagraph(
            ParagraphProperties properties,
            WriterStateVerification verifier,
            WordDocumentPartExporter partExporter )
        {
            verifier.VerifyDocumentIsOpen();

            //  Paragraphs can never be nested
            verifier.VerifyNoParagraphIsOpen(WordDocumentWriterExceptionReason.NestedParagraph);

            partExporter.StartParagraph( properties );

            //  BF 3/1/11
            if ( verifier.CurrentState.CellIsOpen )
            {
                verifier.CurrentState.ParagraphWritten_TableCell = true;
                verifier.CurrentState.ParagraphWritten_EndTable = true;
            }

            verifier.CurrentState.ParagraphOpen = true;
        }

        #endregion StartParagraph

        #region EndParagraph

        /// <summary>
        /// Closes a previously created paragraph.
        /// </summary>
        /// <seealso cref="Infragistics.Documents.Word.WordDocumentWriter.StartParagraph()">StartParagraph</seealso>
        /// <exception cref="WordDocumentWriterException">Thrown if no document or paragraph is currently open.</exception>
        public override void EndParagraph()
        {
            WordprocessingMLWriter.EndParagraph( this.StateVerification, this.DocumentPartExporter );
        }

        static internal void EndParagraph(
            WriterStateVerification verifier,
            WordDocumentPartExporter partExporter )
        {
            verifier.VerifyParagraphIsOpen();
            partExporter.EndParagraph();
            verifier.CurrentState.ParagraphOpen = false;
        }

        #endregion EndParagraph

        #region AddTextRun

        /// <summary>
        /// Adds a text run to the current paragraph.
        /// </summary>
        /// <param name="textRun">
        /// A
        /// <see cref="Infragistics.Documents.Word.TextRun">TextRun</see>
        /// instance which defines the properties of the text run.
        /// </param>
        /// <remarks>
        /// <p class="body">
        /// Before the AddTextRun method can be called, the developer must begin a paragraph via the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.StartParagraph()">StartParagraph</see>
        /// method, or an exception is thrown.
        /// </p>
        /// </remarks>
        /// <exception cref="WordDocumentWriterException">Thrown if no document or paragraph is currently open.</exception>
        /// <exception cref="WordDocumentWriterXmlWriterException">Thrown if the XmlWriter used to write content to the stream throws an exception.</exception>
        public override void AddTextRun( TextRun textRun )
        {
            WordprocessingMLWriter.AddTextRun( textRun, this.NewLineType, this.StateVerification, this.DocumentPartExporter );
        }

        static internal void AddTextRun(
            TextRun textRun,
            NewLineType defaultNewLineType,
            WriterStateVerification verifier,
            WordDocumentPartExporter partExporter )
        {
            verifier.VerifyParagraphIsOpen();
            partExporter.AddTextRun( textRun, RunStyle.None, defaultNewLineType );
        }

        #endregion AddTextRun

        #region AddHyperlink

        /// <summary>
        /// Adds a textual hyperlink to the current paragraph.
        /// </summary>
        /// <param name="hyperlink">
        /// A
        /// <see cref="Infragistics.Documents.Word.TextHyperlink">TextHyperlink</see>
        /// instance which defines the properties of the hyperlink.
        /// </param>
        /// <seealso cref="Infragistics.Documents.Word.TextHyperlink">TextHyperlink class</seealso>
        public override void AddHyperlink( TextHyperlink hyperlink )
        {
            if ( hyperlink == null )
                throw new ArgumentNullException( "hyperlink" );

            WordprocessingMLWriter.AddHyperlink(
                hyperlink.Address,
                hyperlink.GetTextRuns(),
                hyperlink.ToolTipText,
                false,
                this.NewLineType,
                this.StateVerification,
                this.DocumentPartExporter );
        }

        static internal void AddHyperlink(
            string address,
            IList<TextRun> textRuns,
            string toolTipText,
            bool addToHistory,
            NewLineType defaultNewLineType,
            WriterStateVerification verifier,
            WordDocumentPartExporter partExporter )
        {
            verifier.VerifyParagraphIsOpen();

            Hyperlink.VerifyAddress( ref address, false );

            partExporter.AddHyperlink( address, textRuns, toolTipText, addToHistory, defaultNewLineType );
        }

        #endregion AddHyperlink

        #region AddInlinePicture
       
        /// <summary>
        /// Adds the specified
        /// <see cref="Infragistics.Documents.Word.InlinePicture">InlinePicture</see>
        /// to the current paragraph.
        /// </summary>
        /// <param name="inlinePicture">
        /// The
        /// <see cref="Infragistics.Documents.Word.InlinePicture">InlinePicture</see>
        /// to add.
        /// </param>
        public override void AddInlinePicture( InlinePicture inlinePicture )
        {
            WordprocessingMLWriter.AddInlinePicture(



                inlinePicture.Image,
                inlinePicture.Size,
                null,
                inlinePicture.HasHyperlink ? inlinePicture.Hyperlink : null,
                inlinePicture.AlternateTextDescription,
                this.StateVerification, this.DocumentPartExporter,
                this.Unit );
        }

        static internal void AddInlinePicture(



            Image image,
            SizeF? size,
            PictureOutlineProperties outline,
            Hyperlink hyperlink,
            string altText,
            WriterStateVerification verifier,
            WordDocumentPartExporter partExporter,
            UnitOfMeasurement unit )
        {
            verifier.VerifyParagraphIsOpen();



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)


            partExporter.AddInlinePicture( image, size, outline, hyperlink, altText, unit );
        }

        #endregion AddInlinePicture

        #region AddAnchoredPicture
        
        /// <summary>
        /// Adds an anchored picture with the specified size to the current paragraph.
        /// </summary>
        /// <param name="anchoredPicture">
        /// The
        /// <see cref="Infragistics.Documents.Word.AnchoredPicture">AnchoredPicture</see>
        /// instance to be added to the paragraph.
        /// </param>
        /// <remarks>
        /// <p class="body">
        /// An AnchoredPicture instance can be obtained using the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.CreateAnchoredPicture">CreateAnchoredPicture</see>
        /// method. An existing instance can also be used; changing properties on
        /// that instance does not affect previously added pictures because the information
        /// is written to the stream immediately.
        /// </p>
        /// <p class="body">
        /// Before the AddAnchoredPicture method can be called, the developer must begin a paragraph via the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.StartParagraph()">StartParagraph</see>
        /// method, or an exception is thrown.
        /// </p>
        /// </remarks>
        /// <exception cref="WordDocumentWriterException">Thrown if no document or paragraph is currently open.</exception>
        /// <exception cref="WordDocumentWriterXmlWriterException">Thrown if the XmlWriter used to write content to the stream throws an exception.</exception>
        public override void AddAnchoredPicture( AnchoredPicture anchoredPicture )
        {
            WordprocessingMLWriter.AddAnchoredPicture(



                anchoredPicture,
                this.StateVerification,
                this.DocumentPartExporter );
        }

        static internal void AddAnchoredPicture(



            AnchoredPicture anchoredPicture,
            WriterStateVerification verifier,
            WordDocumentPartExporter partExporter )
        {
            if ( anchoredPicture == null )
                throw new ArgumentNullException();

            //  TFS71042




            verifier.VerifyParagraphIsOpen();




            partExporter.AddAnchoredPicture( anchoredPicture );
        }

        #endregion AddAnchoredPicture

        #region AddAnchoredShape

        /// <summary>
        /// Adds an anchored shape with the specified size to the current paragraph.
        /// </summary>
        /// <param name="anchoredShape">
        /// The
        /// <see cref="Infragistics.Documents.Word.AnchoredShape">AnchoredShape</see>
        /// instance to be added to the paragraph.
        /// </param>
        /// <remarks>
        /// <p class="body">
        /// An AnchoredShape instance can be obtained using the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.CreateAnchoredShape(ShapeType)">CreateAnchoredShape</see>
        /// method. An existing instance can also be used; changing properties on
        /// that instance does not affect previously added shapes because the information
        /// is written to the stream immediately.
        /// </p>
        /// <p class="body">
        /// Before the AddAnchoredShape method can be called, the developer must begin a paragraph via the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.StartParagraph()">StartParagraph</see>
        /// method, or an exception is thrown.
        /// </p>
        /// <p class="body">
        /// Shapes are rendered by the consumer using Vector Markup Language (VML),
        /// which is supported in both MS Word 2007 and MS Word 2010.
        /// </p>
        /// </remarks>
        /// <seealso cref="Infragistics.Documents.Word.ShapeType">ShapeType enumeration</seealso>
        /// <exception cref="WordDocumentWriterException">Thrown if no document or paragraph is currently open.</exception>
        /// <exception cref="WordDocumentWriterXmlWriterException">Thrown if the XmlWriter used to write content to the stream throws an exception.</exception>
        public override void AddAnchoredShape( AnchoredShape anchoredShape )
        {
            WordprocessingMLWriter.AddAnchoredShape( anchoredShape, this.StateVerification, this.DocumentPartExporter );
        }

        static internal void AddAnchoredShape(
            AnchoredShape anchoredShape,
            WriterStateVerification verifier,
            WordDocumentPartExporter partExporter )
        {
            if ( anchoredShape == null )
                throw new ArgumentNullException();

            verifier.VerifyParagraphIsOpen();

            partExporter.AddAnchoredShape( anchoredShape );
        }

        #endregion AddAnchoredShape

        #region AddInlineShape

        /// <summary>
        /// Adds an
        /// <see cref="Infragistics.Documents.Word.InlineShape">InlineShape</see>
        /// to the current paragraph, using Vector Markup Language (VML).
        /// </summary>
        /// <param name="inlineShape">
        /// The instance to add.
        /// </param>
        /// <remarks>
        /// <p class="body">
        /// Before the AddInlineShape method can be called, the developer must begin a paragraph via the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.StartParagraph()">StartParagraph</see>
        /// method, or an exception is thrown.
        /// </p>
        /// <p class="body">
        /// Shapes are rendered by the consumer using Vector Markup Language (VML),
        /// which is supported in both MS Word 2007 and MS Word 2010.
        /// </p>
        /// </remarks>
        /// <seealso cref="Infragistics.Documents.Word.ShapeType">ShapeType enumeration</seealso>
        /// <exception cref="WordDocumentWriterException">Thrown if no document or paragraph is currently open.</exception>
        /// <exception cref="WordDocumentWriterXmlWriterException">Thrown if the XmlWriter used to write content to the stream throws an exception.</exception>
        public override void AddInlineShape( InlineShape inlineShape )
        {
            WordprocessingMLWriter.AddInlineShape( inlineShape, this.StateVerification, this.DocumentPartExporter );
        }

        static internal void AddInlineShape(
            InlineShape inlineShape,
            WriterStateVerification verifier,
            WordDocumentPartExporter partExporter )
        {
            if ( inlineShape == null )
                throw new ArgumentNullException();

            verifier.VerifyParagraphIsOpen();

            partExporter.AddInlineShape( inlineShape );
        }

        #endregion AddInlineShape

        #region StartTable

        /// <summary>
        /// Starts a new table in the document with the number of columns
        /// equal to the number of elements in the specified <paramref name="columnWidths"/>
        /// list, and with the width of each column defined by the corresponding value in the list.
        /// </summary>
        /// <param name="columnWidths">
        /// A list of values which contains the width for each column to be added.
        /// The number of columns is determined by the number of elements in the list.
        /// The unit of measure for each value in the list is determined by the value of the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.Unit">Unit</see>
        /// property.
        /// </param>
        /// <param name="properties">
        /// A reference to a
        /// <see cref="Infragistics.Documents.Word.TableProperties">TableProperties</see>
        /// instance which defines formatting attributes for the table.
        /// </param>
        /// <remarks>
        /// <p class="body">
        /// This method starts a &lt;tbl&gt; element.
        /// Callers are responsible for closing this element. The
        /// &lt;tbl&gt; element can be closed by calling the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.EndTable()">EndTable</see>
        /// method.
        /// </p>
        /// <p class="body">
        /// A table must contain at least one and no more than 63 columns; specifying
        /// a value outside this range results in an exception being thrown.
        /// </p>
        /// </remarks>
        /// <seealso cref="Infragistics.Documents.Word.WordDocumentWriter.EndTable()">EndTable</seealso>
        /// <exception cref="ArgumentNullException">Thrown if the value of the <paramref name="columnWidths"/> parameter is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the list referenced by the <paramref name="columnWidths"/> parameter is empty.</exception>
        /// <exception cref="WordDocumentWriterException">Thrown if no document is currently open.</exception>
        public override void StartTable( IList<float> columnWidths, TableProperties properties )
        {
            WordprocessingMLWriter.StartTable( columnWidths, properties, this.StateVerification, this.DocumentPartExporter, this.Unit );
        }

        static internal void StartTable(
            IList<float> columnWidths,
            TableProperties properties,
            WriterStateVerification verifier,
            WordDocumentPartExporter partExporter,
            UnitOfMeasurement unit )
        {
            if ( columnWidths == null )
                throw new ArgumentNullException( "columnWidths" );
            
            WordprocessingMLWriter.VerifyColumnCount( columnWidths.Count );

            //  We check to make sure they don't add nested rows,
            //  but we have to allow nesting in the sense that you
            //  can have a table within a cell. Clear the RowIsOpen
            //  flag here so we allow nesting tables within cells,
            //  but prevent a row from being added while one in that
            //  table is still open
            verifier.VerifyDocumentIsOpen();

            //  BF 1/18/11  TFS63381
            //  Tables can never appear within an open paragraph. They
            //  can be nested within another cell, but not as part of
            //  a paragraph.
            verifier.VerifyNoParagraphIsOpen(WordDocumentWriterExceptionReason.ParagraphNotClosedTableStarted);

            //  BF 1/27/11
            verifier.VerifyNoTableIsOpen( WordDocumentWriterExceptionReason.NestedTable );

            partExporter.StartTable( columnWidths, properties, unit );
            verifier.CurrentState.TableLevel += 1;
        }

        static internal void VerifyColumnCount( int columnCount )
        {
            if ( columnCount < 1 || columnCount > WordUtilities.MaxTableColumns )
            {
                Exception innerException = null;
                throw new ArgumentOutOfRangeException( SR.GetString("WordDocumentWriter_TooManyColumns", columnCount, WordUtilities.MaxTableColumns ), innerException );
            }

        }

        #endregion StartTable

        #region StartTableRow / EndTableRow

        /// <summary>
        /// Starts a new row in the current table.
        /// </summary>
        public override void StartTableRow( TableRowProperties properties )
        {
            WordprocessingMLWriter.StartTableRow( properties, this.StateVerification, this.DocumentPartExporter );
        }

        static internal void StartTableRow(
            TableRowProperties properties,
            WriterStateVerification verifier,
            WordDocumentPartExporter partExporter )
        {
            verifier.VerifyTableIsOpen();
            verifier.VerifyNoTableRowIsOpen( true, WordDocumentWriterExceptionReason.NestedRow );
            verifier.VerifyNoParagraphIsOpen( WordDocumentWriterExceptionReason.ParagraphNotClosedTableRowStarted );

            partExporter.StartTableRow( properties );

            //  BF 3/2/11
            verifier.CurrentState.CellWritten_TableRow = false;
        }

        /// <summary>
        /// Closes the current table row.
        /// </summary>
        public override void EndTableRow()
        {
            WordprocessingMLWriter.EndTableRow( this.StateVerification, this.DocumentPartExporter );
        }

        static internal void EndTableRow(
            WriterStateVerification verifier,
            WordDocumentPartExporter partExporter )
        {
            verifier.VerifyTableRowIsOpen( true );

            //  BF 3/2/11
            //  A table row must have at least one cell, so if
            //  this one doesn't, throw an exception.
            verifier.VerifyTableRowHasCell();
            
            partExporter.EndTableRow();
        }

        #endregion StartTableRow / EndTableRow

        #region StartTableCell / EndTableCell / AddTableCell

        /// <summary>
        /// Starts a new cell in the current row.
        /// </summary>
        public override void StartTableCell( TableCellProperties properties )
        {
            WordprocessingMLWriter.StartTableCell( properties, this.StateVerification, this.DocumentPartExporter );
        }

        static internal void StartTableCell(
            TableCellProperties properties,
            WriterStateVerification verifier,
            WordDocumentPartExporter partExporter )
        {
            WordprocessingMLWriter.VerifyStartTableCell( verifier );
            partExporter.StartTableCell( properties );

            //  BF 3/2/11
            verifier.CurrentState.CellWritten_TableRow = true;
        }

        static private void VerifyStartTableCell( WriterStateVerification verifier )
        {
            verifier.VerifyTableIsOpen();
            verifier.VerifyTableRowIsOpen( false );
            verifier.VerifyNoTableCellIsOpen( true, WordDocumentWriterExceptionReason.NestedCell );
            verifier.VerifyNoParagraphIsOpen(WordDocumentWriterExceptionReason.ParagraphNotClosedTableCellStarted);
        }

        /// <summary>
        /// Closes a previously opened cell.
        /// </summary>
        public override void EndTableCell()
        {
            WordprocessingMLWriter.EndTableCell(
                this.StateVerification,
                this.DocumentPartExporter,
                new NoParagraphWrittenToCellDelegate(this.NoParagraphWrittenToCell) );
        }

        private void NoParagraphWrittenToCell()
        {
            this.AddEmptyParagraph();
        }

        internal delegate void NoParagraphWrittenToCellDelegate();

        static internal void EndTableCell(
            WriterStateVerification verifier,
            WordDocumentPartExporter partExporter,
            NoParagraphWrittenToCellDelegate noParagraphWrittenToCellDelegate )
        {
            verifier.VerifyTableCellIsOpen( true );

            if ( noParagraphWrittenToCellDelegate != null &&
                 verifier.VerifyCurrentTableCellHasParagraph() == false )
                noParagraphWrittenToCellDelegate();

            partExporter.EndTableCell();
        }

        /// <summary>
        /// Adds a cell to the current row, with one paragraph containing
        /// a simple text run with the specified <paramref name="text"/>,
        /// optionally with the specified properties.
        /// </summary>
        /// <param name="text">The string to be displayed within the cell.</param>
        /// <param name="properties">
        /// A
        /// <see cref="Infragistics.Documents.Word.TableCellProperties">TableCellProperties</see>
        /// instance which defines the formatting attributes for the cell.
        /// </param>
        public override void AddTableCell( string text, TableCellProperties properties )
        {
            WordprocessingMLWriter.AddTableCell( text, properties, this.NewLineType, this.StateVerification, this.DocumentPartExporter );
        }

        static internal void AddTableCell(
            string text,
            TableCellProperties properties,
            NewLineType defaultNewLineType,
            WriterStateVerification verifier,
            WordDocumentPartExporter partExporter )
        {
            WordprocessingMLWriter.StartTableCell( properties, verifier, partExporter );
            WordprocessingMLWriter.StartParagraph( null, verifier, partExporter );

            TextRun textRun = new TextRun(text, null);
            WordprocessingMLWriter.AddTextRun( textRun, defaultNewLineType, verifier, partExporter );
            WordprocessingMLWriter.EndParagraph( verifier, partExporter );
            WordprocessingMLWriter.EndTableCell( verifier, partExporter, null );
        }

        #endregion StartTableCell / EndTableCell / AddTableCell

        #region EndTable

        /// <summary>
        /// Closes a previously opened table, optionally adding an empty paragraph.
        /// </summary>
        /// <seealso cref="Infragistics.Documents.Word.WordDocumentWriter.StartTable(int)">StartTable</seealso>
        /// <exception cref="WordDocumentWriterException">Thrown if no document or paragraph is currently open.</exception>
        public override void EndTable()
        {
            WordprocessingMLWriter.EndTable( this.StateVerification, this.DocumentPartExporter );
        }

        static internal void EndTable(
            WriterStateVerification verifier,
            WordDocumentPartExporter partExporter )
        {
            //  TFS67600
            verifier.VerifyNoTableCellIsOpen( false, WordDocumentWriterExceptionReason.CellNotClosedTableClosed );
            verifier.VerifyNoTableRowIsOpen( false, WordDocumentWriterExceptionReason.RowNotClosedTableClosed );

            //  Make sure there is a table open
            verifier.VerifyTableIsOpen();

            //  Close the <w:tbl> element
            partExporter.EndTable();

            verifier.CurrentState.TableLevel -= 1;

            verifier.CurrentState.ParagraphWritten_EndTable = false;
        }

        #endregion EndTable

        #region AddEmptyParagraph
        
        static internal void AddEmptyParagraph(
            WriterStateVerification verifier,
            WordDocumentPartExporter partExporter )
        {
            WordprocessingMLWriter.StartParagraph( null, verifier, partExporter );
            WordprocessingMLWriter.EndParagraph( verifier, partExporter );
        }
        
        #endregion AddEmptyParagraph

        #region Open

        internal void Open( Stream stream, IPackageFactory packageFactory )
		{
//  BF 1/6/11   TFS62660
//#if CLR3

            if ( packageFactory == null )
                packageFactory = new PackageFactory();

            if ( packageFactory == null )
                throw new ArgumentNullException( "packageFactory" );

            this.stream = stream;

			IPackage package = packageFactory.Open( stream, FileMode.Create );

            if ( this.exportManager == null )
                this.exportManager = new WordDocumentExportManager( package );
		} 

        #endregion Open

        #region DefineSection

        /// <summary>
        /// Creates a section in the document, which defines the pagination
        /// for paragraphs and tables that were written since the last section
        /// was created.
        /// </summary>
        /// <param name="properties">
        /// A
        /// <see cref="Infragistics.Documents.Word.SectionProperties">SectionProperties</see>
        /// instance which defines properties such as page size, margins, and orientation.
        /// An new instance of this class can be obtained by calling the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.CreateSectionProperties">CreateSectionProperties</see>
        /// method.
        /// </param>
        /// <remarks>
        /// <p class="body">
        /// WordprocessingML does not natively store the concept of pages, since the
        /// number of pages in a document is driven by the nature of the content.
        /// Consider the case where the user changes the size of the font for all text
        /// in a document; the number of pages in that document is likely to change,
        /// since factors such as the number of characters in the document and
        /// the size of each character determine the total amount of space required to
        /// present the content.
        /// </p>
        /// <p class="body">
        /// This method defines a section which applies to content (i.e., paragraphs
        /// and tables) that was previously written to the document. The properties
        /// of the object passed to this method define the page size, orientation,
        /// and margins for the pages on which that content will appear, thus defining
        /// a section in the document to which the designated content belongs.
        /// </p>
        /// <p class="body">
        /// <b>Example: </b> To add three paragraphs which are to appear on pages that
        /// are 7" x 5" at landscape orientation, first call the StartDocument, AddTextRun,
        /// and EndDocument methods for each of the three paragraphs. Next, call the
        /// DefineSection method, passing in a SectionProperties instance on which the
        /// PageSize property is set to (7, 5), and the PageOrientation property is set
        /// to 'Landscape'.
        /// </p>
        /// <p class="body">
        /// The
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.FinalSectionProperties">FinalSectionProperties</see>
        /// property can be used to define the pagination for all content which is
        /// not associated with any section defined by this method.
        /// </p>
        /// </remarks>
        /// <exception cref="WordDocumentWriterException">Thrown if no document is currently open, or if a paragraph is currently open.</exception>
        /// <seealso cref="Infragistics.Documents.Word.WordDocumentWriter.CreateSectionProperties()">CreateSectionProperties</seealso>
        /// <seealso cref="Infragistics.Documents.Word.WordDocumentWriter.FinalSectionProperties">FinalSectionProperties</seealso>
        public override void DefineSection( SectionProperties properties )
        {
            this.StateVerification.VerifyDocumentIsOpen();

            //  TFS70395
            this.StateVerification.VerifyDefineSection();

            this.DocumentPartExporter.DefineSection( properties );
        }

        #endregion DefineSection

        #region Close
        /// <summary>
        /// Closes the writer and finalizes content.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// If the 
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.StartDocument">StartDocument</see>
        /// has not been called prior to calling this method, this method does nothing.
        /// </p>
        /// <p class="body">
        /// If the StartDocument method was previously called, but the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.EndDocument(bool)">EndDocument</see>
        /// method was not, the EndDocument method is called by this method before proceeding.
        /// </p>
        /// </remarks>
        public override void Close()
        {
            //  BF 2/11/11  TFS66152
            if ( this.StateVerification.CurrentState.WriterClosed )
            {
                // MJK 8/3/11 TFS81579
                this.CloseStreamIfOwned();
                return;
            }

            //  NoOp if the document was never opened
            if (this.StateVerification.CurrentState.DocumentOpened == false)
            {
                // MJK 8/3/11 TFS81579
                this.CloseStreamIfOwned();
                return;
            }

            //  Generally speaking we want to throw exceptions when they call things
            //  out of sync, but I think it is worse to throw an exception in the
            //  Dispose method than to just close it for them. If they got that far,
            //  they only made one mistake and that should not make everything fail.
            //
            if ( this.StateVerification.CurrentState.DocumentOpened &&
                 this.StateVerification.CurrentState.DocumentClosed == false )
                this.EndDocument( false );

            //  BF 1/27/11
            //  We can track these and make sure they are closed before the writer is.
            this.VerifyHeaderFooterWriters();

            //  BF 2/11/11  TFS66152
            //  To make a long story short, don't do this...it will
            //  happen when the WordDocumentPartExporter is closed
            //  (see below)
            #region Obsolete
            //            //  BF 1/10/11   TFS62660
//            //  Under SL, the stream has to be zipped differently.
//            //  Note that we must do this before disposing of the
//            //  export manager, because that class will close and
//            //  dispose all the streams that were opened for each
//            //  package part that was created.
//#if SILVERLIGHT
//            if ( weOwnStream == false )
//                this.exportManager.SaveToZip( this.stream );
//#endif
            #endregion Obsolete

            if ( this.exportManager != null )
            {
                this.exportManager.Close();

                //  Unhook the XmlException event
                if ( this.xmlExceptionHandler != null )
                {
                    this.DocumentPartExporter.XmlException -= this.xmlExceptionHandler;
                    this.xmlExceptionHandler = null;
                }
            }

            // MJK 8/3/11 TFS81579
            this.CloseStreamIfOwned();
            #region refactored
            ////  BF 1/6/11   TFS62660
            ////  Added a check to make sure we own the stream, otherwise
            ////  we won't close it.
            ////  Dispose of the stream
            //if ( this.weOwnStream && this.stream != null )
            //{
            //    this.stream.Flush();
            //    this.stream.Close();
            //    this.stream.Dispose();
            //    this.stream = null;
            //}
            #endregion

            this.DisposeHeaderFooterWriters();

            if ( this.exportManager != null )
            {
                this.exportManager.Dispose();
                this.exportManager = null;
            }

            //  BF 2/11/11  TFS66152
            this.StateVerification.CurrentState.WriterClosed = true;
        }

        // MJK 8/3/11 TFS81579
        // refactored this code block as I am calling it in other places 
        // of the Close method now.
        private void CloseStreamIfOwned()
        {
            //  BF 1/6/11   TFS62660
            //  Added a check to make sure we own the stream, otherwise
            //  we won't close it.
            //  Dispose of the stream
            if (this.weOwnStream && this.stream != null)
            {
                this.stream.Flush();
                this.stream.Close();
                this.stream.Dispose();
                this.stream = null;
            }
        }
        #endregion Close

        #region AddSectionHeaderFooter

        /// <summary>
        /// Adds headers and/or footers to the specified section of the document.
        /// </summary>
        /// <param name="parts">
        /// A bitflags value which defines the header and/or footer parts to be included in the section.
        /// </param>
        /// <param name="sectionProperties">
        /// A
        /// <see cref="Infragistics.Documents.Word.SectionProperties">SectionProperties</see>
        /// instance which contains the properties for the document section in which the headers/footers
        /// will appear. When null is specified, headers/footers are created for the final section,
        /// with the section properties being defined by the instance returned from the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.FinalSectionProperties"></see>
        /// property.
        /// </param>
        /// <seealso cref="Infragistics.Documents.Word.WordDocumentWriter.FinalSectionProperties">FinalSectionProperties</seealso>
        /// <seealso cref="Infragistics.Documents.Word.WordDocumentWriter.DefineSection">DefineSection</seealso>
        /// <seealso cref="Infragistics.Documents.Word.SectionProperties">SectionProperties class</seealso>
        public override SectionHeaderFooterWriterSet AddSectionHeaderFooter( SectionHeaderFooterParts parts, SectionProperties sectionProperties )
        {
            //  TFS67360
            if ( sectionProperties == null )
                sectionProperties = this.FinalSectionProperties;

            bool isFinalSection = sectionProperties == this.FinalSectionProperties;

            //  TFS70395
            //
            //  Note that we could probably allow this for the final section,
            //  because the XML elements are not written out until the document
            //  is closed, but we will disallow it so as to be consistent with
            //  the DefineSection method.
            //
            this.StateVerification.VerifyDefineSection();

            return this.DocumentPartExporter.AddSectionHeaderFooter( this, parts, sectionProperties, isFinalSection );
        }

        #endregion AddSectionHeaderFooter

        #region AddPageNumberField

        //  BF 4/12/11  TFS72408
        #region Obsolete
        ///// <summary>
        ///// Adds a page numbering field to the current paragraph.
        ///// </summary>
        ///// <param name="pageNumberField">
        ///// A
        ///// <see cref="Infragistics.Documents.Word.PageNumberField">PageNumberField</see>
        ///// instance which defines the properties of the page number.
        ///// </param>
        ///// <remarks>
        ///// <p class="body">
        ///// This method can only be called when a paragraph is currently open.
        ///// A page number field is similar to a text run; it can be embedded
        ///// within the paragraph adjacent to any other kind of content such
        ///// as text, images, or hyperlinks.
        ///// </p>
        ///// <p class="body">
        ///// A page number field can be added to either a header or footer.
        ///// </p>
        ///// <p class="body">
        ///// The page number for the first page in a section can be explicitly set using the
        ///// <see cref="Infragistics.Documents.Word.SectionProperties.StartingPageNumber">StartingPageNumber</see>
        ///// property. When this property is not explicitly set, page numbers for new sections
        ///// continue sequentially from the last page of the previous section.
        ///// </p>
        ///// </remarks>
        //public override void AddPageNumberField( PageNumberField pageNumberField )
        //{
        //    if ( pageNumberField == null )
        //        throw new ArgumentNullException( "pageNumberField" );

        //    WordprocessingMLWriter.AddPageNumberField(
        //        this.StateVerification,
        //        this.DocumentPartExporter,
        //        pageNumberField.Format,
                
        //        //  BF 4/12/11  TFS72391
        //        //pageNumberField.Font );
        //        pageNumberField.HasFont ? pageNumberField.Font : null );
        //}
        #endregion Obsolete

        static internal void AddPageNumberField(
            WriterStateVerification verifier,
            WordDocumentPartExporter partExporter,
            PageNumberFieldFormat format,
            Font font )
        {
            verifier.VerifyParagraphIsOpen();
            partExporter.AddPageNumberField( format, font );
        }

        #endregion AddPageNumberField

        #region Flush
        /// <summary>
        /// Flushes the contents of the buffer to the underlying stream.
        /// </summary>
        public override void Flush()
        {
            if ( (this.WriterState & WordDocumentWriterState.DocumentOpen) != WordDocumentWriterState.DocumentOpen )
                return;

            this.DocumentPartExporter.Flush();
        }
        #endregion Flush

        #endregion Methods

        #region Dispose

        /// <summary>
        /// Closes any open streams and releases any resources being use by this object.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// If the document has been properly started but has not yet been closed
        /// by a call to the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.EndDocument(bool)">EndDocument</see>
        /// method, the EndDocument method is automatically called prior to closing and disposing the stream.
        /// </p>
        /// </remarks>
        public override void Dispose()
        {
            this.Close();
        }

        #endregion Dispose
    }
    #endregion WordprocessingMLWriter class

    #region TextRun class
    /// <summary>
    /// Class which pairs a string value with a
    /// <see cref="Infragistics.Documents.Word.Font">Font</see>.
    /// </summary>

    [InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_IGWord)]

    public class TextRun : ParagraphContent
    {
        #region Member variables
        private string text = null;
        private WORD.Font font = null;
        private bool? checkSpellingAndGrammar = null;
        private NewLineType? newLineType = null;
        #endregion Member variables

        #region Constructor

        internal TextRun( string text, WORD.Font font ) : this( null, text, font )
        {
        }

        internal TextRun( IUnitOfMeasurementProvider unitOfMeasurementProvider, string text, WORD.Font font ) : base( unitOfMeasurementProvider )
        {
            this.text = text;
            this.font = font;
        }

        /// <summary>
        /// Returns a new instance which is associated with the specified
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter">WordDocumentWriter</see>.
        /// </summary>
        /// <param name="writer">
        /// The associated
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter">WordDocumentWriter</see>
        /// instance.
        /// </param>
        /// <param name="text">
        /// The text displayed for the hyperlink.
        /// </param>
        /// <param name="font">
        /// A previously created
        /// <see cref="Infragistics.Documents.Word.Font">Font</see>
        /// instance which determines the font attributes for the text run.
        /// Null can be specified, in which case the text run inherits the font
        /// attributes as defined at the document level.
        /// </param>
        /// <returns>
        /// A new
        /// <see cref="Infragistics.Documents.Word.PictureOutlineProperties">PictureOutlineProperties</see>
        /// instance.
        /// </returns>
        static public TextRun Create( WordDocumentWriter writer, string text, WORD.Font font )
        {
            return new TextRun( writer, text, font );
        }

        #endregion Constructor

        #region Properties

        #region Text
        /// <summary>
        /// Returns or sets the string displayed by this run.
        /// </summary>
        public string Text
        { 
            get { return this.text; }
            set { this.text = value; }
        }
        #endregion Text

        #region Font
        /// <summary>
        /// Returns or sets the font which defines the formatting for this run.
        /// </summary>
        public WORD.Font Font
        { 
            get
            { 
                if ( this.font == null )
                    this.font = new WORD.Font( this.unitOfMeasurementProvider );

                return this.font;
            }

            set { this.font = value; }
        }

        internal bool HasFont { get { return this.font != null && this.font.ShouldSerialize(); } }

        #endregion Font

        #region CheckSpellingAndGrammar
        /// <summary>
        /// Retuns or sets a boolean value indicating whether the contents
        /// of this run are checked for spelling and grammar.
        /// </summary>
        public bool? CheckSpellingAndGrammar
        {
            get { return this.checkSpellingAndGrammar; }
            set { this.checkSpellingAndGrammar = value; }
        }
        #endregion CheckSpellingAndGrammar

        #region NewLineType
        /// <summary>
        /// Returns or sets a value which determines the method used to
        /// represent a newline in the generated output.
        /// </summary>
        /// <seealso cref="Infragistics.Documents.Word.WordDocumentWriter.NewLineType">NewLineType (WordDocumentWriter class)</seealso>
        public NewLineType? NewLineType
        {
            get { return this.newLineType; }
            set { this.newLineType = value; }
        }
        #endregion NewLineType

        #endregion Properties

        #region Methods

        #region Reset
        /// <summary>
        /// Restores all properties of this instance to their respective defaults.
        /// </summary>
        /// <param name="text">
        /// The new value for the
        /// <see cref="Infragistics.Documents.Word.TextRun.Text">Text</see>
        /// property.
        /// </param>
        public void Reset( string text )
        {
            this.text = text;
            this.font = null;
            this.checkSpellingAndGrammar = null;
            this.newLineType = null;
        }
        #endregion Reset

        #region ToList
        static internal List<TextRun> ToList( TextRun item )
        {
            return new List<TextRun>( new TextRun[]{ item } );
        }
        #endregion ToList

        #region ToString
        /// <summary>
        /// Returns the string representation of this object.
        /// </summary>
        public override string ToString()
        {
            return
                String.IsNullOrEmpty(this.text) == false ?
                this.text.Length < 256 ?
                this.text :
                string.Format("{0}...", this.text.Substring(0, 256)) :
                base.ToString();
        }
        #endregion ToString

        #endregion Methods
    }
    #endregion TextRun class

    #region WriterStateVerification






    internal class WriterStateVerification
    {
        #region Member variables

        private WriterState currentState = new WriterState();
        private bool bypassDocumentOpenVerification = true;

        #endregion Member variables

        #region Constructor
        internal WriterStateVerification() : this( true ){} 
       
        /// <summary>
        /// Creates an instance of the class.
        /// </summary>
        /// <param name="bypassDocumentOpenVerification">
        /// Unless you directly call VerifyDocumentIsOpen it will not
        /// be called when you cal things like VerifyParagraphIsOpen
        /// </param>
        internal WriterStateVerification( bool bypassDocumentOpenVerification )
        {
            this.bypassDocumentOpenVerification = bypassDocumentOpenVerification;
        }
        #endregion Constructor

        #region Properties

        #region CurrentState
        internal WriterState CurrentState { get { return this.currentState; } }
        #endregion CurrentState

        #region State
        /// <summary>
        /// Returns a bitflag value which describes the current state of the writer.
        /// </summary>
        public WordDocumentWriterState State
        {
            get
            {
                WordDocumentWriterState state = WordDocumentWriterState.None;

                if ( this.currentState != null )
                {
                    if ( this.currentState.DocumentOpened &&
                         this.currentState.DocumentClosed == false )
                        state |= WordDocumentWriterState.DocumentOpen;

                    if ( this.currentState.ParagraphOpen )
                        state |= WordDocumentWriterState.ParagraphOpen;

                    if ( this.currentState.TableLevel >= 0 )
                        state |= WordDocumentWriterState.TableOpen;

                    if ( this.currentState.TableLevelOfLastRowStarted.HasValue )
                        state |= WordDocumentWriterState.TableRowOpen;

                    if ( this.currentState.TableLevelOfLastCellStarted.HasValue )
                        state |= WordDocumentWriterState.TableCellOpen;
                }

                return state;
            }
        }
        #endregion State

        #endregion Properties

        #region Methods

        #region VerifyDocumentIsOpen
        public void VerifyDocumentIsOpen()
        {
            if ( this.currentState.DocumentOpened == false )
                throw new WordDocumentWriterException(WordDocumentWriterExceptionReason.DocumentNotOpen);
        }
        #endregion VerifyDocumentIsOpen

        #region VerifyParagraphIsOpen
        public void VerifyParagraphIsOpen()
        {
            //  Anything that needs an open paragraph also needs
            //  an open document.
            if ( this.bypassDocumentOpenVerification )
                this.VerifyDocumentIsOpen();

            if ( this.currentState.ParagraphOpen == false )
                throw new WordDocumentWriterException(WordDocumentWriterExceptionReason.ParagraphNotOpen);
        }
        #endregion VerifyParagraphIsOpen

        #region VerifyNoParagraphIsOpen
        public void VerifyNoParagraphIsOpen( WordDocumentWriterExceptionReason reason )
        {
            if ( this.currentState.ParagraphOpen )
                throw new WordDocumentWriterException(reason);
        }
        #endregion VerifyNoParagraphIsOpen

        #region VerifyTableIsOpen
        public void VerifyTableIsOpen()
        {
            //  Anything that needs an open table also needs
            //  an open document.
            if ( this.bypassDocumentOpenVerification )
                this.VerifyDocumentIsOpen();

            if ( this.currentState.TableLevel < 0 )
                throw new WordDocumentWriterException(WordDocumentWriterExceptionReason.TableNotOpen);
        }
        #endregion VerifyTableIsOpen

        #region VerifyNoTableIsOpen
        public void VerifyNoTableIsOpen( WordDocumentWriterExceptionReason reason )
        {
            switch ( reason )
            {
                case WordDocumentWriterExceptionReason.TableOpenSectionStarted:
                    if ( this.currentState.TableLevel >= 0 )
                        throw new WordDocumentWriterException(reason);
                    break;

                case WordDocumentWriterExceptionReason.NestedTable:
                    if ( this.currentState.TableLevel >= 0 &&
                         this.currentState.TableLevelOfLastCellStarted.HasValue == false )
                        throw new WordDocumentWriterException(reason);
                    break;

                case WordDocumentWriterExceptionReason.TableNotClosed:
                    if ( this.currentState.TableLevel >= 0 )
                        throw new WordDocumentWriterException(reason);
                    break;

                default:
                    WordUtilities.DebugFail( "Unrecognized WordDocumentWriterExceptionReason constant in VerifyNoTableIsOpen." );
                    break;
            }
        }
        #endregion VerifyNoTableIsOpen

        #region VerifyTableRowIsOpen
        public void VerifyTableRowIsOpen( bool setLevel )
        {
            if ( this.currentState.RowIsOpen == false )
                throw new WordDocumentWriterException(WordDocumentWriterExceptionReason.RowNotOpen);

            //  Clear this so we don't check until a row is added.
            if ( setLevel )
            {
                //  Decrement the current level if it is non-zero, otherwise
                //  nullify the member to indicate that a root-level row was
                //  closed.
                if ( this.currentState.TableLevelOfLastRowStarted.HasValue &&
                     this.currentState.TableLevelOfLastRowStarted.Value > 0 )
                    this.currentState.TableLevelOfLastRowStarted -= 1;
                else
                    this.currentState.TableLevelOfLastRowStarted = null;
            }
        }
        #endregion VerifyTableRowIsOpen

        #region VerifyTableRowHasCell
        public void VerifyTableRowHasCell()
        {
            bool hasCell = this.CurrentState.CellWritten_TableRow;
            if ( hasCell == false )
                throw new WordDocumentWriterException(WordDocumentWriterExceptionReason.RowClosedNoCell);
        }
        #endregion VerifyTableRowHasCell

        #region VerifyNoTableRowIsOpen
        public void VerifyNoTableRowIsOpen( bool setLevel, WordDocumentWriterExceptionReason reason )
        {
            //  If the level of the table for this new row is the same as the last one,
            //  they are trying to add a row within a row at the same table level, which
            //  is not allowed.
            if ( this.currentState.TableLevelOfLastRowStarted.HasValue &&
                 this.currentState.TableLevelOfLastRowStarted.Value == this.currentState.TableLevel )
                throw new WordDocumentWriterException(reason);

            //  'Sync' the table level for the next check.
            if ( setLevel )
                this.currentState.TableLevelOfLastRowStarted = this.currentState.TableLevel;
        }
        #endregion VerifyNoTableRowIsOpen

        #region VerifyTableCellIsOpen
        public void VerifyTableCellIsOpen( bool setLevel )
        {
            if ( this.currentState.CellIsOpen == false )
                throw new WordDocumentWriterException(WordDocumentWriterExceptionReason.CellNotOpen);

            //  Clear this so we don't check until a cell is added.
            if ( setLevel )
            {
                //  Decrement the current level if it is non-zero, otherwise
                //  nullify the member to indicate that a root-level row was
                //  closed.
                if ( this.currentState.TableLevelOfLastCellStarted.HasValue &&
                     this.currentState.TableLevelOfLastCellStarted.Value > 0 )
                    this.currentState.TableLevelOfLastCellStarted -= 1;
                else
                    this.currentState.TableLevelOfLastCellStarted = null;
            }
        }
        #endregion VerifyTableCellIsOpen

        #region VerifyNoTableCellIsOpen
        public void VerifyNoTableCellIsOpen( bool setLevel, WordDocumentWriterExceptionReason reason )
        {
            //  If the level of the table for this new cell is the same as the last one,
            //  they are trying to add a cell within a cell at the same table level, which
            //  is not allowed.
            if ( this.currentState.TableLevelOfLastCellStarted.HasValue &&
                 this.currentState.TableLevelOfLastCellStarted.Value == this.currentState.TableLevel )
                throw new WordDocumentWriterException(reason);

            //  'Sync' the table level for the next check.
            if ( setLevel )
            {
                //  BF 3/1/11
                this.currentState.ParagraphWritten_TableCell = false;

                this.currentState.TableLevelOfLastCellStarted = this.currentState.TableLevel;
            }
        }
        #endregion VerifyNoTableCellIsOpen

        //  BF 3/1/11
        #region VerifyCurrentTableCellHasParagraph
        public bool VerifyCurrentTableCellHasParagraph()
        {
            if ( this.VerifyCurrentTableCellHasParagraphAfterTable() == false )
                return false;

            bool hasParagraph = this.currentState.ParagraphWritten_TableCell;
            this.currentState.ParagraphWritten_TableCell = false;
            return hasParagraph;
        }
        #endregion VerifyCurrentTableCellHasParagraph

        #region VerifyCurrentTableCellHasParagraphAfterTable
        private bool VerifyCurrentTableCellHasParagraphAfterTable()
        {
            if ( this.currentState.TableLevel < 0 )
                return true;

            bool hasParagraph = this.currentState.ParagraphWritten_EndTable;
            this.currentState.ParagraphWritten_EndTable = false;
            return hasParagraph;
        }
        #endregion VerifyCurrentTableCellHasParagraphAfterTable

        #region VerifyDocumentClosure
        public void VerifyDocumentClosure( WordDocumentWriterExceptionReason reason )
        {
            //  Throw a specific exception for the reason the doc
            //  can't be closed, catch it, and rethrow it as a
            //  'DocumentClosureException', with the specific exception
            //  as the reason.
            try
            {
                //  Make sure the document was opened
                if ( this.bypassDocumentOpenVerification )
                    this.VerifyDocumentIsOpen();

                //  Make sure there are no paragraphs pending closure
                this.VerifyNoParagraphIsOpen(WordDocumentWriterExceptionReason.ParagraphNotClosed);

                //  Table cells
                this.VerifyNoTableCellIsOpen( false, WordDocumentWriterExceptionReason.CellNotClosed );

                //  Table rows
                this.VerifyNoTableRowIsOpen( false, WordDocumentWriterExceptionReason.RowNotClosed );

                //  Table rows
                this.VerifyNoTableIsOpen( WordDocumentWriterExceptionReason.TableNotClosed );
            }
            catch( Exception innerException )
            {
                Exception exception = new WordDocumentWriterException( reason, innerException );

                throw exception;
            }
        }
        #endregion VerifyDocumentClosure

        #region VerifyDefineSection
        public void VerifyDefineSection()
        {
            this.VerifyNoParagraphIsOpen( WordDocumentWriterExceptionReason.ParagraphNotClosedSectionStarted );
            this.VerifyNoTableCellIsOpen( false, WordDocumentWriterExceptionReason.CellOpenSectionStarted );
            this.VerifyNoTableRowIsOpen( false, WordDocumentWriterExceptionReason.RowOpenSectionStarted );
            this.VerifyNoTableIsOpen( WordDocumentWriterExceptionReason.TableOpenSectionStarted );
        }
        #endregion VerifyDefineSection

        #endregion Methods

        #region WriterState class
        internal class WriterState
        {
            private bool documentOpened = false;
            private bool documentClosed = false;
            private bool paragraphOpen = false;
            private int tableLevel = -1;
            private int? tableLevelOfLastRowStarted = null;
            private int? tableLevelOfLastCellStarted = null;
            private bool writerClosed = false;

            private List<bool> paragraphWritten_TableCell = new List<bool>();
            private List<bool> cellWritten_TableRow = new List<bool>();
            private List<bool> paragraphWritten_EndTable = new List<bool>();

            internal bool DocumentOpened
            {
                get { return this.documentOpened; }
                set { this.documentOpened = value; }
            }

            internal bool DocumentClosed
            {
                get { return this.documentClosed; }
                set { this.documentClosed = value; }
            }

            internal bool WriterClosed
            {
                get { return this.writerClosed; }
                set { this.writerClosed = value; }
            }

            internal bool ParagraphOpen
            {
                get { return this.paragraphOpen; }
                set { this.paragraphOpen = value; }
            }

            internal bool RowIsOpen
            {
                //  TFS67939
                //get { return this.tableLevelOfLastRowStarted.HasValue; }

                get
                {
                    if ( this.tableLevelOfLastRowStarted.HasValue == false )
                        return false;

                    return this.tableLevelOfLastRowStarted.Value == this.tableLevel;
                }
            }

            internal bool CellIsOpen
            {
                //  TFS67939
                //get { return this.tableLevelOfLastCellStarted.HasValue; }

                get
                {
                    if ( this.tableLevelOfLastCellStarted.HasValue == false )
                        return false;

                    return this.tableLevelOfLastCellStarted.Value == this.tableLevel;
                }
            }

            internal int TableLevel
            {
                get { return this.tableLevel; }
                set
                {
                    //  BF 3/1/11
                    this.UpdateTableLevelDependencies( value );

                    this.tableLevel = value;
                }
            }

            private void UpdateTableLevelDependencies( int newLevel )
            {
                if ( newLevel > this.tableLevel )
                {
                    this.paragraphWritten_TableCell.Add( false );
                    this.cellWritten_TableRow.Add( false );
                    this.paragraphWritten_EndTable.Add( false );
                }
                else
                if ( newLevel < this.tableLevel )
                {
                    if ( this.paragraphWritten_TableCell.Count > 0 )
                        this.paragraphWritten_TableCell.RemoveAt(this.paragraphWritten_TableCell.Count - 1);

                    if ( this.cellWritten_TableRow.Count > 0 )
                        this.cellWritten_TableRow.RemoveAt(this.cellWritten_TableRow.Count - 1);

                    if ( this.paragraphWritten_EndTable.Count > 0 )
                        this.paragraphWritten_EndTable.RemoveAt(this.paragraphWritten_EndTable.Count - 1);
                }

                if ( (this.paragraphWritten_TableCell.Count - 1) != newLevel )
                    WordUtilities.DebugFail( "Mismatch between paragraphWritten_TableCell count and table level." );
                
                if ( (this.cellWritten_TableRow.Count - 1) != newLevel )
                    WordUtilities.DebugFail( "Mismatch between cellWritten_TableRow count and table level." );
                
                if ( (this.paragraphWritten_EndTable.Count - 1) != newLevel )
                    WordUtilities.DebugFail( "Mismatch between paragraphWritten_EndTable count and table level." );                
            }

            internal int? TableLevelOfLastRowStarted
            {
                get { return this.tableLevelOfLastRowStarted; }
                set { this.tableLevelOfLastRowStarted = value; }
            }

            internal int? TableLevelOfLastCellStarted
            {
                get { return this.tableLevelOfLastCellStarted; }
                set { this.tableLevelOfLastCellStarted = value; }
            }

            internal bool ParagraphWritten_TableCell
            {
                get
                { 
                    if ( this.tableLevel < 0 || this.tableLevel >= this.paragraphWritten_TableCell.Count )
                    {
                        WordUtilities.DebugFail( "Should not be in here." );
                        return false;
                    }

                    return this.paragraphWritten_TableCell[this.tableLevel];
                }
                
                set
                { 
                    if ( this.tableLevel < 0 || this.tableLevel >= this.paragraphWritten_TableCell.Count )
                    {
                        WordUtilities.DebugFail( "Should not be in here." );
                        return;
                    }

                    this.paragraphWritten_TableCell[this.tableLevel] = value;
                }
            }

            internal bool CellWritten_TableRow
            {
                get
                { 
                    if ( this.tableLevel < 0 || this.tableLevel >= this.cellWritten_TableRow.Count )
                    {
                        WordUtilities.DebugFail( "Should not be in here." );
                        return false;
                    }

                    return this.cellWritten_TableRow[this.tableLevel];
                }
                
                set
                { 
                    if ( this.tableLevel < 0 || this.tableLevel >= this.cellWritten_TableRow.Count )
                    {
                        WordUtilities.DebugFail( "Should not be in here." );
                        return;
                    }

                    this.cellWritten_TableRow[this.tableLevel] = value;
                }
            }

            internal bool ParagraphWritten_EndTable
            {
                get
                { 
                    //  We only care if this property returns false, so if
                    //  it is irrelevant, return true.
                    if ( this.tableLevel < 0 )
                        return true;

                    if ( this.tableLevel < 0 || this.tableLevel >= this.paragraphWritten_EndTable.Count )
                    {
                        WordUtilities.DebugFail( "Should not be in here." );
                        return false;
                    }

                    return this.paragraphWritten_EndTable[this.tableLevel];
                }
                
                set
                { 
                    if ( this.tableLevel < 0 )
                        return;

                    if ( this.tableLevel < 0 || this.tableLevel >= this.paragraphWritten_EndTable.Count )
                    {
                        WordUtilities.DebugFail( "Should not be in here." );
                        return;
                    }

                    this.paragraphWritten_EndTable[this.tableLevel] = value;
                }
            }

        }
        #endregion WriterState class
    }
    #endregion WriterStateVerification class

    #region SectionHeaderFooterWriterSet
    /// <summary>
    /// Class which holds one or more
    /// <see cref="Infragistics.Documents.Word.WordHeaderFooterWriter">WordHeaderFooterWriter</see>
    /// instances for the purpose of writing content to the headers and footers in a
    /// document section.
    /// </summary>
    /// <seealso cref="Infragistics.Documents.Word.WordHeaderFooterWriter">WordHeaderFooterWriter class</seealso>

    [InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_IGWord)]

    public class SectionHeaderFooterWriterSet
    {
        #region Member variables
        
        WordHeaderFooterWriter footerWriterAllPages = null;
        WordHeaderFooterWriter footerWriterFirstPageOnly = null;
        WordHeaderFooterWriter headerWriterAllPages = null;
        WordHeaderFooterWriter headerWriterFirstPageOnly = null;
        
        #endregion Member variables

        #region Constructor
        internal SectionHeaderFooterWriterSet( List<WordHeaderFooterWriter> writerList )
        {
            if ( writerList == null )
                throw new ArgumentNullException( "writerList" );

            //  Assign the writer members
            foreach( WordHeaderFooterWriter item in writerList )
            {
                if ( item.IsHeader )
                {
                    switch ( item.Type )
                    {
                        case HeaderFooterType.AllPages:
                            this.headerWriterAllPages = item;
                            break;

                        case HeaderFooterType.FirstPageOnly:
                            this.headerWriterFirstPageOnly = item;
                            break;
                    }
                }
                else //  item.IsFooter                
                {
                    switch ( item.Type )
                    {
                        case HeaderFooterType.AllPages:
                            this.footerWriterAllPages = item;
                            break;

                        case HeaderFooterType.FirstPageOnly:
                            this.footerWriterFirstPageOnly = item;
                            break;
                    }
                }
            }
        }
        #endregion Constructor

        #region Properties

        #region FooterWriterAllPages
        /// <summary>
        /// Returns a
        /// <see cref="Infragistics.Documents.Word.WordHeaderFooterWriter">WordHeaderFooterWriter</see>
        /// derived instance with which content can be written to the footer of a document section.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// If the 'FooterAllPages' bit was set on the 'sectionParts' parameter when the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.AddSectionHeaderFooter(SectionHeaderFooterParts, SectionProperties)">AddSectionHeaderFooter</see>
        /// was called, this property returns a non-null value, on which content-creation
        /// methods such as 'StartParagraph' and 'AddTextRun' can be called.
        /// </p>
        /// <p class="body">
        /// If the 'FooterAllPages' bit was not set, this property returns null.
        /// </p>
        /// </remarks>
        /// <seealso cref="Infragistics.Documents.Word.WordDocumentWriter.AddSectionHeaderFooter(SectionHeaderFooterParts, SectionProperties)">AddSectionHeaderFooter method (WordDocumentWriter class)</seealso>
        public WordHeaderFooterWriter FooterWriterAllPages
        {
            get { return this.footerWriterAllPages; }
        }
        #endregion FooterWriterAllPages

        #region HeaderWriterAllPages
        /// <summary>
        /// Returns a
        /// <see cref="Infragistics.Documents.Word.WordHeaderFooterWriter">WordHeaderFooterWriter</see>
        /// derived instance with which content can be written to the header of a document section.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// If the 'HeaderAllPages' bit was set on the 'sectionParts' parameter when the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.AddSectionHeaderFooter(SectionHeaderFooterParts, SectionProperties)">AddSectionHeaderFooter</see>
        /// was called, this property returns a non-null value, on which content-creation
        /// methods such as 'StartParagraph' and 'AddTextRun' can be called.
        /// </p>
        /// <p class="body">
        /// If the 'HeaderAllPages' bit was not set, this property returns null.
        /// </p>
        /// </remarks>
        /// <seealso cref="Infragistics.Documents.Word.WordDocumentWriter.AddSectionHeaderFooter(SectionHeaderFooterParts, SectionProperties)">AddSectionHeaderFooter method (WordDocumentWriter class)</seealso>
        public WordHeaderFooterWriter HeaderWriterAllPages
        {
            get { return this.headerWriterAllPages; }
        }
        #endregion HeaderWriterAllPages

        #region FooterWriterFirstPageOnly
        /// <summary>
        /// Returns a
        /// <see cref="Infragistics.Documents.Word.WordHeaderFooterWriter">WordHeaderFooterWriter</see>
        /// derived instance with which content can be written to the footer for the first page of a
        /// document section.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// If the 'FooterFirst' bit was set on the 'sectionParts' parameter when the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.AddSectionHeaderFooter(SectionHeaderFooterParts, SectionProperties)">AddSectionHeaderFooter</see>
        /// was called, this property returns a non-null value, on which content-creation
        /// methods such as 'StartParagraph' and 'AddTextRun' can be called.
        /// </p>
        /// <p class="body">
        /// If the 'FooterFirst' bit was not set, this property returns null.
        /// </p>
        /// </remarks>
        /// <seealso cref="Infragistics.Documents.Word.WordDocumentWriter.AddSectionHeaderFooter(SectionHeaderFooterParts, SectionProperties)">AddSectionHeaderFooter method (WordDocumentWriter class)</seealso>
        public WordHeaderFooterWriter FooterWriterFirstPageOnly
        {
            get { return this.footerWriterFirstPageOnly; }
        }
        #endregion FooterWriterFirstPageOnly

        #region HeaderWriterFirstPageOnly
        /// <summary>
        /// Returns a
        /// <see cref="Infragistics.Documents.Word.WordHeaderFooterWriter">WordHeaderFooterWriter</see>
        /// derived instance with which content can be written to the header for the first page of a
        /// document section.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// If the 'HeaderFirst' bit was set on the 'sectionParts' parameter when the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.AddSectionHeaderFooter(SectionHeaderFooterParts, SectionProperties)">AddSectionHeaderFooter</see>
        /// was called, this property returns a non-null value, on which content-creation
        /// methods such as 'StartParagraph' and 'AddTextRun' can be called.
        /// </p>
        /// <p class="body">
        /// If the 'HeaderFirst' bit was not set, this property returns null.
        /// </p>
        /// </remarks>
        /// <seealso cref="Infragistics.Documents.Word.WordDocumentWriter.AddSectionHeaderFooter(SectionHeaderFooterParts, SectionProperties)">AddSectionHeaderFooter method (WordDocumentWriter class)</seealso>
        public WordHeaderFooterWriter HeaderWriterFirstPageOnly
        {
            get { return this.headerWriterFirstPageOnly; }
        }
        #endregion HeaderWriterFirstPageOnly

        #region Parts
        /// <summary>
        /// Returns a
        /// <see cref="Infragistics.Documents.Word.SectionHeaderFooterParts">SectionHeaderFooterParts</see>
        /// value to describe which of the header and footer parts are created by this class.
        /// </summary>
        public SectionHeaderFooterParts Parts
        {
            get
            {
                SectionHeaderFooterParts parts = SectionHeaderFooterParts.None;

                if ( this.headerWriterFirstPageOnly != null )
                    parts |= SectionHeaderFooterParts.HeaderFirstPageOnly;

                if ( this.headerWriterAllPages != null )
                    parts |= SectionHeaderFooterParts.HeaderAllPages;

                if ( this.footerWriterFirstPageOnly != null )
                    parts |= SectionHeaderFooterParts.FooterFirstPageOnly;

                if ( this.footerWriterAllPages != null )
                    parts |= SectionHeaderFooterParts.FooterAllPages;

                return parts;
            }
        }
        #endregion Parts

        #endregion Properties

        #region Methods

        #region ToString
        /// <summary>
        /// Returns the string representation of this instance.
        /// </summary>
        public override string ToString()
        {
            return this.Parts.ToString();
        }
        #endregion ToString

        #endregion Methods
    }
    #endregion SectionHeaderFooterWriterSet

    #region WordHeaderFooterWriter class
    /// <summary>
    /// Writes content to a header or footer within a section of the document.
    /// </summary>

    [InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_IGWord)]

    public abstract class WordHeaderFooterWriter : IContentWriter
    {
        #region Member variables
        
        WordDocumentWriter                  writer = null;
        HeaderOrFooter                      headerOrFooter = HeaderOrFooter.Header;
        HeaderFooterType                    type = HeaderFooterType.AllPages;
        
        #endregion Member variables

        internal WordHeaderFooterWriter(
            WordDocumentWriter writer,
            HeaderOrFooter headerOrFooter,
            HeaderFooterType type )
        {
            this.writer = writer;
            this.headerOrFooter = headerOrFooter;
            this.type = type;
        }

        #region Properties

        #region DocumentWriter
        /// <summary>
        /// Returns a reference to the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter">WordDocumentWriter</see>
        /// associated with this instance.
        /// </summary>
        public WordDocumentWriter DocumentWriter
        {
            get { return this.writer; }
        }
        #endregion DocumentWriter

        #region IsHeader
        /// <summary>
        /// Returns a boolean value indicating whether this instance represents a section header.
        /// </summary>
        public bool IsHeader { get { return this.headerOrFooter == HeaderOrFooter.Header; } }
        #endregion IsHeader

        #region IsFooter
        /// <summary>
        /// Returns a boolean value indicating whether this instance represents a section footer.
        /// </summary>
        public bool IsFooter { get { return this.IsHeader == false; } }
        #endregion IsFooter

        #region Type
        /// <summary>
        /// Returns a value which describes whether the asssociated header/footer
        /// appears on all pages in the section, or only on the first page of the
        /// section.
        /// </summary>
        public HeaderFooterType Type { get { return this.type; } }
        #endregion Type

        #region Unit
        internal UnitOfMeasurement Unit { get { return this.writer != null ? this.writer.Unit : WordUtilities.DefaultUnitOfMeasurement; } }
        #endregion Unit

        #region StateVerification
        internal virtual WriterStateVerification StateVerification { get { return null; } }
        #endregion StateVerification

        #endregion Properties

        #region Methods

        #region AddEmptyParagraph
        /// <summary>
        /// Adds a paragraph with no content.
        /// </summary>
        public void AddEmptyParagraph()
        {
            this.StartParagraph();
            this.EndParagraph();
        }
        #endregion AddEmptyParagraph

        #region StartParagraph

        /// <summary>
        /// Starts a new paragraph in the document with the default formatting.
        /// </summary>
        public void StartParagraph()
        {
            this.StartParagraph( null );
        }

        /// <summary>
        /// Starts a new paragraph in the document.
        /// </summary>
        public abstract void StartParagraph( ParagraphProperties properties );

        #endregion StartParagraph

        #region EndParagraph

        /// <summary>
        /// Closes a previously created paragraph.
        /// </summary>
        public abstract void EndParagraph();

        #endregion EndParagraph

        #region AddNewLine

        /// <summary>
        /// Adds a text run consisting of the newline character to the current paragraph.
        /// </summary>
        /// <seealso cref="Infragistics.Documents.Word.WordDocumentWriter.AddNewLine()">WordDocumentWriter.AddNewLine</seealso>
        public void AddNewLine()
        {
            this.AddNewLine( this.DocumentWriter.NewLineType );
        }

        /// <summary>
        /// Adds a text run consisting of the newline character to the current paragraph.
        /// </summary>
        /// <seealso cref="Infragistics.Documents.Word.WordDocumentWriter.AddNewLine(NewLineType)">WordDocumentWriter.AddNewLine</seealso>
        public void AddNewLine( NewLineType newLineType )
        {
            TextRun tr = this.DocumentWriter.GetAddNewLineTextRun(newLineType);
            this.AddTextRun( tr );
        }

        #endregion AddNewLine

        #region AddTextRun

        /// <summary>
        /// Adds a text run to the current paragraph.
        /// </summary>
        public void AddTextRun( string text )
        {
            this.AddTextRun( text, null );
        }

        /// <summary>
        /// Adds a text run to the current paragraph.
        /// </summary>
        public void AddTextRun( string text, WORD.Font font )
        {
            TextRun tr = this.DocumentWriter.GetTempTextRun( text, font );
            this.AddTextRun( tr );
        }

        /// <summary>
        /// Adds a text run to the current paragraph.
        /// </summary>
        public void AddTextRun( string text, WORD.Font font, bool? checkSpellingAndGrammar )
        {
            TextRun textRun = new TextRun( text, font );
            textRun.CheckSpellingAndGrammar = checkSpellingAndGrammar;
            this.AddTextRun( textRun );
        }

        /// <summary>
        /// Adds a text run to the current paragraph.
        /// </summary>
        public abstract void AddTextRun( TextRun textRun );

        #endregion AddTextRun

        #region AddHyperlink

        /// <summary>
        /// Adds a textual hyperlink to the current paragraph.
        /// </summary>
        public void AddHyperlink( string address, string text )
        {
            Font font = null;
            this.AddHyperlink( address, text, font ); 
        }

        /// <summary>
        /// Adds a textual hyperlink to the current paragraph.
        /// </summary>
        public void AddHyperlink( string address, string text, Font font )
        {
            List<TextRun> textRuns = TextRun.ToList( new TextRun(text, font) );
            this.AddHyperlink( address, textRuns, null ); 
        }

        /// <summary>
        /// Adds a textual hyperlink to the current paragraph.
        /// </summary>
        public void AddHyperlink( string address, string text, string toolTipText )
        {
            List<TextRun> textRuns = TextRun.ToList( new TextRun(text, null ) );
            this.AddHyperlink( address, textRuns, toolTipText ); 
        }

        /// <summary>
        /// Adds a textual hyperlink to the current paragraph.
        /// </summary>
        public void AddHyperlink( string address, IList<TextRun> textRuns, string toolTipText )
        {
            TextHyperlink hyperlink = TextHyperlink.Create( this.DocumentWriter, address, null );
            hyperlink.TextRuns = textRuns;
            hyperlink.ToolTipText = toolTipText;
            this.AddHyperlink( hyperlink );
        }

        /// <summary>
        /// Adds a textual hyperlink to the current paragraph.
        /// </summary>
        public abstract void AddHyperlink( TextHyperlink hyperlink );

        #endregion AddHyperlink

        #region AddInlinePicture
       
        /// <summary>
        /// Adds an inline picture to the current paragraph.
        /// </summary>
        public void AddInlinePicture( Image image )
        {
            this.AddInlinePicture( image, null );
        }

        /// <summary>
        /// Adds an inline picture to the current paragraph.
        /// </summary>
        public void AddInlinePicture( Image image, SizeF? size )
        {
            InlinePicture inlinePicture = InlinePicture.Create(this.DocumentWriter, image);
            inlinePicture.Size = size;
            this.AddInlinePicture( inlinePicture );
        }

        /// <summary>
        /// Adds an inline picture to the current paragraph.
        /// </summary>
        public abstract void AddInlinePicture( InlinePicture inlinePicture );

        #endregion AddInlinePicture

        #region AddAnchoredPicture
        
        /// <summary>
        /// Adds an anchored picture to the current paragraph.
        /// </summary>
        public void AddAnchoredPicture( Image image )
        {
            AnchoredPicture anchoredPicture = this.DocumentWriter.CreateAnchoredPicture( image );
            this.AddAnchoredPicture( anchoredPicture );
        }

        /// <summary>
        /// Adds an anchored picture with the specified size to the current paragraph.
        /// </summary>
        public abstract void AddAnchoredPicture( AnchoredPicture anchoredPicture );

        #endregion AddAnchoredPicture

        #region StartTable

        /// <summary>
        /// Starts a new table in the document with the specified number of columns.
        /// </summary>
        public void StartTable( int columnCount )
        {
            this.StartTable( columnCount, null );
        }

        /// <summary>
        /// Starts a new table in the document with the specified number of columns.
        /// </summary>
        public void StartTable( int columnCount, TableProperties properties )
        {
            WordprocessingMLWriter.VerifyColumnCount( columnCount );

            List<float> columnWidths = new List<float>( columnCount );
            for ( int i = 0; i < columnCount; i ++ )
            {
                float width = WordUtilities.DefaultTableColumnWidthInTwips;
                width = WordUtilities.ConvertFromTwips( this.Unit, width );
                columnWidths.Add( width );
            }

            this.StartTable( columnWidths, properties );
        }

        /// <summary>
        /// Starts a new table in the document with the number of columns
        /// equal to the number of elements in the specified <paramref name="columnWidths"/>
        /// list, and with the width of each column defined by the corresponding value in the list.
        /// </summary>
        public abstract void StartTable( IList<float> columnWidths, TableProperties properties );

        #endregion StartTable

        #region StartTableRow / EndTableRow

        /// <summary>
        /// Starts a new row in the current table with the default height.
        /// </summary>
        public void StartTableRow()
        {
            this.StartTableRow( null );
        }

        /// <summary>
        /// Starts a new row in the current table.
        /// </summary>
        public abstract void StartTableRow( TableRowProperties properties );

        /// <summary>
        /// Closes the current table row.
        /// </summary>
        public abstract void EndTableRow();

        #endregion StartTableRow / EndTableRow

        #region StartTableCell / EndTableCell / AddTableCell

        /// <summary>
        /// Starts a new cell in the current row.
        /// </summary>
        public abstract void StartTableCell( TableCellProperties properties );

        /// <summary>
        /// Closes a previously opened cell.
        /// </summary>
        public abstract void EndTableCell();

        /// <summary>
        /// Adds a cell to the current row, with one paragraph containing
        /// a simple text run with the specified <paramref name="text"/>.
        /// </summary>
        public void AddTableCell( string text )
        {
            this.AddTableCell( text, null );
        }

        /// <summary>
        /// Adds a cell to the current row, with one paragraph containing
        /// a simple text run with the specified <paramref name="text"/>,
        /// optionally with the specified properties.
        /// </summary>
        public abstract void AddTableCell( string text, TableCellProperties properties );

        #endregion StartTableCell / EndTableCell / AddTableCell

        #region EndTable

        /// <summary>
        /// Closes a previously opened table.
        /// </summary>
        public abstract void EndTable();

        #endregion EndTable

        #region Open
        /// <summary>
        /// Opens the writer.
        /// </summary>
        public abstract void Open();
        #endregion Open

        #region Close
        /// <summary>
        /// Closes and flushes the writer.
        /// </summary>
        public abstract void Close();
        #endregion Close

        #region AddPageNumberField
        /// <summary>
        /// Adds a page numbering field to the current paragraph.
        /// </summary>
        /// <param name="format">
        /// A
        /// <see cref="Infragistics.Documents.Word.PageNumberFieldFormat">PageNumberFieldFormat</see>
        /// value which defines the format for the page number.
        /// </param>
        public void AddPageNumberField( PageNumberFieldFormat format )
        {
            this.AddPageNumberField( format, null );
        }

        /// <summary>
        /// Adds a page numbering field to the current paragraph.
        /// </summary>
        /// <param name="format">
        /// A
        /// <see cref="Infragistics.Documents.Word.PageNumberFieldFormat">PageNumberFieldFormat</see>
        /// value which defines the format for the page number.
        /// </param>
        /// <param name="font">
        /// A
        /// <see cref="Infragistics.Documents.Word.Font">Font</see>
        /// instance which defines the formatting attributes for the page number.
        /// </param>
        /// <remarks>
        /// <p class="body">
        /// This method can only be called when a paragraph is currently open.
        /// A page number field is similar to a text run; it can be embedded
        /// within the paragraph adjacent to any other kind of content such
        /// as text, images, or hyperlinks.
        /// </p>
        /// <p class="body">
        /// A page number field can be added to either a header or footer.
        /// </p>
        /// <p class="body">
        /// The page number for the first page in a section can be explicitly set using the
        /// <see cref="Infragistics.Documents.Word.SectionProperties.StartingPageNumber">StartingPageNumber</see>
        /// property. When this property is not explicitly set, page numbers for new sections
        /// continue sequentially from the last page of the previous section.
        /// </p>
        /// </remarks>
        public void AddPageNumberField( PageNumberFieldFormat format, Font font )
        {
            PageNumberField pageNumberField = new PageNumberField( this.DocumentWriter, format );
            pageNumberField.Font = font;

            //  BF 4/12/11  TFS72391
            this.AddPageNumberField( pageNumberField );
        }
        
        /// <summary>
        /// Adds a page numbering field to the current paragraph.
        /// </summary>
        /// <param name="pageNumberField">
        /// A
        /// <see cref="Infragistics.Documents.Word.PageNumberField">PageNumberField</see>
        /// value which defines the properties of the page number.
        /// </param>
        /// <remarks>
        /// <p class="body">
        /// This method can only be called when a paragraph is currently open.
        /// A page number field is similar to a text run; it can be embedded
        /// within the paragraph adjacent to any other kind of content such
        /// as text, images, or hyperlinks.
        /// </p>
        /// <p class="body">
        /// A page number field can be added to either a header or footer.
        /// </p>
        /// <p class="body">
        /// The page number for the first page in a section can be explicitly set using the
        /// <see cref="Infragistics.Documents.Word.SectionProperties.StartingPageNumber">StartingPageNumber</see>
        /// property. When this property is not explicitly set, page numbers for new sections
        /// continue sequentially from the last page of the previous section.
        /// </p>
        /// </remarks>
        public abstract void AddPageNumberField( PageNumberField pageNumberField );
        
        #endregion AddPageNumberField

        #region AddAnchoredShape

        /// <summary>
        /// Adds an anchored shape with the specified size to the current paragraph.
        /// </summary>
        /// <param name="shapeType">
        /// A
        /// <see cref="Infragistics.Documents.Word.ShapeType">ShapeType</see>
        /// constant which defines the type of shape to add.
        /// </param>
        /// <seealso cref="Infragistics.Documents.Word.WordDocumentWriter.AddAnchoredShape(ShapeType)">AddAnchoredShape (WordDocumentWriter class)</seealso>
        public void AddAnchoredShape( ShapeType shapeType )
        {
            AnchoredShape shape = AnchoredShape.Create( this.DocumentWriter, shapeType );
            this.AddAnchoredShape( shape );
        }

        /// <summary>
        /// Adds an anchored shape with the specified size to the current paragraph.
        /// </summary>
        /// <param name="anchoredShape">
        /// The
        /// <see cref="Infragistics.Documents.Word.AnchoredShape">AnchoredShape</see>
        /// instance to be added to the paragraph.
        /// </param>
        /// <seealso cref="Infragistics.Documents.Word.WordDocumentWriter.AddAnchoredShape(AnchoredShape)">AddAnchoredShape (WordDocumentWriter class)</seealso>
        /// <exception cref="WordDocumentWriterException">Thrown if no document or paragraph is currently open.</exception>
        /// <exception cref="WordDocumentWriterXmlWriterException">Thrown if the XmlWriter used to write content to the stream throws an exception.</exception>
        public abstract void AddAnchoredShape( AnchoredShape anchoredShape );

        #endregion AddAnchoredShape

        #region AddInlineShape

        /// <summary>
        /// Adds an inline
        /// <see cref="Infragistics.Documents.Word.VmlShape">shape</see>
        /// to the current paragraph, using Vector Markup Language (VML).
        /// </summary>
        /// <param name="shapeType">
        /// A
        /// <see cref="Infragistics.Documents.Word.ShapeType">ShapeType</see>
        /// constant which defines the type of shape to add.
        /// </param>
        /// <remarks>
        /// <p class="body">
        /// Before the AddInlineShape method can be called, the developer must begin a paragraph via the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.StartParagraph()">StartParagraph</see>
        /// method, or an exception is thrown.
        /// </p>
        /// </remarks>
        /// <exception cref="WordDocumentWriterException">Thrown if no document or paragraph is currently open.</exception>
        /// <exception cref="WordDocumentWriterXmlWriterException">Thrown if the XmlWriter used to write content to the stream throws an exception.</exception>
        /// <seealso cref="Infragistics.Documents.Word.ShapeType">ShapeType enumeration</seealso>
        public void AddInlineShape( ShapeType shapeType )
        {
            this.AddInlineShape( shapeType, null );
        }

        /// <summary>
        /// Adds an inline
        /// <see cref="Infragistics.Documents.Word.VmlShape">shape</see>
        /// to the current paragraph, using Vector Markup Language (VML).
        /// </summary>
        /// <param name="shapeType">
        /// A
        /// <see cref="Infragistics.Documents.Word.ShapeType">ShapeType</see>
        /// constant which defines the type of shape to add.
        /// </param>
        /// <param name="size">If specified, the size at which the shape is rendered. If null, the shape is rendered at a size of one inch square..</param>
        /// <remarks>
        /// <p class="body">
        /// Before the AddInlineShape method can be called, the developer must begin a paragraph via the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.StartParagraph()">StartParagraph</see>
        /// method, or an exception is thrown.
        /// </p>
        /// </remarks>
        /// <exception cref="WordDocumentWriterException">Thrown if no document or paragraph is currently open.</exception>
        /// <exception cref="WordDocumentWriterXmlWriterException">Thrown if the XmlWriter used to write content to the stream throws an exception.</exception>
        /// <seealso cref="Infragistics.Documents.Word.ShapeType">ShapeType enumeration</seealso>
        public void AddInlineShape( ShapeType shapeType, Size? size )
        {
            VmlShape shape = VmlShape.Create( this.DocumentWriter, shapeType );
            shape.Size = size;
            this.AddInlineShape( shape );
        }

        /// <summary>
        /// Adds an inline
        /// <see cref="Infragistics.Documents.Word.VmlShape">shape</see>
        /// to the current paragraph, using Vector Markup Language (VML).
        /// </summary>
        /// <param name="shape">
        /// The shape to add, such as a line, rectangle, ellipse, etc.
        /// </param>
        /// <seealso cref="Infragistics.Documents.Word.WordDocumentWriter.AddInlineShape(InlineShape)">AddInlineShape (WordDocumentWriter class)</seealso>
        /// <exception cref="WordDocumentWriterException">Thrown if no document or paragraph is currently open.</exception>
        /// <exception cref="WordDocumentWriterXmlWriterException">Thrown if the XmlWriter used to write content to the stream throws an exception.</exception>
        public void AddInlineShape( VmlShape shape )
        {
            InlineShape inlineShape = InlineShape.Create( this.DocumentWriter, shape );
            this.AddInlineShape( inlineShape );
        }

        /// <summary>
        /// Adds an
        /// <see cref="Infragistics.Documents.Word.InlineShape">InlineShape</see>
        /// to the current paragraph, using Vector Markup Language (VML).
        /// </summary>
        /// <param name="inlineShape">
        /// The instance to add.
        /// </param>
        /// <seealso cref="Infragistics.Documents.Word.WordDocumentWriter.AddInlineShape(InlineShape)">AddInlineShape (WordDocumentWriter class)</seealso>
        /// <exception cref="WordDocumentWriterException">Thrown if no document or paragraph is currently open.</exception>
        /// <exception cref="WordDocumentWriterXmlWriterException">Thrown if the XmlWriter used to write content to the stream throws an exception.</exception>
        public abstract void AddInlineShape( InlineShape inlineShape );

        #endregion AddInlineShape

        #region Dispose
        internal virtual void Dispose()
        {
        }
        #endregion Dispose

        #endregion Methods

        #region IContentWriter interface implementation

        void IContentWriter.AddEmptyParagraph()
        {
            this.AddEmptyParagraph();
        }

        void IContentWriter.StartParagraph(ParagraphProperties properties)
        {
            this.StartParagraph( properties );
        }

        void IContentWriter.EndParagraph()
        {
            this.EndParagraph();
        }

        void IContentWriter.AddNewLine(NewLineType newLineType)
        {
            this.AddNewLine( newLineType );
        }

        void IContentWriter.AddTextRun(TextRun textRun)
        {
            this.AddTextRun( textRun );
        }

        void IContentWriter.AddTextRun(string text)
        {
            this.AddTextRun( text );
        }

        void IContentWriter.AddHyperlink(TextHyperlink hyperlink)
        {
            this.AddHyperlink( hyperlink );
        }


        void IContentWriter.AddInlinePicture(InlinePicture inlinePicture)
        {
            this.AddInlinePicture( inlinePicture );
        }

        void IContentWriter.AddAnchoredPicture(AnchoredPicture anchoredPicture)
        {
           this.AddAnchoredPicture( anchoredPicture );
        }

        void IContentWriter.StartTable(IList<float> columnWidths, TableProperties properties)
        {
            this.StartTable( columnWidths, properties );
        }

        void IContentWriter.StartTableRow(TableRowProperties properties)
        {
            this.StartTableRow( properties );
        }

        void IContentWriter.EndTableRow()
        {
            this.EndTableRow();
        }

        void IContentWriter.StartTableCell(TableCellProperties properties)
        {
            this.StartTableCell( properties );
        }

        void IContentWriter.EndTableCell()
        {
            this.EndTableCell();
        }

        void IContentWriter.AddTableCell(string text, TableCellProperties properties)
        {
            this.AddTableCell( text, properties );
        }

        void IContentWriter.EndTable()
        {
            this.EndTable();
        }

        void IContentWriter.AddAnchoredShape(AnchoredShape anchoredShape)
        {
            this.AddAnchoredShape( anchoredShape );
        }

        void IContentWriter.AddInlineShape(InlineShape inlineShape)
        {
            this.AddInlineShape( inlineShape );
        }

        void IContentWriter.AddPageNumberField(PageNumberField pageNumberField)
        {
            this.AddPageNumberField( pageNumberField );
        }

        void IContentWriter.Start()
        {
            this.Open();
        }

        void IContentWriter.Close()
        {
            this.Close();
        }

        UnitOfMeasurement IContentWriter.Unit { get { return this.Unit; } }

        WordDocumentWriter IContentWriter.DocumentWriter { get { return this.DocumentWriter; } }

        #endregion IContentWriter interface implementation
    }
    #endregion WordHeaderFooterWriter class

    #region WordprocessingMLHeaderFooterWriter class
    /// <summary>
    /// Writes WordprocessingML content to a header or footer
    /// within a section of the document.
    /// </summary>
    /// <remarks>
    /// <p class="body">
    /// The methods exposed by this class are syntactically identical to the
    /// corresponding methods of the
    /// <see cref="Infragistics.Documents.Word.WordDocumentWriter">WordDocumentWriter</see>
    /// class. This is because headers and footers can accommodate virtually all the same
    /// content that the main document part can accommodate; for example, paragraphs, text runs,
    /// hyperlinks, and tables are all valid content for a header or footer section.
    /// </p>
    /// <p class="body">
    /// At the time that the containing
    /// <see cref="Infragistics.Documents.Word.SectionHeaderFooterWriterSet">SectionHeaderFooterWriterSet</see>
    /// is returned from the
    /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.AddSectionHeaderFooter(SectionHeaderFooterParts)">AddSectionHeaderFooter</see>
    /// method, the XML elements required to reference the header and footer parts have already been written
    /// to the content stream - this is a necessary consequence of a forward-only stream writer.
    /// These elements refer to the actual header/footer parts in the package; the content for these parts, however,
    /// is created via calls to the methods of this class.
    /// </p>
    /// <p class="body">
    /// This instance stays alive until it is explicitly discarded by the consumer,
    /// or until the associated
    /// <see cref="Infragistics.Documents.Word.WordDocumentWriter">WordDocumentWriter</see>
    /// is closed. This means  that the developer can write content to the header/footer part
    /// at any time during the lifetime of the WordDocumentWriter, although doing so is not
    /// recommended as this increases the complexity of the resulting implementation.
    /// </p>
    /// <p class="body">
    /// Unlike the WordDocumentWriter, which requires a 'StartDocument' method to be called
    /// prior to calling content creation methods, all required prologues are automatically
    /// written to the content stream when the
    /// <see cref="Infragistics.Documents.Word.WordHeaderFooterWriter.StartParagraph()">StartParagraph</see>
    /// or
    /// <see cref="Infragistics.Documents.Word.WordHeaderFooterWriter.StartTable(int)">StartTable</see>
    /// methods are called. Note that all content which appears within a header or footer must appear
    /// within one of these two types of content blocks.
    /// </p>
    /// <p class="body">
    /// Page numbers can be specified for a header or footer using the
    /// <see cref="Infragistics.Documents.Word.WordHeaderFooterWriter.AddPageNumberField(PageNumberFieldFormat)">AddPageNumberField</see>
    /// method.
    /// </p>
    /// <p class="body">
    /// The
    /// <see cref="Infragistics.Documents.Word.WordHeaderFooterWriter.Close">Close</see>
    /// method must be called after all content has been written.
    /// </p>
    /// </remarks>

    [InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_IGWord)]

    public class WordprocessingMLHeaderFooterWriter : WordHeaderFooterWriter
    {
        #region Member variables
        private WordHeaderFooterPartExporterBase    partExporter = null;
        private WriterStateVerification             stateVerification = null;
        #endregion Member variables

        #region Constructor
        internal WordprocessingMLHeaderFooterWriter(
            WordDocumentWriter writer,
            WordHeaderFooterPartExporterBase partExporter,
            HeaderOrFooter headerOrFooter,
            HeaderFooterType type ) :
            base( writer, headerOrFooter, type )
        {
            this.partExporter = partExporter;
        }
        #endregion Constructor

        #region Properties

        #region PartExporter
        private WordHeaderFooterPartExporterBase PartExporter
        {
            get { return this.partExporter; }
        }
        #endregion PartExporter

        #region StateVerification
        internal override WriterStateVerification StateVerification
        {
            get
            {
                if ( this.stateVerification == null )
                    this.stateVerification = new WriterStateVerification( false );

                return this.stateVerification;
            }
        }
        #endregion StateVerification

        #endregion Properties

        #region Methods

        #region StartParagraph

        /// <summary>
        /// See
        /// <see cref="Infragistics.Documents.Word.WordprocessingMLWriter.StartParagraph(ParagraphProperties)">WordProcessingMLWriter.StartParagraph</see>.
        /// </summary>
        public override void StartParagraph( ParagraphProperties properties )
        {
            this.Open();

            try
            {
                this.VerifyDocumentWriterOpen();
                WordprocessingMLWriter.StartParagraph( properties, this.StateVerification, this.PartExporter );
            }
            catch( Exception exception )
            {
                this.HandleException( exception );
            }
        }

        #endregion StartParagraph

        #region EndParagraph

        /// <summary>
        /// See
        /// <see cref="Infragistics.Documents.Word.WordprocessingMLWriter.EndParagraph()">WordProcessingMLWriter.EndParagraph</see>.
        /// </summary>
        public override void EndParagraph()
        {
            try
            {
                this.VerifyDocumentWriterOpen();
                WordprocessingMLWriter.EndParagraph( this.StateVerification, this.PartExporter );
            }
            catch( Exception exception )
            {
                this.HandleException( exception );
            }
        }

        #endregion EndParagraph

        #region AddTextRun

        /// <summary>
        /// See
        /// <see cref="Infragistics.Documents.Word.WordprocessingMLWriter.AddTextRun(TextRun)">WordProcessingMLWriter.AddTextRun</see>.
        /// </summary>
        public override void AddTextRun( TextRun textRun )
        {
            try
            {
                this.VerifyDocumentWriterOpen();
                WordprocessingMLWriter.AddTextRun( textRun, this.DocumentWriter.NewLineType, this.StateVerification, this.PartExporter );
            }
            catch( Exception exception )
            {
                this.HandleException( exception );
            }
        }

        #endregion AddTextRun

        #region AddHyperlink

        /// <summary>
        /// See
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.AddHyperlink(TextHyperlink)">WordProcessingMLWriter.AddHyperlink</see>.
        /// </summary>
        public override void AddHyperlink( TextHyperlink hyperlink )
        {
            if ( hyperlink == null )
                throw new ArgumentNullException( "hyperlink" );

            try
            {
                this.VerifyDocumentWriterOpen();

                WordprocessingMLWriter.AddHyperlink(
                    hyperlink.Address,
                    hyperlink.GetTextRuns(),
                    hyperlink.ToolTipText,
                    false,
                    this.DocumentWriter.NewLineType,
                    this.StateVerification,
                    this.PartExporter );
            }
            catch( Exception exception )
            {
                this.HandleException( exception );
            }
        }

        #endregion AddHyperlink

        #region AddInlinePicture
    
        /// <summary>
        /// See
        /// <see cref="Infragistics.Documents.Word.WordprocessingMLWriter.AddInlinePicture(InlinePicture)">WordProcessingMLWriter.AddInlinePicture</see>.
        /// </summary>
        public override void AddInlinePicture( InlinePicture inlinePicture )
        {
            try
            {
                this.VerifyDocumentWriterOpen();

                WordprocessingMLWriter.AddInlinePicture(



                    inlinePicture.Image,
                    inlinePicture.Size,
                    null,
                    inlinePicture.HasHyperlink ? inlinePicture.Hyperlink : null,
                    inlinePicture.AlternateTextDescription,
                    this.StateVerification, 
                    this.PartExporter,
                    this.Unit );
            }
            catch( Exception exception )
            {
                this.HandleException( exception );
            }
        }

        #endregion AddInlinePicture

        #region AddAnchoredPicture

        
        /// <summary>
        /// See
        /// <see cref="Infragistics.Documents.Word.WordprocessingMLWriter.AddAnchoredPicture(AnchoredPicture)">WordProcessingMLWriter.AddAnchoredPicture</see>.
        /// </summary>
        public override void AddAnchoredPicture( AnchoredPicture anchoredPicture )
        {
            try
            {
                this.VerifyDocumentWriterOpen();
                WordprocessingMLWriter.AddAnchoredPicture(



                    anchoredPicture,
                    this.StateVerification,
                    this.PartExporter );
            }
            catch( Exception exception )
            {
                this.HandleException( exception );
            }
        }

        #endregion AddAnchoredPicture

        #region StartTable

        /// <summary>
        /// See
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.StartTable(int)">WordProcessingMLWriter.StartTable</see>.
        /// </summary>
        public override void StartTable( IList<float> columnWidths, TableProperties properties )
        {
            this.Open();

            try
            {
                this.VerifyDocumentWriterOpen();
                WordprocessingMLWriter.StartTable( columnWidths, properties, this.StateVerification, this.PartExporter, this.Unit );
            }
            catch( Exception exception )
            {
                this.HandleException( exception );
            }
        }

        #endregion StartTable

        #region StartTableRow / EndTableRow

        /// <summary>
        /// See
        /// <see cref="Infragistics.Documents.Word.WordprocessingMLWriter.StartTableRow(TableRowProperties)">WordProcessingMLWriter.StartTableRow</see>.
        /// </summary>
        public override void StartTableRow( TableRowProperties properties )
        {
            try
            {
                this.VerifyDocumentWriterOpen();
                WordprocessingMLWriter.StartTableRow( properties, this.StateVerification, this.PartExporter );
            }
            catch( Exception exception )
            {
                this.HandleException( exception );
            }
        }

        /// <summary>
        /// See
        /// <see cref="Infragistics.Documents.Word.WordprocessingMLWriter.EndTableRow()">WordProcessingMLWriter.EndTableRow</see>.
        /// </summary>
        public override void EndTableRow()
        {
            WordprocessingMLWriter.EndTableRow( this.StateVerification, this.PartExporter );
        }

        #endregion StartTableRow / EndTableRow

        #region StartTableCell / EndTableCell / AddTableCell

        /// <summary>
        /// See
        /// <see cref="Infragistics.Documents.Word.WordprocessingMLWriter.StartTableCell(TableCellProperties)">WordProcessingMLWriter.StartTableCell</see>.
        /// </summary>
        public override void StartTableCell( TableCellProperties properties )
        {
            try
            {
                this.VerifyDocumentWriterOpen();
                WordprocessingMLWriter.StartTableCell( properties, this.StateVerification, this.PartExporter );
            }
            catch( Exception exception )
            {
                this.HandleException( exception );
            }
        }

        /// <summary>
        /// See
        /// <see cref="Infragistics.Documents.Word.WordprocessingMLWriter.EndTableCell()">WordProcessingMLWriter.EndTableCell</see>.
        /// </summary>
        public override void EndTableCell()
        {
            WordprocessingMLWriter.EndTableCell(
                this.StateVerification,
                this.PartExporter,
                new WordprocessingMLWriter.NoParagraphWrittenToCellDelegate(this.NoParagraphWrittenToTableCell) );
        }

        private void NoParagraphWrittenToTableCell()
        {
            this.AddEmptyParagraph();
        }

        /// <summary>
        /// See
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.AddTableCell(string)">WordProcessingMLWriter.AddTableCell</see>.
        /// </summary>
        public override void AddTableCell( string text, TableCellProperties properties )
        {
            WordprocessingMLWriter.AddTableCell( text, properties, this.DocumentWriter.NewLineType, this.StateVerification, this.PartExporter );
        }

        #endregion StartTableCell / EndTableCell / AddTableCell

        #region EndTable

        /// <summary>
        /// See
        /// <see cref="Infragistics.Documents.Word.WordprocessingMLWriter.EndTable()">WordProcessingMLWriter.EndTable</see>.
        /// </summary>
        public override void EndTable()
        {
            try
            {
                this.VerifyDocumentWriterOpen();
                WordprocessingMLWriter.EndTable( this.StateVerification, this.PartExporter );
            }
            catch( Exception exception )
            {
                this.HandleException( exception );
            }
        }

        #endregion EndTable

        #region Open
        /// <summary>
        /// Opens the writer.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// This method writes the root element to the package part associated
        /// with the header/footer, but writes no content.
        /// </p>
        /// </remarks>
        public override void Open()
        {
            this.PartExporter.Open();
            this.StateVerification.CurrentState.DocumentOpened = true;
        }
        #endregion Open

        #region Close
        /// <summary>
        /// Closes and flushes the writer.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// The Close method closes the underlying XML writer, which finalizes
        /// the content and commits it to the associated package part.
        /// </p>
        /// </remarks>
        public override void Close()
        {
            try
            {
                this.StateVerification.VerifyDocumentClosure(WordDocumentWriterExceptionReason.HeaderFooterWriterClosureError);
                this.PartExporter.Close();
            }
            finally
            {
                //  We use this flag to determine whether we should throw
                //  an exception when the document writer is closed, so
                //  set the flag now that the close method has been called.
                this.StateVerification.CurrentState.DocumentClosed = true;
            }
        }
        #endregion Close

        #region HandleException
        private void HandleException( Exception innerException )
        {
            if ( innerException == null )
                return;

            WordDocumentWriterExceptionReason reason =
                this.IsHeader ?
                WordDocumentWriterExceptionReason.HeaderWriterError : 
                WordDocumentWriterExceptionReason.FooterWriterError;

            WordDocumentWriterException exception = new WordDocumentWriterException( reason, innerException );

            throw exception;
        }
        #endregion HandleException

        #region VerifyDocumentWriterOpen
        private void VerifyDocumentWriterOpen()
        {
            if ( (this.DocumentWriter.WriterState & WordDocumentWriterState.DocumentOpen) != WordDocumentWriterState.DocumentOpen )
                throw new WordDocumentWriterException( WordDocumentWriterExceptionReason.DocumentNotOpen );
        }
        #endregion VerifyDocumentWriterOpen

        #region AddPageNumberField
        /// <summary>
        /// Adds a page numbering field to the current paragraph.
        /// </summary>
        /// <param name="pageNumberField">
        /// A
        /// <see cref="Infragistics.Documents.Word.PageNumberField">PageNumberField</see>
        /// value which defines the properties of the page number.
        /// </param>
        /// <remarks>
        /// <p class="body">
        /// This method can only be called when a paragraph is currently open.
        /// A page number field is similar to a text run; it can be embedded
        /// within the paragraph adjacent to any other kind of content such
        /// as text, images, or hyperlinks.
        /// </p>
        /// <p class="body">
        /// A page number field can be added to either a header or footer.
        /// </p>
        /// <p class="body">
        /// The page number for the first page in a section can be explicitly set using the
        /// <see cref="Infragistics.Documents.Word.SectionProperties.StartingPageNumber">StartingPageNumber</see>
        /// property. When this property is not explicitly set, page numbers for new sections
        /// continue sequentially from the last page of the previous section.
        /// </p>
        /// </remarks>
        public override void AddPageNumberField( PageNumberField pageNumberField )
        {
            if ( pageNumberField == null )
                throw new ArgumentNullException( "pageNumberField" );

            try
            {
                this.VerifyDocumentWriterOpen();

                WordprocessingMLWriter.AddPageNumberField(
                    this.StateVerification,
                    this.PartExporter,
                    pageNumberField.Format,
                
                    //  BF 4/12/11  TFS72391
                    //pageNumberField.Font );
                    pageNumberField.HasFont ? pageNumberField.Font : null );
            }
            catch( Exception exception )
            {
                this.HandleException( exception );
            }
        }
        #endregion AddPageNumberField

        #region AddAnchoredShape

        /// <summary>
        /// Adds an anchored shape with the specified size to the current paragraph.
        /// </summary>
        /// <param name="anchoredShape">
        /// The
        /// <see cref="Infragistics.Documents.Word.AnchoredShape">AnchoredShape</see>
        /// instance to be added to the paragraph.
        /// </param>
        /// <seealso cref="Infragistics.Documents.Word.WordDocumentWriter.AddAnchoredShape(AnchoredShape)">AddAnchoredShape (WordDocumentWriter class)</seealso>
        /// <exception cref="WordDocumentWriterException">Thrown if no document or paragraph is currently open.</exception>
        /// <exception cref="WordDocumentWriterXmlWriterException">Thrown if the XmlWriter used to write content to the stream throws an exception.</exception>
        public override void AddAnchoredShape( AnchoredShape anchoredShape )
        {
            WordprocessingMLWriter.AddAnchoredShape( anchoredShape, this.StateVerification, this.PartExporter );
        }

        #endregion AddAnchoredShape

        #region AddInlineShape

        /// <summary>
        /// Adds an
        /// <see cref="Infragistics.Documents.Word.InlineShape">InlineShape</see>
        /// to the current paragraph, using Vector Markup Language (VML).
        /// </summary>
        /// <param name="inlineShape">
        /// The instance to add.
        /// </param>
        /// <seealso cref="Infragistics.Documents.Word.WordDocumentWriter.AddInlineShape(InlineShape)">AddInlineShape (WordDocumentWriter class)</seealso>
        /// <exception cref="WordDocumentWriterException">Thrown if no document or paragraph is currently open.</exception>
        /// <exception cref="WordDocumentWriterXmlWriterException">Thrown if the XmlWriter used to write content to the stream throws an exception.</exception>
        public override void AddInlineShape( InlineShape inlineShape )
        {
            WordprocessingMLWriter.AddInlineShape( inlineShape, this.StateVerification, this.PartExporter );
        }

        #endregion AddInlineShape

        #region Dispose
        internal override void Dispose()
        {
            base.Dispose();

            if ( this.partExporter != null )
            {
                this.partExporter.Dispose();
                this.partExporter = null;
            }
        }
        #endregion Dispose

        #endregion Methods

    }
    #endregion WordprocessingMLHeaderFooterWriter class

    #region Hyperlink class
    /// <summary>
    /// Encapsulates the address and tooltip text for a hyperlink.
    /// </summary>
    public class Hyperlink : ParagraphContent
    {
        #region Member variables
        private string address = null;
        private string toolTipText = null;
        #endregion Member variables

        #region Constructor

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Infragistics.Documents.Word.Hyperlink">Hyperlink</see>
        /// class.
        /// </summary>
        /// <param name="address">The address of the hyperlink.</param>
        public Hyperlink( string address ) : this( address, false )
        {
        }

        internal Hyperlink( string address, bool allowNullAddress ) : this( null, address, allowNullAddress )
        {
        }

        internal Hyperlink( IUnitOfMeasurementProvider unitOfMeasurementProvider, string address, bool allowNullAddress ) : base( unitOfMeasurementProvider )
        {
            Hyperlink.VerifyAddress( ref address, allowNullAddress );
            this.address = address;
        }



#region Infragistics Source Cleanup (Region)














#endregion // Infragistics Source Cleanup (Region)

        static internal Hyperlink Create( WordDocumentWriter writer, string address )
        {
            return new Hyperlink( writer, address, false );
        }

        #endregion Constructor

        #region Properties

        #region Address
        /// <summary>
        /// Returns or sets the address of the hyperlink.
        /// </summary>
        public string Address
        {
            get { return WordUtilities.StringPropertyGetHelper(this.address); }
            set
            { 
                Hyperlink.VerifyAddress( ref value, true );

                this.address = value;
            }
        }
        #endregion Address

        #region ToolTipText
        /// <summary>
        /// Returns or sets the text of the tooltip that is displayed for the hyperlink.
        /// </summary>
        public string ToolTipText
        {
            get { return WordUtilities.StringPropertyGetHelper(this.toolTipText); }
            set { this.toolTipText = value; }
        }
        #endregion ToolTipText

        #endregion Properties

        #region Methods

        #region GetTextRuns
        
        internal virtual IList<TextRun> GetTextRuns(){ return null; }
        
        #endregion GetTextRuns

        #region Reset
        /// <summary>
        /// Restores all property values to their respective defaults.
        /// </summary>
        /// <param name="address">
        /// The new value for the
        /// <see cref="Infragistics.Documents.Word.Hyperlink.Address">Address</see>
        /// property.
        /// </param>
        public virtual void Reset( string address )
        {
            this.Reset( address, false );
        }

        internal void Reset( string address, bool allowNullAddress )
        {
            if ( allowNullAddress == false )
                Hyperlink.VerifyAddress( ref address, true );

            this.address = address;
            this.toolTipText = null;
        }
        #endregion Reset

        #region VerifyAddress
        static internal void VerifyAddress( ref string address, bool allowNullAddress )
        {
            if ( allowNullAddress == false && string.IsNullOrEmpty(address) )
                throw new ArgumentException( SR.GetString("Exception_MissingHyperlinkAddress") );

            if ( string.IsNullOrEmpty(address) )
                return;

            //  TFS64336
            //  As a convenience we will prepend "http://" when the address begins with "www."
            if ( address.StartsWith("www.", StringComparison.InvariantCultureIgnoreCase) )
                address = address.Replace("www.", "http://www.");

            //  And if the address yields a bad URI, throw an exception
            try
            {
                Uri uri = new Uri(address);
            }
            catch( UriFormatException innerException )
            {
                Exception ex = new Exception(
                    SR.GetString("Exception_AddHyperlink_BadUri", address),
                    innerException );

                throw ex;
            }
        }
        #endregion VerifyAddress

        #region ToString
        /// <summary>
        /// Returns the string representation of this object.
        /// </summary>
        public override string ToString()
        {
            return
                String.IsNullOrEmpty(this.address) == false ?
                this.address :
                base.ToString();
        }
        #endregion ToString
        
        #endregion Methods
    }
    #endregion Hyperlink class

    #region TextHyperlink class
    /// <summary>
    /// Encapsulates a textual hyperlink.
    /// </summary>
    public class TextHyperlink : Hyperlink
    {
        #region Member variables
        private IList<TextRun> textRuns = null;
        private string text = null;
        #endregion Member variables

        #region Constructor

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Infragistics.Documents.Word.TextHyperlink">TextHyperlink</see>
        /// class.
        /// </summary>
        /// <param name="address">The address of the hyperlink.</param>
        public TextHyperlink( string address ) : this( address, null )
        {
        }

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Infragistics.Documents.Word.TextHyperlink">TextHyperlink</see>
        /// class.
        /// </summary>
        /// <param name="address">The address of the hyperlink.</param>
        /// <param name="text">The text displayed for the hyperlink.</param>
        public TextHyperlink( string address, string text ) : this ( null, address, text )
        {
        }

        internal TextHyperlink( IUnitOfMeasurementProvider unitOfMeasurementProvider, string address, string text ) : base ( unitOfMeasurementProvider, address, false )
        {
            this.text = text;
        }

        /// <summary>
        /// Returns a new instance which is associated with the specified
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter">WordDocumentWriter</see>.
        /// </summary>
        /// <param name="writer">
        /// The associated
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter">WordDocumentWriter</see>
        /// instance.
        /// </param>
        /// <param name="address">
        /// The address of the hyperlink.
        /// </param>
        /// <param name="text">
        /// The text displayed for the hyperlink.
        /// </param>
        /// <returns>
        /// A new
        /// <see cref="Infragistics.Documents.Word.PictureOutlineProperties">PictureOutlineProperties</see>
        /// instance.
        /// </returns>
        static public TextHyperlink Create( WordDocumentWriter writer, string address, string text )
        {
            return new TextHyperlink( writer, address, text );
        }

        #endregion Constructor

        #region Properties

        #region Text
        /// <summary>
        /// Returns or sets the text that is displayed for the hyperlink.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// In the case where the 
        /// <see cref="Infragistics.Documents.Word.TextHyperlink.TextRuns">TextRuns</see>
        /// property is set to a non-null value, this property returns the text of the
        /// first element in the list.
        /// </p>
        /// <p class="body">
        /// Use the
        /// <see cref="Infragistics.Documents.Word.TextHyperlink.TextRuns">TextRuns</see>
        /// property when the text for the hyperlink must contain mixed formatting attributes.
        /// </p>
        /// </remarks>
        /// <seealso cref="Infragistics.Documents.Word.TextHyperlink.TextRuns">TextRuns</seealso>
        public string Text
        {
            get
            {
                if ( this.textRuns != null && this.textRuns.Count > 0 &&
                     string.IsNullOrEmpty(this.textRuns[0].Text) == false )
                    return this.textRuns[0].Text;

                if ( string.IsNullOrEmpty(this.text) == false )
                    return this.text;

                return this.Address;
            }
            set
            {
                this.textRuns = null;
                this.text = value;
            }
        }
        #endregion Text

        #region TextRuns
        /// <summary>
        /// Returns or sets a list of
        /// <see cref="Infragistics.Documents.Word.TextRun">TextRun</see>
        /// instances which contain the runs of text that are displayed for this hyperlink.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// The TextRuns property can be used to create a hyperlink with mixed
        /// formatting attributes. Each element in the list contains a fragment
        /// of the text to be displayed along with the font which defines the
        /// formatting attributes for that fragment.
        /// </p>
        /// <p class="body">
        /// The
        /// <see cref="Infragistics.Documents.Word.TextHyperlink.Text">Text</see>
        /// property exists for the developer's convenience since in the majority
        /// of cases a hyperlink does not contain mixed formatting attributes.
        /// </p>
        /// </remarks>
        /// <seealso cref="Infragistics.Documents.Word.TextHyperlink.Text">Text</seealso>
        public IList<TextRun> TextRuns
        {
            get
            {
                if ( this.textRuns == null )
                    this.textRuns = new List<TextRun>();

                return this.textRuns;
            }

            set { this.textRuns = value; }
        }
        #endregion TextRuns

        #endregion Properties

        #region Methods

        #region GetTextRuns

        internal override IList<TextRun> GetTextRuns()
        {
            if ( this.textRuns != null && this.textRuns.Count > 0 )
                return this.textRuns;

            string text = string.IsNullOrEmpty(this.text) == false ? this.text : this.Address;
            TextRun tr = new TextRun( null, text, null );
            List<TextRun> textRuns = new List<TextRun>(1);
            textRuns.Add( tr );
            return textRuns;
        }
        
        #endregion GetTextRuns

        #region Reset
        /// <summary>
        /// Restores all property values to their respective defaults.
        /// </summary>
        /// <param name="address">
        /// The new value for the
        /// <see cref="Infragistics.Documents.Word.Hyperlink.Address">Address</see>
        /// property.
        /// </param>
        public override void Reset( string address )
        {
            base.Reset( address );
            this.textRuns = null;
        }
        #endregion Reset

        #region ToString
        /// <summary>
        /// Returns the string representation of this object.
        /// </summary>
        public override string ToString()
        {
            return
                this.textRuns != null && this.textRuns.Count > 0 && string.IsNullOrEmpty(this.textRuns[0].Text) == false ?
                this.textRuns[0].ToString() :
                string.IsNullOrEmpty(this.Address) == false ?
                this.Address :
                base.ToString();
        }
        #endregion ToString

        #endregion Methods
    }
    #endregion TextHyperlink class

    #region ParagraphContent
    /// <summary>
    /// Base class from which classes that encapsulate paragraph content derive.
    /// </summary>
    /// <seealso cref="Infragistics.Documents.Word.TextRun">TextRun class</seealso>
    /// <seealso cref="Infragistics.Documents.Word.Hyperlink">Hyperlink class</seealso>
    /// <seealso cref="Infragistics.Documents.Word.TextHyperlink">TextHyperlink class</seealso>
    public abstract class ParagraphContent
    {
        #region Member variables
        internal IUnitOfMeasurementProvider     unitOfMeasurementProvider = null;
        #endregion Member variables

        #region Constructor
        internal ParagraphContent( IUnitOfMeasurementProvider unitOfMeasurementProvider )
        {
            this.unitOfMeasurementProvider = unitOfMeasurementProvider;
        }
        #endregion Constructor

        #region UnitOfMeasurementProvider
        internal IUnitOfMeasurementProvider UnitOfMeasurementProvider { get { return this.unitOfMeasurementProvider; } }
        #endregion UnitOfMeasurementProvider

        #region Unit
        internal UnitOfMeasurement Unit { get { return this.unitOfMeasurementProvider != null ? this.unitOfMeasurementProvider.Unit : WordUtilities.DefaultUnitOfMeasurement; } }
        #endregion Unit

        #region InitUnitOfMeasurementProvider
        internal void InitUnitOfMeasurementProvider( IUnitOfMeasurementProvider value )
        {
            this.unitOfMeasurementProvider = value;
        }
        #endregion InitUnitOfMeasurementProvider
}
    #endregion ParagraphContent

    #region SizeableContent
    /// <summary>
    /// Base class for inline shapes and pictures
    /// </summary>
    public abstract class SizeableContent : ParagraphContent
    {
        #region Member variables
        private Hyperlink                       hyperlink = null;
        private SizeF?                          size = null;
        private string                          alternateTextDescription = null;
        #endregion Member variables

        #region Constructor
        internal SizeableContent( IUnitOfMeasurementProvider unitOfMeasurementProvider, SizeF? size ) : base( unitOfMeasurementProvider )
        {
            this.size = size;
        }
        #endregion Constructor

        #region AlternateTextDescription
        /// <summary>
        /// Returns or sets the description of the alternative text for the picture or shape,
        /// for use by assistive technologies or applications which will not display the
        /// associated picture or shape.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// WordprocessingML consumers which do not support the display of pictures or shapes
        /// may make use this property to describe the picture to the user. Screen
        /// reading applications or other assistive technologies may also make use
        /// of the property to describe the picture or shape to handicapped users.
        /// </p>
        /// </remarks>
        public string AlternateTextDescription
        {
            get { return WordUtilities.StringPropertyGetHelper( this.alternateTextDescription ); }
            set { this.alternateTextDescription = value; }
        }

        internal bool HasAlternateTextDescription { get { return string.IsNullOrEmpty(this.alternateTextDescription) == false; } }
        
        #endregion AlternateTextDescription

        #region Hyperlink
        /// <summary>
        /// Returns a
        /// <see cref="Infragistics.Documents.Word.Hyperlink">Hyperlink</see>
        /// instance which provides a way to add a hyperlink to the shape or picture.
        /// </summary>
        public Hyperlink Hyperlink
        {
            get
            {
                if ( this.hyperlink == null )
                    this.hyperlink = new Hyperlink(null, true);

                return this.hyperlink;
            }
        }

        internal bool HasHyperlink { get { return this.hyperlink != null && string.IsNullOrEmpty(this.hyperlink.Address) == false; } }

        #endregion Hyperlink

        #region Size
        /// <summary>
        /// Returns or sets the size at which the shape or picture is rendered.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// In the absence of an explicit setting, this property returns null.
        /// In this case, shapes are displayed at a size of one inch square,
        /// and pictures are displayed at their natural size.
        /// </p>
        /// <p class="body">
        /// When set explicitly, the unit of measurement used to express the property value is defined by the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.Unit">Unit</see>
        /// property.
        /// </p>
        /// </remarks>
        public SizeF? Size
        {
            get
            {
                return Anchor.GetSize( this.SizeRawValue, this.Unit );
            }

            set
            { 
                Anchor.SetSize( value, ref this.size, this.Unit, true );
            }
        }

        internal virtual SizeF?  SizeRawValue
        {
	        get { return this.size; }
        }

        internal abstract SizeF DefaultSizeInTwips { get; }

        /// <summary>
        /// Returns the resolved size in TWIPs
        /// </summary>
        internal Size SizeInTwips
        {
            get
            {
                SizeF size = this.SizeRawValue.HasValue ? this.SizeRawValue.Value : this.DefaultSizeInTwips;
                return WordUtilities.ToSize( size );
            }
        }

        #endregion Size

        #region Reset
        /// <summary>
        /// Restores all property values of this instance to their respective defaults.
        /// </summary>
        public void Reset()
        {
            this.size = null;

            //  Reset the hyperlink rather than nullify so it
            //  doesn't have to be reinstantiated if it is reused.
            if ( this.hyperlink != null )
                this.hyperlink.Reset( null, true );
        }
        #endregion Reset
    }
    #endregion SizeableContent

    #region PageNumberField class
    /// <summary>
    /// Encapsulates a page number field within a paragraph.
    /// </summary>
    public class PageNumberField : ParagraphContent
    {
        #region Member variables
        private PageNumberFieldFormat       format = WordUtilities.DefaultPageNumberFieldFormat;
        private Font                        font = null;
        #endregion Member variables

        #region Constructor
        internal PageNumberField( IUnitOfMeasurementProvider unitOfMeasurementProvider, PageNumberFieldFormat format ) : base( unitOfMeasurementProvider )
        {
            this.format = format;
        }

        /// <summary>
        /// Creates a new instance of the class.
        /// </summary>
        /// <param name="format">
        /// A
        /// <see cref="Infragistics.Documents.Word.PageNumberFieldFormat">PageNumberFieldFormat</see>.
        /// constant which defines the format for the page number field.
        /// </param>
        /// <param name="writer">
        /// The associated
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter">WordDocumentWriter</see>.
        /// </param>
        static public PageNumberField Create( WordDocumentWriter writer, PageNumberFieldFormat format )
        {
            return new PageNumberField( writer, format );
        }
        #endregion Constructor

        #region Properties

        #region Format
        /// <summary>
        /// Returns or sets a value which determines the type
        /// of page number field.
        /// </summary>
        public PageNumberFieldFormat Format
        {
            get { return this.format; }
            set { this.format = value; }
        }
        #endregion Format

        #region Font
        /// <summary>
        /// Returns or sets the font which defines the formatting for this field.
        /// </summary>
        public WORD.Font Font
        { 
            get
            { 
                if ( this.font == null )
                    this.font = new WORD.Font( this.unitOfMeasurementProvider );

                return this.font;
            }

            set { this.font = value; }
        }

        internal bool HasFont { get { return this.font != null && this.font.ShouldSerialize(); } }

        #endregion Font

        #endregion Properties

        #region Methods

        #region Reset
        /// <summary>
        /// Restores all property values to their respective defaults.
        /// </summary>
        public void Reset( PageNumberFieldFormat format )
        {
            this.format = format;

            if ( this.font != null )
                this.font.Reset();
        }
        #endregion Reset

        #endregion Methods
    }
    #endregion Field class

    #region IContentWriter


#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)

    internal interface IContentWriter
    {
        void AddEmptyParagraph();
        void StartParagraph( ParagraphProperties properties );
        void EndParagraph();
        void AddNewLine( NewLineType newLineType );
        void AddTextRun( TextRun textRun );
        void AddTextRun( string text );
        void AddHyperlink( TextHyperlink hyperlink );

        void AddInlinePicture( InlinePicture inlinePicture );
        void AddAnchoredPicture( AnchoredPicture anchoredPicture );

        void StartTable( IList<float> columnWidths, TableProperties properties );
        void StartTableRow( TableRowProperties properties );
        void EndTableRow();
        void StartTableCell( TableCellProperties properties );
        void EndTableCell();
        void AddTableCell( string text, TableCellProperties properties );
        void EndTable();
        void AddAnchoredShape( AnchoredShape anchoredShape );
        void AddInlineShape( InlineShape inlineShape );
        void AddPageNumberField( PageNumberField field );
        void Start();
        void Close();

        UnitOfMeasurement Unit { get; }
        WordDocumentWriter DocumentWriter { get; }
    }
    #endregion IContentWriter
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