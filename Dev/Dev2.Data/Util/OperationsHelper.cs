#pragma warning disable
﻿/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Data.Interfaces;
using System;

namespace Dev2.Data.Util
{
    public static class OperationsHelper
    {
        public static string ExtractUserName(IPathAuth path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }
            var idx = path.Username.IndexOf("\\", StringComparison.Ordinal);
            //TODO: This code should check for >= and not just >
            var result = idx > 0 ? path.Username.Substring(idx + 1) : path.Username;
            return result;
        }

        public static string ExtractDomain(IActivityIOPath path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }
            var result = string.Empty;

            var idx = path.Username.IndexOf("\\", StringComparison.Ordinal);

            if (idx > 0)
            {
                result = path.Username.Substring(0, idx);
            }

            return result;
        }
    }
}
