#pragma warning disable
﻿using System.ComponentModel;
using System.Windows.Input;
using Dev2.Common.Interfaces;
#if NETFRAMEWORK
using Microsoft.Practices.Prism.Mvvm;
#else
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
#endif

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

#if !NETFRAMEWORK
        public string Path => throw new System.NotImplementedException();

        public Task RenderAsync(ViewContext context)
        {
            throw new System.NotImplementedException();
        }
#endif

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
