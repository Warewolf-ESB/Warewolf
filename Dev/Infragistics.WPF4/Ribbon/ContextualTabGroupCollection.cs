using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using Infragistics.Windows.Helpers;
using System.Collections;
using System.Diagnostics;
using System.Collections.Specialized;
using Infragistics.Shared;
using Infragistics.Collections;

namespace Infragistics.Windows.Ribbon
{
	/// <summary>
	/// An observable collection of <see cref="ContextualTabGroup"/>s.
	/// </summary>
	/// <remarks>
	/// <p class="body">Represents a grouping of <see cref="RibbonTabItem"/>s that relates to a specific context that has meaning to the application that is hosting 
	/// the <see cref="XamRibbon"/>.</p>
	/// </remarks>
	/// <seealso cref="XamRibbon"/>
	/// <seealso cref="RibbonTabItem"/>
	/// <seealso cref="ContextualTabGroup"/>
	/// <seealso cref="ContextualTabItemCollection"/>
	public class ContextualTabGroupCollection : ObservableCollectionExtended<ContextualTabGroup>
	{
		#region Member Variables

		private Dictionary<string, ContextualTabGroup>				_keysHash = new Dictionary<string,ContextualTabGroup>(3);
 
		#endregion //Member Variables

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="ContextualTabGroupCollection"/> class.
		/// </summary>
		public ContextualTabGroupCollection()
			: base()
		{
		}

		#endregion //Constructor

		#region Base Class Overrides

			#region NotifyItemsChanged
		/// <summary>
		/// Indicates to the base class that the <see cref="OnItemAdded(ContextualTabGroup)"/> and <see cref="OnItemRemoved(ContextualTabGroup)"/> methods should be invoked.
		/// </summary>
		protected override bool NotifyItemsChanged
		{
			get
			{
				return true;
			}
		}
			#endregion //NotifyItemsChanged

			#region OnCollectionChanged
		/// <summary>
		/// Invoked when the collection has changed.
		/// </summary>
		/// <param name="e">Event arguments that provides information about the change</param>
		protected override void OnCollectionChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			// initialize the indexes of the groups
			for (int i = 0, count = this.Count; i < count; i++)
				this[i].Index = i;

			base.OnCollectionChanged(e);
		} 
			#endregion //OnCollectionChanged

			#region OnItemAdding
		/// <summary>
		/// Invoked before a tab group is added to the collection.
		/// </summary>
		/// <param name="itemAdded">The tab group that is being added</param>
		protected override void OnItemAdding(ContextualTabGroup itemAdded)
		{
			// AS 9/24/07 - Don't require contextualtabgroup keys.
			//if (string.IsNullOrEmpty(itemAdded.Key))
			//	throw new InvalidOperationException("Cannot set ContextualTabGroup Key to null or string.Empty!  ContextualTabGroups added to the ContextualTabGroupCollection must have unique values for their Key property!");

			// AS 9/24/07 - Don't require contextualtabgroup keys.
			//if (this._keysHash.ContainsKey(itemAdded.Key))
			if (false == string.IsNullOrEmpty(itemAdded.Key) && this._keysHash.ContainsKey(itemAdded.Key))
				throw new InvalidOperationException(XamRibbon.GetString("LE_DuplicateKey", itemAdded.Key));

			base.OnItemAdding(itemAdded);
		}
			#endregion //OnItemAdding

			#region OnItemAdded
		/// <summary>
		/// Invoked when a tab group is added to the collection.
		/// </summary>
		/// <param name="itemAdded">The tab group that was added</param>
		protected override void OnItemAdded(ContextualTabGroup itemAdded)
		{
			// AS 9/24/07 - Don't require contextualtabgroup keys.
			if (string.IsNullOrEmpty(itemAdded.Key) == false)
				this._keysHash.Add(itemAdded.Key, itemAdded);

			base.OnItemAdded(itemAdded);
		}
			#endregion //OnItemAdded

			#region OnItemPropertyChanged

		/// <summary>
		/// Raises the Infragistics.Windows.Helpers.ObservableCollectionExtended.ItemPropertyChanged event with the specified arguments.
		/// </summary>
		/// <param name="e">Event arguments that provide information about the property that changed.</param>
		protected override void OnItemPropertyChanged(ItemPropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Key")
			{
				ContextualTabGroup contextualTabGroup = e.Item as ContextualTabGroup;
				if (contextualTabGroup != null)
				{
					// AS 9/24/07 - Don't require contextualtabgroup keys.
					//if (string.IsNullOrEmpty(contextualTabGroup.Key))
					//	throw new InvalidOperationException("Cannot set ContextualTabGroup Key to null or string.Empty!  ContextualTabGroups added to the ContextualTabGroupCollection must have unique values for their Key property!");

					// AS 9/24/07 - Don't require contextualtabgroup keys.
					//if (this._keysHash.ContainsKey(contextualTabGroup.Key))
					if (false == string.IsNullOrEmpty(contextualTabGroup.Key) && this._keysHash.ContainsKey(contextualTabGroup.Key))
						throw new InvalidOperationException(XamRibbon.GetString("LE_DuplicateKey", contextualTabGroup.Key));

					// Remove the old entry in the hash table.  Since we don't know the previous value of the Key property, iterate throught the hashtable
					// until we find it.
					foreach (KeyValuePair<string, ContextualTabGroup> item in this._keysHash)
					{
						if (item.Value.Equals(contextualTabGroup))
						{
							this._keysHash.Remove(item.Key);

							// AS 9/24/07 - Don't require contextualtabgroup keys.
							if (string.IsNullOrEmpty(contextualTabGroup.Key))
								this._keysHash.Add(contextualTabGroup.Key, contextualTabGroup);

							break;
						}
					}
				}
			}

			base.OnItemPropertyChanged(e);
		}

			#endregion //OnItemPropertyChanged	
        
			#region OnItemRemoved
		/// <summary>
		/// Invoked when a tab group is removed from the collection.
		/// </summary>
		/// <param name="itemRemoved">The tab group that was removed</param>
		protected override void OnItemRemoved(ContextualTabGroup itemRemoved)
		{
			// AS 9/24/07 - Don't require contextualtabgroup keys.
			//if (this._keysHash.ContainsKey(itemRemoved.Key))
			if (false == string.IsNullOrEmpty(itemRemoved.Key) && this._keysHash.ContainsKey(itemRemoved.Key))
				this._keysHash.Remove(itemRemoved.Key);

			base.OnItemRemoved(itemRemoved);
		}
			#endregion //OnItemRemoved

		#endregion //Base Class Overrides

		#region Indexer (string key)

		/// <summary>
		/// Returns the <see cref="ContextualTabGroup"/> with the specified key.
		/// </summary>
		/// <param name="key">The key of the ContextualTabGroup to return.</param>
		/// <returns>The ContextualTabGroup with the specified key. or null if not ContextualTabGroup exists with the specified key.</returns>
		public ContextualTabGroup this[string key]
		{
			get
			{
				ContextualTabGroup tabGroup;

				if (this._keysHash.TryGetValue(key, out tabGroup))
					return tabGroup;

				return null;
			}
		}

		#endregion //Indexer (string key)

		#region Methods

			#region Public Methods

				#region Add(string key)

		/// <summary>
		/// Creates and adds a <see cref="ContextualTabGroup"/> with the specified key.
		/// </summary>
		/// <exception cref="InvalidOperationException">If an attempt is made to add a <see cref="ContextualTabGroup"/> whose 
		/// <see cref="Infragistics.Windows.Ribbon.ContextualTabGroup.Key"/> property value is null or string.Empty</exception>
		/// <exception cref="InvalidOperationException">If an attempt is made to add a <see cref="ContextualTabGroup"/> whose 
		/// <see cref="Infragistics.Windows.Ribbon.ContextualTabGroup.Key"/> property value conflicts
		/// with the value of the <see cref="Infragistics.Windows.Ribbon.ContextualTabGroup.Key"/> property of an existing 
		/// <see cref="ContextualTabGroup"/> in the collection.</exception>
		/// <param name="key">The key to assign to the newly created ContextualTabGroup.</param>
		/// <returns>The newly created ContextualTabGroup with the specified key.</returns>
		public ContextualTabGroup Add(string key)
		{
			ContextualTabGroup newContextualTabGroup = new ContextualTabGroup(key);
			this.Add(newContextualTabGroup);

			return newContextualTabGroup;
		}

				#endregion //Add(string key)

				#region ContainsKey

		/// <summary>
		/// Determines if the <see cref="ContextualTabGroupCollection"/> contains an item with the specified key.
		/// </summary>
		/// <param name="key">The <see cref="ContextualTabGroup"/> key to look for.</param>
		/// <returns>True if the ContextualTabGroupCollection contains an item with the specified key, otherwise false.</returns>
		public bool ContainsKey(string key)
		{
			return this._keysHash.ContainsKey(key);
		}

				#endregion //ContainsKey	

				#region Remove(string key)

		/// <summary>
		/// Removes the <see cref="ContextualTabGroup"/> with the specified key.
		/// </summary>
		/// <exception cref="InvalidOperationException">If an attempt is made to remove a <see cref="ContextualTabGroup"/> whose 
		/// <see cref="Infragistics.Windows.Ribbon.ContextualTabGroup.Key"/> property value does not exist</exception>
		/// <param name="key">The key of the ContextualTabGroup to remove.</param>
		public void Remove(string key)
		{
			if (this._keysHash.ContainsKey(key) == false)
				throw new InvalidOperationException(XamRibbon.GetString("LE_ContextualTabGroupKeyMissing"));

			this.Remove(this._keysHash[key]);
		}

				#endregion //Remove(string key)
    
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