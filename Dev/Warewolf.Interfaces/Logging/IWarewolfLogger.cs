/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;

namespace Warewolf.Logging
{
    public interface IWarewolfLogger
    {
        void Debug(object message, string executionId);
        void Debug(object message, Exception exception, string executionId);

        void Error(object message, string executionId);
        void Error(object message, Exception exception, string executionId);

        void Warn(object message, string executionId);
        void Warn(object message, Exception exception, string executionId);

        void Fatal(object message, string executionId);
        void Fatal(object message, Exception exception, string executionId);

        void Info(object message, string executionId);
        void Info(object message, Exception exception, string executionId);
    }
}
