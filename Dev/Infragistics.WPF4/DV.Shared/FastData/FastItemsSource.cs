using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Collections.Generic;
using System.Windows.Threading;

namespace Infragistics
{
    /// <summary>
    /// Represents a view of an enumerable items source
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class FastItemsSource : IEnumerable
    {

        ItemSourceEventProxy ItemsSourceEventProxy;

        /// <summary>
        /// FastItemsSource constructor.
        /// </summary>
        public FastItemsSource()
        {
        }
        /// <summary>
        /// The FastItemsSourceEvent, which is raised when the collection is updated.
        /// </summary>
        public event EventHandler<FastItemsSourceEventArgs> Event;
        private void RaiseDataSourceEvent(FastItemsSourceEventAction action, int position, int count)
        {
            if (Event != null)
            {
                Event(this, new FastItemsSourceEventArgs(action, position, count));
            }
        }

        private void RaiseDataSourceEvent(int position, string propertyName)
        {
            if (Event != null)
            {
                Event(this, new FastItemsSourceEventArgs(position, propertyName));
            }
        }


        /// <summary>
        /// The Dispatcher to defer refresh operations through, in order to avoid updates which are more frequent than necessary.
        /// </summary>
        /// <remarks>Deferring updates through the dispatcher reduces the CPU burden of FastItemsSource updates when the FastItemsSource is being used as a DataSource for a UI control.</remarks>
        public Dispatcher Dispatcher { get; set; }

        /// <summary>
        /// The enumerable list of objects to get data from.
        /// </summary>
        public 
            


            IEnumerable 

            ItemsSource
        {
            get { return itemsSource; }
            set
            {
                if (itemsSource == value)
                {
                    return;
                }

                Detach();

                itemsSource = value;
                contents.Clear();   // trash the old contents
                index = null;         // trash the old index (if there is one)

                Attach();

                foreach (ColumnReference referencedColumn in columns.Values)
                {
                    referencedColumn.FastItemColumn.Reset();
                }

                RaiseDataSourceEvent(FastItemsSourceEventAction.Insert, 0, contents.Count);
            }
        }

        /// <summary>
        /// Detaches this instance by removing old listeners.
        /// </summary>
        public void Detach()
        {

            if (itemsSource != null)
            {
                if (itemsSource is INotifyCollectionChanged)
                {
                    (itemsSource as INotifyCollectionChanged).CollectionChanged -= ItemsSourceEventProxy.CollectionChanged;
                }

                foreach (object item in contents)
                {
                    if (item is INotifyPropertyChanged)
                    {
                        (item as INotifyPropertyChanged).PropertyChanged -= ItemsSourceEventProxy.PropertyChanged;
                    }
                }

                ItemsSourceEventProxy.WeakCollectionChanged = null;
                ItemsSourceEventProxy.WeakPropertyChanged = null;
                ItemsSourceEventProxy = null;
            }

        }


        /// <summary>
        /// Attaches this instance by adding new listeners.
        /// </summary>
        public void Attach()
        {

            if (itemsSource != null)
            {
                ItemsSourceEventProxy = new ItemSourceEventProxy(this);
                ItemsSourceEventProxy.WeakCollectionChanged = (fastItemsSource, sender, e) =>
                {
                    switch (e.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                            fastItemsSource.dataSourceAdd(e.NewStartingIndex, e.NewItems);
                            break;

                        case NotifyCollectionChangedAction.Remove:
                            fastItemsSource.dataSourceRemove(e.OldStartingIndex, e.OldItems);
                            break;

                        case NotifyCollectionChangedAction.Replace:
                            fastItemsSource.dataSourceReplace(e.NewStartingIndex, e.OldItems, e.NewItems);
                            break;

                        case NotifyCollectionChangedAction.Reset:
                            fastItemsSource.dataSourceReset();
                            break;
                    }
                };

                ItemsSourceEventProxy.WeakPropertyChanged = (fastItemsSource, sender, e) =>
                {
                    fastItemsSource.dataSourceChange(sender, e.PropertyName);
                };

                if (itemsSource is INotifyCollectionChanged)
                {
                    (itemsSource as INotifyCollectionChanged).CollectionChanged += ItemsSourceEventProxy.CollectionChanged;
                }

                foreach (object item in itemsSource)
                {
                    contents.Add(item);

                    if (item is INotifyPropertyChanged)
                    {
                        (item as INotifyPropertyChanged).PropertyChanged += ItemsSourceEventProxy.PropertyChanged;
                    }
                }
            }



        }

        private void dataSourceAdd(int position, IList newItems)
        {

            foreach (object item in newItems)
            {
                if (item is INotifyPropertyChanged)
                {
                    (item as INotifyPropertyChanged).PropertyChanged += ItemsSourceEventProxy.PropertyChanged;
                }
            }


            if (index != null)
            {
                for (int i = 0; i < newItems.Count; ++i)
                {
                    index.Add(newItems[i], position + i);
                }

                for (int i = position; i < contents.Count; ++i)
                {
                    index[contents[i]] = i + newItems.Count;
                }
            }







            contents.InsertRange(position, newItems);


            foreach (ColumnReference referencedColumn in columns.Values)
            {                
                referencedColumn.FastItemColumn.InsertRange(position, newItems.Count);
            }

            RaiseDataSourceEvent(FastItemsSourceEventAction.Insert, position, newItems.Count);
        }
        private void dataSourceRemove(int position, IList oldItems)
        {

            foreach (object item in oldItems)
            {
                if (item is INotifyPropertyChanged)
                {
                    (item as INotifyPropertyChanged).PropertyChanged -= ItemsSourceEventProxy.PropertyChanged;
                }
            }


            contents.RemoveRange(position, oldItems.Count);

            if (index != null)
            {
                foreach (object item in oldItems)
                {
                    index.Remove(item);
                }

                for (int i = position; i < contents.Count; ++i)
                {
                    index[contents[i]] = i;
                }
            }

            foreach (ColumnReference referencedColumn in columns.Values)
            {
                referencedColumn.FastItemColumn.RemoveRange(position, oldItems.Count);
            }

            RaiseDataSourceEvent(FastItemsSourceEventAction.Remove, position, oldItems.Count);
        }
        private void dataSourceReplace(int position, IList oldItems, IList newItems)
        {

            foreach (object item in oldItems)
            {
                if (item is INotifyPropertyChanged)
                {
                    (item as INotifyPropertyChanged).PropertyChanged -= ItemsSourceEventProxy.PropertyChanged;
                }
            }

            foreach (object item in newItems)
            {
                if (item is INotifyPropertyChanged)
                {
                    (item as INotifyPropertyChanged).PropertyChanged += ItemsSourceEventProxy.PropertyChanged;
                }
            }


            for (int i = 0; i < newItems.Count; ++i)
            {
                contents[position + i] = newItems[i];
            }

            if (index != null)
            {
                foreach (object item in oldItems)
                {
                    index.Remove(item);
                }

                for (int i = 0; i < newItems.Count; ++i)
                {
                    index.Add(newItems[i], position + i);
                }
            }

            foreach (ColumnReference referencedColumn in columns.Values)
            {
                referencedColumn.FastItemColumn.ReplaceRange(position, newItems.Count);
            }

            RaiseDataSourceEvent(FastItemsSourceEventAction.Replace, position, oldItems.Count);
        }
        private void dataSourceReset()
        {
            // behave like the entire items source was removed

            foreach (object item in contents)
            {
                if (item is INotifyPropertyChanged)
                {
                    (item as INotifyPropertyChanged).PropertyChanged -= ItemsSourceEventProxy.PropertyChanged;
                }
            }

            contents.Clear();
            index = null;

            // behave like the entire items source was added



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

            foreach (object item in itemsSource)
            {
                contents.Add(item);

                if (item is INotifyPropertyChanged)
                {
                    (item as INotifyPropertyChanged).PropertyChanged += ItemsSourceEventProxy.PropertyChanged;
                }                
            }


            foreach (ColumnReference referencedColumn in columns.Values)
            {
                referencedColumn.FastItemColumn.Reset();
            }

            RaiseDataSourceEvent(FastItemsSourceEventAction.Reset, 0, contents.Count);
        }
        private void dataSourceChange(object item, string propertyName)
        {
            ColumnReference columnReference = null;
            int position = this.IndexOf(item);
            if (position == -1)
            {
                throw new ArgumentException("item");
            }

            if (columns.TryGetValue(propertyName, out columnReference))
            {
                columnReference.FastItemColumn.ReplaceRange(position, 1);
            }
            if (columns.TryGetValue(propertyName + "_object", out columnReference))
            {
                columnReference.FastItemColumn.ReplaceRange(position, 1);
            }

            RaiseDataSourceEvent(position, propertyName);
        }

        #region Item Access By Index
        /// <summary>
        /// Gets the number of items in the current FastItemsSource object.
        /// </summary>
        public int Count { get { return contents.Count; } }

        /// <summary>
        /// Gets the items at the specified position.
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public object this[int n] { get { return contents[n]; } }
        #endregion

        #region IEnumerable Members (Item access by enumerator)
        /// <summary>
        /// Gets the IEnumerator used for looping through the FastItemsSource.
        /// </summary>
        /// <returns>The IEnumerator used for looping through the FastItemsSource.</returns>
        public IEnumerator GetEnumerator()
        {
            return contents.GetEnumerator();
        }
        #endregion

        #region Index Access By Item
        /// <summary>
        /// Gets the index of the specified item.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public int this[object item]
        {
            get
            {
                return IndexOf(item);
            }
        }

        /// <summary>
        /// Returns the index of the specified item.
        /// </summary>
        /// <param name="item">The item to get the index for.</param>
        /// <returns>The index of the specified item.</returns>
        public int IndexOf(object item)
        {
            int ret;

            if (index == null && contents.Count > 0)
            {
                index = new Dictionary<object, int>();
                int i = 0;

                foreach (object o in contents)
                {
                    if (!index.ContainsKey(o))
                    {
                        index.Add(o, i++);
                    }
                }
            }







            if (index.TryGetValue(item, out ret))
            {
                return ret;
            }
            else
            {
                return -1;
            }

        }
        #endregion
        /// <summary>
        /// Returns a DateTime FastItemColumn for the given property name.
        /// </summary>
        /// <param name="propertyName">The name of the property on the data items which is used to populate the FastItemColumn.</param>
        /// <returns>A DateTime FastItemColumn for the given property name.</returns>
        public IFastItemColumn<DateTime> RegisterColumnDateTime(string propertyName



            )
        {
            IFastItemColumn<DateTime> fastItemColumn = null;

            if (propertyName != null)
            {
                ColumnReference columnReference = null;

                if (!columns.TryGetValue(propertyName, out columnReference))
                {



                    columnReference = new ColumnReference(new FastItemDateTimeColumn(this, propertyName));

                    columns.Add(propertyName, columnReference);
                }

                columnReference.References = columnReference.References + 1;
                fastItemColumn = columnReference.FastItemColumn as IFastItemColumn<DateTime>;
            }

            return fastItemColumn;
        }
        /// <summary>
        /// Returns an object FastItemColumn for the given property name.
        /// </summary>
        /// <param name="propertyName">The name of the property on the data items which is used to populate the FastItemColumn.</param>
        /// <returns>An object FastItemColumn for the given property name.</returns>
        public IFastItemColumn<object> RegisterColumnObject(string propertyName



            )
        {
            IFastItemColumn<object> fastItemColumn = null;
            string key = propertyName + "_object";

            if (propertyName != null)
            {
                ColumnReference columnReference = null;

                if (!columns.TryGetValue(key, out columnReference))
                {



                    columnReference = new ColumnReference(new FastItemObjectColumn(this, propertyName));

                    columns.Add(key, columnReference);
                }

                columnReference.References = columnReference.References + 1;
                fastItemColumn = columnReference.FastItemColumn as IFastItemColumn<object>;
            }

            return fastItemColumn;
        }
        /// <summary>
        /// Returns an integer FastItemColumn for the given property name.
        /// </summary>
        /// <param name="propertyName">The name of the property on the data items which is used to populate the FastItemColumn.</param>
        /// <returns>An integer FastItemColumn for the given property name.</returns>
        public IFastItemColumn<int> RegisterColumnInt(string propertyName



            )
        {
            IFastItemColumn<int> fastItemColumn = null;

            if (propertyName == null)
            {
                propertyName = "";
            }

            ColumnReference columnReference = null;

            if (!columns.TryGetValue(propertyName, out columnReference))
            {



                columnReference = new ColumnReference(new FastItemIntColumn(this, propertyName));

                columns.Add(propertyName, columnReference);
            }

            columnReference.References = columnReference.References + 1;
            fastItemColumn = columnReference.FastItemColumn as IFastItemColumn<int>;
            
            return fastItemColumn;
        }
        /// <summary>
        /// Returns a FastItemColumn for the given property name.
        /// </summary>
        /// <param name="propertyName">The name of the property on the data items which is used to populate the FastItemColumn.</param>
        /// <returns>A FastItemColumn for the given property name.</returns>
        public IFastItemColumn<double> RegisterColumn(string propertyName



            )
        {
            IFastItemColumn<double> fastItemColumn = null;

            if (propertyName == null)
            {
                propertyName = "";
            }
            
            ColumnReference columnReference = null;

            if (!columns.TryGetValue(propertyName, out columnReference))
            {



                columnReference = new ColumnReference(new FastItemColumn(this, propertyName));

                columns.Add(propertyName, columnReference);
            }

            columnReference.References = columnReference.References + 1;
            fastItemColumn = columnReference.FastItemColumn as IFastItemColumn<double>;


            return fastItemColumn;
        }
        /// <summary>
        /// Uninitializes the FastItemColumn with the given property name.
        /// </summary>
        /// <param name="fastItemColumn">The object responsible for providing the property name of the FastItemColumn.</param>
        public void DeregisterColumn(IFastItemColumnPropertyName fastItemColumn)
        {
            string propertyName = fastItemColumn != null ? fastItemColumn.PropertyName : null;
            string key = propertyName;
            if (fastItemColumn is IFastItemColumn<object>)
            {
                key += "_object";
            }

            if (propertyName != null)
            {
                ColumnReference columnReference = null;

                if (columns.TryGetValue(propertyName, out columnReference))
                {
                    columnReference.References = columnReference.References - 1;

                    if (columnReference.References == 0)
                    {
                        columns.Remove(propertyName);
                    }
                }
            }
        }
        
        private Dictionary<string, ColumnReference> columns = new Dictionary<string, ColumnReference>();
        private



            IEnumerable 

            itemsSource;






        private ArrayList contents = new ArrayList();


        private Dictionary<object, int> index = null;



#region Infragistics Source Cleanup (Region)




























#endregion // Infragistics Source Cleanup (Region)

    }

    internal class ColumnReference
    {
        public ColumnReference(IFastItemColumnInternal fastItemColumn)
        {
            FastItemColumn = fastItemColumn;
            References = 0;
        }
        public IFastItemColumnInternal FastItemColumn;
        public int References { get; set; }
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