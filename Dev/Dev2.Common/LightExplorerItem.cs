using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces;

namespace Dev2.Common
{
    [Serializable]
    public class LightExplorerItem : ILightExplorerItem
    {
        public string ResourceType { get; set; }
        public string ResourcePath { get; set; }
        public string ResourceName { get; set; }
        public Guid ResourceId { get; set; }
        public bool IsSource { get; set; }
        public bool IsService { get; set; }
        public bool IsFolder { get; set; }
        public List<ILightExplorerItem> Children { get; set; }
    }
}
