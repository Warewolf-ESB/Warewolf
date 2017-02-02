/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Windows.Controls;
using Dev2.Runtime.Configuration.ViewModels;
using Warewolf.Resource.Errors;

namespace Dev2.Runtime.Configuration.ComponentModel
{
    /// <summary>
    /// Used to indicate that a property is a setting object
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class SettingsObjectAttribute : Attribute
    {
        public SettingsObjectAttribute(Type view, Type viewModel)
        {
            if (!CheckInheretenceHierarchy(view, typeof(UserControl)))
            {
                throw new Exception(ErrorResource.ViewTypeMustInheritFromUserControl);
            }

            if (view.GetConstructor(Type.EmptyTypes) == null)
            {
                throw new Exception(ErrorResource.ViewTypeMustContainAParameterlessContructor);
            }

            if (!CheckInheretenceHierarchy(viewModel, typeof(SettingsViewModelBase)))
            {
                throw new Exception(ErrorResource.ViewModelTypeMustInheritFromSettingsViewModelBase);
            }

            if (viewModel.GetConstructor(Type.EmptyTypes) == null)
            {
                throw new Exception(ErrorResource.ViewModelTypeMustContainAParameterlessContructor);
            }
            
            View = view;
            ViewModel = viewModel;
        }

        public Type View { get; set; }
        public Type ViewModel { get; set; }

        private bool CheckInheretenceHierarchy(Type type, Type requiredBase)
        {
            if (type == null)
            {
                return false;
            }

            if (type == requiredBase || type.BaseType == requiredBase)
            {
                return true;
            }

            return CheckInheretenceHierarchy(type.BaseType, requiredBase);
        }
    }
}
