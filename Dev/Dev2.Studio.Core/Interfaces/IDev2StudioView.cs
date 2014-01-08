
// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Interfaces
{
    public interface IDev2StudioView
    {
        void ShowDialog(IPropertyEditorWizard editor);
        void Close();
    }
}
