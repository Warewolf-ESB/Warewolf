using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Dev2.Common;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ToolBase;
// ReSharper disable ClassWithVirtualMembersNeverInherited.Global

namespace Dev2.Activities.Designers2.Core
{
    public class GenerateInputsRegion : IGenerateInputArea
    {
        private double _minHeight;
        private double _currentHeight;
        private double _maxHeight;
        double _inputsHeight;
        double _maxInputsHeight;
        ICollection<IServiceInput> _inputs;
        private const double BaseHeight = 90;
        private const double ListBoxHeight = 20;

        public GenerateInputsRegion()
        {
            ToolRegionName = "GenerateInputsRegion";
            IsVisible = true;
        }

        private void SetInitialHeight()
        {
            MinHeight = BaseHeight;
            MaxHeight = BaseHeight;
            CurrentHeight = BaseHeight;
            MaxInputsHeight = BaseHeight;
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
        public bool IsVisible { get; set; }
        public bool IsInputCountEmpty { get; set; }
        public double MaxHeight
        {
            get
            {
                return _maxHeight;
            }
            set
            {
                if (Math.Abs(_maxHeight - value) > GlobalConstants.DesignHeightTolerance)
                {
                    _maxHeight = value;

                    OnPropertyChanged();
                    OnHeightChanged(this);
                }
            }
        }
        public event HeightChanged HeightChanged;
        public IList<IToolRegion> Dependants { get; set; }
        public IList<string> Errors
        {
            get
            {
                IList<string> errors = new List<string>();
                return errors;
            }
        }

        public IToolRegion CloneRegion()
        {
            return null;
        }

        public void RestoreRegion(IToolRegion toRestore)
        {
        }

        void ResetInputsHeight()
        {
            SetInitialHeight();
            InputsHeight = GlobalConstants.RowHeaderHeight + Inputs.Count * GlobalConstants.RowHeight;
            MaxInputsHeight = InputsHeight;
            if (Inputs.Count == 3)
            {
                MinHeight = BaseHeight;
                MaxHeight = (Inputs.Count * GlobalConstants.RowHeight) + ListBoxHeight;
                InputsHeight = MinHeight;
                MaxInputsHeight = (Inputs.Count * GlobalConstants.RowHeight) + ListBoxHeight;
                CurrentHeight = MinHeight;
            }
            else if (Inputs.Count > 3)
            {
                MinHeight = BaseHeight;
                MaxHeight = (Inputs.Count * GlobalConstants.RowHeight) + 15;
                InputsHeight = MinHeight;
                MaxInputsHeight = (Inputs.Count * GlobalConstants.RowHeight) + 15;
                CurrentHeight = MinHeight;
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
            }
        }

        #endregion

        #region Implementation of IGenerateInputArea

        public ICollection<IServiceInput> Inputs
        {
            get
            {
                return _inputs;
            }
            set
            {
                _inputs = value;
                OnPropertyChanged();
                ResetInputsHeight();
            }
        }
        public double InputsHeight
        {
            get
            {
                return _inputsHeight;
            }
            set
            {
                _inputsHeight = value;
                OnPropertyChanged();
            }
        }
        public double MaxInputsHeight
        {
            get
            {
                return _maxInputsHeight;
            }
            set
            {
                _maxInputsHeight = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Implementation of INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnHeightChanged(IToolRegion args)
        {
            var handler = HeightChanged;
            if (handler != null)
            {
                handler(this, args);
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
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
