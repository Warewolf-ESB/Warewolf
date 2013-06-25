using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;

namespace Dev2.Core.Tests.Environments
{
    public class TestLoadEnvironmentRespository : EnvironmentRepository
    {
        public int LoadInternalHitCount { get; set; }

        public TestLoadEnvironmentRespository()
        {
            // Set IsLoaded = true, so that we don't connect to the server when invoking .All()
            IsLoaded = true;
        }

        public TestLoadEnvironmentRespository(IEnvironmentModel source, params IEnvironmentModel[] environments)
            : base(source)
        {
            // Set IsLoaded = true, so that we don't connect to the server when invoking .All()
            IsLoaded = true;

            if (environments != null)
            {
                foreach (var environment in environments)
                {
                    _environments.Add(environment);
                }
            }
        }

        protected override void LoadInternal()
        {
            LoadInternalHitCount++;
        }

        public override System.Collections.Generic.ICollection<IEnvironmentModel> All()
        {
            return _environments;
        }
    }

}