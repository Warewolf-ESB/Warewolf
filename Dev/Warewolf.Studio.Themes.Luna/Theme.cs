using Infragistics.Themes;

namespace Warewolf.Studio.Themes.Luna
{

    public class LunaTheme : ThemeBase
    {
        // Class implementation here 

        protected override sealed void ConfigureControlMappings()
        {
            var assemblyFullName = typeof(LunaTheme).Assembly.GetName().Name;
            var locationString = BuildLocationString(assemblyFullName, @"Theme.xaml");
            Mappings.Add(ControlMappingKeys.MsCoreControls, locationString);

            // Customised Theme Elements
            //Mappings.Add(ControlMappingKeys.XamMenu, BuildLocationString(assemblyFullName, @"\Elements\xamMenu.xaml"));
            //Mappings.Add(ControlMappingKeys.XamDataTree, BuildLocationString(assemblyFullName, @"\Elements\Explorer\Explorer.xaml"));

            // Standard Elements
            Mappings.Add(ControlMappingKeys.XamComboEditor_WpfOnly, BuildLocationString(assemblyFullName, @"\Elements\xamComboEditor.xaml"));
            Mappings.Add(ControlMappingKeys.XamDataPresenter, BuildLocationString(assemblyFullName, @"\Elements\xamDataPresenter.xaml"));
           // Mappings.Add(ControlMappingKeys.XamDialogWindow, BuildLocationString(assemblyFullName, @"\Elements\xamDialogWindow.xaml"));
            Mappings.Add(ControlMappingKeys.XamFormulaEditor, BuildLocationString(assemblyFullName, @"\Elements\xamFormulaEditor.xaml"));
            Mappings.Add(ControlMappingKeys.XamGrid, BuildLocationString(assemblyFullName, @"\Elements\xamGrid.xaml"));
            Mappings.Add(ControlMappingKeys.XamMaskedInput, BuildLocationString(assemblyFullName, @"\Elements\xamMaskedInput.xaml"));

            Mappings.Add(ControlMappingKeys.XamMultiColumnComboEditor, BuildLocationString(assemblyFullName, @"\Elements\xamMultiColumnCombo.xaml"));
            Mappings.Add(ControlMappingKeys.XamRichTextEditor, BuildLocationString(assemblyFullName, @"\Elements\xamRichTextEditor.xaml"));
            Mappings.Add(ControlMappingKeys.XamSpellChecker, BuildLocationString(assemblyFullName, @"\Elements\xamSpellChecker.xaml"));
            Mappings.Add(ControlMappingKeys.XamSyntaxEditor, BuildLocationString(assemblyFullName, @"\Elements\xamSyntaxEditor.xaml"));
            Mappings.Add(ControlMappingKeys.XamTileManager, BuildLocationString(assemblyFullName, @"\Elements\xamTileManager.xaml"));
            Mappings.Add(ControlMappingKeys.XamDockManager, BuildLocationString(assemblyFullName, @"\Elements\DockManager\MainDockManager.xaml"));


         
           
        }

    }



	
}

