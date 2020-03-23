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
using Warewolf.Streams;

namespace Warewolf.Logging
{
    public interface ILoggerConnection : IDisposable
    {
        ILoggerPublisher NewPublisher();
        void StartConsuming(ILoggerConfig config, IConsumer consumer);
    }

    public interface ILoggerConfig
    {
        string Endpoint { get; set; }
    }
}