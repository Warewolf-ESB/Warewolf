using System.Windows;
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
            ActivitiesPresenter.PreviewDragOver += DropPointOnDragEnter;
            _dropEnabledActivityDesignerUtils = new DropEnabledActivityDesignerUtils();
        }

        SequenceDesignerViewModel ViewModel
        {
            get
            {
                return DataContext as SequenceDesignerViewModel;
            }
        }

        void DoDrop(object sender, DragEventArgs e)
        {
            DropPointOnDragEnter(sender, e);
            if(ViewModel.SetModelItemForServiceTypes(e.Data))
            {
                e.Handled = true;
            }
        }



        void DropPointOnDragEnter(object sender, DragEventArgs e)
        {
            if(_dropEnabledActivityDesignerUtils != null)
            {
                var dropEnabled = _dropEnabledActivityDesignerUtils.LimitDragDropOptions(e.Data);
                if(!dropEnabled)
                {
                    e.Effects = DragDropEffects.None;
                    e.Handled = true;
                }
            }
        }

        void SetProperties()
        {
            var viewModel = (SequenceDesignerViewModel)DataContext;
            if(viewModel != null)
            {
                viewModel.ThumbVisibility = Visibility.Collapsed;
            }
        }

        #region Overrides of ActivityDesignerTemplate

        protected override IInputElement GetInitialFocusElement()
        {
            return ActivitiesPresenter;
        }

        #endregion
    }
}
