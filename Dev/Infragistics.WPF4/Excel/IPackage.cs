using System;
using System.Collections.Generic;
using System.Text;
using System.IO;


using System.IO.Packaging;
using System.Collections;




#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)


//  BF 12/15/10 Infragistics.Documents.IO








namespace Infragistics.Documents.Excel

{


    #region PackageFactory class
    internal class PackageFactory : IPackageFactory
    {
    #region IPackageFactory Members

        public IPackage Open(Stream stream, FileMode packageMode)
        {
            FileAccess accessMode;
            if (packageMode == FileMode.Open)
                accessMode = FileAccess.Read;
            else
                accessMode = FileAccess.ReadWrite;

            return new PackageWrapper(Package.Open(stream, packageMode, accessMode));
        }

    #endregion
    }
    #endregion //PackageFactory class

    #region PackageWrapper class
    internal class PackageWrapper : IPackage
    {
    #region Private Members
        private Package package;

        private PackageRelationshipCollectionWrapper packageRelationshipCollectionWrapper;
        private PackagePartCollectionWrapper packagePartCollectionWrapper;

        private Dictionary<PackagePart, PackagePartWrapper> packagePartWrappers;
    #endregion Private Members

    #region Constructor
        public PackageWrapper(Package package)
        {
            this.package = package;
        }
    #endregion //Constructor

    #region Private Properties

    #region PackagePartWrappers
        private Dictionary<PackagePart, PackagePartWrapper> PackagePartWrappers
        {
            get
            {
                if (this.packagePartWrappers == null)
                    this.packagePartWrappers = new Dictionary<PackagePart, PackagePartWrapper>();

                return packagePartWrappers;
            }
        }
    #endregion //PackagePartWrappers

    #endregion //Private Properties

    #region Private Methods

    #region GetPackagePartWrapper
        internal PackagePartWrapper GetPackagePartWrapper(PackagePart packagePart)
        {
            PackagePartWrapper packagePartWrapper;
            bool success = this.PackagePartWrappers.TryGetValue(packagePart, out packagePartWrapper);
            if (success)
                return packagePartWrapper;

            packagePartWrapper = new PackagePartWrapper(packagePart);
            this.PackagePartWrappers[packagePart] = packagePartWrapper;
            return packagePartWrapper;
        }
    #endregion //GetPackagePartWrapper

    #endregion //Private Methods

    #region IPackage Members

    #region CreatePart
        public IPackagePart CreatePart(Uri partUri, string contentType)
        {
            // MD 3/16/09
            // Found while fixing TFS14252
            // We were never compressing any of the package parts. Now the files sizes will be dramatically smaller.
            //PackagePart packagePart = this.package.CreatePart(partUri, contentType);
            PackagePart packagePart = this.package.CreatePart(partUri, contentType, CompressionOption.Normal);

            return new PackagePartWrapper(packagePart);
        }
    #endregion //CreatePart

    #region CreateRelationship
        public IPackageRelationship CreateRelationship(Uri targetUri, RelationshipTargetMode targetMode, string relationshipType, string id)
        {
            TargetMode realTargetMode = PackageWrapper.GetTargetMode(targetMode);
            PackageRelationship packageRelationship = this.package.CreateRelationship(targetUri, realTargetMode, relationshipType, id);
            return new PackageRelationshipWrapper(packageRelationship);
        }
    #endregion //CreateRelationship

    #region GetRelationships
        public IEnumerable<IPackageRelationship> GetRelationships()
        {
            if (this.packageRelationshipCollectionWrapper == null)
            {
                PackageRelationshipCollection packageRelationshipCollection = this.package.GetRelationships();
                this.packageRelationshipCollectionWrapper = new PackageRelationshipCollectionWrapper(packageRelationshipCollection);
            }

            return packageRelationshipCollectionWrapper;
        }
    #endregion //GetRelationships

    #region GetPart
        public IPackagePart GetPart(Uri partUri)
        {
            PackagePart packagePart = this.package.GetPart(partUri);
            return this.GetPackagePartWrapper(packagePart);
        }
    #endregion //GetPart

    #region GetParts
        public IEnumerable<IPackagePart> GetParts()
        {
            if (this.packagePartCollectionWrapper == null)
            {
                PackagePartCollection packagePartCollection = this.package.GetParts();
                this.packagePartCollectionWrapper = new PackagePartCollectionWrapper(packagePartCollection, this);
            }
            return this.packagePartCollectionWrapper;
        }
    #endregion //GetParts

    #region PartExists
        public virtual bool PartExists(Uri partUri)
        {
            return this.package.PartExists(partUri);
        }
    #endregion //PartExists

    #endregion //IPackage Members

    #region IDisposable Members

        public void Dispose()
        {
            ((IDisposable)this.package).Dispose();
        }

    #endregion

    #region Helper Methods

    #region GetTargetMode
        internal static TargetMode GetTargetMode(RelationshipTargetMode relationshipTargetMode)
        {
            switch (relationshipTargetMode)
            {
                case RelationshipTargetMode.External:
                    return TargetMode.External;
                case RelationshipTargetMode.Internal:
                    return TargetMode.Internal;
            }

            throw new ArgumentOutOfRangeException("relationshipTargetMode", "Unknown RelationshipTargetMode");
        }
    #endregion //GetTargetMode

    #region GetRelationshipTargetMode
        internal static RelationshipTargetMode GetRelationshipTargetMode(TargetMode targetMode)
        {
            switch (targetMode)
            {
                case TargetMode.External:
                    return RelationshipTargetMode.External;
                case TargetMode.Internal:
                    return RelationshipTargetMode.Internal;
            }

            throw new ArgumentOutOfRangeException("targetMode", "Unknown TargetMode");
        }
    #endregion //GetRelationshipTargetMode

    #endregion //Helper Methods

    }
    #endregion //PackageWrapper class

    #region PackagePartWrapper class
    internal class PackagePartWrapper : IPackagePart
    {
    #region Private Members
        private PackagePart packagePart;
        private PackageWrapper packageWrapper;
        private PackageRelationshipCollectionWrapper packageRelationshipCollectionWrapper;
        private Dictionary<PackageRelationship, PackageRelationshipWrapper> packageRelationshipWrappers;
    #endregion //Private Members

    #region Constructor
        public PackagePartWrapper(PackagePart packagePart)
        {
            this.packagePart = packagePart;
        }
    #endregion //Constructor

    #region Private Properties

    #region PackageRelationshipWrappers
        private Dictionary<PackageRelationship, PackageRelationshipWrapper> PackageRelationshipWrappers
        {
            get
            {
                if (this.packageRelationshipWrappers == null)
                    this.packageRelationshipWrappers = new Dictionary<PackageRelationship, PackageRelationshipWrapper>();

                return this.packageRelationshipWrappers;
            }
        }
    #endregion //PackageRelationshipWrappers

    #endregion //Private Properties

    #region Private Methods

    #region GetPackageRelationshipWrapper
        private PackageRelationshipWrapper GetPackageRelationshipWrapper(PackageRelationship packageRelationship)
        {
            PackageRelationshipWrapper packageRelationshipWrapper;
            bool success = this.PackageRelationshipWrappers.TryGetValue(packageRelationship, out packageRelationshipWrapper);
            if (success)
                return packageRelationshipWrapper;

            packageRelationshipWrapper = new PackageRelationshipWrapper(packageRelationship);
            this.PackageRelationshipWrappers[packageRelationship] = packageRelationshipWrapper;
            return packageRelationshipWrapper;
        }
    #endregion //GetPackageRelationshipWrapper

    #endregion //Private Methods

    #region IPackagePart Members

    #region Package
        public IPackage Package
        {
            get
            {
                if (this.packageWrapper == null)
                    this.packageWrapper = new PackageWrapper(this.packagePart.Package);

                return this.packageWrapper;
            }
        }
    #endregion //Package

    #region Uri
        public Uri Uri
        {
            get { return this.packagePart.Uri; }
        }
    #endregion //Uri

    #region ContentType
        public string ContentType
        {
            get { return this.packagePart.ContentType; }
        }
    #endregion //ContentType

    #region GetStream
        public Stream GetStream(FileMode mode, FileAccess access)
        {
            return this.packagePart.GetStream(mode, access);
        }
    #endregion //GetStream

    #region CreateRelationship
        public IPackageRelationship CreateRelationship(Uri targetUri, RelationshipTargetMode targetMode, string relationshipType, string id)
        {
            TargetMode realTargetMode = PackageWrapper.GetTargetMode(targetMode);
            PackageRelationship packageRelationship = this.packagePart.CreateRelationship(targetUri, realTargetMode, relationshipType, id);
            return new PackageRelationshipWrapper(packageRelationship);
        }
    #endregion //CreateRelationship

    #region GetRelationships
        public IEnumerable<IPackageRelationship> GetRelationships()
        {
            if (this.packageRelationshipCollectionWrapper == null)
            {
                PackageRelationshipCollection packageRelationshipCollection = this.packagePart.GetRelationships();
                this.packageRelationshipCollectionWrapper = new PackageRelationshipCollectionWrapper(packageRelationshipCollection);
            }
            return this.packageRelationshipCollectionWrapper;
        }
    #endregion //GetRelationships

    #region GetRelationship
        public IPackageRelationship GetRelationship(string id)
        {
            PackageRelationship packageRelationship = this.packagePart.GetRelationship(id);
            return this.GetPackageRelationshipWrapper(packageRelationship);
        }
    #endregion //GetRelationship

    #endregion //IPackagePart Members
    }
    #endregion //PackagePartWrapper class

    #region PackagePartCollectionWrapper class
    internal class PackagePartCollectionWrapper : IEnumerable<IPackagePart>
    {
    #region Private Members
        private PackagePartCollection packagePartCollection;
        private PackageWrapper packageWrapper;
    #endregion //Private Members

    #region Constructor
        public PackagePartCollectionWrapper(PackagePartCollection packagePartCollection, PackageWrapper packageWrapper)
        {
            this.packagePartCollection = packagePartCollection;
            this.packageWrapper = packageWrapper;
        }
    #endregion //Constructor

    #region IEnumerable<IPackagePart> Members

        public IEnumerator<IPackagePart> GetEnumerator()
        {
            foreach (PackagePart packagePart in this.packagePartCollection)
            {
                yield return this.packageWrapper.GetPackagePartWrapper(packagePart);
            }
        }

    #endregion

    #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

    #endregion
    }
    #endregion //PackagePartCollectionWrapper class

    #region PackageRelationshipWrapper class
    internal class PackageRelationshipWrapper : IPackageRelationship
    {
    #region Private Members

        private PackageRelationship packageRelationship;

    #endregion //Private Members

    #region Constructor

        public PackageRelationshipWrapper(PackageRelationship packageRelationship)
        {
            this.packageRelationship = packageRelationship;
        }

    #endregion //Constructor

    #region IPackageRelationship Members

    #region SourceUri
        public Uri SourceUri
        {
            get { return this.packageRelationship.SourceUri; }
        }
    #endregion //SourceUri

    #region TargetUri
        public Uri TargetUri
        {
            get { return this.packageRelationship.TargetUri; }
        }
    #endregion //TargetUri

    #region Id
        public string Id
        {
            get { return this.packageRelationship.Id; }
        }
    #endregion //Id

    #region RelationshipType
        public string RelationshipType
        {
            get { return this.packageRelationship.RelationshipType; }
        }
    #endregion //RelationshipType

    #region TargetMode
        public RelationshipTargetMode TargetMode
        {
            get { return PackageWrapper.GetRelationshipTargetMode(this.packageRelationship.TargetMode); }
        }
    #endregion //TargetMode

    #endregion //IPackageRelationship Members
    }
    #endregion //PackageRelationshipWrapper class

    #region PackageRelationshipCollectionWrapper class
    internal class PackageRelationshipCollectionWrapper : IEnumerable<IPackageRelationship>
    {
    #region Private Members

        private PackageRelationshipCollection packageRelationshipCollection;
        private Dictionary<PackageRelationship, PackageRelationshipWrapper> packageRelationshipWrappers;

    #endregion //Private Members

    #region Constructor

        public PackageRelationshipCollectionWrapper(PackageRelationshipCollection packageRelationship)
        {
            this.packageRelationshipCollection = packageRelationship;
        }

    #endregion //Constructor

    #region Private Properties

    #region PackageRelationshipWrappers
        private Dictionary<PackageRelationship, PackageRelationshipWrapper> PackageRelationshipWrappers
        {
            get
            {
                if (this.packageRelationshipWrappers == null)
                    this.packageRelationshipWrappers = new Dictionary<PackageRelationship, PackageRelationshipWrapper>();

                return this.packageRelationshipWrappers;
            }
        }
    #endregion //PackageRelationshipWrappers

    #endregion //Private Properties

    #region Private Methods

    #region GetPackageRelationshipWrapper
        private PackageRelationshipWrapper GetPackageRelationshipWrapper(PackageRelationship packageRelationship)
        {
            PackageRelationshipWrapper packageRelationshipWrapper;
            bool success = this.PackageRelationshipWrappers.TryGetValue(packageRelationship, out packageRelationshipWrapper);
            if (success)
                return packageRelationshipWrapper;

            packageRelationshipWrapper = new PackageRelationshipWrapper(packageRelationship);
            this.PackageRelationshipWrappers[packageRelationship] = packageRelationshipWrapper;
            return packageRelationshipWrapper;
        }
    #endregion //GetPackageRelationshipWrapper

    #endregion //Private Methods

    #region IEnumerable<IPackageRelationship> Members

        public IEnumerator<IPackageRelationship> GetEnumerator()
        {
            foreach (PackageRelationship packageRelationship in this.packageRelationshipCollection)
            {
                yield return this.GetPackageRelationshipWrapper(packageRelationship);
            }
        }

    #endregion

    #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

    #endregion
    }
    #endregion //PackageRelationshipCollectionWrapper class





#region Infragistics Source Cleanup (Region)








































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































































#endregion // Infragistics Source Cleanup (Region)


    #region IPackageFactory interface
    /// <summary>
    /// Factory class used to create an IPackage given a stream and a FileMode
    /// </summary>



    public

		 interface IPackageFactory
    {
        /// <summary>
        /// Opens an IPackage with a given IO stream and file mode.
        /// </summary>
        /// <param name="stream">The IO stream on which to open the IPackage.</param>
        /// <param name="packageMode">The file mode in which to open the IPackage.</param>
        /// <returns>The opened IPackage.</returns>
        IPackage Open(Stream stream, FileMode packageMode);
    }
    #endregion //IPackageFactory interface

    #region IPackage interface
    /// <summary>
    /// Represents a container that can store multiple data objects.
    /// </summary>



    public

		 interface IPackage : IDisposable
    {
        /// <summary>
        /// Creates a new uncompressed part with a given URI and content type.
        /// </summary>
        /// <param name="partUri">The uniform resource identifier (URI) of the new part.</param>
        /// <param name="contentType">The content type of the data stream.</param>
        /// <returns>The new created part.</returns>
        IPackagePart CreatePart(Uri partUri, string contentType);

        /// <summary>
        /// Creates a package-level relationship to a part with a given URI, target mode, relationship type, and identifier (ID).
        /// </summary>
        /// <param name="targetUri">The uniform resource identifier (URI) of the target part.</param>
        /// <param name="targetMode">Indicates if the target part is System.IO.Packaging.TargetMode.Internal or System.IO.Packaging.TargetMode.External to the package.</param>
        /// <param name="relationshipType">A URI that uniquely defines the role of the relationship.</param>
        /// <param name="id">A unique XML identifier.</param>
        /// <returns>The package-level relationship to the specified part.</returns>
        IPackageRelationship CreateRelationship(Uri targetUri, RelationshipTargetMode targetMode, string relationshipType, string id);

        /// <summary>
        /// Returns a collection of all the package-level relationships.
        /// </summary>
        /// <returns>A collection of all the package-level relationships that are contained in the package.</returns>
        IEnumerable<IPackageRelationship> GetRelationships();

        /// <summary>
        /// Returns the part with a given URI.
        /// </summary>
        /// <param name="partUri">The uniform resource identifier (URI) of the part to return.</param>
        /// <returns>The part with the specified partUri.</returns>        
        IPackagePart GetPart(Uri partUri);

        /// <summary>
        /// Returns a collection of all the parts in the package.
        /// </summary>
        /// <returns>A collection of all the System.IO.Packaging.PackagePart elements that are contained in the package.</returns>
        IEnumerable<IPackagePart> GetParts();

        /// <summary>
        /// Indicates whether a part with a given URI is in the package.
        /// </summary>
        /// <param name="partUri">The System.Uri of the part to check for.</param>
        /// <returns>true if a part with the specified partUri is in the package; otherwise, false.</returns>
        bool PartExists(Uri partUri);
    }
    #endregion //IPackage interface

    #region IPackagePart interface
    /// <summary>
    /// Provides a base class for parts stored in a System.IO.Packaging.Package. This class is abstract.
    /// </summary>



    public

		 interface IPackagePart
    {
        /// <summary>
        /// Gets the parent Package of the part.
        /// </summary>        
        IPackage Package { get; }

        /// <summary>
        /// Gets the URI of the part.
        /// </summary>
        Uri Uri { get; }

        /// <summary>
        ///  Gets the MIME type of the content stream.        
        /// </summary>
        string ContentType { get; }

        /// <summary>
        /// Returns the part content stream opened with a specified System.IO.FileMode and System.IO.FileAccess.
        /// </summary>
        /// <param name="mode">The I/O mode in which to open the content stream.</param>
        /// <param name="access">The access permissions to use in opening the content stream.</param>
        /// <returns>The content stream for the part.</returns>
        Stream GetStream(FileMode mode, FileAccess access);

        /// <summary>
        /// Creates a part-level relationship between this IPackagePart to a specified target IPackagePart or external resource.
        /// </summary>
        /// <param name="targetUri">The URI of the target part.</param>
        /// <param name="targetMode">One of the enumeration values. For example, RelationshipTargetMode.Internal if the target part is inside the IPackage; or RelationshipTargetMode.External if the target is a resource outside the IPackage.</param>
        /// <param name="relationshipType">The role of the relationship.</param>
        /// <param name="id">A unique ID for the relationship.</param>
        /// <returns>The part-level relationship between this IPackagePart to the target IPackagePart or external resource.</returns>
        IPackageRelationship CreateRelationship(Uri targetUri, RelationshipTargetMode targetMode, string relationshipType, string id);

        /// <summary>
        /// Returns a collection of all the relationships that are owned by this part.
        /// </summary>
        /// <returns>A collection of all the relationships that are owned by the part.</returns>
        IEnumerable<IPackageRelationship> GetRelationships();

        /// <summary>
        /// Returns the relationship that has a specified IPackageRelationship.Id.
        /// </summary>
        /// <param name="id">The IPackageRelationship.Id of the relationship to return.</param>
        /// <returns>The relationship that matches the specified id.</returns>
        IPackageRelationship GetRelationship(string id);
    }
    #endregion //IPackagePart interface

    #region IPackageRelationship
    /// <summary>
    /// Represents an association between a source IPackage or IPackagePart, and a target object which can be a IPackagePart or external resource.
    /// </summary>



    public

		 interface IPackageRelationship
    {
        /// <summary>
        /// Gets the URI of the package or part that owns the relationship.
        /// </summary>
        Uri SourceUri { get; }

        /// <summary>
        /// Gets the URI of the target resource of the relationship.
        /// </summary>
        Uri TargetUri { get; }

        /// <summary>
        /// Gets a string that identifies the relationship.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Gets the qualified type name of the relationship.
        /// </summary>
        string RelationshipType { get; }

        /// <summary>
        /// Gets a value that indicates whether the target of the relationship is RelationshipTargetMode.Internal or RelationshipTargetMode.External to the IPackage.
        /// </summary>
        RelationshipTargetMode TargetMode { get; }
    }
    #endregion //IPackageRelationship

    #region RelationshipTargetMode
    /// <summary>
    /// Specifies whether the target of an IPackageRelationship is inside or outside the IPackage.
    /// </summary>



    public

		 enum RelationshipTargetMode
    {
        /// <summary>
        /// The relationship references a part that is inside the package.
        /// </summary>
        Internal,

        /// <summary>
        /// The relationship references a resource that is external to the package.
        /// </summary>
        External
    }
    #endregion //RelationshipTargetMode

    //  BF 11/17/10 IGWordStreamer
    #region PartRelationshipCounter class

    /// <summary>
    /// Duplicated from
    /// Infragistics.Documents.Excel.Serialization.Excel2007.Excel2007WorkbookSerializationManager
    /// </summary>
    internal class PartRelationshipCounter
    {
        private IPackagePart part;
        private int relationshipCount;

        public PartRelationshipCounter(IPackagePart part)
        {
            this.part = part;
        }

        public void IncrementRelationshipCount()
        {
            this.relationshipCount++;
        }

        public IPackagePart Part
        {
            get { return this.part; }
        }

        public int RelationshipCount
        {
            get { return this.relationshipCount; }
        }
    }

    #endregion PartRelationshipCounter class

    //  BF 11/17/10 IGWordStreamer
    #region ContentTypeSaveHandler

    /// <summary>
    /// Defines the signature of the method called by the static
    /// CreatePartInPackage method to save data to the content type handler.
    /// </summary>
    /// <param name="manager">The serialization manager, e.g., Excel2007WorkbookSerializationManager</param>
    /// <param name="stream">The stream to which the data is serialized.</param>
    /// <param name="closeStream">
    /// Signifies to the caller whether the stream should be closed after
    /// execution returns. Unless the implementing part supports realtime
    /// streaming, true should be returned.
    /// </param>
    /// <param name="popCounterStack">
    /// Signifies to the caller whether the partRelationshipsCounter stack
    /// should be popped after execution returns from the Save method.
    /// </param>
    internal delegate void ContentTypeSaveHandler(object manager, Stream stream, out bool closeStream, ref bool popCounterStack);

    #endregion ContentTypeSaveHandler
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