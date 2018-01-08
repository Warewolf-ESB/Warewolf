/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
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

namespace Dev2.Data
{
    public class OptomizedObservableCollection<T> : ObservableCollection<T>
    {
        bool SuppressOnCollectionChanged;

        public OptomizedObservableCollection()
        {
            SuppressOnCollectionChanged = false;
        }
        public void AddRange(IList<T> items)
        {
            if(null == items)
            {
                throw new ArgumentNullException("items");
            }


            if(items.Count > 0)
            {
                try
                {
                    SuppressOnCollectionChanged = true;
                    foreach(var item in items)
                    {
                        Add(item);
                    }

                }
                finally
                {
                    SuppressOnCollectionChanged = false;
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, this));
                }
            }
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if(!SuppressOnCollectionChanged)
            {
                base.OnCollectionChanged(e);
            }
        }
    }
}
