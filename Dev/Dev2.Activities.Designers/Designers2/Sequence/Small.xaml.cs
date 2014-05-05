using System.Windows;
using Dev2.Activities.Utils;

namespace Dev2.Activities.Designers2.Sequence
{
    public partial class Small
    {
        readonly ForeachActivityDesignerUtils _foreachActivityDesignerUtils;

        public Small()
        {
            InitializeComponent();
            DropPoint.PreviewDragOver += DropPoint_OnDragOver;
            DropPoint.PreviewDrop += DropPoint_OnPreviewDrop;
            _foreachActivityDesignerUtils = new ForeachActivityDesignerUtils();
        }

        void AllowDrag(DragEventArgs e)
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

        protected override IInputElement GetInitialFocusElement()
        {
            return InitialFocusElement;
        }

        void DropPoint_OnPreviewDrop(object sender, DragEventArgs e)
        {
            var viewModel = DataContext as SequenceDesignerViewModel;
            if(viewModel != null)
            {
                viewModel.DoDrop(e.Data);
            }

            if(DropPoint.Item != null)
            {
                DropPoint.Item = null;
            }
        }

        void DropPoint_OnDragOver(object sender, DragEventArgs e)
        {
            AllowDrag(e);
            base.OnDragOver(e);
        }
    }
}