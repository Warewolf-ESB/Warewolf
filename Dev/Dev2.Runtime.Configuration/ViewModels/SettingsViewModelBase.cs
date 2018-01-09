/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Caliburn.Micro;

namespace Dev2.Runtime.Configuration.ViewModels
{
    public abstract class SettingsViewModelBase : Screen
    {
        #region Fields

        object _object;

        #endregion

        #region events

        public delegate void UnderlyingObjectChangedHandler();
        public event UnderlyingObjectChangedHandler UnderlyingObjectChanged;

        protected void OnUnderlyingObjectChanged()
        {
            UnderlyingObjectChanged?.Invoke();
        }

        #endregion events

        #region Properties

        public object Object
        {
            get
            {
                return _object;
            }
            set
            {
                _object = value;
                NotifyOfPropertyChange(() => Object);
                OnUnderlyingObjectChanged();
            }
        }

        #endregion

    }
}
