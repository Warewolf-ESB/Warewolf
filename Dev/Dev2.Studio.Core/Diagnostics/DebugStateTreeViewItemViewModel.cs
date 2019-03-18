#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
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
                    if (!string.IsNullOrEmpty(errorMessage) && !currentError.Contains(errorMessage))
                    {
                        currentError.Append(errorMessage);
                        Content.ErrorMessage = currentError.ToString();
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

        
        protected override void Initialize(IDebugState value)
        {
            if (value == null)
            {
                return;
            }
            SelectionType = ActivitySelectionType.Add;
            IsSelected = value.ActivityType != ActivityType.Workflow;

            var isRemote = Guid.TryParse(value.Server, out Guid serverId);
            if (isRemote || string.IsNullOrEmpty(value.Server))
            {
                var envId = value.EnvironmentID;

                var env = _serverRepository.All().FirstOrDefault(e => e.EnvironmentID == envId);
                if (env == null)
                {
                    var environmentModels = _serverRepository.LookupEnvironments(_serverRepository.ActiveServer);
                    env = environmentModels != null ? environmentModels.FirstOrDefault(e => e.EnvironmentID == envId) ?? _serverRepository.ActiveServer : _serverRepository.Source;
                }
                if (Equals(env, _serverRepository.Source))
                {
                    value.Server = "Unknown Remote Server";
                }
                else
                {
                    if (env != null)
                    {
                        value.Server = env.Name;
                    }
                }
            }
            BuildBindableListFromDebugItems(value.Inputs, _inputs, true);
            BuildBindableListFromDebugItems(value.Outputs, _outputs);
            BuildBindableListFromDebugItems(value.AssertResultList, _assertResultList);

            if (value.HasError)
            {
                HasError = true;
            }
            if (value.AssertResultList != null)
            {
                var setAllError = false;
                
                foreach (var debugItem in value.AssertResultList.Where(debugItem => debugItem.ResultsList.Any(debugItemResult => debugItemResult.HasError)))
                {
                    setAllError = true;
                }

                foreach (var debugItemResult in value.AssertResultList.SelectMany(debugItem => debugItem.ResultsList))
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

        static void BuildBindableListFromDebugItems(IEnumerable<IDebugItem> debugItems, ICollection<object> destinationList, bool isInputs = false)
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
                    BuildBindableListFromDebugItems(list, groups, result, isInputs);
                }

                destinationList.Add(list);
            }
        }

        static void BuildBindableListFromDebugItems(DebugLine list, Dictionary<string, DebugLineGroup> groups, IDebugItemResult result, bool isInputs)
        {
            if (string.IsNullOrEmpty(result.GroupName))
            {
                list.LineItems.Add(new DebugLineItem(result));
            }
            else
            {
                if (!groups.TryGetValue(result.GroupName, out DebugLineGroup group))
                {
                    var label = isInputs ? string.Format("{0} = ", result.Label) : result.Label;                    
                    group = new DebugLineGroup(result.GroupName, label)
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

        #region Overrides of Object

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString() => Content.DisplayName;

        #endregion
    }
}