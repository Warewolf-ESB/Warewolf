using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;


namespace Dev2.Studio.Core.Interfaces
{
    public interface IDataMappingViewModel
    {
        IWebActivity Activity { get; set; }
        bool IsInitialLoad { get; set; }
        ObservableCollection<IInputOutputViewModel> Outputs { get; set; }
        ObservableCollection<IInputOutputViewModel> Inputs { get; set; }
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
