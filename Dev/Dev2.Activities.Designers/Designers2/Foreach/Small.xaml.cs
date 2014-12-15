
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Windows;
using Dev2.Activities.Utils;

namespace Dev2.Activities.Designers2.Foreach
{
    public partial class Small
    {
        readonly DropEnabledActivityDesignerUtils _dropEnabledActivityDesignerUtils;

        public Small()
        {
            InitializeComponent();
            DropPoint.PreviewDrop += DoDrop;
            DropPoint.PreviewDragOver += DropPointOnDragEnter;
            _dropEnabledActivityDesignerUtils = new DropEnabledActivityDesignerUtils();
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
            if(_dropEnabledActivityDesignerUtils != null)
            {
                var dropEnabled = _dropEnabledActivityDesignerUtils.LimitDragDropOptions(e.Data);
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
