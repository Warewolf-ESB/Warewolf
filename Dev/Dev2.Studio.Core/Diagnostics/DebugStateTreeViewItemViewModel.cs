using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Services.Events;
using Dev2.Studio.Diagnostics;
using Dev2.Studio.Interfaces;




namespace Dev2.Studio.Core
{
    public class DebugStateTreeViewItemViewModel : DebugTreeViewItemViewModel<IDebugState>
    {
        readonly IServerRepository _serverRepository;
        readonly ObservableCollection<object> _inputs;
        readonly ObservableCollection<object> _outputs;
        readonly ObservableCollection<object> _assertResultList;

        public DebugStateTreeViewItemViewModel(IServerRepository serverRepository)
        {
            VerifyArgument.IsNotNull("environmentRepository", serverRepository);
            _serverRepository = serverRepository;
            _inputs = new ObservableCollection<object>();
            _outputs = new ObservableCollection<object>();
            _assertResultList = new ObservableCollection<object>();
            IsTestView = false;
        }

        public ActivitySelectionType SelectionType
        {
            get;
            set;
        }

        public ObservableCollection<object> Inputs => _inputs;

        public ObservableCollection<object> Outputs => _outputs;

        public ObservableCollection<object> AssertResultList => _assertResultList;

        public void AppendError(string errorMessage)
        {
            if (Content != null)
            {
                if (Content.HasError)
                {
                    var currentError = new StringBuilder(Content.ErrorMessage);
                    if (!string.IsNullOrEmpty(errorMessage))
                    {
                        if (!currentError.Contains(errorMessage))
                        {
                            currentError.Append(errorMessage);
                            Content.ErrorMessage = currentError.ToString();
                        }
                    }
                }
                else
                {
                    Content.ErrorMessage = errorMessage;
                }

                Content.HasError = true;
                OnPropertyChanged("Content.ErrorMessage");
                OnPropertyChanged("Content.HasError");
                OnPropertyChanged("Content");
                HasError = true;
            }
        }

        
        protected override void Initialize(IDebugState content)
        {
            if (content == null)
            {
                return;
            }
            SelectionType = ActivitySelectionType.Add;
            IsSelected = content.ActivityType != ActivityType.Workflow;

            var isRemote = Guid.TryParse(content.Server, out Guid serverId);
            if (isRemote || string.IsNullOrEmpty(content.Server))
            {
                var envId = content.EnvironmentID;

                var env = _serverRepository.All().FirstOrDefault(e => e.EnvironmentID == envId);
                if (env == null)
                {
                    var environmentModels = _serverRepository.LookupEnvironments(_serverRepository.ActiveServer);
                    env = environmentModels != null ? environmentModels.FirstOrDefault(e => e.EnvironmentID == envId) ?? _serverRepository.ActiveServer : _serverRepository.Source;
                }
                if (Equals(env, _serverRepository.Source))
                {
                    content.Server = "Unknown Remote Server";
                }
                else
                {
                    if (env != null)
                    {
                        content.Server = env.Name;
                    }
                }
            }
            BuildBindableListFromDebugItems(content.Inputs, _inputs);
            BuildBindableListFromDebugItems(content.Outputs, _outputs);
            BuildBindableListFromDebugItems(content.AssertResultList, _assertResultList);

            if (content.HasError)
            {
                HasError = true;
            }
            if (content.AssertResultList != null)
            {
                var setAllError = false;
                
                foreach (var debugItem in content.AssertResultList.Where(debugItem => debugItem.ResultsList.Any(debugItemResult => debugItemResult.HasError)))
                {
                    setAllError = true;
                }

                foreach (var debugItemResult in content.AssertResultList.SelectMany(debugItem => debugItem.ResultsList))
                {
                    if (setAllError)
                    {
                        HasError = true;
                        HasNoError = false;
                    }
                    else
                    {
                        HasError = debugItemResult.HasError;
                        HasNoError = !debugItemResult.HasError;
                    }
                    MockSelected = debugItemResult.MockSelected;
                    TestDescription = debugItemResult.MockSelected ? "Mock :" : "Assert :";
                }
            }
            SelectionType = ActivitySelectionType.Single;
        }

        protected override void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);
            switch (propertyName)
            {
                case "IsSelected":
                    NotifySelectionChanged();
                    break;
                default:
                    break;
            }
        }

        void NotifySelectionChanged()
        {
            if (IsSelected)
            {
                var content = Content;

                if (Parent != null)
                {
                    // Only show selection at the root level!
                    var parent = (DebugStateTreeViewItemViewModel)Parent;
                    do
                    {
                        content = parent.Content;
                        parent = (DebugStateTreeViewItemViewModel)parent.Parent;
                    }
                    while (parent != null);
                }
                if (!IsTestView)
                {

                    EventPublishers.Studio.Publish(new DebugSelectionChangedEventArgs
                    {
                        DebugState = content,
                        SelectionType = SelectionType
                    });
                }
            }
            else
            {
                if (!IsTestView)
                {
                    EventPublishers.Studio.Publish(new DebugSelectionChangedEventArgs
                    {
                        DebugState = Content,
                        SelectionType = ActivitySelectionType.Remove
                    });
                }
            }

            SelectionType = ActivitySelectionType.Single;
        }

        static void BuildBindableListFromDebugItems(IEnumerable<IDebugItem> debugItems, ICollection<object> destinationList)
        {
            destinationList.Clear();

            if (debugItems == null)
            {
                return;
            }

            foreach (var item in debugItems)
            {
                var list = new DebugLine();
                var groups = new Dictionary<string, DebugLineGroup>();
                foreach (var result in item.FetchResultsList())
                {
                    if (string.IsNullOrEmpty(result.GroupName))
                    {
                        list.LineItems.Add(new DebugLineItem(result));
                    }
                    else
                    {
                        if (!groups.TryGetValue(result.GroupName, out DebugLineGroup group))
                        {
                            group = new DebugLineGroup(result.GroupName, result.Label)
                            {
                                MoreLink = result.MoreLink
                            };

                            groups.Add(group.GroupName, group);
                            list.LineItems.Add(group);
                        }

                        if (!group.Rows.TryGetValue(result.GroupIndex, out DebugLineGroupRow row))
                        {
                            row = new DebugLineGroupRow();
                            group.Rows.Add(result.GroupIndex, row);
                        }
                        row.LineItems.Add(new DebugLineItem(result));
                    }
                }

                destinationList.Add(list);
            }
        }

        #region Overrides of Object

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return Content.DisplayName;
        }

        #endregion
    }
}