/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

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
                OnPropertyChanged(nameof(Content));
                HasError = true;
            }
        }


        protected override void Initialize(IDebugState debugState)
        {
            if (debugState == null)
            {
                return;
            }
            SelectionType = ActivitySelectionType.Add;
            IsSelected = debugState.ActivityType != ActivityType.Workflow;

            SetServerName(debugState);
            BuildBindableListFromDebugItems(debugState.Inputs, _inputs, true);
            BuildBindableListFromDebugItems(debugState.Outputs, _outputs);
            BuildBindableListFromDebugItems(debugState.AssertResultList, _assertResultList);

            if (debugState.HasError)
            {
                HasError = true;
            }
            if (debugState.AssertResultList != null)
            {
                SetTestDescription(debugState);
            }
            SelectionType = ActivitySelectionType.Single;
        }

        private static bool SetAllError(IDebugState debugState)
        {
            var setAllError = false;

            foreach (var debugItem in debugState.AssertResultList.Where(debugItem => debugItem.ResultsList.Any(debugItemResult => debugItemResult.HasError)))
            {
                setAllError = true;
            }

            return setAllError;
        }

        private void SetTestDescription(IDebugState debugState)
        {
            bool setAllError = SetAllError(debugState);

            foreach (var debugItemResult in debugState.AssertResultList.SelectMany(debugItem => debugItem.ResultsList))
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

        private void SetServerName(IDebugState debugState)
        {
            var isRemote = Guid.TryParse(debugState.Server, out Guid serverId);
            if (isRemote || string.IsNullOrEmpty(debugState.Server))
            {
                var envId = debugState.EnvironmentID;

                var env = _serverRepository.All().FirstOrDefault(e => e.EnvironmentID == envId);
                if (env == null)
                {
                    var environmentModels = _serverRepository.LookupEnvironments(_serverRepository.ActiveServer);
                    env = environmentModels != null ? environmentModels.FirstOrDefault(e => e.EnvironmentID == envId) ?? _serverRepository.ActiveServer : _serverRepository.Source;
                }
                if (Equals(env, _serverRepository.Source))
                {
                    debugState.Server = "Unknown Remote Server";
                }
                else
                {
                    if (env != null)
                    {
                        debugState.Server = env.Name;
                    }
                }
            }
        }

        protected override void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);
            if (propertyName == nameof(IsSelected))
            {
                NotifySelectionChanged();
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
                    var parent = Parent;
                    do
                    {
                        content = parent.As<DebugStateTreeViewItemViewModel>()?.Content;
                        parent = parent.Parent;
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

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString() => Content.DisplayName;
    }
}