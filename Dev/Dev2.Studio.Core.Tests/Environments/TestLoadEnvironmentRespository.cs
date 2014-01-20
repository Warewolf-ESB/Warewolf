using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;

namespace Dev2.Core.Tests.Environments
{
    public class TestLoadEnvironmentRespository : EnvironmentRepository
    {
        public int LoadInternalHitCount { get; set; }

        public TestLoadEnvironmentRespository()
        {
        }

        public TestLoadEnvironmentRespository(IEnvironmentModel source, params IEnvironmentModel[] environments)
            : base(source)
        {
            if(environments != null)
            {
                foreach(var environment in environments)
                {
                    Environments.Add(environment);
                }
            }
        }

        protected override void LoadInternal()
        {
            // Override, so that we don't connect to the server!
            LoadInternalHitCount++;
        }

        public override System.Collections.Generic.ICollection<IEnvironmentModel> All()
        {
            return Environments;
        }
    }

}