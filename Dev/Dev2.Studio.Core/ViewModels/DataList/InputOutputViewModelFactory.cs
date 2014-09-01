using System.Collections.Generic;
using Dev2.Common.Interfaces.Data;
using Dev2.Studio.ViewModels.DataList;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Factory
{
    public static class InputOutputViewModelFactory
    {
        public static InputOutputViewModel CreateInputOutputViewModel(string name, string value, string mapsTo, string defaultValue, bool required, string recordSetName)
        {
            var inputOutputViewModel =
                new InputOutputViewModel(name, value, mapsTo, defaultValue, required, recordSetName, false);

            return inputOutputViewModel;
        }

        public static InputOutputViewModel CreateInputOutputViewModel(string name, string value, string mapsTo, string defaultValue, bool required, string recordSetName, bool emptyToNull)
        {
            var inputOutputViewModel =
                new InputOutputViewModel(name, value, mapsTo, defaultValue, required, recordSetName, emptyToNull);

            return inputOutputViewModel;
        }

        public static IList<IInputOutputViewModel> CreateListToDisplayOutputs(IList<IDev2Definition> outputList)
        {
            IList<IInputOutputViewModel> displayOutputData = new List<IInputOutputViewModel>();
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach(IDev2Definition otp in outputList)
            {
                IInputOutputViewModel inputOutputViewModel = CreateInputOutputViewModel(otp.Name, otp.RawValue, otp.MapsTo, otp.DefaultValue, otp.IsRequired, otp.RecordSetName);
                displayOutputData.Add(inputOutputViewModel);
            }
            return displayOutputData;
        }

        public static IList<IInputOutputViewModel> CreateListToDisplayInputs(IList<IDev2Definition> inputList)
        {
            IList<IInputOutputViewModel> displayInputData = new List<IInputOutputViewModel>();
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach(IDev2Definition itp in inputList)
            {
                IInputOutputViewModel inputOutputViewModel = CreateInputOutputViewModel(itp.Name, itp.RawValue, itp.RawValue, itp.DefaultValue, itp.IsRequired, itp.RecordSetName, itp.EmptyToNull);
                displayInputData.Add(inputOutputViewModel);
            }
            return displayInputData;
        }
    }
}
