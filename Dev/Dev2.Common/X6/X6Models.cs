using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Dev2.Common.X6
{
    public class Cell
    {
        [JsonProperty("position")]
        public Position Position { get; set; }

        [JsonProperty("size")]
        public Size Size { get; set; }

        [JsonProperty("visible")]
        public bool? Visible { get; set; }

        [JsonProperty("shape")]
        public string Shape { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonPropertyName("data")]
        public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();

        [JsonPropertyName("attrs")]
        public Dictionary<string, object> Attrs { get; set; } = new Dictionary<string, object>();

        [JsonProperty("zIndex")]
        public int ZIndex { get; set; }

        [JsonProperty("source")]
        public Connector Source { get; set; }

        [JsonProperty("target")]
        public Connector Target { get; set; }

        [JsonPropertyName("label")]
        public string Label { get; set; }
    }

    public class X6Graph
    {
        [JsonProperty("resourcename")]
        public string ResourceName { get; set; }

        [JsonProperty("workflowxml")]
        public string WorkflowXml { get; set; }

        [JsonPropertyName("cells")]
        public List<Cell> Cells { get; set; } = new List<Cell>();
    }

    public class Position
    {
        [JsonProperty("x")]
        public int X { get; set; }

        [JsonProperty("y")]
        public int Y { get; set; }

        public Position(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    public class Size
    {
        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }

        public Size(int w, int h)
        {
            Width = w;
            Height = h;
        }
    }

    public class Connector
    {
        [JsonProperty("cell")]
        public string Id { get; set; }

        public Connector(string id)
        {
            Id = id;
        }
    }

    
}
