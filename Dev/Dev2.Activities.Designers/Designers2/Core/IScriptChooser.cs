using Dev2.Studio.Core.Messages;

namespace Dev2.Activities.Designers2.Core
{
    public interface IScriptChooser
    {
        FileChooserMessage ChooseScriptSources(string includeFile);
    }
}