using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace Infragistics.Documents.Excel
{
	// MD 7/26/10 - TFS34398
	// Made this an abstract class.
	//internal class GenericCacheElementProxy<T>
	internal abstract class GenericCacheElementProxy<T>
		// MD 2/2/12 - TFS100573
		//where T : GenericCacheElement
		where T : GenericCacheElementEx
	{
		#region Member Variables

		protected T element;

		// MD 12/12/11 - 12.1 - Table Support
		// Moved this to the GenericCacheElement because the element should really know what collection it exists in.
		//private GenericCachedCollection<T> collection;

		// MD 7/26/10 - TFS34398
		// We don't need to store the workbook on the proxy, because we can get it from other sources on the format data proxy.
		//private Workbook workbook;

		#endregion Member Variables

		#region Constructor

		// MD 7/26/10 - TFS34398
		// We don't store the workbook on the proxy anymore.
		//public GenericCacheElementProxy( T element, GenericCachedCollection<T> collection, Workbook workbook )
		// MD 2/2/12 - TFS100573
		//public GenericCacheElementProxy(T element, GenericCachedCollection<T> collection)
		public GenericCacheElementProxy(T element, GenericCachedCollectionEx<T> collection)
		{
			// MD 12/21/11 - 12.1 - Table Support
			// The proxies no longer store a reference to the collection. It is stored on the element.
			//this.collection = collection;

			// MD 7/26/10 - TFS34398
			// We don't store the workbook on the proxy anymore.
			//this.workbook = workbook;

			// MD 4/18/11 - TFS62026
			// The element can be null now.
			//this.element = GenericCacheElement.FindExistingOrAddToCache( element, this.collection );
			if (element != null)
			{
				// MD 12/21/11 - 12.1 - Table Support
				// The proxies no longer store a reference to the collection. It is stored on the element.
				//this.element = GenericCacheElement.FindExistingOrAddToCache(element, this.collection);
				// MD 2/2/12 - TFS100573
				//this.element = GenericCacheElement.FindExistingOrAddToCache(element, collection);
				this.element = GenericCacheElementEx.FindExistingOrAddToCacheEx(element, collection);
			}
		}

		// MD 7/26/10 - TFS34398
		// We don't store the workbook on the proxy anymore.
		//public GenericCacheElementProxy( GenericCachedCollection<T> collection, Workbook workbook )
		//    : this( collection.DefaultElement, collection, workbook ) { }
		// MD 2/2/12 - TFS100573
		//public GenericCacheElementProxy(GenericCachedCollection<T> collection)
		public GenericCacheElementProxy(GenericCachedCollectionEx<T> collection)
			// MD 2/15/11 - TFS66333
			// Use the EmptyElement for the initial data element. The DefaultElement will be populated with data if 
			// the workbook was loaded from a file or stream.
			//: this(collection.DefaultElement, collection) { }
			// MD 1/8/12 - 12.1 - Cell Format Updates
			// Now that the cell format collection only holds cell formats and now style formats, the default element really should
			// be used for initializing the proxy.
			//: this(collection.EmptyElement, collection) { }
			: this(collection.DefaultElement, collection) { }

		#endregion Constructor

		#region Base Class Overrides

		#region Equals

		public override bool Equals( object obj )
		{
			GenericCacheElementProxy<T> proxy = obj as GenericCacheElementProxy<T>;

			if ( proxy == null )
				return false;

			return this.Element.Equals( proxy.Element );
		}

		#endregion Equals

		#region GetHashCode

		public override int GetHashCode()
		{
			return this.Element.GetHashCode();
		}

		#endregion GetHashCode

		#endregion Base Class Overrides

		#region Methods

		#region Public Methods

		#region AfterSet

		// MD 4/18/11 - TFS62026
		// Made this virtual so it could be overridden in derived classes.
		//public void AfterSet()
		// MD 12/21/11 - 12.1 - Table Support
		// The proxies no longer store a reference to the collection. It is stored on the element, so it needs to be passed into the AfterSet.
		//public virtual void AfterSet()
		public virtual void AfterSet(GenericCachedCollection<T> collection)
		{
			// MD 4/12/11 - TFS67084
			// Moved code to static method so it could be used by other classes not derived from GenericCacheElementProxy.
			// MD 12/21/11 - 12.1 - Table Support
			//GenericCacheElementProxy<T>.AfterSet(this.collection, ref this.element);
			// MD 2/2/12 - TFS100573
			//GenericCacheElementProxy<T>.AfterSet(collection, ref this.element);
			GenericCacheElementEx.AfterSetEx(collection, ref this.element);
		}

		// MD 2/2/12 - TFS100573
		// Moved this to the GenericCacheElementEx class.
		#region Moved

		//// MD 4/12/11 - TFS67084
		//public static void AfterSet(GenericCachedCollection<T> collection, ref T element)
		//{
		//    //element = GenericCacheElement.FindExistingOrAddToCache(element, collection);
		//    element = GenericCacheElementEx.FindExistingOrAddToCacheEx(element, collection);
		//}

		#endregion // Moved

		#endregion AfterSet

		#region BeforeSet

		// MD 12/21/11 - 12.1 - Table Support
		// The proxies no longer store a reference to the collection. It is stored on the element, so it needs to be returned from the BeforeSet.
		//public void BeforeSet()
		public GenericCachedCollection<T> BeforeSet()
		{
			// MD 4/18/11 - TFS62026
			// Moved all code to a new overload.
			// MD 12/21/11 - 12.1 - Table Support
			//this.BeforeSet(true);
			return this.BeforeSet(true);
		}

		// MD 4/18/11 - TFS62026
		// Added a virtual overload which allows the caller to specify whether they will modify the element after
		// calling this method. If False is passed in, we will not Clone elements which are referenced by other proxies.
		// MD 12/21/11 - 12.1 - Table Support
		// The proxies no longer store a reference to the collection. It is stored on the element, so it needs to be returned from the BeforeSet.
		//public virtual void BeforeSet(bool willModifyElement)
		public virtual GenericCachedCollection<T> BeforeSet(bool willModifyElement)
		{
			// MD 4/12/11 - TFS67084
			// Moved code to static method so it could be used by other classes not derived from GenericCacheElementProxy.
			// MD 4/18/11 - TFS62026
			// Pass along the willModifyElement parameter.
			//GenericCacheElementProxy<T>.BeforeSet(collection, ref element);
			// MD 12/21/11 - 12.1 - Table Support
			//GenericCacheElementProxy<T>.BeforeSet(this.collection, ref this.element, willModifyElement);
			// MD 2/2/12 - TFS100573
			//return GenericCacheElementProxy<T>.BeforeSet(ref this.element, willModifyElement);
			return GenericCacheElementEx.BeforeSetEx(ref this.element, willModifyElement);
		}

		// MD 2/2/12 - TFS100573
		// Moved this to the GenericCacheElementEx class.
		#region Moved

		//// MD 4/12/11 - TFS67084
		//// MD 4/18/11 - TFS62026
		//// Added a parameter which allows the caller to specify whether they will modify the element after
		//// calling this method. If False is passed in, we will not Clone elements which are referenced by other proxies.
		//// MD 12/21/11 - 12.1 - Table Support
		//// The proxies no longer store a reference to the collection. It is stored on the element, so it needs to be returned from the BeforeSet.
		////public static void BeforeSet(GenericCachedCollection<T> collection, ref T element)
		//public static GenericCachedCollection<T> BeforeSet(ref T element, bool willModifyElement)
		//{
		//    // MD 12/21/11 - 12.1 - Table Support
		//    //GenericCacheElement.Release(element, collection);
		//    //GenericCachedCollection<T> collection = GenericCacheElement.Release(element);
		//    GenericCachedCollection<T> collection = GenericCacheElementEx.ReleaseEx(element);
		//
		//    // If the element still has references to it even after we release, clone the element so we don't change 
		//    // some other proxies properties which is also pointing to this element.
		//    // MD 4/18/11 - TFS62026
		//    // Only clone the element if the element will be modified after the BeforeSet call.
		//    //if (element.ReferenceCount > 0)
		//    if (willModifyElement && element.ReferenceCount > 0)
		//        element = (T)element.Clone();
		//
		//    // MD 12/21/11 - 12.1 - Table Support
		//    Debug.Assert(willModifyElement == false || element.Collection == null, "The collection should be null here.");
		//    return collection;
		//}

		#endregion // Moved

		#endregion BeforeSet

		#region OnRooted

		public void OnRooted( GenericCachedCollection<T> rootCollection )
		{
			this.SetCollection( rootCollection );
		}

		#endregion OnRooted

		#region OnUnrooted

		public void OnUnrooted()
		{
			this.SetCollection( null );
		}

		#endregion OnUnrooted

		#region SetToElement

		// MD 5/12/10 - TFS26732
		// This should only be called by derived classes.
		//public void SetToElement( T newElement )
		protected void SetToElement(T newElement)
		{
			if ( this.Element == newElement )
				return;

			// MD 2/29/12 - 12.1 - Table Support
			#region Old Code

			//// MD 4/18/11 - TFS62026
			//// Now that AfterSet is virtual, this has to be done in a way that allows derived classes to get involved in the process.
			////GenericCacheElement.Release( this.element, this.collection );
			////this.element = GenericCacheElement.FindExistingOrAddToCache( newElement, this.collection );
			//// MD 12/21/11 - 12.1 - Table Support
			//// If the element is frozen, clone is before adding it to the collection.
			////this.BeforeSet(false);
			////this.element = newElement;
			////this.AfterSet();
			//if (newElement.IsFrozen)
			//    newElement = (T)newElement.Clone();

			//// MD 12/22/11 - 12.1 - Table Support
			//// If the new element belongs to a cache collection which is different than what this proxy links into, use a cloned element
			//// because we don't want to affect the reference count of the other element.
			//if (newElement.Collection != null && this.element.Collection != newElement.Collection)
			//    newElement = (T)newElement.Clone();

			//GenericCachedCollection<T> collection = this.BeforeSet(false);
			//this.element = newElement;
			//this.AfterSet(collection);

			#endregion // Old Code
			bool useClone = false;
			if (newElement.IsFrozen)
			{
				useClone = true;
			}
			// If the new element belongs to a cache collection which is different than what this proxy links into, use a cloned element
			// because we don't want to affect the reference count of the other element.
			else if (newElement.Collection != null && this.element.Collection != newElement.Collection)
			{
				useClone = true;
			}
			// If the element is not part of a cache collection, the proxy should be the only owner of the element, 
			// so make sure we use a clone of it.
			else if (newElement.Collection == null && this.element.Collection == null)
			{
				useClone = true;
			}

			if (useClone)
				newElement = (T)newElement.Clone();

			GenericCachedCollection<T> collection = this.BeforeSet(false);
			this.element = newElement;
			this.AfterSet(collection);
		}

		#endregion SetToElement

		#endregion Public Methods

		#region Private Methods

		#region SetCollection

		private void SetCollection( GenericCachedCollection<T> rootCollection )
		{
			// MD 4/12/11 - TFS67084
			// Moved code to static method so it could be used by other classes not derived from GenericCacheElementProxy.
			//if ( this.collection != rootCollection )
			//{
			//    GenericCacheElement.Release( this.element, this.collection );
			//    this.collection = rootCollection;
			//    this.element = GenericCacheElement.FindExistingOrAddToCache( this.element, this.collection );
			//}
			//
			//Debug.Assert( this.collection != null || this.element.ReferenceCount == 1 );

			// MD 4/18/11 - TFS62026
			// Get the virtual property first to make sure any verification of the element is completed.
			GenericCacheElement tmp = this.Element;

			// MD 12/21/11 - 12.1 - Table Support
			// Since the element stores a reference to its collection, it doesn't need to be passed in here.
			//GenericCacheElementProxy<T>.SetCollection(rootCollection, ref this.collection, ref this.element);
			// MD 2/2/12 - TFS100573
		    //GenericCacheElementProxy<T>.SetCollection(rootCollection, ref this.element);
			GenericCacheElementEx.SetCollectionEx(rootCollection, ref this.element);
		}

		// MD 2/2/12 - TFS100573
		// Moved this to the GenericCacheElementEx class.
		#region Moved

		//// MD 4/12/11 - TFS67084
		//// MD 12/21/11 - 12.1 - Table Support
		//// Since the element stores a reference to its collection, it doesn't need to be passed in here.
		////internal static void SetCollection(GenericCachedCollection<T> newCollection,
		////    ref GenericCachedCollection<T> collection, 
		////    ref T element)
		//internal static void SetCollection(GenericCachedCollection<T> newCollection, ref T element)
		//{
		//    // MD 12/21/11 - 12.1 - Table Support
		//    GenericCachedCollection<T> collection = (GenericCachedCollection<T>)element.Collection;

		//    if (collection != newCollection)
		//    {
		//        // MD 4/18/11 - TFS62026
		//        // Use the BeforeSet and AfterSet methods, which also do what was done before.
		//        //GenericCacheElement.Release(element, collection);
		//        //collection = newCollection;
		//        //element = GenericCacheElement.FindExistingOrAddToCache(element, collection);
		//        // MD 12/21/11 - 12.1 - Table Support
		//        // Since the element stores a reference to its collection, it doesn't need to be passed in here.
		//        //GenericCacheElementProxy<T>.BeforeSet(collection, ref element, true);
		//        GenericCachedCollection<T> oldCollection = GenericCacheElementProxy<T>.BeforeSet(ref element, true);
		//        Debug.Assert(oldCollection == collection, "The old collection does not match what was stored on the element here.");

		//        collection = newCollection;
		//        GenericCacheElementProxy<T>.AfterSet(collection, ref element);
		//    }

		//    Debug.Assert(collection != null || element.ReferenceCount == 0);
		//}

		#endregion // Moved

		#endregion SetCollection

		#endregion Private Methods

		#endregion Methods

		#region Properties

		// MD 12/21/11 - 12.1 - Table Support
		// The proxies no longer store a reference to the collection. It is stored on the element.
		#region Removed

		//#region Collection

		//public GenericCachedCollection<T> Collection
		//{
		//    get { return this.collection; }
		//    set { this.collection = value; }
		//}
		//#endregion //Collection

		#endregion // Removed

        #region Element

		// MD 4/18/11 - TFS62026
		// Made this virtual so it could be overridden in derived classes.
		// Also, replaced most references to the element member variable with references to the Element property.
        //public T Element
		public virtual T Element
		{
			get { return this.element; }
		}

		#endregion Element

		// MD 3/2/12 - 12.1 - Table Support
		// This has been replaced by IsEmpty on WorksheetCellFormatProxy.
		#region Removed

		//#region HasDefaultValue

		//public bool HasDefaultValue
		//{
		//    get
		//    {
		//        // MD 12/21/11 - 12.1 - Table Support
		//        // The proxies no longer store a reference to the collection. It is stored on the element.
		//        //if ( this.collection == null )
		//        //{
		//        //    Utilities.DebugFail( "The getter for this property should not be called when the collection is null." );
		//        //    return false;
		//        //}
		//        //
		//        //return this.Element.HasSameData( this.collection.DefaultElement );
		//        // MD 2/2/12 - TFS100573
		//        //IGenericCachedCollection collection = this.Element.Collection;
		//        IGenericCachedCollectionEx collection = this.Element.Collection;
		//        if (collection == null)
		//        {
		//            Utilities.DebugFail("The getter for this property should not be called when the collection is null.");
		//            return false;
		//        }

		//        return this.Element.HasSameData(collection.DefaultElement);
		//    }
		//}

		//#endregion HasDefaultValue

		#endregion // Removed

		// MD 1/8/12 - 12.1 - Cell Format Updates
		// The empty element is no longer needed.
		#region Removed

		//// MD 3/21/11 - TFS65198
		//#region HasEmptyValue

		//public bool HasEmptyValue
		//{
		//    get
		//    {
		//        // MD 12/21/11 - 12.1 - Table Support
		//        // The proxies no longer store a reference to the collection. It is stored on the element.
		//        //if (this.collection == null)
		//        //{
		//        //    Utilities.DebugFail("The getter for this property should not be called when the collection is null.");
		//        //    return false;
		//        //}
		//        //
		//        //return this.Element.HasSameData(this.collection.EmptyElement);
		//        IGenericCachedCollection collection = this.Element.Collection;
		//        if (collection == null)
		//        {
		//            Utilities.DebugFail("The getter for this property should not be called when the collection is null.");
		//            return false;
		//        }

		//        return this.Element.HasSameData(collection.EmptyElement);
		//    }
		//}

		//#endregion HasEmptyValue

		#endregion // Removed

		// MD 12/22/11 - 12.1 - Table Support
		// This is no longer needed.
		#region Removed

		//#region Workbook

		//// MD 7/26/10 - TFS34398
		//// We don't store the workbook on the proxy anymore so let the derived class return this.
		////public Workbook Workbook
		////{
		////    get { return this.workbook; }
		////}
		//public abstract Workbook Workbook { get; }

		//#endregion Workbook

		#endregion // Removed

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