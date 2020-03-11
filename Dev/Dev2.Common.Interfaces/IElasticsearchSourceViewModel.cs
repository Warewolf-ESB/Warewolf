/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Windows.Input;

namespace Dev2.Common.Interfaces
{
    public interface IElasticsearchSourceViewModel
    {
        string HeaderText { get; set; }
        string HostName { get; set; }
        string Port { get; set; }
        string Password { get; set; }
        ICommand TestCommand { get; set; }
        ICommand CancelTestCommand { get; set; }
        bool TestPassed { get; set; }
        bool TestFailed { get; set; }
        string TestMessage { get; }
        bool Testing { get; }
        string ResourceName { get; set; }
        ICommand OkCommand { get; set; }
    }

    public interface IElasticsearchSourceModel
    {
        string ServerName { get; set; }
        string TestConnection(IElasticsearchSourceDefinition resource);
        void Save(IElasticsearchSourceDefinition toElasticsearchSource);
        IElasticsearchSourceDefinition FetchSource(Guid id);
    }
}
