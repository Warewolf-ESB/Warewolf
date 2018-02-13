/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces;
using Microsoft.Practices.Prism.Mvvm;
using System;

namespace Dev2.ViewModels.Merge
{
    public abstract class ConflictItem : BindableBase , IConflictItem
    {
        public abstract override bool Equals(object obj);
        public abstract override int GetHashCode();

        public void SetAutoChecked()
        {
            IsChecked = true;
            AutoChecked = true;
        }
        public bool AutoChecked { get; set; }

        protected ConflictItem()
        {
            RegisterEventHandlers();
        }

        private void RegisterEventHandlers()
        {
            PropertyChanged += (sender, eventArg) => {
                if (eventArg.PropertyName == nameof(IsChecked))
                {
                    NotifyIsCheckedChanged?.Invoke(this, IsChecked);
                }
            };
        }

        public abstract bool IsChecked { get; set; }
        public event Action<IConflictItem, bool> NotifyIsCheckedChanged;
    }
}
