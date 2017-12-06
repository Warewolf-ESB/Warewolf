/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ServerProxyLayer;

namespace Warewolf.Core
{
    public class DatabaseService : IDatabaseService
    {

        #region Implementation of IDatabaseService

        public string Path { get; set; }
        public IDbSource Source { get; set; }
        public IDbAction Action { get; set; }
        public IList<IServiceInput> Inputs { get; set; }
        public IList<IServiceOutputMapping> OutputMappings { get; set; }
        public string Name { get; set; }
        public Guid Id { get; set; }

        #endregion

        public override bool Equals(object obj)
        {
            if (obj.GetType() != GetType())
            {
                return false;
            }
            return Equals((DatabaseService)obj);
        }
    }
}
