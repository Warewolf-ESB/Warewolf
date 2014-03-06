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
using System.Collections.Specialized;

namespace Infragistics.Windows
{

    // JJD 11/17/08 - TFS6743/BR35763 - added
    #region CollectionViewWrapper class

    // JJD 11/17/08 - TFS6743/BR35763 - added
    // Wraps a CollectionView and implements IList
    internal class CollectionViewWrapper : IList, INotifyCollectionChanged
    {
        #region Members

        private CollectionView _collectionView;
        private EditableCollectionViewProxy _proxy;
        private NewItemPlaceholderPositionProxy _lastNewItemPlaceholderPosition;

        #endregion //Members

        #region Constructors

        internal CollectionViewWrapper(CollectionView collectionView, EditableCollectionViewProxy proxy)
        {
            this._collectionView = collectionView;
            this._proxy = proxy;

            // JJD 12/04/08 - TFS6743/BR35763
            // Initialize the _lastNewItemPlaceholderPosition state
            if (this._proxy != null)
                this._lastNewItemPlaceholderPosition = this._proxy.NewItemPlaceholderPosition;
            else
                this._lastNewItemPlaceholderPosition = NewItemPlaceholderPositionProxy.None;

            ((INotifyCollectionChanged)(this._collectionView)).CollectionChanged += new NotifyCollectionChangedEventHandler(OnCollectionChanged);
        }

        #endregion //Constructors

        #region Properties

            #region CurrentPosition

        internal int CurrentPosition
        {
            get
            {
                int position = this._collectionView.CurrentPosition;

                if (position >= 0)
                    position = this.GetIndex(position);

                return position;
            }
        }

            #endregion //CurrentPosition	
    
            #region EditableCollectionViewProxy property

        internal EditableCollectionViewProxy EditableCollectionViewProxy
        {
            get
            {
                return this._proxy;
            }
        }

            #endregion //EditableCollectionViewProxy property

            #region View property

        internal CollectionView View
        {
            get
            {
                return this._collectionView;
            }
        }

            #endregion //View property

        #endregion //Properties	
    
        #region Methods

            #region GetAdjustedProxyIndex

        private int GetAdjustedProxyIndex(int index)
        {
            if (this._proxy == null ||
                 this._proxy.NewItemPlaceholderPosition != NewItemPlaceholderPositionProxy.AtBeginning)
                return index;

            return index + 1;
        }

            #endregion //GetAdjustedProxyIndex	
    
            #region GetIndex

        private int GetIndex(int proxyIndex)
        {
            if (proxyIndex < 1 ||
                 this._proxy == null ||
                 this._proxy.NewItemPlaceholderPosition != NewItemPlaceholderPositionProxy.AtBeginning)
                return proxyIndex;

            return proxyIndex - 1;
        }

            #endregion //GetIndex	
  
            #region MoveCurrentToPosition

        internal bool MoveCurrentToPosition(int position)
        {
            return this._collectionView.MoveCurrentToPosition(position < 0 ? position : this.GetAdjustedProxyIndex(position));
        }

            #endregion //MoveCurrentToPosition	
    
            #region OnCollectionChanged

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            NotifyCollectionChangedEventArgs newArgs = null;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Reset:
                    newArgs = e;
                    break;
                case NotifyCollectionChangedAction.Add:
                    {
                        if (this._proxy != null)
                        {
                            NewItemPlaceholderPositionProxy newPosition = this._proxy.NewItemPlaceholderPosition;

                            // If the placeholder position has changed from 'None" then eat the remove notification 
                            // if it relates to the placeholder being added
                            if ( this._lastNewItemPlaceholderPosition != newPosition &&
                                 this._lastNewItemPlaceholderPosition == NewItemPlaceholderPositionProxy.None )
                            {
                                if (newPosition == NewItemPlaceholderPositionProxy.AtBeginning)
                                {
                                    if (e.NewStartingIndex == 0)
                                        break;
                                }
                                else
                                {
                                    if (e.NewStartingIndex == this.Count)
                                        break;
                                }
                            }
                        }
                        newArgs = new NotifyCollectionChangedEventArgs(e.Action, e.NewItems, this.GetIndex(e.NewStartingIndex));
                        break;
                    }
                case NotifyCollectionChangedAction.Remove:
                    {
                        if (this._proxy != null)
                        {
                            NewItemPlaceholderPositionProxy newPosition = this._proxy.NewItemPlaceholderPosition;

                            // If the placeholder position has changed to 'None" then eat the remove notification 
                            // if it relates to the placeholder being removed
                            if ( this._lastNewItemPlaceholderPosition != newPosition &&
                                 newPosition == NewItemPlaceholderPositionProxy.None )
                            {
                                if (this._lastNewItemPlaceholderPosition == NewItemPlaceholderPositionProxy.AtBeginning)
                                {
                                    if (e.OldStartingIndex == 0)
                                        break;
                                }
                                else
                                {
                                    if (e.OldStartingIndex == this.Count)
                                        break;
                                }
                            }
                        }
                        newArgs = new NotifyCollectionChangedEventArgs(e.Action, e.OldItems, this.GetIndex(e.OldStartingIndex));
                        break;
                    }
                case NotifyCollectionChangedAction.Move:
                    {
                        int count = this.Count;

                        // if the move is an add for placeholder then eat the notification
                        if (e.NewStartingIndex >= count ||
                             e.OldStartingIndex >= count)
                        {
                            if (this._proxy != null)
                            {
                                // If the placeholder position has changed eat the move notification 
                                // since we are logically hoding the placeholders from the listener
                                if (this._lastNewItemPlaceholderPosition != this._proxy.NewItemPlaceholderPosition)
                                    break;
                            }
                        }

                        newArgs = new NotifyCollectionChangedEventArgs(e.Action, e.NewItems, this.GetIndex(e.NewStartingIndex), this.GetIndex(e.OldStartingIndex));
                        break;
                    }
                case NotifyCollectionChangedAction.Replace:
                    newArgs = new NotifyCollectionChangedEventArgs(e.Action, e.NewItems, e.OldItems, this.GetIndex(e.NewStartingIndex));
                    break;
            }

            if (newArgs != null && this.CollectionChanged != null)
                this.CollectionChanged(this, newArgs);

            // Keep track of the placeholder position so we can eat the move notification when it changes
            // since we are logically hoding the placeholders from the listener
            if ( this._proxy != null )
                this._lastNewItemPlaceholderPosition = this._proxy.NewItemPlaceholderPosition;
        }

            #endregion //OnCollectionChanged	
    
        #endregion //Methods	
    
        #region IList Members

        public int Add(object value)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(object value)
        {
            return this._collectionView.Contains(value);
         }

        public int IndexOf(object value)
        {
            return this.GetIndex( this._collectionView.IndexOf(value) );
        }

        public void Insert(int index, object value)
        {
            throw new NotSupportedException();
        }

        public bool IsFixedSize
        {
            get 
            {
                if (this._proxy != null)
                    return this._proxy.CanAddNew == false && this._proxy.CanRemove == false;

                return true;
            }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public void Remove(object value)
        {
            if (this._proxy != null)
            {
                this._proxy.Remove(value);
                return;
            }

            throw new NotSupportedException();
        }

        public void RemoveAt(int index)
        {
            if (this._proxy != null)
            {
                this._proxy.RemoveAt(this.GetAdjustedProxyIndex(index));
                return;
            }

            throw new NotSupportedException();
        }

        public object this[int index]
        {
            get
            {
                return this._collectionView.GetItemAt(this.GetAdjustedProxyIndex(index));
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        #endregion

        #region ICollection Members

        public void CopyTo(Array array, int index)
        {
            object [] tempArray = new object[this.Count];

            int i = 0;

            foreach (object item in this)
            {
                tempArray[i] = item;
                i++;
            }

            Array.Copy(tempArray, 0, array, index, tempArray.Length);
        }

        public int Count
        {
            get 
            { 
                int count = this._collectionView.Count;

                // Decrement the count for the add record placeholder 
                if (count > 0 &&
                     this._proxy != null &&
                     this._proxy.NewItemPlaceholderPosition != NewItemPlaceholderPositionProxy.None)
                    count--;

                return count;
            }
        }

        public bool IsSynchronized
        {
            get { return false; }
        }

        public object SyncRoot
        {
            get { return this; }
        }

        #endregion

        #region IEnumerable Members

        public IEnumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        #endregion

        #region Enumerator private class

        private class Enumerator : IEnumerator
        {
            CollectionViewWrapper _collectionViewWrapper;
            private int _currentPosition;
            private object _currentItem;

            static object UnsetObjectMarker = new object();

            internal Enumerator(CollectionViewWrapper collectionViewWrapper)
            {
                this._collectionViewWrapper = collectionViewWrapper;
                this._currentPosition = -1;
                this._currentItem = UnsetObjectMarker;
            }

            public void Dispose()
            {
                this.Reset();
            }

            #region IEnumerator Members

            public bool MoveNext()
            {
                int count = this._collectionViewWrapper.Count;

                if (this._currentPosition < count - 1)
                {
                    this._currentPosition++;
                    this._currentItem = this._collectionViewWrapper[this._currentPosition];
                    return true;
                }

                this._currentPosition = count;
                this._currentItem = UnsetObjectMarker;
                return false;
            }

            public void Reset()
            {
                this._currentPosition = -1;
                this._currentItem = UnsetObjectMarker;
            }

            object IEnumerator.Current
            {
                get
                {
                    if (this._currentItem == UnsetObjectMarker)
                    {
                        throw new InvalidOperationException();
                    }

                    return this._currentItem;
                }
           }

            #endregion
        }
        #endregion //Enumerator

        #region INotifyCollectionChanged Members

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        #endregion
    }

    #endregion //CollectionViewWrapper class

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