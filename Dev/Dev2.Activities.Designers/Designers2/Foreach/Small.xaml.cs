using System.Windows;
using Dev2.Activities.Utils;

namespace Dev2.Activities.Designers2.Foreach
{
    public partial class Small
    {
        readonly ForeachActivityDesignerUtils _foreachActivityDesignerUtils;

        public Small()
        {
            InitializeComponent();
            DropPoint.PreviewDrop += DoDrop;
            DropPoint.PreviewDragOver += DropPointOnDragEnter;
            _foreachActivityDesignerUtils = new ForeachActivityDesignerUtils();
        }

        ForeachDesignerViewModel ViewModel
        {
            get
            {
                return DataContext as ForeachDesignerViewModel;
            }
        }

        void DoDrop(object sender, DragEventArgs e)
        {
            var dataObject = e.Data;
            bool multipleItemsToSequence = ViewModel.MultipleItemsToSequence(dataObject);
            if(multipleItemsToSequence)
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
            }

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
                else
                {
                    if(ViewModel.MultipleItemsToSequence(e.Data))
                    {
                        e.Effects = DragDropEffects.None;
                        e.Handled = true;
                    }
                }
            }
        }


        protected override IInputElement GetInitialFocusElement()
        {
            return InitialFocusElement;
        }
    }
}
