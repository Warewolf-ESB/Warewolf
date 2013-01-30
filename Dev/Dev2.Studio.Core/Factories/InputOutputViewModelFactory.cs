using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.ViewModels;
using Dev2.Composition;

namespace Dev2.Studio.Core.Factories {
   public static class InputOutputViewModelFactory {
       public static InputOutputViewModel CreateInputOutputViewModel(string name, string value,string mapsTo,string defaultValue,bool required,string recordSetName) {
           InputOutputViewModel inputOutputViewModel = new InputOutputViewModel(name,value,mapsTo,defaultValue,required,recordSetName, false);

           return inputOutputViewModel;
       }

       public static InputOutputViewModel CreateInputOutputViewModel(string name, string value, string mapsTo, string defaultValue, bool required, string recordSetName, bool emptyToNull)
       {
           InputOutputViewModel inputOutputViewModel = new InputOutputViewModel(name, value, mapsTo, defaultValue, required, recordSetName, emptyToNull);

           return inputOutputViewModel;
       }
   }
}
