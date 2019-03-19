#pragma warning disable
﻿// 
// /*
// *  Warewolf - Once bitten, there's no going back
// *  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
// *  Licensed under GNU Affero General Public License 3.0 or later. 
// *  Some rights reserved.
// *  Visit our website for more information <http://warewolf.io/>
// *  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
// *  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
// */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ToolBase;

namespace Dev2.Activities.Designers2.Core.ActionRegion
{
    public class DbActionMemento : IActionToolRegion<IDbAction>
    {
        IDbAction _selectedAction;
        EventHandler<List<string>> _errorsHandler;
        public event PropertyChangedEventHandler PropertyChanged;
        public string ToolRegionName { get; set; }
        public bool IsEnabled { get; set; }
        public IList<IToolRegion> Dependants { get; set; }
        public IList<string> Errors { get; set; }
        public IToolRegion CloneRegion() => null;
        public void RestoreRegion(IToolRegion toRestore) => throw new NotImplementedException();

        public EventHandler<List<string>> ErrorsHandler
        {
            get => null;
            set => _errorsHandler = value;
        }

        #region Implementation of IActionToolRegion<IDbAction>

        public IDbAction SelectedAction
        {
            get
            {
                return _selectedAction;
            }
            set
            {
                _selectedAction = value;
            }
        }
        public ICollection<IDbAction> Actions { get; set; }
        public ICommand RefreshActionsCommand { get; set; }
        public bool IsActionEnabled { get; set; }
        public bool IsRefreshing { get; set; }
        public event SomethingChanged SomethingChanged;
        public double LabelWidth { get; set; }

        #endregion
    }
}