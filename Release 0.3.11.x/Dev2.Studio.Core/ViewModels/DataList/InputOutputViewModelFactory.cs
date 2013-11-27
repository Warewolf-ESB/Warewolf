using System.Collections.Generic;
using Dev2.Data.Interfaces;
using Dev2.DataList.Contract;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.ViewModels.DataList;

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
            IList<IInputOutputViewModel> _displayOutputData = new List<IInputOutputViewModel>();
            foreach(IDev2Definition otp in outputList)
            {
                IInputOutputViewModel inputOutputViewModel = CreateInputOutputViewModel(otp.Name, otp.RawValue, otp.MapsTo, otp.DefaultValue, otp.IsRequired, otp.RecordSetName);
                _displayOutputData.Add(inputOutputViewModel);
            }
            return _displayOutputData;
        }

        public static IList<IInputOutputViewModel> CreateListToDisplayInputs(IList<IDev2Definition> inputList)
        {
            IList<IInputOutputViewModel> _displayInputData = new List<IInputOutputViewModel>();
            foreach(IDev2Definition itp in inputList)
            {
                IInputOutputViewModel inputOutputViewModel = CreateInputOutputViewModel(itp.Name, itp.RawValue, itp.RawValue, itp.DefaultValue, itp.IsRequired, itp.RecordSetName, itp.EmptyToNull);
                _displayInputData.Add(inputOutputViewModel);
            }
            return _displayInputData;
        }
    }
}
