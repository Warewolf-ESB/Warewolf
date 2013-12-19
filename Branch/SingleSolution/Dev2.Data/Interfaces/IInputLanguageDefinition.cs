
namespace Dev2.Data.Interfaces
{
    public interface IInputLanguageDefinition {

        #region Properties
        string Name { get; }

        string MapsTo { get; }

        string StartTagSearch { get; }

        string EndTagSearch { get; }

        string StartTagReplace { get; }

        string EndTagReplace { get; }

        bool IsEvaluated { get; }
        #endregion
    }
}
