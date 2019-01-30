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
        bool _isChecked;

        public abstract override bool Equals(object obj);
        public abstract override int GetHashCode();
        public abstract bool AllowSelection { get; set; }

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

        public bool IsChecked
        {
            get => _isChecked;
            set => SetProperty(ref _isChecked, value);
        }
        public event Action<IConflictItem, bool> NotifyIsCheckedChanged;

        public class Empty : ConflictItem
        {
            readonly int HashCode;

            public Empty()
            {
                HashCode = new Random(178697).Next();
            }

            public override bool AllowSelection { get; set; }

            public override bool Equals(object obj) => GetHashCode() == obj.GetHashCode();

            public override int GetHashCode() => HashCode;
        }
    }
}
