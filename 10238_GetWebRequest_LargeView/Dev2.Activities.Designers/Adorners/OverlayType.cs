namespace Dev2.Activities.Adorners
{
    /// <summary>
    /// The type of overlay being added
    /// </summary>
    /// <author>Jurie.smit</author>
    /// <date>2013/07/24</date>
    public enum OverlayType
    {
        None,
        [AdornerAutomationId("OpenMappingsToggle","OpenMappingsContent")]
        Mappings,
        [AdornerAutomationId("QuickVariableInputToggle", "QuickVariableInputContent")]
        QuickVariableInput,
        [AdornerAutomationId("LargeViewToggle", "LargeViewContent")]
        LargeView,
        Help
    }
}