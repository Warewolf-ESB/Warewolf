using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Dev2.Data.Interfaces;

namespace Dev2.Studio.Core.Interfaces
{
    public interface IDataMappingViewModel
    {
        IWebActivity Activity { get; set; }
        bool IsInitialLoad { get; set; }
        ObservableCollection<IInputOutputViewModel> Outputs { get; }
        ObservableCollection<IInputOutputViewModel> Inputs { get; }
        string XmlOutput { get; set; }
        ICommand UndoCommand { get; }
        ICommand RedoCommand { get; }
        void CreateXmlOutput(IList<IInputOutputViewModel> outputData, IList<IInputOutputViewModel> inputData);
        void CopyFrom(IDataMappingViewModel copyObj);
        void InputLostFocusTextBox(string text);
        void OutputLostFocusTextBox(string text);
        void InputTextBoxGotFocus(IInputOutputViewModel Selected);
        void OutputTextBoxGotFocus(IInputOutputViewModel Selected);
        string GetInputString(IList<IInputOutputViewModel> inputData);
        string GetOutputString(IList<IInputOutputViewModel> outputData);
    }
}
