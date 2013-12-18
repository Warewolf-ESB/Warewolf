namespace Dev2.Data.Interfaces
{
    public interface IDataListInjectionContents {

        bool IsSystemRegion { get; }

        #region Methods
        string ToInjectableState();
        #endregion
    }
}
