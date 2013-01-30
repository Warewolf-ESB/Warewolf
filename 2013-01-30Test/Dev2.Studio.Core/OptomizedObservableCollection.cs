using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Dev2.Studio.Core {

    /// <summary>
    ///  Abstraction of the ObservableCollection object, which does not handle large Add operations
    ///  due to the NotifyCollectionChange event that fires for every object added to it's collection
    ///  this class just suprresses the onCollectionChangedEvent to only fire after a list of objects 
    ///  has been addded to the collection
    /// </summary>
    /// <typeparam name="T">Any object really, if you want to create an observable collection of it</typeparam>
    public class OptomizedObservableCollection<T> : ObservableCollection<T> {

        public bool suppressOnCollectionChanged;

        public OptomizedObservableCollection() 
            : base() {
                this.suppressOnCollectionChanged = false;
        }
        public void AddRange(IList<T> items) {
            if (null == items) {
                throw new ArgumentNullException("items");
            }
            

            if (items.Count > 0) {
                try {
                    suppressOnCollectionChanged = true;
                    foreach (var item in items) {
                        Add(item);
                    }

                }
                finally {
                    suppressOnCollectionChanged = false;
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, this));
                }
            }
        }

        public void RemoveRange(IEnumerable<T> itemsToRemove) {
            if (null == itemsToRemove) {
                throw new ArgumentNullException("items");
            }

            if (itemsToRemove.Any()) {
                try {
                    suppressOnCollectionChanged = true;
                    List<T> listOfT = itemsToRemove.ToList();
                    
                    foreach (var item in listOfT) {
                        Remove(item);
                    }

                }
                finally {
                    suppressOnCollectionChanged = false;
                    //OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, this));
                }
            }
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e) {
            if (!suppressOnCollectionChanged) {
                base.OnCollectionChanged(e);
            }
        }

        protected override void OnPropertyChanged(System.ComponentModel.PropertyChangedEventArgs e) {
            base.OnPropertyChanged(e);
        }
    }
}
