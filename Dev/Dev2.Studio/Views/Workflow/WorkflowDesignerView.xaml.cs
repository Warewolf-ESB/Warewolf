/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

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
            PreviewMouseDown += WorkflowDesignerViewPreviewMouseDown;
            KeyDown += OnKeyDown;
            _dragDropHelpers = new DragDropHelpers(this);
            //            var pattern = Observable.FromEventPattern<KeyEventArgs>(this,"KeyUp");
            //            pattern.Throttle(TimeSpan.FromMilliseconds(50000))
            //                .ObserveOn(SynchronizationContext.Current);
            //
            //            pattern.Subscribe(PerformOnDispatcher);
        }

        void OnKeyDown(object sender, KeyEventArgs e)
        {
            //if (e.Key == Key.Enter)
            //{
                
            //}
        }

        void WorkflowDesignerViewPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var vm = DataContext as WorkflowDesignerViewModel;
            if (vm != null)
            {
                CustomContainer.Get<IMainViewModel>().AddWorkSurfaceContext(vm.ResourceModel);
            }
        }
        //a return from here without settings handled to true and DragDropEffects.None implies that the item drop is allowed
        void DropPointOnDragEnter(object sender, DragEventArgs e)
        {
            var dataObject = e.Data;
            if (_dragDropHelpers.PreventDrop(dataObject))
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
            }
        }
    }
    public interface IWorkflowDesignerView
    {
        object DataContext { get; set; }
    }
}