/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Windows;
using Dev2.Activities.Utils;

namespace Dev2.Activities.Designers2.SuspendExecution
{
    public partial class Large
    {
        readonly DropEnabledActivityDesignerUtils _dropEnabledActivityDesignerUtils;

        public Large()
        {
            InitializeComponent();
            DropPoint.PreviewDrop += DoDrop;
            _dropEnabledActivityDesignerUtils = new DropEnabledActivityDesignerUtils();
        }

        void DoDrop(object sender, DragEventArgs e)
        {
            var dataObject = e.Data;
            if (_dropEnabledActivityDesignerUtils != null)
            {
                var dropEnabled = _dropEnabledActivityDesignerUtils.LimitDragDropOptions(dataObject);
                if (!dropEnabled)
                {
                    e.Effects = DragDropEffects.None;
                    e.Handled = true;
                }
                else
                {
                    //if (SuspendExecutionDesignerViewModel.MultipleItemsToSequence(dataObject))
                    {
                        e.Effects = DragDropEffects.None;
                        e.Handled = true;
                    }
                }
            }
            //var multipleItemsToSequence = SuspendExecutionDesignerViewModel.MultipleItemsToSequence(dataObject);
            //if (multipleItemsToSequence)
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
            }
        }

        protected override IInputElement GetInitialFocusElement() => InitialFocusElement;
    }
}