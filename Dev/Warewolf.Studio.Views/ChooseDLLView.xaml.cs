#pragma warning disable
﻿using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Dev2.Common.Interfaces;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Warewolf.Studio.Views
{
    /// <summary>
    /// Interaction logic for ChooseDLLView.xaml
    /// </summary>
    public partial class ChooseDLLView : IChooseDLLView
    {
        public ChooseDLLView()
        {
            InitializeComponent();
        }

        public string Path => throw new System.NotImplementedException();

        public Task RenderAsync(ViewContext context)
        {
            throw new System.NotImplementedException();
        }

        public void RequestClose()
        {
            DialogResult = true;
            Close();
        }

        public void ShowView(IDLLChooser chooser)
        {
            Closing += ChooseDLLView_OnClosing;
            MouseDown += ChooseDLLView_OnMouseDown;
            DataContext = chooser;
            ShowDialog();
        }

        void ChooseDLLView_OnClosing(object sender, CancelEventArgs e)
        {
        }

        void ChooseDLLView_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }
    }
}
