using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace Dev2.Studio.Core {
    public interface IObjectCloner<T> {

        ObservableCollection<T> CloneObservableCollection(ObservableCollection<T> src);
    }
}
