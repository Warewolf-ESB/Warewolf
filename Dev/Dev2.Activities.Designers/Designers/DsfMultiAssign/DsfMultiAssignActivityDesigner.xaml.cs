using System;
using System.Windows;

namespace Dev2.Activities.Designers.DsfMultiAssign
{
    public partial class DsfMultiAssignActivityDesigner
    {
        public DsfMultiAssignActivityDesigner()
        {
            InitializeComponent();
            DataContextChanged+=DataContextHasChanged;
        }

        void DataContextHasChanged(object sender, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var viewModel = dependencyPropertyChangedEventArgs.NewValue as DsfMultiAssignActivityViewModel;
            if (viewModel != null)
            {
                QuickVariableInputAdornerPresenter.QuickVariableInputView.QuickVariableInputViewModel = viewModel.QuickVariableInputViewModel;
                QuickVariableInputAdornerPresenter.QuickVariableInputView.ActivityViewModelBase = viewModel;
            }
        }

        protected override void OnPreviewDragEnter(DragEventArgs e)
        {
            // 2013.07.29: Ashley Lewis for bug 9949 - workaround for Automatic-drill-down
        }
    }
}
