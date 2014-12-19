using Infragistics.Themes;

namespace Warewolf.Studio.Themes.Luna
{

    public class LunaTheme : ThemeBase
    {

        protected override sealed void ConfigureControlMappings()
        {
            var assemblyFullName = typeof(LunaTheme).Assembly.FullName;
            var location = BuildLocationString(assemblyFullName, @"\Elements\Luna.xamDockManager.xaml");

            Mappings.Add(ControlMappingKeys.XamDockManager,  location);
        }

    }



	
}

