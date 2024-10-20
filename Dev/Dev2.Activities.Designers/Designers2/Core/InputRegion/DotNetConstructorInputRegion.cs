﻿/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
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
using Dev2.Common.Interfaces.ToolBase.DotNet;
using Dev2.Common.Utils;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Studio.Core.Activities.Utils;
using Microsoft.Practices.Prism;
using Warewolf.Core;

namespace Dev2.Activities.Designers2.Core.InputRegion
{
    public class DotNetConstructorInputRegion : IDotNetConstructorInputRegion
    {
        readonly ModelItem _modelItem;
        readonly IConstructorRegion<IPluginConstructor> _action;
        bool _isEnabled;
        ICollection<IServiceInput> _inputs;
        bool _isInputsEmptyRows;
        readonly IActionInputDatatalistMapper _datatalistMapper;
        RelayCommand _viewObjectResult;

        public DotNetConstructorInputRegion()
        {
            ToolRegionName = "DotNetConstructorInputRegion";
        }

        public DotNetConstructorInputRegion(ModelItem modelItem, IConstructorRegion<IPluginConstructor> action)
            : this(new ActionInputDatalistMapper())
        {
            ToolRegionName = "DotNetConstructorInputRegion";
            _modelItem = modelItem;
            _action = action;
            _action.SomethingChanged += SourceOnSomethingChanged;
            var inputsFromModel = _modelItem.GetProperty<List<IServiceInput>>("ConstructorInputs");
            var serviceInputs = inputsFromModel ?? new List<IServiceInput>();
            Inputs = new ObservableCollection<IServiceInput>();
            var inputs = new ObservableCollection<IServiceInput>();
            inputs.CollectionChanged += InputsCollectionChanged;
            inputs.AddRange(serviceInputs);
            Inputs = inputs;
            IsInputsEmptyRows = Inputs.Count == 0;
            if (inputsFromModel == null)
            {
                UpdateOnActionSelection();
            }

            IsEnabled = action?.SelectedConstructor != null;
        }

        
        public DotNetConstructorInputRegion(IActionInputDatatalistMapper datatalistMapper)
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
            _modelItem.SetProperty("ConstructorInputs", Inputs.ToList());
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
            if (_action?.SelectedConstructor != null)
            {
                Inputs = _action.SelectedConstructor.Inputs.Select(parameter => new ServiceInput(parameter.Name, parameter.Value)
                {
                    EmptyIsNull = parameter.EmptyToNull,
                    RequiredField = parameter.IsRequired,
                    TypeName = parameter.TypeName,
                    IntellisenseFilter = parameter.IsObject ? enIntellisensePartType.JsonObject : enIntellisensePartType.None,
                    IsObject = parameter.IsObject,
                    Dev2ReturnType = parameter.Dev2ReturnType,
                    ShortTypeName = parameter.ShortTypeName,
                } as IServiceInput).ToList();
                _datatalistMapper.MapInputsToDatalist(Inputs);
                IsInputsEmptyRows = Inputs.Count < 1;
                IsEnabled = true;
            }
            OnPropertyChanged("Inputs");
        }
        public IJsonObjectsView JsonObjectsView => CustomContainer.GetInstancePerRequestType<IJsonObjectsView>();

        public RelayCommand ViewObjectResult => _viewObjectResult ?? (_viewObjectResult = new RelayCommand(item =>
                                                              {
                                                                  var serviceInput = item as IServiceInput;
                                                                  ViewJsonObjects(serviceInput);
                                                              }, o => true));

        void ViewJsonObjects(IServiceInput input)
        {
            JsonObjectsView?.ShowJsonString(JsonUtils.Format(JsonUtils.Format(input.Dev2ReturnType)));
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
            var inputs2 = Inputs.ToArray().Clone() as List<IServiceInput>;

            return new DotNetConstructorInputRegionClone
            {
                Inputs = inputs2,
                IsEnabled = IsEnabled
            } as IToolRegion;
        }

        public void RestoreRegion(IToolRegion toRestore)
        {
            if (toRestore is DotNetConstructorInputRegionClone region)
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
                    _modelItem.SetProperty("ConstructorInputs", value.ToList());
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