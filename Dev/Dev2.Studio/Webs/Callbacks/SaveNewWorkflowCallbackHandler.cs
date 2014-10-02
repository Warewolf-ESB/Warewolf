
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
using Caliburn.Micro;
using Dev2.Common;
using Dev2.Messages;
using Dev2.Providers.Logs;
using Dev2.Studio.Core.Interfaces;

namespace Dev2.Webs.Callbacks
{
    public class SaveNewWorkflowCallbackHandler
       : WebsiteCallbackHandler
    {
        #region Fields

        private readonly IContextualResourceModel _resourceModel;
        public bool AddToTabManager { private set; get; }

        #endregion
        public SaveNewWorkflowCallbackHandler(IEventAggregator eventPublisher, IEnvironmentRepository currentEnvironmentRepository, IContextualResourceModel resourceModel, bool addToTabManager)
            : base(eventPublisher, currentEnvironmentRepository)
        {
            AddToTabManager = addToTabManager;
            _resourceModel = resourceModel;
        }


        #region Overrides of WebsiteCallbackHandler

        protected override void Save(IEnvironmentModel environmentModel, dynamic jsonObj)
        {
            try
            {
                string resName = jsonObj.resourceName;
                string resCat = SanitizePath((string)jsonObj.resourcePath, resName);
                if(_resourceModel != null)
                {
                    EventPublisher.Publish(new SaveUnsavedWorkflowMessage(_resourceModel, resName, resCat, AddToTabManager));
                }

                Close();
            }
            catch(Exception e)
            {
                Exception e1 = new Exception("There was a problem saving. Please try again.", e);

                Dev2Logger.Log.Info(e.Message + Environment.NewLine + " Stacktrace : " + e.StackTrace + Environment.NewLine + " jsonObj: " + jsonObj.ToString());

                throw e1;
            }
        }
        #endregion

        public string SanitizePath(string path, string resourceName = "")
        {
            if(string.IsNullOrEmpty(path))
            {
                return "";
            }

            if(path.ToLower().StartsWith("root\\\\"))
            {
                path = path.Remove(0, 6);
            }

            if(path.ToLower().Equals("root"))
            {
                path = path.Remove(0, 4);
            }

            if(path.StartsWith("\\"))
            {
                path = path.Remove(0, 1);
            }

            path = string.IsNullOrEmpty(path) ? resourceName : path + "\\" + resourceName;

            return path.Replace("\\\\", "\\")
                .Replace("\\\\", "\\");
        }
    }
}
