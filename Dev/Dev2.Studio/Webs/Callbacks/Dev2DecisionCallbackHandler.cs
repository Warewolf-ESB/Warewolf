using Dev2.Interfaces;
using Dev2.Studio.Core.Interfaces;
using System;
using System.Windows;

namespace Dev2.Webs.Callbacks
{
    public class Dev2DecisionCallbackHandler : IPropertyEditorWizard
    {
        public Window Owner { get; set; }

        public string ModelData { get; set; }

        public ILayoutObjectViewModel SelectedLayoutObject
        {
            get { return null; }
        }

        public void Save(string value, bool closeBrowserWindow = true)
        {
        }

        public void NavigateTo(string uri, string args, string returnUri)
        {
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
            ModelData = value;
            Close();
        }

        public void Dev2Done()
        {
            throw new NotImplementedException();
        }

        public void Dev2ReloadResource(Guid resourceID, string resourceType)
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

        public string GetIntellisenseResults(string searchTerm, int caretPosition)
        {
            return WebsiteCallbackHandler.GetJsonIntellisenseResults(searchTerm, caretPosition);
        }

        public event NavigateRequestedEventHandler NavigateRequested;

        protected void OnNavigateRequested(string uri)
        {
            if(NavigateRequested != null)
            {
                NavigateRequested(uri);
            }
        }

        public string FetchData(string args)
        {
            return ModelData;
        }
    }
}
