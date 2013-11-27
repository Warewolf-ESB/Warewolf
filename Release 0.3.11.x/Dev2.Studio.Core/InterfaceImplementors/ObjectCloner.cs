using System;
using System.Collections.ObjectModel;

namespace Dev2.Studio.Core {
    
    /// <summary>
    /// Very old piece of work by me ;)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObjectCloner<T> : IObjectCloner<T> {

        public ObservableCollection<T> CloneObservableCollection(ObservableCollection<T> src) {

            ObservableCollection<T> result = new ObservableCollection<T>();

            foreach(T elm in src){
                ICloneable tmp = (ICloneable)elm;
                result.Add((T)tmp.Clone());
            }

            return result;
        }
    }
}
