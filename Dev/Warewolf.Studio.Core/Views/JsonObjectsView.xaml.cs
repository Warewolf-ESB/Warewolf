﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Dev2.Common.Interfaces;
using Warewolf.Studio.Core;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Views
{
    /// <summary>
    /// Interaction logic for JsonObjectsView.xaml
    /// </summary>
    public partial class JsonObjectsView: IJsonObjectsView
    {
        readonly Grid _blackoutGrid = new Grid();

        public JsonObjectsView()
        {
            InitializeComponent();
            PopupViewManageEffects.AddBlackOutEffect(_blackoutGrid);
        }

        void JsonObjectsView_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        void JsonObjectsView_OnClosed(object sender, EventArgs e)
        {
            PopupViewManageEffects.RemoveBlackOutEffect(_blackoutGrid);
        }

        void DoneButton_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        public void ShowJsonString(string jsonString)
        {
            ResponseTextbox.Text = jsonString;
            Height = 280;
            ShowDialog();
        }
    }
}
