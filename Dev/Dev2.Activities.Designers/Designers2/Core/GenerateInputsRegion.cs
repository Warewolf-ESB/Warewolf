using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ToolBase;
using Dev2.Common;
using System.Linq;
using System.Collections;


namespace Dev2.Activities.Designers2.Core
{
    public class GenerateInputsRegion : IGenerateInputArea
    {
        ICollection<IServiceInput> _inputs;

        public GenerateInputsRegion()
        {
            ToolRegionName = "GenerateInputsRegion";
            IsEnabled = true;
        }


        public string ToolRegionName { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsInputCountEmpty { get; set; }
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

        public ICollection<IServiceInput> Inputs
        {
            get
            {
                return _inputs;
            }
            set
            {

                var distinct = value.Distinct().ToList();                
                _inputs = distinct;
                OnPropertyChanged();
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
