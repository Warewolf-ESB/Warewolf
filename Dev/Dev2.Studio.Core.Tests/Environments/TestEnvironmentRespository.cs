
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.Generic;
using Dev2.Studio.Core.Interfaces;

namespace Dev2.Core.Tests.Environments
{
    public class TestEnvironmentRespository : TestLoadEnvironmentRespository
    {
        public bool IsReadWriteEnabled { get; set; }
        public int AddInternalHitCount { get; set; }
        public int RemoveInternalHitCount { get; set; }
        public int WriteSessionHitCount { get; set; }
        public int ReadSessionHitCount { get; set; }

        public TestEnvironmentRespository()
        {
            IsReadWriteEnabled = false;
        }

        public TestEnvironmentRespository(IEnvironmentModel source, params IEnvironmentModel[] environments)
            : base(source, environments)
        {
            IsReadWriteEnabled = false;
        }

        protected override void AddInternal(IEnvironmentModel environment)
        {
            AddInternalHitCount++;
            base.AddInternal(environment);
        }

        protected override bool RemoveInternal(IEnvironmentModel environment)
        {
            RemoveInternalHitCount++;
            return base.RemoveInternal(environment);
        }

        protected override void LoadInternal()
        {
            base.LoadInternal();
            IsLoaded = true;
        }

        public override void WriteSession(IEnumerable<Guid> environmentGuids)
        {
            WriteSessionHitCount++;
            if(IsReadWriteEnabled)
            {
                base.WriteSession(environmentGuids);
            }
        }

        public override IList<Guid> ReadSession()
        {
            ReadSessionHitCount++;
            if(IsReadWriteEnabled)
            {
                return base.ReadSession();
            }
            return new List<Guid>();
        }

        public void AddMockEnvironment(IEnvironmentModel environmentToAdd)
        {
            Environments.Add(environmentToAdd);
        }
    }
}
