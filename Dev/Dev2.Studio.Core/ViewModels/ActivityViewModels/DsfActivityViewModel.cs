using Caliburn.Micro;
using Dev2.Studio.Core.Activities.TO;
using Dev2.Studio.Core.Activities.Translators;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.ErrorHandling;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.ViewModels.Base;
using System;
using System.Activities;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

namespace Dev2.Studio.Core.ViewModels.ActivityViewModels
{
    public class DsfActivityViewModel : BaseActivityViewModel, INotifyPropertyChanged, IDisposable,IHandle<HasWizardMessage>
    {
        #region Fields

        private string _hasWizKeyToDereg;
        //private string _getMappingKeyToDereg;
        private ICommand _openMappingCommand;
        private ICommand _openParentCommand;
        private enErrorType _worstError;
        private ModelItem _modelItem;
        private bool _showMapping;
        private bool _showMappingPreviousValue;

        private ObservableCollection<KeyValuePair<string, string>> _propertyCollection;
        //private ObservableCollection<IInputOutputViewModel> _inputs;
        //private ObservableCollection<IInputOutputViewModel> _outputs;
        private IDataMappingViewModel _dataMappingViewModel;

        #endregion Fields

        #region Ctor

        public DsfActivityViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            _modelItem = modelItem;
            EventAggregator.Subscribe(this);
            // _getMappingKeyToDereg = Mediator.RegisterToReceiveMessage(MediatorMessages.GetMappingViewModel, input => SetMappingViewModel(input as DataMappingViewModel));
            SetViewModelProperties(modelItem);
        }

        #endregion Ctor

        #region Properties

        public IDataMappingViewModel DataMappingViewModel
        {
            get
            {
                return _dataMappingViewModel;
            }
            set
            {
                _dataMappingViewModel = value;
                OnPropertyChanged("DataMappingViewModel");
            }
        }

        public ObservableCollection<KeyValuePair<string, string>> PropertyCollection
        {
            get
            {
                return _propertyCollection;
            }
            set
            {
                _propertyCollection = value;
                OnPropertyChanged("PropertyCollection");
            }
        }

        public ObservableCollection<Dev2ErrorObject> ErrorCollection { get; set; }

        public enErrorType WorstError
        {
            get
            {
                _worstError = enErrorType.Correct;
                if (ErrorCollection.FirstOrDefault(c => c.ErrorType == enErrorType.Critical) != null)
                {
                    _worstError = enErrorType.Critical;
                }
                else if (ErrorCollection.FirstOrDefault(c => c.ErrorType == enErrorType.Warning) != null)
                {
                    _worstError = enErrorType.Warning;
                }

                return _worstError;
            }
            set
            {
                _worstError = value;
            }
        }

        public bool ShowMapping
        {
            get
            {
                return _showMapping;
            }
            set
            {
                _showMapping = value;
                OnPropertyChanged("ShowMapping");
                OnPropertyChanged("ActualSupressConnectorNodes");
            }
        }

        public bool ShowMappingPreviousValue
        {
            get
            {
                return _showMappingPreviousValue;
            }
            set
            {
                _showMappingPreviousValue = value;
                OnPropertyChanged("ShowMappingPreviousValue");
            }
        }

        //public ObservableCollection<IInputOutputViewModel> Inputs
        //{
        //    get
        //    {
        //        return _inputs;
        //    }
        //    set
        //    {
        //        _inputs = value;
        //        OnPropertyChanged("Inputs");
        //    }
        //}

        //public ObservableCollection<IInputOutputViewModel> Outputs
        //{
        //    get
        //    {
        //        return _outputs;
        //    }
        //    set
        //    {
        //        _outputs = value;
        //        OnPropertyChanged("Outputs");
        //    }
        //}

        #endregion Properties

        #region Commands

        public ICommand OpenMappingCommand
        {
            get
            {
                if (_openMappingCommand == null)
                {
                    _openMappingCommand = new RelayCommand(param => OpenMapping());
                }
                return _openMappingCommand;
            }
        }

        public ICommand OpenParentCommand
        {
            get
            {
                if (_openParentCommand == null)
                {
                    _openParentCommand = new RelayCommand(param => OpenParent());
                }
                return _openParentCommand;
            }
        }

        private void OpenMapping()
        {
            //Mediator.SendMessage(MediatorMessages.GetMappingViewModel, _modelItem);
            if (ShowMapping)
            {
                ShowMapping = false;
            }
            else
            {
                ShowMapping = true;
            }
        }


        private void OpenParent()
        {
            EventAggregator.Publish(new EditActivityMessage(_modelItem));
        }

        #endregion Commands

        #region Methods

        public override void SetViewModelProperties(ModelItem _model)
        {
            if (_model != null)
            {

                IconPath = ModelItemUtils.GetProperty("IconPath", _model) as string;
                InArgument argument = ModelItemUtils.GetProperty("HelpLink", _model) as InArgument;
                if (argument != null)
                {
                    HelpLink = argument.Expression.ToString();
                    if (!string.IsNullOrWhiteSpace(HelpLink))
                    {
                        HasHelpLink = true;
                    }
                }
                SeriveName = ModelItemUtils.GetProperty("ServiceName", _model) as string;
                PropertyCollection = new ObservableCollection<KeyValuePair<string, string>>();
                ErrorCollection = new ObservableCollection<Dev2ErrorObject>();
                //Test Dummy data until the error handeling is implemented
                ErrorCollection.Add(new Dev2ErrorObject()
                {
                    ErrorType = enErrorType.Correct,
                    UserErrorMessage = "Service Working Normaly"
                });
                //ErrorCollection.Add(new Dev2ErrorObject() { ErrorType = enErrorType.Warning, UserErrorMessage = "Mapping Out Of Date" });
                //ErrorCollection.Add(new Dev2ErrorObject() { ErrorType = enErrorType.Critical, UserErrorMessage = "Source Not Available" });            

                ServiceXmlTranslator translator = new ServiceXmlTranslator();
                ActivityViewModelTO transObject = translator.GetActivityViewModelTO(_model);
                if (transObject != null && PropertyCollection != null)
                {
                    if (!string.IsNullOrWhiteSpace(transObject.SourceName))
                    {
                        PropertyCollection.Add(new KeyValuePair<string, string>("Source :", transObject.SourceName));
                    }
                    if (!string.IsNullOrWhiteSpace(transObject.Type))
                    {
                        PropertyCollection.Add(new KeyValuePair<string, string>("Type :", transObject.Type));
                    }
                    if (!string.IsNullOrWhiteSpace(transObject.Action))
                    {
                        PropertyCollection.Add(new KeyValuePair<string, string>("Procedure :", transObject.Action));
                    }
                    if (!string.IsNullOrWhiteSpace(transObject.Simulation))
                    {
                        PropertyCollection.Add(new KeyValuePair<string, string>("Simulation :", transObject.Simulation));
                    }
                }
                EventAggregator.Publish(new DoesActivityHaveWizardMessage(_model));
            }
        }

        public void SetInputs()
        {
            string inputString = DataMappingViewModel.GetInputString(DataMappingViewModel.Inputs);
            ModelItemUtils.SetProperty("InputMapping", inputString, _modelItem);
        }

        public void SetOuputs()
        {
            string outputString = DataMappingViewModel.GetOutputString(DataMappingViewModel.Outputs);
            ModelItemUtils.SetProperty("OutputMapping", outputString, _modelItem);
        }

        #endregion Methods

        #region Private Methods

        //private void SetMappingViewModel(DataMappingViewModel dataMappingViewModel)
        //{
        //    if (dataMappingViewModel != null && dataMappingViewModel.ActivityName == SeriveName)
        //    {
        //        DataMappingViewModel = dataMappingViewModel;
        //    }
        //}

        #endregion Private Methods

        #region Dispose
        public void Dispose()
        {
            if (PropertyCollection != null)
            {
                PropertyCollection.Clear();
            }


            if (ErrorCollection != null)
            {
                ErrorCollection.Clear();
            }

            _modelItem = null;
            DataMappingViewModel = null;

            EventAggregator.Unsubscribe(this);
        }
        #endregion Dispose

        #region Implementation of IHandle<HasWizardMessage>

        public void Handle(HasWizardMessage message)
        {
            HasWizard = message.HasWizard;
        }

        #endregion
    }

   
}
