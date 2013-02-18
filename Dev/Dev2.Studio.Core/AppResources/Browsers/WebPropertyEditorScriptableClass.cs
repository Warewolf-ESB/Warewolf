using System;
using System.Runtime.InteropServices;
using System.Windows;
using Dev2.Studio.Core.Interfaces;

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

        public void Save(string value)
        {
            if(PropertyEditorViewModel != null)
            {
                InvokeAction(() => PropertyEditorViewModel.Save(value));
            }
        }

        public void NavigateTo(string uri, string args, string returnUri)
        {
            if(PropertyEditorViewModel != null)
            {
                InvokeAction(() => PropertyEditorViewModel.NavigateTo(uri, args, returnUri));
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

        public void Dev2ReloadResource(string resourceName, string resourceType)
        {
            if(PropertyEditorViewModel != null)
            {
                InvokeAction(() => PropertyEditorViewModel.Dev2ReloadResource(resourceName, resourceType));
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
    }
}
