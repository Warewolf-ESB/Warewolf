
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
using System.Runtime.InteropServices;
using System.Windows;
using Dev2.Interfaces;
using Dev2.Studio.Core.Interfaces;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core
{
    [ComVisible(true)]
    public class WebPropertyEditorScriptableClass : IPropertyEditorWizard
    {
        public IPropertyEditorWizard PropertyEditorViewModel { get; set; }

        #region Owner

        public Window Owner
        {
            get
            {
                if(PropertyEditorViewModel != null)
                {
                    return PropertyEditorViewModel.Owner;
                }
                return null;
            }
            set
            {
                if(PropertyEditorViewModel != null)
                {
                    PropertyEditorViewModel.Owner = value;
                }
            }
        }

        #endregion

        public void CloseWizard()
        {
            Close();
        }

        public void Save(string value, bool closeBrowserWindow = true)
        {
            if(PropertyEditorViewModel != null)
            {
                InvokeActionAsync(() => PropertyEditorViewModel.Save(value, closeBrowserWindow));
            }
        }

        public void OpenPropertyEditor()
        {
            if(PropertyEditorViewModel != null)
            {
                InvokeAction(() => PropertyEditorViewModel.OpenPropertyEditor());
            }
        }

        public void Dev2Set(string value1, string value2)
        {
            if(PropertyEditorViewModel != null)
            {

                InvokeAction(() => PropertyEditorViewModel.Dev2Set(value1, value2));
            }
        }

        // Sashen : needed to update the interface IPropertyEditorWizard as this
        // was causing unit tests to fail due to using the interface implementation 
        // this was returned by the factory.
        // Brendon 2012-09-10 : Renamed from Dev2Set to Dev2SetValue because having 2 
        // methods with the same name was causing issues when invoking them from javascript.
        public void Dev2SetValue(string value)
        {
            if(PropertyEditorViewModel != null)
            {
                InvokeAction(() => PropertyEditorViewModel.Dev2SetValue(value));
            }
        }

        public void Dev2Done()
        {
            if(PropertyEditorViewModel != null)
            {
                InvokeAction(() => PropertyEditorViewModel.Dev2Done());
            }
        }

        public void Dev2ReloadResource(Guid resourceID, string resourceType)
        {
            if(PropertyEditorViewModel != null)
            {
                InvokeAction(() => PropertyEditorViewModel.Dev2ReloadResource(resourceID, resourceType));
            }
        }

        public void Close()
        {
            if(PropertyEditorViewModel != null)
            {
                InvokeAction(() => PropertyEditorViewModel.Close());
            }
        }


        public void Cancel()
        {
            if(PropertyEditorViewModel != null)
            {
                InvokeAction(() => PropertyEditorViewModel.Cancel());
            }
        }

        public string FetchData(string args)
        {
            if(PropertyEditorViewModel != null)
            {
                return (string)InvokeFunction(() => PropertyEditorViewModel.FetchData(args));
            }

            return string.Empty;
        }

        public string GetIntellisenseResults(string searchTerm, int caretPosition)
        {
            if(PropertyEditorViewModel != null)
            {
                return (string)InvokeFunction(() => PropertyEditorViewModel.GetIntellisenseResults(searchTerm, caretPosition));
            }
            return string.Empty;
        }

        public ILayoutObjectViewModel SelectedLayoutObject
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }


        public event NavigateRequestedEventHandler NavigateRequested;

        protected void OnNavigateRequested(string uri)
        {
            if(NavigateRequested != null)
            {
                NavigateRequested(uri);
            }
        }

        static void InvokeAction(Action action)
        {
            if(action != null)
            {
                if(!Application.Current.Dispatcher.CheckAccess())
                {
                    Application.Current.Dispatcher.Invoke(action);
                }
                else
                {
                    action();
                }
            }
        }


        static void InvokeActionAsync(Action action)
        {
            if(action != null)
            {
                if(!Application.Current.Dispatcher.CheckAccess())
                {
                    Application.Current.Dispatcher.BeginInvoke(action);
                }
                else
                {
                    action();
                }
            }
        }

        static object InvokeFunction(Func<string> action)
        {
            if(action != null)
            {
                return !Application.Current.Dispatcher.CheckAccess() ? Application.Current.Dispatcher.Invoke(action) : action();
            }
            return null;
        }

    }
}
