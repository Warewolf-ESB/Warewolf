﻿using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Warewolf.Studio.Core;

namespace Warewolf.Studio.Views
{
    public partial class SplashPage : ISplashView
    {
        readonly Grid _blackoutGrid = new Grid();
        bool _isDialog;
        bool _studioShutdown;

        public SplashPage()
        {
            _studioShutdown = false;
            if (_isDialog)
            {
                PopupViewManageEffects.AddBlackOutEffect(_blackoutGrid);
            }
            try
            {
                InitializeComponent();
            }
            catch(Exception e)
            {
                Dev2Logger.Error(e.Message,e, "Warewolf Error");
            }
        }

        public void CloseSplash(bool studioShutDown)
        {
            if (studioShutDown)
            {
                _studioShutdown = true;
                Dispatcher.BeginInvoke(new Action(() => {
                    System.Windows.Threading.Dispatcher.CurrentDispatcher.InvokeShutdown();
                    Close();
                }));
            }
            if (_isDialog)
            {
                PopupViewManageEffects.RemoveBlackOutEffect(_blackoutGrid);
                Close();
            }
            else
            {
                Dispatcher.BeginInvoke(new Action(() => {
                    System.Windows.Threading.Dispatcher.CurrentDispatcher.InvokeShutdown();
                    Close();
                }));
            }
        }        

        public void Show(bool isDialog)
        {
            _isDialog = isDialog;
            if (_isDialog)
            {
                PopupViewManageEffects.AddBlackOutEffect(_blackoutGrid);
                ShowDialog();
            }
            else
            {
                Show();
            }
        }

        void SplashPage_OnClosing(object sender, CancelEventArgs e)
        {
            if (!_studioShutdown)
            {
                e.Cancel = true;
                Visibility = Visibility.Hidden;
            }
        }

        void SplashPage_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                PopupViewManageEffects.RemoveBlackOutEffect(_blackoutGrid);
                Close();
            }
        }

        void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            CloseSplash(false);
        }
    }
}
