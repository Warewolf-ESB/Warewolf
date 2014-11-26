
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Dev2.Runtime.Configuration.ComponentModel
{
    public class SettingsObject : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Impl

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if(PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region Fields

        bool _isSelected;

        #endregion

        #region Constructor

        /// <summary>
        /// Represents and object that contains settings, the view type to display it and the view model type to use as a backer for the view.
        /// To construct these objects please use the static BuildGraph() method.
        /// </summary>
        private SettingsObject(object dataContext, Type view, Type viewModel)
        {
            Object = dataContext;
            View = view;
            ViewModel = viewModel;

            Children = new List<SettingsObject>();
        }

        #endregion

        #region Properties

        public object Object { get; set; }
        public Type View { get; set; }
        public Type ViewModel { get; set; }
        public List<SettingsObject> Children { get; set; }

        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                _isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }

        #endregion

        #region Static Methods

        public static List<SettingsObject> BuildGraph(object attributedObject)
        {
            return BuildGraphImpl(attributedObject, new Stack<object>());
        }

        private static List<SettingsObject> BuildGraphImpl(object attributedObject, Stack<object> referenceStack)
        {
            List<SettingsObject> graph = new List<SettingsObject>();

            // If arributed object is null return empty graph
            if(attributedObject == null)
            {
                return graph;
            }

            // If a circular reference is detected then return empty graph
            if(referenceStack.Contains(attributedObject))
            {
                return graph;
            }

            // Push the current attributed part onto the reference stack
            referenceStack.Push(attributedObject);

            // Loop through properties on the attributed object
            foreach(var property in attributedObject.GetType().GetProperties())
            {
                // Try get value from property
                object value = null;
                try
                {
                    value = property.GetValue(attributedObject, null);
                }
                // ReSharper disable EmptyGeneralCatchClause
                catch(Exception)
                // ReSharper restore EmptyGeneralCatchClause
                {
#if DEBUG
                    //DIE execution DIE ... dude what are you doing?! Fix this otherwise the property won't show as an option in the settings treeview.
#else
                    // If there was a problem skip this property
                    continue;
#endif
                }

                // If value is null skip this property
                if(value == null)
                {
                    continue;
                }

                // If a circular reference is detected then skip this property
                if(referenceStack.Contains(value))
                {
                    continue;
                }

                // Check if the property is adorned with the SettingsObjectAtribute
                object[] attributes = property.GetCustomAttributes(typeof(SettingsObjectAttribute), true);
                if(attributes.Length > 0)
                {
                    SettingsObjectAttribute settingsObjectAttribute = attributes[0] as SettingsObjectAttribute;
                    // Add settings object to graph
                    if(settingsObjectAttribute != null)
                    {
                        graph.Add(new SettingsObject(value, settingsObjectAttribute.View, settingsObjectAttribute.ViewModel));
                    }
                }
            }

            // Find nested settings objects
            foreach(SettingsObject item in graph)
            {
                item.Children.AddRange(BuildGraphImpl(item.Object, referenceStack));
            }

            // Pop from the reference stack
            referenceStack.Pop();

            return graph;
        }

        #endregion
    }
}
