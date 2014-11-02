/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Threading;

namespace Dev2.Collections
{
    public class ObservableCollectionThreadSafe<T> : ObservableCollection<T>
    {
        private readonly Dispatcher _dispatcher;

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
            if (!_dispatcher.CheckAccess())
            {
                _dispatcher.BeginInvoke(new Action(() => RaiseCollectionChanged(e)), DispatcherPriority.Normal,
                    new object[] {});
            }
            else
            {
                RaiseCollectionChanged(e);
            }
        }

        #endregion

        internal DispatcherFrame TestDispatcherFrame { get; set; }

        private void RaiseCollectionChanged(object param)
        {
            // MUST be called on the dispatcher thread!
            base.OnCollectionChanged((NotifyCollectionChangedEventArgs) param);
            if (TestDispatcherFrame != null)
            {
                TestDispatcherFrame.Continue = false;
            }
        }
    }
}