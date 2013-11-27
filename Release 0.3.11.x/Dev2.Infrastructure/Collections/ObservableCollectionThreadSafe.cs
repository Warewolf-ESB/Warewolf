using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Threading;

namespace Dev2.Collections
{
    public class ObservableCollectionThreadSafe<T> : ObservableCollection<T>
    {
        readonly Dispatcher _dispatcher;

        public ObservableCollectionThreadSafe()
        {
            // Save dispatcher so that we always fire CollectionChanged on it's thread
            _dispatcher = Dispatcher.CurrentDispatcher;
        }

        public ObservableCollectionThreadSafe(List<T> list)
            : base(list)
        {
            // Save dispatcher so that we always fire CollectionChanged on it's thread
            _dispatcher = Dispatcher.CurrentDispatcher;
        }

        public ObservableCollectionThreadSafe(IEnumerable<T> collection)
            : base(collection)
        {
            // Save dispatcher so that we always fire CollectionChanged on it's thread
            _dispatcher = Dispatcher.CurrentDispatcher;
        }

        #region Overrides of ObservableCollection<T>

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if(!_dispatcher.CheckAccess())
            {
                _dispatcher.BeginInvoke(new Action(() => RaiseCollectionChanged(e)), DispatcherPriority.Normal, new object[] { });
            }
            else
            {
                RaiseCollectionChanged(e);
            }
        }

        #endregion

        internal DispatcherFrame TestDispatcherFrame { get; set; }

        void RaiseCollectionChanged(object param)
        {
            // MUST be called on the dispatcher thread!
            base.OnCollectionChanged((NotifyCollectionChangedEventArgs)param);
            if(TestDispatcherFrame != null)
            {
                TestDispatcherFrame.Continue = false;
            }
        }
    }
}
