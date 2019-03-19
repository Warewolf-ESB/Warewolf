#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Windows;
using System.Windows.Input;
using Dev2.Activities.Utils;

namespace Dev2.Activities.Designers2.Sequence
{
    /// <summary>
    /// Interaction logic for Large.xaml
    /// </summary>
    public partial class Large
    {
        readonly DropEnabledActivityDesignerUtils _dropEnabledActivityDesignerUtils;

        public Large()
        {
            InitializeComponent();
            Loaded += (sender, args) => SetProperties();
            ActivitiesPresenter.PreviewDrop += DoDrop;
            _dropEnabledActivityDesignerUtils = new DropEnabledActivityDesignerUtils();
        }

        SequenceDesignerViewModel ViewModel => DataContext as SequenceDesignerViewModel;

        void DoDrop(object sender, DragEventArgs e)
        {
            if (_dropEnabledActivityDesignerUtils != null)
            {
                var dropEnabled = _dropEnabledActivityDesignerUtils.LimitDragDropOptions(e.Data);
                if (!dropEnabled)
                {
                    e.Effects = DragDropEffects.None;
                    e.Handled = true;
                }
            }
            if (ViewModel.TrySetModelItemForServiceTypes(e.Data))
            {
                e.Handled = true;
            }
        }

        void SetProperties()
        {
        }

        protected override IInputElement GetInitialFocusElement() => ActivitiesPresenter;

        void CopyCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        void CopyCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        void CopyCommandPreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        void CopyCommandPreviewCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
        }

        void SapvCopyCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        void SapvCopyCommandPreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }
    }
}
