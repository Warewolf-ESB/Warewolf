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
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using Dev2.Activities.Designers2.Core;
using Dev2.Interfaces;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Services.Events;

namespace Dev2.Activities.Designers2.Script_Javascript
{
    public class JavaScriptDesignerViewModel : ActivityDesignerViewModel
    {
        readonly IEventAggregator _eventPublisher;
        readonly IScriptChooser _scriptChooser;
        public JavaScriptDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            _eventPublisher = EventPublishers.Aggregator;
            _eventPublisher.Subscribe(this);

            EscapeScript = true;
            ChooseScriptSourceCommand = new DelegateCommand(o => ChooseScriptSources());
            AddTitleBarLargeToggle();
            _scriptChooser = new ScriptChooser();
            HelpText = Warewolf.Studio.Resources.Languages.HelpText.Tool_Javascript;
        }

        public string IncludeFile
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public bool EscapeScript { get; private set; }


        public ICommand ChooseScriptSourceCommand { get; private set; }

        public string ScriptTypeDefaultText
        {
            get { return (string)GetValue(ScriptTypeTextProperty); }
            set { SetValue(ScriptTypeTextProperty, value); }
        }

        public static readonly DependencyProperty ScriptTypeTextProperty = DependencyProperty.Register("ScriptTypeDefaultText", typeof(string), typeof(JavaScriptDesignerViewModel), new PropertyMetadata("JavaScript Syntax"));


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
            var fileChooserMessage = _scriptChooser.ChooseScriptSources(IncludeFile);
            fileChooserMessage.Filter = "js";
            _eventPublisher.Publish(fileChooserMessage);
        }

    }
}
