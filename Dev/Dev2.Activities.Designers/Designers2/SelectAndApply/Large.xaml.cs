
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
    public partial class Large
    {
        readonly DropEnabledActivityDesignerUtils _dropEnabledActivityDesignerUtils;

        public Large()
        {
            InitializeComponent();
            DropPoint.PreviewDrop += DoDrop;
            DropPoint.PreviewDragOver += DropPointOnDragEnter;
            _dropEnabledActivityDesignerUtils = new DropEnabledActivityDesignerUtils();
        }

        SelectAndApplyDesignerViewModel ViewModel
        {
            get
            {
                return DataContext as SelectAndApplyDesignerViewModel;
            }
        }

        void DoDrop(object sender, DragEventArgs e)
        {
            DropPointOnDragEnter(sender, e);
            var modelItem = this.DropPoint.Item;
           /* if (/*ViewModel.SetModelItemForServiceTypes(e.Data)false)
            {
                e.Handled = true;
            }*/
        }

        void DropPointOnDragEnter(object sender, DragEventArgs e)
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


        protected override IInputElement GetInitialFocusElement()
        {
            return InitialFocusElement;
        }
    }
}
