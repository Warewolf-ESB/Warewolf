using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;

namespace Infragistics.Documents.Excel.Serialization.Excel2007
{
	// MD 9/25/09 - TFS21642
	internal class ExcelXmlDocument : ExcelXmlNode
	{
		#region Member Variables

		private Stream contentTypeStream;
		private XmlNameTable nameTable = new NameTable();

		private XmlReader reader;
		private XmlWriter writer;

		internal string strDocumentName;
		internal string strDocumentFragmentName;
		internal string strCommentName;
		internal string strTextName;
		internal string strCDataSectionName;
		internal string strEntityName;
		internal string strID;
		internal string strNonSignificantWhitespaceName;
		internal string strSignificantWhitespaceName;
		internal string strXmlns;
		internal string strXml;
		internal string strSpace;
		internal string strLang;
		internal string strReservedXmlns;
		internal string strReservedXml;
		internal string strEmpty;

		#endregion Member Variables

		#region Constructor

		public ExcelXmlDocument( Stream contentTypeStream, bool isForLoading )
			: base( null )
		{
			this.contentTypeStream = contentTypeStream;

			if ( isForLoading )
			{
				this.reader = new XmlTextReader( contentTypeStream );
			}
			else
			{
				// MD 10/5/11 - TFS89817
				// Writing to the zip part stream directly seems to be relatively slow for large files. We can speed this up by using a 
				// buffered stream. 2^16 is small enough and provided the best saving in my average of 4 tests for powers of 2 ranging 
				// from 12 to 20.
				//this.writer = new XmlTextWriter( contentTypeStream, Encoding.UTF8 );
				this.writer = new XmlTextWriter(new BufferedStream(contentTypeStream, 1 << 16), Encoding.UTF8);
			}


#region Infragistics Source Cleanup (Region)










#endregion // Infragistics Source Cleanup (Region)

			XmlNameTable nameTable = this.NameTable;
			nameTable.Add( string.Empty );
			this.strDocumentName = nameTable.Add( "#document" );
			this.strDocumentFragmentName = nameTable.Add( "#document-fragment" );
			this.strCommentName = nameTable.Add( "#comment" );
			this.strTextName = nameTable.Add( "#text" );
			this.strCDataSectionName = nameTable.Add( "#cdata-section" );
			this.strEntityName = nameTable.Add( "#entity" );
			this.strID = nameTable.Add( "id" );
			this.strNonSignificantWhitespaceName = nameTable.Add( "#whitespace" );
			this.strSignificantWhitespaceName = nameTable.Add( "#significant-whitespace" );
			this.strXmlns = nameTable.Add( "xmlns" );
			this.strXml = nameTable.Add( "xml" );
			this.strSpace = nameTable.Add( "space" );
			this.strLang = nameTable.Add( "lang" );
			this.strReservedXmlns = nameTable.Add( "http://www.w3.org/2000/xmlns/" );
			this.strReservedXml = nameTable.Add( "http://www.w3.org/XML/1998/namespace" );
			this.strEmpty = nameTable.Add( string.Empty );
		}

		#endregion Constructor

		#region Base Class Overrides

		public override string LocalName
		{
			get { return string.Empty; }
		}

		public override XmlNodeType NodeType
		{
			get { return XmlNodeType.Document; }
		}

		#endregion Base Class Overrides

		#region Methods

		public ExcelXmlAttribute CreateAttribute( string prefix, string localName, string namespaceName )
		{
			return new ExcelXmlAttribute( prefix, localName, namespaceName, this );
		}

		public ExcelXmlElement CreateElement( string prefix, string name, string namespaceName )
		{
			return new ExcelXmlElement( prefix, name, namespaceName, this );
		}

		public ExcelXmlSignificantWhitespace CreateSignificantWhitespace( string value )
		{
			return new ExcelXmlSignificantWhitespace( value, this );
		}

		public ExcelXmlNode CreateTextNode( string value )
		{
			return new ExcelXmlText( value, this );
		}

		public ExcelXmlDeclaration CreateXmlDeclaration( string version, string encoding, string standalone )
		{
			return new ExcelXmlDeclaration( version, encoding, standalone, this );
		}

        //  BF 11/8/10  Infragistics.Word
        #region PopulateChildNodes
        internal void PopulateChildNodes( Stream stream, PopulateChildNodesCallback elementCreatedCallback )
        {
            if ( this.ChildNodes.Count > 0 )
            {
				Utilities.DebugFail("ChildNodes is not empty here. Execution will continue but this is not expected.");
                this.ChildNodes.Clear();
            }

            stream.Position = 0;

            int lastDepth = 0;
            ExcelXmlNode currentElement = this;
            ExcelXmlNode currentParent = this;
            List<ExcelXmlNode> currentParentAtThisLevel = new List<ExcelXmlNode>(10);


            using ( XmlReader reader = XmlReader.Create(stream) )
            {
                while ( reader.EOF == false )
                {
                    reader.Read();

                    switch ( reader.NodeType )
                    {
                        case XmlNodeType.Element:
                        case XmlNodeType.EndElement:
                        {
                            //  If the level has changed, reset the current references
                            //  so that elements go in the right ChildNodes collection.
                            if ( reader.Depth != lastDepth )
                            {
                                if ( reader.Depth > lastDepth )
                                {
                                    currentParent = currentElement;

                                    if ( currentParent != this )
                                    {
                                        if ( reader.Depth >= currentParentAtThisLevel.Count )
                                            currentParentAtThisLevel.Add(null);
                                        
                                        currentParentAtThisLevel[reader.Depth - 1] = currentParent;
                                    }
                                }
                                else
                                {
                                    currentParent =
                                        reader.Depth == 0 ?
                                        this :
                                        currentParentAtThisLevel[reader.Depth - 1];
                                }

                                lastDepth = reader.Depth;
                            }

                            if ( reader.NodeType == XmlNodeType.Element )
                            {
                                //  Create the element
                                ExcelXmlElement element = new ExcelXmlElement(
                                    reader.Prefix,
                                    reader.LocalName,
                                    reader.NamespaceURI,
                                    this );

                                //  Set its value if it has one
                                if ( string.IsNullOrEmpty(reader.Value) == false )
                                    element.Value = reader.Value;

                                //  Populate its Attributes collection
                                ExcelXmlDocument.GetAttributes( this, element, reader );

                                //  If a callback delegate was specified, call it
                                if ( elementCreatedCallback != null )
                                    elementCreatedCallback( element );

                                //  Add the element to whatever collection it should belong to
                                currentParent.AppendChild( element );

                                //  Update the reference to the last element processed.
                                currentElement = element;
                            }

                        }
                        break;
                    }
                }
            }
        }

        internal delegate void PopulateChildNodesCallback( ExcelXmlElement element );

        static private void GetAttributes(
            ExcelXmlDocument document,
            ExcelXmlElement element,
            XmlReader reader )
        {
            if ( reader.HasAttributes )
            {
                for ( int i = 0; i < reader.AttributeCount; i ++ )
                {
                    reader.MoveToAttribute(i);

                    ExcelXmlAttribute attribute =
                        new ExcelXmlAttribute(
                            reader.Prefix,
                            reader.LocalName,
                            reader.NamespaceURI,
                            document );

                    attribute.Value = reader.Value;

                    element.Attributes.Add( attribute );
                }
            }
        }

        #endregion PopulateChildNodes

		#endregion Methods

		#region Properties

		public XmlNameTable NameTable
		{
			get { return this.nameTable; }
		}

		public XmlReader Reader
		{
			get { return this.reader; }
		}

		public XmlWriter Writer
		{
			get { return this.writer; }
		}

		#endregion Properties
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