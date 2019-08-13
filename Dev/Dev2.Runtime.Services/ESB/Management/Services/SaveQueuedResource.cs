#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common;
using Dev2.Common.Interfaces.Data.TO;
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces.Queue;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.Security;
using Dev2.Scheduler;
using Dev2.Workspaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Warewolf.Resource.Errors;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class QueueResource : IQueueResource, INotifyPropertyChanged
    {
        string _name;
        string _workflowName;
        QueueStatus _status;

        bool _isDirty;
        string _userName;
        string _password;
        string _oldName;
        IErrorResultTO _errors;
        DateTime _nextRunDate;
        bool _isNew;

        public QueueResource(string name, QueueStatus status, string workflowName, string resourceId)
        {
            WorkflowName = workflowName;
            _status = status;
            Name = history.First();
            IsDirty = false;
            _errors = new ErrorResultTO();
            if (!String.IsNullOrEmpty(resourceId))
            {
                ResourceId = Guid.Parse(resourceId);
            }
        }

        public bool IsDirty
        {
            get
            {
                return _isDirty;
            }
            set
            {
                _isDirty = value;
                OnPropertyChanged("IsDirty");
                OnPropertyChanged("NameForDisplay");
            }
        }

        public string OldName
        {
            get
            {
                return _oldName;
            }
            set
            {
                _oldName = value;
                OnPropertyChanged("OldName");
                if (!IsDirty)
                {
                    OnPropertyChanged("NameForDisplay");
                }
            }
        }

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if (NameForDisplay != value)
                {
                    IsDirty = true;
                }
                _name = value;
                OnPropertyChanged("Name");
                OnPropertyChanged("NameForDisplay");
            }
        }

        public string NameForDisplay
        {
            get
            {
                if (IsDirty)
                {
                    return Name + " *";
                }
                return Name;
            }
        }

        public QueueStatus Status
        {
            get
            {
                return _status;
            }
            set
            {
                if (Status != value)
                {
                    IsDirty = true;
                }
                _status = value;
                OnPropertyChanged("Status");
                OnPropertyChanged("StatusAlt");
            }
        }

        [JsonIgnore]
        public QueueStatus StatusAlt
        {
            get
            {
                return _status;
            }
            set
            {
                IsDirty = true;
                _status = _status == QueueStatus.Disabled ? QueueStatus.Enabled : QueueStatus.Disabled;
                OnPropertyChanged("StatusAlt");
                OnPropertyChanged("Status");
                Dev2Logger.Info("Queue Resource Alt Status set to " + value, GlobalConstants.WarewolfInfo);
            }
        }

        public string WorkflowName
        {
            get
            {
                return _workflowName;
            }
            set
            {
                _workflowName = value;
                OnPropertyChanged("WorkflowName");
            }
        }
        public string UserName
        {
            get { return _userName; }
            set
            {
                _userName = value;
                OnPropertyChanged("UserName");
            }
        }

        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                OnPropertyChanged("Password");
            }
        }

        public IErrorResultTO Errors
        {
            get
            {
                return _errors;
            }
            set
            {
                _errors = value;
                OnPropertyChanged("Errors");
            }
        }
        public bool IsNew
        {
            get
            {
                return _isNew;
            }
            set
            {
                IsDirty = value;
                _isNew = value;
            }
        }
        public bool IsNewItem { get; set; }


        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool Equals(IQueueResource other)
        {
            if (other == null)
            {
                return false;
            }
            if (IsNew)
            {
                return false;
            }
            var nameEqual = other.Name.Equals(Name, StringComparison.CurrentCultureIgnoreCase);
            var statusEqual = other.Status == Status;
            var workflowNameEqual = other.WorkflowName.Equals(WorkflowName, StringComparison.InvariantCultureIgnoreCase);
            var userNameEqual = !string.IsNullOrEmpty(other.UserName) && !string.IsNullOrEmpty(UserName) ? other.UserName.Equals(UserName, StringComparison.InvariantCultureIgnoreCase) : string.IsNullOrEmpty(other.UserName) && string.IsNullOrEmpty(UserName);
            return nameEqual && statusEqual && workflowNameEqual && allowMultipleInstancesEqual && userNameEqual;
        }

        public override string ToString() => String.Format("Name:{0} ResourceId:{1}", Name, ResourceId);

        public Guid ResourceId { get; set; }
    }
    public class SaveQueuedResource : IEsbManagementEndpoint
    {
        IResourceCatalog _catalog;
        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs) => Guid.Empty;

        public AuthorizationContext GetAuthorizationContextForService() => AuthorizationContext.Contribute;

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var result = new ExecuteMessage { HasError = false };
            values.TryGetValue("Resource", out StringBuilder tmp);
            var serializer = new Dev2JsonSerializer();
            try
            {
                TryExecute(values, result, tmp, serializer);
            }
            catch (Exception e)
            {
                Dev2Logger.Error(e, GlobalConstants.WarewolfError);
                result.Message.Append($"Error while saving: {e.Message.Remove(e.Message.IndexOf('.'))}");
                result.HasError = true;
            }
            return serializer.SerializeToBuilder(result);
        }

        void TryExecute(Dictionary<string, StringBuilder> values, ExecuteMessage result, StringBuilder tmp, Dev2JsonSerializer serializer)
        {
            if (tmp != null)
            {
                var res = serializer.Deserialize<IQueueResource>(tmp);
                Dev2Logger.Info("Save Queue Resource. Queue Resource:" + res, GlobalConstants.WarewolfInfo);

                values.TryGetValue("UserName", out StringBuilder userName);
                values.TryGetValue("Password", out StringBuilder password);
                if (userName == null || password == null)
                {
                    result.Message.Append(ErrorResource.NoUserNameAndPassword);
                    result.HasError = true;
                }
                else
                {
                    values.TryGetValue("PreviousResource", out StringBuilder previousTask);
                    var model;
                    model.Save(res, userName.ToString(), password.ToString());
                    if (!string.IsNullOrEmpty(previousTask?.ToString()) && previousTask.ToString() != res.Name)
                    {
                        model.DeleteQueue(new QueueResource(previousTask.ToString(), QueueStatus.Disabled, DateTime.MaxValue, null, null, Guid.NewGuid().ToString()));
                    }
                }
            }
            else
            {
                result.Message.Append(ErrorResource.NoResourceSelected);
                result.HasError = true;
            }
        }

        public IResourceCatalog ResourceCatalogue
        {
            get => _catalog ?? ResourceCatalog.Instance;
            set => _catalog = value;
        }

        public DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public string HandlesType() => "SaveQueueResourceService";
    }
}
