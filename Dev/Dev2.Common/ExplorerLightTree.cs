using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces;
using Newtonsoft.Json;

namespace Dev2.Common
{
    [Serializable]
    public class ExplorerLightTree : IExplorerLightTree
    {
        public string ParentId { get; set; }
        public string ResourceId { get; set; }
        public string IconPath { get; set; }
        public string ResourceName { get; set; }
    }

    public static class ExplorerLightTreeUtilities
    {
        public static string LightTreeToXml(this List<ExplorerLightTree> lightTree) => JsonConvert.SerializeObject(lightTree);
        public static List<ExplorerLightTree> XmlToLightTree(this string lightTreeXml) => JsonConvert.DeserializeObject<List<ExplorerLightTree>>(lightTreeXml);
    }
}
