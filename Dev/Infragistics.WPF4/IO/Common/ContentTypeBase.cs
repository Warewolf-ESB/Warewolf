using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using Infragistics.Documents.Core.Packaging;

namespace Infragistics.Documents.Core
{
    #region ContentTypeBase class






    /// <summary>
    /// This class supports the Infragistics infrastructure and
    /// should not be used by your code.
    /// </summary>

    internal abstract class ContentTypeBase
    {
        #region Constants

        internal const string XmlVersion = "1.0";
        internal const string XmlEncoding = "UTF-8";
        internal const string XmlStandalone = "yes";
        internal const string XmlName = "xml";

        #endregion Constants

        #region Abstract decs.
        
        public abstract string ContentType { get; }
		public abstract string RelationshipType { get; }
        
        #endregion Abstract decs.

        #region GetElementName
        protected abstract string GetElementName( Enum element, out string namespaceURI, out string prefix, out string localName );
        #endregion GetElementName

        #region CreateElement
        /// <summary>
        /// Creates a new OfficeXmlElement instance from the specified <paramref name="element"/>.
        /// This method relies on the GetElementName method, so the specified element enum value
        /// must be handled properly in that method.
        /// </summary>
        /// <param name="document">
        /// The owning OfficeXmlDocument
        /// </param>
        /// <param name="element">
        /// An enum value which corresponds to the local name of an element
        /// that is recognized by the GetElementName method.
        /// </param>
        /// <returns>
        /// A new OfficeXmlElementinstance.
        /// </returns>
        public OfficeXmlElement CreateElement( OfficeXmlDocument document, Enum element )
        {
            string ns = null;
            string prefix = null;
            string localName = null;

            this.GetElementName( element, out ns, out prefix, out localName );
            return document.CreateElement( prefix, localName, ns );
        }
        #endregion CreateElement

        #region CreateAttribute
        /// <summary>
        /// Creates a new OfficeXmlAttribute instance from the specified <paramref name="attribute"/>.
        /// This method relies on the GetElementName method, so the specified element enum value
        /// must be handled properly in that method.
        /// </summary>
        /// <param name="document">
        /// The owning OfficeXmlDocument.
        /// </param>
        /// <param name="attribute">
        /// An enum value which corresponds to the local name of an attribute
        /// that is recognized by the GetElementName method.
        /// </param>
        /// <returns>
        /// A new OfficeXmlAttribute instance.
        /// </returns>
        public OfficeXmlAttribute CreateAttribute( OfficeXmlDocument document, Enum attribute )
        {
            string ns = null;
            string prefix = null;
            string localName = null;

            this.GetElementName( attribute, out ns, out prefix, out localName );
            return document.CreateAttribute( prefix, localName, ns );
        }
        #endregion CreateAttribute
    }
    #endregion ContentTypeBase class

    #region ContentTypeExporterBase class
    /// <summary>
    /// This was partially duplicated from
    /// Infragistics.Documents.Excel.Serialization.Excel2007.ContentTypeBase
    /// </summary>
    internal abstract class ContentTypeExporterBase : ContentTypeBase, IContentType
    {
        public abstract void Save( OfficeDocumentExportManager manager, Stream stream, out bool closeStream, ref bool popCounterStack );

        static public void WriteElementsToStream( OfficeXmlDocument document, Stream contentTypeStream )
        {
            List<OfficeXmlNode> childNodes = document.ChildNodes;

            foreach( OfficeXmlNode node in childNodes )
            {
                OfficeXmlNode.WriteNodeRecursive( node );
            }
        }

        #region GetElementName
        protected override string GetElementName( Enum element, out string namespaceURI, out string prefix, out string localName )
        {
            throw new NotSupportedException( "This should have been overridden." );
        }
        #endregion GetElementName

        private void SaveContent( object manager, Stream stream, out bool closeStream, ref bool popCounterStack )
        {
            this.Save( manager as OfficeDocumentExportManager, stream, out closeStream, ref popCounterStack );
        }

        #region IContentType Members

        string IContentType.ContentType
        {
            get { return this.ContentType; }
        }

        string IContentType.RelationshipType
        {
            get { return this.RelationshipType; }
        }

        ContentTypeSaveHandler IContentType.SaveHandler
        {
            get { return new ContentTypeSaveHandler(this.SaveContent); }
        }

        #endregion
    }
    #endregion ContentTypeExporterBase class
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