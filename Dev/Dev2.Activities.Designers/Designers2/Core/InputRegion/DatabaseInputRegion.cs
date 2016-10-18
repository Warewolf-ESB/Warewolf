using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Dev2.Activities.Designers2.Core.CloneInputRegion;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ToolBase;
using Dev2.Common.Interfaces.ToolBase.Database;
using Dev2.Data.Util;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.Models.DataList;

// ReSharper disable ExplicitCallerInfoArgument

namespace Dev2.Activities.Designers2.Core.InputRegion
{
    public sealed class DatabaseInputRegion : IDatabaseInputRegion
    {
        private readonly ModelItem _modelItem;
        private readonly IActionToolRegion<IDbAction> _action;
        bool _isEnabled;
        // ReSharper disable once NotAccessedField.Local
        private IList<IServiceInput> _inputs;
        private bool _isInputsEmptyRows;

        public DatabaseInputRegion()
        {
            ToolRegionName = "DatabaseInputRegion";
        }

        public DatabaseInputRegion(ModelItem modelItem, IActionToolRegion<IDbAction> action)
        {
            ToolRegionName = "DatabaseInputRegion";
            _modelItem = modelItem;
            _action = action;
            _action.SomethingChanged += SourceOnSomethingChanged;
            var inputsFromModel = _modelItem.GetProperty<List<IServiceInput>>("Inputs");
            Inputs = new List<IServiceInput>(inputsFromModel ?? new List<IServiceInput>());
            if (inputsFromModel == null)
                UpdateOnActionSelection();
            IsEnabled = _action?.SelectedAction != null;
        }

        private void SourceOnSomethingChanged(object sender, IToolRegion args)
        {
            try
            {
                Errors.Clear();

                // ReSharper disable once ExplicitCallerInfoArgument
                UpdateOnActionSelection();
                // ReSharper disable once ExplicitCallerInfoArgument
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

        private void CallErrorsEventHandler()
        {
            ErrorsHandler?.Invoke(this, new List<string>(Errors));
        }

        private void UpdateOnActionSelection()
        {
            Inputs = new List<IServiceInput>();
            IsEnabled = false;
            if (_action?.SelectedAction != null)
            {
                Inputs = _action.SelectedAction.Inputs;
                UpdateActiveDatalistWithInputs();
                IsInputsEmptyRows = Inputs.Count < 1;
                IsEnabled = true;
            }
            OnPropertyChanged("Inputs");
        }

        private void UpdateActiveDatalistWithInputs()
        {
            if (Inputs != null)
            {
                foreach (var serviceInput in Inputs)
                {

                    if (DataListSingleton.ActiveDataList != null)
                    {
                        if (DataListSingleton.ActiveDataList.ScalarCollection != null)
                        {
                            var alreadyExists = DataListSingleton.ActiveDataList.ScalarCollection.Count(model => model.Name.Equals(serviceInput.Name, StringComparison.InvariantCulture));
                            if (alreadyExists < 1)
                            {
                                var variable = DataListUtil.AddBracketsToValueIfNotExist(serviceInput.Name);
                                serviceInput.Value = variable;
                            }
                            else
                            {
                                var variable = DataListUtil.AddBracketsToValueIfNotExist(serviceInput.Name);
                                serviceInput.Value = variable;
                            }
                        }
                    }

               
                }
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
            return new DatabaseInputRegionClone
            {
                Inputs = inputs2,
                IsEnabled = IsEnabled
            };
        }

        public void RestoreRegion(IToolRegion toRestore)
        {
            var region = toRestore as DatabaseInputRegionClone;
            if (region != null)
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
            set
            {
                ErrorsHandler.Invoke(this, new List<string>(value));
            }
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region Implementation of IDatabaseInputRegion

        public IList<IServiceInput> Inputs
        {
            get
            {
                return _modelItem.GetProperty<List<IServiceInput>>("Inputs") ?? new List<IServiceInput>();
            }
            set
            {
                _inputs = value;
                OnPropertyChanged();
                _modelItem.SetProperty("Inputs", value);
                OnPropertyChanged();
            }
        }

        #endregion
    }
}
