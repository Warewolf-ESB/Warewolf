using System;
using System.Collections.ObjectModel;
using System.Globalization;
using Infragistics.Collections;
using System.ComponentModel;

namespace Infragistics.Controls.Menus
{
    /// <summary>
    /// A Collection of <see cref="NodeLayout"/> objects.
    /// </summary>
    public class GlobalNodeLayoutCollection : CollectionBase<NodeLayout>, IProvidePersistenceLookupKeys
    {
        #region Members

        XamDataTree _tree;

        #endregion // Members

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="GlobalNodeLayoutCollection"/> class.
        /// </summary>
        internal GlobalNodeLayoutCollection(XamDataTree tree)
            : base()
        {
            this._tree = tree;
        }
        #endregion // Constructor

        #region Methods

        #region Public

        #region FromType

        /// <summary>
        /// Searches the <see cref="GlobalNodeLayoutCollection"/> for a <see cref="NodeLayout"/> with the specified Type.
        /// And returns the first <see cref="NodeLayout"/> that represents the specified type. 
        /// </summary>
        /// <returns>Null if none of the <see cref="NodeLayout"/> objects have the specified Type.</returns>
        public NodeLayout FromType(Type type)
        {
            NodeLayout returnValue = null;

            if (type != null)
            {
                string typeName = type.Name.ToLower(CultureInfo.CurrentCulture);
                string typeFullName = type.FullName.ToLower(CultureInfo.CurrentCulture);
                foreach (NodeLayout layout in this.Items)
                {
                    string name = (layout.TargetTypeName != null) ? layout.TargetTypeName.ToLower(CultureInfo.CurrentCulture) : "";
                    if (name == typeName || name == typeFullName)
                    {
                        returnValue = layout;
                        break;
                    }
                }
            }

            return returnValue;
        }
        #endregion // FromType

        #region FromKey
        /// <summary>
        /// Searches the <see cref="GlobalNodeLayoutCollection"/> for a <see cref="NodeLayout"/> with the specified Key. And returns the 
        /// first <see cref="NodeLayout"/> with that key.
        /// </summary>
        /// <returns>Null if none of the <see cref="NodeLayout"/> objects have the specified Key.</returns>
        public NodeLayout FromKey(string key)
        {
            NodeLayout returnLayout = null;
            foreach (NodeLayout layout in this.Items)
            {
                if (layout.Key == key)
                {
                    returnLayout = layout;
                    break;
                }
            }
            return returnLayout;
        }
        #endregion // FromKey

        #endregion // Public

        #region Protected

        #region AddItemLocally

        /// <summary>
        /// Adds the item to the Collection silently, and sets the <see cref="NodeLayout.IsDefinedGlobally"/> property to false.
        /// </summary>
        /// <param propertyName="item"></param>
        protected internal void AddItemLocally(NodeLayout item)
        {
            if (item != null)
            {
                item.IsDefinedGlobally = false;
                this.AddItemSilently(this.Count, item);
            }
        }

        #endregion // AddItemLocally

        #region GetLookupKeys

        /// <summary>
        /// Gets a list of keys that each object in the collection has. 
        /// </summary>
        /// <returns></returns>
        protected virtual Collection<string> GetLookupKeys()
        {
            Collection<string> keys = new Collection<string>();

            foreach (NodeLayout layout in this.Items)
            {
                keys.Add(layout.Key);
            }

            return keys;
        }

        #endregion // GetLookupKeys

        #region CanRehydrate

        /// <summary>
        /// Looks through the keys, and determines that all the keys are in the collection, and that the same about of objects are in the collection.
        /// If this isn't the case, false is returned, and the Control Persistence Framework, will not try to reuse the object that are already in the collection.
        /// </summary>
        /// <param name="lookupKeys"></param>
        /// <returns></returns>
        protected virtual bool CanRehydrate(Collection<string> lookupKeys)
        {
            if (lookupKeys == null || lookupKeys.Count != this.Items.Count)
                return false;

            for (int i = 0; i < lookupKeys.Count; i++)
            {
                if (lookupKeys[i] != this.Items[i].Key)
                    return false;
            }

            return true;
        }
        #endregion // CanRehydrate

        #endregion // Protected

        #region Internal

        internal NodeLayout InternalFromType(Type type)
        {
            foreach (NodeLayout layout in this.Items)
            {
                string name = (layout.TargetTypeName != null) ? layout.TargetTypeName.ToLower(CultureInfo.CurrentCulture) : "";
                if (layout.IsDefinedGlobally && (name == type.Name.ToLower(CultureInfo.CurrentCulture) || name == type.FullName.ToLower(CultureInfo.CurrentCulture)))
                    return layout;
            }
            return null;
        }

        internal NodeLayout InternalFromKey(string key)
        {
            NodeLayout returnLayout = null;
            foreach (NodeLayout layout in this.Items)
            {
                if (layout.IsDefinedGlobally && layout.Key == key)
                {
                    returnLayout = layout;
                    break;
                }
            }
            return returnLayout;
        }

        #endregion // Internal

        #endregion // Methods

        #region OnItemAdded

        /// <summary>
        /// Invoked when a <see cref="NodeLayout"/> is added at the specified index.
        /// </summary>
        /// <param propertyName="index"></param>
        /// <param propertyName="item"></param>
        protected override void OnItemAdded(int index, NodeLayout item)
        {
            if (item != null)
            {
                if (!string.IsNullOrEmpty(item.Key))
                    item.IsDefinedGlobally = true;
                else
                {



                    bool isDesignTime = false;
                    if (item != null)
                    {
                        isDesignTime = DesignerProperties.GetIsInDesignMode(item);
                    }

                    if (!isDesignTime)
                    {
                        throw new Exception(SRDataTree.GetString("DuplicateKeyException"));
                    }
                }
            }
            base.OnItemAdded(index, item);
        }

        #endregion // OnItemAdded

        #region AddItemSilently

        /// <summary>
        /// Adds the item at the specified index, without triggering any events. 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        protected override void AddItemSilently(int index, NodeLayout item)
        {
            // This is mainly for persistence, if we come across a root layout layout
            // Then make sure everything is updated. 
            if (item != null)
            {
                if (item.Key == NodeLayout.ROOT_LAYOUT_KEY)
                {
                    if (this._tree != null)
                    {
                        item.Tree = this._tree;
                        if (this._tree.NodesManager != null)
                        {
                            this._tree.NodesManager.NodeLayout = item;
                        }
                    }

                }
            }

            base.AddItemSilently(index, item);
        }
        #endregion // AddItemSilently

        #region Indexer

        /// <summary>
        /// Looks in the collection for a <see cref="NodeLayout"/> with the specified key. 
        /// If no NodeLayout in the collection has that key, then null is returned. 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public NodeLayout this[string key]
        {
            get
            {
                foreach (NodeLayout layout in this.Items)
                {
                    if (layout.Key == key)
                        return layout;
                }

                return null;
            }
        }

        #endregion // Indexer

        #region IProvidePersistenceLookupKeys Members

        Collection<string> IProvidePersistenceLookupKeys.GetLookupKeys()
        {
            return this.GetLookupKeys();
        }

        bool IProvidePersistenceLookupKeys.CanRehydrate(Collection<string> lookupKeys)
        {
            return this.CanRehydrate(lookupKeys);
        }

        #endregion
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