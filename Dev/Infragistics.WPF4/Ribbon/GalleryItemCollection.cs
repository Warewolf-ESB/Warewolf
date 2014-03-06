using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.Collections;
using System.Diagnostics;
using Infragistics.Windows.Helpers;
using Infragistics.Shared;
using Infragistics.Collections;

namespace Infragistics.Windows.Ribbon
{
	/// <summary>
	/// An observable  collection of <see cref="GalleryItem"/> instances.
	/// </summary>
	/// <remarks>
	/// <p class="note"><b>Note: </b>Every <see cref="GalleryItem"/> added to the collection must have a unique value set on its <see cref="GalleryItem.Key"/> 
	/// property.  If an attempt is made to add a <see cref="GalleryItem"/> whose <see cref="GalleryItem.Key"/> property value conflicts with the value of the
	/// <see cref="GalleryItem.Key"/> property of an existing <see cref="GalleryItem"/> in the collection, an exception will be thrown.</p>
	/// </remarks>
	/// <exception cref="InvalidOperationException">If an attempt is made to add a <see cref="GalleryItem"/> whose <see cref="GalleryItem.Key"/> property value conflicts
	/// with the value of the <see cref="GalleryItem.Key"/> property of an existing <see cref="GalleryItem"/> in the collection</exception>
	/// <seealso cref="GalleryItem"/>
	/// <seealso cref="GalleryItem.Key"/>
	public class GalleryItemCollection : ObservableCollectionExtended<GalleryItem>
	{
		#region Member Variables

		private Dictionary<string, GalleryItem>					_keysHash = new Dictionary<string,GalleryItem>(10);
 
		#endregion //Member Variables

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="GalleryItemCollection"/> class.
		/// </summary>
		public GalleryItemCollection()
			: base(new List<GalleryItem>())
		{
		}

		#endregion //Constructor

		#region Base Class Overrides

			// AS 11/14/07 BR28451
			// I made a couple of changes here. First, I changes _keysHash from a hashtable to a dictionary to avoid unnecessary
			// casting since we're always dealing with specific types. Second, I overrode NotifyItemsChanged so we can override
			// the OnItemAdding, OnItemRemoved, etc. so we get a notification for every item even when the collection is 
			// cleared, etc.
			//
			#region NotifyItemsChanged
		/// <summary>
		/// Indicates to the base class that the <see cref="OnItemAdding(GalleryItem)"/> and <see cref="OnItemRemoved(GalleryItem)"/> methods should be invoked.
		/// </summary>
		protected override bool NotifyItemsChanged
		{
			get
			{
				return true;
			}
		}
			#endregion //NotifyItemsChanged

			#region OnItemAdding
		/// <summary>
		/// Invoked when a GalleryItem is being added to the collection.
		/// </summary>
		/// <param name="itemAdded">The GalleryItem that is about to be added to the collection</param>
		protected override void OnItemAdding(GalleryItem itemAdded)
		{
			if (string.IsNullOrEmpty(itemAdded.Key))
				throw new InvalidOperationException(XamRibbon.GetString("LE_InvalidGalleryItemKey"));

			if (this._keysHash.ContainsKey(itemAdded.Key))
				throw new InvalidOperationException(XamRibbon.GetString("LE_DuplicateKey", itemAdded.Key));

			this._keysHash.Add(itemAdded.Key, itemAdded);

			base.OnItemAdding(itemAdded);
		} 
			#endregion //OnItemAdding

			#region OnItemRemoved
		/// <summary>
		/// Invoked when a GalleryItem is removed from the collection.
		/// </summary>
		/// <param name="itemRemoved">The GalleryItem that was remove from the collection</param>
		protected override void OnItemRemoved(GalleryItem itemRemoved)
		{
			this._keysHash.Remove(itemRemoved.Key);

			base.OnItemRemoved(itemRemoved);
		}
			#endregion //OnItemRemoved

			#region OnItemPropertyChanged

		/// <summary>
		/// Raises the Infragistics.Windows.Helpers.ObservableCollectionExtended.ItemPropertyChanged event with the specified arguments.
		/// </summary>
		/// <param name="e">Event arguments that provide information about the property that changed.</param>
		protected override void OnItemPropertyChanged(ItemPropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Key")
			{
				GalleryItem galleryItem = e.Item as GalleryItem;
				if (galleryItem != null)
				{
					if (string.IsNullOrEmpty(galleryItem.Key))
						throw new InvalidOperationException(XamRibbon.GetString("LE_InvalidGalleryItemKey"));

					if (this._keysHash.ContainsKey(galleryItem.Key))
						throw new InvalidOperationException(XamRibbon.GetString("LE_DuplicateKey", galleryItem.Key));

					// Remove the old entry in the hash table.  Since we don't know the previous value of the Key property, iterate throught the hashtable
					// until we find it.
					foreach (GalleryItem item in this._keysHash.Values)
					{
						if (item.Equals(galleryItem))
						{
							this._keysHash.Remove(item.Key);
							this._keysHash.Add(galleryItem.Key, galleryItem);
							break;
						}
					}
				}
			}

			base.OnItemPropertyChanged(e);
		}

			#endregion //OnItemPropertyChanged	
    
		#endregion //Base Class Overrides

		#region Indexer (string key)

		/// <summary>
		/// Returns the element with the specified key.
		/// </summary>
		/// <param name="key">The key of the element to return.</param>
		/// <returns>T GalleryItem with the specified key. or null if not GalleryItem exists with the specified key.</returns>
		public GalleryItem this[string key]
		{
			get
			{
				GalleryItem item = null;

				if (string.IsNullOrEmpty(key) == false && this._keysHash.TryGetValue(key, out item))
					return item;

				return null;
			}
		}

		#endregion //Indexer (string key)

		#region Methods

			#region Public Methods

				#region ContainsKey

		/// <summary>
		/// Determines if the <see cref="GalleryItemCollection"/> contains an item with the specified key.
		/// </summary>
		/// <param name="key">The <see cref="GalleryItem"/> key to look for.</param>
		/// <returns>True if the GalleryItemCollection contains an item with the specified key, otherwise false.</returns>
		public bool ContainsKey(string key)
		{
			return this._keysHash.ContainsKey(key);
		}

				#endregion //ContainsKey	
    
			#endregion //Public Methods

		#endregion //Methods
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