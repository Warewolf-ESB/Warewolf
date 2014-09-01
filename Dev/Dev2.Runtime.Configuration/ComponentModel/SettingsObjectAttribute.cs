using System;
using System.Windows.Controls;
using Dev2.Runtime.Configuration.ViewModels;

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
                throw new Exception("View type must inherit from UserControl.");
            }

            if (view.GetConstructor(Type.EmptyTypes) == null)
            {
                throw new Exception("View type must contain a parameterless contructor.");
            }

            if (!CheckInheretenceHierarchy(viewModel, typeof(SettingsViewModelBase)))
            {
                throw new Exception("ViewModel type must inherit from SettingsViewModelBase.");
            }

            if (viewModel.GetConstructor(Type.EmptyTypes) == null)
            {
                throw new Exception("ViewModel type must contain a parameterless contructor.");
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
