
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Windows.Controls;
using System.Windows.Data;

namespace Warewolf.Studio.Views
{
    /// <summary>
    /// Interaction logic for MappingsView.xaml
    /// </summary>
    public partial class MappingsView : UserControl
    {
        public MappingsView()
        {
            InitializeComponent();
        }

        public ItemCollection GetInputMappings()
        {
            BindingExpression be = InputsMappingDataGrid.GetBindingExpression(ItemsControl.ItemsSourceProperty);
            if (be != null)
            {
                be.UpdateTarget();
            }
            return InputsMappingDataGrid.ItemsSource as ItemCollection;
        }

        public ItemCollection GetOutputMappings()
        {
            BindingExpression be = OutputsMappingDataGrid.GetBindingExpression(ItemsControl.ItemsSourceProperty);
            if (be != null)
            {
                be.UpdateTarget();
            }
            return OutputsMappingDataGrid.ItemsSource as ItemCollection;
        }

        #region Implementation of IComponentConnector

        /// <summary>
        /// Attaches events and names to compiled content. 
        /// </summary>
        /// <param name="connectionId">An identifier token to distinguish calls.</param><param name="target">The target to connect events and names to.</param>
        public void Connect(int connectionId, object target)
        {
        }

        #endregion
    }
}
