using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Infragistics.Controls.Grids.Primitives;

namespace Warewolf.Studio
{
    /// <summary>
    /// Interaction logic for InfragisticsControlTesting.xaml
    /// </summary>
    public partial class InfragisticsControlTesting : Window
    {
        public InfragisticsControlTesting()
        {
            InitializeComponent();
            List<Data> data = new List<Data>();
            data.Add(new Data() { Country = "USA", Cities = new List<string>() { "NY", "LA" } });
            data.Add(new Data() { Country = "England", Cities = new List<string>() { "London", "Liverpool" } });
            data.Add(new Data() { Country = "Bulgaria", Cities = new List<string>() { "Sofia", "Varna" } });

            this.DataContext = data;
        }
        public class Data
        {
            public string Country { get; set; }
            public List<string> Cities { get; set; }
        }

        private void Grid1_CellEnteredEditMode(object sender, Infragistics.Controls.Grids.EditingCellEventArgs e)
        {
            if (e.Cell.Column.Key == "Name" && e.Cell is AddNewRowCell)
            {
                TextBox tb = e.Editor as TextBox;
                tb.IsReadOnly = true;
            }
        }

        private void Grid1_CellExitingEditMode(object sender, Infragistics.Controls.Grids.ExitEditingCellEventArgs e)
        {
            if (e.Cell.Column.Key == "Name" && e.Cell is AddNewRowCell)
            {
                TextBox tb = e.Editor as TextBox;
                tb.IsReadOnly = false;
            }
        }
    }
}
