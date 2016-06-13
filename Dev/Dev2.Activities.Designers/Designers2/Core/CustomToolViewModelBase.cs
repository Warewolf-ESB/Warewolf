
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Dev2.Common.Interfaces.DB;
using Dev2.Interfaces;

namespace Dev2.Activities.Designers2.Core
{
    public abstract class CustomToolViewModelBase<T> : ActivityDesignerViewModel, INotifyPropertyChanged
    {
        bool _inputsHasItems;
        bool _outputsHasItems;
        bool _testComplete;
        private string _recordsetName;
        protected bool PreviousInputsVisible;
        private bool _inputsVisible;
        protected bool PreviousTestComplete;

        private ICollection<IServiceInput> _inputs;
        double _labelWidth;
        bool _isInputsEmptyRows;
        bool _isOutputsEmptyRows;

        protected CustomToolViewModelBase(ModelItem modelItem)
            : base(modelItem)
        {
        }

        public double LabelWidth
        {
            get
            {
                return _labelWidth;
            }
            set
            {
                _labelWidth = value;
                OnPropertyChanged("LabelWidth");
            }
        }
        public bool InputsHasItems
        {
            get
            {
                return _inputsHasItems;
            }
            set
            {
                _inputsHasItems = value;
                OnPropertyChanged("InputsHasItems");
            }
        }
        public bool OutputsHasItems
        {
            get
            {
                return _outputsHasItems;
            }
            set
            {
                _outputsHasItems = value;
                OnPropertyChanged("OutputsHasItems");
            }
        }
        public bool TestComplete
        {
            get
            {
                return _testComplete;
            }
            set
            {
                PreviousTestComplete = _testComplete;
                _testComplete = value;
                OnPropertyChanged("TestComplete");
            }
        }
        public bool InputsVisible
        {
            get
            {
                return _inputsVisible;
            }
            set
            {
                PreviousInputsVisible = _inputsVisible;
                _inputsVisible = value;
                OnPropertyChanged("InputsVisible");
            }
        }
        public bool IsInputsEmptyRows
        {
            get
            {
                return _isInputsEmptyRows;
            }
            set
            {
                _isInputsEmptyRows = value;
                OnPropertyChanged("IsInputsEmptyRows");
            }
        }
        public bool IsOutputsEmptyRows
        {
            get
            {
                return _isOutputsEmptyRows;
            }
            set
            {
                _isOutputsEmptyRows = value;
                OnPropertyChanged("IsOutputsEmptyRows");
            }
        }
        public bool SourceVisible { get; set; }
        public ICommand TestInputCommand { get; set; }
        public ICommand EditSourceCommand { get; set; }
        public ICommand NewSourceCommand { get; set; }
        public ICollection<IServiceInput> Inputs
        {
            get
            {
                var serviceInputs = GetProperty<ICollection<IServiceInput>>();
                return serviceInputs;
            }
            set
            {
                if (!Equals(value, _inputs))
                {
                    _inputs = value;
                    InputsHasItems = _inputs != null && _inputs.Count > 0;
                    IsInputsEmptyRows = _inputs != null && _inputs.Count == 0;
                    SetProperty(value);
                    OnPropertyChanged("Inputs");
                }
            }
        }
        public ICollection<IServiceOutputMapping> Outputs
        {
            get
            {
                var serviceOutputMappings = GetProperty<ICollection<IServiceOutputMapping>>();
                return serviceOutputMappings;
            }
            set
            {
                SetProperty(value);
                OutputsHasItems = value != null && value.Count > 0;
                IsOutputsEmptyRows = value != null && value.Count == 0;
                OnPropertyChanged("Outputs");
            }
        }
        public abstract double ToolHeight { get; set; }
        public abstract double MaxToolHeight { get; set; }
        public abstract ObservableCollection<T> Sources { get; set; }
        public abstract T SelectedSource { get; set; }
        public string RecordsetName
        {
            get
            {
                return _recordsetName;
            }
            set
            {
                if (Outputs != null)
                {
                    foreach (var serviceOutputMapping in Outputs)
                    {
                        if (_recordsetName != null && serviceOutputMapping.RecordSetName != null && serviceOutputMapping.RecordSetName.Equals(_recordsetName))
                        {
                            serviceOutputMapping.RecordSetName = value;
                        }
                    }
                }
                _recordsetName = value;
                OnPropertyChanged("RecordsetName");
            }
        }

        public override void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IMainViewModel>();
            if (mainViewModel != null)
            {
                mainViewModel.HelpViewModel.UpdateHelpText(helpText);
            }
        }

        #region Implementation of INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

    }
}