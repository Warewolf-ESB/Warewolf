#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
﻿using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Dev2.Activities.Designers2.Core.CloneInputRegion;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ToolBase;
using Dev2.Common.Interfaces.ToolBase.Database;
using Dev2.Studio.Core.Activities.Utils;
using Microsoft.Practices.Prism;
using Warewolf.Core;

namespace Dev2.Activities.Designers2.Core.InputRegion
{
    public sealed class DatabaseInputRegion : IDatabaseInputRegion
    {
        readonly IActionInputDatatalistMapper _datatalistMapper;
        readonly ModelItem _modelItem;
        readonly IActionToolRegion<IDbAction> _action;
        bool _isEnabled;
        ICollection<IServiceInput> _inputs;
        bool _isInputsEmptyRows;

        public DatabaseInputRegion() => ToolRegionName = "DatabaseInputRegion";

        public DatabaseInputRegion(ModelItem modelItem, IActionToolRegion<IDbAction> action)
            : this(new ActionInputDatalistMapper())
        {
            ToolRegionName = "DatabaseInputRegion";
            _modelItem = modelItem;
            _action = action;
            _action.SomethingChanged += SourceOnSomethingChanged;
            var inputsFromModel = _modelItem.GetProperty<ICollection<IServiceInput>>(nameof(Inputs));
            var serviceInputs = inputsFromModel ?? new List<IServiceInput>();
            var inputs = new ObservableCollection<IServiceInput>();
            inputs.CollectionChanged += InputsCollectionChanged;
            inputs.AddRange(serviceInputs);
            Inputs = inputs;
            if (inputsFromModel == null)
            {
                UpdateOnActionSelection();
            }

            IsEnabled = _action?.SelectedAction != null;
        }

        void InputsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            AddItemPropertyChangeEvent(e);
            RemoveItemPropertyChangeEvent(e);
        }

        public void ResetInputs(ICollection<IServiceInput> inputs)
        {
            var newInputs = new ObservableCollection<IServiceInput>();
            newInputs.CollectionChanged += InputsCollectionChanged;
            newInputs.AddRange(inputs);
            Inputs = newInputs;
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

        void ItemPropertyChanged(object sender, PropertyChangedEventArgs e) => _modelItem.SetProperty(nameof(Inputs), _inputs.ToList());

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

        public DatabaseInputRegion(IActionInputDatatalistMapper datatalistMapper) => _datatalistMapper = datatalistMapper;

        void SourceOnSomethingChanged(object sender, IToolRegion args)
        {
            try
            {
                Errors.Clear();
                UpdateOnActionSelection();
                OnPropertyChanged(nameof(Inputs));
                OnPropertyChanged(nameof(IsEnabled));
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

        void CallErrorsEventHandler() => ErrorsHandler?.Invoke(this, new List<string>(Errors));

        void UpdateOnActionSelection()
        {
            IsEnabled = _action?.SelectedAction != null;
            var inputCopy = Inputs.ToArray().Clone() as ICollection<IServiceInput>;
            Inputs = new List<IServiceInput>();
            if (_action?.SelectedAction != null)
            {
                var selectedActionInputs = _action.SelectedAction.Inputs;
                var selectedAction = ((DbAction)_action.SelectedAction).Name;
                var isTheSameActionWithPrevious = inputCopy.All(input => input.ActionName?.Equals(selectedAction) ?? false);
                if (inputCopy.Any() && isTheSameActionWithPrevious)
                {

                    var newInputs = InputsFromSameAction(selectedActionInputs);
                    var removedInputs = inputCopy.Except(selectedActionInputs, new ServiceInputNameComparer()).ToList();
                    var union = inputCopy.Union(newInputs, new ServiceInputNameComparer()).ToList();
                    union.RemoveAll(a => removedInputs.Any(k => a.Equals(k)));
                    ResetInputs(union);
                }
                else
                {
                    ResetInputs(selectedActionInputs);
                    _datatalistMapper.MapInputsToDatalist(Inputs);
                    IsInputsEmptyRows = Inputs.Count < 1;
                    IsEnabled = true;
                }
            }
            OnPropertyChanged(nameof(Inputs));
        }

        ICollection<IServiceInput> InputsFromSameAction(IList<IServiceInput> selectedActionInputs)
        {
            var newInputs = selectedActionInputs.Except(Inputs, new ServiceInputNameComparer());
            var serviceInputs = newInputs as IServiceInput[] ?? newInputs.ToArray();
            _datatalistMapper.MapInputsToDatalist(serviceInputs);
            return serviceInputs;
        }

        public bool IsInputsEmptyRows
        {
            get => _isInputsEmptyRows;
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
            get => _isEnabled;
            set
            {
                _isEnabled = value;
                OnPropertyChanged();
            }
        }

        public int? CommandTimeout
        {
            get => _modelItem.GetProperty<int?>("CommandTimeout");
            set
            {
                _modelItem.SetProperty<int?>("CommandTimeout", value);
                OnPropertyChanged();
            }
        }

        public IList<IToolRegion> Dependants { get; set; }

        public IToolRegion CloneRegion()
        {
            var inputs2 = new List<IServiceInput>(Inputs);
            return new DatabaseInputRegionClone
            {
                Inputs = inputs2,
                IsEnabled = IsEnabled
            } as IToolRegion;
        }

        public void RestoreRegion(IToolRegion toRestore)
        {
            if (toRestore is DatabaseInputRegionClone region)
            {
                Inputs.Clear();
                if (region.Inputs != null)
                {
                    Inputs = region.Inputs.ToList();
                }
                OnPropertyChanged(nameof(Inputs));
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
            set => ErrorsHandler.Invoke(this, new List<string>(value));
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region Implementation of IDatabaseInputRegion

        public ICollection<IServiceInput> Inputs
        {
            get => _inputs;
            set
            {
                if (value != null)
                {
                    _inputs = value;
                    _modelItem.SetProperty(nameof(Inputs), value.ToList());
                    OnPropertyChanged();
                }
                else
                {
                    _inputs.Clear();
                    _modelItem.SetProperty(nameof(Inputs), _inputs.ToList());
                    OnPropertyChanged();
                }

            }
        }

        #endregion
    }
}
