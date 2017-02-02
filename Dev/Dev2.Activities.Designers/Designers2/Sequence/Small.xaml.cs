/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

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
                if (_dropEnabledActivityDesignerUtils != null)
                {
                    var dropEnabled = _dropEnabledActivityDesignerUtils.LimitDragDropOptions(e.Data);
                    if (!dropEnabled)
                    {
                        e.Effects = DragDropEffects.None;
                        e.Handled = true;
                    }
                }
                viewModel.DoDrop(e.Data);
            }
            DropPoint.Item = null;
        }
    }
}
