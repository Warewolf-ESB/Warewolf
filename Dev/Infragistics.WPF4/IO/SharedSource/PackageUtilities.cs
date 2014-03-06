using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Globalization;
using System.IO;

//  BF 12/15/10
#region Obsolete
//#if SILVERLIGHT
//namespace Infragistics.Documents.Excel.Serialization.Excel2007
//#else
//namespace Infragistics.Documents.Excel.Serialization.Excel2007
//#endif
#endregion Obsolete


namespace Infragistics.Documents.Core.Packaging


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

{
    #region PackageUtilities class
    internal static class PackageUtilities
	{
		#region Constants

		private const string ParentDirectoryValue = "..";
		internal const string RelationshipsPartsSubFolder = "_rels";
		internal const string RelationshipsPartsSuffix = ".rels";
		public const char SegmentSeparator = '/';

        //  BF 11/17/10 Infragistics.Documents.IO
        internal const string RelationshipIdPrefix = "rId";

		#endregion Constants

		#region Static Variables

		[ThreadStatic]
		private static Regex percentEncodedRegex;

		[ThreadStatic]
		private static Regex unreservedCharRegex; 

		[ThreadStatic]
		private static Regex validPCharsRegex; 

		[ThreadStatic]
		private static Regex nonDotRegex; 

        // MBS 9/11/08
        [ThreadStatic]
        private static Regex driveLetterRegex;

		#endregion Static Variables


		#region CombinePaths( string, string )

		public static string CombinePaths( string parentPath, string childPath )
		{
			// MD 7/14/10
			// Found while using TFS35800
			// We should really be using the constants here.
			//if ( childPath.StartsWith( "../" ) )
			if ( childPath.StartsWith( PackageUtilities.ParentDirectoryValue + PackageUtilities.SegmentSeparator ) )
			{
				return PackageUtilities.CombinePaths(
					PackageUtilities.GetDirectoryName( parentPath ),
					childPath.Substring( 3 ) );
			}

			bool parentHasTrailingSeparator = 
				parentPath.Length > 0 && 
				parentPath[ parentPath.Length - 1 ] == PackageUtilities.SegmentSeparator;

			bool childHasLeadingSeparator = 
				childPath.Length > 0 && 
				childPath[ 0 ] == PackageUtilities.SegmentSeparator;

			// MD 7/14/10 - TFS35800
			// Apparently, when the child path starts with a separator, it is an absolute path instead of a relative path, 
			// so just return it.
			if (childHasLeadingSeparator)
				return childPath;

			if ( parentHasTrailingSeparator ^ childHasLeadingSeparator )
				return parentPath + childPath;

			if ( parentHasTrailingSeparator )
				return parentPath + childPath.Substring( 1 );

			return parentPath + PackageUtilities.SegmentSeparator + childPath;
		}

		#endregion CombinePaths( string, string )

		#region CombinePaths( Uri, Uri )

		public static Uri CombinePaths( Uri parentPath, Uri childPath )
		{
			string parentPathValue = parentPath.ToString();
			string childPathValue = childPath.ToString();

			return new Uri( 
				PackageUtilities.CombinePaths( parentPathValue, childPathValue ), 
				UriKind.RelativeOrAbsolute );
		}

		#endregion CombinePaths( Uri, Uri )

		#region DateTimeToW3CDTFValue

		public static string DateTimeToW3CDTFValue( DateTime dateTime )
		{
			DateTime dateTimeUniversal = dateTime.ToUniversalTime();
			return dateTimeUniversal.ToString( "s" ) + "Z";
		}

		#endregion DateTimeToW3CDTFValue

		#region GetAssociatedRelationshipsPartPath

		public static string GetAssociatedRelationshipsPartPath( string sourcePartPath )
		{
			string directoryName = PackageUtilities.GetDirectoryName( sourcePartPath );
			string partName = PackageUtilities.GetPartName( sourcePartPath );

			string relationshipPartDirectory = PackageUtilities.CombinePaths( directoryName, PackageUtilities.RelationshipsPartsSubFolder );
			string relationshipPartName = partName + PackageUtilities.RelationshipsPartsSuffix;

			return PackageUtilities.CombinePaths( relationshipPartDirectory, relationshipPartName );
		} 

		#endregion GetAssociatedRelationshipsPartPath

		#region GetDirectoryName( string )

		public static string GetDirectoryName( string partPath )
		{
			int lastSeparatorIndex = partPath.LastIndexOf( PackageUtilities.SegmentSeparator );

			if ( lastSeparatorIndex < 0 )
				return partPath;

			return partPath.Substring( 0, lastSeparatorIndex );
		}

		#endregion GetDirectoryName( string )

		#region GetDirectoryName( Uri )

		public static Uri GetDirectoryName( Uri partPath )
		{
			return new Uri( PackageUtilities.GetDirectoryName( partPath.ToString() ), UriKind.RelativeOrAbsolute );
		} 

		#endregion GetDirectoryName( Uri )

		#region GetPartName

		public static string GetPartName( string partPath )
		{
			int lastSeparatorIndex = partPath.LastIndexOf( PackageUtilities.SegmentSeparator );

			if ( lastSeparatorIndex < 0 )
				return partPath;

			return partPath.Substring( lastSeparatorIndex + 1 );
		}

		#endregion GetPartName

		#region GetPercentEncodedValue

		internal static byte GetPercentEncodedValue( string percentEncoded )
		{
			if ( percentEncoded == null ||
				percentEncoded.Length != 3 ||
				percentEncoded[ 0 ] != '%' )
			{
				PackageUtilities.DebugFail( "The value passed to GetPercentEncodedValue was not expoected: " + percentEncoded );
				return 0;
			}

			try
			{
				return Convert.ToByte( percentEncoded.Substring( 1 ), 16 );
			}
			catch ( FormatException )
			{
				PackageUtilities.DebugFail( "The value passed to GetPercentEncodedValue was not expoected: " + percentEncoded );
				return 0;
			}
		} 

		#endregion GetPercentEncodedValue

		#region GetRelativePath

        public static Uri GetRelativePath(Uri sourcePart, Uri targetPart)
        {
            return GetRelativePath(sourcePart, targetPart, PackageUtilities.SegmentSeparator);
        }

        public static Uri GetRelativePath(Uri sourcePart, Uri targetPart, char segmentSeparator)
        {
			string relativePath = PackageUtilities.GetRelativePath( sourcePart.ToString(), targetPart.ToString(), PackageUtilities.SegmentSeparator, false );
			return new Uri( relativePath, UriKind.RelativeOrAbsolute );
		}

        public static string GetRelativePath(string sourcePart, string targetPart)
        {
            return GetRelativePath(sourcePart, targetPart, PackageUtilities.SegmentSeparator, false);
        }

        public static string GetRelativePath(string sourcePart, string targetPart, char segmentSeparator, bool useFullPathIfTargetIsParent)
        {
			List<string> sourceSegments = PackageUtilities.GetSegments( sourcePart, segmentSeparator );
            List<string> targetSegments = PackageUtilities.GetSegments( targetPart, segmentSeparator );

			for ( int i = 0; 
				i < sourceSegments.Count - 1 && i < targetSegments.Count; 
				i++ )
			{
				// Keep removing common parent directories.
				if ( sourceSegments[ i ] == targetSegments[ i ] )
				{
					sourceSegments.RemoveAt( i );
					targetSegments.RemoveAt( i );
					i--;
					continue;
				}
				else
				{
                    // MBS 9/11/08 
                    // When saving out external links, if the target is an ancestor of the source, Excel doesn't
                    // create a relative path but rather uses the full path without the drive letter.  Therefore,
                    // we should use the target path passed in without the drive letter
                    if (useFullPathIfTargetIsParent)
                    {
                        string localDrivePath = DriveLetterRegex.Replace(targetPart, String.Empty);
                        return localDrivePath.Replace(segmentSeparator, PackageUtilities.SegmentSeparator);
                    }
                    else
                    {
                        // When the first non-common parent segment is encountered, add a parent direcotry segment
                        // for each remaining source segment under the common parents.
                        for (int j = 0; j < sourceSegments.Count - 1; j++)
                            targetSegments.Insert(0, PackageUtilities.ParentDirectoryValue);
                    }
                    break;
				}
			}

			StringBuilder fullPath = new StringBuilder();

			// Concatentate all segments together
			foreach ( string segment in targetSegments )
			{
                // MBS 9/11/08
                // We should replace the current directory separator with that used
                // by the package manager
                //
				//fullPath.Append( segment );                
                if(segmentSeparator != PackageUtilities.SegmentSeparator)                    
                    fullPath.Append( segment.Replace(segmentSeparator, PackageUtilities.SegmentSeparator) );
                else
                    fullPath.Append( segment );

				fullPath.Append( PackageUtilities.SegmentSeparator );
			}

			fullPath.Remove( fullPath.Length - 1, 1 );

			return fullPath.ToString();
		} 

		#endregion GetRelativePath

		#region GetSegments

        internal static List<string> GetSegments(string partPath)
        {
            return GetSegments(partPath, PackageUtilities.SegmentSeparator);
        }

		internal static List<string> GetSegments( string partPath, char segmentSeparator)
		{
			if ( String.IsNullOrEmpty( partPath ) )
				return new List<string>();

            return new List<string>(partPath.Split(segmentSeparator));
		} 

		#endregion GetSegments

		#region GetTargetPartPath






		public static Uri GetTargetPartPath( IPackageRelationship relationship )
		{
			// The target path in the relationship is relative to the owning directory of the source file in the
			// relationship, so get the source file's owning directory.
			Uri sourceUriDirectory = PackageUtilities.GetDirectoryName( relationship.SourceUri );

			return PackageUtilities.CombinePaths( sourceUriDirectory, relationship.TargetUri );
		}

		#endregion GetTargetPartPath

		#region IsValidPartPath( string )

		public static bool IsValidPartPath( string partPath )
		{
			List<string> segments = PackageUtilities.GetSegments( partPath );

			// Open Packaging Conventions:
			// A part name shall not be empty. [M1.1]
			if ( segments.Count == 0 )
				return false;

			// Open Packaging Conventions:
			// A part name shall start with a forward slash (“/”) character. [M1.4]
			// The part path will only have an empty first segment if it began with a '/'
			if ( segments[ 0 ].Length != 0 )
				return false;

			for ( int i = 1; i < segments.Count; i++ )
			{
				if ( PackageUtilities.IsValidSegmentName( segments[ i ] ) == false )
					return false;
			}

			return true;
		}

		#endregion IsValidPartPath( string )

		#region IsValidPartPath( Uri )

		public static bool IsValidPartPath( Uri partPath )
		{
			return PackageUtilities.IsValidPartPath( partPath.ToString() );
		}

		#endregion IsValidPartPath( Uri )

		#region IsValidSegmentName

		internal static bool IsValidSegmentName( string segmentName )
		{
			// Open Packaging Conventions:
			// A part name shall not have empty segments. [M1.3]
			if ( String.IsNullOrEmpty( segmentName ) )
				return false;

			// Open Packaging Conventions:
			// A segment shall not end with a dot (“.”) character. [M1.9]
			// A segment shall include at least one non-dot character. [M1.10]
			if ( segmentName[ segmentName.Length - 1 ] == '.' )
				return false;

			// Open Packaging Conventions:
			// A segment shall not hold any characters other than pchar characters. [M1.6]
			Match pchars = PackageUtilities.ValidPCharsRegex.Match( segmentName );
			if ( pchars.Success == false ||
				pchars.Length != segmentName.Length )
			{
				return false;
			}

			Match percentEncoded = PackageUtilities.PercentEncodedRegex.Match( segmentName );

			while ( percentEncoded.Success )
			{
				byte value = PackageUtilities.GetPercentEncodedValue( percentEncoded.Value );
				char charValue = Convert.ToChar( value );

				switch ( charValue )
				{
					// Open Packaging Conventions:
					// A segment shall not contain percent-encoded forward slash (“/”), or backward slash (“\”) characters. [M1.7]
					case '\\':
					case '/':
						return false;
				}

				string stringValue = charValue.ToString();

				// Open Packaging Conventions:
				// A segment shall not contain percent-encoded unreserved characters. [M1.8]
				if ( PackageUtilities.UnreservedCharRegex.Match( stringValue ).Success )
					return false;

				percentEncoded = percentEncoded.NextMatch();
			}

			return true;
		} 

		#endregion IsValidSegmentName

		#region ParseW3CDTFValue

		public static DateTime ParseW3CDTFValue( string value )
		{
			// Make sure there is a final 'Z'
			if ( value.EndsWith( "Z" ) == false )
			{
				PackageUtilities.DebugFail( "The date format was not correct." );
				return DateTime.Now;
			}

			// Remove the final 'Z'
			value = value.Substring( 0, value.Length - 1 );

			try
			{
				DateTime dateTimeUniversal = DateTime.ParseExact( value, "s", null );
				return dateTimeUniversal.ToLocalTime();
			}
			catch ( Exception )
			{
				PackageUtilities.DebugFail( "Error occurred while parsing the date in W3CDTF format: " + value );
				return DateTime.Now;
			}
		} 

		#endregion ParseW3CDTFValue


		#region PercentEncodedRegex

		internal static Regex PercentEncodedRegex
		{
			get
			{
				if ( PackageUtilities.percentEncodedRegex == null )
				{
					PackageUtilities.percentEncodedRegex = new Regex(
						"%[0-9A-F]{2}",
						PackageUtilities.RegexOptionsCompiled | RegexOptions.IgnoreCase );
				}

				return PackageUtilities.percentEncodedRegex;
			}
		} 

		#endregion PercentEncodedRegex

		#region UnreservedCharRegex

		internal static Regex UnreservedCharRegex
		{
			get
			{
				if ( PackageUtilities.unreservedCharRegex == null )
				{
					PackageUtilities.unreservedCharRegex = new Regex(
						@"[\-\._~0-9A-Z]",
						PackageUtilities.RegexOptionsCompiled | RegexOptions.IgnoreCase );
				}

				return PackageUtilities.unreservedCharRegex;
			}
		}

		#endregion UnreservedCharRegex

		#region ValidPCharsRegex

		internal static Regex ValidPCharsRegex
		{
			get
			{
				if ( PackageUtilities.validPCharsRegex == null )
				{
					PackageUtilities.validPCharsRegex = new Regex(
						@"([:@\-\._~!$&'()*+,;=0-9A-Z]|(%[0-9A-F]{2}))*",
						PackageUtilities.RegexOptionsCompiled | RegexOptions.IgnoreCase );
				}

				return PackageUtilities.validPCharsRegex;
			}
		} 

		#endregion ValidPCharsRegex

		#region NonDotRegex

		internal static Regex NonDotRegex
		{
			get
			{
				if ( PackageUtilities.nonDotRegex == null )
				{
					PackageUtilities.nonDotRegex = new Regex(
						@"[^.]",
						PackageUtilities.RegexOptionsCompiled | RegexOptions.IgnoreCase );
				}

				return PackageUtilities.nonDotRegex;
			}
		} 

		#endregion NonDotRegex

        // MBS 9/11/08
        #region DriveLetterRegex

        internal static Regex DriveLetterRegex
        {
            get
            {
                if (driveLetterRegex == null)
                    driveLetterRegex = new Regex(@"\w:", PackageUtilities.RegexOptionsCompiled);

                return driveLetterRegex;
            }
        }
        #endregion //DriveLetterRegex

        //  BF 12/15/10 Infragistics.Documents.IO
        #region Moved from Excel2007WorkbookSerializationManager

        #region CreatePartInPackage

        //  BF 1/10/11  TFS62660
        //  Added an overload that lets the caller get a ref to the stream
        //  created by the part.

		static internal Uri CreatePartInPackage(
            object manager,
            IContentType contentType,
            IPackage zipPackage,
            ref IPackagePart activePart,
            Dictionary<string, object> partData,
            Stack<PartRelationshipCounter> partRelationshipCounters,
            ref int zipPackageRelationshipCount,
            string partPath,
            object context,
            out string relationshipId )
		{
            Stream createdStream = null;
            return PackageUtilities.CreatePartInPackage(
                    manager,
                    contentType,
                    zipPackage,
                    ref activePart,
                    partData,
                    partRelationshipCounters,
                    ref zipPackageRelationshipCount,
                    partPath,
                    context,
                    out relationshipId,
                    out createdStream );
        }

		static internal Uri CreatePartInPackage(
            object manager,
            IContentType contentType,
            IPackage zipPackage,
            ref IPackagePart activePart,
            Dictionary<string, object> partData,
            Stack<PartRelationshipCounter> partRelationshipCounters,
            ref int zipPackageRelationshipCount,
            string partPath,
            object context,
            out string relationshipId,
            out Stream createdStream )
		{

            //  BF 1/10/11  TFS62660
            createdStream = null;

            //  BF 11/16/10 IGWordStreamer
            //  This will be passed in instead.
			//ContentTypeBase contentTypeHandler = ContentTypeBase.GetContentType( contentType );

			if ( contentType == null )
			{
				PackageUtilities.DebugFail("Parameter 'contentType' was null; expecting an IContentType implementor here");
				relationshipId = null;
				return null;
			}

			Uri partName = new Uri( partPath, UriKind.RelativeOrAbsolute );
			Debug.Assert( PackageUtilities.IsValidPartPath( partName ), "An invalid part name has been created for a part: " + partName );

			relationshipId =
                PackageUtilities.CreateRelationshipInPackage(
                zipPackage,
                partRelationshipCounters,
                ref zipPackageRelationshipCount,
                partName,
                contentType.RelationshipType,
                RelationshipTargetMode.Internal,
                true );

            //  BF 11/16/10 IGWordStreamer
			//IPackagePart part = zipPackage.CreatePart( partName, contentType );
			IPackagePart part = zipPackage.CreatePart( partName, contentType.ContentType );

            partRelationshipCounters.Push( new PartRelationshipCounter( part ) );

            //  BF 11/16/10 IGWordStreamer
            //  We need to support keeping the stream open now
            #region Old code
            //using ( Stream stream = part.GetStream( FileMode.Create, FileAccess.Write ) )
            //{
            //    IPackagePart lastActivePart = activePart;
            //    activePart = part;

            //    if ( serializationManager != null )
            //        serializationManager.Save( contentTypeHandler, stream );

            //    activePart = lastActivePart;
            //}
            #endregion Old code

			Stream stream = part.GetStream( FileMode.Create, FileAccess.Write );
            bool disposeStream = true;
            
            //  BF 3/31/11  TFS67621 (see below)
            bool popCounterStack = true;
            
            try
			{
				IPackagePart lastActivePart = activePart;
				activePart = part;

                //  BF 3/31/11  TFS67621
                //
                //  Whether we pop an element off the partRelationshipCounters
                //  does not necessarily have anything to do with whether the
                //  stream should be disposed of. By default we will do this
                //  since this approach made sense for the Excel implementation,
                //  but we will now pass this to the Save handler and let the caller
                //  of this method decide whether that is what they want to do.
                //
				//contentType.SaveHandler( manager, stream, out disposeStream );
				contentType.SaveHandler( manager, stream, out disposeStream, ref popCounterStack );

				activePart = lastActivePart;
			}
            finally
            {
                //  Only dispose of the stream if the IContentType implementor
                //  did not want to keep it open.
                if ( disposeStream )
                    stream.Dispose();
            }

            //  If the stream was not disposed of, we should not
            //  pop the counter off the stack.
            
            //  BF 3/31/11  TFS67621 (see above)
            if ( popCounterStack )
                partRelationshipCounters.Pop();

            partData.Add(partPath, context);

            //  BF 1/10/11  TFS62660
            createdStream = stream;

			return partName;
		} 

        #endregion CreatePartInPackage

		#region CreateRelationshipId

		internal static string CreateRelationshipId( int currentRelationshipCount )
		{
			return PackageUtilities.RelationshipIdPrefix + ( currentRelationshipCount + 1 ).ToString();
		} 

		#endregion CreateRelationshipId

        #region CreateRelationshipInPackage
        static internal string CreateRelationshipInPackage(
            IPackage zipPackage,
            Stack<PartRelationshipCounter> partRelationshipCounters,
            ref int zipPackageRelationshipCount,
            Uri targetPartName,
            string relationshipType,
            RelationshipTargetMode targetMode,
            bool createRelativeUri)
		{
			string relationshipId;

			if ( partRelationshipCounters.Count == 0 )
			{
				relationshipId =
                    PackageUtilities.CreateRelationshipId( zipPackageRelationshipCount++ );

				Uri rootUri = new Uri( PackageUtilities.SegmentSeparator.ToString(), UriKind.RelativeOrAbsolute );
				Uri relativePath = PackageUtilities.GetRelativePath( rootUri, targetPartName );
				zipPackage.CreateRelationship( targetPartName, RelationshipTargetMode.Internal, relationshipType, relationshipId );
			}
			else
			{
				PartRelationshipCounter owningPartCounter = partRelationshipCounters.Peek();

				relationshipId = PackageUtilities.CreateRelationshipId( owningPartCounter.RelationshipCount );
				owningPartCounter.IncrementRelationshipCount();

                if (createRelativeUri)
                {
                    Uri relativePath = PackageUtilities.GetRelativePath(owningPartCounter.Part.Uri, targetPartName);
                    owningPartCounter.Part.CreateRelationship(relativePath, targetMode, relationshipType, relationshipId);
                }
                else
                    owningPartCounter.Part.CreateRelationship(targetPartName, targetMode, relationshipType, relationshipId);
			}

			return relationshipId;
		}
        #endregion CreateRelationshipInPackage

        #region GetNumberedPartNameHelper
        internal static string GetNumberedPartNameHelper( IPackage zipPackage, string basePartName, bool ignoreExtension )
        {
            Dictionary<string, object> partNamesWithoutExtensions = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);

            if (ignoreExtension)
            {
                foreach (IPackagePart part in zipPackage.GetParts())
                {
                    string partName = part.Uri.ToString();
                    string partNameExtension = Path.GetExtension(partName);
                    string partNameWithoutExtension = partName.Substring(0, partName.Length - partNameExtension.Length);

                    partNamesWithoutExtensions[partNameWithoutExtension] = null;
                }
            }

            string extension = Path.GetExtension(basePartName);
            string beginning = basePartName.Substring(0, basePartName.Length - extension.Length);

            int suffix = 1;
            while (true)
            {
                if (ignoreExtension)
                {
                    string testValue = beginning + suffix;

                    if (partNamesWithoutExtensions.ContainsKey(testValue) == false)
                    {
                        testValue += extension;
                        Debug.Assert(PackageUtilities.IsValidPartPath(testValue), "The numbered part name generated is invalid: " + testValue);
                        return testValue;
                    }
                }
                else
                {
                    string testValue = beginning + suffix + extension;
                    Debug.Assert(PackageUtilities.IsValidPartPath(testValue), "The numbered part name generated is invalid: " + testValue);

                    if (zipPackage.PartExists(new Uri(testValue, UriKind.RelativeOrAbsolute)) == false)
                        return testValue;
                }

                suffix++;
                Debug.Assert(suffix < 5000, "Something is wrong, there shouldn't be this many numbered parts.");
            }
        }
        #endregion GetNumberedPartNameHelper

        #endregion Moved from Excel2007WorkbookSerializationManager

        //  BF 12/15/10 Infragistics.Documents.IO
        #region Aliased Utilities methods

        internal static void DebugFail( string message )
        {

            SerializationUtilities.DebugFail( message );



        }

        internal static RegexOptions RegexOptionsCompiled
        {
            get
            {

                return SerializationUtilities.RegexOptionsCompiled;



            }
        }

        #endregion Aliased Utilities methods
    }
    #endregion PackageUtilities class

    //  BF 12/15/10 Infragistics.Documents.IO
    
    #region IContentType interface
    /// <summary>
    /// Used to decouple the PackageUtilities.CreatePartInPackage method
    /// from any specific type, for example, so ContentTypeBase-derived classes
    /// can use it as well as WordContentTypeExporterBase-derived classes.
    /// </summary>
    internal interface IContentType
    {
        string ContentType { get; }
        string RelationshipType { get; }
        ContentTypeSaveHandler SaveHandler { get; }
    }
    #endregion IContentType interface
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