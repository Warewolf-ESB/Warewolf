﻿/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using CommandLine;
using Dev2.Util;

namespace Warewolf.Logger
{
    public interface IArgs
    {
        bool Verbose { get; }
        Uri ServerEndpoint { get; }
    }
    public class Args : IArgs
    {
        [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
        public bool Verbose { get; set; }
        
        private Uri _serverEndpoint;
        [Option('s', "server", Required = false, HelpText = "Warewolf server url")]
        public Uri ServerEndpoint
        {
            get
            {
                if (_serverEndpoint is null)
                {
                    var applicationServerUri = new Uri(string.IsNullOrEmpty(AppUsageStats.LocalHost) ? $"https://{Environment.MachineName.ToLowerInvariant()}:3143" : AppUsageStats.LocalHost);
                    _serverEndpoint = new Uri(applicationServerUri.ToString().ToUpper().Replace("localhost".ToUpper(), Environment.MachineName));
                }
                return _serverEndpoint;
            }
            set
            {
                _serverEndpoint = value;
            }
        }
    }
}