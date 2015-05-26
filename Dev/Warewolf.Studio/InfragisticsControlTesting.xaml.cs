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
    }
}
