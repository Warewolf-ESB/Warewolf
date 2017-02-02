/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Dev2
{
    public abstract class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public Common.Interfaces.Core.Graph.IPath Path { get; set; }

        protected bool OnPropertyChanged<T>(ref T propertyValue, T newValue, [CallerMemberName] string propertyName = null)
        {
            var propertyChanged = !EqualityComparer<T>.Default.Equals(propertyValue, newValue);
            if(propertyChanged)
            {
                propertyValue = newValue;
                OnPropertyChanged(propertyName);
            }
            return propertyChanged;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
