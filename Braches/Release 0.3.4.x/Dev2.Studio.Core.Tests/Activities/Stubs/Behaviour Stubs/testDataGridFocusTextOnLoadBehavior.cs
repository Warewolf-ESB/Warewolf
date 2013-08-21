using System.Windows;
using System.Windows.Controls;
using Dev2.Activities;
using Moq;

namespace Dev2.Core.Tests.Activities
{
// ReSharper disable InconsistentNaming
    public class testDataGridFocusTextOnLoadBehavior : DataGridFocusTextOnLoadBehavior
// ReSharper restore InconsistentNaming
    {
        public void TestAssociatedObject_Loaded(Mock<DataGrid> dataGrid)
        {
            AssociatedObject_Loaded(dataGrid.Object, It.IsAny<RoutedEventArgs>());
        }
    }
}