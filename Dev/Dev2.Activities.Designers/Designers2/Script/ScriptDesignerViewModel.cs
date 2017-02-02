/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using Dev2.Activities.Designers2.Core;
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces.Enums.Enums;
using Dev2.Interfaces;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Services.Events;
using Dev2.Studio.Core.Messages;

namespace Dev2.Activities.Designers2.Script
{
    public class ScriptDesignerViewModel : ActivityDesignerViewModel
    {
        readonly IEventAggregator _eventPublisher;
        public ScriptDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            _eventPublisher = EventPublishers.Aggregator;
            _eventPublisher.Subscribe(this);

            EscapeScript = true;
            ScriptTypes = Dev2EnumConverter.ConvertEnumsTypeToStringList<enScriptType>();
            SelectedScriptType = Dev2EnumConverter.ConvertEnumValueToString(ScriptType);
            ChooseScriptSourceCommand = new DelegateCommand(o => ChooseScriptSources());
            AddTitleBarLargeToggle();
            HelpText = Warewolf.Studio.Resources.Languages.HelpText.Tool_Scripting_Script;
        }

        public string IncludeFile
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public bool EscapeScript { get; private set; }

        public IList<string> ScriptTypes { get; private set; }

        public string SelectedScriptType
        {
            get { return (string)GetValue(SelectedScriptTypeProperty); }
            set { SetValue(SelectedScriptTypeProperty, value); }
        }
        
        public static readonly DependencyProperty SelectedScriptTypeProperty =
            DependencyProperty.Register("SelectedScriptType", typeof(string), typeof(ScriptDesignerViewModel), new PropertyMetadata(null, OnSelectedScriptTypeChanged));

        static void OnSelectedScriptTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var viewModel = (ScriptDesignerViewModel)d;
            var value = e.NewValue as string;

            if (!string.IsNullOrWhiteSpace(value))
            {
                switch (value)
                {
                    case "Ruby":
                        viewModel.ScriptTypeDefaultText = "Ruby Syntax";
                        break;
                    case "Python":
                        viewModel.ScriptTypeDefaultText = "Python Syntax";
                        break;
                    default:
                        viewModel.ScriptTypeDefaultText = "JavaScript Syntax";
                        break;
                }
                viewModel.ScriptType = (enScriptType)Dev2EnumConverter.GetEnumFromStringDiscription(value, typeof(enScriptType));
            }
        }

        public ICommand ChooseScriptSourceCommand { get; private set; }

        public string ScriptTypeDefaultText
        {
            get { return (string)GetValue(ScriptTypeTextProperty); }
            set { SetValue(ScriptTypeTextProperty, value); }
        }

        public static readonly DependencyProperty ScriptTypeTextProperty =
            DependencyProperty.Register("ScriptTypeDefaultText", typeof(string), typeof(ScriptDesignerViewModel), new PropertyMetadata(null));

        // DO NOT bind to these properties - these are here for convenience only!!!
        enScriptType ScriptType { set { SetProperty(value); } get { return GetProperty<enScriptType>(); } }

        public override void Validate()
        {
        }

        public override void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IMainViewModel>();
            mainViewModel?.HelpViewModel.UpdateHelpText(helpText);
        }

        public void ChooseScriptSources()
        {
            const string Separator = @";";
            var chooserMessage = new FileChooserMessage();
            if (IncludeFile == null)
            {
                IncludeFile = "";
            }
            chooserMessage.SelectedFiles = IncludeFile?.Split(Separator.ToCharArray());
            chooserMessage.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == @"SelectedFiles")
                {
                    if (chooserMessage.SelectedFiles != null)
                    {
                        if (string.IsNullOrEmpty(IncludeFile))
                        {
                            IncludeFile = string.Join(Separator, chooserMessage.SelectedFiles);
                        }
                        else
                        {
                            IncludeFile += Separator + string.Join(Separator, chooserMessage.SelectedFiles);
                        }
                    }
                }
            };
            _eventPublisher.Publish(chooserMessage);
        }

    }
}
