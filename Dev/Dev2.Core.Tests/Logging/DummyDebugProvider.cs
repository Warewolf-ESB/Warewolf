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
using System.Collections.Generic;
using Dev2.Common.Interfaces.Diagnostics.Debug;

namespace Dev2.Tests.Logging
{
    public class DummyDebugProvider : IDebugProvider
    {
        private Guid _serverID = Guid.NewGuid();
        private Guid _workflow1ID = Guid.Parse("D72A6E61-EC79-43D4-99EC-9E26DB9A0A4B");
        private Guid _workflow2ID = Guid.NewGuid();
        private Guid _assign1ID = Guid.NewGuid();
        private Guid _assign2ID = Guid.NewGuid();
        private Guid _assign3ID = Guid.NewGuid();
        private DateTime _startDate = DateTime.Now;

        public Guid ServerID
        {
            get { return _serverID; }
            set { _serverID = value; }
        }

        public Guid Workflow1ID
        {
            get { return _workflow1ID; }
            set { _workflow1ID = value; }
        }

        public Guid Workflow2ID
        {
            get { return _workflow2ID; }
            set { _workflow2ID = value; }
        }

        public Guid Assign1ID
        {
            get { return _assign1ID; }
            set { _assign1ID = value; }
        }

        public Guid Assign2ID
        {
            get { return _assign2ID; }
            set { _assign2ID = value; }
        }

        public Guid Assign3ID
        {
            get { return _assign3ID; }
            set { _assign3ID = value; }
        }

        #region Implementation of IDebugProvider

        public IEnumerable<IDebugState> GetDebugStates(string serverWebUri, IDirectoryPath directory, IFilePath path)
        {
            yield break;
        }

        #endregion
    }

}
