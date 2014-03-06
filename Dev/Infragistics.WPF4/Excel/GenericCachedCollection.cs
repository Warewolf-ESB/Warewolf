using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Diagnostics;
using System.Globalization;


using Infragistics.Shared;


namespace Infragistics.Documents.Excel
{
	internal class GenericCachedCollection<T> : 
		ICollection<T>
		where T : GenericCacheElement
	{
		#region Member Variables

		// MD 1/9/12 - 12.1 - Cell Format Updates
		// The empty element no longer needs to be stored.
		//// MD 2/15/11 - TFS66333
		//// Cache the empty element so we can easily store a default data element with out creating a new instance each time.
		//// The defaultElement is not truly default because it is populated with data when the workbook is loaded from a file 
		//// or stream. 
		//private T emptyElement;

		// MD 2/2/12 - TFS100573
		// Moved this to the derived GenericCachedCollectionEx collection.
		//private T defaultElement;

		// MD 5/31/11 - TFS75574
		// Use a custom hash set to save space because a dictionary will store the element in the keys and values collections.
		// This cuts the space needed for the cache is half.
		//private Dictionary<T, T> cache;
		internal GenericElementHashSet<T> cache;

		private Workbook workbook;
		private int maxCount;

		#endregion Member Variables

		#region Constructor

		// MD 2/2/12 - TFS100573
		// Moved the default element to the derived GenericCachedCollectionEx collection.
		//public GenericCachedCollection( T defaultElement, Workbook workbook, int maxCount )
		public GenericCachedCollection(Workbook workbook, int maxCount)
			// MD 5/31/11 - TFS75574
			// Moved all code to the new overload.
			// MD 2/2/12 - TFS100573
			//: this(defaultElement, workbook, maxCount, 0.2) { }
			: this(workbook, maxCount, 0.2) { }

		// MD 5/31/11 - TFS75574
		// Added a new overload to take a load factor for the internal hash set.
		// MD 2/2/12 - TFS100573
		// Moved the default element to the derived GenericCachedCollectionEx collection.
		//public GenericCachedCollection(T defaultElement, Workbook workbook, int maxCount, double maxLoadFactor)
		public GenericCachedCollection(Workbook workbook, int maxCount, double maxLoadFactor)
		{
			// MD 5/31/11 - TFS75574
			// Use a custom hash set to save space because a dictionary will store the element in the keys and values collections.
			//this.cache = new Dictionary<T, T>();
			this.cache = new GenericElementHashSet<T>(maxLoadFactor);

			this.workbook = workbook;
			this.maxCount = maxCount;

			// MD 6/30/11 - TFS78207
			// Moved this to the if block below because we only really need to do this when the defaultElement is not null.
			//// MD 2/15/11 - TFS66333
			//this.emptyElement = defaultElement;

			// MD 2/2/12 - TFS100573
			// Moved the default element to the derived GenericCachedCollectionEx collection.
			//// MD 11/3/10 - TFS49093
			//// We now allow null to be passed into the constructor.
			////this.DefaultElement = defaultElement;
			//if (defaultElement != null)
			//{
			//    // MD 1/8/12 - 12.1 - Cell Format Updates
			//    // The empty element no longer needs to be stored.
			//    //// MD 6/30/11 - TFS78207
			//    //// Moved this line from above. Also bumped the reference count for this element because we never want the empty element
			//    //// to change. Normally, it wouldn't change because setting the DefaultElement property would also increment the reference
			//    //// count, but in the case where a workbook is loaded from a file, the DefaultElement is changed and assigned a new value
			//    //// which decremented the reference count on the original default element. So the reference count was dropping to zero even
			//    //// though it was still referenced by the emptyElement member variable of the collection.
			//    //this.emptyElement = defaultElement;
			//    //this.emptyElement.BumpReferenceCount();
			//
			//    this.DefaultElement = defaultElement;
			//}
		}

		#endregion Constructor

		#region Interfaces

		#region ICollection<T> Members

		void ICollection<T>.Add( T item )
		{
			this.Add( item );
		}

		void ICollection<T>.Clear()
		{
			Utilities.DebugFail( "This collection should not be cleared" );
		}

		bool ICollection<T>.Contains( T item )
		{
			// MD 5/31/11 - TFS75574
			//return this.cache.ContainsValue( item );
			T tmp;
			return this.cache.Contains(item, out tmp);
		}

		void ICollection<T>.CopyTo( T[] array, int arrayIndex )
		{
			// MD 5/31/11 - TFS75574
			//this.cache.Values.CopyTo( array, arrayIndex );
			this.cache.CopyTo(array, arrayIndex);
		}

		int ICollection<T>.Count
		{
			get { return this.Count; }
		}

		bool ICollection<T>.IsReadOnly
		{
			get { return false; }
		}

		bool ICollection<T>.Remove( T item )
		{
			return this.Remove( item );
		}

		#endregion

		#region IEnumerable<T> Members

		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			// MD 5/31/11 - TFS75574
			//return this.cache.Values.GetEnumerator();
			return this.cache.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			// MD 5/31/11 - TFS75574
			//return this.cache.Values.GetEnumerator();
			return this.cache.GetEnumerator();
		}

		#endregion

		#endregion Interfaces

		#region Methods

		#region Add

		// MD 5/31/11 - TFS75574
		// Refactored this for performance into an Add and an OnAdded method.
		//// MD 4/12/11 - TFS67084
		//// Made virtual so this could be overridden in derived classes.
		////public void Add( T element )
		//public virtual void Add(T element)
		//{
		//    this.cache.Add( element, element );
		//    element.OnAddedToRootCollection();
		//
		//    if (this.cache.Count > this.maxCount)
		//    {
		//        // MBS 7/15/08 - Excel 2007 Format
		//        //throw new InvalidOperationException( SR.GetString( "LE_InvalidOperationException_MaxCellFormats", Workbook.MaxExcelCellFormatCount ) );
		//        if (element is WorksheetCellFormatData)
		//            throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_MaxCellFormats", this.maxCount));
		//        // MD 4/12/11 - TFS67084
		//        // Added a case for strings because they are now represented as generic cache elements.
		//        else if (element is FormattedStringElement)
		//            throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_MaxFormattedStrings", this.maxCount));
		//        else
		//            throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_MaxFonts", this.maxCount));
		//    }   
		//}
		public void Add(T element)
		{
			T existingElement;
			if (this.cache.AddIfItemDoesntExist(element, out existingElement))
				this.OnAdded(element);
		}

		#endregion Add

		#region Find

		public T Find( T element )
		{
			T mappedElement;

			// MD 5/31/11 - TFS75574
			//if ( this.cache.TryGetValue( element, out mappedElement ) )
			if (this.cache.Contains(element, out mappedElement))
				return mappedElement;

			return null;
		}

		#endregion Find

		// MD 3/13/12 - 12.1 - Table Support
		#region FindIndex

		internal int FindIndex(T element)
		{
			int hashCode = HashHelpers.InternalGetHashCode(element);

			int entryListCount;
			int cumulativeCount;
			GenericElementHashSet<T>.Entry[] entryList = this.cache.GetEntryList(hashCode, out entryListCount, out cumulativeCount);

			if (entryList != null)
			{
				for (int i = 0; i < entryListCount; i++)
				{
					GenericElementHashSet<T>.Entry entry = entryList[i];
					if (entry.hashCode != hashCode)
						continue;

					T other = entry.element;

					if (other == null)
						break;

					if (Object.Equals(element, other))
						return cumulativeCount + i;
				}
			}

			Utilities.DebugFail("This shouldn't happen.");
			return -1;
		}

		#endregion // FindIndex

		// MD 5/31/11 - TFS75574
		#region FindOrAdd

		public void FindOrAdd(T element, out T equivalentElement)
		{
			if (this.cache.AddIfItemDoesntExist(element, out equivalentElement))
			{
				equivalentElement = element;
				this.OnAdded(element);
			}
		}

		#endregion  // FindOrAdd

		// MD 5/31/11 - TFS75574
		#region OnAdded

		protected virtual void OnAdded(T element)
		{
			if (this.cache.Count > this.maxCount)
			{
				if (element is WorksheetCellFormatData)
					throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_MaxCellFormats", this.maxCount));
				else if (element is StringElement)
					throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_MaxFormattedStrings", this.maxCount));
				else
					throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_MaxFonts", this.maxCount));
			}
		}

		#endregion  // OnAdded

		// MD 2/1/12 - TFS100573
		#region OnSaving

		public void OnSaving()
		{
			this.cache.OnSaving();
		}

		#endregion // OnSaving

		// MD 2/1/12 - TFS100573
		#region OnSaved

		public void OnSaved()
		{
			this.cache.OnSaved();
		}

		#endregion // OnSaved

        #region Remove

		// MD 4/12/11 - TFS67084
		// Made virtual so this could be overridden in derived classes.
        //public bool Remove( T element )
		public virtual bool Remove(T element)
		{
			return this.cache.Remove( element );
		}

		#endregion Remove

		// MD 1/16/12 - 12.1 - Cell Format Updates
		// This is no longer needed.
		#region Removed

		//// MD 1/14/08 - BR29635
		//#region RemoveUsedColorIndicies

		//internal void RemoveUsedColorIndicies( List<int> unusedIndicies )
		//{
		//    if ( this.defaultElement != null )
		//        this.defaultElement.RemoveUsedColorIndicies( unusedIndicies );

		//    // MD 5/31/11 - TFS75574
		//    //foreach ( T element in this.cache.Keys )
		//    foreach (T element in this.cache)
		//        element.RemoveUsedColorIndicies( unusedIndicies );
		//}

		//#endregion RemoveUsedColorIndicies

		#endregion // Removed

        #endregion Methods

        #region Properties

        #region Count

        public int Count
		{
			get { return this.cache.Count; }
		}

		#endregion Count

		// MD 2/2/12 - TFS100573
		// Moved the default element to the derived GenericCachedCollectionEx collection.
		#region Removed

		//#region DefaultElement

		//public T DefaultElement
		//{
		//    get { return this.defaultElement; }
		//    set
		//    {
		//        Debug.Assert( value != null );

		//        if ( this.defaultElement != null )
		//        {
		//            // MD 12/21/11 - 12.1 - Table Support
		//            //GenericCacheElement.Release( this.defaultElement, this );
		//            GenericCacheElement.Release(this.defaultElement);
		//        }

		//        // MD 1/10/12 - 12.1 - Cell Format Updates
		//        // The element may be referenced by other things, so we can't just replace it. Only replace it if it is unreferenced.
		//        // Otherwise, copy the values to the existing element and incremeent its reference count.
		//        //this.defaultElement = GenericCacheElement.FindExistingOrAddToCache( value, this );
		//        if (this.defaultElement != null && this.defaultElement.ReferenceCount > 0)
		//        {
		//            this.defaultElement.CopyValues(value);
		//            this.defaultElement.IncrementReferenceCount();
		//        }
		//        else
		//        {
		//            this.defaultElement = GenericCacheElement.FindExistingOrAddToCache(value, this);
		//        }

		//        // MD 1/10/12 - 12.1 - Cell Format Updates
		//        Debug.Assert(ReferenceEquals(this.defaultElement, this.orderedElements[0]), "The default element should always be the first item in the list.");
		//    }
		//}

		//#endregion DefaultElement

		#endregion // Removed

		// MD 1/8/12 - 12.1 - Cell Format Updates
		// The empty element no longer needs to be stored.
		#region Removed

		//// MD 2/15/11 - TFS66333
		//// Cache the empty element so we can easily store a default data element with out creating a new instance each time.
		//// The defaultElement is not truly default because it is populated with data when the workbook is loaded from a file 
		//// or stream. 
		//#region EmptyElement

		//public T EmptyElement
		//{
		//    get { return this.emptyElement; }
		//}

		//#endregion // EmptyElement

		#endregion // Removed

		// MBS 7/15/08 - Excel 2007 Format
        #region MaxCount

        public int MaxCount
        {
            get { return this.maxCount; }
            set 
            {
                if (this.Count > value)
                    throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_CollectionLongerThanMaxValue"));

                this.maxCount = value; 
            }
        }
        #endregion //MaxCount

        #region Workbook

        public Workbook Workbook
		{
			get { return this.workbook; }
		}

		#endregion Workbook

		#endregion Properties
	}

	// MD 2/2/12 - TFS100573
	internal class GenericCachedCollectionEx<T> : GenericCachedCollection<T>
		, IGenericCachedCollectionEx 
		where T : GenericCacheElementEx
	{
		#region Member Variables

		private T defaultElement;

		#endregion // Member Variables

		#region Constructor

		public GenericCachedCollectionEx(T defaultElement, Workbook workbook, int maxCount)
			: base(workbook, maxCount)
		{
			if (defaultElement != null)
				this.DefaultElement = defaultElement;
		}

		#endregion // Constructor

		#region Methods

		#region OnAdded

		protected override void OnAdded(T element)
		{
			Debug.Assert(element.IsFrozen == false, "Frozen elements cannot be added to the collection.");
			element.OnAddedToRootCollection(this);

			base.OnAdded(element);
		}

		#endregion // OnAdded

		#region Remove

		public override bool Remove(T element)
		{
			if (base.Remove(element) == false)
				return false;

			element.OnRemovedFromRootCollection();
			return true;
		}

		#endregion // Remove

		#endregion // Methods

		#region Properties

		#region DefaultElement

		public T DefaultElement
		{
			get { return this.defaultElement; }
			set
			{
				Debug.Assert(value != null);

				if (this.defaultElement != null)
				{
					// MD 12/21/11 - 12.1 - Table Support
					//GenericCacheElement.Release( this.defaultElement, this );
					GenericCacheElementEx.ReleaseEx(this.defaultElement);
				}

				// MD 1/10/12 - 12.1 - Cell Format Updates
				// The element may be referenced by other things, so we can't just replace it. Only replace it if it is unreferenced.
				// Otherwise, copy the values to the existing element and increment its reference count.
				//this.defaultElement = GenericCacheElement.FindExistingOrAddToCache( value, this );
				if (this.defaultElement != null && this.defaultElement.ReferenceCount > 0)
				{
					this.defaultElement.CopyValues(value);
					this.defaultElement.IncrementReferenceCount();
				}
				else
				{
					this.defaultElement = GenericCacheElementEx.FindExistingOrAddToCacheEx(value, this);
				}
			}
		}

		#endregion DefaultElement

		#endregion // Properties
	}

	// MD 12/21/11 - 12.1 - Table Support
	internal interface IGenericCachedCollectionEx
	{
		Workbook Workbook { get; }
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