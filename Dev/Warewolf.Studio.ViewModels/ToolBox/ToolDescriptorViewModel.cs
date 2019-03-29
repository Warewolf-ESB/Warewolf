#pragma warning disable
﻿using System;
using System.Activities.Presentation;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using Dev2;
using Dev2.Common.Interfaces.Toolbox;
using Microsoft.Practices.Prism.Mvvm;
using Unlimited.Applications.BusinessDesignStudio.Activities;


namespace Warewolf.Studio.ViewModels.ToolBox
{
    public class ToolDescriptorViewModel : BindableBase,IToolDescriptorViewModel
    {
        IToolDescriptor _tool;
        bool _isEnabled;

        DataObject _activityType;
        DrawingImage _icon;

        public ToolDescriptorViewModel(IToolDescriptor tool, bool isEnabled)
        {
            VerifyArgument.AreNotNull(new Dictionary<string, object>{ {"tool",tool}});
            IsEnabled = isEnabled;
            UpdateToolActualType(tool);
            Tool = tool;
        }

        void UpdateToolActualType(IToolDescriptor tool)
        {
            var type = typeof(DsfNativeActivity<>);
            var assembly = type.Assembly;
            {
                foreach (var exportedType in assembly.GetTypes())
                {
                    if (exportedType.FullName == tool.Activity?.FullyQualifiedName)
                    {
                        if (exportedType.AssemblyQualifiedName != null)
                        {
                            UpdateToolActualType(exportedType);
                        }
                        return;
                    }
                }
            }
        }

        private void UpdateToolActualType(Type exportedType)
        {
            if (exportedType.FullName != null && exportedType.FullName.Contains("DsfFlowDecisionActivity"))
            {
                var decisionType = typeof(FlowDecision);
                if (decisionType.AssemblyQualifiedName != null)
                {
                    _activityType = new DataObject(DragDropHelper.WorkflowItemTypeNameFormat, decisionType.AssemblyQualifiedName);
                }
            }
            else if (exportedType.FullName != null && exportedType.FullName.Contains("DsfFlowSwitchActivity"))
            {
                var switchType = typeof(FlowSwitch<string>);
                if (switchType.AssemblyQualifiedName != null)
                {
                    _activityType = new DataObject(DragDropHelper.WorkflowItemTypeNameFormat, switchType.AssemblyQualifiedName);
                }
            }
            else
            {
                _activityType = new DataObject(DragDropHelper.WorkflowItemTypeNameFormat, exportedType.AssemblyQualifiedName);
            }
        }

        #region Implementation of IToolDescriptorViewModel

        public IToolDescriptor Tool
        {
            get
            {
                return _tool;
            }
            private set
            {
                OnPropertyChanged("Tool");
                _tool = value;
            }
        }

        public string Name => Tool.Name;

        public DrawingImage Icon => _icon ?? (_icon= GetImage(Tool.Icon, Tool.IconUri));

        DrawingImage GetImage(string icon, string iconUri) => (DrawingImage)((ResourceDictionary)Application.LoadComponent(new Uri(iconUri,
               UriKind.RelativeOrAbsolute)))[icon];

        public IWarewolfType Designer => Tool.Designer;

        public IWarewolfType Activity => Tool.Activity;

        public object ActivityType => _activityType;

        public bool IsEnabled
        {
            get
            {
                return _isEnabled;
            }
            private set
            {
                OnPropertyChanged("IsEnabled");
                _isEnabled = value;
            }
        }

        #endregion
    }
}
