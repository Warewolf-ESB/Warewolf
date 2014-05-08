using System.Windows;
using Dev2.Activities.Utils;

namespace Dev2.Activities.Designers2.Sequence
{
    public partial class Small
    {
        readonly DropEnabledActivityDesignerUtils _dropEnabledActivityDesignerUtils;

        public Small()
        {
            InitializeComponent();
            DropPoint.PreviewDragOver += DropPoint_OnDragOver;
            DropPoint.PreviewDrop += DropPoint_OnPreviewDrop;
            _dropEnabledActivityDesignerUtils = new DropEnabledActivityDesignerUtils();
        }

        void AllowDrag(DragEventArgs e)
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
            DropPoint.Item = null;
        }

        void DropPoint_OnDragOver(object sender, DragEventArgs e)
        {
            AllowDrag(e);
            base.OnDragOver(e);
        }
    }
}