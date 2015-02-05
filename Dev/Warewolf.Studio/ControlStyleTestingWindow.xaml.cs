using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Warewolf.Studio
{
    /// <summary>
    /// Interaction logic for ControlStyleTestingWindow.xaml
    /// </summary>
    public partial class ControlStyleTestingWindow : Window
    {



        public ControlStyleTestingWindow()
        {
            InitializeComponent();
            InitGrid();
        }

        void InitGrid()
        {
            IList<string> comboboxItems = new List<string>{"Bob","Dora","Jake","Phineas"};
            ComboBox.ItemsSource = comboboxItems;
            var people = new List<Person>
            {
                new Person {Name="Bob",Gender = "Male",IsLoadShedded = false,Site = "http://www.google.com", Genders = new []{"Male","Female","Unknown"}}, 
                new Person {Name="Dora",Gender = "Female",IsLoadShedded = false,Site = "http://www.bing.com", Genders = new []{"Male","Female","Unknown"}}, 
                new Person {Name="Jake",Gender = "Male",IsLoadShedded = false,Site = "http://www.google.com", Genders = new []{"Male","Female","Unknown"}}, 
                new Person {Name="Phineas",Gender = "Male",IsLoadShedded = false,Site = "http://www.google.com", Genders = new []{"Male","Female","Unknown"}}, 
            };
            BobDataGrid.ItemsSource = people;
            //        <DataGridCheckBoxColumn Header="Is Load Shedded" Binding="{Binding IsLoadShedded}"></DataGridCheckBoxColumn>
            //<DataGridTextColumn Header="Name" Binding="{Binding Name}"></DataGridTextColumn>
            //<DataGridHyperlinkColumn Header="Site" Binding="{Binding Site}"></DataGridHyperlinkColumn>
            //<DataGridComboBoxColumn Header="Gender" ItemsSource="{Binding Genders}"></DataGridComboBoxColumn>
            bool bob = true;
            Task t = new Task(() =>
            {
                while (bob)
                {
                    Thread.Sleep(30);
                    Dispatcher.Invoke(() => ProgressBar.Value = (ProgressBar.Value + 1) % 100);
                }
            }
            );
            t.Start();
            Application.Current.Exit += (a, b) => { bob = false; t.Wait(); };

        }
    }
    public class Person
    {
        public bool IsLoadShedded { get; set; }
        public string Name { get; set; }
        public string Site { get; set; }
        public string[] Genders { get; set; }
        public string Gender { get; set; }
    }
}
