using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Services.Events;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Diagnostics;

namespace Dev2.ViewModels.Diagnostics
{
    public class DebugStringTreeViewItemViewModel : DebugTreeViewItemViewModel<string>
    {
        public DebugStringTreeViewItemViewModel()
        {
            IsExpanded = false;
        }

        protected override void Initialize(string content)
        {
        }
    }

    public class DebugStateTreeViewItemViewModel : DebugTreeViewItemViewModel<IDebugState>
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
            if(Content != null)
            {
                if(Content.HasError)
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

            if(content == null)
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
            if(isRemote && serverID == Guid.Empty)
            {
                isRemote = false;
            }

            var envID = content.EnvironmentID;

            var env = _environmentRepository.All().FirstOrDefault(e => e.ID == envID);
            if (env == null)
            {
                var environmentModels = _environmentRepository.LookupEnvironments(_environmentRepository.ActiveEnvironment);
                if(environmentModels != null)
                {
                    env = environmentModels.FirstOrDefault(e => e.ID == envID) ?? _environmentRepository.ActiveEnvironment;
                }
                else
                {
                    env = _environmentRepository.Source;
                }
            }
            if(Equals(env, _environmentRepository.Source) && isRemote)
            {
                // We have an unknown remote server ;)
                content.Server = "Unknown Remote Server";
            }
            else
            {
                if(env != null)
                {
                    content.Server = env.Name;
                }
            }

            BuildBindableListFromDebugItems(content.Inputs, _inputs);
            BuildBindableListFromDebugItems(content.Outputs, _outputs);

            if(content.HasError)
            {
                HasError = true;
            }
        }

        protected override void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);
            switch(propertyName)
            {
                case "IsSelected":
                    NotifySelectionChanged();
                    break;
            }
        }

        void NotifySelectionChanged()
        {
            if(IsSelected)
            {
                var content = Content;
                if(Parent != null)
                {
                    // Only show selection at the root level!
                    var parent = (DebugStateTreeViewItemViewModel)Parent;
                    do
                    {
                        content = parent.Content;
                        parent = (DebugStateTreeViewItemViewModel)parent.Parent;
                    }
                    while(parent != null);
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

            if(debugItems == null)
            {
                return;
            }

            foreach(var item in debugItems)
            {
                var list = new DebugLine();
                var groups = new Dictionary<string, DebugLineGroup>();
                foreach(var result in item.FetchResultsList())
                {
                    if(string.IsNullOrEmpty(result.GroupName))
                    {
                        list.LineItems.Add(new DebugLineItem(result));
                    }
                    else
                    {
                        DebugLineGroup group;
                        if(!groups.TryGetValue(result.GroupName, out group))
                        {
                            group = new DebugLineGroup(result.GroupName, result.Label)
                            {
                                MoreLink = result.MoreLink
                            };

                            groups.Add(group.GroupName, group);
                            list.LineItems.Add(group);
                        }

                        DebugLineGroupRow row;
                        if(!group.Rows.TryGetValue(result.GroupIndex, out row))
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

    public interface IDebugTreeViewItemViewModel : INotifyPropertyChanged
    {
        bool IsExpanded { get; set; }
        bool IsSelected { get; set; }
        int Depth { get; }
        ObservableCollection<IDebugTreeViewItemViewModel> Children { get; }
        IDebugTreeViewItemViewModel Parent { get; set; }
        bool? HasError { get; set; }

        void VerifyErrorState();
    }

    public abstract class DebugTreeViewItemViewModel<TContent> : IDebugTreeViewItemViewModel
        where TContent : class
    {
        readonly ObservableCollection<IDebugTreeViewItemViewModel> _children;

        bool? _hasError = false;
        bool _isExpanded;
        bool _isSelected;
        TContent _content;
        IDebugTreeViewItemViewModel _parent;

        protected DebugTreeViewItemViewModel()
        {
            _children = new ObservableCollection<IDebugTreeViewItemViewModel>();
        }

        public TContent Content
        {
            get { return _content; }
            set
            {
                if(value != _content)
                {
                    _content = value;
                    Initialize(value);
                    OnPropertyChanged("Content");
                }
            }
        }

        abstract protected void Initialize(TContent value);

        public int Depth
        {
            get
            {
                return _parent == null ? 0 : _parent.Depth + 1;
            }
        }

        public ObservableCollection<IDebugTreeViewItemViewModel> Children
        {
            get
            {
                return _children;
            }
        }

        public IDebugTreeViewItemViewModel Parent
        {
            get
            {
                return _parent;
            }
            set
            {
                if(value != _parent)
                {
                    _parent = value;
                    if(_parent != null)
                    {
                        _parent.VerifyErrorState();
                    }
                    OnPropertyChanged("Parent");
                    OnPropertyChanged("Depth");
                }
            }
        }

        public bool? HasError
        {
            get
            {
                return _hasError;
            }
            set
            {
                _hasError = value;
                if(_parent != null)
                {
                    _parent.VerifyErrorState();
                }
                OnPropertyChanged("HasError");
            }
        }

        public void VerifyErrorState()
        {
            if(HasError == true)
            {
                return;
            }

            if(_children.Any(c => c.HasError == true || c.HasError == null))
            {
                HasError = null;
            }
            else
            {
                HasError = false;
            }
            OnPropertyChanged("HasError");
        }

        public bool IsExpanded
        {
            get
            {
                return _isExpanded;
            }
            set
            {
                if(value != _isExpanded)
                {
                    _isExpanded = value;
                    OnPropertyChanged("IsExpanded");
                }

                // Expand all the way up to the root.
                if(_isExpanded && _parent != null)
                {
                    _parent.IsExpanded = true;
                }
            }
        }

        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                if(value != _isSelected)
                {
                    _isSelected = value;
                    OnPropertyChanged("IsSelected");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if(PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}