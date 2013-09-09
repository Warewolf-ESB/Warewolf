using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace Dev2.Studio.Core {
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
