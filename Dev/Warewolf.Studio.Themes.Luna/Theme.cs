using Infragistics.Themes;

namespace Warewolf.Studio.Themes.Luna
{

    public class LunaTheme : ThemeBase
    {
        // Class implementation here 

        protected override sealed void ConfigureControlMappings()
        {
            var assemblyFullName = typeof(LunaTheme).Assembly.FullName;
            Mappings.Add(ControlMappingKeys.MsCoreControls, BuildLocationString(assemblyFullName, @"\Elements\Luna.xaml"));

            Mappings.Add(ControlMappingKeys.XamComboEditor_WpfOnly, BuildLocationString(assemblyFullName, @"\Elements\Luna.xamComboEditor.xaml"));
            Mappings.Add(ControlMappingKeys.XamDataPresenter, BuildLocationString(assemblyFullName, @"\Elements\Luna.xamDataPresenter.xaml"));
            Mappings.Add(ControlMappingKeys.XamDataTree, BuildLocationString(assemblyFullName, @"\Elements\Luna.xamDataTree.xaml"));
            Mappings.Add(ControlMappingKeys.XamDialogWindow, BuildLocationString(assemblyFullName, @"\Elements\Luna.xamDialogWindow.xaml"));
            Mappings.Add(ControlMappingKeys.XamDockManager, BuildLocationString(assemblyFullName, @"\Elements\Luna.xamDockManager.xaml"));
            Mappings.Add(ControlMappingKeys.XamFormulaEditor, BuildLocationString(assemblyFullName, @"\Elements\Luna.xamFormulaEditor.xaml"));
            Mappings.Add(ControlMappingKeys.XamGrid, BuildLocationString(assemblyFullName, @"\Elements\Luna.xamGrid.xaml"));
            Mappings.Add(ControlMappingKeys.XamMaskedInput, BuildLocationString(assemblyFullName, @"\Elements\Luna.xamMaskedInput.xaml"));
            Mappings.Add(ControlMappingKeys.XamMenu, BuildLocationString(assemblyFullName, @"\Elements\Luna.xamMenu.xaml"));
            Mappings.Add(ControlMappingKeys.XamMultiColumnComboEditor, BuildLocationString(assemblyFullName, @"\Elements\Luna.xamMultiColumnCombo.xaml"));
            Mappings.Add(ControlMappingKeys.XamRichTextEditor, BuildLocationString(assemblyFullName, @"\Elements\Luna.xamRichTextEditor.xaml"));
            Mappings.Add(ControlMappingKeys.XamSpellChecker, BuildLocationString(assemblyFullName, @"\Elements\Luna.xamSpellChecker.xaml"));
            Mappings.Add(ControlMappingKeys.XamSyntaxEditor, BuildLocationString(assemblyFullName, @"\Elements\Luna.xamSyntaxEditor.xaml"));
            Mappings.Add(ControlMappingKeys.XamTileManager, BuildLocationString(assemblyFullName, @"\Elements\Luna.xamTileManager.xaml"));


            
           
        }

    }



	
}

