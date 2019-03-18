#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
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
