namespace Dev2.Common.Interfaces.Core
{
    public class ExplorerLightTreeSource : IExplorerLightTreeSource
    {
        #region Implementation of IExplorerLightTreeSource

        public string IconPath { get; set; }
        public string ParentId { get; set; }
        public string ResourceId { get; set; }
        public string ResourceName { get; set; }

        #endregion
    }
}
