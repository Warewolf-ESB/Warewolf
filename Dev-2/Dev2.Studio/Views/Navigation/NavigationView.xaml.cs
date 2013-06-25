using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Activities.Presentation;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Core.ViewModels;
using System.Windows.Controls;

namespace Dev2.Studio.Views.Navigation
{
    public partial class NavigationView : UserControl
    {
        #region Constructor

        public NavigationView() 
        {
            InitializeComponent();
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
