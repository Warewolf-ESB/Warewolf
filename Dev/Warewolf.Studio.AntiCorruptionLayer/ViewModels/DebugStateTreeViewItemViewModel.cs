using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Dev2;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Services.Events;
using Dev2.Studio.Core.Interfaces;
using Warewolf.Studio.AntiCorruptionLayer.Interfaces;

namespace Warewolf.Studio.AntiCorruptionLayer.ViewModels
{
    internal class DebugStateTreeViewItemViewModel : DebugTreeViewItemViewModel<IDebugState>
    {
        readonly IEnvironmentRepository _environmentRepository;
        readonly ObservableCollection<object> _inputs;
        readonly ObservableCollection<object> _outputs;

        public DebugStateTreeViewItemViewModel(IEnvironmentRepository environmentRepository)
        {
            VerifyArgument.IsNotNull("environmentRepository", environmentRepository);
            _environmentRepository = environmentRepository;
            _inputs = new ObservableCollection<object>();
            _outputs = new ObservableCollection<object>();
        }

        public ActivitySelectionType SelectionType { get; set; }

        public ObservableCollection<object> Inputs { get { return _inputs; } }

        public ObservableCollection<object> Outputs { get { return _outputs; } }

        public void AppendError(string errorMessage)
        {
            if (Content != null)
            {
                if (Content.HasError)
                {
                    var currentError = new StringBuilder(Content.ErrorMessage);
                    currentError.Append(errorMessage);
                    Content.ErrorMessage = currentError.ToString();
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

            if (content == null)
            {
                return;
            }

            // Multiple when creating - so that we show the path of the execution when debugging
            SelectionType = ActivitySelectionType.Add;
            IsSelected = content.ActivityType != ActivityType.Workflow;

            // check for remote server ID ;)
            Guid serverID;
            var isRemote = Guid.TryParse(content.Server, out serverID);

            // avoid flagging empty guid as valid ;)
            if (isRemote && serverID == Guid.Empty)
            {
                isRemote = false;
            }

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
            if (Equals(env, _environmentRepository.Source) && isRemote)
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

            BuildBindableListFromDebugItems(content.Inputs, _inputs);
            BuildBindableListFromDebugItems(content.Outputs, _outputs);

            if (content.HasError)
            {
                HasError = true;
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

    public class DebugLineItem : IDebugLineItem
    {
        public DebugLineItem()
        {
        }

        public DebugLineItem(IDebugItemResult result)
        {
            Type = result.Type;
            Value = result.Value;
            MoreLink = result.MoreLink;
            Label = result.Label;
            Variable = result.Variable;
            Operator = result.Operator;
        }

        public string MoreLink { get; set; }
        public DebugItemResultType Type { get; set; }
        public string Value { get; set; }
        public string Label { get; set; }
        public string Variable { get; set; }
        public string Operator { get; set; }
    }

    public class DebugLineGroup : IDebugLineItem
    {
        public DebugLineGroup(string groupName, string groupLabel)
        {
            GroupName = groupName;
            Rows = new Dictionary<int, DebugLineGroupRow>();
            GroupLabel = groupLabel;
        }

        public string MoreLink { get; set; }
        public string GroupName { get; set; }
        public string GroupLabel { get; set; }
        public Dictionary<int, DebugLineGroupRow> Rows { get; private set; }
    }

    public class DebugLineGroupRow
    {
        public DebugLineGroupRow()
        {
            LineItems = new List<IDebugLineItem>();
        }

        public List<IDebugLineItem> LineItems { get; private set; }
    }

    public class DebugLine
    {
        public DebugLine()
        {
            LineItems = new List<IDebugLineItem>();
        }

        public List<IDebugLineItem> LineItems { get; private set; }

    }
}