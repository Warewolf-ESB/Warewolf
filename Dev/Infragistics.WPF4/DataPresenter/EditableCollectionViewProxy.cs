using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using System.Reflection;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Interop;
using System.Windows.Input;
using System.Runtime.InteropServices;
using System.Collections;
using System.Diagnostics;
using System.Windows.Data;

namespace Infragistics.Windows
{

    // JJD 11/17/08 - TFS6743/BR35763 - added
    #region EditableCollectionViewProxy class

    // JJD 11/17/08 - TFS6743/BR35763 - added
    // Since IEditableCollectionView wasn't implemented until v3.5 sp1 of the .Net Framework
    // and we are targeting the 3.0 framework we created a proxy class that calls the
    // interface's methods using reflection
    internal class EditableCollectionViewProxy
    {
        #region Members

        private CollectionView _collectionView;

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

        static EditableCollectionViewProxy()
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

        internal EditableCollectionViewProxy(CollectionView collectionView)
        {
            this._collectionView = collectionView;
        }

        #endregion //Constructors

        #region IEditableCollectionViewType static property

        internal static Type IEditableCollectionViewType { get { return s_IEditableCollectionViewType; } }

        #endregion //IEditableCollectionViewType static property

        #region View property

        internal CollectionView View
        {
            get
            {
                return this._collectionView;
            }
        }

        #endregion //View property

        #region IEditableCollectionView Members

        public object AddNew()
        {
            return s_AddNew.Invoke(this._collectionView, null);
        }

        public bool CanAddNew
        {
            get { return (bool)s_CanAddNew.GetValue(this._collectionView, null); }
        }

        public bool CanCancelEdit
        {
            get { return (bool)s_CanCancelEdit.GetValue(this._collectionView, null); }
        }

        public bool CanRemove
        {
            get { return (bool)s_CanRemove.GetValue(this._collectionView, null); }
        }

        public void CancelEdit()
        {
            s_CancelEdit.Invoke(this._collectionView, null);
        }

        public void CancelNew()
        {
            s_CancelNew.Invoke(this._collectionView, null);
        }

        public void CommitEdit()
        {
            s_CommitEdit.Invoke(this._collectionView, null);
        }

        public void CommitNew()
        {
            s_CommitNew.Invoke(this._collectionView, null);
        }

        public object CurrentAddItem
        {
            get { return s_CurrentAddItem.GetValue(this._collectionView, null); }
        }

        public object CurrentEditItem
        {
            get { return s_CurrentEditItem.GetValue(this._collectionView, null); }
        }

        public void EditItem(object item)
        {
            s_EditItem.Invoke(this._collectionView, new object[] { item });
        }

        public bool IsAddingNew
        {
            get { return (bool)s_IsAddingNew.GetValue(this._collectionView, null); }
        }

        public bool IsEditingItem
        {
            get { return (bool)s_IsEditingItem.GetValue(this._collectionView, null); }
        }

        public NewItemPlaceholderPositionProxy NewItemPlaceholderPosition
        {
            get { return (NewItemPlaceholderPositionProxy)(int)s_NewItemPlaceholderPosition.GetValue(this._collectionView, null); }
            set { s_NewItemPlaceholderPosition.SetValue(this._collectionView, (NewItemPlaceholderPositionProxy)(int)value, null); }
        }

        public void Remove(object item)
        {
            s_Remove.Invoke(this._collectionView, new object[] { item });
        }

        public void RemoveAt(int index)
        {
            s_RemoveAt.Invoke(this._collectionView, new object[] { index });
        }

        #endregion

    }

    #endregion //EditableCollectionViewProxy private class

    #region NewItemPlaceholderPositionProxy enum

    internal enum NewItemPlaceholderPositionProxy
    {
        None,
        AtBeginning,
        AtEnd
    }

    #endregion //NewItemPlaceholderPositionProxy enum

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