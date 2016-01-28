
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
using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;
using Dev2.Common.Interfaces.DB;
using Dev2.Interfaces;

namespace Dev2.Activities.Designers2.Core
{
    public abstract class CustomToolViewModelBase<T> : ActivityDesignerViewModel, INotifyPropertyChanged
    {
        double _inputGridHeight = 60;
        double _outputGridHeight = 60;
        const double RowHeight = 30;
        double _inputsMinHeight;
        double _outputsMinHeight;
        bool _inputsHasItems;
        bool _outputsHasItems;
        double _designHeight;
        double _designMinHeight;
        double _designMaxHeight;
        bool _testComplete;
        protected string _recordsetName;
        protected bool PreviousInputsVisible;
        private bool _inputsVisible;
        protected bool PreviousTestComplete;

        protected ICollection<IServiceInput> _inputs;

        protected CustomToolViewModelBase(ModelItem modelItem)
            : base(modelItem)
        {
        }

        public double DesignMaxHeight
        {
            get
            {
                return _designMaxHeight;
            }
            set
            {
                _designMaxHeight = value;
                OnPropertyChanged("DesignMaxHeight");
            }
        }
        public double DesignMinHeight
        {
            get
            {
                return _designMinHeight;
            }
            set
            {
                _designMinHeight = value;
                OnPropertyChanged("DesignMinHeight");
            }
        }
        public double DesignHeight
        {
            get
            {
                return _designHeight;
            }
            set
            {
                _designHeight = value;
                OnPropertyChanged("DesignHeight");
            }
        }
        public double InputsMinHeight
        {
            get
            {
                return _inputsMinHeight;
            }
            set
            {
                _inputsMinHeight = value;
                OnPropertyChanged("InputsMinHeight");
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
        public double OutputsMinHeight
        {
            get
            {
                return _outputsMinHeight;
            }
            set
            {
                _outputsMinHeight = value;
                OnPropertyChanged("OutputsMinHeight");
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


        public void SetToolHeight()
        {
            if (TestComplete)
            {
                if (Inputs != null && InputsVisible)
                {
                    switch (Inputs.Count)
                    {
                        case 0:
                            // Add the Grid Height to the tool Height
                            ToolHeight += _inputGridHeight + 10;
                            InputsHasItems = false;
                            break;
                        default:
                            /* 30px used for row Height multiply by Inputs count plus 1 extra row
                             * Add grid Height to tool Height original Height value
                             * Set Maximum tool Height to calculated tool Height */
                            if (Inputs.Count > 0 && Inputs.Count < 5 && Outputs != null && TestComplete)
                            {
                                _inputGridHeight = RowHeight * (Inputs.Count + 1);
                                InputsMinHeight = _inputGridHeight;
                                ToolHeight += _inputGridHeight + RowHeight;
                                MaxToolHeight = ToolHeight;
                                InputsHasItems = false;
                            }
                            else if (Inputs.Count > 0 && Inputs.Count < 5)
                            {
                                _inputGridHeight = RowHeight * (Inputs.Count + 1);
                                InputsMinHeight = _inputGridHeight;
                                ToolHeight += _inputGridHeight + RowHeight;
                                MaxToolHeight = ToolHeight;
                                InputsHasItems = false;
                            }
                            else
                            {
                                // Set grid Height to Maximum 6 count to allow for scroll option and Maximize design
                                _inputGridHeight = RowHeight * 6;
                                InputsMinHeight = _inputGridHeight;
                                ToolHeight += _inputGridHeight + RowHeight;
                                MaxToolHeight += (RowHeight * Inputs.Count + 2) + RowHeight + 15;
                                InputsHasItems = true;
                            }
                            break;
                    }
                }
                if (Outputs != null)
                {
                    switch (Outputs.Count)
                    {
                        case 0:
                            // Add the Grid Height to the tool Height
                            ToolHeight += _outputGridHeight + 10;
                            OutputsHasItems = false;
                            break;
                        default:
                            /* 30px used for row Height multiply by Outputs count plus 1 extra row
                             * Add grid Height to tool Height original Height value
                             * Set Maximum tool Height to calculated tool Height */
                            if (Inputs != null && (Outputs.Count > 0 && Outputs.Count < 5 && Inputs.Count > 0 && Inputs.Count < 5))
                            {
                                _outputGridHeight = RowHeight * (Outputs.Count + 1);
                                OutputsMinHeight = _outputGridHeight;
                                ToolHeight += _outputGridHeight + RowHeight;
                                MaxToolHeight = ToolHeight;
                                OutputsHasItems = false;
                            }
                            else if (Inputs != null && (Outputs.Count > 0 && Outputs.Count < 5 && Inputs.Count > 5))
                            {
                                _outputGridHeight = RowHeight * (Outputs.Count + 1);
                                OutputsMinHeight = _outputGridHeight;
                                MaxToolHeight += OutputsMinHeight;
                                OutputsHasItems = false;
                            }
                            else if (Outputs.Count > 0 && Outputs.Count < 5)
                            {
                                _outputGridHeight = RowHeight * (Outputs.Count + 1);
                                OutputsMinHeight = _outputGridHeight;
                                ToolHeight += _outputGridHeight + RowHeight;
                                MaxToolHeight = ToolHeight;
                                OutputsHasItems = false;
                            }
                            else
                            {
                                // Set grid Height to Maximum 6 count to allow for scroll option and Maximize design
                                _outputGridHeight = RowHeight * 6;
                                OutputsMinHeight = _outputGridHeight;
                                ToolHeight += _outputGridHeight + RowHeight;
                                MaxToolHeight += (RowHeight * (Outputs.Count + 2)) + RowHeight + 15;
                                OutputsHasItems = true;
                            }
                            break;
                    }
                }
                SetToolHeightValue(ToolHeight);
            }
            else if (InputsVisible)
            {
                if (Inputs != null)
                {
                    switch (Inputs.Count)
                    {
                        case 0:
                            // Add the Grid Height to the tool Height
                            ToolHeight += _inputGridHeight + 10;
                            break;
                        default:
                            /* 30px used for row Height multiply by Inputs count plus 1 extra row
                             * Add grid Height to tool Height original Height value
                             * Set Maximum tool Height to calculated tool Height */
                            if (Inputs.Count > 0 && Inputs.Count < 5)
                            {
                                _inputGridHeight = RowHeight * (Inputs.Count + 1);
                                InputsMinHeight = _inputGridHeight;
                                ToolHeight += _inputGridHeight + RowHeight;
                                MaxToolHeight = ToolHeight;
                            }
                            else
                            {
                                // Set grid Height to Maximum 6 count to allow for scroll option and Maximize design
                                _inputGridHeight = RowHeight * 6;
                                InputsMinHeight = _inputGridHeight;
                                ToolHeight += _inputGridHeight + RowHeight;
                                MaxToolHeight += (RowHeight * Inputs.Count) + 10;
                            }
                            break;
                    }
                }
                SetToolHeightValue(ToolHeight);
            }
            else
            {
                SetInitialHeight();
            }
        }

        public void SetInitialHeight()
        {
            DesignHeight = ToolHeight;
            DesignMinHeight = ToolHeight;
            DesignMaxHeight = MaxToolHeight;
        }

        void SetToolHeightValue(double newToolHeight)
        {
            DesignHeight = newToolHeight;
            DesignMinHeight = newToolHeight;
            DesignMaxHeight = MaxToolHeight;
            InputsMinHeight = _inputGridHeight;
            OutputsMinHeight = _outputGridHeight;
        }

        public void ResetHeightValues(double height)
        {
            // Reset the values
            _inputGridHeight = 60;
            _outputGridHeight = 60;
            ToolHeight = height;
            MaxToolHeight = height;
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
        [ExcludeFromCodeCoverage]
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