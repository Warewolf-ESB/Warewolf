
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
using System.Linq;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core
{

    /// <summary>
    ///  Abstraction of the ObservableCollection object, which does not handle large Add operations
    ///  due to the NotifyCollectionChange event that fires for every object added to it's collection
    ///  this class just suprresses the onCollectionChangedEvent to only fire after a list of objects 
    ///  has been addded to the collection
    /// </summary>
    /// <typeparam name="T">Any object really, if you want to create an observable collection of it</typeparam>
    public class OptomizedObservableCollection<T> : ObservableCollection<T>
    {

        public bool SuppressOnCollectionChanged;

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

        public void RemoveRange(IEnumerable<T> itemsToRemove)
        {
            if(null == itemsToRemove)
            {
                // ReSharper disable once NotResolvedInText
                throw new ArgumentNullException("items");
            }

            IEnumerable<T> toRemove = itemsToRemove as IList<T> ?? itemsToRemove.ToList();
            if(toRemove.Any())
            {
                try
                {
                    SuppressOnCollectionChanged = true;
                    List<T> listOfT = toRemove.ToList();

                    foreach(var item in listOfT)
                    {
                        Remove(item);
                    }

                }
                finally
                {
                    SuppressOnCollectionChanged = false;
                    //OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, this));
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
