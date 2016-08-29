/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
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

        public string Name => _name;

        public string Path => _path;
    }
}