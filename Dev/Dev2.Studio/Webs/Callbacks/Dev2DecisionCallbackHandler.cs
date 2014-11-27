
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
using System.Windows;
using Dev2.Interfaces;
using Dev2.Studio.Core.Interfaces;

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
