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
using Dev2.Activities.Utils;

namespace Dev2.Activities.Designers2.SelectAndApply
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
            if (_dropEnabledActivityDesignerUtils != null)
            {
                var dropEnabled = _dropEnabledActivityDesignerUtils.LimitDragDropOptions(e.Data);
                if (!dropEnabled)
                {
                    e.Effects = DragDropEffects.None;
                    e.Handled = true;
                }
            }
        }

        void DropPoint_OnPreviewDrop(object sender, DragEventArgs e)
        {
            var viewModel = DataContext as SelectAndApplyDesignerViewModel;
            if (viewModel != null)
            {
               viewModel.DoDrop(e.Data);
            }
        }

        void DropPoint_OnDragOver(object sender, DragEventArgs e)
        {
            AllowDrag(e);
            OnDragOver(e);
        }


        protected override IInputElement GetInitialFocusElement()
        {
            return InitialFocusElement;
        }
    }
}
