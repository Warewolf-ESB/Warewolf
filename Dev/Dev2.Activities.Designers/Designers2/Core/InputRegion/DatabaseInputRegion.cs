using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Dev2.Activities.Designers2.Core.CloneInputRegion;
using Dev2.Common;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ToolBase;
using Dev2.Common.Interfaces.ToolBase.Database;
using Dev2.Studio.Core.Activities.Utils;
// ReSharper disable NotAccessedField.Local

// ReSharper disable ClassWithVirtualMembersNeverInherited.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ExplicitCallerInfoArgument
// ReSharper disable UnusedMember.Global

namespace Dev2.Activities.Designers2.Core.InputRegion
{
    public class DatabaseInputRegion : IDatabaseInputRegion
    {
        private readonly ModelItem _modelItem;
        private readonly IActionToolRegion<IDbAction> _action;
        private double _minHeight;
        private double _currentHeight;
        private double _maxHeight;
        private double _headersHeight;
        double _maxHeadersHeight;
        bool _isVisible;
        private IList<IServiceInput> _inputs;
        private bool _isInputsEmptyRows;
        private const double BaseHeight = 60;

        public DatabaseInputRegion()
        {
            ToolRegionName = "DatabaseInputRegion";
            SetInitialHeight();
        }

        private void SetInitialHeight()
        {
            MinHeight = BaseHeight;
            MaxHeight = BaseHeight;
            CurrentHeight = BaseHeight;
            MaxHeadersHeight = BaseHeight;
        }

        void ResetInputsHeight()
        {
            SetInitialHeight();
            HeadersHeight = GlobalConstants.RowHeaderHeight + Inputs.Count * GlobalConstants.RowHeight;
            MaxHeadersHeight = HeadersHeight;
            if (Inputs.Count >= 3)
            {
                MinHeight = 115;
                MaxHeight = GlobalConstants.RowHeaderHeight + Inputs.Count * GlobalConstants.RowHeight;
                MaxHeadersHeight = 115;
                CurrentHeight = MinHeight;
                OnPropertyChanged("Inputs");
            }
            else
            {
                CurrentHeight = GlobalConstants.RowHeaderHeight + Inputs.Count * GlobalConstants.RowHeight;
                if (CurrentHeight < BaseHeight)
                {
                    CurrentHeight = BaseHeight;
                }
                MinHeight = CurrentHeight;
                MaxHeight = CurrentHeight;
                OnHeightChanged(this);
                OnPropertyChanged("Inputs");
            }
        }

        public DatabaseInputRegion(ModelItem modelItem, IActionToolRegion<IDbAction> action)
        {
            ToolRegionName = "DatabaseInputRegion";
            _modelItem = modelItem;
            _action = action;
            _action.SomethingChanged += SourceOnSomethingChanged;
            SetInitialHeight();
            var inputsFromModel = _modelItem.GetProperty<List<IServiceInput>>("Inputs");
            Inputs = new List<IServiceInput>(inputsFromModel??new List<IServiceInput>());
            if(inputsFromModel == null)
                UpdateOnActionSelection();
            IsVisible = _action != null && _action.SelectedAction != null;
        }

        private void SourceOnSomethingChanged(object sender, IToolRegion args)
        {
            // ReSharper disable once ExplicitCallerInfoArgument
            UpdateOnActionSelection();
            // ReSharper disable once ExplicitCallerInfoArgument
            OnPropertyChanged(@"Inputs");
            OnPropertyChanged(@"IsVisible");
            OnHeightChanged(this);
        }

        private void UpdateOnActionSelection()
        {
            Inputs.Clear();
            IsVisible = false;
            if(_action != null && _action.SelectedAction != null)
            {
            
                Inputs = _action.SelectedAction.Inputs;
                IsInputsEmptyRows = Inputs.Count < 1;
                IsVisible = true;
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

        #region Implementation of IDatabaseInputArea

        public double HeadersHeight
        {
            get
            {
                return _headersHeight;
            }
            set
            {
                _headersHeight = value;
                OnPropertyChanged();
            }
        }

        #endregion

        public double MaxHeadersHeight
        {
            get
            {
                return _maxHeadersHeight;
            }
            set
            {
                _maxHeadersHeight = value;
                OnPropertyChanged();
            }
        }

        #region Implementation of IToolRegion

        public string ToolRegionName { get; set; }
        public double MinHeight
        {
            get
            {
                return _minHeight;
            }
            set
            {
                _minHeight = value;
                OnPropertyChanged();
            }
        }
        public double CurrentHeight
        {
            get
            {
                return _currentHeight;
            }
            set
            {
                _currentHeight = value;
                OnPropertyChanged();
            }
        }
        public bool IsVisible
        {
            get
            {
                return _isVisible;
            }
            set
            {
                _isVisible = value;
                OnPropertyChanged();
            }
        }
        public double MaxHeight
        {
            get
            {
                return _maxHeight;
            }
            set
            {
                _maxHeight = value;

                OnPropertyChanged();
                OnHeightChanged(this);
            }
        }

        public IList<IToolRegion> Dependants { get; set; }

        public IToolRegion CloneRegion()
        {
            //var ser = new Dev2JsonSerializer();
            //return ser.Deserialize<IToolRegion>(ser.SerializeToBuilder(this));
            var inputs2 = new List<IServiceInput>(Inputs);
            return new DatabaseInputRegionClone
            {
                Inputs = inputs2,
                IsVisible = IsVisible
            };
        }

        public void RestoreRegion(IToolRegion toRestore)
        {
            var region = toRestore as DatabaseInputRegionClone;
            if (region != null)
            {
                IsVisible = region.IsVisible;
                Inputs.Clear();
                if (region.Inputs != null)
                {
                    foreach(var serviceInput in region.Inputs)
                    {
                        Inputs.Add(serviceInput);
                    }
                   
                }
                OnPropertyChanged("Inputs");
                ResetInputsHeight();
                IsInputsEmptyRows = Inputs == null ||Inputs.Count == 0;
            }
        }

        public IList<string> Errors
        {
            get
            {
                IList<string> errors = new List<string>();
                return errors;
            }
        }

        public event HeightChanged HeightChanged;

        #endregion

        protected virtual void OnHeightChanged(IToolRegion args)
        {
            var handler = HeightChanged;
            if (handler != null)
            {
                handler(this, args);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
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
                _modelItem.SetProperty("Inputs",value);
                ResetInputsHeight();
            }
        }

        #endregion
    }
}
