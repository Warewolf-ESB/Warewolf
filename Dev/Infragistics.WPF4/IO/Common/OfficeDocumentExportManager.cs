using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using System.Globalization;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;
using Infragistics.Documents.Core.Packaging;
using System.Security;



using System.Windows.Media;
using System.Windows.Media.Imaging;








using Image = System.Windows.Media.Imaging.BitmapSource;


//  BF 3/23/11




namespace Infragistics.Documents.Core
{
    #region OfficeDocumentExportManager class


#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)

    internal abstract class OfficeDocumentExportManager :
        IDisposable
    {
        #region Constants
        
        internal const string ImagePartNameWithoutExtension_Word = "/word/media/image";
        internal const string ImagePartNameWithoutExtension_Excel = "/xl/media/image";
        
        internal const string ContentTypeValue_Png = "image/png";
        internal const string ContentTypeValue_Jpg = "image/jpeg";
        internal const string ContentTypeValue_Gif = "image/gif";
        internal const string ContentTypeValue_Tiff = "image/tiff";
        internal const string ContentTypeValue_Bmp = "image/bmp";

		private const string ExtensionGif = ".gif";
		private const string ExtensionJpeg = ".jpeg";
		private const string ExtensionPng = ".png";
		private const string ExtensionTiff = ".tiff";
		private const string ExtensionBmp = ".bmp";

        #endregion Constants

        #region Members

        //  IGWordStreamer
		private IPackagePart activePart;
		private Dictionary<string, object> partData = new Dictionary<string, object>();
		protected Stack<PartRelationshipCounter> partRelationshipCounters = new Stack<PartRelationshipCounter>();
		protected IPackage zipPackage = null;
		private int zipPackageRelationshipCount;

        //  BF 1/10/11  TFS62660
        private List<Stream> partStreams = null;

        //  BF 3/8/11   TFS67979
        #region Moved
        //#if !SILVERLIGHT
        //        private Dictionary<ImageCacheInfo, PackagePartCache> imageCache;
        //#endif
        #endregion Moved

        //  BF 3/30/11  TFS67621
        internal ImageContentTypeHandler imageContentTypeHandler = null;

        #endregion Members

        #region Constructor

        internal OfficeDocumentExportManager( IPackage zipPackage )
        {
            if ( zipPackage == null )
                throw new ArgumentNullException( "zipPackage" );

            this.zipPackage = zipPackage;
        }

        #endregion Constructor

        //  BF 1/11/10  TFS62660
        #region PartStreams
        private List<Stream> PartStreams
        {
            get
            {
                if ( this.partStreams == null )
                    this.partStreams = new List<Stream>();

                return this.partStreams;
            }
        }
        #endregion PartStreams

        #region Methods

        #region CreatePartInPackage

        //  BF 1/10/11  TFS62660
        internal Uri CreatePartInPackage( IContentType contentTypeHandler, string partPath, object context )
		{
			string relationshipId = null;

            return this.CreatePartInPackage(
                contentTypeHandler,
                partPath,
                context,
                this.partRelationshipCounters,
                out relationshipId );
        }

        internal Uri CreatePartInPackage(IContentType contentTypeHandler, string partPath, object context, Stack<PartRelationshipCounter> partRelationshipCounters, out string relationshipId)
		{
            Stream partStream = null;

            Uri retVal =
                PackageUtilities.CreatePartInPackage(
                    this,
                    contentTypeHandler,
                    this.zipPackage,
                    ref this.activePart,
                    this.partData,
                    partRelationshipCounters,
                    ref this.zipPackageRelationshipCount,
                    partPath,
                    context,
                    out relationshipId,
                    out partStream );

            //  BF 1/10/11  TFS62660
            //  The caller needn't be concerned with this, but we need to
            //  hang onto them and close them when we get disposed of.
            if ( partStream != null )
                this.PartStreams.Add( partStream );

            return retVal;
        } 

        #endregion CreatePartInPackage

		#region CreateRelationshipInPackage

        public string CreateRelationshipInPackage(Uri targetPartName, string relationshipType, RelationshipTargetMode targetMode, bool createRelativeUri)
        {
            return this.CreateRelationshipInPackage(
                targetPartName,
                relationshipType,
                targetMode,
                createRelativeUri,
                this.partRelationshipCounters);
        }

        public string CreateRelationshipInPackage(Uri targetPartName, string relationshipType, RelationshipTargetMode targetMode, bool createRelativeUri, Stack<PartRelationshipCounter> partRelationshipCounters)
		{
            return PackageUtilities.CreateRelationshipInPackage(
                this.zipPackage,
                partRelationshipCounters,
                ref this.zipPackageRelationshipCount,
                targetPartName,
                relationshipType,
                targetMode,
                createRelativeUri );
		}

		#endregion CreateRelationshipInPackage

		#region GetNumberedPartName

		public string GetNumberedPartName( string basePartName )
		{
			return this.GetNumberedPartName( basePartName, false );
		}

		public string GetNumberedPartName( string basePartName, bool ignoreExtension )
		{
            return PackageUtilities.GetNumberedPartNameHelper( this.zipPackage, basePartName, ignoreExtension );
		} 

        #endregion GetNumberedPartName

        #region GetPartData

		internal object GetPartData( IPackageRelationship relationship )
        {
			string absolutePath = PackageUtilities.GetTargetPartPath( relationship ).OriginalString;
			return this.GetPartData( absolutePath );
		}

		internal object GetPartData( string absolutePartPath )
		{
            if (this.partData.ContainsKey(absolutePartPath) == false)
                return null;

            return this.partData[absolutePartPath];
        }
        #endregion GetPartData

        #region GetPartPath

        internal string GetPartPath(object context)
        {
            Dictionary<string, object>.Enumerator enumerator = this.partData.GetEnumerator();
            while (enumerator.MoveNext())
            {
                KeyValuePair<string, object> currentItem = enumerator.Current;
                if (currentItem.Value == context)
                    return currentItem.Key;
            }
            return null;
        }
        #endregion GetPartPath

        #region GetRelationshipId

        internal string GetRelationshipId(string absolutePartPath)
        {
            if ( this.activePart == null )
                return string.Empty;

            IEnumerable<IPackageRelationship> relationships = this.activePart.GetRelationships();
            if (relationships != null)
            {
                foreach (IPackageRelationship relationship in relationships)
                {
                    if (absolutePartPath == PackageUtilities.GetTargetPartPath(relationship).OriginalString)
                        return relationship.Id;
                }
            }
            return String.Empty;
        }
        #endregion GetRelationshipId

        #region GetRelationshipPathFromActivePart

        public string GetRelationshipPathFromActivePart( string relationshipId )
        {
            IPackagePart activePart = this.activePart;
            IPackageRelationship packageRelationship = activePart != null ?
                activePart.GetRelationship( relationshipId ) :
                null;

            return packageRelationship != null ?
                PackageUtilities.GetTargetPartPath( packageRelationship ).OriginalString :
                null;
        }
        #endregion GetRelationshipPathFromActivePart

        #region SaveDocumentProperties
        /// <summary>
        /// Saves the core properties and extended properties.
        /// Returns true if successful, false otherwise.
        /// </summary>
        protected bool SaveDocumentProperties( OfficeDocumentProperties documentProperties )
        {
            if ( this.zipPackage == null )
                return false;

            Uri corePropsUri = new Uri(CorePropertiesPartExporter.DefaultPartName, UriKind.RelativeOrAbsolute);
            Uri extendedPropsUri = new Uri(ExtendedPropertiesPartExporter.DefaultPartName, UriKind.RelativeOrAbsolute);

            //  If the part does not exist, create it
            if ( this.zipPackage.PartExists(extendedPropsUri) == false )
            {
                //  Create '/docProps/app.xml' 
                this.CreatePartInPackage(
                    new ExtendedPropertiesPartExporter(),
                    ExtendedPropertiesPartExporter.DefaultPartName,
                    null);

                //  BF 2/4/11   TFS64927
                this.partRelationshipCounters.Clear();
            }

            //  If the part does not exist, create it
            if ( this.zipPackage.PartExists(corePropsUri) == false )
            {
                //  Create '/docProps/core.xml' 
                this.CreatePartInPackage(
                    new CorePropertiesPartExporter(),
                    CorePropertiesPartExporter.DefaultPartName,
                    null);
            }

            return  this.zipPackage.PartExists(corePropsUri) &&
                    this.zipPackage.PartExists(extendedPropsUri);
        }
        #endregion SaveDocumentProperties

        #region DocumentProperties
        internal abstract OfficeDocumentProperties GetDocumentProperties();
        #endregion DocumentProperties

		#region AddImageToPackage

        //  NA 2011.1 - Infragistics.Documents.Word
        //  Refactored
		// AS 2/3/11 NA 2011.1 - WPF DataPresenter Word Writer
		// Take a BitmapSource instead of System.Drawing.Image
		//
		public static Uri AddImageToPackage(
            OfficeDocumentExportManager manager,
            Dictionary<ImageCacheInfo, PackagePartCache> imageCache,
			Image image,

			BitmapEncoder preferredFormat,





            Stack<PartRelationshipCounter> partRelationshipCounters,
            out string relationshipId,
            out string name )
		{
            return OfficeDocumentExportManager.AddImageToPackage(
                manager,
                imageCache,
                image,



                preferredFormat,

                partRelationshipCounters,
                OfficeDocumentExportManager.ImagePartNameWithoutExtension_Word,
                true,
                out relationshipId,
                out name );
        }

		public static Uri AddImageToPackage(
            OfficeDocumentExportManager manager,
            Dictionary<ImageCacheInfo, PackagePartCache> imageCache,
			Image image,

			BitmapEncoder preferredFormat,





            Stack<PartRelationshipCounter> partRelationshipCounters,
            string basePartNameWithoutExtension,
            bool convertBMPToPNG,
            out string relationshipId,
            out string name )
		{
			string contentType;
			string extension;
			Guid formatGuid;
            
            relationshipId = null;
            name = null;

			#region Setup

			// at least right now we always call with null so try to use the format of 
			// the image. the only way I could find to get the format was if it was a
			// bitmapframe in which case we could get it from the decoder if it had one
			if (preferredFormat == null)
			{
				BitmapFrame bf = image as BitmapFrame;

				if (bf != null)
				{
					BitmapDecoder decoder = bf.Decoder;

					if (null != decoder && decoder.CodecInfo != null)
						preferredFormat = BitmapEncoder.Create(decoder.CodecInfo.ContainerFormat);
				}
			}
			if (preferredFormat == null)
				preferredFormat = new PngBitmapEncoder();

			formatGuid = preferredFormat.CodecInfo.ContainerFormat;

			if (formatGuid == ImageContentTypeHandler.JpgFormat)
			{
				contentType = OfficeDocumentExportManager.ContentTypeValue_Jpg;
				extension = OfficeDocumentExportManager.ExtensionJpeg;
			}
			else if (formatGuid == ImageContentTypeHandler.GifFormat)
			{
				contentType = OfficeDocumentExportManager.ContentTypeValue_Gif;
				extension = OfficeDocumentExportManager.ExtensionGif;
			}
			else if (formatGuid == ImageContentTypeHandler.TiffFormat)
			{
				contentType = OfficeDocumentExportManager.ContentTypeValue_Tiff;
				extension = OfficeDocumentExportManager.ExtensionTiff;
			}
			else if (formatGuid == ImageContentTypeHandler.BmpFormat)
			{
				contentType = OfficeDocumentExportManager.ContentTypeValue_Bmp;
				extension = OfficeDocumentExportManager.ExtensionBmp;
			}
			else
			{
				Debug.Assert(formatGuid == ImageContentTypeHandler.PngFormat, "Unsupported image format: " + formatGuid);
				formatGuid = ImageContentTypeHandler.PngFormat;
				contentType = OfficeDocumentExportManager.ContentTypeValue_Png;
				extension = OfficeDocumentExportManager.ExtensionPng;
			}


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)




			#endregion //Setup

			// AS 2/3/11 NA 2011.1 - WPF DataPresenter Word Writer
			// Added caching support. When possibly try to point to an existing image.
			//
			ImageCacheInfo itemCache = ImageCacheInfo.Create(image, formatGuid);

            //  BF 3/8/11   TFS67979
            //
            //  Headers & footers write their stuff to different package parts
            //  than the main document part, so we need to store the tables on
            //  the part exporter. This means duplicating images that appear in
            //  both the main document part and a header/footer, but redundancy
            //  is still reduced because each image only appears once in each part.
            //
            #region Refactored
            //if (null != itemCache)
            //{
            //    if ( imageCache == null )
            //        imageCache = new Dictionary<ImageCacheInfo, PackagePartCache>();

            //    PackagePartCache partCache;

            //    imageCache.TryGetValue(itemCache, out partCache);

            //    if (null != partCache)
            //    {
            //        relationshipId = partCache.RelationshipId;
            //        name = partCache.Name;
            //        return partCache.Uri;
            //    }
            //}
            #endregion Refactored

			if ( itemCache != null && imageCache != null )
			{
				PackagePartCache partCache;

				imageCache.TryGetValue(itemCache, out partCache);

				if (null != partCache)
				{
					relationshipId = partCache.RelationshipId;
					name = partCache.Name;
					return partCache.Uri;
				}
			}

            //  NA 2011.1 - Infragistics.Documents.Word
			//string partPath = manager.GetNumberedPartName( ImageBasePart.BasePartNameWithoutExtension + extension, true );
			string partPath = manager.GetNumberedPartName( basePartNameWithoutExtension + extension, true );

            //  BF 3/30/11  TFS67621
            //  This had no bearing on the bug but we can reduce instantions
            //  by reusing the same instance.
            manager.imageContentTypeHandler =

				ImageContentTypeHandler.Create( manager.imageContentTypeHandler, image, preferredFormat, contentType );






            Uri retVal = manager.CreatePartInPackage(
                manager.imageContentTypeHandler,
                partPath,
                image,
                partRelationshipCounters,
                out relationshipId );

            string relId = relationshipId.Replace( PackageUtilities.RelationshipIdPrefix, string.Empty );

            name = string.Format( "image{0}{1}", relId, extension );

			// AS 2/3/11 NA 2011.1 - WPF DataPresenter Word Writer
			// Added caching support. When possibly try to point to an existing image.
			//
            //  BF 3/8/11   TFS67979
            #region Refactored
            //if (null != itemCache)
            //    manager.imageCache[itemCache] = new PackagePartCache(retVal, name, relationshipId);
            #endregion Refactored

			if ( itemCache != null && imageCache != null )
				imageCache[itemCache] = new PackagePartCache(retVal, name, relationshipId);

            return retVal;
		} 

		#endregion AddImageToPackage

        #region GetImageFormatInfo


#region Infragistics Source Cleanup (Region)














































#endregion // Infragistics Source Cleanup (Region)

        #endregion GetImageFormatInfo

        //  BF 1/7/11   TFS62660
        #region SaveToZip
		[SecurityCritical] // MD 10/26/11 - TFS94123
        internal void SaveToZip( Stream stream )
        {






        }
        #endregion SaveToZip

        //  BF 1/10/11   TFS62660
        #region ClosePartStreams
        private void ClosePartStreams()
        {
            if ( this.partStreams != null )
            {
                foreach ( Stream stream in this.partStreams )
                {
                    stream.Close();
                    stream.Dispose();
                }

                this.partStreams = null;
            }
        }
        #endregion ClosePartStreams

        #endregion Methods

        #region IDisposable

        void IDisposable.Dispose()
        {            
            this.Dispose();
        }

        public virtual void Dispose()
        {
            this.Close();
        }

        public void Close()
        {
            //  BF 2/11/11  TFS66152
            //  The Package.Dispose method calls ZipFile.Save, which actually
            //  does cause the content to be saved a second time, which of course
            //  is bad, at least for the callers who want to be able to do something
            //  with the content that gets generated.
            //
            //  I moved the call to ClosePartStreams below the call to Dispose,
            //  and removed the code that calls ZipFile.Save directly since once
            //  is enough.

            ////  BF 1/10/11  TFS62660
            //this.ClosePartStreams();

            if ( this.zipPackage != null )
            {
			    ( (IDisposable)this.zipPackage ).Dispose();

                //  BF 2/11/11  TFS66152
                //  Nullify this ref so we can't get in here again
                this.zipPackage = null;
            }

            //  BF 1/10/11  TFS62660
            this.ClosePartStreams();
        }

        #endregion IDisposable
    }
    #endregion OfficeDocumentExportManager

	#region PackagePartCache
	internal class PackagePartCache
	{
		internal readonly Uri Uri;
		internal readonly string Name;
		internal readonly string RelationshipId;

		internal PackagePartCache(Uri uri, string name, string relationshipId)
		{
			this.Uri = uri;
			this.Name = name;
			this.RelationshipId = relationshipId;
		}
	}
	#endregion //PackagePartCache

	#region ImageCacheInfo

	internal class ImageCacheInfo : IEquatable<ImageCacheInfo>
	{
		#region Member Variables

		private WeakReference _image;
		private int _hashCode;
		private Guid _targetFormat;


		private static readonly ImageSourceConverter Converter = new ImageSourceConverter();
		private bool _hasChanged;
		private string _cachedUriString;


		#endregion //Member Variables

		#region Constructor
		private ImageCacheInfo(Image sourceImage, Guid format)
		{
			_targetFormat = format;
			_image = new WeakReference(sourceImage);


			TryGetUri(sourceImage, out _cachedUriString);

			// if its not frozen then watch for changes and invalidate the cache
			// if it does change
			if (!sourceImage.IsFrozen)
				sourceImage.Changed += new EventHandler(OnSourceImageChanged);

			if (_cachedUriString != null)
				_hashCode = _cachedUriString.GetHashCode();
			else
				_hashCode = sourceImage.GetHashCode();



		}
		#endregion //Constructor

		#region Base class overrides
		public override bool Equals(object obj)
		{
			return this.Equals(obj as ImageCacheInfo);
		}

		public override int GetHashCode()
		{
			return _hashCode;
		} 
		#endregion //Base class overrides

		#region Methods

		#region Public Methods

		#region Equals
		public bool Equals(ImageCacheInfo other)
		{
			if (other != null)
			{

				// if either was not frozen and has been modified then assume they're invalid
				if (_hasChanged || other._hasChanged)
					return false;


				if (other._targetFormat == _targetFormat)
				{
					Image target = CommonUtilities.GetWeakReferenceTarget(_image) as Image;
					Image otherTarget = CommonUtilities.GetWeakReferenceTarget(other._image) as Image;

					if (object.Equals(target, otherTarget))
						return true;


					if (_cachedUriString != null && 
						string.Equals(_cachedUriString, other._cachedUriString, StringComparison.Ordinal))
						return true;

				}
			}

			return false;
		}
		#endregion //Equals

		#endregion //Public Methods

		#region Private Methods

		#region OnSourceImageChanged

		private void OnSourceImageChanged(object sender, EventArgs e)
		{
			// if the imagesource changes then we have to assume the cache is invalid
			((System.Windows.Freezable)sender).Changed -= new EventHandler(OnSourceImageChanged);
			_hasChanged = true;
		}

		#endregion //OnSourceImageChanged

		#region TryGetUri

		private static bool TryGetUri(ImageSource source, out string uriString)
		{
			uriString = null;

			try
			{
				BitmapImage img = source as BitmapImage;

				if (null != img)
				{
					Uri uri = img.UriSource;

					if (uriString != null && !uri.IsAbsoluteUri)
					{
						// if its relative then try to create the absolute
						if (img.BaseUri != null)
							Uri.TryCreate(img.BaseUri, uriString, out uri);
						else // if its relative and there is no base then ignore it
							uriString = null;
					}

					if (uri != null)
						uriString = uri.AbsoluteUri;

					return uriString != null;
				}

				BitmapFrame frame = source as BitmapFrame;

				if (null != frame && frame.Decoder != null)
				{
					uriString = Converter.ConvertToString(frame);

					if (!string.IsNullOrEmpty(uriString))
					{
						Uri uri;
						return Uri.TryCreate(uriString, UriKind.Absolute, out uri);
					}
				}
			}
			catch (NotSupportedException)
			{
			}
			catch (InvalidOperationException)
			{
			}

			return false;
		}

		#endregion //TryGetUri

		#endregion //Private Methods

		#region Internal Methods

		#region Create
		internal static ImageCacheInfo Create(Image image, Guid targetFormat)
		{
			if (image != null)
			{
				// there is no clean reliable way to compare imagesources but if 
				// the same image source reference is used then we can cache that.
				// so if the imagesource is frozen we can compare references to that.
				// if the imagesource is not frozen we can watch for changes and as 
				// long as none come through we can compare references. also to cover 
				// the case where the imagesources are different but the uri's are 
				// the same we can try to compare uris
				return new ImageCacheInfo(image, targetFormat);
			}

			return null;
		}
		#endregion //Create

		#endregion //Internal Methods

		#endregion //Methods
	} 

	#endregion //ImageCacheInfo

#region Before SL changes (3/23/11)
//    #region ImageContentTypeHandler class
//#if ! SILVERLIGHT
//    // AS 2/3/11 NA 2011.1 - WPF DataPresenter Word Writer
//    // Added support for writing out BitmapSource in WPF
//    //
//    internal class ImageContentTypeHandler : IContentType
//    {
//#if WPF
//        internal static readonly Guid JpgFormat = new Guid(0x19e4a5aa, 0x5662, 0x4fc5, 160, 0xc0, 0x17, 0x58, 2, 0x8e, 0x10, 0x57);
//        internal static readonly Guid GifFormat = new Guid(0x1f8a5601, 0x7d4d, 0x4cbd, 0x9c, 130, 0x1b, 200, 0xd4, 0xee, 0xb9, 0xa5);
//        internal static readonly Guid TiffFormat = new Guid(0x163bcc30, 0xe2e9, 0x4f0b, 150, 0x1d, 0xa3, 0xe9, 0xfd, 0xb7, 0x88, 0xa3);
//        internal static readonly Guid BmpFormat = new Guid(0xaf1d87e, 0xfcfe, 0x4188, 0xbd, 0xeb, 0xa7, 0x90, 100, 0x71, 0xcb, 0xe3);
//        internal static readonly Guid PngFormat = new Guid(0x1b7cfaf4, 0x713f, 0x473c, 0xbb, 0xcd, 0x61, 0x37, 0x42, 0x5f, 0xae, 0xaf);
//#endif

//        private const string RelationshipTypeValue = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/image"; 
//        string contentType = null;

//#if WPF
//        private BitmapEncoder encoder = null;
//#else
//        private Image image = null;
//#endif

//#if WPF
//        internal ImageContentTypeHandler(BitmapSource image, BitmapEncoder encoder, string contentType)
//        {
//            this.encoder = encoder;
//            this.encoder.Frames.Add(BitmapFrame.Create(image));
//            this.contentType = contentType;
//        }
//#else
//        internal ImageContentTypeHandler( Image image, string contentType )
//        { 
//            this.image = image;
//            this.contentType = contentType;
//        }
//#endif

//        #region FromContentType
//#if !WPF
//        static private ImageFormat FromContentType( string contentType )
//        {
//            switch ( contentType )
//            {
//                case OfficeDocumentExportManager.ContentTypeValue_Bmp:
//                    return ImageFormat.Bmp;

//                case OfficeDocumentExportManager.ContentTypeValue_Gif:
//                    return ImageFormat.Gif;

//                case OfficeDocumentExportManager.ContentTypeValue_Jpg:
//                    return ImageFormat.Jpeg;

//                case OfficeDocumentExportManager.ContentTypeValue_Png:
//                    return ImageFormat.Png;

//                case OfficeDocumentExportManager.ContentTypeValue_Tiff:
//                    return ImageFormat.Tiff;

//                default:
//                    CommonUtilities.DebugFail( "Unrecognized contentType in 'FromContentType'" );
//                    return null;
//            }
//        }
//#endif
//        #endregion FromContentType

//        #region Save
//        private void Save( object manager, Stream stream, out bool closeStream )
//        {
//            closeStream = true;

//#if WPF
//            encoder.Save(stream);
//#else
//            ImageFormat format = ImageContentTypeHandler.FromContentType( this.contentType );
//            ImageCodecInfo codecInfo = GetEncoderInfo(format);
//            if (codecInfo != null)
//            {
//                Encoder encoder = Encoder.Quality;
//                EncoderParameters encoderParameters = new EncoderParameters(1);
//                encoderParameters.Param[0] = new EncoderParameter(encoder, 100L);
//                image.Save(stream, codecInfo, encoderParameters);
//            }
//            else
//                CommonUtilities.DebugFail("Unable to get the code info for the specified format: " + format);
//#endif
//        }
//        #endregion Save

//        #region GetEncoderInfo
//#if !WPF
//        private static ImageCodecInfo GetEncoderInfo(ImageFormat format)
//        {
//            int j;
//            ImageCodecInfo[] encoders;
//            encoders = ImageCodecInfo.GetImageEncoders();
//            for (j = 0; j < encoders.Length; j++)
//            {
//                if (encoders[j].FormatID == format.Guid)
//                    return encoders[j];
//            }
//            return null;
//        }
//#endif
//        #endregion GetEncoderInfo

//        #region IContentType

//        string IContentType.ContentType
//        {
//            get { return this.contentType; }
//        }

//        string IContentType.RelationshipType
//        {
//            get { return ImageContentTypeHandler.RelationshipTypeValue; }
//        }

//        ContentTypeSaveHandler IContentType.SaveHandler
//        {
//            get { return new ContentTypeSaveHandler(this.Save); }
//        }

//        #endregion IContentType
//    }
//#endif
//    #endregion ImageContentTypeHandler class
#endregion Before SL changes (3/23/11)


    #region ImageContentTypeHandler class

	// AS 2/3/11 NA 2011.1 - WPF DataPresenter Word Writer
	// Added support for writing out BitmapSource in WPF
	//
	internal class ImageContentTypeHandler : IContentType
    {
        #region Member variables

		internal static readonly Guid JpgFormat = new Guid(0x19e4a5aa, 0x5662, 0x4fc5, 160, 0xc0, 0x17, 0x58, 2, 0x8e, 0x10, 0x57);
		internal static readonly Guid GifFormat = new Guid(0x1f8a5601, 0x7d4d, 0x4cbd, 0x9c, 130, 0x1b, 200, 0xd4, 0xee, 0xb9, 0xa5);
		internal static readonly Guid TiffFormat = new Guid(0x163bcc30, 0xe2e9, 0x4f0b, 150, 0x1d, 0xa3, 0xe9, 0xfd, 0xb7, 0x88, 0xa3);
		internal static readonly Guid BmpFormat = new Guid(0xaf1d87e, 0xfcfe, 0x4188, 0xbd, 0xeb, 0xa7, 0x90, 100, 0x71, 0xcb, 0xe3);
		internal static readonly Guid PngFormat = new Guid(0x1b7cfaf4, 0x713f, 0x473c, 0xbb, 0xcd, 0x61, 0x37, 0x42, 0x5f, 0xae, 0xaf);


        private const string RelationshipTypeValue = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/image"; 
        string contentType = null;


		private BitmapEncoder encoder = null;







        //  BF 3/30/11  TFS67621
        private ContentTypeSaveHandler saveHandler = null;





        #endregion Member variables

        #region Constructor


		private ImageContentTypeHandler(BitmapSource image, BitmapEncoder encoder, string contentType)
		{
            this.Initialize( image, encoder, contentType );
        }

		private void Initialize( BitmapSource image, BitmapEncoder encoder, string contentType )
		{
			this.encoder = encoder;
			this.encoder.Frames.Add(BitmapFrame.Create(image));
			this.contentType = contentType;
		}

        static internal ImageContentTypeHandler Create( ImageContentTypeHandler instance, BitmapSource image, BitmapEncoder encoder, string contentType )
        {
            if ( instance == null )
                instance = new ImageContentTypeHandler( image, encoder, contentType );
            else
                instance.Initialize( image, encoder, contentType );

            return instance;
        }


#region Infragistics Source Cleanup (Region)


















#endregion // Infragistics Source Cleanup (Region)


#region Infragistics Source Cleanup (Region)

















#endregion // Infragistics Source Cleanup (Region)


        #endregion Constructor

        #region FromContentType


#region Infragistics Source Cleanup (Region)






















#endregion // Infragistics Source Cleanup (Region)

        #endregion FromContentType

        #region Save
        private void Save( object manager, Stream stream, out bool closeStream, ref bool popCounterStack )
        {
            //  BF 3/31/11  TFS67621
            //
            //  Disposing of the stream is very time consuming, or at
            //  least, disposing it here was. Anything that we don't
            //  let the PackageUtilities.CreatePartInPackage method
            //  dispose of gets disposed of by us when the writer is
            //  disposed of, so this does not cause a memory leak,
            //  and not disposing until after we do all our XML writing
            //  boosts performance by 300%.
            //
            //closeStream = true;
            closeStream = false;
            popCounterStack = true;


			encoder.Save(stream);


#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)


#region Infragistics Source Cleanup (Region)





















#endregion // Infragistics Source Cleanup (Region)

        }
        #endregion Save 

        #region GetCodecInfo


#region Infragistics Source Cleanup (Region)












#endregion // Infragistics Source Cleanup (Region)

        #endregion GetCodecInfo

        #region GetEncoderInfo


#region Infragistics Source Cleanup (Region)









#endregion // Infragistics Source Cleanup (Region)

        #endregion GetEncoderInfo

        #region IContentType

        string IContentType.ContentType
        {
            get { return this.contentType; }
        }

        string IContentType.RelationshipType
        {
            get { return ImageContentTypeHandler.RelationshipTypeValue; }
        }

        ContentTypeSaveHandler IContentType.SaveHandler
        {
            //  BF 3/30/11  TFS67621
            //get { return new ContentTypeSaveHandler(this.Save); }
            get
            {
                if ( this.saveHandler == null )
                    this.saveHandler = new ContentTypeSaveHandler( this.Save );

                return this.saveHandler;
            }
        }

        #endregion IContentType
    }

    #endregion ImageContentTypeHandler class
}

//  BF 1/10/11  TFS62660
//  Used for debugging
#region MemoryStreamX
//
//  Used for debugging
//
//internal class MemoryStreamX : MemoryStream
//{
//    private string name = null;
//    public MemoryStreamX( string name ) : base() { this.name = name; }

//    public override void Close()
//    {
//        base.Close();
//    }

//    protected override void Dispose(bool disposing)
//    {
//        base.Dispose(disposing);
//    }

//    public override long Position
//    {
//        get
//        {
//            try
//            {
//                return base.Position;
//            }
//            catch( Exception ex )
//            {
//                throw new Exception( string.Format("MemoryStreamX.get_Position: {0}", this.name), ex );
//            }
//        }
//        set
//        {
//            try
//            {
//                base.Position = value;
//            }
//            catch( Exception ex )
//            {
//                throw new Exception( string.Format("MemoryStreamX.set_Position: {0}", this.name), ex );
//            }
//        }
//    }
//}
#endregion MemoryStreamX


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