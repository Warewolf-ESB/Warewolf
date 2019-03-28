#pragma warning disable
namespace Dev2.Common.Interfaces
{
    public interface IIntellisenseStringResultBuilder
    {
        IIntellisenseStringResult Build(string selectedOption, int originalCaret, string originalText, string editorText);
    }
}