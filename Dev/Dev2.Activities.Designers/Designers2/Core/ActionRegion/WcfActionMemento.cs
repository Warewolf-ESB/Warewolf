#pragma warning disable
﻿using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.ToolBase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;

namespace Dev2.Activities.Designers2.Core.ActionRegion
{
    public class WcfActionMemento : IActionToolRegion<IWcfAction>
    {
        IWcfAction _selectedAction;

        #region Implementation of INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Implementation of IToolRegion

        public string ToolRegionName { get; set; }
        public bool IsEnabled { get; set; }
        public IList<IToolRegion> Dependants { get; set; }
        public IList<string> Errors { get; set; }
        public IToolRegion CloneRegion() => null;
        public void RestoreRegion(IToolRegion toRestore) => throw new NotImplementedException();

        public EventHandler<List<string>> ErrorsHandler
        {
            get;
            set;
        }

        #endregion

        #region Implementation of IActionToolRegion<IDbAction>

        public IWcfAction SelectedAction
        {
            get => _selectedAction;
            set => _selectedAction = value;
        }
        public ICollection<IWcfAction> Actions { get; set; }
        public ICommand RefreshActionsCommand { get; set; }
        public bool IsActionEnabled { get; set; }
        public bool IsRefreshing { get; set; }
        public event SomethingChanged SomethingChanged;
        public double LabelWidth { get; set; }
        ICommand IActionToolRegion<IWcfAction>.RefreshActionsCommand => throw new NotImplementedException();

        #endregion
    }
}
