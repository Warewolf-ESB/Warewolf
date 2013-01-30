#define Debug
using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using Dev2.Composition;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.ViewModels.UiAutomation;

namespace Dev2.Studio.AppResources.Behaviors
{
    public class AutomationIdCreationBehaviour : Behavior<MenuItem>
    {
        #region Ctor
        public AutomationIdCreationBehaviour()
        {
            ImportService.SatisfyImports(this);
        }
        #endregion Ctor

        [Import]
        public IDev2WindowManager WindowNavigationBehavior { get; set; }

        #region Override Methods

        protected override void OnAttached()
        {
            if (!App.IsAutomationMode)
            {
                AssociatedObject.Visibility = Visibility.Collapsed;
            }
            base.OnAttached();

            AssociatedObject.Click += AssociatedObject_Click;

        }

        void AssociatedObject_Click(object sender, RoutedEventArgs e)
        {
#if Debug
            //AutomationIdCreaterView view = new AutomationIdCreaterView();
            string _automationId = string.Empty;
            ModelItem modItem = AutomationIdCreation;
            if (modItem.Properties["AutomationID"].ComputedValue != null)
            {
                _automationId = modItem.Properties["AutomationID"].ComputedValue.ToString();
            }
            AutomationIdCreaterViewModel viewModel = new AutomationIdCreaterViewModel();
            if (!string.IsNullOrEmpty(_automationId))
            {
                viewModel.AutomationID = _automationId;
            }
            WindowNavigationBehavior.ShowDialog(viewModel);

            //view.DataContext = viewModel;
            //view.ShowDialog();
            if (viewModel.DialogResult == ViewModelDialogResults.Okay)
            {
                modItem.Properties["AutomationID"].ComputedValue = viewModel.AutomationID;
            }
#endif
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
        }

        #endregion Override Methods

        #region Dependency Properties

        #region AutomationIdCreation

        public ModelItem AutomationIdCreation
        {
            get { return (ModelItem)GetValue(AutomationIdCreationProperty); }
            set { SetValue(AutomationIdCreationProperty, value); }
        }


        public static readonly DependencyProperty AutomationIdCreationProperty =
            DependencyProperty.Register("AutomationIdCreation", typeof(ModelItem), typeof(AutomationIdCreationBehaviour), new PropertyMetadata(null));

        #endregion AutomationIdCreation

        #endregion Dependency Properties
    }
}
