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
            return string.Format("<root><echo>{0}</echo><hack>wtf</hack></root>", text);
        }

        public string NoEcho()
        {
            return "<root><echo>None</echo><hack>wtf</hack></root>";
        }
    }
}