
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
using System.Collections.ObjectModel;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core
{

    /// <summary>
    /// Very old piece of work by me ;)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObjectCloner<T> : IObjectCloner<T>
    {

        public ObservableCollection<T> CloneObservableCollection(ObservableCollection<T> src)
        {

            ObservableCollection<T> result = new ObservableCollection<T>();

            foreach(T elm in src)
            {
                ICloneable tmp = (ICloneable)elm;
                result.Add((T)tmp.Clone());
            }

            return result;
        }
    }
}
