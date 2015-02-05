using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
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
        private Task _task;
        private CancellationToken _cancellationToken;

        public ControlStyleTestingWindow()
        {
            InitializeComponent();
            InitGrid();
            DataContext = this;
            TestingListBox.ItemsSource = Persons;
            var tokenSource2 = new CancellationTokenSource();
            _cancellationToken = tokenSource2.Token;
            Closing += delegate
            {
                if (_task != null && _task.Status == TaskStatus.Running)
                {
                    tokenSource2.Cancel();
                }
            };
            ComboBox.ItemsSource = Persons;
        }

        void InitGrid()
        {
            var people = Persons;
            BobDataGrid.ItemsSource = people;
            //        <DataGridCheckBoxColumn Header="Is Load Shedded" Binding="{Binding IsLoadShedded}"></DataGridCheckBoxColumn>
            //<DataGridTextColumn Header="Name" Binding="{Binding Name}"></DataGridTextColumn>
            //<DataGridHyperlinkColumn Header="Site" Binding="{Binding Site}"></DataGridHyperlinkColumn>
            //<DataGridComboBoxColumn Header="Gender" ItemsSource="{Binding Genders}"></DataGridComboBoxColumn>
            _task = new Task(() =>
            {
                while (true)
                {
                    if (_cancellationToken.IsCancellationRequested)
                    {
                        // Clean up here, then...

                    }
                    else
                    {
                        Thread.Sleep(30);
                        Dispatcher.Invoke(() => ProgressBar.Value = (ProgressBar.Value + 1) % 100);
                    }
                }
            }, _cancellationToken
            );
            _task.Start();


        }

        private static List<Person> Persons
        {
            get
            {
                var people = new List<Person>
                {
                    new Person
                    {
                        Name = "Bob",
                        Gender = "Male",
                        IsLoadShedded = false,
                        Site = "http://www.google.com",
                        Genders = new[] {"Male", "Female", "Unknown"}
                    },
                    new Person
                    {
                        Name = "Dora",
                        Gender = "Female",
                        IsLoadShedded = false,
                        Site = "http://www.bing.com",
                        Genders = new[] {"Male", "Female", "Unknown"}
                    },
                    new Person
                    {
                        Name = "Jake",
                        Gender = "Male",
                        IsLoadShedded = false,
                        Site = "http://www.google.com",
                        Genders = new[] {"Male", "Female", "Unknown"}
                    },
                    new Person
                    {
                        Name = "Phineas",
                        Gender = "Male",
                        IsLoadShedded = false,
                        Site = "http://www.google.com",
                        Genders = new[] {"Male", "Female", "Unknown"}
                    },
                };
                return people;
            }
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
