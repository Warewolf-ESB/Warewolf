using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Dev2.Common;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ToolBase;

namespace Dev2.Activities.Designers2.Core
{
    public class GenerateOutputsRegion : IGenerateOutputArea
    {
        private double _minHeight;
        private double _currentHeight;
        private double _maxHeight;
        double _outputsHeight;
        double _maxOutputsHeight;
        ICollection<IServiceOutputMapping> _outputs;
        bool _isVisible;
        private int _outputRowCount;
        private bool _textResults;

        private const double BaseHeight = 60;

        public GenerateOutputsRegion()
        {
            ToolRegionName = "GenerateOutputsRegion";
            IsVisible = false;
        }

        private void SetInitialHeight()
        {
            MinHeight = BaseHeight;
            MaxHeight = BaseHeight;
            CurrentHeight = BaseHeight;
            MaxOutputsHeight = BaseHeight;
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

        void ResetOutputsHeight()
        {
            SetInitialHeight();
            OutputsHeight = GlobalConstants.RowHeaderHeight + OutputRowCount * GlobalConstants.RowHeight;
            MaxOutputsHeight = OutputsHeight;
            if (OutputRowCount >= 3)
            {
                MinHeight = 3 * GlobalConstants.RowHeight;
                MaxHeight = (OutputRowCount * GlobalConstants.RowHeight) + 15;
                OutputsHeight = MinHeight;
                CurrentHeight = MinHeight;
            }
            else
            {
                CurrentHeight = GlobalConstants.RowHeaderHeight + OutputRowCount * GlobalConstants.RowHeight;
                if (CurrentHeight < BaseHeight)
                {
                    CurrentHeight = BaseHeight;
                }
                MinHeight = CurrentHeight;
                MaxHeight = CurrentHeight + 15;
            }
            OnHeightChanged(this);
        }

        #endregion

        #region Implementation of IGenerateOutputArea

        public ICollection<IServiceOutputMapping> Outputs
        {
            get
            {
                return _outputs;
            }
            set
            {
                _outputs = value;
                ResetOutputsHeight();
            }
        }
        public double OutputsHeight
        {
            get
            {
                return _outputsHeight;
            }
            set
            {
                _outputsHeight = value;
                OnPropertyChanged();
            }
        }
        public double MaxOutputsHeight
        {
            get
            {
                return _maxOutputsHeight;
            }
            set
            {
                _maxOutputsHeight = value;
                OnPropertyChanged();
            }
        }
        public int OutputRowCount
        {
            get
            {
                return _outputRowCount;
            }
            set
            {
                _outputRowCount = value;
                ResetOutputsHeight();
            }
        }
        public bool TextResults
        {
            get
            {
                return _textResults;
            }
            set
            {
                _textResults = value;
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
