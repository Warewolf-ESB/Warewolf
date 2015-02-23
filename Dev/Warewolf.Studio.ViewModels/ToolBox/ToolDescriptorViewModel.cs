using System;
using System.Activities;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Toolbox;
using Microsoft.Practices.Prism.Mvvm;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Warewolf.Studio.ViewModels.ToolBox
{
    public class ToolDescriptorViewModel : BindableBase,IToolDescriptorViewModel
    {
        IToolDescriptor _tool;
        bool _isEnabled;

              static ResourceDictionary Resources;
        private DataObject _activityType;

        static ToolDescriptorViewModel()
        {
            

        }

        public ToolDescriptorViewModel(IToolDescriptor tool, bool isEnabled)
        {
            VerifyArgument.AreNotNull(new Dictionary<string, object>{ {"tool",tool}});
            IsEnabled = isEnabled;
            UpdateToolActualType(tool);
            Tool = tool;
        }

        private void UpdateToolActualType(IToolDescriptor tool)
        {
            try
            {
                //_activityType = new DataObject(System.Activities.Presentation.DragDropHelper.WorkflowItemTypeNameFormat, tool.Activity.FullyQualifiedName);
                var type = typeof(DsfNativeActivity<>);
                var assembly = type.Assembly;
                {
                    foreach (var exportedType in assembly.GetTypes())
                    {
                        if (exportedType.FullName == tool.Activity.FullyQualifiedName)
                        {
                            _activityType = new DataObject(System.Activities.Presentation.DragDropHelper.WorkflowItemTypeNameFormat,exportedType);
                            return;
                        }
                    }
                }                
            }
            catch (Exception e)
            {
                if (e != null)
                {
                    
                }
            }
            //tool.Designer.ActualType = Assembly.Load(tool.Designer.FullyQualifiedName).GetType();
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

        public string Name
        {
            get
            {
                return Tool.Name;
            }
        }

        public DrawingImage Icon
        {
            get
            {
                return GetImage( Tool.Icon,Tool.IconUri);
            }
        }

        DrawingImage GetImage(string icon,string iconUri)
        {
           return (DrawingImage)((ResourceDictionary)Application.LoadComponent(new Uri(iconUri,
               UriKind.RelativeOrAbsolute)))[icon];
        }

        public IWarewolfType Designer
        {
            get { return Tool.Designer; }
        }
        
        public IWarewolfType Activity
        {
            get { return Tool.Activity; }
        }

        public DataObject ActivityType
        {
            get { return _activityType; }
        }

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
