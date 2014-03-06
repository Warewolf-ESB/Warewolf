using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Shapes;
using System.Windows.Navigation;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Automation;
using System.Windows.Automation.Provider;
using System.Windows.Threading;
using System.Diagnostics;
//using System.Windows.Events;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Data;
using Infragistics.Windows.Helpers;
using Infragistics.Windows.Tiles;
using Infragistics.Windows.Controls;
using Infragistics.Shared;
using Infragistics.Windows.Internal;

namespace Infragistics.Windows.Tiles
{
    // Used to remove an item from the list
    internal class ItemSourceWrapper
    {
        #region Members

        
        private ItemCollection _itemCollection;
        private IList _list;
        private IBindingList _bindingList;
        private ICollectionView _collectionView;
        private INotifyCollectionChanged _notifyCollectionChanged;

        private static Type s_IEditableCollectionViewType;
        private static MethodInfo s_AddNew;
        private static MethodInfo s_CancelEdit;
        private static MethodInfo s_CancelNew;
        private static MethodInfo s_CommitEdit;
        private static MethodInfo s_CommitNew;
        private static MethodInfo s_EditItem;
        private static MethodInfo s_Remove;
        private static MethodInfo s_RemoveAt;

        private static PropertyInfo s_CanAddNew;
        private static PropertyInfo s_CanCancelEdit;
        private static PropertyInfo s_CanRemove;
        private static PropertyInfo s_CurrentAddItem;
        private static PropertyInfo s_CurrentEditItem;
        private static PropertyInfo s_IsAddingNew;
        private static PropertyInfo s_IsEditingItem;
        private static PropertyInfo s_NewItemPlaceholderPosition;

        #endregion //Members

        #region Constructors

        static ItemSourceWrapper()
        {
            s_IEditableCollectionViewType = Type.GetType("System.ComponentModel.IEditableCollectionView, " + typeof(ICollectionView).Assembly.FullName);

            if (s_IEditableCollectionViewType != null)
            {
                #region Cache all methods

                MethodInfo[] minfos = s_IEditableCollectionViewType.GetMethods(BindingFlags.Public | BindingFlags.Instance);

                foreach (MethodInfo mi in minfos)
                {
                    switch (mi.Name)
                    {
                        case "AddNew":
                            s_AddNew = mi;
                            break;
                        case "CancelEdit":
                            s_CancelEdit = mi;
                            break;
                        case "CancelNew":
                            s_CancelNew = mi;
                            break;
                        case "CommitNew":
                            s_CommitNew = mi;
                            break;
                        case "CommitEdit":
                            s_CommitEdit = mi;
                            break;
                        case "EditItem":
                            s_EditItem = mi;
                            break;
                        case "Remove":
                            s_Remove = mi;
                            break;
                        case "RemoveAt":
                            s_RemoveAt = mi;
                            break;
                        default:
                            break;
                    }
                }

                #endregion //Cache all methods

                #region Cache all properties

                PropertyInfo[] pinfos = s_IEditableCollectionViewType.GetProperties();

                foreach (PropertyInfo pi in pinfos)
                {
                    switch (pi.Name)
                    {
                        case "CanAddNew":
                            s_CanAddNew = pi;
                            break;
                        case "CanCancelEdit":
                            s_CanCancelEdit = pi;
                            break;
                        case "CanRemove":
                            s_CanRemove = pi;
                            break;
                        case "CurrentAddItem":
                            s_CurrentAddItem = pi;
                            break;
                        case "CurrentEditItem":
                            s_CurrentEditItem = pi;
                            break;
                        case "IsAddingNew":
                            s_IsAddingNew = pi;
                            break;
                        case "IsEditingItem":
                            s_IsEditingItem = pi;
                            break;
                        case "NewItemPlaceholderPosition":
                            s_NewItemPlaceholderPosition = pi;
                            break;
                        default:
                            Debug.Fail("Unknown property on IEditableCollectionView: " + pi.Name);
                            break;
                    }
                }

                #endregion //Cache all Properties
            }
        }

        internal ItemSourceWrapper(IEnumerable source)
        {
            ItemCollection itemsColl = source as ItemCollection;

            
            // If we aren't bound use the ItemCollection directly since
            // its ICollectionView always returns false for CanRemove
            // which is not true in the unbound scenario
            if (itemsColl != null)
            {
                IEnumerable sourceColl = itemsColl.SourceCollection;

                if (sourceColl == null || sourceColl == itemsColl)
                {
                    this._itemCollection = itemsColl;
                }
            }

            this._list                      = source as IList;
            this._notifyCollectionChanged   = source as INotifyCollectionChanged;
            this._bindingList               = source as IBindingList;
            this._collectionView            = source as ICollectionView;

            if ( this._collectionView != null && 
                !IEditableCollectionViewType.IsAssignableFrom(source.GetType()))
                this._collectionView = null;
        }

        #endregion //Constructors

        #region IEditableCollectionViewType static property

        private static Type IEditableCollectionViewType { get { return s_IEditableCollectionViewType; } }

        #endregion //IEditableCollectionViewType static property

        #region Properties

            #region CanRemove

        public bool CanRemove
        {
            get
            {
                
                // If the _itemcollection is unbound then return true
                if (this._itemCollection != null)
                    return true;
            
                if (this._collectionView != null)
                    return (bool)s_CanRemove.GetValue(this._collectionView, null);

                if (this._bindingList != null)
                    return this._bindingList.SupportsChangeNotification && this._bindingList.AllowRemove;

                if (this._notifyCollectionChanged != null && this._list != null)
                    return this._list.IsReadOnly == false && this._list.IsFixedSize == false;

                return false;
            }
        }

            #endregion //CanRemove

        #endregion //Properties

        #region Methods

            #region Remove

        public void Remove(object item)
        {
            
            // Use the itemcollection if available
            if (this._itemCollection != null)
                this._itemCollection.Remove(item);
            else
            if (this._collectionView != null)
                s_Remove.Invoke(this._collectionView, new object[] { item });
            else
            {
                if (this._list != null)
                    this._list.Remove(item);
            }
        }

            #endregion //Remove	
    
            #region RemoveAt

        public void RemoveAt(int index)
        {
            
            // Use the itemcollection if available
            if (this._itemCollection != null)
                this._itemCollection.RemoveAt(index);
            else
            if (this._collectionView != null)
                s_RemoveAt.Invoke(this._collectionView, new object[] { index });
            else
            {
                if (this._list != null)
                    this._list.RemoveAt(index);
            }
        }

            #endregion //RemoveAt	
    
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