
using System;
using System.Linq;
using System.Windows;
using Dev2.Activities.Utils;

namespace Dev2.Activities.Designers2.Foreach
{
    public partial class Small
    {
        public Small()
        {
            InitializeComponent();
            DropPoint.PreviewDrop += DropPointOnDragEnter;
            DropPoint.PreviewDragOver += DropPointOnDragEnter;
        }

        void DropPointOnDragEnter(object sender, DragEventArgs e)
        {
            bool dropEnabled = true;
            var formats = e.Data.GetFormats();
            if (!formats.Any())
                return;
            var modelItemString = formats.FirstOrDefault(s => s.IndexOf("ModelItemFormat") >= 0);
            if (String.IsNullOrEmpty(modelItemString))
            {
                modelItemString = formats.FirstOrDefault(s => s.IndexOf("WorkflowItemTypeNameFormat") >= 0);
                if (String.IsNullOrEmpty(modelItemString))
                    return;
            }
            var objectData = e.Data.GetData(modelItemString);
            var foreachActivityDesignerUtils = new ForeachActivityDesignerUtils();
            dropEnabled = foreachActivityDesignerUtils.ForeachDropPointOnDragEnter(objectData);
            if (!dropEnabled)
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
            }
        }


        protected override IInputElement GetInitialFocusElement()
        {
            return InitialFocusElement;
        }
    }
}
