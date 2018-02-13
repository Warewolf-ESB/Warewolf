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
    public abstract class ConflictRow : BindableBase, IConflictRow, IConflictCheckable
    {
        public abstract bool IsEmptyItemSelected { get; set; }
        public bool HasConflict => !Current.Equals(Different);
        public abstract bool IsChecked { get; set; }
        readonly private Guid _uniqueId = Guid.NewGuid();
        public abstract IConflictItem Current { get; }
        public abstract IConflictItem Different { get; }
        public abstract bool IsStartNode { get; set; }

        protected ConflictRow()
        {
        }

        public bool IsCurrentChecked
        {
            get => Current.IsChecked;
            set => Current.IsChecked = value;
        }

        public Guid UniqueId => _uniqueId;
    }
}
