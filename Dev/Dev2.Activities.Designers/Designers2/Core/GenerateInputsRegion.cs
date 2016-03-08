﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ToolBase;
// ReSharper disable ClassWithVirtualMembersNeverInherited.Global

namespace Dev2.Activities.Designers2.Core
{
    public class GenerateInputsRegion : IGenerateInputArea
    {
        ICollection<IServiceInput> _inputs;

        public GenerateInputsRegion()
        {
            ToolRegionName = "GenerateInputsRegion";
            IsVisible = true;
        }

        #region Implementation of IToolRegion

        public string ToolRegionName { get; set; }
        public bool IsVisible { get; set; }
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
            }
        }

        #endregion

        #region Implementation of INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

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
