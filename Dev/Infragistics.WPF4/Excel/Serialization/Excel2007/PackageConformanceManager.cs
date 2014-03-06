//  BF 7/8/08   OpenPackagingConventions

#region Notes

//  RFC3986:
//  http://www.rfc.net/rfc3986.html

//  RFC3986 -> DGC reference 
//  http://www.w3.org/People/cmsmcq/2005/rfc3986.dcg

//  Regex character classes:
//  http://msdn.microsoft.com/en-us/library/20bw873z.aspx

//  HTTP RFC2616
//  http://www.rfc.net/rfc2616.html#p14

//  Uniform Resource Identifiers (URI): Generic Syntax
//  http://www.ietf.org/rfc/rfc2396.txt

    #region How to find the XSD for a content type (sometimes)

//  1.  Given a content type, find the name of one element that uses it.
//      For example, 'ExternalWorkbookPart' has a content type of
//      "application/vnd.openxmlformats-officedocument.spreadsheetml.externalLink+xml",
//      and the associated element name is "externalLink"
//
//  2.  Search the 'Office Open XML Part 4 - Markup Language Reference' document for that
//      element (start at page 1861, since Adobe reader is a bit lethargic on searching)
//
//  3.  Find one of the complex types (e.g., "CT_ExternalBook", "CT_DdeLink") that it uses
//  4.  Search the text of each XSD for that type using the following method:
//
//private string FindXSD( string path, string searchString )
//{
//    DirectoryInfo di =  new DirectoryInfo( path );
//    FileInfo[] fis = di.GetFiles();
//    string foundit = string.Empty;
//    string search = this.txtSearch.Text;

//    foreach ( FileInfo  fi in fis )
//    {
//        using( StreamReader sr = fi.OpenText() )
//        {
//            while ( sr.EndOfStream == false )
//            {
//                string line = sr.ReadLine();

//                if ( line != null && line.Contains( search ) )
//                    return fi.Name;
//            }
//        }
//    }
//}
    #endregion How to find the XSD for a content type

#endregion Notes

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Schema;
using System.Diagnostics;
using System.IO;
using Infragistics.Documents.Excel.Serialization.Excel2007.SharedContentTypes;
using Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.ContentTypes;






using Infragistics.Shared;
using System.Drawing;
using System.IO.Compression;


namespace Infragistics.Documents.Excel.Serialization.Excel2007
{
    #region PackageConformanceManager class





    internal class PackageConformanceManager
    {
        #region Constants

        internal const char ForwardSlashChar = '/';
        internal const char SemiColonChar = ';';
        internal const char SpaceChar = ' ';
        internal const char EqualsChar = '=';

        internal const string RegexPatternEqualSignWithWhiteSpace = " =|= ";

        #region Section [M1.22]
        public const string CorePropertiesPartContentTypeValue = CorePropertiesPart.ContentTypeValue;
        public const string DigitalSignatureCertficatePartContentTypeValue = "application/vnd.openxmlformats-package.digital-signaturecertificate";
        public const string DigitalSignatureOriginPartContentTypeValue = "application/vnd.openxmlformats-package.digital-signature-origin";
        public const string DigitalSignatureXMLSignaturePartContentTypeValue = "application/vnd.openxmlformats-package.digital-signaturexmlsignature+xml";
        public const string RelationshipsPartContentTypeValue = "application/vnd.openxmlformats-package.relationships+xml";
        #endregion Section [M1.22]

        internal const string MarkupCompatibilityNamespace = "http://schemas.openxmlformats.org/markup-compatibility/2006";

        internal const string XsdResourceLocation = "Infragistics.Documents.Excel.Serialization.Excel2007.XSD.";

        internal const string XsiNamespaceName = "http://www.w3.org/2001/XMLSchema-instance";
        internal const string XmlNamespaceName = "http://www.w3.org/XML/1998/namespace";
        internal const string DcTermsNamespaceName = "http://purl.org/dc/terms/";
        internal const string PrinterSettingsContentTypeValue = "application/vnd.openxmlformats-officedocument.spreadsheetml.printerSettings";

        internal const string MissingPartNameToken = "{MissingPartName}";
        #endregion Constants

        #region Constructor
        // MRS NAS v8.3
        internal PackageConformanceManager(bool verifyExcel2007Xml)
        {
            this.verifyExcel2007Xml = verifyExcel2007Xml;
        }
        #endregion //Constructor

        #region Member variables

        private bool                                                    wasVerified = false;
        private Dictionary<string, IPackagePart>                         conformantParts = null;
        private Dictionary<string, OpenPackagingNonConformanceReason>     nonConformantParts = null;
        static private List<string>                                     packageSpecificContentTypes = null;
        private IPackage                                                 package = null;





        private Dictionary<OpenPackagingNonConformanceReason, object>     exemptions = null;

		[ThreadStatic]
		private static Dictionary<string, XmlSchema> cachedSchemas;

        // MRS NAS v8.3
        private bool verifyExcel2007Xml = true;

        #endregion Member variables

        #region Methods

            #region IsPartConformant
        /// <summary>
        /// Returns whether the specified IPackagePart is fully conformant
        /// to the ECMA TC45 Open Packaging Conventions.
        /// </summary>
		/// <param name="packagePartPath">A string representing the absolute target to the IPackagePart to test.</param>
        /// <returns>A boolean indicating whether the specified IPackagePart is ECMA TC45-conformant.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="packagePartPath"/> is null or empty.</exception>
        public bool IsPartConformant( string packagePartPath )
        {
            if ( string.IsNullOrEmpty(packagePartPath) )
                throw new ArgumentNullException( "packagePartPath" );

            OpenPackagingNonConformanceReason reason = this.GetPartConformance( packagePartPath );
            return (reason == OpenPackagingNonConformanceReason.Conformant);
        }

        /// <summary>
        /// Returns whether the specified IPackagePart is fully conformant
        /// to the ECMA TC45 Open Packaging Conventions.
        /// </summary>
        /// <param name="packagePartUri">The URI of the IPackagePart to test.</param>
        /// <returns>A boolean indicating whether the specified IPackagePart is ECMA TC45-conformant.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="packagePartUri"/> is null.</exception>
        public bool IsPartConformant( Uri packagePartUri )
        {
            if ( packagePartUri == null )
                throw new ArgumentNullException( "packagePartUri" );

            return this.IsPartConformant( packagePartUri.ToString() );
        }

        /// <summary>
        /// Returns whether the specified IPackagePart is fully conformant
        /// to the ECMA TC45 Open Packaging Conventions.
        /// </summary>
        /// <param name="packagePart">The IPackagePart to test.</param>
        /// <returns>A boolean indicating whether the specified IPackagePart is ECMA TC45-conformant.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="packagePart"/> is null.</exception>
        public bool IsPartConformant( IPackagePart packagePart )
        {
            if ( packagePart == null )
                throw new ArgumentNullException( "packagePart" );

            if ( packagePart.Package != this.package )
                throw new Exception( "The specified packagePart is not associated with the IPackage that was verified." );

            return this.IsPartConformant( packagePart.Uri.ToString() );
        }



#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

        private OpenPackagingNonConformanceReason GetPartConformance( string partPath )
        {
            if ( this.wasVerified == false )
                throw new Exception( "The IPackage associated with this instance has not been verified for conformance." );

            OpenPackagingNonConformanceReason retVal = OpenPackagingNonConformanceReason.Conformant;

            if ( string.IsNullOrEmpty(partPath) )
            {
                PackageConformanceManager.Assert( false, "Empty part name specified in PackageConformanceManager.GetPartConformance." );
                return OpenPackagingNonConformanceReason.NameMissing;
            }

            if ( this.nonConformantParts != null && this.nonConformantParts.ContainsKey(partPath) )
                retVal = this.nonConformantParts[partPath];

            if ( this.exemptions != null && this.exemptions.ContainsKey(retVal) )
                retVal = OpenPackagingNonConformanceReason.Conformant;

            return retVal;
        }
            #endregion IsPartConformant

            #region VerifyPackageConformance
        /// <summary>
        /// Verifies that the specified IPackage conforms to the Open Packing Conventions
        /// established by ECMA TC45. Throws an exception if the reason for non-conformance
        /// warrants treating the whole package as invalid.
        /// </summary>
        /// <param name="package">The IPackage to verify</param>
        /// <returns>True if all PackageParts passed all conformance tests, false otherwise</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="package"/> is null.</exception>
		/// <exception cref="OpenPackagingNonConformanceException">
        /// Thrown if a part was determined to be non-conformant, and the degree of
        /// non-conformance is such that the entire package should be considered invalid.
        /// </exception>
        public bool VerifyPackageConformance( IPackage package )
        {
            if ( package == null )
                throw new ArgumentNullException( "package" );

            this.package = package;
            OpenPackagingNonConformanceException exception = null;
            bool result = true;
            
            try
            {



                result = PackageConformanceManager.VerifyPackageConformance(
                    package,
                    this.NonConformantParts,
                    this.ConformantParts,
                    this.verifyExcel2007Xml,
                    out exception );
            }
            finally
            {



            }

            if ( exception != null )
                throw exception;

            this.wasVerified = true;

            return result;
        }

        /// <summary>
        /// Verifies that the specified IPackage conforms to the Open Packing Conventions
        /// established by ECMA TC45.
        /// </summary>
        /// <param name="package">The IPackage to verify</param>
        /// <param name="nonConformantParts">A dictionary to which the non-conformant parts are added.</param>
        /// <param name="conformantParts">A dictionary to which the conformant parts are added.</param>
        /// <param name="exception">[out] Upon return, contains a reference to the PackageNonConformanceException-derived exception that describes the reason the package was determined to be non-conformant, or null if the package was at least partially conformant.</param>
        /// <returns>True if all PackageParts passed all conformance tests, false otherwise</returns>
        static public bool VerifyPackageConformance(
            IPackage package,
            Dictionary<string, OpenPackagingNonConformanceReason> nonConformantParts,
            Dictionary<string, IPackagePart> conformantParts,
            out OpenPackagingNonConformanceException exception)
        {
            return VerifyPackageConformance(package, nonConformantParts, conformantParts, true, out exception);
        }

        /// <summary>
        /// Verifies that the specified IPackage conforms to the Open Packing Conventions
        /// established by ECMA TC45.
        /// </summary>
        /// <param name="package">The IPackage to verify</param>
        /// <param name="nonConformantParts">A dictionary to which the non-conformant parts are added.</param>
        /// <param name="conformantParts">A dictionary to which the conformant parts are added.</param>
        /// <param name="verifyXMLContent">A boolean specifying whether or not to verify the contents of the markup against the rules defined in Part 2 of the 'Office Open XML - Open Packaging Conventions' document (see final draft, <a href="http://www.ecma-international.org">ECMA</a> document TC45).</param>
        /// <param name="exception">[out] Upon return, contains a reference to the PackageNonConformanceException-derived exception that describes the reason the package was determined to be non-conformant, or null if the package was at least partially conformant.</param>
        /// <returns>True if all PackageParts passed all conformance tests, false otherwise</returns>
        static public bool VerifyPackageConformance(
            IPackage package,
            Dictionary<string, OpenPackagingNonConformanceReason> nonConformantParts,
            Dictionary<string, IPackagePart> conformantParts,
            bool verifyXMLContent,
            out OpenPackagingNonConformanceException exception )
        {
            exception = null;

            if ( conformantParts != null )
                conformantParts.Clear();
            else
                conformantParts = new Dictionary<string,IPackagePart>();

            if ( nonConformantParts != null )
                nonConformantParts.Clear();
            else
                nonConformantParts = new Dictionary<string,OpenPackagingNonConformanceReason>();
            
            //  Verify the XML content of the relationship parts themselves
            PackageConformanceManager.VerifyRelationshipContent( package, nonConformantParts, conformantParts );

            bool corePropertiesPartProcessed = false;
            bool exceptionThrown = false;

            try
            {
                IEnumerable<IPackageRelationship> relationships = package.GetRelationships();
                PackageConformanceManager.VerifyPackageRelationships( package, null, relationships, nonConformantParts, conformantParts, verifyXMLContent, ref corePropertiesPartProcessed );
            }
            catch( Exception ex )
            {
                exceptionThrown = true;

                if ( (ex is OpenPackagingNonConformanceException) == false )
                    PackageConformanceManager.Assert( false, string.Format("{0}{1}{2}{3}{4}", "Unexpected exception thrown in PackageConformanceManager.VerifyPackageConformance:", Environment.NewLine, ex.Message, Environment.NewLine, ex.StackTrace) );
                else
                    exception = ex as OpenPackagingNonConformanceException;
            }

            return  (exceptionThrown == false) &&
                    (nonConformantParts == null || nonConformantParts.Count == 0);
        }
            #endregion VerifyPackageConformance

            #region VerifyRelationshipContent

        static internal void VerifyRelationshipContent(
            IPackage package,
            Dictionary<string, OpenPackagingNonConformanceReason> nonConformantParts,
            Dictionary<string, IPackagePart> conformantParts )
        {
            //  First get all the PackageParts that represent the relationships
            List<IPackagePart> relationshipParts = new List<IPackagePart>();
            IEnumerable<IPackagePart> parts = package.GetParts();

            //  5/27/09 TFS17689
            //  Verify part name equivalence here, which iterates through all
            //  parts in the package.
            #region M1.11, M1.12
            //
            //  M1.11: A package implementer shall neither create nor recognize
            //  a part with a part name derived from another part name...
            //
            //  M1.12: Packages shall not contain equivalent part names and package
            //  implementers shall neither create nor recognize packages with equivalent
            //  part names.
            //
            foreach ( IPackagePart part in parts )
            {
                //  Get the abolute path of the Uri
                Uri partUri = part.Uri;
                string partPath = partUri != null ? partUri.ToString() : string.Empty;

                PackageConformanceManager.VerifyPartNameEquivalence(partPath, nonConformantParts, conformantParts );
            }

            //  Clear the dictionaries, since the above verification stands
            //  separate from the other ones, which involve things like XML
            //  formation and content type.
            conformantParts.Clear();
            nonConformantParts.Clear();

            #endregion M1.11, M1.12


            foreach ( IPackagePart part in parts )
            {
                if ( string.Equals(part.ContentType, PackageConformanceManager.RelationshipsPartContentTypeValue, StringComparison.InvariantCultureIgnoreCase) )
                    relationshipParts.Add( part );
            }

            //  Iterate each relationship part and validate each relationship therein
            for ( int i = 0; i < relationshipParts.Count; i ++ )
            {
                IPackagePart relationshipPart = relationshipParts[i];
                PackageConformanceManager.VerifyRelationshipContent( relationshipPart, nonConformantParts, conformantParts );
            }
        }

        static internal void VerifyRelationshipContent(
            IPackagePart relationshipPart,
            Dictionary<string, OpenPackagingNonConformanceReason> nonConformantParts,
            Dictionary<string, IPackagePart> conformantParts )
        {
            #region [M1.30]
            //  Make sure the name of the relationship and its containing directory conform.
            string relationshipPartPath = relationshipPart.Uri.ToString();
            List<string> segments = PackageUtilities.GetSegments(relationshipPartPath);
            if ( segments.Count < 2 ||
                 segments[segments.Count - 1].EndsWith(PackageUtilities.RelationshipsPartsSuffix, StringComparison.InvariantCultureIgnoreCase) == false ||
                 segments[segments.Count - 2].EndsWith(PackageUtilities.RelationshipsPartsSubFolder, StringComparison.InvariantCultureIgnoreCase) == false )
            {
                PackageConformanceManager.LogNonConformantPart( relationshipPartPath, OpenPackagingNonConformanceReason.RelationshipNameInvalid, nonConformantParts, conformantParts );
                return;
            }
            #endregion [M1.30]

            Dictionary<string, object> relationshipIds = new Dictionary<string,object>( StringComparer.InvariantCultureIgnoreCase );

			using ( Stream partStream = relationshipPart.GetStream(FileMode.Open, FileAccess.Read) )
			{			    

                XmlDocument document = new XmlDocument();
                document.Load( partStream );

                foreach( XmlNode node in document.ChildNodes )
                {
                    PackageConformanceManager.VerifyRelationshipContent(
                        node,
                        relationshipPart,
                        relationshipIds,
                        nonConformantParts,
                        conformantParts );
                }


#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

            }
        }

        static internal void VerifyRelationshipContent(
            XmlNode node,
            IPackagePart relationshipPart,
            Dictionary<string, object> relationshipIds,
            Dictionary<string, OpenPackagingNonConformanceReason> nonConformantParts,
            Dictionary<string, IPackagePart> conformantParts )
        {
            string nodeLocalName = Utilities.GetXmlNodeName(node);
            if (string.Equals(nodeLocalName, "relationship", StringComparison.InvariantCultureIgnoreCase))
            {
                string relationshipPartPath = relationshipPart.Uri.ToString();

                string id = string.Empty;
                string target = string.Empty;
                RelationshipTargetMode targetMode = RelationshipTargetMode.Internal;
                string type = string.Empty;

                // Iterate the attributes and cache the relevant values
                foreach (XmlAttribute attribute in Utilities.GetXmlNodeAttributes(node))
                {
                    string name = Utilities.GetXmlAttributeName(attribute);
                    string value = attribute.Value;

                    if ( string.Equals(name, "id", StringComparison.InvariantCultureIgnoreCase) )
                        id = value;
                    else
                    if ( string.Equals(name, "target", StringComparison.InvariantCultureIgnoreCase) )
                        target = value;
                    else
                    if ( string.Equals(name, "targetmode", StringComparison.InvariantCultureIgnoreCase) )
                        targetMode = string.Equals(value, "external", StringComparison.InvariantCultureIgnoreCase) ? RelationshipTargetMode.External : RelationshipTargetMode.Internal;
                    else
                    if ( string.Equals(name, "type", StringComparison.InvariantCultureIgnoreCase) )
                        type = value;
                }

                int nonConformantCount = nonConformantParts.Count;

                #region [M1.26]
                
                //  ...The package implementer shall require that every Relationship
                //  element has an Id attribute, the value of which is unique within the
                //  Relationships part...

                if ( string.IsNullOrEmpty(id) == false )
                {
                    //  If the attribute exists, but has no value, return failure.
                    if ( string.IsNullOrEmpty(id) )
                        PackageConformanceManager.LogNonConformantPart( relationshipPartPath, OpenPackagingNonConformanceReason.RelationshipIdInvalid, nonConformantParts, conformantParts );

                    //  If the ID is a duplicate, log a failure.
                    bool isDuplicate = relationshipIds.ContainsKey(id);
                    if ( isDuplicate )
                        PackageConformanceManager.LogNonConformantPart( relationshipPartPath, OpenPackagingNonConformanceReason.RelationshipIdInvalid, nonConformantParts, conformantParts );
                    else
                    //  ...which conforms to the naming restrictions for xsd:ID as
                    //  described in the W3C Recommendation “XML Schema Part 2: Datatypes.”
                    //  (in a nutshell, this means no colons).
                    if ( id.Contains(":") )
                        PackageConformanceManager.LogNonConformantPart( relationshipPartPath, OpenPackagingNonConformanceReason.RelationshipIdInvalid, nonConformantParts, conformantParts );
                }
                else
                    PackageConformanceManager.LogNonConformantPart( relationshipPartPath, OpenPackagingNonConformanceReason.RelationshipIdInvalid, nonConformantParts, conformantParts );
                
                if ( nonConformantParts.Count > nonConformantCount )
                    return;
                else
                    nonConformantCount = nonConformantParts.Count;

                #endregion [M1.26]

                #region [M1.27]
                if ( string.IsNullOrEmpty(type) == false )
                {
                    
                    
                    
                    if ( Uri.IsWellFormedUriString(type, UriKind.RelativeOrAbsolute) == false ) 
                        PackageConformanceManager.LogNonConformantPart( relationshipPartPath, OpenPackagingNonConformanceReason.RelationshipTypeInvalid, nonConformantParts, conformantParts );
                }

                if ( nonConformantParts.Count > nonConformantCount )
                    return;
                else
                    nonConformantCount = nonConformantParts.Count;

                #endregion [M1.27]

                #region [M1.25], [M1.28], [M1.29], [M1.30]
                if ( string.IsNullOrEmpty(target) == false )
                {
                    //  BF 8/14/08
                    //  I am removing this check based on the following excerpt from
                    //  section 4.2 of RFC3986:
                    //
                    //  "A relative reference that begins with two slash characters is termed
                    //  a network-path reference; such references are rarely used.  A
                    //  relative reference that begins with a single slash character is
                    //  termed an absolute-path reference.  A relative reference that does
                    //  not begin with a slash character is termed a relative-path reference."
                    //
                    //  As I remember, I had determined that a relative reference was implied to
                    //  not begin with a forward slash, but based on the paragraph above, I see
                    //  that they can begin with a forward slash. When we create our relationships
                    //  (see Excel2007WorkbookSerializationManager.CreateRelationshipInPackage), we
                    //  begin the URIs with a forward slash, and Excel seems to handle them, so I
                    //  conclude that a forward slash in a relative reference is OK.
                    //
                    #region Obsolete code
                    ////  [M1.30]
                    ////  If the target begins with a forward slash, that is implied to
                    ////  be an absolute reference, which is non-comformant.
                    //if ( target[0] == PackageConformanceManager.ForwardSlashChar )
                    //    PackageConformanceManager.LogNonConformantPart( relationshipPartPath, OpenPackagingNonConformanceReason.RelationshipTargetNotRelativeReference, nonConformantParts, conformantParts );
                    #endregion Obsolete code
                    
                    //  BF 8/18/08
                    //  One thing we can check for to make sure the target is not an absolute
                    //  reference is the presence of a colon, which is intended to separate the
                    //  the scheme from the authority (see [M1.29])
                    if ( targetMode == RelationshipTargetMode.Internal )
                    {
                        string[] segments = target.Split(PackageConformanceManager.ForwardSlashChar);
                        if ( segments.Length > 1 &&
                             segments[0].EndsWith(":", StringComparison.InvariantCultureIgnoreCase) )
                            PackageConformanceManager.LogNonConformantPart( relationshipPartPath, OpenPackagingNonConformanceReason.RelationshipTargetNotRelativeReference, nonConformantParts, conformantParts );
                    }

                    if ( targetMode == RelationshipTargetMode.Internal )
                    {
                        //  Get the absolute Uri
                        string targetPath = PackageConformanceManager.GetTargetPath( relationshipPart, target );

                        if ( string.IsNullOrEmpty(targetPath) == false )
                        {
                            //  [M1.28]
                            //  Make sure the target can be resolved to a part in this package.
                            Uri targetUri = new Uri( targetPath, UriKind.RelativeOrAbsolute );
                            IPackage package = relationshipPart.Package;
                            if ( package.PartExists(targetUri) == false )
                                PackageConformanceManager.LogNonConformantPart( relationshipPartPath, OpenPackagingNonConformanceReason.RelationshipTargetInvalid, nonConformantParts, conformantParts );
                            else
                            {
                                //  [M1.25]
                                //  Make sure the target package part is not a relationship.
                                IPackagePart targetPart = package.GetPart( targetUri );

                                if ( string.Equals(targetPart.ContentType, PackageConformanceManager.RelationshipsPartContentTypeValue) )
                                    PackageConformanceManager.LogNonConformantPart( relationshipPartPath, OpenPackagingNonConformanceReason.RelationshipTargetsOtherRelationship, nonConformantParts, conformantParts );
                            }
                        }
                        else
                            PackageConformanceManager.LogNonConformantPart( relationshipPartPath, OpenPackagingNonConformanceReason.RelationshipTargetInvalid, nonConformantParts, conformantParts );

                    }
                    else
                    {
                        
                        //  See Annex B
                        #region [M1.29] (see also [O1.5])
                        #endregion [M1.29] (see also [O1.5])
                    }
                    
                }
                else
                    //  [M1.28]
                    //  If the attribute was present, but had no value, there is no target.
                    PackageConformanceManager.LogNonConformantPart( relationshipPartPath, OpenPackagingNonConformanceReason.RelationshipTargetInvalid, nonConformantParts, conformantParts );

                if ( nonConformantParts.Count > nonConformantCount )
                    return;
                else
                    nonConformantCount = nonConformantParts.Count;

                #endregion [M1.25], [M1.28], [M1.29], [M1.30]

                //  Add the Id to the dictionary if this relationship was determined to be valid.
                relationshipIds.Add( id, null );
            }

            //  I haven't seen any nested elements for <Relationship> yet; this
            //  recursive call might be unnecessary.
            foreach (XmlNode childNode in Utilities.GetXmlNodeNodes(node))
            {
                PackageConformanceManager.VerifyRelationshipContent(
                    childNode,
                    relationshipPart,
                    relationshipIds,
                    nonConformantParts,
                    conformantParts );
            }
        }
            #endregion VerifyRelationshipContent

            #region GetTargetPath


#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

        private static string GetTargetPath( IPackagePart relationshipPart, string target )
        {
            if ( string.IsNullOrEmpty(target) )
                return null;

            string targetFullPath = string.Empty;
            List<string> targetSegments = PackageUtilities.GetSegments( target );
            bool isAbsolute = targetSegments[0].Length == 0;
            bool useCurrent = targetSegments.IndexOf(".") >= 0;
            bool useParent = targetSegments.IndexOf("..") >= 0;
            string relationshipPartPath = relationshipPart.Uri.ToString();
            string retVal = target;

            if ( isAbsolute == false )
            {
                StringBuilder sb = new StringBuilder();
                
                //  According to RFC2396, section 5, "...Within a
                //  relative-path reference, the complete path segments "." and ".." have
                //  special meanings: "the current hierarchy level" and "the level above this
                //  hierarchy level", respectively.  Although this is very similar to their use
                //  within Unix-based file systems to indicate directory levels, these path components
                //  are only considered special when resolving a relative-path reference to its absolute
                //  form.
                //
                //  Note that in the case of "/xl/worksheets/_rels/[some sheet].rels", a target
                //  listed as "Target="../printerSettings/printerSettings1.bin" actually resides
                //  TWO levels above "this hierarchy level" if you count the '_rels' directory.
                //  Based on this I am assuming that you should not count the '_rels' directory.

                int indexOfRelationshipsDirectory = relationshipPartPath.IndexOf( "/_rels" );

                //  We are expecting the relationshipPart to be in a '_rels' directory.
                if ( indexOfRelationshipsDirectory < 0 )
                {
                    PackageConformanceManager.Assert( false, "Could not find the '_rels' directory for a IPackagePart that was determined to be a relationship - unexpected." );
                    return null;
                }

                //  Now we have the base URI string...
                string baseURIString = relationshipPartPath.Substring(0, indexOfRelationshipsDirectory);

                //  If the target was relative, but did not have any special tokens,
                //  we can just combine the base URI and the relative target.
                if ( useCurrent == false && useParent == false )
                {
                    sb.Append( baseURIString );
                    sb.Append( string.Format("{0}{1}", PackageConformanceManager.ForwardSlashChar, target) );
                }
                //  If we have  "." or ".." in the target, we need to translate it
                else
                {
                    //  Reverse iterate the segments of the target URI (which was split on a
                    //  forward slash), and increment a counter with each ".." that is encountered.
                    //  Note that if a "." is encountered, we immediately break the loop and consider
                    //  anything preceding that to be meaningless, since "./." is invalid relative URI 
                    //  syntax.
                    int levels = 0;
                    int firstSegmentIndex = -1;
                    for ( int  i = targetSegments.Count - 1; i >= 0; i -- )
                    {
                        string segment = targetSegments[i];

                        if ( useParent )
                        {
                            if ( string.Equals(segment, "..") )
                            {
                                if ( firstSegmentIndex == -1 )
                                    firstSegmentIndex = (i + 1);

                                levels++;
                            }
                        }
                        else
                        if ( useCurrent && string.Equals(segment, ".") )
                        {
                            firstSegmentIndex = i + 1;
                            break;
                        }

                    }

                    //  Remove whatever segments of the base URI we need to.
                    if ( levels > 0 )
                    {
                        List<string> baseURISegments = PackageUtilities.GetSegments( baseURIString );

                        int endIndex = baseURISegments.Count - (levels + 1);
                        for ( int i = 1; i <= endIndex; i ++ )
                        {
                            string segment = baseURISegments[i];
                            sb.Append( string.Format("{0}{1}", PackageConformanceManager.ForwardSlashChar, segment) );
                        }
                    }

                    //  Now append the relative URI to the base URI to get the absolute URI
                    for ( int i = firstSegmentIndex; i < targetSegments.Count; i ++ )
                    {
                        string segment = targetSegments[i];
                        if ( segment.Length == 0 )
                            continue;

                        sb.Append( string.Format("{0}{1}", PackageConformanceManager.ForwardSlashChar, segment) );
                    }
                
                }
    
                retVal = sb.ToString();
            }

            return retVal;
        }
            #endregion GetTargetPath
        
            #region VerifyPackageRelationships

        static private void VerifyPackageRelationships(
            IPackage package,
            IPackagePart parent,
            IEnumerable<IPackageRelationship> relationships,
            Dictionary<string, OpenPackagingNonConformanceReason> nonConformantParts,
            Dictionary<string, IPackagePart> conformantParts,
            bool verifyXMLContent,
            ref bool corePropertiesPartProcessed )
        {
            foreach( IPackageRelationship relationship in relationships )
            {
                //  BF 7/31/08
                //  If the target mode is external, don't verify the part
                //  because there won't be one.
                if ( relationship.TargetMode == RelationshipTargetMode.External )
                    continue;

				Uri partUri = PackageUtilities.GetTargetPartPath( relationship );
                PackageConformanceManager.VerifyPackagePart(package, partUri, nonConformantParts, conformantParts, verifyXMLContent, ref corePropertiesPartProcessed);
            }
        }
            #endregion VerifyPackageRelationships

            #region VerifyPackagePart

        static public void VerifyPackagePart(
            IPackage package,
            Uri partUri,
            Dictionary<string, OpenPackagingNonConformanceReason> nonConformantParts,
            Dictionary<string, IPackagePart> conformantParts,
            ref bool corePropertiesPartProcessed)
        {
            VerifyPackagePart(package, partUri, nonConformantParts, conformantParts, true, ref corePropertiesPartProcessed);
        }

        static public void VerifyPackagePart(
            IPackage package,
            Uri partUri,
            Dictionary<string, OpenPackagingNonConformanceReason> nonConformantParts,
            Dictionary<string, IPackagePart> conformantParts,
            bool verifyXMLContent,
            ref bool corePropertiesPartProcessed )
        {
            //  Get the abolute path of the Uri
            string partPath = partUri != null ? partUri.ToString() : string.Empty;

            //  BF 5/27/09  TFS17689
            //  If we have already verified this part, return, so we don't
            //  run it through all the same logic a second time.
            if ( (conformantParts != null && conformantParts.ContainsKey(partPath)) ||
                 (nonConformantParts != null && nonConformantParts.ContainsKey(partPath)) )
                return;

            //  Get the part segments...note that the first member of the list
            //  is expected to be an empty string, since GetSegments splits 
            //  on a forward slash.
			List<string> segments = string.IsNullOrEmpty(partPath) ?
                null :
                PackageUtilities.GetSegments( partPath );

            #region M1.1: The package implementer shall require a part name.

            if ( segments == null || segments.Count == 0 )
            {
                PackageConformanceManager.LogNonConformantPart( PackageConformanceManager.MissingPartNameToken, OpenPackagingNonConformanceReason.NameMissing, nonConformantParts, conformantParts );
                return;
			}

            #endregion M1.1: The package implementer shall require a part name.

            #region M1.4: A part name shall start with a forward slash (“/”) character.

            if ( segments[0].Length != 0 )
            {
                PackageConformanceManager.LogNonConformantPart( partPath, OpenPackagingNonConformanceReason.NameDoesNotStartWithForwardSlash, nonConformantParts, conformantParts );
                return;
            }
            
            #endregion M1.4: A part name shall start with a forward slash (“/”) character.

            #region M1.5: A part name shall not have a forward slash as the last character.

            if ( partPath[partPath.Length - 1] == PackageUtilities.SegmentSeparator )
            {
                PackageConformanceManager.LogNonConformantPart( partPath, OpenPackagingNonConformanceReason.NameEndsWithForwardSlash, nonConformantParts, conformantParts );
                return;
            }
            
            #endregion M1.5: A part name shall not have a forward slash as the last character.

            #region M1.3, M1.6, M1.7, M1.8, M1.9, M1.10 (segments)
            for ( int i = 1; i < segments.Count; i ++ )
            {
                string segment = segments[i];
                OpenPackagingNonConformanceReason invalidSegmentReason = OpenPackagingNonConformanceReason.Conformant;
                if ( PackageConformanceManager.IsSegmentValid(segment, out invalidSegmentReason) == false )
                {
                    PackageConformanceManager.LogNonConformantPart( partPath, invalidSegmentReason, nonConformantParts, conformantParts );
                    return;
                }
            }
            #endregion M1.3, M1.6, M1.7, M1.8, M1.9, M1.10 (segments)

            //  BF 5/27/09  TFS17689
            //
            //  This is the wrong place to be doing this; it should be
            //  verified for each part in the package, but doing it here
            //  could potentially verify it for each reference to the part.
            //
            #region Obsolete code
            //#region M1.11, M1.12 (part name equivalence)
            ////  M1.11: A package implementer shall neither create nor recognize
            ////  a part with a part name derived from another part name...
            ////
            ////  M1.12: Packages shall not contain equivalent part names and package
            ////  implementers shall neither create nor recognize packages with equivalent
            ////  part names.

            //if ( PackageConformanceManager.VerifyPartNameEquivalence(partPath, nonConformantParts, conformantParts ) == false )
            //    return;

            //#endregion M1.11, M1.12 (part name equivalence)
            #endregion Obsolete code

            #region Get the IPackagePart

            IPackagePart part = null;

            try
            {
                part = package.GetPart( partUri );
            }
            catch( Exception ex )
            {
                PackageConformanceManager.Assert( false, string.Format("Exception thrown by IPackage.GetPart:{0}{1}", Environment.NewLine, ex.Message) );
                PackageConformanceManager.LogNonConformantPart( partPath, OpenPackagingNonConformanceReason.CouldNotGetPackagePart, nonConformantParts, conformantParts );
                return;
            }

            #endregion Get the IPackagePart

            #region M1.2: The package implementer shall require a content type...

            if ( string.IsNullOrEmpty(part.ContentType) )
            {
                PackageConformanceManager.LogNonConformantPart( partPath, OpenPackagingNonConformanceReason.ContentTypeMissing, nonConformantParts, conformantParts );
                return;
            }
            
            #endregion M1.2: The package implementer shall require a content type...

            #region M1.13, M1.14, M1.15 (content type)
            // IPackage implementers shall only create and only recognize parts with a content type;
            // format designers shall specify a content type for each part included in the format.
            // Content types for package parts shall fit the definition and syntax for media types
            // as specified in RFC 2616, §3.7. [M1.13]

            // Content types shall not use linear white space either between the type and subtype or
            // between an attribute and its value. Content types also shall not have leading or
            // trailing white spaces. IPackage implementers shall create only such content types
            // and shall require such content types when retrieving a part from a package;
            // format designers shall specify only such content types for inclusion in the format. [M1.14]
            if ( PackageConformanceManager.VerifyContentType(partPath, part.ContentType, nonConformantParts, conformantParts) == false )
                return;

            #endregion M1.13, M1.14, M1.15 (content type)

            #region M1.16, M1.17, M1.18, M1.19, M1.20, M1.21 (Xml Usage)

            if (verifyXMLContent)
            {
                OpenPackagingNonConformanceReason xmlContentReason = PackageConformanceManager.VerifyXmlContent(partPath, part, ref corePropertiesPartProcessed);
                if (xmlContentReason != OpenPackagingNonConformanceReason.Conformant)
                {
                    PackageConformanceManager.LogNonConformantPart(partPath, xmlContentReason, nonConformantParts, conformantParts);
                    return;
                }
            }

            #endregion M1.16, M1.17, M1.18, M1.19, M1.20, M1.21 (Xml Usage)

            //  Add this part to the conformant list, if it survived all those verifications
            PackageConformanceManager.LogConformantPart( part, nonConformantParts, conformantParts );

            //  Call VerifyPackageRelationships recursively on this part's relationships
            IEnumerable<IPackageRelationship> relationships = part.GetRelationships();
            PackageConformanceManager.VerifyPackageRelationships( package, part, relationships, nonConformantParts, conformantParts, verifyXMLContent, ref corePropertiesPartProcessed );
        }
            #endregion VerifyPackagePart

            #region IsSegmentValid
        static internal bool IsSegmentValid( string segment, out OpenPackagingNonConformanceReason reason )
        {
            reason = OpenPackagingNonConformanceReason.Conformant;

            //  M1.3: A part name shall not have empty segments.
			if ( String.IsNullOrEmpty( segment ) ) 
            {
                reason = OpenPackagingNonConformanceReason.SegmentEmpty;
				return false;
            }

            //  M1.10: A segment shall include at least one non-dot character
			if ( PackageUtilities.NonDotRegex.IsMatch(segment) == false )
			{
                reason = OpenPackagingNonConformanceReason.SegmentMissingNonDotCharacter;
				return false;
			}

            //  M1.9: A segment shall not end with a dot (“.”) character.
			if ( segment[ segment.Length - 1 ] == '.' )
            {
                reason = OpenPackagingNonConformanceReason.SegmentEndsWithDotCharacter;
				return false;
            }

            //  M1.7: A segment shall not contain percent-encoded forward slash (“/”), or backward slash (“\”) characters.
            //  Note that a percent encoded forward slash is theoretically impossible since
            //  we split on a forward slash,  but we'll check just the same.
            if (Regex.IsMatch(segment, @"(%\\)|(%/)", Utilities.RegexOptionsCompiled | RegexOptions.IgnoreCase))
            {
                reason = OpenPackagingNonConformanceReason.SegmentHasPercentEncodedSlashCharacters;
				return false;
            }

            //  M1.8: A segment shall not contain percent-encoded unreserved characters.
            //  (See RFC2396, section 2.3)
            //
            //  Note: Octets must be allowed (the open packing conventions document specifically
            //  shows a URI with percent-encoded octets as a valid URI) so I have to assume that
            //  by "shall not contain percent-encoded unreserved characters" they mean that there
            //  cannot be a SINGLE unreserved character with a percent sign to the left of it.
            if (Regex.IsMatch(segment, @"(%[\-\._~!*'\(\)0-9A-Z]{1})$", Utilities.RegexOptionsCompiled | RegexOptions.IgnoreCase))
            {
                reason = OpenPackagingNonConformanceReason.SegmentHasPercentEncodedUnreservedCharacters;
				return false;
            }

			//  M1.6: A segment shall not hold any characters other than pchar characters.
			Match pchars = PackageUtilities.ValidPCharsRegex.Match( segment );
			if ( pchars.Success == false || pchars.Length != segment.Length )
			{
                reason = OpenPackagingNonConformanceReason.SegmentHasNonPCharCharacters;
				return false;
			}

            return true;
        }

            #endregion IsSegmentValid

            #region LogNonConformantPart

        static private void LogNonConformantPart( string partPath, OpenPackagingNonConformanceReason reason, Dictionary<string, OpenPackagingNonConformanceReason> nonConformantParts, Dictionary<string, IPackagePart> conformantParts )
        {
            if ( nonConformantParts == null )
                nonConformantParts = new Dictionary<string,OpenPackagingNonConformanceReason>();

            if ( nonConformantParts.ContainsKey(partPath) == false )
                nonConformantParts.Add( partPath, reason );
            else
                PackageConformanceManager.Assert( false, "LogNonConformantPart called more than once for the same path - unexpected." );

            //  Remove it from the conformant list
            if ( conformantParts != null && conformantParts.ContainsKey(partPath) )
                conformantParts.Remove( partPath );

        }
            #endregion LogNonConformantPart

            #region LogConformantPart






        static private void LogConformantPart( IPackagePart part, Dictionary<string, OpenPackagingNonConformanceReason> nonConformantParts, Dictionary<string, IPackagePart> conformantParts )
        {
            string partPath = part.Uri.ToString();

            if ( conformantParts == null )
                conformantParts = new Dictionary<string, IPackagePart>();

            if ( conformantParts.ContainsKey(partPath) == false )
                conformantParts.Add( partPath, part );

            //  Make sure it isn't in the non-conformant list
            if ( nonConformantParts != null && nonConformantParts.ContainsKey(partPath) )
                PackageConformanceManager.Assert( false, string.Format( "Part '{0}' is being logged as a conformant part, but it has also been logged as a non-conformant part - unexpected.", partPath) );

        }
            #endregion LogConformantPart

            #region VerifyPartNameEquivalence


#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)


        static internal bool VerifyPartNameEquivalence(
            string partName,
            Dictionary<string, OpenPackagingNonConformanceReason> nonConformantParts,
            Dictionary<string, IPackagePart> conformantParts )
        {
            //  First, test M1.12, since that check is less expensive
            //  Note that we throw an exception if a duplicate name is encountered.
            if ( conformantParts.ContainsKey(partName) )
            {
                string message = PackageConformanceManager.GetExceptionMessage(OpenPackagingNonConformanceExceptionReason.DuplicatePartName); 
                throw new OpenPackagingNonConformanceException( message, null, OpenPackagingNonConformanceExceptionReason.DuplicatePartName );
            }

            int pathLength = partName.Length;
            string nonConformantPartName = null;
            bool retVal = true;

            foreach ( KeyValuePair<string, IPackagePart> pair in conformantParts )
            {
                string existingPath = pair.Key;
                int existingLength = existingPath.Length;

                //  Since we checked for an exact match above (for section M1.12), we
                //  can skip this iteration  if the string lengths are the same.
                if ( pathLength == existingLength )
                    continue;

                //  Get the shorter of the two lengths
                int length = Math.Min( existingLength, pathLength );

                //  Normalize each string so they are the same length
                string s1 = existingLength > length ? existingPath.Substring(0, length) : existingPath;
                string s2 = pathLength > length ? partName.Substring(0, length) : partName;

                //  Compare the longer one's substring to the shorter string
                bool equal = string.Equals( s1, s2, StringComparison.InvariantCultureIgnoreCase );

                //  If they match, add the LONGER one to the non-conformant list.
                //  Also, return false if the new one is the non-conformant one.
                if ( equal )
                {
                    if ( existingLength > length )
                        nonConformantPartName = existingPath;
                    else
                    {
                        nonConformantPartName = partName;
                        retVal = false;
                    }

                    break;
                }
            }

            //  Log the non-conformant part if there is one. Note that this can be
            //  either the new one or an existing one.
            if ( nonConformantPartName != null )
            {
                retVal = false;
                PackageConformanceManager.LogNonConformantPart( nonConformantPartName, OpenPackagingNonConformanceReason.NameDerivesFromExistingPartName, nonConformantParts, conformantParts );
            }

            return retVal;
        }
            #endregion VerifyPartNameEquivalence

            #region VerifyContentType
        static public bool VerifyContentType( string partPath, string contentType, Dictionary<string, OpenPackagingNonConformanceReason> nonConformantParts, Dictionary<string, IPackagePart> conformantParts )
        {
            //  Check for leading/trailing whitespace
            if ( contentType[0] == ' ' || contentType[contentType.Length - 1] == ' ' )
            {
                PackageConformanceManager.LogNonConformantPart( partPath, OpenPackagingNonConformanceReason.ContentTypeHasInvalidWhitespace, nonConformantParts, conformantParts );
                return false;
            }

            //  Make sure there is a type and subtype, and no superfluous forward slashes [M1.13]
            string[] typeAndSubtype = contentType.Split(PackageConformanceManager.ForwardSlashChar);

            if ( typeAndSubtype.Length != 2 ||
                 typeAndSubtype[0].Length == 0 || typeAndSubtype[1].Length == 0 )
            {
                PackageConformanceManager.LogNonConformantPart( partPath, OpenPackagingNonConformanceReason.ContentTypeHasInvalidSyntax, nonConformantParts, conformantParts );
                return false;
            }

            //  Check for whitespace between the type and subtype [M1.14]
            if ( typeAndSubtype[0][typeAndSubtype.Length - 1] == ' ' || typeAndSubtype[1][0] == ' ' )
            {
                PackageConformanceManager.LogNonConformantPart( partPath, OpenPackagingNonConformanceReason.ContentTypeHasInvalidSyntax, nonConformantParts, conformantParts );
                return false;
            }

            //  Split the subtype on a semicolon and assume that each subsequent entry
            //  is an atribute name/value pair.
            string[] attributes = typeAndSubtype[1].Split( PackageConformanceManager.SemiColonChar );

            #region [M1.22] (...shall not create content types with parameters...)
            //  [M1.22] (see Annex F)
            //
            //  "IPackage implementers and format designers shall not create content types
            //  with parameters for the package specific parts defined in this Open Packaging
            //  specification and shall treat the presence of parameters in these content types
            //  as an error."
            //
            //  Since (according to RFC2616 3.7) the structure of the content type is:
            //
            //      media-type = type "/" subtype *( ";" parameter )
            //
            //  The presence of a semicolon implies the presence of an attribute, so we can
            //  simply check for the presence of a semicolon in the string.

            //  Determine whether this is one of the "package specific" content types
            List<string> packageSpecificContentTypes = PackageConformanceManager.PackageSpecificContentTypes;
            bool isPackageSpecificContentType = false;
            for ( int i = 0; i < packageSpecificContentTypes.Count; i ++ )
            {
                if ( contentType.StartsWith(packageSpecificContentTypes[i]) )
                {
                    isPackageSpecificContentType = true;
                    break;
                }
            }

            //  If it is a "package specific" content type, and there was a semicolon,
            //  log the error and return.
            if ( isPackageSpecificContentType && attributes.Length > 1 )
            {
                PackageConformanceManager.LogNonConformantPart( partPath, OpenPackagingNonConformanceReason.ContentTypeHasParameters, nonConformantParts, conformantParts );
                return false;
            }

            #endregion [M1.22] (...shall not create content types with parameters...)

            #region [M1.14], [M1.15]
            //  If one of these strings is not an attribute/value pair (i.e., has no equal sign),
            //  we will interpret that as a "comment", which violates M1.15.
            
            

            if ( attributes.Length > 1 )
            {
                for ( int i = 1; i < attributes.Length; i ++ )
                {
                    string attribute = attributes[i];

                    //  If it has no equal sign, assume it is a "comment" [M1.15].
                    if ( attribute.IndexOf(PackageConformanceManager.EqualsChar) < 0 )
                    {
                        PackageConformanceManager.LogNonConformantPart( partPath, OpenPackagingNonConformanceReason.ContentTypeHasComments, nonConformantParts, conformantParts );
                        return false;
                    }

                    //  If it has an equal sign, make sure there is no whitespace around it [M1.14].
                    if ( Regex.IsMatch(attribute, PackageConformanceManager.RegexPatternEqualSignWithWhiteSpace) )
                    {
                        PackageConformanceManager.LogNonConformantPart( partPath, OpenPackagingNonConformanceReason.ContentTypeHasInvalidWhitespace, nonConformantParts, conformantParts );
                        return false;
                    }
                }
            }
            #endregion [M1.14], [M1.15]

            return true;
        }
            #endregion VerifyContentType

            #region VerifyXmlContent

        static private OpenPackagingNonConformanceReason VerifyXmlContent( string partPath, IPackagePart part, ref bool corePropertiesPartProcessed )
        {

            string message = string.Empty;

			// MD 9/2/08
			// Bail out for known types which are not XML
			switch ( part.ContentType )
			{
				case PackageConformanceManager.PrinterSettingsContentTypeValue:
				case GifImagePart.ContentTypeValue:
				case JpegImagePart.ContentTypeValue:
				case PngImagePart.ContentTypeValue:
				case TiffImagePart.ContentTypeValue:
					return OpenPackagingNonConformanceReason.Conformant;
			}

            //  Flag whether this is the core properties document, which has
            //  its own set of schema validation rules.
            bool isCoreProperties = string.Equals(part.ContentType, CorePropertiesPart.ContentTypeValue, StringComparison.InvariantCultureIgnoreCase);

            #region [M4.1] The format designer shall specify and the format producer shall create at most one core properties relationship for a package...
            if ( isCoreProperties )
            {
                //  Throw an exception if the core properties relationship has already been processed.
                if ( corePropertiesPartProcessed )
                {
                    message = PackageConformanceManager.GetExceptionMessage(OpenPackagingNonConformanceExceptionReason.CorePropertiesRelationshipAlreadyProcessed); 
                    throw new OpenPackagingNonConformanceException( message, part, OpenPackagingNonConformanceExceptionReason.CorePropertiesRelationshipAlreadyProcessed );
                }

                //  Set the flag that indicates the core properties relationship has already been processed.
                corePropertiesPartProcessed = true;
            }
            #endregion [M4.1] The format designer shall specify and the format producer shall create at most one core properties relationship for a package...

			using ( Stream partStream = part.GetStream(FileMode.Open, FileAccess.Read) )
			{
                bool hasSchema = false;
                XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();

				// MD 9/12/08 - 8.3 Performance
				// Use a custom xml resolver so first chance exceptions are not thrown when the default logic tried to
				// find related xsd files.
				XmlEmbeddedResourceResolver xmlResolver = new XmlEmbeddedResourceResolver();
				xmlReaderSettings.XmlResolver = xmlResolver;
				xmlReaderSettings.Schemas.XmlResolver = xmlResolver;

                //  The reason I am setting ProhibitDtd to false is because when it is true and
                //  DTD is encountered, an XmlException is thrown, but as far as I could
                //  tell there is no way to know that it was because DTD was processed,
                //  so the exception is useless in that regard. With each iteration, I check
                //  the NodeType property, and if it is DocumentType, we throw our own exception.
                //  (see M1.18])

                xmlReaderSettings.DtdProcessing = DtdProcessing.Parse;




                #region [M1.20], [M1.21] XML content shall be valid against the corresponding XSD schema...

				//  Get the corresponding schema for the XML content...note that we don't have to
                //  bother with this for the core properties relationship.
                if ( isCoreProperties == false )
                {
                    string xsdResourceName = PackageConformanceManager.SchemaResourceNameFromContentType( part.ContentType );

					// MD 11/19/10 - TFS49853
					// Moved this code to the AddSchema helper method so it could be used in multiple places.
					//if ( String.IsNullOrEmpty( xsdResourceName ) == false )
					//{
					//    hasSchema = true;
					//    xmlReaderSettings.ValidationType = ValidationType.Schema;
					//
					//    string resolvedResourceName = XmlEmbeddedResourceResolver.ResolveXSDResourcePath( xsdResourceName );
					//    XmlSchema schema = PackageConformanceManager.GetSchema( resolvedResourceName );
					//    xmlReaderSettings.Schemas.Add( schema );
					//}
					PackageConformanceManager.AddSchema(xmlReaderSettings, xsdResourceName, ref hasSchema);

					// MD 11/19/10 - TFS49853
					// The drawing part allows any content to be in the graphicData element. If it is unknown, it is most likely
					// a chart, because that is not linked by the normal xsd file, so add the schema file for charting as well.
					if (part.ContentType == DrawingsPart.ContentTypeValue)
						PackageConformanceManager.AddSchema(xmlReaderSettings, "dml-chart", ref hasSchema);
                }
                #endregion [M1.20], [M1.21] XML content shall be valid against the corresponding XSD schema...

                #region TFS34157
                //
                //  BF 10/14/10 TFS34157
                //  This handler is called when the XmlReader throws an exception.
                //  If the exception was thrown because the "mc:Ignorable" attribute
                //  was encountered, store the ignorable namespaces and allow execution
                //  to continue. If the exception was thrown because an element/attribute
                //  was not recognized, but its prefix is one of the ignorable ones, allow
                //  execution to continue. For all other failed validations with a severity
                //  of 'Error', throw the exception so the appropriate error code is returned.
                //
                xmlReaderSettings.ValidationType = ValidationType.Schema;
                Dictionary<string, object> ignorablePrefixes = null;

                ValidationEventHandler validationHandler =
                delegate( object sender, ValidationEventArgs args )
                {
                    XmlReader xmlReader = sender as XmlReader;
                    if ( xmlReader != null )
                    {
                        // MD 11/19/10 - TFS49853
						// We can ignore anything in the markup compatibility namespace. This is defined in the ECMA standard as a way to 
						// mark elements that may not be supported by all versions of Office application.
						//if ( xmlReader.NodeType == XmlNodeType.Attribute &&
						//     xmlReader.LocalName == "Ignorable" &&
						//     xmlReader.NamespaceURI.Equals(PackageConformanceManager.MarkupCompatibilityNamespace, StringComparison.InvariantCultureIgnoreCase) )
						//    PackageConformanceManager.AddPrefix( ref ignorablePrefixes, xmlReader.Value );
						if (xmlReader.NamespaceURI.Equals(PackageConformanceManager.MarkupCompatibilityNamespace, StringComparison.InvariantCultureIgnoreCase))
						{
							if (xmlReader.NodeType == XmlNodeType.Attribute && xmlReader.LocalName == "Ignorable")
								PackageConformanceManager.AddPrefix(ref ignorablePrefixes, xmlReader.Value);
						}
                        else
                        {
                            if ( (args.Exception is XmlSchemaValidationException) == false ||
                                 ignorablePrefixes == null ||
                                 ignorablePrefixes.ContainsKey(xmlReader.Prefix) == false )
                            {
                                if ( args.Severity == XmlSeverityType.Error )
                                    throw args.Exception;
                            }
                        }
                    }
                };
                xmlReaderSettings.ValidationEventHandler += validationHandler;
                
                #endregion TFS34157

                using ( XmlTextReader xmlTextReader = new XmlTextReader(partStream) )
                using ( XmlReader xmlReader = XmlReader.Create(xmlTextReader, xmlReaderSettings) )
                {
                    //  Read the first line so we can get the encoding method...if an exception
                    try { xmlReader.Read(); }
                    catch(Exception ex)
                    {
                        //  I'm not sure if this will ever happen, but if the exception is
                        //  an XmlSchemaValidationException, return that code, otherwise we
                        //  assume that this is not XML, in which case it is conformant.
                        if ( ex is XmlSchemaValidationException )
                            return OpenPackagingNonConformanceReason.XmlContentInvalidForSchema;
                        else
                            return OpenPackagingNonConformanceReason.Conformant; 
                    }

                    #region [M1.17] (UTF-8 | UTF-16 encoding only)
                    Encoding encoding = xmlTextReader.Encoding;
                    string encodingName = encoding != null ? encoding.EncodingName : string.Empty;
                    
                    if ( encodingName.Length > 0 &&
                         string.Equals(encodingName, Encoding.UTF8.EncodingName) == false &&
						// MD 10/8/10
						// Found while fixing TFS44359
						// UTF-16 is represented by the Unicode encoding, not the BigEndianUnicode encoding.
                         //string.Equals(encodingName, Encoding.BigEndianUnicode.EncodingName) == false )
						string.Equals(encodingName, Encoding.Unicode.EncodingName) == false)
                    {
                        return OpenPackagingNonConformanceReason.XmlEncodingUnsupported;
                    }
                    #endregion [M1.17] (UTF-8 | UTF-16 encoding only)

                    while ( xmlReader.EOF == false )
                    {
                        try { xmlReader.Read(); }
                        catch( XmlSchemaValidationException ) 
                        {
                            return OpenPackagingNonConformanceReason.XmlContentInvalidForSchema;
                        } 
                           
                        XmlNodeType nodeType = xmlReader.NodeType;

                        string namespaceURI = xmlReader.NamespaceURI;

                        #region [M1.18] (No DTD processing)

                        if ( nodeType == XmlNodeType.DocumentType )
                        {
                            message = PackageConformanceManager.GetExceptionMessage(OpenPackagingNonConformanceExceptionReason.XmlContainsDocumentTypeDefinition);
                            throw new OpenPackagingNonConformanceException( message, part, OpenPackagingNonConformanceExceptionReason.XmlContainsDocumentTypeDefinition );

                        }
                        #endregion [M1.18] (No DTD processing)

                        
                        
                        if ( nodeType == XmlNodeType.EndElement ||
                             nodeType == XmlNodeType.Whitespace ||
                             nodeType == XmlNodeType.EndEntity )
                            continue;

                        #region [M1.19], [M4.2] (Markup Compatibility Namespace)
                        //
                        //  Noe that We are not supporting [M1.19], at least not now.
                        //  [M1.19]:
                        //  If the XML content contains the Markup Compatibility namespace,
                        //  as described in Part 5: “Markup Compatibility and Extensibility”,
                        //  it shall be processed by the package implementer to remove Markup
                        //  Compatibility elements and attributes, ignorable namespace declarations,
                        //  and ignored elements and attributes before applying subsequent validation
                        //  rules.
                        if ( string.IsNullOrEmpty(namespaceURI) == false &&
                             string.Equals(namespaceURI, PackageConformanceManager.MarkupCompatibilityNamespace) )
                        {
                            //  [M4.2] - No Markup Compatibility namespace for core properties part
                            if ( isCoreProperties )
                            {
                                message = PackageConformanceManager.GetExceptionMessage(OpenPackagingNonConformanceExceptionReason.UsesMarkupCompatibilityNamespace); 
                                throw new OpenPackagingNonConformanceException( message, part, OpenPackagingNonConformanceExceptionReason.UsesMarkupCompatibilityNamespace );
                            }

                            //  Since we don't even expect to hit any of these, assert if we do.
							// MD 10/31/11
							// Found while fixing TFS90733
							// According to rule [M1.19] (listed above), the validation rules do not apply to the elements and attributes
							// in the Markup Compatibility namespace, but they are allowed.
                            //PackageConformanceManager.Assert( false, "An XMLElement with the Markup Compatibility namespace was encountered" );
                        }

                        #endregion [M1.19], [M4.2] (Markup Compatibility Namespace)

                        #region [M1.21] XML content shall not contain elements or attributes drawn from “xml” or “xsi” namespaces...
                        if ( isCoreProperties == false )
                        {
                            if ( string.IsNullOrEmpty(namespaceURI) == false )
                            {
                                bool hasXmlNamespaceName = string.Equals(namespaceURI, PackageConformanceManager.XmlNamespaceName);
                                bool hasXsiNamespaceName = string.Equals(namespaceURI, PackageConformanceManager.XsiNamespaceName);

								if ( ( hasXmlNamespaceName && xmlResolver.SchemaHasXmlNamespace == false ) ||
									 ( hasXsiNamespaceName && xmlResolver.SchemaHasXsiNamespace == false ) )
                                {
                                    return OpenPackagingNonConformanceReason.XmlContentDrawsOnUndefinedNamespace;
                                }
                            }

                            //  Check the attributes for the XSI/XML namespaces, but only if
                            //  there is no schema...if there is, the schema validation layer
                            //  will catch this.
							// MD 10/8/10
							// Found while fixing TFS44359
							// It appears that this restriction does not apply to custom Xml files.
                            //if ( hasSchema == false && xmlReader.HasAttributes )
							if (hasSchema == false && xmlReader.HasAttributes && part.ContentType != CustomXmlPart.ContentTypeValue)
                            {
                                for ( int i = 0, count = xmlReader.AttributeCount; i < count; i ++ )
                                {
                                    string attributeValue = xmlReader.GetAttribute(i);

                                    if ( string.Equals(attributeValue, PackageConformanceManager.XmlNamespaceName, StringComparison.InvariantCultureIgnoreCase) ||
                                         string.Equals(attributeValue, PackageConformanceManager.XsiNamespaceName, StringComparison.InvariantCultureIgnoreCase) )
                                        return OpenPackagingNonConformanceReason.XmlContentDrawsOnUndefinedNamespace;
                                }
                            }

                        }

                        #endregion [M1.21] XML content shall not contain elements or attributes drawn from “xml” or “xsi” namespaces...

                        #region [M4.3] thru [M4.5] (Core properties related)
                        if ( isCoreProperties )
                        {
                            if ( string.Equals(namespaceURI, PackageConformanceManager.DcTermsNamespaceName, StringComparison.InvariantCultureIgnoreCase) )
                            {
                                OpenPackagingNonConformanceExceptionReason exceptionReason = OpenPackagingNonConformanceExceptionReason.None;
                                bool isCreatedElement = string.Equals(xmlReader.LocalName, "created", StringComparison.InvariantCultureIgnoreCase);
                                bool isModifiedElement = string.Equals(xmlReader.LocalName, "modified", StringComparison.InvariantCultureIgnoreCase);

                                //  [M4.3] - No Dublin Core elements except 'created' and 'modified'
                                if ( isCreatedElement == false && isModifiedElement == false )
                                    exceptionReason = OpenPackagingNonConformanceExceptionReason.ContainsDublinCoreRefinements;
                                else
                                //  [M4.4], [M4.5]
                                if ( xmlReader.HasAttributes )
                                {
                                    //  [M4.4] - No xml:lang attribute
                                    string xmlLangAttribute = xmlReader.GetAttribute( "lang", PackageConformanceManager.XmlNamespaceName );
                                    if ( string.IsNullOrEmpty(xmlLangAttribute) == false )
                                        exceptionReason = OpenPackagingNonConformanceExceptionReason.ContainsXmlLanguageAttribute;
                                    else
                                    {
                                        //  [M4.5] - No xsi:type attribute except for dcterms:created and dcterms:modified,
                                        //  for which it is required.

                                        string xsiTypeAttribute = xmlReader.GetAttribute( "type", PackageConformanceManager.XsiNamespaceName );

                                        if ( string.IsNullOrEmpty(xsiTypeAttribute) )
                                        {
                                            if ( isCreatedElement == false && isModifiedElement == false )
                                                exceptionReason = OpenPackagingNonConformanceExceptionReason.XsiTypeAttributeInvalid;
                                        }
                                        else
                                        {
                                            if ( isCreatedElement || isModifiedElement )
                                            {
                                                string expectedValue = string.Format( "{0}{1}{2}", xmlReader.Prefix, ":", "W3CDTF" );
                                                if ( string.Equals(xsiTypeAttribute, expectedValue) == false )
                                                    exceptionReason = OpenPackagingNonConformanceExceptionReason.XsiTypeAttributeInvalid;
                                            }
                                        }
                                    }

                                }

                                if ( exceptionReason != OpenPackagingNonConformanceExceptionReason.None )
                                {
                                    message = PackageConformanceManager.GetExceptionMessage( exceptionReason );
                                    throw new OpenPackagingNonConformanceException( message, part, exceptionReason );
                                }
                            }
                        }
                        #endregion [M4.1] thru [M4.5] (Core properties related)
                    }
                }
			}

            return OpenPackagingNonConformanceReason.Conformant;
        }
            #endregion VerifyXmlContent

            #region SchemaResourceNameFromContentType





        private static string SchemaResourceNameFromContentType( string contentType )
        {
            switch ( contentType )
            {
                #region Known to not have any associated schema
                
                case ThemePart.ContentTypeValue:
                case PackageConformanceManager.PrinterSettingsContentTypeValue:
                case CorePropertiesPart.ContentTypeValue:
                case JpegImagePart.ContentTypeValue:
                case PngImagePart.ContentTypeValue:
                case GifImagePart.ContentTypeValue:
                case TiffImagePart.ContentTypeValue:
				// MD 10/1/08 - TFS8453
				case VBAProjectPart.ContentTypeValue:
                    break;

                #endregion Known to not have any associated schema

                //  application/vnd.openxmlformats-officedocument.extended-properties+xml
                case ExtendedPropertiesPart.ContentTypeValue:
                    return "shared-documentPropertiesExtended";

                //  application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml
                case WorkbookPart.ContentTypeValue:
                    return "sml-workbook";

				// MD 10/1/08 - TFS8453
				//  application/vnd.ms-excel.sheet.macroEnabled.main+xml
				case MacroEnabledWorkbookPart.ContentTypeValue:
					return "sml-workbook";

				// MD 5/7/10 - 10.2 - Excel Templates
				//  application/vnd.ms-excel.template.macroEnabled.main+xml
				case MacroEnabledTemplatePart.ContentTypeValue:
					return "sml-workbook";

				// MD 5/7/10 - 10.2 - Excel Templates
				//  application/vnd.openxmlformats-officedocument.spreadsheetml.template.main+xml
				case TemplatePart.ContentTypeValue:
					return "sml-workbook";

                //  application/vnd.openxmlformats-officedocument.spreadsheetml.calcChain+xml
                case CalculationChainPart.ContentTypeValue:
                    return "sml-calculationChain";

                //  application/vnd.openxmlformats-officedocument.spreadsheetml.externalLink+xml
                case ExternalWorkbookPart.ContentTypeValue:
                    return "sml-supplementaryWorkbooks";

                //  application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml
                case WorksheetPart.ContentTypeValue:
                    return "sml-sheet";

                //  application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml
                case WorkbookStylesPart.ContentTypeValue:
                    return "sml-styles";

                //  application/vnd.openxmlformats-officedocument.spreadsheetml.sharedStrings+xml
                case SharedStringTablePart.ContentTypeValue:
                    return "sml-sharedStringTable";

                //  application/vnd.openxmlformats-officedocument.drawing+xml
                case DrawingsPart.ContentTypeValue:
                    return "dml-spreadsheetDrawing";

				// MD 9/10/08 - Cell Comments
				// application/vnd.openxmlformats-officedocument.spreadsheetml.comments+xml
				case CommentsPart.ContentTypeValue:
					return "sml-comments";

				// application/vnd.openxmlformats-officedocument.vmlDrawing
				case LegacyDrawingsPart.ContentTypeValue:
					return "vml-spreadsheetDrawing";

				// MD 10/8/10
				// Found while fixing TFS44359
				// Added support to round-trip custom Xml parts.
				// application/vnd.openxmlformats-officedocument.customXmlProperties+xml
				case CustomXmlPropertiesPart.ContentTypeValue:
					return "shared-customXmlDataProperties";

				// There is no scema for regualr xml files.
				// application/xml
				case CustomXmlPart.ContentTypeValue:
					break;

				// MD 10/12/10 - TFS49853
				// application/vnd.openxmlformats-officedocument.drawingml.chart+xml
				case ChartPart.ContentTypeValue:
					return "dml-chart";

				// MD 12/6/11 - 12.1 - Table Support
				// application/vnd.openxmlformats-officedocument.spreadsheetml.table+xml
				case TablePart.ContentTypeValue:
					return "sml-table";

                // MBS 9/10/08 
                // There are some types that we don't support that aren't worth creating a new 
                // part for, so we'll just have a default handler here for those elements
                // that we are aware of so we don't have to see the assert every time
                case "application/vnd.openxmlformats-officedocument.custom-properties+xml":
				case "application/vnd.ms-excel.controlproperties+xml": // MD 10/31/11 - TFS90733
                    break;

                default:
                    PackageConformanceManager.Assert( false, string.Format("Unrecognized contentType in SchemaResourceNameFromContentType:{0}{1}", Environment.NewLine, contentType) );
                    break;
            }

            return null;
        }
            #endregion SchemaResourceNameFromContentType

			#region ClearCachedSchemas

		public static void ClearCachedSchemas()
		{
			if ( PackageConformanceManager.cachedSchemas == null )
				return;

			PackageConformanceManager.cachedSchemas.Clear();
		} 

			#endregion ClearCachedSchemas

            #region GetSchema

		public static XmlSchema GetSchema( string schemaResourceName )
		{

			if ( PackageConformanceManager.cachedSchemas == null )
				PackageConformanceManager.cachedSchemas = new Dictionary<string, XmlSchema>();

			XmlSchema cachedSchema;
			if ( PackageConformanceManager.cachedSchemas.TryGetValue( schemaResourceName, out cachedSchema ) )
				return cachedSchema;

			using ( Stream stream = typeof( PackageConformanceManager ).Assembly.GetManifestResourceStream( schemaResourceName ) )
			{
				using ( GZipStream gzs = new GZipStream( stream, CompressionMode.Decompress ) )
				{
					XmlSchema schema = XmlSchema.Read( gzs, null );
					PackageConformanceManager.cachedSchemas.Add( schemaResourceName, schema );

					return schema;
				}
			}



		}

            #endregion GetSchema

            #region GetExceptionMessage
        static private string GetExceptionMessage( OpenPackagingNonConformanceExceptionReason exceptionReason )
        {
            string resourceName = string.Format("LE_OpenPackagingNonConformanceException_{0}", exceptionReason);
            return SR.GetString( resourceName );
        }
            #endregion GetExceptionMessage

            #region Assert
        static internal void Assert( bool condition, string message )
        {




        }
            #endregion Assert

            #region MakeExempt
        /// <summary>
        /// Marks each member of the specified OpenPackagingNonConformanceReason list as
        /// exempt from conformance verification.
        /// </summary>
        /// <param name="reasons">A generic list of the OpenPackagingNonConformanceReason constants to mark as exempt.</param>
        public void MakeExempt( List<OpenPackagingNonConformanceReason> reasons )
        {
            if ( this.exemptions == null )
                this.exemptions = new Dictionary<OpenPackagingNonConformanceReason,object>();

            foreach ( OpenPackagingNonConformanceReason reason in reasons )
            {
                if ( this.exemptions.ContainsKey(reason) == false )
                    this.exemptions.Add( reason, null );
            }
        }

        /// <summary>
        /// Marks the specified OpenPackagingNonConformanceReason as exempt from
        /// conformance verification.
        /// </summary>
        /// <param name="reason">The OpenPackagingNonConformanceReason to mark as exempt.</param>
        public void MakeExempt( OpenPackagingNonConformanceReason reason )
        {
            List<OpenPackagingNonConformanceReason> reasons = new List<OpenPackagingNonConformanceReason>(1);
            reasons.Add( reason );
            this.MakeExempt( reasons );
        }
            #endregion MakeExempt

        //  BF 10/4/10  TFS34157
            #region AddPrefix/ContainsPrefix
        static private void AddPrefix( ref Dictionary<string, object> prefixes, string value )
        {
            if ( string.IsNullOrEmpty(value) )
                return;

            if ( prefixes == null )
                prefixes = new Dictionary<string,object>( StringComparer.InvariantCultureIgnoreCase );

            char[] whitespace = new char[]{ (char)32 }; //  (space character)
            string[] split = value.Split( whitespace, StringSplitOptions.RemoveEmptyEntries );
            if ( split == null || split.Length == 0 )
                return;

            foreach( string prefix in split )
            {
                if ( prefixes.ContainsKey(prefix) == false )
                    prefixes.Add( prefix, null );
            }
        }

        static private bool ContainsPrefix( Dictionary<string, object> prefixes, string value )
        {
            if ( prefixes == null )
                return false;

            if ( prefixes.ContainsKey(value) )
                return true;

            return false;
        }
            #endregion AddPrefix/ContainsPrefix


		// MD 11/19/10 - TFS49853
		// Moved this code from the VerifyXmlContent method so it could be used in multiple places.
		#region AddSchema

		private static void AddSchema(XmlReaderSettings xmlReaderSettings, string xsdResourceName, ref bool hasSchema)
		{
			if (String.IsNullOrEmpty(xsdResourceName))
				return;

			hasSchema = true;
			xmlReaderSettings.ValidationType = ValidationType.Schema;

			string resolvedResourceName = XmlEmbeddedResourceResolver.ResolveXSDResourcePath(xsdResourceName);
			XmlSchema schema = PackageConformanceManager.GetSchema(resolvedResourceName);
			xmlReaderSettings.Schemas.Add(schema);
		}

		#endregion // AddSchema 


        #endregion Methods

        #region Properties

            #region ConformantParts


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

        private Dictionary<string, IPackagePart> ConformantParts
        {
            get
            {
                if ( this.conformantParts == null )
                    this.conformantParts = new Dictionary<string, IPackagePart>( StringComparer.InvariantCultureIgnoreCase );

                return this.conformantParts;
            }
        }
            #endregion ConformantParts

            #region NonConformantParts






        private Dictionary<string, OpenPackagingNonConformanceReason> NonConformantParts
        {
            get
            {
                if ( this.nonConformantParts == null )
                    this.nonConformantParts = new Dictionary<string, OpenPackagingNonConformanceReason>();

                return this.nonConformantParts;
            }
        }
            #endregion NonConformantParts

            #region PackageSpecificContentTypes
        private static List<string> PackageSpecificContentTypes
        {
            get
            {
                if ( PackageConformanceManager.packageSpecificContentTypes == null )
                {
                    PackageConformanceManager.packageSpecificContentTypes =
                        new List<string>(5);

                    PackageConformanceManager.packageSpecificContentTypes.Add(PackageConformanceManager.CorePropertiesPartContentTypeValue);
                    PackageConformanceManager.packageSpecificContentTypes.Add(PackageConformanceManager.DigitalSignatureCertficatePartContentTypeValue);
                    PackageConformanceManager.packageSpecificContentTypes.Add(PackageConformanceManager.DigitalSignatureOriginPartContentTypeValue);
                    PackageConformanceManager.packageSpecificContentTypes.Add(PackageConformanceManager.DigitalSignatureXMLSignaturePartContentTypeValue);
                    PackageConformanceManager.packageSpecificContentTypes.Add(PackageConformanceManager.RelationshipsPartContentTypeValue);
                }

                return PackageConformanceManager.packageSpecificContentTypes;
            }
        }
            #endregion PackageSpecificContentTypes

            #region TimeToVerify (DEBUG mode only)






            #endregion TimeToVerify (DEBUG mode only)

        #endregion Properties
    }
    #endregion PackageConformanceManager class

    #region OpenPackagingNonConformanceException class
    /// <summary>
    /// Thrown when a SpreadsheetMLpackage is found to be non-conformant.
    /// </summary>



	public

		 class OpenPackagingNonConformanceException : Exception
    {
        #region Member variables
        
        private IPackagePart packagePart = null;
        private OpenPackagingNonConformanceExceptionReason reason = OpenPackagingNonConformanceExceptionReason.None;
        
        #endregion Member variables

        #region Constructor (internal)
        internal OpenPackagingNonConformanceException(
            string message,
            IPackagePart packagePart,
            OpenPackagingNonConformanceExceptionReason reason ) : base( message )
        {
            this.packagePart = packagePart;
            this.reason = reason;
        }
        #endregion Constructor (internal)

        #region Properties

            #region IPackagePart
        /// <summary>
        /// Returns the <a href="http://msdn.microsoft.com/en-us/library/system.io.packaging.packagepart.aspx">IPackagePart</a>
        /// instance which caused the exception to be thrown, or null (Nothing in VB) if the exception was not caused by
        /// a IPackagePart.
        /// </summary>
        public IPackagePart IPackagePart { get { return this.packagePart; } }
            #endregion IPackagePart

            #region Reason
        /// <summary>
        /// Returns the <see cref="OpenPackagingNonConformanceExceptionReason">reason</see> the exception was thrown.
        /// </summary>
        public OpenPackagingNonConformanceExceptionReason Reason { get { return this.reason; } }
            #endregion Reason
        
        #endregion Properties
    }
    #endregion OpenPackagingNonConformanceException class

	#region XmlEmbeddedResourceResolver class

	internal class XmlEmbeddedResourceResolver : XmlResolver
	{
		#region Member Variables

		private bool schemaHasXmlNamespace;
		private bool schemaHasXsiNamespace; 

		#endregion Member Variables

		#region Base Class Overrides

		public override object GetEntity( Uri absoluteUri, string role, Type ofObjectToReturn )
		{
			XmlSchema schema = PackageConformanceManager.GetSchema( absoluteUri.OriginalString );


			if ( this.schemaHasXmlNamespace == false || this.schemaHasXsiNamespace == false )
			{
				//  Determine whether any of the schemas contain declarations
				//  for the XML or XSI namespaces.
				XmlQualifiedName[] namespaces = schema.Namespaces.ToArray();

				for ( int i = 0; i < namespaces.Length; i++ )
				{
					string ns = namespaces[ i ].Namespace;

					if ( string.Equals( ns, PackageConformanceManager.XmlNamespaceName ) )
						schemaHasXmlNamespace = true;

					if ( string.Equals( ns, PackageConformanceManager.XsiNamespaceName ) )
						schemaHasXsiNamespace = true;

					if ( schemaHasXmlNamespace && schemaHasXsiNamespace )
						break;
				}
			}

			return schema;
		}

		public override Uri ResolveUri( Uri baseUri, string relativeUri )
		{
			return new Uri( XmlEmbeddedResourceResolver.ResolveXSDResourcePath( relativeUri ), UriKind.RelativeOrAbsolute );
		}


		public override System.Net.ICredentials Credentials
		{
			set { }
		} 

		#endregion Base Class Overrides

		#region ResolveXSDResourcePath

		public static string ResolveXSDResourcePath( string xsdBaseName )
		{
			if ( xsdBaseName.EndsWith( ".xsd" ) == false )
				xsdBaseName += ".xsd";

			return PackageConformanceManager.XsdResourceLocation + xsdBaseName + ".gz";
		} 

		#endregion ResolveXSDResourcePath

		#region Properties

		public bool SchemaHasXmlNamespace
		{
			get { return this.schemaHasXmlNamespace; }
		}

		public bool SchemaHasXsiNamespace
		{
			get { return this.schemaHasXsiNamespace; }
		} 

		#endregion Properties
	} 

	#endregion XmlEmbeddedResourceResolver class
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