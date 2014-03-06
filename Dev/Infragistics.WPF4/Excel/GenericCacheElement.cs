using System;
using System.Collections;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;

namespace Infragistics.Documents.Excel
{


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

    internal abstract class GenericCacheElement
// MD 2/2/12 - TFS100573
//#if !SILVERLIGHT
//        : ICloneable
//#endif
	{
		#region Member Variables

		private int referenceCount;

		// MD 2/2/12 - TFS100573
		// The base element no longer needs to store a reference to the workbook.
		//private Workbook workbook;

		#endregion Member Variables

		#region Constructor

		// MD 2/2/12 - TFS100573
		// The base element no longer needs to store a reference to the workbook.
		//public GenericCacheElement( Workbook workbook )
		//{
		//    this.workbook = workbook;
		//}
		public GenericCacheElement() 
		{
		}

		#endregion Constructor

		#region Base Class Overrides

		#region Equals

		public override bool Equals( object obj )
		{
			Utilities.DebugFail( "Internal: Must override Equals(object) in derived class" );
			return base.Equals( obj );
		}

		#endregion Equals

		#region GetHashCode

		public override int GetHashCode()
		{
			Utilities.DebugFail( "Internal: Must override GetHashCode() in derived class" );
			return base.GetHashCode();
		}

		#endregion GetHashCode

		#endregion Base Class Overrides

		#region Methods

		#region Abstract Methods

		// MD 2/2/12 - TFS100573
		//public abstract object Clone();
		public abstract object Clone(Workbook workbook);

		public abstract bool HasSameData( GenericCacheElement otherElement );

		// MD 1/16/12 - 12.1 - Cell Format Updates
		// This is no longer needed.
		//// MD 1/14/08 - BR29635
		//public abstract void RemoveUsedColorIndicies( List<int> unusedIndicies );

		#endregion Abstract Methods

		#region Public Methods

		// MD 1/8/12 - 12.1 - Cell Format Updates
		#region DecrementReferenceCount

		public void DecrementReferenceCount()
		{
			this.referenceCount--;
		}

		#endregion // DecrementReferenceCount

		#region IncrementReferenceCount

		// MD 6/30/11 - TFS78207
		// MD 1/8/12 - 12.1 - Cell Format Updates
		// Renamed for clarity.
		//public void BumpReferenceCount()
		public void IncrementReferenceCount()
		{
			this.referenceCount++;
		}

		#endregion // IncrementReferenceCount

		#endregion // Public Methods

		#region Virtual Methods

		// MD 2/2/12 - TFS100573
		//public virtual void OnAddedToRootCollection() { }

		// MD 1/19/12 - 12.1 - Cell Format Updates
		// This is no longer needed.
		//// MD 11/24/10 - TFS34598
		//public virtual void OnCurrentFormatChanged() { }

		// MD 2/2/12 - TFS100573
		//public virtual void OnRemovedFromRootCollection() { }

		// MD 12/30/11 - 12.1 - Cell Format Updates
		protected virtual void OnWorkbookChanged(bool resetStyle) { }

		// MD 1/19/12 - 12.1 - Cell Format Updates
		// This is no longer needed.
		//// MBS 7/15/08 - Excel 2007 Format
		//public virtual void VerifyFormatLimits(FormatLimitErrors errors, WorkbookFormat testFormat) { }

		#endregion Virtual Methods

		#region Static Methods

		// MD 2/2/12 - TFS100573
		// Moved from GenericCacheElementProxy
		#region AfterSet

		public static void AfterSet<T>(GenericCachedCollection<T> collection, ref T element) where T : GenericCacheElement
		{
			element = GenericCacheElement.FindExistingOrAddToCache(element, collection);
		}

		#endregion // AfterSet

		// MD 2/2/12 - TFS100573
		// Moved from GenericCacheElementProxy
		#region BeforeSet

		public static void BeforeSet<T>(GenericCachedCollection<T> collection, ref T element, bool willModifyElement) where T : GenericCacheElement
		{
			GenericCacheElement.Release(element, collection);

			if (willModifyElement && element.ReferenceCount > 0)
				element = (T)element.Clone(collection.Workbook);
		}

		#endregion // BeforeSet

		#region FindExistingOrAddToCache

		internal static T FindExistingOrAddToCache<T>( T newElement, GenericCachedCollection<T> collection ) where T : GenericCacheElement
		{
			T equivalentElement;

			if ( collection == null )
			{
				// If there is no collection for this element, clone the existing one so the proxy calling this can 
				// have a unique element that it has the only reference to.
				// MD 9/12/08 - 8.3 Performance
				// We don't need to clone the element if it doesn't belong to a collection.
				//equivalentElement = (T)newElement.Clone();
				if ( newElement.ReferenceCount <= 0 )
				{
					equivalentElement = newElement;
				}
				else
				{
					// MD 2/2/12 - TFS100573
					//equivalentElement = (T)newElement.Clone();
					equivalentElement = (T)newElement.Clone(null);
				}
			}
			else
			{
				// MD 5/31/11 - TFS75574
				// Rather than do one lookup in the hash set with the Find call and another with the Add call, combine them into one method 
				// that does both.
				//// If there is a collection associated, try to find an equivalent element in the collection.
				//equivalentElement = collection.Find( newElement );
				//
				//// If an equivalent element is not found in the collection, add the new element to the collection
				//if ( equivalentElement == null )
				//{
				//    collection.Add( newElement );
				//    equivalentElement = newElement;
				//}
				collection.FindOrAdd(newElement, out equivalentElement);

				// MD 4/12/11 - TFS67084
				// Moved into the else block from below. We are only going to increment the reference count when the element
				// belongs to a collection. Otherwise, it is only owned by owned proxy and therefore we don't need to keep track
				// of the reference count. We will increment the reference count when the proxy is rooted.
				// MD 1/8/12 - 12.1 - Cell Format Updates
				// Moved this code to the IncrementReferenceCount method so we could keep logic centralized.
				//equivalentElement.referenceCount++;
				equivalentElement.IncrementReferenceCount();
			}

			// MD 4/12/11 - TFS67084
			// Moved into else block above. See comment on other change.
			//// Add a reference to whatever element will be returned
			//equivalentElement.referenceCount++;

			return equivalentElement;
		}

		#endregion FindExistingOrAddToCache

		#region Release

		public static void Release<T>( T element, GenericCachedCollection<T> collection ) where T : GenericCacheElement
		{
			// Decrease the reference count of the element
			// MD 4/12/11 - TFS67084
			// Due to a change in FindExistingOrAddToCache, the reference count will only be incremented when the collection
			// is valid and therefore the proxy is rooted. Therefore, we should also only decrement the reference count when
			// the proxy is rooted.
			//element.referenceCount--;
			if (collection != null)
			{
				// MD 1/8/12 - 12.1 - Cell Format Updates
				// Moved this code to the DecrementReferenceCount method so we could keep logic centralized.
				//element.referenceCount--;
				element.DecrementReferenceCount();
			}

			Debug.Assert( element.referenceCount >= 0 );

			// If there are no more references to the element, remove it from the collection
			if ( collection != null && element.referenceCount <= 0 )
				collection.Remove( element );
		}

		#endregion Release

		// MD 2/2/12 - TFS100573
		// Moved from GenericCacheElementProxy
		#region SetCollection

		internal static void SetCollection<T>(GenericCachedCollection<T> newCollection, ref GenericCachedCollection<T> collection, ref T element) where T : GenericCacheElement
		{
			if (collection != newCollection)
			{
				GenericCacheElement.BeforeSet(collection, ref element, true);
				collection = newCollection;
				GenericCacheElement.AfterSet(collection, ref element);
			}

			Debug.Assert(collection != null || element.ReferenceCount == 0);
		}

		#endregion SetCollection

		#endregion Static Methods

		// MD 4/18/11 - TFS62026
		// Added a way to reset the reference count.
		#region ResetReferenceCount

		protected void ResetReferenceCount()
		{
			this.referenceCount = 0;
		}

		#endregion  // ResetReferenceCount

		#endregion Methods

		#region Properties

		#region ReferenceCount

		public int ReferenceCount
		{
			get { return this.referenceCount; }
		}

		#endregion ReferenceCount

		// MD 2/2/12 - TFS100573
		// The base element no longer needs to store a reference to the workbook.
		#region Removed

		//#region Workbook

		//public Workbook Workbook
		//{
		//    get { return this.workbook; }

		//    // MD 11/3/10 - TFS49093
		//    // Added the ability to set the workbook.
		//    set { this.workbook = value; }
		//}

		//#endregion Workbook

		#endregion // Removed

		#endregion Properties
	}

	// MD 2/2/12 - TFS100573
	// Added another base element type which does store a reference to the workbook.
	internal abstract class GenericCacheElementEx : GenericCacheElement
	{
		#region Member Variables

		// MD 12/12/11 - 12.1 - Table Support
		// Moved this from the GenericCacheElementProxy
		private IGenericCachedCollectionEx collection;

		private Workbook workbook;

		// MD 1/10/12 - 12.1 - Cell Format Updates
		private bool isFrozen;

		#endregion // Member Variables

		#region Constructor

		public GenericCacheElementEx(Workbook workbook)
		{
			this.workbook = workbook;
		}

		#endregion Constructor

		#region Base Class Overrides

		#region OnAddedToRootCollection

		public virtual void OnAddedToRootCollection(IGenericCachedCollectionEx collection)
		{
			Debug.Assert(this.collection == null, "The collection should not be set here.");
			this.Collection = collection;
		}

		#endregion // OnAddedToRootCollection

		#region OnRemovedFromRootCollection

		public virtual void OnRemovedFromRootCollection()
		{
			this.Collection = null;
		}

		#endregion // OnRemovedFromRootCollection

		#endregion // Base Class Overrides

		#region Methods

		public abstract void CopyValues(GenericCacheElement element);
		protected abstract void VerifyCanSetValue();

		#region AfterSetEx

		public static void AfterSetEx<T>(GenericCachedCollection<T> collection, ref T element) where T : GenericCacheElementEx
		{
			element = GenericCacheElementEx.FindExistingOrAddToCacheEx(element, collection);
		}

		#endregion // AfterSetEx

		#region BeforeSetEx

		public static GenericCachedCollection<T> BeforeSetEx<T>(ref T element, bool willModifyElement) where T : GenericCacheElementEx
		{
			GenericCachedCollection<T> collection = GenericCacheElementEx.ReleaseEx(element);

			if (willModifyElement && element.ReferenceCount > 0)
				element = (T)element.Clone();

			Debug.Assert(
				willModifyElement == false || (element.Collection is GenericCachedCollection<T>) == false, 
				"The collection should be null here.");

			return collection;
		}

		#endregion // BeforeSetEx

		#region Clone

		public object Clone()
		{
			return this.Clone(this.Workbook);
		}

		#endregion // Clone

		#region FindExistingOrAddToCache

		internal static T FindExistingOrAddToCacheEx<T>(T newElement, GenericCachedCollection<T> collection) where T : GenericCacheElementEx
		{
			T equivalentElement;

			if (collection == null)
			{
				if (newElement.ReferenceCount <= 0)
					equivalentElement = newElement;
				else
					equivalentElement = (T)newElement.Clone();
			}
			else
			{
				if (newElement.collection == collection)
					equivalentElement = newElement;
				else
					collection.FindOrAdd(newElement, out equivalentElement);

				equivalentElement.IncrementReferenceCount();
			}

			return equivalentElement;
		}

		#endregion FindExistingOrAddToCache

		#region ReleaseEx

		public static GenericCachedCollection<T> ReleaseEx<T>(T element) where T : GenericCacheElementEx
		{
			GenericCachedCollection<T> collection = element.Collection as GenericCachedCollection<T>;

			if (collection != null)
				element.DecrementReferenceCount();

			Debug.Assert(element.ReferenceCount >= 0);

			// If there are no more references to the element, remove it from the collection
			if (collection != null && element.ReferenceCount <= 0)
				collection.Remove(element);

			return collection;
		}

		#endregion // ReleaseEx

		#region SetCollectionEx

		internal static void SetCollectionEx<T>(GenericCachedCollection<T> newCollection, ref T element) where T : GenericCacheElementEx
		{
			GenericCachedCollection<T> collection = element.Collection as GenericCachedCollection<T>;

			if (collection != newCollection)
			{
				GenericCachedCollection<T> oldCollection = GenericCacheElementEx.BeforeSetEx(ref element, true);
				Debug.Assert(oldCollection == collection, "The old collection does not match what was stored on the element here.");

				collection = newCollection;
				GenericCacheElementEx.AfterSetEx(collection, ref element);
			}

			Debug.Assert(collection != null || element.ReferenceCount == 0);
		}

		#endregion // SetCollectionEx

		// MD 3/6/12 - 12.1 - Table Support
		#region SetWorkbookInternal

		internal void SetWorkbookInternal(Workbook value, bool resetStyle)
		{
			if (this.workbook == value)
				return;

			this.workbook = value;
			this.OnWorkbookChanged(resetStyle);
		}

		#endregion // SetWorkbookInternal

		#endregion // Methods

		#region Properties

		#region Collection

		public IGenericCachedCollectionEx Collection
		{
			get { return this.collection; }
			protected set
			{
				this.collection = value;

				if (this.collection != null)
				{
					if (this.Workbook == null)
					{
						this.Workbook = this.collection.Workbook;
					}
					else
					{
						Debug.Assert(collection.Workbook == null || collection.Workbook == this.Workbook,
							"The element is being added to a collection on another workbook.");
					}
				}
			}
		}

		#endregion // Collection

		// MD 1/15/12 - 12.1 - Cell Format Updates
		#region CurrentFormat

		public WorkbookFormat CurrentFormat
		{
			get
			{
				if (this.Workbook == null)
				{
					// MD 2/24/12
					// Found while implementing 12.1 - Table Support
					// We should use the least restrictive format version when there is no workbook, not the most.
					//return WorkbookFormat.Excel97To2003;
					return Workbook.LatestFormat;
				}

				return this.Workbook.CurrentFormat;
			}
		}

		#endregion // CurrentFormatv

		#region IsFrozen

		public bool IsFrozen
		{
			get { return this.isFrozen; }
			protected set { this.isFrozen = value; }
		}

		#endregion // IsFrozen

		#region Workbook

		public Workbook Workbook
		{
			get { return this.workbook; }

			// MD 12/22/11 - 12.1 - Table Support
			// This does not need to be set anymore.
			//// MD 11/3/10 - TFS49093
			//// Added the ability to set the workbook.
			//set { this.workbook = value; }

			// MD 12/28/11 - 12.1 - Cell Format Updates
			// We now need this again, but I made it protected so only derived classes could set it.
			protected set { this.SetWorkbookInternal(value, true); }
		}

		#endregion Workbook

		#endregion // Properties
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