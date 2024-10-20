﻿using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Dev2.Activities.Designers2.Core.CloneInputRegion;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ToolBase;
using Dev2.Common.Interfaces.ToolBase.WCF;
using Dev2.Studio.Core.Activities.Utils;
using Microsoft.Practices.Prism;



namespace Dev2.Activities.Designers2.Core.InputRegion
{
    public class WcfInputRegion : IWcfInputRegion
    {
        readonly ModelItem _modelItem;
        readonly IActionToolRegion<IWcfAction> _action;
        bool _isEnabled;

        ICollection<IServiceInput> _inputs;
        bool _isInputsEmptyRows;


        public WcfInputRegion()
        {
            ToolRegionName = "WcfInputRegion";
        }
        readonly IActionInputDatatalistMapper _datatalistMapper;
        public WcfInputRegion(ModelItem modelItem, IActionToolRegion<IWcfAction> action)
                   : this(new ActionInputDatalistMapper())
        {
            ToolRegionName = "WcfInputRegion";
            _modelItem = modelItem;
            _action = action;
            _action.SomethingChanged += SourceOnSomethingChanged;
            var inputsFromModel = _modelItem.GetProperty<List<IServiceInput>>("Inputs");
            var serviceInputs = inputsFromModel ?? new List<IServiceInput>();
            Inputs = new ObservableCollection<IServiceInput>();
            var inputs = new ObservableCollection<IServiceInput>();
            inputs.CollectionChanged += InputsCollectionChanged;
            inputs.AddRange(serviceInputs);
            Inputs = inputs;
            if (inputsFromModel == null)
            {
                UpdateOnActionSelection();
            }

            IsEnabled = action?.SelectedAction != null;
        }
        
        public WcfInputRegion(IActionInputDatatalistMapper datatalistMapper)
        {
            _datatalistMapper = datatalistMapper;
        }


        void InputsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            AddItemPropertyChangeEvent(e);
            RemoveItemPropertyChangeEvent(e);
        }

        void AddItemPropertyChangeEvent(NotifyCollectionChangedEventArgs args)
        {
            if (args.NewItems == null)
            {
                return;
            }

            foreach (INotifyPropertyChanged item in args.NewItems)
            {
                if (item != null)
                {
                    item.PropertyChanged += ItemPropertyChanged;
                }
            }
        }

        void ItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            _modelItem.SetProperty("Inputs", Inputs.ToList());
        }

        void RemoveItemPropertyChangeEvent(NotifyCollectionChangedEventArgs args)
        {
            if (args.OldItems == null)
            {
                return;
            }

            foreach (INotifyPropertyChanged item in args.OldItems)
            {
                if (item != null)
                {
                    item.PropertyChanged -= ItemPropertyChanged;
                }
            }
        }


        void SourceOnSomethingChanged(object sender, IToolRegion args)
        {
            try
            {
                Errors.Clear();


                UpdateOnActionSelection();

                OnPropertyChanged(@"Inputs");
                OnPropertyChanged(@"IsEnabled");
            }
            catch (Exception e)
            {
                Errors.Add(e.Message);
            }
            finally
            {
                CallErrorsEventHandler();
            }
        }

        void CallErrorsEventHandler()
        {
            ErrorsHandler?.Invoke(this, new List<string>(Errors));
        }

        void UpdateOnActionSelection()
        {
            Inputs = new List<IServiceInput>();
            IsEnabled = false;
            if (_action?.SelectedAction != null)
            {
                Inputs = _action.SelectedAction.Inputs;
                _datatalistMapper.MapInputsToDatalist(Inputs);
                IsInputsEmptyRows = Inputs.Count < 1;
                IsEnabled = true;
            }
            OnPropertyChanged("Inputs");
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
                OnPropertyChanged();
            }
        }

        #region Implementation of IToolRegion

        public string ToolRegionName { get; set; }
        public bool IsEnabled
        {
            get
            {
                return _isEnabled;
            }
            set
            {
                _isEnabled = value;
                OnPropertyChanged();
            }
        }

        public IList<IToolRegion> Dependants { get; set; }

        public IToolRegion CloneRegion()
        {
            var inputs2 = new List<IServiceInput>(Inputs);
            return new WcfInputRegionClone
            {
                Inputs = inputs2,
                IsEnabled = IsEnabled
            } as IToolRegion;
        }

        public void RestoreRegion(IToolRegion toRestore)
        {
            if (toRestore is WcfInputRegionClone region)
            {
                Inputs.Clear();
                if (region.Inputs != null)
                {
                    var inp = region.Inputs.ToList();

                    Inputs = inp;
                }
                OnPropertyChanged("Inputs");
                IsInputsEmptyRows = Inputs == null || Inputs.Count == 0;
            }
        }

        public EventHandler<List<string>> ErrorsHandler
        {
            get;
            set;
        }

        public IList<string> Errors
        {
            get
            {
                IList<string> errors = new List<string>();
                return errors;
            }
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region Implementation of IDotNetInputRegion

        public ICollection<IServiceInput> Inputs
        {
            get
            {
                return _inputs;
            }
            set
            {
                if (value != null)
                {
                    _inputs = value;
                    _modelItem.SetProperty("Inputs", value.ToList());
                    OnPropertyChanged();
                }
                else
                {
                    _inputs.Clear();
                    _modelItem.SetProperty("Outputs", _inputs.ToList());
                    OnPropertyChanged();
                }

            }
        }

        #endregion
    }
}
