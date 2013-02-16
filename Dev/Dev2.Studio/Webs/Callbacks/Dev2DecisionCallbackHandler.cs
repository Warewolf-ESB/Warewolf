using Dev2.Studio.Core.Interfaces;
using System;
using System.Windows;

namespace Dev2.Studio.Core.AppResources.Browsers
{
    class Dev2DecisionCallbackHandler : IPropertyEditorWizard
    {
        public Window Owner { get; set; }

        public ILayoutObjectViewModel SelectedLayoutObject
        {
            get { return null; }
        }

        public void OpenPropertyEditor()
        {

        }

        public void Dev2Set(string data, string uri)
        {
            throw new NotImplementedException();
        }

        public void Dev2SetValue(string value)
        {
            throw new NotImplementedException();
        }

        public void Dev2Done()
        {
            throw new NotImplementedException();
        }

        public void Dev2ReloadResource(string resourceName, string resourceType)
        {
            throw new NotImplementedException();
        }

        public void Close()
        {
            if(Owner != null)
            {
                Owner.Close();
            }

        }

        public void Cancel()
        {
            Close();
        }

        public event NavigateRequestedEventHandler NavigateRequested;

        protected void OnNavigateRequested(string uri)
        {
            if(NavigateRequested != null)
            {
                NavigateRequested(uri);
            }
        }
    }
}
