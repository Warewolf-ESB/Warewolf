namespace Dev2.Data.SystemTemplates.Models
{

    /// <summary>
    /// Used to figure out the correct model type
    /// </summary>
    public class Dev2ModelTypeCheck
    {
        public Dev2ModelType ModelName { get; set; }

    }

    /// <summary>
    /// The model types ;)
    /// </summary>
    public enum Dev2ModelType
    {
        Dev2DecisionStack,
        Dev2Decision,
        Dev2Switch,
        Dev2SwitchCase
    }
}
