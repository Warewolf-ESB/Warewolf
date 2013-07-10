using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Studio.Core.Interfaces;

namespace Dev2.Studio.Core.Models {
    public class DataMappingDefinition : IDataMappingOutputs {
        private string _name;
        private string _mapsTo;

        public DataMappingDefinition(string name,string mapsTo) {
            Name = name;
            MapsTo = mapsTo;
        }

        public string Name {
            get {
                return _name;
            }
            set {
                _name = value;
            }
        }

        public string MapsTo {
            get {
                return _mapsTo;
            }
            set {
                _mapsTo = value;
            }
        }
    }
}
