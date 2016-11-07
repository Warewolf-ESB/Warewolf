using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Services.Events;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Diagnostics;
// ReSharper disable UnusedMemberInSuper.Global

namespace Dev2.Studio.Core
{
    public class DebugStateTreeViewItemViewModel : DebugTreeViewItemViewModel<IDebugState>
    {
        readonly IEnvironmentRepository _environmentRepository;
        readonly ObservableCollection<object> _inputs;
        readonly ObservableCollection<object> _outputs;
        readonly ObservableCollection<object> _assertResultList;

        public DebugStateTreeViewItemViewModel(IEnvironmentRepository environmentRepository)
        {
            VerifyArgument.IsNotNull("environmentRepository", environmentRepository);
            _environmentRepository = environmentRepository;
            _inputs = new ObservableCollection<object>();
            _outputs = new ObservableCollection<object>();
            _assertResultList = new ObservableCollection<object>();
        }

        public ActivitySelectionType SelectionType { get; set; }

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
                        if (!currentError.Contains(errorMessage))
                        {
                            currentError.Append(errorMessage);
                            Content.ErrorMessage = currentError.ToString();
                        }
                }
                else Content.ErrorMessage = errorMessage;
                Content.HasError = true;
                OnPropertyChanged("Content.ErrorMessage");
                OnPropertyChanged("Content.HasError");
                OnPropertyChanged("Content");
                HasError = true;
            }
        }

        protected override void Initialize(IDebugState content)
        {
            _inputs.Clear();
            _outputs.Clear();
            _assertResultList.Clear();

            if (content == null)
            {
                return;
            }

            // Multiple when creating - so that we show the path of the execution when debugging
            SelectionType = ActivitySelectionType.Add;
            IsSelected = content.ActivityType != ActivityType.Workflow;

            Guid serverID;
            var isRemote = Guid.TryParse(content.Server, out serverID);
            if (isRemote|| String.IsNullOrEmpty( content.Server))
            {


                var envID = content.EnvironmentID;

                var env = _environmentRepository.All().FirstOrDefault(e => e.ID == envID);
                if (env == null)
                {
                    var environmentModels = _environmentRepository.LookupEnvironments(_environmentRepository.ActiveEnvironment);
                    if (environmentModels != null)
                    {
                        env = environmentModels.FirstOrDefault(e => e.ID == envID) ?? _environmentRepository.ActiveEnvironment;
                    }
                    else
                    {
                        env = _environmentRepository.Source;
                    }
                }
                if (Equals(env, _environmentRepository.Source))
                {
                    // We have an unknown remote server ;)
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
                foreach (var debugItem in content.AssertResultList)
                {
                    foreach (var debugItemResult in debugItem.ResultsList)
                    {
                        if (debugItemResult.HasError)
                        {
                            HasError = true;
                            HasNoError = false;
                        }
                        else
                        {
                            HasError = false;
                            HasNoError = true;
                        }
                    }
                }
            }
        }

        protected override void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);
            switch (propertyName)
            {
                case "IsSelected":
                    NotifySelectionChanged();
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
                EventPublishers.Studio.Publish(new DebugSelectionChangedEventArgs { DebugState = content, SelectionType = SelectionType });
            }
            else
            {
                EventPublishers.Studio.Publish(new DebugSelectionChangedEventArgs { DebugState = Content, SelectionType = ActivitySelectionType.Remove });
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
                        DebugLineGroup group;
                        if (!groups.TryGetValue(result.GroupName, out group))
                        {
                            group = new DebugLineGroup(result.GroupName, result.Label)
                            {
                                MoreLink = result.MoreLink
                            };

                            groups.Add(group.GroupName, group);
                            list.LineItems.Add(group);
                        }

                        DebugLineGroupRow row;
                        if (!group.Rows.TryGetValue(result.GroupIndex, out row))
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