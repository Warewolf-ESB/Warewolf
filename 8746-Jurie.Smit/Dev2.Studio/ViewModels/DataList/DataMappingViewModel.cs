using Dev2.Composition;
using Dev2.Data.Binary_Objects;
using Dev2.DataList.Contract;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Actions;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Interfaces.DataList;
using Dev2.Studio.Core.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Xml;
using Dev2.Studio.Factory;
using Dev2.Studio.ViewModels.DataList.Actions;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Unlimited.Applications.BusinessDesignStudio.Undo;

namespace Dev2.Studio.ViewModels.DataList
{
    public class DataMappingViewModel : SimpleBaseViewModel, IDataMappingViewModel
    {
        #region Locals

        private IWebActivity _activity;
        private ObservableCollection<IInputOutputViewModel> _outputs;
        private ObservableCollection<IInputOutputViewModel> _inputs;
        public bool _isInitialLoad = false;
        RelayCommand _autoMappingInput;
        RelayCommand _undo;
        RelayCommand _redo;
        RelayCommand _autoMappingOutput;
        ActionManager _actionManager;
        private string _activityName;
        private string _xmlOutput;

        private IInputOutputViewModel _currentlySelectedOutput;
        private IInputOutputViewModel _currentlySelectedInput;
        private IDataListViewModel _dataListViewModel;

        #endregion Locals

        #region Imports

        public IMainViewModel MainViewModel { get; set; }

        #endregion Imports

        #region Ctor
        public DataMappingViewModel(IWebActivity activity)
        {
            MainViewModel = ImportService.GetExportValue<IMainViewModel>();

            _activity = activity;
            _actionManager = new ActionManager();
            _inputs = new ObservableCollection<IInputOutputViewModel>();
            _outputs = new ObservableCollection<IInputOutputViewModel>();

            Initialize(_activity);
        }
        #endregion Ctor

        #region Initialize
        internal void Initialize(IWebActivity activity)
        {
            //if (activity.ResourceModel == null) return;

            Activity = activity;
            if (Activity.UnderlyingWebActivityObjectType != typeof(DsfWebPageActivity))
            {
                _dataListViewModel = DataListViewModelFactory.CreateDataListViewModel(activity.ResourceModel);
            }
            if (string.IsNullOrEmpty(activity.SavedInputMapping) && string.IsNullOrEmpty(activity.SavedOutputMapping))
            {
                IsInitialLoad = true;
            }

            string liveDataListAsInputMapping;
            string liveDataListAsOutputMapping;

            ActivityName = activity.ServiceName;

            if (Activity.UnderlyingWebActivityObjectType == typeof(DsfWebPageActivity))
            {
                string sanitizedWebpageObject = activity.XMLConfiguration.Replace("&gt;", ">").Replace("&lt;", "<");
                //Massimo.Guerrera - 25-01-2013 - Added for the mapping for webpages        
                IDataListViewModel activeDataList = DataListSingleton.ActiveDataList;
                string dataListString = activeDataList.WriteToResourceModel();
                liveDataListAsInputMapping = DataListFactory.GenerateMappingFromWebpage(sanitizedWebpageObject, dataListString, enDev2ArgumentType.Input);
                liveDataListAsOutputMapping = DataListFactory.GenerateMappingFromWebpage(sanitizedWebpageObject, dataListString, enDev2ArgumentType.Output);

                //string otherInputMapping = DataListFactory.GenerateMappingFromDataList(dataListString,
                //                                                                       enDev2ArgumentType.Input,
                //                                                                        enDev2ColumnArgumentDirection
                //                                                                           .Input);
                //string otherOutputMapping = DataListFactory.GenerateMappingFromDataList(dataListString,
                //                                                                       enDev2ArgumentType.Output,
                //                                                                       enDev2ColumnArgumentDirection
                //                                                                           .Output);
                //otherInputMapping = otherInputMapping.Replace(InputsOpenTag, string.Empty).Replace(InputsCloseTag, string.Empty);
                //otherOutputMapping = otherOutputMapping.Replace(OutputsOpenTag, string.Empty).Replace(OutputsCloseTag, string.Empty);
                //liveDataListAsInputMapping =
                //    liveDataListAsInputMapping.Insert(liveDataListAsInputMapping.IndexOf(InputsCloseTag, StringComparison.Ordinal), otherInputMapping);
                //liveDataListAsOutputMapping =
                //    liveDataListAsOutputMapping.Insert(liveDataListAsOutputMapping.IndexOf(OutputsCloseTag, StringComparison.Ordinal), otherOutputMapping);

            }
            else
            {

                XmlDocument xDoc = new XmlDocument();
                xDoc.LoadXml(Activity.ResourceModel.ServiceDefinition);
                if (Activity.ResourceModel.ResourceType == ResourceType.WorkflowService)
                {
                    liveDataListAsInputMapping = DataListFactory.GenerateMappingFromDataList(_dataListViewModel.Resource.DataList, enDev2ArgumentType.Input, enDev2ColumnArgumentDirection.Input);
                    liveDataListAsOutputMapping = DataListFactory.GenerateMappingFromDataList(_dataListViewModel.Resource.DataList, enDev2ArgumentType.Output, enDev2ColumnArgumentDirection.Output);
                }
                else
                {
                    var input = xDoc.SelectSingleNode(StringResources.DataMapping_InputXpathExpression);
                    var output = xDoc.SelectSingleNode(StringResources.DataMapping_OutputXpathExpression);

                    if (input == null || output == null)
                    {
                        return;
                    }

                    liveDataListAsInputMapping = input.OuterXml;
                    liveDataListAsOutputMapping = output.OuterXml;

                }
            }

            activity.LiveInputMapping = liveDataListAsInputMapping;
            activity.LiveOutputMapping = liveDataListAsOutputMapping;

            IList<IDev2Definition> liveOutput = DataMappingListFactory.CreateListOutputMapping(Activity.LiveOutputMapping);
            IList<IInputOutputViewModel> liveOutputMappingCopy = DataMappingListFactory.CreateListToDisplayOutputs(liveOutput);

            IList<IDev2Definition> liveInput = DataMappingListFactory.CreateListInputMapping(Activity.LiveInputMapping);
            IList<IInputOutputViewModel> liveInputMappingCopy = DataMappingListFactory.CreateListToDisplayInputs(liveInput);

            if (string.IsNullOrEmpty(Activity.SavedInputMapping) && string.IsNullOrEmpty(Activity.SavedOutputMapping))
            {
                Activity.SavedInputMapping = Activity.LiveInputMapping;
                Activity.SavedOutputMapping = Activity.LiveOutputMapping;
            }
            else
            {
                IList<IDev2Definition> savedOutput = DataMappingListFactory.CreateListOutputMapping(Activity.SavedOutputMapping);
                IList<IInputOutputViewModel> savedOutputMappingCopy = DataMappingListFactory.CreateListToDisplayOutputs(savedOutput);
                if (liveOutputMappingCopy.Any())
                {
                    int outputsCounter = 0;
                    while (outputsCounter < liveOutputMappingCopy.Count)
                    {
                        try
                        {
                            liveOutputMappingCopy[outputsCounter] = savedOutputMappingCopy.First(c => c.DisplayName == liveOutputMappingCopy[outputsCounter].DisplayName);
                        }
                        catch (Exception)
                        {
                        }
                        outputsCounter++;
                    }
                    CurrentlySelectedOutput = liveOutputMappingCopy[0];
                }

                IList<IDev2Definition> savedInput = DataMappingListFactory.CreateListInputMapping(Activity.SavedInputMapping);
                IList<IInputOutputViewModel> savedInputMappingCopy = DataMappingListFactory.CreateListToDisplayInputs(savedInput);
                if (liveInputMappingCopy.Any())
                {
                    int inputsCounter = 0;
                    while (inputsCounter < liveInputMappingCopy.Count)
                    {
                        try
                        {
                            liveInputMappingCopy[inputsCounter] = savedInputMappingCopy.First(c => c.DisplayName == liveInputMappingCopy[inputsCounter].DisplayName);
                        }
                        catch (Exception)
                        {
                        }
                        inputsCounter++;
                    }

                    CurrentlySelectedInput = liveInputMappingCopy[0];
                }
            }

            Outputs = liveOutputMappingCopy.ToObservableCollection();
            Inputs = liveInputMappingCopy.ToObservableCollection();
            //if (IsInitialLoad) {
            //    AutoMappingInput(Activity, IsInitialLoad);
            //    AutoMappingOutput(Activity, IsInitialLoad);
            //    IsInitialLoad = false;
            //}
            CreateXmlOutput(_outputs, _inputs);
        }

        #endregion Initialize

        #region Bindings
        public string XmlOutput
        {
            get
            {
                return _xmlOutput;
            }
            set
            {
                _xmlOutput = value;
                OnPropertyChanged("XmlOutput");
            }
        }

        public IWebActivity Activity
        {
            get
            {
                return _activity;
            }
            set
            {
                _activity = value;
                OnPropertyChanged("Activity");
            }
        }

        public bool IsInitialLoad
        {
            get
            {
                return _isInitialLoad;
            }
            set
            {
                _isInitialLoad = value;
            }
        }

        public string ActivityName
        {
            get
            {
                return _activityName;
            }
            set
            {
                _activityName = value;
                OnPropertyChanged("ActivityName");
            }
        }

        public ObservableCollection<IInputOutputViewModel> Outputs
        {
            get
            {
                return _outputs.ToObservableCollection();
            }
            set
            {
                _outputs = value;
                OnPropertyChanged("Outputs");
            }
        }

        public ObservableCollection<IInputOutputViewModel> Inputs
        {
            get
            {
                return _inputs.ToObservableCollection();
            }
            set
            {
                _inputs = value;
                OnPropertyChanged("Inputs");
            }
        }

        public ICommand AutoMappingInputCommand
        {
            get
            {
                if (_autoMappingInput == null)
                {
                    _autoMappingInput = new RelayCommand(c => AutoMappingInput(Activity));
                }

                return _autoMappingInput;
            }
        }

        public ICommand AutoMappingOutputCommand
        {
            get
            {
                if (_autoMappingOutput == null)
                {
                    _autoMappingOutput = new RelayCommand(c => AutoMappingOutput(Activity));
                }

                return _autoMappingOutput;
            }
        }

        public ICommand UndoCommand
        {
            get
            {
                if (_undo == null)
                {
                    _undo = new RelayCommand(c => Undo());
                }
                return _undo;
            }
        }

        public ICommand RedoCommand
        {
            get
            {
                if (_redo == null)
                {
                    _redo = new RelayCommand(c => Redo());
                }
                return _redo;
            }
        }

        public void Undo()
        {
            UndoFramework.Undo();
        }

        public void Redo()
        {
            UndoFramework.Redo();
        }

        public void AutoMappingInput(IWebActivity activity)
        {
            var autoMapInputs = new AutoMappingInputAction(this, activity);

            UndoFramework.RecordAction(autoMapInputs);

        }

        public void AutoMappingOutput(IWebActivity activity)
        {
            var autoMapOutputs = new AutoMappingOutputAction(this, activity);
            UndoFramework.RecordAction(autoMapOutputs);
        }

        public void CopyFrom(IDataMappingViewModel copyObj)
        {
            IObjectCloner<IInputOutputViewModel> cloner = new ObjectCloner<IInputOutputViewModel>();

            Inputs = cloner.CloneObservableCollection(copyObj.Inputs);
            Outputs = cloner.CloneObservableCollection(copyObj.Outputs);
            XmlOutput = copyObj.XmlOutput;

        }

        public ActionManager UndoFramework
        {
            get
            {
                return _actionManager;
            }
        }
        #endregion Bindings

        #region Final Output
        public void CreateXmlOutput(IList<IInputOutputViewModel> outputData, IList<IInputOutputViewModel> inputData)
        {
            string outputString = GetOutputString(outputData);
            XmlOutput = outputString;
            Activity.SavedOutputMapping = outputString;
            Activity.LiveOutputMapping = Activity.SavedOutputMapping;


            string inputString = GetInputString(inputData);
            XmlOutput = (XmlOutput + inputString);
            Activity.SavedInputMapping = inputString;
            Activity.LiveInputMapping = Activity.SavedInputMapping;
        }
        #endregion Final Output

        #region Work Around Till Refactor

        public string GetInputString(IList<IInputOutputViewModel> inputData)
        {
            string inputString = string.Empty;
            IList<IDev2Definition> inputs = new List<IDev2Definition>();
            if (inputData.Count != 0)
            {
                foreach (IInputOutputViewModel itp in inputData)
                {
                    inputs.Add(itp.GetGenerationTO());
                }
                inputString = DataMappingListFactory.GenerateMapping(inputs, enDev2ArgumentType.Input);
            }
            return inputString;
        }

        public string GetOutputString(IList<IInputOutputViewModel> outputData)
        {
            string outputString = string.Empty;
            IList<IDev2Definition> outputs = new List<IDev2Definition>();
            if (outputData.Count != 0)
            {
                foreach (IInputOutputViewModel otp in outputData)
                {
                    outputs.Add(otp.GetGenerationTO());
                }
                outputString = DataMappingListFactory.GenerateMapping(outputs, enDev2ArgumentType.Output);
            }
            return outputString;
        }

        #endregion Work Around Till Refactor

        #region Get Current Output Item

        public IInputOutputViewModel CurrentlySelectedOutput
        {
            get
            {
                return _currentlySelectedOutput;
            }
            set
            {
                _currentlySelectedOutput = value;
                base.OnPropertyChanged("CurrentlySelectedOutput");
            }
        }
        #endregion Get Current Output Item

        #region Get Current Input Item

        public IInputOutputViewModel CurrentlySelectedInput
        {
            get
            {
                return _currentlySelectedInput;
            }
            set
            {
                _currentlySelectedInput = value;
                base.OnPropertyChanged("CurrentlySelectedInput");
            }
        }
        #endregion Get Current Input Item

        #region Events
        public void InputLostFocusTextBox(string text)
        {
            CurrentlySelectedInput.MapsTo = text;
            CurrentlySelectedInput.Value = text;
            CreateXmlOutput(Outputs, Inputs);
        }

        public void OutputLostFocusTextBox(string text)
        {
            CurrentlySelectedOutput.Value = text;
            CreateXmlOutput(Outputs, Inputs);
        }

        public void InputTextBoxGotFocus(IInputOutputViewModel Selected)
        {
            CurrentlySelectedInput = Selected;
            CreateXmlOutput(Outputs, Inputs);
        }

        public void OutputTextBoxGotFocus(IInputOutputViewModel Selected)
        {
            //CurrentlySelectedOutput = Selected;
            CreateXmlOutput(Outputs, Inputs);
        }
        #endregion Events
    }
}
