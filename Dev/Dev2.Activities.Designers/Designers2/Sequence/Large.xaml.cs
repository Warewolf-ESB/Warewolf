using System.Windows;
using Dev2.Activities.Utils;

namespace Dev2.Activities.Designers2.Sequence
{
    /// <summary>
    /// Interaction logic for Large.xaml
    /// </summary>
    public partial class Large
    {
        readonly ForeachActivityDesignerUtils _foreachActivityDesignerUtils;

        public Large()
        {
            InitializeComponent();
            Loaded += (sender, args) => SetProperties();
            ActivitiesPresenter.PreviewDrop += DoDrop;
            ActivitiesPresenter.PreviewDragOver += DropPointOnDragEnter;
            _foreachActivityDesignerUtils = new ForeachActivityDesignerUtils();
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
            ViewModel.SetModelItemForServiceTypes(e.Data);
        }



        void DropPointOnDragEnter(object sender, DragEventArgs e)
        {
            if(_foreachActivityDesignerUtils != null)
            {
                var dropEnabled = _foreachActivityDesignerUtils.LimitDragDropOptions(e.Data);
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
