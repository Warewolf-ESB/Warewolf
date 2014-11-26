
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Reactive;
using System.Windows;
using System.Windows.Input;
using Dev2.Interfaces;
using Dev2.Studio.ViewModels.Workflow;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.Views.Workflow
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class WorkflowDesignerView : IWorkflowDesignerView
    {
        readonly DragDropHelpers _dragDropHelpers;
        //IDisposable _subscription;

        public WorkflowDesignerView()
        {
            InitializeComponent();
            PreviewDrop += DropPointOnDragEnter;
            PreviewDragOver += DropPointOnDragEnter;
            PreviewMouseDown += WorkflowDesignerView_PreviewMouseDown;
            _dragDropHelpers = new DragDropHelpers(this);
//            var pattern = Observable.FromEventPattern<KeyEventArgs>(this,"KeyUp");
//            pattern.Throttle(TimeSpan.FromMilliseconds(50000))
//                .ObserveOn(SynchronizationContext.Current);
//
//            pattern.Subscribe(PerformOnDispatcher);
        }

        void PerformOnDispatcher(EventPattern<KeyEventArgs> eventPattern)
        {
            //Dispatcher.BeginInvoke(DispatcherPriority.Background,new Action(()=>PerfromAction(eventPattern.EventArgs)));
        }

        void WorkflowDesignerView_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var vm = (DataContext as WorkflowDesignerViewModel);
            if(vm != null)
            {
                CustomContainer.Get<IMainViewModel>().AddWorkSurfaceContext(vm.ResourceModel);
            }
        }

        //a return from here without settings handled to true and DragDropEffects.None implies that the item drop is allowed
        void DropPointOnDragEnter(object sender, DragEventArgs e)
        {
            var dataObject = e.Data;
            if(_dragDropHelpers.PreventDrop(dataObject))
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
            }
        }

        void UIElement_OnKeyUp(object sender, KeyEventArgs e)
        {
            //PerfromAction(e);
        }

        void PerfromAction(KeyEventArgs e)
        {
            
            if(e.Key == Key.Back || e.Key == Key.Delete || e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.PageDown || e.Key == Key.PageUp || e.Key==Key.Left || e.Key==Key.Right)
            {
                return;
            }
            var workflowDesignerViewModel = DataContext as WorkflowDesignerViewModel;
            if (workflowDesignerViewModel != null)
            {
                workflowDesignerViewModel.AddMissingWithNoPopUpAndFindUnusedDataListItems();
            }
        }
    }

    public interface IWorkflowDesignerView
    {
        object DataContext { get; set; }
    }
}
