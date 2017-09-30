namespace Dev2.Instrumentation
{
    public static class ApplicationTrackerConstants
    {
      
        public const string ProviderName = "revulytics";


        public class TrackerEventGroup
        {
            public const string MainMenuClicked = "Main Menu";
            public const string TabsOpened = "Tabs Opened";
            public const string Deploy = "Deploy";
            public const string ToolBoxSearch = "Tool Box Search";
            public const string ExplorerSearch = "Explorer Search";
            public const string VariablesSearch = "Variables Search";
            public const string VariablesUsed = "Variables Used";
        
            public const string Exception = "Exception";
            public const string Help = "Help";
            public const string DragOnDesignSurface = "Drag On Design Surface";
        }

        public class TrackerEventName
        {
            public const string NewServiceClicked = "New Service Clicked";
            public const string DeployClicked = "Deploy Clicked";
            public const string TaskClicked = "Task Clicked";
            public const string DebugClicked = "Debug Clicked";
           
            public const string ViewInBrowserClicked = "View In Browser Clicked";
            public const string SaveClicked = "Save Clicked";
            
            public const string F6Debug = "Debug(F6)";
            public const string F7Browser = "View in Browser(F7)";
            public const string SettingsClicked = "Settings Clicked";
            public const string HelpClicked = "Help Clicked";
            public const string LinkURLClicked = "Link(URL) Clicked";
            public const string StartPageClicked = "Start Page";
            public const string UpgradeVersion = "Upgrade to New Version Clicked";
            public const string VariablesInputClicked = "Variables - Input Clicked ";
            public const string VariablesOutputClicked = "Variables - Output Clicked";
            public const string HelloWorldClicked = "Hello World Clicked";
            public const string WarewolfStoreClicked = "Warewolf Store Clicked";
            public const string ExamplesClicked = "Examples Clicked";
            public const string NewRemoteServerClicked = "New Remote Server Clicked";       
         
            public const string SharedResourcesServerClicked = "Shared Resources Server Clicked";

            public const string CreateNewTestClicked = "Create a new Test Clicked";
            public const string WhatDoesThisDoClicked = "What does this do Clicked?";
            public const string RedBracketsSyntax = "Red Brackets Syntax Clicked?";
            public const string ToolSearch = "Tool Search";
            public const string ExplorerSearch = "Explorer Search";
            public const string VariableSearch = "Variable Search";

            public const string UsedVariables = "Used Variables";
            public const string UnUsedVariables = "Un used Variables";

            public const string ItemDragged = "Item Dragged";
        }

    }
}
