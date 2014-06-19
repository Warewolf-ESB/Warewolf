using System.Windows;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Views.Navigation
{
    public partial class NavigationView
    {
        #region Constructor

        public NavigationView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            DataContextChanged += OnDataContextChanged;
        }

        void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            object newValue = dependencyPropertyChangedEventArgs.NewValue;

        }

        void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            var currentDataContext = DataContext;
        }

        #endregion Constructor

        #region Dependency Properties

        #region ItemContainerStyle

        public Style ItemContainerStyle
        {
            get { return (Style)GetValue(ItemContainerStyleProperty); }
            set { SetValue(ItemContainerStyleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemContainerStyleProperty =
            DependencyProperty.Register("ItemContainerStyle", typeof(Style), typeof(NavigationView), new PropertyMetadata(null));

        #endregion ItemContainerStyle

        #region ItemTemplate

        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemTemplateProperty =
            DependencyProperty.Register("ItemTemplate", typeof(DataTemplate), typeof(NavigationView), new PropertyMetadata(null));

        #endregion ItemTemplate

        #endregion Dependency Properties
    }
}
