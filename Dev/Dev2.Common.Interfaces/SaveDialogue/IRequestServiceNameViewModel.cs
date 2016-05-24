﻿using System;
using System.Windows;
using System.Windows.Input;

// ReSharper disable once CheckNamespace
namespace Dev2.Common.Interfaces.SaveDialog
{
    public interface IRequestServiceNameViewModel : IDisposable
    {
        MessageBoxResult ShowSaveDialog();

        ResourceName ResourceName { get; }
        string Name { get; set; }
        string ErrorMessage { get; set; }
        ICommand OkCommand { get; set; }
        ICommand CancelCommand { get; }
        IExplorerViewModel SingleEnvironmentExplorerViewModel { get; }
        string Header { get; }
    }

    public class ResourceName
    {
        private readonly string _name;
        private readonly string _path;

        public ResourceName(string path, string name)
        {
            _path = path;
            _name = name;
        }

        public string Name
        {
            get
            {
                return _name;
            }
        }

        public string Path
        {
            get
            {
                return _path;
            }
        }
    }
}