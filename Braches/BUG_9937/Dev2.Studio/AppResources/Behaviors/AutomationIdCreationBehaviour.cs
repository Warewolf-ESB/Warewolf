#define Debug
using Caliburn.Micro;
using Dev2.Composition;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.ViewModels.UiAutomation;
using System.Activities.Presentation.Model;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace Dev2.Studio.AppResources.Behaviors
{
    public class AutomationIdCreationBehaviour : Behavior<MenuItem>
    {
        #region Ctor
        public AutomationIdCreationBehaviour()
        {
            WindowNavigationBehavior = ImportService.GetExportValue<IWindowManager>();
        }
        #endregion Ctor

        public IWindowManager WindowNavigationBehavior { get; set; }

        #region Override Methods

        protected override void OnAttached()
        {
            if (!App.IsAutomationMode)
            {
                AssociatedObject.Visibility = Visibility.Collapsed;
            }
            base.OnAttached();

            AssociatedObject.Click -= AssociatedObject_Click;
            AssociatedObject.Click += AssociatedObject_Click;
            AssociatedObject.Unloaded -= AssociatedObjectOnUnloaded;
            AssociatedObject.Unloaded += AssociatedObjectOnUnloaded;
        }

        void AssociatedObjectOnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            WindowNavigationBehavior = null;
            AssociatedObject.Click -= AssociatedObject_Click;
            AssociatedObject.Unloaded -= AssociatedObjectOnUnloaded;
        }

        void AssociatedObject_Click(object sender, RoutedEventArgs e)
        {
#if Debug
            //AutomationIdCreaterView view = new AutomationIdCreaterView();
            string _automationId = ModelItemUtils.GetProperty("AutomationID", AutomationIdCreation) as string ?? string.Empty;;

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
                ModelItemUtils.SetProperty("AutomationID", viewModel.AutomationID, AutomationIdCreation);
            }
#endif
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
