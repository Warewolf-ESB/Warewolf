using System;

namespace DummyNamespaceForTest
{
    [Serializable]
    public class DummyClassForPluginTest
    {
        public string Name { get; set; }

        public DummyClassForPluginTest DummyMethod()
        {
            return new DummyClassForPluginTest
            {
                Name = "test data"
            };
        }

        public string Echo(string text)
        {
            return string.Format("<echo>{0}</echo>", text);
        }

        public string NoEcho()
        {
            return "<echo>None</echo>";
        }
    }
}