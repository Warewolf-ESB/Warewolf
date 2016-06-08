
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using Dev2.Common.Utils;
using Dev2.Interfaces;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.InterfaceImplementors;
using Newtonsoft.Json.Linq;

namespace Dev2.Webs.Callbacks
{
    public abstract class WebsiteCallbackHandler : IPropertyEditorWizard
    {
        protected readonly IEventAggregator EventPublisher;

        protected WebsiteCallbackHandler(IEventAggregator eventPublisher, IEnvironmentRepository currentEnvironmentRepository, IShowDependencyProvider showDependencyProvider = null)
        {
            VerifyArgument.IsNotNull("eventPublisher", eventPublisher);
            VerifyArgument.IsNotNull("currentEnvironmentRepository", currentEnvironmentRepository);
            EventPublisher = eventPublisher;

            CurrentEnvironmentRepository = currentEnvironmentRepository;
        }


        #region Properties

        public Window Owner { get; set; }

        public IEnvironmentRepository CurrentEnvironmentRepository { get; private set; }

        #endregion

        protected abstract void Save(IEnvironmentModel environmentModel, dynamic jsonArgs);

        #region Navigate

        #endregion

        #region ReloadResource

        #endregion

        #region Implementation of IPropertyEditorWizard

        public ILayoutObjectViewModel SelectedLayoutObject
        {
            get
            {
                return null;
            }
        }

        public virtual void Save(string value, bool closeBrowserWindow = true)
        {
            Save(value, EnvironmentRepository.Instance.Source, closeBrowserWindow);
        }

        public virtual void Save(string value, IEnvironmentModel environmentModel, bool closeBrowserWindow = true)
        {
            if(closeBrowserWindow)
            {
                Close();
            }

            if(string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException("value");
            }
            value = JSONUtils.ScrubJSON(value);

            dynamic jsonObj = JObject.Parse(value);
            Save(environmentModel, jsonObj);
        }

        public virtual void OpenPropertyEditor()
        {
        }

        public virtual void Dev2Set(string data, string uri)
        {
        }

        public virtual void Dev2SetValue(string value)
        {
        }

        public virtual void Dev2Done()
        {
        }

        public virtual void Dev2ReloadResource(Guid resourceName, string resourceType)
        {
            throw new NotImplementedException();
        }

        public virtual void Close()
        {
            if(Owner != null)
            {
                Owner.Close();
            }
        }

        public virtual void Cancel()
        {
            Close();
        }

        public string FetchData(string args)
        {
            return null;
        }

        public string GetIntellisenseResults(string searchTerm, int caretPosition)
        {
            return GetJsonIntellisenseResults(searchTerm, caretPosition);
        }

        public event NavigateRequestedEventHandler NavigateRequested;

        #endregion

        #region GetJsonIntellisenseResults

        public static string GetJsonIntellisenseResults(string searchTerm, int caretPosition)
        {
            var provider = new DefaultIntellisenseProvider();
            var context = new IntellisenseProviderContext { InputText = searchTerm, CaretPosition = caretPosition };

            return "[" + string.Join(",", provider.GetIntellisenseResults(context).Where(c => !c.IsError).Select(r => string.Format("\"{0}\"", r.ToString()))) + "]";
        }

        #endregion
    }
}
