using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ToolBase;

namespace Dev2.Activities.Designers2.Core
{
    public class GenerateOutputsRegion : IGenerateOutputArea
    {
        ICollection<IServiceOutputMapping> _outputs;
        private bool _textResults;

        public GenerateOutputsRegion()
        {
            ToolRegionName = "GenerateOutputsRegion";
            IsEnabled = false;
        }

        #region Implementation of IToolRegion

        public string ToolRegionName { get; set; }
        public bool IsEnabled { get; set; }
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

        public EventHandler<List<string>> ErrorsHandler
        {
            get;
            set;
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
                OnPropertyChanged();
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

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
