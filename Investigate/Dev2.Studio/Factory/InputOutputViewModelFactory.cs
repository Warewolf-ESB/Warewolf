using Dev2.Studio.ViewModels.DataList;

namespace Dev2.Studio.Factory {
   public static class InputOutputViewModelFactory {
       public static InputOutputViewModel CreateInputOutputViewModel(string name, string value,string mapsTo,string defaultValue,bool required,string recordSetName) {
           var inputOutputViewModel = 
               new InputOutputViewModel(name,value,mapsTo,defaultValue,required,recordSetName, false);

           return inputOutputViewModel;
       }

       public static InputOutputViewModel CreateInputOutputViewModel(string name, string value, string mapsTo, string defaultValue, bool required, string recordSetName, bool emptyToNull)
       {
           var inputOutputViewModel = 
               new InputOutputViewModel(name, value, mapsTo, defaultValue, required, recordSetName, emptyToNull);

           return inputOutputViewModel;
       }
   }
}
