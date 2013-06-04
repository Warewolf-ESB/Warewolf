namespace DummyNamespaceForTest
{
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
    }
}