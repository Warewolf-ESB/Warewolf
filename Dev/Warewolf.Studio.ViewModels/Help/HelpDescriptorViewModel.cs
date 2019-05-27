#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Windows.Media;
using Dev2.Common.Interfaces.Help;
using Microsoft.Practices.Prism.Mvvm;

namespace Warewolf.Studio.ViewModels.Help
{
    public class HelpDescriptorViewModel : BindableBase, IHelpDescriptorViewModel
    {
        readonly IHelpDescriptor _descriptor;
        bool _isEnabled;

        public HelpDescriptorViewModel(IHelpDescriptor descriptor)
        {
            if (descriptor == null)
            {
                throw new ArgumentNullException("descriptor");
            }
            _descriptor = descriptor;
        }

        #region Implementation of IHelpDescriptorViewModel

        /// <summary>
        /// Display name
        /// </summary>
        public string Name => _descriptor.Name;

        /// <summary>
        /// The help text
        /// </summary>
        public string Description => _descriptor.Description;

        /// <summary>
        /// The icon to display
        /// </summary>
        public object Icon => _descriptor.Icon as DrawingImage;

        /// <summary>
        /// Is this help enabled.
        /// </summary>
        public bool IsEnabled
        {
            get
            {
                return _isEnabled;
            }
            set
            {
                OnPropertyChanged("IsEnabled");
                _isEnabled = value;
            }
        }

        #endregion
    }
}
