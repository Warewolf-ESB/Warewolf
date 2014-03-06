using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Infragistics.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Infragistics.Controls.Grids
{
    /// <summary>
    /// A collection of chilren <see cref="Column"/> objects that are displayed under a <see cref="GroupColumn"/>
    /// </summary>
    public class GroupColumnsCollection : CollectionBase<Column>, IProvidePersistenceLookupKeys, IProvidePropertyPersistenceSettings
    {
        #region Members

        ColumnLayout _colLayout;
        List<string> _propertiesThatShouldntBePersisted;

        #endregion // Members

        #region Properties

        #region Public
        
        #region ColumnLayout

        /// <summary>
        /// The <see cref="ColumnLayout"/> that the <see cref="GroupColumn"/> that owns this collection belongs to.
        /// </summary>
        public virtual ColumnLayout ColumnLayout
        {
            get
            {
                return this._colLayout;
            }
            protected internal set
            {
                if (this._colLayout != value)
                {
                    if (this._colLayout != null)
                    {

                    }

                    this._colLayout = value;

                    if (this._colLayout != null)
                    {
                        foreach (Column col in this.Items)
                            this._colLayout.OnColumnAdded(col);
                    }
                }
            }
        }

        #endregion // ColumnLayout

        #region PropertiesToIgnore

        /// <summary>
        /// Gets a List of properties that shouldn't be saved when the PersistenceManager goes to save them.
        /// </summary>
        protected virtual List<string> PropertiesToIgnore
        {
            get
            {
                if (this._propertiesThatShouldntBePersisted == null)
                {
                    this._propertiesThatShouldntBePersisted = new List<string>()
					{
                        "AllColumns",
                        "AllVisibleColumns"
					};
                }

                return this._propertiesThatShouldntBePersisted;
            }
        }
        #endregion // PropertiesToIgnore

        #region PriorityProperties

        /// <summary>
        /// Gets a List of properties that should be applied, before even trying to look at any other property on the object.
        /// </summary>
        protected virtual List<string> PriorityProperties
        {
            get { return null; }
        }

        #endregion // PriorityProperties

        #endregion // Public

        #endregion // Properties

        #region Overrides

        #region InsertItem
        /// <summary>
        /// Inserts the <see cref="Column"/> at the specified index.
        /// </summary>
        /// <param propertyName="index">The index that the Column should be inserted.</param>
        /// <param propertyName="item">The <see cref="Column"/> that should be inserted.</param>
        protected override void AddItem(int index, Column item)
        {
            if (!item.IsMoving)
            {
                if (this.ColumnLayout != null)
                    this.ColumnLayout.OnColumnAdded(item);                
            }

            base.AddItem(index, item);
        }

        /// <summary>
        /// Inserts the specified <see cref="Column"/> as the specified index. 
        /// </summary>
        /// <param propertyName="index"></param>
        /// <param propertyName="item"></param>
        public override void Insert(int index, Column item)
        {
            this.AddItem(index, item);
        }

        #endregion // InsertItem

        #region RemoveItem

        /// <summary>
        /// Removes the <see cref="Column"/> a the specified index. 
        /// </summary>
        /// <param propertyName="index">The index of the Column that should be removed.</param>
        protected override bool RemoveItem(int index)
        {
            Column col = this.Items[index];

            if (!col.IsMoving)
            {
                this.ColumnLayout.OnColumnRemoved(col);
            }

            if (this.ColumnLayout.Grid != null)
                this.ColumnLayout.Grid.InvalidateScrollPanel(true);

            return base.RemoveItem(index);
        }
        #endregion // RemoveItem

        #region ResetItems
        /// <summary>
        /// Removes all <see cref="ColumnBase"/> objects from the <see cref="ColumnBaseCollection"/>.
        /// </summary>
        protected override void ResetItems()
        {
            base.ResetItems();

            ReadOnlyKeyedColumnBaseCollection<Column> allColumns = this.AllColumns;
            foreach (ColumnBase col in allColumns)
            {
                this.ColumnLayout.OnColumnRemoved(col);
            }

            if (this.ColumnLayout != null && this.ColumnLayout.Grid != null)
                this.ColumnLayout.Grid.InvalidateScrollPanel(true);

        }
        #endregion // ResetItems

        #endregion // Overrides

        #region Properties

        #region Public

        #region AllColumns

        /// <summary>
        /// Gets a recursive collection of all <see cref="Column"/> objects in particular <see cref="GroupColumn"/>
        /// </summary>
        public ReadOnlyKeyedColumnBaseCollection<Column> AllColumns
        {
            get
            {
                List<Column> allColumns = new List<Column>();
                foreach (Column col in this.Items)
                {
                    allColumns.Add(col);
                    ReadOnlyKeyedColumnBaseCollection<Column> cols = col.AllColumns;
                    foreach (Column c in cols)
                        allColumns.Add(c);
                }

                return new ReadOnlyKeyedColumnBaseCollection<Column>(allColumns);
            }

        }

        #endregion // AllColumns

        #region Indexer[string]

        /// <summary>
        /// Gets the <see cref="Column"/> that has the specified key. 
        /// </summary>
        /// <param propertyName="key"></param>
        /// <returns>
        /// The column with the specified Key. 
        /// If more than one <see cref="Column"/> has the same key, the first Column is returned.
        /// </returns>
        public Column this[string key]
        {
            get
            {
                foreach (Column column in this.Items)
                {
                    if (column.Key == key)
                        return column;
                }
                return null;
            }
        }

        #endregion // Indexer[string]

        #endregion // Public

        #endregion // Properties

        #region Methods
        #region GetLookupKeys

        /// <summary>
        /// Gets a list of keys that each object in the collection has. 
        /// </summary>
        /// <returns></returns>
        protected virtual Collection<string> GetLookupKeys()
        {
            Collection<string> keys = new Collection<string>();

            foreach (ColumnBase col in this.Items)
            {
                keys.Add(col.Key);
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

            bool reorderNeeded = false;

            Dictionary<string, Column> cols = new Dictionary<string, Column>();

            for (int i = 0; i < lookupKeys.Count; i++)
            {
                string key = lookupKeys[i];

                Column col = this[key];
                cols.Add(key, col);

                if (this[key] == null)
                    return false;
                else
                {
                    if (this.IndexOf(col) != i)
                    {
                        reorderNeeded = true;
                    }
                }
            }

            if (reorderNeeded)
            {
                foreach (string key in lookupKeys)
                {
                    ColumnBase col = this[0];

                    Column column = col as Column;
                    if (column != null)
                    {
                        column.IsMoving = true;
                    }

                    this.RemoveItemSilently(0);

                    if (column != null)
                    {
                        column.IsMoving = false;
                    }
                }

                for (int i = 0; i < lookupKeys.Count; i++)
                {
                    Column col = cols[lookupKeys[i]];
                    Column column = col as Column;

                    this.AddItemSilently(i, col);
                }

            }

            return true;
        }

        #endregion // CanRehydrate

        #region FinishedLoadingPersistence

        /// <summary>
        /// Allows an object to perform an operation, after it's been loaded.
        /// </summary>
        protected virtual void FinishedLoadingPersistence()
        {

        }

        #endregion // FinishedLoadingPersistence

        #endregion // Methods

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

        #region IProvidePropertyPersistenceSettings Members

        #region PropertiesToIgnore

        List<string> IProvidePropertyPersistenceSettings.PropertiesToIgnore
        {
            get
            {
                return this.PropertiesToIgnore;
            }
        }

        #endregion // PropertiesToIgnore

        #region PriorityProperties

        List<string> IProvidePropertyPersistenceSettings.PriorityProperties
        {
            get { return this.PriorityProperties; }
        }

        void IProvidePropertyPersistenceSettings.FinishedLoadingPersistence()
        {
            this.FinishedLoadingPersistence();
        }
        #endregion // PriorityProperties

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