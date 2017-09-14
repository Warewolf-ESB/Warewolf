using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using Dev2.Activities.Designers2.Core;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Toolbox;
using Microsoft.Practices.Prism.Mvvm;
using Newtonsoft.Json;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Core;

namespace Dev2.ViewModels.Merge
{
    public class MergeToolModel : BindableBase, IMergeToolModel
    {
        private bool _isMergeExpanderEnabled;
        private ImageSource _mergeIcon;
        private bool _isMergeExpanded;
        private string _mergeDescription;
        private List<string> _fieldCollection;
        private bool _isMergeChecked;

        public bool IsVariablesChecked { get; set; }

        public void SetMergeIcon(Type type)
        {
            if (type == null)
            {
                return;
            }
            if (type == typeof(DsfActivity))
            {
                MergeIcon = Application.Current?.TryFindResource("Explorer-WorkflowService") as ImageSource;
            }
            else if (type.GetCustomAttributes().Any(a => a is ToolDescriptorInfo))
            {
                var desc = GetDescriptorFromAttribute(type);
                MergeIcon = Application.Current?.TryFindResource(desc.Icon) as ImageSource;
            }
            else
            {
                MergeIcon = null;
            }
        }

        public ActivityDesignerViewModel ActivityDesignerViewModel { get; set; }

        IToolDescriptor GetDescriptorFromAttribute(Type type)
        {
            var info = type.GetCustomAttributes(typeof(ToolDescriptorInfo)).First() as ToolDescriptorInfo;

            return new ToolDescriptor(info.Id, info.Designer, new WarewolfType(type.FullName, type.Assembly.GetName().Version, type.Assembly.Location), info.Name, info.Icon, type.Assembly.GetName().Version, true, info.Category, ToolType.Native, info.IconUri, info.FilterTag, info.ResourceToolTip, info.ResourceHelpText);
        }

        public List<string> FieldCollection
        {
            get { return _fieldCollection; }
            set
            {
                _fieldCollection = value;
                OnPropertyChanged(() => FieldCollection);
            }
        }
        public bool IsMergeExpanderEnabled
        {
            get { return _isMergeExpanderEnabled; }
            set
            {
                _isMergeExpanderEnabled = value;
                OnPropertyChanged(() => IsMergeExpanderEnabled);
            }
        }
        public bool IsMergeExpanded
        {
            get { return _isMergeExpanded; }
            set
            {
                _isMergeExpanded = value;
                OnPropertyChanged(() => IsMergeExpanded);
            }
        }
        [JsonIgnore]
        public ImageSource MergeIcon
        {
            get { return _mergeIcon; }
            set
            {
                _mergeIcon = value;
                OnPropertyChanged(() => MergeIcon);
            }
        }
        public string MergeDescription
        {
            get { return _mergeDescription; }
            set
            {
                _mergeDescription = value;
                OnPropertyChanged(() => MergeDescription);
            }
        }
        public bool IsMergeChecked
        {
            get { return _isMergeChecked; }
            set
            {
                _isMergeChecked = value;
                OnPropertyChanged(() => IsMergeChecked);
            }
        }
    }
}